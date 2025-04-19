using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class TextureViewerTab
{
    public TextureViewerTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show character names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Characters);
            UIHelper.Tooltip("Show matching character aliases within the file list.");

            ImGui.Checkbox("Show asset names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Assets);
            UIHelper.Tooltip("Show matching asset/object aliases within the file list.");

            ImGui.Checkbox("Show part names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Parts);
            UIHelper.Tooltip("Show matching part aliases within the file list.");

            ImGui.Checkbox("Show low detail entries", ref CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries);
            UIHelper.Tooltip("Show the low-detail texture containers.");
        }

        if (ImGui.CollapsingHeader("Texture List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show particle names", ref CFG.Current.TextureViewer_TextureList_ShowAliasName_Particles);
            UIHelper.Tooltip("Show matching particle aliases within the texture list.");
        }
    }
}
