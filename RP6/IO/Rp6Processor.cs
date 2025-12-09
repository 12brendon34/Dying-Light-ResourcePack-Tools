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
    private static bool _isDyingLight1;

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

        // determine DL1 layout once and store it on the instance
        _isDyingLight1 = DetermineChromeVersion(definedTypes, _stream.Length);
        
        var decompressedSections = DecompressDefinedTypes(_stream, definedTypes, _stream.Length);
        var resources = ExtractLogicalResources(_stream, physEntries, logHeaders, namesBuffer, namesIndices, definedTypes, decompressedSections, outputRoot);
        return resources;
    }

    //If DL1 or DL2/TB
    private static bool DetermineChromeVersion(ResourceTypeHeader[] definedTypes, long fileLength)
    {
        var entrySize = Marshal.SizeOf<ResourceEntryHeader>();

        var entries = 0;
        var dl1Valid = 0;
        var dl2Valid = 0;

        foreach (var dt in definedTypes)
        {
            entries++;
            long raw = dt.DataFileOffset;

            // DL1 raw is a byte offset
            if (raw < fileLength)
                dl1Valid++;

            // DL2 raw is an index, byte offset = raw * entrySize
            // check overflow before multiplying
            if (raw > long.MaxValue / entrySize)
                continue;
            
            var dl2Offset = raw * entrySize;
            if (dl2Offset < fileLength)
                dl2Valid++;
            
            //else overflow, dl2 invalid for this entry
        }

        // No Results, Assume DL1
        if (dl1Valid == 0 && dl2Valid == 0)
            return true;

        //DL2 valid but DL1 not, DL2 wins
        if (dl2Valid == entries && dl1Valid != entries)
            return false;

        //DL1 Valid not DL2, DL1 Wins
        if (dl1Valid == entries && dl2Valid != entries)
            return true;

        //if "both" are valid, assume DL2
        return false;
    }
    
    private static List<byte[]?> DecompressDefinedTypes(Stream input, ResourceTypeHeader[] definedTypes, long fileLength)
    {
        var result = new List<byte[]?>(definedTypes.Length);
        var entrySize = Marshal.SizeOf<ResourceEntryHeader>();

        for (var i = 0; i < definedTypes.Length; i++)
        {
            var dt = definedTypes[i];
            long dataFileOffset = dt.DataFileOffset; // ResourceEntryHeader count
            long compressedSize = dt.CompressedByteSize; // bytes
            long uncompressedSize = dt.DataByteSize; // bytes
            
            long dataFileOffsetBytes;
            if (_isDyingLight1)
            {
                dataFileOffsetBytes = dataFileOffset;
            }
            else
            {
                //dataFileOffset is a count of ResourceEntryHeader structures
                try
                {
                    checked
                    {
                        dataFileOffsetBytes = entrySize * dataFileOffset;
                    }
                }
                catch (OverflowException)
                {
                    Console.Error.WriteLine($"[WARN] dataFileOffset multiplication overflow for defined type {i}: ({dataFileOffset}). Falling back to treating as bytes.");
                    dataFileOffsetBytes = dataFileOffset;
                }
            }

            if (dataFileOffsetBytes > fileLength)
            {
                Console.Error.WriteLine($"[WARN] invalid dataFileOffset for defined type {i}: ({dataFileOffsetBytes} bytes). Skipping.");
                result.Add(item: null);
                continue;
            }
            

            if (dataFileOffsetBytes < 0 || dataFileOffsetBytes > fileLength)
            {
                Console.Error.WriteLine($"[WARN] invalid dataFileOffset for defined type {i}: ({dataFileOffsetBytes} bytes). Skipping.");
                result.Add(null);
                continue;
            }

            if (compressedSize <= 0)
            {
                // nothing to decompress
                result.Add(null);
                Debug.WriteLine($"[INFO] DefinedType[{i}] not compressed at {dataFileOffsetBytes} (uncompressed size {uncompressedSize}).");
                continue;
            }

            if (dataFileOffsetBytes + compressedSize > fileLength)
            {
                Console.Error.WriteLine($"[WARN] compressed blob for type {i} extends beyond file; skipping.");
                result.Add(null);
                continue;
            }

            try
            {
                input.Seek(dataFileOffsetBytes, SeekOrigin.Begin);

                var compressedBuf = new byte[compressedSize];
                var cRead = input.Read(compressedBuf, offset: 0, (int)compressedSize);
                if (cRead != compressedSize)
                {
                    Console.Error.WriteLine($"[WARN] short read of compressed blob for type {i}: {cRead}/{compressedSize}.");
                    result.Add(null);
                    continue;
                }

                // Choose zlib vs LZMA based on header
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
                result.Add(null);
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
        var entrySize = Marshal.SizeOf<ResourceEntryHeader>();

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
                var dataSize = phys.DataByteSize; // size in bytes
                //Count of ResourceEntryHeader on DL2 and TB, Bytes on DL1 and older
                
                long partOffsetBytes = phys.DataOffset;
                if (!_isDyingLight1)
                {
                    try
                    {
                        checked
                        {
                            partOffsetBytes = entrySize * phys.DataOffset;
                        }
                    }
                    catch (OverflowException)
                    {
                        Console.Error.WriteLine($"[WARN] dataFileOffset multiplication overflow for defined type {i}: ({phys.DataOffset}). Falling back to treating as bytes.");
                    }
                }
                
                if (physSection < 0 || physSection >= definedTypes.Length)
                {
                    Console.Error.WriteLine($"[WARN] invalid physSection {physSection} for part {p}");
                    break;
                }
                
                var typeHdr = definedTypes[physSection];
                long sectionBaseBytes = typeHdr.DataFileOffset;
                if (!_isDyingLight1)
                {
                    try
                    {
                        checked
                        {
                            sectionBaseBytes = entrySize * typeHdr.DataFileOffset;
                        }
                    }
                    catch (OverflowException)
                    {
                        Console.Error.WriteLine($"[WARN] dataFileOffset multiplication overflow for defined type {i}: ({typeHdr.DataFileOffset}). Falling back to treating as bytes.");
                    }
                }
                
                var sectionMarkedCompressed = typeHdr.CompressedByteSize > 0;
                var hasDecompressedBuffer = physSection < decompressedSections.Count && decompressedSections[physSection] != null;
                
                if (sectionMarkedCompressed && !hasDecompressedBuffer)
                {
                    Console.Error.WriteLine($"[WARN] section {physSection} marked compressed and no decompressed buffer available; skipping part {p}.");
                    currentResource++;
                    continue;
                }
                
                var absoluteOffsetBytes = sectionBaseBytes + partOffsetBytes;
                var part = new byte[dataSize];

                if (hasDecompressedBuffer)
                {
                    var dec = decompressedSections[physSection]!;
                    // decompressed buffer is the section contents, so offset into it is the data offset only
                    int decOffset;
                    try
                    {
                        decOffset = checked((int)partOffsetBytes);
                    }
                    catch (OverflowException)
                    {
                        Console.Error.WriteLine($"[ERROR] partOffsetBytes too large for section {physSection}.");
                        break;
                    }

                    Buffer.BlockCopy(dec, decOffset, part, dstOffset: 0, count: (int)dataSize);
                    Debug.WriteLine($"[INFO] Read part {p} from decompressed section {physSection} offset {decOffset} size {dataSize}");
                }
                else
                {
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
                isDyingLight1 = _isDyingLight1,
                FileType = filetype,
                Parts = fileParts,
                OutputDir = resourceOutputDir
            };

            resources.Add(info);
        } // end logHeaders loop

        return resources;
    }
}