using System.Runtime.InteropServices;
namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialSlot
{
    public UInt64 m_Pointer_A;
    public uint m_MaterialName;
    public uint m_Unknown_A;
    public UInt64 m_Pointer_B;
    
    public uint m_VertexLayout;
    public uint m_Unknown_B;
}