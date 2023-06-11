using L4RH;
using L4RH.Model;
using L4RH.Readers;
using System.Numerics;

namespace UG2Mappings;

public class TrackSectionsReader : ISectionsReader
{
    public uint ChunkId => 0x00034110;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public IList<TrackSection> Deserialize(BinarySpan span, long dataBasePosition)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        int size = (span.Length - 8) / 0x50;
        var sections = new List<TrackSection>() { Capacity = size };

        for (int i = 0; i < size; i++)
        {
            var section = new TrackSection()
            {
                Name = span.ReadString(8),
                Id = span.ReadInt32()
            };

            span.Pointer += 4;

            section.Usable = span.ReadInt32() != 0;
            section.AssociatedChunkOffset = span.ReadUInt32();

            span.Pointer += 8;

            section.Priority = span.ReadInt32();
            section.Center = span.ReadStruct<Vector2>();
            section.Radius = span.ReadSingle();

            span.Pointer += 32;

            sections.Add(section);
        }

        return sections;
    }
}