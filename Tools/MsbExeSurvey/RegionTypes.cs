using Iced.Intel;
using SoulsFormats;
using System.Reflection;
using System.Text;

/// <summary>
/// Which region types a region type-data read belongs to. The game reads region type data through small getters in
/// the CSMsbPoint unit that don't know the type; their callers do. A caller's type comes from:
///   * a typed bind: mov edx,T ... call 140CFE230 (binds the wrapper to a point of type T);
///   * being a virtual of a CSMsbPoint subclass;
///   * a type check cmp [point+8],T recorded by the flow pass (access context "Point=T").
/// A getter's reads get the types of its callers that have exactly one.
/// </summary>
public static class RegionTypes
{
    const ulong Bind = 0x140CFE230;
    public static string OutDir;

    // CSMsbPoint subclasses and the region type each one is
    static readonly Dictionary<string, uint> SubclassType = new()
    {
        ["CS::CSMsbPointSound"] = 4, ["CS::CSMsbPointHintBloodMessage"] = 9, ["CS::CSMsbPointConnectionRefe"] = 21,
        ["CS::CSMsbPointOcclusionBox"] = 28, ["CS::CSMsbPointOcclusionPortal"] = 29, ["CS::CSMsbPointSoundRegion"] = 30,
        ["CS::CSMsbPointWorldMapPoint"] = 33, ["CS::CSMsbPointGroupPlacementPoint"] = 37, ["CS::CSMsbPointPlayRegion"] = 43,
    };

    static Dictionary<uint, string> _names;
    /// <summary>
    /// Region type number -> layout entry ("Point.Message"), from each SoulsFormats region class's Type
    /// </summary>
    public static Dictionary<uint, string> Names
    {
        get
        {
            if (_names != null) return _names;
            _names = new();
            foreach (var t in typeof(MSBE.Region).GetNestedTypes(BindingFlags.Public).Where(t => t.IsSubclassOf(typeof(MSBE.Region)) && !t.IsAbstract))
            {
                try
                {
                    var inst = Activator.CreateInstance(t);
                    var p = t.GetProperty("Type", BindingFlags.Instance | BindingFlags.NonPublic);
                    var v = Convert.ToUInt32(p.GetValue(inst));
                    _names[v] = "Point." + t.Name;
                }
                catch { }
            }
            return _names;
        }
    }

    // functions that take a region type in edx and pass it on to the per-type lookup; and functions that call one of
    // them with a constant type (their callers handle that type)
    static HashSet<ulong> _takers;
    static Dictionary<ulong, HashSet<uint>> _binders;
    static Dictionary<ulong, List<ulong>> _calls;   // target -> call/jmp sites

    static void BuildBinders()
    {
        if (_takers != null) return;
        _calls = new();
        foreach (var ins in Exe.Sweep())
            if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64)
            {
                if (!_calls.TryGetValue(ins.NearBranchTarget, out var l)) _calls[ins.NearBranchTarget] = l = new();
                l.Add(ins.IP);
            }
        // MsbResCap functions that index the per-type point tables (+318h/+320h) by the type in edx
        _takers = new() { 0x140CF8D70, 0x140CF8DF0, 0x140CF8FC0, 0x140CF9130, 0x140CF9180, 0x140CF92E0 };
        _binders = new();
        var queue = new Queue<ulong>(_takers);
        var seen = new HashSet<ulong>(_takers);
        while (queue.Count > 0)
        {
            var t = queue.Dequeue();
            foreach (var site in _calls.GetValueOrDefault(t) ?? new())
            {
                var f = Extra.FuncStart(site);
                // what edx holds at the call: a constant set shortly before, or untouched since entry
                var state = EdxAt(f, site);
                if (state is uint c)
                {
                    if (!_binders.TryGetValue(f, out var s)) _binders[f] = s = new();
                    s.Add(c);
                }
                else if (state == null && seen.Add(f)) { _takers.Add(f); queue.Enqueue(f); }
            }
        }
        Console.Error.WriteLine($"region types: {_takers.Count} type-taking functions, {_binders.Count} fixed binders");
        File.WriteAllLines(Path.Combine(OutDir ?? ".", "region_binders.tsv"),
            _binders.OrderBy(b => b.Key).Select(b => $"{b.Key:X}\t{string.Join(",", b.Value.Select(t => $"{t}:{Names.GetValueOrDefault(t, "?")}"))}\t{(_calls.GetValueOrDefault(b.Key)?.Count ?? 0)} callers"));
    }

    /// <summary>
    /// edx at a call site: the constant last moved into it, null if not written since the function's start, or
    /// "other" (any other write, or a call in between)
    /// </summary>
    static object EdxAt(ulong fn, ulong site)
    {
        // a constant moved into edx in the few instructions before the call (found by decoding a short window that
        // lands exactly on the call)
        for (ulong k = 0x30; k >= 0x8; k--)
        {
            var fo = Exe.FileOffset(site - k);
            if (fo <= 0) continue;
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)k + 16));
            dec.IP = site - k;
            object w = null; var landed = false; var clean = true;
            while (dec.IP <= site)
            {
                dec.Decode(out var ins);
                if (ins.IsInvalid) { clean = false; break; }
                if (ins.IP == site) { landed = true; break; }
                if (ins.Mnemonic == Mnemonic.Mov && ins.Op0Kind == OpKind.Register && ins.Op0Register == Register.EDX && ins.Op1Kind == OpKind.Immediate32) w = ins.Immediate32;
                else if (ins.FlowControl is not (FlowControl.Next or FlowControl.ConditionalBranch)) w = null;
                else if (ins.OpCount > 0 && ins.Op0Kind == OpKind.Register && ins.Op0Register.GetFullRegister() == Register.RDX && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test)) w = "other";
            }
            if (landed && clean && w is uint) return w;
            if (landed && clean) break;
        }

        object state = null;
        if (site < fn || site - fn > 0x4000) return "other";
        foreach (var ins in Exe.Function(fn, 6000))
        {
            if (ins.IsInvalid || ins.IP > site) break;
            if (ins.IP == site) return state;
            if (ins.Mnemonic == Mnemonic.Mov && ins.Op0Kind == OpKind.Register && ins.Op0Register == Register.EDX && ins.Op1Kind == OpKind.Immediate32)
                state = ins.Immediate32;
            else if (ins.FlowControl == FlowControl.Call) state = "other";
            else if (ins.OpCount > 0 && ins.Op0Kind == OpKind.Register && ins.Op0Register.GetFullRegister() == Register.RDX
                     && ins.Mnemonic is not (Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Push)) state = "other";
        }
        return "other";
    }

    /// <summary>
    /// The region types a function handles: its typed binds, the fixed binders it calls, its class and its type checks
    /// </summary>
    static HashSet<uint> TypesOf(ulong fn, Dictionary<ulong, HashSet<uint>> ctxTypes, int up = 1)
    {
        BuildBinders();
        var set = new HashSet<uint>();
        if (Exe.VirtualOf.TryGetValue(fn, out var v))
            foreach (var (cls, _) in v)
                if (SubclassType.TryGetValue(cls, out var st)) set.Add(st);
        if (ctxTypes.TryGetValue(fn, out var ct)) set.UnionWith(ct);
        if (_binders.TryGetValue(fn, out var own)) set.UnionWith(own);
        foreach (var ins in Exe.Function(fn, 6000))
        {
            if (ins.IsInvalid) break;
            if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64
                && _binders.TryGetValue(ins.NearBranchTarget, out var b) && b.Count == 1)
                set.UnionWith(b);
        }
        // no type of its own: the wrapper came from the callers
        if (set.Count == 0 && up > 0)
        {
            var fromCallers = new HashSet<uint>();
            var sites = _calls.GetValueOrDefault(fn) ?? new();
            foreach (var site in sites.Take(20))
            {
                var ft = TypesOf(Extra.FuncStart(site), ctxTypes, up - 1);
                if (ft.Count == 1) fromCallers.UnionWith(ft);
            }
            if (fromCallers.Count == 1) set.UnionWith(fromCallers);
        }
        return set;
    }

    /// <summary>
    /// Function -> region entries its Point.Type reads belong to (only where the callers agree on one type each)
    /// </summary>
    public static Dictionary<ulong, HashSet<string>> Attribute(string dir)
    {
        OutDir = dir;
        var result = new Dictionary<ulong, HashSet<string>>();
        var names = Names;

        // type checks the flow pass saw, per function
        var ctxTypes = new Dictionary<ulong, HashSet<uint>>();
        foreach (var a in Flow.Accesses)
            if (a.Ctx != null)
                foreach (var part in a.Ctx.Split(','))
                    if (part.StartsWith("Point=") && uint.TryParse(part[6..], out var t))
                    {
                        if (!ctxTypes.TryGetValue(a.Func, out var s)) ctxTypes[a.Func] = s = new();
                        s.Add(t);
                    }

        var getters = Flow.Accesses.Where(a => a.Kind == "Point.Type" && !a.Write).Select(a => a.Func).ToHashSet();
        var callers = new Dictionary<ulong, List<ulong>>();
        foreach (var ins in Exe.Sweep())
            if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64 && getters.Contains(ins.NearBranchTarget))
            {
                if (!callers.TryGetValue(ins.NearBranchTarget, out var l)) callers[ins.NearBranchTarget] = l = new();
                l.Add(ins.IP);
            }

        var sb = new StringBuilder();
        foreach (var g in getters.OrderBy(x => x))
        {
            var types = new HashSet<uint>();
            var own = TypesOf(g, ctxTypes);
            if (own.Count == 1) types.UnionWith(own);
            var detail = new List<string>();
            foreach (var site in callers.GetValueOrDefault(g) ?? new())
            {
                var f = Extra.FuncStart(site);
                var ft = TypesOf(f, ctxTypes);
                detail.Add($"{f:X}:{(ft.Count == 0 ? "?" : string.Join("/", ft))}");
                if (ft.Count == 1) types.UnionWith(ft);
            }
            var entries = types.Select(t => names.GetValueOrDefault(t, $"type{t}")).ToHashSet();
            if (entries.Count > 0) result[g] = entries;
            var offs = string.Join(",", Flow.Accesses.Where(a => a.Func == g && a.Kind == "Point.Type").Select(a => $"{a.Off:X}").Distinct());
            sb.AppendLine($"{g:X}\t+{offs}\t{(entries.Count == 0 ? "-" : string.Join(",", entries))}\t{string.Join(" ", detail.Take(12))}");
        }
        File.WriteAllText(Path.Combine(dir, "region_types.tsv"), sb.ToString());
        Console.Error.WriteLine($"region types: {result.Count} of {getters.Count} region getters attributed");
        return result;
    }
}
