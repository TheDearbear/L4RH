using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings.CollisionVolumeReaders;

public class CollisionVolumeReader : ICollisionVolumeReader
{
    public uint ChunkId => 0x80034020;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public IList<CollisionVolume> Deserialize(BinarySpan span, long dataBasePosition)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;

        var header = new VolumeHeader();
        var vertices = new VolumeVertices();
        var indices = new VolumeIndices();

        var volumes = new List<CollisionVolume>();

        CollisionVolume? currentVolume = null;
        while (span.Pointer < span.Length)
        {
            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == header.ChunkId)
            {
                if (currentVolume is not null)
                    volumes.Add(currentVolume);

                currentVolume = header.Deserialize(chunkBuffer);
            }
            else if (id == vertices.ChunkId && currentVolume is not null)
            {
                vertices.Deserialize(chunkBuffer, currentVolume);
            }
            else if (id == indices.ChunkId && currentVolume is not null)
            {
                indices.Deserialize(chunkBuffer, currentVolume);
            }
        }

        if (currentVolume is not null)
            volumes.Add(currentVolume);

        return volumes;
    }
}
