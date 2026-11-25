using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Per-param naming rules: params whose rows the general sources name poorly get a rule of their own,
/// and are named by it alone. Rules are keyed by the param's name and read fields by name, so a rule
/// does nothing in a game whose param lacks the field (no per-game switches).
/// </summary>
public partial class RowNameGeneratorCore
{
    private delegate GeneratedRowName Rule(string paramName, Param.Row row);

    private const string ListSeparator = ", ";

    private static readonly string[] PlayerBehaviors = { PlayerBehaviorParam };
    private static readonly string[] NpcBehaviors = { NpcBehaviorParam };
    private static readonly string[] AllBehaviors = { NpcBehaviorParam, PlayerBehaviorParam };
    private static readonly string[] Bullets = { "Bullet" };
    private static readonly string[] Npcs = { "NpcParam" };

    private Rule GetRule(string paramName)
    {
        switch (paramName)
        {
            // Attacks: named after the behaviors that use them, else the bullets.
            // The behavior side decides Pc / Npc, as refId points at both params.
            case "AtkParam_Pc":
                return (p, r) => FromIncoming(p, r, new[] { PlayerBehaviors, Bullets });
            case "AtkParam_Npc":
                return (p, r) => FromIncoming(p, r, new[] { NpcBehaviors, Bullets });
            case "AtkParam":
                return (p, r) => FromIncoming(p, r, new[] { AllBehaviors, Bullets });

            // Behaviors: the time act events that use them and the behavior state of the animation
            case NpcBehaviorParam:
            case PlayerBehaviorParam:
                return (p, r) => Options.UseAnimations ? FromAnimation(p, r) : null;

            // Bullets: the behaviors that fire them, else the bullets that spawn them
            case "Bullet":
                return (p, r) => FromIncoming(p, r, new[] { AllBehaviors, Bullets });

            case "AttackElementCorrectParam":
                return FromEnabledScaling;

            case "EquipMtrlSetParam":
                return (p, r) => FromItemList(p, r, "materialId", "itemNum");

            case "GameAreaParam":
                return (p, r) => FromField(p, r, "defeatBossFlagId", ProjectAliasType.EventFlags);

            case "Magic":
                return (p, r) => FromSameId(p, r, "EquipParamGoods");

            case "NpcParam":
                return (p, r) => FromField(p, r, "nameId") ?? FromCharacterId(p, r);

            case "NpcThinkParam":
                return (p, r) => FromSameId(p, r, "NpcParam") ?? FromCharacterId(p, r);

            case "ResistCorrectParam":
                return (p, r) => FromFieldUsage(p, r, Npcs);

            case "ShopLineupParam":
                return (p, r) => FromField(p, r, "equipId");

            case "SwordArtsParam":
                return (p, r) => FromField(p, r, "textId");
        }

        switch (paramName)
        {
            case "CharaInitParam":
                return (p, r) => FromIncoming(p, r, new[] { new[] { "BaseChrSelectMenuParam" } });

            case "AssetEnvironmentGeometryParam":
                return FromAssetId;

            // The map the row ID starts with
            case "BuddyStoneParam":
            case "CutsceneGparamTimeParam":
            case "CutsceneGparamWeatherParam":
            case "CutsceneMapIdParam":
            case "CutSceneTextureLoadParam":
            case "MapDefaultInfoParam":
            case "MapGdRegionInfoParam":
            case "PlayRegionParam":
            case "RollingObjLotParam":
                return (p, r) => FromMapId(p, r, r.ID, null);

            case "MapGridCreateHeightDetailLimitInfo":
                return (p, r) => FromMapField(p, r, "currentGridBlockId");

            // The character the row ID starts with
            case "ChrModelParam":
                return (p, r) => FromCharacterNumber(p, r, FirstDigits(r.ID, 4));

            // Negative IDs show the text of the positive ID
            case "MapNameTexParam":
                return (p, r) => FromField(p, r, "mapNameId", absoluteValue: true);

            case "RideParam":
                return (p, r) => FromCharacterPair(p, r, "atkChrId", "defChrId");

            case "ThrowParam":
                return (p, r) => FromCharacterPair(p, r, "AtkChrId", "DefChrId");
        }

        // ER splits item lots (ItemLotParam_map, ItemLotParam_enemy)
        if (paramName.StartsWith("ItemLotParam", StringComparison.Ordinal))
            return FromItemLot;

        return null;
    }

    #region Rules
    /// <summary>
    /// What one field points at (text, row or alias), e.g. a shop line's equipId or a skill's textId.
    /// With <paramref name="aliasType"/>, the field is read as that alias even if its meta doesn't say so.
    /// </summary>
    private GeneratedRowName FromField(string paramName, Param.Row row, string fieldName, ProjectAliasType? aliasType = null,
        bool absoluteValue = false)
    {
        var field = GetLinkField(paramName, fieldName, aliasType);

        if (field == null)
            return null;

        string name;
        string detail;

        if (absoluteValue)
        {
            // Text lookups of a negative ID use the positive one (TextFinder takes the absolute value itself)
            if (!TryGetInt(field.Column, row, out var value) || value == 0 ||
                !TryResolveTextRef(field, row, Math.Abs(value), out name, out detail))
                return null;
        }
        else if (!TryResolveField(field, row, out name, out detail))
        {
            return null;
        }

        return MakeResult(paramName, row, name, field.Label, RowNameGenSource.ParamRule, detail);
    }

    /// <summary>
    /// Item lots: the items listed, optionally after the map the row ID starts with ("Stormveil Castle - Golden Rune [1]")
    /// </summary>
    private GeneratedRowName FromItemLot(string paramName, Param.Row row)
    {
        var result = FromItemList(paramName, row, "lotItemId", "lotItemNum");

        if (result == null || !Options.ItemLotMapPrefix || CountDigits(row.ID) < 8)
            return result;

        if (GetMapName(row.ID) is { } map)
        {
            result.NewName = $"{map} - {result.NewName}";

            if (result.NewName.Length > MaxNameLength)
                result.NewName = result.NewName[..MaxNameLength].TrimEnd();
        }

        return result;
    }

    /// <summary>The asset the row ID is (1003 -> aeg001_003), by its alias; no alias, no name</summary>
    private GeneratedRowName FromAssetId(string paramName, Param.Row row)
    {
        if (row.ID < 0)
            return null;

        var asset = $"aeg{row.ID / 1000:D3}_{row.ID % 1000:D3}";

        if (!GetAliases(ProjectAliasType.Assets).TryGetValue(asset, out var alias) || CleanName(alias) is not { } name)
            return null;

        return MakeResult(paramName, row, name, null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Asset_Id", asset));
    }

    /// <summary>The map a number names, by its alias (see <see cref="GetMapName"/>)</summary>
    private GeneratedRowName FromMapId(string paramName, Param.Row row, long number, string fieldLabel)
    {
        var map = GetMapName(number, out var mapId);

        if (map == null)
            return null;

        return MakeResult(paramName, row, map, fieldLabel, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Map_Id", mapId));
    }

    private GeneratedRowName FromMapField(string paramName, Param.Row row, string fieldName)
    {
        if (!Bank.Params.TryGetValue(paramName, out var param) || FindColumn(param, fieldName) is not { } column)
            return null;

        if (!TryGetInt(column, row, out var value) || value <= 0)
            return null;

        // The field is unsigned: read it back as such
        return FromMapId(paramName, row, unchecked((uint)value), GetFieldLabel(param, column));
    }

    private GeneratedRowName FromCharacterNumber(string paramName, Param.Row row, long number)
    {
        if (GetCharacterAlias(number, out var character) is not { } name)
            return null;

        return MakeResult(paramName, row, name, null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Character_Id", character));
    }

    /// <summary>Two character fields, e.g. a throw's attacker and defender: "Margit - Player"</summary>
    private GeneratedRowName FromCharacterPair(string paramName, Param.Row row, string firstField, string secondField)
    {
        if (!Bank.Params.TryGetValue(paramName, out var param))
            return null;

        var first = FindColumn(param, firstField);
        var second = FindColumn(param, secondField);

        if (first == null || second == null)
            return null;

        string Name(Param.Column column, out string character)
        {
            character = null;

            // 0 is the player; -1 is no character
            if (!TryGetInt(column, row, out var value) || value < 0)
                return null;

            return GetCharacterAlias(unchecked((uint)value), out character);
        }

        var firstName = Name(first, out var firstCharacter);
        var secondName = Name(second, out var secondCharacter);

        if (firstName == null && secondName == null)
            return null;

        var name = $"{firstName ?? "?"} - {secondName ?? "?"}";

        return MakeResult(paramName, row, name, null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Character_Pair", firstCharacter ?? "?", secondCharacter ?? "?"));
    }

    /// <summary>
    /// The row with the same ID in another param, by name or its own text (Magic = EquipParamGoods,
    /// NpcThinkParam = NpcParam)
    /// </summary>
    private GeneratedRowName FromSameId(string paramName, Param.Row row, string otherParam)
    {
        if (!Bank.Params.TryGetValue(otherParam, out var param))
            return null;

        if (!GetIdIndex(otherParam, param).TryGetValue(row.ID, out var other))
            return null;

        var name = CleanName(NameOf(other)) ?? GetRowText(otherParam, other.ID);

        if (name == null)
            return null;

        return MakeResult(paramName, row, name, otherParam, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Same_Id", otherParam, other.ID));
    }

    /// <summary>
    /// The character the row ID belongs to (NpcParam 31000000 -> c3100), by its alias
    /// </summary>
    private GeneratedRowName FromCharacterId(string paramName, Param.Row row)
    {
        var character = GetCharacterOfId(row.ID);

        if (character == null)
            return null;

        return MakeResult(paramName, row, GetCharacterName(character), null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Character_Id", character));
    }

    /// <summary>
    /// The defined entries of numbered item fields (lotItemId01..08 with lotItemNum01..08), e.g. "Golden Rune [1] x3, Smithing Stone [2]"
    /// </summary>
    private GeneratedRowName FromItemList(string paramName, Param.Row row, string idPrefix, string countPrefix)
    {
        var items = new List<string>();

        foreach (var (field, count) in GetNumberedFields(paramName, idPrefix, countPrefix))
        {
            if (!TryResolveField(field, row, out var name, out _))
                continue;

            if (count != null && TryGetInt(count, row, out var number) && number > 1)
                name = $"{name} x{number}";

            items.Add(name);
        }

        if (items.Count == 0)
            return null;

        return MakeResult(paramName, row, string.Join(ListSeparator, items), null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Item_List", items.Count));
    }

    /// <summary>
    /// The scaling flags that are on, grouped by damage type: "Physical: STR DEX, Fire: FAI".
    /// Fields are is{Stat}Correct_by{Type}; their annotation names ("Physical Scaling: ARC") give the words.
    /// </summary>
    private GeneratedRowName FromEnabledScaling(string paramName, Param.Row row)
    {
        var groups = new List<(string Type, List<string> Stats)>();

        foreach (var (column, type, stat) in GetScalingFields(paramName))
        {
            if (!TryGetInt(column, row, out var value) || value == 0)
                continue;

            var group = groups.FirstOrDefault(g => g.Type == type);

            if (group.Stats == null)
            {
                group = (type, new List<string>());
                groups.Add(group);
            }

            group.Stats.Add(stat);
        }

        if (!GetScalingFields(paramName).Any())
            return null;

        var name = groups.Count == 0
            ? LOC.Get("PARAM_RowNameGen_Name_No_Scaling")
            : string.Join(ListSeparator, groups.Select(g => $"{g.Type}: {string.Join(" ", g.Stats)}"));

        return MakeResult(paramName, row, name, null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Scaling"));
    }

    /// <summary>
    /// The fields of the given params that use the row, e.g. a ResistCorrectParam row used only by
    /// NpcParam resistCorrectId_curse is "Death Blight". Named by the fields, not the referencing rows.
    /// </summary>
    private GeneratedRowName FromFieldUsage(string paramName, Param.Row row, string[] sourceParams)
    {
        if (!Incoming.TryGetValue(row, out var refs))
            return null;

        var labels = refs
            .Where(r => sourceParams.Contains(r.Param))
            .GroupBy(r => r.FieldLabel)
            .OrderByDescending(g => g.Count())
            .Select(g => ShortFieldLabel(g.Key))
            .Distinct()
            .ToList();

        if (labels.Count == 0)
            return null;

        return MakeResult(paramName, row, string.Join(ListSeparator, labels), null, RowNameGenSource.ParamRule,
            LOC.Get("PARAM_RowNameGen_Detail_Field_Usage", string.Join(ListSeparator, sourceParams), refs.Count));
    }
    #endregion

    #region Rule Fields
    // Param -> (id prefix, count prefix) -> numbered field pairs
    private readonly Dictionary<(string, string, string), List<(LinkField Field, Param.Column Count)>> NumberedFieldCache = new();

    private List<(LinkField Field, Param.Column Count)> GetNumberedFields(string paramName, string idPrefix, string countPrefix)
    {
        var key = (paramName, idPrefix, countPrefix);

        if (NumberedFieldCache.TryGetValue(key, out var cached))
            return cached;

        var list = new List<(LinkField, Param.Column)>();
        NumberedFieldCache[key] = list;

        if (!Bank.Params.TryGetValue(paramName, out var param))
            return list;

        var pattern = new Regex($"^{Regex.Escape(idPrefix)}(\\d+)$");

        foreach (var field in GetLinkFields(paramName))
        {
            var match = pattern.Match(field.Column.Def.InternalName);

            if (!match.Success)
                continue;

            list.Add((field, FindColumn(param, countPrefix + match.Groups[1].Value)));
        }

        return list;
    }

    private readonly Dictionary<string, List<(Param.Column Column, string Type, string Stat)>> ScalingFieldCache = new();

    private static readonly Regex ScalingFieldPattern = new(@"^is(\w+?)Correct_by(\w+)$");

    private List<(Param.Column Column, string Type, string Stat)> GetScalingFields(string paramName)
    {
        if (ScalingFieldCache.TryGetValue(paramName, out var cached))
            return cached;

        var list = new List<(Param.Column, string, string)>();
        ScalingFieldCache[paramName] = list;

        if (!Bank.Params.TryGetValue(paramName, out var param))
            return list;

        foreach (var column in param.Columns)
        {
            var match = ScalingFieldPattern.Match(column.Def.InternalName);

            if (!match.Success)
                continue;

            var type = match.Groups[2].Value;
            var stat = match.Groups[1].Value;

            // "Physical Scaling: ARC" -> Physical, ARC
            var label = GetFieldLabel(param, column);
            var separator = label.LastIndexOf(':');

            if (separator > 0 && separator < label.Length - 1)
            {
                type = label[..separator].Trim();
                stat = label[(separator + 1)..].Trim();

                if (type.EndsWith(" Scaling", StringComparison.OrdinalIgnoreCase))
                    type = type[..^" Scaling".Length];
            }

            list.Add((column, type, stat));
        }

        return list;
    }

    /// <summary>
    /// A field of the param by name, as a link field. With an alias type, a field whose meta has no alias
    /// is still read as one.
    /// </summary>
    private LinkField GetLinkField(string paramName, string fieldName, ProjectAliasType? aliasType)
    {
        var field = GetLinkFields(paramName).FirstOrDefault(f => f.Column.Def.InternalName == fieldName);

        if (field != null)
        {
            if (aliasType == null || field.AliasType != null)
                return field;

            // A copy, so the general alias source keeps reading the field as its meta says
            return new LinkField
            {
                Column = field.Column,
                Label = field.Label,
                Meta = field.Meta,
                ParamRefs = field.ParamRefs,
                TextRefs = field.TextRefs,
                AliasType = aliasType
            };
        }

        if (aliasType == null || !Bank.Params.TryGetValue(paramName, out var param))
            return null;

        var column = FindColumn(param, fieldName);

        if (column == null || !IsIntegral(column))
            return null;

        return new LinkField
        {
            Column = column,
            Label = GetFieldLabel(param, column),
            AliasType = aliasType
        };
    }

    // "mAA_BB_CC_DD" and "mAA_BB" -> map alias name
    private Dictionary<string, string> MapNames;

    private string GetMapName(long number) => GetMapName(number, out _);

    /// <summary>
    /// The alias of the map a number names. With 8 or more digits, the first 8 are tried as a whole map ID
    /// (60423600 -> m60_42_36_00); then the first 4 digits name the area (10000100 -> m10_00, preferring m10_00_00_00).
    /// </summary>
    private string GetMapName(long number, out string mapId)
    {
        mapId = null;

        if (number < 1000)
            return null;

        if (MapNames == null)
        {
            MapNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (View.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapNames, out var entries))
            {
                // Area keys prefer the _00_00 map, so add those first
                foreach (var entry in entries.OrderBy(e => e.ID?.EndsWith("_00_00") == true ? 0 : 1))
                {
                    if (entry?.ID == null || entry.ID.Length < 6 || CleanName(entry.Name) is not { } name)
                        continue;

                    MapNames.TryAdd(entry.ID, name);
                    MapNames.TryAdd(entry.ID[..6], name);
                }
            }
        }

        var digits = number.ToString();

        if (digits.Length >= 8)
        {
            var full = $"m{digits[..2]}_{digits[2..4]}_{digits[4..6]}_{digits[6..8]}";

            if (MapNames.TryGetValue(full, out var exact))
            {
                mapId = full;
                return exact;
            }
        }

        var area = $"m{digits[..2]}_{digits[2..4]}";

        if (MapNames.TryGetValue(area, out var areaName))
        {
            mapId = area;
            return areaName;
        }

        return null;
    }

    /// <summary>
    /// The alias of a character number (0 = c0000). A longer number is shortened to 4 digits;
    /// a variant without an alias falls back to its base (c3101 -> c3100 -> c3100).
    /// </summary>
    private string GetCharacterAlias(long number, out string character)
    {
        character = null;

        if (number < 0)
            return null;

        if (number > 9999)
            number = FirstDigits(number, 4);

        var aliases = GetAliases(ProjectAliasType.Characters);

        foreach (var candidate in new[] { number, number / 10 * 10, number / 100 * 100 })
        {
            if (aliases.TryGetValue($"{candidate}", out var alias) && CleanName(alias) is { } name)
            {
                character = $"c{candidate:D4}";
                return name;
            }
        }

        return null;
    }

    private static int CountDigits(long number) => Math.Abs(number).ToString().Length;

    /// <summary>The first digits of a number (31000000, 4 -> 3100); a number with fewer digits as it is</summary>
    private static long FirstDigits(long number, int count)
    {
        var digits = Math.Abs(number).ToString();

        return digits.Length <= count ? Math.Abs(number) : long.Parse(digits[..count]);
    }

    /// <summary>"Resist Correction - Death Blight" -> "Death Blight"; a label without a separator as it is</summary>
    private static string ShortFieldLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
            return label;

        foreach (var separator in new[] { " - ", ": " })
        {
            var index = label.LastIndexOf(separator, StringComparison.Ordinal);

            if (index > 0 && index + separator.Length < label.Length)
                return label[(index + separator.Length)..].Trim();
        }

        return label;
    }

    /// <summary>
    /// The character a param row ID belongs to (31000000 -> c3100, DS3 120000 -> c1200), with its variant falling
    /// back to the base, checked against the characters that have a time act or an alias
    /// </summary>
    private string GetCharacterOfId(int id)
    {
        var aliases = GetAliases(ProjectAliasType.Characters);

        foreach (var scale in new[] { 10000, 1000, 100 })
        {
            var number = id / scale;

            if (number < 1000 || number > 9999)
                continue;

            foreach (var candidate in new[] { number, number / 10 * 10, number / 100 * 100 })
            {
                var name = $"c{candidate:D4}";

                if (aliases.ContainsKey($"{candidate}") || HasCharacterTimeAct(name))
                    return name;
            }
        }

        return null;
    }
    #endregion
}
