using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using System.Numerics;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamToolView
{
    public GparamEditor Editor;
    public Project Project;
    public GparamToolView(Project curPoject, GparamEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor"))
        {
            Editor.EditorFocus.SetFocusContext(GparamEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            if (ImGui.CollapsingHeader("Quick Edit"))
            {
                Editor.QuickEdit.DisplayInputWindow();
            }
            if (ImGui.CollapsingHeader("Quick Edit Commands"))
            {
                Editor.QuickEdit.DisplayCheatSheet();
            }

            // Gparam Reloader
            /*
            if (GparamMemoryTools.IsGparamReloaderSupported())
            {
                if (ImGui.CollapsingHeader("Gparam Reloader"))
                {
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Reload Current Gparam", defaultButtonSize))
                    {
                        GparamMemoryTools.ReloadCurrentGparam(Screen.Selection._selectedGparamInfo);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ReloadParam.HintText}");
                }
            }
            */
        }
    }
}
