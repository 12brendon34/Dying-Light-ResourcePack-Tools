using System.Runtime.InteropServices;

namespace Mesh.Format;

public enum ChunkTypes : uint
{
    MSH_ROOT = 'M' | 'S' << 8 | 'H' << 16,

    //plaintext strings, material and surface names
    MSH_MATERIALS = 0x500,
    MSH_SURFACE_TYPES = 0x700,
    
    //nodes
    MSH_NODE_OLD = 1, // actually marked as old in editor
    MSH_NODE_OLD_MTX43_BY_ROWS = 2,
    MSH_NODE = 3,
    
    //MshMesh struct, stores NumIndices, NumVertices, NumSurfaces. child of node
    MSH_MESH = 0x100,
    
    MSH_VERTICES = 0x101,
    MSH_NORMALS = 0x102,
    MSH_TANGENTS = 0x103,
    MSH_TARGETS = 0x104, //unimplemented
    
    MSH_COLORS0 = 0x110,
    MSH_COLORS1 = 0x111,
    MSH_COLORS2 = 0x112,
    MSH_COLORS3 = 0x113,
    
    MSH_UV0 = 0x120,
    MSH_UV1 = 0x121,
    MSH_UV2 = 0x122,
    MSH_UV3 = 0x123,
    
    //unimplemented
    MSH_VBLENDS = 0x130,
    MSH_VBLENDS_OPT = 0x131,
    
    MSH_INDICES = 0x140,
    MSH_SURFACES = 0x150, //unimplemented
    MSH_SURFACES1 = 0x151,//unimplemented
    MSH_VFORMAT = 0x160, //g
    
    MSH_VERTICES1 = 0x171, //same as MSH_VERTICES/0x101 but Vxyz[1]
    MSH_NORMALS1 = 0x181,
    MSH_TANGENTS1 = 0x191,
    MSH_BITANGENTS0 = 0x195,
    MSH_BITANGENTS1 = 0x196,
    
    //unimplemented
    MSH_TRACK_POS = 0x200,
    MSH_TRACK_ROT = 0x210,
    MSH_TRACK_SCL = 0x220,
    MSH_CLOTH = 0x300,
    MSH_COLMAP = 0x600,
    
    //colltree, always packed, despite some weird logic
    MSH_COLLTREE_GEOM = 0x601,
    MSH_COLLTREE_HULL = 0x602,
    MSH_COLLTREE_TERRAIN = 0x603, //maybe HtgM / HeightMap
    
    //new DL2 COLLTREE things?
    //0x604
    //0x605
    
    //newer flag, guessed name
    MSH_METAINFO = 0x900,
    
    //oddities
    //These seem redundent, maybe they are "New" types compared to their lower number counterpart
    //I'm having hard time finding exact reference code for them
    MSH_VERTICES0 = 0x170,
    MSH_NORMALS0 = 0x180,
    MSH_TANGENTS0 = 0x190,
    
    //no idea
    /*
    0xa00
    0x800
    0x801 //12 bytes long
    */
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FChunk
{
    public ChunkTypes Id;
    public uint Version;
    public uint ChunkSize;
    public uint DataSize;
}