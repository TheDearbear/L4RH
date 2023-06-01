using L4RH.Model;
using Speed.Engine.Logging;
using Speed.Engine.Render;
using Veldrid;
using Veldrid.Sdl2;

namespace SpeedMapper;

internal static class Single
{
    public static Sdl2Window MainWindow { get; set; } = null!;
    public static IRenderContext MainRenderContext { get; set; } = null!;
    public static GraphicsDevice GraphicsDevice { get; set; } = null!;
    public static ConsoleLogger Logger { get; set; } = null!;

    public static Region Region { get; set; } = null!;
}
