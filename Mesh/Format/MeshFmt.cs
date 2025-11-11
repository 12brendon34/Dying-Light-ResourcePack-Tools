using System.Runtime.InteropServices;
namespace Mesh.Format;
public class MeshFmt
{
    public MeshFmt()
    {
        //engine does this usually after initalizing mtool_fmt anyways
        //might as well just put it here, idgaf.
        Vxyz = new Mpack4[2];
        Vxyz[0] = new Mpack4();
        Vxyz[1] = new Mpack4();

        Vxyz[0].Fmt = MvFmt.Float3;
        Vxyz[0].BiasScale = new Vec4 { X = 0f, Y = 0f, Z = 0f, W = 1f };
        Vxyz[0].Stride = 0xC; // 12 bytes -> 3 floats

        VNormalFmt = MvFmt.Float3;
        VNormalScale = 1.0f;
        VNormalStride = 0xC;

        VTangentFmt = MvFmt.Float3;
        VTangentScale = 1.0f;
        VTangentStride = 0xC;

        VBitangentFmt = MvFmt.Float3;
        VBitangentScale = 1.0f;
        VBitangentStride = 0xC;

        VUvFmt = MvFmt.Float2;
        VUvScale = 1.0f;
        VUvStride = 8u;
    }

    public Mpack4[] Vxyz { get; } //mpack4 v_xyz[2];

    //VTarget

    //char* VNormal[0]
    //public byte[] VNormal0 { get; set; } = [];
    //public byte[] VNormal1 { get; set; } = [];

    //feels gross
    public byte[][] VNormal { get; set; } = [[], []]; //void *v_normal[2]; malloc ptr
    public MvFmt VNormalFmt { get; set; }
    public float VNormalScale { get; set; }
    public uint VNormalStride { get; set; }


    public byte[][] VTangent { get; set; } = [[], []]; //void *v_tangent[2];
    public MvFmt VTangentFmt { get; set; }
    public float VTangentScale { get; set; }
    public uint VTangentStride { get; set; }

    public byte[][] VBitangent { get; set; } = [[], []]; //void *v_bitangent[2];
    public MvFmt VBitangentFmt { get; set; }
    public float VBitangentScale { get; set; }
    public uint VBitangentStride { get; set; }

    //00000084     vblend *v_vblend;
    //00000088     col4b *v_color[4];
    public col4b[][] VColor { get; set; } = [[], []]; //col4b *v_color[4];
    //public col4b[] VColor { get; set; } = []; //col4b *v_color[4];

    public byte[][] VUv { get; set; } = [[], []]; //void *v_uv[4];
    public MvFmt VUvFmt { get; set; }
    public float VUvScale { get; set; }
    public uint VUvStride { get; set; }

    // Index and surface data
    public ushort[] Indices { get; set; } = [];
    public SurfaceDesc[] Surfaces { get; set; } = [];


    // Counts and metadata
    public uint NumSurfaces { get; set; }
    public uint NumVertices { get; set; }
    public uint NumIndices { get; set; }
    public byte NumBpv { get; set; }

    public ushort[] FaceMatId { get; set; } = [];
    public uint[] FaceAttr { get; set; } = [];
}