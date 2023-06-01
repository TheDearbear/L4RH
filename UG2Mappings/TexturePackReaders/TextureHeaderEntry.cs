using L4RH;
using L4RH.Model.Textures;
using L4RH.Readers;

namespace UG2Mappings.TexturePackReaders;

internal class TextureHeaderEntry : IChunkReader
{
    public uint ChunkId => 0x33310004;

    public void Deserialize(BinarySpan span, TexturePack pack)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        int entries = span.ReadInt32() / 0x7C;

        for (int i = 0; i < entries; i++)
        {
            span.Pointer += 12;

            string name = span.ReadString(0x18);
            span.Pointer += 12;
            int dataOffset = span.ReadInt32();
            int paletteOffset = span.ReadInt32();
            int dataSize = span.ReadInt32();
            int paletteSize = span.ReadInt32();
            span.Pointer += 4;

            Texture texture = new()
            {
                Name = name,
                DataOffset = dataOffset,
                DataSize = dataSize,
                PaletteOffset = paletteOffset,
                PaletteSize = paletteSize,
                Width = span.ReadInt16(),
                Height = span.ReadInt16()
            };

            span.Pointer += 2;

            texture.CompressionType = span.ReadByte();

            span.Pointer += 2;

            texture.Mipmaps = span.ReadByte();

            span.Pointer += 0x2E;

            pack.Add(texture);
        }
    }
}
