using ImGuiNET;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneActionMenubar
{
    private CutsceneEditorScreen Screen;
    private CutsceneActionHandler ActionHandler;

    public CutsceneActionMenubar(CutsceneEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {

            ImGui.EndMenu();
        }
    }
}
