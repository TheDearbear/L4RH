using L4RH.Model.Sceneries;
using L4RH.Model.Textures;

namespace L4RH.Model;

public class Region
{
    public virtual IList<RegionSection> Sections { get; set; }
    public virtual IList<Scenery> Sceneries { get; set; }
    public virtual IList<TexturePack> TexturePacks { get; set; }
    public virtual IList<CollisionVolume> Volumes { get; set; }
    public virtual IList<VisibleSection> VisibleSections { get; set; }

    public Region()
    {
        Sections = new List<RegionSection>();
        Sceneries = new List<Scenery>();
        TexturePacks = new List<TexturePack>();
        Volumes = new List<CollisionVolume>();
        VisibleSections = new List<VisibleSection>();
    }
}
