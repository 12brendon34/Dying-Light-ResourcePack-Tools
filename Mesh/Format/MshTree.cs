namespace Mesh.Format;

public class MshTree
{
    //public uint Index;

    public MshNode Node;

    //public ICollTree CollGeom;
    //public ICollTree CollHull;
    //public MeshFmt[] Mesh;
    public List<MeshFmt> Mesh { get; set; } = [];
}