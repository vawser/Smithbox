using System.Diagnostics;

// "states" mode: find memory slots holding a pointer to the start of a BHV state record, then watch them while the
// user plays. A slot that moves between states is a current-state pointer; each move is logged with the state's
// index, ID, kind and animation ID.
static class StateTrack
{
    record StateInfo(int Buffer, int Index, short Id, short Kind, int AnimIndex, int AnimId);

    public static void Run(Mem mem, List<(ulong Base, ulong Size)> regions, List<(ulong Addr, int Len, string Name, byte[] Orig)> found,
        Action<string> log, int seconds, Func<ulong, ulong> toStatic, Func<ulong, bool> isVtable)
    {
        // state record starts, from each buffer's own header
        var states = new Dictionary<ulong, StateInfo>();
        for (int b = 0; b < found.Count; b++)
        {
            var f = found[b].Orig;
            short ver = BitConverter.ToInt16(f, 0);
            int stateSize = ver >= 10 ? 0xA0 : 0x80, animSize = ver >= 7 ? 0x60 : ver == 6 ? 0x50 : ver >= 4 ? 0x40 : 0x30;
            int so = BitConverter.ToInt32(f, 0x20), sc = BitConverter.ToInt32(f, 0x24), ao = BitConverter.ToInt32(f, 0x28), ac = BitConverter.ToInt32(f, 0x2C);
            for (int i = 0; i < sc; i++)
            {
                int s = 0x20 + so + i * stateSize;
                short anim = BitConverter.ToInt16(f, s + 0x10);
                int animId = anim >= 0 && anim < ac ? BitConverter.ToInt32(f, 0x20 + ao + anim * animSize) : -1;
                states[found[b].Addr + (ulong)s] = new(b, i, BitConverter.ToInt16(f, s), BitConverter.ToInt16(f, s + 2), anim, animId);
            }
        }
        string Describe(ulong p) => states.TryGetValue(p, out var s)
            ? $"{found[s.Buffer].Name.Split(' ')[0]} state[{s.Index}] id={s.Id} kind={s.Kind} anim[{s.AnimIndex}]={s.AnimId}"
            : p == 0 ? "null" : $"other {p:X}";
        log($"\n== state mode: {states.Count} state records in {found.Count} buffers");

        // slots pointing at a state start, outside the buffers
        var inBuffer = found.Select(f => (f.Addr, End: f.Addr + (ulong)f.Len)).ToList();
        var slots = new List<ulong>();
        mem.Scan(regions, 0, (addr, buf, n) =>
        {
            for (int o = 0; o + 8 <= n; o += 8)
            {
                ulong v = BitConverter.ToUInt64(buf, o);
                if (!states.ContainsKey(v)) continue;
                ulong at = addr + (ulong)o;
                if (inBuffer.Any(r => at >= r.Addr && at < r.End)) continue;
                if (slots.Count < 50000) slots.Add(at);
            }
        });
        log($"{slots.Count} slots point at a state start");

        // the object owning each slot, for the vtable
        string Owner(ulong at)
        {
            var back = mem.Read(at - 0x400, 0x408);
            if (back == null) return "";
            for (int o = 0x400; o >= 0; o -= 8)
            {
                ulong q = BitConverter.ToUInt64(back, o);
                if (isVtable(q)) return $"obj {at - 0x400 + (ulong)o:X}+{0x400 - o:X} vtable {toStatic(q):X}";
            }
            return "no vtable";
        }

        var pages = slots.GroupBy(s => s & ~0xFFFUL).ToDictionary(g => g.Key, g => g.ToList());
        var last = new Dictionary<ulong, ulong>();
        foreach (var (page, list) in pages)
        {
            var b = mem.Read(page, 0x1000);
            foreach (var s in list) last[s] = b == null ? 0 : BitConverter.ToUInt64(b, (int)(s - page));
        }

        log($"\n== scans done. Switch to the game and press F10 to start watching {slots.Count} slots for {seconds}s (F9 = marker).");
        Console.Beep(880, 300);
        Native.GetAsyncKeyState(0x79);
        var wait = Stopwatch.StartNew();
        while ((Native.GetAsyncKeyState(0x79) & 1) == 0 && wait.Elapsed.TotalMinutes < 15) Thread.Sleep(50);
        foreach (var (page, list) in pages)
        {
            var b = mem.Read(page, 0x1000);
            foreach (var s in list) last[s] = b == null ? 0 : BitConverter.ToUInt64(b, (int)(s - page));
        }
        Console.Beep(1320, 200);
        Native.GetAsyncKeyState(0x78);

        var moves = new Dictionary<ulong, List<(double T, ulong To)>>();
        var start = new Dictionary<ulong, ulong>(last);
        var sw = Stopwatch.StartNew();
        int marker = 0, lines = 0;
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            Thread.Sleep(50);
            double t = sw.Elapsed.TotalSeconds;
            if ((Native.GetAsyncKeyState(0x78) & 1) != 0) log($"t={t:0.00}s MARK {++marker}");
            foreach (var (page, list) in pages)
            {
                var b = mem.Read(page, 0x1000);
                if (b == null) continue;
                foreach (var s in list)
                {
                    ulong v = BitConverter.ToUInt64(b, (int)(s - page));
                    if (v == last[s]) continue;
                    if (!moves.TryGetValue(s, out var m)) moves[s] = m = new();
                    m.Add((t, v));
                    if (lines++ < 8000) log($"t={t:0.00}s slot {s:X}: {Describe(last[s])} -> {Describe(v)}");
                    last[s] = v;
                }
            }
        }
        Console.Beep(660, 500);

        log($"\n== {moves.Count} slots moved ({lines} moves{(lines > 8000 ? ", first 8000 logged" : "")})");
        foreach (var (s, m) in moves.OrderByDescending(x => x.Value.Count).Take(60))
        {
            log($"-- slot {s:X} ({Owner(s)}): {m.Count} moves, started at {Describe(start[s])}");
            var visited = m.Select(x => x.To).Where(states.ContainsKey).GroupBy(x => x).OrderByDescending(g => g.Count()).Take(20);
            foreach (var g in visited) log($"     {g.Count(),4}x {Describe(g.Key)}");
        }
        log("done");
    }
}
