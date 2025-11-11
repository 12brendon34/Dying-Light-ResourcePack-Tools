using System.Runtime.InteropServices;

namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential)]
public record struct CWObjectResolve
{
    public int offset;
    public uint class_id;
    public uint num_elements;
}