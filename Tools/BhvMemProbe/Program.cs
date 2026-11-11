using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

// BhvMemProbe [sampleSeconds]   (run from src/Smithbox.Data while armoredcore6.exe is running OFFLINE)
//
// Read-only: opens the game process with PROCESS_VM_READ | PROCESS_QUERY_INFORMATION and never writes to it.
//   1. finds every loaded .bhv buffer by matching headers against the game's own files, and compares the bytes
//   2. finds memory that points into those buffers, and the objects (vtables) holding those pointers
//   3. finds the owners of those objects
//   4. waits for F10 in game, then samples the objects for sampleSeconds (default 90) and logs integer fields that
//      change; press F9 in game
//      to drop a numbered marker into the log
// Output: BhvMemProbe.log in the working directory. Addresses in the exe are given as static VAs (base 0x140000000).

int sampleSeconds = args.Length > 0 ? int.Parse(args[0]) : 90;
using var log = new StreamWriter("BhvMemProbe.log", false, Encoding.UTF8) { AutoFlush = true };
void Log(string s) { Console.WriteLine(s); log.WriteLine(s); }

Log($"BhvMemProbe {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

// ---- 1. The game's .bhv files, keyed by their 0x40-byte header
var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "Claude", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var root = gamePaths["AC6"];
List<VirtualFileSystem> layers = [];
try { layers.Add(ArchiveBinderVirtualFileSystem.FromGameFolder(root, Game.AC6)); } catch (Exception e) { Log($"no archives: {e.Message}"); }
layers.Add(new RealVirtualFileSystem(root, false));
var fs = new CompundVirtualFileSystem(layers);
var dict = JsonDocument.Parse(File.ReadAllText(Path.Combine("Assets", "File Dictionaries", "AC6-File-Dictionary.json")));
var files = new Dictionary<string, List<(string Name, byte[] Bytes)>>();
var sizes = new HashSet<int>();
foreach (var p in dict.RootElement.GetProperty("Entries").EnumerateArray().Select(e => e.GetProperty("Path").GetString()).Where(p => p.EndsWith(".behbnd.dcx")).Distinct())
{
    if (!fs.FileExists(p)) continue;
    var bnd = BND4.Read(fs.ReadFile(p).Value);
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".bhv", StringComparison.OrdinalIgnoreCase)))
    {
        var bytes = f.Bytes.ToArray();
        if (bytes.Length <= 0x40) continue; // weapon.bhv is empty and matches too much
        var key = Convert.ToHexString(bytes, 0, 0x40);
        if (!files.TryGetValue(key, out var l)) files[key] = l = new();
        l.Add(($"{Path.GetFileName(p).Split('.')[0]}/{Path.GetFileName(f.Name)}", bytes));
        sizes.Add(bytes.Length);
    }
}
Log($"{files.Sum(f => f.Value.Count)} game .bhv files, {files.Count} distinct headers");

// ---- Process
var proc = Process.GetProcessesByName("armoredcore6").FirstOrDefault();
if (proc == null) { Log("armoredcore6.exe is not running."); return; }
var h = Native.OpenProcess(Native.PROCESS_VM_READ | Native.PROCESS_QUERY_INFORMATION, false, proc.Id);
if (h == IntPtr.Zero) { Log($"OpenProcess failed (error {Marshal.GetLastWin32Error()}). Is Easy Anti-Cheat running? Start the game offline without EAC."); return; }
var mem = new Mem(h);
ulong modBase = (ulong)proc.MainModule.BaseAddress, modSize = (ulong)proc.MainModule.ModuleMemorySize;
Log($"pid {proc.Id}, exe base {modBase:X}, size {modSize:X}");
var pe = mem.Read(modBase, 0x1000);
int lfanew = BitConverter.ToInt32(pe, 0x3C);
int nsec = BitConverter.ToUInt16(pe, lfanew + 6), optSize = BitConverter.ToUInt16(pe, lfanew + 20);
var text = new List<(ulong, ulong)>(); (ulong, ulong) rdata = (0, 0);
for (int i = 0; i < nsec; i++)
{
    int o = lfanew + 24 + optSize + i * 40;
    var name = Encoding.ASCII.GetString(pe, o, 8).TrimEnd('\0');
    ulong va = modBase + BitConverter.ToUInt32(pe, o + 12), vs = BitConverter.ToUInt32(pe, o + 8);
    if (name == ".text") text.Add((va, va + vs));
    if (name == ".rdata") rdata = (va, va + vs);
}
ulong Static(ulong a) => a >= modBase && a < modBase + modSize ? a - modBase + 0x140000000UL : 0;
bool InText(ulong a) => text.Any(t => a >= t.Item1 && a < t.Item2);
var vtCache = new Dictionary<ulong, bool>();
bool IsVtable(ulong q)
{
    if (q < rdata.Item1 || q >= rdata.Item2 || (q & 7) != 0) return false;
    if (vtCache.TryGetValue(q, out var r)) return r;
    var b = mem.Read(q, 8);
    return vtCache[q] = b != null && InText(BitConverter.ToUInt64(b, 0));
}

var regions = mem.Regions();
Log($"{regions.Count} readable regions, {regions.Sum(r => (double)r.Size) / (1 << 30):0.00} GiB");

// ---- 2. Loaded .bhv buffers
var found = new List<(ulong Addr, int Len, string Name, byte[] Orig)>();
var seen = new HashSet<ulong>();
mem.Scan(regions, 0x40, (addr, buf, n) =>
{
    for (int o = 0; o + 0x40 <= n; o += 8)
    {
        if (buf[o + 1] != 0 || buf[o + 2] != 3 || buf[o + 3] != 0) continue;
        byte v = buf[o]; if (v != 5 && v != 6 && v != 10 && v != 12) continue;
        if (!sizes.Contains(BitConverter.ToInt32(buf, o + 4))) continue;
        var key = Convert.ToHexString(buf, o, 0x40);
        if (!files.TryGetValue(key, out var cands) || !seen.Add(addr + (ulong)o)) continue;
        found.Add((addr + (ulong)o, cands[0].Bytes.Length, string.Join(" | ", cands.Select(c => c.Name).Take(3)) + (cands.Count > 3 ? $" (+{cands.Count - 3})" : ""), cands[0].Bytes));
    }
});
Log($"\n== {found.Count} loaded .bhv buffers");
foreach (var f in found.OrderBy(f => f.Addr))
{
    var live = mem.Read(f.Addr, f.Len);
    string cmp;
    if (live == null) cmp = "unreadable";
    else
    {
        var diffs = Enumerable.Range(0, f.Len).Where(i => live[i] != f.Orig[i]).ToList();
        cmp = diffs.Count == 0 ? "identical to file" : $"{diffs.Count} bytes differ, first at: {string.Join(", ", diffs.Take(10).Select(d => $"{d:X}({Label(f.Orig, d)})"))}";
    }
    Log($"  {f.Addr:X} len {f.Len:X} {f.Name}: {cmp}");
}
if (found.Count == 0) { Log("No .bhv data found. Load into a mission or the test area with your AC, then run again."); return; }

// ---- 3. Pointers into the buffers
var ranges = found.Select((f, i) => (Start: f.Addr, End: f.Addr + (ulong)f.Len, I: i)).OrderBy(r => r.Start).ToArray();
ulong lo = ranges[0].Start, hi = ranges.Max(r => r.End);
int FindRange(ulong v)
{
    int a = 0, b = ranges.Length - 1;
    while (a <= b) { int m = (a + b) / 2; if (v < ranges[m].Start) b = m - 1; else if (v >= ranges[m].End) a = m + 1; else return ranges[m].I; }
    return -1;
}
var hits = new List<(ulong At, int Bhv, int Off)>();
mem.Scan(regions, 0, (addr, buf, n) =>
{
    for (int o = 0; o + 8 <= n; o += 8)
    {
        ulong v = BitConverter.ToUInt64(buf, o);
        if (v < lo || v >= hi) continue;
        int r = FindRange(v); if (r < 0) continue;
        ulong at = addr + (ulong)o;
        if (FindRange(at) >= 0) continue;
        if (hits.Count < 20000) hits.Add((at, r, (int)(v - found[r].Addr)));
    }
});
Log($"\n== {hits.Count} pointers into the buffers");

// group hits by owning object (nearest vtable within 0x400 bytes before)
var objects = new Dictionary<ulong, (ulong Vt, List<(int Field, int Bhv, int Off)> Fields)>();
var loose = new List<(ulong At, int Bhv, int Off)>();
foreach (var hit in hits)
{
    var back = mem.Read(hit.At - 0x400, 0x408);
    ulong obj = 0, vt = 0;
    if (back != null)
        for (int o = 0x400; o >= 0; o -= 8)
        {
            ulong q = BitConverter.ToUInt64(back, o);
            if (IsVtable(q)) { obj = hit.At - 0x400 + (ulong)o; vt = q; break; }
        }
    if (obj == 0) { loose.Add(hit); continue; }
    if (!objects.TryGetValue(obj, out var e)) objects[obj] = e = (vt, new());
    e.Fields.Add(((int)(hit.At - obj), hit.Bhv, hit.Off));
}
var topObjects = objects.OrderByDescending(o => o.Value.Fields.Count).Take(80).ToList();
Log($"{objects.Count} objects with a vtable, {loose.Count} pointers without one");
int idx = 0;
var objIndex = new Dictionary<ulong, int>();
foreach (var (obj, e) in topObjects)
{
    objIndex[obj] = idx;
    Log($"\n-- obj#{idx++} {obj:X} vtable {Static(e.Vt):X} ({e.Fields.Count} pointers)");
    var vfn = mem.Read(e.Vt, 12 * 8);
    if (vfn != null) Log($"   vfuncs: {string.Join(" ", Enumerable.Range(0, 12).Select(i => BitConverter.ToUInt64(vfn, i * 8)).TakeWhile(InText).Select(f => Static(f).ToString("X")))}");
    foreach (var (field, b, off) in e.Fields.OrderBy(x => x.Field).Take(24))
        Log($"   +{field:X3} -> {found[b].Name} +{off:X} ({Label(found[b].Orig, off)})");
    DumpObject(obj, 0x100);
}
foreach (var l in loose.Take(40))
    Log($"   no vtable: {l.At:X} -> {found[l.Bhv].Name} +{l.Off:X} ({Label(found[l.Bhv].Orig, l.Off)})");

// ---- 4. Owners of those objects
var objSet = topObjects.Select(o => o.Key).ToHashSet();
var owners = new Dictionary<ulong, (ulong Vt, List<(int Field, int Obj)> Refs)>();
var ownerHits = new List<(ulong At, ulong Obj)>();
mem.Scan(regions, 0, (addr, buf, n) =>
{
    for (int o = 0; o + 8 <= n; o += 8)
    {
        ulong v = BitConverter.ToUInt64(buf, o);
        if (objSet.Contains(v) && ownerHits.Count < 5000) ownerHits.Add((addr + (ulong)o, v));
    }
});
foreach (var (at, obj) in ownerHits)
{
    var back = mem.Read(at - 0x400, 0x408);
    if (back == null) continue;
    for (int o = 0x400; o >= 0; o -= 8)
    {
        ulong q = BitConverter.ToUInt64(back, o);
        if (!IsVtable(q)) continue;
        ulong owner = at - 0x400 + (ulong)o;
        if (!owners.TryGetValue(owner, out var e)) owners[owner] = e = (q, new());
        e.Refs.Add(((int)(at - owner), objIndex[obj]));
        break;
    }
}
Log($"\n== {owners.Count} owner objects");
var topOwners = owners.Where(o => !objSet.Contains(o.Key)).OrderByDescending(o => o.Value.Refs.Count).Take(30).ToList();
foreach (var (owner, e) in topOwners)
{
    var vfn = mem.Read(e.Vt, 12 * 8);
    Log($"-- owner {owner:X} vtable {Static(e.Vt):X}: {string.Join(", ", e.Refs.Take(12).Select(r => $"+{r.Field:X3}->obj#{r.Obj}"))}");
    if (vfn != null) Log($"   vfuncs: {string.Join(" ", Enumerable.Range(0, 12).Select(i => BitConverter.ToUInt64(vfn, i * 8)).TakeWhile(InText).Select(f => Static(f).ToString("X")))}");
}

// ---- 5. Sample objects while playing
var watch = topObjects.Take(40).Select(o => (Addr: o.Key, Name: $"obj#{objIndex[o.Key]}"))
    .Concat(topOwners.Take(20).Select((o, i) => (Addr: o.Key, Name: $"owner{i}"))).ToList();
const int span = 0x400;
var prev = watch.Select(w => mem.Read(w.Addr, span)).ToList();
var changes = new Dictionary<(int W, int Off), (int Count, HashSet<int> Values)>();
int lines = 0, marker = 0;
Log($"\n== scans done. Switch to the game and press F10 to start sampling {watch.Count} objects for {sampleSeconds}s (F9 = marker).");
Console.Beep(880, 300);
Native.GetAsyncKeyState(0x79);
var wait = Stopwatch.StartNew();
while ((Native.GetAsyncKeyState(0x79) & 1) == 0 && wait.Elapsed.TotalMinutes < 15) Thread.Sleep(50);
prev = watch.Select(w => mem.Read(w.Addr, span)).ToList();
Console.Beep(1320, 200);
Native.GetAsyncKeyState(0x78);
var sw = Stopwatch.StartNew();
while (sw.Elapsed.TotalSeconds < sampleSeconds)
{
    Thread.Sleep(100);
    if ((Native.GetAsyncKeyState(0x78) & 1) != 0) Log($"t={sw.Elapsed.TotalSeconds:0.0}s MARK {++marker}");
    for (int w = 0; w < watch.Count; w++)
    {
        var now = mem.Read(watch[w].Addr, span);
        if (now == null || prev[w] == null) { prev[w] = now; continue; }
        for (int o = 0; o < span; o += 4)
        {
            int a = BitConverter.ToInt32(prev[w], o), b = BitConverter.ToInt32(now, o);
            if (a == b || a < -1 || b < -1 || a > 100000 || b > 100000) continue;
            var k = (w, o);
            if (!changes.TryGetValue(k, out var c)) changes[k] = c = (0, new HashSet<int> { a });
            c.Values.Add(b);
            changes[k] = (c.Count + 1, c.Values);
            if (lines++ < 6000) log.WriteLine($"t={sw.Elapsed.TotalSeconds:0.0}s {watch[w].Name} +{o:X3}: {a} -> {b}");
        }
        prev[w] = now;
    }
}
Console.Beep(660, 500);
Log($"\n== change summary ({lines} int changes{(lines > 6000 ? ", first 6000 logged" : "")})");
foreach (var ((w, o), c) in changes.OrderBy(k => k.Key.W).ThenBy(k => k.Key.Off))
    Log($"  {watch[w].Name} +{o:X3}: {c.Count} changes, values {string.Join(",", c.Values.OrderBy(x => x).Take(16))}{(c.Values.Count > 16 ? $" (+{c.Values.Count - 16})" : "")}");
Log("done");

void DumpObject(ulong obj, int len)
{
    var b = mem.Read(obj, len);
    if (b == null) return;
    for (int o = 0; o < len; o += 8)
    {
        ulong q = BitConverter.ToUInt64(b, o);
        int r = FindRange(q);
        string note = r >= 0 ? $"-> {found[r].Name} +{q - found[r].Addr:X}" : Static(q) != 0 ? $"exe {Static(q):X}" : "";
        Log($"     +{o:X3} {q:X16} {note}");
    }
}

static string Label(byte[] f, int off)
{
    int I(int a) => BitConverter.ToInt32(f, a);
    short ver = BitConverter.ToInt16(f, 0);
    int stateSize = ver >= 10 ? 0xA0 : 0x80, animSize = ver switch { 5 => 0x40, 6 => 0x50, _ => 0x60 };
    if (off < 0x40) return "header";
    int rel = off - 0x20;
    int so = I(0x20), sc = I(0x24), bo = I(0x28), bc = I(0x2C), co = I(0x30), cc = BitConverter.ToInt16(f, 0x34), cs = BitConverter.ToInt16(f, 0x36), dof = I(0x38), dc = I(0x3C);
    if (rel < so) return $"preamble+{rel - (0x40 - 0x20):X}";
    if (rel < so + sc * stateSize) return $"state[{(rel - so) / stateSize}]+{(rel - so) % stateSize:X}";
    if (rel < bo) return "graph (state data/transitions/conditions)";
    if (rel < bo + bc * animSize) return $"anim[{(rel - bo) / animSize}]+{(rel - bo) % animSize:X}";
    if (cs > 0 && rel >= co && rel < co + cc * cs) return $"C[{(rel - co) / cs}]+{(rel - co) % cs:X}";
    if (rel >= dof && rel < dof + dc * 0x20) return $"D[{(rel - dof) / 0x20}]+{(rel - dof) % 0x20:X}";
    return "DA/strings";
}

sealed class Mem(IntPtr h)
{
    public byte[] Read(ulong addr, int len)
    {
        var buf = new byte[len];
        return Native.ReadProcessMemory(h, (IntPtr)(long)addr, buf, (IntPtr)len, out var got) && (long)got == len ? buf : null;
    }

    public List<(ulong Base, ulong Size)> Regions()
    {
        var list = new List<(ulong, ulong)>();
        ulong a = 0x10000;
        while (a < 0x7FFFFFFFFFFF)
        {
            if (Native.VirtualQueryEx(h, (IntPtr)(long)a, out var mbi, (IntPtr)Marshal.SizeOf<Native.MBI>()) == IntPtr.Zero) break;
            bool readable = mbi.State == 0x1000 && (mbi.Protect & 0x101) == 0 && (mbi.Protect & 0xEE) != 0;
            if (readable) list.Add((mbi.BaseAddress, mbi.RegionSize));
            a = mbi.BaseAddress + mbi.RegionSize;
            if (mbi.RegionSize == 0) break;
        }
        return list;
    }

    // Calls visit(chunkAddress, buffer, usableLength) for every readable chunk; chunks overlap by `overlap` bytes.
    public void Scan(List<(ulong Base, ulong Size)> regions, int overlap, Action<ulong, byte[], int> visit)
    {
        const int chunk = 16 << 20;
        var buf = new byte[chunk + overlap];
        foreach (var (b, s) in regions)
        {
            for (ulong off = 0; off < s; off += chunk)
            {
                int len = (int)Math.Min((ulong)chunk + (ulong)overlap, s - off);
                if (!Native.ReadProcessMemory(h, (IntPtr)(long)(b + off), buf, (IntPtr)len, out var got)) continue;
                visit(b + off, buf, (int)(long)got);
            }
        }
    }
}

static class Native
{
    public const int PROCESS_VM_READ = 0x10, PROCESS_QUERY_INFORMATION = 0x400;

    [StructLayout(LayoutKind.Sequential)]
    public struct MBI
    {
        public ulong BaseAddress, AllocationBase;
        public uint AllocationProtect, Align1;
        public ulong RegionSize;
        public uint State, Protect, Type, Align2;
    }

    [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr OpenProcess(int access, bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] public static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, IntPtr size, out IntPtr read);
    [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr VirtualQueryEx(IntPtr h, IntPtr addr, out MBI info, IntPtr len);
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
}
