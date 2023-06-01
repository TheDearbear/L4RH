using L4RH.Model.Textures;

namespace L4RH.Readers;

public interface ITexturePackReader : IChunkReader
{
    TexturePack Deserialize(BinarySpan span, long dataBasePosition);
}
