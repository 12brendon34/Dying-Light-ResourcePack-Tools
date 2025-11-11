using System.Runtime.InteropServices;

namespace Mesh.Format;

[StructLayout(LayoutKind.Sequential, Pack=1)]
public struct Aabb
{
    public Vec3 Origin;
    public Vec3 Span;
}