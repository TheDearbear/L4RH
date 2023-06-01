using L4RH;
using L4RH.Model.Textures;
using L4RH.Readers;

namespace UG2Mappings.TexturePackReaders;

internal class TexturePackHeader : IChunkReader
{
    public uint ChunkId => 0xB3310000;

    public void Deserialize(BinarySpan span, TexturePack pack)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;

        var header = new TexturePackDataHeader();
        var texture = new TextureHeaderEntry();

        while (span.Pointer < span.Length)
        {
            span.SkipPadding();

            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == header.ChunkId)
            {
                header.Deserialize(chunkBuffer, pack);
            }
            else if (id == texture.ChunkId)
            {
                texture.Deserialize(chunkBuffer, pack);
            }
        }
    }
}
