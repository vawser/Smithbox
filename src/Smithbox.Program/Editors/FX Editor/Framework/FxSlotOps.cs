using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// A config's fixed action slots (FX Editor Plan §23, phase E4), from the schema's config slot table (fxr-main's
/// ConfigActionSlots). Basic (1004) configs have 15 slots and NodeEmitter (1005) 10, in file order, each holding one
/// action or an empty one (type 0, header 0/1/0, no data); DS3 has 13 and 9, the force slots it has no actions for.
/// Every vanilla config of DS3, SDT, ER, AC6 and NR has each action in its slot (FxTreeEditCheck --slots).
/// Only SoulsFormats is used, so the check tool compiles this file on its own.
/// </summary>
public static class FxSlotOps
{
    /// <summary>An empty slot, as vanilla stores one.</summary>
    public static FxrAction Empty() => new() { Type = 0, Unk02 = 0, Unk03 = 1, Unk04 = 0 };

    /// <summary>
    /// The slots a config type has in a game: the table's, without the trailing ones none of whose actions exist in
    /// the game. Null for config types without fixed slots (LevelsOfDetail).
    /// </summary>
    public static List<int[]> SlotsFor(FxrSchema schema, int configType, FxrGame game)
    {
        if (!schema.ConfigSlots.TryGetValue(configType, out var slots))
            return null;
        int count = slots.Count;
        while (count > 0 && !slots[count - 1].Any(t => schema.GetLayout(t, game) != null))
            count--;
        return slots.Take(count).ToList();
    }

    /// <summary>The slot's name (the schema's slot of its actions, e.g. NodeMovement), or "Slot n".</summary>
    public static string SlotName(FxrSchema schema, int[] types, int index)
    {
        foreach (var t in types)
            if (schema.Actions.TryGetValue(t, out var def) && def.Slot != null)
                return def.Slot;
        return $"Slot {index}";
    }

    /// <summary>Whether the slot may hold an empty action (the schema's slot table; unknown slots may).</summary>
    public static bool IsNullable(FxrSchema schema, int[] types, int index) =>
        !schema.Slots.TryGetValue(SlotName(schema, types, index), out var s) || s.Nullable;

    /// <summary>The action types that can fill the slot in this game.</summary>
    public static IEnumerable<int> TypesFor(FxrSchema schema, int[] types, FxrGame game) =>
        types.Where(t => schema.GetLayout(t, game) != null);

    /// <summary>The config's actions padded out with empty ones so that <paramref name="slot"/> exists. Returns how many were added.</summary>
    public static int PadTo(FxrConfig config, int slot)
    {
        int added = 0;
        while (config.Actions.Count <= slot)
        {
            config.Actions.Add(Empty());
            added++;
        }
        return added;
    }

    /// <summary>A problem with a config's slots: the action's index, and what's wrong (wrong-slot, empty-required, too-many).</summary>
    public record Problem(int Index, string Code, int ActionType, int ExpectedSlot);

    /// <summary>What doesn't fit the slot table: an action in another slot's place, a required slot left empty, the wrong count.</summary>
    public static List<Problem> Problems(FxrSchema schema, FxrConfig config, FxrGame game)
    {
        var problems = new List<Problem>();
        var slots = SlotsFor(schema, config.Type, game);
        if (slots == null)
            return problems;

        for (int i = 0; i < config.Actions.Count; i++)
        {
            var a = config.Actions[i];
            if (i >= slots.Count)
            {
                problems.Add(new Problem(i, "too-many", a.Type, -1));
                continue;
            }
            if (a.Type == 0)
            {
                if (!IsNullable(schema, slots[i], i))
                    problems.Add(new Problem(i, "empty-required", 0, i));
                continue;
            }
            if (Array.IndexOf(slots[i], a.Type) < 0)
                problems.Add(new Problem(i, "wrong-slot", a.Type, schema.SlotOf(config.Type, a.Type)));
        }
        // Fewer actions than slots isn't one: 13 vanilla SDT configs stop at 13 of 15, and the game plays them
        return problems;
    }
}
