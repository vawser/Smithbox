using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// A structural edit: add, duplicate, remove or move records, resize a blob, or anything that renumbers index
/// references. The edit runs once; its effect is recorded as a snapshot of the file's structure before and after,
/// and undo/redo restore those snapshots onto the same record objects. That keeps the field actions on the undo
/// stack (which hold record objects) valid, and avoids writing an inverse for every remap.
/// </summary>
public class BhvStructureChange : EditorAction
{
    private readonly BhvFileWrapper File;
    private readonly Action<BHV> Apply;
    private Snapshot Before;
    private Snapshot After;

    /// <summary>Called after execute, undo and redo, e.g. to select what the edit made</summary>
    public Action<bool> OnDone;

    public BhvStructureChange(BhvFileWrapper file, Action<BHV> apply)
    {
        File = file;
        Apply = apply;
    }

    public override ActionEvent Execute()
    {
        if (After == null)
        {
            Before = Snapshot.Take(File.BHV);
            Apply(File.BHV);
            After = Snapshot.Take(File.BHV);
        }
        else
        {
            After.Restore(File.BHV);
        }

        File.MarkChanged();
        OnDone?.Invoke(false);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Restore(File.BHV);
        File.MarkUndone();
        OnDone?.Invoke(true);
        return ActionEvent.NoEvent;
    }

    /// <summary>
    /// Everything a structural edit can change: list memberships and order, transition targets, blob array
    /// references, and the contents of the blobs index remaps write into (preamble, state bodies).
    /// Field values on records (IDs, condition bytes) are left to the field actions.
    /// </summary>
    private class Snapshot
    {
        private List<BHV.State> States;
        private List<byte[]> Animations;
        private List<byte[]> StructCs;
        private List<BHV.StructD> StructDs;
        private byte[] Preamble;
        private byte[] PreambleBytes;

        private readonly List<(BHV.State State, byte[] Body, byte[] BodyBytes, byte[] Data, List<BHV.Transition> Transitions)> StateParts = new();
        private readonly List<(BHV.Transition Transition, int Target, byte[] Params, List<BHV.Condition> Conditions)> TransitionParts = new();
        private readonly List<(BHV.Condition Condition, byte[] Data)> ConditionParts = new();
        private readonly List<(BHV.StructD D, List<byte[]> DAs)> StructDParts = new();

        public static Snapshot Take(BHV b)
        {
            var s = new Snapshot
            {
                States = new(b.States),
                Animations = new(b.Animations),
                StructCs = new(b.StructCs),
                StructDs = new(b.StructDs),
                Preamble = b.Preamble,
                PreambleBytes = (byte[])b.Preamble.Clone(),
            };

            foreach (var state in b.States)
            {
                s.StateParts.Add((state, state.Body, (byte[])state.Body.Clone(), state.Data, new(state.Transitions)));

                foreach (var t in state.Transitions)
                {
                    s.TransitionParts.Add((t, t.TargetStateIndex, t.Params, new(t.Conditions)));

                    foreach (var c in t.Conditions)
                        s.ConditionParts.Add((c, c.Data));
                }
            }

            foreach (var d in b.StructDs)
                s.StructDParts.Add((d, new(d.StructDAs)));

            return s;
        }

        public void Restore(BHV b)
        {
            Replace(b.States, States);
            Replace(b.Animations, Animations);
            Replace(b.StructCs, StructCs);
            Replace(b.StructDs, StructDs);

            b.Preamble = Preamble;
            PreambleBytes.CopyTo(Preamble, 0);

            foreach (var (state, body, bodyBytes, data, transitions) in StateParts)
            {
                state.Body = body;
                bodyBytes.CopyTo(body, 0);
                state.Data = data;
                Replace(state.Transitions, transitions);
            }

            foreach (var (t, target, parameters, conditions) in TransitionParts)
            {
                t.TargetStateIndex = target;
                t.Params = parameters;
                Replace(t.Conditions, conditions);
            }

            foreach (var (c, data) in ConditionParts)
                c.Data = data;

            foreach (var (d, das) in StructDParts)
                Replace(d.StructDAs, das);
        }

        private static void Replace<T>(List<T> target, List<T> source)
        {
            target.Clear();
            target.AddRange(source);
        }
    }
}
