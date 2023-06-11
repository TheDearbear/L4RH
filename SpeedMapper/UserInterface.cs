using ImGuiNET;
using System.Linq;
using Veldrid;

namespace SpeedMapper;

internal static class UserInterface
{
    private static bool _showDemoWindow;

    private static bool _showMappingOptions;
    private static bool _showBindings;

    public static void DrawUI()
    {
        DrawStreamingSectionsMenu();
        DrawMenuBar();

        if (_showDemoWindow)
            ImGui.ShowDemoWindow(ref _showDemoWindow);

        if (_showMappingOptions)
            DrawMappingOptions();

        if (_showBindings)
            DrawBindings();
    }

    public static void DrawMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Project"))
                {
                    // TODO
                }

                if (ImGui.MenuItem("Open Project"))
                {
                    // TODO
                }

                if (ImGui.MenuItem("Save Project"))
                {
                    // TODO
                }

                if (ImGui.MenuItem("Close Project"))
                {
                    // TODO
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Exit"))
                {
                    // TODO Better exit handling
                    Single.MainWindow.Close();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Mapping", string.Empty, _showMappingOptions))
                {
                    _showMappingOptions = !_showMappingOptions;
                }

                if (ImGui.MenuItem("Bindings", string.Empty, _showBindings))
                {
                    _showBindings = !_showBindings;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }

    public static void DrawStreamingSectionsMenu()
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse |
                                ImGuiWindowFlags.AlwaysAutoResize;

        if (ImGui.Begin("Streaming Sections", flags))
        {
            foreach (var section in Single.MainRenderContext.RenderedSections)
            {
                if (section.Scenery is null)
                    continue;

                if (ImGui.TreeNode($"{section.Name} ({section.Id})"))
                {
                    foreach (var info in section.Scenery.ObjectInfos)
                    {
                        if (ImGui.TreeNode(info.Name))
                        {
                            foreach (var instance in section.Scenery.ObjectInstances.Where(instance => instance.Info == info))
                            {
                                ImGui.Text(instance.Position.ToString());
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }
        }

        ImGui.End();
    }

    public static void DrawMappingOptions()
    {
        if (ImGui.Begin("Mapping Options", ref _showMappingOptions, ImGuiWindowFlags.NoSavedSettings))
        {
            if (ImGui.BeginTable("Mapping Supported Chunks", 2, ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Chunk id");
                ImGui.TableSetupColumn("Class name");

                foreach (var reader in Single.Mappings)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("0x" + reader.ChunkId.ToString("X8"));
                    ImGui.TableNextColumn();
                    ImGui.Text(reader.GetType().FullName);
                }

                ImGui.EndTable();
            }
        }

        ImGui.End();
    }

    public static void DrawBindings()
    {
        if (ImGui.Begin("Key binds", ref _showBindings, ImGuiWindowFlags.NoSavedSettings))
        {
            if (ImGui.BeginTable("Key binds", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.NoSavedSettings))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Key");
                ImGui.TableSetupColumn("Pressed");

                foreach ((Key _, KeyBinder<Key>.KeyBind key) in Single.KeyBinder.KeyBinds)
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.Text(key.Name);

                    ImGui.TableNextColumn();
                    ImGui.Text(key.Binding.ToString());

                    ImGui.TableNextColumn();
                    ImGui.Text(key.Pressed.ToString());
                }

                ImGui.EndTable();
            }

        }

        ImGui.End();
    }
}
