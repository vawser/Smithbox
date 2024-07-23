using ImGuiNET;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Tools;

public class ToolSubMenu
{
    private TextEditorScreen Screen;

    public ToolSubMenu(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            // Upgrade FMG Files
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Upgrade FMG Files"))
            {
                FmgUpdater.UpdateFMGs();
            }

            ImGui.EndMenu();
        }
    }
}
