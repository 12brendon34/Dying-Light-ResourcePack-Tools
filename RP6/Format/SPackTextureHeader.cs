using System.Runtime.InteropServices;
namespace RP6.Format;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SPackTextureHeader
{
    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public ushort ArraySize;
    public ushort MipLevels;
    public ushort Flags;
    private DLTextureFormat _format;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public uint[] MipLevelOffsets;

    public ResourceTypeInfo.TextureFormat GetFormat()
    {
        return _format switch
        {
            DLTextureFormat.R8_UNORM => ResourceTypeInfo.TextureFormat.R8_UNORM,
            DLTextureFormat.R8G8B8 => ResourceTypeInfo.TextureFormat.R8G8B8,
            DLTextureFormat.B8G8R8 => ResourceTypeInfo.TextureFormat.B8G8R8,
            DLTextureFormat.A8R8G8B8 => ResourceTypeInfo.TextureFormat.A8R8G8B8,
            DLTextureFormat.X8R8G8B8 => ResourceTypeInfo.TextureFormat.X8R8G8B8,
            DLTextureFormat.B8G8R8X8 => ResourceTypeInfo.TextureFormat.B8G8R8X8,
            DLTextureFormat.B8G8R8A8 => ResourceTypeInfo.TextureFormat.B8G8R8A8,
            DLTextureFormat.X8B8G8R8 => ResourceTypeInfo.TextureFormat.X8B8G8R8,
            DLTextureFormat.R5G6B5 => ResourceTypeInfo.TextureFormat.R5G6B5,
            DLTextureFormat.L8 => ResourceTypeInfo.TextureFormat.L8,
            DLTextureFormat.L16 => ResourceTypeInfo.TextureFormat.L16,
            DLTextureFormat.D24FS8 => ResourceTypeInfo.TextureFormat.D24FS8,
            DLTextureFormat.A2R10G10B10 => ResourceTypeInfo.TextureFormat.A2R10G10B10,
            DLTextureFormat.A8R8G8B8_GAMMA => ResourceTypeInfo.TextureFormat.A8R8G8B8_GAMMA,
            DLTextureFormat.A2R10G10B10_GAMMA => ResourceTypeInfo.TextureFormat.A2R10G10B10_GAMMA,
            DLTextureFormat.B32G32R32F => ResourceTypeInfo.TextureFormat.B32G32R32F,
            DLTextureFormat.R32G32B32A32_UINT => ResourceTypeInfo.TextureFormat.R32G32B32A32_UINT,
            DLTextureFormat.R32G32B32A32_SINT => ResourceTypeInfo.TextureFormat.R32G32B32A32_SINT,
            DLTextureFormat.R16G16B16A16_SNORM => ResourceTypeInfo.TextureFormat.R16G16B16A16_SNORM,
            DLTextureFormat.R16G16B16A16_UINT => ResourceTypeInfo.TextureFormat.R16G16B16A16_UINT,
            DLTextureFormat.R16G16B16A16_SINT => ResourceTypeInfo.TextureFormat.R16G16B16A16_SINT,
            DLTextureFormat.R32G32_UINT => ResourceTypeInfo.TextureFormat.R32G32_UINT,
            DLTextureFormat.R32G32_SINT => ResourceTypeInfo.TextureFormat.R32G32_SINT,
            DLTextureFormat.R10G10B10A2_UNORM => ResourceTypeInfo.TextureFormat.R10G10B10A2_UNORM,
            DLTextureFormat.R10G10B10A2_UINT => ResourceTypeInfo.TextureFormat.R10G10B10A2_UINT,
            DLTextureFormat.R8G8B8A8_SNORM => ResourceTypeInfo.TextureFormat.R8G8B8A8_SNORM,
            DLTextureFormat.R8G8B8A8_UINT => ResourceTypeInfo.TextureFormat.R8G8B8A8_UINT,
            DLTextureFormat.R8G8B8A8_SINT => ResourceTypeInfo.TextureFormat.R8G8B8A8_SINT,
            DLTextureFormat.R16G16_SNORM => ResourceTypeInfo.TextureFormat.R16G16_SNORM,
            DLTextureFormat.R16G16_UINT => ResourceTypeInfo.TextureFormat.R16G16_UINT,
            DLTextureFormat.R16G16_SINT => ResourceTypeInfo.TextureFormat.R16G16_SINT,
            DLTextureFormat.R32_UINT => ResourceTypeInfo.TextureFormat.R32_UINT,
            DLTextureFormat.R32_SINT => ResourceTypeInfo.TextureFormat.R32_SINT,
            DLTextureFormat.R8G8_UNORM => ResourceTypeInfo.TextureFormat.R8G8_UNORM,
            DLTextureFormat.R8G8_SNORM => ResourceTypeInfo.TextureFormat.R8G8_SNORM,
            DLTextureFormat.R8G8_UINT => ResourceTypeInfo.TextureFormat.R8G8_UINT,
            DLTextureFormat.R8G8_SINT => ResourceTypeInfo.TextureFormat.R8G8_SINT,
            DLTextureFormat.R16_UNORM => ResourceTypeInfo.TextureFormat.R16_UNORM,
            DLTextureFormat.R16_SNORM => ResourceTypeInfo.TextureFormat.R16_SNORM,
            DLTextureFormat.R16_UINT => ResourceTypeInfo.TextureFormat.R16_UINT,
            DLTextureFormat.R16_SINT => ResourceTypeInfo.TextureFormat.R16_SINT,
            DLTextureFormat.R8_UINT => ResourceTypeInfo.TextureFormat.R8_UINT,
            DLTextureFormat.R8_SNORM => ResourceTypeInfo.TextureFormat.R8_SNORM,
            DLTextureFormat.R8_SINT => ResourceTypeInfo.TextureFormat.R8_SINT,
            DLTextureFormat.BC5_SNORM => ResourceTypeInfo.TextureFormat.BC5_SNORM,
            DLTextureFormat.BC6H_UF16 => ResourceTypeInfo.TextureFormat.BC6H_UF16,
            DLTextureFormat.BC6H_SF16 => ResourceTypeInfo.TextureFormat.BC6H_SF16,
            DLTextureFormat.BC7_UNORM => ResourceTypeInfo.TextureFormat.BC7_UNORM,


            //Formats not in DL2

            DLTextureFormat.A4R4G4B4 => ResourceTypeInfo.TextureFormat.A4R4G4B4,
            DLTextureFormat.X4R4G4B4 => ResourceTypeInfo.TextureFormat.X4R4G4B4,
            DLTextureFormat.A4L4 => ResourceTypeInfo.TextureFormat.A4L4,
            DLTextureFormat.L6V5U5 => ResourceTypeInfo.TextureFormat.L6V5U5,
            DLTextureFormat.X8L8V8U8 => ResourceTypeInfo.TextureFormat.X8L8V8U8,
            DLTextureFormat.Q8W8V8U8 => ResourceTypeInfo.TextureFormat.Q8W8V8U8,
            DLTextureFormat.CxV8U8 => ResourceTypeInfo.TextureFormat.CxV8U8,
            DLTextureFormat.D16S8 => ResourceTypeInfo.TextureFormat.D16S8,
            DLTextureFormat.DF16 => ResourceTypeInfo.TextureFormat.DF16,
            DLTextureFormat.DF24 => ResourceTypeInfo.TextureFormat.DF24,
            DLTextureFormat.XENON_HDR_16FF => ResourceTypeInfo.TextureFormat.XENON_HDR_16FF,
            DLTextureFormat.XENON_HDR_16F => ResourceTypeInfo.TextureFormat.XENON_HDR_16F,
            DLTextureFormat.XENON_HDR_16 => ResourceTypeInfo.TextureFormat.XENON_HDR_16,
            DLTextureFormat.XENON_HDR_8 => ResourceTypeInfo.TextureFormat.XENON_HDR_8,
            DLTextureFormat.DXT3A => ResourceTypeInfo.TextureFormat.DXT3A,
            DLTextureFormat.DXT5A => ResourceTypeInfo.TextureFormat.DXT5A,
            DLTextureFormat.DXN => ResourceTypeInfo.TextureFormat.DXN,
            DLTextureFormat.CTX1 => ResourceTypeInfo.TextureFormat.CTX1,
            DLTextureFormat.DXT3A_1111 => ResourceTypeInfo.TextureFormat.DXT3A_1111,
            DLTextureFormat.XENON_HDR_10 => ResourceTypeInfo.TextureFormat.XENON_HDR_10,
            DLTextureFormat.XENON_HDR_11 => ResourceTypeInfo.TextureFormat.XENON_HDR_11,
            DLTextureFormat.A8R8G8B8_GAMMA_AS16 => ResourceTypeInfo.TextureFormat.A8R8G8B8_GAMMA_AS16,
            DLTextureFormat.A2R10G10B10_GAMMA_AS16 => ResourceTypeInfo.TextureFormat.A2R10G10B10_GAMMA_AS16,

            DLTextureFormat.A8B8G8R8 => ResourceTypeInfo.TextureFormat.A8B8G8R8,
            DLTextureFormat.X1R5G5B5 => ResourceTypeInfo.TextureFormat.X1R5G5B5,

            DLTextureFormat.A1R5G5B5 => ResourceTypeInfo.TextureFormat.A1R5G5B5,
            DLTextureFormat.A8 => ResourceTypeInfo.TextureFormat.A8,
            DLTextureFormat.A8L8 => ResourceTypeInfo.TextureFormat.A8L8,
            DLTextureFormat.DXT1 => ResourceTypeInfo.TextureFormat.BC1_UNORM,
            DLTextureFormat.DXT3 => ResourceTypeInfo.TextureFormat.BC2_UNORM,
            DLTextureFormat.DXT5 => ResourceTypeInfo.TextureFormat.BC3_UNORM,
            DLTextureFormat.V8U8 => ResourceTypeInfo.TextureFormat.V8U8,
            DLTextureFormat.G16R16 => ResourceTypeInfo.TextureFormat.G16R16,
            DLTextureFormat.A16B16G16R16 => ResourceTypeInfo.TextureFormat.A16B16G16R16,
            DLTextureFormat.R16F => ResourceTypeInfo.TextureFormat.R16F,
            DLTextureFormat.G16R16F => ResourceTypeInfo.TextureFormat.G16R16F,
            DLTextureFormat.A16B16G16R16F => ResourceTypeInfo.TextureFormat.A16B16G16R16F,
            DLTextureFormat.R32F => ResourceTypeInfo.TextureFormat.R32F,
            DLTextureFormat.G32R32F => ResourceTypeInfo.TextureFormat.G32R32F,
            DLTextureFormat.A32B32G32R32F => ResourceTypeInfo.TextureFormat.A32B32G32R32F,
            DLTextureFormat.D16 => ResourceTypeInfo.TextureFormat.D16,
            DLTextureFormat.D24S8 => ResourceTypeInfo.TextureFormat.D24S8,
            DLTextureFormat.D24X8 => ResourceTypeInfo.TextureFormat.D24X8,
            DLTextureFormat.D32 => ResourceTypeInfo.TextureFormat.D32,
            DLTextureFormat.D32FS8 => ResourceTypeInfo.TextureFormat.D32FS8,
            DLTextureFormat.R11G11B10 => ResourceTypeInfo.TextureFormat.R11G11B10,
            DLTextureFormat.R11G11B10F => ResourceTypeInfo.TextureFormat.R11G11B10F,
            DLTextureFormat.R32_FLOAT_X8X24_TYPELESS => ResourceTypeInfo.TextureFormat.R32_FLOAT_X8X24_TYPELESS,
            DLTextureFormat.X32_TYPELESS_G8X24_UINT => ResourceTypeInfo.TextureFormat.X32_TYPELESS_G8X24_UINT,
            DLTextureFormat.X24_TYPELESS_G8_UINT => ResourceTypeInfo.TextureFormat.X24_TYPELESS_G8_UINT,

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public RTextureInfo ToRTextureInfo(byte texType = 0)
    {
        var info = new RTextureInfo
        {
            Magic = "IMGC"u8.ToArray(),
            ID = 0,
            Unk = 0,
            // Header_Size -> use the marshaled size of the struct (useful if expected)
            Header_Size = (uint)Marshal.SizeOf<RTextureInfo>(),

            //left as zero, might make white
            Col_Min = new Color(),
            Col_Max = new Color(),
            Col_Average = new Color(),

            Width = Width,
            Height = Height,
            Depth = Depth,
            // TexType (lower 2 bits) and MipLevels (upper 6 bits).
            // Clamp mip levels to 0..63 because RTextureInfo stores 6 bits for mip count.
            TexType = (byte)(texType & 0x03),
            MipLevels = (byte)(this.MipLevels > 63 ? 63 : this.MipLevels),
            Reserved = new ushort[4]
        };

        return info;
    }
}

//DL1 Texture Format
public enum DLTextureFormat : uint
{
    R8G8B8 = 0,
    B8G8R8,
    A8R8G8B8,
    X8R8G8B8,
    B8G8R8X8,
    B8G8R8A8,
    A8B8G8R8,
    X8B8G8R8,
    R5G6B5,
    X1R5G5B5,
    A1R5G5B5,
    A4R4G4B4,
    X4R4G4B4,
    A8,
    L8,
    A8L8,
    A4L4,
    DXT1,
    DXT3,
    DXT5,
    V8U8,
    L6V5U5,
    X8L8V8U8,
    Q8W8V8U8,
    CxV8U8,
    L16,
    G16R16,
    A16B16G16R16,
    R16F,
    G16R16F,
    A16B16G16R16F,
    R32F,
    G32R32F,
    A32B32G32R32F,
    D16,
    D24S8,
    D16S8,
    D24X8,
    D32,
    DF16,
    DF24,
    D24FS8,
    D32FS8,
    XENON_HDR_16FF,
    XENON_HDR_16F,
    XENON_HDR_16,
    XENON_HDR_8,
    DXT3A,
    DXT5A,
    DXN,
    CTX1,
    DXT3A_1111,
    XENON_HDR_10,
    XENON_HDR_11,
    A2R10G10B10,
    R11G11B10,
    A8R8G8B8_GAMMA,
    A8R8G8B8_GAMMA_AS16,
    A2R10G10B10_GAMMA,
    A2R10G10B10_GAMMA_AS16,
    B32G32R32F,
    R11G11B10F,
    UNKNOWN,
    R32G32B32A32_UINT,
    R32G32B32A32_SINT,
    R16G16B16A16_SNORM,
    R16G16B16A16_UINT,
    R16G16B16A16_SINT,
    R32G32_UINT,
    R32G32_SINT,
    R10G10B10A2_UNORM,
    R10G10B10A2_UINT,
    R8G8B8A8_SNORM,
    R8G8B8A8_UINT,
    R8G8B8A8_SINT,
    R16G16_SNORM,
    R16G16_UINT,
    R16G16_SINT,
    R32_UINT,
    R32_SINT,
    R8G8_UNORM,
    R8G8_SNORM,
    R8G8_UINT,
    R8G8_SINT,
    R16_UNORM,
    R16_SNORM,
    R16_UINT,
    R16_SINT,
    R8_UNORM,
    R8_UINT,
    R8_SNORM,
    R8_SINT,
    BC5_SNORM,
    R32_FLOAT_X8X24_TYPELESS,
    X32_TYPELESS_G8X24_UINT,
    X24_TYPELESS_G8_UINT,
    BC6H_UF16,
    BC6H_SF16,
    BC7_UNORM
}