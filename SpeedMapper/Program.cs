#define SPEEDMAPPER_IMGUI

using ImGuiNET;
using L4RH;
using L4RH.Model;
using L4RH.Model.Sceneries;
using L4RH.Model.Solids;
using L4RH.Model.Textures;
using Speed.Engine.Camera;
using Speed.Engine.Camera.Frustum;
using Speed.Engine.DebugUtils;
using Speed.Engine.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace SpeedMapper;

internal class Program
{
    static bool _loaded = false;
    static FreeMoveCamera _camera = null!;
#if SPEEDMAPPER_IMGUI
    static ImGuiRenderer _imgui = null!;
#endif

    static void Main()
    {
        Console.WriteLine("Hello, World!");

        Single.Region = new();
        LoadRegion();

        Single.Logger = new();
        var oldOut = Console.Out;
        Console.SetOut(Single.Logger);

        Console.WriteLine("Creating window");

        #region Init Render

        Single.MainWindow = VeldridStartup.CreateWindow(new(50, 50, 1280, 720, WindowState.Normal, "Veldrid SpeedMapper"));
        Single.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Single.MainWindow, new GraphicsDeviceOptions()
        {
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            ResourceBindingModel = ResourceBindingModel.Improved,
            SyncToVerticalBlank = true,
            SwapchainDepthFormat = PixelFormat.R32_Float,
#if DEBUG
            Debug = true
#endif
        }, GraphicsBackend.Direct3D11);

        _camera = new FreeMoveCamera() { AspectRatio = 16f / 9, FOV = 75, ZFar = 5000 };
#if SPEEDMAPPER_IMGUI
        _imgui = new(Single.GraphicsDevice, Single.GraphicsDevice.SwapchainFramebuffer.OutputDescription, Single.MainWindow.Width, Single.MainWindow.Height);
        SetupImGuiStyle();
#endif

        Single.MainWindow.KeyDown += KeyDown;
        Single.MainWindow.MouseMove += Move;

        Single.MainWindow.Resized += () =>
        {
            Single.MainRenderContext.Resize((uint)Single.MainWindow.Width, (uint)Single.MainWindow.Height);
#if SPEEDMAPPER_IMGUI
            _imgui.WindowResized(Single.MainWindow.Width, Single.MainWindow.Height);
#endif
        };

        Single.MainRenderContext = new VeldridRenderContext(Single.GraphicsDevice, _camera) { BackgroundColor = new(0, 0, 0, 255) };
        Single.MainRenderContext.NewLogicFrame += OnLogic;
#if SPEEDMAPPER_IMGUI
        Single.MainRenderContext.NewRenderFrame += ImGuiRender;
#endif

        #endregion

        new DebugCube(Single.GraphicsDevice, (VeldridRenderContext)Single.MainRenderContext);

        Console.WriteLine("Show window");
        
        DateTime previous = DateTime.Now;
        InputSnapshot? input = null;
        while (Single.MainWindow.Exists)
        {
            DateTime current = DateTime.Now;
            TimeSpan delta = current - previous;

            Single.MainRenderContext.DoLogic(delta.TotalMilliseconds, input);
            Single.MainRenderContext.DoRender(delta.TotalMilliseconds);

            Single.GraphicsDevice.SwapBuffers();
            Single.GraphicsDevice.WaitForIdle();

            input = Single.MainWindow.PumpEvents();
            previous = current;
        }

        Console.Out.Close();
        Console.SetOut(oldOut);

        Console.WriteLine("Goodbye!");
    }

    static void OnLogic(object? sender, (double Delta, InputSnapshot? Input) e)
    {
        if (sender is null || sender is not IRenderContext context)
            return;

        if (_loaded)
        {
            Console.WriteLine("Updating Region. Please wait...");
            context.UpdateRegion(Single.Region);
            Console.WriteLine("Updated");
            _loaded = false;
        }

#if SPEEDMAPPER_IMGUI
        if (e.Input is not null)
            _imgui.Update((float)(e.Delta / 1000), e.Input);
#endif
    }

    static void LoadRegion()
    {
        var loader = new ChunkDeserializer();
        loader.ReadersSources.Add(Assembly.LoadFile("D:\\L4RH\\csharp\\UG2Mappings\\bin\\Debug\\net6.0\\UG2Mappings.dll"));
        loader.AddDataFromFile("D:\\L4RH\\L4RH.BUN");
        loader.AddDataFromFile("D:\\L4RH\\STREAML4RH.BUN");

        loader.SerializationEndedChunks += (sender, chunks) =>
        {
            Console.WriteLine("Serialization ended... Processing...");
            foreach (var (ChunkId, Data) in chunks)
                switch (Data)
                {
                    case IList<RegionSection> sections:
                        Console.WriteLine("Sections");
                        Single.Region.Sections = sections;
                        break;

                    case SolidObjectList list:
                        Console.WriteLine("Solids");
                        RegionSection? section = Single.Region.Sections.FirstOrDefault(section => section.Name == list.ParentSectionName);

                        if (section is null)
                        {
                            Single.Region.Sections.Add(section = new RegionSection()
                            {
                                Name = /*"UNLISTED " +*/ list.ParentSectionName,
                                Usable = true,
                                Id = -1
                            });
                        }

                        if (section.Solids.Count == 0)
                            section.Solids = list;
                        else
                            foreach (var solid in list)
                                section.Solids.Add(solid);

                        break;

                    case Scenery scenery:
                        Console.WriteLine("Scenery");
                        Single.Region.Sceneries.Add(scenery);
                        break;

                    case TexturePack pack:
                        Console.WriteLine("Textures");
                        Single.Region.TexturePacks.Add(pack);
                        break;

                    case IList<CollisionVolume> volumes:
                        Console.WriteLine("Volumes");
                        Single.Region.Volumes = volumes;
                        break;

                    case IList<VisibleSection> visibleSections:
                        Console.WriteLine("Visible Sections");
                        Single.Region.VisibleSections = visibleSections;
                        break;

                    default:
                        Single.Logger.Warn($"Unknown chunk 0x{ChunkId:X8}");
                        break;
                };

            _loaded = true;
            Console.WriteLine("Done.");
        };

        loader.Start();
    }

    static void KeyDown(KeyEvent key)
    {
#if SPEEDMAPPER_IMGUI
        if (ImGui.GetIO().WantCaptureKeyboard)
            return;
#endif

        float speed = key.Modifiers.HasFlag(ModifierKeys.Shift) ? 10 : 1;


        if (key.Key == Key.W)
        {
            _camera.Forward(speed);
        }
        if (key.Key == Key.A)
        {
            _camera.Left(speed);
        }
        if (key.Key == Key.S)
        {
            _camera.Backward(speed);
        }
        if (key.Key == Key.D)
        {
            _camera.Right(speed);
        }
        if (key.Key == Key.Space)
        {
            Vector3 pos = _camera.Position;
            pos.Y += speed;
            _camera.Position = pos;
        }
        if (key.Key == Key.ControlLeft)
        {
            Vector3 pos = _camera.Position;
            pos.Y -= speed;
            _camera.Position = pos;
        }
    }

    static void Move(MouseMoveEventArgs e)
    {
#if SPEEDMAPPER_IMGUI
        if (ImGui.GetIO().WantCaptureMouse)
            return;
#endif

        if (e.State.IsButtonDown(MouseButton.Left))
        {

            _camera.Pitch -= Single.MainWindow.MouseDelta.Y / 5;
            _camera.Yaw += Single.MainWindow.MouseDelta.X / 5;

            if (_camera.Pitch > 89)
                _camera.Pitch = 89;
            if (_camera.Pitch < -89)
                _camera.Pitch = -89;

            var x = Math.Cos(_camera.Yaw * Math.PI / 180) * Math.Cos(_camera.Pitch * Math.PI / 180);
            var y = Math.Sin(_camera.Pitch * Math.PI / 180);
            var z = Math.Sin(_camera.Yaw * Math.PI / 180) * Math.Cos(_camera.Pitch * Math.PI / 180);

            _camera.Target = Vector3.Normalize(new((float)x, (float)y, (float)z));

            //Console.WriteLine($"Delta: {delta} | Yaw: {_camera.Yaw} | Pitch: {_camera.Pitch}");
        }
    }

#if SPEEDMAPPER_IMGUI
    static void ImGuiRender(object? sender, double delta)
    {
        if (sender is not VeldridRenderContext ctx) return;

        ImGuiInterface(delta, ctx);

        var cmd = Single.GraphicsDevice.ResourceFactory.CreateCommandList();
        cmd.Begin();
        cmd.SetFramebuffer(Single.GraphicsDevice.SwapchainFramebuffer);

        _imgui.Render(Single.GraphicsDevice, cmd);

        cmd.End();
        Single.GraphicsDevice.SubmitCommands(cmd);
        Single.GraphicsDevice.DisposeWhenIdle(cmd);
    }

    static void ImGuiInterface(double delta, VeldridRenderContext ctx)
    {
        #region High-level stats
        if (ImGui.Begin("Stats"))
        {
            ImGui.Text("Total Objects: " + ctx.TotalObjects);
            ImGui.Text("Total Rendered: " + ctx.TotalRendered);
            ImGui.Text("FPS: " + Math.Round(1000 / delta, 2));

            ImGui.Text("Cam X: " + ctx.Camera.Position.X);
            ImGui.Text("Cam Y: " + ctx.Camera.Position.Y);
            ImGui.Text("Cam Z: " + ctx.Camera.Position.Z);

            if (ctx.Camera is IFrustumCamera cam)
            {
                ImGui.Text("Target X: " + cam.Target.X);
                ImGui.Text("Target Y: " + cam.Target.Y);
                ImGui.Text("Target Z: " + cam.Target.Z);
            }
        }

        ImGui.End();
        #endregion

        #region Stream zone
        if (ImGui.Begin("Sceneries in stream zone"))
        {
            ImGui.Text("Total: " + ctx.RenderedSceneries.Count);
            ImGui.Spacing();

            foreach (var scenery in ctx.RenderedSceneries)
                ImGui.Text("Name: null (lol) | Id: " + scenery.VisibleSectionId);
        }

        ImGui.End();
        #endregion

        #region Camera

        if (ImGui.Begin("Camera"))
        {
            ImGui.Text($"Position: {_camera.Position.X:000.0000} {_camera.Position.Y:000.0000} {_camera.Position.Z:000.0000}");
            ImGui.Text($"Target: {_camera.Target.X:0.0000} {_camera.Target.Y:0.0000} {_camera.Target.Z:0.0000}");
            ImGui.Text($"Up: {_camera.CameraUp.X:0.00} {_camera.CameraUp.Y:0.00} {_camera.CameraUp.Z:0.00}");
            ImGui.Text($"Right: {_camera.CameraRight.X:0.00} {_camera.CameraRight.Y:0.00} {_camera.CameraRight.Z:0.00}");

            if (_camera is IFrustumCamera cam && ImGui.Button("Snapshot/Reset view matrix"))
            {
                cam.ViewSnapshot = cam.ViewSnapshot is null ? _camera.View : null;
            }

            /*if (_camera is IFrustumCamera camera && ImGui.Button("Debug print frustum"))
            {
                camera.DebugDrawFrustum();
            }*/

            if (ImGui.CollapsingHeader("View Matrix"))
            {
                ImGui.Text($"[ {_camera.View.M11:0.0000} {_camera.View.M12:0.0000} {_camera.View.M13:0.0000} {_camera.View.M14:0.0000} ]");
                ImGui.Text($"[ {_camera.View.M21:0.0000} {_camera.View.M22:0.0000} {_camera.View.M23:0.0000} {_camera.View.M24:0.0000} ]");
                ImGui.Text($"[ {_camera.View.M31:0.0000} {_camera.View.M32:0.0000} {_camera.View.M33:0.0000} {_camera.View.M34:0.0000} ]");
                ImGui.Text($"[ {_camera.View.M41:0.0000} {_camera.View.M42:0.0000} {_camera.View.M43:0.0000} {_camera.View.M44:0.0000} ]");
            }
        }

        ImGui.End();
        #endregion

        #region Region loading box
        if (ctx.IsLoading && BeginMessageBox("Loading..."))
            ImGui.Text("Loading region please wait...");

        ImGui.End();
        #endregion

        ImGui.ShowDemoWindow();
    }

    private static void SetupImGuiStyle()
    {
        var style = ImGui.GetStyle();

        style.WindowRounding = 0;
        style.Colors[(int)ImGuiCol.TitleBg] = HexToVector(0x00D2FFFFu);
    }

    private static Vector4 HexToVector(uint color)
    {
        static float Map(uint value) => (value & 255) / 255f;

        var x = Map(color >> 24);
        var y = Map(color >> 16);
        var z = Map(color >> 8);
        var w = Map(color);

        return new(x, y, z, w);
    }

    private static bool BeginMessageBox(string name)
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoSavedSettings;

        bool result = ImGui.Begin(name, flags);
        if (result)
        {
            ImGui.SetWindowSize(new(Single.MainWindow.Width, Single.MainWindow.Height));
            ImGui.SetWindowPos(Vector2.Zero);
            ImGui.SetWindowFocus();
        }

        return result;
    }
#endif
}