using System.Runtime.InteropServices;
using System.Text;

namespace Utils.IO.Extensions;

public static class BinaryExtensions
{
    #region BinaryWriter

    public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : struct
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        var size = Marshal.SizeOf<T>();
        var buffer = new byte[size];

        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(value, ptr, fDeleteOld: false);
            writer.Write(buffer);
        }
        finally
        {
            handle.Free();
        }
    }

    #endregion

    #region BinaryReader

    public static T ReadStruct<T>(this BinaryReader reader) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var buffer = reader.ReadBytes(size);

        if (buffer.Length != size)
            throw new EndOfStreamException($"Could not read {typeof(T).Name}: expected {size} bytes, got {buffer.Length}.");

        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            return Marshal.PtrToStructure<T>(ptr)!;
        }
        finally
        {
            handle.Free();
        }
    }

    //idgaf about optimization, this is easy
    public static T[] ReadStructArray<T>(this BinaryReader reader, int count) where T : struct
    {
        switch (count)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(nameof(count));
            case 0:
                return [];
        }

        var result = new T[count];
        for (var i = 0; i < count; i++)
            result[i] = reader.ReadStruct<T>();

        return result;
    }

    public static string ReadTerminatedString(this BinaryReader reader, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        int b;
        var bytes = new List<byte>();

        while ((b = reader.BaseStream.ReadByte()) != -1)
        {
            if (b == 0)
                break;

            bytes.Add((byte)b);
        }

        return bytes.Count == 0 ? string.Empty : encoding.GetString(bytes.ToArray());
    }

    #endregion
}