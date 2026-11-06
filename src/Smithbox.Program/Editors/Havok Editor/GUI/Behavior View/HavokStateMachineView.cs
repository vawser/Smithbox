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
        ImGui.BeginChild("havokBehaviorElementListSection");

        for (int i = 0; i < StateMachines.Count; i++)
        {
            var entry = StateMachines[i];
            var entryName = HavokTypeUtils.GetName(entry);

            var selected = SelectedStateMachines.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{entryName}##stateMachine_{entryName}{i}", selected))
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
