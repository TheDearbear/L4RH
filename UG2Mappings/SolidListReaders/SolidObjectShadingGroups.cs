using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;
using System.Diagnostics;
using System.Numerics;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectShadingGroups : IChunkReader
{
    public uint ChunkId => 0x00134B02;

    public void Deserialize(BinarySpan span, SolidObject solid)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        int length = span.ReadInt32();
        length -= span.AlignPosition();

        Debug.Assert(length % 60 == 0);

        int materialsCount = length / 60;
        for (int i = 0; i < materialsCount; i++)
        {
            ShadingGroup group = new()
            {
                BoundsMin = span.ReadStruct<Vector3>().SwapYZ(),
                Length = span.ReadInt32(),
                BoundsMax = span.ReadStruct<Vector3>().SwapYZ(),
                Texture = span.ReadInt32(),
                Shader = span.ReadInt32()
            };

            span.Pointer += 16;

            group.Offset = span.ReadInt32();
            group.Flags = span.ReadUInt32();

            solid.ShadingGroups.Add(group);
        }
    }
}
