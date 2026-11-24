using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The Modifiers category: every modifier of the file (look-at, foot / hand IK, keyframe bones, jiggler ... and the
/// modifier lists), with what holds it and whether it's enabled. The Outline's "Modifiers" toggle shows the same
/// modifiers under their modifier generators.
/// </summary>
public class HavokModifierView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public object Selected;
    private bool ScrollToSelection;

    // The type filter (null: every type)
    private Type TypeFilter;

    private readonly List<object> Visible = new();

    public HavokModifierView(HavokEditorView view, HavokBehaviorView owner, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = owner;
    }

    public void ResetSelection()
    {
        Selected = null;
    }

    public void Setup(object sourceObject)
    {
        if (Owner.Symbols == null || !Owner.Symbols.Contains(Selected))
            Selected = null;
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Select a modifier and show this category
    /// </summary>
    public void RequestSelect(object modifier)
    {
        Selected = modifier;
        Owner.ContentsMode = HavokBehaviorContentsMode.Modifiers;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
        ScrollToSelection = true;
    }

    public void DisplayContents()
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        // Header
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);
        ImGui.EndChild();

        var types = symbols.Modifiers.Select(m => m.GetType()).Distinct().OrderBy(t => t.Name, StringComparer.Ordinal).ToList();

        ImGui.SetNextItemWidth(240f * DPI.UIScale());

        if (ImGui.BeginCombo("##modifierTypeFilter", TypeFilter?.Name ?? LOC.Get("HAVOK_Modifiers_All_Types", symbols.Modifiers.Count)))
        {
            if (ImGui.Selectable(LOC.Get("HAVOK_Modifiers_All_Types", symbols.Modifiers.Count), TypeFilter == null))
                TypeFilter = null;

            foreach (var type in types)
            {
                if (ImGui.Selectable($"{type.Name} ({symbols.Modifiers.Count(m => m.GetType() == type)})", TypeFilter == type))
                    TypeFilter = type;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_Modifiers_Type_Filter_TT"));

        if (symbols.Modifiers.Count == 0)
        {
            GUI.WrappedText(LOC.Get("HAVOK_Modifiers_None"));
            return;
        }

        // Read here, while the Contents window is current (its focus is what's checked)
        var step = HavokListKeys.GetStep();

        // List
        ImGui.BeginChild("havokBehaviorElementListSection");

        Visible.Clear();

        foreach (var modifier in symbols.Modifiers)
        {
            if (TypeFilter != null && modifier.GetType() != TypeFilter)
                continue;

            var name = HavokBehaviorSymbols.GetValue(modifier, "m_name") as string ?? "";

            if (!EditorFilters.IsMatch(Owner.PropFilter, name, Owner.ExactPropFilter, modifier.GetType().Name))
                continue;

            Visible.Add(modifier);
        }

        // Up / Down step the selection through the shown rows
        var stepRow = HavokListKeys.StepIndex(Selected == null ? -1 : Visible.IndexOf(Selected), Visible.Count, step);

        if (stepRow >= 0)
        {
            Selected = Visible[stepRow];
            Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;

            GUI.ScrollClippedRowIntoView(ImGui.GetCursorPosY(), stepRow);
        }

        var scrollTo = ScrollToSelection ? Visible.IndexOf(Selected) : -1;
        ScrollToSelection = false;

        if (scrollTo >= 0)
            ImGui.SetScrollY(scrollTo * ImGui.GetTextLineHeightWithSpacing());

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var modifier = Visible[i];
                var name = HavokBehaviorSymbols.GetValue(modifier, "m_name") as string ?? "";

                var status = Owner.Changes.ObjectStatus(modifier);
                var coloured = VanillaMarkers.PushColour(status);
                var clicked = ImGui.Selectable($"{name}##modifier_{i}", ReferenceEquals(modifier, Selected), ImGuiSelectableFlags.AllowOverlap);
                VanillaMarkers.PopColour(coloured);
                VanillaMarkers.Tooltip(status);

                if (clicked)
                {
                    Selected = modifier;
                    Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
                }

                if (ImGui.BeginPopupContextItem($"modifierMenu_{i}"))
                {
                    Selected = modifier;
                    Owner.ActionsView.DisplayModifierItems(modifier);
                    ImGui.EndPopup();
                }

                ImGui.SameLine();
                ImGui.TextDisabled(HavokBehaviorSymbols.GetOutlineTag(modifier));

                if (HavokBehaviorOperations.IsModifierEnabled(modifier) == false)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(LOC.Get("HAVOK_Modifiers_Disabled_Tag"));
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    public bool CanDisplayProperties() => IsCurrentTab && Selected != null && Owner.Symbols?.Contains(Selected) == true;

    public void DisplayProperties()
    {
        if (Selected == null)
            return;

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        // Where it runs
        var (lists, generator) = HavokBehaviorOperations.FindModifierHolders(Owner.Symbols, Selected);
        var holder = generator ?? lists.FirstOrDefault();

        if (holder != null)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_Modifiers_Held_By", HavokBehaviorSymbols.GetOutlineLabel(holder),
                Owner.Symbols.FindOutlineNode(holder) is { } node ? HavokBehaviorSymbols.GetOutlineLabel(node) : "-"));
            ImGui.Separator();
        }

        var columnCount = CFG.Current.HavokEditor.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);
        View.PropertyView.HavokPropEditGeneric(Selected, HavokMetaHelper.GetMeta(Project, Selected.GetType()));
        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
