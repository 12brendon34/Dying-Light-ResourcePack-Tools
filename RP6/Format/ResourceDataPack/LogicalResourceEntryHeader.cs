using System.Runtime.InteropServices;

namespace RP6.Format.ResourceDataPack;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LogicalResourceEntryHeader
{
    public uint Bitfields;
    public uint FirstNameIndex;
    public uint FirstResource;
}