using Andre.Core;
using Andre.IO.VFS;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Binary.Util;
using SoulsFormats;
using System.Reflection;
using System.Text.RegularExpressions;

// Compares the types embedded in a game's behavior files against HKLib's hk2018 type registry.
//   HkTypeDiff <GAME> <binder filter regex> [max binders]
// Run with the working directory at src/Smithbox.Data.

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var gameName = args[0];
var game = Enum.Parse<Game>(gameName);
var filter = new Regex(args.Length > 1 ? args[1] : ".", RegexOptions.IgnoreCase);
var maxBinders = args.Length > 2 ? int.Parse(args[2]) : 5;
var roundtrip = args.Contains("--roundtrip");
int rtOk = 0, rtFail = 0, cmsgTotal = 0, basePoseTrue = 0;

var real = new RealVirtualFileSystem(gamePaths[gameName], false);
using VirtualFileSystem fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(gamePaths[gameName], game), real]);

var registry = args.Contains("--base") || game != Game.NR ? HavokTypeRegistry.Instance : HavokTypeRegistry.Nightreign;
var reported = new HashSet<string>();
var ser = new HavokBinarySerializer();
MethodInfo M(string n) => typeof(HavokBinarySerializer).GetMethod(n, BindingFlags.NonPublic | BindingFlags.Instance)!;

var binders = fs.GetFileNamesMatching("/chr", @"(?i)\.behbnd(\.dcx)?$").Select(p => p.Replace((char)92, '/'))
    .Distinct(StringComparer.OrdinalIgnoreCase).Where(p => filter.IsMatch(p))
    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).Take(maxBinders).ToList();

foreach (var path in binders)
{
    var raw = fs.ReadFile(path).Value.ToArray();
    var bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);
    foreach (var file in bnd.Files)
    {
        var name = file.Name.Replace((char)92, '/');
        if (!name.Contains("/Behaviors/", StringComparison.OrdinalIgnoreCase) || !name.Contains(".hkx", StringComparison.OrdinalIgnoreCase)) continue;
        var bytes = name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? DCX.Decompress(file.Bytes).ToArray() : file.Bytes.ToArray();
        Console.WriteLine($"== {path} :: {Path.GetFileName(name)}");
        try
        {
            if (roundtrip) Roundtrip(bytes);
            else Diff(bytes);
        }
        catch (Exception e) { Console.WriteLine($"  failed: {e.GetType().Name}: {e.Message}"); }
    }
}

if (roundtrip)
    Console.WriteLine($"TOTAL files ok {rtOk}, failed {rtFail}; CMSG {cmsgTotal}, isBasePoseAnim true {basePoseTrue}");

// Reads with the game's registry, writes, rereads, and checks every CMSG field survived. Also checks the
// written type hash: rereading verifies THSH against the same registry, and the other registry must reject it.
void Roundtrip(byte[] bytes)
{
    var gameReg = game == Game.NR ? HavokTypeRegistry.Nightreign : HavokTypeRegistry.Instance;
    var otherReg = game == Game.NR ? HavokTypeRegistry.Instance : HavokTypeRegistry.Nightreign;

    var a = (HKLib.hk2018.hkRootLevelContainer)new HavokBinarySerializer(gameReg).Read(new MemoryStream(bytes));
    var ms = new MemoryStream();
    new HavokBinarySerializer(gameReg).Write(a, ms);
    var written = ms.ToArray();
    var b = (HKLib.hk2018.hkRootLevelContainer)new HavokBinarySerializer(gameReg).Read(new MemoryStream(written));

    var ca = CollectCmsg(a);
    var cb = CollectCmsg(b);
    string Sig(HKLib.hk2018.CustomManualSelectorGenerator c) =>
        $"{c.m_name}|{c.m_animId}|{c.m_offsetType}|{c.m_animeEndEventType}|{c.m_enableScript}|{c.m_enableTae}|{c.m_changeTypeOfSelectedIndexAfterActivate}|{c.m_checkAnimEndSlotNo}|{c.m_replanningAI}|{c.m_rideSync}|{c.m_isBasePoseAnim}|{c.m_generators.Count}";
    var sa = ca.Select(Sig).OrderBy(x => x).ToList();
    var sb = cb.Select(Sig).OrderBy(x => x).ToList();

    bool otherRejects = false;
    if (ca.Count > 0)
    {
        try { new HavokBinarySerializer(otherReg).Read(new MemoryStream(written)); }
        catch (InvalidDataException) { otherRejects = true; }
    }

    int trues = ca.Count(c => c.m_isBasePoseAnim);
    cmsgTotal += ca.Count;
    basePoseTrue += trues;
    bool ok = sa.SequenceEqual(sb) && (ca.Count == 0 || otherRejects);
    if (ok) rtOk++; else rtFail++;
    Console.WriteLine($"  {(ok ? "ok" : "MISMATCH")}: CMSG {ca.Count}, isBasePoseAnim true {trues}, size {bytes.Length} -> {written.Length}, other registry rejects: {otherRejects}");
}

static List<HKLib.hk2018.CustomManualSelectorGenerator> CollectCmsg(object root)
{
    var found = new HashSet<object>(ReferenceEqualityComparer.Instance);
    var result = new List<HKLib.hk2018.CustomManualSelectorGenerator>();
    var stack = new Stack<object>();
    stack.Push(root);
    while (stack.Count > 0)
    {
        var o = stack.Pop();
        if (o == null || o is string || o.GetType().IsPrimitive || o.GetType().IsEnum || !found.Add(o)) continue;
        if (o is HKLib.hk2018.CustomManualSelectorGenerator c) result.Add(c);
        if (o is System.Collections.IEnumerable list) { foreach (var x in list) stack.Push(x); continue; }
        foreach (var f in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            if (!f.FieldType.IsPrimitive && !f.FieldType.IsEnum && f.FieldType != typeof(string)) stack.Push(f.GetValue(o));
    }
    return result;
}

void Diff(byte[] bytes)
{
    var r = new HavokBinaryReader(bytes);
    r.EnterSection("TAG0");
    M("ReadSDKV").Invoke(ser, [r]);
    M("ReadDATA").Invoke(ser, [r]);
    if (r.GetSectionId() != "TYPE") { Console.WriteLine($"  section {r.GetSectionId()}, no embedded types"); return; }
    r.EnterSection("TYPE");
    M("ReadTPTR").Invoke(ser, [r]);
    var tstr = M("ReadTSTR").Invoke(ser, [r]);
    var builders = (IReadOnlyList<HavokTypeBuilder>)M("ReadTNA1").Invoke(ser, [r, tstr])!;
    var fstr = M("ReadFSTR").Invoke(ser, [r]);
    M("ReadTBDY").Invoke(ser, [r, builders, fstr]);
    var types = builders.Select((b, i) => i == 0 ? null : b.Build()).ToArray();

    var fileHashes = new Dictionary<int, uint>();
    if (r.GetSectionId() == "THSH")
    {
        r.EnterSection("THSH");
        int n = (int)r.ReadHavokVarUInt();
        for (int i = 0; i < n; i++) { int ti = (int)r.ReadHavokVarUInt(); fileHashes[ti] = r.ReadUInt32(); }
        r.ExitSection();
    }

    for (int i = 1; i < types.Length; i++)
    {
        var ft = types[i];
        var rt = registry.GetType(ft.Identity);
        var diffs = new List<string>();
        if (rt == null) diffs.Add("missing from registry");
        else
        {
            if (fileHashes.TryGetValue(i, out var fh) && rt.Hash != fh) diffs.Add($"hash file {fh} registry {rt.Hash}");
            if (ft.Optionals.HasFlag(HKLib.Reflection.hk2018.HavokType.Optional.SizeAlign) && (ft.Size != rt.Size || ft.Alignment != rt.Alignment)) diffs.Add($"size/align file {ft.Size}/{ft.Alignment} registry {rt.Size}/{rt.Alignment}");
            if (ft.Optionals.HasFlag(HKLib.Reflection.hk2018.HavokType.Optional.Version) && ft.Version != rt.Version) diffs.Add($"version file {ft.Version} registry {rt.Version}");
            if (ft.Optionals.HasFlag(HKLib.Reflection.hk2018.HavokType.Optional.Format) && ft.Format != rt.Format) diffs.Add($"format file {ft.Format} registry {rt.Format}");
            if (ft.Optionals.HasFlag(HKLib.Reflection.hk2018.HavokType.Optional.Flags) && ft.Flags != rt.Flags) diffs.Add($"flags file {ft.Flags} registry {rt.Flags}");
            if (ft.Parent?.Identity != rt.Parent?.Identity) diffs.Add($"parent file {ft.Parent?.Identity} registry {rt.Parent?.Identity}");
            if (ft.Optionals.HasFlag(HKLib.Reflection.hk2018.HavokType.Optional.Members))
            {
                string F(HKLib.Reflection.hk2018.HavokType.Member m) => $"{m.Name}@{m.Offset}:{m.Type.Identity}:{(int)m.Flags}";
                // file lists all fields including parent's after Build? compare own fields only
                var ff = ft.Fields.Select(F).ToList();
                var rf = rt.Fields.Select(F).ToList();
                if (!ff.SequenceEqual(rf))
                {
                    diffs.Add("fields differ:");
                    foreach (var x in ff.Except(rf)) diffs.Add($"    file only     {x}");
                    foreach (var x in rf.Except(ff)) diffs.Add($"    registry only {x}");
                }
            }
        }
        if (diffs.Count > 0 && reported.Add(ft.Identity + string.Join("|", diffs)))
        {
            Console.WriteLine($"  [{i}] {ft.Identity}");
            foreach (var d in diffs) Console.WriteLine($"    {d}");
        }
    }
}
