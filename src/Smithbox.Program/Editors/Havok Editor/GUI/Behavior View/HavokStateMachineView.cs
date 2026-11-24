using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokStateMachineView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<IStateMachine> SelectedStateMachines = new();
    private List<IStateMachine> StateMachines = new();

    // Set by an arrow key step, so the list scrolls to the new selection if it's out of view
    private bool ScrollToSelection = false;

    public HavokStateMachineView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedStateMachines.Clear();
    }

    public void Setup(object sourceObject)
    {
        // The marker interface is implemented by the class in every SDK's class set
        StateMachines = HavokTreeSearch.FindAll<IStateMachine>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList();

        // Drop selections that are no longer in the graph (after a structural edit)
        SelectedStateMachines.RemoveAll(e => !StateMachines.Contains(e));
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Draws this view in the Contents window (its category is selected)
    /// </summary>
    public void DisplayContents()
    {
        DisplayTabHeader();
        DisplayTabContents();
    }

    public void DisplayTabContents()
    {
        // Up / Down step the selection through the rows that pass the filter
        var step = HavokListKeys.GetStep();

        if (step != 0)
        {
            var visible = StateMachines.Where(e => EditorFilters.IsMatch(Owner.PropFilter, HavokTypeUtils.GetName(e), Owner.ExactPropFilter)).ToList();
            var current = SelectedStateMachines.FirstOrDefault();
            var row = HavokListKeys.StepIndex(current == null ? -1 : visible.IndexOf(current), visible.Count, step);

            if (row >= 0)
            {
                SelectedStateMachines.Clear();
                SelectedStateMachines.Add(visible[row]);
                ScrollToSelection = true;
            }
        }

        ImGui.BeginChild("havokBehaviorElementListSection");

        for (int i = 0; i < StateMachines.Count; i++)
        {
            var entry = StateMachines[i];
            var entryName = HavokTypeUtils.GetName(entry);

            var selected = SelectedStateMachines.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            var status = Owner.Changes.ObjectStatus(entry);
            var coloured = VanillaMarkers.PushColour(status);
            var clicked = ImGui.Selectable($"{entryName}##stateMachine_{entryName}{i}", selected);
            VanillaMarkers.PopColour(coloured);
            VanillaMarkers.Tooltip(status);

            if (selected && ReferenceEquals(entry, SelectedStateMachines.FirstOrDefault()))
                HavokListKeys.ScrollIfRequested(ref ScrollToSelection);

            if (clicked)
            {
                if (InputManager.HasCtrlDown())
                {
                    SelectedStateMachines.Add(entry);
                }
                else
                {
                    SelectedStateMachines.Clear();
                    SelectedStateMachines.Add(entry);
                }
            }

            if (ImGui.BeginPopupContextItem($"stateMachineMenu_{i}"))
            {
                Owner.ActionsView.DisplayGeneratorItems(entry, HavokBehaviorActionsView.Source.List, SelectedStateMachines.Cast<object>().ToList());
                ImGui.EndPopup();
            }
        }

        ImGui.EndChild();
    }

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();
    }

    public bool CanDisplayProperties()
    {
        var firstSelection = SelectedStateMachines.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedStateMachines.FirstOrDefault();

        if (firstSelection == null)
            return;

        var havokMeta = HavokMetaHelper.GetMeta(Project, firstSelection.GetType());

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(firstSelection, havokMeta);

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
