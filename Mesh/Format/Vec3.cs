using System.Runtime.InteropServices;

namespace Mesh.Format;
[StructLayout(LayoutKind.Sequential, Pack=1)]
public struct Vec3
{
    public float X;
    public float Y;
    public float Z;
}