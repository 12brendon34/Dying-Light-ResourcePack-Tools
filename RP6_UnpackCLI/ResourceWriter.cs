using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Mesh.Format;
using Mesh.IO;
using RP6;
using RP6.Format;
using RP6.Format.CompactMesh;
using RP6.Format.ResourceDataPack;
using Utils.IO.Extensions;

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
        if (Handlers.TryGetValue(info.FileType, out var handler))
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

        var outputFile = Path.Combine(info.OutputDir, info.BaseName + ".dds");
        // = FileHelpers.MakeUniqueFilename(outputFile);

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

        using var output = File.OpenWrite(outputFile);
        using var outputWriter = new BinaryWriter(output);


        // write magic
        outputWriter.Write(DDS_MAGIC);

        // write header
        outputWriter.Write(header.Size);
        outputWriter.Write(header.Flags);
        outputWriter.Write(header.Height);
        outputWriter.Write(header.Width);
        outputWriter.Write(header.PitchOrLinearSize);
        outputWriter.Write(header.Depth);
        outputWriter.Write(header.MipMapCount);

        foreach (var v in header.Reserved1)
            outputWriter.Write(v);

        // pixel format
        outputWriter.Write(header.PixelFormat.Size);
        outputWriter.Write(header.PixelFormat.Flags);
        outputWriter.Write(header.PixelFormat.FourCC);
        outputWriter.Write(header.PixelFormat.RGBBitCount);
        outputWriter.Write(header.PixelFormat.RBitMask);
        outputWriter.Write(header.PixelFormat.GBitMask);
        outputWriter.Write(header.PixelFormat.BBitMask);
        outputWriter.Write(header.PixelFormat.ABitMask);

        // caps
        outputWriter.Write(header.Caps);
        outputWriter.Write(header.Caps2);
        outputWriter.Write(header.Caps3);
        outputWriter.Write(header.Caps4);
        outputWriter.Write(header.Reserved2);

        // optional DX10
        if (header.PixelFormat.FourCC == DDS.MakeFourCC("DX10"))
        {
            outputWriter.Write((uint)dx10Header.DxgiFormat);
            outputWriter.Write((uint)dx10Header.ResourceDimension);
            outputWriter.Write(dx10Header.MiscFlag);
            outputWriter.Write(dx10Header.ArraySize);
            outputWriter.Write(dx10Header.MiscFlags2);
        }

        // write texture data
        output.Write(info.Parts[1], offset: 0, info.Parts[1].Length);
        Debug.WriteLine($"[OUT] Wrote {outputFile} ({new FileInfo(outputFile).Length} bytes)");
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
            
            //remove later, not possible if read properly
            if (nodeName.Length > 64)
            {
                Console.WriteLine($"[ERROR] Node {nodeName} of size {nodeName.Length} is > 64");
            }
            
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

                
                float[] pos = null, tan = null, bitan = null, nrm = null, uv0 = null, uv1 = null;
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

    /*
    private static void WriteMesh2(ResourceInfo info)
    {
        var fixups = MeshFixups.FromBytes(info.Parts[2]);

        if (fixups.numVisibleResolves > 1)
            Console.WriteLine("numVisibleResolves > 1");

        using var ms0 = new MemoryStream(info.Parts[0]);
        using var reader = new BinaryReader(ms0);

        // map resolves by offset -> (resolve, index)
        var resolvesByOffset = new Dictionary<int, (CWObjectResolve resolve, int idx)>();
        for (int ri = 0; ri < fixups.numResolves; ri++)
            resolvesByOffset[fixups.resolves[ri].offset] = (fixups.resolves[ri], ri);

        // Build pointerFieldsByOwner: ownerResolveIdx -> list of (relOffset, targetOffset, targetResolveIdx?)
        var pointerFieldsByOwner = new Dictionary<int, List<(int relOffset, int targetOffset, int? targetResolveIdx)>>();
        for (var i = 0; i < fixups.numPointerResolves; i++)
        {
            var pointerOffset = (int)fixups.pointerResolveOffsets[i];

            reader.BaseStream.Position = pointerOffset;
            var rawValue = reader.ReadUInt16();

            var targetOffset = (rawValue == 0 || rawValue == 0xFFFF) ? -1 : (rawValue - 1);

            // find owner resolve index (last resolve whose offset <= pointerOffset)
            var ownerIdx = -1;
            for (var ri = 0; ri < fixups.numResolves; ri++)
            {
                if (fixups.resolves[ri].offset <= pointerOffset)
                    ownerIdx = ri;
                else
                    break;
            }

            var relOffset = pointerOffset - (ownerIdx >= 0 ? fixups.resolves[ownerIdx].offset : 0);

            int? targetResolveIdx = null;
            if (targetOffset >= 0 && resolvesByOffset.TryGetValue(targetOffset, out var targetResolveEntry))
                targetResolveIdx = targetResolveEntry.idx;

            if (!pointerFieldsByOwner.TryGetValue(ownerIdx, out var list))
            {
                list = new List<(int, int, int?)>();
                pointerFieldsByOwner[ownerIdx] = list;
            }

            list.Add((relOffset, targetOffset, targetResolveIdx));
        }

        // Collect mesh node info from part 0 resolves, but driven from root resolve
        var data = new MshData();

        uint nodeCount = 0;
        for (var i = 0; i < fixups.numResolves; i++)
        {
            var resolve = fixups.resolves[i];
            reader.BaseStream.Position = resolve.offset;

            var start = resolve.offset;
            var end = i + 1 < fixups.numResolves ? fixups.resolves[i + 1].offset : (int)fixups.memorySize;
            var resSize = end - start;

            Debug.WriteLine($"Resolve[{i}] offset=0x{resolve.offset:X} class=0x{resolve.class_id:X} numElems={resolve.num_elements}");

            // If this resolve is the root list (first class at beginning of part 0)
            if (resolve.class_id != 0xB0000003)
                continue;

            Debug.WriteLine("  Root Node (0xB0000003) - following its resolve pointers");

            // Assume the root contains `num_elements` 16-bit pointers starting at resolve.offset
            for (uint ei = 0; ei < resolve.num_elements; ei++)
            {
                // position at the ei-th pointer entry in the root block
                reader.BaseStream.Position = resolve.offset + (ei * 2);

                var rawPtr = reader.ReadUInt16();
                var targetOffset = rawPtr is 0 or 0xFFFF ? -1 : (rawPtr - 1);

                if (targetOffset < 0)
                {
                    Debug.WriteLine($"    Root entry[{ei}] -> null");
                    continue;
                }

                if (!resolvesByOffset.TryGetValue(targetOffset, out var targetResolveEntry))
                {
                    Debug.WriteLine($"    Root entry[{ei}] -> unknown target offset 0x{targetOffset:X}");
                    continue;
                }

                var targetResolve = targetResolveEntry.resolve;
                var targetIdx = targetResolveEntry.idx;

                Debug.WriteLine($"    Root entry[{ei}] -> Resolve[{targetIdx}] offset=0x{targetResolve.offset:X} class=0x{targetResolve.class_id:X}");

                // Handle the target resolve by its class
                switch (targetResolve.class_id)
                {
                    case 0xB0000004:
                    case 0xA0000002:
                        Debug.WriteLine("      <mesh node block>");

                        nodeCount++;
                        if (nodeCount > 1)
                            Console.WriteLine("MultiNodeMesh");

                        // Seek to the target resolve data and read structs
                        reader.BaseStream.Position = targetResolve.offset;

                        var tree = new MshTree
                        {
                            Node = new MshNode
                            {
                                Type = MshType.Mesh,
                                Local = reader.ReadStruct<Matrix3X4>(),
                                BoneTransform = reader.ReadStruct<Matrix3X4>(),
                                Bounds = reader.ReadStruct<Aabb>()
                            },
                            Index = nodeCount
                        };

                        // If there are pointer fields for this resolve, attempt to read the first pointer as name
                        if (pointerFieldsByOwner.TryGetValue(targetIdx, out var fields) && fields.Count > 0)
                        {
                            var nameTargetOffset = fields[0].targetOffset;
                            if (nameTargetOffset >= 0)
                            {
                                reader.BaseStream.Position = nameTargetOffset;
                                tree.Node.Name = reader.ReadTerminatedString();
                            }
                        }

                        data.Tree.Add(tree);
                        break;

                    case 0xB000000C:
                    case 0xA0000001:
                        Debug.WriteLine("      <packed section> dumped (ignored for now)");
                        break;

                    case 0xB0000000:
                    case 0xA0000000:
                        // text/trivial classes
                        Debug.WriteLine("      <text/trivial>");
                        break;

                    default:
                        Debug.WriteLine($"      <unhandled class 0x{targetResolve.class_id:X}>");
                        break;
                }
            } // end root entries loop

            // After processing root we can stop iterating resolves (root was first and drives everything)
            break;

            // end if root
            // otherwise, non-root resolves are ignored here because we're driven from root
        } // end resolves loop

        // (At this point `data` contains parsed MshTree entries per the root.)
        // TODO: write or return `data` as required by the rest of your pipeline.
        Console.WriteLine(data.Mats.Count);
    }

    //old, reimplement properly.
    private static void WriteMesh(ResourceInfo info)
    {
        var fixups = MeshFixups.FromBytes(info.Parts[2]);

        if (fixups.numVisibleResolves > 1)
            Console.WriteLine("numVisibleResolves > 1");

        using var ms0 = new MemoryStream(info.Parts[0]);
        using var reader = new BinaryReader(ms0);

        // map resolves by offset (int) for quick lookup
        var resolvesByOffset = fixups.resolves.ToDictionary(r => r.offset, r => r);

        // Build pointerFieldsByOwner properly
        var pointerFieldsByOwner = new Dictionary<int, List<(int relOffset, int targetOffset, int? targetResolveIdx)>>();

        for (var i = 0; i < fixups.numPointerResolves; i++)
        {
            var pointerOffset = (int)fixups.pointerResolveOffsets[i];

            reader.BaseStream.Position = pointerOffset;
            var rawValue = reader.ReadUInt16();

            var targetOffset = rawValue is 0 or 0xFFFF ? -1 : rawValue - 1;

            var ownerIdx = -1;
            for (var ri = 0; ri < fixups.numResolves; ri++)
                if (fixups.resolves[ri].offset <= pointerOffset)
                    ownerIdx = ri;
                else
                    break;

            var relOffset = pointerOffset - (ownerIdx >= 0 ? fixups.resolves[ownerIdx].offset : 0);

            int? targetResolveIdx = null;
            if (targetOffset >= 0 && resolvesByOffset.TryGetValue(targetOffset, out var targetResolve))
                targetResolveIdx = Array.IndexOf(fixups.resolves, targetResolve);

            if (!pointerFieldsByOwner.TryGetValue(ownerIdx, out var list))
            {
                list = [];
                pointerFieldsByOwner[ownerIdx] = list;
            }

            list.Add((relOffset, targetOffset, targetResolveIdx));
        }

        // Collect mesh node info from part 0 resolves
        var data = new MshData();

        uint nodeCount = 0;
        for (var i = 0; i < fixups.numResolves; i++)
        {
            var resolve = fixups.resolves[i];
            reader.BaseStream.Position = resolve.offset;

            var start = resolve.offset;
            var end = i + 1 < fixups.numResolves
                ? fixups.resolves[i + 1].offset
                : (int)fixups.memorySize;
            var resSize = end - start;

            Debug.WriteLine($"Resolve[{i}] offset=0x{resolve.offset:X} class=0x{resolve.class_id:X}");

            // find pointer fields that belong to this resolve (if any)
            pointerFieldsByOwner.TryGetValue(i, out var fields);

            switch (resolve.class_id)
            {
                case 0xB0000000:
                case 0xA0000000:
                    // text / trivial classes
                    break;

                case 0xB0000004:
                case 0xA0000002:
                    Debug.WriteLine("  <mesh node block>");
                    nodeCount++;

                    if (nodeCount > 1)
                        Console.WriteLine("MultiNodeMesh");

                    var tree = new MshTree
                    {
                        Node = new MshNode
                        {
                            Type = MshType.Mesh,
                            Local = reader.ReadStruct<Matrix3X4>(),
                            BoneTransform = reader.ReadStruct<Matrix3X4>(),
                            Bounds = reader.ReadStruct<Aabb>()
                        },
                        Index = nodeCount
                    };

                    if (fields is { Count: > 0 })
                    {
                        reader.BaseStream.Position = fields[0].targetOffset;

                        tree.Node.Name = reader.ReadTerminatedString();
                    }


                    data.Tree.Add(tree);
                    break;

                case 0xB000000C:
                case 0xA0000001:
                    Debug.WriteLine("  <packed section> dumped (ignored for now)");
                    break;
            }
        }

        using var ms3 = new MemoryStream(info.Parts[3]);
        using var ms3Reader = new BinaryReader(ms3);

        var vertexSize = Marshal.SizeOf<DlVertex32>();
        if (ms3.Length % vertexSize != 0)
            throw new InvalidDataException("Buffer length is not a multiple of DL_Vertex32 size.");

        var count = (int)(ms3.Length / vertexSize);
        var verts = ms3Reader.ReadStructArray<DlVertex32>(count);






        foreach (var mesh in data.Tree)
        {
            if (mesh.Node.Type != MshType.Mesh)
                continue;

            var mFmt = new MeshFmt();


            mesh.Mesh.Add(mFmt);
        }


        var outName = info.BaseName + ".msh";
        var outputFile = Path.Combine(info.OutputDir, outName);
        //outputFile = FileHelpers.MakeUniqueFilename(outputFile);



        using var fsOut = File.OpenWrite(outputFile);
        var writer = new MshWriter(fsOut);

        //write msh data
        writer.MshSave(ref data);
    }

    private static void WriteMesh(ResourceInfo info)
    {
        var fixups = MeshFixups.FromBytes(info.Parts[2]);

        if (fixups.numVisibleResolves > 1)
            Console.WriteLine("numVisibleResolves > 1");

        using var ms0 = new MemoryStream(info.Parts[0]);
        using var reader = new BinaryReader(ms0);

        // map resolves by offset (int) for quick lookup
        var resolvesByOffset = fixups.resolves.ToDictionary(r => r.offset, r => r);

        // Build pointerFieldsByOwner properly
        var pointerFieldsByOwner =
            new Dictionary<int, List<(int relOffset, int targetOffset, int? targetResolveIdx)>>();

        for (var i = 0; i < fixups.numPointerResolves; i++)
        {
            var pointerOffset = (int)fixups.pointerResolveOffsets[i];

            reader.BaseStream.Position = pointerOffset;
            var rawValue = reader.ReadUInt16();

            var targetOffset = rawValue is 0 or 0xFFFF ? -1 : rawValue - 1;

            var ownerIdx = -1;
            for (var ri = 0; ri < fixups.numResolves; ri++)
                if (fixups.resolves[ri].offset <= pointerOffset)
                    ownerIdx = ri;
                else
                    break;

            var relOffset = pointerOffset - (ownerIdx >= 0 ? fixups.resolves[ownerIdx].offset : 0);

            int? targetResolveIdx = null;
            if (targetOffset >= 0 && resolvesByOffset.TryGetValue(targetOffset, out var targetResolve))
                targetResolveIdx = Array.IndexOf(fixups.resolves, targetResolve);

            if (!pointerFieldsByOwner.TryGetValue(ownerIdx, out var list))
            {
                list = [];
                pointerFieldsByOwner[ownerIdx] = list;
            }

            list.Add((relOffset, targetOffset, targetResolveIdx));
        }

        // Collect mesh node info from part 0 resolves
        var data = new MshData
        {
            Tree = [],
            Mats = [],
            SurfaceTypes = []
        };

        uint nodeCount = 0;

        for (var i = 0; i < fixups.numResolves; i++)
        {
            var resolve = fixups.resolves[i];
            reader.BaseStream.Position = resolve.offset;

            var start = resolve.offset;
            var end = i + 1 < fixups.numResolves
                ? fixups.resolves[i + 1].offset
                : (int)fixups.memorySize;
            var resSize = end - start;

            Debug.WriteLine($"Resolve[{i}] offset=0x{resolve.offset:X} class=0x{resolve.class_id:X}");

            // find pointer fields that belong to this resolve (if any)
            pointerFieldsByOwner.TryGetValue(i, out var fields);

            switch (resolve.class_id)
            {
                case 0xB0000000:
                case 0xA0000000:
                    // text / trivial classes
                    break;

                case 0xB0000004:
                case 0xA0000002:
                    Debug.WriteLine("  <mesh node block>");
                    nodeCount++;

                    if (nodeCount > 1)
                        Console.WriteLine("MultiNodeMesh");

                    MshTree tree = default;

                    tree.Node = new MshNode();
                    tree.Node.Type = MshType.Mesh;

                    tree.Index = nodeCount;

                    // read inline matrices/bounds
                    tree.Node.Local = StreamHelpers.ReadStruct<Matrix3X4>(reader.BaseStream);
                    tree.Node.BoneTransform = StreamHelpers.ReadStruct<Matrix3X4>(reader.BaseStream);
                    tree.Node.Bounds = StreamHelpers.ReadStruct<Aabb>(reader.BaseStream);

                    if (fields is { Count: > 0 })
                    {
                        reader.BaseStream.Position = fields[0].targetOffset;

                        tree.Node.Name = StreamHelpers.ReadCString(reader.BaseStream, Encoding.ASCII);
                    }


                    data.Tree.Add(tree);
                    break;

                case 0xB000000C:
                case 0xA0000001:
                    Debug.WriteLine("  <packed section> dumped (ignored for now)");
                    break;
            }
        }

        using var ms3 = new MemoryStream(info.Parts[3]);
        var vertexSize = Marshal.SizeOf<DlVertex32>();
        if (ms3.Length % vertexSize != 0)
            throw new InvalidDataException("Buffer length is not a multiple of DL_Vertex32 size.");

        var count = (int)(ms3.Length / vertexSize);
        var verts = StreamHelpers.ReadArray<DlVertex32>(ms3, count);

        var positionBuffer = new float[count * 3];
        var tangentData = new float[count * 3];
        var bitangentData = new float[count * 3];
        var normalData = new float[count * 3];
        var uv0Array = new float[count * 2];
        var uv1Array = new float[count * 2];

        for (var i = 0; i < count; i++)
        {
            var v = verts[i];
            positionBuffer[i * 3 + 0] = v.PX;
            positionBuffer[i * 3 + 1] = v.PY;
            positionBuffer[i * 3 + 2] = v.PZ;

            var qx = v.QShort[0] / 32767.0f;
            var qy = v.QShort[1] / 32767.0f;
            var qz = v.QShort[2] / 32767.0f;
            var qw = v.QShort[3] / 32767.0f;

            Quaternion q = new(qx, qy, qz, qw);
            q = Quaternion.Normalize(q);

            var tangent = Vector3.Transform(Vector3.UnitX, q);
            var bitangent = Vector3.Transform(Vector3.UnitY, q);
            var normal = Vector3.Transform(Vector3.UnitZ, q);

            tangentData[i * 3 + 0] = tangent.X;
            tangentData[i * 3 + 1] = tangent.Y;
            tangentData[i * 3 + 2] = tangent.Z;

            bitangentData[i * 3 + 0] = bitangent.X;
            bitangentData[i * 3 + 1] = bitangent.Y;
            bitangentData[i * 3 + 2] = bitangent.Z;

            normalData[i * 3 + 0] = normal.X;
            normalData[i * 3 + 1] = normal.Y;
            normalData[i * 3 + 2] = normal.Z;

            uv0Array[i * 2] = (float)v.HalfUV0U;
            uv0Array[i * 2 + 1] = (float)v.HalfUV0V;

            uv1Array[i * 2] = (float)v.HalfUV1U;
            uv1Array[i * 2 + 1] = (float)v.HalfUV1V;
        }

        var surfaceDesc = new SurfaceDescManaged
        {
            MatId = 0,
            Offset = 0,
            Count = 0,
            NumBones = 0
        };

        var vertexFormat = new VertexFormat
        {
            Fmt = MvFmt.Float3,
            BiasScale = new Vec4 { X = 0, Y = 0, Z = 0, W = 1 },
            Stride = 12,
            VNormalFmt = MvFmt.Float3,
            VNormalScale = 1f,
            VNormalStride = 12,
            VTangentFmt = MvFmt.Float3,
            VTangentScale = 1f,
            VTangentStride = 12,
            VUvFmt = MvFmt.Float2,
            VUvScale = 1f,
            VUvStride = 8
        };

        // Build the MTOOl_FMT group
        byte[] mtoolGroupBytes;
        using (var mtoolStream = new MemoryStream())
        {
            // MshMesh struct (mtool header data)
            var mtool = new MshMesh
            {
                NumIndices = (uint)(info.Parts[4].Length / sizeof(short)),
                NumVertices = (uint)count,
                NumSurfaces = 1
            };

            // Write MshMesh (this is the "data" of the MTOOl_FMT chunk)
            //I don't write the chunk struct here, as I'll do that when writing the node group
            StreamHelpers.WriteStruct(mtoolStream, mtool);

            // Child chunks for MTOOl_FMT
            // VertexFormat chunk
            var vertexFormatChunk = new Chunk
            {
                Id = ChunkTypes.VertexFormat,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + Marshal.SizeOf<VertexFormat>()),
                DataSize = (uint)Marshal.SizeOf<VertexFormat>()
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexFormatChunk);
            StreamHelpers.WriteStruct(mtoolStream, vertexFormat);

            // VertexBuffer chunk
            var vertexBufferChunk = new Chunk
            {
                Id = ChunkTypes.VertexBuffer,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + positionBuffer.Length * sizeof(float)),
                DataSize = (uint)(positionBuffer.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexBufferChunk);
            StreamHelpers.WriteArray(mtoolStream, positionBuffer);

            // Normals / Tangents / Bitangent / UVs
            var vertexNormalChunk = new Chunk
            {
                Id = ChunkTypes.VertexNormal0,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + normalData.Length * sizeof(float)),
                DataSize = (uint)(normalData.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexNormalChunk);
            StreamHelpers.WriteArray(mtoolStream, normalData);

            var vertexTangentChunk = new Chunk
            {
                Id = ChunkTypes.VertexTangent0,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + tangentData.Length * sizeof(float)),
                DataSize = (uint)(tangentData.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexTangentChunk);
            StreamHelpers.WriteArray(mtoolStream, tangentData);

            var vertexBitangentChunk = new Chunk
            {
                Id = ChunkTypes.VertexBitangent0,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + bitangentData.Length * sizeof(float)),
                DataSize = (uint)(bitangentData.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexBitangentChunk);
            StreamHelpers.WriteArray(mtoolStream, bitangentData);

            var vertexUv0Chunk = new Chunk
            {
                Id = ChunkTypes.VertexUV0,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + uv0Array.Length * sizeof(float)),
                DataSize = (uint)(uv0Array.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexUv0Chunk);
            StreamHelpers.WriteArray(mtoolStream, uv0Array);

            var vertexUv1Chunk = new Chunk
            {
                Id = ChunkTypes.VertexUV1,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + uv1Array.Length * sizeof(float)),
                DataSize = (uint)(uv1Array.Length * sizeof(float))
            };
            StreamHelpers.WriteStruct(mtoolStream, vertexUv1Chunk);
            StreamHelpers.WriteArray(mtoolStream, uv1Array);

            // Index buffer chunk
            var indexBufferChunk = new Chunk
            {
                Id = ChunkTypes.IndexBuffer,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + info.Parts[4].Length),
                DataSize = (uint)info.Parts[4].Length
            };
            StreamHelpers.WriteStruct(mtoolStream, indexBufferChunk);
            mtoolStream.Write(info.Parts[4], 0, info.Parts[4].Length);

            // SurfaceDescAlt chunk
            var surfaceDescAltChunk = new Chunk
            {
                Id = ChunkTypes.SurfaceDescAlt,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + Marshal.SizeOf<SurfaceDescManaged>()),
                DataSize = (uint)Marshal.SizeOf<SurfaceDescManaged>()
            };
            StreamHelpers.WriteStruct(mtoolStream, surfaceDescAltChunk);
            StreamHelpers.WriteStruct(mtoolStream, surfaceDesc);

            mtoolGroupBytes = mtoolStream.ToArray();
        }

        // For each node, build a Node chunk that contains a MTOOl_FMT chunk
        using var nodeGroupStream = new MemoryStream();
        for (var i = 0; i < data.Tree.Count; i++)
        {
            var node = data.Tree[i].Node;

            var mtoolDataSize = (uint)Marshal.SizeOf<MshMesh>();
            var mtoolChunk = new Chunk
            {
                Id = ChunkTypes.MTOOl_FMT,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + mtoolGroupBytes.Length),
                DataSize = mtoolDataSize
            };

            using var nodePayload = new MemoryStream();
            StreamHelpers.WriteStruct(nodePayload, node);

            StreamHelpers.WriteStruct(nodePayload, mtoolChunk);
            nodePayload.Write(mtoolGroupBytes, 0, mtoolGroupBytes.Length);

            var nodePayloadBytes = nodePayload.ToArray();
            var nodeChunk = new Chunk
            {
                Id = ChunkTypes.NodeV3,
                Version = 0,
                ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + nodePayloadBytes.Length),
                DataSize = (uint)Marshal.SizeOf<MshNode>()
            };

            StreamHelpers.WriteStruct(nodeGroupStream, nodeChunk);
            nodeGroupStream.Write(nodePayloadBytes, 0, nodePayloadBytes.Length);
        }

        // Build final root header chunk and write a single .msh file
        var nodeGroupBytes = nodeGroupStream.ToArray();

        data.Root = new MshRoot
        {
            NumNodes = nodeCount,
            NumMaterials = 0,
            NumSurfaceTypes = 0
        };

        var header = new Chunk
        {
            Id = ChunkTypes.Header,
            Version = 0,
            ChunkSize = (uint)(Marshal.SizeOf<Chunk>() + Marshal.SizeOf<MshRoot>() + nodeGroupBytes.Length),
            DataSize = (uint)Marshal.SizeOf<MshRoot>()
        };

        var outName = info.BaseName + ".msh";
        var outputFile = Path.Combine(info.OutputDir, outName);
        outputFile = FileHelpers.MakeUniqueFilename(outputFile);

        using (var output = File.OpenWrite(outputFile))
        {
            StreamHelpers.WriteStruct(output, header);
            StreamHelpers.WriteStruct(output, data.Root);
            output.Write(nodeGroupBytes, 0, nodeGroupBytes.Length);
        }

        Console.WriteLine($"Wrote {outputFile}");
    }
    * /

    //may not be used anymore
    private static void WriteBuilderInformation(ResourceInfo info)
    {
        for (var f = 0; f < info.Parts.Count; f++)
        {
            var part = info.Parts[f];
            var outName = info.BaseName + ".txt";
            if (f >= 1) outName = info.BaseName + "_Part_" + f + ".txt";

            var outputFile = Path.Combine(info.OutputDir, outName);
            outputFile = FileHelpers.MakeUniqueFilename(outputFile);

            File.WriteAllBytes(outputFile, part);
        }
    }
    */
}