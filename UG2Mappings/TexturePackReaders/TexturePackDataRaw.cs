using L4RH;
using L4RH.Model.Textures;
using L4RH.Readers;

namespace UG2Mappings.TexturePackReaders;

internal class TexturePackDataRaw : IChunkReader
{
    public uint ChunkId => 0x33320002;

    public void Deserialize(BinarySpan span, TexturePack pack, long dataBasePosition)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;
        span.AlignPosition();

        int dataBegin = span.Pointer;

        foreach (var texture in pack)
        {
            if (texture.PaletteSize > 0)
            {
                span.Pointer = dataBegin + texture.PaletteOffset;
                texture.Palette = span.ReadArray(texture.PaletteSize).ToArray();
            }

            int dataPos = dataBegin + texture.DataOffset;
            span.Pointer = dataPos - (dataPos & 0xFF);
            texture.Data = span.ReadArray(texture.DataSize).ToArray();
        }
    }
}
