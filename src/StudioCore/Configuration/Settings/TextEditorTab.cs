using ImGuiNET;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class TextEditorTab
{
    public TextEditorTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);
            ImguiUtils.ShowHoverTooltip("Show the original FMG file names within the Text Editor file list.");

            if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                Smithbox.EditorHandler.TextEditor.OnProjectChanged();
            ImguiUtils.ShowHoverTooltip("If enabled then FMG entries will not be grouped automatically.");

            if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                Smithbox.EditorHandler.TextEditor.OnProjectChanged();
            ImguiUtils.ShowHoverTooltip("If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
        }
    }
}
