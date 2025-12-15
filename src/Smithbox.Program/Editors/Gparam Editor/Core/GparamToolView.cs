using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

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
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        if (Screen.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor", ImGuiWindowFlags.MenuBar))
        {
            Selection.SwitchWindowContext(GparamEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_GparamEditor_Tool_QuickEdit)
            {
                if (ImGui.CollapsingHeader("Quick Edit"))
                {
                    Screen.QuickEditHandler.DisplayInputWindow();
                }

                if (ImGui.CollapsingHeader("Quick Edit Commands"))
                {
                    Screen.QuickEditHandler.DisplayCheatSheet();
                }
            }

            // Gparam Reloader
            /*
            if (GparamMemoryTools.IsGparamReloaderSupported())
            {
                if (ImGui.CollapsingHeader("Gparam Reloader"))
                {
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Reload Current Gparam", DPI.StandardButtonSize))
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
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Quick Edit"))
            {
                CFG.Current.Interface_GparamEditor_Tool_QuickEdit = !CFG.Current.Interface_GparamEditor_Tool_QuickEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_QuickEdit);

            ImGui.EndMenu();
        }
    }
}
