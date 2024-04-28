using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StudioCore.TextureViewer;

namespace StudioCore.Editors.TextureViewer.Toolbar;

public static class TexAction_ExportTexture
{
    public static void Select()
    {
        if (ImGui.RadioButton("Export Texture##tool_ExportTexture", TextureViewerToolbarView.SelectedAction == TextureViewerAction.ExportTexture))
        {
            TextureViewerToolbarView.SelectedAction = TextureViewerAction.ExportTexture;
        }
        ImguiUtils.ShowHoverTooltip("Use this to export the currently viewed texture.");
    }

    public static void Configure()
    {
        if (TextureViewerToolbarView.SelectedAction == TextureViewerAction.ExportTexture)
        {
            ImGui.Text("Export the viewed texture.");
            ImGui.Text("");

            ImGui.Text("Export Location:");
            ImGui.InputText("##location", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation, 255);
            ImguiUtils.ShowHoverTooltip("The folder location to export the texture to.");
            ImGui.Text("");
        }
    }

    public static void Act()
    {
        if (TextureViewerToolbarView.SelectedAction == TextureViewerAction.ExportTexture)
        {
            if (ImGui.Button("Apply##action_Selection_ExportTexture", new Vector2(200, 32)))
            {
                if (CFG.Current.Interface_TextureViewer_PromptUser)
                {
                    var result = PlatformUtils.Instance.MessageBox($"You are about to use the Export Texture action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        ExportTexture();
                    }
                }
                else
                {
                    ExportTexture();
                }
            }
        }
    }

    public static void ExportTexture()
    {
        if(TextureViewerScreen.CurrentTextureInView != null)
        {
            // TODO: add
        }
        else
        {
            PlatformUtils.Instance.MessageBox($"No texture is currently viewed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
