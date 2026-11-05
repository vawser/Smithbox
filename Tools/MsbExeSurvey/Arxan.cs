using Iced.Intel;

/// <summary>
/// Undoes Arxan's control-flow stubs. A function jumps into the second .text section, where the original instructions
/// are mixed with stack tricks that push constant addresses, choose one with cmovcc, and jump through [rsp-8]. Emulating
/// only the stack and constant registers gives the original instructions and the real branch, jump or call.
/// </summary>
public static class Arxan
{
    public sealed class Stub
    {
        public List<Instruction> Real = new();
        public List<ulong> Targets = new();   // one: jump/call target; two: conditional (taken, not taken)
        public ConditionCode Cond;
        public bool IsCall;
        public ulong Return;
        public bool IsRet;     // returns to the caller
        public int Mech;       // stack tricks seen; 0 means an ordinary function
    }

    static ulong lo, hi;
    static readonly Dictionary<ulong, Stub> Cache = new();

    public static bool InStubs(ulong va)
    {
        if (hi == 0)
        {
            var s = Exe.Sections.Where(x => x.Name == ".text").OrderBy(x => x.VA).ToList();
            if (s.Count < 2) { hi = 1; return false; }
            lo = Exe.ImageBase + s[1].VA; hi = lo + s[1].VSize;
        }
        return va >= lo && va < hi;
    }

    abstract record Val;
    sealed record Const(ulong V) : Val;
    sealed record Sym(Register R) : Val;
    sealed record Choice(ConditionCode Cc, Val Taken, Val Not) : Val;
    sealed record Unknown : Val;

    public static Stub Resolve(ulong va)
    {
        lock (Cache)
            if (Cache.TryGetValue(va, out var c)) return c;
        var r = Emulate(va);
        lock (Cache) Cache[va] = r;
        return r;
    }

    static Stub Emulate(ulong va)
    {
        var regs = new Dictionary<Register, Val>();
        var stack = new Dictionary<long, Val>();
        long sp = 0;
        var stub = new Stub();
        Val Get(Register r) => regs.TryGetValue(r.GetFullRegister(), out var v) ? v : new Sym(r.GetFullRegister());

        var ip = va;
        for (var steps = 0; steps < 400; steps++)
        {
            var fo = Exe.FileOffset(ip);
            if (fo <= 0) return null;
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, 16));
            dec.IP = ip;
            dec.Decode(out var ins);
            if (ins.IsInvalid) return null;
            ip = ins.NextIP;

            bool RspMem(int op) => ins.GetOpKind(op) == OpKind.Memory && ins.MemoryBase == Register.RSP && ins.MemoryIndex == Register.None;
            long Slot() => sp + (long)ins.MemoryDisplacement64;

            switch (ins.Mnemonic)
            {
                case Mnemonic.Lea when ins.Op0Register == Register.RSP && ins.MemoryBase == Register.RSP:
                    sp += (long)ins.MemoryDisplacement64; stub.Mech++;
                    continue;
                case Mnemonic.Pushfq: sp -= 8; stack[sp] = new Unknown(); continue;
                case Mnemonic.Popfq: sp += 8; continue;
                case Mnemonic.Push:
                    sp -= 8;
                    stack[sp] = ins.Op0Kind == OpKind.Register ? Get(ins.Op0Register) : ins.Op0Kind == OpKind.Memory ? new Unknown() : new Const((ulong)(long)ins.GetImmediate(0));
                    continue;
                case Mnemonic.Pop when ins.Op0Kind == OpKind.Register:
                    regs[ins.Op0Register.GetFullRegister()] = stack.TryGetValue(sp, out var pv) ? pv : new Unknown();
                    sp += 8;
                    continue;
                case Mnemonic.Mov when ins.Op0Kind == OpKind.Register && ins.Op1Kind == OpKind.Immediate64:
                    regs[ins.Op0Register.GetFullRegister()] = new Const(ins.Immediate64);
                    continue;
                case Mnemonic.Mov when RspMem(0) && Slot() < 0 && ins.Op1Kind == OpKind.Register:
                    stack[Slot()] = Get(ins.Op1Register);
                    continue;
                case Mnemonic.Mov when ins.Op0Kind == OpKind.Register && RspMem(1) && Slot() < 0:
                    regs[ins.Op0Register.GetFullRegister()] = stack.TryGetValue(Slot(), out var mv) ? mv : new Unknown();
                    continue;
                case Mnemonic.Xchg when ins.Op0Kind == OpKind.Memory && RspMem(0) && Slot() < 0 && ins.Op1Kind == OpKind.Register:
                    {
                        stub.Mech++;
                        var a = Get(ins.Op1Register);
                        regs[ins.Op1Register.GetFullRegister()] = stack.TryGetValue(Slot(), out var xv2) ? xv2 : new Unknown();
                        stack[Slot()] = a;
                        continue;
                    }
                case Mnemonic.Xchg when ins.Op0Kind == OpKind.Register && RspMem(1) && Slot() < 0:
                    {
                        var a = Get(ins.Op0Register);
                        regs[ins.Op0Register.GetFullRegister()] = stack.TryGetValue(Slot(), out var xv) ? xv : new Unknown();
                        stack[Slot()] = a;
                        continue;
                    }
                case Mnemonic.Jmp when ins.Op0Kind == OpKind.NearBranch64 && InStubs(ins.NearBranchTarget):
                    ip = ins.NearBranchTarget;
                    continue;
                case Mnemonic.Jmp when ins.Op0Kind == OpKind.NearBranch64:
                    stub.Targets.Add(ins.NearBranchTarget);
                    return Finish(stub, sp, stack);
                case Mnemonic.Jmp when RspMem(0):
                    {
                        // through the caller's return address: a disguised ret
                        if (Slot() == 0 && sp == 8 && !stack.ContainsKey(0)) { stub.IsRet = true; stub.Mech++; return stub; }
                        var t = stack.TryGetValue(Slot(), out var tv) ? tv : new Unknown();
                        if (!AddTarget(stub, t)) return null;
                        return Finish(stub, sp, stack);
                    }
                case Mnemonic.Ret:
                    {
                        if (sp == 0 && !stack.ContainsKey(0)) { stub.IsRet = true; return stub; }
                        var t = stack.TryGetValue(sp, out var tv) ? tv : new Unknown();
                        sp += 8;
                        if (!AddTarget(stub, t)) return null;
                        return Finish(stub, sp, stack);
                    }
            }

            if (ins.Mnemonic.ToString().StartsWith("Cmov") && ins.Op0Kind == OpKind.Register && ins.Op1Kind == OpKind.Register)
            {
                var d = ins.Op0Register.GetFullRegister();
                regs[d] = new Choice(ins.ConditionCode, Get(ins.Op1Register), Get(d));
                continue;
            }
            if (ins.FlowControl is FlowControl.ConditionalBranch or FlowControl.Call or FlowControl.IndirectBranch or FlowControl.IndirectCall)
                return null;

            // an original instruction
            stub.Real.Add(ins);
            if (ins.OpCount > 0 && ins.Op0Kind == OpKind.Register) regs.Remove(ins.Op0Register.GetFullRegister());
        }
        return null;
    }

    static bool AddTarget(Stub s, Val t)
    {
        switch (t)
        {
            case Const c: s.Targets.Add(c.V); return true;
            case Choice { Taken: Const a, Not: Const b } ch: s.Cond = ch.Cc; s.Targets.Add(a.V); s.Targets.Add(b.V); return true;
            default: return false;
        }
    }

    static Stub Finish(Stub s, long sp, Dictionary<long, Val> stack)
    {
        // a value left pushed below the entry stack is the return address of a call
        if (sp == -8 && stack.TryGetValue(-8, out var r) && r is Const rc) { s.IsCall = true; s.Return = rc.V; }
        else if (sp != 0) return null;
        return s;
    }

    /// <summary>
    /// The stub as plain instructions: the original ones, then a call, jcc + jmp, or jmp, placed at the jump's address
    /// so branch bookkeeping works
    /// </summary>
    public static List<Instruction> Expand(Instruction jmp)
    {
        var s = Resolve(jmp.NearBranchTarget);
        if (s == null || s.Mech == 0 || (s.Targets.Count == 0 && !s.IsRet)) return null;
        var list = new List<Instruction>(s.Real);
        Instruction Br(Code code, ulong target)
        {
            var i = Instruction.CreateBranch(code, target);
            i.IP = jmp.IP; i.Length = jmp.Length; i.NextIP = jmp.NextIP;
            return i;
        }
        if (s.IsRet)
        {
            var r = Instruction.Create(Code.Retnq);
            r.IP = jmp.IP; r.Length = jmp.Length; r.NextIP = jmp.NextIP;
            list.Add(r);
        }
        else if (s.IsCall)
        {
            list.Add(Br(Code.Call_rel32_64, s.Targets[0]));
            if (s.Return != jmp.NextIP) list.Add(Br(Code.Jmp_rel32_64, s.Return));
        }
        else if (s.Targets.Count == 2)
        {
            list.Add(Br(JccCode(s.Cond), s.Targets[0]));
            list.Add(Br(Code.Jmp_rel32_64, s.Targets[1]));
        }
        else list.Add(Br(Code.Jmp_rel32_64, s.Targets[0]));
        return list;
    }

    static Code JccCode(ConditionCode c) => c switch
    {
        ConditionCode.e => Code.Je_rel32_64, ConditionCode.ne => Code.Jne_rel32_64,
        ConditionCode.b => Code.Jb_rel32_64, ConditionCode.ae => Code.Jae_rel32_64,
        ConditionCode.be => Code.Jbe_rel32_64, ConditionCode.a => Code.Ja_rel32_64,
        ConditionCode.l => Code.Jl_rel32_64, ConditionCode.ge => Code.Jge_rel32_64,
        ConditionCode.le => Code.Jle_rel32_64, ConditionCode.g => Code.Jg_rel32_64,
        ConditionCode.s => Code.Js_rel32_64, ConditionCode.ns => Code.Jns_rel32_64,
        ConditionCode.o => Code.Jo_rel32_64, ConditionCode.no => Code.Jno_rel32_64,
        ConditionCode.p => Code.Jp_rel32_64, _ => Code.Jnp_rel32_64,
    };
}
