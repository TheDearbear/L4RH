namespace L4RH.Model.Textures;

public class TextureStreamEntry
{
    public uint Hash { get; set; }
    public int DataOffset { get; set; }
    public int CompressedDataSize { get; set; }
    public int DecompressedDataSize { get; set; }
}
