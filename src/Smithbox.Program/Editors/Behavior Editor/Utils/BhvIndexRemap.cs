using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Keeps the known index references valid when states or animations are inserted, moved or deleted. Ported from
/// Tools/BhvSurvey/Renumber.cs, which passed on every AC6 file (insert + delete and move + back are byte-identical;
/// delete keeps every other record). The slots are the fixed table in <see cref="BhvLayout"/>.
/// References in undecoded data can't be remapped; the editor warns before structural edits.
/// </summary>
public static class BhvIndexRemap
{
    /// <summary>
    /// Every known state reference outside transitions: (getter, setter) pairs on the preamble
    /// </summary>
    private static IEnumerable<(Func<int> Get, Action<int> Set)> StateRefs(BHV b)
    {
        foreach (var off in BhvLayout.PreambleStateSlots(b))
        {
            var o = off;
            if (o + 2 > b.Preamble.Length)
                continue;

            yield return (() => BitConverter.ToInt16(b.Preamble, o), v => BitConverter.TryWriteBytes(b.Preamble.AsSpan(o), (short)v));
        }
    }

    private static IEnumerable<(Func<int> Get, Action<int> Set)> AnimationRefs(BHV b)
    {
        var slots = BhvLayout.BodyAnimationSlots(b);

        foreach (var s in b.States)
        {
            var body = s.Body;
            foreach (var off in slots)
            {
                var o = off;
                yield return (() => BitConverter.ToInt16(body, o), v => BitConverter.TryWriteBytes(body.AsSpan(o), (short)v));
            }
        }
    }

    /// <summary>
    /// Applies <paramref name="map"/> (old index → new index, −1 if gone) to every state reference. Transitions to a
    /// gone state are removed when <paramref name="dropTransitions"/>. Returns how many non-transition references
    /// were set to −1.
    /// </summary>
    public static int RemapStates(BHV b, Func<int, int> map, bool dropTransitions)
    {
        int dangling = 0;

        foreach (var s in b.States)
        {
            if (dropTransitions)
                s.Transitions.RemoveAll(t => map(t.TargetStateIndex) < 0);

            foreach (var t in s.Transitions)
                t.TargetStateIndex = map(t.TargetStateIndex);
        }

        foreach (var (get, set) in StateRefs(b))
        {
            var v = get();
            if (v < 0)
                continue;

            var n = map(v);
            if (n < 0)
                dangling++;
            set(n);
        }

        return dangling;
    }

    /// <summary>
    /// Applies <paramref name="map"/> to every animation reference in state bodies. Returns how many were set to −1.
    /// </summary>
    public static int RemapAnimations(BHV b, Func<int, int> map)
    {
        int dangling = 0;

        foreach (var (get, set) in AnimationRefs(b))
        {
            var v = get();
            if (v < 0)
                continue;

            var n = map(v);
            if (n < 0)
                dangling++;
            set(n);
        }

        return dangling;
    }

    /// <summary>How references move when the item at <paramref name="from"/> moves to <paramref name="to"/></summary>
    public static int MoveMap(int i, int from, int to)
    {
        if (i == from) return to;
        if (from < to && i > from && i <= to) return i - 1;
        if (to < from && i >= to && i < from) return i + 1;
        return i;
    }

    public static Func<int, int> InsertMap(int at) => i => i >= at ? i + 1 : i;

    public static Func<int, int> DeleteMap(int at) => i => i == at ? -1 : i > at ? i - 1 : i;

    /// <summary>How many known references point at a state: (transitions, preamble slots)</summary>
    public static (int Transitions, int Preamble) CountStateRefs(BHV b, int index)
    {
        var transitions = b.States.Sum(s => s.Transitions.Count(t => t.TargetStateIndex == index));
        var preamble = StateRefs(b).Count(r => r.Get() == index);
        return (transitions, preamble);
    }

    /// <summary>How many state body slots point at an animation record</summary>
    public static int CountAnimationRefs(BHV b, int index) => AnimationRefs(b).Count(r => r.Get() == index);
}
