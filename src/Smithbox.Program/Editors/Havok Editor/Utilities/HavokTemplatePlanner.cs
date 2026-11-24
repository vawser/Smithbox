using StudioCore.Editors.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Plans a Category template (Claude/Documentation/Behavior.md (Havok behavior edits)): for each animation aNEW_ID, the
/// selectors (CustomManualSelectorGenerator) that get its clip, and the clip each one copies.
///
/// IDs come from the ticked groups (targets: the selectors with the ID, of the template's offset types, with clips), or
/// from a reference category (targets: the selectors of those offset types holding the reference's clip of the ID).
/// With a reference, a new clip copies the selector's reference clip (its mode and bindings: 69 of ER's 297 a025 clips
/// have bindings); otherwise the selector's first clip, as Add Animation does.
/// </summary>
public static class HavokTemplatePlanner
{
    public enum RowStatus
    {
        /// <summary>At least one selector gets the clip</summary>
        Add,
        /// <summary>Every target already has the clip</summary>
        Registered,
        /// <summary>No selector of the template's offset types plays the ID</summary>
        NoSelector
    }

    public class Target
    {
        public object Selector;
        public string OffsetType;
        public bool HasClip;

        /// <summary>The clip the new one copies (the selector's reference clip), or null for the selector's first</summary>
        public object Template;
    }

    public class Row
    {
        public long AnimationId;
        public string AnimationName;
        public string Group;
        public RowStatus Status;
        public List<Target> Targets = new();
        public bool Include;
    }

    public class Inputs
    {
        public int Category;
        public int Reference = -1;

        /// <summary>The IDs are the reference category's (else the ticked groups')</summary>
        public bool FromReference;

        public HashSet<int> Groups = new();
    }

    private static readonly Regex ClipAnimationRegex = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    public static string AnimationName(long category, long id, int categorySize) =>
        categorySize == 10000 ? $"a{category:D2}_{id:D4}" : $"a{category:D3}_{id:D6}";

    public static List<Row> Plan(HavokBehaviorSymbols symbols, HavokBehaviorTemplate template, Inputs inputs, int categorySize)
    {
        var rows = new List<Row>();

        if (symbols == null || template == null || inputs.Category < 0)
            return rows;

        bool AllowedType(string offsetType) => template.OffsetTypes.Count == 0 ||
            template.OffsetTypes.Any(t => string.Equals(t, offsetType, StringComparison.OrdinalIgnoreCase));

        // Selectors with clips of the allowed types, by ID, with their clips by category
        var byId = new Dictionary<long, List<(object Selector, string OffsetType, Dictionary<long, object> Clips)>>();

        foreach (var selector in symbols.Selectors)
        {
            var offsetType = HavokBehaviorSymbols.GetValue(selector, "m_offsetType")?.ToString() ?? "";

            if (!AllowedType(offsetType))
                continue;

            var clips = GetClipsByCategory(selector, out var clipCount);

            if (clipCount == 0)
                continue;

            var id = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1);

            if (!byId.TryGetValue(id, out var list))
                byId[id] = list = new();

            list.Add((selector, offsetType, clips));
        }

        // The IDs, with the group each first came from
        var ids = new List<(long Id, string Group)>();
        var seen = new HashSet<long>();

        if (inputs.FromReference)
        {
            if (inputs.Reference < 0)
                return rows;

            var group = categorySize == 10000 ? $"a{inputs.Reference:D2}" : $"a{inputs.Reference:D3}";

            foreach (var id in byId.Where(p => p.Value.Any(s => s.Clips.ContainsKey(inputs.Reference))).Select(p => p.Key).OrderBy(i => i))
                ids.Add((id, group));
        }
        else
        {
            for (var i = 0; i < template.Groups.Count; i++)
            {
                if (!inputs.Groups.Contains(i))
                    continue;

                foreach (var id in template.Groups[i].Ids)
                {
                    if (seen.Add(id))
                        ids.Add((id, template.Groups[i].Name));
                }
            }
        }

        foreach (var (id, group) in ids)
        {
            var row = new Row
            {
                AnimationId = id,
                AnimationName = AnimationName(inputs.Category, id, categorySize),
                Group = group,
            };

            if (byId.TryGetValue(id, out var selectors))
            {
                foreach (var (selector, offsetType, clips) in selectors)
                {
                    clips.TryGetValue(inputs.Reference, out var reference);

                    // From a reference: exactly the selectors that have it
                    if (inputs.FromReference && reference == null)
                        continue;

                    row.Targets.Add(new Target
                    {
                        Selector = selector,
                        OffsetType = offsetType,
                        HasClip = clips.ContainsKey(inputs.Category),
                        Template = inputs.Reference >= 0 ? reference : null,
                    });
                }
            }

            row.Status = row.Targets.Count == 0 ? RowStatus.NoSelector
                : row.Targets.Any(t => !t.HasClip) ? RowStatus.Add
                : RowStatus.Registered;

            row.Include = row.Status is RowStatus.Add;

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>A selector's clips by category (the first of each), and how many clips it has</summary>
    private static Dictionary<long, object> GetClipsByCategory(object selector, out int clipCount)
    {
        var result = new Dictionary<long, object>();
        clipCount = 0;

        if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is not IList generators)
            return result;

        foreach (var generator in generators)
        {
            if (generator?.GetType().Name != "hkbClipGenerator")
                continue;

            clipCount++;

            var match = ClipAnimationRegex.Match(HavokBehaviorSymbols.GetValue(generator, "m_animationName") as string ?? "");

            if (match.Success)
                result.TryAdd(long.Parse(match.Groups[1].Value), generator);
        }

        return result;
    }

    public static int CountAdds(IEnumerable<Row> rows) =>
        rows.Where(r => r.Include).Sum(r => r.Targets.Count(t => !t.HasClip));

    /// <summary>
    /// The edit: each included row's clip added to the targets that lack it, as one undo step. Clips that can't be
    /// added are skipped, and their reasons returned.
    /// </summary>
    public static EditorAction Apply(HavokBehaviorContext context, IEnumerable<Row> rows, int category, out int added, out List<string> errors)
    {
        var builders = new List<Func<(EditorAction, string)>>();

        foreach (var row in rows.Where(r => r.Include))
        {
            foreach (var target in row.Targets.Where(t => !t.HasClip))
            {
                var selector = target.Selector;
                var clipTemplate = target.Template;

                builders.Add(() =>
                {
                    var action = HavokBehaviorOperations.AddAnimation(context, selector, category, out _, out var error, clipTemplate);
                    return (action, error);
                });
            }
        }

        var result = HavokBehaviorOperations.Sequence(context, builders, out errors);

        added = builders.Count - errors.Count;

        return result;
    }
}
