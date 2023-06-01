using L4RH;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectVertices : IChunkReader
{
    public uint ChunkId => 0x00134B01;

    public Span<byte> Deserialize(BinarySpan span)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        int length = span.ReadInt32();

        length -= span.AlignPosition();

        return length == 0 ? Span<byte>.Empty : span.ReadArray(length);
    }
}
