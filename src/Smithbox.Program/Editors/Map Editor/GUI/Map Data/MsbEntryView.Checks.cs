using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The entry table's checks: the map's issues (the Save Check's broken references, duplicate entity IDs and names,
/// missing param rows), what refers to each entry, its vanilla status, and following the selection made elsewhere.
/// </summary>
public partial class MsbEntryView
{
    // The table's fixed columns, before the field columns
    private const int IndexColumn = 0;
    private const int NameColumn = 1;
    private const int IssuesColumn = 2;
    private const int UsedByColumn = 3;
    private const int FirstFieldColumn = 4;

    private const int MaxTooltipNames = 10;

    // The map's issues by entity, and what refers to each cached entry (-1 when it can't be referred to).
    // Worked out again with the cells, whenever the entries or the map's history change.
    private Dictionary<Entity, List<MapCheckIssue>> _issues = new();
    private int[] _usedBy = Array.Empty<int>();

    // Quick filters of the header, for this session
    private bool _onlyChanged = false;
    private bool _onlyIssues = false;

    // Follow selection: the entity last followed, and the one to scroll to once its row is drawn
    private Entity _lastFollowed;
    private Entity _scrollTarget;

    /// <summary>
    /// Runs the map check and counts the references, for the cached entries
    /// </summary>
    private void RebuildChecks()
    {
        _issues = new Dictionary<Entity, List<MapCheckIssue>>();

        var map = Selection.SelectedMap;

        if (map != null)
        {
            foreach (var issue in MapCheck.Run(Project, new[] { map }))
            {
                if (issue.Entity == null)
                    continue;

                if (!_issues.TryGetValue(issue.Entity, out var list))
                {
                    list = new List<MapCheckIssue>();
                    _issues[issue.Entity] = list;
                }

                list.Add(issue);
            }

            AddFirstDuplicates(map);
        }

        RebuildOverview(map);

        // Models aren't map objects, so their uses are the parts that place them
        var partsByModel = new Dictionary<string, int>(StringComparer.Ordinal);

        if (map != null)
        {
            foreach (var obj in map.Objects)
            {
                if (obj.WrappedObject is IMsbPart part && !string.IsNullOrEmpty(part.ModelName))
                {
                    partsByModel.TryGetValue(part.ModelName, out var n);
                    partsByModel[part.ModelName] = n + 1;
                }
            }
        }

        _usedBy = new int[_cachedEntries.Count];

        for (int i = 0; i < _cachedEntries.Count; i++)
        {
            var (entry, ent, _, _) = _cachedEntries[i];

            if (ent == null)
            {
                _usedBy[i] = -1;
            }
            else if (entry is IMsbModel model)
            {
                _usedBy[i] = model.Name != null && partsByModel.TryGetValue(model.Name, out var n) ? n : 0;
            }
            else
            {
                _usedBy[i] = ent.GetReferencingObjects().Count;
            }
        }
    }

    /// <summary>
    /// The map check flags each later object sharing an entity ID, not the first one to have it. In the table
    /// both ends should show, so the first is given the same warning, naming one that shares its ID.
    /// </summary>
    private void AddFirstDuplicates(MapContainer map)
    {
        var sharedBy = new Dictionary<string, Entity>(StringComparer.Ordinal);

        foreach (var list in _issues.Values)
        {
            foreach (var issue in list)
            {
                if (issue.Kind is MapCheckKind.DuplicateEntityID)
                    sharedBy.TryAdd(issue.Value, issue.Entity);
            }
        }

        if (sharedBy.Count == 0)
            return;

        foreach (var ent in map.Objects)
        {
            var id = ent.WrappedObject?.GetType().GetProperty("EntityID")?.GetValue(ent.WrappedObject)?.ToString();

            if (id == null || !sharedBy.TryGetValue(id, out var other) || other == ent)
                continue;

            if (!_issues.TryGetValue(ent, out var list))
            {
                list = new List<MapCheckIssue>();
                _issues[ent] = list;
            }

            if (list.Any(e => e.Kind is MapCheckKind.DuplicateEntityID))
                continue;

            list.Add(new MapCheckIssue(map, ent, MapCheckKind.DuplicateEntityID, MapCheckSeverity.Warning, id, other.PrettyName));
        }
    }

    private List<MapCheckIssue> GetIssues(Entity ent)
    {
        return ent != null && _issues.TryGetValue(ent, out var list) ? list : null;
    }

    /// <summary>
    /// 2 for an error, 1 for a warning, 0 for none: the issue column's sort
    /// </summary>
    private int GetIssueRank(Entity ent)
    {
        var issues = GetIssues(ent);

        if (issues == null)
            return 0;

        return issues.Any(e => e.Severity is MapCheckSeverity.Error) ? 2 : 1;
    }

    private void DisplayIssuesCell(Entity ent)
    {
        var issues = GetIssues(ent);

        if (issues == null)
            return;

        var hasError = issues.Any(e => e.Severity is MapCheckSeverity.Error);
        var color = hasError ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color;

        ImGui.TextColored(color, issues.Count > 1 ? $"{Icons.ExclamationTriangle} {issues.Count}" : Icons.ExclamationTriangle);

        if (ImGui.IsItemHovered())
        {
            var lines = issues.Select(e => $"{(e.Severity is MapCheckSeverity.Error ? "Error" : "Warning")}: {e.Describe()}");

            ImGui.SetTooltip(string.Join("\n", lines));
        }
    }

    private void DisplayUsedByCell(int row)
    {
        if (row >= _usedBy.Length || _usedBy[row] < 0)
            return;

        var count = _usedBy[row];

        if (count == 0)
        {
            ImGui.TextDisabled("0");
            GUI.Tooltip("Nothing in this map refers to this entry.");
            return;
        }

        ImGui.TextUnformatted(count.ToString());

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(GetUsedByTooltip(row, count));
        }
    }

    private string GetUsedByTooltip(int row, int count)
    {
        var (entry, ent, _, _) = _cachedEntries[row];
        var map = Selection.SelectedMap;

        IEnumerable<string> names;

        if (entry is IMsbModel model)
        {
            names = map?.Objects
                .Where(e => e.WrappedObject is IMsbPart part && part.ModelName == model.Name)
                .Select(e => e.Name) ?? Enumerable.Empty<string>();
        }
        else
        {
            names = ent.GetReferencingObjects().Select(e => e.Name);
        }

        var shown = names.Take(MaxTooltipNames).ToList();
        var text = $"Referred to by:\n{string.Join("\n", shown)}";

        if (count > shown.Count)
        {
            text += $"\n... and {count - shown.Count} more";
        }

        return text;
    }

    /// <summary>
    /// Whether the entry passes the header's quick filters
    /// </summary>
    private bool PassesQuickFilters(int row)
    {
        var ent = _cachedEntries[row].ent;

        if (_onlyIssues && GetIssues(ent) == null)
            return false;

        if (_onlyChanged)
        {
            var status = ent != null ? View.Editor.VanillaCompare.GetStatus(ent) : VanillaStatus.None;

            if (status is not (VanillaStatus.Modified or VanillaStatus.Added))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The quick filter and follow selection toggles of the header
    /// </summary>
    private void DisplayQuickFilterButtons()
    {
        DisplayToggle("entryOnlyIssues", Icons.ExclamationTriangle, ref _onlyIssues,
            "Issues only: show only the entries the Save Check finds a problem with.");

        DisplayToggle("entryOnlyChanged", Icons.Pencil, ref _onlyChanged,
            "Changed only: show only the entries added or modified since vanilla.");

        DisplayToggle("entryFollowSelection", Icons.Crosshairs, ref CFG.Current.MapEditor.MapDataEditor_EntryView_Follow_Selection,
            "Follow selection: an entity selected in the viewport or Map Content is shown here, switching category if needed.");

        DisplayNewButton();
    }

    /// <summary>
    /// Creates an entry of the selected category at the placement orb, as the Create tool does
    /// </summary>
    private void DisplayNewButton()
    {
        var map = Selection.SelectedMap;
        var type = Selection.SelectedSubCategoryType;
        var canCreate = Selection.IsEntityCategory && View.CreateAction.CanCreate(type, map);

        ImGui.SameLine();

        ImGui.BeginDisabled(!canCreate);

        if (ImGui.Button($"{Icons.Plus}##entryNew", DPI.IconButtonSize))
        {
            View.CreateAction.CreateInMap(type, map);
        }

        ImGui.EndDisabled();

        // Shown while disabled too, as that is when it says why
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            var text = type == null
                ? "New: select a category to create an entry of its type."
                : canCreate
                    ? $"New {Selection.SelectedSubCategory}: creates one in {map.Name} at the placement orb, and selects it. Can be undone."
                    : $"New: entries of {Selection.SelectedSubCategory} can't be created here (only the part, region and event types the game saves, and lights in a map with a BTL).";

            ImGui.SetTooltip(text);
        }
    }

    private static void DisplayToggle(string id, string icon, ref bool value, string tooltip)
    {
        ImGui.SameLine();

        var on = value;

        if (on)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);
        }

        if (ImGui.Button($"{icon}##{id}", DPI.IconButtonSize))
        {
            value = !value;
        }

        if (on)
        {
            ImGui.PopStyleColor();
        }

        GUI.Tooltip($"{tooltip}\n\n{(value ? "On" : "Off")}");
    }

    /// <summary>
    /// Shows the entity selected elsewhere: switches to its category, and scrolls to its row
    /// </summary>
    private void UpdateFollowSelection()
    {
        if (!CFG.Current.MapEditor.MapDataEditor_EntryView_Follow_Selection)
            return;

        // Only a single selection, so building up a multi-selection doesn't jump between categories
        var selected = View.ViewportSelection.IsSingleSelection()
            ? View.ViewportSelection.GetSingleSelection() as MsbEntity
            : null;

        if (selected == null || selected == _lastFollowed)
            return;

        _lastFollowed = selected;

        if (selected.Container != Selection.SelectedMap || selected.WrappedObject == null)
            return;

        if (View.MapDataView.CategoryView.SelectCategoryFor(selected.WrappedObject))
        {
            _scrollTarget = selected;
        }
    }

    /// <summary>
    /// Call right after an entry's selectable: scrolls it into view if it is the one to follow
    /// </summary>
    private void ScrollToIfTarget(Entity ent)
    {
        if (_scrollTarget == null || ent != _scrollTarget)
            return;

        if (!GUI.IsLastItemFullyInView())
        {
            ImGui.SetScrollHereY(0.5f);
        }

        _scrollTarget = null;
    }
}
