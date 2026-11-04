using Iced.Intel;
using System.Text;

/// <summary>
/// Follows an event through the time act dispatcher with its type known, and records each read through the
/// event's parameter pointer: the offset, the width, and whether the game treats it as a float, a signed or an
/// unsigned value. The pointer and the event context are followed through moves, the stack and direct calls.
/// </summary>
public static class ParamTrace
{
    public enum Kind { Ctx, Param, Type, Value, This, Chr, Bag, Module, ChrSub, Vtbl }

    public readonly record struct Tag(Kind Kind, long Value);

    public sealed record Access(long Offset, int Size, string How, ulong Where, bool Write, int Depth);

    sealed class State
    {
        public Dictionary<string, Tag> Regs = new();
        public long Rsp;          // rsp relative to the function's entry
        public bool TypeCmp;      // the flags hold a compare of the type
        public long TypeCmpValue; // type delta
        public long TypeCmpImm;   // the immediate compared against
        public bool TypeSwitch;   // the switch about to be taken was bounds checked on the type

        public State Clone() => new()
        {
            Regs = new Dictionary<string, Tag>(Regs),
            Rsp = Rsp,
            TypeCmp = TypeCmp,
            TypeCmpValue = TypeCmpValue,
            TypeCmpImm = TypeCmpImm,
            TypeSwitch = TypeSwitch,
        };

        public string Key() => $"{Rsp}|{TypeCmp}{TypeCmpValue}:{TypeCmpImm}|{TypeSwitch}|" + string.Join(",", Regs.OrderBy(r => r.Key).Select(r => $"{r.Key}={r.Value.Kind}{r.Value.Value}"));
    }

    static Dictionary<ulong, ExeScan.Switch> switches;
    static List<(uint Begin, uint End)> functions;

    public sealed class Result
    {
        public List<Access> Accesses = new();
        public List<Access> CtxAccesses = new();
        public HashSet<string> Notes = new();
        public HashSet<ulong> Functions = new();
        public List<(long Offset, string Sink)> Sinks = new();
        public List<(long Offset, string Const)> Consts = new();
        public List<string> Effects = new();
        public int Steps;
    }

    public static void Init()
    {
        switches = ExeScan.FindSwitches().GroupBy(s => s.Address).ToDictionary(g => g.Key, g => g.First());

        // .pdata gives each function's bounds
        var pdata = ExeScan.Sections.First(s => s.Name == ".pdata");
        functions = new();
        for (var o = pdata.Raw; o + 12 <= pdata.Raw + pdata.VSize; o += 12)
        {
            var b = BitConverter.ToUInt32(ExeScan.Image, (int)o);
            var e = BitConverter.ToUInt32(ExeScan.Image, (int)o + 4);
            if (b == 0) break;
            functions.Add((b, e));
        }
        functions.Sort();
    }

    static (uint, uint) FunctionOf(ulong va)
    {
        var rva = (uint)(va - ExeScan.ImageBase);
        int lo = 0, hi = functions.Count - 1;
        while (lo <= hi)
        {
            var mid = (lo + hi) / 2;
            if (functions[mid].Begin <= rva) lo = mid + 1; else hi = mid - 1;
        }
        if (hi >= 0 && rva < functions[hi].End) return functions[hi];
        return (rva, rva + 0x1000);
    }

    public static Result Trace(ulong entry, int type, Dictionary<string, Tag> initial)
    {
        var result = new Result();
        current = result;
        var st = new State { Regs = new(initial) };
        Walk(entry, st, type, result, 0, new HashSet<string>());
        return result;
    }

    static readonly string[] Volatile = { "RAX", "RCX", "RDX", "R8", "R9", "R10", "R11", "XMM0", "XMM1", "XMM2", "XMM3", "XMM4", "XMM5" };
    static readonly string[] Args = { "RCX", "RDX", "R8", "R9" };
    static readonly string[] FloatArgs = { "XMM0", "XMM1", "XMM2", "XMM3" };
    [ThreadStatic] static Result current;
    [ThreadStatic] static ulong currentFunction;

    static void Walk(ulong entry, State initial, int type, Result result, int depth, HashSet<string> callStack)
    {
        if (depth > 7) return;

        var callKey = $"{entry:X}|{initial.Key()}";
        if (!callStack.Add(callKey)) return;
        result.Functions.Add(entry);

        currentFunction = entry;
        var (fb, fe) = FunctionOf(entry);
        var visited = new HashSet<string>();
        var work = new Stack<(ulong, State)>();
        work.Push((entry, initial));

        while (work.Count > 0)
        {
            var (ip, s) = work.Pop();

            while (true)
            {
                if (++result.Steps > 400000) return;
                if (!visited.Add($"{ip:X}|{s.Key()}")) break;
                if (visited.Count > 6000) return;

                var fo = ExeScan.FileOffset(ip);
                if (fo < 0) break;
                var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)fo, 16));
                decoder.IP = ip;
                decoder.Decode(out var ins);
                if (ins.IsInvalid) break;

                var next = ins.NextIP;
                var flow = ins.FlowControl;

                RecordMemory(ins, s, result, depth);
                RecordObjectWrite(s, ins, result, depth);

                if (flow == FlowControl.Return || ins.Mnemonic == Mnemonic.Int3 || ins.Mnemonic == Mnemonic.Ud2)
                    break;

                if (flow == FlowControl.Call)
                {
                    RecordObjectCall(s, ins, result, depth);
                    if (ins.Op0Kind == OpKind.NearBranch64)
                    {
                        var callee = ins.NearBranchTarget;
                        RecordCallSinks(s, callee, result);
                        var cs = ArgState(s);
                        if (cs.Regs.Count > 0)
                            Walk(callee, cs, type, result, depth + 1, callStack);
                    }
                    else if (Args.Any(a => s.Regs.TryGetValue(a, out var t) && t.Kind != Kind.Type))
                    {
                        result.Notes.Add($"indirect call with the event at 0x{ins.IP:X} ({string.Join(",", Args.Where(a => s.Regs.ContainsKey(a)).Select(a => $"{a}={s.Regs[a].Kind}+{s.Regs[a].Value:X}"))})");
                    }

                    foreach (var v in Volatile) s.Regs.Remove(v);
                    s.TypeCmp = false;
                    ip = next;
                    continue;
                }

                if (flow == FlowControl.UnconditionalBranch)
                {
                    var target = ins.NearBranchTarget;
                    var rva = (uint)(target - ExeScan.ImageBase);

                    // a jump out of the function is a tail call
                    if (rva < fb || rva >= fe)
                    {
                        RecordObjectCall(s, ins, result, depth);
                        RecordCallSinks(s, target, result);
                        var cs = ArgState(s);
                        cs.Rsp = 0;
                        if (cs.Regs.Count > 0)
                            Walk(target, cs, type, result, depth + 1, callStack);
                        break;
                    }

                    ip = target;
                    continue;
                }

                if (flow == FlowControl.IndirectBranch)
                {
                    RecordObjectCall(s, ins, result, depth);
                    if (switches.TryGetValue(ins.IP, out var sw))
                    {
                        if (s.TypeSwitch)
                        {
                            var t = sw.Targets.TryGetValue(type, out var x) ? x : sw.DefaultTarget;
                            var ns = s.Clone(); ns.TypeSwitch = false;
                            work.Push((t, ns));
                        }
                        else
                        {
                            foreach (var t in sw.Targets.Values.Append(sw.DefaultTarget).Distinct())
                            {
                                var ns = s.Clone(); ns.TypeSwitch = false;
                                work.Push((t, ns));
                            }
                        }
                    }
                    else if (ins.Op0Kind == OpKind.Memory && ins.MemoryBase == Register.RIP)
                    {
                        // import thunk, nothing to follow
                    }
                    else
                    {
                        result.Notes.Add($"unresolved indirect jump at 0x{ins.IP:X}");
                    }
                    break;
                }

                if (flow == FlowControl.ConditionalBranch)
                {
                    var target = ins.NearBranchTarget;

                    if (s.TypeCmp)
                    {
                        var taken = Evaluate(ins.ConditionCode, type + s.TypeCmpValue, s.TypeCmpImm);
                        if (taken.HasValue)
                        {
                            // A switch on the type is bounds checked by a ja past the table
                            if (!taken.Value && ins.ConditionCode == ConditionCode.a)
                                s.TypeSwitch = true;
                            ip = taken.Value ? target : next;
                            continue;
                        }
                    }

                    work.Push((target, s.Clone()));
                    ip = next;
                    continue;
                }

                currentFunction = entry;
                Step(ins, s);
                ip = next;
            }
        }
    }

    static State ArgState(State s)
    {
        var cs = new State();
        foreach (var a in Args.Concat(FloatArgs))
            if (s.Regs.TryGetValue(a, out var t))
                cs.Regs[a] = t;
        return cs;
    }

    /// <summary>
    /// A field's value passed to a function is a sink that names it, when a named field reaches the same one
    /// </summary>
    static void RecordCallSinks(State s, ulong callee, Result result)
    {
        for (var i = 0; i < 4; i++)
        {
            if (s.Regs.TryGetValue(Args[i], out var t) && t.Kind == Kind.Value)
                result.Sinks.Add((t.Value, $"call {callee:X} arg{i + 1}"));
            if (s.Regs.TryGetValue(FloatArgs[i], out var f) && f.Kind == Kind.Value)
                result.Sinks.Add((f.Value, $"call {callee:X} farg{i + 1}"));
        }
    }

    /// <summary>
    /// The modules of a character's bag (chr+190h). Offsets read from the bag builder at 0x1403FE190, with
    /// the slots it doesn't create directly taken from fromsoftware-rs ChrInsModuleContainer.
    /// </summary>
    public static long BagOffset = 0x190;

    /// <summary>
    /// Nightreign keeps the bag at chr+1B8h, with its own modules after +118h (read from its builders at
    /// 0x1403DE8C0 and 0x140673220)
    /// </summary>
    public static void UseNightreign()
    {
        BagOffset = 0x1B8;
        foreach (var (o, n) in new (long, string)[] {
            (0x118, "GrassHit"), (0x128, "CliffWind"), (0x138, "Hookshot"), (0x140, "WallEdgeLanding"), (0x148, "Skill"),
            (0x150, "PlayerResonance"), (0x158, "JamaisVu"), (0x160, "NecroBuddy"), (0x168, "RebornBuddy"), (0x170, "BirdMove"),
            (0x178, "ThrowCounter"), (0x180, "MixcraftMagic"), (0x188, "PostureControl"), (0x190, "Majyuu"),
            (0x1A0, "FatalDamageDodge"), (0x1A8, "PlayerAnalyzeCam"), (0x1B0, "PlayerJoint"), (0x1B8, "Trans"), (0x1C0, "BonePile") })
            Modules[o] = n;
        Modules.Remove(0x120); Modules.Remove(0x130);
    }

    public static readonly Dictionary<long, string> Modules = new()
    {
        [0x00] = "Data", [0x08] = "ActionFlag", [0x10] = "BehaviorScript", [0x18] = "TimeAct", [0x20] = "Resist",
        [0x28] = "Behavior", [0x30] = "BehaviorSync", [0x38] = "Ai", [0x40] = "SuperArmor", [0x48] = "Toughness",
        [0x50] = "Talk", [0x58] = "Event", [0x60] = "Magic", [0x68] = "Physics", [0x70] = "Fall", [0x78] = "Ladder",
        [0x80] = "ActionRequest", [0x88] = "Throw", [0x90] = "HitStop", [0x98] = "Damage", [0xA0] = "Material",
        [0xA8] = "KnockBack", [0xB0] = "Sfx", [0xB8] = "Vfx", [0xC0] = "BehaviorData", [0xC8] = "UnkC8",
        [0xD0] = "ModelParamModifier", [0xD8] = "Dripping", [0xE0] = "UnkE0", [0xE8] = "Ride", [0xF0] = "BoneMove",
        [0xF8] = "Wet", [0x100] = "AutoHoming", [0x108] = "AboveShadowTest", [0x110] = "SwordArts",
        [0x118] = "GrassHit", [0x120] = "WheelRot", [0x128] = "CliffWind", [0x130] = "NavimeshCostEffect",
    };

    static bool IsObject(Kind k) => k is Kind.This or Kind.Chr or Kind.Bag or Kind.Module or Kind.ChrSub;

    static string ObjName(Tag t) => t.Kind switch
    {
        Kind.This => "TimeActModule",
        Kind.Chr => "ChrIns",
        Kind.Bag => "Modules",
        Kind.Module => Modules.TryGetValue(t.Value, out var n) ? n + "Module" : $"Module+{t.Value:X}",
        Kind.ChrSub => $"ChrIns+{t.Value:X}",
        _ => t.Kind.ToString()
    };

    // a vtable tag keeps its owner: the kind in the high bits, the offset in the low
    static Tag VtblOf(Tag owner) => new(Kind.Vtbl, ((long)owner.Kind << 32) | (owner.Value & 0xFFFFFFFF));
    static Tag OwnerOf(Tag vt) => new((Kind)(vt.Value >> 32), (int)(vt.Value & 0xFFFFFFFF));

    /// <summary>
    /// Loads of the character and its modules: this+18h is the character, chr+190h its modules
    /// </summary>
    static Tag? ObjectLoad(Tag bt, long d)
    {
        return bt.Kind switch
        {
            Kind.This when d == 0x18 => new Tag(Kind.Chr, 0),
            Kind.Chr when d == BagOffset => new Tag(Kind.Bag, 0),
            Kind.Chr when d == 0 => VtblOf(bt),
            Kind.Chr => new Tag(Kind.ChrSub, d),
            Kind.Bag => new Tag(Kind.Module, d),
            Kind.Module or Kind.ChrSub or Kind.This when d == 0 => VtblOf(bt),
            _ => null
        };
    }

    static void RecordObjectCall(State s, Instruction ins, Result result, int depth)
    {
        string fieldArgs()
        {
            var parts = new List<string>();
            for (var i = 0; i < 4; i++)
            {
                if (s.Regs.TryGetValue(Args[i], out var a) && a.Kind == Kind.Value) parts.Add($"a{i + 1}=+{a.Value:X2}");
                if (s.Regs.TryGetValue(FloatArgs[i], out var f) && f.Kind == Kind.Value) parts.Add($"a{i + 1}=+{f.Value:X2}f");
            }
            return parts.Count > 0 ? " (" + string.Join(",", parts) + ")" : "";
        }

        if (ins.Op0Kind == OpKind.NearBranch64)
        {
            if (s.Regs.TryGetValue("RCX", out var rcx) && IsObject(rcx.Kind) && rcx.Kind != Kind.This)
            {
                var v = Rtti.VirtualOf.TryGetValue(ins.NearBranchTarget, out var vf) ? $" [{vf.Class.Replace("CS::", "")}::vf{vf.Index}]" : "";
                result.Effects.Add($"d{depth} call {ins.NearBranchTarget:X} on {ObjName(rcx)}{v}{fieldArgs()}");
            }
        }
        else if (ins.Op0Kind == OpKind.Memory && ins.MemoryIndex == Register.None && s.Regs.TryGetValue(Key(ins.MemoryBase), out var vt) && vt.Kind == Kind.Vtbl)
        {
            result.Effects.Add($"d{depth} vcall {ObjName(OwnerOf(vt))}::vf{ins.MemoryDisplacement64 / 8}{fieldArgs()}");
        }
    }

    static void RecordObjectWrite(State s, Instruction ins, Result result, int depth)
    {
        if (ins.Op0Kind != OpKind.Memory || ins.MemoryIndex != Register.None) return;
        var bt = BaseTag(s, ins);
        if (bt == null || !IsObject(bt.Value.Kind) || bt.Value.Kind == Kind.Bag) return;
        var m = ins.Mnemonic;
        if (m is Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Bt) return;

        string value = "?";
        if (ins.OpCount > 1 && IsImm(ins.Op1Kind)) value = $"0x{ins.GetImmediate(1):X}";
        else if (ins.OpCount > 1 && ins.Op1Kind == OpKind.Register)
            value = s.Regs.TryGetValue(Key(ins.Op1Register), out var v) ? (v.Kind == Kind.Value ? $"field +{v.Value:X2}" : v.Kind.ToString()) : "reg";
        var op = m switch { Mnemonic.Or => "|=", Mnemonic.And => "&=", Mnemonic.Bts => "bts", Mnemonic.Btr => "btr", Mnemonic.Add or Mnemonic.Addss => "+=", _ => "=" };
        result.Effects.Add($"d{depth} {ObjName(bt.Value)}+{ins.MemoryDisplacement64:X} {op} {value}");
    }

    static bool IsImm(OpKind k) => k is OpKind.Immediate8 or OpKind.Immediate8to32 or OpKind.Immediate32 or OpKind.Immediate8to64 or OpKind.Immediate32to64 or OpKind.Immediate16 or OpKind.Immediate8to16;

    /// <summary>
    /// Tracks the values loaded from the parameters, and records what they are compared against and stored into
    /// </summary>
    static bool StepValue(Instruction ins, State s)
    {
        var m = ins.Mnemonic;

        // a load of a field
        if (ins.Op0Kind == OpKind.Register && ins.OpCount >= 2 && ins.Op1Kind == OpKind.Memory && ins.MemoryIndex == Register.None
            && m is Mnemonic.Mov or Mnemonic.Movzx or Mnemonic.Movsx or Mnemonic.Movsxd or Mnemonic.Movss or Mnemonic.Movd or Mnemonic.Cvtss2sd or Mnemonic.Cvtsi2ss)
        {
            var bt = BaseTag(s, ins);
            if (bt?.Kind == Kind.Param && !(m == Mnemonic.Mov && ins.MemorySize.GetSize() == 8))
            {
                s.Regs[Key(ins.Op0Register)] = new Tag(Kind.Value, bt.Value.Value + (long)ins.MemoryDisplacement64);
                s.TypeCmp = false;
                return true;
            }
        }

        // compares of a field against a constant, as in an enum or a flag
        if ((m == Mnemonic.Cmp || m == Mnemonic.Test) && IsImm(ins.Op1Kind))
        {
            long? off = null;
            if (ins.Op0Kind == OpKind.Register && s.Regs.TryGetValue(Key(ins.Op0Register), out var t) && t.Kind == Kind.Value) off = t.Value;
            else if (ins.Op0Kind == OpKind.Memory && BaseTag(s, ins)?.Kind == Kind.Param) off = BaseTag(s, ins).Value.Value + (long)ins.MemoryDisplacement64;
            if (off != null)
                current.Consts.Add((off.Value, $"{(m == Mnemonic.Test ? "bit" : "")}{(long)(int)ins.GetImmediate(1)}"));
        }

        // a field stored into an object
        if (ins.Op0Kind == OpKind.Memory && ins.OpCount >= 2 && ins.Op1Kind == OpKind.Register && ins.MemoryBase != Register.RSP
            && s.Regs.TryGetValue(Key(ins.Op1Register), out var v) && v.Kind == Kind.Value && BaseTag(s, ins) == null)
        {
            current.Sinks.Add((v.Value, $"store {currentFunction:X} +{ins.MemoryDisplacement64:X}"));
        }

        return false;
    }

    static bool? Evaluate(ConditionCode cc, long left, long right)
    {
        // 32 bit compares, as the type is an int
        var ul = (uint)left; var ur = (uint)right;
        var sl = (int)left; var sr = (int)right;
        return cc switch
        {
            ConditionCode.e => ul == ur,
            ConditionCode.ne => ul != ur,
            ConditionCode.a => ul > ur,
            ConditionCode.ae => ul >= ur,
            ConditionCode.b => ul < ur,
            ConditionCode.be => ul <= ur,
            ConditionCode.g => sl > sr,
            ConditionCode.ge => sl >= sr,
            ConditionCode.l => sl < sr,
            ConditionCode.le => sl <= sr,
            ConditionCode.s => sl - sr < 0,
            ConditionCode.ns => sl - sr >= 0,
            _ => null
        };
    }

    static string Key(Register r) => r.IsGPR() ? r.GetFullRegister().ToString() : r.ToString();

    static string StackKey(State s, Instruction ins) => $"STK{s.Rsp + (long)ins.MemoryDisplacement64}";

    static Tag? BaseTag(State s, Instruction ins)
    {
        if (ins.MemoryBase == Register.None || ins.MemoryBase == Register.RIP) return null;
        return s.Regs.TryGetValue(Key(ins.MemoryBase), out var t) ? t : null;
    }

    static void RecordMemory(Instruction ins, State s, Result result, int depth)
    {
        for (var op = 0; op < ins.OpCount; op++)
        {
            if (ins.GetOpKind(op) != OpKind.Memory) continue;
            if (ins.Mnemonic == Mnemonic.Lea || ins.Mnemonic == Mnemonic.Nop) return;

            var bt = BaseTag(s, ins);
            if (bt == null) return;

            var disp = (long)ins.MemoryDisplacement64;
            if (ins.MemoryDisplacement64 > 0x7FFFFFFF && ins.MemoryDisplacement64 > 0xFFFFFFFF00000000) disp = (long)ins.MemoryDisplacement64;
            var size = ins.MemorySize.GetSize();
            var how = $"{ins.Mnemonic.ToString().ToLowerInvariant()}:{ins.MemorySize}{(ins.MemoryIndex != Register.None ? "[idx]" : "")}";
            var write = op == 0 && ins.Mnemonic is Mnemonic.Mov or Mnemonic.Movss or Mnemonic.Movups or Mnemonic.Movaps or Mnemonic.Movdqu or Mnemonic.Movsd or Mnemonic.Movq or Mnemonic.Movd;

            if (bt.Value.Kind == Kind.Param)
                result.Accesses.Add(new Access(bt.Value.Value + disp, size, how, ins.IP, write, depth));
            else if (bt.Value.Kind == Kind.Ctx)
                result.CtxAccesses.Add(new Access(disp, size, how, ins.IP, write, depth));
            return;
        }
    }

    static void Step(Instruction ins, State s)
    {
        var m = ins.Mnemonic;

        if (StepValue(ins, s))
            return;

        // rsp bookkeeping
        if (m == Mnemonic.Push) { s.Rsp -= 8; return; }
        if (m == Mnemonic.Pop)
        {
            s.Rsp += 8;
            if (ins.Op0Kind == OpKind.Register) s.Regs.Remove(Key(ins.Op0Register));
            return;
        }

        if (ins.Op0Kind == OpKind.Register && ins.Op0Register == Register.RSP)
        {
            if (m == Mnemonic.Sub && ins.Op1Kind != OpKind.Register) s.Rsp -= (long)ins.GetImmediate(1);
            else if (m == Mnemonic.Add && ins.Op1Kind != OpKind.Register) s.Rsp += (long)ins.GetImmediate(1);
            return;
        }

        // compares against the type
        if (m == Mnemonic.Cmp && ins.Op0Kind == OpKind.Register && s.Regs.TryGetValue(Key(ins.Op0Register), out var ct) && ct.Kind == Kind.Type
            && ins.Op1Kind is OpKind.Immediate8 or OpKind.Immediate8to32 or OpKind.Immediate32 or OpKind.Immediate8to64 or OpKind.Immediate32to64)
        {
            s.TypeCmp = true;
            s.TypeCmpValue = ct.Value;
            s.TypeCmpImm = (long)(int)ins.GetImmediate(1);
            return;
        }

        if (ins.Op0Kind == OpKind.Register && m == Mnemonic.Test && ins.Op1Kind == OpKind.Register && ins.Op0Register == ins.Op1Register
            && s.Regs.TryGetValue(Key(ins.Op0Register), out var tt) && tt.Kind == Kind.Type)
        {
            s.TypeCmp = true;
            s.TypeCmpValue = tt.Value;
            s.TypeCmpImm = 0;
            return;
        }

        if (m is Mnemonic.Cmp or Mnemonic.Test or Mnemonic.Comiss or Mnemonic.Ucomiss or Mnemonic.Bt)
        {
            s.TypeCmp = false;
            return;
        }

        if (ins.Op0Kind == OpKind.Memory)
        {
            // spills to the stack
            if (m == Mnemonic.Mov && (ins.MemoryBase == Register.RSP) && ins.Op1Kind == OpKind.Register)
            {
                var k = StackKey(s, ins);
                if (s.Regs.TryGetValue(Key(ins.Op1Register), out var t) && ins.MemorySize.GetSize() == 8)
                    s.Regs[k] = t;
                else
                    s.Regs.Remove(k);
            }
            else if (ins.MemoryBase == Register.RSP)
            {
                s.Regs.Remove(StackKey(s, ins));
            }
            return;
        }

        if (ins.Op0Kind != OpKind.Register) return;

        var dst = Key(ins.Op0Register);
        var dstFull = ins.Op0Register.IsGPR64() || (ins.Op0Register.IsGPR32() && m != Mnemonic.Cmp);

        // flags writers that aren't compares
        s.TypeCmp = false;

        if ((m == Mnemonic.Mov || m == Mnemonic.Movsxd || m == Mnemonic.Movzx) && ins.Op1Kind == OpKind.Register)
        {
            if (s.Regs.TryGetValue(Key(ins.Op1Register), out var t) && (ins.Op0Register.IsGPR64() || t.Kind == Kind.Type || t.Kind == Kind.Value))
                s.Regs[dst] = t;
            else
                s.Regs.Remove(dst);
            return;
        }

        if ((m == Mnemonic.Mov || m == Mnemonic.Movsxd || m == Mnemonic.Movzx || m == Mnemonic.Movsx) && ins.Op1Kind == OpKind.Memory)
        {
            var bt = BaseTag(s, ins);

            if (ins.MemoryBase == Register.RSP && ins.MemorySize.GetSize() == 8 && s.Regs.TryGetValue(StackKey(s, ins), out var st))
            {
                s.Regs[dst] = st;
                return;
            }

            if (bt != null && IsObject(bt.Value.Kind) && ins.MemoryIndex == Register.None && ins.MemorySize.GetSize() == 8 && ObjectLoad(bt.Value, (long)ins.MemoryDisplacement64) is { } ot)
            {
                s.Regs[dst] = ot;
                return;
            }

            if (bt?.Kind == Kind.Ctx && ins.MemoryIndex == Register.None)
            {
                var d = (long)ins.MemoryDisplacement64;
                if (d == 8 && ins.MemorySize.GetSize() == 8) { s.Regs[dst] = new Tag(Kind.Param, 0); return; }
                if (d == 0 && ins.MemorySize.GetSize() == 4) { s.Regs[dst] = new Tag(Kind.Type, 0); return; }
            }

            s.Regs.Remove(dst);
            return;
        }

        if (m == Mnemonic.Lea)
        {
            var bt = BaseTag(s, ins);
            if (bt != null && ins.MemoryIndex == Register.None && bt.Value.Kind != Kind.Ctx)
                s.Regs[dst] = new Tag(bt.Value.Kind, bt.Value.Value + (long)ins.MemoryDisplacement64);
            else if (ins.MemoryBase == Register.RSP)
                s.Regs.Remove(dst);
            else
                s.Regs.Remove(dst);
            return;
        }

        if ((m == Mnemonic.Add || m == Mnemonic.Sub) && ins.Op1Kind != OpKind.Register && ins.Op1Kind != OpKind.Memory && s.Regs.TryGetValue(dst, out var at) && at.Kind != Kind.Ctx)
        {
            var imm = (long)(int)ins.GetImmediate(1);
            s.Regs[dst] = new Tag(at.Kind, at.Value + (m == Mnemonic.Add ? imm : -imm));
            if (at.Kind == Kind.Type) { s.TypeCmp = true; s.TypeCmpValue = at.Value + (m == Mnemonic.Add ? imm : -imm); s.TypeCmpImm = 0; }
            return;
        }

        if (m == Mnemonic.Xchg && ins.Op1Kind == OpKind.Register)
        {
            var a = s.Regs.TryGetValue(dst, out var ta) ? ta : (Tag?)null;
            var b = s.Regs.TryGetValue(Key(ins.Op1Register), out var tb) ? tb : (Tag?)null;
            if (b != null) s.Regs[dst] = b.Value; else s.Regs.Remove(dst);
            if (a != null) s.Regs[Key(ins.Op1Register)] = a.Value; else s.Regs.Remove(Key(ins.Op1Register));
            return;
        }

        // copies between vector registers
        if (ins.Op0Register.IsXMM() && ins.Op1Kind == OpKind.Register && m is Mnemonic.Movaps or Mnemonic.Movss or Mnemonic.Movups or Mnemonic.Movd or Mnemonic.Movq)
        {
            if (s.Regs.TryGetValue(Key(ins.Op1Register), out var xt)) s.Regs[dst] = xt; else s.Regs.Remove(dst);
            return;
        }

        // a vector op keeps the field it works on, as with the lerp of a start and end value
        if (ins.Op0Register.IsXMM() && m is Mnemonic.Mulss or Mnemonic.Addss or Mnemonic.Subss or Mnemonic.Divss)
            return;

        // anything else overwrites the destination
        s.Regs.Remove(dst);
    }
}
