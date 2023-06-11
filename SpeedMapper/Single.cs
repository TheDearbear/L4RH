using L4RH.Model;
using L4RH.Readers;
using Speed.Engine.Logging;
using Speed.Engine.Render;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Sdl2;

namespace SpeedMapper;

internal static class Single
{
    /// <summary>
    /// OS-level window
    /// </summary>
    public static Sdl2Window MainWindow { get; set; } = null!;

    /// <summary>
    /// Main render context for <see cref="Region"/>
    /// </summary>
    public static IRenderContext MainRenderContext { get; set; } = null!;

    /// <summary>
    /// Current graphics device for rendering
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; set; } = null!;

    /// <summary>
    /// Simple console logger
    /// </summary>
    public static ConsoleLogger Logger { get; set; } = null!;

    /// <summary>
    /// Currently loaded region
    /// </summary>
    public static Region Region { get; set; } = null!;

    /// <summary>
    /// Mapping for L4RH
    /// </summary>
    public static List<IChunkReader> Mappings { get; set; } = new();

    /// <summary>
    /// Key binds manager
    /// </summary>
    public static KeyBinder<Key> KeyBinder { get; set; } = new();
}
