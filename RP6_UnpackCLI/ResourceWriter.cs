using System.Diagnostics;
using Mesh.Format;
using Mesh.IO;
using RP6;
using RP6.Format;
using RP6.Format.CompactMesh;
using RP6.Format.ResourceDataPack;
using Utils.IO.Extensions;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.TextureFormats;
using Configuration = SixLabors.ImageSharp.Textures.Configuration;

namespace RP6_UnpackCLI;

static class ResourceWriter
{
    private readonly static Dictionary<int, Action<ResourceInfo>> Handlers = new()
    {
        { (int)EResType.Type.Texture, WriteTexture },
        { (int)EResType.Type.Mesh, WriteMesh },
        { (int)EResType.Type.Animation, WriteAnimation }
        //{ (int)ResourceTypeInfo.Fx, WriteFx }, //removed
        //{ (int)EResType.Type.BuilderInformation, WriteBuilderInformation }
        // add more mappings here
    };

    public static void WriteResource(ResourceInfo info)
    {
        if (Handlers.TryGetValue(info.FileType, out var handler) && !Options.Current.EnableRawDumping)
            try
            {
                handler(info);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] writing resource {info.BaseName} type={info.TypeName}: {ex.Message}");
                // fallback to binary dump
                WriteBinary(info);
            }
        else //fallback unhandled / unsupported type
            WriteBinary(info);
    }

    private static void WriteBinary(ResourceInfo info)
    {
        var index = 0;
        foreach (var part in info.Parts)
        {
            var filename = Path.Combine(info.OutputDir, $"{info.LogicalIndex:D4}_{info.BaseName}_part{index:D2}.bin");
            File.WriteAllBytes(filename, part);
            index++;
        }
    }

    private static void WriteAnimation(ResourceInfo info)
    {
        if (info.Parts.Count > 1)
            Console.Error.WriteLine($"[WARN] Animation {info.BaseName} contains unexpected parts. Skipping data.");

        var part = info.Parts[0];
        var outName = info.BaseName + ".anm2";
        var outputFile = Path.Combine(info.OutputDir, outName);

        File.WriteAllBytes(outputFile, part);
    }
    private static void WriteTexture(ResourceInfo info)
    {
        if (info.Parts.Count < 2)
        {
            Console.Error.WriteLine($"[WARN] Texture {info.BaseName} does not have enough parts.");
            return;
        }
        
        using var reader = new BinaryReader(new MemoryStream(info.Parts[0]));
        var textureHeader = reader.ReadStruct<RTextureInfo>();

        var infoFmt = ResourceTypeInfo.FormatInfo.Get(textureHeader.Format);
        uint pitchOrLinearSize;

        if (infoFmt.IsBlockCompressed)
        {
            // block count = ceil(width/4) * ceil(height/4)
            var blockWidth = (textureHeader.Width + 3) / 4;
            var blockHeight = (textureHeader.Height + 3) / 4;
            pitchOrLinearSize = (uint)(blockWidth * blockHeight * infoFmt.BlockSizeBytes);
        }
        else
        {
            pitchOrLinearSize = (uint)(textureHeader.Width * infoFmt.BytesPerPixel + 3 & ~3);
        }

        // DDS constants
        const uint DDS_MAGIC = 0x20534444; // "DDS "
        const uint DDS_HEADER_SIZE = 124;
        const uint DDSCAPS_TEXTURE = 0x1000;
        const uint DDSCAPS_MIPMAP = 0x00400000;
        const uint DDSCAPS_COMPLEX = 0x00000008;

        const uint DDSD_CAPS = 0x1;
        const uint DDSD_HEIGHT = 0x2;
        const uint DDSD_WIDTH = 0x4;
        const uint DDSD_PITCH = 0x8;
        const uint DDSD_PIXELFORMAT = 0x1000;
        const uint DDSD_MIPMAPCOUNT = 0x20000;
        const uint DDSD_LINEARSIZE = 0x80000;
        //const uint DDSD_DEPTH = 0x800000;

        var ddsFlags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;
        if (infoFmt.IsBlockCompressed)
            ddsFlags |= DDSD_LINEARSIZE;
        else
            ddsFlags |= DDSD_PITCH;

        if (textureHeader.MipLevels > 1)
            ddsFlags |= DDSD_MIPMAPCOUNT;

        // Clamp mip levels
        var mipCount = textureHeader.MipLevels == 0 ? 1u : textureHeader.MipLevels;

        var header = new DDS.DDS_HEADER
        {
            Size = DDS_HEADER_SIZE,
            Flags = ddsFlags,
            Height = textureHeader.Height,
            Width = textureHeader.Width,
            PitchOrLinearSize = pitchOrLinearSize,
            Depth = textureHeader.Depth,
            MipMapCount = mipCount,
            Reserved1 = new uint[11],
            PixelFormat = DDS.GetPixelFormat(textureHeader.Format),
            Caps = DDSCAPS_TEXTURE | (textureHeader.MipLevels > 1 ? DDSCAPS_MIPMAP | DDSCAPS_COMPLEX : 0),
            Caps2 = 0,
            Caps3 = 0,
            Caps4 = 0,
            Reserved2 = 0
        };

        // ReSharper disable InconsistentNaming
        const uint DDSCAPS2_VOLUME = 0x00200000;
        const uint DDSCAPS2_CUBEMAP = 0x00000200;
        const uint DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
        const uint DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
        const uint DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
        const uint DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
        const uint DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
        const uint DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
        const uint DDSCAPS2_CUBEMAP_ALLFACES = DDSCAPS2_CUBEMAP_POSITIVEX | DDSCAPS2_CUBEMAP_NEGATIVEX |
                                               DDSCAPS2_CUBEMAP_POSITIVEY | DDSCAPS2_CUBEMAP_NEGATIVEY |
                                               DDSCAPS2_CUBEMAP_POSITIVEZ | DDSCAPS2_CUBEMAP_NEGATIVEZ;
        // ReSharper restore InconsistentNaming

        switch (textureHeader.TexType)
        {
            case 1:
                header.Caps2 = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_ALLFACES;
                break;
            case 2:
                header.Caps2 = DDSCAPS2_VOLUME;
                break;
        }

        // extended DX10 header if needed
        var dx10Header = new DDS.DDS_HEADER_DX10
        {
            DxgiFormat = ResourceTypeInfo.GetDXGIFormat(textureHeader.Format),
            ResourceDimension = DDS.D3D10_RESOURCE_DIMENSION.Texture2D,
            MiscFlag = 0,
            ArraySize = 1,
            MiscFlags2 = 0
        };

        if (header.PixelFormat.FourCC == DDS.MakeFourCC("DX10"))
        {
            if (dx10Header.DxgiFormat == DDS.DXGI_FORMAT.DXGI_FORMAT_UNKNOWN)
            {
                Console.Error.WriteLine($"[WARN] Texture {info.BaseName}, with textureHeader.Format of {textureHeader.Format} does not have matching DxgiFormat.");
                return;
            }
        }
        else
        {
            Debug.WriteLine($"[INFO] Texture {info.BaseName}, with textureHeader.Format of {textureHeader.Format} supports only DX9.");
        }

        using var stream = new MemoryStream();
        //using var output = File.OpenWrite(outputFile);
        using var writer = new BinaryWriter(stream);
        
        // write magic
        writer.Write(DDS_MAGIC);

        // write header
        writer.Write(header.Size);
        writer.Write(header.Flags);
        writer.Write(header.Height);
        writer.Write(header.Width);
        writer.Write(header.PitchOrLinearSize);
        writer.Write(header.Depth);
        writer.Write(header.MipMapCount);

        foreach (var v in header.Reserved1)
            writer.Write(v);

        // pixel format
        writer.Write(header.PixelFormat.Size);
        writer.Write(header.PixelFormat.Flags);
        writer.Write(header.PixelFormat.FourCC);
        writer.Write(header.PixelFormat.RGBBitCount);
        writer.Write(header.PixelFormat.RBitMask);
        writer.Write(header.PixelFormat.GBitMask);
        writer.Write(header.PixelFormat.BBitMask);
        writer.Write(header.PixelFormat.ABitMask);

        // caps
        writer.Write(header.Caps);
        writer.Write(header.Caps2);
        writer.Write(header.Caps3);
        writer.Write(header.Caps4);
        writer.Write(header.Reserved2);

        // optional DX10
        if (header.PixelFormat.FourCC == DDS.MakeFourCC("DX10"))
        {
            writer.Write((uint)dx10Header.DxgiFormat);
            writer.Write((uint)dx10Header.ResourceDimension);
            writer.Write(dx10Header.MiscFlag);
            writer.Write(dx10Header.ArraySize);
            writer.Write(dx10Header.MiscFlags2);
        }

        // write texture data
        writer.Write(info.Parts[1]);
        writer.Flush();

        //write png
        var outputFile = Path.Combine(info.OutputDir, info.BaseName);
        if (info.BaseName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && Options.Current.EnablePngFixup)
        {
            stream.Position = 0;
            var ddsDecoder = new DdsDecoder();
            var texture = ddsDecoder.DecodeTexture(Configuration.Default, stream);
            
            //not going to mess with other formats like cubemap
            if (texture is FlatTexture flatTexture)
            {
                var mipMap = flatTexture.MipMaps.FirstOrDefault();
                if (mipMap != null)
                {
                    var image = mipMap.GetImage();
                    image.Save(outputFile);
                    return;
                }
            }
            
            //fall through if not handled
        }

        //write dds
        outputFile = Path.Combine(info.OutputDir, info.BaseName);
        using var fileStream = File.OpenWrite(outputFile);
        stream.Position = 0;
        stream.CopyTo(fileStream);

        //Debug.WriteLine($"[OUT] Wrote {outputFile} ({new FileInfo(outputFile).Length} bytes)");
    }

    private static void WriteMesh(ResourceInfo info)
    {
        if (info.Parts.Count != 5)
        {
            Console.WriteLine($"[ERROR] Mesh {info.BaseName} does not have 5 parts. Unhandled for now");
            return;
        }
        
        //modifies part0 to change "pointers" (starts at 1) to "offsets" (starts at 0)
        //this would usually be used in engine to convert the raw disk data into functional pointers the engine can use natively instead of reading everything individually
        var part0 = info.Parts[0];
        var fixups = MeshFixups.FromBytes(info.Parts[2]);

        // allocate a new buffer with +4 bytes padding so we can safely write 4 bytes at the end
        var outBuf = new byte[part0.Length + 4];
        Buffer.BlockCopy(part0, srcOffset: 0, outBuf, dstOffset: 0, part0.Length);

        var memSize = part0.Length;

        for (var i = 0; i < (int)fixups.numPointerResolves; i++)
        {
            var slot = (int)fixups.pointerResolveOffsets[i];

            // validate slot is readable as ushort in original buffer
            if (slot < 0 || slot + 2 > memSize)
            {
                // skip invalid/out-of-range slots
                Console.WriteLine($"Skip pointerResolveOffsets[{i}] = {slot} (out of range)");
                continue;
            }

            var raw = BitConverter.ToUInt64(outBuf, slot);
            var targetOffset = raw is 0 or 0xFFFFUL ? -1L : (long)(raw - 1);

            // Write a UInt64
            var toWrite = targetOffset < 0 ? 0xFFFFFFFFFFFFFFFFUL : (ulong)targetOffset;


            // inline WriteUInt32LE
            var b = BitConverter.GetBytes(toWrite);
            Buffer.BlockCopy(b, srcOffset: 0, outBuf, slot, count: 4);
        }

        //setup streams for part's 0 and 3
        using var ms0 = new MemoryStream(outBuf);
        using var ms3 = new MemoryStream(info.Parts[3]);
        using var ms4 = new MemoryStream(info.Parts[4]);
        
        using var reader0 = new BinaryReader(ms0);
        using var reader3 = new BinaryReader(ms3);
        using var reader4 = new BinaryReader(ms4);
        
        var mesh = reader0.ReadStruct<MeshFileInFile>();

        //read surface id's
        reader0.BaseStream.Position = (long)mesh.m_SurfaceParams;
        var surfaceIds = new SurfaceId[mesh.m_SurfacesCount];
        for (var i = 0; i < mesh.m_SurfacesCount; i++)
        {
            surfaceIds[i] = (SurfaceId)reader0.ReadUInt32();
        }
        
        //read MeshMaterial
        reader0.BaseStream.Position = (long)mesh.m_MaterialsDatabase;
        var mshMaterial = reader0.ReadStruct<MeshMaterial>();

        //read material slots
        reader0.BaseStream.Position = (long)mshMaterial.m_MaterialSlot;
        var materialSlots = reader0.ReadStructArray<MaterialSlot>(mshMaterial.m_MaterialCount);
        
        //read material names
        var mats = new List<string>();
        foreach (var slot in materialSlots)
        {
            // The UInt64 is the offset to the string
            reader0.BaseStream.Position = slot.m_MaterialName;
            var matName = reader0.ReadTerminatedString();
            mats.Add(matName);
        }
        
        //read nodes
        reader0.BaseStream.Position = (long)mesh.m_Nodes;
        var nodes = reader0.ReadStructArray<MeshEntityInFile>((int)mesh.m_NodesCount);
        
        var tree = new List<MshTree>();
        foreach (var n in nodes)
        {
            reader0.BaseStream.Position = (long)n.m_Name;
            var nodeName = reader0.ReadTerminatedString();
            
            var mshTree = new MshTree
            {
                Node = new MshNode
                {
                    Name = nodeName,
                    Parent = (short)n.m_Parent,
                    Type = n.MshType,
                    NumLods = n.m_LodsCount,
                    Local = n.m_LocalTM,
                    BoneTransform = n.m_BoneInitTM,
                    Bounds = n.m_Bounds,
                    Children = CountDescendants(n.m_NodeIdx, nodes) // total descendants
                },
                
                Mesh = []
            };
            
            if (n.m_NodeFormat == 0)
            {
                tree.Add(mshTree);
                continue;
            }
            
            //Read mesh formats
            reader0.BaseStream.Position = (long)n.m_NodeFormat;
            var fmt = reader0.ReadStructArray<NodeFormat>(n.m_LodsCount);
            
            foreach (var f in fmt)
            {
                reader0.BaseStream.Position = (long)f.m_MeshIndexCount;
                var indexCountPerSurface = reader0.ReadStructArray<uint>(f.m_ObjectCount_A);
                var indexCount = (int)indexCountPerSurface.Sum(i => i);
                var indexData = reader4.ReadStructArray<ushort>(indexCount);
                
                var mFmt = new MeshFmt
                {
                    NumVertices = f.m_VertexCount,
                    NumIndices = (uint)indexCount,
                    
                    //should eventually be data.SurfaceTypes.Length or whatever
                    NumSurfaces = f.m_ObjectCount_B, //IDK m_ObjectCount_B tends to == m_ObjectCount_A. maybe one is NumSurfaces and the other is indexCount.Length or smt
                    Indices = indexData,
                    Surfaces = new SurfaceDesc[f.m_ObjectCount_B]
                };
                
                uint currentOffset = 0;
                for (var i = 0; i < f.m_ObjectCount_B; i++)
                {
                    // handle mismatch between object count and indexCountPerSurface length
                    var count = (i < indexCountPerSurface.Length)
                        ? indexCountPerSurface[i]
                        : 0u;

                    mFmt.Surfaces[i] = new SurfaceDesc
                    {
                        //idk how to handle this for the moment
                        Bones = [],
                        MatId = 0,
                        NumBones = 0,

                        Offset = currentOffset,
                        Count = count
                    };

                    currentOffset += count;
                }

                
                float[] pos, tan, bitan, nrm, uv0, uv1;
                switch (f.m_VertexLayoutID)
                {
                    case 3:
                        var vertices = reader3.ReadStructArray<DlVertex32>((int)f.m_VertexCount);
                        (pos, tan, bitan, nrm, uv0, uv1) = DlVertex32.ExtractArrays(vertices, (int)f.m_VertexCount);
                        break;
                    
                    //0 (DlVertex16)
                    //8 DlVertex80
                    default:
                        Console.WriteLine($"[ERROR] Vertex layout { f.m_VertexLayoutID } not supported in mesh {info.BaseName}");
                        return;
                        //throw new Exception($"Vertex layout { f.m_VertexLayoutID } not supported in mesh {info.BaseName}");
                }
                
                // Convert the float[] data to byte[] before assigning, if mesh expects byte arrays
                mFmt.Vxyz[0].Data = new byte[pos.Length * sizeof(float)];
                Buffer.BlockCopy(pos, 0, mFmt.Vxyz[0].Data, 0, mFmt.Vxyz[0].Data.Length);
                
                mFmt.VNormal[0] = new byte[nrm.Length * sizeof(float)];
                Buffer.BlockCopy(nrm, 0, mFmt.VNormal[0], 0, mFmt.VNormal[0].Length);
                
                mFmt.VTangent[0] = new byte[tan.Length * sizeof(float)];
                Buffer.BlockCopy(tan, 0, mFmt.VTangent[0], 0, mFmt.VTangent[0].Length);
                
                mFmt.VBitangent[0] = new byte[bitan.Length * sizeof(float)];
                Buffer.BlockCopy(bitan, 0, mFmt.VBitangent[0], 0, mFmt.VBitangent[0].Length);
                
                mFmt.VUv[0] = new byte[uv0.Length * sizeof(float)];
                Buffer.BlockCopy(uv0, 0, mFmt.VUv[0], 0, mFmt.VUv[0].Length);
                
                mFmt.VUv[1] = new byte[uv1.Length * sizeof(float)];
                Buffer.BlockCopy(uv1, 0, mFmt.VUv[1], 0, mFmt.VUv[1].Length);
                
                mshTree.Mesh.Add(mFmt);
            }

            tree.Add(mshTree);
        }
        
        //create Msh Data for writer
        var data = new MshData
        {
            Tree = tree,
            Mats = mats,
            SurfaceTypes = surfaceIds.Select(id => id.ToString()).ToList(),
            Root = new MshRoot()
            {
                NumMaterials = mshMaterial.m_MaterialCount,
                NumNodes = mesh.m_NodesCount,
                NumSurfaceTypes = mshMaterial.m_SurfaceCount
            }
        };
        
        //pass to my mesh writer and write to file
        var outName = info.BaseName + ".msh";
        var outputFile = Path.Combine(info.OutputDir, outName);

        using var fsOut = File.OpenWrite(outputFile);
        var writer = new MshWriter(fsOut);

        //write msh data
        writer.MshSave(ref data);
        return;

        short CountDescendants(int parentIdx, MeshEntityInFile[] nodes)
        {
            short count = 0;

            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].m_Parent != parentIdx)
                    continue;

                count++;
                count += CountDescendants(i, nodes);
            }

            return count;
        }
    }
}