namespace Mesh.Format;

public class MshData
{
    public MshRoot Root;
    public List<MshTree> Tree { get; set; } = [];
    public List<string> Mats { get; set; } = [];
    public List<string> SurfaceTypes { get; set; } = [];
}