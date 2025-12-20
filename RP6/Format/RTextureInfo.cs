using System.Runtime.InteropServices;
using Utils;

namespace RP6.Format;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RTextureInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Magic; // "IMGC"

    public uint ID;
    public uint Header_Size;
    public uint Unk;

    public Color Col_Min;
    public Color Col_Max;
    public Color Col_Average;

    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public EFormat Format;

    private byte Tex_Mip; // lower 2 bits = tex_type, upper 6 bits = mip_count

    public byte TexType
    {
        get => (byte)(Tex_Mip & 0x03);
        set => Tex_Mip = (byte)(Tex_Mip & 0xFC | value & 0x03);
    }

    public byte MipLevels
    {
        get => (byte)(Tex_Mip >> 2 & 0x3F);
        set => Tex_Mip = (byte)(Tex_Mip & 0x03 | (value & 0x3F) << 2);
    }

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ushort[] Reserved;
    
    //DL2 and DLTB Texture Format
    public enum EFormat : byte
    {
        R8_UNORM = 0,
        R8_SNORM,
        R8_UINT,
        R8_SINT,
        A8_UNORM,
        L8,
        R16_FLOAT,
        R16_UNORM,
        R16_SNORM,
        R16_UINT,
        R16_SINT,
        L16,
        R32_FLOAT,
        R32_UINT,
        R32_SINT,
        R8G8_UNORM,
        R8G8_SNORM,
        R8G8_UINT,
        R8G8_SINT,
        R16G16_FLOAT,
        R16G16_UNORM,
        R16G16_SNORM,
        R16G16_UINT,
        R16G16_SINT,
        R32G32_FLOAT,
        R32G32_UINT,
        R32G32_SINT,
        R5G6B5,
        R8G8B8,
        B8G8R8,
        R11G11B10_FLOAT,
        B32G32R32F,
        A8R8G8B8,
        A8R8G8B8_GAMMA,
        X8R8G8B8,
        B8G8R8A8,
        B8G8R8X8,
        X8B8G8R8,
        R8G8B8A8_UNORM,
        R8G8B8A8_SNORM,
        R8G8B8A8_UINT,
        R8G8B8A8_SINT,
        A2R10G10B10,
        A2R10G10B10_GAMMA,
        R10G10B10A2_UNORM,
        R10G10B10A2_UINT,
        R16G16B16A16_FLOAT,
        R16G16B16A16_UNORM,
        R16G16B16A16_SNORM,
        R16G16B16A16_UINT,
        R16G16B16A16_SINT,
        R32G32B32A32_FLOAT,
        R32G32B32A32_UINT,
        R32G32B32A32_SINT,
        D16_UNORM,
        D24_UNORM_S8_UINT,
        D32_FLOAT,
        D24FS8,
        D32_FLOAT_S8X24_UINT,
        BC1_UNORM,
        BC2_UNORM,
        BC3_UNORM,
        BC4_SNORM,
        BC4_UNORM,
        BC5_SNORM,
        BC5_UNORM,
        BC6H_UF16,
        BC6H_SF16,
        BC7_UNORM,
        R8_UNORM_NO_TYPELESS
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public override string ToString()
    {
        return $"{R}, {G}, {B}, {A}";
    }
}
