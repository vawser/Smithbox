using System.Numerics;
using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class TimeActToolView
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionManager Selection;
    private TimeActActionHandler ActionHandler;
    private TimeActSearch TimeActSearch;

    public TimeActToolView(TimeActEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
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
            Selection.SwitchWindowContext(TimeActEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();

            if(ImGui.CollapsingHeader("Time Act Search"))
            {
                TimeActSearch.Display();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
