using L4RH;
using L4RH.Model.Textures;
using L4RH.Readers;

namespace UG2Mappings.TexturePackReaders;

public class TexturePackReader : ITexturePackReader
{
    public uint ChunkId => 0xB3300000;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public TexturePack Deserialize(BinarySpan span, long dataBasePosition)
    {
        var pack = new TexturePack();

        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        var header = new TexturePackHeader();
        var data = new TexturePackData();

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
            else if (id == data.ChunkId)
            {
                data.Deserialize(chunkBuffer, pack, dataBasePosition);
            }
        }

        return pack;
    }
}
