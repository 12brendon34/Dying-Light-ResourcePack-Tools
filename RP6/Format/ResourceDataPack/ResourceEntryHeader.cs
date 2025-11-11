using System.Runtime.InteropServices;

namespace RP6.Format.ResourceDataPack;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ResourceEntryHeader
{
    public uint Bitfields;
    public uint DataOffset;
    public uint DataByteSize;
    public short CompressedByteSize;
    public short ReferencedResource;
}