using Hexa.NET.ImGui;
using System.Collections.Generic;

namespace StudioCore.Application;

public static class Test_FLVER2
{
    public static List<string> Output = new List<string>();

    public static void Display(ProjectEntry project)
    {
        if (ImGui.Button("Check Loaded Model for Byte Perfectness", DPI.StandardButtonSize))
        {
            Run();
        }

        ImGui.Separator();

        // Info
        foreach(var entry in Output)
        {
            ImGui.Text(entry);
        }
    }

    public static void Run()
    {

    }
}
