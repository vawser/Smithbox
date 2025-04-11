using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEventGraphView
{
    private TimeActEditorScreen Screen;
    private ActionManager EditorActionManager;
    private TimeActSelectionManager Selection;

    public TimeActEventGraphView(TimeActEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
        Selection = screen.Selection;
    }

    private Dictionary<TAE.EventGroup, List<TAE.Event>> eventDict = null;

    public void ResetGraph()
    {
        eventDict = null;
    }

    public void Display()
    {
        ImGui.Begin("Event Graph##TimeActAnimEventGraph");
        Selection.SwitchWindowContext(TimeActEditorContext.EventGraph);

        if (!Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }
        var events = Selection.CurrentTimeActAnimation.Events;
        var height = ImGui.GetWindowHeight();
        var width = ImGui.GetWindowWidth();

        // Build groups of events based on shared EventGroup (this is determine the 'tracks')
        if (eventDict == null)
        {
            eventDict = new();

            foreach (var evt in events)
            {
                if (eventDict.ContainsKey(evt.Group))
                {
                    eventDict[evt.Group].Add(evt);
                }
                else
                {
                    eventDict.Add(evt.Group, new List<TAE.Event> { evt });
                }
            }
        }

        var frameWidth = width / 500; // 0.05 frame width
        var trackHeight = height / events.Count;

        foreach(var entry in eventDict)
        {
            var groups = entry.Value;

            foreach (var group in groups)
            {
                var startTime = group.StartTime;
                var endTime = group.EndTime;

            }
        }

        ImGui.End();
    }
}
