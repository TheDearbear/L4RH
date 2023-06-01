using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectIndices : IChunkReader
{
    public uint ChunkId => 0x00134B03;

    public void Deserialize(BinarySpan span, SolidObject solid)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        int length = span.ReadInt32();
        length -= span.AlignPosition();

        int items = length / 2;
        solid.Indices = new ushort[items];

        for (uint i = 0; i < items; i++)
            solid.Indices[i] = span.ReadUInt16();
    }
}
