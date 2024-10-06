using System.Numerics;
using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class TimeActToolView
{
    private TimeActEditorScreen Screen;
    private TimeActActionHandler ActionHandler;
    private TimeActSearch TimeActSearch;

    public TimeActToolView(TimeActEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
        TimeActSearch = new TimeActSearch(screen);
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TimeActEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();

            if(ImGui.CollapsingHeader("Property Search"))
            {
                TimeActSearch.Display();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
