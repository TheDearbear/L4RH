using System.Diagnostics;

namespace L4RH.Model.Sceneries;

[DebuggerDisplay("{Name}")]
public class SceneryInfo
{
    public string Name { get; set; } = string.Empty;

    public uint SolidLodA { get; set; }
    public uint SolidLodB { get; set; }
    public uint SolidLodC { get; set; }

    public ushort SolidLodAFlags { get; set; }
    public ushort SolidLodBFlags { get; set; }

    public float Radius { get; set; }
    public uint HierarchyKey { get; set; }
}
