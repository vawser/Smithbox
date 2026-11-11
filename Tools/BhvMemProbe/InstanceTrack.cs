using System.Diagnostics;

// "instances" mode: state machine instances hold a pointer to their reader's view (reader+8) at +0x18, and
// EnterState (exe 0x1408155E0) increments instance+0x150. Find readers by vtable, find what points at reader+8,
// and watch those objects: log the entry counter and int fields that move within the file's state range.
static class InstanceTrack
{
    const ulong ReaderVtableStatic = 0x1437FFF98;

    public static void Run(Mem mem, List<(ulong Base, ulong Size)> regions, List<(ulong Addr, int Len, string Name, byte[] Orig)> found,
        Action<string> log, int seconds, ulong modBase, Func<ulong, ulong> toStatic, Func<ulong, bool> isVtable)
    {
        ulong readerVt = ReaderVtableStatic - 0x140000000UL + modBase;
        var dataStart = new Dictionary<ulong, int>();
        for (int b = 0; b < found.Count; b++) dataStart[found[b].Addr + 0x20] = b;

        // 1. readers: [R] = reader vtable, [R+8] = a buffer's data start
        var readers = new Dictionary<ulong, int>();
        mem.Scan(regions, 16, (addr, buf, n) =>
        {
            for (int o = 0; o + 16 <= n; o += 8)
                if (BitConverter.ToUInt64(buf, o) == readerVt && dataStart.TryGetValue(BitConverter.ToUInt64(buf, o + 8), out var b))
                    readers[addr + (ulong)o] = b;
        });
        log($"\n== instance mode: {readers.Count} readers: {string.Join(", ", readers.Values.Distinct().Select(b => found[b].Name.Split(' ')[0]).Take(20))}");

        // 2. views shaped {ptr = data start, dword = header dword, ptr = data start}, anywhere; reader+8 is one.
        //    Run 3 found no pointers to reader+8, so the instance's view at +0x18 must be a separate copy.
        var views = new Dictionary<ulong, int>();
        mem.Scan(regions, 24, (addr, buf, n) =>
        {
            for (int o = 0; o + 24 <= n; o += 8)
            {
                if (!dataStart.TryGetValue(BitConverter.ToUInt64(buf, o), out var b)) continue;
                // run 4 also required {dword header, ptr data start} after it, which missed the player's files;
                // the code only guarantees [view] = data start
                views[addr + (ulong)o] = b;
            }
        });
        log($"{views.Count} views ({views.Keys.Count(v => readers.ContainsKey(v - 8))} inside readers): " +
            string.Join(", ", views.Values.GroupBy(b => found[b].Name.Split(' ')[0]).OrderByDescending(g => g.Count()).Take(12).Select(g => $"{g.Key} x{g.Count()}")));
        var holders = new List<(ulong At, int Buffer)>();
        mem.Scan(regions, 0, (addr, buf, n) =>
        {
            for (int o = 0; o + 8 <= n; o += 8)
                if (views.TryGetValue(BitConverter.ToUInt64(buf, o), out var b) && holders.Count < 50000) holders.Add((addr + (ulong)o, b));
        });
        log($"{holders.Count} slots point at a view; offsets of their 16-aligned position: {string.Join(" ", holders.GroupBy(h => h.At & 0xF).Select(g => $"{g.Key:X}:{g.Count()}"))}");

        // assume the slot is instance+0x18 (EnterState reads [instance+0x18]); watch 0x400 bytes from there
        // instance+0 is the current state index (exe 0x14081460A: GetState([inst+0x18], [inst+0])); c0200 is the player AC in missions
        var inst = holders.Select(h => (Base: h.At - 0x18, h.Buffer)).DistinctBy(i => i.Base)
            .Where(i => mem.Read(i.Base, 4) is byte[] d && BitConverter.ToInt32(d, 0) is var s && s >= 0 && s < BitConverter.ToInt32(found[i.Buffer].Orig, 0x24))
            .OrderBy(i => found[i.Buffer].Name.StartsWith("c0200/") || found[i.Buffer].Name.StartsWith("c0000/") ? 0 : 1).Take(400).ToList();
        log($"{inst.Count} instance candidates with a valid state index at +0 ({inst.Count(i => found[i.Buffer].Name.StartsWith("c0000/"))} for c0000)");
        if (inst.Count == 0)
        {
            log("no slots point at a view: watching 0x400 bytes around each view instead (base = view - 0x100)");
            inst = views.Select(v => (Base: v.Key - 0x100, Buffer: v.Value)).Take(400).ToList();
        }
        string Name(int b) => found[b].Name.Split(' ')[0];
        int StateCount(int b) => BitConverter.ToInt32(found[b].Orig, 0x24);
        string State(int b, int i)
        {
            var f = found[b].Orig; short ver = BitConverter.ToInt16(f, 0);
            int ss = ver >= 10 ? 0xA0 : 0x80, asz = ver >= 7 ? 0x60 : ver == 6 ? 0x50 : ver >= 4 ? 0x40 : 0x30;
            int s = 0x20 + BitConverter.ToInt32(f, 0x20) + i * ss;
            short anim = BitConverter.ToInt16(f, s + 0x10);
            int ac = BitConverter.ToInt32(f, 0x2C);
            int animId = anim >= 0 && anim < ac ? BitConverter.ToInt32(f, 0x20 + BitConverter.ToInt32(f, 0x28) + anim * asz) : -1;
            return $"s{i}(id={BitConverter.ToInt16(f, s)} kind={BitConverter.ToInt16(f, s + 2)} anim={animId})";
        }
        foreach (var (b, i) in inst.Select((x, i) => (x, i)).Take(80))
        {
            var head = mem.Read(b.Base, 0x20);
            string vt = head != null && isVtable(BitConverter.ToUInt64(head, 0)) ? $"vtable {toStatic(BitConverter.ToUInt64(head, 0)):X}" : "no vtable at +0";
            var ctr = mem.Read(b.Base + 0x150, 4);
            log($"  inst#{i} {b.Base:X} {Name(b.Buffer)} states={StateCount(b.Buffer)} {vt} counter(+150)={(ctr == null ? "?" : BitConverter.ToInt32(ctr, 0).ToString())}");
        }

        const int span = 0x400;
        var prev = inst.Select(x => mem.Read(x.Base, span)).ToList();
        log($"\n== scans done. Switch to the game and press F10 to start watching {inst.Count} instances for {seconds}s (F9 = marker).");
        Console.Beep(880, 300);
        Native.GetAsyncKeyState(0x79);
        var wait = Stopwatch.StartNew();
        while ((Native.GetAsyncKeyState(0x79) & 1) == 0 && wait.Elapsed.TotalMinutes < 15) Thread.Sleep(50);
        prev = inst.Select(x => mem.Read(x.Base, span)).ToList();
        Console.Beep(1320, 200);
        Native.GetAsyncKeyState(0x78);

        var stats = new Dictionary<(int I, int Off), (int Count, HashSet<int> Values)>();
        var sw = Stopwatch.StartNew();
        int marker = 0, lines = 0;
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            Thread.Sleep(50);
            double t = sw.Elapsed.TotalSeconds;
            if ((Native.GetAsyncKeyState(0x78) & 1) != 0) log($"t={t:0.00}s MARK {++marker}");
            for (int i = 0; i < inst.Count; i++)
            {
                var now = mem.Read(inst[i].Base, span);
                if (now == null || prev[i] == null) { prev[i] = now; continue; }
                int sc = StateCount(inst[i].Buffer);
                for (int o = 0; o < span; o += 2)
                {
                    bool dword = o % 4 == 0;
                    int a = dword ? BitConverter.ToInt32(prev[i], o) : BitConverter.ToInt16(prev[i], o);
                    int c = dword ? BitConverter.ToInt32(now, o) : BitConverter.ToInt16(now, o);
                    if (a == c) continue;
                    bool counter = o == 0x150;
                    bool stateLike = a >= -1 && c >= -1 && a < sc && c < sc;
                    if (!counter && !stateLike) continue;
                    var k = (i, o);
                    if (!stats.TryGetValue(k, out var e)) stats[k] = e = (0, new HashSet<int> { a });
                    e.Values.Add(c); stats[k] = (e.Count + 1, e.Values);
                    if (lines++ < 10000)
                        log($"t={t:0.00}s inst#{i} {Name(inst[i].Buffer)} +{o:X3}{(dword ? "" : " s16")}: {a} -> {c}" +
                            (counter ? " (entry counter)" : c >= 0 ? $"  {State(inst[i].Buffer, c)}" : ""));
                }
                prev[i] = now;
            }
        }
        Console.Beep(660, 500);
        log($"\n== summary ({lines} changes)");
        foreach (var ((i, o), e) in stats.OrderBy(k => k.Key.I).ThenBy(k => k.Key.Off))
            log($"  inst#{i} {Name(inst[i].Buffer)} +{o:X3}: {e.Count} changes, values {string.Join(",", e.Values.OrderBy(x => x).Take(20))}{(e.Values.Count > 20 ? $" (+{e.Values.Count - 20})" : "")}");
        log("done");
    }
}
