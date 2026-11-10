using System.Text;

// Layout analyzer for dumped .bhv files: walks the pointer graph from TKGP's 010 template, records every region,
// and reports gaps, overlaps, shared targets, section order, and size distributions for the variable blobs.
public static class Analyze
{
    record Region(int Start, int Size, string Kind, string Key);

    public static void Run(string dumpRoot)
    {
        var files = Directory.GetFiles(dumpRoot, "*.bhv", SearchOption.AllDirectories);
        var stats = new SortedDictionary<string, SortedDictionary<string, int>>();
        void Stat(string cat, object val)
        {
            if (!stats.TryGetValue(cat, out var d)) stats[cat] = d = new();
            var k = val.ToString();
            d[k] = d.GetValueOrDefault(k) + 1;
        }

        int shown = 0;
        foreach (var path in files)
        {
            var b = File.ReadAllBytes(path);
            var rel = Path.GetRelativePath(dumpRoot, path);
            string fileKind = Path.GetFileName(path) switch { "basenormal.bhv" => "base", "weapon.bhv" => "weapon", _ => "w" };
            var regions = new List<Region>();
            var problems = new List<string>();
            short ver = BitConverter.ToInt16(b, 0);
            int I(int abs) => BitConverter.ToInt32(b, abs);
            short S(int abs) => BitConverter.ToInt16(b, abs);
            const int D0 = 0x20;

            Stat("version", $"{ver} {fileKind}");
            Stat("fileSize==len", I(4) == b.Length);
            int statesOff = I(0x20), stateCount = I(0x24), offB = I(0x28), countB = I(0x2C), offC = I(0x30);
            int countC = S(0x34), sizeC = S(0x36), offD = I(0x38), countD = I(0x3C);
            Stat($"sizeC v{ver}", $"{sizeC:X} cnt>0:{countC > 0}");
            int stateSize = ver >= 10 ? 0xA0 : 0x80;
            int sizeB = ver switch { 5 => 0x40, 6 => 0x50, _ => 0x60 };
            int daSize = ver >= 10 ? 0x30 : 0x20;

            regions.Add(new(0, 0x40, "header", ""));
            int preSize = D0 + statesOff - 0x40;
            Stat($"preamble size {fileKind}", $"{preSize:X}");
            if (preSize > 0) regions.Add(new(0x40, preSize, "preamble", ""));

            int[] stateData = new int[stateCount];
            int firstTrans = int.MaxValue;
            for (int i = 0; i < stateCount; i++)
            {
                int s = D0 + statesOff + i * stateSize;
                regions.Add(new(s, stateSize, "state", ""));
                stateData[i] = I(s + 4);
                int tOff = I(s + 8), tCount = I(s + 12);
                if (tCount > 0) firstTrans = Math.Min(firstTrans, tOff);
                for (int t = 0; t < tCount; t++)
                {
                    int ta = D0 + tOff + t * 0x20;
                    regions.Add(new(ta, 0x20, "transition", ""));
                    int cOff = I(ta + 4), cCount = I(ta + 8), abbOff = I(ta + 12);
                    for (int z = 0x10; z < 0x20; z += 4) if (I(ta + z) != 0) problems.Add($"trans unk{z:X} != 0");
                    Stat("transition cond count", cCount);
                    byte abbType = b[D0 + abbOff + 1];
                    regions.Add(new(D0 + abbOff, -1, "abb", abbType.ToString()));
                    for (int c = 0; c < cCount; c++)
                    {
                        int ca = D0 + cOff + c * 0x10;
                        regions.Add(new(ca, 0x10, "condition", ""));
                        regions.Add(new(D0 + I(ca + 4), -1, "conddata", b[ca].ToString()));
                        Stat("cond b1", b[ca + 1]); Stat("cond b2", b[ca + 2]); Stat("cond b3", b[ca + 3]);
                        Stat("cond b8", b[ca + 8]); Stat("cond b9", b[ca + 9]); Stat("cond bA", b[ca + 10]); Stat("cond bB", b[ca + 11]);
                        Stat("cond 0C", I(ca + 12));
                    }
                }
            }
            for (int i = 0; i < stateCount; i++)
                regions.Add(new(D0 + stateData[i], -1, "statedata", ""));

            for (int i = 0; i < countB; i++) regions.Add(new(D0 + offB + i * sizeB, sizeB, "B", ""));
            for (int i = 0; i < countC; i++) regions.Add(new(D0 + offC + i * sizeC, sizeC, "C", ""));
            for (int i = 0; i < countD; i++)
            {
                int da = D0 + offD + i * 0x20;
                regions.Add(new(da, 0x20, "D", ""));
                int aOff = I(da), aCount = I(da + 4);
                Stat("D unk08/0C/14/18/1C nonzero", I(da + 8) != 0 || I(da + 12) != 0 || I(da + 0x14) != 0 || I(da + 0x18) != 0 || I(da + 0x1C) != 0);
                for (int a = 0; a < aCount; a++) regions.Add(new(D0 + aOff + a * daSize, daSize, "DA", ""));
            }

            // Resolve open-ended blobs to the next region start; dedupe shared targets
            var starts = regions.Select(r => r.Start).Distinct().OrderBy(x => x).ToList();
            var fixedStarts = regions.Where(r => r.Size > 0).Select(r => r.Start).ToHashSet();
            int NextStart(int s) { int idx = starts.BinarySearch(s); return idx + 1 < starts.Count ? starts[idx + 1] : b.Length; }
            var resolved = new List<Region>();
            foreach (var g in regions.GroupBy(r => (r.Start, r.Kind)))
            {
                var r = g.First();
                if (g.Count() > 1) Stat($"shared {r.Kind}", g.Select(x => x.Key).Distinct().Count() > 1 ? "different keys" : "same key");
                int size = r.Size >= 0 ? r.Size : fixedStarts.Contains(r.Start) ? 0 : NextStart(r.Start) - r.Start;
                if (r.Size < 0) Stat($"{r.Kind} size v{ver}", $"{r.Key}:{size:X}");
                resolved.Add(r with { Size = size });
            }
            resolved = resolved.OrderBy(r => r.Start).ThenBy(r => r.Size == 0 ? 0 : 1).ToList();

            int pos = 0; var order = new List<string>();
            foreach (var r in resolved)
            {
                if (r.Start > pos) { problems.Add($"gap {pos:X}-{r.Start:X} ({r.Start - pos:X}) before {r.Kind}"); }
                else if (r.Start < pos && r.Size > 0) problems.Add($"overlap {r.Kind}@{r.Start:X} < {pos:X}");
                if (r.Size > 0 && (order.Count == 0 || order[^1] != r.Kind)) order.Add(r.Kind);
                pos = Math.Max(pos, r.Start + r.Size);
            }
            if (pos < b.Length && pos + 2 <= b.Length)
            {
                int cnt = BitConverter.ToInt16(b, pos); int sb = pos + 2 + cnt * 2; int end = sb;
                for (int i = 0; i < cnt && end < b.Length; i++) { int so = sb + BitConverter.ToInt16(b, pos + 2 + i * 2); if (so != end) problems.Add($"string {i} not packed"); while (end < b.Length && b[end] != 0) end++; end++; }
                if (end == b.Length) { Stat("strings", $"{fileKind} {cnt}"); pos = end; order.Add("strings"); }
            }
            if (pos < b.Length) problems.Add($"tail {pos:X}-{b.Length:X}: {Encoding.ASCII.GetString(b, pos, Math.Min(24, b.Length - pos)).Replace('\0', '.')}");
            Stat("order", string.Join(" ", order));
            foreach (var p in problems) Stat("problem", System.Text.RegularExpressions.Regex.Replace(p, "[0-9A-F]{2,}", "#"));
            if (problems.Count > 0 && shown++ < 15) Console.WriteLine($"{rel}: {string.Join("; ", problems.Take(4))}");
        }

        foreach (var (cat, d) in stats)
        {
            Console.WriteLine($"== {cat}");
            foreach (var (k, v) in d.OrderByDescending(kv => kv.Value).Take(40)) Console.WriteLine($"   {v,6}  {k}");
        }
    }
}
