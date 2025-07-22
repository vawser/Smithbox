using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

                // By default the name is null, as we need to apply it from EMELD
                if(evt.Name == null)
                {
                    var emeldEntry = Project.EmevdData.PrimaryBank.EventNames
                        .FirstOrDefault(e => e.Key.Filename == Editor.Selection.SelectedFileEntry.Filename);

                    if(emeldEntry.Value != null)
                    {
                        var match = emeldEntry.Value.Events.FirstOrDefault(e => e.ID == evt.ID);
                        if(match != null)
                        {
                            evt.Name = match.Name;
                        }
                    }
                }

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
                            EventContextMenu(evt);
                        }
                    }

                    UIHelper.DisplayColoredAlias(eventName, UI.Current.ImGui_AliasName_Text);
                }

                if (Editor.Selection.FocusEventSelection)
                {
                    Editor.Selection.FocusEventSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
    public void EventContextMenu(EMEVD.Event evt)
    {
        if (ImGui.BeginPopupContextItem($"EventContext##EventContext{evt.ID}"))
        {
            // Event Name
            var curName = Editor.Selection.SelectedEvent.Name;
            var name = "";
            if (curName != "")
                name = curName;

            var input = new InputTextHandler(name);
            input.Draw("##nameInput", out string newValue);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                evt.Name = newValue;

                var curEmeldSelection = Project.EmevdData.PrimaryBank.EventNames
                    .FirstOrDefault(e => e.Key.Filename == Editor.Selection.SelectedFileEntry.Filename);

                if(curEmeldSelection.Value != null)
                {
                    var emeld = curEmeldSelection.Value;

                    var eventEntry = emeld.Events.FirstOrDefault(e => e.ID == evt.ID);
                    if(eventEntry != null)
                    {
                        eventEntry.Name = newValue;
                    }
                    else
                    {
                        emeld.Events.Add(new EMELD.Event(evt.ID, evt.Name));
                    }
                }
            }

            ImGui.EndPopup();
        }
    }
}
