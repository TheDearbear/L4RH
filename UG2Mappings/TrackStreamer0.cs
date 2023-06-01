using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings;

public class TrackStreamer0 : ISectionsReader
{
    public uint ChunkId => 0x00034110;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public IList<RegionSection> Deserialize(BinarySpan span, long dataBasePosition)
    {
        var sections = new List<RegionSection>();

        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        while (span.Pointer < span.Length)
        {
            var section = new RegionSection()
            {
                Name = span.ReadString(8),
                Id = span.ReadInt32()
            };

            span.Pointer += 4;

            section.Usable = span.ReadInt32() != 0;

            span.Pointer += 60;

            sections.Add(section);
        }

        return sections;
    }
}