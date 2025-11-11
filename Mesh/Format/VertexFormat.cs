using System.Runtime.InteropServices;

namespace Mesh.Format;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexFormat
{
    public MvFmt Fmt;
    public Vec4 BiasScale;

    public uint Stride;

    public MvFmt VNormalFmt;
    public float VNormalScale;
    public uint VNormalStride;

    public MvFmt VTangentFmt;
    public float VTangentScale;
    public uint VTangentStride;

    public MvFmt VUvFmt;
    public float VUvScale;
    public uint VUvStride;
}