using System.Runtime.InteropServices;

namespace RP6.Format.ResourceDataPack;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ResourceTypeHeader
{
    public uint Bitfields;
    public uint DataFileOffset;
    public uint DataByteSize;
    public uint CompressedByteSize;
    public uint ResourceCount;
}