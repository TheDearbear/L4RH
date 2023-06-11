using ImGuiNET;
using L4RH;
using L4RH.Model;
using L4RH.Model.Solids;
using L4RH.Model.Textures;
using L4RH.Readers;
using Speed.Engine.Camera;
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
    static ImGuiRenderer _imgui = null!;
    static RegionUpdateStatus? _regionLoading = null;

    static void Main()
    {
        Console.WriteLine("Hello, World!");

        PopulateReaders(typeof(Program).Assembly.Location[0] + ":\\L4RH\\csharp\\UG2Mappings\\bin\\Debug\\net6.0\\UG2Mappings.dll");

        Single.Region = new();
        LoadRegion();

        Single.Logger = new();
        var oldOut = Console.Out;
        Console.SetOut(Single.Logger);

        #region Key Binder
        
        Console.WriteLine("Initializing Key Binds");

        Single.KeyBinder.AddKeyBind(Key.W, "Camera forward key");
        Single.KeyBinder.AddKeyBind(Key.A, "Camera left key");
        Single.KeyBinder.AddKeyBind(Key.S, "Camera backward key");
        Single.KeyBinder.AddKeyBind(Key.D, "Camera right key");
        Single.KeyBinder.AddKeyBind(Key.Space, "Camera up key");
        Single.KeyBinder.AddKeyBind(Key.ControlLeft, "Camera down key");
        Single.KeyBinder.AddKeyBind(Key.ShiftLeft, "Camera boost key");

        #endregion

        Console.WriteLine("Creating window");

        #region Init Render

        #region Window/Backend Creating

        Single.MainWindow = VeldridStartup.CreateWindow(new(50, 50, 1280, 720, WindowState.Normal, "Veldrid SpeedMapper"));
        Single.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Single.MainWindow, new GraphicsDeviceOptions()
        {
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            ResourceBindingModel = ResourceBindingModel.Improved,
            SyncToVerticalBlank = true,
            SwapchainDepthFormat = PixelFormat.R32_Float
        });

        #endregion

        _camera = new FreeMoveCamera() { AspectRatio = 16f / 9, FOV = 75, ZFar = 5000 };
        _imgui = new(Single.GraphicsDevice, Single.GraphicsDevice.SwapchainFramebuffer.OutputDescription, Single.MainWindow.Width, Single.MainWindow.Height);
        SetupImGui();

        #region Window Setup

        Single.MainWindow.MouseMove += Move;
        Single.MainWindow.Resized += () =>
        {
            Single.MainRenderContext.Resize((uint)Single.MainWindow.Width, (uint)Single.MainWindow.Height);
            _imgui.WindowResized(Single.MainWindow.Width, Single.MainWindow.Height);
        };

        #endregion

        #region Render Context Setup

        Single.MainRenderContext = new VeldridRenderContext(Single.GraphicsDevice, _camera) { BackgroundColor = new(0, 0, 0, 255) };
        Single.MainRenderContext.NewLogicFrame += OnLogic;
        Single.MainRenderContext.NewRenderFrame += ImGuiRender;

        #endregion

        #endregion

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

    static void OnLogic(object? sender, double delta, InputSnapshot? input)
    {
        if (sender is null || sender is not IRenderContext context)
            return;

        if (_loaded)
        {
            Console.WriteLine("Updating Region. Please wait...");
            _regionLoading = context.UpdateRegion(Single.Region);
            _loaded = false;
        }

        if (input is not null)
        {
            _imgui.Update((float)(delta / 1000), input);

            Single.KeyBinder.Update(input.KeyEvents.Select(e => new ValueTuple<Key, bool>(e.Key, e.Down)));
        }

        KeyLogic();
    }

    static void LoadRegion()
    {
        var loader = new ChunkDeserializer();

        char diskLetter = typeof(Program).Assembly.Location[0];
        const string region = "L4RA";

        loader.ReadersSources.AddRange(Single.Mappings);

        loader.AddDataFromFile(diskLetter + ":\\L4RH\\GlobalB.lzc");
        loader.AddDataFromFile(diskLetter + ":\\L4RH\\" + region + ".BUN");
        loader.AddDataFromFile(diskLetter + ":\\L4RH\\STREAM" + region + ".BUN");

        loader.SerializationEndedChunks += (sender, chunks) =>
        {
            Console.WriteLine("Serialization ended... Processing...");
            foreach (var (ChunkId, Data) in chunks)
                switch (Data)
                {
                    case IList<TrackSection> sections:
                        if (Single.Region.Sections.Any())
                        {
                            foreach (var dataSection in sections)
                                Single.Region.Sections.Add(dataSection);
                        }
                        else Single.Region.Sections = sections;

                        break;

                    case SolidObjectList list:
                        if (!Single.Region.Sections.Any())
                            break;

                        TrackSection section = Single.Region.Sections.First(section => section.Name == list.ParentSectionName);

                        if (section.Solids is not null && section.Solids.Any())
                        {
                            foreach (var solid in list)
                                section.Solids.Add(solid);
                        }
                        else section.Solids = list;

                        break;

                    case TexturePack pack:
                        Single.Region.TexturePacks.Add(pack);
                        break;

                    case IList<CollisionVolume> volumes:
                        if (Single.Region.Volumes.Any())
                        {
                            foreach (var dataVolume in volumes)
                                Single.Region.Volumes.Add(dataVolume);
                        }
                        else Single.Region.Volumes = volumes;

                        break;

                    default:
                        Single.Logger.Warn($"Unhandled chunk of type {Data.GetType().Name} (0x{ChunkId:X8})");
                        break;
                };

            _loaded = true;
            Console.WriteLine("Done.");
        };

        loader.Start();
    }

    static void KeyLogic()
    {
        if (ImGui.GetIO().WantCaptureKeyboard)
            return;

        float speed = Single.KeyBinder.IsPressed(Key.ShiftLeft) ? 10 : .25f;

        if (Single.KeyBinder.IsPressed(Key.W))
        {
            _camera.LerpForward(speed, .5f);
        }
        if (Single.KeyBinder.IsPressed(Key.A))
        {
            _camera.LerpLeft(speed, .5f);
        }
        if (Single.KeyBinder.IsPressed(Key.S))
        {
            _camera.LerpBackward(speed, .5f);
        }
        if (Single.KeyBinder.IsPressed(Key.D))
        {
            _camera.LerpRight(speed, .5f);
        }
        if (Single.KeyBinder.IsPressed(Key.Space))
        {
            Vector3 pos = _camera.Position;
            pos.Y += speed;
            _camera.Position = pos;
        }
        if (Single.KeyBinder.IsPressed(Key.ControlLeft))
        {
            Vector3 pos = _camera.Position;
            pos.Y -= speed;
            _camera.Position = pos;
        }
    }

    static void Move(MouseMoveEventArgs e)
    {
        if (ImGui.GetIO().WantCaptureMouse)
            return;

        if (e.State.IsButtonDown(MouseButton.Left))
        {
            _camera.Pitch -= Single.MainWindow.MouseDelta.Y / 5;
            _camera.Yaw += Single.MainWindow.MouseDelta.X / 5;

            if (_camera.Pitch > 90)
                _camera.Pitch = 90;
            if (_camera.Pitch < -90)
                _camera.Pitch = -90;

            var x = MathF.Cos(_camera.Yaw * MathF.PI / 180) * MathF.Cos(_camera.Pitch * MathF.PI / 180);
            var y = MathF.Sin(_camera.Pitch * MathF.PI / 180);
            var z = MathF.Sin(_camera.Yaw * MathF.PI / 180) * MathF.Cos(_camera.Pitch * MathF.PI / 180);

            _camera.Target = Vector3.Normalize(new(x, y, z));
        }
    }

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
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        
        if (ImGui.Begin("Stats"))
        {
            ImGui.Text("Total Objects: " + ctx.TotalObjects);
            ImGui.Text("Total Rendered: " + ctx.TotalRendered);
            ImGui.Text("FPS: " + Math.Round(1000 / delta, 2));

            ImGui.Text("Cam X: " + ctx.Camera.Position.X);
            ImGui.Text("Cam Y: " + ctx.Camera.Position.Y);
            ImGui.Text("Cam Z: " + ctx.Camera.Position.Z);
        }
        ImGui.End();

        if (ctx.IsLoading && BeginMessageBox("Loading..."))
        {
            ImGui.Text("Loading region please wait...");

            if (_regionLoading is not null)
            {
                ImGui.Spacing();

                ImGui.Text($"Sections loaded: {_regionLoading.SectionsLoaded} / {_regionLoading.SectionsTotal}");
                ImGui.Text($"Info in section loaded: {_regionLoading.InfoLoaded} / {_regionLoading.InfoTotal}");
            }
        }
        ImGui.End();

        UserInterface.DrawUI();
    }

    private static void SetupImGui()
    {
        var style = ImGui.GetStyle();
        var io = ImGui.GetIO();

        style.WindowRounding = 0;
        style.Colors[(int)ImGuiCol.TitleBg] = HexToVector(0x00BDE5FFu);

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
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

    static void PopulateReaders(string path)
    {
        try
        {
            var asm = Assembly.LoadFrom(path);

            IEnumerable<Type> types = asm.GetExportedTypes().Where(t => t.GetInterface(nameof(IChunkReader)) is not null);
            foreach (Type type in types)
            {
                if (Activator.CreateInstance(type) is not IChunkReader chunkReader) continue;
                if (Single.Mappings.Any(m => m.ChunkId == chunkReader.ChunkId)) continue;

                Single.Mappings.Add(chunkReader);
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Unable to get/activate derived class from interface " + nameof(IChunkReader), e);
        }
    }
}