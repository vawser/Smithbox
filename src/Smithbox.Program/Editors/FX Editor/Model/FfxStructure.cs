using SoulsFormats;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Structure edits on a DLSE effect (DES / DS2 / DS2S; FFX Older Games Plan §21, F7a): states, exits, a state's
/// actions, the entries of a call list, and new actions from vanilla templates. Each operation works on one
/// <see cref="FFXDLSE.FXEffect"/> (a DES wrapper's chosen variant, or the effect itself) and keeps the effect valid:
/// exit targets follow their states (−1 ends the effect), and the ResourceSet is completed on save (F3c's keep +
/// append). SoulsFormats + BCL only, like the rest of Model/, so the check tools can link it.
/// </summary>
public static class FfxStructure
{
    // ---------------------------------------------------------------- states

    public static List<FFXDLSE.State> States(FFXDLSE.FXEffect fx) => fx.StateMap.States;

    public static IEnumerable<FFXDLSE.Trigger> Exits(FFXDLSE.FXEffect fx) => fx.StateMap.States.SelectMany(s => s.Triggers);

    /// <summary>Adds an empty state at the end (no actions, no exits: once entered, the effect stays there).</summary>
    public static int AddState(FFXDLSE.FXEffect fx)
    {
        fx.StateMap.States.Add(new FFXDLSE.State());
        return fx.StateMap.States.Count - 1;
    }

    /// <summary>Puts a copy of state <paramref name="index"/> just after it. Exits into later states keep their targets.</summary>
    public static int DuplicateState(FFXDLSE.FXEffect fx, FFXDLSE.StreamFormat format, int index)
    {
        var states = fx.StateMap.States;
        if (index < 0 || index >= states.Count)
            return -1;
        int at = index + 1;
        var copy = FFXDLSE.Clone(states[index], format);
        foreach (var t in Exits(fx))
            if (t.StateIndex >= at)
                t.StateIndex++;
        // the copy's own exits were cloned before the shift: shift them the same way
        foreach (var t in copy.Triggers)
            if (t.StateIndex >= at)
                t.StateIndex++;
        states.Insert(at, copy);
        return at;
    }

    /// <summary>How many exits lead to state <paramref name="index"/> (they end the effect once it's deleted).</summary>
    public static int ExitsInto(FFXDLSE.FXEffect fx, int index) => Exits(fx).Count(t => t.StateIndex == index);

    /// <summary>
    /// Removes state <paramref name="index"/>: exits into it end the effect instead, later states move down by one.
    /// The last remaining state can't be removed.
    /// </summary>
    public static bool DeleteState(FFXDLSE.FXEffect fx, int index)
    {
        var states = fx.StateMap.States;
        if (states.Count <= 1 || index < 0 || index >= states.Count)
            return false;
        states.RemoveAt(index);
        foreach (var t in Exits(fx))
        {
            if (t.StateIndex == index)
                t.StateIndex = -1;
            else if (t.StateIndex > index)
                t.StateIndex--;
        }
        return true;
    }

    /// <summary>Swaps states <paramref name="a"/> and <paramref name="b"/> (state 0 is where the effect starts).</summary>
    public static bool SwapStates(FFXDLSE.FXEffect fx, int a, int b)
    {
        var states = fx.StateMap.States;
        if (a == b || a < 0 || b < 0 || a >= states.Count || b >= states.Count)
            return false;
        (states[a], states[b]) = (states[b], states[a]);
        foreach (var t in Exits(fx))
        {
            if (t.StateIndex == a) t.StateIndex = b;
            else if (t.StateIndex == b) t.StateIndex = a;
        }
        return true;
    }

    // ---------------------------------------------------------------- exits

    /// <summary>Puts a copy of a state's exit <paramref name="index"/> just after it (same target and condition).</summary>
    public static int DuplicateExit(FFXDLSE.State state, FFXDLSE.StreamFormat format, int index)
    {
        if (index < 0 || index >= state.Triggers.Count)
            return -1;
        state.Triggers.Insert(index + 1, FFXDLSE.Clone(state.Triggers[index], format));
        return index + 1;
    }

    public static bool DeleteExit(FFXDLSE.State state, int index)
    {
        if (index < 0 || index >= state.Triggers.Count)
            return false;
        state.Triggers.RemoveAt(index);
        return true;
    }

    // ---------------------------------------------------------------- lists (a state's actions, a call list)

    /// <summary>Puts a copy of item <paramref name="index"/> just after it.</summary>
    public static int Duplicate<T>(List<T> list, int index, Func<T, T> clone)
    {
        if (index < 0 || index >= list.Count)
            return -1;
        list.Insert(index + 1, clone(list[index]));
        return index + 1;
    }

    public static bool Delete<T>(List<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
            return false;
        list.RemoveAt(index);
        return true;
    }

    /// <summary>Moves item <paramref name="from"/> to <paramref name="to"/> (the others keep their order).</summary>
    public static bool Move<T>(List<T> list, int from, int to)
    {
        if (from == to || from < 0 || to < 0 || from >= list.Count || to >= list.Count)
            return false;
        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
        return true;
    }

    /// <summary>
    /// Whether a param list is a list of calls (every entry an effect or action call: 37 / 38, DES 31 / 32), whose
    /// entries may be added or removed. Other param lists have the fixed slots of their action's layout.
    /// </summary>
    public static bool IsCallList(FFXDLSE.ParamList list) =>
        list?.Params.Count > 0 && list.Params.All(p => p is FFXDLSE.Param37 or FFXDLSE.Param38);

    // ---------------------------------------------------------------- templates

    /// <summary>
    /// A new action from vanilla: for each action id, the instance that occurs most often in the given effects
    /// (compared by its written bytes), so a new action starts as a real, common one of that game. Built once per
    /// game from the loaded effects.
    /// </summary>
    public sealed class Templates
    {
        private readonly FFXDLSE.StreamFormat format;
        private readonly Dictionary<int, FFXDLSE.Action> byId = new();
        private readonly Dictionary<int, int> uses = new();

        public Templates(FFXDLSE.StreamFormat format, IEnumerable<FFXDLSE.FXEffect> effects)
        {
            this.format = format;
            var counts = new Dictionary<int, Dictionary<string, (int Count, FFXDLSE.Action First)>>();
            foreach (var fx in effects)
                foreach (var a in AllActions(fx))
                {
                    string key = Convert.ToBase64String(Bytes(a));
                    if (!counts.TryGetValue(a.ID, out var forms))
                        counts[a.ID] = forms = new();
                    forms[key] = forms.TryGetValue(key, out var f) ? (f.Count + 1, f.First) : (1, a);
                    uses[a.ID] = uses.GetValueOrDefault(a.ID) + 1;
                }
            foreach (var (id, forms) in counts)
                byId[id] = forms.Values.OrderByDescending(f => f.Count).First().First;
        }

        /// <summary>The action ids with a template, with how often each occurs.</summary>
        public IReadOnlyDictionary<int, int> Uses => uses;

        /// <summary>A new copy of the template for <paramref name="id"/>, or null when the game has none.</summary>
        public FFXDLSE.Action New(int id) => byId.TryGetValue(id, out var a) ? FFXDLSE.Clone(a, format) : null;

        private byte[] Bytes(FFXDLSE.Action a)
        {
            // the action alone, in its own class table: equal bytes = equal action
            var fx = new FFXDLSE { Format = format };
            fx.Effect.StateMap.States.Add(new FFXDLSE.State { Actions = { a } });
            return fx.Write();
        }
    }

    /// <summary>
    /// Every action of an effect: the states' actions and every nested action call, depth first, through effect calls
    /// (Param37) too, which hold most of them. A nested call is returned as an action over the call's own param list
    /// (the same object, so an edit through it is an edit of the call).
    /// </summary>
    public static IEnumerable<FFXDLSE.Action> AllActions(FFXDLSE.FXEffect fx)
    {
        foreach (var s in fx.StateMap.States)
            foreach (var a in s.Actions)
                foreach (var x in Walk(a.ID, a.ParamList, a))
                    yield return x;
        static IEnumerable<FFXDLSE.Action> Walk(int id, FFXDLSE.ParamList list, FFXDLSE.Action self)
        {
            if (self != null)
                yield return self;
            foreach (var p in list?.Params ?? [])
            {
                if (p is FFXDLSE.Param38 call && call.ParamList != null)
                    foreach (var x in Walk(call.ActionID, call.ParamList, new FFXDLSE.Action { ID = call.ActionID, ParamList = call.ParamList }))
                        yield return x;
                else if (p is FFXDLSE.Param37 effect && effect.ParamList != null)
                    foreach (var x in Walk(0, effect.ParamList, null))
                        yield return x;
            }
        }
    }

    // ---------------------------------------------------------------- snapshots

    /// <summary>A whole-effect snapshot for undo: the file written to bytes.</summary>
    public static byte[] Snapshot(FFXDLSE file) => file.Write();

    /// <summary>Restores a snapshot into the same file object (its effect is replaced; adapters must be rebuilt).</summary>
    public static void Restore(FFXDLSE file, byte[] snapshot)
    {
        var read = FFXDLSE.Read(snapshot);
        file.Format = read.Format;
        file.Effect = read.Effect;
    }
}
