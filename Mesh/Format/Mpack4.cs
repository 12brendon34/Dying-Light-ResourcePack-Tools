using System.Runtime.InteropServices;

namespace Mesh.Format;

[StructLayout(LayoutKind.Sequential)]
public class Mpack4
{
    public MvFmt Fmt;
    public uint Stride;
    public Vec4 BiasScale;

    public byte[] Data { get; set; } = [];
}