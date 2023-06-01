using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectHashes : IChunkReader
{
    public uint ChunkId => 0x00134012;

    public void Deserialize(BinarySpan span, SolidObject obj)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        int count = span.ReadInt32() / 8;
        obj.TextureHashes = new uint[count];

        for (int i = 0; i < count; i++)
        {
            obj.TextureHashes[i] = span.ReadUInt32();
            span.Pointer += 4;
        }
    }
}
