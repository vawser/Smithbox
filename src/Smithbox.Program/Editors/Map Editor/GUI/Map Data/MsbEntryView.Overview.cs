using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The map overview, shown while no category is selected: what the map holds, its issues, the entity IDs it
/// uses and the gaps between them, and how its models are used. Worked out with the checks, so it follows edits.
/// </summary>
public partial class MsbEntryView
{
    private const int MaxOverviewIssues = 15;
    private const int MaxOverviewGaps = 5;
    private const int MaxOverviewModels = 10;

    private sealed class MapOverview
    {
        public int Parts;
        public int Regions;
        public int Events;
        public int Models;

        public int Errors;
        public int Warnings;
        public List<MapCheckIssue> Issues = new();

        public int UniqueIds;
        public long MinId;
        public long MaxId;

        // The widest runs of unused IDs between the lowest and highest used
        public List<(long From, long To)> Gaps = new();

        public List<(Entity Model, int Uses)> UnusedModels = new();
        public List<(Entity Model, int Uses)> MostUsedModels = new();
    }

    private MapOverview _overview;

    private void RebuildOverview(MapContainer map)
    {
        if (map == null)
        {
            _overview = null;
            return;
        }

        var overview = new MapOverview
        {
            Parts = map.Objects.Count(e => e.WrappedObject is IMsbPart),
            Regions = map.Objects.Count(e => e.WrappedObject is IMsbRegion),
            Events = map.Objects.Count(e => e.WrappedObject is IMsbEvent),
            Models = map.Models.Count(e => e.WrappedObject is IMsbModel)
        };

        // Issues: errors first, as the check orders them
        var issues = _issues.Values.SelectMany(e => e).OrderBy(e => e.Severity).ToList();

        overview.Errors = issues.Count(e => e.Severity is MapCheckSeverity.Error);
        overview.Warnings = issues.Count - overview.Errors;
        overview.Issues = issues;

        // Entity IDs, read by field as their type differs between games
        var ids = new SortedSet<long>();

        foreach (var ent in map.Objects)
        {
            if (ent.WrappedObject is not (IMsbPart or IMsbRegion or IMsbEvent))
                continue;

            long id = ent.WrappedObject.GetType().GetProperty("EntityID")?.GetValue(ent.WrappedObject) switch
            {
                int i => i,
                uint u => u,
                _ => 0
            };

            if (id > 0)
                ids.Add(id);
        }

        overview.UniqueIds = ids.Count;

        if (ids.Count > 0)
        {
            overview.MinId = ids.Min;
            overview.MaxId = ids.Max;

            var gaps = new List<(long From, long To)>();
            long? previous = null;

            foreach (var id in ids)
            {
                if (previous is long p && id - p > 1)
                    gaps.Add((p + 1, id - 1));

                previous = id;
            }

            overview.Gaps = gaps.OrderByDescending(g => g.To - g.From).Take(MaxOverviewGaps).ToList();
        }

        // Models, by the parts that place them
        var uses = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var ent in map.Objects)
        {
            if (ent.WrappedObject is IMsbPart part && !string.IsNullOrEmpty(part.ModelName))
            {
                uses.TryGetValue(part.ModelName, out var n);
                uses[part.ModelName] = n + 1;
            }
        }

        var models = map.Models
            .Where(e => e.WrappedObject is IMsbModel)
            .Select(e => (Model: e, Uses: uses.TryGetValue(((IMsbModel)e.WrappedObject).Name ?? "", out var n) ? n : 0))
            .ToList();

        overview.UnusedModels = models.Where(e => e.Uses == 0).ToList();
        overview.MostUsedModels = models.Where(e => e.Uses > 0).OrderByDescending(e => e.Uses).Take(MaxOverviewModels).ToList();

        _overview = overview;
    }

    private void DisplayOverview()
    {
        var map = Selection.SelectedMap;
        var overview = _overview;

        if (map == null || overview == null)
        {
            ImGui.TextDisabled("No category selected.");
            return;
        }

        ImGui.TextDisabled("No category selected. An overview of the map:");
        ImGui.Spacing();

        // Contents
        ImGui.SeparatorText("Contents");
        ImGui.TextUnformatted($"{overview.Parts:N0} parts, {overview.Regions:N0} regions, {overview.Events:N0} events, {overview.Models:N0} models");

        DisplayOverviewChanges(map);

        // Issues
        ImGui.SeparatorText("Issues");

        if (overview.Issues.Count == 0)
        {
            ImGui.TextUnformatted("None found by the Save Check.");
        }
        else
        {
            var color = overview.Errors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color;
            ImGui.TextColored(color, $"{overview.Errors} errors, {overview.Warnings} warnings");

            for (int i = 0; i < Math.Min(MaxOverviewIssues, overview.Issues.Count); i++)
            {
                var issue = overview.Issues[i];
                var severity = issue.Severity is MapCheckSeverity.Error ? "Error" : "Warning";

                DisplayOverviewEntity($"{issue.Entity.Name}: {severity}: {issue.Describe()}##overviewIssue_{i}", issue.Entity);
            }

            if (overview.Issues.Count > MaxOverviewIssues)
            {
                ImGui.TextDisabled($"... and {overview.Issues.Count - MaxOverviewIssues} more. Turn on Issues only in a category to see them all.");
            }
        }

        // Entity IDs
        ImGui.SeparatorText("Entity IDs");

        if (overview.UniqueIds == 0)
        {
            ImGui.TextUnformatted("No entity IDs are used.");
        }
        else
        {
            ImGui.TextUnformatted($"{overview.UniqueIds:N0} used, from {overview.MinId} to {overview.MaxId}");
            ImGui.TextUnformatted($"Next after the highest: {overview.MaxId + 1}");

            if (overview.Gaps.Count > 0)
            {
                ImGui.TextDisabled("Widest unused runs between them:");

                foreach (var (from, to) in overview.Gaps)
                {
                    var text = from == to ? $"{from} (1 free)" : $"{from} to {to} ({to - from + 1:N0} free)";

                    ImGui.BulletText(text);

                    if (ImGui.IsItemClicked())
                    {
                        ImGui.SetClipboardText(from.ToString());
                    }
                    GUI.Tooltip("Click to copy the first free ID.");
                }
            }
        }

        // Models
        ImGui.SeparatorText("Models");

        if (overview.UnusedModels.Count == 0)
        {
            ImGui.TextUnformatted("Every model is placed by a part.");
        }
        else
        {
            ImGui.TextUnformatted($"{overview.UnusedModels.Count} models are placed by no part:");

            for (int i = 0; i < Math.Min(MaxOverviewModels, overview.UnusedModels.Count); i++)
            {
                var model = overview.UnusedModels[i].Model;

                DisplayOverviewEntity($"{model.Name}##overviewUnused_{i}", model);
            }

            if (overview.UnusedModels.Count > MaxOverviewModels)
            {
                ImGui.TextDisabled($"... and {overview.UnusedModels.Count - MaxOverviewModels} more. Sort Model by Used By to see them all.");
            }
        }

        if (overview.MostUsedModels.Count > 0)
        {
            ImGui.TextDisabled("Most placed:");

            for (int i = 0; i < overview.MostUsedModels.Count; i++)
            {
                var (model, count) = overview.MostUsedModels[i];

                DisplayOverviewEntity($"{model.Name} ({count})##overviewUsed_{i}", model);
            }
        }
    }

    /// <summary>
    /// The map's vanilla changes. Read each time, as the vanilla compare can finish after the overview is made.
    /// </summary>
    private void DisplayOverviewChanges(MapContainer map)
    {
        if (!View.Editor.VanillaCompare.HasVanilla(map))
            return;

        int added = 0, modified = 0;

        foreach (var ent in map.Objects)
        {
            switch (View.Editor.VanillaCompare.GetStatus(ent))
            {
                case VanillaStatus.Added:
                    added++;
                    break;
                case VanillaStatus.Modified:
                    modified++;
                    break;
            }
        }

        ImGui.TextUnformatted($"Since vanilla: {added:N0} added, {modified:N0} modified");
    }

    /// <summary>
    /// A line naming an entity: a click selects it, so Follow Selection shows it in its category
    /// </summary>
    private void DisplayOverviewEntity(string label, Entity ent)
    {
        if (ImGui.Selectable(label))
        {
            View.ViewportSelection.ClearSelection();
            View.ViewportSelection.AddSelection(ent);
        }

        GUI.Tooltip(CFG.Current.MapEditor.MapDataEditor_EntryView_Follow_Selection
            ? "Click to select it and show it in its category."
            : "Click to select it.");
    }
}
