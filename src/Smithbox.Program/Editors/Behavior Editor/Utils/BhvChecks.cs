using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

public enum BhvSeverity { Error, Warning, Note }

/// <summary>
/// A problem in a BHV. <see cref="Code"/> names the localization key (BHV_Check_{Code}), formatted with
/// <see cref="Args"/>. The location fields follow <see cref="BhvRecordRef"/>.
/// </summary>
public record BhvFinding(BhvSeverity Severity, string Code, BhvRecordKind Kind, int State, int Transition, int Condition,
    int Index, object[] Args);

/// <summary>
/// What a check can look up outside the file. Each returns null when the data isn't available (Param Editor off,
/// character not in the Time Act Editor), and the check is then skipped.
/// </summary>
public class BhvCheckContext
{
    public Func<long, bool?> HasTaeAnimation = _ => null;
    public Func<int, bool?> HasChangeStateMatrixRow = _ => null;
    public Func<int, bool?> HasSpEffectRow = _ => null;
}

/// <summary>
/// The Problems check. Errors are things the game reads as an index past a list (vanilla has none: Tools/BhvSurvey
/// `checks`). Warnings are references to data outside the file that wasn't found. Notes are normal in vanilla
/// files but worth seeing when editing: states entered only by code, transitions to the state itself (the game
/// skips them) and type 0 conditions with no data (the editor's Add Condition default). Vanilla baseline (2068 files):
/// 0 errors; 434 missing-TAE warnings in 22 v12 files; notes in most files.
/// </summary>
public static class BhvChecks
{
    public static List<BhvFinding> Check(BHV b, BhvCheckContext ctx)
    {
        var findings = new List<BhvFinding>();
        void Add(BhvSeverity sev, string code, BhvRecordKind kind, int s, int t, int c, int i, params object[] args) =>
            findings.Add(new(sev, code, kind, s, t, c, i, args));

        var incoming = new int[b.States.Count];

        // States: IDs, animation slots, transitions, conditions
        var firstWithId = new Dictionary<short, int>();
        var animationSlots = BhvLayout.BodyAnimationSlots(b);

        for (int s = 0; s < b.States.Count; s++)
        {
            var state = b.States[s];

            if (!firstWithId.TryAdd(state.ID, s))
                Add(BhvSeverity.Error, "Duplicate_State_ID", BhvRecordKind.StateBody, s, -1, -1, -1, state.ID, firstWithId[state.ID]);

            foreach (var off in animationSlots)
            {
                if (off + 2 > state.Body.Length)
                    continue;

                var a = BitConverter.ToInt16(state.Body, off);
                if (a >= b.Animations.Count || a < -1)
                    Add(BhvSeverity.Error, "Animation_Out_Of_Range", BhvRecordKind.StateBody, s, -1, -1, -1, off, a, b.Animations.Count);
            }

            for (int t = 0; t < state.Transitions.Count; t++)
            {
                var tr = state.Transitions[t];

                if (tr.TargetStateIndex < 0 || tr.TargetStateIndex >= b.States.Count)
                    Add(BhvSeverity.Error, "Target_Out_Of_Range", BhvRecordKind.TransitionParams, s, t, -1, -1, tr.TargetStateIndex, b.States.Count);
                else if (tr.TargetStateIndex == s)
                    Add(BhvSeverity.Note, "Self_Transition", BhvRecordKind.TransitionParams, s, t, -1, -1);
                else
                    incoming[tr.TargetStateIndex]++;

                if (b.Version >= 10 && tr.Params.Length >= 8)
                {
                    var matrix = BitConverter.ToInt32(tr.Params, BHV.V12.ParamsChangeStateMatrixId);
                    if (matrix != 0 && ctx.HasChangeStateMatrixRow(matrix) == false)
                        Add(BhvSeverity.Warning, "Missing_Change_State_Matrix", BhvRecordKind.TransitionParams, s, t, -1, -1, matrix);
                }

                for (int c = 0; c < tr.Conditions.Count; c++)
                {
                    var cond = tr.Conditions[c];

                    if (cond.Type == 0 && cond.Data.Length == 0)
                        Add(BhvSeverity.Note, "Empty_Type_0_Condition", BhvRecordKind.ConditionData, s, t, c, -1);

                    if (cond.Type == 43 && b.Version != 10 && cond.Data.Length >= BHV.V12.Condition43SpEffectId + 4)
                    {
                        var sp = BitConverter.ToInt32(cond.Data, BHV.V12.Condition43SpEffectId);
                        if (sp > 0 && ctx.HasSpEffectRow(sp) == false)
                            Add(BhvSeverity.Warning, "Missing_SpEffect", BhvRecordKind.ConditionData, s, t, c, -1, sp);
                    }
                }
            }
        }

        // Preamble state slots
        foreach (var off in BhvLayout.PreambleStateSlots(b))
        {
            if (off + 2 > b.Preamble.Length)
                continue;

            var v = BitConverter.ToInt16(b.Preamble, off);
            if (v < -1 || v >= b.States.Count)
                Add(BhvSeverity.Error, "Preamble_State_Out_Of_Range", BhvRecordKind.Preamble, -1, -1, -1, -1, off, v, b.States.Count);
            else if (v >= 0)
                incoming[v]++;
        }

        // Animation records: TAE IDs (v10+; in v5 the field matches the anibnds only half the time, P0 animmatch).
        // No "unused record" note: most vanilla records are played by nothing in a state body (84k in 1485 files),
        // so they are reached some other way.
        var idFields = b.Version >= 10
            ? BhvLayout.AnimationFields(b, b.Kind is BHVKind.Weapon).Where(f => f.Ref is BhvRefKind.TaeAnimation).ToList()
            : new List<BhvField>();

        for (int a = 0; a < b.Animations.Count; a++)
        {
            foreach (var field in idFields)
            {
                var id = field.Read(b.Animations[a]);
                if (id is >= 0 && ctx.HasTaeAnimation((long)id.Value) == false)
                    Add(BhvSeverity.Warning, "Missing_Tae_Animation", BhvRecordKind.Animation, -1, -1, -1, a, (long)id.Value, field.Offset);
            }

        }

        // States nothing in the file leads to: entered by game code (normal; listed so edits don't orphan states)
        for (int s = 0; s < b.States.Count; s++)
            if (incoming[s] == 0)
                Add(BhvSeverity.Note, "Entered_By_Code", BhvRecordKind.StateBody, s, -1, -1, -1);

        return findings.OrderBy(f => f.Severity).ToList();
    }
}
