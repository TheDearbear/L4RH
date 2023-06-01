using System.Diagnostics;
using L4RH.Model.Solids;

namespace L4RH.Model;

[DebuggerDisplay("{Name} ({Id})")]
public class RegionSection
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
    public bool Usable { get; set; }
    public SolidObjectList Solids { get; set; } = new();
}
