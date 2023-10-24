using L4RH.Model.Textures;
using System.Collections.Generic;

namespace L4RH.Model;

public class Region
{
    public virtual IList<TrackSection> Sections { get; set; }
    public virtual IList<TexturePack> TexturePacks { get; set; }
    public virtual IList<CollisionVolume> Volumes { get; set; }

    public Region()
    {
        Sections = new List<TrackSection>();
        TexturePacks = new List<TexturePack>();
        Volumes = new List<CollisionVolume>();
    }
}
