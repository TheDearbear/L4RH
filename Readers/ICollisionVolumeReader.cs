using L4RH.Model;

namespace L4RH.Readers;

public interface ICollisionVolumeReader : IChunkReader
{
    IList<CollisionVolume> Deserialize(BinarySpan span, long dataBasePosition);
}
