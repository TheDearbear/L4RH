using L4RH;
using L4RH.Model;
using L4RH.Model.Solids;
using L4RH.Readers;
using System.Numerics;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectBody : IChunkReader
{
    public uint ChunkId => 0x80134100;

    public void Deserialize(BinarySpan span, SolidObject solid)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;

        var descriptor = new SolidObjectDescriptor();
        var vertices = new SolidObjectVertices();
        var groups = new SolidObjectShadingGroups();
        var indices = new SolidObjectIndices();

        Span<byte> verts = Span<byte>.Empty;

        while (span.Pointer < span.Length)
        {
            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == descriptor.ChunkId)
            {
                descriptor.Deserialize(chunkBuffer, solid);
            }
            else if (id == vertices.ChunkId)
            {
                verts = vertices.Deserialize(chunkBuffer);
            }
            else if (id == groups.ChunkId)
            {
                groups.Deserialize(chunkBuffer, solid);
            }
            else if (id == indices.ChunkId)
            {
                indices.Deserialize(chunkBuffer, solid);
            }
        }

        if (verts.Length > 0)
            ProcessVerts(verts, solid);
    }

    private static void ProcessVerts(Span<byte> verts, SolidObject solid)
    {
        if (solid.Vertices.Capacity == 0) return;
        if (verts.Length % solid.Vertices.Capacity != 0) return;

        int stride = verts.Length / solid.Vertices.Capacity;

        var span = new BinarySpan(verts);

        while (span.Pointer < span.Length)
        {
            var vert = new Vertice(span.ReadStruct<Vector3>().SwapYZ());
            vert.Unpack(solid.MinPoint, solid.MaxPoint);
            int read = 24;

            if (stride > 24)
            {
                read += 4;
                span.Pointer += 4;
            }

            vert.SetColor(span.ReadUInt32());
            vert.TextureX = span.ReadSingle();
            vert.TextureY = span.ReadSingle();

            solid.Vertices.Add(vert);
            span.Pointer += stride - read;
        }
    }
}
