using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Mesh.Format;
using Utils.IO.Extensions;

namespace Mesh.IO;

public class MshReader
{
    private readonly BinaryReader _br;
    private readonly int _chunkHeaderSize;
    private readonly Stream _stream;

    public MshReader(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _br = new BinaryReader(_stream, Encoding.ASCII, leaveOpen: true);
        _chunkHeaderSize = Marshal.SizeOf<FChunk>();
    }

    public void Dispose()
    {
        _br.Dispose();
    }

    public void MshLoad(ref MshData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Read root chunk header
        var rootHeader = ReadChunkHeader();

        if (rootHeader.Id != ChunkTypes.MSH_ROOT)
            throw new InvalidDataException("Not a valid .MSH file.");

        // Validate root header size
        var rootStart = _stream.Position - _chunkHeaderSize;
        var rootEnd = CheckedChunkEnd(rootStart, rootHeader);

        // Read inline root data
        data.Root = _br.ReadStruct<MshRoot>();

        // Pre-allocate if reasonable
        if (data.Root.NumMaterials > 0)
            data.Mats = new List<string>((int)data.Root.NumMaterials);
        if (data.Root.NumSurfaceTypes > 0)
            data.SurfaceTypes = new List<string>((int)data.Root.NumSurfaceTypes);

        // Loop over child chunks contained within root chunk
        while (_stream.Position < rootEnd)
        {
            var child = ReadChunkHeader();
            var childStart = _stream.Position - _chunkHeaderSize;
            var childEnd = CheckedChunkEnd(childStart, child);

            switch (child.Id)
            {
                case ChunkTypes.MSH_MATERIALS:
                    for (var i = 0; i < data.Root.NumMaterials; i++)
                    {
                        var name = _br.ReadTerminatedString();
                        data.Mats.Add(name);


                        if (_stream.Position < childEnd)
                            _stream.Position = childEnd;
                    }

                    break;

                case ChunkTypes.MSH_SURFACE_TYPES:
                    for (var i = 0; i < data.Root.NumSurfaceTypes; i++)
                    {
                        var name = _br.ReadTerminatedString();
                        data.SurfaceTypes.Add(name);


                        if (_stream.Position < childEnd)
                            _stream.Position = childEnd;
                    }

                    break;

                case ChunkTypes.MSH_NODE_OLD:
                    Debug.WriteLine("NodeV1: unsupported; skipping entire chunk.");
                    SkipToChunkEnd(childEnd);
                    break;

                case ChunkTypes.MSH_NODE_OLD_MTX43_BY_ROWS:
                case ChunkTypes.MSH_NODE:
                {
                    var currentTree = new MshTree();
                    ReadNode(ref currentTree, childEnd);
                    data.Tree.Add(currentTree);
                    break;
                }

                case ChunkTypes.MSH_COLLTREE_GEOM:
                case ChunkTypes.MSH_COLLTREE_HULL:
                    Debug.WriteLine("Skipping Coll Tree, unsupported.");
                    SkipToChunkEnd(childEnd);
                    break;

                default:
                    Debug.WriteLine($"Root chunk skipped: {child.Id}");
                    SkipToChunkEnd(childEnd);
                    break;
            }
        }

        Debug.WriteLine("Done.");
    }

    private void ReadNode(ref MshTree tree, long nodeEnd)
    {
        tree.Node = _br.ReadStruct<MshNode>();
        Debug.WriteLine($"Node: {tree.Node.Name}");

        // iterate children inside this node
        while (_stream.Position < nodeEnd)
        {
            var child = ReadChunkHeader();
            var childStart = _stream.Position - _chunkHeaderSize;
            var childEnd = CheckedChunkEnd(childStart, child);

            Debug.WriteLine($"  Node child: {child.Id}");

            switch (child.Id)
            {
                case ChunkTypes.MSH_MESH:
                {
                    // read mesh header inline
                    var mesh = _br.ReadStruct<MshMesh>();

                    var mFmt = new MeshFmt
                    {
                        NumIndices = mesh.NumIndices,
                        NumVertices = mesh.NumVertices,
                        NumSurfaces = mesh.NumSurfaces
                    };

                    // iterate subchunks inside Mesh chunk
                    while (_stream.Position < childEnd)
                    {
                        var mshchild = ReadChunkHeader();
                        var mshChildStart = _stream.Position - _chunkHeaderSize;
                        var mshChildEnd = CheckedChunkEnd(mshChildStart, mshchild);
                        Debug.WriteLine($"    Mesh child: {mshchild.Id}");

                        switch (mshchild.Id)
                        {
                            case ChunkTypes.MSH_VFORMAT:

                                //change to read struct <VertexFormat>, instead of manual reads

                                mFmt.Vxyz[0].Fmt = (MvFmt)_br.ReadUInt32();

                                mFmt.Vxyz[0].BiasScale.X = _br.ReadSingle();
                                mFmt.Vxyz[0].BiasScale.Y = _br.ReadSingle();
                                mFmt.Vxyz[0].BiasScale.Z = _br.ReadSingle();
                                mFmt.Vxyz[0].BiasScale.W = _br.ReadSingle();

                                mFmt.Vxyz[0].Stride = _br.ReadUInt32();

                                //copy 0->1
                                mFmt.Vxyz[1] = mFmt.Vxyz[0];

                                mFmt.VNormalFmt = (MvFmt)_br.ReadUInt32();
                                mFmt.VNormalScale = _br.ReadSingle();
                                mFmt.VNormalStride = _br.ReadUInt32();

                                mFmt.VTangentFmt = (MvFmt)_br.ReadUInt32();
                                mFmt.VBitangentFmt = mFmt.VTangentFmt;

                                mFmt.VTangentScale = _br.ReadSingle();
                                mFmt.VBitangentScale = mFmt.VTangentScale;

                                mFmt.VTangentStride = _br.ReadUInt32();
                                mFmt.VBitangentStride = mFmt.VTangentStride;

                                mFmt.VUvFmt = (MvFmt)_br.ReadUInt32();
                                mFmt.VUvScale = _br.ReadSingle();
                                mFmt.VUvStride = _br.ReadUInt32();
                                break;

                            case ChunkTypes.MSH_VERTICES:
                                mFmt.Vxyz[0].Data = _br.ReadBytes((int)mshchild.DataSize);
                                break;
                            
                            case ChunkTypes.MSH_VERTICES1:
                                mFmt.Vxyz[1].Data = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_NORMALS:
                                mFmt.VNormal[0] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_NORMALS1:
                                mFmt.VNormal[1] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_TANGENTS:
                                mFmt.VTangent[0] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_TANGENTS1:
                                mFmt.VTangent[1] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_BITANGENTS0:
                                mFmt.VBitangent[0] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_BITANGENTS1:
                                mFmt.VBitangent[1] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_COLORS0:
                            case ChunkTypes.MSH_COLORS1:
                            case ChunkTypes.MSH_COLORS2:
                            case ChunkTypes.MSH_COLORS3:
                            {
                                var count = (int)mshchild.DataSize / Marshal.SizeOf<col4b>();
                                var colors = _br.ReadStructArray<col4b>(count);

                                var idx = mshchild.Id switch
                                {
                                    ChunkTypes.MSH_COLORS0 => 0,
                                    ChunkTypes.MSH_COLORS1 => 1,
                                    ChunkTypes.MSH_COLORS2 => 2,
                                    ChunkTypes.MSH_COLORS3 => 3,
                                    _ => 0
                                };
                                mFmt.VColor[idx] = colors;
                                break;
                            }

                            case ChunkTypes.MSH_UV0:
                                mFmt.VUv[0] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_UV1:
                                mFmt.VUv[1] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_UV2:
                                mFmt.VUv[2] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_UV3:
                                mFmt.VUv[3] = _br.ReadBytes((int)mshchild.DataSize);
                                break;

                            case ChunkTypes.MSH_INDICES:
                            {
                                var indexCount = (int)mshchild.DataSize / sizeof(ushort);
                                var values = new ushort[indexCount];
                                for (var i = 0; i < indexCount; i++)
                                {
                                    values[i] = _br.ReadUInt16();
                                }

                                mFmt.Indices = values;
                                break;
                            }
                            
                            case ChunkTypes.MSH_SURFACES1:
                            {
                                var surfaces = new SurfaceDesc[mesh.NumSurfaces];
                                for (var i = 0; i < mesh.NumSurfaces; i++)
                                {
                                    var s = new SurfaceDesc
                                    {
                                        MatId = _br.ReadUInt16(),
                                        Offset = _br.ReadUInt32(),
                                        Count = _br.ReadUInt32(),
                                        NumBones = _br.ReadUInt16()
                                    };

                                    if (s.NumBones > 0)
                                    {
                                        s.Bones = new ushort[s.NumBones];
                                        for (var b = 0; b < s.NumBones; b++)
                                            s.Bones[b] = _br.ReadUInt16();
                                    }
                                    else
                                    {
                                        s.Bones = [];
                                    }

                                    surfaces[i] = s;
                                }

                                mFmt.Surfaces = surfaces;
                                break;
                            }
                            
                            default:
                                Debug.WriteLine($"    Unimplemented mesh child: {mshchild.Id}.");
                                SkipToChunkEnd(mshChildEnd);
                                break;
                        }
                    }

                    // Add the parsed mesh to the tree
                    tree.Mesh.Add(mFmt);
                    break;
                }

                    case ChunkTypes.MSH_COLLTREE_GEOM:
                    case ChunkTypes.MSH_COLLTREE_HULL:
                    Debug.WriteLine("Skipping Coll Tree inside node.");
                    SkipToChunkEnd(childEnd);
                    break;

                default:
                    Debug.WriteLine($"Node chunk skipped: {child.Id}");
                    SkipToChunkEnd(childEnd);
                    break;
            }
        }
    }


    //kinda unnecessary, but looks nice

    #region Helpers

    private FChunk ReadChunkHeader()
    {
        var header = _br.ReadStruct<FChunk>();
        return header;
    }

    private long CheckedChunkEnd(long chunkStart, FChunk chunk)
    {
        // Compute the absolute end of the chunk and validate boundaries
        var chunkSize = (long)chunk.ChunkSize;
        if (chunkSize < _chunkHeaderSize)
            throw new InvalidDataException($"Invalid chunk size: {chunkSize}");

        var chunkEnd = chunkStart + chunkSize;

        if (chunkEnd > _stream.Length)
            throw new InvalidDataException("Chunk end extends beyond stream length.");

        return chunkEnd;
    }

    private void SkipToChunkEnd(long chunkEnd)
    {
        // Move stream to the end of the chunk.
        if (_stream.Position > chunkEnd)
        {
            // This shouldn't happen
            Debug.WriteLine("Warning: current stream position already past chunk end.");
            return;
        }

        _stream.Position = chunkEnd;
    }

    #endregion
}