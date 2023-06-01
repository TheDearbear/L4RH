using L4RH;
using L4RH.Model;
using L4RH.Readers;
using System.Numerics;

namespace UG2Mappings.VisibleSectionReaders;

internal class SectionPolygon : IChunkReader
{
    public uint ChunkId => 0x00034152;

    public void Deserialize(BinarySpan span, List<VisibleSection> sections)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        var length = span.ReadInt32();

        while (span.Pointer < span.Length)
        {
            long start = span.Pointer;

            span.Pointer += 8;

            ushort id = span.ReadUInt16();

            ushort pointsCount = span.ReadUInt16();
            Vector2[] polygon = new Vector2[pointsCount];

            Vector2 minPoint = span.ReadStruct<Vector2>();
            Vector2 maxPoint = span.ReadStruct<Vector2>();
            Vector2 center = span.ReadStruct<Vector2>();

            for (int i = 0; i < pointsCount; i++)
                polygon[i] = span.ReadStruct<Vector2>();

            var section = sections.FindIndex(section => section.Id == id);
            if (section > -1)
            {
                sections[section].Polygon = polygon;
                sections[section].MinPoint = minPoint;
                sections[section].MaxPoint = maxPoint;
                sections[section].Center = center;
            }
        }
    }
}
