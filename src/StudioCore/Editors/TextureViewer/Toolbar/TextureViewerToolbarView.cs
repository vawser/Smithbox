using ImGuiNET;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextureViewer;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Toolbar;

public enum TextureViewerAction
{
    None,
    ExportTexture
}

public class TextureViewerToolbarView
{

    private TextureViewerScreen _screen;

    public static TextureViewerAction SelectedAction;

    public TextureViewerToolbarView(TextureViewerScreen screen)
    {
        _screen = screen;
    }

    public void OnGui()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        if (!CFG.Current.Interface_TextureViewer_Toolbar)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar##TextureViewerToolbar"))
        {
            var width = ImGui.GetWindowWidth();
            var height = ImGui.GetWindowHeight();

            if (CFG.Current.Interface_TextureViewer_Toolbar_HorizontalOrientation)
            {
                ImGui.Columns(2);

                ImGui.BeginChild("##TextureViewerToolbar_Selection");

                ShowActionList();

                ImGui.EndChild();

                ImGui.NextColumn();

                ImGui.BeginChild("##TextureViewerToolbar_Configuration");

                ShowSelectedConfiguration();

                ImGui.EndChild();
            }
            else
            {
                ImGui.BeginChild("##TextureViewerToolbar_Selection", new Vector2((width - 10), (height / 3)));

                ShowActionList();

                ImGui.EndChild();

                ImGui.BeginChild("##TextureViewerToolbar_Configuration");

                ShowSelectedConfiguration();

                ImGui.EndChild();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ShowActionList()
    {
        ImGui.Separator();
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Actions");
        ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
        {
            CFG.Current.Interface_TextureViewer_Toolbar_HorizontalOrientation = !CFG.Current.Interface_TextureViewer_Toolbar_HorizontalOrientation;
        }
        ImguiUtils.ShowHoverTooltip("Toggle the orientation of the toolbar.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (CFG.Current.Interface_TextureViewer_PromptUser)
            {
                CFG.Current.Interface_TextureViewer_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Texture Viewer Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                CFG.Current.Interface_TextureViewer_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Texture Viewer Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        TexAction_ExportTexture.Select();
    }

    public void ShowSelectedConfiguration()
    {
        ImGui.Indent(10.0f);
        ImGui.Separator();
        ImGui.Text("Configuration");
        ImGui.Separator();

        TexAction_ExportTexture.Configure();

        TexAction_ExportTexture.Act();
    }
}
