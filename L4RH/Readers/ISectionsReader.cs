using L4RH.Model;

namespace L4RH.Readers;

public interface ISectionsReader : IChunkReader
{
    IList<RegionSection> Deserialize(BinarySpan span, long dataBasePosition);
}
