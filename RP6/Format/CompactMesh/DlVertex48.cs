using System.Numerics;
using System.Runtime.InteropServices;
namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DlVertex48
{
    public float PX;
    public float PY;
    public float PZ;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public sbyte[] QByte;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] BoneIndices;

    public Half HalfUV0U;
    public Half HalfUV0V;

    public Half HalfUV1U;
    public Half HalfUV1V;

    public Half HalfUV0U_B;
    public Half HalfUV0V_B;

    public Half HalfUV1U_B;
    public Half HalfUV1V_B;

    public uint Padding;

    public static (float[] positionBuffer, float[] tangentData, float[] bitangentData, float[] normalData, float[] uv0Array, float[] uv1Array) ExtractArrays(DlVertex48[] vertices, int vertexCount = 0)
    {
        if (vertices == null) throw new ArgumentNullException(nameof(vertices));
        if (vertexCount <= 0 || vertexCount > vertices.Length)
            vertexCount = vertices.Length;

        var positionBuffer = new float[vertexCount * 3];
        var tangentData = new float[vertexCount * 3];
        var bitangentData = new float[vertexCount * 3];
        var normalData = new float[vertexCount * 3];
        var uv0Array = new float[vertexCount * 2];
        var uv1Array = new float[vertexCount * 2];

        for (var i = 0; i < vertexCount; i++)
        {
            var v = vertices[i];

            positionBuffer[i * 3 + 0] = v.PX;
            positionBuffer[i * 3 + 1] = v.PY;
            positionBuffer[i * 3 + 2] = v.PZ;

            float qx = 0f, qy = 0f, qz = 0f, qw = 1f;
            if (v.QByte != null && v.QByte.Length >= 4)
            {
                qx = v.QByte[0] / 127.0f;
                qy = v.QByte[1] / 127.0f;
                qz = v.QByte[2] / 127.0f;
                qw = v.QByte[3] / 127.0f;
            }

            var q = new Quaternion(qx, qy, qz, qw);
            var lenSq = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
            if (lenSq > 1e-8f)
            {
                var invLen = 1.0f / MathF.Sqrt(lenSq);
                q = new Quaternion(q.X * invLen, q.Y * invLen, q.Z * invLen, q.W * invLen);
            }
            else
            {
                q = Quaternion.Identity;
            }

            var tangent = Vector3.Transform(Vector3.UnitX, q);
            var bitangent = Vector3.Transform(Vector3.UnitY, q);
            var normal = Vector3.Transform(Vector3.UnitZ, q);

            tangentData[i * 3 + 0] = tangent.X;
            tangentData[i * 3 + 1] = tangent.Y;
            tangentData[i * 3 + 2] = tangent.Z;
            bitangentData[i * 3 + 0] = bitangent.X;
            bitangentData[i * 3 + 1] = bitangent.Y;
            bitangentData[i * 3 + 2] = bitangent.Z;
            normalData[i * 3 + 0] = normal.X;
            normalData[i * 3 + 1] = normal.Y;
            normalData[i * 3 + 2] = normal.Z;

            uv0Array[i * 2] = (float)v.HalfUV0U;
            uv0Array[i * 2 + 1] = (float)v.HalfUV0V;
            uv1Array[i * 2] = (float)v.HalfUV1U;
            uv1Array[i * 2 + 1] = (float)v.HalfUV1V;
        }

        return (positionBuffer, tangentData, bitangentData, normalData, uv0Array, uv1Array);
    }
}