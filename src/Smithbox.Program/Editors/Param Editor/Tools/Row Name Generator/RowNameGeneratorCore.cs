using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

public enum RowNameGenScope
{
    SelectedRows,
    SelectedParam,
    AllParams
}

public enum RowNameGenSource
{
    RowText,
    Animation,
    ParamRule,
    IncomingReference,
    TextReference,
    ParamReference,
    Alias
}

public class RowNameGenOptions
{
    public bool UseRowText = true;

    /// <summary>Name BehaviorParam rows from the time act events that use them and the Havok state of the animation.</summary>
    public bool UseAnimations = true;

    /// <summary>Add the animation (a000_003000) after the state name.</summary>
    public bool IncludeAnimationId = false;

    public bool UseIncomingReferences = true;
    public bool UseTextReferences = true;
    public bool UseParamReferences = true;
    public bool UseAliases = true;

    /// <summary>
    /// Params with a rule of their own (item lots, attacks, NPCs ...) are named only by that rule,
    /// not by the general sources. See RowNameGeneratorRules.cs.
    /// </summary>
    public bool UseParamRules = true;

    /// <summary>Item lots whose row ID has 8 or more digits start with the alias of the map the ID begins with.</summary>
    public bool ItemLotMapPrefix = false;

    /// <summary>Only rows with an empty name are given one.</summary>
    public bool OnlyUnnamed = true;

    /// <summary>Repeat the lookups so a name generated in one pass can be used by the next.</summary>
    public bool ChainGeneratedNames = true;

    /// <summary>Try the rows that reference a row before the rows it references.</summary>
    public bool PreferIncoming = true;

    /// <summary>Add the field that gave the name, e.g. "Fireball (Attack ID)".</summary>
    public bool IncludeSourceLabel = false;
}

public class GeneratedRowName
{
    public string Param;
    public Param.Row Row;
    public string OldName;
    public string NewName;
    public RowNameGenSource Source;
    public string Detail;
}

/// <summary>
/// Builds row names from what a row is linked to: its own text entry, the rows that reference it,
/// the rows and text it references, and project aliases (characters, flags, sounds...).
/// Meant for modded regulations that ship without row names.
/// Nothing is changed until <see cref="Apply"/> is called, which is one undoable step.
/// </summary>
public partial class RowNameGeneratorCore
{
    private const int MaxPasses = 8;
    private const int MaxNameLength = 120;

    private readonly ParamEditorView View;
    private readonly ParamBank Bank;
    private readonly ParamData Data;
    private readonly RowNameGenOptions Options;

    // Param name -> ID -> first row with that ID (Param's own indexer is a linear scan)
    private readonly Dictionary<string, Dictionary<int, Param.Row>> IdIndex = new();

    // Param name -> fields that link elsewhere
    private readonly Dictionary<string, List<LinkField>> LinkFields = new();

    // Param name -> row ID -> text of the row's own text entry
    private readonly Dictionary<string, Dictionary<int, string>> RowTextCache = new();

    // Alias type -> value -> alias name
    private readonly Dictionary<ProjectAliasType, Dictionary<string, string>> AliasCache = new();

    // Target row -> rows that reference it
    private readonly Dictionary<Param.Row, List<IncomingRef>> Incoming = new();

    // Names settled in earlier passes (the current pass reads only these, so the order of rows doesn't matter)
    private readonly Dictionary<Param.Row, string> Settled = new();

    private record struct IncomingRef(string Param, Param.Row Row, string FieldLabel);

    private class LinkField
    {
        public Param.Column Column;
        public string Label;
        public ParamFieldMeta Meta;
        public List<(ParamRef Ref, Param.Column Condition)> ParamRefs = new();
        public List<(FMGRef Ref, Param.Column Condition)> TextRefs = new();
        public ProjectAliasType? AliasType;
        public Param.Column AliasCondition;
        public int AliasConditionValue;
    }

    public RowNameGeneratorCore(ParamEditorView view, RowNameGenOptions options)
    {
        View = view;
        Bank = view.GetPrimaryBank();
        Data = view.GetParamData();
        Options = options;
    }

    #region Generate
    public List<GeneratedRowName> Generate(RowNameGenScope scope)
    {
        return Generate(GetTargets(scope));
    }

    /// <summary>Generate for rows captured earlier with <see cref="GetTargets"/> (param name -> rows)</summary>
    public List<GeneratedRowName> Generate(Dictionary<string, List<Param.Row>> targets)
    {
        var results = new List<GeneratedRowName>();

        if (Bank?.Params == null || targets == null)
            return results;

        // Rows deleted since they were captured are left out
        targets = targets
            .Where(t => Bank.Params.TryGetValue(t.Key, out var p) && p.Rows.Count > 0)
            .ToDictionary(t => t.Key, t =>
            {
                var live = new HashSet<Param.Row>(Bank.Params[t.Key].Rows);
                return t.Value.Where(live.Contains).ToList();
            });

        if (targets.Count == 0)
            return results;

        // Rules for attacks, bullets and resist corrections read it too
        if (Options.UseIncomingReferences || Options.UseParamRules)
            BuildIncomingIndex(targets);

        var passes = Options.ChainGeneratedNames ? MaxPasses : 1;

        for (var pass = 0; pass < passes; pass++)
        {
            var found = new List<GeneratedRowName>();

            foreach (var (paramName, rows) in targets)
            {
                foreach (var row in rows)
                {
                    if (Settled.ContainsKey(row))
                        continue;

                    var result = GenerateForRow(paramName, row);

                    if (result != null)
                        found.Add(result);
                }
            }

            if (found.Count == 0)
                break;

            foreach (var result in found)
            {
                Settled[result.Row] = result.NewName;
                results.Add(result);
            }
        }

        // Drop results that wouldn't change anything (overwrite mode, same name)
        results.RemoveAll(e => e.NewName == e.OldName);

        return results;
    }

    /// <summary>The rows a scope covers now (param name -> rows), filtered by Only Unnamed Rows</summary>
    public Dictionary<string, List<Param.Row>> GetTargets(RowNameGenScope scope)
    {
        var targets = new Dictionary<string, List<Param.Row>>();

        void Add(string paramName, IEnumerable<Param.Row> rows)
        {
            var list = rows
                .Where(r => r != null)
                .Where(r => !Options.OnlyUnnamed || string.IsNullOrWhiteSpace(r.Name))
                .ToList();

            if (list.Count > 0)
                targets[paramName] = list;
        }

        var activeParam = View.Selection.GetActiveParam();

        switch (scope)
        {
            case RowNameGenScope.SelectedRows:
                if (activeParam != null && Bank.Params.ContainsKey(activeParam))
                    Add(activeParam, View.Selection.GetSelectedRows());
                break;

            case RowNameGenScope.SelectedParam:
                if (activeParam != null && Bank.Params.TryGetValue(activeParam, out var param))
                    Add(activeParam, param.Rows);
                break;

            case RowNameGenScope.AllParams:
                foreach (var (name, p) in Bank.Params)
                    Add(name, p.Rows);
                break;
        }

        return targets;
    }

    private GeneratedRowName GenerateForRow(string paramName, Param.Row row)
    {
        // A param with a rule of its own is named by it alone: the general sources give it poor names
        if (Options.UseParamRules && GetRule(paramName) is { } rule)
            return rule(paramName, row);

        GeneratedRowName result = null;

        if (Options.UseRowText)
            result = FromRowText(paramName, row);

        if (result == null && Options.UseAnimations)
            result = FromAnimation(paramName, row);

        if (result == null)
        {
            if (Options.PreferIncoming)
            {
                result = FromIncoming(paramName, row) ?? FromOutgoing(paramName, row);
            }
            else
            {
                result = FromOutgoing(paramName, row) ?? FromIncoming(paramName, row);
            }
        }

        return result;
    }

    private GeneratedRowName FromOutgoing(string paramName, Param.Row row)
    {
        GeneratedRowName result = null;

        if (Options.UseTextReferences)
            result = FromTextReferences(paramName, row);

        if (result == null && Options.UseParamReferences)
            result = FromParamReferences(paramName, row);

        if (result == null && Options.UseAliases)
            result = FromAliases(paramName, row);

        return result;
    }

    private GeneratedRowName MakeResult(string paramName, Param.Row row, string name, string label, RowNameGenSource source, string detail)
    {
        var newName = name;

        if (Options.IncludeSourceLabel && !string.IsNullOrEmpty(label))
            newName = $"{name} ({label})";

        if (newName.Length > MaxNameLength)
            newName = newName.Substring(0, MaxNameLength).TrimEnd();

        return new GeneratedRowName
        {
            Param = paramName,
            Row = row,
            OldName = row.Name ?? "",
            NewName = newName,
            Source = source,
            Detail = detail
        };
    }

    /// <summary>The name a row has as far as this run is concerned: its generated name if it got one in an earlier pass.</summary>
    private string NameOf(Param.Row row)
    {
        if (Settled.TryGetValue(row, out var name))
            return name;

        return row.Name;
    }
    #endregion

    #region Sources
    private GeneratedRowName FromRowText(string paramName, Param.Row row)
    {
        var name = GetRowText(paramName, row.ID);

        if (name != null)
            return MakeResult(paramName, row, name, null, RowNameGenSource.RowText, LOC.Get("PARAM_RowNameGen_Detail_Row_Text"));

        return null;
    }

    private string GetRowText(string paramName, int id)
    {
        if (!RowTextCache.TryGetValue(paramName, out var texts))
        {
            texts = new Dictionary<int, string>();

            foreach (var entry in ParamFmgUtils.GetRowDecoratorFmgEntries(View.Editor, paramName))
            {
                var text = CleanName(entry.Text);

                if (text != null)
                    texts.TryAdd(entry.ID, text);
            }

            RowTextCache[paramName] = texts;
        }

        return texts.TryGetValue(id, out var name) ? name : null;
    }

    private GeneratedRowName FromIncoming(string paramName, Param.Row row)
    {
        return FromIncoming(paramName, row, null);
    }

    /// <summary>
    /// The name most of the rows that reference this row share, so a row used by one family of rows isn't named after
    /// an odd one out (ties: the first referencing row). With <paramref name="groups"/>, only referencing rows of those
    /// params count, a group at a time: the first group with a named referencing row wins.
    /// </summary>
    private GeneratedRowName FromIncoming(string paramName, Param.Row row, string[][] groups)
    {
        if (!Incoming.TryGetValue(row, out var refs))
            return null;

        foreach (var group in groups ?? new string[][] { null })
        {
            var candidates = group == null ? refs : refs.Where(r => group.Contains(r.Param)).ToList();

            string bestName = null;
            IncomingRef bestRef = default;
            var bestCount = 0;
            var counts = new Dictionary<string, int>();

            foreach (var rf in candidates)
            {
                var name = CleanName(NameOf(rf.Row));

                if (name == null)
                    continue;

                counts.TryGetValue(name, out var count);
                count++;
                counts[name] = count;

                if (count > bestCount)
                {
                    bestCount = count;
                    bestName = name;
                    bestRef = rf;
                }
            }

            if (bestName == null)
                continue;

            var detail = LOC.Get("PARAM_RowNameGen_Detail_Incoming", bestRef.Param, bestRef.Row.ID, bestRef.FieldLabel);

            if (candidates.Count > 1)
                detail += " " + LOC.Get("PARAM_RowNameGen_Detail_Incoming_Count", candidates.Count);

            return MakeResult(paramName, row, bestName, bestRef.Param, RowNameGenSource.IncomingReference, detail);
        }

        return null;
    }

    private GeneratedRowName FromParamReferences(string paramName, Param.Row row)
    {
        foreach (var field in GetLinkFields(paramName))
        {
            if (TryResolveParamRef(field, row, out var name, out var detail))
                return MakeResult(paramName, row, name, field.Label, RowNameGenSource.ParamReference, detail);
        }

        return null;
    }

    private GeneratedRowName FromTextReferences(string paramName, Param.Row row)
    {
        foreach (var field in GetLinkFields(paramName))
        {
            if (TryResolveTextRef(field, row, out var name, out var detail))
                return MakeResult(paramName, row, name, field.Label, RowNameGenSource.TextReference, detail);
        }

        return null;
    }

    private GeneratedRowName FromAliases(string paramName, Param.Row row)
    {
        foreach (var field in GetLinkFields(paramName))
        {
            if (TryResolveAlias(field, row, out var name, out var detail))
                return MakeResult(paramName, row, name, field.Label, RowNameGenSource.Alias, detail);
        }

        return null;
    }

    /// <summary>What a field of the row points at, named: its text, else its row, else its alias</summary>
    private bool TryResolveField(LinkField field, Param.Row row, out string name, out string detail)
    {
        return TryResolveTextRef(field, row, out name, out detail)
            || TryResolveParamRef(field, row, out name, out detail)
            || TryResolveAlias(field, row, out name, out detail);
    }

    /// <summary>
    /// The name of the row a field points at. A row that isn't named yet still counts by its own text entry
    /// (an item lot can list goods of a mod that has no row names).
    /// </summary>
    private bool TryResolveParamRef(LinkField field, Param.Row row, out string name, out string detail)
    {
        name = null;
        detail = null;

        if (field.ParamRefs.Count == 0)
            return false;

        if (!TryGetInt(field.Column, row, out var value) || value <= 0)
            return false;

        foreach (var (pref, condition) in field.ParamRefs)
        {
            if (!ConditionMet(condition, row, pref.ConditionValue))
                continue;

            var (target, hint) = ResolveParamRef(pref, value);

            if (target == null || target == row)
                continue;

            var targetName = CleanName(NameOf(target)) ?? GetRowText(pref.ParamName, target.ID);

            if (targetName == null)
                continue;

            name = targetName + hint;
            detail = LOC.Get("PARAM_RowNameGen_Detail_Param_Ref", field.Label, pref.ParamName, target.ID);
            return true;
        }

        return false;
    }

    private bool TryResolveTextRef(LinkField field, Param.Row row, out string name, out string detail)
    {
        name = null;
        detail = null;

        if (field.TextRefs.Count == 0)
            return false;

        if (!TryGetInt(field.Column, row, out var value) || value <= 0)
            return false;

        return TryResolveTextRef(field, row, value, out name, out detail);
    }

    /// <summary>The text a field points at, for a value given by the caller (e.g. the absolute value of a negative ID)</summary>
    private bool TryResolveTextRef(LinkField field, Param.Row row, int value, out string name, out string detail)
    {
        name = null;
        detail = null;

        if (field.TextRefs.Count == 0)
            return false;

        var textView = View.Project.Handler.TextEditor?.ViewHandler?.ActiveView;

        if (textView == null)
            return false;

        foreach (var (fref, condition) in field.TextRefs)
        {
            if (!ConditionMet(condition, row, fref.conditionValue))
                continue;

            var result = TextFinder.GetTextResult(textView, fref.fmg, value, fref.offset);
            var text = CleanName(result?.Entry?.Text);

            if (text == null)
                continue;

            name = text;
            detail = LOC.Get("PARAM_RowNameGen_Detail_Text_Ref", field.Label, fref.fmg, value + fref.offset);
            return true;
        }

        return false;
    }

    private bool TryResolveAlias(LinkField field, Param.Row row, out string name, out string detail)
    {
        name = null;
        detail = null;

        if (field.AliasType == null)
            return false;

        if (!ConditionMet(field.AliasCondition, row, field.AliasConditionValue))
            return false;

        if (!TryGetInt(field.Column, row, out var value) || value <= 0)
            return false;

        if (!GetAliases(field.AliasType.Value).TryGetValue($"{value}", out var alias))
            return false;

        name = CleanName(alias);

        if (name == null)
            return false;

        detail = LOC.Get("PARAM_RowNameGen_Detail_Alias", field.Label, field.AliasType.Value, value);
        return true;
    }
    #endregion

    #region Animations
    // As TimeActEventResolver: BehaviorParam row = NPC base + variation * 1000 + judge,
    // BehaviorParam_PC row = player base + weapon variation * 1000 + judge (ER/NR keep a category in the judge's thousands)
    private const string NpcBehaviorParam = "BehaviorParam";
    private const string PlayerBehaviorParam = "BehaviorParam_PC";
    private const string PlayerCharacter = "c0000";
    private const int NpcBehaviorBase = 200000000;
    private const int PlayerBehaviorBase = 100000000;
    private const int VariationScale = 1000;

    // NpcParam behaviorVariationId -> characters (cXXXX) with a time act
    private Dictionary<int, List<string>> NpcVariationCharacters;

    // EquipParamWeapon behaviorVariationId -> weapons
    private Dictionary<int, List<Param.Row>> WeaponVariations;

    private HashSet<string> TimeActCharacters;

    /// <summary>
    /// The characters whose time acts and behaviors the generator needs for the rows in scope, so they can be read in
    /// the background before <see cref="Generate"/> runs
    /// </summary>
    public HashSet<string> GetAnimationCharacters(Dictionary<string, List<Param.Row>> targets)
    {
        var characters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Options.UseAnimations || Bank?.Params == null || targets == null)
            return characters;

        if (targets.TryGetValue(PlayerBehaviorParam, out var playerRows) && playerRows.Count > 0 &&
            HasCharacterTimeAct(PlayerCharacter))
        {
            characters.Add(PlayerCharacter);
        }

        if (targets.TryGetValue(NpcBehaviorParam, out var npcRows))
        {
            foreach (var row in npcRows)
            {
                if (!TryDecodeNpcBehavior(row.ID, out var variation, out _))
                    continue;

                if (GetNpcVariationCharacters().TryGetValue(variation, out var list))
                {
                    foreach (var character in list)
                        characters.Add(character);
                }
            }
        }

        return characters;
    }

    private GeneratedRowName FromAnimation(string paramName, Param.Row row)
    {
        if (paramName == NpcBehaviorParam)
            return FromNpcAnimation(paramName, row);

        if (paramName == PlayerBehaviorParam)
            return FromPlayerAnimation(paramName, row);

        return null;
    }

    private GeneratedRowName FromNpcAnimation(string paramName, Param.Row row)
    {
        if (TryDecodeNpcBehavior(row.ID, out var variation, out var judge))
        {
            if (!GetNpcVariationCharacters().TryGetValue(variation, out var characters))
                return null;

            foreach (var character in characters)
            {
                var info = RowNameAnimationIndex.Get(View.Project, character);

                if (info == null || !info.VariationJudges.TryGetValue(judge, out var animations))
                    continue;

                return MakeAnimationResult(paramName, row, GetCharacterName(character), info, animations, judge);
            }

            return null;
        }

        // Below the NPC base: a common behavior, used as it is by any character that has it
        CharacterAnimationInfo commonInfo = null;
        List<long> commonAnimations = null;
        var users = 0;

        foreach (var character in GetNpcVariationCharacters().Values.SelectMany(e => e).Distinct())
        {
            var info = RowNameAnimationIndex.Get(View.Project, character);

            if (info == null || !info.CommonJudges.TryGetValue(row.ID, out var animations))
                continue;

            users++;

            if (commonInfo == null)
            {
                commonInfo = info;
                commonAnimations = animations;
            }
        }

        if (commonInfo == null)
            return null;

        // Shared by several characters: the state alone names it
        var prefix = users == 1 ? GetCharacterName(commonInfo.Character) : null;

        return MakeAnimationResult(paramName, row, prefix, commonInfo, commonAnimations, row.ID);
    }

    private GeneratedRowName FromPlayerAnimation(string paramName, Param.Row row)
    {
        var info = RowNameAnimationIndex.Get(View.Project, PlayerCharacter);

        if (info == null)
            return null;

        foreach (var (variation, judge) in DecodePlayerBehavior(row.ID))
        {
            List<long> animations = null;

            // The row of the ID as it is: common behaviors, and attacks without a weapon (kicks)
            if (variation < 0)
            {
                if (!info.CommonJudges.TryGetValue(judge, out animations))
                    info.VariationJudges.TryGetValue(judge, out animations);
            }
            else
            {
                info.VariationJudges.TryGetValue(judge, out animations);
            }

            if (animations == null || animations.Count == 0)
                continue;

            var prefix = variation > 0 ? GetWeaponName(variation) : null;

            return MakeAnimationResult(paramName, row, prefix, info, animations, judge);
        }

        return null;
    }

    private GeneratedRowName MakeAnimationResult(string paramName, Param.Row row, string prefix,
        CharacterAnimationInfo info, List<long> animations, int judge)
    {
        // The first animation whose state is named, else the first animation
        var animation = animations.FirstOrDefault(a => info.GetStateName(a) != null, animations[0]);
        var animationName = info.FormatAnimationName(animation);
        var state = CleanName(info.GetStateName(animation));

        string label;

        if (state == null)
            label = animationName;
        else if (Options.IncludeAnimationId)
            label = $"{state} [{animationName}]";
        else
            label = state;

        var name = string.IsNullOrEmpty(prefix) ? label : $"{prefix} - {label}";

        var detail = LOC.Get("PARAM_RowNameGen_Detail_Animation", info.Character, animationName, judge);

        if (animations.Count > 1)
            detail += " " + LOC.Get("PARAM_RowNameGen_Detail_Animation_Count", animations.Count);

        return MakeResult(paramName, row, name, null, RowNameGenSource.Animation, detail);
    }

    private static bool TryDecodeNpcBehavior(int id, out int variation, out int judge)
    {
        variation = 0;
        judge = 0;

        if (id < NpcBehaviorBase)
            return false;

        var rest = id - NpcBehaviorBase;
        variation = rest / VariationScale;
        judge = rest % VariationScale;

        return true;
    }

    /// <summary>
    /// The (weapon variation, judge) pairs a BehaviorParam_PC row ID can come from, best first.
    /// Variation 0 is the row without a variation; -1 is the row of the judge ID as it is.
    /// </summary>
    private IEnumerable<(int Variation, int Judge)> DecodePlayerBehavior(int id)
    {
        if (id >= PlayerBehaviorBase)
        {
            var category = id / PlayerBehaviorBase;
            var rest = id % PlayerBehaviorBase;
            var variation = rest / VariationScale;
            var low = rest % VariationScale;

            // Elden Ring and Nightreign keep a further category in the thousands of the judge ID (as TimeActEventResolver)
            if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (category == 1)
                {
                    yield return (variation, low);
                    yield return (variation, 1000 + low);
                }
                else if (category <= 9)
                {
                    yield return (variation, category * 1000 + low);
                }
            }
            else if (category == 1)
            {
                yield return (variation, low);
            }
        }

        yield return (-1, id);
    }

    private Dictionary<int, List<string>> GetNpcVariationCharacters()
    {
        if (NpcVariationCharacters != null)
            return NpcVariationCharacters;

        NpcVariationCharacters = new Dictionary<int, List<string>>();

        if (!Bank.Params.TryGetValue("NpcParam", out var npcParam))
            return NpcVariationCharacters;

        var column = FindColumn(npcParam, "behaviorVariationId");

        if (column == null)
            return NpcVariationCharacters;

        foreach (var row in npcParam.Rows)
        {
            if (!TryGetInt(column, row, out var variation) || variation <= 0)
                continue;

            var character = GetNpcCharacter(row.ID);

            if (character == null)
                continue;

            if (!NpcVariationCharacters.TryGetValue(variation, out var list))
            {
                list = new List<string>();
                NpcVariationCharacters[variation] = list;
            }

            if (!list.Contains(character))
                list.Add(character);
        }

        return NpcVariationCharacters;
    }

    /// <summary>
    /// The character an NpcParam row belongs to (31000000 -> c3100, DS3 120000 -> c1200), falling back from a variant
    /// to its base as the Time Act Editor does (c3101 -> c3100 -> c3100), to one that has a time act
    /// </summary>
    private string GetNpcCharacter(int npcId)
    {
        foreach (var scale in new[] { 10000, 1000, 100 })
        {
            var id = npcId / scale;

            if (id < 1000 || id > 9999)
                continue;

            foreach (var candidate in new[] { id, id / 10 * 10, id / 100 * 100 })
            {
                var name = $"c{candidate:D4}";

                if (HasCharacterTimeAct(name))
                    return name;
            }
        }

        return null;
    }

    private bool HasCharacterTimeAct(string character)
    {
        TimeActCharacters ??= new HashSet<string>(
            View.Project.Locator.TimeActFiles.Entries
                .Where(e => e.Folder.StartsWith("/chr", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Filename),
            StringComparer.OrdinalIgnoreCase);

        return TimeActCharacters.Contains(character);
    }

    private string GetCharacterName(string character)
    {
        var aliases = GetAliases(ProjectAliasType.Characters);

        if (long.TryParse(character.TrimStart('c', 'C'), out var number) && aliases.TryGetValue($"{number}", out var alias))
        {
            var name = CleanName(alias);

            if (name != null)
                return name;
        }

        return character;
    }

    /// <summary>
    /// The name of a weapon with this behavior variation: one with exactly it, else one whose base variation it is
    /// (upgrades and infusions fall back on variation / 100 * 100). The weapon's own text counts even if the
    /// row isn't named yet, so behaviors don't have to wait a pass for their weapon.
    /// </summary>
    private string GetWeaponName(int variation)
    {
        if (WeaponVariations == null)
        {
            WeaponVariations = new Dictionary<int, List<Param.Row>>();

            if (Bank.Params.TryGetValue("EquipParamWeapon", out var weapons) &&
                FindColumn(weapons, "behaviorVariationId") is { } column)
            {
                foreach (var weapon in weapons.Rows)
                {
                    if (!TryGetInt(column, weapon, out var v) || v <= 0)
                        continue;

                    if (!WeaponVariations.TryGetValue(v, out var list))
                    {
                        list = new List<Param.Row>();
                        WeaponVariations[v] = list;
                    }

                    list.Add(weapon);
                }
            }
        }

        string NameOfWeapon(Param.Row weapon) => CleanName(NameOf(weapon)) ?? GetRowText("EquipParamWeapon", weapon.ID);

        if (WeaponVariations.TryGetValue(variation, out var exact))
        {
            foreach (var weapon in exact)
            {
                if (NameOfWeapon(weapon) is { } name)
                    return name;
            }
        }

        if (variation % 100 == 0)
        {
            foreach (var (v, list) in WeaponVariations)
            {
                if (v / 100 * 100 != variation)
                    continue;

                foreach (var weapon in list)
                {
                    if (NameOfWeapon(weapon) is { } name)
                        return name;
                }
            }
        }

        return null;
    }
    #endregion

    #region Lookups
    private void BuildIncomingIndex(Dictionary<string, List<Param.Row>> targets)
    {
        var targetRows = new HashSet<Param.Row>(targets.Values.SelectMany(e => e));

        foreach (var (sourceName, sourceParam) in Bank.Params)
        {
            foreach (var field in GetLinkFields(sourceName))
            {
                foreach (var (pref, condition) in field.ParamRefs)
                {
                    // Only references into the params being named matter
                    if (!targets.ContainsKey(pref.ParamName))
                        continue;

                    foreach (var row in sourceParam.Rows)
                    {
                        if (!TryGetInt(field.Column, row, out var value) || value <= 0)
                            continue;

                        if (!ConditionMet(condition, row, pref.ConditionValue))
                            continue;

                        var (target, _) = ResolveParamRef(pref, value);

                        if (target == null || target == row || !targetRows.Contains(target))
                            continue;

                        if (!Incoming.TryGetValue(target, out var list))
                        {
                            list = new List<IncomingRef>();
                            Incoming[target] = list;
                        }

                        list.Add(new IncomingRef(sourceName, row, field.Label));
                    }
                }
            }
        }
    }

    private List<LinkField> GetLinkFields(string paramName)
    {
        if (LinkFields.TryGetValue(paramName, out var cached))
            return cached;

        var fields = new List<LinkField>();
        LinkFields[paramName] = fields;

        if (!Bank.Params.TryGetValue(paramName, out var param) || param.AppliedParamdef == null)
            return fields;

        var meta = Data.GetParamMeta(param.AppliedParamdef);

        if (meta == null)
            return fields;

        foreach (var column in param.Columns)
        {
            if (!IsIntegral(column))
                continue;

            var fieldMeta = Data.GetParamFieldMeta(meta, column.Def);

            if (fieldMeta == null)
                continue;

            var field = new LinkField
            {
                Column = column,
                Meta = fieldMeta,
                Label = GetFieldLabel(param, column)
            };

            if (fieldMeta.RefTypes != null)
            {
                foreach (var pref in fieldMeta.RefTypes)
                {
                    if (pref.ParamName == null || !Bank.Params.ContainsKey(pref.ParamName))
                        continue;

                    field.ParamRefs.Add((pref, FindColumn(param, pref.ConditionField)));
                }
            }

            if (fieldMeta.FmgRef != null)
            {
                foreach (var fref in fieldMeta.FmgRef)
                {
                    if (string.IsNullOrEmpty(fref.fmg))
                        continue;

                    field.TextRefs.Add((fref, FindColumn(param, fref.conditionField)));
                }
            }

            if (fieldMeta.ShowCharacterEnumList)
                field.AliasType = ProjectAliasType.Characters;
            else if (fieldMeta.ShowFlagEnumList)
            {
                field.AliasType = ProjectAliasType.EventFlags;
                SetAliasCondition(field, param, fieldMeta.FlagAliasEnum_ConditionalField, fieldMeta.FlagAliasEnum_ConditionalValue);
            }
            else if (fieldMeta.ShowMovieEnumList)
            {
                field.AliasType = ProjectAliasType.Movies;
                SetAliasCondition(field, param, fieldMeta.MovieAliasEnum_ConditionalField, fieldMeta.MovieAliasEnum_ConditionalValue);
            }
            else if (fieldMeta.ShowCutsceneEnumList)
                field.AliasType = ProjectAliasType.Cutscenes;
            else if (fieldMeta.ShowParticleEnumList)
                field.AliasType = ProjectAliasType.Particles;
            else if (fieldMeta.ShowSoundEnumList)
                field.AliasType = ProjectAliasType.Sounds;

            if (field.ParamRefs.Count > 0 || field.TextRefs.Count > 0 || field.AliasType != null)
                fields.Add(field);
        }

        return fields;
    }

    private static void SetAliasCondition(LinkField field, Param param, string conditionField, string conditionValue)
    {
        if (string.IsNullOrEmpty(conditionField))
            return;

        if (!int.TryParse(conditionValue, out var value))
            return;

        field.AliasCondition = FindColumn(param, conditionField);
        field.AliasConditionValue = value;
    }

    private Dictionary<string, string> GetAliases(ProjectAliasType type)
    {
        if (AliasCache.TryGetValue(type, out var cached))
            return cached;

        var map = new Dictionary<string, string>();
        AliasCache[type] = map;

        if (View.Project.Handler.ProjectData.Aliases.TryGetValue(type, out var entries))
        {
            foreach (var entry in entries)
            {
                if (entry?.ID == null)
                    continue;

                // Character alias IDs carry the model prefix (c1234); the field holds the number
                var key = type == ProjectAliasType.Characters ? entry.ID.Replace("c", "") : entry.ID;

                // Numeric keys are normalised so "0100" and "100" match
                if (long.TryParse(key, out var number))
                    key = $"{number}";

                map.TryAdd(key, entry.Name);
            }
        }

        return map;
    }

    /// <summary>
    /// Finds the row a param reference points at, following the same offset rules as the field window:
    /// the reference's own offset, then the target param's FixedOffset and OffsetSize (e.g. weapon upgrade levels).
    /// </summary>
    private (Param.Row Row, string Hint) ResolveParamRef(ParamRef pref, int value)
    {
        if (!Bank.Params.TryGetValue(pref.ParamName, out var targetParam))
            return (null, "");

        var meta = Data.GetParamMeta(targetParam.AppliedParamdef);
        var index = GetIdIndex(pref.ParamName, targetParam);

        var id = value + pref.Offset;

        if (meta != null && meta.Row0Dummy && id == 0)
            return (null, "");

        if (index.TryGetValue(id, out var row))
            return (row, "");

        if (meta == null || id <= 0)
            return (null, "");

        var hint = "";

        if (meta.FixedOffset != 0)
            id = value + meta.FixedOffset;

        if (meta.OffsetSize > 0)
        {
            var remainder = value % meta.OffsetSize;
            id -= id % meta.OffsetSize;

            if (remainder != 0)
                hint = $" +{remainder}";
        }

        if (index.TryGetValue(id, out row))
            return (row, hint);

        return (null, "");
    }

    private Dictionary<int, Param.Row> GetIdIndex(string paramName, Param param)
    {
        if (IdIndex.TryGetValue(paramName, out var index))
            return index;

        index = new Dictionary<int, Param.Row>();

        foreach (var row in param.Rows)
            index.TryAdd(row.ID, row);

        IdIndex[paramName] = index;

        return index;
    }
    #endregion

    #region Utilities
    // Param type -> field -> annotation name
    private readonly Dictionary<string, Dictionary<string, string>> FieldLabelCache = new();

    /// <summary>The field's name as the field window shows it: its annotation name, else its meta AltName, else its internal name</summary>
    private string GetFieldLabel(Param param, Param.Column column)
    {
        var paramType = param.ParamType ?? param.AppliedParamdef?.ParamType ?? "";

        if (!FieldLabelCache.TryGetValue(paramType, out var labels))
        {
            labels = new Dictionary<string, string>();

            var annotations = Data.GetParamAnnotations(paramType);

            if (annotations?.Fields != null)
            {
                foreach (var entry in annotations.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Field) && !string.IsNullOrWhiteSpace(entry.Name))
                        labels.TryAdd(entry.Field, entry.Name.Trim());
                }
            }

            FieldLabelCache[paramType] = labels;
        }

        if (labels.TryGetValue(column.Def.InternalName, out var label))
            return label;

        var meta = param.AppliedParamdef != null ? Data.GetParamMeta(param.AppliedParamdef) : null;
        var altName = meta != null ? Data.GetParamFieldMeta(meta, column.Def)?.AltName : null;

        return string.IsNullOrWhiteSpace(altName) ? column.Def.InternalName : altName;
    }

    private static Param.Column FindColumn(Param param, string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return null;

        return param.Columns.FirstOrDefault(c => c.Def.InternalName == fieldName);
    }

    /// <summary>A missing condition always passes; otherwise the condition field must hold the value.</summary>
    private static bool ConditionMet(Param.Column condition, Param.Row row, int expected)
    {
        if (condition == null)
            return true;

        if (!TryGetInt(condition, row, out var value))
            return true;

        return value == expected;
    }

    private static bool IsIntegral(Param.Column column)
    {
        var type = column.ValueType;

        return type == typeof(int) || type == typeof(uint)
            || type == typeof(short) || type == typeof(ushort)
            || type == typeof(sbyte) || type == typeof(byte);
    }

    private static bool TryGetInt(Param.Column column, Param.Row row, out int value)
    {
        value = 0;

        if (column == null || !IsIntegral(column))
            return false;

        var raw = column.GetValue(row);

        switch (raw)
        {
            case int i: value = i; return true;
            case uint u: value = unchecked((int)u); return true;
            case short s: value = s; return true;
            case ushort us: value = us; return true;
            case sbyte sb: value = sb; return true;
            case byte b: value = b; return true;
        }

        return false;
    }

    // Markup in FMG text: <img .../>, <font color="...">, </font>, <br> etc.
    private static readonly Regex MarkupTag = new(@"<[^<>]*>", RegexOptions.Compiled);

    /// <summary>
    /// One line, trimmed, without text markup (image tags, font colours) or HTML entities;
    /// null if there's nothing usable (empty or a text placeholder).
    /// </summary>
    private static string CleanName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Tags go first, so an entity-encoded "&lt;" in the text isn't mistaken for a tag
        if (text.IndexOf('<') >= 0)
            text = MarkupTag.Replace(text, "");

        if (text.IndexOf('&') >= 0)
            text = WebUtility.HtmlDecode(text);

        var sb = new StringBuilder(text.Length);
        var lastWasSpace = false;

        foreach (var ch in text)
        {
            var c = char.IsWhiteSpace(ch) ? ' ' : ch;

            if (c == ' ')
            {
                if (lastWasSpace)
                    continue;

                lastWasSpace = true;
            }
            else
            {
                lastWasSpace = false;
            }

            sb.Append(c);
        }

        var result = sb.ToString().Trim();

        if (result.Length == 0)
            return null;

        if (result == "%null%" || result == "[ERROR]" || result == "Unnamed Row")
            return null;

        return result;
    }
    #endregion

    #region Apply
    /// <summary>Renames the rows as one undoable step. Returns the number of rows renamed.</summary>
    public static int Apply(ParamEditorScreen editor, IEnumerable<GeneratedRowName> results)
    {
        var prop = typeof(Param.Row).GetProperty(nameof(Param.Row.Name));

        var actions = results
            .Where(e => e.Row != null && e.Row.Name != e.NewName)
            .Select(e => (EditorAction)new PropertiesChangedAction(prop, -1, e.Row, e.NewName))
            .ToList();

        if (actions.Count == 0)
            return 0;

        // Refused while a save is running
        if (!editor.ActionManager.ExecuteAction(new CompoundAction(actions)))
            return 0;

        editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        return actions.Count;
    }
    #endregion
}
