using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

public enum EmevdProblemKind
{
    /// <summary>A skip runs past the end of the event</summary>
    SkipPastEnd,
    /// <summary>A goto has no matching label after it</summary>
    GotoNoLabel,
    /// <summary>A parameter points at no instruction</summary>
    ParamNoInstruction,
    /// <summary>A parameter's target byte isn't the start of an arg, or it runs past the args</summary>
    ParamNotOnArg,
    /// <summary>No layout decodes the instruction</summary>
    Undecoded,
    /// <summary>An Initialize Event passes fewer argument bytes than the started event's parameters read</summary>
    CallTooFewArgs,
}

public record EmevdProblem(EmevdProblemKind Kind, long EventId, int Line, string Detail);

/// <summary>
/// The checks pass (Claude/Documentation/EMEVD Block Editor Analysis.md §4): the mistakes an edit can make that the
/// game wouldn't report, it would just behave differently. Pure, so EmevdEditorCheck runs it on every vanilla event
/// and around every test edit.
/// </summary>
public static class EmevdChecks
{
    /// <param name="findEvent">Finds the event an Initialize Event starts (own file, common_func, common), or null</param>
    public static List<EmevdProblem> Run(EmevdScript script, Func<long, EMEVD.Event> findEvent = null)
    {
        var problems = new List<EmevdProblem>();
        var id = script.Event.ID;

        foreach (var span in script.Spans.Where(s => s.Broken))
        {
            problems.Add(new(span.IsGoto ? EmevdProblemKind.GotoNoLabel : EmevdProblemKind.SkipPastEnd, id, span.From,
                span.IsGoto ? $"label {script.Lines[span.From].GotoLabel}" : $"{script.Lines[span.From].SkipCount}"));
        }

        foreach (var line in script.Lines.Where(l => !l.Decoded.Fits))
            problems.Add(new(EmevdProblemKind.Undecoded, id, line.Index, line.Key));

        foreach (var p in script.Event.Parameters)
        {
            if (p.InstructionIndex < 0 || p.InstructionIndex >= script.Lines.Count)
            {
                problems.Add(new(EmevdProblemKind.ParamNoInstruction, id, -1, $"{p.InstructionIndex}"));
                continue;
            }

            var line = script.Lines[(int)p.InstructionIndex];
            var length = line.Instr.ArgData?.Length ?? 0;

            if (p.TargetStartByte + p.ByteCount > length || (line.Decoded.Fits && !line.Decoded.Args.Any(a => a.Offset == p.TargetStartByte)))
                problems.Add(new(EmevdProblemKind.ParamNotOnArg, id, line.Index, $"+{p.TargetStartByte:X2}"));
        }

        if (findEvent != null)
        {
            foreach (var line in script.Lines.Where(l => l.Kind == EmevdBlockKind.Call && !l.CallTargetIsParam && l.CallTarget >= 0))
            {
                var target = findEvent(line.CallTarget);
                if (target == null || target.Parameters.Count == 0)
                    continue;

                var needed = target.Parameters.Max(p => p.SourceStartByte + p.ByteCount);
                var passed = (line.Instr.ArgData?.Length ?? 0) - (line.FlowArgOffset + 4);

                if (passed < needed)
                    problems.Add(new(EmevdProblemKind.CallTooFewArgs, id, line.Index, $"{passed}/{needed}"));
            }
        }

        return problems;
    }
}
