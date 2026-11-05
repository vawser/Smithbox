using Iced.Intel;

/// <summary>
/// Runtime objects that MSB values are copied into. Every access made through an object pointer ([this+off]) is
/// indexed by the object's class: the RTTI class of a virtual function's this, carried into the functions it passes
/// the pointer to. On that index:
///   identity &lt;exe&gt; &lt;fn&gt; [arg]                 the classes whose fields match what fn does with its argument
///   watch    &lt;exe&gt; &lt;class&gt; &lt;off,off..&gt; [seed..]  every access to those fields of the class, with what the value is used for
/// A seed "fn:arg=Name" gives a function's argument a class by hand (S:... for an object with no RTTI class).
/// </summary>
public static class Objects
{
    public sealed class Acc
    {
        public ulong Fn, IP;
        public int Arg, Off, Size;
        public bool Write;
        public string Text;
        public Register Dst;     // the register a read went to
        public Register Base;
        public Register Src;     // the register a write stored
    }

    sealed class Scan
    {
        public List<Acc> Accs = new();
        public List<(ulong Target, int[] ArgOf)> Calls = new();   // ArgOf[i]: which of our arguments the callee's arg i is, or -1
    }

    static readonly Register[] ArgRegs = { Register.RCX, Register.RDX, Register.R8, Register.R9 };
    static readonly Register[] Volatile = { Register.RAX, Register.RCX, Register.RDX, Register.R8, Register.R9, Register.R10, Register.R11 };

    /// <summary>
    /// Linear scan of a function, following copies of the chosen argument registers (moves and stack spills)
    /// </summary>
    static Scan Run(ulong fn, IEnumerable<int> args, Dictionary<int, HashSet<string>> classes = null)
    {
        var s = new Scan();
        var alias = new Dictionary<Register, int>();
        var glob = new Dictionary<Register, ulong>();   // registers holding the value of a path global
        foreach (var a in args) alias[ArgRegs[a]] = a;
        var spill = new Dictionary<long, int>();
        if (Exe.FileOffset(fn) <= 0) return s;
        // registers holding the object at forward branch targets: the union over the paths there, so an object
        // register clobbered on one path is still known where the paths join
        var saved = new Dictionary<ulong, Dictionary<Register, int>>();
        var dead = false;
        foreach (var ins in Exe.Function(fn, 3000))
        {
            if (ins.IsInvalid) break;
            if (saved.Remove(ins.IP, out var st))
            {
                if (dead) alias = st;
                else foreach (var (r, a) in st) alias.TryAdd(r, a);
            }
            // code after a jump that no branch seen so far reaches: keep what we had (a jump table or loop may reach it)
            dead = false;
            if (ins.FlowControl is FlowControl.ConditionalBranch or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64 && ins.NearBranchTarget > ins.IP && ins.NearBranchTarget < fn + 0x8000)
            {
                if (!saved.TryGetValue(ins.NearBranchTarget, out var t)) saved[ins.NearBranchTarget] = new(alias);
                else foreach (var (r, a) in alias) t.TryAdd(r, a);
                if (ins.FlowControl == FlowControl.UnconditionalBranch) dead = true;
                continue;
            }
            if (ins.Mnemonic == Mnemonic.Ret) { dead = true; continue; }
            var dst = ins.OpCount > 0 && ins.Op0Kind == OpKind.Register ? ins.Op0Register.GetFullRegister() : Register.None;
            var mem = -1;
            for (var i = 0; i < ins.OpCount; i++) if (ins.GetOpKind(i) == OpKind.Memory) { mem = i; break; }

            if (ins.FlowControl == FlowControl.Call || (ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind == OpKind.NearBranch64 && (ins.NearBranchTarget < fn || ins.NearBranchTarget >= fn + 0x8000)))
            {
                if (ins.Op0Kind == OpKind.NearBranch64)
                {
                    var argOf = ArgRegs.Select(r => alias.TryGetValue(r, out var a) ? a : -1).ToArray();
                    if (argOf.Any(a => a >= 0)) s.Calls.Add((ins.NearBranchTarget, argOf));
                }
                foreach (var r in Volatile) { alias.Remove(r); glob.Remove(r); }
                continue;
            }

            // object pointers reached through a declared path: a global, or a pointer field of a global or a class
            if (Paths.Count > 0 && ins.Mnemonic == Mnemonic.Mov && mem == 1 && dst != Register.None && ins.Op0Register.GetSize() == 8 && ins.MemoryIndex == Register.None)
            {
                int hit = -1;
                if (ins.IsIPRelativeMemoryOperand)
                {
                    var g = ins.IPRelativeMemoryAddress;
                    hit = Paths.FindIndex(p => p.Global == g && p.Off < 0);
                    if (PathGlobals.Contains(g)) glob[dst] = g; else glob.Remove(dst);
                }
                else
                {
                    var b0 = ins.MemoryBase.GetFullRegister();
                    var d0 = (int)(long)ins.MemoryDisplacement64;
                    if (glob.TryGetValue(b0, out var g)) hit = Paths.FindIndex(p => p.Global == g && p.Off == d0);
                    else if (alias.TryGetValue(b0, out var ba))
                    {
                        var bc = ba >= PathBase ? new HashSet<string> { Paths[ba - PathBase].Cls } : classes?.GetValueOrDefault(ba);
                        if (bc != null) hit = Paths.FindIndex(p => p.From != null && p.Off == d0 && bc.Contains(p.From));
                    }
                    glob.Remove(dst);
                }
                if (hit >= 0)
                {
                    if (!ins.IsIPRelativeMemoryOperand && alias.TryGetValue(ins.MemoryBase.GetFullRegister(), out var pa))
                        s.Accs.Add(new Acc { Fn = fn, IP = ins.IP, Arg = pa, Off = (int)(long)ins.MemoryDisplacement64, Size = 8, Text = Exe.Format(ins), Dst = dst });
                    alias[dst] = PathBase + hit;
                    continue;
                }
            }
            else if (dst != Register.None && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt or Mnemonic.Push)) glob.Remove(dst);

            if (mem >= 0 && ins.MemoryBase != Register.None)
            {
                var b = ins.MemoryBase.GetFullRegister();
                var disp = (int)(long)ins.MemoryDisplacement64;
                if ((b == Register.RSP || b == Register.RBP) && ins.MemoryIndex == Register.None && ins.Mnemonic == Mnemonic.Mov)
                {
                    var key = ((long)b << 32) + disp;
                    if (mem == 0 && ins.Op1Kind == OpKind.Register && ins.Op1Register.GetSize() == 8)
                    {
                        if (alias.TryGetValue(ins.Op1Register.GetFullRegister(), out var a)) spill[key] = a; else spill.Remove(key);
                        continue;
                    }
                    if (mem == 1 && dst != Register.None && ins.Op0Register.GetSize() == 8 && spill.TryGetValue(key, out var sa)) { alias[dst] = sa; continue; }
                }
                else if (alias.TryGetValue(b, out var a) && ins.MemoryIndex == Register.None && ins.Mnemonic is not (Mnemonic.Lea or Mnemonic.Nop or Mnemonic.Prefetchw or Mnemonic.Prefetcht0))
                {
                    var write = mem == 0 && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt);
                    s.Accs.Add(new Acc { Fn = fn, IP = ins.IP, Arg = a, Off = disp, Size = ins.MemorySize.GetSize(), Write = write, Text = Exe.Format(ins), Dst = mem == 1 ? dst : Register.None });
                }
            }

            if (ins.Mnemonic == Mnemonic.Mov && dst != Register.None && ins.Op1Kind == OpKind.Register && ins.Op0Register.GetSize() == 8)
            {
                if (alias.TryGetValue(ins.Op1Register.GetFullRegister(), out var a)) alias[dst] = a; else alias.Remove(dst);
                continue;
            }
            if (dst != Register.None && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt or Mnemonic.Push)) alias.Remove(dst);
            if (ins.Mnemonic is Mnemonic.Xchg or Mnemonic.Pop && dst != Register.None) alias.Remove(dst);
        }
        return s;
    }

    // declared object paths: G:<global>=Cls, G:<global>+<off>=Cls, F:<class>+<off>=Cls; their objects are pseudo-arguments PathBase+i
    public sealed record PathDef(ulong Global, string From, int Off, string Cls);
    public static List<PathDef> Paths = new();
    static HashSet<ulong> PathGlobals = new();
    const int PathBase = 100;

    static HashSet<string> ClassesOf(Dictionary<int, HashSet<string>> args, int a) =>
        a >= PathBase ? new HashSet<string> { Paths[a - PathBase].Cls } : args.GetValueOrDefault(a) ?? new();

    // function -> argument -> classes
    public static Dictionary<ulong, Dictionary<int, HashSet<string>>> Known = new();
    public static Dictionary<(string Cls, int Off), List<Acc>> Index = new();
    public static Dictionary<string, HashSet<(int Off, int Size)>> ClassFields = new();

    public static void Build(IEnumerable<string> seeds, int depth = 4)
    {
        // the classes of a virtual's this; a base implementation shared by a few classes keeps all of them
        foreach (var (fn, v) in Exe.VirtualOf)
        {
            var cls = v.Select(x => x.Class).Distinct().ToList();
            if (cls.Count > 6) continue;
            Add(fn, 0, cls);
        }
        foreach (var s in seeds.Where(s => s.StartsWith("G:") || s.StartsWith("F:")))
        {
            var eq = s.Split('=');
            var body = eq[0].Substring(2);
            var plus = body.LastIndexOf('+');
            var head = plus >= 0 ? body.Substring(0, plus) : body;
            var off = plus >= 0 ? Convert.ToInt32(body.Substring(plus + 1).Replace("0x", ""), 16) : -1;
            Paths.Add(s[0] == 'G' ? new PathDef(Hex(head), null, off, eq[1]) : new PathDef(0, head, off, eq[1]));
        }
        PathGlobals = Paths.Where(p => p.From == null).Select(p => p.Global).ToHashSet();
        var pathFns = new HashSet<ulong>();
        if (PathGlobals.Count > 0)
            foreach (var ins in Exe.Sweep())
                if (ins.IsIPRelativeMemoryOperand && PathGlobals.Contains(ins.IPRelativeMemoryAddress))
                    pathFns.Add(Owner(ins.IP));
        foreach (var fn in pathFns) if (!Known.ContainsKey(fn)) Known[fn] = new();
        Console.Error.WriteLine($"objects: {Paths.Count} paths, {pathFns.Count} functions reading their globals");

        foreach (var s in seeds.Where(s => !s.StartsWith("G:") && !s.StartsWith("F:")))
        {
            // fn:arg=Name
            var eq = s.Split('=');
            var fa = eq[0].Split(':');
            Add(Convert.ToUInt64(fa[0].Replace("0x", ""), 16), fa.Length > 1 ? int.Parse(fa[1]) : 0, new[] { eq[1] });
        }

        var done = new HashSet<(ulong, int)>();
        var frontier = Known.Keys.ToList();
        for (var d = 0; d <= depth && frontier.Count > 0; d++)
        {
            var votes = new Dictionary<(ulong, int), HashSet<string>>();
            foreach (var fn in frontier)
            {
                var args = Known[fn];
                var fresh = args.Keys.Where(a => done.Add((fn, a))).ToList();
                var first = done.Add((fn, -1));
                if (fresh.Count == 0 && !first) continue;
                var scan = Run(fn, fresh, args);
                foreach (var acc in scan.Accs)
                    foreach (var c in ClassesOf(args, acc.Arg))
                    {
                        if (!Index.TryGetValue((c, acc.Off), out var l)) Index[(c, acc.Off)] = l = new();
                        l.Add(acc);
                        if (!ClassFields.TryGetValue(c, out var f)) ClassFields[c] = f = new();
                        f.Add((acc.Off, acc.Size));
                    }
                if (d == depth) continue;
                foreach (var (t, argOf) in scan.Calls)
                    for (var i = 0; i < 4; i++)
                        if (argOf[i] >= 0)
                        {
                            if (!votes.TryGetValue((t, i), out var set)) votes[(t, i)] = set = new();
                            set.UnionWith(ClassesOf(args, argOf[i]));
                        }
            }
            frontier = new();
            foreach (var ((t, i), cls) in votes)
            {
                // a generic helper called with many unrelated objects says nothing
                if (cls.Count > 6 || (Known.TryGetValue(t, out var k) && k.ContainsKey(i))) continue;
                Add(t, i, cls);
                frontier.Add(t);
            }
            frontier = frontier.Distinct().ToList();
            Console.Error.WriteLine($"objects depth {d}: {Known.Count} functions, {Index.Count} fields, {frontier.Count} new");
        }
    }

    static void Add(ulong fn, int arg, IEnumerable<string> cls)
    {
        if (!Known.TryGetValue(fn, out var m)) Known[fn] = m = new();
        if (!m.TryGetValue(arg, out var set)) m[arg] = set = new();
        set.UnionWith(cls);
    }

    public static void Run(string mode, string[] args)
    {
        if (mode == "owner") { foreach (var a in args.Skip(2)) Console.WriteLine($"{a} -> {Owner(Hex(a)):X}"); return; }
        var seeds = args.Skip(2).Where(a => a.Contains('=')).ToList();
        Build(seeds);
        if (mode == "identity") Identity(Hex(args[2]), args.Length > 3 && !args[3].Contains('=') ? int.Parse(args[3]) : 0);
        else Watch(args[2], args[3].Split(',').Select(o => Convert.ToInt32(o.Replace("0x", ""), 16)).ToArray());
    }

    static ulong Hex(string s) => Convert.ToUInt64(s.Replace("0x", ""), 16);
    static string Fn(ulong f)
    {
        var s = Exe.VirtualOf.TryGetValue(f, out var v) ? $"{f:X}({v[0].Class}#{v[0].Index}{(v.Count > 1 ? $"+{v.Count - 1}" : "")})" : $"{f:X}";
        return Anchors.Bound.TryGetValue(f, out var b) ? $"{s}<{b}>" : s;
    }

    /// <summary>
    /// The function an instruction is in: the nearest call target before it when one is closer than the .pdata start
    /// (leaf functions have no .pdata entry)
    /// </summary>
    static ulong Owner(ulong ip)
    {
        var p = Extra.FuncStart(ip);
        if (ip - p > 0x20000) return p;
        CallersOf(0);
        // a known entry just before ip (the decode from p can be misaligned when p is a chained .pdata fragment)
        for (var a = ip; a + 0x40 > ip && a > p; a--)
            if (_calls.Contains(a) || Anchors.Bound.ContainsKey(a) || Exe.VirtualOf.ContainsKey(a)) return a;
        var best = p;
        var targets = new HashSet<ulong>();
        var prevTerm = false;
        var fo = Exe.FileOffset(p);
        var dec = Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(ip - p) + 16));
        dec.IP = p;
        while (dec.IP <= ip)
        {
            dec.Decode(out var ins);
            if (ins.IsInvalid) break;
            if (ins.IP != p && (_calls.Contains(ins.IP) || Anchors.Bound.ContainsKey(ins.IP) || Exe.VirtualOf.ContainsKey(ins.IP)
                                || (prevTerm && (ins.IP & 0xF) == 0 && !targets.Contains(ins.IP))))
                best = ins.IP;
            if (ins.FlowControl is FlowControl.ConditionalBranch or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64)
                targets.Add(ins.NearBranchTarget);
            prevTerm = ins.Mnemonic is Mnemonic.Ret or Mnemonic.Int3 || ins.FlowControl is FlowControl.UnconditionalBranch or FlowControl.IndirectBranch;
        }
        return best;
    }

    /// <summary>
    /// Ranks the classes by how many of the fields fn touches through its argument (offset and size) they also have
    /// </summary>
    static void Identity(ulong fn, int arg)
    {
        var scan = Run(fn, new[] { arg });
        var sig = scan.Accs.Select(a => (a.Off, a.Size)).Distinct().ToList();
        Console.WriteLine($"{Fn(fn)} arg{arg}: {sig.Count} fields: {string.Join(" ", sig.OrderBy(x => x.Off).Select(x => $"{x.Off:X}/{x.Size}"))}");
        if (Known.TryGetValue(fn, out var k) && k.TryGetValue(arg, out var kc)) Console.WriteLine($"  propagated: {string.Join(", ", kc)}");

        Console.WriteLine("  callers:");
        var callers = new List<ulong>();
        foreach (var ins in Exe.Sweep())
            if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64 && ins.NearBranchTarget == fn)
                callers.Add(ins.IP);
        foreach (var site in callers.Take(30))
        {
            var c = Extra.FuncStart(site);
            var kn = Known.TryGetValue(c, out var ck) ? string.Join(" ", ck.Select(x => $"arg{x.Key}={string.Join("|", x.Value.Take(3))}")) : "";
            Console.WriteLine($"    {site:X} in {Fn(c)} {kn}");
        }

        // the class fields as seen outside fn, so a class propagated into fn doesn't match itself
        var fields = new Dictionary<string, HashSet<(int, int)>>();
        foreach (var ((c, off), list) in Index)
            foreach (var a in list)
                if (a.Fn != fn)
                {
                    if (!fields.TryGetValue(c, out var f)) fields[c] = f = new();
                    f.Add((a.Off, a.Size));
                }
        Console.WriteLine("  best matching classes (matched/fields, class size seen):");
        var ranked = fields.Select(kv => (Cls: kv.Key, Hit: sig.Count(s => kv.Value.Contains(s)), Seen: kv.Value.Count))
            .Where(x => x.Hit > 0).OrderByDescending(x => x.Hit).ThenBy(x => x.Seen).Take(15);
        foreach (var r in ranked)
        {
            var hits = sig.Where(s => fields[r.Cls].Contains(s)).OrderBy(s => s.Off).Select(s => $"{s.Off:X}");
            Console.WriteLine($"    {r.Hit,3}/{sig.Count}  {r.Cls}  ({r.Seen} fields)  {string.Join(" ", hits.Take(20))}");
        }
    }

    /// <summary>
    /// Every access to the fields of a class (and of classes whose name starts with it plus "@" for secondary
    /// vtables), with what a read value is used for
    /// </summary>
    static void Watch(string clsList, int[] offs)
    {
        var names = clsList.Split('|');
        bool Match(string k) => names.Any(n => k == n || k.StartsWith(n + "@"));
        var cls = clsList;
        foreach (var off in offs)
        {
            var accs = Index.Where(kv => kv.Key.Off == off && Match(kv.Key.Cls))
                .SelectMany(kv => kv.Value).DistinctBy(a => a.IP).OrderBy(a => a.Write).ThenBy(a => a.Fn).ToList();
            Console.WriteLine($"== {cls} +{off:X}: {accs.Count} accesses ({accs.Count(a => !a.Write)} reads)");
            foreach (var a in accs)
            {
                var uses = a.Write ? "" : string.Join("; ", Uses.After(a.IP, a.Dst));
                Console.WriteLine($"  {(a.Write ? "W" : "R")} {Fn(a.Fn)}  {a.Text}{(uses.Length > 0 ? "  -> " + uses : "")}");
                if (uses.StartsWith("ret") || uses.Contains("; ret")) Getter(a.Fn);
            }

            // the same displacement and size elsewhere, through a pointer whose class isn't known: candidates only
            var sizes = accs.Select(a => a.Size).ToHashSet();
            var seen = accs.Select(a => a.IP).ToHashSet();
            var loose = Loose(off).Where(l => sizes.Contains(l.Size) && !seen.Contains(l.IP)).ToList();
            if (loose.Count == 0) continue;
            // how many other fields of the class the same base register touches in that function
            var classFields = Index.Where(kv => Match(kv.Key.Cls)).SelectMany(kv => kv.Value).Select(a => (a.Off, a.Size)).ToHashSet();
            var scored = loose.Select(l => (L: l, Fn: Owner(l.IP))).Select(x => (x.L, x.Fn, Score: CoFields(x.Fn, x.L.Base, classFields, off))).OrderByDescending(x => x.Score.Hit).ThenBy(x => x.Fn).ToList();
            Console.WriteLine($"  -- same offset and size, object not identified: {loose.Count} (n/m: other {cls} fields through the same register)");
            foreach (var (l, fn, sc) in scored.Take(40))
            {
                var kn = Known.TryGetValue(fn, out var k) ? " [" + string.Join(" ", k.Select(x => $"arg{x.Key}={string.Join("|", x.Value.Take(2))}")) + "]" : "";
                var uses = l.Write ? "" : string.Join("; ", Uses.After(l.IP, l.Dst));
                Console.WriteLine($"  ?{(l.Write ? "W" : "R")} {sc.Hit}/{sc.Of} {Fn(fn)}{kn}  {l.Text}{(uses.Length > 0 ? "  -> " + uses : "")}");
                if (uses == "ret" && l.IP - fn < 0x20) Getter(fn);
                if (l.Write && l.Src != Register.None && l.IP - fn < 0x40) Setter(fn, l.Src);
            }
        }
    }

    // MSVC starts functions 16-aligned after int3 padding or the previous function's ret
    static bool AfterPadding(ulong a)
    {
        if ((a & 0xF) != 0) return false;
        var fo = Exe.FileOffset(a);
        return fo > 0 && Exe.Image[fo - 1] is 0xCC or 0xC3;
    }

    static Dictionary<ulong, List<ulong>> _callers;
    static HashSet<ulong> _calls = new();
    static List<ulong> CallersOf(ulong fn)
    {
        if (_callers == null)
        {
            _callers = new();
            foreach (var ins in Exe.Sweep())
                if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64)
                {
                    if (!_callers.TryGetValue(ins.NearBranchTarget, out var l)) _callers[ins.NearBranchTarget] = l = new();
                    l.Add(ins.IP);
                    if (ins.FlowControl == FlowControl.Call || (ins.NearBranchTarget > ins.IP ? ins.NearBranchTarget - ins.IP : ins.IP - ins.NearBranchTarget) > 0x1000) _calls.Add(ins.NearBranchTarget);
                }
        }
        return _callers.GetValueOrDefault(fn) ?? new();
    }

    /// <summary>
    /// A getter that returns the field: what each caller does with the result
    /// </summary>
    static void Getter(ulong fn, int depth = 0)
    {
        var sites = CallersOf(fn);
        var pad = new string(' ', 6 + depth * 4);
        Console.WriteLine($"{pad}getter {Fn(fn)}: {sites.Count} callers");
        foreach (var site in sites.Take(16))
        {
            var c = Owner(site);
            var uses = string.Join("; ", Uses.After(site, Register.RAX));
            Console.WriteLine($"{pad}  {site:X} in {Fn(c)}{(uses.Length > 0 ? "  -> " + uses : "")}");
            // passed straight up: the caller is a getter too
            if (uses.EndsWith("ret") && depth < 2 && sites.Count < 40) Getter(c, depth + 1);
        }
    }

    /// <summary>
    /// A small function storing an argument into the field: its callers, with the constant they pass when there is one
    /// </summary>
    static void Setter(ulong fn, Register src)
    {
        var argIdx = Array.FindIndex(ArgRegs, r => r == src);
        if (argIdx < 0) return;
        var sites = CallersOf(fn);
        Console.WriteLine($"      setter {Fn(fn)} (arg{argIdx}): {sites.Count} callers");
        foreach (var site in sites.Take(16))
        {
            // the last write of the argument register in the few instructions before the call
            string val = "?";
            var start = site - 0x30;
            foreach (var ins in Exe.Function(start, 40))
            {
                if (ins.IP >= site) break;
                if (ins.OpCount > 0 && ins.Op0Kind == OpKind.Register && ins.Op0Register.GetFullRegister() == src)
                    val = ins.Mnemonic == Mnemonic.Xor && ins.Op1Kind == OpKind.Register && ins.Op1Register.GetFullRegister() == src ? "0"
                        : ins.OpCount > 1 && ins.GetOpKind(1) is OpKind.Immediate8 or OpKind.Immediate32 or OpKind.Immediate8to32 or OpKind.Immediate64 or OpKind.Immediate32to64 ? ((long)ins.GetImmediate(1)).ToString()
                        : "var";
            }
            Console.WriteLine($"        {site:X} in {Fn(Owner(site))}  value {val}");
        }
    }

    /// <summary>
    /// The start of the small function containing ip: a call target at most 0x40 bytes before it
    /// </summary>
    static ulong StartNear(ulong ip)
    {
        for (var a = ip; a + 0x40 > ip && a > 0; a--)
            if (CallersOf(a).Count > 0) return a;
        return 0;
    }

    static List<Acc> Loose(int off)
    {
        var r = new List<Acc>();
        foreach (var ins in Exe.Sweep())
        {
            var mem = -1;
            for (var i = 0; i < ins.OpCount; i++) if (ins.GetOpKind(i) == OpKind.Memory) { mem = i; break; }
            if (mem < 0 || ins.MemoryIndex != Register.None || ins.MemoryBase is Register.None or Register.RIP or Register.RSP or Register.RBP) continue;
            if ((int)(long)ins.MemoryDisplacement64 != off || ins.Mnemonic is Mnemonic.Lea or Mnemonic.Nop) continue;
            var write = mem == 0 && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt);
            r.Add(new Acc { IP = ins.IP, Off = off, Size = ins.MemorySize.GetSize(), Write = write, Text = Exe.Format(ins), Base = ins.MemoryBase.GetFullRegister(), Src = mem == 0 && ins.OpCount > 1 && ins.Op1Kind == OpKind.Register ? ins.Op1Register.GetFullRegister() : Register.None,
                            Dst = mem == 1 && ins.Op0Kind == OpKind.Register ? ins.Op0Register.GetFullRegister() : Register.None });
        }
        return r;
    }

    /// <summary>
    /// The fields read or written through a register anywhere in a function (ignoring where it is redefined),
    /// and how many of them the class has
    /// </summary>
    static (int Hit, int Of) CoFields(ulong fn, Register reg, HashSet<(int, int)> classFields, int except)
    {
        var set = new HashSet<(int, int)>();
        foreach (var ins in Exe.Function(fn, 6000))
        {
            if (ins.IsInvalid) break;
            for (var i = 0; i < ins.OpCount; i++)
                if (ins.GetOpKind(i) == OpKind.Memory && ins.MemoryIndex == Register.None && ins.MemoryBase.GetFullRegister() == reg && ins.Mnemonic != Mnemonic.Lea)
                {
                    var d = (int)(long)ins.MemoryDisplacement64;
                    if (d != except) set.Add((d, ins.MemorySize.GetSize()));
                }
        }
        return (set.Count(classFields.Contains), set.Count);
    }
}

/// <summary>
/// What a value in a register is used for in the instructions after it: stores, compares, calls it is an argument
/// of, arithmetic with constants. Follows copies to other registers; stops at a return or jump.
/// </summary>
public static class Uses
{
    static readonly Register[] Args = { Register.RCX, Register.RDX, Register.R8, Register.R9 };

    public static List<string> After(ulong ip, Register reg, int max = 32)
    {
        var found = new List<string>();
        var fo = Exe.FileOffset(ip);
        if (fo <= 0) return found;
        var live = new HashSet<Register>();
        var dec = Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, 0x400));
        dec.IP = ip;
        dec.Decode(out var first);
        // a compare or test at the read itself
        if (first.Mnemonic is Mnemonic.Cmp or Mnemonic.Test && first.OpCount > 1 && first.GetOpKind(1) is not (OpKind.Register or OpKind.Memory))
            found.Add($"{first.Mnemonic.ToString().ToLower()} {(long)first.GetImmediate(1)}");
        if (reg == Register.None) return found;
        live.Add(reg.GetFullRegister());

        for (var i = 0; i < max && found.Count < 6 && live.Count > 0; i++)
        {
            dec.Decode(out var ins);
            bool Reads(int op) => ins.GetOpKind(op) == OpKind.Register && live.Contains(ins.GetOpRegister(op).GetFullRegister());
            var m = ins.Mnemonic.ToString().ToLower();
            if (ins.FlowControl == FlowControl.Call)
            {
                var t = ins.Op0Kind == OpKind.NearBranch64 ? ins.NearBranchTarget : 0;
                for (var a = 0; a < 4; a++)
                    if (live.Contains(Args[a]) || live.Contains((Register.XMM0 + a).GetFullRegister()))
                        found.Add($"arg{a} of {(t == 0 ? "indirect" : Name(t))}");
                foreach (var r in new[] { Register.RAX, Register.RCX, Register.RDX, Register.R8, Register.R9, Register.R10, Register.R11 }) live.Remove(r);
                for (var x = 0; x < 6; x++) live.Remove((Register.XMM0 + x).GetFullRegister());
                continue;
            }
            if (ins.Mnemonic == Mnemonic.Ret) { if (live.Contains(Register.RAX) || live.Contains(Register.ZMM0)) found.Add("ret"); break; }
            if (ins.FlowControl is FlowControl.UnconditionalBranch or FlowControl.IndirectBranch) break;
            if (ins.FlowControl != FlowControl.Next && ins.FlowControl != FlowControl.ConditionalBranch) break;

            if (ins.OpCount > 1 && ins.Op0Kind == OpKind.Memory && Reads(1))
            {
                found.Add(ins.MemoryBase is Register.RSP or Register.RBP ? $"stack {ins.MemoryDisplacement64:X}" : $"store [{ins.MemoryBase}+{ins.MemoryDisplacement64:X}]");
                continue;
            }
            if (ins.OpCount > 1 && ins.Op0Kind == OpKind.Register && Reads(0))
            {
                var k = ins.GetOpKind(1);
                if (k is OpKind.Register or OpKind.Memory)
                {
                    if (ins.Mnemonic is not (Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movss or Mnemonic.Movaps))
                    {
                        var c = k == OpKind.Memory && ins.IsIPRelativeMemoryOperand ? Const(ins) : k == OpKind.Memory ? $"[{ins.MemoryBase}+{ins.MemoryDisplacement64:X}]" : "";
                        found.Add($"{m} {c}".Trim());
                    }
                }
                else found.Add($"{m} {(long)ins.GetImmediate(1)}");
                if (ins.Mnemonic is Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt) continue;
            }
            else if (ins.OpCount > 1 && ins.Op0Kind == OpKind.Register && Reads(1) && ins.Mnemonic is not (Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movsxd or Mnemonic.Movss or Mnemonic.Movaps or Mnemonic.Movd or Mnemonic.Cvtsi2ss))
                found.Add($"{m} with");

            // copies
            if (ins.OpCount > 1 && ins.Op0Kind == OpKind.Register && Reads(1)
                && ins.Mnemonic is Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movsxd or Mnemonic.Movss or Mnemonic.Movaps or Mnemonic.Movd or Mnemonic.Cvtsi2ss or Mnemonic.Cvtss2sd)
                live.Add(ins.Op0Register.GetFullRegister());
            else if (ins.OpCount > 0 && ins.Op0Kind == OpKind.Register && !Reads(0) && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt or Mnemonic.Push))
                live.Remove(ins.Op0Register.GetFullRegister());
        }
        return found;
    }

    static string Const(in Instruction ins)
    {
        var fo = Exe.FileOffset(ins.IPRelativeMemoryAddress);
        if (fo <= 0) return $"[{ins.IPRelativeMemoryAddress:X}]";
        return ins.MemorySize.GetSize() == 8 && ins.Mnemonic.ToString().EndsWith("sd")
            ? $"{BitConverter.ToDouble(Exe.Image, (int)fo):0.######}"
            : $"{BitConverter.ToSingle(Exe.Image, (int)fo):0.######}";
    }

    public static string Name(ulong t) => Exe.VirtualOf.TryGetValue(t, out var v) ? $"{t:X}({v[0].Class}#{v[0].Index})" : $"{t:X}";
}


/// <summary>
/// Functions whose purpose the exe names: script bindings registered as lea rax,[fn] / mov [slot],rax followed by
/// the binding's name string (DLConcreteMethodInvoker for Lua/ESD functions)
/// </summary>
public static class Anchors
{
    static Dictionary<ulong, string> _bound;
    public static Dictionary<ulong, string> Bound => _bound ??= FindBindings();

    static Dictionary<ulong, string> FindBindings()
    {
        var r = new Dictionary<ulong, string>();
        ulong pending = 0;
        var age = 0;
        foreach (var ins in Exe.Sweep())
        {
            age++;
            if (ins.Mnemonic == Mnemonic.Lea && ins.IsIPRelativeMemoryOperand)
            {
                var t = ins.IPRelativeMemoryAddress;
                if (Exe.IsCode(t)) { pending = t; age = 0; continue; }
                if (pending != 0 && age < 14 && ins.Op0Register == Register.R8)
                {
                    var s = Exe.ReadString(t);
                    if (s != null && !s.StartsWith("u:") && s.All(ch => char.IsLetterOrDigit(ch) || ch == '_')) r.TryAdd(pending, s);
                    pending = 0;
                }
            }
            if (ins.FlowControl is FlowControl.Return or FlowControl.UnconditionalBranch) pending = 0;
        }
        Console.Error.WriteLine($"anchors: {r.Count} named bindings");
        return r;
    }
}