using System.Runtime.InteropServices;
using Mesh.Format;

namespace RP6.Format.CompactMesh;
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshEntityInFile
{
    public Matrix3X4 m_LocalTM;
    public Matrix3X4 m_BoneInitTM;
    public Aabb m_Bounds;

    public UInt64 m_Name;
    public UInt64 m_Unknown1;
    public UInt64 m_NodeFormat;
    public UInt64 m_Unknown3;
    public UInt64 m_Unknown4;
    public UInt64 m_Unknown5;
    public UInt64 m_Unknown6;
    public UInt64 m_CollTreeGeom;
    public UInt64 m_CollTreeHull;

    public uint m_Flags;
    public ushort m_NodeIdx;
    public ushort m_Parent; //same as source msh
    
    private byte _m_Type;
    public MshType MshType
    {
        get => (MshType)_m_Type;
        set => _m_Type = (byte)value;
    }
    
    public byte m_LodsCount;
    public ushort m_ChildrenCount;
    public uint m_Unknown7;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
    public byte[] buff; //Only on DLTB
}