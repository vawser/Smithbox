using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>How a block is drawn</summary>
public enum EmevdBlockKind
{
    /// <summary>Bank &lt; 1000: writes a result into a condition group</summary>
    Condition,
    /// <summary>Wait For …: blocks until a condition holds</summary>
    Wait,
    /// <summary>Skip If … / Skip Unconditionally: skips the next n lines</summary>
    Skip,
    /// <summary>End If … / End Unconditionally: ends or restarts the event</summary>
    End,
    /// <summary>Goto If …: jumps forward to a label</summary>
    Goto,
    /// <summary>1014[n]: label n</summary>
    Label,
    /// <summary>Other control-flow instructions (banks 1000-1999)</summary>
    ControlFlow,
    /// <summary>Initialize Event / Initialize Common Event</summary>
    Call,
    /// <summary>Banks 2000+</summary>
    Command,
    /// <summary>No layout, or the bytes fit none of its forms</summary>
    Unknown,
}

/// <summary>A skip or goto, drawn as a bracket in the margin</summary>
public class EmevdSpan
{
    public int From;
    /// <summary>The last line skipped, or the label line a goto lands on</summary>
    public int To;
    public int Lane;
    public bool IsGoto;
    /// <summary>Runs past the end of the event, or a goto whose label isn't defined after it</summary>
    public bool Broken;
}

/// <summary>One instruction, worked out for display</summary>
public class EmevdLine
{
    public int Index;
    public EMEVD.Instruction Instr;
    public EmevdDecoded Decoded;
    public EmevdBlockKind Kind;
    /// <summary>The ending instruction has no condition (End Unconditionally): nothing after it runs</summary>
    public bool IsCap;

    /// <summary>Event parameters written over this instruction's args, by target byte</summary>
    public Dictionary<int, EMEVD.Parameter> Params = new();

    public int SkipCount = -1;
    public int GotoLabel = -1;
    public int LabelId = -1;

    /// <summary>An Initialize Event's target event, unless a parameter supplies it</summary>
    public long CallTarget = -1;
    public bool CallTargetIsParam;

    public string Name => Decoded.Def?.DisplayName ?? $"{Instr.Bank}[{Instr.ID:D2}]";
    public string Key => $"{Instr.Bank}[{Instr.ID:D2}]";
}

/// <summary>
/// An event worked out for the block view: every instruction decoded with the layouts, the skip/goto spans with
/// their bracket lanes, and the event's parameter slots. Flat (P1): structure comes with P3's decompiler.
/// </summary>
public class EmevdScript
{
    public EMEVD.Event Event;
    public List<EmevdLine> Lines = new();
    public List<EmevdSpan> Spans = new();
    public int LaneCount;

    /// <summary>The event's parameter slots (source byte, size), e.g. X0_4, X4_4</summary>
    public List<(long Source, int Size)> Slots = new();

    /// <summary>Parameters that point at no instruction, or no whole arg</summary>
    public int BadParams;
    public int UndecodedLines;

    /// <summary>The arg names (shipped Japanese) that identify special args in every game's layout</summary>
    private const string SkipCountArg = "スキップアクション数";
    private const string LabelEnum = "ラベル";
    private const string CallEventArg = "起動イベントID";

    public static string SlotName(long source, int size) => $"X{source}_{size}";

    public static EmevdScript Build(EmevdCatalog catalog, EMEVD.Event ev)
    {
        var script = new EmevdScript { Event = ev };

        for (var i = 0; i < ev.Instructions.Count; i++)
        {
            var instr = ev.Instructions[i];
            var def = catalog?.Find(instr.Bank, instr.ID);
            var decoded = EmevdDecoded.Decode(def, instr);
            var line = new EmevdLine { Index = i, Instr = instr, Decoded = decoded };

            line.Kind = KindOf(line);

            if (decoded.Fits)
            {
                foreach (var arg in decoded.Args)
                {
                    if (arg.Def.JapaneseName == SkipCountArg)
                        line.SkipCount = (int)arg.Int;
                    else if (arg.Def.EnumKey == LabelEnum && line.Kind == EmevdBlockKind.Goto)
                        line.GotoLabel = (int)arg.Int;
                    else if (arg.Def.JapaneseName == CallEventArg)
                        line.CallTarget = arg.Int;
                }
            }

            if (line.Kind == EmevdBlockKind.Label)
                line.LabelId = instr.ID;

            if (!decoded.Fits)
                script.UndecodedLines++;

            script.Lines.Add(line);
        }

        foreach (var p in ev.Parameters)
        {
            if (p.InstructionIndex < 0 || p.InstructionIndex >= script.Lines.Count)
            {
                script.BadParams++;
                continue;
            }

            var line = script.Lines[(int)p.InstructionIndex];
            line.Params[(int)p.TargetStartByte] = p;

            if (line.Decoded.Fits && !line.Decoded.Args.Any(a => a.Offset == p.TargetStartByte))
                script.BadParams++;

            if (line.Kind == EmevdBlockKind.Call && line.Decoded.Fits)
            {
                var target = line.Decoded.Args.FirstOrDefault(a => a.Def.JapaneseName == CallEventArg);
                if (target.Def != null && target.Offset == p.TargetStartByte)
                    line.CallTargetIsParam = true;
            }
        }

        script.Slots = ev.Parameters
            .Select(p => (p.SourceStartByte, p.ByteCount))
            .Distinct()
            .OrderBy(s => s.SourceStartByte)
            .ToList();

        script.BuildSpans();

        return script;
    }

    private static EmevdBlockKind KindOf(EmevdLine line)
    {
        var def = line.Decoded.Def;
        var bank = line.Instr.Bank;

        if (def == null || !line.Decoded.Fits)
            return EmevdBlockKind.Unknown;

        if (bank < 1000)
            return EmevdBlockKind.Condition;

        if (bank == 1014)
            return EmevdBlockKind.Label;

        if (bank < 2000)
        {
            var jp = def.JapaneseName ?? "";

            if (def.Args.Any(a => a.JapaneseName == SkipCountArg))
                return EmevdBlockKind.Skip;
            if (def.Args.Any(a => a.EnumKey == LabelEnum) && jp.Contains("ラベルジャンプ"))
                return EmevdBlockKind.Goto;
            if (jp.EndsWith("イベント終了"))
            {
                line.IsCap = jp.StartsWith("無条件");
                return EmevdBlockKind.End;
            }
            if (jp.EndsWith("待機"))
                return EmevdBlockKind.Wait;

            return EmevdBlockKind.ControlFlow;
        }

        if (def.Args.Any(a => a.JapaneseName == CallEventArg))
            return EmevdBlockKind.Call;

        return EmevdBlockKind.Command;
    }

    /// <summary>
    /// Skips cover the next n lines; gotos run to the first matching label after them. Each span gets the lowest
    /// lane free over its lines, so overlapping brackets sit side by side.
    /// </summary>
    private void BuildSpans()
    {
        foreach (var line in Lines)
        {
            if (line.Kind == EmevdBlockKind.Skip && line.SkipCount > 0 && !line.Params.ContainsKey(SkipOffset(line)))
            {
                var to = line.Index + line.SkipCount;
                Spans.Add(new EmevdSpan { From = line.Index, To = Math.Min(to, Lines.Count - 1), Broken = to >= Lines.Count });
            }
            else if (line.Kind == EmevdBlockKind.Goto && line.GotoLabel >= 0)
            {
                var label = Lines.Skip(line.Index + 1).FirstOrDefault(l => l.LabelId == line.GotoLabel);
                Spans.Add(new EmevdSpan
                {
                    From = line.Index,
                    To = label?.Index ?? Lines.Count - 1,
                    IsGoto = true,
                    Broken = label == null
                });
            }
        }

        // Longest first, so the outer brackets get the outer lanes
        var lanes = new List<List<EmevdSpan>>();

        foreach (var span in Spans.OrderByDescending(s => s.To - s.From).ThenBy(s => s.From))
        {
            var lane = 0;
            while (lane < lanes.Count && lanes[lane].Any(o => o.From <= span.To && span.From <= o.To))
                lane++;

            if (lane == lanes.Count)
                lanes.Add(new());

            lanes[lane].Add(span);
            span.Lane = lane;
        }

        LaneCount = lanes.Count;
    }

    private static int SkipOffset(EmevdLine line)
    {
        foreach (var a in line.Decoded.Args)
        {
            if (a.Def.JapaneseName == SkipCountArg)
                return a.Offset;
        }

        return -1;
    }
}
