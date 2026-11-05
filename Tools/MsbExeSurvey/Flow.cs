using Iced.Intel;
using System.Text;

/// <summary>
/// Follows MSB data through the exe. The MSB loader turns each offset of an entry into a pointer, so the game reads
/// a field as [[entry+sub]+field]. A register holding a known struct is labelled with its kind and an offset into
/// it; loads through a known pointer field give the child kind. Labels flow between functions through the argument
/// registers and rax, iterated to a fixed point.
/// </summary>
public static class Flow
{
    public readonly record struct Label(string Kind, int Off);

    public sealed class Access
    {
        public string Kind;
        public int Off;
        public int Size;
        public bool Write;
        public ulong IP;
        public ulong Func;
        public string Text;
        public string Ctx;
        public List<string> Uses = new();
        public void Use(string u) { if (Uses.Count < 8 && !Uses.Contains(u)) Uses.Add(u); }
    }

    // pointer fields: kind -> offset -> child kind
    static readonly Dictionary<string, Dictionary<int, string>> Ptr = new()
    {
        ["W.Parts"] = new() { [0x8] = "MRC", [0x18] = "Part" },
        ["W.Point"] = new() { [0x8] = "MRC", [0x18] = "Point" },
        ["W.Event"] = new() { [0x8] = "MRC", [0x10] = "Event" },
        ["MRC"] = new() { [0x80] = "Tbl.Model", [0x88] = "Tbl.Event", [0x90] = "Tbl.Point", [0x98] = "Tbl.Part", [0xA0] = "Tbl.Layer", [0xA8] = "Tbl.Route" },
        ["Part"] = new()
        {
            [0x00] = "Part.Name", [0x18] = "Part.Sib", [0x50] = "Part.Display", [0x58] = "Part.DisplayGroup", [0x60] = "Part.Entity", [0x68] = "Part.Type",
            [0x70] = "Part.Gparam", [0x78] = "Part.SceneGparam", [0x80] = "Part.Grass", [0x88] = "Part.Unk8", [0x90] = "Part.Unk9",
            [0x98] = "Part.TileLoad", [0xA0] = "Part.Unk11",
        },
        ["Part.Type"] = new() { [0x68] = "Part.Type.68", [0x70] = "Part.Type.70", [0x78] = "Part.Type.78", [0x80] = "Part.Type.80" },
        ["Point"] = new() { [0x00] = "Point.Name", [0x30] = "Point.A", [0x38] = "Point.B", [0x48] = "Point.Shape", [0x50] = "Point.Entity", [0x58] = "Point.Type", [0x60] = "Point.Unk4" },
        ["Event"] = new() { [0x00] = "Event.Name", [0x18] = "Event.Entity", [0x20] = "Event.Type", [0x28] = "Event.Unk3" },
        ["Model"] = new() { [0x00] = "Model.Name", [0x18] = "Model.Sib" },
        ["Route"] = new() { [0x00] = "Route.Name" },
    };

    // per-type tables of the MsbResCap, from its loader: (first slot, type count, kind)
    static readonly (int Base, int Count, string Kind)[] Slots =
    {
        (0xD0, 11, "Model"), (0x180, 26, "Event"), (0x320, 56, "Point"), (0x6A0, 14, "Part"), (0x780, 7, "Layer"), (0x7F0, 5, "Route"),
    };

    static Flow()
    {
        foreach (var (b, n, k) in Slots)
            for (var t = 0; t < n; t++) Ptr["MRC"][b + t * 16] = "Slot." + k;
    }

    static readonly Dictionary<int, string> TableGuess = new() { [0x80] = "Model", [0x88] = "Event", [0x90] = "Point", [0x98] = "Part", [0xA8] = "Route" };
    // where the type of an entry is
    static readonly Dictionary<string, int> TypeField = new() { ["Part"] = 0xC, ["Point"] = 0x8, ["Event"] = 0xC, ["Route"] = 0x10, ["Model"] = 0x8 };

    public static Dictionary<ulong, Label?[]> Params = new();   // function -> rcx,rdx,r8,r9
    public static Dictionary<ulong, Label?> Returns = new();
    public static List<Access> Accesses = new();
    public static Dictionary<ulong, string> SeedCtx = new();      // function -> wrapper class

    static readonly Register[] ArgRegs = { Register.RCX, Register.RDX, Register.R8, Register.R9 };

    public static void Seed()
    {
        foreach (var (vt, cls) in Exe.VtableClass)
        {
            string kind = null;
            if (cls.StartsWith("CS::CSMsbParts") && !cls.Contains("Pose")) kind = "W.Parts";
            else if (cls.StartsWith("CS::CSMsbPoint") && !cls.Contains("InitParam") && !cls.Contains("Man") && !cls.Contains("Debug") && !cls.Contains("Helper")) kind = "W.Point";
            else if (cls.StartsWith("CS::CSMsbEvent")) kind = "W.Event";
            else if (cls == "CS::MsbResCap") kind = "MRC";
            var msb = kind != null;
            kind ??= "C:" + cls;

            for (var i = 0; i < 200; i++)
            {
                var fn = Exe.ReadU64(vt + (ulong)i * 8);
                if (!Exe.IsCode(fn) || (i > 0 && Exe.VtableClass.ContainsKey(vt + (ulong)i * 8))) break;
                // a function shared by many classes is a base implementation that may not be ours
                if (Exe.VirtualOf[fn].Count > (msb ? 12 : 1)) continue;
                if (!msb && Params.ContainsKey(fn)) continue;
                Params[fn] = new Label?[] { new Label(kind, 0), null, null, null };
                Fixed.Add(fn);
                if (msb) SeedCtx[fn] = cls;
            }
        }
    }

    public static HashSet<ulong> Fixed = new();
    public static HashSet<ulong> Rejected = new();
    static bool IsData(string k) => !IsClass(k) && !k.StartsWith("W.") && !k.StartsWith("Tbl") && !k.StartsWith("Slot.") && k != "MRC";
    static int Limit(string k) => k switch
    {
        "Part" => 0xB8, "Point" => 0x68, "Event" => 0x30, "Model" => 0x30, "Route" => 0x30,
        "Part.Entity" => 0x50, "Point.Entity" => 0x20, "Event.Entity" => 0x20, "Point.Shape" => 0x90,
        _ => 0x200,
    };
    public static Dictionary<(string, int), string> FieldKind = new();      // class field holding a pointer to an MSB kind
    public static Dictionary<(string, int), string> EmbeddedKind = new();   // class field that is an MSB wrapper
    static Dictionary<(string, int, bool), Dictionary<string, int>> Votes = new();

    static bool IsClass(string k) => k.StartsWith("C:") || k == "Null";

    static void Vote(string cls, int off, bool embedded, string kind)
    {
        lock (Votes)
        {
            if (!Votes.TryGetValue((cls, off, embedded), out var d)) Votes[(cls, off, embedded)] = d = new();
            d[kind] = d.GetValueOrDefault(kind) + 1;
        }
    }

    static int ApplyVotes()
    {
        var changed = 0;
        foreach (var ((cls, off, emb), d) in Votes)
        {
            var best = d.OrderByDescending(x => x.Value).First();
            if (best.Value * 4 < d.Values.Sum() * 3) continue;
            var map = emb ? EmbeddedKind : FieldKind;
            if (!map.TryGetValue((cls, off), out var old) || old != best.Key) { map[(cls, off)] = best.Key; changed++; }
        }
        Votes.Clear();
        return changed;
    }

    /// <summary>
    /// For functions in the MSB module, tries each kind on each argument register and keeps a guess whose reads go
    /// deep into the struct without touching anything outside it
    /// </summary>
    static void Probe(List<(ulong Begin, ulong End)> funcs, ulong lo, ulong hi, params string[] kinds)
    {
        var added = 0;
        foreach (var (b, e) in funcs.Where(f => f.Begin >= lo && f.Begin < hi))
        {
            if (Fixed.Contains(b)) continue;
            var p = new Label?[4];
            for (var r = 0; r < 4; r++)
            {
                var scores = new List<(string Kind, int Deep)>();
                foreach (var k in kinds)
                {
                    var trial = (Label?[])p.Clone();
                    trial[r] = new Label(k, 0);
                    var (deep, bad) = Analyse(b, e, new Observed(), false, trial);
                    if (!bad && deep > 0) scores.Add((k, deep));
                }
                if (scores.Count == 0) continue;
                var best = scores.OrderByDescending(s => s.Deep).ToList();
                // a tie goes to the kind listed first, the wrapper of the unit
                if (best.Count > 1 && best[1].Deep == best[0].Deep && Array.IndexOf(kinds, best[0].Kind) > Array.IndexOf(kinds, best[1].Kind)) best = best.Skip(1).ToList();
                p[r] = new Label(best[0].Kind, 0);
            }
            if (p.Any(x => x != null)) { Params[b] = p; Fixed.Add(b); added++; }
        }
        Console.Error.WriteLine($"probe: {added} functions given argument kinds");
    }

    sealed class Observed
    {
        public Dictionary<ulong, List<Label?[]>> CallArgs = new();
        public Dictionary<ulong, List<Label?>> Rets = new();
    }

    public static ulong TraceFn;
    public static void Run(int iterations)
    {
        Seed();
        var funcs = AllFunctions();
        Probe(funcs, 0x140CEF000, 0x140CF5600, "W.Parts", "Part", "MRC");
        Probe(funcs, 0x140CF5600, 0x140CF6300, "W.Event", "Event", "MRC");
        Probe(funcs, 0x140CF6300, 0x140CFBD00, "MRC", "Part", "Point", "Event");
        Probe(funcs, 0x140CFBD00, 0x140D10000, "W.Point", "Point", "MRC");
        for (var it = 0; it < iterations; it++)
        {
            var obs = new Observed();
            Accesses.Clear();
            var collect = it == iterations - 1;
            foreach (var (b, e) in funcs)
                Analyse(b, e, obs, collect);

            var changed = ApplyVotes();
            foreach (var (fn, sites) in obs.CallArgs)
            {
                if (Fixed.Contains(fn)) continue;
                if (Rejected.Contains(fn)) { Params.Remove(fn); continue; }
                var p = new Label?[4];
                for (var i = 0; i < 4; i++)
                {
                    var labels = sites.Select(s => s[i]).ToList();
                    var known = labels.Where(l => l != null).Distinct().ToList();
                    // agreed by every labelled caller, and by most callers when the function has many
                    var msbKnown = known.Where(l => !IsClass(l.Value.Kind)).ToList();
                    if (msbKnown.Count == 1) p[i] = msbKnown[0];
                    else if (msbKnown.Count == 0 && known.Count == 1 && labels.Count(l => l != null) * 3 >= labels.Count) p[i] = known[0];
                }
                if (p.All(x => x == null)) continue;
                if (!Params.TryGetValue(fn, out var old) || !old.SequenceEqual(p)) changed++;
                Params[fn] = p;
            }
            foreach (var (fn, rets) in obs.Rets)
            {
                var known = rets.Where(l => l != null).Distinct().ToList();
                if (Rejected.Contains(fn)) { Returns.Remove(fn); continue; }
                if (known.Count == 1 && !rets.Contains(null))
                {
                    if (!Returns.TryGetValue(fn, out var old) || old != known[0]) changed++;
                    Returns[fn] = known[0];
                }
            }
            Console.Error.WriteLine($"iteration {it}: {Params.Count} functions with args, {Returns.Count} returning, {FieldKind.Count} fields, {EmbeddedKind.Count} embedded, {changed} changed, {Accesses.Count} accesses");
        }
    }

    /// <summary>
    /// The .pdata functions, and the call targets without an entry there (leaf functions have none), each ending
    /// at the ret past its furthest forward branch
    /// </summary>
    public static List<(ulong Begin, ulong End)> AllFunctions()
    {
        var result = new List<(ulong, ulong)>(Extra.Funcs);
        var begins = result.Select(f => f.Item1).ToHashSet();
        var targets = new HashSet<ulong>();
        foreach (var ins in Exe.Sweep())
            if ((ins.FlowControl == FlowControl.Call || ins.FlowControl == FlowControl.UnconditionalBranch) && ins.Op0Kind == OpKind.NearBranch64)
                targets.Add(ins.NearBranchTarget);
        // vtable slots too
        foreach (var fn in Exe.VirtualOf.Keys) targets.Add(fn);
        // the real targets behind Arxan stubs
        var stubTargets = new List<ulong>();
        foreach (var t in targets.ToList())
            if (Arxan.InStubs(t) && Arxan.Resolve(t) is Arxan.Stub st)
            {
                stubTargets.AddRange(st.Targets);
                if (st.IsCall) stubTargets.Add(st.Return);
            }
        foreach (var t in stubTargets) targets.Add(t);
        Console.Error.WriteLine($"{stubTargets.Count} targets behind stubs");


        foreach (var t in targets)
        {
            if (begins.Contains(t) || !Exe.IsCode(t)) continue;
            ulong last = t;
            var n = 0;
            foreach (var ins in Exe.Function(t, 3000)) { last = ins.NextIP; if (ins.IsInvalid) break; n++; }
            if (n > 0) result.Add((t, last));
        }
        Console.Error.WriteLine($"{Extra.Funcs.Count} pdata functions, {result.Count} with leaf targets");
        return result;
    }

    static Label? Child(Label l, int disp, bool indexed)
    {
        var off = l.Off + disp;
        if (l.Kind.StartsWith("TblG."))
            return indexed && disp == 0x10 && l.Off == 0 ? new Label(l.Kind.Substring(5), 0) : null;
        if (l.Kind.StartsWith("Slot."))
            return (off & 7) == 0 && off >= 0 ? new Label(l.Kind.Substring(5), 0) : null;
        if (l.Kind == "MRC" && indexed)
            foreach (var (b, n, k) in Slots)
                if (off == b) return new Label("Slot." + k, 0);
        if (l.Kind.StartsWith("Tbl."))
        {
            // [tbl + i*8 + 10h] is an entry
            if (indexed && disp == 0x10 && l.Off == 0) return new Label(l.Kind.Substring(4), 0);
            if (!indexed && off >= 0x10 && (off & 7) == 0) return new Label(l.Kind.Substring(4), 0);
            return null;
        }
        if (indexed) return null;
        if (Ptr.TryGetValue(l.Kind, out var map) && map.TryGetValue(off, out var c)) return new Label(c, 0);
        if (IsClass(l.Kind))
        {
            if (FieldKind.TryGetValue((l.Kind, off), out var fk)) return new Label(fk, 0);
            if (Embedded(l.Kind, off, out var e, out var ek)) return Child(new Label(ek, 0), off - e, false);
        }
        return null;
    }

    static bool Embedded(string cls, int off, out int at, out string kind)
    {
        for (var e = off; e >= off - 0x60; e -= 8)
            if (EmbeddedKind.TryGetValue((cls, e), out kind)) { at = e; return true; }
        at = 0; kind = null;
        return false;
    }

    static readonly MasmFormatter Fmt = new();

    static (int Deep, bool Bad) Analyse(ulong begin, ulong end, Observed obs, bool collect, Label?[] probe = null)
    {
        var fo = Exe.FileOffset(begin);
        if (fo <= 0) return (0, false);
        var len = (int)(end - begin);
        if (len <= 0 || len > 0x40000) return (0, false);

        var regs = new Dictionary<Register, Label>();
        var vals = new Dictionary<Register, (Label Src, int Size)>();   // registers holding a scalar read from a struct
        var stack = new Dictionary<long, Label>();
        var typeCtx = new SortedSet<string>();
        var fnAccesses = collect || probe != null ? new List<Access>() : null;
        var touched = false;
        var bad = false;
        var useOf = new Dictionary<Register, Access>();
        Access lastAccess = null;

        Label?[] p;
        if (probe != null) p = probe;
        else if (!Params.TryGetValue(begin, out p) || !(Fixed.Contains(begin) || !Rejected.Contains(begin))) p = null;
        if (p != null)
            for (var i = 0; i < 4; i++)
                if (p[i] is Label l) { regs[ArgRegs[i]] = l; touched = true; }

        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, len));
        decoder.IP = begin;
        var output = new StringOutput();

        // state carried to forward branch targets
        var saved = new Dictionary<ulong, (Dictionary<Register, Label> R, Dictionary<Register, (Label, int)> V, Dictionary<long, Label> S)>();
        var dead = false;

        void Save(ulong target)
        {
            if (regs.Count == 0 && vals.Count == 0 && stack.Count == 0 && !saved.ContainsKey(target)) { saved[target] = (new(), new(), new()); return; }
            if (saved.TryGetValue(target, out var s))
            {
                foreach (var k in s.R.Keys.ToList()) if (!regs.TryGetValue(k, out var v) || v != s.R[k]) s.R.Remove(k);
                foreach (var k in s.V.Keys.ToList()) if (!vals.TryGetValue(k, out var v) || v != s.V[k]) s.V.Remove(k);
                foreach (var k in s.S.Keys.ToList()) if (!stack.TryGetValue(k, out var v) || v != s.S[k]) s.S.Remove(k);
            }
            else saved[target] = (new(regs), new(vals), new(stack));
        }

        var pending = new Queue<Instruction>();
        while (pending.Count > 0 || decoder.IP < end)
        {
            var synthetic = pending.Count > 0;
            Instruction ins;
            if (synthetic) ins = pending.Dequeue(); else decoder.Decode(out ins);
            if (begin == TraceFn && collect) Console.WriteLine($"{(synthetic ? "*" : " ")}{Exe.Format(ins)}    | {string.Join(" ", regs.Select(k => $"{k.Key}={k.Value.Kind}+{k.Value.Off:X}"))}");
            if (ins.IsInvalid) break;

            if (!synthetic && saved.Remove(ins.IP, out var st))
            {
                if (dead) { regs = st.R; vals = st.V; stack = st.S; }
                else
                {
                    foreach (var k in regs.Keys.ToList()) if (!st.R.TryGetValue(k, out var v) || v != regs[k]) regs.Remove(k);
                    foreach (var k in vals.Keys.ToList()) if (!st.V.TryGetValue(k, out var v) || v != vals[k]) vals.Remove(k);
                    foreach (var k in stack.Keys.ToList()) if (!st.S.TryGetValue(k, out var v) || v != stack[k]) stack.Remove(k);
                }
            }
            dead = false;

            // an Arxan stub: its original instructions and real branch instead of the jump
            if (!synthetic && ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind == OpKind.NearBranch64
                && (ins.NearBranchTarget < begin || ins.NearBranchTarget >= end) && Arxan.Expand(ins) is List<Instruction> ex)
            {
                foreach (var x in ex) pending.Enqueue(x);
                continue;
            }

            if (ins.Op0Kind == OpKind.NearBranch64 && ins.NearBranchTarget > ins.IP && ins.NearBranchTarget < end
                && (ins.FlowControl == FlowControl.ConditionalBranch || ins.FlowControl == FlowControl.UnconditionalBranch))
            {
                Save(ins.NearBranchTarget);
                if (ins.FlowControl == FlowControl.UnconditionalBranch) dead = true;
                continue;
            }
            if (ins.Mnemonic == Mnemonic.Int3 || (ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind != OpKind.NearBranch64)) dead = true;

            // memory operand with a labelled base
            Label? memBase = null;
            int memDisp = 0;
            bool indexed = false;
            int memOp = -1;
            for (var i = 0; i < ins.OpCount; i++)
                if (ins.GetOpKind(i) == OpKind.Memory) { memOp = i; break; }

            if (memOp >= 0 && ins.MemoryBase != Register.None && ins.MemoryBase != Register.RIP)
            {
                var br = ins.MemoryBase.GetFullRegister();
                indexed = ins.MemoryIndex != Register.None;
                memDisp = (int)(long)ins.MemoryDisplacement64;
                if (regs.TryGetValue(br, out var bl)) memBase = bl;
                else if ((br == Register.RSP || br == Register.RBP) && ins.Mnemonic == Mnemonic.Mov && memOp == 1
                         && stack.TryGetValue(((long)br << 32) + memDisp, out var sl) && ins.Op0Kind == OpKind.Register)
                {
                    // reload of a spilled label
                    regs[ins.Op0Register.GetFullRegister()] = sl;
                    vals.Remove(ins.Op0Register.GetFullRegister());
                    continue;
                }
            }

            if (memBase is Label mb && ins.Mnemonic != Mnemonic.Lea && ins.Mnemonic != Mnemonic.Nop)
            {
                touched = true;
                if (IsData(mb.Kind))
                {
                    var o = mb.Off + memDisp;
                    var w = memOp == 0 && ins.Mnemonic != Mnemonic.Cmp && ins.Mnemonic != Mnemonic.Test && ins.Mnemonic != Mnemonic.Bt;
                    if (!mb.Kind.EndsWith(".Name") && !mb.Kind.EndsWith(".Sib") && (o < 0 || o >= Limit(mb.Kind) || w)) bad = true;
                }
                if (fnAccesses != null && IsData(mb.Kind) && !mb.Kind.EndsWith(".Name") && !mb.Kind.EndsWith(".Sib"))
                {
                    Fmt.Format(ins, output);
                    var write = memOp == 0 && ins.Mnemonic != Mnemonic.Cmp && ins.Mnemonic != Mnemonic.Test && ins.Mnemonic != Mnemonic.Bt;
                    var acc = new Access
                    {
                        Kind = mb.Kind, Off = mb.Off + memDisp, Size = ins.MemorySize.GetSize(), Write = write,
                        IP = ins.IP, Func = begin, Text = output.ToStringAndReset() + (indexed ? " [idx]" : ""),
                    };
                    fnAccesses.Add(acc);
                    lastAccess = acc;
                    if ((ins.Mnemonic == Mnemonic.Cmp || ins.Mnemonic == Mnemonic.Test || ins.Mnemonic == Mnemonic.Bt) && ins.OpCount > 1
                        && ins.GetOpKind(1) != OpKind.Register && ins.GetOpKind(1) != OpKind.Memory)
                        acc.Use($"{ins.Mnemonic.ToString().ToLower()} {(long)ins.GetImmediate(1)}");
                }

                // type checks
                if (TypeField.TryGetValue(mb.Kind, out var tf) && mb.Off + memDisp == tf && ins.Mnemonic == Mnemonic.Cmp && memOp == 0 && ins.GetOpKind(1) != OpKind.Register)
                    typeCtx.Add($"{mb.Kind}={ins.GetImmediate(1)}");
            }

            // register effects
            var dst = ins.OpCount > 0 && ins.Op0Kind == OpKind.Register ? ins.Op0Register.GetFullRegister() : Register.None;
            var flow = ins.FlowControl;

            // what a value read from a field is used for
            if (useOf.Count > 0)
            {
                Access U(Register r) => useOf.TryGetValue(r.GetFullRegister(), out var a) ? a : null;
                if (flow == FlowControl.Call || (flow == FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64 && (ins.NearBranchTarget < begin || ins.NearBranchTarget >= end)))
                {
                    var t = ins.Op0Kind == OpKind.NearBranch64 ? ins.NearBranchTarget : 0;
                    var name = t != 0 && Exe.VirtualOf.TryGetValue(t, out var vv) ? $"({vv[0].Class}#{vv[0].Index})" : t == 0 ? "(indirect)" : "";
                    for (var i = 0; i < 4; i++)
                    {
                        U(ArgRegs[i])?.Use($"arg{i} {t:X}{name}");
                        U(Register.XMM0 + i)?.Use($"farg{i} {t:X}{name}");
                    }
                    if (flow == FlowControl.UnconditionalBranch) { U(Register.RAX)?.Use("ret"); U(Register.XMM0)?.Use("ret"); }
                    foreach (var r in new[] { Register.RAX, Register.RCX, Register.RDX, Register.R8, Register.R9, Register.R10, Register.R11 }) useOf.Remove(r);
                    for (var i = 0; i < 6; i++) useOf.Remove((Register.XMM0 + i).GetFullRegister());
                }
                else if (ins.Mnemonic == Mnemonic.Ret) { U(Register.RAX)?.Use("ret"); U(Register.XMM0)?.Use("ret"); }
                else if (memOp == 0 && ins.OpCount > 1 && ins.GetOpKind(1) == OpKind.Register && U(ins.GetOpRegister(1)) is Access sa)
                {
                    var bdesc = memBase is Label sl2 ? $"{sl2.Kind}+{sl2.Off + memDisp:X}" : $"[{ins.MemoryBase}+{memDisp:X}]";
                    sa.Use($"store {bdesc}");
                }
                else if (dst != Register.None && U(dst) is Access oa && ins.OpCount > 1 && ins.GetOpKind(1) != OpKind.Register && ins.GetOpKind(1) != OpKind.Memory
                         && ins.Mnemonic != Mnemonic.Mov)
                {
                    oa.Use($"{ins.Mnemonic.ToString().ToLower()} {(long)ins.GetImmediate(1)}");
                    if (ins.Mnemonic is Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt) { /* keep */ }
                }
                else if (ins.Mnemonic == Mnemonic.Test && ins.Op0Kind == OpKind.Register && ins.Op1Kind == OpKind.Register && U(ins.Op0Register) is Access ta)
                    ta.Use("test");
                else if (dst != Register.None && ins.OpCount > 1 && ins.GetOpKind(1) == OpKind.Register && U(ins.GetOpRegister(1)) is Access ma
                         && ins.Mnemonic is Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movsxd or Mnemonic.Movss or Mnemonic.Movaps or Mnemonic.Movd or Mnemonic.Cvtsi2ss or Mnemonic.Cvtss2sd)
                    useOf[dst] = ma;
                else if (dst != Register.None && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt))
                {
                    // other arithmetic between registers
                    if (ins.OpCount > 1 && ins.GetOpKind(1) == OpKind.Register && U(ins.GetOpRegister(1)) is Access xa) xa.Use(ins.Mnemonic.ToString().ToLower());
                    if (!(U(dst) is Access da && ins.Mnemonic is Mnemonic.And or Mnemonic.Shr or Mnemonic.Sar or Mnemonic.Shl or Mnemonic.Or)) useOf.Remove(dst);
                }
            }

            if (flow == FlowControl.Call || (flow == FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64 && (ins.NearBranchTarget < begin || ins.NearBranchTarget >= end)))
            {
                if (ins.Op0Kind == OpKind.NearBranch64)
                {
                    var t = ins.NearBranchTarget;
                    var args = new Label?[4];
                    var any = false;
                    Params.TryGetValue(t, out var want);
                    for (var i = 0; i < 4; i++)
                    {
                        var has = regs.TryGetValue(ArgRegs[i], out var al);
                        if (want?[i] is Label w && !IsClass(w.Kind) && w.Off == 0)
                        {
                            // learn where the callee's MSB argument came from
                            if (has && IsClass(al.Kind)) { Vote(al.Kind, al.Off, true, w.Kind); al = w; }
                            else if (!has && vals.TryGetValue(ArgRegs[i], out var src) && src.Size == 8 && IsClass(src.Src.Kind)) Vote(src.Src.Kind, src.Src.Off, false, w.Kind);
                        }
                        if (has && al.Kind != "Null") { args[i] = al; any = true; }
                    }
                    if (!obs.CallArgs.TryGetValue(t, out var list)) obs.CallArgs[t] = list = new();
                    list.Add(any ? args : new Label?[4]);
                    if (flow == FlowControl.UnconditionalBranch)
                    {
                        // tail call: our return is theirs
                        if (Returns.TryGetValue(t, out var tr) && tr != null) Ret(obs, begin, tr);
                    }
                }
                foreach (var r in new[] { Register.RAX, Register.RCX, Register.RDX, Register.R8, Register.R9, Register.R10, Register.R11 })
                { regs.Remove(r); vals.Remove(r); }
                if (ins.Op0Kind == OpKind.NearBranch64 && Returns.TryGetValue(ins.NearBranchTarget, out var rl) && rl is Label rlv)
                    regs[Register.RAX] = rlv;
                if (flow == FlowControl.UnconditionalBranch) dead = true;
                continue;
            }

            if (ins.Mnemonic == Mnemonic.Ret)
            {
                Ret(obs, begin, regs.TryGetValue(Register.RAX, out var rv) ? rv : null);
                dead = true;
                continue;
            }

            switch (ins.Mnemonic)
            {
                case Mnemonic.Mov when dst != Register.None && ins.Op0Register.GetSize() == 8 && ins.Op1Kind == OpKind.Register:
                    {
                        var src = ins.Op1Register.GetFullRegister();
                        if (regs.TryGetValue(src, out var sl)) regs[dst] = sl; else regs.Remove(dst);
                        if (vals.TryGetValue(src, out var sv)) vals[dst] = sv; else vals.Remove(dst);
                        continue;
                    }
                case Mnemonic.Mov when memOp == 0 && ins.Op1Kind == OpKind.Register && ins.Op1Register.GetSize() == 8:
                    {
                        // spill
                        var br = ins.MemoryBase.GetFullRegister();
                        if ((br == Register.RSP || br == Register.RBP) && ins.MemoryIndex == Register.None)
                        {
                            var key = ((long)br << 32) + (int)(long)ins.MemoryDisplacement64;
                            if (regs.TryGetValue(ins.Op1Register.GetFullRegister(), out var sl)) stack[key] = sl; else stack.Remove(key);
                        }
                        else if (memBase is Label sb && IsClass(sb.Kind) && !indexed && regs.TryGetValue(ins.Op1Register.GetFullRegister(), out var stored) && !IsClass(stored.Kind) && stored.Off == 0)
                            Vote(sb.Kind, sb.Off + memDisp, false, stored.Kind);
                        continue;
                    }
                case Mnemonic.Mov when dst != Register.None && memOp == 1 && ins.Op0Register.GetSize() == 8:
                    {
                        if (memBase == null && !indexed && ins.MemoryBase != Register.RSP && ins.MemoryBase != Register.RBP && ins.MemoryBase != Register.RIP
                            && TableGuess.TryGetValue(memDisp, out var tg)) { regs[dst] = new Label("TblG." + tg, 0); vals.Remove(dst); continue; }
                        if (memBase is Label b && Child(b, memDisp, indexed) is Label c) { regs[dst] = c; vals.Remove(dst); }
                        else
                        {
                            regs.Remove(dst);
                            if (memBase is Label b2) vals[dst] = (new Label(b2.Kind, b2.Off + memDisp), 8); else vals.Remove(dst);
                            if (lastAccess?.IP == ins.IP) useOf[dst] = lastAccess;
                        }
                        continue;
                    }
                case Mnemonic.Lea when dst != Register.None && memBase is Label lb && !indexed:
                    regs[dst] = IsClass(lb.Kind) && EmbeddedKind.TryGetValue((lb.Kind, lb.Off + memDisp), out var ek) ? new Label(ek, 0) : new Label(lb.Kind, lb.Off + memDisp);
                    vals.Remove(dst);
                    continue;
                case Mnemonic.Add when dst != Register.None && regs.TryGetValue(dst, out var al2) && ins.Op1Kind != OpKind.Register && ins.Op1Kind != OpKind.Memory:
                    regs[dst] = new Label(al2.Kind, al2.Off + (int)ins.GetImmediate(1));
                    continue;
                case Mnemonic.Cmp when ins.Op0Kind == OpKind.Register && ins.Op1Kind != OpKind.Register && ins.Op1Kind != OpKind.Memory:
                    if (vals.TryGetValue(dst, out var cv) && TypeField.TryGetValue(cv.Src.Kind, out var tf2) && cv.Src.Off == tf2)
                        typeCtx.Add($"{cv.Src.Kind}={(int)ins.GetImmediate(1)}");
                    continue;
                case Mnemonic.Xor when ins.Op0Kind == OpKind.Register && ins.Op1Kind == OpKind.Register && ins.Op0Register == ins.Op1Register:
                    regs[dst] = new Label("Null", 0); vals.Remove(dst);
                    continue;
                case Mnemonic.Test:
                case Mnemonic.Cmp:
                case Mnemonic.Bt:
                case Mnemonic.Push:
                case Mnemonic.Nop:
                    continue;
            }

            // a scalar read from a struct
            if (dst != Register.None && memOp == 1 && memBase is Label vb && ins.Mnemonic is Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movsxd
                    or Mnemonic.Movss or Mnemonic.Movsd or Mnemonic.Movd or Mnemonic.Movq or Mnemonic.Movups or Mnemonic.Movaps or Mnemonic.Movdqu or Mnemonic.Cvtsi2ss)
            {
                regs.Remove(dst);
                vals[dst] = (new Label(vb.Kind, vb.Off + memDisp), ins.MemorySize.GetSize());
                if (lastAccess?.IP == ins.IP) useOf[dst] = lastAccess;
                continue;
            }

            // anything else written to a register loses its label
            for (var i = 0; i < ins.OpCount; i++)
                if (ins.GetOpKind(i) == OpKind.Register && i == 0)
                {
                    var r = ins.GetOpRegister(i).GetFullRegister();
                    regs.Remove(r); vals.Remove(r);
                }
            if (ins.Mnemonic == Mnemonic.Xchg || ins.Mnemonic == Mnemonic.Pop)
                for (var i = 0; i < ins.OpCount; i++)
                    if (ins.GetOpKind(i) == OpKind.Register) { var r = ins.GetOpRegister(i).GetFullRegister(); regs.Remove(r); vals.Remove(r); }
        }

        if (probe != null)
        {
            // deep: reads of sub-struct fields, which only a right guess of the kind reaches
            var deep = fnAccesses.Count(a => a.Kind.Contains('.') && !a.Write);
            return (deep, bad);
        }
        if (bad && !Fixed.Contains(begin)) lock (Rejected) Rejected.Add(begin);
        if (collect && touched && fnAccesses.Count > 0 && !bad)
        {
            var ctx = string.Join(",", typeCtx);
            if (SeedCtx.TryGetValue(begin, out var sc)) ctx = sc + (ctx.Length > 0 ? "," + ctx : "");
            foreach (var a in fnAccesses) a.Ctx = ctx;
            lock (Accesses) Accesses.AddRange(fnAccesses);
        }
        return (0, bad);
    }

    static void Ret(Observed obs, ulong fn, Label? l)
    {
        if (l?.Kind == "Null") return;
        if (!obs.Rets.TryGetValue(fn, out var list)) obs.Rets[fn] = list = new();
        list.Add(l);
    }
}
