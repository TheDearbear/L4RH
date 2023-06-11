using L4RH.Model;
using L4RH.Model.Textures;
using Speed.Engine.ObjectTracking;

namespace Speed.Engine.TrackableModel;

public class TrackableRegion : Region
{
    public NotifyList<TrackSection> TrackableSections { get; set; } = new();
    public NotifyList<TexturePack> TrackableTexturePacks { get; set; } = new();
    public NotifyList<CollisionVolume> TrackableVolumes { get; set; } = new();

    public override IList<TrackSection> Sections
    {
        get => TrackableSections;
        set
        {
            if (value == TrackableSections) return;

            TrackableSections.Clear();
            TrackableSections.AddRange(value);
        }
    }

    public override IList<TexturePack> TexturePacks
    {
        get => TrackableTexturePacks;
        set
        {
            if (value == TrackableTexturePacks) return;

            TrackableTexturePacks.Clear();
            TrackableTexturePacks.AddRange(value);
        }
    }

    public override IList<CollisionVolume> Volumes
    {
        get => TrackableVolumes;
        set
        {
            if (value == TrackableVolumes) return;

            TrackableVolumes.Clear();
            TrackableVolumes.AddRange(value);
        }
    }
}
