using System.Runtime.InteropServices;
namespace Mesh.Format;

//[StructLayout(LayoutKind.Sequential, Pack = 1)]
//Bones makes this not possible
public struct SurfaceDesc
{
    public ushort MatId;
    public uint Offset;
    public uint Count;

    public ushort NumBones;
    public ushort[] Bones;
}