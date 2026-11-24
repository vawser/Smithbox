using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationSelectorView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<ICustomManualSelectorGenerator> SelectedAnimSelectors = new();
    private List<ICustomManualSelectorGenerator> AnimSelectors = new();

    // Set by an arrow key step, so the list scrolls to the new selection if it's out of view
    private bool ScrollToSelection = false;

    public HavokAnimationSelectorView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedAnimSelectors.Clear();
    }

    public void Setup(object sourceObject)
    {
        // The marker interface is implemented by the class in every SDK's class set
        AnimSelectors = HavokTreeSearch.FindAll<ICustomManualSelectorGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList();

        // Drop selections that are no longer in the graph (after a structural edit)
        SelectedAnimSelectors.RemoveAll(e => !AnimSelectors.Contains(e));
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
            var visible = AnimSelectors.Where(e => EditorFilters.IsMatch(Owner.PropFilter, HavokTypeUtils.GetName(e), Owner.ExactPropFilter)).ToList();
            var current = SelectedAnimSelectors.FirstOrDefault();
            var row = HavokListKeys.StepIndex(current == null ? -1 : visible.IndexOf(current), visible.Count, step);

            if (row >= 0)
            {
                SelectedAnimSelectors.Clear();
                SelectedAnimSelectors.Add(visible[row]);
                ScrollToSelection = true;
            }
        }

        ImGui.BeginChild("havokBehaviorElementListSection");

        for (int i = 0; i < AnimSelectors.Count; i++)
        {
            var entry = AnimSelectors[i];
            var entryName = HavokTypeUtils.GetName(entry);

            var selected = SelectedAnimSelectors.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            var status = Owner.Changes.ObjectStatus(entry);
            var coloured = VanillaMarkers.PushColour(status);
            var clicked = ImGui.Selectable($"{entryName}##animSelector_{entryName}{i}", selected);
            VanillaMarkers.PopColour(coloured);
            VanillaMarkers.Tooltip(status);

            if (selected && ReferenceEquals(entry, SelectedAnimSelectors.FirstOrDefault()))
                HavokListKeys.ScrollIfRequested(ref ScrollToSelection);

            if (clicked)
            {
                if (InputManager.HasCtrlDown())
                {
                    SelectedAnimSelectors.Add(entry);
                }
                else
                {
                    SelectedAnimSelectors.Clear();
                    SelectedAnimSelectors.Add(entry);
                }
            }

            if (ImGui.BeginPopupContextItem($"animSelectorMenu_{i}"))
            {
                Owner.ActionsView.DisplayGeneratorItems(entry, HavokBehaviorActionsView.Source.List, SelectedAnimSelectors.Cast<object>().ToList());
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
        var firstSelection = SelectedAnimSelectors.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedAnimSelectors.FirstOrDefault();

        if (firstSelection == null)
            return;

        var havokMeta = HavokMetaHelper.GetMeta(Project, firstSelection.GetType());

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = 2;
        if (CFG.Current.HavokEditor.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(firstSelection, havokMeta);

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
