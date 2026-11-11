using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// BhvHook: patches the rel32 of four `call` instructions inside the BHV state machine step (exe 0x140814570) so
// each call goes through a handler here that runs the original and records what happened. Nothing in the game's
// behaviour is changed. Requires Arxan to be off (me3 disable_arxan).
//
// Output folder: next to this DLL, BhvHook_<yyyyMMdd_HHmmss>\
//   events.log    every state entry, transition outcome, remap and condition-result flip, with timestamps
//   summary.txt   rewritten every 10 s: counts per buffer / state / transition / condition record
//   buffers\      each BHV buffer seen, dumped once as <dataStart>.bhv (the whole file, from header)
//   hook.log      install results
public static unsafe class Hooks
{
    // static VAs (image base 0x140000000) of the call sites and their expected targets
    record Site(string Name, ulong Call, ulong Target);
    static readonly Site EnterSite = new("EnterState", 0x1408149AC, 0x1408155E0);
    static readonly Site TransSite = new("TransitionCheck", 0x14081491A, 0x140814C30);
    static readonly Site CondSite = new("ConditionEval", 0x140814E81, 0x1407DD9C0);
    static readonly Site RemapSite = new("TargetRemap", 0x140814954, 0x1408185C0);

    static nint origEnter, origTrans, origCond, origRemap;
    static string outDir;
    static readonly ConcurrentQueue<string> events = new();
    static readonly ConcurrentDictionary<nint, byte> buffersSeen = new();
    static readonly ConcurrentQueue<nint> buffersToDump = new();
    static readonly ConcurrentDictionary<nint, nint> instanceData = new();
    static readonly ConcurrentDictionary<(nint Data, int From, int To), Counter> enters = new();
    static readonly ConcurrentDictionary<(nint Data, int State, int Result), Counter> transitions = new();
    static readonly ConcurrentDictionary<(nint Data, nint Cond), Counter> conditions = new();
    static readonly ConcurrentDictionary<(int In, int Out), Counter> remaps = new();
    static readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    static int installed;

    sealed class Counter { public long Count, True; public int Last = -1, Type, Flips; }

    // ---- entry point, called by me3 (initializer = { function = "BhvHookInit" })
    [UnmanagedCallersOnly(EntryPoint = "BhvHookInit")]
    public static void Init()
    {
        if (Interlocked.Exchange(ref installed, 1) != 0) return;
        try
        {
            outDir = Path.Combine(Path.GetDirectoryName(OwnPath()) ?? ".", $"BhvHook_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(Path.Combine(outDir, "buffers"));
            var hookLog = new StringBuilder();
            ulong module = (ulong)Native.GetModuleHandleW(null);
            hookLog.AppendLine($"exe base {module:X}");

            origEnter = Patch(module, EnterSite, (nint)(delegate* unmanaged<nint, nint, nint, nint, nint, nint, nint>)&EnterHook, hookLog);
            origTrans = Patch(module, TransSite, (nint)(delegate* unmanaged<nint, nint, nint, nint, nint, nint, nint>)&TransHook, hookLog);
            origCond = Patch(module, CondSite, (nint)(delegate* unmanaged<nint, nint, float, nint, nint, nint, byte>)&CondHook, hookLog);
            origRemap = Patch(module, RemapSite, (nint)(delegate* unmanaged<nint, nint, nint, nint, nint>)&RemapHook, hookLog);
            File.WriteAllText(Path.Combine(outDir, "hook.log"), hookLog.ToString());

            new Thread(Writer) { IsBackground = true, Name = "BhvHook writer" }.Start();
        }
        catch (Exception e)
        {
            try { File.WriteAllText(Path.Combine(outDir ?? Path.GetTempPath(), "hook_error.log"), e.ToString()); } catch { }
        }
    }

    // ---- hooks
    [UnmanagedCallersOnly]
    static nint EnterHook(nint inst, nint a2, nint view, nint a4, nint a5, nint a6)
    {
        // view from GetState (exe 0x14081A400): +0 record, +8 header dword, +0x10 data start, +0x18 index
        int from = *(int*)inst, q1 = *(int*)(inst + 0x17C), q2 = *(int*)(inst + 0x180);
        int to = *(int*)(view + 0x18); nint data = *(nint*)(view + 0x10);
        nint r = ((delegate* unmanaged<nint, nint, nint, nint, nint, nint, nint>)origEnter)(inst, a2, view, a4, a5, a6);
        SeeBuffer(data); instanceData[inst] = data;
        Count(enters, (data, from, to));
        events.Enqueue($"{Ms()} E inst={inst:X} data={data:X} {from}->{to} q17C={q1} q180={q2} after17C={*(int*)(inst + 0x17C)} after180={*(int*)(inst + 0x180)}");
        return r;
    }

    [UnmanagedCallersOnly]
    static nint TransHook(nint inst, nint a2, nint a3, nint a4, nint a5, nint a6)
    {
        int cur = *(int*)inst;
        nint view = *(nint*)(inst + 0x18);
        nint data = view != 0 ? *(nint*)view : 0;
        nint r = ((delegate* unmanaged<nint, nint, nint, nint, nint, nint, nint>)origTrans)(inst, a2, a3, a4, a5, a6);
        int result = (int)r;
        SeeBuffer(data);
        var c = Count(transitions, (data, cur, result));
        if (result >= 0 || c.Count == 1) events.Enqueue($"{Ms()} T inst={inst:X} data={data:X} state={cur} -> transition {result}");
        return r;
    }

    [UnmanagedCallersOnly]
    static byte CondHook(nint ctx, nint condView, float f, nint a4, nint a5, nint a6)
    {
        byte r = ((delegate* unmanaged<nint, nint, float, nint, nint, nint, byte>)origCond)(ctx, condView, f, a4, a5, a6);
        nint cond = *(nint*)condView, data = *(nint*)(condView + 0x10);
        SeeBuffer(data);
        if (!conditions.TryGetValue((data, cond), out var c)) c = conditions.GetOrAdd((data, cond), new Counter { Type = *(byte*)cond });
        Interlocked.Increment(ref c.Count);
        if (r != 0) Interlocked.Increment(ref c.True);
        int prev = Interlocked.Exchange(ref c.Last, r);
        if (prev != r && Interlocked.Increment(ref c.Flips) <= 200) events.Enqueue($"{Ms()} C data={data:X} cond=+{cond - data:X} type={c.Type} {prev}->{r} f={f:0.###}");
        return r;
    }

    [UnmanagedCallersOnly]
    static nint RemapHook(nint outp, nint idx, nint a3, nint a4)
    {
        nint r = ((delegate* unmanaged<nint, nint, nint, nint, nint>)origRemap)(outp, idx, a3, a4);
        int i = (short)idx, o = r != 0 ? *(int*)r : int.MinValue;
        var c = Count(remaps, (i, o));
        if (i != o && c.Count == 1) events.Enqueue($"{Ms()} R {i} -> {o}");
        return r;
    }

    // ---- helpers
    static Counter Count<TKey>(ConcurrentDictionary<TKey, Counter> d, TKey k) where TKey : notnull
    {
        var c = d.GetOrAdd(k, _ => new Counter());
        Interlocked.Increment(ref c.Count);
        return c;
    }

    static string Ms() => $"{clock.Elapsed.TotalSeconds:0.000}";

    static void SeeBuffer(nint data)
    {
        if (data != 0 && buffersSeen.TryAdd(data, 0)) buffersToDump.Enqueue(data);
    }

    static nint Patch(ulong module, Site s, nint handler, StringBuilder log)
    {
        nint call = (nint)(module + (s.Call - 0x140000000));
        if (*(byte*)call != 0xE8) { log.AppendLine($"{s.Name}: no call at {s.Call:X} (byte {*(byte*)call:X2}); not hooked"); return 0; }
        nint target = call + 5 + *(int*)(call + 1);
        nint expected = (nint)(module + (s.Target - 0x140000000));
        if (target != expected) { log.AppendLine($"{s.Name}: call at {s.Call:X} goes to {target:X}, expected {expected:X}; not hooked"); return 0; }

        // 12-byte stub within +-2 GB of the call: mov rax, handler; jmp rax
        nint stub = AllocNear(call);
        if (stub == 0) { log.AppendLine($"{s.Name}: no memory near {call:X}; not hooked"); return 0; }
        byte* p = (byte*)stub;
        p[0] = 0x48; p[1] = 0xB8; *(long*)(p + 2) = handler; p[10] = 0xFF; p[11] = 0xE0;

        long rel = (long)stub - (long)(call + 5);
        Native.VirtualProtect(call + 1, 4, 0x40, out uint old);
        *(int*)(call + 1) = (int)rel;
        Native.VirtualProtect(call + 1, 4, old, out _);
        Native.FlushInstructionCache(Native.GetCurrentProcess(), call, 5);
        log.AppendLine($"{s.Name}: hooked call at {s.Call:X} (stub {stub:X})");
        return target;
    }

    static nint stubPage, stubNext;
    static nint AllocNear(nint addr)
    {
        if (stubPage == 0 || stubNext + 16 > stubPage + 0x1000)
        {
            for (long d = 0x10000; d < 0x7FF00000; d += 0x10000)
                foreach (long cand in new[] { ((long)addr - d) & ~0xFFFFL, ((long)addr + d) & ~0xFFFFL })
                {
                    nint got = Native.VirtualAlloc((nint)cand, 0x1000, 0x3000, 0x40);
                    if (got != 0) { stubPage = stubNext = got; goto found; }
                }
            return 0;
        }
    found:
        if (Math.Abs((long)stubNext - (long)addr) > 0x7FF00000) return 0;
        nint r = stubNext; stubNext += 16;
        return r;
    }

    static string OwnPath()
    {
        Native.GetModuleHandleExW(6, (nint)(delegate* unmanaged<void>)&Init, out nint mod);
        var sb = new char[1024];
        fixed (char* b = sb) { int n = Native.GetModuleFileNameW(mod, b, 1024); return new string(b, 0, n); }
    }

    // ---- writer thread
    static void Writer()
    {
        using var ev = new StreamWriter(Path.Combine(outDir, "events.log"), true, Encoding.UTF8);
        var lastSummary = clock.Elapsed;
        while (true)
        {
            Thread.Sleep(500);
            try
            {
                while (events.TryDequeue(out var e)) ev.WriteLine(e);
                ev.Flush();
                while (buffersToDump.TryDequeue(out var data)) DumpBuffer(data);
                if ((clock.Elapsed - lastSummary).TotalSeconds >= 10) { WriteSummary(); lastSummary = clock.Elapsed; }
            }
            catch (Exception e) { try { ev.WriteLine($"writer error: {e.Message}"); } catch { } }
        }
    }

    static void DumpBuffer(nint data)
    {
        byte* file = (byte*)(data - 0x20);
        short ver = *(short*)file; int size = *(int*)(file + 4);
        if (*(short*)(file + 2) != 3 || size < 0x40 || size > 0x2000000) return;
        File.WriteAllBytes(Path.Combine(outDir, "buffers", $"{data:X}.bhv"), new ReadOnlySpan<byte>(file, size).ToArray());
    }

    static void WriteSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"t={Ms()}s buffers={buffersSeen.Count} instances={instanceData.Count}");
        sb.AppendLine("== enters (data from to count)");
        foreach (var (k, c) in enters.OrderBy(k => k.Key.Data).ThenBy(k => k.Key.From).ThenBy(k => k.Key.To)) sb.AppendLine($"{k.Data:X} {k.From} {k.To} {c.Count}");
        sb.AppendLine("== transitions (data state result count)");
        foreach (var (k, c) in transitions.OrderBy(k => k.Key.Data).ThenBy(k => k.Key.State).ThenBy(k => k.Key.Result)) sb.AppendLine($"{k.Data:X} {k.State} {k.Result} {c.Count}");
        sb.AppendLine("== conditions (data condOffset type evaluated true)");
        foreach (var (k, c) in conditions.OrderBy(k => k.Key.Data).ThenBy(k => k.Key.Cond)) sb.AppendLine($"{k.Data:X} {k.Cond - k.Data:X} {c.Type} {c.Count} {c.True}");
        sb.AppendLine("== remaps (in out count)");
        foreach (var (k, c) in remaps.OrderBy(k => k.Key.In)) sb.AppendLine($"{k.In} {k.Out} {c.Count}");
        File.WriteAllText(Path.Combine(outDir, "summary.txt"), sb.ToString());
    }
}

static unsafe class Native
{
    [DllImport("kernel32")] public static extern nint GetModuleHandleW(char* name);
    [DllImport("kernel32")] public static extern bool GetModuleHandleExW(uint flags, nint addr, out nint module);
    [DllImport("kernel32")] public static extern int GetModuleFileNameW(nint module, char* buf, int size);
    [DllImport("kernel32")] public static extern bool VirtualProtect(nint addr, nint size, uint prot, out uint old);
    [DllImport("kernel32")] public static extern nint VirtualAlloc(nint addr, nint size, uint type, uint prot);
    [DllImport("kernel32")] public static extern bool FlushInstructionCache(nint proc, nint addr, nint size);
    [DllImport("kernel32")] public static extern nint GetCurrentProcess();
}
