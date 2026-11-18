using StudioCore.Editors.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Plans Register in Behavior: which selectors (CustomManualSelectorGenerator) need a clip so the game can play
/// time act animations. An animation aCCC_NNNNNN is played by a selector with m_animId N whose offset type gives
/// category C, through its child clip aCCC_NNNNNN.
///
/// Rules (Claude/Documentation/TAE Behavior Auto Registration Analysis.md, checked against ER, NR, DS3 and SDT):
///   - every selector of the file with m_animId N gets the clip: selectors sharing an ID hold the same categories
///   - only if its offset type admits C: another selector of the file with the same offset type has a clip of C
///     (98-99.8% precise, at least 99.5% recall in vanilla)
///   - selectors without clips are skipped: they're placeholders whose children are other generators
///   - the new clip copies one of the selector's own clips, which differ only in name and animation in vanilla
/// Animations no selector has (a new state would be needed) aren't planned here.
/// </summary>
public static class HavokRegistrationPlanner
{
    public enum RowStatus
    {
        /// <summary>Every selector that admits the category already has the clip</summary>
        Registered,
        /// <summary>At least one selector admits the category and lacks the clip</summary>
        AddClip,
        /// <summary>Selectors have the ID, but no offset type of theirs uses the category yet</summary>
        NotAdmitted,
        /// <summary>No selector with clips has the ID</summary>
        NoSelector
    }

    public class Target
    {
        public object Selector;
        public string OffsetType;

        /// <summary>Whether the selector's offset type already uses the row's category in this file</summary>
        public bool Admitted;

        /// <summary>Whether the selector already has the row's clip</summary>
        public bool HasClip;
    }

    public class Row
    {
        public long FullId;
        public long Category;
        public long AnimationId;
        public string AnimationName;
        public RowStatus Status;
        public List<Target> Targets = new();

        /// <summary>Included in the edit (defaults: AddClip on, the others off)</summary>
        public bool Include;

        /// <summary>The selectors the edit adds the clip to: those lacking it, admitted ones unless forced</summary>
        public IEnumerable<Target> Adds(bool includeNotAdmitted) =>
            Targets.Where(t => !t.HasClip && (t.Admitted || includeNotAdmitted));
    }

    private static readonly Regex ClipAnimationRegex = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    /// <summary>
    /// One row per animation (category, ID), in the order given, duplicates removed
    /// </summary>
    public static List<Row> Plan(HavokBehaviorSymbols symbols, IEnumerable<long> fullIds, int categorySize)
    {
        var rows = new List<Row>();

        if (symbols == null)
            return rows;

        // Selectors with clips, by ID, and the categories each offset type uses in this file
        var byId = new Dictionary<long, List<(object Selector, string OffsetType, HashSet<long> Categories)>>();
        var admitted = new Dictionary<string, HashSet<long>>();

        foreach (var selector in symbols.Selectors)
        {
            var categories = GetClipCategories(selector, out var clipCount);

            if (clipCount == 0)
                continue;

            var id = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1);
            var offsetType = HavokBehaviorSymbols.GetValue(selector, "m_offsetType")?.ToString() ?? "";

            if (!byId.TryGetValue(id, out var list))
                byId[id] = list = new();

            list.Add((selector, offsetType, categories));

            if (!admitted.TryGetValue(offsetType, out var set))
                admitted[offsetType] = set = new HashSet<long>();

            set.UnionWith(categories);
        }

        var seen = new HashSet<long>();

        foreach (var fullId in fullIds)
        {
            if (fullId < 0 || !seen.Add(fullId))
                continue;

            var row = new Row
            {
                FullId = fullId,
                Category = fullId / categorySize,
                AnimationId = fullId % categorySize,
            };

            row.AnimationName = categorySize == 10000
                ? $"a{row.Category:D2}_{row.AnimationId:D4}"
                : $"a{row.Category:D3}_{row.AnimationId:D6}";

            if (byId.TryGetValue(row.AnimationId, out var selectors))
            {
                foreach (var (selector, offsetType, categories) in selectors)
                {
                    row.Targets.Add(new Target
                    {
                        Selector = selector,
                        OffsetType = offsetType,
                        Admitted = admitted.TryGetValue(offsetType, out var set) && set.Contains(row.Category),
                        HasClip = categories.Contains(row.Category),
                    });
                }
            }

            if (row.Targets.Count == 0)
                row.Status = RowStatus.NoSelector;
            else if (row.Targets.Any(t => t.Admitted && !t.HasClip))
                row.Status = RowStatus.AddClip;
            else if (row.Targets.Any(t => t.HasClip))
                row.Status = RowStatus.Registered;
            else
                row.Status = RowStatus.NotAdmitted;

            row.Include = row.Status is RowStatus.AddClip;

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// The categories of a selector's clips (aCCC_...), and how many clips it has
    /// </summary>
    private static HashSet<long> GetClipCategories(object selector, out int clipCount)
    {
        var categories = new HashSet<long>();
        clipCount = 0;

        if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is not IList generators)
            return categories;

        foreach (var generator in generators)
        {
            if (generator?.GetType().Name != "hkbClipGenerator")
                continue;

            clipCount++;

            var match = ClipAnimationRegex.Match(HavokBehaviorSymbols.GetValue(generator, "m_animationName") as string ?? "");

            if (match.Success)
                categories.Add(long.Parse(match.Groups[1].Value));
        }

        return categories;
    }

    /// <summary>
    /// How many clips the edit would add
    /// </summary>
    public static int CountAdds(IEnumerable<Row> rows, bool includeNotAdmitted) =>
        rows.Where(r => r.Include).Sum(r => r.Adds(includeNotAdmitted || r.Status is RowStatus.NotAdmitted).Count());

    /// <summary>
    /// The edit: each included row's clip added to its selectors, as one undo step. A selector's own clip is the
    /// template (AddAnimation's default). Rows that can't be added are skipped, and their reasons returned.
    /// </summary>
    public static EditorAction Apply(HavokBehaviorContext context, IEnumerable<Row> rows, bool includeNotAdmitted,
        out int added, out List<string> errors)
    {
        var builders = new List<Func<(EditorAction, string)>>();

        foreach (var row in rows.Where(r => r.Include))
        {
            // A row whose category no selector uses yet is only added if it was ticked, so its selectors are all forced
            foreach (var target in row.Adds(includeNotAdmitted || row.Status is RowStatus.NotAdmitted))
            {
                var category = row.Category;
                var selector = target.Selector;

                builders.Add(() =>
                {
                    var action = HavokBehaviorOperations.AddAnimation(context, selector, category, out _, out var error);
                    return (action, error);
                });
            }
        }

        var result = HavokBehaviorOperations.Sequence(context, builders, out errors);

        added = builders.Count - errors.Count;

        return result;
    }
}
