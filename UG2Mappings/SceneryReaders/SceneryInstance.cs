using L4RH.Readers;
using L4RH;
using System.Numerics;
using SceneryClass = L4RH.Model.Sceneries.Scenery;
using SceneryInstanceClass = L4RH.Model.Sceneries.SceneryInstance;
using L4RH.Model.Sceneries;

namespace UG2Mappings.SceneryReaders;

internal class SceneryInstance : IChunkReader
{
    public uint ChunkId => 0x00034103;

    public void Deserialize(BinarySpan span, SceneryClass scenery)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        var length = span.ReadInt32();
        var start = span.Pointer;
        span.AlignPosition();
        var chunkStart = span.Pointer - start;

        var size = (uint)(length - chunkStart) / 0x40;

        for (var i = 0; i < size; i++)
        {
            var sinfo = new SceneryInstanceClass(scenery)
            {
                BoundBoxMin = span.ReadStruct<Vector3>(),
                BoundBoxMax = span.ReadStruct<Vector3>(),
                SceneryInfo = span.ReadUInt16(),
                Flags = (InstanceFlags)span.ReadUInt16(),
                PreCullerInfo = span.ReadInt32()
            };

            var pos = span.ReadStruct<Vector3>().SwapYZ();
            var v1 = new Vector3(span.ReadInt16(), span.ReadInt16(), span.ReadInt16()) / 8192;
            var v2 = new Vector3(span.ReadInt16(), span.ReadInt16(), span.ReadInt16()) / 8192;
            var v3 = new Vector3(span.ReadInt16(), span.ReadInt16(), span.ReadInt16()) / 8192;

            span.Pointer += 2;

            var matrix = Matrix4x4.Identity with
            {
                M11 = -v1.X, M12 = v1.Z, M13 = v1.Y,
                M21 = v2.X, M22 = v2.Y, M23 = v2.Z,
                M31 = v3.X, M32 = v3.Y, M33 = v3.Z,
                M41 = -pos.X, M42 = pos.Y, M43 = pos.Z, M44 = 1
            };

            // Replace YZ
            /*Vector4 one = matrix.GetRow(1);
            Vector4 two = matrix.GetRow(2);
            matrix.SetRow(1, two);
            matrix.SetRow(2, one);

            one = matrix.GetColumn(1);
            two = matrix.GetColumn(2);
            matrix.SetColumn(1, two);
            matrix.SetColumn(2, one);*/

            // Write matrix
            sinfo.InstanceMatrix = matrix;

            scenery.ObjectInstances.Add(sinfo);
        }
    }
}
