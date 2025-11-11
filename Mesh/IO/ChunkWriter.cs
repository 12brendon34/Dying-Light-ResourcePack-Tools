using System.Runtime.InteropServices;
using Mesh.Format;
using Utils.IO.Extensions;

namespace Mesh.IO;

public class ChunkWriter(
    Stream stream,
    BinaryWriter binaryWriter)
{
    private readonly BinaryWriter _bw = binaryWriter ?? throw new ArgumentNullException(nameof(binaryWriter));
    private readonly int _chunkHeaderSize = Marshal.SizeOf<FChunk>(); // Should always be 16
    private readonly Stack<OpenChunk> _stack = new();
    private readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public void Dispose()
    {
        _stack.Clear();
    }


    public void BeginChunk(ChunkTypes id, uint version = 0)
    {
        var currentPos = _stream.Position;
        if (_stack.Count > 0)
        {
            var parent = _stack.Peek();
            if (!parent.InlineDataSize.HasValue)
            {
                parent.InlineDataSize = currentPos - parent.DataStart;
            }
        }

        // Header placeholder
        var headerPos = _stream.Position;
        _bw.Write(new byte[_chunkHeaderSize]);
        _bw.Flush();

        var dataStart = _stream.Position;
        _stack.Push(new OpenChunk
        {
            HeaderPosition = headerPos,
            DataStart = dataStart,
            Id = id,
            Version = version,
            InlineDataSize = null
        });
    }

    public void EndChunk()
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException("EndChunk called with no open chunk.");

        var top = _stack.Pop();
        var endPos = _stream.Position;

        var chunkSizeLong = endPos - top.HeaderPosition;
        var dataSizeLong = top.InlineDataSize.HasValue
            ? top.InlineDataSize.Value
            : endPos - top.DataStart;

        if (chunkSizeLong < _chunkHeaderSize)
            throw new InvalidDataException("Computed chunk size is smaller than header size.");

        if (chunkSizeLong > uint.MaxValue || dataSizeLong > uint.MaxValue)
            throw new InvalidDataException("Chunk or data size exceeds uint.MaxValue.");

        var chunkSize = (uint)chunkSizeLong;
        var dataSize = (uint)dataSizeLong;

        // Seek back and write the header
        var savedPos = _stream.Position;
        _stream.Position = top.HeaderPosition;
        _bw.WriteStruct(new FChunk
        {
            Id = top.Id,
            Version = top.Version,
            ChunkSize = chunkSize,
            DataSize = dataSize
        });
        _bw.Flush();

        // Restore position to the end so further writes append correctly
        _stream.Position = savedPos;
    }

    private sealed class OpenChunk
    {
        public long HeaderPosition { get; set; }
        public long DataStart { get; set; }
        public ChunkTypes Id { get; set; }
        public uint Version { get; set; }
        public long? InlineDataSize { get; set; }
    }
}