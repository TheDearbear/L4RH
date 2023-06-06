using ImGuiNET;
using System;

namespace SpeedMapper;

internal static class UserInterface
{
    public static void DrawUI()
    {
        DrawMenuBar();
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

            ImGui.EndMainMenuBar();
        }
    }
}
