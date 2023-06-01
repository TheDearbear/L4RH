using L4RH.Model.Solids;

namespace L4RH.Readers;

public interface ISolidListReader : IChunkReader
{
    SolidObjectList Deserialize(BinarySpan span, long dataBasePosition);
}
