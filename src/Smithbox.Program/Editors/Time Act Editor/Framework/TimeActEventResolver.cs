#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

public enum TimeActVisualKind
{
    Effect,
    Sound,
    Attack,
    Throw,
    Bullet
}

/// <summary>
/// One shape an event displays. A negative dummy poly stands for the center of the body.
/// </summary>
/// <param name="Dummy">The dummy poly it sits on.</param>
/// <param name="EndDummy">The other end of a capsule, or -1 for a sphere.</param>
/// <param name="Radius">The radius in meters.</param>
/// <param name="WeaponPart">The index of the character part holding the dummy polys, or -1 for the body.</param>
public sealed record TimeActVisualShape(int Dummy, int EndDummy, float Radius, int WeaponPart);

/// <summary>
/// What an event displays in the viewport.
/// </summary>
/// <param name="Kind">What the event is.</param>
/// <param name="Shapes">The shapes to draw. Empty if the event couldn't be resolved.</param>
/// <param name="Status">Explains how the event was resolved, or why it couldn't be.</param>
/// <param name="ThrowRanges">For a throw, the reaches its ThrowParam rows give, if any were found.</param>
public sealed record TimeActEventVisual(TimeActVisualKind Kind, IReadOnlyList<TimeActVisualShape> Shapes, string Status,
    IReadOnlyList<TimeActThrowRange>? ThrowRanges = null);

/// <summary>
/// Where a throw can catch the other character, from its ThrowParam row: within Distance of the base, within HalfAngle
/// degrees of the way the thrower faces, and between Lower below and Upper above it.
/// </summary>
/// <param name="BaseDummy">The dummy poly the range is measured from (judgeRangeBasePosDmyId1), or -1 for the character's origin.</param>
public sealed record TimeActThrowRange(int RowId, float Distance, float HalfAngle, float Upper, float Lower, int BaseDummy);

/// <summary>
/// Works out what the events of an animation look like in the viewport. Attacks go from the event to a
/// BehaviorParam row, which is found through the variation of whatever is attacking: a weapon for a
/// player, or the NpcParam row of an enemy. That row then points to an AtkParam or Bullet row.
/// </summary>
public class TimeActEventResolver
{
    private const int NpcBehaviorBase = 200000000;
    private const int PlayerBehaviorBase = 100000000;
    private const int VariationScale = 1000;

    private const int BehaviorRefAttack = 0;
    private const int BehaviorRefBullet = 1;

    private const int AttackHitCount = 16;

    // An AtkParam row is a throw when its throwFlag is this (DS3 names the field ThrowFlag; the lookup ignores case)
    private const string ThrowFlagField = "throwFlag";
    private const int ThrowFlagThrow = 2;

    // Dummy polys over 1000 name the model they are on
    private const int DummyGroupSize = 1000;
    private const int RightWeaponGroup = 10;
    private const int LeftWeaponGroup = 20;

    private const int SourceLeftHand = 2;

    // PCBehavior says which hand by flags: 1 and 16 are the right hand, 2 and 128 the left
    private const int PcBehaviorLeftHandFlags = 2 | 128;

    private const float EffectRadius = 0.1f;
    private const float SoundRadius = 0.18f;
    private const float FallbackBulletRadius = 0.1f;

    private static readonly int[] NpcRowScales = { 10000, 1000, 100 };

    private readonly ProjectEntry _project;

    private readonly Dictionary<string, List<int>> _npcVariations = new();

    public TimeActEventResolver(ProjectEntry project)
    {
        _project = project;
    }

    /// <summary>
    /// Forgets what was read from the params, for when they have been edited
    /// </summary>
    public void ClearParamCache()
    {
        _npcVariations.Clear();
    }

    /// <summary>
    /// Gets what an event displays, or null if it doesn't display anything.
    /// </summary>
    public TimeActEventVisual? Resolve(TAE.Event ev, TimeActContainer container)
    {
        // Without a template there is no telling what the parameters are
        if (ev.Parameters == null)
            return null;

        // The meta of the game says which events invoke a behavior, whatever their type is there
        switch (_project.Handler.TimeActData?.GetSimulationRole(ev) ?? TimeActSimulationRole.None)
        {
            case TimeActSimulationRole.Attack:
                return ResolveAttack(ev, container, TimeActVisualKind.Attack);
            case TimeActSimulationRole.Throw:
                return ResolveAttack(ev, container, TimeActVisualKind.Throw);
            case TimeActSimulationRole.Bullet:
                return ResolveBullet(ev, container);
        }

        if (IsEffectEvent(ev))
        {
            return ResolveMarker(ev, container, TimeActVisualKind.Effect, EffectRadius);
        }

        if (IsSoundEvent(ev))
        {
            return ResolveMarker(ev, container, TimeActVisualKind.Sound, SoundRadius);
        }

        return null;
    }

    // Effect and sound events are the ones with a parameter the meta tags with the Particles or Sounds alias.
    // They are told apart by that first, so renaming an event in a def doesn't change what it displays.
    private bool IsEffectEvent(TAE.Event ev)
    {
        if (_project.Handler.TimeActData?.HasAliasField(ev, ProjectAliasType.Particles) == true)
            return true;

        // Events whose effect isn't an alias, such as SpawnFFX_ThrowDirection, and defs without meta
        return (ev.TypeName ?? "").Contains("FFX", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSoundEvent(TAE.Event ev)
    {
        if (_project.Handler.TimeActData?.HasAliasField(ev, ProjectAliasType.Sounds) == true)
            return true;

        return (ev.TypeName ?? "").Contains("PlaySound", StringComparison.OrdinalIgnoreCase);
    }

    private TimeActEventVisual ResolveMarker(TAE.Event ev, TimeActContainer container, TimeActVisualKind kind, float radius)
    {
        var dummy = GetParameter(ev, "DummyPolyID") ?? GetParameter(ev, "DummyPolyBladeBaseID") ?? -1;
        var endDummy = GetParameter(ev, "DummyPolyBladeTipID") ?? -1;

        var part = GetWeaponPartOfDummy(container, dummy, -1);

        return new TimeActEventVisual(kind,
            new[] { new TimeActVisualShape(ToLocalDummy(container, dummy), ToLocalDummy(container, endDummy), radius, part) },
            LOC.Get(dummy >= 0 ? "TAE_EventDisplay_On_Dummy" : "TAE_EventDisplay_On_Body", dummy));
    }

    private TimeActEventVisual ResolveAttack(TAE.Event ev, TimeActContainer container, TimeActVisualKind kind)
    {
        if (!TryGetBehavior(ev, container, out Param.Row? behavior, out var weaponPart, out var failure))
        {
            return Failed(kind, failure);
        }

        if (CharacterComposition.GetValue(behavior!, "refType") != BehaviorRefAttack)
        {
            return Failed(kind, LOC.Get("TAE_EventDisplay_Not_An_Attack", behavior!.ID));
        }

        var attackId = CharacterComposition.GetValue(behavior!, "refId");

        if (GetRow(IsPlayer(container) ? "AtkParam_Pc" : "AtkParam_Npc", attackId) is not { } attack)
        {
            return Failed(kind, LOC.Get("TAE_EventDisplay_No_Attack_Row", attackId));
        }

        List<TimeActVisualShape> shapes = new();

        for (var i = 0; i < AttackHitCount; i++)
        {
            if (!CharacterComposition.HasField(attack, $"hit{i}_Radius"))
                break;

            var radius = CharacterComposition.GetFloat(attack, $"hit{i}_Radius");
            var dummy = CharacterComposition.GetValue(attack, $"hit{i}_DmyPoly1");
            var endDummy = CharacterComposition.GetValue(attack, $"hit{i}_DmyPoly2");

            if (radius <= 0 || dummy <= 0)
                continue;

            shapes.Add(new TimeActVisualShape(ToLocalDummy(container, dummy),
                endDummy > 0 ? ToLocalDummy(container, endDummy) : -1, radius,
                GetWeaponPartOfDummy(container, dummy, weaponPart)));
        }

        var status = LOC.Get("TAE_EventDisplay_Attack", behavior!.ID, attackId, shapes.Count);

        // An attack whose AtkParam has a throwFlag of 2 is a throw, whatever its event: it has the reach in which it
        // catches the other character, from the ThrowParam rows of its throwTypeId
        var isThrow = CharacterComposition.HasField(attack, ThrowFlagField) &&
                      CharacterComposition.GetValue(attack, ThrowFlagField) == ThrowFlagThrow;

        if (!isThrow)
            return new TimeActEventVisual(kind, shapes, status);

        var ranges = FindThrowRanges(attack, container);

        // TEMPORARY: throw reach diagnostics, to be removed once the wedge is confirmed in the app
        Smithbox.Log(this, $"[TAE Throw] {container.Name} event type {ev.Type} at {ev.StartTime:0.000}s: " +
            $"BehaviorParam {behavior.ID}, AtkParam {attackId}, throwFlag 2, throwTypeId " +
            $"{CharacterComposition.GetValue(attack, "throwTypeId")}, ThrowParam rows: " +
            (ranges.Count > 0 ? string.Join(", ", ranges.Select(r => $"{r.Range.RowId} (Dist {r.Range.Distance:0.00})")) : "none"));

        if (ranges.Count == 0)
        {
            return new TimeActEventVisual(TimeActVisualKind.Throw, shapes,
                $"{status}\n{LOC.Get("TAE_EventDisplay_Throw_No_Range", container.Name)}");
        }

        var lines = ranges.Select(r =>
            LOC.Get("TAE_EventDisplay_Throw_Range", r.Range.RowId, r.Range.Distance, r.Range.HalfAngle, r.Facing));

        return new TimeActEventVisual(TimeActVisualKind.Throw, shapes, $"{status}\n{string.Join("\n", lines)}",
            ranges.Select(r => r.Range).ToList());
    }

    // ThrowParam rows are numbered in a different way in each game, but always end in two digits that the AtkParam's
    // throwTypeId picks among the rows of a thrower (ER: 34201000 Blaidd grab #1, 34201010 grab #2 for throwTypeId 10)
    private const int ThrowVariantDigits = 100;

    /// <summary>
    /// The ThrowParam rows a throw attack of the character can start: those with the character as the thrower
    /// (AtkChrId) against no particular character (DefChrId 0), whose ID ends in the throwTypeId of the AtkParam row. A
    /// character can have several (a grab, a backstab, a riposte after a parry), for whichever state the other is in.
    /// Without a row ending in the offset, a throwTypeId of 0 takes all of the thrower's rows (DS1R's player rows are
    /// numbered 1 to 4). Also gives the facing the other character must have (DiffAngMin to DiffAngMax).
    /// </summary>
    private List<(TimeActThrowRange Range, string Facing)> FindThrowRanges(Param.Row attack, TimeActContainer container)
    {
        var ranges = new List<(TimeActThrowRange Range, string Facing)>();

        if (_project.Handler.ParamData?.PrimaryBank?.Params is not { } bank || !bank.TryGetValue("ThrowParam", out Param? throws))
            return ranges;

        var offset = CharacterComposition.HasField(attack, "throwTypeId") ? CharacterComposition.GetValue(attack, "throwTypeId") : 0;

        // The player is 0; a character variant (c2011) can share the rows of its base (c2010)
        var character = IsPlayer(container) ? 0
            : container.Name.Length == 5 && int.TryParse(container.Name.AsSpan(1), out var number) ? number : -1;

        if (character < 0)
            return ranges;

        foreach (var thrower in character == 0 ? new[] { 0 } : new[] { character, character / 10 * 10 })
        {
            var own = throws.Rows.Where(r => r.ID > 0 && CharacterComposition.GetValue(r, "AtkChrId") == thrower &&
                                             CharacterComposition.GetValue(r, "DefChrId") == 0).ToList();

            var picked = own.Where(r => r.ID % ThrowVariantDigits == offset % ThrowVariantDigits).ToList();

            if (picked.Count == 0 && offset == 0)
                picked = own;

            foreach (Param.Row row in picked)
            {
                if (ToThrowRange(row) is { } range)
                    ranges.Add(range);
            }

            if (ranges.Count > 0)
                break;
        }

        return ranges;
    }

    private static (TimeActThrowRange Range, string Facing)? ToThrowRange(Param.Row row)
    {
        if (CharacterComposition.GetFloat(row, "Dist") <= 0)
            return null;

        var baseDummy = CharacterComposition.HasField(row, "judgeRangeBasePosDmyId1")
            ? CharacterComposition.GetValue(row, "judgeRangeBasePosDmyId1")
            : -1;

        var range = new TimeActThrowRange(row.ID,
            CharacterComposition.GetFloat(row, "Dist"),
            CharacterComposition.GetFloat(row, "diffAngMyToDef"),
            CharacterComposition.GetFloat(row, "upperYRange"),
            CharacterComposition.GetFloat(row, "lowerYRange"),
            baseDummy);

        var facing = $"{CharacterComposition.GetFloat(row, "DiffAngMin"):0}..{CharacterComposition.GetFloat(row, "DiffAngMax"):0}";

        return (range, facing);
    }

    private TimeActEventVisual ResolveBullet(TAE.Event ev, TimeActContainer container)
    {
        if (!TryGetBehavior(ev, container, out Param.Row? behavior, out var weaponPart, out var failure))
        {
            return Failed(TimeActVisualKind.Bullet, failure);
        }

        if (CharacterComposition.GetValue(behavior!, "refType") != BehaviorRefBullet)
        {
            return Failed(TimeActVisualKind.Bullet, LOC.Get("TAE_EventDisplay_Not_A_Bullet", behavior!.ID));
        }

        var bulletId = CharacterComposition.GetValue(behavior!, "refId");

        // The param has a different name in some games
        if ((GetRow("Bullet", bulletId) ?? GetRow("BulletParam", bulletId)) is not { } bullet)
        {
            return Failed(TimeActVisualKind.Bullet, LOC.Get("TAE_EventDisplay_No_Bullet_Row", bulletId));
        }

        var radius = CharacterComposition.GetFloat(bullet, "hitRadius");

        if (radius <= 0)
        {
            var attackId = CharacterComposition.GetValue(bullet, "atkId_Bullet");
            var attack = GetRow(IsPlayer(container) ? "AtkParam_Pc" : "AtkParam_Npc", attackId);

            radius = attack != null ? CharacterComposition.GetFloat(attack, "hit0_Radius") : 0;
        }

        if (radius <= 0)
        {
            radius = FallbackBulletRadius;
        }

        var dummy = GetParameter(ev, "DummyPolyID") ?? -1;

        return new TimeActEventVisual(TimeActVisualKind.Bullet,
            new[]
            {
                new TimeActVisualShape(ToLocalDummy(container, dummy), -1, radius,
                    GetWeaponPartOfDummy(container, dummy, weaponPart))
            },
            LOC.Get("TAE_EventDisplay_Bullet", behavior!.ID, bulletId));
    }

    private static TimeActEventVisual Failed(TimeActVisualKind kind, string reason)
    {
        return new TimeActEventVisual(kind, Array.Empty<TimeActVisualShape>(), reason);
    }

    private bool TryGetBehavior(TAE.Event ev, TimeActContainer container, out Param.Row? behavior, out int weaponPart,
        out string failure)
    {
        behavior = null;
        weaponPart = -1;
        failure = "";

        if (_project.Handler.ParamData?.PrimaryBank?.Params == null)
        {
            failure = LOC.Get("TAE_EventDisplay_No_Params");
            return false;
        }

        if (GetParameter(ev, "BehaviorJudgeID") is not { } judge)
        {
            failure = LOC.Get("TAE_EventDisplay_No_Judge");
            return false;
        }

        // Common behaviors are shared by everything, so they use the ID as it is and come from the body
        if (IsCommonJudge(ev))
        {
            behavior = GetRow(IsPlayer(container) ? BehaviorParamPc : "BehaviorParam", judge);

            if (behavior == null)
            {
                failure = LOC.Get("TAE_EventDisplay_No_Behavior", judge);
            }
        }
        else if (IsPlayer(container))
        {
            weaponPart = GetSourceWeapon(ev, container);
            behavior = FindPlayerBehavior(container, weaponPart, judge, out failure);
        }
        else
        {
            behavior = FindNpcBehavior(container, judge, out failure);
        }

        return behavior != null;
    }

    /// <summary>
    /// Whether the meta says the judge ID of the event is a row ID as it is, rather than one added to the variation
    /// of the attacker
    /// </summary>
    private bool IsCommonJudge(TAE.Event ev)
    {
        return _project.Handler.TimeActData?.GetFieldMeta(ev, "BehaviorJudgeID")?.BehaviorRef == "Common";
    }

    private Param.Row? FindPlayerBehavior(TimeActContainer container, int weaponPart, int judge, out string failure)
    {
        failure = "";

        var weaponId = weaponPart >= 0 ? container.CharacterAppearance!.Parts[weaponPart].EquipParamId : -1;

        foreach (var id in GetPlayerBehaviorIds(weaponId, judge, false))
        {
            if (GetRow(BehaviorParamPc, id) is { } row)
            {
                return row;
            }
        }

        failure = LOC.Get(weaponPart < 0 ? "TAE_EventDisplay_No_Weapon" : "TAE_EventDisplay_No_Behavior", judge);
        return null;
    }

    /// <summary>
    /// The BehaviorParam_PC rows a judge ID may pick, best first: those of the variation of the weapon,
    /// then those of its base weapon, then the row without a variation and the row of the ID as it is,
    /// which attacks that don't involve a weapon, such as kicks, use.
    /// </summary>
    private IEnumerable<int> GetPlayerBehaviorIds(int weaponId, int judge, bool withoutWeapon)
    {
        var behaviorBase = PlayerBehaviorBase;
        var variationJudge = judge;

        // Elden Ring keeps a further category in the thousands of the ID
        if (_project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR && judge is >= 1000 and <= 9999)
        {
            behaviorBase = PlayerBehaviorBase * (judge / 1000);
            variationJudge = judge % 1000;
        }

        var hasWeapon = false;

        if (GetRow("EquipParamWeapon", weaponId) is { } weapon)
        {
            var variation = CharacterComposition.GetValue(weapon, "behaviorVariationId");

            // Upgraded weapons and infusions fall back on the base version of the weapon
            yield return behaviorBase + variation * VariationScale + variationJudge;
            yield return behaviorBase + variation / 100 * 100 * VariationScale + variationJudge;
            hasWeapon = true;
        }

        // The row without a variation is only a fallback for a weapon, unless every row is wanted
        if (hasWeapon || withoutWeapon)
        {
            yield return behaviorBase + variationJudge;
        }

        yield return judge;
    }

    private const string BehaviorParamPc = "BehaviorParam_PC";

    private Param.Row? FindNpcBehavior(TimeActContainer container, int judge, out string failure)
    {
        var variations = GetNpcVariations(container.Name);

        if (variations.Count == 0)
        {
            failure = LOC.Get("TAE_EventDisplay_No_Npc", container.Name);
            return null;
        }

        foreach (var variation in variations)
        {
            if (GetRow("BehaviorParam", NpcBehaviorBase + variation * VariationScale + judge) is { } row)
            {
                failure = "";
                return row;
            }
        }

        failure = LOC.Get("TAE_EventDisplay_No_Behavior", judge);
        return null;
    }

    /// <summary>
    /// The param a behavior judge ID of a character refers to: the player has a param of its own
    /// </summary>
    public static string GetBehaviorParamName(string characterName)
    {
        return CharacterComposition.IsComposite(characterName) ? BehaviorParamPc : "BehaviorParam";
    }

    /// <summary>
    /// Gets the BehaviorParam rows a judge ID can pick, best first, for the quick links of the properties.
    /// A variation judge ID is added to the variation of the attacker: the weapon the character holds in
    /// the viewport, if it holds one, for the player, and every NpcParam row of the character otherwise.
    /// A common one is the row ID as it is.
    /// </summary>
    /// <param name="ev">The event, whose Source picks the hand of the weapon.</param>
    /// <param name="characterName">The character the time act belongs to, such as c0000.</param>
    /// <param name="container">The character in the viewport, if it is the one the time act belongs to.</param>
    /// <param name="judge">The judge ID.</param>
    /// <param name="common">Whether the judge ID is the row ID as it is.</param>
    public List<Param.Row> GetBehaviorRows(TAE.Event ev, string characterName, TimeActContainer? container,
        long judge, bool common)
    {
        var rows = new List<Param.Row>();

        if (_project.Handler.ParamData?.PrimaryBank?.Params == null || judge is < int.MinValue or > int.MaxValue)
            return rows;

        var paramName = GetBehaviorParamName(characterName);
        var id = (int)judge;

        IEnumerable<int> ids;

        if (common)
        {
            ids = new[] { id };
        }
        else if (CharacterComposition.IsComposite(characterName))
        {
            var weaponPart = container != null ? GetSourceWeapon(ev, container) : -1;
            var weaponId = weaponPart >= 0 ? container!.CharacterAppearance!.Parts[weaponPart].EquipParamId : -1;

            ids = GetPlayerBehaviorIds(weaponId, id, true);
        }
        else
        {
            ids = GetNpcVariations(characterName).Select(v => NpcBehaviorBase + v * VariationScale + id);
        }

        foreach (var rowId in ids.Distinct())
        {
            if (GetRow(paramName, rowId) is { } row && !rows.Contains(row))
            {
                rows.Add(row);
            }
        }

        return rows;
    }

    /// <summary>
    /// Gets the variations of the NpcParam rows of a character. There are usually
    /// several rows, which mostly agree, and variant characters can share the rows of their base.
    /// </summary>
    private List<int> GetNpcVariations(string modelName)
    {
        if (_npcVariations.TryGetValue(modelName, out var variations))
        {
            return variations;
        }

        variations = new();

        if (modelName.Length == 5 && modelName[0] is 'c' or 'C' && int.TryParse(modelName.AsSpan(1), out var id) &&
            _project.Handler.ParamData?.PrimaryBank?.Params is { } bank && bank.TryGetValue("NpcParam", out Param? npcParam))
        {
            foreach (var character in new[] { id, id / 10 * 10, id / 100 * 100 })
            {
                foreach (var scale in NpcRowScales)
                {
                    variations = npcParam.Rows.Where(r => r.ID / scale == character)
                        .Select(r => CharacterComposition.GetValue(r, "behaviorVariationId"))
                        .Distinct()
                        .ToList();

                    if (variations.Count > 0)
                        break;
                }

                if (variations.Count > 0)
                    break;
            }
        }

        _npcVariations[modelName] = variations;

        return variations;
    }

    private static bool IsPlayer(TimeActContainer container)
    {
        return CharacterComposition.IsComposite(container.Name);
    }

    /// <summary>
    /// Gets the character part an attack comes from. Attacks use the right hand unless they say otherwise: by
    /// their Source, or the hand flags of their PCBehaviorType.
    /// </summary>
    private static int GetSourceWeapon(TAE.Event ev, TimeActContainer container)
    {
        var useLeft = GetParameter(ev, "PCBehaviorType") is { } pcBehavior
            ? (pcBehavior & PcBehaviorLeftHandFlags) != 0
            : GetParameter(ev, "Source") == SourceLeftHand;

        return FindWeapon(container, useLeft);
    }

    private static int FindWeapon(TimeActContainer container, bool leftHand)
    {
        var parts = container.CharacterAppearance?.Parts;

        if (parts == null)
            return -1;

        for (var i = 0; i < parts.Count; i++)
        {
            if (parts[i].AttachNode != null && CharacterComposition.IsLeftHand(parts[i]) == leftHand)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Gets the character part that a dummy poly is on. The group of the ID says so
    /// when it matters, and otherwise it's on whatever the attack came from.
    /// </summary>
    private static int GetWeaponPartOfDummy(TimeActContainer container, int dummy, int defaultPart)
    {
        if (!IsPlayer(container) || dummy < 0)
        {
            return -1;
        }

        return (dummy / DummyGroupSize) switch
        {
            RightWeaponGroup => FindWeapon(container, false),
            LeftWeaponGroup => FindWeapon(container, true),
            _ => defaultPart
        };
    }

    private static int ToLocalDummy(TimeActContainer container, int dummy)
    {
        return IsPlayer(container) && dummy >= 0 ? dummy % DummyGroupSize : dummy;
    }

    private Param.Row? GetRow(string paramName, int id)
    {
        return CharacterComposition.GetRow(_project, paramName, id);
    }

    private static int? GetParameter(TAE.Event ev, string name)
    {
        return ev.Parameters!.Values.TryGetValue(name, out var value) && value is IConvertible number
            ? number.ToInt32(null)
            : null;
    }
}
