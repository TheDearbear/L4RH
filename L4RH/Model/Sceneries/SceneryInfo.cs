using System.Diagnostics;

namespace L4RH.Model.Sceneries;

[DebuggerDisplay("{Name}")]
public class SceneryInfo
{
    public string Name { get; set; } = string.Empty;

    public uint SolidMeshKey1 { get; set; }
    public uint SolidMeshKey2 { get; set; }
    public uint SolidMeshKey3 { get; set; }

    public ushort SomeFlag1 { get; set; }
    public ushort SomeFlag2 { get; set; }

    public float Radius { get; set; }
    public uint HierarchyKey { get; set; }
}
