using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Tools;

public class TexToolView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexToolView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextureViewer"))
        {
            Editor.Selection.SwitchWindowContext(TextureViewerContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            // Export Texture
            if (ImGui.CollapsingHeader("Export Texture"))
            {
                UIHelper.WrappedText("Export the viewed texture.");
                UIHelper.WrappedText("");

                var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

                UIHelper.WrappedText("Export File Type:");
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                if (ImGui.Combo("##ExportType", ref index, Editor.Tools.exportTypes, Editor.Tools.exportTypes.Length))
                {
                    CFG.Current.TextureViewerToolbar_ExportTextureType = index;
                }
                UIHelper.Tooltip("The file type the exported texture will be saved as.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Export Destination:");
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                ImGui.InputText("##exportDestination", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation, 255);
                if (ImGui.Button("Select", halfButtonSize))
                {
                    string path;
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
                    if (result)
                    {
                        CFG.Current.TextureViewerToolbar_ExportTextureLocation = path;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("View Folder", halfButtonSize))
                {
                    Process.Start("explorer.exe", CFG.Current.TextureViewerToolbar_ExportTextureLocation);
                }
                UIHelper.Tooltip("The folder destination to export the texture to.");
                UIHelper.WrappedText("");

                ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
                UIHelper.Tooltip("Place the exported texture in a folder with the title of the texture container.");

                ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
                UIHelper.Tooltip("Display the confirmation message box after each export.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Export##action_Selection_ExportTexture", defaultButtonSize))
                {
                    Editor.Tools.ExportTextureHandler();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
