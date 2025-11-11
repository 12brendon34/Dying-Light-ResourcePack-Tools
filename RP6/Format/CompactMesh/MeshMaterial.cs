using System.Runtime.InteropServices;
namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshMaterial
{
    public UInt64 m_MaterialSlot;
    public ushort m_SurfaceCount;
    public ushort m_MaterialCount;
    public uint m_Unknown;
}