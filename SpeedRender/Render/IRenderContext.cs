using L4RH.Model;
using Speed.Engine.Cache;
using Speed.Engine.Camera;
using Speed.Engine.Texture;
using System.Numerics;
using Veldrid;

namespace Speed.Engine.Render;

public interface IRenderContext : IDisposable
{
    public delegate void LogicEventHandler(object sender, double delta, InputSnapshot? input);

    /// <summary>
    /// Should be used when you implementing your logic code (Keyboard, Calculations, ...)
    /// </summary>
    event LogicEventHandler? NewLogicFrame;

    /// <summary>
    /// Should be used when you implementing your render code (Draw in render context)
    /// </summary>
    event EventHandler<double>? NewRenderFrame;

    ICamera Camera { get; }

    /// <summary>
    /// Texture storage for current context (key == texture hash)
    /// </summary>
    IDictionary<uint, IObjectTexture> TextureStorage { get; }
    TextureFSCacheController CacheController { get; }
    Region Region { get; }
    Vector4 BackgroundColor { get; set; }

    IReadOnlyList<TrackSection> RenderedSections { get; }

    void Resize(uint width, uint height);

    void DoRender(double delta);
    void DoLogic(double delta, InputSnapshot? input);

    RegionUpdateStatus UpdateRegion(Region region);
}
