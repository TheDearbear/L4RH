using L4RH.Model;
using System.Collections.Generic;

namespace L4RH.Readers;

public interface IVisibleSectionReader : IChunkReader
{

    IList<VisibleSection> Deserialize(BinarySpan span, long dataBasePosition);
}
