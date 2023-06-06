using L4RH;
using L4RH.Model.Sceneries;
using L4RH.Readers;
using System.Numerics;

using SceneryClass = L4RH.Model.Sceneries.Scenery;
using SceneryInstanceClass = L4RH.Model.Sceneries.SceneryInstance;

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

        var size = (length - chunkStart) / 0x40;

        for (var i = 0; i < size; i++)
        {
            var instance = new SceneryInstanceClass(scenery)
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

            // Create matrix from vectors
            var matrix = new Matrix4x4(
                v1.X,  v1.Y,  v1.Z, 0,
                v2.X,  v2.Y,  v2.Z, 0,
                v3.X,  v3.Y,  v3.Z, 0,
               pos.X, pos.Y, pos.Z, 1);

            // Replace YZ
            if (Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
            {
                var scaleMatrix = Matrix4x4.CreateScale(scale);
                var rotationMatrix = Matrix4x4.CreateFromQuaternion(new(rotation.X, -rotation.Z, rotation.Y, rotation.W));
                var translationMatrix = Matrix4x4.CreateTranslation(translation);

                matrix = scaleMatrix * rotationMatrix * translationMatrix;
            }

            // Write matrix
            instance.InstanceMatrix = matrix;

            scenery.ObjectInstances.Add(instance);
        }
    }
}
