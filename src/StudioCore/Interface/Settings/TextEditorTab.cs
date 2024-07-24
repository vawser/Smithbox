using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class TextEditorTab
{
    public TextEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("文本编辑器 Text Editor"))
        {
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示原始FMG名称 Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);
                ImguiUtils.ShowHoverTooltip("Show the original FMG file names within the Text Editor file list.");

                if (ImGui.Checkbox("分离关联的FMGs和实例 Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                    Smithbox.EditorHandler.TextEditor.OnProjectChanged();
                ImguiUtils.ShowHoverTooltip("如果开启FMG实例将不自动分组 If enabled then FMG entries will not be grouped automatically.");

                if (ImGui.Checkbox("分离FMGs补丁 Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                    Smithbox.EditorHandler.TextEditor.OnProjectChanged();
                ImguiUtils.ShowHoverTooltip("如果启用DLCs的FMG文件将不被划为寻常文件组 If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
            }

            ImGui.EndTabItem();
        }
    }
}
