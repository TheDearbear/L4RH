using System.Diagnostics;

namespace L4RH.Model.Textures;

[DebuggerDisplay("{Name}")]
public class Texture
{
    public string Name { get; set; } = string.Empty;

    public int PaletteOffset { get; set; }
    public int PaletteSize { get; set; }
    public byte[] Palette { get; set; } = Array.Empty<byte>();

    public int DataOffset { get; set; }
    public int DataSize { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public short Width { get; set; }
    public short Height { get; set; }

    public byte CompressionType { get; set; }
    public byte Mipmaps { get; set; }
}
