using System.Diagnostics;

namespace L4RH.Model.Sceneries;

[DebuggerDisplay("Scenery {VisibleSectionId}")]
public class Scenery
{
    public int VisibleSectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SceneryInstance> ObjectInstances { get; set; } = new();
    public List<SceneryInfo> ObjectInfos { get; set; } = new();
}
