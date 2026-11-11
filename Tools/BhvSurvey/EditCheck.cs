using SoulsFormats;

// Edits every file through SoulsFormats.BHV, writes, re-reads and checks the graph survived; also prints format facts.
public static class EditCheck
{
    public static void Run(IEnumerable<(string Name, byte[] Bytes)> files)
    {
        int ok = 0, total = 0;
        var facts = new SortedDictionary<string, int>();
        void Fact(string k) => facts[k] = facts.GetValueOrDefault(k) + 1;
        foreach (var (name, bytes) in files)
        {
            total++;
            var bhv = BHV.Read(bytes);
            Fact($"v{bhv.Version} target<count: {bhv.States.SelectMany(s => s.Transitions).All(t => t.TargetStateIndex >= 0 && t.TargetStateIndex < bhv.States.Count)}");
            Fact($"v{bhv.Version} id==index: {bhv.States.Select((s, i) => s.ID == i).All(x => x)}");
            foreach (var t in bhv.States.SelectMany(s => s.Transitions)) Fact($"v{bhv.Version} params type {t.Params[1]} len {t.Params.Length:X}");
            if (bhv.Strings != null) Fact($"strings first: {string.Join(",", bhv.Strings.Take(3))}");

            // Edit: grow first state's data, drop a transition, add a condition with data, add a B and a D with a DA
            if (bhv.States.Count > 0) bhv.States[0].Data = [.. bhv.States[0].Data, 1, 2, 3, 4];
            var withT = bhv.States.FirstOrDefault(s => s.Transitions.Count > 0);
            withT?.Transitions.RemoveAt(withT.Transitions.Count - 1);
            var tr = bhv.States.SelectMany(s => s.Transitions).FirstOrDefault();
            tr?.Conditions.Add(new BHV.Condition { Type = 99, Data = [9, 9, 9, 9, 9, 9, 9, 9] });
            bhv.Animations.Add(new byte[bhv.AnimationSize]);
            if (bhv.Preamble.Length > 0) bhv.StructDs.Add(new BHV.StructD { Unk10 = 7, StructDAs = [new byte[bhv.StructDASize]] });

            var again = BHV.Read(bhv.Write());
            bool same = Signature(bhv) == Signature(again);
            if (same) ok++; else Console.WriteLine($"  EDIT MISMATCH {name}");
        }
        Console.WriteLine($"  edit check: {ok}/{total} graphs survived");
        foreach (var (k, v) in facts) Console.WriteLine($"  {v,7} {k}");
    }

    static string Signature(BHV b)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{b.Version} {Convert.ToHexString(b.Preamble)} {b.StructCSize}|");
        foreach (var s in b.States)
        {
            sb.Append($"S{s.ID},{s.Kind},{Convert.ToHexString(s.Body)},{Convert.ToHexString(s.Data)}");
            foreach (var t in s.Transitions)
            {
                sb.Append($"T{t.TargetStateIndex},{Convert.ToHexString(t.Params)}");
                foreach (var c in t.Conditions) sb.Append($"C{c.Type},{c.Unk02}{c.Unk03}{c.Unk08}{c.Unk0B},{Convert.ToHexString(c.Data)}");
            }
        }
        foreach (var x in b.Animations) sb.Append("B" + Convert.ToHexString(x));
        foreach (var x in b.StructCs) sb.Append("C" + Convert.ToHexString(x));
        foreach (var d in b.StructDs) { sb.Append($"D{d.Unk10}"); foreach (var a in d.StructDAs) sb.Append("A" + Convert.ToHexString(a)); }
        if (b.Strings != null) sb.Append("Str" + string.Join("\0", b.Strings));
        return sb.ToString();
    }
}
