using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class TextureViewerTab
{
    public TextureViewerTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("纹理显示器 Texture Viewer"))
        {
            if (ImGui.CollapsingHeader("文件列表 File List", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示角色名 Show character names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Characters);
                ImguiUtils.ShowHoverTooltip("Show matching character aliases within the file list.");

                ImGui.Checkbox("显示资源名 Show asset names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Assets);
                ImguiUtils.ShowHoverTooltip("Show matching asset/object aliases within the file list.");

                ImGui.Checkbox("显示局部名称 Show part names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Parts);
                ImguiUtils.ShowHoverTooltip("Show matching part aliases within the file list.");

                ImGui.Checkbox("显示低细节实例 Show low detail entries", ref CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries);
                ImguiUtils.ShowHoverTooltip("Show the low-detail texture containers.");
            }

            if (ImGui.CollapsingHeader("纹理列表 Texture List", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示微粒名称 Show particle names", ref CFG.Current.TextureViewer_TextureList_ShowAliasName_Particles);
                ImguiUtils.ShowHoverTooltip("在纹理列表中显示匹配的粒子别名\nShow matching particle aliases within the texture list.");
            }

            ImGui.EndTabItem();
        }
    }
}
