using L4RH.Model.Sceneries;

namespace L4RH.Readers;

public interface ISceneryReader : IChunkReader
{
    Scenery Deserialize(BinarySpan span, long dataBasePosition);
}
