namespace Speed.Engine.Texture;

public interface IObjectTexture : IEqualityComparer<IObjectTexture>
{
    public string Name { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public uint Mipmaps { get; init; }
    public byte[] Data { get; init; }
    public TextureFormat Format { get; init; }
    public TextureType Type { get; init; }

    public bool TextureLoaded { get; }

    void LoadTexture();
    void UnloadTexture();

    static byte[] ConvertPaletteToRaw(byte[] data, byte[] palette, int paletteIndexSize)
    {
        if (paletteIndexSize != 1 && paletteIndexSize != 2 && paletteIndexSize != 4)
            throw new ArgumentOutOfRangeException(nameof(paletteIndexSize), paletteIndexSize, "Invalid stride (x != (1, 2, 4))");

        var chunked = data.Chunk(paletteIndexSize).ToArray();
        var paletteColors = palette.Chunk(4).ToArray();
        var result = new byte[chunked.Length * 4];

        for (int i = 0; i < chunked.Length; i++)
        {
            var chunk = chunked[i];
            int index;

            if (paletteIndexSize == 1)
                index = chunk[0];
            else if (paletteIndexSize == 2)
                index = BitConverter.ToUInt16(chunk);
            else
                index = BitConverter.ToInt32(chunk);

            paletteColors[index].CopyTo(result, i * 4);
        }

        return result;
    }
}
