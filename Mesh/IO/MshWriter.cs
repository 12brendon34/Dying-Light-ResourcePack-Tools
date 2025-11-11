using System.Text;
using Mesh.Format;
using Utils.IO.Extensions;

namespace Mesh.IO;

public class MshWriter
{
    private readonly BinaryWriter _bw;
    private readonly ChunkWriter _chunkWriter;
    private readonly FileStream _stream;

    public MshWriter(FileStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _bw = new BinaryWriter(_stream, Encoding.ASCII, leaveOpen: true);
        _chunkWriter = new ChunkWriter(_stream, _bw);
    }

    public void Dispose()
    {
        _chunkWriter?.Dispose();
        _bw?.Dispose();
    }

    public void MshSave(ref MshData data)
    {
        _chunkWriter.BeginChunk(ChunkTypes.MSH_ROOT);
        _bw.WriteStruct(data.Root);

        if (data.Root.NumMaterials > 0)
        {
            _chunkWriter.BeginChunk(ChunkTypes.MSH_MATERIALS);
            foreach (var m in data.Mats)
            {
                var bytes = Encoding.ASCII.GetBytes(m);
                //set to 64 bytes
                Array.Resize(ref bytes, newSize: 64);
                _bw.Write(bytes);
            }

            _chunkWriter.EndChunk();
        }

        if (data.Root.NumSurfaceTypes > 0)
        {
            _chunkWriter.BeginChunk(ChunkTypes.MSH_SURFACE_TYPES);
            foreach (var m in data.SurfaceTypes)
            {
                var bytes = Encoding.ASCII.GetBytes(m);
                //set to 64 bytes
                Array.Resize(ref bytes, newSize: 64);
                _bw.Write(bytes);
            }

            _chunkWriter.EndChunk();
        }

        SaveTree(data.Tree);

        // write data.Tree CollTree's
        // //public ICollTree CollGeom;
        // //public ICollTree CollHull;
        _chunkWriter.EndChunk();
    }

    private void SaveTree(List<MshTree> tree)
    {
        foreach (var meshTree in tree)
        {
            _chunkWriter.BeginChunk(ChunkTypes.MSH_NODE);
            _bw.WriteStruct(meshTree.Node);

            foreach (var mesh in meshTree.Mesh)
            {
                _chunkWriter.BeginChunk(ChunkTypes.MSH_MESH);

                _bw.WriteStruct(new MshMesh
                {
                    NumIndices = mesh.NumIndices,
                    NumVertices = mesh.NumVertices,
                    NumSurfaces = mesh.NumSurfaces,
                    NumTargets = 0 //Haven't implemented VTarget Reads, No ref currently, uncommon
                });

                _chunkWriter.BeginChunk(ChunkTypes.MSH_VFORMAT);
                _bw.WriteStruct(new VertexFormat
                {
                    Fmt = mesh.Vxyz[0].Fmt,
                    BiasScale = mesh.Vxyz[0].BiasScale,

                    Stride = mesh.Vxyz[0].Stride,

                    VNormalFmt = mesh.VNormalFmt,
                    VNormalScale = mesh.VNormalScale,
                    VNormalStride = mesh.VNormalStride,

                    VTangentFmt = mesh.VTangentFmt,
                    VTangentScale = mesh.VTangentScale,
                    VTangentStride = mesh.VTangentStride,

                    VUvFmt = mesh.VUvFmt,
                    VUvScale = mesh.VUvScale,
                    VUvStride = mesh.VUvStride
                });
                _chunkWriter.EndChunk();

                if (mesh.Vxyz[0].Data.Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_VERTICES);
                    _bw.Write(mesh.Vxyz[0].Data);
                    _chunkWriter.EndChunk();
                }


                if (mesh.Vxyz[1].Data.Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_VERTICES1);
                    _bw.Write(mesh.Vxyz[1].Data);
                    _chunkWriter.EndChunk();
                }


                if (mesh.VNormal[0].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_NORMALS);
                    _bw.Write(mesh.VNormal[0]);
                    _chunkWriter.EndChunk();
                }

                if (mesh.VNormal[1].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_NORMALS1);
                    _bw.Write(mesh.VNormal[1]);
                    _chunkWriter.EndChunk();
                }

                if (mesh.VTangent[0].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_TANGENTS);
                    _bw.Write(mesh.VTangent[0]);
                    _chunkWriter.EndChunk();
                }

                if (mesh.VTangent[1].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_TANGENTS1);
                    _bw.Write(mesh.VTangent[1]);
                    _chunkWriter.EndChunk();
                }


                if (mesh.VBitangent[0].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_BITANGENTS0);
                    _bw.Write(mesh.VBitangent[0]);
                    _chunkWriter.EndChunk();
                }

                if (mesh.VBitangent[1].Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_BITANGENTS1);
                    _bw.Write(mesh.VBitangent[1]);
                    _chunkWriter.EndChunk();
                }


                for (var i = 0; i < mesh.VColor.Length; i++)
                {
                    var colors = mesh.VColor[i];
                    if (colors.Length == 0)
                        continue; // skip, empty

                    // Choose the proper chunk type
                    var chunkType = i switch
                    {
                        0 => ChunkTypes.MSH_COLORS0,
                        1 => ChunkTypes.MSH_COLORS1,
                        2 => ChunkTypes.MSH_COLORS2,
                        3 => ChunkTypes.MSH_COLORS3,
                        _ => throw new InvalidOperationException($"Unexpected VColor index {i}")
                    };

                    _chunkWriter.BeginChunk(chunkType);
                    //this is prob bad and slow
                    foreach (var c in colors)
                        _bw.WriteStruct(c);

                    _chunkWriter.EndChunk();
                }
                
                for (var i = 0; i < mesh.VUv.Length; i++)
                {
                    var uvSet = mesh.VUv[i];
                    if (uvSet.Length == 0)
                        continue; // skip empty UV sets

                    // Choose chunk type
                    var chunkType = i switch
                    {
                        0 => ChunkTypes.MSH_UV0,
                        1 => ChunkTypes.MSH_UV1,
                        2 => ChunkTypes.MSH_UV2,
                        3 => ChunkTypes.MSH_UV3,
                        _ => throw new InvalidOperationException($"Unexpected VUv index {i}")
                    };

                    _chunkWriter.BeginChunk(chunkType);

                    foreach (var uv in uvSet)
                        _bw.WriteStruct(uv);

                    _chunkWriter.EndChunk();
                }

                if (mesh.Indices.Length > 0)
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_INDICES);
                    foreach (var index in mesh.Indices)
                        _bw.Write(index);
                    _chunkWriter.EndChunk();
                }
                
                if (mesh.Surfaces is { Length: > 0 })
                {
                    _chunkWriter.BeginChunk(ChunkTypes.MSH_SURFACES1);

                    foreach (var s in mesh.Surfaces)
                    {
                        _bw.Write(s.MatId);
                        _bw.Write(s.Offset);
                        _bw.Write(s.Count);
                        _bw.Write(s.NumBones);

                        if (s.NumBones <= 0)
                            continue;

                        foreach (var bone in s.Bones)
                            _bw.Write(bone);
                    }

                    _chunkWriter.EndChunk();
                }

                _chunkWriter.EndChunk(); //end mesh-node chunk
            }

            _chunkWriter.EndChunk(); //end node chunk
        }
    }
}