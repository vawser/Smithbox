using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// One row per CustomManualSelectorGenerator: its animation ID and offset type, the state it's in, its clips,
/// and what the character has for it (time act categories, animation files).
/// Selecting a row edits the selector; double-clicking shows it in the Outline.
/// </summary>
public class HavokAnimationTableView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public object Selected;

    private bool OnlyMissingFiles = false;

    private enum SortMode { File, Name, AnimationID, State }
    private SortMode Sort = SortMode.File;

    private class Row
    {
        public object Selector;
        public string Name;
        public long AnimationID;
        public string OffsetType;
        public string State;
        public int ClipCount;
        public int ClipsWithFile;
        public int TimeActCategories;
        public bool? IsBasePose;
    }

    private List<Row> Rows;
    private List<Row> FilteredRows;

    // What the rows were built from, so they're rebuilt when it changes
    private HavokBehaviorSymbols RowsSymbols;
    private bool RowsAnimationsReady;
    private object RowsLastAction;
    private string FilteredFilter;
    private bool FilteredExact;
    private bool FilteredOnlyMissing;
    private SortMode FilteredSort;

    public HavokAnimationTableView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        Selected = null;
        Rows = null;
    }

    public void Setup(object sourceObject)
    {
        if (Owner.Symbols == null || !Owner.Symbols.Contains(Selected))
            Selected = null;

        Rows = null;
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

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();

        // Sort and filter
        ImGui.SetNextItemWidth(160f * DPI.UIScale());

        if (ImGui.BeginCombo("##animationTableSort", LOC.Get($"HAVOK_AnimationTable_Sort_{Sort}")))
        {
            foreach (var mode in Enum.GetValues<SortMode>())
            {
                if (ImGui.Selectable(LOC.Get($"HAVOK_AnimationTable_Sort_{mode}"), mode == Sort))
                    Sort = mode;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_AnimationTable_Sort_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("HAVOK_AnimationTable_Only_Missing")}##animationTableOnlyMissing", ref OnlyMissingFiles);
        GUI.Tooltip(LOC.Get("HAVOK_AnimationTable_Only_Missing_TT"));

        // Animation index state
        var animations = Owner.Animations;

        if (animations != null)
        {
            ImGui.SameLine();

            if (!animations.IsReady)
            {
                ImGui.TextDisabled(LOC.Get("HAVOK_AnimationRow_Loading", animations.Character));
            }
            else
            {
                if (ImGui.SmallButton($"{LOC.Get("HAVOK_AnimationTable_Reload")}##animationTableReload"))
                {
                    HavokAnimationIndex.Invalidate(Project, animations.Character);
                    Owner.Animations = HavokAnimationIndex.Get(Project, animations.Character);
                    Rows = null;
                }

                GUI.Tooltip(LOC.Get("HAVOK_AnimationTable_Reload_TT", animations.Character,
                    animations.TimeActEntryCount, animations.AnimationFileCount));
            }
        }
    }

    public void DisplayTabContents()
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        BuildRows(symbols);
        FilterRows();

        var isNightreign = Project.Descriptor.ProjectType is ProjectType.NR;
        var columnCount = isNightreign ? 8 : 7;

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("havokAnimationTable", columnCount, flags))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Selector"), ImGuiTableColumnFlags.WidthStretch, 3f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Animation_ID"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Offset"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_State"), ImGuiTableColumnFlags.WidthStretch, 2.5f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Clips"), ImGuiTableColumnFlags.WidthStretch, 0.7f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Time_Acts"), ImGuiTableColumnFlags.WidthStretch, 0.9f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Files"), ImGuiTableColumnFlags.WidthStretch, 0.9f);

        if (isNightreign)
            ImGui.TableSetupColumn(LOC.Get("HAVOK_AnimationTable_Column_Base_Pose"), ImGuiTableColumnFlags.WidthStretch, 0.8f);

        ImGui.TableHeadersRow();

        var animationsReady = Owner.Animations is { IsReady: true, Error: null };

        var clipper = new ImGuiListClipper();
        clipper.Begin(FilteredRows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = FilteredRows[i];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                var selected = ReferenceEquals(row.Selector, Selected);

                if (ImGui.Selectable($"{row.Name}##animationRow{i}", selected,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Selected = row.Selector;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        Owner.OutlineView.Navigate(row.Selector);
                }

                GUI.Tooltip(LOC.Get("HAVOK_AnimationTable_Row_TT"));

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(row.AnimationID.ToString());

                ImGui.TableSetColumnIndex(2);
                ImGui.Text(row.OffsetType);

                ImGui.TableSetColumnIndex(3);
                ImGui.Text(row.State);

                ImGui.TableSetColumnIndex(4);
                ImGui.Text(row.ClipCount.ToString());

                ImGui.TableSetColumnIndex(5);
                if (animationsReady)
                    ImGui.TextColored(row.TimeActCategories > 0 ? UI.Current.ImGui_Default_Text_Color : UI.Current.ImGui_Warning_Text_Color,
                        row.TimeActCategories.ToString());
                else
                    ImGui.TextDisabled("-");

                ImGui.TableSetColumnIndex(6);
                if (animationsReady)
                    ImGui.TextColored(row.ClipsWithFile == row.ClipCount ? UI.Current.ImGui_Default_Text_Color : UI.Current.ImGui_Warning_Text_Color,
                        $"{row.ClipsWithFile}/{row.ClipCount}");
                else
                    ImGui.TextDisabled("-");

                if (isNightreign)
                {
                    ImGui.TableSetColumnIndex(7);
                    ImGui.Text(row.IsBasePose == true ? LOC.Get("HAVOK_AnimationTable_Yes") : LOC.Get("HAVOK_AnimationTable_No"));
                }
            }
        }

        clipper.End();

        ImGui.EndTable();
    }

    #region Rows
    private void BuildRows(HavokBehaviorSymbols symbols)
    {
        var animations = Owner.Animations;
        var animationsReady = animations is { IsReady: true, Error: null };

        // Rebuilt after any edit too (an edit to m_animId or a clip's animation changes a row)
        var lastAction = View.ActionManager.PeekUndoAction();

        if (Rows != null && ReferenceEquals(RowsSymbols, symbols) && RowsAnimationsReady == animationsReady &&
            ReferenceEquals(RowsLastAction, lastAction))
            return;

        RowsLastAction = lastAction;

        Rows = new List<Row>(symbols.Selectors.Count);
        RowsSymbols = symbols;
        RowsAnimationsReady = animationsReady;
        FilteredRows = null;

        foreach (var selector in symbols.Selectors)
        {
            var animationID = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1);
            var state = symbols.FindOwningState(selector);

            var row = new Row
            {
                Selector = selector,
                Name = HavokBehaviorSymbols.GetValue(selector, "m_name") as string ?? "",
                AnimationID = animationID,
                OffsetType = HavokBehaviorSymbols.GetValue(selector, "m_offsetType")?.ToString() ?? "",
                State = state == null ? "" : HavokBehaviorSymbols.GetOutlineLabel(state),
                IsBasePose = HavokBehaviorSymbols.GetValue(selector, "m_isBasePoseAnim") as bool?
            };

            if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is IList generators)
            {
                foreach (var generator in generators)
                {
                    if (generator?.GetType().Name != "hkbClipGenerator")
                        continue;

                    row.ClipCount++;

                    if (animationsReady && animations.HasAnimationFile(HavokBehaviorSymbols.GetValue(generator, "m_animationName") as string))
                        row.ClipsWithFile++;
                }
            }

            if (animationsReady)
                row.TimeActCategories = animations.GetCategories(animationID).Count;

            Rows.Add(row);
        }
    }

    private void FilterRows()
    {
        if (FilteredRows != null &&
            FilteredFilter == Owner.PropFilter &&
            FilteredExact == Owner.ExactPropFilter &&
            FilteredOnlyMissing == OnlyMissingFiles &&
            FilteredSort == Sort)
        {
            return;
        }

        FilteredFilter = Owner.PropFilter;
        FilteredExact = Owner.ExactPropFilter;
        FilteredOnlyMissing = OnlyMissingFiles;
        FilteredSort = Sort;

        IEnumerable<Row> rows = Rows;

        if (!string.IsNullOrEmpty(Owner.PropFilter))
        {
            rows = rows.Where(r =>
                EditorFilters.IsMatch(Owner.PropFilter, r.Name, Owner.ExactPropFilter) ||
                EditorFilters.IsMatch(Owner.PropFilter, r.AnimationID.ToString(), Owner.ExactPropFilter) ||
                EditorFilters.IsMatch(Owner.PropFilter, r.State, Owner.ExactPropFilter));
        }

        if (OnlyMissingFiles)
            rows = rows.Where(r => r.ClipsWithFile < r.ClipCount);

        rows = Sort switch
        {
            SortMode.Name => rows.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase),
            SortMode.AnimationID => rows.OrderBy(r => r.AnimationID),
            SortMode.State => rows.OrderBy(r => r.State, StringComparer.OrdinalIgnoreCase).ThenBy(r => r.AnimationID),
            _ => rows
        };

        FilteredRows = rows.ToList();
    }
    #endregion

    public bool CanDisplayProperties()
    {
        return IsCurrentTab && Selected != null;
    }

    public void DisplayProperties()
    {
        if (Selected == null)
            return;

        ImGui.BeginChild("havokBehaviorPropEditSection");

        var columnCount = CFG.Current.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(Selected, HavokMetaHelper.GetMeta(Project, Selected.GetType()));

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
