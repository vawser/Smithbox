using SoulsFormats;
using System.Buffers.Binary;
using System.Text;

public static class Surveys
{
    // ---- R4: files --------------------------------------------------------------------------------------------
    public static void Files(GameMaterials g)
    {
        Console.WriteLine("== files (R4)");

        // What the Material Editor reads each binder as (MaterialBank: BND3 for these, else BND4)
        var editorFormat = g.Name is "DES" or "DS1" or "DS1R" ? "BND3" : "BND4";

        foreach (var b in g.Binders)
        {
            var mark = b.Selected ? "EDITOR " : "MISSED ";
            var formatNote = b.Format == "missing" ? "" : b.Format == editorFormat ? "" : $"  <-- editor reads as {editorFormat}";

            var total = 0;
            var unreadable = new List<string>();

            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                total++;
                try
                {
                    if (b.Kind == MaterialKind.MTD) MTD.Read(data); else MATBIN.Read(data);
                }
                catch (Exception e)
                {
                    unreadable.Add($"{file.Name}: {e.GetType().Name} {e.Message}");
                }
            }

            var others = b.Binder?.Files.Count - total ?? 0;
            Console.WriteLine($"  {mark}{b.Kind,-6} {b.Path}  [{b.Format}{(b.Raw != null && DCX.Is(b.Raw) ? "+DCX" : "")}] " +
                $"{total} files{(others > 0 ? $", {others} other" : "")}, {unreadable.Count} unreadable{formatNote}");

            foreach (var u in unreadable.Take(5))
                Console.WriteLine($"      unreadable {u}");
        }

        if (g.Binders.Count == 0)
            Console.WriteLine("  no material binders in the dictionary");
    }

    // ---- R2: dupes --------------------------------------------------------------------------------------------
    public static void Dupes(GameMaterials g)
    {
        Console.WriteLine("== dupes (R2)");

        foreach (var kind in new[] { MaterialKind.MTD, MaterialKind.MATBIN })
        {
            // short name -> (binder, file name, bytes), in load order
            var byName = new Dictionary<string, List<(string binder, string file, byte[] data, bool selected)>>(StringComparer.OrdinalIgnoreCase);

            foreach (var b in g.Binders.Where(b => b.Kind == kind))
            {
                foreach (var (file, data, _) in GameMaterials.Files(b))
                {
                    var key = GameMaterials.ShortName(file.Name);
                    if (!byName.TryGetValue(key, out var list))
                        byName[key] = list = new();
                    list.Add((b.Path, file.Name, data, b.Selected));
                }
            }

            var dupes = byName.Where(kv => kv.Value.Select(v => v.binder).Distinct().Count() > 1).ToList();
            var inBinder = byName.Where(kv => kv.Value.Count > kv.Value.Select(v => v.binder).Distinct().Count()).ToList();

            if (byName.Count == 0)
                continue;

            Console.WriteLine($"  {kind}: {byName.Count} short names, {dupes.Count} in more than one binder, {inBinder.Count} repeated within one binder");

            // Pattern of binder sets the duplicates fall in
            foreach (var set in dupes.GroupBy(kv => string.Join(" + ", kv.Value.Select(v => v.binder).Distinct())).OrderByDescending(s => s.Count()))
            {
                var differ = set.Count(kv => kv.Value.Select(v => Convert.ToHexString(System.Security.Cryptography.SHA1.HashData(v.data))).Distinct().Count() > 1);
                var editorBoth = set.First().Value.Count(v => v.selected);
                Console.WriteLine($"    {set.Count(),5} names in {set.Key}");
                Console.WriteLine($"          {differ} differ in content, {set.Count() - differ} identical; editor loads {editorBoth} of the copies");
                foreach (var ex in set.Where(kv => kv.Value.Select(v => Convert.ToHexString(System.Security.Cryptography.SHA1.HashData(v.data))).Distinct().Count() > 1).Take(3))
                    Console.WriteLine($"          e.g. {ex.Key}: {string.Join(" | ", ex.Value.Select(v => v.file))}");
            }

            foreach (var ex in inBinder.Take(3))
                Console.WriteLine($"    within one binder: {ex.Key}: {string.Join(" | ", ex.Value.Select(v => v.file))}");
        }
    }

    // ---- R3: round trip ---------------------------------------------------------------------------------------
    public static void RoundTrip(GameMaterials g)
    {
        Console.WriteLine("== roundtrip (R3)");

        foreach (var b in g.Binders.Where(b => b.Binder != null))
        {
            // Binder: read -> write -> decompress -> compare with the decompressed original
            string binderResult;
            try
            {
                var written = b.Binder is BND3 bnd3 ? bnd3.Write() : ((BND4)b.Binder).Write();
                var writtenData = DCX.Is(written) ? DCX.Decompress(written).ToArray() : written;
                binderResult = Compare(b.Data, writtenData);

                // Not byte-identical: do the files come back the same?
                if (binderResult != "identical")
                {
                    IBinder back = b.Binder is BND3 ? BND3.Read(writtenData) : BND4.Read(writtenData);
                    var changed = b.Binder.Files.Zip(back.Files).Count(p =>
                        p.First.Name != p.Second.Name || p.First.ID != p.Second.ID || p.First.Flags != p.Second.Flags ||
                        !p.First.Bytes.Span.SequenceEqual(p.Second.Bytes.Span));
                    var header = b.Binder is BND4 o4 && back is BND4 n4
                        ? $"; header version {o4.Version}->{n4.Version} format {o4.Format}->{n4.Format} unicode {o4.Unicode}->{n4.Unicode}"
                        : "";
                    binderResult += $" (read back: {back.Files.Count}/{b.Binder.Files.Count} files, {changed} changed{header})";
                }
            }
            catch (Exception e)
            {
                binderResult = $"write failed: {e.GetType().Name} {e.Message}";
            }

            var same = 0;
            var innerDcx = 0;
            var failures = new List<string>();
            var diffs = new List<string>();
            var fieldLoss = new List<string>();
            var float3Tails = new Dictionary<string, int>();

            // Why MATBINs differ, by file: Float3 without the two extra floats (the writer adds them),
            // and unreferenced data (the writer drops it)
            var plainFloat3Files = 0;
            var unreferencedFiles = 0;
            var unreferencedBytes = 0L;
            var unexplained = new List<string>();

            foreach (var (file, data, dcx) in GameMaterials.Files(b))
            {
                if (dcx) innerDcx++;

                byte[] written;
                string before, after;
                try
                {
                    if (b.Kind == MaterialKind.MTD)
                    {
                        var m = MTD.Read(data);
                        before = Inspect.Dump(m);
                        written = m.Write();
                        after = Inspect.Dump(MTD.Read(written));
                    }
                    else
                    {
                        var m = MATBIN.Read(data);
                        before = Inspect.Dump(m);
                        written = m.Write();
                        after = Inspect.Dump(MATBIN.Read(written));
                    }

                    if (DCX.Is(written)) written = DCX.Decompress(written).ToArray();
                }
                catch (Exception e)
                {
                    failures.Add($"{file.Name}: {e.GetType().Name} {e.Message}");
                    continue;
                }

                if (before != after)
                    fieldLoss.Add(file.Name);

                var result = Compare(data, written);
                if (result == "identical")
                {
                    same++;
                }
                else
                {
                    diffs.Add($"{file.Name}: {result}");

                    if (b.Kind == MaterialKind.MATBIN)
                    {
                        var (_, plain, unref) = Inspect.Classify(data);
                        if (plain > 0) plainFloat3Files++;
                        if (unref > 0) { unreferencedFiles++; unreferencedBytes += unref; }
                        if (plain == 0 && unref == 0) unexplained.Add($"{file.Name}: {result}");
                    }
                }

                if (b.Kind == MaterialKind.MATBIN)
                    Inspect.Classify(data, float3Tails);
            }

            var total = same + diffs.Count + failures.Count;
            Console.WriteLine($"  {b.Kind,-6} {b.Path}: binder {binderResult}; files {same}/{total} byte-identical, " +
                $"{diffs.Count} differ, {failures.Count} fail, {fieldLoss.Count} change a field when read back" +
                $"{(innerDcx > 0 ? $", {innerDcx} inner DCX" : "")}");

            foreach (var d in diffs.Take(3)) Console.WriteLine($"      differ {d}");
            foreach (var f in failures.Take(4)) Console.WriteLine($"      fail   {f}");
            foreach (var f in fieldLoss.Take(4)) Console.WriteLine($"      FIELD CHANGE {f}");

            if (b.Kind == MaterialKind.MATBIN && diffs.Count > 0)
            {
                Console.WriteLine($"      of those: {plainFloat3Files} have a Float3 stored as 3 floats (writer adds 8 bytes), " +
                    $"{unreferencedFiles} hold unreferenced data ({unreferencedBytes} non-zero bytes, dropped), {unexplained.Count} neither");
                foreach (var u in unexplained.Take(3)) Console.WriteLine($"      unexplained {u}");
            }

            if (float3Tails.Count > 0)
                Console.WriteLine($"      Float3 extra floats as stored (value -> count): {string.Join(", ", float3Tails.OrderByDescending(kv => kv.Value).Take(6).Select(kv => $"{kv.Key} x{kv.Value}"))}");
        }
    }

    private static string Compare(byte[] a, byte[] b)
    {
        var n = Math.Min(a.Length, b.Length);
        for (var i = 0; i < n; i++)
        {
            if (a[i] != b[i])
                return $"first diff at 0x{i:X} (len {a.Length} -> {b.Length})";
        }

        return a.Length == b.Length ? "identical" : $"length {a.Length} -> {b.Length}";
    }

    /// <summary>
    /// The two floats after a Float3 param's three, read from the file: the reader drops them, the writer writes 1, 1
    /// </summary>
    private static void CountFloat3Tails(byte[] data, Dictionary<string, int> counts)
    {
        var paramCount = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(0x1C));

        for (var i = 0; i < paramCount; i++)
        {
            var at = 0x38 + i * 0x28;
            var valueOffset = (int)BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(at + 8));
            var type = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(at + 0x14));

            if (type != (uint)MATBIN.ParamType.Float3 || valueOffset + 20 > data.Length)
                continue;

            var f3 = BitConverter.ToSingle(data, valueOffset + 12);
            var f4 = BitConverter.ToSingle(data, valueOffset + 16);
            var key = $"({f3}, {f4})";
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
    }

    // ---- R1: keys ---------------------------------------------------------------------------------------------
    public static void Keys(GameMaterials g)
    {
        var matbins = new List<(string binder, string file, MATBIN m)>();

        foreach (var b in g.Binders.Where(b => b.Kind == MaterialKind.MATBIN))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                try { matbins.Add((b.Path, file.Name, MATBIN.Read(data))); }
                catch { }
            }
        }

        if (matbins.Count == 0)
            return;

        Console.WriteLine($"== keys (R1): {matbins.Count} MATBINs");

        // MATBIN.Key against the strings a material is known by
        var fileInputs = new Dictionary<string, Func<(string binder, string file, MATBIN m), string>>
        {
            ["binder file name"] = x => x.file,
            ["file name"] = x => Path.GetFileName(x.file.Replace('\\', '/')),
            ["short name"] = x => GameMaterials.ShortName(x.file),
            ["SourcePath"] = x => x.m.SourcePath,
            ["SourcePath file"] = x => Path.GetFileName(x.m.SourcePath.Replace('\\', '/')),
            ["SourcePath stem"] = x => GameMaterials.ShortName(x.m.SourcePath),
            ["ShaderPath"] = x => x.m.ShaderPath,
            ["ShaderPath stem"] = x => GameMaterials.ShortName(x.m.ShaderPath),
        };

        ReportKey("MATBIN.Key", matbins.Select(x => (x.m.Key, (Func<string, string>)(name => fileInputs[name](x)))).ToList(), fileInputs.Keys.ToList());
        ReportRule("MATBIN.Key", matbins.Select(x => (x.m.Key, GameMaterials.ShortName(x.file))).ToList());

        var paramInputs = new List<string> { "Name" };
        var parms = matbins.SelectMany(x => x.m.Params.Select((p, i) => (p, i, x))).ToList();
        ReportKey("Param.Key", parms.Select(t => (t.p.Key, (Func<string, string>)(_ => t.p.Name))).ToList(), paramInputs);
        ReportRule("Param.Key", parms.Select(t => (t.p.Key, t.p.Name)).ToList());
        ReportIndex("Param.Key", parms.Select(t => (t.p.Key, t.i)).ToList());
        ReportSameInShader("Param.Key", parms.Select(t => (t.p.Key, $"{t.x.m.ShaderPath}|{t.p.Name}")).ToList());

        var samplerInputs = new List<string> { "Type", "Path" };
        var samplers = matbins.SelectMany(x => x.m.Samplers.Select((s, i) => (s, i, x))).ToList();
        ReportKey("Sampler.Key", samplers.Select(t => (t.s.Key, (Func<string, string>)(name => name == "Type" ? t.s.Type : t.s.Path))).ToList(), samplerInputs);
        ReportRule("Sampler.Key", samplers.Select(t => (t.s.Key, t.s.Type)).ToList());
        ReportIndex("Sampler.Key", samplers.Select(t => (t.s.Key, t.i)).ToList());

        var unk14 = samplers.GroupBy(t => t.s.Unk14).OrderByDescending(gr => gr.Count()).Take(5);
        Console.WriteLine($"  Sampler.Unk14 commonest: {string.Join(", ", unk14.Select(gr => $"{gr.Key} x{gr.Count()}"))}");
    }

    private static void ReportKey(string field, List<(uint key, Func<string, string> input)> items, List<string> inputs)
    {
        var zero = items.Count(i => i.key == 0);
        var distinct = items.Select(i => i.key).Distinct().Count();
        Console.WriteLine($"  {field}: {items.Count} values, {distinct} distinct, {zero} zero; samples {string.Join(" ", items.Take(4).Select(i => $"{i.key:X8}"))}");

        var any = false;
        foreach (var input in inputs)
        {
            foreach (var (hashName, hash) in Hashes.All)
            {
                foreach (var lower in new[] { false, true })
                {
                    var matches = items.Count(i =>
                    {
                        var text = i.input(input) ?? "";
                        return hash(lower ? text.ToLowerInvariant() : text) == i.key;
                    });

                    if (matches > 0)
                    {
                        any = true;
                        Console.WriteLine($"    {matches,6}/{items.Count} = {hashName}({input}{(lower ? ", lower" : "")})");
                    }
                }
            }
        }

        if (!any)
            Console.WriteLine("    no candidate hash matches");
    }

    /// <summary>
    /// Whether one name always has one key, and one key one name
    /// </summary>
    private static void ReportRule(string field, List<(uint key, string name)> items)
    {
        var namesWithManyKeys = items.GroupBy(i => i.name, StringComparer.OrdinalIgnoreCase).Count(gr => gr.Select(i => i.key).Distinct().Count() > 1);
        var keysWithManyNames = items.GroupBy(i => i.key).Count(gr => gr.Select(i => i.name.ToLowerInvariant()).Distinct().Count() > 1);
        var names = items.Select(i => i.name.ToLowerInvariant()).Distinct().Count();
        Console.WriteLine($"    {names} names: {namesWithManyKeys} have more than one key, {keysWithManyNames} keys are shared by different names");

        foreach (var gr in items.GroupBy(i => i.name, StringComparer.OrdinalIgnoreCase).Where(gr => gr.Select(i => i.key).Distinct().Count() > 1).Take(2))
            Console.WriteLine($"      e.g. {gr.Key}: {string.Join(" ", gr.Select(i => $"{i.key:X8}").Distinct().Take(6))}");
    }

    private static void ReportIndex(string field, List<(uint key, int index)> items)
    {
        var equal = items.Count(i => i.key == i.index);
        if (equal > 0)
            Console.WriteLine($"    {equal}/{items.Count} keys equal the entry's index");
    }

    private static void ReportSameInShader(string field, List<(uint key, string shaderAndName)> items)
    {
        var groups = items.GroupBy(i => i.shaderAndName, StringComparer.OrdinalIgnoreCase).ToList();
        var stable = groups.Count(gr => gr.Select(i => i.key).Distinct().Count() == 1);
        Console.WriteLine($"    per (shader, name): {stable}/{groups.Count} have a single key");
    }

    // ---- ids --------------------------------------------------------------------------------------------------
    public static void Ids(GameMaterials g)
    {
        Console.WriteLine("== ids");

        foreach (var b in g.Binders.Where(b => b.Binder != null))
        {
            var files = b.Binder.Files;
            var sequential = files.Select((f, i) => f.ID == i).All(x => x);
            var startsAt = files.Count > 0 ? files[0].ID : -1;
            var contiguous = files.Zip(files.Skip(1)).All(p => p.Second.ID == p.First.ID + 1);
            var sortedByName = files.Zip(files.Skip(1)).All(p => string.CompareOrdinal(p.First.Name, p.Second.Name) <= 0);
            var sortedById = files.Zip(files.Skip(1)).All(p => p.First.ID < p.Second.ID);
            var flags = string.Join(", ", files.GroupBy(f => f.Flags).Select(gr => $"{gr.Key} x{gr.Count()}"));
            var compression = string.Join(", ", files.GroupBy(f => f.CompressionType).Select(gr => $"{gr.Key} x{gr.Count()}"));

            var header = b.Binder switch
            {
                BND4 b4 => $"BND4 version {b4.Version} format {b4.Format} unicode {b4.Unicode} extended {b4.Extended} bigEndian {b4.BigEndian}",
                BND3 b3 => $"BND3 version {b3.Version} format {b3.Format} bigEndian {b3.BigEndian}",
                _ => "?"
            };

            Console.WriteLine($"  {b.Path}: {files.Count} files, {header}");
            Console.WriteLine($"      IDs start {startsAt}, contiguous {contiguous}, = index {sequential}, sorted by ID {sortedById}, sorted by name {sortedByName}");
            Console.WriteLine($"      flags {flags}; compression {compression}");
            foreach (var f in files.Take(2).Concat(files.TakeLast(1)))
                Console.WriteLine($"      {f.ID,6} {f.Name}");
        }
    }
}

/// <summary>
/// Candidate hashes for the MATBIN keys
/// </summary>
public static class Hashes
{
    public static readonly (string, Func<string, uint>)[] All =
    [
        ("Adler-32", s => Adler32(Encoding.UTF8.GetBytes(s))),
        ("Adler-32 UTF-16", s => Adler32(Encoding.Unicode.GetBytes(s))),
        ("FromSoft32 (x37)", s => Mul(s, 37)),
        ("FromSoft64 (x133) low", s => (uint)Mul64(s, 133)),
        ("FromSoft64 (x133) high", s => (uint)(Mul64(s, 133) >> 32)),
        ("FNV-1a 32", s => Fnv1a(Encoding.UTF8.GetBytes(s))),
        ("FNV-1a 32 UTF-16", s => Fnv1a(Encoding.Unicode.GetBytes(s))),
        ("FNV-1 32", s => Fnv1(Encoding.UTF8.GetBytes(s))),
        ("CRC32", s => Crc32(Encoding.UTF8.GetBytes(s))),
        ("CRC32 UTF-16", s => Crc32(Encoding.Unicode.GetBytes(s))),
        ("djb2", s => Djb2(s)),
        ("sdbm", s => Sdbm(s)),
    ];

    public static uint Adler32(byte[] b)
    {
        uint a = 1, s = 0;
        foreach (var x in b) { a = (a + x) % 65521; s = (s + a) % 65521; }
        return (s << 16) | a;
    }

    private static uint Mul(string s, uint m) { uint h = 0; foreach (var c in s) h = h * m + c; return h; }
    private static ulong Mul64(string s, ulong m) { ulong h = 0; foreach (var c in s) h = h * m + c; return h; }
    private static uint Fnv1a(byte[] b) { uint h = 2166136261; foreach (var x in b) { h ^= x; h *= 16777619; } return h; }
    private static uint Fnv1(byte[] b) { uint h = 2166136261; foreach (var x in b) { h *= 16777619; h ^= x; } return h; }
    private static uint Djb2(string s) { uint h = 5381; foreach (var c in s) h = h * 33 + c; return h; }
    private static uint Sdbm(string s) { uint h = 0; foreach (var c in s) h = c + (h << 6) + (h << 16) - h; return h; }

    private static readonly uint[] CrcTable = Enumerable.Range(0, 256).Select(n =>
    {
        var c = (uint)n;
        for (var k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
        return c;
    }).ToArray();

    private static uint Crc32(byte[] b)
    {
        var c = 0xFFFFFFFF;
        foreach (var x in b) c = CrcTable[(c ^ x) & 0xFF] ^ (c >> 8);
        return c ^ 0xFFFFFFFF;
    }
}
