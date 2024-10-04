using ImGuiNET;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneToolMenubar
{
    private CutsceneEditorScreen Screen;
    private CutsceneTools Tools;

    public CutsceneToolMenubar(CutsceneEditorScreen screen)
    {
        Screen = screen;
        Tools = screen.Tools;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {

            ImGui.EndMenu();
        }
    }
}
