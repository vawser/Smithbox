using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.TextEditor.Tools;
using StudioCore.Editors.TextureViewer.Actions;
using StudioCore.Platform;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Tools;

public class ToolWindow
{
    private TextureViewerScreen Screen;
    public ActionHandler Handler;

    public ToolWindow(TextureViewerScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextureViewer"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            // Export Texture
            if (ImGui.CollapsingHeader("Export Texture"))
            {
                ImguiUtils.WrappedText("Export the viewed texture.");
                ImguiUtils.WrappedText("");

                var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

                ImguiUtils.WrappedText("Export File Type:");
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                if (ImGui.Combo("##ExportType", ref index, Handler.exportTypes, Handler.exportTypes.Length))
                {
                    CFG.Current.TextureViewerToolbar_ExportTextureType = index;
                }
                ImguiUtils.ShowHoverTooltip("The file type the exported texture will be saved as.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Export Destination:");
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
                ImguiUtils.ShowHoverTooltip("The folder destination to export the texture to.");
                ImguiUtils.WrappedText("");

                ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
                ImguiUtils.ShowHoverTooltip("Place the exported texture in a folder with the title of the texture container.");

                ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
                ImguiUtils.ShowHoverTooltip("Display the confirmation message box after each export.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Export##action_Selection_ExportTexture", defaultButtonSize))
                {
                    Handler.ExportTextureHandler();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
