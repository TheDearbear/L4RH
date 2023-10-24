using System;
using System.Diagnostics;

namespace L4RH.Model;

[DebuggerDisplay("{Name} (0x{SomeHash:X8})")]
public class CollisionVolume
{
    public float[] Vertices { get; set; } = Array.Empty<float>();
    public ushort[] Indices { get; set; } = Array.Empty<ushort>();
    public string Name { get; set; } = string.Empty;
    public uint SomeHash { get; set; }
}
