#nullable enable
using Andre.Formats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// What a character is drawn and animated with, when an NpcParam row swaps something in: the model shown, the character
/// whose animations (and TAE) it plays, and the character whose textures it wears
/// </summary>
public sealed record LuaModelSource(string Model, string Animations, string Textures);

/// <summary>
/// The NpcParam row the AI Simulation runs as: its character (from the row ID), the animation offsets it plays with,
/// and its behavior variation. Everything is read by the fields the project's params have, not by game.
/// <list type="bullet">
/// <item>Animation offsets: every clip is played at its ID plus an offset when the character has that clip, else the
/// next offset, else the ID as it is. Tried in this order: the animIdOffset of each resident SpEffect (spEffectID0..)
/// that has one (−1 and 0 are none), then the row's animIdOffset2, then its animIdOffset. The param descriptions say
/// offset 2 falls back on offset 1, and offset 1 on the plain ID; where a SpEffect's offset comes in that order is an
/// assumption (it is tried first, as the more specific state).</item>
/// <item>Behavior variation: the row's behaviorVariationId picks its own BehaviorParam rows for bullet events.</item>
/// </list>
/// </summary>
public sealed class LuaNpc : ILuaNpc
{
    private const string NpcThinkParamName = "NpcThinkParam";
    private const int MaxResidentSpEffectSlots = 32;
    private const string NpcParamName = "NpcParam";
    private const string SpEffectParamName = "SpEffectParam";
    private const int MaxResidentSpEffects = 32;

    // NpcParam rows are the character's number times one of these, plus a variant (ER 43000000 → c4300, DS3 430000)
    private static readonly int[] RowScales = { 10000, 1000, 100 };

    public Param.Row Row { get; }
    public string Character { get; }

    /// <summary>The offsets to try, best first, each with where it came from</summary>
    public IReadOnlyList<(int Offset, string Source)> AnimationOffsets { get; }

    public int? BehaviorVariation { get; }

    /// <summary>The NpcThinkParam row (GetExcelParam's answers): the one of the same ID, else the goal's first user</summary>
    public Param.Row? ThinkRow { get; private set; }

    /// <summary>The SpEffects resident on the character (spEffectID0..), each with its slot</summary>
    public IReadOnlyList<(int Id, string Slot)> ResidentSpEffects { get; private set; } = Array.Empty<(int, string)>();

    /// <summary>The map capsule radius (hitRadius), when the row has the field</summary>
    public double? MapHitRadius { get; private set; }

    private const int MaskCount = 32;

    /// <summary>The model masks the row shows (1) and hides (0), by mask group; null when the row has no mask fields</summary>
    public int[]? Masks { get; private set; }

    /// <summary>What the row swaps in (normalChangeModelId, normalChangeAnimChrId, normalChangeTexChrId); null for none</summary>
    public LuaModelSource? Source { get; private set; }

    /// <summary>How fast the character turns on its own, in degrees a second (turnVellocity), when set above 0</summary>
    public float? TurnVelocity { get; private set; }

    public bool HasResidentSpEffect(int id) => ResidentSpEffects.Any(s => s.Id == id);

    bool ILuaNpc.HasSpEffect(int id) => HasResidentSpEffect(id);

    // What the target does is the scene's (First Person Mode), not the NPC rows'
    bool ILuaNpc.TargetHasSpEffect(int id) => false;
    bool? ILuaNpc.TargetGuarding => null;
    double? ILuaNpc.TeamRole => null;

    public double? ThinkParam(string field) => ReadField(ThinkRow, field);

    /// <summary>A field of a row as a number (what GetExcelParam gives), or null without the row or the field</summary>
    public static double? ReadField(Param.Row? row, string field)
    {
        if (row == null || !CharacterComposition.HasField(row, field) || row[field]?.Value is not IConvertible value)
            return null;
        try
        {
            return value.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    public string Label => string.IsNullOrEmpty(Row.Name) ? $"{Row.ID}" : $"{Row.ID} {Row.Name}";

    private LuaNpc(Param.Row row, string character, List<(int, string)> offsets, int? variation)
    {
        Row = row;
        Character = character;
        AnimationOffsets = offsets;
        BehaviorVariation = variation;
    }

    /// <summary>The NpcThinkParam rows of the project, or null if it has no NpcThinkParam</summary>
    public static Param? GetThinkParam(ProjectEntry project) =>
        project.Handler.ParamData?.PrimaryBank?.Params is { } bank && bank.TryGetValue(NpcThinkParamName, out var param)
            ? param
            : null;

    /// <summary>The NpcParam rows of the project, or null if it has no NpcParam</summary>
    public static Param? GetParam(ProjectEntry project) =>
        project.Handler.ParamData?.PrimaryBank?.Params is { } bank && bank.TryGetValue(NpcParamName, out var param)
            ? param
            : null;

    /// <summary>The character of an NpcParam row ID: its number at the first scale that names a known chr</summary>
    public static string? CharacterOf(int rowId, ICollection<string> characters)
    {
        foreach (var scale in RowScales)
        {
            var number = rowId / scale;
            if (number is < 1000 or > 9999)
                continue;
            var chr = $"c{number:D4}";
            if (characters.Contains(chr))
                return chr;
        }
        return null;
    }

    /// <summary>The row as the simulation uses it, or null if its ID names no character of the project</summary>
    /// <param name="goalId">The battle goal being simulated: its first NpcThinkParam user is the think row when none
    /// shares the NpcParam row's ID</param>
    public static LuaNpc? From(ProjectEntry project, Param.Row row, ICollection<string> characters, int? goalId = null)
    {
        var npc = Read(project, row, characters);
        if (npc == null)
            return null;

        // Resident SpEffects (spEffectID0..31): the slots the row has, set above 0
        var resident = new List<(int, string)>();
        for (var i = 0; i < MaxResidentSpEffectSlots && CharacterComposition.HasField(row, $"spEffectID{i}"); i++)
        {
            var id = CharacterComposition.GetValue(row, $"spEffectID{i}");
            if (id > 0)
                resident.Add((id, $"spEffectID{i}"));
        }
        npc.ResidentSpEffects = resident;

        if (CharacterComposition.HasField(row, "hitRadius"))
            npc.MapHitRadius = CharacterComposition.GetFloat(row, "hitRadius");

        if (CharacterComposition.HasField(row, "turnVellocity") && CharacterComposition.GetFloat(row, "turnVellocity") is > 0 and var turn)
            npc.TurnVelocity = turn;

        // The model masks (modelDispMask0..31: 1 shows a mask group), as the Map Editor applies them to a placed enemy
        if (CharacterComposition.HasField(row, "modelDispMask0"))
        {
            var masks = new int[MaskCount];
            for (var i = 0; i < MaskCount && CharacterComposition.HasField(row, $"modelDispMask{i}"); i++)
                masks[i] = CharacterComposition.GetValue(row, $"modelDispMask{i}") != 0 ? 1 : 0;
            npc.Masks = masks;
        }

        // What the row swaps in: another model, another character's animations, another character's textures
        string? Replacement(string field)
        {
            if (!CharacterComposition.HasField(row, field))
                return null;
            var id = CharacterComposition.GetValue(row, field);
            if (id is <= 0 or > 9999)
                return null;
            var other = $"c{id:D4}";
            return characters.Contains(other) ? other : null;
        }
        var model = Replacement("normalChangeModelId") ?? npc.Character;
        var animations = Replacement("normalChangeAnimChrId") ?? model;
        var textures = Replacement("normalChangeTexChrId") ?? model;
        if (model != npc.Character || animations != model || textures != model)
            npc.Source = new LuaModelSource(model, animations, textures);

        if (project.Handler.ParamData?.PrimaryBank?.Params is { } bank && bank.TryGetValue(NpcThinkParamName, out var think))
        {
            npc.ThinkRow = think[row.ID];
            if (npc.ThinkRow == null && goalId is { } goal && project.Handler.LuaData?.UsersOf(goal) is { Count: > 0 } users)
                npc.ThinkRow = think[users[0].Id];
        }
        return npc;
    }

    private static LuaNpc? Read(ProjectEntry project, Param.Row row, ICollection<string> characters)
    {
        if (CharacterOf(row.ID, characters) is not { } chr)
            return null;

        var offsets = new List<(int, string)>();

        // The resident SpEffects' offsets first
        for (var i = 0; i < MaxResidentSpEffects && CharacterComposition.HasField(row, $"spEffectID{i}"); i++)
        {
            var id = CharacterComposition.GetValue(row, $"spEffectID{i}");
            if (id <= 0 || CharacterComposition.GetRow(project, SpEffectParamName, id) is not { } effect ||
                !CharacterComposition.HasField(effect, "animIdOffset"))
                continue;
            var offset = CharacterComposition.GetValue(effect, "animIdOffset");
            if (offset > 0)
                offsets.Add((offset, LOC.Get("LUA_Npc_Offset_SpEffect", id, $"spEffectID{i}")));
        }

        foreach (var field in new[] { "animIdOffset2", "animIdOffset" })
        {
            if (!CharacterComposition.HasField(row, field))
                continue;
            var offset = CharacterComposition.GetValue(row, field);
            if (offset > 0)
                offsets.Add((offset, LOC.Get("LUA_Npc_Offset_Field", field)));
        }

        int? variation = CharacterComposition.HasField(row, "behaviorVariationId")
            ? CharacterComposition.GetValue(row, "behaviorVariationId")
            : null;

        return new LuaNpc(row, chr, offsets, variation);
    }

    /// <summary>The IDs to try for a clip, best first: with each offset, then as it is</summary>
    public IEnumerable<long> Candidates(long id)
    {
        foreach (var (offset, _) in AnimationOffsets)
            yield return id + offset;
        yield return id;
    }

    /// <summary>
    /// The rows a battle goal's character runs as, best first: those whose ID is an NpcThinkParam row that uses the
    /// goal (NpcThinkParam and NpcParam rows usually share IDs), then the rest of the character's rows
    /// </summary>
    public static List<Param.Row> RowsForGoal(ProjectEntry project, int? goalId, string? chr, ICollection<string> characters)
    {
        var result = new List<Param.Row>();
        if (GetParam(project) is not { } param)
            return result;

        var users = goalId is { } goal && project.Handler.LuaData is { } data
            ? data.UsersOf(goal).Select(u => u.Id).ToHashSet()
            : new HashSet<int>();

        var own = param.Rows.Where(r => r.ID > 0 && (chr == null || CharacterOf(r.ID, characters) == chr)).ToList();
        result.AddRange(own.Where(r => users.Contains(r.ID)).OrderBy(r => r.ID));
        if (chr != null)
            result.AddRange(own.Where(r => !users.Contains(r.ID)).OrderBy(r => r.ID));
        return result;
    }
}
