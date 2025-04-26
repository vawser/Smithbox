using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptEventView
{
    public Project Project;
    public EventScriptEditor Editor;

    public EventScriptEventView(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.EventList);

        if (Editor.Selection.SelectedScript != null)
        {
            Editor.Filters.DisplayEventFilterSearch();

            ImGui.BeginChild("EventSelectionList");

            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(Editor.Selection.SelectedScript.Events.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curEntry = Editor.Selection.SelectedScript.Events[i];

                    var eventName = curEntry.Name;
                    if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                    {
                        eventName = EventScriptUtils.GetDS2ItemAlias(curEntry);
                    }

                    if (Editor.Filters.IsEventFilterMatch(curEntry))
                    {
                        // Event row
                        if (ImGui.Selectable($@" {curEntry.ID}##eventRow{i}", curEntry == Editor.Selection.SelectedEvent))
                        {
                            Editor.Selection.SelectedEvent = curEntry;
                            Editor.Selection.SelectedEventIndex = i;

                            Editor.Selection.SelectedInstruction = null;
                            Editor.Selection.SelectedInstructionIndex = -1;
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.SelectNextEvent)
                        {
                            Editor.Selection.SelectNextEvent = false;
                            Editor.Selection.SelectedEvent = curEntry;
                            Editor.Selection.SelectedEventIndex = i;

                            Editor.Selection.SelectedInstruction = null;
                            Editor.Selection.SelectedInstructionIndex = -1;
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
                                EventContextMenu(curEntry);
                            }
                        }

                        UIHelper.DisplayColoredAlias(eventName, UI.Current.ImGui_AliasName_Text);
                    }
                }
            }

            ImGui.EndChild();
        }
    }

    public void EventContextMenu(EMEVD.Event evt)
    {
        if (ImGui.BeginPopupContextItem($"EventContext##EventContext{evt.ID}"))
        {
            if (ImGui.Selectable($"Create##createActionEvent{evt.ID}"))
            {
                // TODO: event creation modal
            }

            ImGui.EndPopup();
        }
    }
}
