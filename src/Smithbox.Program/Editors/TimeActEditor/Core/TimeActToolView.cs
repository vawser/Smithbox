using System.Numerics;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class TimeActToolView
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActToolView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TimeActEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(TimeActEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_TimeActEditor_Tool_TimeActSearch)
            {
                if (ImGui.CollapsingHeader("Time Act Search"))
                {
                    Editor.TimeActSearch.Display();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Time Act Search"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_TimeActSearch = !CFG.Current.Interface_TimeActEditor_Tool_TimeActSearch;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_TimeActSearch);

            ImGui.EndMenu();
        }
    }
}
