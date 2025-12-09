namespace RP6.Format;

// ReSharper disable InconsistentNaming
public static class ResourceTypeInfo
{
    //Custom TextureFormat Enum, to support both DL1 and DL2 and DLTB formats
    //each engine's texture format will be converted over to this one.
    public enum TextureFormat : uint
    {
        //DL2/TB DX10 Formats
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
        //R8G8B8,
        //B8G8R8,
        R11G11B10_FLOAT,
        B32G32R32F,
        //A8R8G8B8,
        //A8R8G8B8_GAMMA,
        X8R8G8B8,
        B8G8R8A8,
        B8G8R8X8,
        //X8B8G8R8,
        R8G8B8A8_UNORM,
        R8G8B8A8_SNORM,
        R8G8B8A8_UINT,
        R8G8B8A8_SINT,
        A2R10G10B10,
        //A2R10G10B10_GAMMA,
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
        //D24FS8,
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
        R8_UNORM_NO_TYPELESS,

        
        //TODO MAKE SURE EVERYTHING UNDER THIS POINT IS PROPERLY IN FormatInfo!!!
        //DL2/TB Legacy DX9 Formats
        A8R8G8B8,
        R8G8B8,
        B8G8R8,
        X8B8G8R8,
        D24FS8,
        A8R8G8B8_GAMMA,
        A2R10G10B10_GAMMA,
        
        //DL1 DX10 Formats
        //some might have not originally been dx10, but are (hopefully) fully compatable
        A8B8G8R8,
        A1R5G5B5,
        A8,
        V8U8,
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
        R11G11B10,
        R32_FLOAT_X8X24_TYPELESS,
        X32_TYPELESS_G8X24_UINT,
        X24_TYPELESS_G8_UINT,
        
        //todo NEEDS IMPLEMENTATION
        //fallback dx9 textures
        //DL1 DX9
        DF16,
        X1R5G5B5,
        A8L8,
        D24X8,
        D32,
        D32FS8,
        R11G11B10F,
        A4R4G4B4,
        X8L8V8U8,
        L6V5U5,
        X4R4G4B4,
        A4L4,
        Q8W8V8U8,
        CxV8U8,
        DF24,
        XENON_HDR_16FF,
        XENON_HDR_16F,
        XENON_HDR_16,
        XENON_HDR_8,
        XENON_HDR_10,
        XENON_HDR_11,
        DXT3A_1111,
        DXT3A,
        DXT5A,
        DXN,
        CTX1,
        NV_NULL,
        A16B16G16R16F_EXPAND,
        A2B10G10R10F_EDRAM,
        A16L16,
        G16R16_EDRAM,
        A16B16G16R16_EDRAM,
        A8R8G8B8_GAMMA_AS16,
        A2R10G10B10_GAMMA_AS16,
        D16S8,
    }

    private readonly static Dictionary<TextureFormat, DDS.DXGI_FORMAT> TextureToDXGIMap = new()
    {
        { TextureFormat.R8_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },
        { TextureFormat.R8_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SNORM },
        { TextureFormat.R8_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UINT },
        { TextureFormat.R8_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_SINT },
        { TextureFormat.A8_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM },
        { TextureFormat.L8, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM }, // Luminance -> map to single-channel R8
        { TextureFormat.R16_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT },
        { TextureFormat.R16_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UNORM },
        { TextureFormat.R16_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SNORM },
        { TextureFormat.R16_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UINT },
        { TextureFormat.R16_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_SINT },
        { TextureFormat.L16, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_UNORM }, // Luminance16 -> map to R16_UNORM
        { TextureFormat.R32_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT },
        { TextureFormat.R32_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32_UINT },
        { TextureFormat.R32_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32_SINT },

        { TextureFormat.R8G8_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM },
        { TextureFormat.R8G8_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM },
        { TextureFormat.R8G8_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT },
        { TextureFormat.R8G8_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT },

        { TextureFormat.R16G16_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT },
        { TextureFormat.R16G16_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM },
        { TextureFormat.R16G16_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM },
        { TextureFormat.R16G16_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT },
        { TextureFormat.R16G16_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT },

        { TextureFormat.R32G32_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT },
        { TextureFormat.R32G32_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT },
        { TextureFormat.R32G32_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT },

        { TextureFormat.R11G11B10_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT },
        {
            TextureFormat.B32G32R32F, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT
        }, // channel-order differs (BGR -> RGB); shader swizzle may be required

        { TextureFormat.R8G8B8A8_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM },
        { TextureFormat.R8G8B8A8_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM },
        { TextureFormat.R8G8B8A8_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT },
        { TextureFormat.R8G8B8A8_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT },

        {
            TextureFormat.A2R10G10B10, DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM
        }, // D3D9 ordering may differ; use R10G10B10A2
        {
            TextureFormat.A2R10G10B10_GAMMA, DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM
        }, // no SRGB variant in DXGI for this format; gamma must be handled externally

        { TextureFormat.R10G10B10A2_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM },
        { TextureFormat.R10G10B10A2_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT },

        { TextureFormat.R16G16B16A16_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT },
        { TextureFormat.R16G16B16A16_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM },
        { TextureFormat.R16G16B16A16_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM },
        { TextureFormat.R16G16B16A16_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT },
        { TextureFormat.R16G16B16A16_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT },

        { TextureFormat.R32G32B32A32_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT },
        { TextureFormat.R32G32B32A32_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT },
        { TextureFormat.R32G32B32A32_SINT, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT },

        // Depth/stencil
        { TextureFormat.D16_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_D16_UNORM },
        { TextureFormat.D24_UNORM_S8_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT },
        { TextureFormat.D32_FLOAT, DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT },
        {
            TextureFormat.D24FS8, DDS.DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS
        }, // best-effort typeless variant for 24-bit depth + 8 bits (legacy)
        { TextureFormat.D32_FLOAT_S8X24_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT },

        // BC (block-compressed)
        { TextureFormat.BC1_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM },
        { TextureFormat.BC2_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM },
        { TextureFormat.BC3_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM },
        { TextureFormat.BC4_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM },
        { TextureFormat.BC4_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM },
        { TextureFormat.BC5_SNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM },
        { TextureFormat.BC5_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM },
        { TextureFormat.BC6H_UF16, DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16 },
        { TextureFormat.BC6H_SF16, DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16 },
        { TextureFormat.BC7_UNORM, DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM },

        // Single-channel R8 fallback for "no typeless" variant
        { TextureFormat.R8_UNORM_NO_TYPELESS, DDS.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM },

        //DL1 DX10 mapping
        { TextureFormat.A8B8G8R8, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM },
        { TextureFormat.A1R5G5B5, DDS.DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM },
        { TextureFormat.A8, DDS.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM },
        { TextureFormat.V8U8, DDS.DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM },
        { TextureFormat.G16R16, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM },
        { TextureFormat.A16B16G16R16, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM },
        { TextureFormat.R16F, DDS.DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT },
        { TextureFormat.G16R16F, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT },
        { TextureFormat.A16B16G16R16F, DDS.DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT },
        { TextureFormat.R32F, DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT },
        { TextureFormat.G32R32F, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT },
        { TextureFormat.A32B32G32R32F, DDS.DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT },
        { TextureFormat.D16, DDS.DXGI_FORMAT.DXGI_FORMAT_D16_UNORM },
        { TextureFormat.D24S8, DDS.DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT },
        { TextureFormat.R11G11B10, DDS.DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT },
        { TextureFormat.R32_FLOAT_X8X24_TYPELESS, DDS.DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS },
        { TextureFormat.X32_TYPELESS_G8X24_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT },
        { TextureFormat.X24_TYPELESS_G8_UINT, DDS.DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT }
        
        // All other legacy formats are handled in DDS.GetPixelFormat
    };

    public static DDS.DXGI_FORMAT GetDXGIFormat(TextureFormat textureFormat)
    {
        return TextureToDXGIMap.GetValueOrDefault(textureFormat, DDS.DXGI_FORMAT.DXGI_FORMAT_UNKNOWN);
    }

    public static class FormatInfo
    {
        public static Info Get(TextureFormat fmt)
        {
            return fmt switch
            {
                TextureFormat.R8_UNORM or
                    TextureFormat.R8_SNORM or
                    TextureFormat.R8_UINT or
                    TextureFormat.R8_SINT or
                    TextureFormat.A8_UNORM or
                    TextureFormat.L8 or
                    TextureFormat.R8_UNORM_NO_TYPELESS
                    => new Info { BytesPerPixel = 1 },

                TextureFormat.R16_FLOAT or
                    TextureFormat.R16_UNORM or
                    TextureFormat.R16_SNORM or
                    TextureFormat.R16_UINT or
                    TextureFormat.R16_SINT or
                    TextureFormat.L16
                    => new Info { BytesPerPixel = 2 },

                TextureFormat.R32_FLOAT or TextureFormat.R32_UINT or TextureFormat.R32_SINT
                    => new Info { BytesPerPixel = 4 },

                TextureFormat.R8G8_UNORM or TextureFormat.R8G8_SNORM or TextureFormat.R8G8_UINT or TextureFormat.R8G8_SINT
                    => new Info { BytesPerPixel = 2 },

                TextureFormat.R16G16_FLOAT or TextureFormat.R16G16_UNORM or TextureFormat.R16G16_SNORM or TextureFormat.R16G16_UINT or TextureFormat.R16G16_SINT
                    => new Info { BytesPerPixel = 4 },

                TextureFormat.R32G32_FLOAT or TextureFormat.R32G32_UINT or TextureFormat.R32G32_SINT
                    => new Info { BytesPerPixel = 8 },

                TextureFormat.R5G6B5 => new Info { BytesPerPixel = 2 },
                TextureFormat.R8G8B8 or TextureFormat.B8G8R8 => new Info { BytesPerPixel = 3 },
                TextureFormat.B32G32R32F => new Info { BytesPerPixel = 12 },

                TextureFormat.A4R4G4B4 => new Info { BytesPerPixel = 2 },
                TextureFormat.X4R4G4B4 => new Info { BytesPerPixel = 2 },
                TextureFormat.A4L4 => new Info { BytesPerPixel = 1 },
                TextureFormat.L6V5U5 => new Info { BytesPerPixel = 2 },

                TextureFormat.R11G11B10_FLOAT => new Info { BytesPerPixel = 4 },
                TextureFormat.A8R8G8B8 or
                    TextureFormat.A8R8G8B8_GAMMA or
                    TextureFormat.X8R8G8B8 or
                    TextureFormat.B8G8R8A8 or
                    TextureFormat.B8G8R8X8 or
                    TextureFormat.X8B8G8R8 or
                    TextureFormat.R8G8B8A8_UNORM or
                    TextureFormat.R8G8B8A8_SNORM or
                    TextureFormat.R8G8B8A8_UINT or
                    TextureFormat.R8G8B8A8_SINT or
                    TextureFormat.A2R10G10B10 or
                    TextureFormat.A2R10G10B10_GAMMA or
                    TextureFormat.R10G10B10A2_UNORM or
                    TextureFormat.R10G10B10A2_UINT
                    => new Info { BytesPerPixel = 4 },

                TextureFormat.R16G16B16A16_FLOAT or
                    TextureFormat.R16G16B16A16_UNORM or
                    TextureFormat.R16G16B16A16_SNORM or
                    TextureFormat.R16G16B16A16_UINT or
                    TextureFormat.R16G16B16A16_SINT
                    => new Info { BytesPerPixel = 8 },

                TextureFormat.R32G32B32A32_FLOAT or TextureFormat.R32G32B32A32_UINT or TextureFormat.R32G32B32A32_SINT
                    => new Info { BytesPerPixel = 16 },

                TextureFormat.Q8W8V8U8 => new Info { BytesPerPixel = 4 },
                TextureFormat.X8L8V8U8 => new Info { BytesPerPixel = 4 },
                TextureFormat.CxV8U8 => new Info { BytesPerPixel = 2 },

                TextureFormat.XENON_HDR_16FF or TextureFormat.XENON_HDR_16 or TextureFormat.XENON_HDR_16F
                    => new Info { BytesPerPixel = 8 },
                TextureFormat.XENON_HDR_8 or TextureFormat.XENON_HDR_10 or TextureFormat.XENON_HDR_11
                    => new Info { BytesPerPixel = 4 },

                TextureFormat.A2B10G10R10F_EDRAM or
                    TextureFormat.A16L16 or
                    TextureFormat.G16R16_EDRAM or
                    TextureFormat.A8R8G8B8_GAMMA_AS16 or
                    TextureFormat.A2R10G10B10_GAMMA_AS16
                    => new Info { BytesPerPixel = 4 },

                TextureFormat.A16B16G16R16F_EXPAND or TextureFormat.A16B16G16R16_EDRAM
                    => new Info { BytesPerPixel = 8 },

                TextureFormat.D16_UNORM or TextureFormat.DF16
                    => new Info { BytesPerPixel = 2 },
                TextureFormat.DF24
                    => new Info { BytesPerPixel = 3 },
                TextureFormat.D24_UNORM_S8_UINT or TextureFormat.D24FS8
                    => new Info { BytesPerPixel = 4 },
                TextureFormat.D32_FLOAT
                    => new Info { BytesPerPixel = 4 },
                TextureFormat.D32_FLOAT_S8X24_UINT
                    => new Info { BytesPerPixel = 8 },
                TextureFormat.D16S8
                    => new Info { BytesPerPixel = 3 },

                TextureFormat.BC1_UNORM
                    => new Info { IsBlockCompressed = true, BlockSizeBytes = 8 },
                TextureFormat.BC2_UNORM or
                    TextureFormat.BC3_UNORM or
                    TextureFormat.BC5_SNORM or
                    TextureFormat.BC5_UNORM or
                    TextureFormat.BC6H_UF16 or
                    TextureFormat.BC6H_SF16 or
                    TextureFormat.BC7_UNORM
                    => new Info { IsBlockCompressed = true, BlockSizeBytes = 16 },
                TextureFormat.BC4_SNORM or TextureFormat.BC4_UNORM
                    => new Info { IsBlockCompressed = true, BlockSizeBytes = 8 },

                TextureFormat.DXT3A_1111 or TextureFormat.DXT3A or TextureFormat.DXT5A or TextureFormat.DXN
                    => new Info { IsBlockCompressed = true, BlockSizeBytes = 16 },
                TextureFormat.CTX1
                    => new Info { IsBlockCompressed = true, BlockSizeBytes = 8 },

                _ => throw new NotSupportedException($"Format {fmt} not handled")
            };
        }
        
        public struct Info
        {
            public int BytesPerPixel; // for uncompressed formats
            public bool IsBlockCompressed; // true if BC/DXT/etc.
            public int BlockSizeBytes; // size of one 4x4 block if compressed
        }
    }
}