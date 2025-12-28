using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamToolView
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public GparamToolView(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        if (Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(GparamEditorContext.ToolWindow);

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
                    Editor.QuickEditHandler.DisplayInputWindow();
                }

                if (ImGui.CollapsingHeader("Quick Edit Commands"))
                {
                    Editor.QuickEditHandler.DisplayCheatSheet();
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
