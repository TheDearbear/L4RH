using Speed.Engine.Render;

namespace Speed.Engine.SceneryObjects;

public interface IObjectInfo
{
    public bool ModelLoaded { get; }
    public int InstancesCount { get; set; }

    public void LoadModel(IRenderContext context);
    public void UnloadModel(IRenderContext context);
}
