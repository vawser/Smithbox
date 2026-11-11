using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>Which raw blob of a BHV a record is</summary>
public enum BhvRecordKind { Preamble, StateBody, StateData, TransitionParams, ConditionData, Animation, StructC, StructDA }

/// <summary>
/// One raw blob of a BHV, where it sits, and its known fields. <see cref="Index"/> is the animation, struct C or
/// struct D index; <see cref="SubIndex"/> the DA within a struct D. Unused indices are -1.
/// </summary>
public record BhvRecordRef(BhvRecordKind Kind, int State, int Transition, int Condition, int Index, int SubIndex,
    byte[] Blob, List<BhvField> Fields);

/// <summary>
/// Every raw blob of a file with its known fields, for Search, Used By and Problems. No editor state, so
/// Tools/BhvSurvey `checks` runs the same code.
/// </summary>
public static class BhvRecords
{
    public static IEnumerable<BhvRecordRef> Enumerate(BHV b)
    {
        yield return new(BhvRecordKind.Preamble, -1, -1, -1, -1, -1, b.Preamble, BhvLayout.PreambleFields(b));

        var bodyFields = BhvLayout.StateBodyFields(b);

        for (int s = 0; s < b.States.Count; s++)
        {
            var state = b.States[s];
            yield return new(BhvRecordKind.StateBody, s, -1, -1, -1, -1, state.Body, bodyFields);
            yield return new(BhvRecordKind.StateData, s, -1, -1, -1, -1, state.Data, new());

            for (int t = 0; t < state.Transitions.Count; t++)
            {
                var tr = state.Transitions[t];
                yield return new(BhvRecordKind.TransitionParams, s, t, -1, -1, -1, tr.Params, BhvLayout.TransitionParamFields(b, tr.Params));

                for (int c = 0; c < tr.Conditions.Count; c++)
                    yield return new(BhvRecordKind.ConditionData, s, t, c, -1, -1, tr.Conditions[c].Data, BhvLayout.ConditionDataFields(b, tr.Conditions[c]));
            }
        }

        var animationFields = BhvLayout.AnimationFields(b, b.Kind is BHVKind.Weapon);
        for (int a = 0; a < b.Animations.Count; a++)
            yield return new(BhvRecordKind.Animation, -1, -1, -1, a, -1, b.Animations[a], animationFields);

        for (int i = 0; i < b.StructCs.Count; i++)
            yield return new(BhvRecordKind.StructC, -1, -1, -1, i, -1, b.StructCs[i], new());

        for (int i = 0; i < b.StructDs.Count; i++)
            for (int k = 0; k < b.StructDs[i].StructDAs.Count; k++)
                yield return new(BhvRecordKind.StructDA, -1, -1, -1, i, k, b.StructDs[i].StructDAs[k], new());
    }

    /// <summary>Every offset where <paramref name="pattern"/> occurs in a blob</summary>
    public static IEnumerable<int> FindBytes(byte[] blob, byte[] pattern)
    {
        if (pattern.Length == 0 || blob.Length < pattern.Length)
            yield break;

        for (int i = 0; i <= blob.Length - pattern.Length; i++)
            if (blob.AsSpan(i, pattern.Length).SequenceEqual(pattern))
                yield return i;
    }
}
