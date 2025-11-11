using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Ionic.Zlib;
using Lzma;
using RP6.Format.ResourceDataPack;
using Utils.IO;
using Utils.IO.Extensions;
using CompressionMode = Ionic.Zlib.CompressionMode;

namespace RP6.IO;

public class Rp6Processor
{
    private readonly BinaryReader _br;
    private readonly Stream _stream;

    public Rp6Processor(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _br = new BinaryReader(_stream, Encoding.ASCII, leaveOpen: true);
    }

    public void Dispose()
    {
        _br.Dispose();
    }

    public List<ResourceInfo> Process(string outputRoot = ".")
    {
        var mainHeader = _br.ReadStruct<MainHeader>();

        var definedTypes = _br.ReadStructArray<ResourceTypeHeader>((int)mainHeader.PhysResTypeCount);
        var physEntries = _br.ReadStructArray<ResourceEntryHeader>((int)mainHeader.PhysResCount);
        var logHeaders = _br.ReadStructArray<LogicalResourceEntryHeader>((int)mainHeader.ResourceNamesCount);

        var namesIndices = new uint[mainHeader.ResourceNamesCount];
        for (var i = 0; i < namesIndices.Length; i++)
            namesIndices[i] = _br.ReadUInt32();


        var namesBlockSize = (int)mainHeader.ResourceNamesBlockSize;
        var namesBufBytes = new byte[namesBlockSize];
        var actuallyRead = _stream.Read(namesBufBytes, offset: 0, namesBlockSize);
        if (actuallyRead != namesBlockSize)
            throw new EndOfStreamException($"Unable to read names block: wanted {namesBlockSize}, read {actuallyRead}.");

        var namesBuffer = Encoding.ASCII.GetString(namesBufBytes);

        var decompressedSections = DecompressDefinedTypes(_stream, definedTypes, _stream.Length);
        var resources = ExtractLogicalResources(_stream, physEntries, logHeaders, namesBuffer, namesIndices, definedTypes, decompressedSections, outputRoot);
        return resources;
    }

    private static List<byte[]?> DecompressDefinedTypes(Stream input, ResourceTypeHeader[] definedTypes, long fileLength)
    {
        var result = new List<byte[]?>(definedTypes.Length);

        for (var i = 0; i < definedTypes.Length; i++)
        {
            var dt = definedTypes[i];
            long dataFileOffset = dt.DataFileOffset; // ResourceEntryHeader count
            long compressedSize = dt.CompressedByteSize; // bytes
            long uncompressedSize = dt.DataByteSize; // bytes

            var dataFileOffsetBytes = Marshal.SizeOf<ResourceEntryHeader>() * dataFileOffset;

            if (dataFileOffsetBytes > fileLength)
            {
                Console.Error.WriteLine($"[WARN] invalid dataFileOffset for defined type {i}: ({dataFileOffsetBytes} bytes). Skipping.");
                result.Add(item: null);
                continue;
            }

            if (compressedSize > 0)
            {
                if (dataFileOffsetBytes + compressedSize > fileLength)
                {
                    Console.Error.WriteLine($"[WARN] compressed blob for type {i} extends beyond file; skipping.");
                    result.Add(item: null);
                    continue;
                }

                input.Seek(dataFileOffsetBytes, SeekOrigin.Begin);
                var compressedBuf = new byte[compressedSize];
                var cRead = input.Read(compressedBuf, offset: 0, (int)compressedSize);
                if (cRead != compressedSize)
                {
                    Console.Error.WriteLine($"[WARN] short read of compressed blob for type {i}: {cRead}/{compressedSize}.");
                    result.Add(item: null);
                    continue;
                }

                try
                {
                    //if (compressedBuf is [0x78, ..]) // zlib (0x78 header)
                    if (CheckZlib(compressedBuf))
                    {
                        using var mem = new MemoryStream(compressedBuf);
                        using var z = new ZlibStream(mem, CompressionMode.Decompress, leaveOpen: true);
                        var outBuf = new byte[uncompressedSize];
                        var got = 0;
                        while (got < outBuf.Length)
                        {
                            var r = z.Read(outBuf, got, outBuf.Length - got);
                            if (r <= 0) break;

                            got += r;
                        }

                        if (got != outBuf.Length)
                            Console.Error.WriteLine($"[WARN] zlib produced {got}/{outBuf.Length} bytes for type {i}.");

                        result.Add(outBuf);
                        Debug.WriteLine($"[INFO] DefinedType[{i}] zlib-decompressed: {uncompressedSize} bytes.");
                    }
                    else
                    {
                        using var mem = new MemoryStream(compressedBuf);
                        using var decoder = new DecoderStream(mem);
                        decoder.Initialize(DecoderProperties.Default);
                        var outBuf = new byte[uncompressedSize];
                        var got = 0;
                        while (got < outBuf.Length)
                        {
                            var r = decoder.Read(outBuf, got, outBuf.Length - got);
                            if (r <= 0) break;

                            got += r;
                        }

                        if (got != outBuf.Length)
                            Console.Error.WriteLine($"[WARN] LZMA produced {got}/{outBuf.Length} bytes for type {i}.");

                        result.Add(outBuf);
                        Console.WriteLine($"[INFO] DefinedType[{i}] LZMA-decompressed: {uncompressedSize} bytes.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ERROR] decompressing defined type {i}: {ex.Message}");
                    result.Add(item: null);
                }
            }
            else
            {
                result.Add(item: null);
                Debug.WriteLine($"[INFO] DefinedType[{i}] not compressed {dataFileOffsetBytes} bytes, size {uncompressedSize}).");
            }
        }

        return result;

        static bool CheckZlib(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
                return false;

            var cmf = data[index: 0];
            var flg = data[index: 1];

            // Compression method = 8 (DEFLATE)
            if ((cmf & 0x0F) != 8)
                return false;

            // Checksum must be divisible by 31 per zlib spec
            return ((cmf << 8) + flg) % 31 == 0;
        }
    }

    private static List<ResourceInfo> ExtractLogicalResources(Stream input, ResourceEntryHeader[] physEntries, LogicalResourceEntryHeader[] logHeaders, string namesBuffer, uint[] namesIndices, ResourceTypeHeader[] definedTypes, List<byte[]?> decompressedSections, string outputRoot)
    {
        var resources = new List<ResourceInfo>();
        var unitSize = Marshal.SizeOf<ResourceEntryHeader>(); // bytes per "unit" in this format

        for (var i = 0; i < logHeaders.Length; i++)
        {
            var logHeader = logHeaders[i];
            var filetype = (int)(logHeader.Bitfields >> 16 & 0xFFu);
            var entryCount = (int)(logHeader.Bitfields & 0xFFu);
            var currentResource = (int)logHeader.FirstResource;

            var fullText = FileHelpers.GetNullTerminatedString(namesBuffer, (int)namesIndices[i]);
            var baseName = FileHelpers.SanitizeFileName(fullText);
            var typeName = EResType.GetPrettyName((EResType.Type)filetype);

            var fileParts = new List<byte[]>();

            for (var p = 0; p < entryCount; p++)
            {
                if (currentResource < 0 || currentResource >= physEntries.Length)
                {
                    Console.Error.WriteLine($"[WARN] physical resource index {currentResource} out of range.");
                    break;
                }

                var phys = physEntries[currentResource];
                var physSection = (int)(phys.Bitfields & 0xFFu);
                long dataSize = phys.DataByteSize; // size in bytes
                ulong dataOffsetUnits = phys.DataOffset; // offset expressed in units (number of ResourceEntryHeader-sized blocks)

                if (physSection < 0 || physSection >= definedTypes.Length)
                {
                    Console.Error.WriteLine($"[WARN] invalid physSection {physSection} for part {p}");
                    break;
                }

                var typeHdr = definedTypes[physSection];
                ulong sectionBaseUnits = typeHdr.DataFileOffset;
                var sectionMarkedCompressed = typeHdr.CompressedByteSize > 0;

                var hasDecompressedBuffer = physSection < decompressedSections.Count && decompressedSections[physSection] != null;
                if (sectionMarkedCompressed && !hasDecompressedBuffer)
                {
                    Console.Error.WriteLine($"[WARN] section {physSection} marked compressed and no decompressed buffer available; skipping part {p}.");
                    currentResource++;
                    continue;
                }

                var sectionBaseBytes = sectionBaseUnits * (ulong)unitSize;
                var partOffsetBytes = dataOffsetUnits * (ulong)unitSize;

                // Overflow guard
                if (sectionBaseBytes > ulong.MaxValue - partOffsetBytes)
                {
                    Console.Error.WriteLine("[WARN] computed file offset overflow.");
                    break;
                }

                var absoluteOffsetBytesU = sectionBaseBytes + partOffsetBytes;

                var part = new byte[dataSize];

                if (hasDecompressedBuffer)
                {
                    var dec = decompressedSections[physSection]!;
                    if (absoluteOffsetBytesU + (ulong)dataSize > (ulong)dec.Length)
                    {
                        Console.Error.WriteLine($"[WARN] requested slice outside decompressed buffer (section {physSection}).");
                        break;
                    }

                    if (absoluteOffsetBytesU > int.MaxValue || dataSize > int.MaxValue)
                    {
                        Console.Error.WriteLine("[WARN] decompressed slice too large to copy with BlockCopy.");
                        break;
                    }

                    Buffer.BlockCopy(dec, (int)absoluteOffsetBytesU, part, dstOffset: 0, (int)dataSize);
                    Debug.WriteLine($"[INFO] Read part {p} from decompressed section {physSection} offset {absoluteOffsetBytesU} size {dataSize}");
                }
                else
                {
                    if (absoluteOffsetBytesU > (ulong)input.Length || absoluteOffsetBytesU + (ulong)dataSize > (ulong)input.Length)
                    {
                        Console.Error.WriteLine($"[WARN] attempted file read outside bounds: offset={absoluteOffsetBytesU}, size={dataSize}");
                        break;
                    }

                    if (absoluteOffsetBytesU > long.MaxValue)
                    {
                        Console.Error.WriteLine("[WARN] computed absolute offset too large for Seek.");
                        break;
                    }

                    var absoluteOffsetBytes = (long)absoluteOffsetBytesU;
                    input.Seek(absoluteOffsetBytes, SeekOrigin.Begin);

                    var read = 0;
                    while (read < dataSize)
                    {
                        var r = input.Read(part, read, (int)(dataSize - read));
                        if (r <= 0) break;

                        read += r;
                    }

                    if (read != dataSize)
                    {
                        Console.Error.WriteLine($"[WARN] short read for part {p}: {read}/{dataSize}");
                        break;
                    }

                    Debug.WriteLine($"[INFO] Read part {p} from file offset {absoluteOffsetBytes} size {dataSize}");
                }

                fileParts.Add(part);
                currentResource++;
            } // end parts loop

            if (fileParts.Count == 0)
                continue;

            var resourceOutputDir = Path.Combine(outputRoot, typeName);
            Directory.CreateDirectory(resourceOutputDir);

            var info = new ResourceInfo
            {
                LogicalIndex = i,
                BaseName = baseName,
                TypeName = typeName,
                FileType = filetype,
                Parts = fileParts,
                OutputDir = resourceOutputDir
            };

            resources.Add(info);
        } // end logHeaders loop

        return resources;
    }
}