using System.Runtime.InteropServices;

namespace Mesh.Format;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct col4b
{
    public byte B;
    public byte G;
    public byte R;
    public byte A;
}