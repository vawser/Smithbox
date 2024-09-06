using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Configuration.Settings.SettingsWindow;

namespace StudioCore.Configuration.Settings;

public class EmevdEditorTab
{
    public EmevdEditorTab() { }

    public void Display()
    {
        // Instructions
        if (ImGui.CollapsingHeader("Instructions"))
        {
            ImGui.Checkbox("Display instruction category", ref CFG.Current.EmevdEditor_DisplayInstructionCategory);
            ImguiUtils.ShowHoverTooltip("Display the instruction category within the Instruction row.");

            ImGui.Checkbox("Display instruction parameter names", ref CFG.Current.EmevdEditor_DisplayInstructionParameterNames);
            ImguiUtils.ShowHoverTooltip("Display the instruction parameter names within the Instruction row.");
        }
    }
}
