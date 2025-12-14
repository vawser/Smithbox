using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.DebugNS;

public static class Test_FLVER2
{
    public static List<string> Output = new List<string>();

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.Button("Check Loaded Model for Byte Perfectness", DPI.StandardButtonSize))
        {
            Run(baseEditor);
        }

        ImGui.Separator();

        // Info
        foreach(var entry in Output)
        {
            ImGui.Text(entry);
        }
    }

    public static void Run(Smithbox baseEditor)
    {

    }
}
