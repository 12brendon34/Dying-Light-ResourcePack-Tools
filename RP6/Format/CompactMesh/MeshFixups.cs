using Utils.IO.Extensions;

namespace RP6.Format.CompactMesh;

public class MeshFixups
{
    public uint memorySize;
    public uint numPointerResolves;
    public uint numResolves;
    public uint numVisibleResolves;
    public uint[] pointerResolveOffsets;
    public CWObjectResolve[] resolves;

    public static MeshFixups FromBytes(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var fixups = new MeshFixups
        {
            memorySize = reader.ReadUInt32(),
            numResolves = reader.ReadUInt32(),
            numVisibleResolves = reader.ReadUInt32()
        };

        fixups.resolves = new CWObjectResolve[fixups.numResolves];
        for (var i = 0; i < fixups.numResolves; i++)
            fixups.resolves[i] = reader.ReadStruct<CWObjectResolve>();

        fixups.numPointerResolves = reader.ReadUInt32();
        fixups.pointerResolveOffsets = new uint[fixups.numPointerResolves];
        for (var i = 0; i < fixups.numPointerResolves; i++)
            fixups.pointerResolveOffsets[i] = reader.ReadUInt32();

        return fixups;
    }
}