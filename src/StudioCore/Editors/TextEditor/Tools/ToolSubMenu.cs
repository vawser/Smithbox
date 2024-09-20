using ImGuiNET;
using StudioCore.Core.Project;
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
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                // Upgrade FMG Files
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.MenuItem("Upgrade Text Files"))
                {
                    FmgUpdater.UpdateFMGs();
                }
            }

            ImGui.EndMenu();
        }
    }
}
