using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.ParamEditor;

public enum WeaponGeneratorRuleOp
{
    /// <summary>
    /// The field is set to the value
    /// </summary>
    Set = 0,

    /// <summary>
    /// The value is added to the base row's value
    /// </summary>
    Add = 1,

    /// <summary>
    /// The base row's value is multiplied by the value (rounded for integer fields)
    /// </summary>
    Multiply = 2,

    /// <summary>
    /// The field becomes the sum of the base row's source fields, multiplied by the value (e.g. Fire damage from
    /// the base's physical damage)
    /// </summary>
    ScaleFrom = 3
}

/// <summary>
/// The value a rule uses for base rows whose group field has this value
/// </summary>
public class WeaponGeneratorRuleGroup
{
    /// <summary>
    /// The group field's value, formatted in the invariant culture
    /// </summary>
    public string Key { get; set; } = "";

    public string Value { get; set; } = "";
}

public enum WeaponGeneratorExistingMode
{
    /// <summary>
    /// A row whose ID is already taken is not generated
    /// </summary>
    Skip = 0,

    /// <summary>
    /// The existing row is replaced
    /// </summary>
    Overwrite = 1
}

public class WeaponGeneratorRule
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The field's internal name
    /// </summary>
    public string Field { get; set; } = "";

    public WeaponGeneratorRuleOp Op { get; set; } = WeaponGeneratorRuleOp.Set;

    /// <summary>
    /// Kept as text so one rule type fits integer and float fields; parsed in the field's type when applied.
    /// With a group field, this is the value for base rows whose group isn't listed.
    /// </summary>
    public string Value { get; set; } = "0";

    /// <summary>
    /// Scale From: the base row's fields that are summed, separated by "+"
    /// </summary>
    public string SourceFields { get; set; } = "";

    /// <summary>
    /// Optional: a field of the base row (e.g. the weapon type) that picks the value from <see cref="Groups"/>
    /// </summary>
    public string GroupField { get; set; } = "";

    public List<WeaponGeneratorRuleGroup> Groups { get; set; } = new();

    /// <summary>
    /// Round the result to the nearest whole number. Whole-number fields are always rounded; this is for decimal
    /// fields that hold whole numbers, such as the scaling stats.
    /// </summary>
    public bool Round { get; set; } = false;

    /// <summary>
    /// Learned rules only: the share of the project's existing infused rows the rule reproduces (0 to 1)
    /// </summary>
    public double? Accuracy { get; set; }
}

public class WeaponGeneratorInfusion
{
    public bool Enabled { get; set; } = true;

    public string Name { get; set; } = "";

    /// <summary>
    /// Added to the base row's ID (and to each upgrade row's ID)
    /// </summary>
    public int IdOffset { get; set; } = 100;

    /// <summary>
    /// The new row's name. {name} is the source row's name, {infusion} the infusion's name.
    /// </summary>
    public string RowNameTemplate { get; set; } = "{infusion} {name}";

    /// <summary>
    /// The new title FMG entry. {name} is the source row's title text, {infusion} the infusion's name.
    /// </summary>
    public string FmgTitleTemplate { get; set; } = "{infusion} {name}";

    public List<WeaponGeneratorRule> Rules { get; set; } = new();
}

public class WeaponGeneratorStore
{
    public string ParamName { get; set; } = "EquipParamWeapon";

    /// <summary>
    /// The ID range one weapon's rows (its infusions and upgrade rows) take: 10000 in ER and DS3, 1000 in DS1.
    /// Only used by Learn, to tell base rows from infused ones.
    /// </summary>
    public int IdBlock { get; set; } = 10000;

    /// <summary>
    /// Also generate the rows at base +1 … +99 that exist (per-level upgrade rows, as in DS1)
    /// </summary>
    public bool IncludeUpgradeRows { get; set; } = true;

    public WeaponGeneratorExistingMode ExistingMode { get; set; } = WeaponGeneratorExistingMode.Skip;

    public bool GenerateFmg { get; set; } = true;

    /// <summary>
    /// Copy the source's other text slots (summary, description) to the new entry
    /// </summary>
    public bool CopyOtherTextSlots { get; set; } = true;

    /// <summary>
    /// Replace the text of FMG entries that already exist for a generated ID
    /// </summary>
    public bool OverwriteFmg { get; set; } = false;

    /// <summary>
    /// Learn also tries rules for the stat fields (damage and scaling), which vanilla tunes per weapon
    /// </summary>
    public bool LearnStatRules { get; set; } = false;

    /// <summary>
    /// The stat fields Learn tries: comma-separated names, * as a wildcard, ! to leave a match out
    /// </summary>
    public string StatFields { get; set; } = "attackBase*, correct*, !correctType*";

    /// <summary>
    /// The fields Learn may group stat rules by (the first one that exists and fits best is used)
    /// </summary>
    public string GroupFields { get; set; } = "wepType, wepmotionCategory, weaponCategory";

    public List<WeaponGeneratorInfusion> Infusions { get; set; } = new();
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    UseStringEnumConverter = true)
]
[JsonSerializable(typeof(WeaponGeneratorStore))]
public partial class WeaponGeneratorSerializationContext
    : JsonSerializerContext
{ }
