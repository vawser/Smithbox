using SoulsFormats;
using System.Buffers.Binary;
using System.Text;

/// <summary>
/// Looks inside files that don't round trip, and at key values, to explain the survey's numbers.
///   inspect GAME   with MATSURVEY_FILE=substring of a binder file name: layout and the bytes no field covers
///   keydump GAME   key values beside simple sums of their strings
/// </summary>
public static class Inspect
{
    public static void Run(GameMaterials g, string fileFilter)
    {
        foreach (var b in g.Binders)
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                if (!file.Name.Contains(fileFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                Console.WriteLine($"== {file.Name} ({data.Length} bytes)");

                if (b.Kind == MaterialKind.MATBIN)
                    Matbin(data);
                else
                    Mtd(data);

                return;
            }
        }

        Console.WriteLine("no file matched MATSURVEY_FILE");
    }

    /// <summary>
    /// Bytes of a MATBIN no field points at: Float3 params stored with the two extra floats, and other
    /// non-zero bytes (unreferenced data, which a re-write drops)
    /// </summary>
    public static (int float3Tails, int float3Plain, int unreferencedBytes) Classify(byte[] d, Dictionary<string, int> tailValues = null)
    {
        long I64(int at) => BinaryPrimitives.ReadInt64LittleEndian(d.AsSpan(at));
        int I32(int at) => BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(at));

        var covered = new bool[d.Length];
        void Cover(long from, long length) { for (var i = from; i < from + length && i < d.Length; i++) covered[i] = true; }
        int Utf16Length(long at) { var i = (int)at; while (i + 1 < d.Length && (d[i] != 0 || d[i + 1] != 0)) i += 2; return i - (int)at + 2; }

        var paramCount = I32(0x1C);
        var samplerCount = I32(0x20);
        Cover(0, 0x38);
        Cover(I64(0x8), Utf16Length(I64(0x8)));
        Cover(I64(0x10), Utf16Length(I64(0x10)));

        var float3Values = new List<long>();

        for (var i = 0; i < paramCount; i++)
        {
            var at = 0x38 + i * 0x28;
            Cover(at, 0x28);
            Cover(I64(at), Utf16Length(I64(at)));

            var type = (MATBIN.ParamType)BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(at + 0x14));
            var size = type switch
            {
                MATBIN.ParamType.Bool => 1, MATBIN.ParamType.Int or MATBIN.ParamType.Float => 4,
                MATBIN.ParamType.Int2 or MATBIN.ParamType.Float2 => 8, MATBIN.ParamType.Float3 => 12,
                MATBIN.ParamType.Float4 => 16, MATBIN.ParamType.Float5 => 20, _ => 0
            };
            Cover(I64(at + 8), size);

            if (type == MATBIN.ParamType.Float3)
                float3Values.Add(I64(at + 8));
        }

        for (var i = 0; i < samplerCount; i++)
        {
            var at = 0x38 + paramCount * 0x28 + i * 0x30;
            Cover(at, 0x30);
            Cover(I64(at), Utf16Length(I64(at)));
            Cover(I64(at + 8), Utf16Length(I64(at + 8)));
        }

        // A Float3 has its two extra floats when the 8 bytes after its three are not another field
        var tails = 0;
        foreach (var v in float3Values)
        {
            var tail = v + 12;
            if (tail + 8 <= d.Length && Enumerable.Range((int)tail, 8).All(i => !covered[i]))
            {
                tails++;
                if (tailValues != null) { var k = $"({BitConverter.ToSingle(d, (int)tail)}, {BitConverter.ToSingle(d, (int)tail + 4)})"; tailValues[k] = tailValues.GetValueOrDefault(k) + 1; }
                Cover(tail, 8);
            }
        }

        var unreferenced = Enumerable.Range(0, d.Length).Count(i => !covered[i] && d[i] != 0);

        return (tails, float3Values.Count - tails, unreferenced);
    }

    /// <summary>
    /// Every field of a material as text, to compare what a re-write keeps
    /// </summary>
    public static string Dump(MATBIN m)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{m.ShaderPath}|{m.SourcePath}|{m.Key}");
        foreach (var p in m.Params)
            sb.AppendLine($"P {p.Name}|{p.Type}|{p.Key}|{ValueText(p.Value)}");
        foreach (var s in m.Samplers)
            sb.AppendLine($"S {s.Type}|{s.Path}|{s.Key}|{s.Unk14}");
        return sb.ToString();
    }

    public static string Dump(MTD m)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{m.ShaderPath}|{m.Description}");
        foreach (var p in m.Params)
            sb.AppendLine($"P {p.Name}|{p.Type}|{ValueText(p.Value)}");
        foreach (var t in m.Textures)
            sb.AppendLine($"T {t.Type}|{t.Extended}|{t.UVNumber}|{t.ShaderDataIndex}|{t.Path}|{string.Join(",", t.UnkFloats)}");
        return sb.ToString();
    }

    private static string ValueText(object v) => v is Array a ? string.Join(",", a.Cast<object>()) : $"{v}";

    private static void Matbin(byte[] d)
    {
        long I64(int at) => BinaryPrimitives.ReadInt64LittleEndian(d.AsSpan(at));
        int I32(int at) => BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(at));

        var covered = new bool[d.Length];
        void Cover(long from, long length) { for (var i = from; i < from + length && i < d.Length; i++) covered[i] = true; }
        int Utf16Length(long at) { var i = (int)at; while (i + 1 < d.Length && (d[i] != 0 || d[i + 1] != 0)) i += 2; return i - (int)at + 2; }

        var paramCount = I32(0x1C);
        var samplerCount = I32(0x20);
        Cover(0, 0x38);

        Console.WriteLine($"  shader @{I64(0x8):X} source @{I64(0x10):X} key {(uint)I32(0x18):X8} params {paramCount} samplers {samplerCount}");
        Cover(I64(0x8), Utf16Length(I64(0x8)));
        Cover(I64(0x10), Utf16Length(I64(0x10)));

        var m = MATBIN.Read(d);

        for (var i = 0; i < paramCount; i++)
        {
            var at = 0x38 + i * 0x28;
            Cover(at, 0x28);
            var name = I64(at);
            var value = I64(at + 8);
            Cover(name, Utf16Length(name));

            var type = (MATBIN.ParamType)BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(at + 0x14));
            var size = type switch
            {
                MATBIN.ParamType.Bool => 1, MATBIN.ParamType.Int or MATBIN.ParamType.Float => 4,
                MATBIN.ParamType.Int2 or MATBIN.ParamType.Float2 => 8, MATBIN.ParamType.Float3 => 12,
                MATBIN.ParamType.Float4 => 16, MATBIN.ParamType.Float5 => 20, _ => 0
            };
            Cover(value, size);

            if (i < 6)
                Console.WriteLine($"  param {i} name @{name:X} value @{value:X} {type} {m.Params[i].Name}");
        }

        for (var i = 0; i < samplerCount; i++)
        {
            var at = 0x38 + paramCount * 0x28 + i * 0x30;
            Cover(at, 0x30);
            Cover(I64(at), Utf16Length(I64(at)));
            Cover(I64(at + 8), Utf16Length(I64(at + 8)));
        }

        // Runs of bytes nothing points at
        var gaps = new List<(int from, int to)>();
        for (var i = 0; i < d.Length; i++)
        {
            if (covered[i]) continue;
            var start = i;
            while (i < d.Length && !covered[i]) i++;
            gaps.Add((start, i));
        }

        var nonZero = gaps.Where(gp => d.Skip(gp.from).Take(gp.to - gp.from).Any(x => x != 0)).ToList();
        Console.WriteLine($"  {gaps.Count} uncovered runs, {gaps.Sum(gp => gp.to - gp.from)} bytes, {nonZero.Count} runs not all zero");

        foreach (var (from, to) in nonZero.Take(8))
        {
            var bytes = d.AsSpan(from, Math.Min(to - from, 96)).ToArray();
            var text = Encoding.Unicode.GetString(bytes.Length % 2 == 0 ? bytes : bytes[..^1]).Replace('\0', '·');
            Console.WriteLine($"    @{from:X}-{to:X} ({to - from}): {Convert.ToHexString(bytes[..Math.Min(32, bytes.Length)])} \"{text}\"");
        }

        // Strings the file holds more than once (the writer stores each once per field)
        Console.WriteLine($"  re-written length {m.Write().Length}");
    }

    private static void Mtd(byte[] d)
    {
        var w = MTD.Read(d).Write();
        Console.WriteLine($"  re-written length {w.Length}");

        // Every run of differing bytes, so a shifted block shows as where it starts
        var shown = 0;
        for (var i = 0; i < Math.Min(d.Length, w.Length) && shown < 8; i++)
        {
            if (d[i] == w[i]) continue;
            var start = i;
            while (i < Math.Min(d.Length, w.Length) && d[i] != w[i]) i++;
            shown++;
            var from = Math.Max(0, start - 8);
            Console.WriteLine($"  diff @{start:X}-{i:X}");
            Console.WriteLine($"    file  {Convert.ToHexString(d.AsSpan(from, Math.Min(40, d.Length - from)))}");
            Console.WriteLine($"    write {Convert.ToHexString(w.AsSpan(from, Math.Min(40, w.Length - from)))}");
            Console.WriteLine($"    text  \"{Encoding.ASCII.GetString(d, from, Math.Min(40, d.Length - from)).Replace('\0', '·')}\"");
        }
    }

    public static void KeyDump(GameMaterials g)
    {
        var shown = 0;
        foreach (var b in g.Binders.Where(b => b.Kind == MaterialKind.MATBIN))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                var m = MATBIN.Read(data);
                if (shown < 6)
                {
                    Describe("MATBIN", m.Key, ("file", file.Name), ("source", m.SourcePath), ("short", GameMaterials.ShortName(file.Name)));
                    foreach (var p in m.Params.Take(2)) Describe("  param", p.Key, ("name", p.Name));
                    foreach (var s in m.Samplers.Take(2)) Describe("  sampler", s.Key, ("type", s.Type));
                }
                shown++;
            }
        }
    }

    private static void Describe(string what, uint key, params (string label, string text)[] inputs)
    {
        Console.WriteLine($"{what} key {key:X8} (hi {key >> 16}, lo {key & 0xFFFF})");
        foreach (var (label, text) in inputs)
        {
            var sum = text.Sum(c => (int)c);
            var sumLower = text.ToLowerInvariant().Sum(c => (int)c);
            var weighted = text.Select((c, i) => (i + 1) * c).Sum();
            Console.WriteLine($"    {label} \"{text}\" len {text.Length} sum {sum} sumLower {sumLower} weighted {weighted & 0xFFFF}");
        }
    }
}
