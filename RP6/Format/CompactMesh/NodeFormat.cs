using System.Runtime.InteropServices;
namespace RP6.Format.CompactMesh;


//may be MorphTargets or something??
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NodeFormat
{
    //pointers
    public UInt64 m_Unknown_A;
    public UInt64 m_SurfaceMaterialIndex;
    
    public ushort m_ObjectCount_A;
    public ushort m_Field_A;

    public ushort m_ObjectCount_B;
    public ushort m_VertexLayoutID; //
    
    //pointers
    public UInt64 m_MeshIndexCount;
    public UInt64 m_Unknown_D;

    public uint m_VertexBuffer;
    public uint m_VertexCount; //
    public uint m_Field_C;

    public ushort m_Field_D;
    public ushort m_Field_E;

    public ushort m_Field_F;
    public ushort m_Field_G;

    public uint m_Field_H;
}