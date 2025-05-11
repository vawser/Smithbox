using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.GraphicsParamEditorNS;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor.Core;

public class GparamToolView
{
    private GparamEditorScreen Screen;
    private GparamSelection Selection;
    public GparamActionHandler ActionHandler;

    public GparamToolView(GparamEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ActionHandler = new GparamActionHandler(screen);
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        if (Screen.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor"))
        {
            Selection.SwitchWindowContext(GparamEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            if (ImGui.CollapsingHeader("Quick Edit"))
            {
                Screen.QuickEditHandler.DisplayInputWindow();
            }
            if (ImGui.CollapsingHeader("Quick Edit Commands"))
            {
                Screen.QuickEditHandler.DisplayCheatSheet();
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

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
