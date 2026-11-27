#nullable enable
using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Utilities;

/// <summary>A row to add: a copy of a param's row under a free ID, with fields set on the copy</summary>
public sealed class ParamRowCopy
{
    public string Param = "";
    public Param.Row Source = null!;
    public int NewId;
    public Dictionary<string, object> Fields = new();
}

/// <summary>A field set on an existing row</summary>
public sealed record ParamFieldEdit(string Param, Param.Row Row, string Field, object Value);

/// <summary>What the linked param groups of a character scale add and change (Character Scale Plan.md §4.7)</summary>
public sealed class CharacterParamPlan
{
    public List<ParamRowCopy> Copies = new();
    public List<ParamFieldEdit> Edits = new();

    /// <summary>One line per group: what is scaled, what is forked</summary>
    public List<string> Summary = new();

    public int AttackRows, AttackForks, VariationForks, LockCamRows, LockCamForks, ThinkRows;
    public bool IsEmpty => Copies.Count == 0 && Edits.Count == 0;
}

/// <summary>Which linked param groups to scale (D4: each a checkbox, all on)</summary>
public sealed class CharacterParamGroups
{
    /// <summary>AtkParam_Npc hit radii of the character's attacks (BehaviorParam refType 0)</summary>
    public bool AttackRadii = true;

    /// <summary>LockCamParam camera distance and pivot height of the character's rows</summary>
    public bool LockCamera = true;

    /// <summary>NpcThinkParam combat spacing (BB: near, mid, far, out, arrive distances)</summary>
    public bool AiSpacing = true;
}

/// <summary>
///     The linked param groups of a character scale (D4): attack hit radii, lock-on camera, AI spacing. Rows only this
///     character uses are scaled in place; rows others also use are copied to free IDs and this character pointed at
///     the copies (a behavior variation other models share is copied as a whole). Pure: it reads the params and
///     returns copies and edits, which the tool applies through the Param Editor.
/// </summary>
public static class CharacterParamScale
{
    public static readonly string[] AttackFields = Enumerable.Range(0, 16).Select(i => $"hit{i}_Radius").ToArray();

    // Camera distance (m) and the camera's pivot height on the target (m): Param Fields (ER).md §3AQ
    public static readonly string[] LockCamFields = { "camDistTarget", "chrOrgOffset_Y" };

    // The only combat spacing NpcThinkParam keeps (BB); elsewhere spacing is in the Lua AI and time act event 760
    public static readonly string[] ThinkFields = { "nearDist", "midDist", "farDist", "outDist", "target_ArriveDist", "point_ArriveDist" };

    private static readonly string[] LockCamIdFields = { "lockCameraParamId", "lockCamParamId" };

    /// <summary>The model an NpcParam row uses: normalChangeModelId when set, else the character its ID names</summary>
    public static string? ModelOf(Param.Row row, Func<int, string?> rowCharacter)
    {
        if (row["normalChangeModelId"] is { } swap && Convert.ToInt32(swap.Value) is > 0 and <= 9999 and var swapped)
            return $"c{swapped:D4}";
        return rowCharacter(row.ID);
    }

    /// <param name="asNew">
    ///     For a copy under a new ID (P6): the source's rows the copy will be made from. Every existing row then counts
    ///     as another character's, so everything they reference is copied; edits to these rows (and their think rows)
    ///     are for the caller to put on the copies.
    /// </param>
    public static CharacterParamPlan Plan(IReadOnlyDictionary<string, Param> parameters, string character,
        Func<int, string?> rowCharacter, float s, CharacterParamGroups groups, IReadOnlyList<Param.Row>? asNew = null, Func<int, int>? variationStart = null)
    {
        var plan = new CharacterParamPlan();
        if (!parameters.TryGetValue("NpcParam", out var npc))
            return plan;

        Func<Param.Row, bool> Mine = asNew != null
            ? _ => false
            : r => string.Equals(ModelOf(r, rowCharacter), character, StringComparison.OrdinalIgnoreCase);
        var mine = asNew?.ToList() ?? npc.Rows.Where(Mine).ToList();
        if (mine.Count == 0)
            return plan;

        var ids = new FreeIds(parameters);

        if (groups.AttackRadii)
            PlanAttacks(parameters, npc, mine, Mine, s, ids, plan, variationStart);
        if (groups.LockCamera)
            PlanLockCamera(parameters, npc, mine, Mine, s, ids, plan);
        if (groups.AiSpacing)
            PlanThink(parameters, mine, s, plan);

        return plan;
    }

    // The row ID scales a character's NpcParam rows use (ER 20100000 = c2010 × 10000)
    private static readonly int[] RowScales = { 10000, 1000, 100000, 100 };

    /// <summary>A source row's ID moved to the target's block (20100000 → 20110000 for c2010 → c2011), or null</summary>
    public static int? MoveRowId(int id, int sourceNumber, int targetNumber)
    {
        foreach (var k in RowScales)
            if (id / k == sourceNumber)
                return targetNumber * k + id % k;
        return null;
    }

    /// <summary>The source character's own NpcParam rows (by ID; rows that swap in another model are left)</summary>
    public static List<Param.Row> SourceRows(Param npc, string source, Func<int, string?> rowCharacter) =>
        npc.Rows.Where(r => string.Equals(rowCharacter(r.ID), source, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(ModelOf(r, rowCharacter), source, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>Whether a character number's NpcParam row block is free (for every scale the source's rows use)</summary>
    public static bool RowBlockFree(Param npc, IEnumerable<Param.Row> sourceRows, int sourceNumber, int targetNumber)
    {
        var ids = npc.Rows.Select(r => r.ID).ToHashSet();
        return sourceRows.All(r => MoveRowId(r.ID, sourceNumber, targetNumber) is not { } id || !ids.Contains(id));
    }

    /// <summary>
    ///     The param side of a copy under a new ID (P6): the source's NpcParam rows (and NpcThinkParam rows) copied to
    ///     the target's block, sharing the source's textures (normalChangeTexChrId), playing their own clips
    ///     (normalChangeAnimChrId cleared), body size scaled (P2) and the linked groups planned for the copies (P5),
    ///     every referenced row copied as the source keeps its own.
    /// </summary>
    public static CharacterParamPlan PlanCopy(IReadOnlyDictionary<string, Param> parameters, string source, string target,
        Func<int, string?> rowCharacter, float s, CharacterParamGroups groups, bool bodyParams)
    {
        var plan = new CharacterParamPlan();
        if (!parameters.TryGetValue("NpcParam", out var npc))
            return plan;

        var sourceNumber = int.Parse(source.AsSpan(1));
        var targetNumber = int.Parse(target.AsSpan(1));
        var rows = SourceRows(npc, source, rowCharacter);
        var npcCopies = new Dictionary<Param.Row, ParamRowCopy>();
        foreach (var row in rows)
        {
            if (MoveRowId(row.ID, sourceNumber, targetNumber) is not { } id || npc[id] != null)
                continue;
            var copy = new ParamRowCopy { Param = "NpcParam", Source = row, NewId = id };
            if (row["normalChangeTexChrId"] is { } tex && Convert.ToInt32(tex.Value) <= 0)
                copy.Fields["normalChangeTexChrId"] = sourceNumber;
            if (row["normalChangeAnimChrId"] != null)
                copy.Fields["normalChangeAnimChrId"] = -1;
            if (bodyParams)
                foreach (var (field, minusOneUnset) in CharacterScale.BodyFields)
                    if (row[field] is { Value: float v } && !(minusOneUnset && v == -1f) && HavokFieldScale.Mul(v, s) is var scaled && scaled != v)
                        copy.Fields[field] = scaled;
            npcCopies[row] = copy;
            plan.Copies.Add(copy);
        }

        // Think rows move with their NpcParam rows
        var thinkCopies = new Dictionary<Param.Row, ParamRowCopy>();
        if (parameters.TryGetValue("NpcThinkParam", out var think))
            foreach (var (row, npcCopy) in npcCopies)
                if (think[row.ID] is { } thinkRow && think[npcCopy.NewId] == null)
                {
                    var copy = new ParamRowCopy { Param = "NpcThinkParam", Source = thinkRow, NewId = npcCopy.NewId };
                    thinkCopies[thinkRow] = copy;
                    plan.Copies.Add(copy);
                }

        // A variation numbered after the character (c2010: 20100) moves to the copy's numbers (c2011: 20110); the search starts
        // just before, so the first free one there is taken
        var linked = Plan(parameters, source, rowCharacter, s, groups, npcCopies.Keys.ToList(),
            v => v / 10 == sourceNumber ? targetNumber * 10 + v % 10 - 1 : v);
        plan.Copies.AddRange(linked.Copies);
        plan.Summary.AddRange(linked.Summary);
        plan.AttackRows = linked.AttackRows;
        plan.AttackForks = linked.AttackForks;
        plan.VariationForks = linked.VariationForks;
        plan.LockCamRows = linked.LockCamRows;
        plan.LockCamForks = linked.LockCamForks;
        plan.ThinkRows = linked.ThinkRows;

        // Edits aimed at the source's rows go on their copies
        foreach (var edit in linked.Edits)
        {
            if (edit.Param == "NpcParam" && npcCopies.TryGetValue(edit.Row, out var npcCopy))
                npcCopy.Fields[edit.Field] = edit.Value;
            else if (edit.Param == "NpcThinkParam" && thinkCopies.TryGetValue(edit.Row, out var thinkCopy))
                thinkCopy.Fields[edit.Field] = edit.Value;
            else
                plan.Edits.Add(edit);
        }

        plan.Summary.Insert(0, $"NpcParam: {npcCopies.Count} rows copied to {target}'s block" +
                               (thinkCopies.Count > 0 ? $", NpcThinkParam: {thinkCopies.Count}" : ""));
        return plan;
    }

    private static void PlanAttacks(IReadOnlyDictionary<string, Param> parameters, Param npc, List<Param.Row> mine,
        Func<Param.Row, bool> isMine, float s, FreeIds ids, CharacterParamPlan plan, Func<int, int>? variationStart)
    {
        if (!parameters.TryGetValue("BehaviorParam", out var behavior) || !parameters.TryGetValue("AtkParam_Npc", out var attack) ||
            mine[0]["behaviorVariationId"] == null)
            return;

        // The character's behavior rows, per variation; a variation other models' rows use is copied whole
        var rowsByVariation = behavior.Rows.Where(r => r["variationId"] != null)
            .GroupBy(r => Convert.ToInt32(r["variationId"]!.Value.Value)).ToDictionary(g => g.Key, g => g.ToList());
        var behaviorRows = new List<(Param.Row Row, ParamRowCopy? Copy)>();
        foreach (var variation in mine.Select(r => Convert.ToInt32(r["behaviorVariationId"]!.Value.Value)).Distinct())
        {
            if (!rowsByVariation.TryGetValue(variation, out var rows))
                continue;

            // 0 (and below) is the shared default set, not this character's: never scaled or copied
            if (variation <= 0)
            {
                plan.Summary.Add($"BehaviorParam variation {variation} is the shared default; its attacks are left");
                continue;
            }

            var shared = npc.Rows.Any(r => !isMine(r) && r["behaviorVariationId"] is { } v && Convert.ToInt32(v.Value) == variation);
            if (!shared)
            {
                behaviorRows.AddRange(rows.Select(r => ((Param.Row)r, (ParamRowCopy?)null)));
                continue;
            }

            // Copy the variation: IDs move with it when they are (base + variation) × 1000 + judge
            var bases = rows.Select(r => r.ID / 1000 - variation).Distinct().ToList();
            if (bases.Count != 1 || rows.Any(r => r.ID < 0))
            {
                plan.Summary.Add($"BehaviorParam variation {variation} is shared but its row IDs don't follow it; its attacks are left");
                continue;
            }

            var newVariation = ids.FreeVariation(rowsByVariation.Keys, npc, bases[0], variationStart?.Invoke(variation) ?? variation);
            foreach (var row in rows)
            {
                var copy = new ParamRowCopy
                {
                    Param = "BehaviorParam",
                    Source = row,
                    NewId = row.ID + (newVariation - variation) * 1000
                };
                copy.Fields["variationId"] = newVariation;
                ids.Take("BehaviorParam", copy.NewId);
                plan.Copies.Add(copy);
                behaviorRows.Add((row, copy));
            }

            foreach (var row in mine.Where(r => Convert.ToInt32(r["behaviorVariationId"]!.Value.Value) == variation))
                plan.Edits.Add(new ParamFieldEdit("NpcParam", row, "behaviorVariationId", newVariation));
            plan.VariationForks++;
        }

        // Who uses each attack: behavior rows (refType 0) and bullets (atkId_Bullet)
        var myRows = new HashSet<Param.Row>(behaviorRows.Where(b => b.Copy == null).Select(b => b.Row));
        var users = new Dictionary<int, int>();
        foreach (var row in behavior.Rows)
        {
            if (myRows.Contains(row) || row["refType"] is not { } t || Convert.ToInt32(t.Value) != 0)
                continue;
            var id = Convert.ToInt32(row["refId"]!.Value.Value);
            users[id] = users.GetValueOrDefault(id) + 1;
        }
        foreach (var bullet in parameters.Where(p => p.Key is "BulletParam" or "Bullet").Select(p => p.Value))
            foreach (var row in bullet.Rows)
                if (row["atkId_Bullet"] is { } a && Convert.ToInt32(a.Value) is > 0 and var id)
                    users[id] = users.GetValueOrDefault(id) + 1;

        // Each attack once: scaled in place, or copied when others use it (the copy shared by this character's rows)
        var done = new Dictionary<int, int>();
        foreach (var (row, copy) in behaviorRows)
        {
            if (Convert.ToInt32(row["refType"]!.Value.Value) != 0)
                continue;
            var atkId = Convert.ToInt32(row["refId"]!.Value.Value);
            if (atkId <= 0 || attack[atkId] is not { } atk)
                continue;

            if (!done.TryGetValue(atkId, out var target))
            {
                if (users.GetValueOrDefault(atkId) > 0)
                {
                    target = ids.Next("AtkParam_Npc", atkId);
                    var atkCopy = new ParamRowCopy { Param = "AtkParam_Npc", Source = atk, NewId = target };
                    foreach (var (field, value) in ScaledFields(atk, AttackFields, s, false))
                        atkCopy.Fields[field] = value;
                    plan.Copies.Add(atkCopy);
                    plan.AttackForks++;
                }
                else
                {
                    target = atkId;
                    foreach (var (field, value) in ScaledFields(atk, AttackFields, s, false))
                        plan.Edits.Add(new ParamFieldEdit("AtkParam_Npc", atk, field, value));
                }

                done[atkId] = target;
                plan.AttackRows++;
            }

            if (target == atkId)
                continue;
            // Point this character's behavior row at the copy
            if (copy != null)
                copy.Fields["refId"] = target;
            else
                plan.Edits.Add(new ParamFieldEdit("BehaviorParam", row, "refId", target));
        }

        plan.Summary.Add($"AtkParam_Npc: {plan.AttackRows} attacks ({plan.AttackForks} copied, as other characters use them)" +
                         (plan.VariationForks > 0 ? $"; {plan.VariationForks} behavior variations copied, as other models use them" : ""));
    }

    private static void PlanLockCamera(IReadOnlyDictionary<string, Param> parameters, Param npc, List<Param.Row> mine,
        Func<Param.Row, bool> isMine, float s, FreeIds ids, CharacterParamPlan plan)
    {
        var idField = LockCamIdFields.FirstOrDefault(f => mine[0][f] != null);
        if (idField == null || !parameters.TryGetValue("LockCamParam", out var lockCam))
            return;

        foreach (var group in mine.GroupBy(r => Convert.ToInt32(r[idField]!.Value.Value)))
        {
            if (group.Key <= 0 || lockCam[group.Key] is not { } row)
                continue;

            var fields = ScaledFields(row, LockCamFields, s, false).ToList();
            if (fields.Count == 0)
                continue;

            var shared = npc.Rows.Any(r => !isMine(r) && r[idField] is { } v && Convert.ToInt32(v.Value) == group.Key);
            if (shared)
            {
                var copy = new ParamRowCopy { Param = "LockCamParam", Source = row, NewId = ids.Next("LockCamParam", group.Key) };
                foreach (var (field, value) in fields)
                    copy.Fields[field] = value;
                plan.Copies.Add(copy);
                foreach (var npcRow in group)
                    plan.Edits.Add(new ParamFieldEdit("NpcParam", npcRow, idField, copy.NewId));
                plan.LockCamForks++;
            }
            else
            {
                foreach (var (field, value) in fields)
                    plan.Edits.Add(new ParamFieldEdit("LockCamParam", row, field, value));
            }

            plan.LockCamRows++;
        }

        if (plan.LockCamRows > 0)
            plan.Summary.Add($"LockCamParam: {plan.LockCamRows} rows ({plan.LockCamForks} copied, as other characters use them)");
    }

    private static void PlanThink(IReadOnlyDictionary<string, Param> parameters, List<Param.Row> mine, float s,
        CharacterParamPlan plan)
    {
        if (!parameters.TryGetValue("NpcThinkParam", out var think))
            return;

        // Think rows share the NpcParam row's ID
        foreach (var npcRow in mine)
        {
            if (think[npcRow.ID] is not { } row)
                continue;
            var fields = ScaledFields(row, ThinkFields, s, false).ToList();
            if (fields.Count == 0)
                continue;
            foreach (var (field, value) in fields)
                plan.Edits.Add(new ParamFieldEdit("NpcThinkParam", row, field, value));
            plan.ThinkRows++;
        }

        if (plan.ThinkRows > 0)
            plan.Summary.Add($"NpcThinkParam: {plan.ThinkRows} rows (combat spacing)");
    }

    /// <summary>The float fields of a row × s (0 and unset −1 kept), as converted values</summary>
    private static IEnumerable<(string Field, object Value)> ScaledFields(Param.Row row, IEnumerable<string> fields, float s,
        bool minusOneUnset)
    {
        foreach (var field in fields)
        {
            if (row[field] is not { Value: float v } || v == 0 || minusOneUnset && v == -1f)
                continue;
            var scaled = HavokFieldScale.Mul(v, s);
            if (scaled != v)
                yield return (field, scaled);
        }
    }

    /// <summary>Free row IDs per param, so copies don't collide with existing rows or each other</summary>
    private sealed class FreeIds
    {
        private readonly IReadOnlyDictionary<string, Param> Params;
        private readonly Dictionary<string, HashSet<int>> Used = new();

        public FreeIds(IReadOnlyDictionary<string, Param> parameters) => Params = parameters;

        private HashSet<int> Of(string param)
        {
            if (!Used.TryGetValue(param, out var set))
                Used[param] = set = Params.TryGetValue(param, out var p) ? p.Rows.Select(r => r.ID).ToHashSet() : new();
            return set;
        }

        public void Take(string param, int id) => Of(param).Add(id);

        /// <summary>The first free ID after a row's, kept beside it</summary>
        public int Next(string param, int after)
        {
            var used = Of(param);
            var id = after + 1;
            while (used.Contains(id)) id++;
            used.Add(id);
            return id;
        }

        private HashSet<int>? TakenVariations;

        /// <summary>
        ///     The first variation number after <paramref name="after" /> (so it stays beside the character's own:
        ///     41000 → 41001) that no behavior row or NpcParam row uses and whose ID block is free
        /// </summary>
        public int FreeVariation(IEnumerable<int> variations, Param npc, int idBase, int after)
        {
            if (TakenVariations == null)
            {
                TakenVariations = variations.ToHashSet();
                foreach (var row in npc.Rows)
                    if (row["behaviorVariationId"] is { } v)
                        TakenVariations.Add(Convert.ToInt32(v.Value));
            }

            var blocks = Of("BehaviorParam").Select(id => id / 1000 - idBase).ToHashSet();
            var candidate = after + 1;
            while (TakenVariations.Contains(candidate) || blocks.Contains(candidate))
                candidate++;
            TakenVariations.Add(candidate);
            return candidate;
        }
    }
}
