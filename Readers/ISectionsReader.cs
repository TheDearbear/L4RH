using L4RH.Model;
using System.Collections.Generic;

namespace L4RH.Readers;

public interface ISectionsReader : IChunkReader
{
    IList<TrackSection> Deserialize(BinarySpan span, long dataBasePosition);
}
