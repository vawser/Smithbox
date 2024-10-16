using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Configuration.SettingsWindow;

namespace StudioCore.Configuration.Settings;

public class EmevdEditorTab
{
    public EmevdEditorTab() { }

    public void Display()
    {
        // Search Filters
        if (ImGui.CollapsingHeader("Search Filters"))
        {
            ImGui.Checkbox("Propagate filter commands to all search bars", ref CFG.Current.EmevdEditor_PropagateFilterCommands);
            UIHelper.ShowHoverTooltip("Apply the special filter commands to all three of the search bars when used.");
        }

        // Instructions
        if (ImGui.CollapsingHeader("Instructions"))
        {
            ImGui.Checkbox("Display instruction category", ref CFG.Current.EmevdEditor_DisplayInstructionCategory);
            UIHelper.ShowHoverTooltip("Display the instruction category within the Instruction row.");

            ImGui.Checkbox("Display instruction parameter names", ref CFG.Current.EmevdEditor_DisplayInstructionParameterNames);
            UIHelper.ShowHoverTooltip("Display the instruction parameter names within the Instruction row.");
        }
    }
}
