using System.Numerics;
using System.Runtime.InteropServices;

namespace RP6.Format.CompactMesh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DlVertex32
{
    public float PX;
    public float PY;
    public float PZ;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public short[] QShort;

    public Half HalfUV0U;
    public Half HalfUV0V;

    public Half HalfUV1U;
    public Half HalfUV1V;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Weights;
        
    public static (float[] positionBuffer, float[] tangentData,float[] bitangentData, float[] normalData, float[] uv0Array, float[] uv1Array) ExtractArrays(DlVertex32[] vertices, int vertexCount = 0)
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

            // quaternion packed as shorts -> convert to [-1,1] floats
            float qx = 0f, qy = 0f, qz = 0f, qw = 1f;
            if (v.QShort.Length >= 4)
            {
                qx = v.QShort[0] / 32767.0f;
                qy = v.QShort[1] / 32767.0f;
                qz = v.QShort[2] / 32767.0f;
                qw = v.QShort[3] / 32767.0f;
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
                // fallback to identity quaternion
                q = Quaternion.Identity;
            }

            // transform basis vectors by quaternion -> tangent/bitangent/normal
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

            // Half -> float
            uv0Array[i * 2] = (float)v.HalfUV0U;
            uv0Array[i * 2 + 1] = (float)v.HalfUV0V;

            uv1Array[i * 2] = (float)v.HalfUV1U;
            uv1Array[i * 2 + 1] = (float)v.HalfUV1V;
        }

        return (positionBuffer, tangentData, bitangentData, normalData, uv0Array, uv1Array);
    }
}
