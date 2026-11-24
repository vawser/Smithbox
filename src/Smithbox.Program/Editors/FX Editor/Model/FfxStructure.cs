using SoulsFormats;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Structure edits on a DLSE effect (DES / DS2 / DS2S; FX.md, FFX §21, F7a): states, exits, a state's
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

    // ---------------------------------------------------------------- value types (§21.7)

    /// <summary>The value family of a body class: the classes a slot may switch between keep their family.</summary>
    public static string Family(int bodyClass) => bodyClass switch
    {
        1 or 5 or 6 => "int",
        7 or 9 or 11 or 12 or 13 => "float",
        15 or 17 or 18 or 19 or 20 or 21 => "colour",
        _ => null,
    };

    /// <summary>The DS2-scheme class of a file type id (DES ids differ; <see cref="FFXDLSE.Param.DesBodyClass"/>).</summary>
    public static int BodyClass(int fileType, FFXDLSE.StreamFormat format) =>
        format == FFXDLSE.StreamFormat.DES && FFXDLSE.Param.DesBodyClass.TryGetValue(fileType, out int b) ? b : fileType;

    /// <summary>Whether a param can become <paramref name="fileType"/>: a different type of the same family.</summary>
    public static bool CanChangeType(FFXDLSE.Param param, int fileType, FFXDLSE.StreamFormat format)
    {
        int from = BodyClass(param.FileType, format), to = BodyClass(fileType, format);
        return from != to && Family(from) is { } f && Family(to) == f;
    }

    /// <summary>
    /// Replaces param <paramref name="index"/> of a list with one of <paramref name="fileType"/>, keeping its value:
    /// a constant becomes one key at time 0, a sequence becomes a constant of its first key, sequences keep their keys,
    /// and the cubic types (13, 21) get zero tangents (dropped going the other way). Ints round. Returns false when
    /// the types aren't of one family.
    /// </summary>
    public static bool ChangeType(FFXDLSE.ParamList list, int index, int fileType, FFXDLSE.StreamFormat format)
    {
        if (list == null || index < 0 || index >= list.Params.Count || !CanChangeType(list.Params[index], fileType, format))
            return false;
        var keys = Keys(list.Params[index]);
        if (keys.Count == 0)
            keys.Add((0, new float[4]));
        var made = Make(BodyClass(fileType, format), keys);
        if (made == null)
            return false;
        made.FileType = fileType;
        list.Params[index] = made;
        return true;
    }

    private static List<(float Tick, float[] V)> Keys(FFXDLSE.Param p)
    {
        static float[] C(FFXDLSE.PrimitiveColor c) => new[] { c.R, c.G, c.B, c.A };
        return p switch
        {
            FFXDLSE.Param1 x => [(0, new float[] { x.Int })],
            FFXDLSE.Param5 x => x.TickInts.Select(k => (k.Tick, new float[] { k.Int })).ToList(),
            FFXDLSE.Param6 x => x.TickInts.Select(k => (k.Tick, new float[] { k.Int })).ToList(),
            FFXDLSE.Param7 x => [(0, new[] { x.Float })],
            FFXDLSE.Param9 x => x.TickFloats.Select(k => (k.Tick, new[] { k.Float })).ToList(),
            FFXDLSE.Param11 x => x.TickFloats.Select(k => (k.Tick, new[] { k.Float })).ToList(),
            FFXDLSE.Param12 x => x.TickFloats.Select(k => (k.Tick, new[] { k.Float })).ToList(),
            FFXDLSE.Param13 x => x.TickFloat3s.Select(k => (k.Tick, new[] { k.Float1 })).ToList(),
            FFXDLSE.Param15 x => [(0, C(x.Color))],
            FFXDLSE.Param17 x => x.TickColors.Select(k => (k.Tick, C(k.Color))).ToList(),
            FFXDLSE.Param18 x => x.TickColors.Select(k => (k.Tick, C(k.Color))).ToList(),
            FFXDLSE.Param19 x => x.TickColors.Select(k => (k.Tick, C(k.Color))).ToList(),
            FFXDLSE.Param20 x => x.TickColors.Select(k => (k.Tick, C(k.Color))).ToList(),
            FFXDLSE.Param21 x => x.TickColor3s.Select(k => (k.Tick, C(k.Color1))).ToList(),
            _ => [],
        };
    }

    private static FFXDLSE.Param Make(int bodyClass, List<(float Tick, float[] V)> keys)
    {
        static FFXDLSE.PrimitiveColor Col(float[] v) => new() { R = At(v, 0), G = At(v, 1), B = At(v, 2), A = v.Length > 3 ? v[3] : 1 };
        static float At(float[] v, int i) => i < v.Length ? v[i] : v.Length > 0 ? v[0] : 0;
        static int I(float[] v) => (int)MathF.Round(At(v, 0));
        var first = keys[0].V;
        return bodyClass switch
        {
            1 => new FFXDLSE.Param1 { Int = I(first) },
            5 => new FFXDLSE.Param5 { TickInts = keys.Select(k => new FFXDLSE.TickInt(k.Tick, I(k.V))).ToList() },
            6 => new FFXDLSE.Param6 { TickInts = keys.Select(k => new FFXDLSE.TickInt(k.Tick, I(k.V))).ToList() },
            7 => new FFXDLSE.Param7 { Float = At(first, 0) },
            9 => new FFXDLSE.Param9 { TickFloats = keys.Select(k => new FFXDLSE.TickFloat(k.Tick, At(k.V, 0))).ToList() },
            11 => new FFXDLSE.Param11 { TickFloats = keys.Select(k => new FFXDLSE.TickFloat(k.Tick, At(k.V, 0))).ToList() },
            12 => new FFXDLSE.Param12 { TickFloats = keys.Select(k => new FFXDLSE.TickFloat(k.Tick, At(k.V, 0))).ToList() },
            13 => new FFXDLSE.Param13 { TickFloat3s = keys.Select(k => new FFXDLSE.TickFloat3(k.Tick, At(k.V, 0), 0, 0)).ToList() },
            15 => new FFXDLSE.Param15 { Color = Col(first) },
            17 => new FFXDLSE.Param17 { TickColors = keys.Select(k => new FFXDLSE.TickColor(k.Tick, Col(k.V))).ToList() },
            18 => new FFXDLSE.Param18 { TickColors = keys.Select(k => new FFXDLSE.TickColor(k.Tick, Col(k.V))).ToList() },
            19 => new FFXDLSE.Param19 { TickColors = keys.Select(k => new FFXDLSE.TickColor(k.Tick, Col(k.V))).ToList() },
            20 => new FFXDLSE.Param20 { TickColors = keys.Select(k => new FFXDLSE.TickColor(k.Tick, Col(k.V))).ToList() },
            21 => new FFXDLSE.Param21 { TickColor3s = keys.Select(k => new FFXDLSE.TickColor3(k.Tick, Col(k.V), new FFXDLSE.PrimitiveColor(), new FFXDLSE.PrimitiveColor())).ToList() },
            _ => null,
        };
    }

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

        private FFXDLSE.Trigger exit;
        private int exitUses;


        /// <summary>
        /// A new exit: the commonest vanilla exit condition (compared with its target set aside), ending the effect
        /// (target −1). Its target and values are then edited as any exit's (F3c). Null when no effect has an exit.
        /// </summary>
        public FFXDLSE.Trigger NewExit()
        {
            if (exit == null)
                return null;
            var t = FFXDLSE.Clone(exit, format);
            t.StateIndex = -1;
            return t;
        }

        /// <summary>How often the exit template's condition occurs.</summary>
        public int ExitUses => exitUses;

        /// <summary>Collects the exit template from the effects (called by the constructor's caller once).</summary>
        public Templates WithExits(IEnumerable<FFXDLSE.FXEffect> effects)
        {
            var forms = new Dictionary<string, (int Count, FFXDLSE.Trigger First)>();
            foreach (var fx in effects)
                foreach (var t in fx.StateMap.States.SelectMany(s => s.Triggers))
                {
                    int target = t.StateIndex;
                    t.StateIndex = -1;
                    string key = Convert.ToBase64String(ExitBytes(t));
                    t.StateIndex = target;
                    forms[key] = forms.TryGetValue(key, out var f) ? (f.Count + 1, f.First) : (1, t);
                }
            if (forms.Count > 0)
            {
                var best = forms.Values.OrderByDescending(f => f.Count).First();
                exit = FFXDLSE.Clone(best.First, format);
                exitUses = best.Count;
            }
            return this;
        }

        private byte[] ExitBytes(FFXDLSE.Trigger t)
        {
            var fx = new FFXDLSE { Format = format };
            fx.Effect.StateMap.States.Add(new FFXDLSE.State { Triggers = { t } });
            return fx.Write();
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
