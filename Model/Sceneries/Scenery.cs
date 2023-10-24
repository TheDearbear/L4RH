using System.Collections.Generic;
using System.Diagnostics;

namespace L4RH.Model.Sceneries;

[DebuggerDisplay("Scenery {VisibleSectionId}")]
public class Scenery
{
    public uint Offset { get; set; }
    public int VisibleSectionId { get; set; }
    public List<SceneryInstance> ObjectInstances { get; set; } = new();
    public List<SceneryInfo> ObjectInfos { get; set; } = new();
}
