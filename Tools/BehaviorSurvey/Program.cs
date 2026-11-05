using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

// Checks the behavior graph assumptions the Havok Editor's behavior views rely on, against the game files.
//   BehaviorSurvey <GAME> <binder filter regex> [max binders]
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var gameName = args[0];
var game = Enum.Parse<Game>(gameName);
var filter = new Regex(args.Length > 1 ? args[1] : ".", RegexOptions.IgnoreCase);
var maxBinders = args.Length > 2 ? int.Parse(args[2]) : 5;

using var fs = BuildFS(game, gamePaths[gameName]);

if (args.Length > 3 && args[3] == "taedump")
{
    // taedump: the anibnd time acts of the first matching chr, and its clip names by time act
    var p = fs.GetFileNamesMatching("/chr", @"(?i)\.anibnd(\.dcx)?$").Select(x => x.Replace('\\', '/')).Where(x => filter.IsMatch(x)).OrderBy(x => x).ToList();
    if (args[1].StartsWith("/")) p = new List<string> { args[1] };
    foreach (var ap in p.Take(maxBinders))
    {
        var raw0 = fs.ReadFile(ap).Value.ToArray();
        var b0 = BND4.Read(DCX.Is(raw0) ? DCX.Decompress(raw0).ToArray() : raw0);
        Console.WriteLine($"{ap}: {b0.Files.Count} files, marker 9999999: {b0.Files.Any(f => f.ID == 9999999)}");
        foreach (var f in b0.Files.Where(f => !f.Name.Contains(".hkx", StringComparison.OrdinalIgnoreCase)).Take(30))
        {
            string info = "";
            if (f.Name.ToLower().Contains(".tae") && f.Bytes.Length >= 4)
            {
                try { var t = TAE.Read(f.Bytes); info = $"tae id {t.ID} anims {t.Animations.Count} first [{string.Join(",", t.Animations.Take(5).Select(a => a.ID))}] skeleton {t.SkeletonName} sib {t.SibName}"; } catch (Exception e) { info = "read fail " + e.Message; }
            }
            Console.WriteLine($"  [{f.ID}] {f.Name} ({f.Bytes.Length}) {info}");
        }
        var hkx = b0.Files.Where(f => f.Name.Contains(".hkx", StringComparison.OrdinalIgnoreCase)).ToList();
        Console.WriteLine($"  hkx files: {hkx.Count}, sample: {string.Join(", ", hkx.Take(6).Select(f => $"[{f.ID}] {Path.GetFileName(f.Name)}"))}");
    }
    return;
}

var binders = fs.GetFileNamesMatching("/chr", @"(?i)\.behbnd(\.dcx)?$")
    .Select(p => p.Replace('\\', '/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .Where(p => filter.IsMatch(p))
    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
    .Take(maxBinders)
    .ToList();

Console.WriteLine($"{gameName}: {binders.Count} binders");

var total = new Survey();

foreach (var path in binders)
{
    var raw = fs.ReadFile(path).Value.ToArray();
    var bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);

    byte[] compendium = bnd.Files.FirstOrDefault(f => f.Name.Contains(".compendium", StringComparison.OrdinalIgnoreCase)) is { } c
        ? (c.Name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? DCX.Decompress(c.Bytes).ToArray() : c.Bytes.ToArray())
        : null;

    if (!bnd.Files.Any(f => f.Name.Contains("/Behaviors/", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("\\Behaviors\\", StringComparison.OrdinalIgnoreCase)))
        Console.WriteLine($"  {path}: no Behaviors folder; extensions: {string.Join(", ", bnd.Files.GroupBy(f => Path.GetExtension(f.Name.Replace(".dcx", ""))).Select(g => $"{g.Key} x{g.Count()}"))}; hkx: {string.Join(", ", bnd.Files.Where(f => f.Name.Contains(".hkx")).Take(5).Select(f => f.Name))}");

    var timeActs = ReadTimeActs(fs, game, path);
    var animFiles = ReadAnimationFiles(fs, path);
    if (Environment.GetEnvironmentVariable("BS_DEBUG") == "1") Console.WriteLine($"  DEBUG {path}: anim files {animFiles?.Count ?? -1}, time acts {timeActs?.Count ?? -1}");

    foreach (var file in bnd.Files)
    {
        var name = file.Name.Replace('\\', '/');

        if (!name.Contains("/Behaviors/", StringComparison.OrdinalIgnoreCase) || !name.Contains(".hkx", StringComparison.OrdinalIgnoreCase))
            continue;

        var bytes = name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? DCX.Decompress(file.Bytes).ToArray() : file.Bytes.ToArray();

        object root;

        try
        {
            root = ReadRoot(game, bytes, compendium);
        }
        catch (Exception e)
        {
            Console.WriteLine($"  {Path.GetFileName(name)}: read failed: {e.GetType().Name}: {e.Message}");
            continue;
        }

        Console.WriteLine($"== {path} :: {Path.GetFileName(name)}");
        var survey = new Survey { TimeActs = timeActs, AnimFiles = animFiles, FileKind = Path.GetFileName(name).StartsWith("c9997", StringComparison.OrdinalIgnoreCase) ? "c9997" : (Path.GetFileNameWithoutExtension(name).Split('.')[0].Equals(Path.GetFileName(path).Split('.')[0], StringComparison.OrdinalIgnoreCase) ? "own" : "other") };
        survey.Run(root);
        survey.Print(detail: true);
        total.Merge(survey);
    }
}

Console.WriteLine();
Console.WriteLine("==== TOTAL");
total.Print(detail: false);

static object ReadRoot(Game game, byte[] bytes, byte[] compendium)
{
    switch (game)
    {
        case Game.DS3:
        {
            var deserializer = new HKX2.PackFileDeserializer();
            return deserializer.Deserialize(new BinaryReaderEx(false, bytes));
        }
        case Game.AC6:
        {
            var serializer = new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer();
            if (compendium != null)
                serializer.LoadCompendium(new MemoryStream(compendium));
            return serializer.Read(new MemoryStream(bytes));
        }
        default:
        {
            var serializer = new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer(game == Game.NR
                ? HKLib.Reflection.hk2018.HavokTypeRegistry.Nightreign
                : HKLib.Reflection.hk2018.HavokTypeRegistry.Instance);
            if (compendium != null)
                serializer.LoadCompendium(new MemoryStream(compendium));
            return serializer.Read(new MemoryStream(bytes));
        }
    }
}

// The time acts of the chr's anibnd, as the Time Act Editor reads them (TimeActContainerWrapper.AddTimeActs):
// time act number -> full animation IDs (number * 1,000,000 + animation ID, unless the ID is already full).
// Null if the chr has no anibnd.
static Dictionary<int, HashSet<long>> ReadTimeActs(VirtualFileSystem fs, Game game, string behbndPath)
{
    var anibndPath = Regex.Replace(behbndPath, @"\.behbnd", ".anibnd", RegexOptions.IgnoreCase);

    if (!fs.FileExists(anibndPath))
        return null;

    var raw = fs.ReadFile(anibndPath).Value.ToArray();
    var bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);
    var firstID = bnd.Files.Any(f => f.ID == 9999999) ? 5000000 : 3000000;
    var result = new Dictionary<int, HashSet<long>>();

    foreach (var file in bnd.Files)
    {
        if (!file.Name.ToLower().Contains(".tae") || file.Bytes.Length < 4)
            continue;

        var number = file.ID % firstID;
        var ids = new HashSet<long>();

        try
        {
            var tae = TAE.Read(file.Bytes);
            foreach (var animation in tae.Animations)
                ids.Add(animation.ID >= 1000000 ? animation.ID : (long)number * 1000000 + animation.ID);
        }
        catch
        {
            continue;
        }

        result[number] = ids;
    }

    return result;
}

// The animation HKX files (aXXX_YYYYYY, lower case) in the chr's anibnd and the anibnds split from it
// (cXXXX_div00..., and the player's c0000_aXX_... files). Null if there are none.
static HashSet<string> ReadAnimationFiles(VirtualFileSystem fs, string behbndPath)
{
    var chr = Path.GetFileName(behbndPath).Split('.')[0];
    var paths = fs.GetFileNamesMatching("/chr", $@"(?i)(^|/){chr}(_[^./]+)?\.anibnd(\.dcx)?$").Select(p => p.Replace('\\', '/')).ToList();
    if (Environment.GetEnvironmentVariable("BS_DEBUG") == "1") Console.WriteLine($"  DEBUG anibnds for {chr}: {string.Join(", ", paths.Take(10))}");
    var main = $"/chr/{chr}.anibnd.dcx";
    if (fs.FileExists(main) && !paths.Contains(main)) paths.Add(main);

    var result = new HashSet<string>();
    foreach (var p in paths.Distinct())
    {
        try
        {
            var raw = fs.ReadFile(p).Value.ToArray();
            var bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);
            foreach (var f in bnd.Files)
            {
                var stem = Path.GetFileName(f.Name.Replace('\\', '/')).Split('.')[0].ToLowerInvariant();
                if (Regex.IsMatch(stem, @"^a\d+_\d+$")) result.Add(stem);
            }
        }
        catch { }
    }
    return result.Count > 0 ? result : null;
}

static VirtualFileSystem BuildFS(Game game, string root)
{
    var real = new RealVirtualFileSystem(root, false);

    if (game is Game.BB or Game.DES or Game.DB_NT or Game.DB)
        return real;

    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}

class Survey
{
    private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new();

    public static FieldInfo[] Fields(Type t)
    {
        if (!FieldCache.TryGetValue(t, out var f))
        {
            f = t.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.MetadataToken).ToArray();
            FieldCache[t] = f;
        }
        return f;
    }

    public static object Get(object o, string n) => o?.GetType().GetField(n, BindingFlags.Instance | BindingFlags.Public)?.GetValue(o);
    public static int GetInt(object o, string n) => Get(o, n) is { } v ? Convert.ToInt32(v) : int.MinValue;

    // object -> (parent, field name)
    readonly Dictionary<object, List<(object Parent, string Field)>> Parents = new(ReferenceEqualityComparer.Instance);
    readonly List<object> All = new();

    // Counters (name -> count), and example lists
    public Dictionary<int, HashSet<long>> TimeActs;
    public HashSet<string> AnimFiles;
    public string FileKind = "";
    HashSet<long> AllAnimations;

    public readonly SortedDictionary<string, long> Counts = new();
    public readonly SortedDictionary<string, List<string>> Examples = new();

    void Count(string key, long n = 1) => Counts[key] = Counts.GetValueOrDefault(key) + n;

    void Example(string key, string value, int max = 12)
    {
        if (!Examples.TryGetValue(key, out var list))
            Examples[key] = list = new List<string>();
        if (list.Count < max && !list.Contains(value))
            list.Add(value);
    }

    public void Merge(Survey other)
    {
        foreach (var (k, v) in other.Counts)
            Count(k, v);
        foreach (var (k, list) in other.Examples)
            foreach (var e in list)
                Example(k, e, 25);
    }

    static bool IsObj(object v) => v != null && v.GetType().IsClass && v is not string && v is not Array;

    public void Run(object root)
    {
        // Walk, recording parents with the field they came through
        var stack = new Stack<(object, object, string)>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        stack.Push((root, null, null));

        while (stack.Count > 0)
        {
            var (obj, parent, field) = stack.Pop();

            if (parent != null)
            {
                if (!Parents.TryGetValue(obj, out var pl))
                    Parents[obj] = pl = new();
                pl.Add((parent, field));
            }

            if (!visited.Add(obj))
                continue;

            All.Add(obj);

            foreach (var f in Fields(obj.GetType()))
            {
                if (f.FieldType == typeof(string) || f.FieldType.IsValueType)
                    continue;
                var v = f.GetValue(obj);
                if (v is IList list && v is not string)
                {
                    foreach (var e in list)
                        if (IsObj(e))
                            stack.Push((e, obj, f.Name));
                }
                else if (IsObj(v))
                {
                    stack.Push((v, obj, f.Name));
                }
            }
        }

        Count("objects", All.Count);

        var stringData = All.FirstOrDefault(o => o.GetType().Name == "hkbBehaviorGraphStringData");
        var graphData = All.FirstOrDefault(o => o.GetType().Name == "hkbBehaviorGraphData");

        if (stringData == null)
        {
            Count("NO STRING DATA");
            return;
        }

        var eventNames = (IList)Get(stringData, "m_eventNames");
        var variableNames = (IList)Get(stringData, "m_variableNames");
        var charPropNames = (IList)Get(stringData, "m_characterPropertyNames");
        var animNames = Get(stringData, "m_animationNames") as IList;

        Count("names.events", eventNames?.Count ?? 0);
        Count("names.variables", variableNames?.Count ?? 0);
        Count("names.characterProperties", charPropNames?.Count ?? 0);
        Count("names.animations", animNames?.Count ?? -1);
        Count("names.attributes", (Get(stringData, "m_attributeNames") as IList)?.Count ?? 0);

        // Parallel arrays
        var eventInfos = Get(graphData, "m_eventInfos") as IList;
        var varInfos = Get(graphData, "m_variableInfos") as IList;
        var varBounds = Get(graphData, "m_variableBounds") as IList;
        var cpInfos = Get(graphData, "m_characterPropertyInfos") as IList;
        var initVals = Get(Get(graphData, "m_variableInitialValues"), "m_wordVariableValues") as IList;
        if (eventInfos?.Count != eventNames?.Count) Count("MISMATCH eventInfos vs eventNames");
        if (varInfos?.Count != variableNames?.Count) Count("MISMATCH variableInfos vs variableNames");
        if (varBounds != null && varBounds.Count != 0 && varBounds.Count != variableNames?.Count) Count("MISMATCH variableBounds vs variableNames");
        if (varBounds != null && varBounds.Count == 0) Count("variableBounds empty");
        if (initVals?.Count != variableNames?.Count) Count("MISMATCH wordVariableValues vs variableNames");
        if (cpInfos?.Count != charPropNames?.Count) Count("MISMATCH characterPropertyInfos vs characterPropertyNames");
        Example("sample.varInfo/bounds/init counts", $"{varInfos?.Count}/{varBounds?.Count}/{initVals?.Count} vs names {variableNames?.Count}");

        foreach (var obj in All)
        {
            var type = obj.GetType();
            var tn = type.Name;
            Count($"type.{tn}");

            // Events: every int field whose name says event
            foreach (var f in Fields(type))
            {
                if (f.FieldType != typeof(int))
                    continue;

                var isEventField = (tn is "hkbEvent" or "hkbEventProperty" or "hkbEventBase" && f.Name == "m_id") || f.Name.EndsWith("EventId", StringComparison.OrdinalIgnoreCase);

                if (!isEventField)
                    continue;

                var id = (int)f.GetValue(obj);
                var key = $"event.{tn}.{f.Name}";
                if (id == -1) Count(key + " =-1");
                else if (id >= 0 && id < eventNames.Count) Count(key + " ok");
                else { Count(key + " OUT OF RANGE"); Example(key + " OUT OF RANGE", id.ToString()); }
            }

            // Other int fields that look like indices, to find fields not tagged yet
            foreach (var f in Fields(type))
            {
                if (f.FieldType != typeof(int) && f.FieldType != typeof(short))
                    continue;
                var lname = f.Name.ToLowerInvariant();
                if ((lname.Contains("event") || lname.Contains("variable") || lname.Contains("stateid") || lname.Contains("property")) )
                    Example("candidate index fields", $"{tn}.{f.Name}", 60);
            }

            if (tn == "hkbEventProperty" || tn == "hkbEvent")
                foreach (var (p, pf) in Parents.GetValueOrDefault(obj) ?? new()) Example("event objects reached via", $"{p.GetType().Name}.{pf}", 30);

            // Generator types, with the fields that hold other generators
            if (IsGenerator(type))
                Example("generator types", tn + (IsTransitionEffect(type) ? " (transition effect)" : ""), 60);

            if (tn == "hkbClipGenerator")
            {
                var internalId = GetInt(obj, "m_animationInternalId");
                var animName = Get(obj, "m_animationName") as string;
                if (animNames == null || animNames.Count == 0) Count("clip.internalId no animationNames list");
                else if (internalId >= 0 && internalId < animNames.Count)
                {
                    var listed = animNames[internalId] as string;
                    if (string.Equals(listed, animName, StringComparison.OrdinalIgnoreCase)) Count("clip.internalId -> animationNames matches m_animationName");
                    else if (string.Equals(Path.GetFileNameWithoutExtension((listed ?? "").Replace('\\', '/')), animName, StringComparison.OrdinalIgnoreCase)) Count("clip.internalId -> animationNames STEM matches m_animationName");
                    else { Count("clip.internalId -> animationNames DIFFERENT name"); Example("clip internalId name mismatch", $"{internalId}: list '{listed}' vs clip '{animName}'"); }
                }
                else { Count("clip.internalId OUT OF RANGE"); Example("clip internalId out of range", $"{internalId} (list {animNames.Count})"); }
                Example("clip m_animationName", animName ?? "null");
            }

            // Phase 3: clips and CMSGs against the chr's time acts
    void CheckTimeActs(object obj, string tn)
    {
        if (TimeActs == null)
        {
            if (tn == "hkbClipGenerator") Count("tae.no anibnd for chr");
            return;
        }

        // Every full animation ID the chr's time acts have (a player has one time act per category;
        // an enemy has one time act whose animations carry the category in their ID)
        AllAnimations ??= TimeActs.Values.SelectMany(v => v).ToHashSet();
        if (TimeActs.Count == 1) Count("tae.chr with 1 time act (per clip)");

        if (tn == "hkbClipGenerator")
        {
            var name = Get(obj, "m_animationName") as string ?? "";
            var m = Regex.Match(name, @"^a(\d+)_(\d+)$");
            if (!m.Success) { Count("tae.clip name not aXXX_YYYYYY"); Example("tae clip odd names", name); return; }
            var full = int.Parse(m.Groups[1].Value) * 1000000L + long.Parse(m.Groups[2].Value);
            var holders = TimeActs.Where(t => t.Value.Contains(full)).Select(t => t.Key).ToList();
            if (AnimFiles != null) Count($"anim.hkx [{FileKind}] " + (AnimFiles.Contains(name.ToLowerInvariant()) ? "exists" : "MISSING") + (holders.Count > 0 ? ", has TAE entry" : ", no TAE entry"));
            if (AnimFiles != null && !AnimFiles.Contains(name.ToLowerInvariant()) && FileKind == "own") Example("anim hkx missing [own]", name, 25);
            if (holders.Count == 1) Count("tae.clip found in exactly 1 time act");
            else if (holders.Count > 1) { Count("tae.clip found in SEVERAL time acts"); Example("tae clip several", $"{name}: {string.Join(",", holders.Take(5))}"); }
            else
            {
                // Under an AnimIdOffset/category CMSG the clip's own category may be a placeholder
                var parentCmsg = Parents.GetValueOrDefault(obj)?.Select(p => p.Parent).FirstOrDefault(p => p.GetType().Name == "CustomManualSelectorGenerator");
                var offset = Get(parentCmsg, "m_offsetType")?.ToString() ?? "no CMSG";
                Count($"tae.clip NOT FOUND (parent CMSG offset {offset})");
                Example($"tae clip not found ({offset})", name);
            }
        }

        if (tn == "CustomManualSelectorGenerator")
        {
            var animId = GetInt(obj, "m_animId");
            var offset = Get(obj, "m_offsetType")?.ToString() ?? "?";
            var categories = AllAnimations.Where(id => id % 1000000 == animId).Select(id => id / 1000000).Distinct().OrderBy(c => c).ToList();
            var bucket = categories.Count switch { 0 => "0", 1 => "1", < 5 => "2-4", _ => "5+" };
            Count($"tae.cmsg {offset}: animId in {bucket} categories");
            if (categories.Count > 0) Example($"tae cmsg {offset} categories", $"{animId}: {string.Join(",", categories.Take(6))}", 6);

            // The CMSG's clips: are they all the same animation, or one per category?
            var clipNames = ((IList)Get(obj, "m_generators")).Cast<object>().Where(g => g?.GetType().Name == "hkbClipGenerator")
                .Select(g => Get(g, "m_animationName") as string ?? "").ToList();
            var lowerMatch = clipNames.All(n => Regex.Match(n, @"^a\d+_(\d+)$") is { Success: true } mm && int.Parse(mm.Groups[1].Value) == animId);
            Count(lowerMatch ? "tae.cmsg all clips' lower id == animId" : "tae.cmsg some clip lower id DIFFERS");
            Example($"tae cmsg {offset} clips", $"{animId}: {string.Join(",", clipNames.Take(5))}", 6);
        }
    }

            CheckTimeActs(obj, tn);

    // NR's extra CMSG field: how it's used, and what else is set on CMSGs that have it
            if (tn == "CustomManualSelectorGenerator" && Get(obj, "m_isBasePoseAnim") is bool basePose)
            {
                Count($"cmsg.isBasePoseAnim {basePose}");
                if (basePose)
                    Example("cmsg base pose", $"{Get(obj, "m_name")} anim {Get(obj, "m_animId")} offset {Get(obj, "m_offsetType")} gens {((IList)Get(obj, "m_generators")).Count} tae {Get(obj, "m_enableTae")}", 15);
            }

            if (tn == "hkbStateMachine")
            {
                var states = (IList)Get(obj, "m_states");
                var ids = states.Cast<object>().Where(s => s != null).Select(s => GetInt(s, "m_stateId")).ToList();
                if (ids.Count != ids.Distinct().Count()) Count("sm DUPLICATE stateIds");
                var start = GetInt(obj, "m_startStateId");
                Count(ids.Contains(start) ? "sm.startStateId ok" : "sm.startStateId NOT FOUND");
                if (!ids.Contains(start)) Example("sm start not found", $"{Get(obj, "m_name")} start {start} ids [{string.Join(",", ids.Take(8))}]");
                Count("sm.syncVariableIndex " + (GetInt(obj, "m_syncVariableIndex") == -1 ? "-1" : "set"));
                if (Get(obj, "m_startStateIdSelector") != null) Count("sm has startStateIdSelector");
            }

            // Transitions: resolve toStateId in the owning state machine
            if (tn is "TransitionInfo" or "hkbStateMachineTransitionInfo")
            {
                var to = GetInt(obj, "m_toStateId");
                var sm = FindOwningSM(obj, out var via);
                if (sm == null) { Count("transition NO OWNING SM"); continue; }
                var states = ((IList)Get(sm, "m_states")).Cast<object>().Where(s => s != null).ToList();
                var ok = states.Any(s => GetInt(s, "m_stateId") == to);
                Count($"transition.toStateId ({via}) " + (ok ? "ok" : "NOT FOUND"));
                if (!ok) Example("transition to not found", $"sm {Get(sm, "m_name")} to {to} via {via} flags {Get(obj, "m_flags")}");
                var fromNested = GetInt(obj, "m_fromNestedStateId");
                var toNested = GetInt(obj, "m_toNestedStateId");
                if (fromNested != 0 || toNested != 0) { Count("transition nested ids non-zero"); Example("nested ids", $"from {fromNested} to {toNested} flags {Get(obj, "m_flags")}"); }
                else Count("transition nested ids zero");
                var tri = Parents.GetValueOrDefault(obj)?.Select(p => p.Parent).FirstOrDefault();
                if (tri != null && Parents.TryGetValue(tri, out var triParents) && triParents.Count > 1) Count("transitionArray SHARED");
            }

            // Bindings
            if (tn is "Binding" or "hkbVariableBindingSetBinding")
            {
                var memberPath = Get(obj, "m_memberPath") as string ?? "";
                var bindingType = Convert.ToInt32(Get(obj, "m_bindingType"));
                var index = GetInt(obj, "m_variableIndex");
                var names = bindingType == 1 ? charPropNames : variableNames;
                Count($"binding.type{bindingType}");
                Count("binding.index " + (index >= 0 && index < names.Count ? "ok" : "OUT OF RANGE"));
                Count("binding.bitIndex " + (Convert.ToInt32(Get(obj, "m_bitIndex")) == -1 ? "-1" : "set"));

                // Owner: binding -> binding set -> bindable(s)
                var set = Parents.GetValueOrDefault(obj)?.FirstOrDefault().Parent;
                var owners = set == null ? new() : Parents.GetValueOrDefault(set) ?? new();
                if (owners.Count > 1) Count("bindingSet SHARED by several bindables");
                foreach (var (owner, viaField) in owners.Take(1))
                {
                    var shape = Regex.Replace(memberPath, @"\d+", "#");
                    Example("binding path shapes", $"{owner.GetType().Name}: {shape}", 40);
                    var direct = Fields(owner.GetType()).Any(f => string.Equals(f.Name, "m_" + memberPath, StringComparison.OrdinalIgnoreCase) || string.Equals(f.Name, memberPath, StringComparison.OrdinalIgnoreCase));
                    if (direct) Count("binding.path matches direct field");
                    else if (memberPath.Contains('/') || memberPath.Contains(':'))
                    {
                        Count("binding.path compound");
                        Example("binding compound paths", $"{owner.GetType().Name}: {memberPath}", 20);
                        var resolved = ResolvePath(owner, memberPath, out var why);
                        Count("binding.compound " + (resolved ? "RESOLVES (field:index/field)" : "UNRESOLVED"));
                        if (!resolved) Example("binding compound unresolved", $"{owner.GetType().Name}: {memberPath} ({why})", 20);
                    }
                    else { Count("binding.path UNMATCHED simple"); Example("binding unmatched simple", $"{owner.GetType().Name}: {memberPath}", 20); }
                }
            }
        }

        // Outline reachability, with the editor's rules: nodes are generators (not transition effects) and states;
        // intermediates are looked through up to 2 deep; effects, modifiers, conditions, bindings, bags and graph data are skipped
        {
            var reached = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var queue = new Queue<object>(All.Where(o => o.GetType().Name == "hkbBehaviorGraph"));
            foreach (var g in queue) reached.Add(g);
            var maxChildren = 0;
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var children = new List<object>();
                var seen = new HashSet<object>(ReferenceEqualityComparer.Instance) { node };
                CollectOutline(node, children, 0, seen);
                maxChildren = Math.Max(maxChildren, children.Count);
                foreach (var c in children)
                    if (reached.Add(c)) queue.Enqueue(c);
            }
            Count("outline.reached nodes", reached.Count);
            Example("outline max children", maxChildren.ToString());
            foreach (var obj in All)
            {
                if (OutlineKind(obj.GetType()) != 0 || reached.Contains(obj)) continue;
                Count("outline.UNREACHED " + obj.GetType().Name);
                var via = Parents.GetValueOrDefault(obj)?.FirstOrDefault();
                Example("outline unreached via", $"{obj.GetType().Name} <- {via?.Parent?.GetType().Name}.{via?.Field}", 20);
            }
        }

        // Shared objects, by type, and the fields generators are reached through
        foreach (var (obj, parents) in Parents)
        {
            var distinct = parents.Select(p => p.Parent).Distinct(ReferenceEqualityComparer.Instance).Count();
            if (distinct > 1)
            {
                Count($"shared.{obj.GetType().Name}");
                Example("shared max parents", $"{obj.GetType().Name} x{distinct}", 15);
            }

            if (IsGenerator(obj.GetType()))
            {
                foreach (var (parent, field) in parents)
                    Example("generator reached via", $"{parent.GetType().Name}.{field}", 60);
            }
        }
    }

    /// <summary>
    /// Follows "field:index/field/..." from the owner; true if it ends on a field
    /// </summary>
    static bool ResolvePath(object owner, string path, out string why)
    {
        why = "";
        var current = owner;
        var segments = path.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            var parts = segments[i].Split(':');
            var field = Fields(current.GetType()).FirstOrDefault(f => string.Equals(f.Name, "m_" + parts[0], StringComparison.OrdinalIgnoreCase));
            if (field == null) { why = $"no field {parts[0]} on {current.GetType().Name}"; return false; }
            var value = field.GetValue(current);
            if (parts.Length > 1)
            {
                if (value is not IList list) { why = $"{parts[0]} not a list"; return false; }
                var idx = int.Parse(parts[1]);
                if (idx >= list.Count) { why = $"{parts[0]}:{idx} past end ({list.Count})"; return false; }
                value = list[idx];
            }
            if (i == segments.Length - 1) return true;
            if (value == null) { why = $"{segments[i]} null"; return false; }
            current = value;
        }
        return true;
    }

    // 0 node, 1 intermediate, 2 skipped (mirrors HavokBehaviorSymbols.GetNodeKind)
    static int OutlineKind(Type type)
    {
        if (type.Name is "StateInfo" or "hkbStateMachineStateInfo" or "hkbGeneratorTransitionEffect") return 0;
        if (type.Name is "hkbManualSelectorTransitionEffect") return 1;
        if (type.Name is "hkbVariableBindingSet" or "hkPropertyBag" or "hkbBehaviorGraphData" or "hkbBehaviorGraphStringData") return 2;
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name is "hkbTransitionEffect" or "hkbModifier" or "hkbCondition") return 2;
            if (t.Name == "hkbGenerator") return 0;
        }
        return 1;
    }

    static void CollectOutline(object obj, List<object> results, int depth, HashSet<object> seen)
    {
        foreach (var f in Fields(obj.GetType()))
        {
            if (f.FieldType == typeof(string) || f.FieldType.IsValueType) continue;
            var v = f.GetValue(obj);
            IEnumerable items = v is IList l ? l : new[] { v };
            foreach (var e in items)
            {
                if (!IsObj(e) || !seen.Add(e)) continue;
                var k = OutlineKind(e.GetType());
                if (k == 0) results.Add(e);
                else if (k == 1 && depth < 3) CollectOutline(e, results, depth + 1, seen);
            }
        }
    }

    static bool IsTransitionEffect(Type t)
    {
        for (var b = t; b != null; b = b.BaseType)
            if (b.Name == "hkbTransitionEffect")
                return true;
        return false;
    }

    static bool IsGenerator(Type t)
    {
        for (var b = t; b != null; b = b.BaseType)
            if (b.Name == "hkbGenerator")
                return true;
        return false;
    }

    object FindOwningSM(object obj, out string via)
    {
        via = "";
        var queue = new Queue<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { obj };
        queue.Enqueue(obj);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (!Parents.TryGetValue(cur, out var ps)) continue;
            foreach (var (p, f) in ps)
            {
                if (p.GetType().Name == "hkbStateMachine") { via = f; return p; }
                if (visited.Add(p)) { via = f; queue.Enqueue(p); }
            }
        }
        return null;
    }

    public void Print(bool detail)
    {
        foreach (var (k, v) in Counts)
        {
            if (!detail && k.StartsWith("type.")) continue;
            if (detail && k.StartsWith("type.") && v < 20) continue;
            Console.WriteLine($"  {k}: {v}");
        }
        foreach (var (k, list) in Examples)
            Console.WriteLine($"  [{k}] {string.Join(" | ", list)}");
    }
}
