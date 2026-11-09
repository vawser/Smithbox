using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Buffers.Binary;

// FXR round trip and survey (Documentation/Claude/FXR Format Plan.md).
//   FxrRoundTrip <outDir> [GAME...] [--new] [--edit]     default games: DS3 SDT ER AC6 NR
//   FxrRoundTrip --dump <file.fxr>                       records in file order with offsets
//   FxrRoundTrip --fields <file.fxr>                     field pool blocks by offset, with owners
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.
//
// For every .fxr inside every /sfx/*.ffxbnd(.dcx):
//   1. FXR: read -> write(DCX none) -> byte compare (--edit adds an edit-then-reread check)
//   2. Raw walker (skipped with --new): format facts from Phase 0
// The Phase 0 FXR3 comparison was removed when FXR3 moved to .archive.

if (args[0] == "--dump")
{
    DumpLayout(File.ReadAllBytes(args[1]));
    return;
}
if (args[0] == "--fields")
{
    DumpFieldOwners(File.ReadAllBytes(args[1]));
    return;
}

var outDir = Path.GetFullPath(args[0]);
var dataDir = Directory.GetCurrentDirectory();
Directory.CreateDirectory(outDir);

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

// --new: only the FXR round trip (skips the raw survey)
bool newOnly = args.Contains("--new");
// --edit: also run the edit check (EditCheck) on every byte-perfect file
bool editCheck = args.Contains("--edit");
var games = args.Skip(1).Where(a => !a.StartsWith("--")).ToList();
if (games.Count == 0) games = ["DS3", "SDT", "ER", "AC6", "NR"];

using var log = new StreamWriter(Path.Combine(outDir, "report.txt"));
void Log(string s) { Console.WriteLine(s); log.WriteLine(s); log.Flush(); }

foreach (var gameName in games)
{
    if (!gamePaths.TryGetValue(gameName, out var root) || !Directory.Exists(root))
    {
        Log($"== {gameName}: game folder missing, skipped");
        continue;
    }

    var game = Enum.Parse<Game>(gameName);
    VirtualFileSystem fs;
    // The VFS loads its dictionaries from the working directory (src/Smithbox.Data); Oodle looks for its
    // DLL in the working directory at decompress time, so switch to the tool's own folder after building.
    Directory.SetCurrentDirectory(dataDir);
    try { fs = BuildFS(game, root); }
    catch (Exception e) { Log($"== {gameName}: VFS failed: {e.Message}"); continue; }
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);

    // Oodle is not shipped with Smithbox; use the game's own copy, as OodleSetup does for the editor
    foreach (var dll in Directory.GetFiles(root, "oo2core_*_win64.dll"))
    {
        var target = Path.Combine(AppContext.BaseDirectory, Path.GetFileName(dll));
        if (!File.Exists(target)) File.Copy(dll, target);
    }

    var bnds = Find(fs, "/sfx", @"(?i)\.ffxbnd(\.dcx)?$")
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var s = new Survey(game);
    var gameOut = Path.Combine(outDir, gameName);
    var seenFxr = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var path in bnds)
    {
        BND4 bnd;
        try { bnd = BND4.Read(fs.ReadFile(path).Value); }
        catch (Exception e) { s.Fail($"{path}: BND4 read failed: {e.Message}"); continue; }
        s.Binders++;

        foreach (var file in bnd.Files)
        {
            if (!file.Name.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase))
                continue;

            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var name = $"{Path.GetFileName(path)}|{Path.GetFileName(file.Name)}";

            // Same FXR id can appear in several binders; survey every copy but count duplicates
            if (!seenFxr.Add(Path.GetFileName(file.Name))) s.Duplicates++;
            s.Files++;

            if (!newOnly)
            {
                try { new RawWalker(bytes, s, name).Walk(); }
                catch (Exception e) { s.Fail($"{name}: raw walk failed: {e.Message}"); }
            }

            CheckFxr(name, bytes, s, gameOut);
        }
    }

    s.Report(Log, bnds.Count);
}

void CheckFxr(string name, byte[] plain, Survey s, string gameOut)
{
    FXR fxr;
    try { fxr = FXR.Read(plain); }
    catch (Exception e) { s.FxrFail++; s.FxrErrors.Add($"{name}: read: {e.Message}"); return; }

    s.Count(s.FxrStyles, $"zero empty offsets: {fxr.ZeroEmptyOffsets}");

    byte[] written;
    try { written = fxr.Write(DCX.Type.None); }
    catch (Exception e) { s.FxrFail++; s.FxrErrors.Add($"{name}: write: {e.Message}"); return; }

    if (written.AsSpan().SequenceEqual(plain))
    {
        s.FxrOk++;
        if (editCheck) EditCheck(name, plain, s);
        return;
    }

    int at = 0;
    while (at < plain.Length && at < written.Length && plain[at] == written[at]) at++;
    var where = RawWalker.Describe(plain, at);
    s.Count(s.FxrDiffs, where);
    if (s.FxrErrors.Count < 40)
        s.FxrErrors.Add($"{name}: differs ({plain.Length} -> {written.Length}, first diff 0x{at:X} {where}, " +
            $"orig {Hex(plain, at)} new {Hex(written, at)})");

    if (s.FxrDiffs[where] <= 2 && s.FxrDumped++ < 30)
    {
        var dir = Path.Combine(gameOut, "new");
        Directory.CreateDirectory(dir);
        var stem = name.Replace('|', '_');
        File.WriteAllBytes(Path.Combine(dir, stem + ".orig"), plain);
        File.WriteAllBytes(Path.Combine(dir, stem + ".written"), written);
    }
}

// Prints records in file order with their offsets, to compare layout against the writer's order
static void DumpLayout(byte[] b)
{
    int I32(long o) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan((int)o));
    long I64(long o) => BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)o));
    string[] names = ["States", "Conditions", "Nodes", "Configs", "Actions", "Properties", "Modifiers", "ModProps", "Section10s", "Fields"];
    for (int i = 0; i < names.Length; i++) Console.WriteLine($"{names[i],-11} off 0x{I32(0x18 + i * 8):X5} count {I32(0x1C + i * 8)}");

    long nodes = I32(0x28); int nodeCount = I32(0x2C);
    Console.WriteLine("-- nodes");
    for (int i = 0; i < nodeCount; i++)
    {
        long n = nodes + i * 0x30L;
        Console.WriteLine($"0x{n:X5} type {BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)n))} c{I32(n + 8)} a{I32(n + 12)} n{I32(n + 16)} +4={I32(n + 4)} +14={I32(n + 20)}  configs 0x{I64(n + 0x18):X5} actions 0x{I64(n + 0x20):X5} children 0x{I64(n + 0x28):X5}");
    }
    long configs = I32(0x30); int configCount = I32(0x34);
    Console.WriteLine("-- configs");
    for (int i = 0; i < configCount; i++)
    {
        long c = configs + i * 0x20L;
        Console.WriteLine($"0x{c:X5} type {BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)c))} +4={I32(c + 4)} +8={I32(c + 8)} a{I32(c + 12)} +10={I32(c + 16)} +14={I32(c + 20)} actions 0x{I64(c + 0x18):X5}");
    }
    long actions = I32(0x38); int actionCount = I32(0x3C);
    Console.WriteLine("-- actions (type f1/f2/p1/p2/s10, fields, props, s10)");
    for (int i = 0; i < actionCount && i < 400; i++)
    {
        long a = actions + i * 0x40L;
        Console.WriteLine($"0x{a:X5} {BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)a)),5} u{b[a + 2]}{b[a + 3]} {I32(a + 4)}  {I32(a + 8)}/{I32(a + 20)}/{I32(a + 16)}/{I32(a + 28)}/{I32(a + 12)}  f 0x{I64(a + 0x20):X5} p 0x{I64(a + 0x30):X5} s 0x{I64(a + 0x28):X5}");
    }
}

// Lists every field block (offset, words, owner) sorted by offset, marking blocks that overlap earlier ones
static void DumpFieldOwners(byte[] b)
{
    int I32(long o) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan((int)o));
    short I16(long o) => BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)o));
    long I64(long o) => BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)o));
    var blocks = new List<(long off, int words, string owner)>();

    long conds = I32(0x20); int condCount = I32(0x24);
    for (int i = 0; i < condCount; i++)
        for (int side = 0; side < 2; side++)
        {
            long o = conds + i * 0x60L + 0x10 + side * 0x28;
            if (I32(o + 8) != 0) blocks.Add((I64(o + 0x10), 1, $"cond{i} {(side == 0 ? "L" : "R")} type {I16(o)}"));
        }

    void Prop(long p, string owner, bool modifiable)
    {
        ushort a = (ushort)I16(p);
        string label = $"{owner} func {(a >> 4) & 0xF} vt {a & 3}";
        int fc = I32(p + 8);
        if (fc > 0) blocks.Add((I64(p + 0x10), fc, label));
        if (!modifiable) return;
        long mods = I64(p + 0x18); int mc = I32(p + 0x20);
        for (int m = 0; m < mc; m++)
        {
            long mo = mods + m * 0x20L;
            ushort ma = (ushort)I16(mo);
            int mtype = ((ma >> 12) & 3) << 4 | (ma >> 4) & 0xF;
            int mf = I32(mo + 8), mp = I32(mo + 12);
            if (mf > 0) blocks.Add((I64(mo + 0x10), mf, $"{label} / mod{m} type {mtype}"));
            for (int q = 0; q < mp; q++) Prop(I64(mo + 0x18) + q * 0x18L, $"{label} / mod{m} prop{q}", false);
        }
    }

    long actions = I32(0x38); int actionCount = I32(0x3C);
    for (int i = 0; i < actionCount; i++)
    {
        long a = actions + i * 0x40L;
        short type = I16(a);
        int f1 = I32(a + 8), s10 = I32(a + 12), p1 = I32(a + 16), f2 = I32(a + 20), p2 = I32(a + 28);
        string owner = $"A{i}@0x{a:X} type {type}";
        if (f1 + f2 > 0) blocks.Add((I64(a + 0x20), f1 + f2, $"{owner} fields"));
        for (int p = 0; p < p1 + p2; p++) Prop(I64(a + 0x30) + p * 0x28L, $"{owner} p{p}", true);
        for (int s = 0; s < s10; s++)
        {
            long e = I64(a + 0x28) + s * 0x10L;
            if (I32(e + 8) > 0) blocks.Add((I64(e), I32(e + 8), $"{owner} s10[{s}]"));
        }
    }

    long maxEnd = 0;
    foreach (var (off, words, owner) in blocks.OrderBy(x => x.off).ThenByDescending(x => x.words))
    {
        long end = off + words * 4L;
        string mark = off < maxEnd ? "  <== OVERLAPS" : "";
        Console.WriteLine($"0x{off:X5}-0x{end:X5} {words,4}w {owner}{mark}  [{Convert.ToHexString(b, (int)off, Math.Min(words, 6) * 4)}]");
        maxEnd = Math.Max(maxEnd, end);
    }
    Console.WriteLine($"field pool 0x{I32(0x60):X} words {I32(0x64)}, blocks end 0x{maxEnd:X}");
}

// Edits every Constant property (each gets a distinct value), writes, re-reads, and checks each
// property reads back its own value: shared blocks, aliases and reserved words must not leak edits.
void EditCheck(string name, byte[] plain, Survey s)
{
    try
    {
        var fxr = FXR.Read(plain);
        // Strip the modifiers of every other top-level property first (breaks modifier sharing)
        var top = AllProperties(fxr).ToList();
        for (int i = 0; i < top.Count; i += 2) top[i].Modifiers.Clear();

        var props = AllProperties(fxr).Where(p => p.Function == FxrPropertyFunction.Constant && p.Fields.Count > 0).ToList();
        for (int i = 0; i < props.Count; i++)
            props[i].Fields[0] = BitConverter.SingleToUInt32Bits(1000f + i);
        var all = AllProperties(fxr).ToList();

        var back = FXR.Read(fxr.Write(DCX.Type.None));
        var props2 = AllProperties(back).Where(p => p.Function == FxrPropertyFunction.Constant && p.Fields.Count > 0).ToList();
        var all2 = AllProperties(back).ToList();
        bool ok = props2.Count == props.Count && all2.Count == all.Count;
        for (int i = 0; ok && i < props2.Count; i++)
            ok = props2[i].Fields[0] == BitConverter.SingleToUInt32Bits(1000f + i);
        for (int i = 0; ok && i < all2.Count; i++)
            ok = all2[i].Modifiers.Count == all[i].Modifiers.Count;
        if (ok) s.EditOk++;
        else { s.EditFail++; if (s.EditErrors.Count < 10) s.EditErrors.Add($"{name}: edited values did not read back"); }
    }
    catch (Exception e) { s.EditFail++; if (s.EditErrors.Count < 10) s.EditErrors.Add($"{name}: {e.Message}"); }
}

static IEnumerable<FxrProperty> AllProperties(FXR fxr)
{
    var stack = new Stack<FxrNode>();
    stack.Push(fxr.Root);
    while (stack.Count > 0)
    {
        var node = stack.Pop();
        var actions = node.Actions.Concat(node.Configs.SelectMany(c => c.Actions));
        foreach (var a in actions)
            foreach (var p in a.Properties1.Concat(a.Properties2))
            {
                yield return p;
                foreach (var m in p.Modifiers)
                    foreach (var mp in m.Properties) yield return mp;
            }
        for (int i = node.Children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]);
    }
}

static string Hex(byte[] b, int at) => at + 8 <= b.Length ? Convert.ToHexString(b, at, 8) : "<end>";

static IEnumerable<string> Find(VirtualFileSystem fs, string dir, string pattern)
{
    try { return fs.GetFileNamesMatchingRecursive(dir, pattern).Select(p => p.Replace('\\', '/')).ToList(); }
    catch { return []; }
}

static VirtualFileSystem BuildFS(Game game, string root)
{
    var real = new RealVirtualFileSystem(root, false);
    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}

// Walks the FXR structure straight from bytes, following the layout in the plan (section 2).
class RawWalker
{
    static readonly string[] SectionNames =
        ["States", "Conditions", "Nodes", "Configs", "Actions", "Properties", "Modifiers", "ModProps", "Section10s", "Fields"];

    readonly byte[] b;
    readonly Survey s;
    readonly string name;
    long memOffset;
    int version;

    // Recomputed header lists (plan 2.1)
    readonly SortedSet<int> sfx = new(), ext1 = new(), ext2 = new();
    // Offset sharing detection
    readonly HashSet<long> seenProps = new(), seenFieldBlocks = new(), seenActions = new();

    public RawWalker(byte[] bytes, Survey survey, string fileName) { b = bytes; s = survey; name = fileName; }

    int I32(long o) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan((int)o));
    short I16(long o) => BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)o));
    ushort U16(long o) => BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan((int)o));
    long Off(long o) => BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)o)) is var v && v == 0 ? 0 : v - memOffset;
    float F32(long o) => BitConverter.Int32BitsToSingle(I32(o));

    static readonly Dictionary<string, int> RecordSizes = new()
    {
        ["StateList"] = 0x10, ["States"] = 0x10, ["Conditions"] = 0x60, ["Nodes"] = 0x30, ["Configs"] = 0x20,
        ["Actions"] = 0x40, ["Properties"] = 0x28, ["Modifiers"] = 0x20, ["ModProps"] = 0x18, ["Section10s"] = 0x10,
    };

    /// <summary>Section name plus the byte offset inside its record, e.g. "Actions +0x20".</summary>
    public static string Describe(byte[] bytes, int at)
    {
        var section = SectionAt(bytes, at);
        if (!RecordSizes.TryGetValue(section, out var size)) return section;
        int start = section == "StateList" ? BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x10))
            : BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x18 + Array.IndexOf(SectionNames, section) * 8));
        return $"{section} +0x{(at - start) % size:X2}";
    }

    public static string SectionAt(byte[] bytes, int at)
    {
        if (bytes.Length < 0x70) return "header";
        if (at < 0x70) return "header";
        var starts = new List<(string, int)>();
        starts.Add(("StateList", BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x10))));
        for (int i = 0; i < SectionNames.Length; i++)
            starts.Add((SectionNames[i], BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x18 + i * 8))));
        if (BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(6)) == 5)
        {
            if (at < 0x90) return "header v5";
            starts.Add(("SfxRefs", BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x70))));
            starts.Add(("ExtVal1", BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x78))));
            starts.Add(("ExtVal2", BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x80))));
        }
        var best = starts.Where(x => x.Item2 > 0 && x.Item2 <= at).OrderByDescending(x => x.Item2).FirstOrDefault();
        return best.Item1 ?? "header";
    }

    public void Walk()
    {
        if (I32(0) != 0x00525846) throw new Exception("bad magic");
        var minor = I16(4);
        version = I16(6);
        s.Count(s.Headers, $"minor {minor} version {version}");
        if (I32(8) != 1) s.Count(s.Oddities, "header +0x08 != 1");
        if (I32(0x14) != 1) s.Count(s.Oddities, "state machine count != 1");
        if (I32(0x68) != 1 || I32(0x6C) != 0) s.Count(s.Oddities, "header +0x68/+0x6C != 1/0");

        // Declared section offsets in order (check they ascend, plan 2.5)
        var offs = new List<int> { I32(0x10) };
        for (int i = 0; i < SectionNames.Length; i++) offs.Add(I32(0x18 + i * 8));
        for (int i = 1; i < offs.Count; i++)
            if (offs[i] < offs[i - 1]) { s.Count(s.Oddities, "section offsets not ascending"); break; }
        if (offs.Any(o => o % 16 != 0)) s.Count(s.Oddities, "section offset not 16-aligned");

        // State list
        long sl = I32(0x10);
        int statesOffset = I32(0x18);
        if (I32(sl) != 0) s.Count(s.Oddities, "state list +0 != 0");
        int stateCount = I32(sl + 4);
        long statesPtr = BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)sl + 8));
        memOffset = statesPtr - statesOffset;
        if (memOffset != 0) s.Count(s.Oddities, "memOffset != 0");
        if (stateCount != I32(0x1C)) s.Count(s.Oddities, "state list count != header state count");

        for (int i = 0; i < stateCount; i++)
        {
            long st = statesOffset + i * 0x10L;
            int cc = I32(st + 4);
            long co = Off(st + 8);
            for (int c = 0; c < cc; c++) Condition(co + c * 0x60L);
        }

        Node(I32(0x28), 0);

        if (version == 5)
        {
            var hSfx = b.Length >= 0x90 ? Ints(I32(0x70), I32(0x74)) : [];
            var hE1 = Ints(I32(0x78), I32(0x7C));
            var hE2 = Ints(I32(0x80), I32(0x84));
            Compare("sfx refs", hSfx, sfx);
            Compare("ext values 1", hE1, ext1);
            Compare("ext values 2", hE2, ext2);
            if (I32(0x88) != 0 || I32(0x8C) != 0)
                s.Count(s.Oddities, $"unk empty list offset/count = {I32(0x88)}/{I32(0x8C)}");
        }
    }

    int[] Ints(int off, int count) => Enumerable.Range(0, count).Select(i => I32(off + i * 4)).ToArray();

    void Compare(string what, int[] header, SortedSet<int> recomputed)
    {
        bool sorted = header.SequenceEqual(header.Order());
        bool sameSet = header.ToHashSet().SetEquals(recomputed);
        bool exact = header.SequenceEqual(recomputed);
        s.Count(s.RefLists, $"{what}: {(exact ? "exact match" : sameSet ? (sorted ? "same set, dupes" : "same set, unsorted") : "DIFFERENT set")}");
        if (!sameSet && s.RefDiffExamples.Count < 15)
            s.RefDiffExamples.Add($"{name} {what}: header [{string.Join(",", header.Take(12))}] recomputed [{string.Join(",", recomputed.Take(12))}]");
    }

    void Condition(long c)
    {
        short bits = I16(c);
        s.Count(s.Conditions, $"op {bits & 3} unk1 {(bits >> 2) & 3}{((bits & ~0xF) != 0 ? " +highbits" : "")}");
        for (int side = 0; side < 2; side++)
        {
            long o = c + 0x10 + side * 0x28L;
            short type = I16(o);
            int hasValue = I32(o + 8);
            long valOff = Off(o + 0x10);
            s.Count(s.Conditions, $"operand {type} hasValue {hasValue}");
            if (hasValue != 0 && type == -3) ext1.Add(I32(valOff));
            if (hasValue != 0 && type is -2 or -1) s.Count(s.Oddities, "condition operand -1/-2 with stray value");
        }
    }

    void Node(long n, int depth)
    {
        short type = I16(n);
        int configCount = I32(n + 8), actionCount = I32(n + 12), childCount = I32(n + 16);
        long configs = Off(n + 0x18), actions = Off(n + 0x20), children = Off(n + 0x28);
        s.Count(s.Nodes, $"node {type} configs {configCount} actions {actionCount}");
        s.MaxDepth = Math.Max(s.MaxDepth, depth);

        for (int i = 0; i < actionCount; i++)
        {
            var a = actions + i * 0x40L;
            Action(a, $"node {type}");
            if (type == 2001 && I16(a) == 132)
            {
                long f = Off(a + 0x20);
                if (f != 0) sfx.Add(I32(f));
            }
        }
        for (int i = 0; i < configCount; i++)
        {
            long cf = configs + i * 0x20L;
            short ctype = I16(cf);
            int ac = I32(cf + 12);
            s.Count(s.Configs, $"config {ctype} actions {ac}");
            long ao = Off(cf + 0x18);
            for (int j = 0; j < ac; j++) Action(ao + j * 0x40L, $"config {ctype}");
        }
        for (int i = 0; i < childCount; i++) Node(children + i * 0x30L, depth + 1);
    }

    void Action(long a, string owner)
    {
        if (!seenActions.Add(a)) s.Count(s.Oddities, "action record shared");
        short type = I16(a);
        byte u02 = b[a + 2], u03 = b[a + 3];
        int u04 = I32(a + 4);
        int f1 = I32(a + 8), s10 = I32(a + 12), p1 = I32(a + 16), f2 = I32(a + 20), z = I32(a + 24), p2 = I32(a + 28);
        long fields = Off(a + 0x20), s10s = Off(a + 0x28), props = Off(a + 0x30);
        long unk = BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)a + 0x38));

        if (u02 != 0 || u03 != 0) s.Count(s.Oddities, $"action unk02/unk03 = {u02}/{u03}");
        if (u04 != 0) s.Count(s.Oddities, "action unk04 != 0");
        if (z != 0) s.Count(s.Oddities, "action +0x18 != 0");
        if (unk != 0) s.Count(s.Oddities, "action unknown offset +0x38 != 0");
        if (f1 + f2 == 0) s.Count(s.EmptyOffsets, $"action no fields: offset {(fields == 0 ? "0" : "nonzero")}");
        else FieldBlock("action", fields, f1 + f2);
        if (s10 == 0) s.Count(s.EmptyOffsets, $"action no section10s: offset {(s10s == 0 ? "0" : "nonzero")}");
        if (p1 + p2 == 0) s.Count(s.EmptyOffsets, $"action no properties: offset {(props == 0 ? "0" : "nonzero")}");
        if (u04 != 0) s.Count(s.Unk04Values, $"type {type} unk04 {u04}");
        s.Count(s.Unk0203, $"{u02}/{u03}");

        s.ActionLayouts.TryAdd(type, new());
        var layout = $"f1 {f1} f2 {f2} p1 {p1} p2 {p2} s10 {s10}";
        s.ActionLayouts[type][layout] = s.ActionLayouts[type].GetValueOrDefault(layout) + 1;
        s.ActionOwners.TryAdd(type, new());
        s.ActionOwners[type].Add(owner);

        if (s.Game == Game.DS3 && type == 10012 && f2 > 0 && fields != 0 && I32(fields + f1 * 4L) == -2)
            s.Count(s.Oddities, "DS3 Tracer fields2 starts with -2");

        for (int i = 0; i < p1 + p2; i++) Property(props + i * 0x28L, true);

        for (int i = 0; i < s10; i++)
        {
            long e = s10s + i * 0x10L;
            if (I32(e + 12) != 0) s.Count(s.Oddities, "section10 +0x0C != 0");
        }
    }

    void Property(long p, bool modifiable)
    {
        if (!seenProps.Add(p)) s.Count(s.Oddities, "property record shared");
        ushort a = U16(p);
        int vt = a & 3, func = (a >> 4) & 0xF, loop = (a >> 12) & 1;
        if ((a & ~0x10F3) != 0) s.Count(s.Oddities, $"property typeEnumA extra bits 0x{a:X4}");
        if (b[p + 2] != 0 || b[p + 3] != 1) s.Count(s.Oddities, "property +2/+3 != 0/1");
        int enumB = I32(p + 4);
        int expectB = vt | func << 2 | loop << 4;
        s.Count(s.PropEnumB,
            expectB == 0 ? (enumB == 0 ? "packed is 0 (ambiguous)" : "packed is 0, stored nonzero") :
            enumB == expectB ? "typeEnumB = packed" : enumB == 0 ? "typeEnumB = 0" : $"typeEnumB other (func {func} vt {vt} stored 0x{enumB:X})");
        s.Count(s.PropFuncs, $"func {func} vt {vt}{(loop != 0 ? " loop" : "")}{(modifiable ? "" : " (modprop)")}");
        int fieldCount = I32(p + 8);
        long fields = Off(p + 0x10);
        if (fieldCount > 0) FieldBlock(modifiable ? "property" : "modprop", fields, fieldCount);
        else s.Count(s.EmptyOffsets, $"{(modifiable ? "property" : "modprop")} no fields: offset {(fields == 0 ? "0" : "nonzero")}");
        CheckPropFieldCount(func, vt, fieldCount, fields);

        if (!modifiable) return;
        long mods = Off(p + 0x18);
        int modCount = I32(p + 0x20);
        if (modCount == 0) s.Count(s.EmptyOffsets, $"property no modifiers: offset {(mods == 0 ? "0" : "nonzero")}");
        for (int i = 0; i < modCount; i++) Modifier(mods + i * 0x20L);
    }

    // Field pool sharing: is a block's offset reused, and is identical content always reused?
    readonly Dictionary<long, string> blockOwner = new();
    readonly Dictionary<string, long> contentFirst = new();

    void FieldBlock(string kind, long offset, int count)
    {
        var content = Convert.ToHexString(b, (int)offset, count * 4);
        if (blockOwner.TryGetValue(offset, out var prevKind))
        {
            bool sameLen = contentFirst.TryGetValue(content, out var first) && first == offset;
            s.Count(s.Sharing, $"{kind} block reuses offset of earlier {prevKind}{(sameLen ? "" : " (different length)")}");
            return;
        }
        blockOwner[offset] = kind;
        if (contentFirst.ContainsKey(content)) s.Count(s.Sharing, $"{kind} block duplicates earlier content but NOT shared");
        else { contentFirst[content] = offset; s.Count(s.Sharing, $"{kind} block unique"); }
    }

    void CheckPropFieldCount(int func, int vt, int fieldCount, long fields)
    {
        int c = vt + 1;
        int expect = -1;
        if (func is 0 or 1) expect = 0;
        else if (func == 2) expect = c;
        else if (func is >= 3 and <= 6 && fieldCount > 0)
        {
            int n = I32(fields);
            expect = 1 + 2 * c + n + n * c * (func >= 5 ? 3 : 1);
        }
        else if (func == 7 && fieldCount > 0)
        {
            int total = 0;
            for (int i = 0; i < c; i++) total += I32(fields + 4 + i * 4L);
            expect = 1 + 3 * c + 4 * total;
        }
        if (expect >= 0 && expect != fieldCount) s.Count(s.Oddities, $"property func {func} field count mismatch");
    }

    void Modifier(long m)
    {
        ushort a = U16(m);
        int type = ((a >> 12) & 3) << 4 | (a >> 4) & 0xF;
        int vt = a & 3;
        int enumB = I32(m + 4);
        int fieldCount = I32(m + 8), propCount = I32(m + 12);
        long fields = Off(m + 0x10), props = Off(m + 0x18);
        s.Count(s.Modifiers, $"type {type} enumB {enumB & ~3}");
        if (fieldCount > 0) FieldBlock("modifier", fields, fieldCount);
        if (type is 38 or 39 && fieldCount > 0)
        {
            int ev = I32(fields);
            if (type == 38 || ev != 10000) ext1.Add(ev);
            if (type == 39) ext2.Add(ev);
        }
        for (int i = 0; i < propCount; i++) Property(props + i * 0x18L, false);
    }
}

class Survey(Game game)
{
    public Game Game = game;
    public int Binders, Files, Duplicates, Dumped, MaxDepth;
    public int Fxr3Ok, Fxr3ReadFail, Fxr3WriteFail;
    public int FxrOk, FxrFail, FxrDumped, EditOk, EditFail;
    public List<string> EditErrors = new();
    public List<string> FxrErrors = new();
    public Dictionary<string, int> FxrDiffs = new(), FxrStyles = new();
    public List<string> Fxr3Errors = new(), Failures = new(), RefDiffExamples = new();
    public Dictionary<string, int> Fxr3DiffSections = new();
    public Dictionary<string, int> Headers = new(), Oddities = new(), RefLists = new(), Conditions = new(),
        Nodes = new(), Configs = new(), PropEnumB = new(), PropFuncs = new(), Modifiers = new(),
        EmptyOffsets = new(), Sharing = new(), Unk04Values = new(), Unk0203 = new();
    public SortedDictionary<int, Dictionary<string, int>> ActionLayouts = new();
    public Dictionary<int, HashSet<string>> ActionOwners = new();

    public void Fail(string m) => Failures.Add(m);
    public void Count(Dictionary<string, int> d, string k) => d[k] = d.GetValueOrDefault(k) + 1;

    public void Report(Action<string> log, int bndCount)
    {
        log($"== {Game}: {bndCount} ffxbnds ({Binders} read), {Files} fxr ({Duplicates} duplicate names), max node depth {MaxDepth}");
        log($"   FXR (new) round trip: {FxrOk}/{Files} byte-perfect, {FxrFail} read/write fail");
        if (EditOk + EditFail > 0) log($"   edit check: {EditOk}/{EditOk + EditFail} pass");
        foreach (var e in EditErrors) log($"     ! edit {e}");
        foreach (var kv in FxrDiffs.OrderByDescending(k => k.Value)) log($"     first diff at {kv.Key}: {kv.Value}");
        foreach (var e in FxrErrors.Take(12)) log($"     ! {e}");
        foreach (var kv in FxrStyles.OrderByDescending(k => k.Value).Take(8)) log($"     {kv.Key}: {kv.Value}");
        foreach (var f in Failures.Take(5)) log($"   ! {f}");
        if (Fxr3Ok + Fxr3ReadFail + Fxr3WriteFail + Fxr3Errors.Count == 0) return;
        log($"   FXR3 round trip: {Fxr3Ok}/{Files} byte-perfect, {Fxr3ReadFail} read fail, {Fxr3WriteFail} write fail");
        foreach (var kv in Fxr3DiffSections.OrderByDescending(k => k.Value)) log($"     first diff in {kv.Key}: {kv.Value}");
        foreach (var e in Fxr3Errors.Take(8)) log($"     ! {e}");
        Dump(log, "header", Headers);
        Dump(log, "oddities", Oddities);
        Dump(log, "action unk02/unk03", Unk0203);
        Dump(log, "action unk04 (top 12)", Unk04Values.OrderByDescending(k => k.Value).Take(12).ToDictionary());
        Dump(log, "empty list offsets", EmptyOffsets);
        Dump(log, "field pool sharing", Sharing);
        Dump(log, "reference lists", RefLists);
        foreach (var e in RefDiffExamples.Take(6)) log($"     ! {e}");
        Dump(log, "property typeEnumB", PropEnumB);
        Dump(log, "property functions", PropFuncs);
        Dump(log, "modifiers", Modifiers);
        Dump(log, "conditions", Conditions);
        Dump(log, "nodes", Nodes);
        Dump(log, "configs", Configs);
        log($"   action types: {ActionLayouts.Count}");
        foreach (var (type, layouts) in ActionLayouts)
        {
            var total = layouts.Values.Sum();
            log($"     {type,6} x{total,-7} [{string.Join("; ", ActionOwners[type].Order())}]");
            foreach (var l in layouts.OrderByDescending(k => k.Value).Take(4)) log($"            {l.Key}  x{l.Value}");
            if (layouts.Count > 4) log($"            ... {layouts.Count - 4} more layouts");
        }
        foreach (var f in Failures.Take(20)) log($"   ! {f}");
    }

    static void Dump(Action<string> log, string title, Dictionary<string, int> d)
    {
        log($"   {title}:{(d.Count == 0 ? " none" : "")}");
        foreach (var kv in d.OrderBy(k => k.Key)) log($"     {kv.Key}: {kv.Value}");
    }
}
