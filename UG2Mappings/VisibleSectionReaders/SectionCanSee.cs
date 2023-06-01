using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings.VisibleSectionReaders;

internal class SectionCanSee : IChunkReader
{
    public uint ChunkId => 0x00034153;

    public void Deserialize(BinarySpan span, List<VisibleSection> sections)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        uint length = span.ReadUInt32();

        while (span.Pointer < span.Length)
        {
            int start = span.Pointer;

            span.Pointer += 8;

            ushort id = span.ReadUInt16();

            span.Pointer += 6;

            int count = span.ReadInt32();
            ushort[] ids = new ushort[count];

            for (int i = 0; i < count; i++)
                ids[i] = span.ReadUInt16();

            var section = sections.FindIndex(section => section.Id == id);
            if (section > -1)
                sections[section].CanSeeIds = ids;

            span.Pointer = start + 0xA4;
        }
    }
}
