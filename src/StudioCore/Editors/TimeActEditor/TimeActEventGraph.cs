using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEventGraph
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;
    private TimeActSelectionHandler Handler;

    public TimeActEventGraph(ActionManager editorActionManager, TimeActEditorScreen screen, TimeActSelectionHandler handler)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
        Handler = handler;
    }

    private Dictionary<TAE.EventGroup, List<TAE.Event>> eventDict = null;

    public void ResetGraph()
    {
        eventDict = null;
    }

    public void Display()
    {
        var events = Handler.CurrentTimeActAnimation.Events;
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
    }
}
