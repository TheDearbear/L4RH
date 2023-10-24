using System;
using System.Collections.Generic;
using System.Numerics;

namespace L4RH.Model.Solids;

public class SolidObject
{
    public byte Version { get; set; }
    public SolidFlags Flags { get; set; }
    public uint Key { get; set; }
    public uint NumTris { get; set; }
    public float Volume { get; set; }
    public float Density { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Vertice> Vertices { get; set; } = new();
    public ushort[] Indices { get; set; } = Array.Empty<ushort>();
    public List<ShadingGroup> ShadingGroups { get; set; } = new();
    public uint[] TextureHashes { get; set; } = Array.Empty<uint>();

    public int IndicesNumber { get; set; }
    public int VerticesNumber { get; set; }

    public Vector3 MinPoint { get; set; }
    public Vector3 MaxPoint { get; set; }
    public Matrix4x4 Matrix { get; set; }
}
