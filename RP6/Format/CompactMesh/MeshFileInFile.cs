using System.Runtime.InteropServices;
namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshFileInFile
{
    public UInt64 m_UniqueName;
    public UInt64 m_Nodes;
    public UInt64 m_SurfaceParams; 
    
    public UInt64 m_MaterialsDatabase;
    public UInt64 m_MorphNames;
    //struct CCollTreePacked *m_CollTree0;
    //struct CCollTreePacked *m_CollTree1;
    public UInt64 m_CollTree0;
    public UInt64 m_CollTree1;
    public UInt64 m_CollTree2; //kinda weird, but logic is there in engine to read another colltree
    public UInt64 m_AnimScriptName;
    
    public UInt64 m_MorphPresets; //prob
    public UInt64 m_MeshEntity;
    public uint m_NodesCount;
    public uint m_RootsCount;
    public uint m_SurfacesCount;
    public uint m_EntityCount;
}