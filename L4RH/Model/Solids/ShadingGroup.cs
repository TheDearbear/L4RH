using System.Numerics;

namespace L4RH.Model.Solids;

public class ShadingGroup
{
    public Vector3 BoundsMin { get; set; }
    public Vector3 BoundsMax { get; set; }
    public int Length { get; set; }
    public int Texture { get; set; }
    public int Shader { get; set; }
    public int Offset { get; set; }
    public uint Flags { get; set; }
}
