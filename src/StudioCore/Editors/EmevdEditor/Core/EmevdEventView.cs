using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.ParamEditor;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD event selection, viewing and editing.
/// </summary>
public class EmevdEventView
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdViewSelection Selection;
    private EmevdFilters Filters;

    public EmevdEventView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.ViewSelection;
        Filters = screen.Filters;
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

        Filters.DisplayEventFilterSearch();
        ImGui.Separator();

        if (Selection.SelectedScript != null)
        {
            for (int i = 0; i < Selection.SelectedScript.Events.Count; i++)
            {
                var evt = Selection.SelectedScript.Events[i];

                var eventName = evt.Name;
                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    var itemName = ParamBank.PrimaryBank.GetParamFromName("ItemParam");
                    var itemRow = itemName.Rows.Where(e => e.ID == (int)evt.ID).FirstOrDefault();

                    if (itemRow != null)
                        eventName = itemRow.Name;
                }

                if (Filters.IsEventFilterMatch(evt))
                {
                    // Event row
                    if (ImGui.Selectable($@" {evt.ID}##eventRow{i}", evt == Selection.SelectedEvent))
                    {
                        Selection.SelectedEvent = evt;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextEvent)
                    {
                        Selection.SelectNextEvent = false;
                        Selection.SelectedEvent = evt;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextEvent = true;
                    }

                    UIHelper.DisplayColoredAlias(eventName, UI.Current.ImGui_AliasName_Text);
                }
            }
        }

        ImGui.End();
    }
}
