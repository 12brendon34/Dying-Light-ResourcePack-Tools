using System.Runtime.InteropServices;

namespace RP6.Format.ResourceDataPack;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MainHeader
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Magic;

    public uint Version;
    public uint Flags;
    public uint PhysResCount;
    public uint PhysResTypeCount;
    public uint ResourceNamesCount;
    public uint ResourceNamesBlockSize;
    public uint LogResCount;
    public uint SectorAlignment;
}