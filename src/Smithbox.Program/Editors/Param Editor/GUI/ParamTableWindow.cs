using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.FileSystemGlobbing;
using SoapstoneLib.Proto.Internal;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor;


public class ParamTableWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamEditorView ParentView;

    public bool _arrowKeyPressed;

    public string CurrentTableGroupSearch = "";

    public List<int> CurrentTableGroupIDs = new List<int>();
    public int CurrentTableGroupID = -1;
    public List<int> CurrentTableGroupIndices = new();

    // The param the current group selection belongs to, so it can be dropped when another param is shown
    private string CurrentTableGroupParam = null;

    public int TotalChance = 0;
    // Each row of the selected group: its weight and its share of the group's total, as a percentage
    private Dictionary<Param.Row, (long Weight, float Percent)> RollChances = new();

    public ParamTableWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView parentView)
    {
        Editor = editor;
        Project = project;
        ParentView = parentView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo, string paramKey)
    {
        DisplayTitle();

        DisplayHeader(isActiveView);

        ImGui.BeginChild("rowGroupSection");
        FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

        DisplayTableList(doFocus, scrollTo);

        Shortcuts();

        ImGui.EndChild();
    }
    public void DisplayTitle()
    {
        GUI.TitleHeader(
            LOC.Get("PARAM_TableWindow_Table_List"), 
            LOC.Get("PARAM_TableWindow_Table_List_TT"));
    }

    private void DisplayHeader(bool isActiveView)
    {
        ImGui.BeginChild("ParamTableListHeaderSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        // Collapse / stretch toggle, shared with the other editors' search bars
        ImGui.AlignTextToFramePadding();
        EditorFilters.DisplaySearchbarSizeToggle();

        ImGui.AlignTextToFramePadding();
        EditorFilters.ApplySearchbarWidth();
        ImGui.InputTextWithHint($"##rowGroupSearch",
            CFG.Current.ParamEditor_Table_List_Search_Rows
                ? LOC.Get("PARAM_TableWindow_Search_Rows_Hint")
                : LOC.Get("PARAM_TableWindow_Search_Hint"),
            ref CurrentTableGroupSearch, 256);

        // Toggle: search the groups' rows (row search syntax) instead of the group IDs and names
        GUI.DisplayToggleButton("tableListSearchRows", Icons.Filter,
            ref CFG.Current.ParamEditor_Table_List_Search_Rows,
            "PARAM_TableWindow_ToggleSearchRows_Groups",
            "PARAM_TableWindow_ToggleSearchRows_Rows",
            "PARAM_TableWindow_ToggleSearchRows_Hint");

        // Toggle: row count / total weight columns
        GUI.DisplayToggleButton("tableListGroupStats", Icons.Calculator,
            ref CFG.Current.ParamEditor_Table_List_Display_Group_Stats,
            "PARAM_TableWindow_ToggleGroupStats_Hidden",
            "PARAM_TableWindow_ToggleGroupStats_Visible",
            "PARAM_TableWindow_ToggleGroupStats_Hint");

        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickTableNameExport(Editor, Project);
        }

        // Match count, from the previous frame's list (drawn after this header)
        if (_shownGroupCount >= 0 && _totalGroupCount >= 0)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(LOC.Get("PARAM_TableWindow_Search_Match_Count",
                _shownGroupCount.ToString("N0"), _totalGroupCount.ToString("N0")));
        }

        if (CFG.Current.ParamEditor_Table_List_Search_Rows)
        {
            ParamFieldUtils.DisplaySearchError(_rowSearchError);
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Draws the group list as one scrolling table whose first row (the column header) is frozen, so the header
    /// and the list share their column widths. A list clipper submits only the groups in view.
    /// </summary>
    private void DisplayTableList(bool doFocus, float scrollTo)
    {
        ImGui.BeginChild("TableListSection", ImGuiChildFlags.None);

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY;

        if (CFG.Current.ParamEditor_Enable_Table_Borders)
        {
            tblFlags = tblFlags | ImGuiTableFlags.Borders;
        }
        else
        {
            tblFlags = tblFlags | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV;
        }

        var showStats = CFG.Current.ParamEditor_Table_List_Display_Group_Stats;
        var stats = showStats ? GetGroupStats() : null;
        var showWeight = stats?.HasWeight == true;

        var columnCount = 2 + (showStats ? 1 : 0) + (showWeight ? 1 : 0);

        // The ID includes the column layout, so each layout keeps its own column widths
        if (ImGui.BeginTable($"TableListTbl_{columnCount}", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

            if (showStats)
            {
                ImGui.TableSetupColumn("Rows", ImGuiTableColumnFlags.WidthStretch, 0.15f);
            }

            if (showWeight)
            {
                ImGui.TableSetupColumn("Weight", ImGuiTableColumnFlags.WidthStretch, 0.15f);
            }

            ImGui.TableSetupScrollFreeze(columnCount, 1);

            // Header row (frozen), drawn like the Rows window's header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("PARAM_TableWindow_Col_ID"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(LOC.Get("PARAM_TableWindow_Col_Name"));

            if (showStats)
            {
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(LOC.Get("PARAM_TableWindow_Col_Rows"));
                GUI.Tooltip(LOC.Get("PARAM_TableWindow_Col_Rows_TT"));
            }

            if (showWeight)
            {
                ImGui.TableSetColumnIndex(3);
                ImGui.Text(LOC.Get("PARAM_TableWindow_Col_Weight"));
                GUI.Tooltip(LOC.Get("PARAM_TableWindow_Col_Weight_TT"));
            }

            // The IDs of the modified rows, once per frame (cached against the diff set), instead of scanning the
            // diff set for every group
            HashSet<int> modifiedGroupIds = null;
            var activeParam = ParentView.Selection.GetActiveParam();

            if (activeParam != null)
            {
                var vanillaDiffCache = Project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(activeParam);

                modifiedGroupIds = CacheBank.GetCached(ParentView.Editor, vanillaDiffCache, "tableModifiedGroupIds",
                    () => vanillaDiffCache.Select(e => e.ID).ToHashSet());
            }

            // Group IDs in the vanilla param, to tell added groups from modified ones
            var vanillaIds = GetVanillaGroupIDs();

            var shownIDs = GetShownGroupIDs();

            HandlePageKeys(shownIDs);

            // For the count in the search bar, shown next frame
            _shownGroupCount = shownIDs.Count;
            _totalGroupCount = CurrentTableGroupIDs.Count;

            var rowMatches = GetActiveRowSearch()?.MatchCounts;

            var clipper = new ImGuiListClipper();
            clipper.Begin(shownIDs.Count);

            // A pending scroll to the selected group happens inside that group's draw code (SetScrollHereY),
            // so it must be drawn even when it's off screen
            if (_scrollToGroup)
            {
                var index = shownIDs.IndexOf(CurrentTableGroupID);

                if (index >= 0)
                    clipper.IncludeItemByIndex(index);
                else
                    _scrollToGroup = false; // Not in the list (e.g. hidden by the search): nothing to scroll to
            }

            // A group being renamed stays drawn while scrolled out of view, so its input keeps its state
            if (_nameEditGroup != null)
            {
                var index = shownIDs.IndexOf(_nameEditGroup.Value);

                if (index >= 0)
                    clipper.IncludeItemByIndex(index);
                else
                    _nameEditGroup = null; // Hidden by the search or gone (e.g. undo)
            }

            // Home/End/Page Up/Page Down target: scrolled to and focused in its draw code
            if (_keyFocusGroup != null)
            {
                var index = shownIDs.IndexOf(_keyFocusGroup.Value);

                if (index >= 0)
                    clipper.IncludeItemByIndex(index);
                else
                    _keyFocusGroup = null;
            }

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DisplayTableEntry(shownIDs[i], modifiedGroupIds, vanillaIds, stats, rowMatches);
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        if (ApplyTableGroupDuplicate)
        {
            DuplicateTableGroup();
            ApplyTableGroupDuplicate = false;
        }

        if (ApplyTableGroupDelete)
        {
            DeleteTableGroup();
            ApplyTableGroupDelete = false;
        }

        if (_pendingChangeID != null)
        {
            var change = _pendingChangeID.Value;
            _pendingChangeID = null;
            ChangeGroupID(change.OldID, change.NewID);
        }

        if (_pendingNewGroupID != null)
        {
            var newID = _pendingNewGroupID.Value;
            _pendingNewGroupID = null;
            CreateNewGroup(newID);
        }

        ImGui.EndChild();
    }

    private void DisplayTableEntry(int curTableID, HashSet<int> modifiedGroupIds, HashSet<int> vanillaIds,
        GroupStats stats, Dictionary<int, int> rowMatches)
    {
        // Added (no such group in vanilla) or modified (a row differs from vanilla)
        var isAdded = vanillaIds != null && !vanillaIds.Contains(curTableID);
        var isModified = !isAdded && modifiedGroupIds != null && modifiedGroupIds.Contains(curTableID);
        bool applyDiffColoring = isAdded || isModified;

        if (isAdded)
        {
            // The shared vanilla "added" colour (VanillaMarkers)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Vanilla_Added_Text);
        }
        else if (isModified)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Vanilla_Modified_Text);
        }

        // ID
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        if (_scrollToGroup && curTableID == CurrentTableGroupID)
        {
            ImGui.SetScrollHereY();
            _scrollToGroup = false;
        }

        // Home/End/Page keys moved here: scroll to it and give it nav focus, so the arrow keys continue from it
        if (_keyFocusGroup == curTableID)
        {
            ImGui.SetScrollHereY();
            ImGui.SetKeyboardFocusHere();
            _keyFocusGroup = null;
        }

        DisplayTableSelectable_ID(curTableID);

        // Name
        ImGui.TableSetColumnIndex(1);
        DisplayTableSelectable_Name(curTableID);

        if (applyDiffColoring)
        {
            ImGui.PopStyleColor(1);
        }

        if (stats != null)
        {
            // Rows: the count, or "matches / count" while a row search is active
            ImGui.TableSetColumnIndex(2);

            var count = stats.Counts.TryGetValue(curTableID, out var c) ? c : 0;

            if (rowMatches != null)
            {
                var matches = rowMatches.TryGetValue(curTableID, out var m) ? m : 0;
                ImGui.TextDisabled($"{matches} / {count}");
            }
            else
            {
                ImGui.TextDisabled($"{count}");
            }

            if (stats.HasWeight)
            {
                ImGui.TableSetColumnIndex(3);

                var weight = stats.Weights.TryGetValue(curTableID, out var w) ? w : 0;

                // A group whose weights add up to 0 can never roll anything: flag it
                if (weight == 0)
                {
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, "0");
                    GUI.Tooltip(LOC.Get("PARAM_TableWindow_Col_Weight_Zero_TT"));
                }
                else
                {
                    ImGui.TextDisabled($"{weight:N0}");
                }
            }
        }
    }

    /// <summary>
    /// Row count and total weight per group of the active param. Cached in the CacheBank, which every action, undo
    /// and redo clears, so edits (including mass edits) are picked up.
    /// </summary>
    private sealed class GroupStats
    {
        public Dictionary<int, int> Counts = new();
        public Dictionary<int, long> Weights = new();
        public bool HasWeight;
    }

    private GroupStats GetGroupStats()
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (activeParam == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(activeParam, out var param))
            return null;

        return CacheBank.GetCached(Editor, param, "tableGroupStats", () =>
        {
            var stats = new GroupStats();
            var weightCol = GetChanceColumn(param);
            stats.HasWeight = weightCol != null;

            foreach (var row in param.Rows)
            {
                stats.Counts[row.ID] = stats.Counts.TryGetValue(row.ID, out var n) ? n + 1 : 1;

                if (weightCol != null)
                {
                    long weight = 0;

                    try
                    {
                        weight = Convert.ToInt64(weightCol.GetValue(row));
                    }
                    catch
                    {
                        // Not a number: counts as 0
                    }

                    stats.Weights[row.ID] = (stats.Weights.TryGetValue(row.ID, out var t) ? t : 0) + weight;
                }
            }

            return stats;
        });
    }

    /// <summary>
    /// Result of a row search across all groups: matching rows per group ID. Kept in the CacheBank (cleared by
    /// every action) and recomputed in place when the search text changes.
    /// </summary>
    private sealed class RowSearchResult
    {
        public string Term;
        public Dictionary<int, int> MatchCounts;
        public string Error;
    }

    private string _rowSearchError = null;

    /// <summary>
    /// The row search across groups, or null when it's off or the search box is empty
    /// </summary>
    private RowSearchResult GetActiveRowSearch()
    {
        _rowSearchError = null;

        if (!CFG.Current.ParamEditor_Table_List_Search_Rows || CurrentTableGroupSearch == "")
            return null;

        var activeParam = ParentView.Selection.GetActiveParam();

        if (activeParam == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(activeParam, out var param))
            return null;

        var result = CacheBank.GetCached(Editor, (ParentView.ViewIndex, param), "tableRowSearch",
            () => new RowSearchResult());

        if (result.Term != CurrentTableGroupSearch)
        {
            using var errors = SearchErrors.Begin();

            var rows = ParentView.MassEdit.RSE.Search(
                (Project.Handler.ParamData.PrimaryBank, param), CurrentTableGroupSearch, true, true);

            var counts = new Dictionary<int, int>();

            foreach (var row in rows)
            {
                counts[row.ID] = counts.TryGetValue(row.ID, out var n) ? n + 1 : 1;
            }

            result.Term = CurrentTableGroupSearch;
            result.MatchCounts = counts;
            result.Error = errors.FirstError;
        }

        _rowSearchError = result.Error;

        return result;
    }

    /// <summary>
    /// Scroll the group list to the selected group on the next frame (after it was selected from outside the
    /// list, e.g. by a "go to row" link).
    /// </summary>
    public void RequestScrollToGroup()
    {
        _scrollToGroup = true;
    }

    private bool _scrollToGroup = false;

    // The group Home/End/Page Up/Page Down moved to, focused when it's drawn
    private int? _keyFocusGroup = null;

    /// <summary>
    /// Home / End / Page Up / Page Down move the group selection through the shown groups (as in the Rows window).
    /// Up / Down are handled by ImGui's keyboard navigation, see <see cref="HandleArrowSelection"/>.
    /// </summary>
    private void HandlePageKeys(List<int> shownIDs)
    {
        if (shownIDs.Count == 0 || _nameEditGroup != null)
            return;

        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) || ImGui.IsAnyItemActive())
            return;

        var home = ImGui.IsKeyPressed(ImGuiKey.Home);
        var end = ImGui.IsKeyPressed(ImGuiKey.End);
        var pageUp = ImGui.IsKeyPressed(ImGuiKey.PageUp);
        var pageDown = ImGui.IsKeyPressed(ImGuiKey.PageDown);

        if (!home && !end && !pageUp && !pageDown)
            return;

        var current = shownIDs.IndexOf(CurrentTableGroupID);

        // Groups that fit in the (scrolling) table window, less one for the frozen header row
        var pageSize = Math.Max(1, (int)(ImGui.GetWindowHeight() / ImGui.GetFrameHeightWithSpacing()) - 1);

        int target;

        if (home)
            target = 0;
        else if (end)
            target = shownIDs.Count - 1;
        else if (pageUp)
            target = current < 0 ? 0 : current - pageSize;
        else
            target = current < 0 ? 0 : current + pageSize;

        target = Math.Clamp(target, 0, shownIDs.Count - 1);

        UpdateTableGroupSelection(shownIDs[target]);
        _keyFocusGroup = shownIDs[target];
    }

    /// <summary>
    /// Up / Down move ImGui's nav focus between the groups; the group that receives it is selected. Called after
    /// each of a group's selectables (ID and Name), so it works whichever column has focus. Only while this list
    /// has focus, so arrow keys pressed in another window don't leave a stale request behind.
    /// </summary>
    private void HandleArrowSelection(int curTableID)
    {
        if (InputManager.HasArrowSelection() && !ImGui.IsAnyItemActive() &&
            ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows))
        {
            _arrowKeyPressed = true;
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused())
        {
            if (CurrentTableGroupID != curTableID)
            {
                UpdateTableGroupSelection(curTableID);
            }

            _arrowKeyPressed = false;
        }
    }

    public void DisplayTableSelectable_ID(int curTableID)
    {
        var displayName = $"{curTableID}";

        if (ImGui.Selectable($"{displayName}##tableEntry_ID_{curTableID}", CurrentTableGroupID == curTableID))
        {
            UpdateTableGroupSelection(curTableID);
        }

        HandleArrowSelection(curTableID);

        DisplayContextMenu("id", curTableID);
    }

    public void DisplayTableSelectable_Name(int curTableID)
    {
        // Inline rename (Alt+click, as in the Rows window)
        if (_nameEditGroup == curTableID)
        {
            ImGui.SetNextItemWidth(-1);

            if (_focusNameEdit)
            {
                ImGui.SetKeyboardFocusHere();
                _focusNameEdit = false;
            }

            ImGui.InputText($"##tableGroupNameInput_{curTableID}", ref _nameEditBuffer, 255);

            if (GUI.InputCommitted())
            {
                // Enter, Tab or click-away commits; Escape doesn't
                ApplyNameChange(curTableID, _nameEditBuffer);
                _nameEditGroup = null;
            }
            else if (ImGui.IsItemDeactivated())
            {
                // Left without an edit, or with Escape
                _nameEditGroup = null;
            }

            return;
        }

        var displayName = GetTableName(curTableID);

        // An unnamed group shows its first named row's name, dimmed, so the column isn't blank
        var isPlaceholder = false;

        if (displayName == "")
        {
            var firstRowNames = GetFirstRowNames();

            if (firstRowNames != null && firstRowNames.TryGetValue(curTableID, out var rowName))
            {
                displayName = rowName;
                isPlaceholder = true;
            }
        }

        if (isPlaceholder)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        }

        if (ImGui.Selectable($"{Utils.ImGuiEscape(displayName)}##tableEntry_Name_{curTableID}", CurrentTableGroupID == curTableID))
        {
            if (InputManager.HasAltDown())
            {
                // Renames the clicked group only, whatever is selected. Starts from the placeholder (the first
                // row's name) so it can be adjusted rather than retyped; it's only saved if edited.
                _nameEditGroup = curTableID;
                _nameEditBuffer = displayName;
                _focusNameEdit = true;
            }
            else
            {
                UpdateTableGroupSelection(curTableID);
            }
        }

        if (isPlaceholder)
        {
            ImGui.PopStyleColor(1);
            GUI.Tooltip(LOC.Get("PARAM_TableWindow_Placeholder_Name_TT"));
        }

        HandleArrowSelection(curTableID);

        DisplayContextMenu("name", curTableID);
    }

    // The group being renamed inline, or null
    private int? _nameEditGroup = null;
    private string _nameEditBuffer = "";
    private bool _focusNameEdit = false;

    /// <summary>
    /// Rebuild the cached group names on next use (after the name store changed outside this window, e.g. an
    /// undo of a rename)
    /// </summary>
    public void InvalidateTableNames()
    {
        _nameLookupDirty = true;
    }

    /// <summary>
    /// Rename a group, as one undoable action
    /// </summary>
    private void ApplyNameChange(int groupKey, string newName)
    {
        var activeParam = ParentView.Selection.GetActiveParam();
        var store = Project.Handler.ParamData.TableGroupNames;

        if (activeParam == null || store == null)
            return;

        if (GetTableName(groupKey) == newName)
            return;

        var action = new TableGroupNamesChangedAction(Editor, store, activeParam,
            group => UpdateTableGroupNames(activeParam, group, groupKey, newName));

        action.DisplayName = LOC.Get("PARAM_TableWindow_Action_Rename", groupKey);

        Editor.ActionManager.ExecuteAction(action);
    }

    public bool IsValidMatch(int curTableID, string filterText)
    {
        var matchStr = CurrentTableGroupSearch;

        if (matchStr == "")
            return true;

        return $"{curTableID}".Contains(matchStr, StringComparison.OrdinalIgnoreCase) ||
               filterText.Contains(matchStr, StringComparison.OrdinalIgnoreCase);
    }

    public string GetTableName(int tableKey)
    {
        return GetTableNameLookup().TryGetValue(tableKey, out var name) ? name : "";
    }

    /// <summary>
    /// What the Name column shows: the group name, else the first named row's name (the placeholder)
    /// </summary>
    private string GetDisplayName(int tableKey, Dictionary<int, string> firstRowNames)
    {
        var name = GetTableName(tableKey);

        if (name == "" && firstRowNames != null && firstRowNames.TryGetValue(tableKey, out var rowName))
            return rowName;

        return name;
    }

    /// <summary>
    /// The first non-empty row name of each group of the active param. In the CacheBank, so row renames (which
    /// are actions) refresh it.
    /// </summary>
    private Dictionary<int, string> GetFirstRowNames()
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (activeParam == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(activeParam, out var param))
            return null;

        return CacheBank.GetCached(Editor, param, "tableGroupFirstRowNames", () =>
        {
            var names = new Dictionary<int, string>();

            foreach (var row in param.Rows)
            {
                if (!string.IsNullOrEmpty(row.Name))
                {
                    names.TryAdd(row.ID, row.Name);
                }
            }

            return names;
        });
    }

    /// <summary>
    /// The group IDs of the vanilla param, or null if there's no vanilla param to compare with
    /// </summary>
    private HashSet<int> GetVanillaGroupIDs()
    {
        var activeParam = ParentView.Selection.GetActiveParam();
        var vanillaBank = Project.Handler.ParamData.VanillaBank;

        if (activeParam == null || vanillaBank?.Params == null ||
            !vanillaBank.Params.TryGetValue(activeParam, out var vanillaParam) || vanillaParam == null)
            return null;

        return CacheBank.GetCached(Editor, vanillaParam, "tableVanillaGroupIds",
            () => vanillaParam.Rows.Select(r => r.ID).ToHashSet());
    }

    // Group ID -> name for the active param. Rebuilt when the param, its name group or its entry count changes,
    // or after a name/ID edit here (which change entries in place), instead of scanning the entries per group
    // per frame.
    private Dictionary<int, string> _nameLookup = new();
    private string _nameLookupParam = null;
    private TableGroupParamEntry _nameLookupGroup = null;
    private int _nameLookupCount = -1;
    private bool _nameLookupDirty = true;
    private int _nameLookupVersion = 0;

    private Dictionary<int, string> GetTableNameLookup()
    {
        var activeParam = ParentView.Selection.GetActiveParam();
        var group = Project.Handler.ParamData.TableGroupNames?.Groups?.FirstOrDefault(e => e.Param == activeParam);
        var count = group?.Entries?.Count ?? 0;

        if (_nameLookupDirty || activeParam != _nameLookupParam || group != _nameLookupGroup || count != _nameLookupCount)
        {
            _nameLookup = new();

            if (group?.Entries != null)
            {
                foreach (var entry in group.Entries)
                {
                    // First entry wins, as the old FirstOrDefault lookup did
                    _nameLookup.TryAdd(entry.ID, entry.Name ?? "");
                }
            }

            _nameLookupParam = activeParam;
            _nameLookupGroup = group;
            _nameLookupCount = count;
            _nameLookupDirty = false;
            _nameLookupVersion++;
        }

        return _nameLookup;
    }

    // The group IDs that pass the search, rebuilt only when the search, the group list or the names change
    private List<int> _shownGroupIDs = new();
    private string _shownGroupsSearch = null;
    private int _shownGroupsListVersion = -1;
    private int _shownGroupsListCount = -1;
    private int _shownGroupsNameVersion = -1;
    private int _groupListVersion = 0;

    private bool _shownGroupsRowMode = false;
    private object _shownGroupsRowSearch = null;
    private Dictionary<int, int> _shownGroupsRowMatches = null;
    private Dictionary<int, string> _shownGroupsFirstRowNames = null;

    private int _shownGroupCount = -1;
    private int _totalGroupCount = -1;

    private List<int> GetShownGroupIDs()
    {
        // Refresh the names first: a rebuild bumps the name version
        GetTableNameLookup();

        var rowMode = CFG.Current.ParamEditor_Table_List_Search_Rows;
        var rowSearch = rowMode ? GetActiveRowSearch() : null;
        var rowMatches = rowSearch?.MatchCounts;

        // The search matches what's shown, so placeholder names count too
        var firstRowNames = GetFirstRowNames();

        if (_shownGroupsSearch != CurrentTableGroupSearch ||
            _shownGroupsListVersion != _groupListVersion ||
            _shownGroupsListCount != CurrentTableGroupIDs.Count ||
            _shownGroupsNameVersion != _nameLookupVersion ||
            _shownGroupsRowMode != rowMode ||
            _shownGroupsRowSearch != rowSearch ||
            _shownGroupsRowMatches != rowMatches ||
            _shownGroupsFirstRowNames != firstRowNames)
        {
            if (rowMode)
            {
                // Groups with at least one matching row (all groups while the search box is empty)
                _shownGroupIDs = rowMatches == null
                    ? CurrentTableGroupIDs.ToList()
                    : CurrentTableGroupIDs.Where(id => rowMatches.ContainsKey(id)).ToList();
            }
            else
            {
                _shownGroupIDs = CurrentTableGroupIDs
                    .Where(id => IsValidMatch(id, GetDisplayName(id, firstRowNames)))
                    .ToList();
            }

            _shownGroupsSearch = CurrentTableGroupSearch;
            _shownGroupsListVersion = _groupListVersion;
            _shownGroupsListCount = CurrentTableGroupIDs.Count;
            _shownGroupsNameVersion = _nameLookupVersion;
            _shownGroupsRowMode = rowMode;
            _shownGroupsRowSearch = rowSearch;
            _shownGroupsRowMatches = rowMatches;
            _shownGroupsFirstRowNames = firstRowNames;
        }

        return _shownGroupIDs;
    }

    public void DisplayContextMenu(string imguiKey, int groupKey)
    {
        if (ImGui.BeginPopupContextItem($"contextmenu_{groupKey}_{imguiKey}"))
        {
            if (imguiKey == "id")
            {
                DisplayEditInput_ID(imguiKey, groupKey);
            }
            else if (imguiKey == "name")
            {
                DisplayEditInput_Name(imguiKey, groupKey);
            }

            // Duplicate
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_TableWindow_Context_Duplicate_Header")}##duplicateMenuHeader"))
            {
                DisplayDuplicateMenu();

                ImGui.EndMenu();
            }

            // New Group
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_TableWindow_Context_New_Group_Header")}##newGroupMenuHeader"))
            {
                DisplayNewGroupMenu(groupKey);

                ImGui.EndMenu();
            }

            // Delete
            if (ImGui.Selectable($"{LOC.Get("PARAM_TableWindow_Context_Action_Delete")}##deleteAction"))
            {
                ApplyTableGroupDelete = true;
            }
            GUI.Tooltip(LOC.Get("PARAM_TableWindow_Context_Action_Delete_TT"));

            ImGui.EndPopup();
        }
    }

    public void DisplayEditInput_ID(string imguiKey, int groupKey)
    {
        var newID = groupKey;

        // No +/- buttons (step 0): each click would re-ID the whole group
        ImGui.InputInt($"{LOC.Get("PARAM_TableWindow_Context_Change_ID")}##changeTableGroupID", ref newID, 0, 0);
        var committed = GUI.InputCommitted();
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_Context_Change_ID_TT"));

        if (committed && newID != groupKey)
        {
            // Applied after the list is drawn, like duplicate and delete, since it rebuilds the list
            _pendingChangeID = (groupKey, newID);
        }
    }

    private (int OldID, int NewID)? _pendingChangeID = null;
    private int? _pendingNewGroupID = null;
    private int _newGroupID = 0;

    /// <summary>
    /// "New Group" submenu: an ID (defaulting to the next free ID after this group) and Create
    /// </summary>
    public void DisplayNewGroupMenu(int groupKey)
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (activeParam == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(activeParam, out var param))
            return;

        var existing = param.Rows.Select(e => e.ID).ToHashSet();

        if (ImGui.IsWindowAppearing())
        {
            _newGroupID = FindFreeGroupID(existing, groupKey + 1);
        }

        ImGui.InputInt($"{LOC.Get("PARAM_TableWindow_New_Group_ID")}##newGroupID", ref _newGroupID, 0, 0);
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_New_Group_ID_TT"));

        var used = existing.Contains(_newGroupID);

        if (used)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);
            ImGui.TextUnformatted(LOC.Get("PARAM_TableWindow_New_Group_ID_Used",
                FindFreeGroupID(existing, _newGroupID)));
            ImGui.PopStyleColor(1);
        }

        ImGui.BeginDisabled(used);

        if (ImGui.Selectable($"{LOC.Get("PARAM_TableWindow_New_Group_Action_Create")}##createNewGroup"))
        {
            _pendingNewGroupID = _newGroupID;
        }

        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_New_Group_Action_Create_TT"));
    }

    /// <summary>
    /// The first ID at or after <paramref name="start"/> that no row uses
    /// </summary>
    private static int FindFreeGroupID(HashSet<int> existing, int start)
    {
        var id = start;

        while (existing.Contains(id) && id < int.MaxValue)
        {
            id++;
        }

        return id;
    }

    /// <summary>
    /// After an action that changes the group list: select this group (and scroll to it) if this view still shows
    /// the param. An undo can run while another param is shown, and then there's nothing to select.
    /// </summary>
    private void SelectGroupIfShown(string paramKey, int groupID)
    {
        if (ParentView.Selection.GetActiveParam() != paramKey || !IsInTableGroupMode(paramKey))
            return;

        UpdateTableGroupSelection(groupID);
        RequestScrollToGroup();
    }

    /// <summary>
    /// Give every row of a group a new ID, and move the group's name with it, as one undoable action. Refused if a
    /// row already uses the new ID, since that would merge two groups. The rows keep their places in the param.
    /// </summary>
    public void ChangeGroupID(int oldID, int newID)
    {
        var paramKey = ParentView.Selection.GetActiveParam();

        if (paramKey == null || oldID == newID ||
            !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(paramKey, out var param))
            return;

        var rows = param.Rows.Where(r => r.ID == oldID).ToList();

        if (rows.Count == 0)
            return;

        var existing = param.Rows.Select(e => e.ID).ToHashSet();

        if (existing.Contains(newID))
        {
            Smithbox.LogError(this, LOC.Get("PARAM_TableWindow_Change_ID_Error_Used",
                newID, FindFreeGroupID(existing, newID)));
            return;
        }

        var idProp = typeof(Param.Row).GetProperty(nameof(Param.Row.ID));

        var actions = rows
            .Select(r => (EditorAction)new PropertiesChangedAction(idProp, -1, r, newID))
            .ToList();

        var store = Project.Handler.ParamData.TableGroupNames;

        if (store != null && GetTableName(oldID) != "")
        {
            actions.Add(new TableGroupNamesChangedAction(Editor, store, paramKey, group =>
            {
                // An unused name left at the new ID would otherwise share the ID with the moved one
                group.Entries.RemoveAll(e => e.ID == newID);

                foreach (var entry in group.Entries.Where(e => e.ID == oldID))
                {
                    entry.ID = newID;
                }

                _nameLookupDirty = true;
            }));
        }

        var compound = new CompoundAction(actions);
        compound.DisplayName = LOC.Get("PARAM_TableWindow_Action_Change_ID", oldID, newID);

        compound.SetPostExecutionAction(undo =>
        {
            Editor.ViewHandler.RefreshTableGroupsFor(param);
            SelectGroupIfShown(paramKey, undo ? oldID : newID);
        });

        if (Editor.ActionManager.ExecuteAction(compound))
        {
            Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
    }

    /// <summary>
    /// Add a group with one row, whose fields have their paramdef defaults, as one undoable action
    /// </summary>
    public void CreateNewGroup(int newID)
    {
        var paramKey = ParentView.Selection.GetActiveParam();

        if (paramKey == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(paramKey, out var param))
            return;

        var existing = param.Rows.Select(e => e.ID).ToHashSet();

        if (existing.Contains(newID))
        {
            Smithbox.LogError(this, LOC.Get("PARAM_TableWindow_Change_ID_Error_Used",
                newID, FindFreeGroupID(existing, newID)));
            return;
        }

        // A template only: the action inserts a copy of it, in ID order
        var template = new Param.Row(newID, "", param);

        foreach (var col in param.Columns)
        {
            var def = col.Def?.Default;

            if (def == null || col.ValueType.IsArray || col.ValueType == typeof(string))
                continue;

            try
            {
                template[col].SetValue(Convert.ChangeType(def, col.ValueType, System.Globalization.CultureInfo.InvariantCulture));
            }
            catch
            {
                // Left zeroed if the default doesn't convert to the field's type
            }
        }

        var previousGroup = CurrentTableGroupID;

        var add = new AddRowsToTableGroup(Editor, param, new List<Param.Row> { template }, -1, false, true);
        var compound = new CompoundAction(new List<EditorAction> { add });
        compound.DisplayName = LOC.Get("PARAM_TableWindow_Action_New_Group", newID);

        compound.SetPostExecutionAction(undo =>
        {
            SelectGroupIfShown(paramKey, undo ? previousGroup : newID);
        });

        if (Editor.ActionManager.ExecuteAction(compound))
        {
            Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
    }

    public void DisplayEditInput_Name(string imguiKey, int groupKey)
    {
        if (Project.Handler.ParamData.TableGroupNames == null)
            return;

        var curName = GetTableName(groupKey);

        // Name Input
        ImGui.InputText("##editTableGroupName", ref curName, 255);

        if (GUI.InputCommitted())
        {
            ApplyNameChange(groupKey, curName);
        }
    }

    public void DisplayDuplicateMenu()
    {
        // Offset
        ImGui.InputInt($"{LOC.Get("PARAM_TableWindow_Duplicate_Offset")}##duplicateOffset", 
            ref CFG.Current.Param_Toolbar_Duplicate_Offset);
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_Duplicate_Offset_TT"));

        // Amount
        ImGui.InputInt($"{LOC.Get("PARAM_TableWindow_Duplicate_Amount")}##duplicateAmount", 
            ref CFG.Current.Param_Toolbar_Duplicate_Amount);
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_Duplicate_Amount_TT"));

        // Allow Unrestricted Duplicate
        ImGui.Checkbox($"{LOC.Get("PARAM_TableWindow_Duplicate_Toggle_Unrestricted")}##allowUnrestrictedDuplicate", 
            ref CFG.Current.Param_TableGroupView_AllowDuplicateInject);
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_Duplicate_Toggle_Unrestricted_TT"));

        // Apply
        if (ImGui.Selectable($"{LOC.Get("PARAM_TableWindow_Duplicate_Action_Apply")}##applyAction"))
        {
            ApplyTableGroupDuplicate = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_TableWindow_Duplicate_Action_Apply_TT"));
    }

    public void Shortcuts()
    {
        // The focus context is shared by every view, so also require this to be the active view.
        // Otherwise one key press duplicates/deletes the current group in each open view.
        if (FocusManager.Focus is EditorFocusContext.ParamEditor_TableList &&
            Editor.ViewHandler.ActiveView == ParentView)
        {
            if (!ImGui.IsAnyItemActive() && CurrentTableGroupID != -1)
            {
                if (InputManager.IsPressed(KeybindID.Duplicate))
                {
                    ApplyTableGroupDuplicate = true;
                }

                if (InputManager.IsPressed(KeybindID.Delete))
                {
                    ApplyTableGroupDelete = true;
                }
            }
        }
    }

    private bool ApplyTableGroupDuplicate = false;
    private bool ApplyTableGroupDelete = false;

    public void DuplicateTableGroup()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();
        var idOffset = CFG.Current.Param_Toolbar_Duplicate_Offset;
        var allowUnrestricted = CFG.Current.Param_TableGroupView_AllowDuplicateInject;

        if (curParamKey == null)
            return;

        Param param = Editor.Project.Handler.ParamData.PrimaryBank.Params[curParamKey];

        var newId = -1;

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroupID).ToList();

        // The group may no longer exist (e.g. it was just deleted)
        if (targetRows.Count == 0)
            return;

        // Copy k (1..amount) gets the group ID + k * offset (see ParamRowDuplicate), so check every copy's ID,
        // not just the first, or later copies silently merge into existing groups
        var groupId = targetRows.First().ID;
        var amount = Math.Max(1, CFG.Current.Param_Toolbar_Duplicate_Amount);
        var existingIds = param.Rows.Select(e => e.ID).ToHashSet();

        var collides = false;
        for (int k = 1; k <= amount; k++)
        {
            newId = groupId + k * idOffset;

            if (existingIds.Contains(newId))
            {
                collides = true;
                break;
            }
        }

        if (!allowUnrestricted && collides)
        {
            var suggested = FindFreeDuplicateOffset(existingIds, groupId, idOffset, amount);

            if (suggested != null)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_TableWindow_Duplicate_Error_Abort_Suggest", newId, suggested.Value));
            }
            else
            {
                Smithbox.LogError(this, LOC.Get("PARAM_TableWindow_Duplicate_Error_Abort"));
            }
        }
        else
        {
            ParentView.Selection.ClearRowSelection();

            foreach (var entry in targetRows)
            {
                ParentView.Selection.AddRowToSelection(entry);
            }

            CurrentTableGroupIDs.Clear();

            ParamRowDuplicate.ApplyDuplicate(ParentView, true);

            UpdateTableSelection(curParamKey);
        }
    }

    /// <summary>
    /// The nearest offset (from the current one, in its direction) at which every copy's ID is free, or null if
    /// none is found within a million steps
    /// </summary>
    private static int? FindFreeDuplicateOffset(HashSet<int> existing, int groupId, int offset, int amount)
    {
        var step = offset < 0 ? -1 : 1;
        var off = offset == 0 ? 1 : offset;

        for (int tries = 0; tries < 1_000_000; tries++, off += step)
        {
            var allFree = true;

            for (long k = 1; k <= amount; k++)
            {
                var id = groupId + k * off;

                if (id > int.MaxValue || id < int.MinValue || existing.Contains((int)id))
                {
                    allFree = false;
                    break;
                }
            }

            if (allFree)
                return off;
        }

        return null;
    }

    public void DeleteTableGroup()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = Editor.Project.Handler.ParamData.PrimaryBank.Params[curParamKey];

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroupID).ToList();

        // Nothing to delete: don't push an empty action onto the undo stack
        if (targetRows.Count == 0)
            return;

        ParentView.Selection.ClearRowSelection();

        foreach (var entry in targetRows)
        {
            ParentView.Selection.AddRowToSelection(entry);
        }

        CurrentTableGroupIDs.Clear();

        ParamRowDelete.ApplyDelete(ParentView);

        UpdateTableSelection(curParamKey);

        // Only drop the group selection if the rows actually went (the delete is refused while saving)
        if (!param.Rows.Any(e => e.ID == CurrentTableGroupID))
        {
            // One key removes a whole group, so say what went
            Smithbox.Log(this, LOC.Get("PARAM_TableWindow_Delete_Log", CurrentTableGroupID, targetRows.Count));

            CurrentTableGroupID = -1;
            RollChances = new();
            TotalChance = 0;
        }
    }

    public void UpdateTableGroupNames(string activeParam, TableGroupParamEntry curTableGroup, int groupKey, string curName)
    {
        _nameLookupDirty = true;

        if (curTableGroup == null)
        {
            var newTableGroup = new TableGroupParamEntry();
            newTableGroup.Param = activeParam;
            newTableGroup.Entries = new();

            var newTableEntry = new TableGroupEntry();
            newTableEntry.ID = groupKey;
            newTableEntry.Name = curName;

            newTableGroup.Entries.Add(newTableEntry);

            Project.Handler.ParamData.TableGroupNames.Groups.Add(newTableGroup);
        }
        else
        {
            if (curTableGroup.Entries.Any(e => e.ID == groupKey))
            {
                var curEntry = curTableGroup.Entries.FirstOrDefault(e => e.ID == groupKey);
                if (curEntry != null)
                {
                    curEntry.Name = curName;
                }
            }
            else
            {
                var newTableEntry = new TableGroupEntry();
                newTableEntry.ID = groupKey;
                newTableEntry.Name = curName;

                curTableGroup.Entries.Add(newTableEntry);
            }
        }
    }

    public void UpdateTableGroupIDs(string activeParam, TableGroupParamEntry curTableGroup, int groupKey, int newID, string curName)
    {
        _nameLookupDirty = true;

        if (curTableGroup == null)
        {
            var newTableGroup = new TableGroupParamEntry();
            newTableGroup.Param = activeParam;
            newTableGroup.Entries = new();

            var newTableEntry = new TableGroupEntry();
            newTableEntry.ID = newID;
            newTableEntry.Name = curName;

            newTableGroup.Entries.Add(newTableEntry);

            Project.Handler.ParamData.TableGroupNames.Groups.Add(newTableGroup);
        }
        else
        {
            if (curTableGroup.Entries.Any(e => e.ID == groupKey))
            {
                var curEntry = curTableGroup.Entries.FirstOrDefault(e => e.ID == groupKey);
                if (curEntry != null)
                {
                    curEntry.ID = newID;
                }
            }
            else
            {
                var newTableEntry = new TableGroupEntry();
                newTableEntry.ID = newID;
                newTableEntry.Name = curName;

                curTableGroup.Entries.Add(newTableEntry);
            }
        }
    }

    public void UpdateTableSelection()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();

        UpdateTableSelection(curParamKey);
    }

    /// <summary>
    /// Build the row group list when a valid param is selected
    /// </summary>
    /// <param name="activeParam"></param>
    public void UpdateTableSelection(string activeParam)
    {
        CurrentTableGroupIDs.Clear();

        // A group ID only means something within its own param: on a param switch, drop the old group
        // and its chances rather than showing that ID's rows (or nothing) in the new param
        if (activeParam != CurrentTableGroupParam)
        {
            _nameEditGroup = null;
            CurrentTableGroupID = -1;
            RollChances = new();
            TotalChance = 0;
            CurrentTableGroupParam = activeParam;
        }

        _groupListVersion++;

        if (IsInTableGroupMode(activeParam))
        {
            var curParam = Project.Handler.ParamData.PrimaryBank.Params[activeParam];

            // The set gives the distinct IDs in first-seen order without a List.Contains scan per row
            var seen = new HashSet<int>();

            foreach (var entry in curParam.Rows)
            {
                if (seen.Add(entry.ID))
                {
                    CurrentTableGroupIDs.Add(entry.ID);
                }
            }
        }
    }

    public void UpdateTableGroupSelection(int group)
    {
        RollChances = new();

        TotalChance = 0;

        var curParam = ParentView.Selection.GetActiveParam();

        if (curParam != null && Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(curParam, out var param))
        {
            // The weight column is looked up once, and the group's rows gathered in one pass, rather than
            // searching every row's cells by name twice
            var weightCol = GetChanceColumn(param);

            if (weightCol != null)
            {
                var groupRows = new List<(Param.Row Row, long Weight)>();
                long total = 0;

                foreach (var row in param.Rows)
                {
                    if (row.ID != group)
                        continue;

                    long weight = 0;

                    try
                    {
                        weight = Convert.ToInt64(weightCol.GetValue(row));
                    }
                    catch
                    {
                        // Not a number: counts as 0
                    }

                    groupRows.Add((row, weight));
                    total += weight;
                }

                TotalChance = (int)Math.Clamp(total, int.MinValue, int.MaxValue);

                if (total != 0)
                {
                    foreach (var (row, weight) in groupRows)
                    {
                        RollChances.TryAdd(row, (weight, (float)weight / total * 100));
                    }
                }
            }
        }

        CurrentTableGroupID = group;
        CurrentTableGroupParam = curParam;
    }

    public void DisplayTableEntryChance(Param.Row r)
    {
        if (RollChances.TryGetValue(r, out var chance))
        {
            ImGui.SameLine();

            // A row with no weight can't be rolled: dimmed, so the live entries stand out
            if (chance.Weight == 0)
            {
                ImGui.TextDisabled(" 0%");
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
                ImGui.TextUnformatted($@" {chance.Percent:F3}%");
                ImGui.PopStyleColor(1);
            }

            GUI.Tooltip(LOC.Get("PARAM_TableWindow_Roll_Chance_TT", chance.Weight, TotalChance));
        }
    }

    private const string ChanceFieldName = "chanceWeight";

    /// <summary>
    /// The roll-weight field of the active param, or "" if it has none. Driven by the field being present, so any
    /// table param with a chanceWeight field (in any game, or added by a project) gets roll chances.
    /// </summary>
    public string GetChanceFieldName()
    {
        var curParam = ParentView.Selection.GetActiveParam();

        if (curParam == null || !Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(curParam, out var param))
            return "";

        return GetChanceColumn(param) != null ? ChanceFieldName : "";
    }

    private static Param.Column GetChanceColumn(Param param)
    {
        var col = param[ChanceFieldName];

        // A single number only (not an array)
        if (col == null || col.ValueType.IsArray)
            return null;

        return col;
    }

    public bool IsInTableGroupMode(string activeParam)
    {
        if (!CFG.Current.ParamEditor_Display_Table_List)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Contains(activeParam))
        {
            return true;
        }

        return false;
    }
    public bool AllowTableGroupToggle(string activeParam)
    {
        if (!CFG.Current.ParamEditor_Display_Table_List)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Contains(activeParam))
        {
            return true;
        }

        return false;
    }

    public void WriteTableGroupNames(string writeDir = "")
    {
        // If the project type doesn't support table groups, don't attempt to write anything
        if (Project.Handler.ParamData.TableGroupNames == null)
            return;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return;

        var targetDir = writeDir;

        if (writeDir == "")
        {
            var lang = CFG.Current.ParamEditor_Import_Language;
            targetDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Param Table Names", lang);
        }

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        foreach (var group in Project.Handler.ParamData.TableGroupNames.Groups)
        {
            var filePath = Path.Combine(targetDir, $"{group.Param}.json");

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(group, typeof(TableGroupParamEntry), options);

            File.WriteAllText(filePath, json);
        }
    }
}
