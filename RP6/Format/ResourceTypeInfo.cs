using Utils.IO.Extensions;
namespace RP6.Format;

// ReSharper disable InconsistentNaming
public static class ResourceTypeInfo
{
    public class TextureFormat
    {
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public byte MipLevels;

        public struct Info()
        {
            public DDS.DDS_PIXELFORMAT PixelFormat = default;
            public DDS.DXGI_FORMAT DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
            public DX9_FORMAT Dx9Format = DX9_FORMAT.Unknown;
            public uint pitchOrLinearSize = 0;
            public bool IsBlockCompressed = false;
            public bool IsLegacyDX9 = false;
        }

        public Info FmtInfo;

        //not based on anything, just my dx9 format enum
        public enum DX9_FORMAT : uint
        {
            A8R8G8B8 = 0,
            X8R8G8B8,
            A8R8G8B8_GAMMA,
            A8R8G8B8_GAMMA_AS16,
            B8G8R8A8,
            B8G8R8X8,
            X8B8G8R8,

            A2R10G10B10,
            A2R10G10B10_GAMMA,
            A2R10G10B10_GAMMA_AS16,

            R8G8B8,
            B8G8R8,
            R5G6B5,
            X1R5G5B5,
            X4R4G4B4,

            R8_UNORM_NO_TYPELESS,
            L8,
            L16,
            A4L4,

            D16S8,
            D24FS8,
            DF16,
            DF24,

            B32G32R32F,
            XENON_HDR_16FF,
            XENON_HDR_16F,
            XENON_HDR_16,
            XENON_HDR_11,
            XENON_HDR_10,
            XENON_HDR_8,

            CTX1,
            DXN,
            DXT3A_1111,
            DXT3A,
            DXT5A,

            V8U8,
            Q8W8V8U8,
            L6V5U5,
            X8L8V8U8,
            CxV8U8,
            Unknown
        };

        public void FromCE6(BinaryReader reader)
        {
            const uint DDS_PIXELFORMAT_SIZE = 32;

            var header = reader.ReadStruct<SPackTextureHeader>();
            Width = header.Width;
            Height = header.Height;
            Depth = header.Depth;
            MipLevels = (byte)(header.MipLevels > 63 ? 63 : header.MipLevels);

            FmtInfo = header.Format switch
            {
                //any LegacyDX9 without PixelFormat are unsupported
                SPackTextureHeader.TextureFormatCE6.R8G8B8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 24, 0xff0000, 0x00ff00, 0x0000ff, 0), Dx9Format = DX9_FORMAT.R8G8B8 },
                SPackTextureHeader.TextureFormatCE6.B8G8R8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8 },
                SPackTextureHeader.TextureFormatCE6.A8R8G8B8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, Dx9Format = DX9_FORMAT.A8R8G8B8_GAMMA, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgba, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000) },
                SPackTextureHeader.TextureFormatCE6.X8R8G8B8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM },
                SPackTextureHeader.TextureFormatCE6.B8G8R8X8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8X8 },
                SPackTextureHeader.TextureFormatCE6.B8G8R8A8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8A8 },
                SPackTextureHeader.TextureFormatCE6.A8B8G8R8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM },
                SPackTextureHeader.TextureFormatCE6.X8B8G8R8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0), Dx9Format = DX9_FORMAT.X8B8G8R8 },
                SPackTextureHeader.TextureFormatCE6.R5G6B5 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM },
                SPackTextureHeader.TextureFormatCE6.X1R5G5B5 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 16, 0x7c00, 0x03e0, 0x001f, 0), Dx9Format = DX9_FORMAT.X1R5G5B5 },
                SPackTextureHeader.TextureFormatCE6.A1R5G5B5 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM },
                SPackTextureHeader.TextureFormatCE6.A4R4G4B4 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM },
                SPackTextureHeader.TextureFormatCE6.X4R4G4B4 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 16, 0x0f00, 0x00f0, 0x000f, 0), Dx9Format = DX9_FORMAT.X4R4G4B4 },
                SPackTextureHeader.TextureFormatCE6.A8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM },
                SPackTextureHeader.TextureFormatCE6.L8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },
                SPackTextureHeader.TextureFormatCE6.A8L8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM },
                SPackTextureHeader.TextureFormatCE6.A4L4 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.LuminanceA, 8, 0x0f, 0, 0, 0xf0), Dx9Format = DX9_FORMAT.A4L4 },

                SPackTextureHeader.TextureFormatCE6.DXT1 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM },
                SPackTextureHeader.TextureFormatCE6.DXT3 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM },
                SPackTextureHeader.TextureFormatCE6.DXT5 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM },

                SPackTextureHeader.TextureFormatCE6.BC5_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM },
                SPackTextureHeader.TextureFormatCE6.BC6H_UF16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16 },
                SPackTextureHeader.TextureFormatCE6.BC6H_SF16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16 },
                SPackTextureHeader.TextureFormatCE6.BC7_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM },

                SPackTextureHeader.TextureFormatCE6.V8U8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.BumpDuDv, 16, 0x00ff, 0xff00, 0, 0), Dx9Format = DX9_FORMAT.V8U8 },
                SPackTextureHeader.TextureFormatCE6.L6V5U5 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.BumpLuminance, 16, 0x001f, 0x03e0, 0xfc00, 0), Dx9Format = DX9_FORMAT.L6V5U5 },
                SPackTextureHeader.TextureFormatCE6.X8L8V8U8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.BumpLuminance, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0), Dx9Format = DX9_FORMAT.X8L8V8U8 },
                SPackTextureHeader.TextureFormatCE6.Q8W8V8U8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.BumpDuDv, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000), Dx9Format = DX9_FORMAT.Q8W8V8U8 },
                SPackTextureHeader.TextureFormatCE6.CxV8U8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.CxV8U8 },
                SPackTextureHeader.TextureFormatCE6.L16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UNORM },
                SPackTextureHeader.TextureFormatCE6.G16R16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM },
                SPackTextureHeader.TextureFormatCE6.A16B16G16R16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM },
                SPackTextureHeader.TextureFormatCE6.R16F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT },
                SPackTextureHeader.TextureFormatCE6.G16R16F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT },
                SPackTextureHeader.TextureFormatCE6.A16B16G16R16F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT },
                SPackTextureHeader.TextureFormatCE6.R32F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT },
                SPackTextureHeader.TextureFormatCE6.G32R32F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT },
                SPackTextureHeader.TextureFormatCE6.A32B32G32R32F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT },
                SPackTextureHeader.TextureFormatCE6.D16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D16_UNORM },
                SPackTextureHeader.TextureFormatCE6.D24S8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT },
                SPackTextureHeader.TextureFormatCE6.D16S8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.D16S8 },
                SPackTextureHeader.TextureFormatCE6.D24X8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS },
                SPackTextureHeader.TextureFormatCE6.D32 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT },
                SPackTextureHeader.TextureFormatCE6.DF16 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DF16 },
                SPackTextureHeader.TextureFormatCE6.DF24 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DF24 },
                SPackTextureHeader.TextureFormatCE6.D24FS8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT },
                SPackTextureHeader.TextureFormatCE6.D32FS8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT },

                SPackTextureHeader.TextureFormatCE6.A2R10G10B10 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM },
                SPackTextureHeader.TextureFormatCE6.R11G11B10 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT },

                SPackTextureHeader.TextureFormatCE6.A8R8G8B8_GAMMA => new Info { IsLegacyDX9 = true },
                SPackTextureHeader.TextureFormatCE6.A8R8G8B8_GAMMA_AS16 => new Info { IsLegacyDX9 = true },

                SPackTextureHeader.TextureFormatCE6.A2R10G10B10_GAMMA => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A2R10G10B10_GAMMA },
                SPackTextureHeader.TextureFormatCE6.A2R10G10B10_GAMMA_AS16 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A2R10G10B10_GAMMA_AS16 },
                SPackTextureHeader.TextureFormatCE6.B32G32R32F => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B32G32R32F },
                SPackTextureHeader.TextureFormatCE6.R11G11B10F => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT },
                SPackTextureHeader.TextureFormatCE6.R32G32B32A32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT },
                SPackTextureHeader.TextureFormatCE6.R32G32B32A32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT },
                SPackTextureHeader.TextureFormatCE6.R16G16B16A16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM },
                SPackTextureHeader.TextureFormatCE6.R16G16B16A16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT },
                SPackTextureHeader.TextureFormatCE6.R16G16B16A16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT },
                SPackTextureHeader.TextureFormatCE6.R32G32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT },
                SPackTextureHeader.TextureFormatCE6.R32G32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT },
                SPackTextureHeader.TextureFormatCE6.R10G10B10A2_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM },
                SPackTextureHeader.TextureFormatCE6.R10G10B10A2_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT },
                SPackTextureHeader.TextureFormatCE6.R8G8B8A8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM },
                SPackTextureHeader.TextureFormatCE6.R8G8B8A8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT },
                SPackTextureHeader.TextureFormatCE6.R8G8B8A8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT },
                SPackTextureHeader.TextureFormatCE6.R16G16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM },
                SPackTextureHeader.TextureFormatCE6.R16G16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT },
                SPackTextureHeader.TextureFormatCE6.R16G16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT },
                SPackTextureHeader.TextureFormatCE6.R32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_UINT },
                SPackTextureHeader.TextureFormatCE6.R32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_SINT },
                SPackTextureHeader.TextureFormatCE6.R8G8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM },
                SPackTextureHeader.TextureFormatCE6.R8G8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM },
                SPackTextureHeader.TextureFormatCE6.R8G8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT },
                SPackTextureHeader.TextureFormatCE6.R8G8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT },
                SPackTextureHeader.TextureFormatCE6.R16_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UNORM },
                SPackTextureHeader.TextureFormatCE6.R16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UINT },
                SPackTextureHeader.TextureFormatCE6.R16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SNORM },
                SPackTextureHeader.TextureFormatCE6.R16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SINT },
                SPackTextureHeader.TextureFormatCE6.R8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },
                SPackTextureHeader.TextureFormatCE6.R8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UINT },
                SPackTextureHeader.TextureFormatCE6.R8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SNORM },
                SPackTextureHeader.TextureFormatCE6.R8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SINT },

                SPackTextureHeader.TextureFormatCE6.R32_FLOAT_X8X24_TYPELESS => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS },
                SPackTextureHeader.TextureFormatCE6.X32_TYPELESS_G8X24_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT },
                SPackTextureHeader.TextureFormatCE6.X24_TYPELESS_G8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT },

                //Xbox
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_16FF => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_16FF },
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_16F => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_16F },
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_16 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_16 },
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_8 },
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_10 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_10 },
                SPackTextureHeader.TextureFormatCE6.XENON_HDR_11 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.XENON_HDR_11 },

                //Compressed Xbox
                SPackTextureHeader.TextureFormatCE6.DXT3A_1111 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DXT3A_1111 },
                SPackTextureHeader.TextureFormatCE6.DXT3A => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DXT3A },
                SPackTextureHeader.TextureFormatCE6.DXT5A => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DXT5A },
                SPackTextureHeader.TextureFormatCE6.DXN => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.DXN },
                SPackTextureHeader.TextureFormatCE6.CTX1 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.CTX1 },

                SPackTextureHeader.TextureFormatCE6.UNKNOWN => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_UNKNOWN },
                _ => throw new ArgumentOutOfRangeException()
            };

            FmtInfo.IsBlockCompressed = IsCompressed(FmtInfo.DxgiFormat);
            FmtInfo.PixelFormat = new DDS.DDS_PIXELFORMAT
            {
                Size = DDS_PIXELFORMAT_SIZE,
                Flags = (uint)DDS.PixelFormatFlags.FourCC,
                FourCC = DDS.MakeFourCC("DX10")
            };

            uint rowPitch;
            uint slicePitch;

            if (FmtInfo.IsLegacyDX9)
                (rowPitch, slicePitch) = ComputePitchDX9(FmtInfo.Dx9Format, Width, Height);
            else
                (rowPitch, slicePitch) = ComputePitchDXGI(FmtInfo.DxgiFormat, Width, Height);

            FmtInfo.pitchOrLinearSize = FmtInfo.IsBlockCompressed ? slicePitch : rowPitch;
        }

        public void FromCEngine(BinaryReader reader)
        {
            const uint DDS_PIXELFORMAT_SIZE = 32;

            var header = reader.ReadStruct<RTextureInfo>();
            Width = header.Width;
            Height = header.Height;
            Depth = header.Depth;
            MipLevels = header.MipLevels;

            FmtInfo = header.Format switch
            {
                RTextureInfo.EFormat.BC1_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM },
                RTextureInfo.EFormat.BC2_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM },
                RTextureInfo.EFormat.BC3_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM },
                RTextureInfo.EFormat.BC4_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM },
                RTextureInfo.EFormat.BC4_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM },
                RTextureInfo.EFormat.BC5_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM },
                RTextureInfo.EFormat.BC5_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM },
                RTextureInfo.EFormat.BC6H_UF16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16 },
                RTextureInfo.EFormat.BC6H_SF16 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16 },
                RTextureInfo.EFormat.BC7_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM },

                RTextureInfo.EFormat.R8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },
                RTextureInfo.EFormat.R8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SNORM },
                RTextureInfo.EFormat.R8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UINT },
                RTextureInfo.EFormat.R8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SINT },
                RTextureInfo.EFormat.A8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM },
                RTextureInfo.EFormat.R16_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT },
                RTextureInfo.EFormat.R16_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UNORM },
                RTextureInfo.EFormat.R16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SNORM },
                RTextureInfo.EFormat.R16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UINT },
                RTextureInfo.EFormat.R16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SINT },
                RTextureInfo.EFormat.R32_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT },
                RTextureInfo.EFormat.R32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_UINT },
                RTextureInfo.EFormat.R32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32_SINT },
                RTextureInfo.EFormat.R8G8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM },
                RTextureInfo.EFormat.R8G8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM },
                RTextureInfo.EFormat.R8G8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT },
                RTextureInfo.EFormat.R8G8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT },
                RTextureInfo.EFormat.R16G16_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT },
                RTextureInfo.EFormat.R16G16_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM },
                RTextureInfo.EFormat.R16G16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM },
                RTextureInfo.EFormat.R16G16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT },
                RTextureInfo.EFormat.R16G16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT },
                RTextureInfo.EFormat.R32G32_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT },
                RTextureInfo.EFormat.R32G32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT },
                RTextureInfo.EFormat.R32G32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT },
                RTextureInfo.EFormat.R11G11B10_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT },
                RTextureInfo.EFormat.R8G8B8A8_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM },
                RTextureInfo.EFormat.R8G8B8A8_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM },
                RTextureInfo.EFormat.R8G8B8A8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT },
                RTextureInfo.EFormat.R8G8B8A8_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT },
                RTextureInfo.EFormat.R10G10B10A2_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM },
                RTextureInfo.EFormat.R10G10B10A2_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT },
                RTextureInfo.EFormat.R16G16B16A16_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT },
                RTextureInfo.EFormat.R16G16B16A16_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM },
                RTextureInfo.EFormat.R16G16B16A16_SNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM },
                RTextureInfo.EFormat.R16G16B16A16_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT },
                RTextureInfo.EFormat.R16G16B16A16_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT },
                RTextureInfo.EFormat.R32G32B32A32_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT },
                RTextureInfo.EFormat.R32G32B32A32_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT },
                RTextureInfo.EFormat.R32G32B32A32_SINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT },
                RTextureInfo.EFormat.D16_UNORM => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D16_UNORM },
                RTextureInfo.EFormat.D24_UNORM_S8_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT },
                RTextureInfo.EFormat.D32_FLOAT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT },
                RTextureInfo.EFormat.D32_FLOAT_S8X24_UINT => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT },

                RTextureInfo.EFormat.L8 => new Info { DxgiFormat = DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },
                RTextureInfo.EFormat.L16 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.L16 },
                RTextureInfo.EFormat.R5G6B5 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.R5G6B5 },
                RTextureInfo.EFormat.R8G8B8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 24, 0xff0000, 0x00ff00, 0x0000ff, 0), Dx9Format = DX9_FORMAT.R8G8B8 },
                RTextureInfo.EFormat.B8G8R8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8 },
                RTextureInfo.EFormat.B32G32R32F => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B32G32R32F },
                RTextureInfo.EFormat.A8R8G8B8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A8R8G8B8, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgba, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000) },
                RTextureInfo.EFormat.A8R8G8B8_GAMMA => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A8R8G8B8_GAMMA },
                RTextureInfo.EFormat.X8R8G8B8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.X8R8G8B8 },
                RTextureInfo.EFormat.B8G8R8A8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8A8 },
                RTextureInfo.EFormat.B8G8R8X8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.B8G8R8X8 },
                RTextureInfo.EFormat.X8B8G8R8 => new Info { IsLegacyDX9 = true, PixelFormat = DDS.CreateBitmaskFormat(DDS.PixelFormatFlags.Rgb, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0), Dx9Format = DX9_FORMAT.X8B8G8R8 },
                RTextureInfo.EFormat.A2R10G10B10 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A2R10G10B10 },
                RTextureInfo.EFormat.A2R10G10B10_GAMMA => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.A2R10G10B10_GAMMA },
                RTextureInfo.EFormat.D24FS8 => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.D24FS8 },
                RTextureInfo.EFormat.R8_UNORM_NO_TYPELESS => new Info { IsLegacyDX9 = true, Dx9Format = DX9_FORMAT.R8_UNORM_NO_TYPELESS },

                _ => throw new ArgumentOutOfRangeException()
            };

            FmtInfo.IsBlockCompressed = IsCompressed(FmtInfo.DxgiFormat);

            uint rowPitch;
            uint slicePitch;

            if (FmtInfo.IsLegacyDX9)
            {
                (rowPitch, slicePitch) = ComputePitchDX9(FmtInfo.Dx9Format, Width, Height);
            }
            else
            {
                (rowPitch, slicePitch) = ComputePitchDXGI(FmtInfo.DxgiFormat, Width, Height);

                FmtInfo.PixelFormat = new DDS.DDS_PIXELFORMAT
                {
                    Size = DDS_PIXELFORMAT_SIZE,
                    Flags = (uint)DDS.PixelFormatFlags.FourCC,
                    FourCC = DDS.MakeFourCC("DX10")
                };
            }

            FmtInfo.pitchOrLinearSize = FmtInfo.IsBlockCompressed ? slicePitch : rowPitch;
        }

        //https://github.com/microsoft/DirectXTex/blob/main/DirectXTex/DirectXTex.inl#L63
        bool IsCompressed(DDS.DXGI_FORMAT fmt)
        {
            switch (fmt)
            {
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                    return true;

                default:
                    return false;
            }
        }

        //make non platform specific, use D3DFORMAT or smt
        //very loosely based on https://github.com/microsoft/DirectXTex/blob/5e7688d0e2d73c7a84bc9d7a6f6e9c846733df12/ScreenGrab/ScreenGrab9.cpp#L418
        (uint rowPitch, uint slicePitch) ComputePitchDX9(DX9_FORMAT fmt, uint width, uint height)
        {
            //split into DX9_FORMAT's get bpp
            uint bpp = fmt switch
            {
                /*
                DX9_FORMAT.A8R8G8B8_GAMMA => expr,
                DX9_FORMAT.A8R8G8B8_GAMMA_AS16 => expr,
                DX9_FORMAT.B8G8R8A8 => expr,
                DX9_FORMAT.B8G8R8X8 => expr,
                DX9_FORMAT.A2R10G10B10_GAMMA => expr,
                DX9_FORMAT.A2R10G10B10_GAMMA_AS16 => expr,
                DX9_FORMAT.B8G8R8 => expr,
                DX9_FORMAT.R8_UNORM_NO_TYPELESS => expr,
                DX9_FORMAT.D16S8 => expr,
                DX9_FORMAT.DF16 => expr,
                DX9_FORMAT.DF24 => expr,
                DX9_FORMAT.B32G32R32F => expr,
                DX9_FORMAT.XENON_HDR_16FF => expr,
                DX9_FORMAT.XENON_HDR_16F => expr,
                DX9_FORMAT.XENON_HDR_16 => expr,
                DX9_FORMAT.XENON_HDR_11 => expr,
                DX9_FORMAT.XENON_HDR_10 => expr,
                DX9_FORMAT.XENON_HDR_8 => expr,
                DX9_FORMAT.CTX1 => expr,
                DX9_FORMAT.DXN => expr,
                DX9_FORMAT.DXT3A_1111 => expr,
                DX9_FORMAT.DXT3A => expr,
                DX9_FORMAT.DXT5A => expr,
                DX9_FORMAT.CxV8U8 => expr,
                DX9_FORMAT.Unknown => expr,
                */

                DX9_FORMAT.X8B8G8R8 or DX9_FORMAT.X8L8V8U8 or DX9_FORMAT.A8R8G8B8 or DX9_FORMAT.X8R8G8B8 or DX9_FORMAT.A2R10G10B10 or DX9_FORMAT.D24FS8 or DX9_FORMAT.Q8W8V8U8 => 32,

                DX9_FORMAT.R8G8B8 => 24,

                DX9_FORMAT.X1R5G5B5 or DX9_FORMAT.X4R4G4B4 or DX9_FORMAT.V8U8 or DX9_FORMAT.L16 or DX9_FORMAT.R5G6B5 or DX9_FORMAT.L6V5U5 => 16,

                DX9_FORMAT.L8 or
                    DX9_FORMAT.A4L4 => 8,

                _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, "Unsupported texture format")
            };

            var pitch = (width * bpp + 7u) / 8u;
            var slice = pitch * height;

            return (pitch, slice);
        }

        (uint rowPitch, uint slicePitch) ComputePitchDXGI(DDS.DXGI_FORMAT fmt, uint width, uint height)
        {
            uint pitch = 0;
            uint slice = 0;

            switch (fmt)
            {
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                {
                    var nbw = (width + 3) / 4;
                    var nbh = (height + 3) / 4;

                    pitch = nbw * 8u;
                    slice = pitch * nbh;
                }
                    break;

                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                {
                    var nbw = (width + 3) / 4;
                    var nbh = (height + 3) / 4;

                    pitch = nbw * 16u;
                    slice = pitch * nbh;
                }
                    break;
                case DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_YUY2:
                    pitch = (width + 1u >> 1) * 4u;
                    slice = pitch * height;
                    break;

                case DDS.DXGI_FORMAT.DXGI_FORMAT_Y210:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_Y216:
                    pitch = (width + 1u >> 1) * 8u;
                    slice = pitch * height;
                    break;

                case DDS.DXGI_FORMAT.DXGI_FORMAT_NV12:
                case DDS.DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                    pitch = (width + 1u >> 1) * 2u;
                    slice = pitch * ((height) + (height + 1u >> 1));
                    break;

                //write a DXGI get bpp function
                //exclude formats above.
                //default:
                //pitch = (width * bpp + 7u) / 8u;
                //slice = pitch * height;
                //break;
            }

            return (pitch, slice);
        }
    }
}