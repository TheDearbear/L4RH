using L4RH.Readers;
using L4RH;
using SceneryClass = L4RH.Model.Sceneries.Scenery;
using SceneryInfoClass = L4RH.Model.Sceneries.SceneryInfo;

namespace UG2Mappings.SceneryReaders;

internal class SceneryInfo : IChunkReader
{
    public uint ChunkId => 0x00034102;

    public void Deserialize(BinarySpan span, SceneryClass scenery)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        var length = span.ReadInt32() / 0x44;

        for (var i = 0; i < length; i++)
        {
            var sinfo = new SceneryInfoClass()
            {
                Name = span.ReadString(0x20),
                SolidMeshKey1 = span.ReadUInt32(),
                SolidMeshKey2 = span.ReadUInt32(),
                SolidMeshKey3 = span.ReadUInt32(),
                SomeFlag1 = span.ReadUInt16(),
                SomeFlag2 = span.ReadUInt16()
            };

            span.Pointer += 12;

            sinfo.Radius = span.ReadSingle();
            sinfo.HierarchyKey = span.ReadUInt32();

            scenery.ObjectInfos.Add(sinfo);
        }
    }
}
