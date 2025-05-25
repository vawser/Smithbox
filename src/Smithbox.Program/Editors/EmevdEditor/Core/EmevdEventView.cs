using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using System.Linq;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the EMEVD event selection, viewing and editing.
/// </summary>
public class EmevdEventView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdEventView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Events##EventListView");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.EventList);

        Editor.Filters.DisplayEventFilterSearch();

        ImGui.BeginChild("EventListSection");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.EventList);

        if (Editor.Selection.SelectedScript != null)
        {
            for (int i = 0; i < Editor.Selection.SelectedScript.Events.Count; i++)
            {
                var evt = Editor.Selection.SelectedScript.Events[i];

                var eventName = evt.Name;
                if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    eventName = EmevdUtils.GetDS2ItemAlias(Editor, evt);
                }

                if (Editor.Filters.IsEventFilterMatch(evt))
                {
                    // Event row
                    if (ImGui.Selectable($@" {evt.ID}##eventRow{i}", evt == Editor.Selection.SelectedEvent))
                    {
                        Editor.Selection.SelectEvent(evt, i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextEvent)
                    {
                        Editor.Selection.SelectNextEvent = false;
                        Editor.Selection.SelectEvent(evt, i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectNextEvent = true;
                    }

                    // Only apply to selection
                    if (Editor.Selection.SelectedEventIndex != -1)
                    {
                        if (Editor.Selection.SelectedEventIndex == i)
                        {
                            Editor.ContextMenu.EventContextMenu(evt);
                        }
                    }

                    UIHelper.DisplayColoredAlias(eventName, UI.Current.ImGui_AliasName_Text);
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
