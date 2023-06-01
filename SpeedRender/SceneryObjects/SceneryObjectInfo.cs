using L4RH.Model.Sceneries;
using L4RH.Model.Solids;
using Speed.Engine.Render;
using System.Numerics;

namespace Speed.Engine.SceneryObjects;

/// <summary>
/// Mesh class
/// </summary>
public abstract class SceneryObjectInfo : SceneryInfo, IObjectInfo
{
    public SolidObject Solid { get; set; }
    public Vector3 MinPoint { get; set; }
    public Vector3 MaxPoint { get; set; }

    public bool ModelLoaded { get; protected set; }
    public virtual int InstancesCount { get; set; }

    public SceneryObjectInfo(SolidObject solid, SceneryInfo? info = null)
    {
        if (info is not null)
        {
            Name = info.Name;
            SolidMeshKey1 = info.SolidMeshKey1;
            SolidMeshKey2 = info.SolidMeshKey2;
            SolidMeshKey3 = info.SolidMeshKey3;
            SomeFlag1 = info.SomeFlag1;
            SomeFlag2 = info.SomeFlag2;
            Radius = info.Radius;
            HierarchyKey = info.HierarchyKey;
        }

        Solid = solid;
    }

    public abstract void LoadModel(IRenderContext context);
    public abstract void UnloadModel(IRenderContext context);
}
