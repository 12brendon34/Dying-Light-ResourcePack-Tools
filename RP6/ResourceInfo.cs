namespace RP6;

public class ResourceInfo
{
    public int LogicalIndex { get; init; }
    public required string BaseName { get; init; }
    public required string TypeName { get; init; }
    public required bool isDyingLight1 { get; init; } = false;
    public int FileType { get; init; }
    public List<byte[]> Parts { get; init; } = [];
    public string OutputDir { get; init; } = ".";
}