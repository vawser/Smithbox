using System.Numerics;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Actions;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class ToolWindow
{
    private TimeActEditorScreen Screen;
    public ActionHandler Handler;
    public TimeActSearch TimeActSearch;

    public ToolWindow(TimeActEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
        TimeActSearch = new TimeActSearch(Screen, Handler);
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

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TimeActEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();

            if(ImGui.CollapsingHeader("Search for Value"))
            {
                TimeActSearch.Display();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
