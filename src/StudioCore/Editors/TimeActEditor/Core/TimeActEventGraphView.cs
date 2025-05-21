using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
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
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActEventGraphView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private Dictionary<TAE.EventGroup, List<TAE.Event>> eventDict = null;

    public void ResetGraph()
    {
        eventDict = null;
    }

    public void Display()
    {
        ImGui.Begin("Event Graph##TimeActAnimEventGraph");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.EventGraph);

        if (!Editor.Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }
        var events = Editor.Selection.CurrentTimeActAnimation.Events;
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
