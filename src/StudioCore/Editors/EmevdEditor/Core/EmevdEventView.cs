using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.EmevdEditor;
using StudioCore.Interface;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD event selection, viewing and editing.
/// </summary>
public class EmevdEventView
{
    private EmevdEditorScreen Editor;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;
    private EmevdFilters Filters;
    private EmevdContextMenu ContextMenu;

    public EmevdEventView(EmevdEditorScreen screen)
    {
        Editor = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the event view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Events##EventListView");
        Selection.SwitchWindowContext(EmevdEditorContext.EventList);

        Filters.DisplayEventFilterSearch();

        ImGui.BeginChild("EventListSection");
        Selection.SwitchWindowContext(EmevdEditorContext.EventList);

        if (Selection.SelectedScript != null)
        {
            for (int i = 0; i < Selection.SelectedScript.Events.Count; i++)
            {
                var evt = Selection.SelectedScript.Events[i];

                var eventName = evt.Name;
                if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    eventName = EmevdUtils.GetDS2ItemAlias(Editor, evt);
                }

                if (Filters.IsEventFilterMatch(evt))
                {
                    // Event row
                    if (ImGui.Selectable($@" {evt.ID}##eventRow{i}", evt == Selection.SelectedEvent))
                    {
                        Selection.SelectedEvent = evt;
                        Selection.SelectedEventIndex = i;

                        Selection.SelectedInstruction = null;
                        Selection.SelectedInstructionIndex = -1;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextEvent)
                    {
                        Selection.SelectNextEvent = false;
                        Selection.SelectedEvent = evt;
                        Selection.SelectedEventIndex = i;

                        Selection.SelectedInstruction = null;
                        Selection.SelectedInstructionIndex = -1;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextEvent = true;
                    }

                    // Only apply to selection
                    if (Selection.SelectedEventIndex != -1)
                    {
                        if (Selection.SelectedEventIndex == i)
                        {
                            ContextMenu.EventContextMenu(evt);
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
