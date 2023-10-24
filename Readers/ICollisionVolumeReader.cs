using L4RH.Model;
using System.Collections.Generic;

namespace L4RH.Readers;

public interface ICollisionVolumeReader : IChunkReader
{
    IList<CollisionVolume> Deserialize(BinarySpan span, long dataBasePosition);
}
