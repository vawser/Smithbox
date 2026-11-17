using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// What Learn found in a param
/// </summary>
public class WeaponGeneratorLearnResult
{
    public int IdBlock;
    public List<WeaponGeneratorInfusion> Infusions = new();

    /// <summary>
    /// Base weapons that had at least one infusion
    /// </summary>
    public int BaseCount;

    /// <summary>
    /// Offsets seen on too few weapons to be an infusion (e.g. ER's pot variants)
    /// </summary>
    public List<int> IgnoredOffsets = new();
}

/// <summary>
/// What Learn looks at beyond the plain Set and Add rules
/// </summary>
public class WeaponGeneratorStatOptions
{
    /// <summary>
    /// Also learn factor rules (Multiply, Scale From) for the stat fields
    /// </summary>
    public bool LearnStats = false;

    /// <summary>
    /// Comma-separated field names; * is a wildcard and a leading ! leaves the matches out
    /// </summary>
    public string StatFields = "";

    /// <summary>
    /// Comma-separated fields any rule may take a value per group of (e.g. the weapon type)
    /// </summary>
    public string GroupFields = "";
}

/// <summary>
/// The Weapon Generator's learn and rule logic. Only uses the param data (no editor state), so it can be checked
/// against the game data outside the editor.
/// </summary>
public static class WeaponGeneratorCore
{
    public const string NameToken = "{name}";
    public const string InfusionToken = "{infusion}";

    /// <summary>
    /// The ID blocks Learn chooses between: 10000 per weapon in ER and DS3, 1000 in DS1
    /// </summary>
    private static readonly int[] CandidateBlocks = { 1000, 10000 };

    /// <summary>
    /// Upgrade rows are at base +1 … +99 (DS1 has +1 … +15)
    /// </summary>
    public const int MaxUpgradeRows = 99;

    // Learn thresholds
    private const double SameShare = 0.9;
    private const double RuleShare = 0.6;
    private const double OffsetShare = 0.25;
    private const double NameShare = 0.3;

    // The field whose delta equals the infusion offset in the games' data; used only to detect the ID block
    private const string ReinforceField = "reinforceTypeId";

    #region Rows

    /// <summary>
    /// The base row and, if <paramref name="includeUpgradeRows"/>, the rows at base +1 … +99 that exist, in ID order
    /// </summary>
    public static List<Param.Row> GetSourceRows(Dictionary<int, Param.Row> byId, Param.Row baseRow, bool includeUpgradeRows)
    {
        var result = new List<Param.Row> { baseRow };

        if (!includeUpgradeRows)
            return result;

        for (int j = 1; j <= MaxUpgradeRows; j++)
        {
            if (byId.TryGetValue(baseRow.ID + j, out var row))
                result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// The first row with each ID
    /// </summary>
    public static Dictionary<int, Param.Row> IndexById(Param param)
    {
        var byId = new Dictionary<int, Param.Row>();

        foreach (var row in param.Rows)
        {
            byId.TryAdd(row.ID, row);
        }

        return byId;
    }

    /// <summary>
    /// Fills a name template. An empty source name gives an empty result, so unnamed rows stay unnamed.
    /// </summary>
    public static string FormatName(string template, string sourceName, string infusionName)
    {
        if (string.IsNullOrEmpty(sourceName))
            return "";

        if (string.IsNullOrEmpty(template))
            return sourceName;

        return template.Replace(InfusionToken, infusionName ?? "").Replace(NameToken, sourceName);
    }

    #endregion

    #region Rules

    /// <summary>
    /// Applies the infusion's enabled rules to the row. Returns the rules that could not be applied
    /// (unknown field, unparsable value, or a result out of the field's range), as "field: reason".
    /// </summary>
    /// <remarks>
    /// Every rule reads the values of <paramref name="source"/> (the unchanged base or upgrade row), so the order of
    /// the rules doesn't matter: a Fire damage rule scaled from physical damage still sees the base's physical
    /// damage after another rule has lowered it.
    /// </remarks>
    public static List<string> ApplyRules(Param.Row row, Param.Row source, WeaponGeneratorInfusion infusion)
    {
        var errors = new List<string>();

        foreach (var rule in infusion.Rules)
        {
            if (!rule.Enabled || string.IsNullOrWhiteSpace(rule.Field))
                continue;

            var error = ApplyRule(row, source, rule);

            if (error != null)
                errors.Add($"{rule.Field}: {error}");
        }

        return errors;
    }

    private static Param.Column FindColumn(Param.Row row, string field)
    {
        if (row.Def == null || string.IsNullOrWhiteSpace(field))
            return null;

        var name = field.Trim();

        return row.Columns.FirstOrDefault(c => c.Def.InternalName == name);
    }

    /// <summary>
    /// The source fields of a Scale From rule
    /// </summary>
    public static List<string> SplitFields(string fields)
    {
        return (fields ?? "")
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>
    /// The rule's value for the source row: its group's value when the rule has a group field and the source's
    /// group is listed, otherwise the rule's own value
    /// </summary>
    public static string GetRuleValue(Param.Row source, WeaponGeneratorRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.GroupField) || rule.Groups == null || rule.Groups.Count == 0)
            return rule.Value;

        var groupColumn = FindColumn(source, rule.GroupField);

        if (groupColumn == null)
            return rule.Value;

        var groupValue = groupColumn.GetValue(source);

        if (groupValue is byte[])
            return rule.Value;

        var key = groupValue is string text ? text : FormatValue(Convert.ToDouble(groupValue, CultureInfo.InvariantCulture));
        var group = rule.Groups.FirstOrDefault(g => g.Key?.Trim() == key);

        return group != null ? group.Value : rule.Value;
    }

    /// <summary>
    /// Applies one rule, reading from <paramref name="source"/> and writing to <paramref name="row"/>.
    /// Returns null on success, otherwise the reason it failed.
    /// </summary>
    public static string ApplyRule(Param.Row row, Param.Row source, WeaponGeneratorRule rule)
    {
        var column = FindColumn(row, rule.Field);

        if (column == null)
            return "unknown field";

        var current = column.GetValue(source);
        var ruleValue = GetRuleValue(source, rule);

        if (current is string)
        {
            if (rule.Op != WeaponGeneratorRuleOp.Set)
                return "text fields only take Set";

            column.SetValue(row, ruleValue ?? "");
            return null;
        }

        if (current is byte[])
            return "unsupported field type";

        if (!double.TryParse(ruleValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return "value is not a number";

        double result;
        var baseValue = Convert.ToDouble(current, CultureInfo.InvariantCulture);

        // -1 means "no reference": offsetting it would point at an unrelated row
        if ((rule.Op == WeaponGeneratorRuleOp.Add || rule.Op == WeaponGeneratorRuleOp.Multiply) && baseValue == -1)
            return null;

        switch (rule.Op)
        {
            case WeaponGeneratorRuleOp.Add:
                result = baseValue + value;
                break;
            case WeaponGeneratorRuleOp.Multiply:
                result = baseValue * value;
                break;
            case WeaponGeneratorRuleOp.ScaleFrom:
                {
                    var sum = 0.0;
                    var sources = SplitFields(rule.SourceFields);

                    if (sources.Count == 0)
                        return "no source fields";

                    foreach (var sourceField in sources)
                    {
                        var sourceColumn = FindColumn(source, sourceField);

                        if (sourceColumn == null)
                            return $"unknown source field {sourceField}";

                        var sourceValue = sourceColumn.GetValue(source);

                        if (sourceValue is string or byte[])
                            return $"source field {sourceField} isn't a number";

                        sum += Convert.ToDouble(sourceValue, CultureInfo.InvariantCulture);
                    }

                    result = sum * value;
                    break;
                }
            default:
                result = value;
                break;
        }

        var type = current.GetType();

        if (rule.Round || (type != typeof(float) && type != typeof(double)))
            result = Math.Round(result, MidpointRounding.AwayFromZero);

        try
        {
            column.SetValue(row, Convert.ChangeType(result, type, CultureInfo.InvariantCulture));
        }
        catch (OverflowException)
        {
            return "result out of range";
        }

        // Bit fields keep only their low bits, so a value that doesn't fit is changed: report it
        if (column.Def.BitSize > 0 && Convert.ToDouble(column.GetValue(row), CultureInfo.InvariantCulture) != result)
            return "result out of range";

        return null;
    }

    /// <summary>
    /// Formats a value for a rule, in the invariant culture
    /// </summary>
    public static string FormatValue(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    #endregion

    #region Learn

    /// <summary>
    /// Builds one infusion per ID offset found in the param, from pairs of rows (source, source + offset).
    /// <paramref name="titleText"/> gives a row ID's title text (null if there is no text to learn from).
    /// </summary>
    /// <param name="stats">
    /// When given, the stat fields (damage, scaling) also get rules: the best of a fixed value, the base's own
    /// value or other stat fields times a factor, optionally with a factor per group (e.g. per weapon type)
    /// </param>
    public static WeaponGeneratorLearnResult Learn(Param param, int fallbackBlock, Func<int, string> titleText,
        WeaponGeneratorStatOptions stats = null)
    {
        var result = new WeaponGeneratorLearnResult();
        var byId = IndexById(param);

        // The stat fields are always matched (so they stay out of the grouped plain rules); they get the factor
        // models only with the damage and scaling option
        var allStatFields = stats == null ? new List<(Param.Column, int)>() : MatchStatFields(param, stats.StatFields);
        var statFields = stats == null || !stats.LearnStats ? null : allStatFields;
        var groupColumns = stats == null ? new List<Param.Column>() : GetGroupColumns(param, stats.GroupFields);

        result.IdBlock = DetectIdBlock(param, byId, fallbackBlock);

        var block = result.IdBlock;

        // Pairs per offset: base-level pairs (counted to decide what is an infusion) and all pairs including
        // upgrade rows (used to learn the rules)
        var basePairs = new Dictionary<int, List<(Param.Row Source, Param.Row Target)>>();
        var allPairs = new Dictionary<int, List<(Param.Row Source, Param.Row Target)>>();
        var bases = new HashSet<int>();

        foreach (var baseRow in byId.Values.Where(r => r.ID % block == 0))
        {
            var sources = GetSourceRows(byId, baseRow, true);

            for (int offset = 100; offset < block; offset += 100)
            {
                if (!byId.TryGetValue(baseRow.ID + offset, out var target))
                    continue;

                bases.Add(baseRow.ID);
                Add(basePairs, offset, (baseRow, target));

                foreach (var source in sources)
                {
                    if (byId.TryGetValue(source.ID + offset, out var upgradeTarget))
                        Add(allPairs, offset, (source, upgradeTarget));
                }
            }
        }

        result.BaseCount = bases.Count;

        if (basePairs.Count == 0)
            return result;

        var maxCount = basePairs.Values.Max(l => l.Count);

        foreach (var offset in basePairs.Keys.OrderBy(k => k))
        {
            if (basePairs[offset].Count < maxCount * OffsetShare)
            {
                result.IgnoredOffsets.Add(offset);
                continue;
            }

            result.Infusions.Add(LearnInfusion(param, offset, basePairs[offset], allPairs[offset], titleText, statFields, allStatFields, groupColumns));
        }

        return result;
    }

    private static void Add(Dictionary<int, List<(Param.Row, Param.Row)>> dict, int key, (Param.Row, Param.Row) pair)
    {
        if (!dict.TryGetValue(key, out var list))
            dict[key] = list = new();

        list.Add(pair);
    }

    /// <summary>
    /// The largest candidate block that has real infusions at offsets the next smaller block can't hold. An
    /// infusion pair is (base, base + offset) whose reinforce type differs by the offset. In ER, Blood (+1100)
    /// minus Poison (+1000) is also 100, so with a 1000 block the Poison row would look like a base with its own
    /// infusions; the larger block is chosen because its +1000 … +1200 offsets are infusions of the real base.
    /// Falls back to <paramref name="fallbackBlock"/> when the param has no reinforce type field.
    /// </summary>
    private static int DetectIdBlock(Param param, Dictionary<int, Param.Row> byId, int fallbackBlock)
    {
        var column = param[ReinforceField];

        if (column == null)
            return fallbackBlock > 0 ? fallbackBlock : CandidateBlocks[^1];

        var blocks = CandidateBlocks.OrderByDescending(b => b).ToList();

        for (int i = 0; i < blocks.Count - 1; i++)
        {
            var block = blocks[i];
            var smaller = blocks[i + 1];
            var matches = new Dictionary<int, int>();

            foreach (var baseRow in byId.Values.Where(r => r.ID % block == 0))
            {
                var baseValue = Convert.ToInt64(column.GetValue(baseRow));

                for (int offset = 100; offset < block; offset += 100)
                {
                    if (!byId.TryGetValue(baseRow.ID + offset, out var target))
                        continue;

                    if (Convert.ToInt64(column.GetValue(target)) - baseValue == offset)
                        matches[offset] = matches.GetValueOrDefault(offset) + 1;
                }
            }

            if (matches.Count == 0)
                continue;

            var top = matches.Values.Max();
            var topBeyondSmaller = matches.Where(kv => kv.Key >= smaller).Select(kv => kv.Value).DefaultIfEmpty(0).Max();

            if (topBeyondSmaller >= top * OffsetShare)
                return block;
        }

        return blocks[^1];
    }

    private static WeaponGeneratorInfusion LearnInfusion(Param param, int offset,
        List<(Param.Row Source, Param.Row Target)> basePairs,
        List<(Param.Row Source, Param.Row Target)> allPairs,
        Func<int, string> titleText,
        List<(Param.Column Column, int Family)> statFields,
        List<(Param.Column Column, int Family)> allStatFields,
        List<Param.Column> groupColumns)
    {
        var infusion = new WeaponGeneratorInfusion { IdOffset = offset };

        // Names
        var rowTemplate = LearnTemplate(basePairs.Select(p => (p.Source.Name, p.Target.Name)));
        var name = TemplateLabel(rowTemplate);

        string fmgTemplate = null;

        if (titleText != null)
        {
            fmgTemplate = LearnTemplate(basePairs.Select(p => (titleText(p.Source.ID), titleText(p.Target.ID))));

            // The title text is what players see, so it names the infusion when it has a label
            var fmgName = TemplateLabel(fmgTemplate);

            if (!string.IsNullOrEmpty(fmgName))
                name = fmgName;
        }

        infusion.Name = string.IsNullOrEmpty(name) ? $"+{offset}" : name;
        infusion.RowNameTemplate = Tokenize(rowTemplate ?? NameToken, infusion.Name);
        infusion.FmgTitleTemplate = Tokenize(fmgTemplate ?? rowTemplate ?? NameToken, infusion.Name);

        // Rules: Set, Add and a value per group for every numeric field; the stat fields also get the factor models
        var sample = allPairs.Count > 0 ? allPairs[0].Source : null;

        var numericFields = param.Columns
            .Where(c => c.Def.DisplayType is not (PARAMDEF.DefType.dummy8 or PARAMDEF.DefType.fixstr or PARAMDEF.DefType.fixstrW))
            .Where(c => sample != null && c.GetValue(sample) is not (byte[] or string))
            .ToList();

        var statColumns = new HashSet<Param.Column>((statFields ?? allStatFields).Select(s => s.Column));

        var plainFields = numericFields.Where(c => !statColumns.Contains(c)).Select(c => (c, -1)).ToList();

        infusion.Rules.AddRange(LearnFieldRules(allPairs, plainFields, null, groupColumns, RuleShare));

        if (statFields != null)
        {
            infusion.Rules.AddRange(LearnFieldRules(allPairs, statFields, statFields, groupColumns, StatMinShare));
        }
        else
        {
            // Without the damage and scaling option the stat fields only get plain Set and Add rules (not a value
            // per weapon type), as they did before the option existed
            var plainStats = numericFields.Where(statColumns.Contains).Select(c => (c, -1)).ToList();
            infusion.Rules.AddRange(LearnFieldRules(allPairs, plainStats, null, new List<Param.Column>(), RuleShare));
        }

        return infusion;
    }

    /// <summary>
    /// The most common "prefix{name}suffix" pattern that turns source names into target names, or null if no
    /// pattern covers enough of the named pairs
    /// </summary>
    private static string LearnTemplate(IEnumerable<(string Source, string Target)> names)
    {
        var counts = new Dictionary<string, int>();
        var named = 0;

        foreach (var (rawSource, rawTarget) in names)
        {
            var source = rawSource?.Trim();
            var target = rawTarget?.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                continue;

            named++;

            string template = null;

            if (target.EndsWith(source, StringComparison.Ordinal) && target.Length > source.Length)
                template = target[..^source.Length] + NameToken;
            else if (target.StartsWith(source, StringComparison.Ordinal) && target.Length > source.Length)
                template = NameToken + target[source.Length..];

            if (template != null)
                counts[template] = counts.GetValueOrDefault(template) + 1;
        }

        if (counts.Count == 0)
            return null;

        var top = counts.OrderByDescending(kv => kv.Value).First();

        return top.Value >= named * NameShare ? top.Key : null;
    }

    /// <summary>
    /// The template's literal text without brackets, e.g. "Heavy" for "Heavy {name}", "Crystal" for "{name} [Crystal]"
    /// </summary>
    private static string TemplateLabel(string template)
    {
        if (template == null)
            return null;

        return template.Replace(NameToken, "").Trim().Trim('[', ']', '(', ')', '<', '>').Trim();
    }

    /// <summary>
    /// Replaces the infusion name in the template with the token, so renaming the infusion renames its rows
    /// </summary>
    private static string Tokenize(string template, string infusionName)
    {
        if (string.IsNullOrEmpty(infusionName) || infusionName.StartsWith('+'))
            return template;

        var literal = template.Replace(NameToken, "\u0001");
        var index = literal.IndexOf(infusionName, StringComparison.Ordinal);

        if (index < 0)
            return template;

        literal = literal[..index] + InfusionToken + literal[(index + infusionName.Length)..];

        return literal.Replace("\u0001", NameToken);
    }

    #endregion

    #region Stat Rules

    // A stat rule must reproduce at least this share of the rows, and this much more than leaving the field as it is
    private const double StatMinShare = 0.3;
    private const double StatBeatCopy = 0.1;

    // A grouped rule is only used when it reproduces this much more than the best ungrouped one
    private const double StatGroupGain = 0.05;

    // A group field with more values than this is per weapon, not a category
    private const int MaxGroups = 100;

    // A decimal field holds whole numbers when at least this share of its infused values are whole. Not all: DS1's
    // Magic infusions have a few scaling values like 21.6 among the whole ones.
    private const double WholeShare = 0.9;

    /// <summary>
    /// The numeric fields matching the patterns, each with the index of the pattern that matched (its family:
    /// the fields summed by the "sum of the family" model)
    /// </summary>
    public static List<(Param.Column Column, int Family)> MatchStatFields(Param param, string patterns)
    {
        var includes = new List<Regex>();
        var excludes = new List<Regex>();

        foreach (var raw in (patterns ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var exclude = raw.StartsWith('!');
            var text = exclude ? raw[1..].Trim() : raw;

            if (text == "")
                continue;

            var regex = new Regex("^" + Regex.Escape(text).Replace("\\*", ".*") + "$", RegexOptions.CultureInvariant);

            if (exclude)
                excludes.Add(regex);
            else
                includes.Add(regex);
        }

        var result = new List<(Param.Column, int)>();
        var sample = param.Rows.FirstOrDefault();

        if (sample == null)
            return result;

        foreach (var column in param.Columns)
        {
            var name = column.Def.InternalName;

            if (column.Def.DisplayType == PARAMDEF.DefType.dummy8 || column.GetValue(sample) is byte[] or string)
                continue;

            if (excludes.Any(r => r.IsMatch(name)))
                continue;

            var family = includes.FindIndex(r => r.IsMatch(name));

            if (family >= 0)
                result.Add((column, family));
        }

        return result;
    }

    /// <summary>
    /// The listed fields that exist and hold a number
    /// </summary>
    public static List<Param.Column> GetGroupColumns(Param param, string fields)
    {
        var result = new List<Param.Column>();
        var sample = param.Rows.FirstOrDefault();

        if (sample == null)
            return result;

        foreach (var name in (fields ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var column = param[name];

            if (column != null && column.GetValue(sample) is not (byte[] or string))
                result.Add(column);
        }

        return result;
    }

    private class StatCandidate
    {
        public WeaponGeneratorRuleOp Op;
        public double Value;
        public List<Param.Column> Sources;
        public Param.Column Group;
        public Dictionary<double, double> GroupValues;
        public int Hits;
    }

    /// <summary>
    /// For each field that infusions change, the best simple model of the infused value: a fixed value, an offset,
    /// and a value per group (e.g. a status effect per weapon type). With <paramref name="sourceFields"/> (the stat
    /// fields) also the factor models: the base's own value times a factor, another stat field (or the sum of the
    /// field's family) times a factor, and each of those per group.
    /// <para>
    /// Set and Add models count a row only when they give its exact value (they are IDs as often as numbers); the
    /// factor models count it when they land within 1 (or 1%). A model must reproduce at least
    /// <paramref name="minShare"/> of the rows, and clearly more than leaving the field as it is.
    /// </para>
    /// </summary>
    private static List<WeaponGeneratorRule> LearnFieldRules(List<(Param.Row Source, Param.Row Target)> pairs,
        List<(Param.Column Column, int Family)> fields, List<(Param.Column Column, int Family)> sourceFields,
        List<Param.Column> groupColumns, double minShare)
    {
        var rules = new List<WeaponGeneratorRule>();
        var n = pairs.Count;

        if (n == 0)
            return rules;

        static double D(object value) => Convert.ToDouble(value, CultureInfo.InvariantCulture);

        // Values read once per pair
        var readColumns = fields.Select(f => f.Column)
            .Concat(sourceFields?.Select(f => f.Column) ?? Enumerable.Empty<Param.Column>())
            .Distinct()
            .ToList();

        var src = readColumns.ToDictionary(c => c, c => pairs.Select(p => D(c.GetValue(p.Source))).ToArray());
        var tgt = readColumns.ToDictionary(c => c, c => pairs.Select(p => D(c.GetValue(p.Target))).ToArray());

        // Group fields that look like categories for these pairs
        var groups = new List<(Param.Column Column, double[] Keys)>();

        foreach (var column in groupColumns)
        {
            var keys = pairs.Select(p => D(column.GetValue(p.Source))).ToArray();
            var distinct = keys.Distinct().Count();

            if (distinct >= 2 && distinct <= MaxGroups)
                groups.Add((column, keys));
        }

        foreach (var (target, family) in fields)
        {
            var tv = tgt[target];
            var sv = src[target];

            var same = 0;
            for (int i = 0; i < n; i++)
                if (sv[i] == tv[i]) same++;

            if (same >= n * SameShare)
                continue;

            var isInteger = target.GetValue(pairs[0].Target) is not (float or double);

            // A decimal field whose infused values are whole numbers (the scaling stats) gets rounded rules
            var wholeValues = !isInteger && tv.Count(v => v == Math.Round(v)) >= n * WholeShare;

            double Round(double value) => isInteger || wholeValues ? Math.Round(value, MidpointRounding.AwayFromZero) : value;
            // Exact for Set and Add, close enough for the factor models
            static bool Hit(double predicted, double real, bool approximate) => approximate
                ? Math.Abs(predicted - real) <= Math.Max(1, Math.Abs(real) * 0.01)
                : Math.Abs(predicted - real) <= 1e-4;

            int CountHits(Func<int, double> predict, bool approximate = false)
            {
                var hits = 0;
                for (int i = 0; i < n; i++)
                    if (Hit(predict(i), tv[i], approximate)) hits++;
                return hits;
            }

            StatCandidate bestPlain = null;
            StatCandidate bestGrouped = null;

            void Consider(StatCandidate c)
            {
                if (c.Group == null)
                {
                    if (bestPlain == null || c.Hits > bestPlain.Hits)
                        bestPlain = c;
                }
                else if (bestGrouped == null || c.Hits > bestGrouped.Hits)
                {
                    bestGrouped = c;
                }
            }

            // Fixed value and offset
            var topSet = Enumerable.Range(0, n).GroupBy(i => tv[i]).OrderByDescending(g => g.Count()).First().Key;
            Consider(new StatCandidate { Op = WeaponGeneratorRuleOp.Set, Value = topSet, Hits = CountHits(i => topSet) });

            var topDelta = Enumerable.Range(0, n).GroupBy(i => Math.Round(tv[i] - sv[i], 4)).OrderByDescending(g => g.Count()).First().Key;
            if (topDelta != 0)
                Consider(new StatCandidate { Op = WeaponGeneratorRuleOp.Add, Value = topDelta, Hits = CountHits(i => sv[i] == -1 ? -1 : sv[i] + topDelta) });

            foreach (var (groupColumn, keys) in groups)
            {
                var values = Enumerable.Range(0, n).GroupBy(i => keys[i])
                    .ToDictionary(g => g.Key, g => g.GroupBy(i => tv[i]).OrderByDescending(h => h.Count()).First().Key);

                Consider(new StatCandidate
                {
                    Op = WeaponGeneratorRuleOp.Set,
                    Value = topSet,
                    Group = groupColumn,
                    GroupValues = values,
                    Hits = CountHits(i => values[keys[i]])
                });
            }

            // Factor models (stat fields only): the field itself, each other stat field, and the sum of its family
            var specs = new List<List<Param.Column>>();

            if (sourceFields != null)
            {
                specs.Add(new() { target });
                specs.AddRange(sourceFields.Where(s => s.Column != target).Select(s => new List<Param.Column> { s.Column }));

                var familyColumns = sourceFields.Where(s => s.Family == family).Select(s => s.Column).ToList();
                if (familyColumns.Count > 1)
                    specs.Add(familyColumns);
            }

            foreach (var spec in specs)
            {
                var isSelf = spec.Count == 1 && spec[0] == target;
                var sum = new double[n];

                for (int i = 0; i < n; i++)
                    foreach (var c in spec)
                        sum[i] += src[c][i];

                // Multiply leaves -1 (no reference) as it is
                double Predict(int i, double factor) => isSelf && sv[i] == -1 ? -1 : Round(sum[i] * factor);

                var withSource = Enumerable.Range(0, n).Where(i => sum[i] > 0).ToList();

                if (withSource.Count < n * StatMinShare)
                    continue;

                var factor = Math.Round(Median(withSource.Select(i => tv[i] / sum[i])), 4);
                var op = isSelf ? WeaponGeneratorRuleOp.Multiply : WeaponGeneratorRuleOp.ScaleFrom;

                Consider(new StatCandidate { Op = op, Value = factor, Sources = spec, Hits = CountHits(i => Predict(i, factor), true) });

                foreach (var (groupColumn, keys) in groups)
                {
                    var factors = withSource.GroupBy(i => keys[i])
                        .ToDictionary(g => g.Key, g => Math.Round(Median(g.Select(i => tv[i] / sum[i])), 4));

                    Consider(new StatCandidate
                    {
                        Op = op,
                        Value = factor,
                        Sources = spec,
                        Group = groupColumn,
                        GroupValues = factors,
                        Hits = CountHits(i => Predict(i, factors.TryGetValue(keys[i], out var f) ? f : factor), true)
                    });
                }
            }

            var best = bestPlain;

            if (bestGrouped != null && (best == null || bestGrouped.Hits >= best.Hits + n * StatGroupGain))
                best = bestGrouped;

            // A rule must reproduce enough rows, and clearly more than copying the base would (e.g. not "Set -1"
            // where most bases already have -1)
            if (best == null || best.Hits < n * minShare || best.Hits < same + n * StatBeatCopy)
                continue;

            var rule = ToRule(target, best, n);

            // Set and Add keep whole values whole on their own
            rule.Round = wholeValues && best.Op is WeaponGeneratorRuleOp.Multiply or WeaponGeneratorRuleOp.ScaleFrom;

            rules.Add(rule);
        }

        return rules;
    }

    private static WeaponGeneratorRule ToRule(Param.Column target, StatCandidate candidate, int pairCount)
    {
        var rule = new WeaponGeneratorRule
        {
            Field = target.Def.InternalName,
            Op = candidate.Op,
            Value = FormatValue(candidate.Value),
            Accuracy = (double)candidate.Hits / pairCount
        };

        if (candidate.Op == WeaponGeneratorRuleOp.ScaleFrom)
            rule.SourceFields = string.Join(" + ", candidate.Sources.Select(c => c.Def.InternalName));

        if (candidate.Group != null)
        {
            rule.GroupField = candidate.Group.Def.InternalName;
            rule.Groups = candidate.GroupValues
                .OrderBy(kv => kv.Key)
                .Select(kv => new WeaponGeneratorRuleGroup { Key = FormatValue(kv.Key), Value = FormatValue(kv.Value) })
                .ToList();
        }

        return rule;
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();

        return sorted.Count == 0 ? 0 : sorted[sorted.Count / 2];
    }

    #endregion
}
