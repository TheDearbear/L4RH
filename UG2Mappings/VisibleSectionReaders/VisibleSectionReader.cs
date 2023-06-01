using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings.VisibleSectionReaders;

public class VisibleSectionReader : IVisibleSectionReader
{
    public uint ChunkId => 0x80034150;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public IList<VisibleSection> Deserialize(BinarySpan span, long dataBasePosition)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        List<VisibleSection> sections = new();

        var ids = new SectionIds();
        var canSee = new SectionCanSee();
        var polygon = new SectionPolygon();

        while (span.Pointer < span.Length)
        {
            span.SkipPadding();

            var id = span.ReadUInt32();
            var length = span.ReadInt32();

            span.Pointer -= 8;
            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == ids.ChunkId)
            {
                sections = ids.Deserialize(chunkBuffer).Select(id => new VisibleSection() { Id = id }).ToList();
            }
            else if (id == canSee.ChunkId)
            {
                canSee.Deserialize(chunkBuffer, sections);
            }
            else if (id == polygon.ChunkId)
            {
                polygon.Deserialize(chunkBuffer, sections);
            }
        }

        return sections;
    }
}
