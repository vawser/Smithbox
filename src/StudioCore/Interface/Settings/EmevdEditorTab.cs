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
using static StudioCore.Interface.Windows.SettingsWindow;

namespace StudioCore.Interface.Settings;

public class EmevdEditorTab
{
    public EmevdEditorTab() { }

    public void Display()
    {
        // Instructions
        if (ImGui.CollapsingHeader("Instructions"))
        {
            ImGui.Checkbox("Display time act aliases", ref CFG.Current.TimeActEditor_DisplayTimeActRow_AliasInfo);
            ImguiUtils.ShowHoverTooltip("Display aliases for each of the Time Act rows");
        }
    }
}
