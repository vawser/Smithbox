// SceneParamsAudit: checks every shader's copy of the sceneParams struct (and its helper structs sceneLight and
// mapDirectionalLight) against the full one in FlverShader.frag, by std140 layout, and optionally switches the
// compatible copies to a shared SceneParams.glsl include. See Documentation/Claude/Older Games/DES Light Set Redesign.md.
//
// Usage: SceneParamsAudit <Shaders dir> [--apply]
//   Writes report.md next to the executable's project folder (the current directory).

using System.Text;
using System.Text.RegularExpressions;

var shadersDir = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var apply = args.Contains("--apply");

if (args.Contains("--spirv"))
{
    SpirvCheck.Run(shadersDir);
    return;
}

if (args.Contains("--csharp"))
{
    CSharpCheck.Run(shadersDir);
    return;
}
var canonicalPath = Path.Combine(shadersDir, "FlverShader", "FlverShader.frag");
var includePath = Path.Combine(shadersDir, "SceneParams.glsl");

string[] helperNames = { "sceneLight", "mapDirectionalLight" };
const string MainName = "sceneParams";

var canonicalText = File.ReadAllText(canonicalPath);
var canonicalBlocks = new Dictionary<string, Block>();
foreach (var name in helperNames.Append(MainName))
{
    var b = Block.Find(canonicalText, name) ?? throw new Exception($"{name} not found in {canonicalPath}");
    canonicalBlocks[name] = b;
}

var structSizes = new Dictionary<string, (int Size, int Align)>();
foreach (var name in helperNames)
{
    var layout = Layout.Compute(canonicalBlocks[name].Members, structSizes);
    structSizes[name] = (Layout.RoundUp(layout.Size, 16), 16);
}
var canonical = Layout.Compute(canonicalBlocks[MainName].Members, structSizes);

var report = new StringBuilder();
report.AppendLine("# SceneParams audit");
report.AppendLine();
report.AppendLine($"Canonical: `FlverShader/FlverShader.frag`, {canonical.Fields.Count} members, {canonical.Size} bytes (std140).");
report.AppendLine();
report.AppendLine("| File | Members | Outcome | Notes |");
report.AppendLine("|---|---|---|---|");

var files = Directory.EnumerateFiles(shadersDir, "*.*", SearchOption.AllDirectories)
    .Where(f => f.EndsWith(".vert") || f.EndsWith(".frag") || f.EndsWith(".comp") || f.EndsWith(".geom"))
    .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".archive"))
    .OrderBy(f => f)
    .ToList();

int switchable = 0, manual = 0;
var toApply = new List<(string Path, string Text, Dictionary<string, string> Renames)>();

foreach (var file in files)
{
    var text = File.ReadAllText(file);
    var rel = Path.GetRelativePath(shadersDir, file).Replace('\\', '/');
    Block main;
    try
    {
        main = Block.Find(text, MainName);
    }
    catch (Exception e)
    {
        manual++;
        report.AppendLine($"| `{rel}` | ? | MANUAL | {e.Message.Replace("|", "\\|").Replace("\n", " ")} |");
        continue;
    }
    if (main == null)
        continue;

    var notes = new List<string>();
    var ok = true;

    // Helper structs the file defines must match the canonical ones, since the include brings its own
    var localSizes = new Dictionary<string, (int Size, int Align)>(structSizes);
    foreach (var name in helperNames)
    {
        var hb = Block.Find(text, name);
        if (hb == null)
            continue;
        var l = Layout.Compute(hb.Members, structSizes);
        var c = Layout.Compute(canonicalBlocks[name].Members, structSizes);
        if (!Layout.SameTypes(l, c))
        {
            ok = false;
            notes.Add($"`{name}` differs from the canonical one");
        }
    }

    var copy = Layout.Compute(main.Members, localSizes);
    var renames = new Dictionary<string, string>();
    var used = Usage.Find(text, main);

    // What matters is the members the shader reads: each must be a canonical member at the same offset with the same
    // type. Members it never reads are only padding in its copy, and the include leaves its reads unchanged.
    var canonicalByOffset = canonical.Fields.GroupBy(f => f.Offset).ToDictionary(g => g.Key, g => g.First());
    var unread = 0;

    foreach (var a in copy.Fields)
    {
        if (!used.Contains(a.Name))
        {
            unread++;
            continue;
        }

        if (!canonicalByOffset.TryGetValue(a.Offset, out var b))
        {
            ok = false;
            notes.Add($"reads `{a.Type} {a.Name}` at {a.Offset}, where the canonical struct has no member start");
            continue;
        }

        if (b.Type != a.Type || b.Count != a.Count)
        {
            ok = false;
            notes.Add($"reads `{a.Type} {a.Name}` at {a.Offset}, which is `{b.Type} {b.Name}` in the canonical struct");
            continue;
        }

        if (b.Name != a.Name)
        {
            renames[a.Name] = b.Name;
            notes.Add($"`{a.Name}` -> `{b.Name}`");
        }
    }

    foreach (var n in used)
    {
        if (copy.Fields.All(f => f.Name != n))
        {
            ok = false;
            notes.Add($"reads `sceneparam.{n}`, which its copy doesn't have");
        }
    }

    if (unread > 0)
        notes.Add($"{unread} unread");

    var outcome = ok ? (renames.Count > 0 ? "switch, renames" : "switch") : "MANUAL";
    if (ok) { switchable++; toApply.Add((file, text, renames)); }
    else manual++;

    report.AppendLine($"| `{rel}` | {copy.Fields.Count} | {outcome} | {string.Join("; ", notes)} |");
}

report.AppendLine();
report.AppendLine($"Switchable: {switchable}. Manual: {manual}.");

if (apply)
{
    File.WriteAllText(includePath, IncludeFile.Build(canonicalText, canonicalBlocks, helperNames, MainName));
    foreach (var (path, text, renames) in toApply)
        File.WriteAllText(path, Rewrite.Apply(text, path, includePath, helperNames, MainName, renames));
    report.AppendLine();
    report.AppendLine($"Applied: wrote `SceneParams.glsl` and rewrote {toApply.Count} files.");
}

File.WriteAllText("report.md", report.ToString());
Console.WriteLine(report.ToString());

// A struct definition in a shader: its text span and members, comments stripped
class Block
{
    public int Start;  // index of "struct"
    public int End;    // index after the closing "};"
    public List<(string Type, string Name, string Count)> Members = new();

    public static Block Find(string text, string name)
    {
        var m = Regex.Match(text, @"(?m)^[ \t]*struct\s+" + name + @"\s*\{");
        if (!m.Success)
            return null;
        var close = text.IndexOf("};", m.Index + m.Length, StringComparison.Ordinal);
        if (close < 0)
            return null;

        var body = text.Substring(m.Index + m.Length, close - (m.Index + m.Length));
        var directive = Regex.Match(body, @"(?m)^[ \t]*#\s*\w+[^\n]*");
        if (directive.Success)
            throw new Exception($"preprocessor line inside {name}: `{directive.Value.Trim()}`; its layout depends on the variant");
        body = Regex.Replace(body, @"//[^\n]*", "");
        body = Regex.Replace(body, @"/\*.*?\*/", "", RegexOptions.Singleline);

        var block = new Block { Start = m.Index, End = close + 2 };
        foreach (var raw in body.Split(';'))
        {
            var decl = Regex.Replace(raw, @"\s+", " ").Trim();
            if (decl.Length == 0)
                continue;
            var dm = Regex.Match(decl, @"^(?:(?:highp|mediump|lowp|layout\([^)]*\))\s+)*(\w+)\s+(.+)$");
            if (!dm.Success)
                throw new Exception($"Can't parse member '{decl}' of {name}");
            foreach (var part in dm.Groups[2].Value.Split(','))
            {
                var nm = Regex.Match(part.Trim(), @"^(\w+)\s*(?:\[\s*(\w+)\s*\])?$");
                if (!nm.Success)
                    throw new Exception($"Can't parse declarator '{part}' of {name}");
                block.Members.Add((dm.Groups[1].Value, nm.Groups[1].Value, nm.Groups[2].Success ? nm.Groups[2].Value : null));
            }
        }
        return block;
    }
}

record Field(string Type, string Name, int Count, int Offset, int Size);

class LayoutResult
{
    public List<Field> Fields = new();
    public int Size;
}

static class Layout
{
    public static int RoundUp(int v, int a) => (v + a - 1) / a * a;

    static (int Size, int Align) Basic(string type, Dictionary<string, (int, int)> structs)
    {
        switch (type)
        {
            case "float": case "int": case "uint": case "bool": return (4, 4);
            case "vec2": case "ivec2": case "uvec2": case "bvec2": return (8, 8);
            case "vec3": case "ivec3": case "uvec3": case "bvec3": return (12, 16);
            case "vec4": case "ivec4": case "uvec4": case "bvec4": return (16, 16);
            case "mat3": return (48, 16);
            case "mat4": return (64, 16);
        }
        if (structs.TryGetValue(type, out var s))
            return s;
        throw new Exception($"Unknown type '{type}'");
    }

    public static LayoutResult Compute(List<(string Type, string Name, string Count)> members, Dictionary<string, (int, int)> structs)
    {
        var r = new LayoutResult();
        var offset = 0;
        foreach (var (type, name, countText) in members)
        {
            var (size, align) = Basic(type, structs);
            var count = 1;
            if (countText != null)
            {
                // The one define the copies use for array lengths; every copy defines it as 16
                count = countText == "MAX_SCENE_LIGHTS" ? 16 : int.Parse(countText);
                align = 16;
                size = RoundUp(size, 16) * count;
            }
            offset = RoundUp(offset, align);
            r.Fields.Add(new Field(type, name, countText == null ? 0 : count, offset, size));
            offset += size;
        }
        r.Size = offset;
        return r;
    }

    public static bool SameTypes(LayoutResult a, LayoutResult b) =>
        a.Fields.Count == b.Fields.Count &&
        a.Fields.Zip(b.Fields).All(p => p.First.Type == p.Second.Type && p.First.Count == p.Second.Count);
}

static class Usage
{
    // The members read through the instance "sceneparam", outside the struct itself
    public static HashSet<string> Find(string text, Block main)
    {
        var outside = text.Remove(main.Start, main.End - main.Start);
        outside = Regex.Replace(outside, @"//[^\n]*", "");
        outside = Regex.Replace(outside, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return Regex.Matches(outside, @"\bsceneparam\s*\.\s*(\w+)").Select(m => m.Groups[1].Value).ToHashSet();
    }
}

static class IncludeFile
{
    public static string Build(string canonicalText, Dictionary<string, Block> blocks, string[] helpers, string main)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// The scene parameters every shader reads at set 0, binding 0, laid out as SceneParam.cs (std140), and the");
        sb.AppendLine("// light structs inside them. The one definition: shaders #include this rather than copying the struct, so a");
        sb.AppendLine("// field added here (and in SceneParam.cs, keeping its 16 byte rows) reaches all of them.");
        sb.AppendLine("#ifndef SCENE_PARAMS_GLSL");
        sb.AppendLine("#define SCENE_PARAMS_GLSL");
        sb.AppendLine();
        sb.AppendLine("#ifndef MAX_SCENE_LIGHTS");
        sb.AppendLine("#define MAX_SCENE_LIGHTS 16");
        sb.AppendLine("#endif");
        sb.AppendLine();
        foreach (var name in helpers.Append(main))
        {
            var b = blocks[name];
            sb.AppendLine(LeadingComment(canonicalText, b.Start) + canonicalText.Substring(b.Start, b.End - b.Start));
            sb.AppendLine();
        }
        sb.AppendLine("#endif");
        return sb.ToString().Replace("\r\n", "\n");
    }

    // The comment lines directly above a struct
    static string LeadingComment(string text, int start)
    {
        var lines = text.Substring(0, start).Replace("\r\n", "\n").Split('\n').ToList();
        lines.RemoveAt(lines.Count - 1); // the partial line the struct starts on (empty)
        var taken = new List<string>();
        for (var i = lines.Count - 1; i >= 0 && lines[i].TrimStart().StartsWith("//"); i--)
            taken.Insert(0, lines[i]);
        return taken.Count == 0 ? "" : string.Join("\n", taken) + "\n";
    }
}

static class Rewrite
{
    public static string Apply(string text, string path, string includePath, string[] helpers, string main,
        Dictionary<string, string> renames)
    {
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";

        // Remove the blocks (with their leading comment lines), last first, and note where the first one was
        var spans = new List<(int Start, int End)>();
        foreach (var name in helpers.Append(main))
        {
            var b = Block.Find(text, name);
            if (b == null)
                continue;
            spans.Add((CommentStart(text, b.Start), LineEnd(text, b.End)));
        }
        spans.Sort((x, y) => y.Start.CompareTo(x.Start));

        var first = spans.Min(s => s.Start);
        foreach (var (start, end) in spans)
            text = text.Remove(start, end - start);

        var rel = Path.GetRelativePath(Path.GetDirectoryName(path), includePath).Replace('\\', '/');
        text = text.Insert(first, $"#include \"{rel}\"{newline}");

        // Members renamed to the canonical names, through the instance only
        if (renames.Count > 0)
        {
            text = Regex.Replace(text, @"\bsceneparam(\s*\.\s*)(\w+)",
                m => renames.TryGetValue(m.Groups[2].Value, out var to) ? $"sceneparam{m.Groups[1].Value}{to}" : m.Value);
        }

        // glslangValidator needs the extension for #include; the variant compiler expands includes itself
        if (!text.Contains("GL_GOOGLE_include_directive"))
        {
            var v = Regex.Match(text, @"(?m)^#version[^\n]*\n");
            text = text.Insert(v.Index + v.Length, $"#extension GL_GOOGLE_include_directive : require{newline}");
        }

        // Collapse the blank lines the removed blocks leave behind
        text = Regex.Replace(text, @"(\r?\n){3,}", newline + newline);
        return text;
    }

    static int LineEnd(string text, int i)
    {
        var n = text.IndexOf('\n', i);
        return n < 0 ? text.Length : n + 1;
    }

    // The start of the comment lines directly above a struct, or of its own line
    static int CommentStart(string text, int start)
    {
        var lineStart = text.LastIndexOf('\n', Math.Max(0, start - 1)) + 1;
        while (lineStart > 0)
        {
            var prevStart = text.LastIndexOf('\n', Math.Max(0, lineStart - 2)) + 1;
            var line = text.Substring(prevStart, lineStart - prevStart).Trim();
            if (!line.StartsWith("//"))
                break;
            lineStart = prevStart;
        }
        return lineStart;
    }
}

// --spirv: for each compiled shader, the sceneParams member offsets in the working copy's .spv against the committed
// one (git HEAD). Every member the old struct had must start at the same offset in the new one, under its old name
// or, for a renamed member, any name; differences are listed so they can be matched against the source audit.
static class SpirvCheck
{
    public static void Run(string shadersDir)
    {
        var repo = Git("rev-parse --show-toplevel", shadersDir).Trim();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# SPIR-V sceneParams offsets, HEAD vs working copy");
        sb.AppendLine();
        sb.AppendLine("| File | Old members | New members | Result |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var file in Directory.EnumerateFiles(shadersDir, "*.spv", SearchOption.AllDirectories).OrderBy(f => f))
        {
            var rel = Path.GetRelativePath(repo, file).Replace(Path.DirectorySeparatorChar, '/');
            var oldBytes = GitBytes($"show HEAD:\"{rel}\"", repo);
            if (oldBytes == null || oldBytes.Length < 20)
                continue;

            var oldMembers = Members(oldBytes);
            var newMembers = Members(File.ReadAllBytes(file));
            if (oldMembers == null && newMembers == null)
                continue;
            if (oldMembers == null || newMembers == null)
            {
                sb.AppendLine($"| `{rel}` | {oldMembers?.Count ?? 0} | {newMembers?.Count ?? 0} | struct only in one |");
                continue;
            }

            var newByOffset = newMembers.GroupBy(m => m.Offset).ToDictionary(g => g.Key, g => g.First().Name);
            var issues = new List<string>();
            foreach (var (name, offset) in oldMembers)
            {
                if (!newByOffset.TryGetValue(offset, out var newName))
                    issues.Add($"`{name}`@{offset} no member there");
                else if (newName != name)
                    issues.Add($"`{name}`@{offset} is `{newName}`");
            }
            foreach (var (name, offset) in newMembers)
            {
                var old = oldMembers.FirstOrDefault(m => m.Name == name);
                if (old.Name != null && old.Offset != offset)
                    issues.Add($"`{name}` moved {old.Offset} -> {offset}");
            }

            sb.AppendLine($"| `{rel}` | {oldMembers.Count} | {newMembers.Count} | {(issues.Count == 0 ? "same" : string.Join("; ", issues))} |");
        }

        File.WriteAllText("spirv-report.md", sb.ToString());
        Console.WriteLine(sb.ToString());
    }

    // The members of the struct type named sceneParams (or structName), with their Offset decorations
    public static List<(string Name, int Offset)> Members(byte[] bytes, string structName = "sceneParams")
    {
        if (BitConverter.ToUInt32(bytes, 0) != 0x07230203)
            return null;
        var words = new uint[bytes.Length / 4];
        Buffer.BlockCopy(bytes, 0, words, 0, words.Length * 4);

        uint typeId = 0;
        var names = new Dictionary<(uint, uint), string>();
        var offsets = new Dictionary<(uint, uint), int>();

        for (var i = 5; i < words.Length;)
        {
            var count = (int)(words[i] >> 16);
            var op = words[i] & 0xFFFF;
            if (count == 0)
                break;
            if (op == 5 && Str(words, i + 2, count - 2) == structName)
                typeId = words[i + 1];
            else if (op == 6)
                names[(words[i + 1], words[i + 2])] = Str(words, i + 3, count - 3);
            else if (op == 72 && words[i + 3] == 35)
                offsets[(words[i + 1], words[i + 2])] = (int)words[i + 4];
            i += count;
        }

        if (typeId == 0)
            return null;
        return offsets.Where(o => o.Key.Item1 == typeId).OrderBy(o => o.Key.Item2)
            .Select(o => (names.TryGetValue(o.Key, out var n) ? n : $"#{o.Key.Item2}", o.Value)).ToList();
    }

    static string Str(uint[] words, int start, int count)
    {
        var b = new byte[count * 4];
        Buffer.BlockCopy(words, start * 4, b, 0, b.Length);
        var end = Array.IndexOf(b, (byte)0);
        return System.Text.Encoding.UTF8.GetString(b, 0, end < 0 ? b.Length : end);
    }

    static string Git(string args, string cwd)
    {
        var bytes = GitBytes(args, cwd);
        return bytes == null ? "" : System.Text.Encoding.UTF8.GetString(bytes);
    }

    static byte[] GitBytes(string args, string cwd)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("git", args)
        {
            WorkingDirectory = cwd, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false
        };
        using var p = System.Diagnostics.Process.Start(psi);
        using var ms = new MemoryStream();
        p.StandardOutput.BaseStream.CopyTo(ms);
        p.WaitForExit();
        return p.ExitCode == 0 ? ms.ToArray() : null;
    }
}

// --csharp: the C# structs uploaded to the GPU against the GLSL structs they fill. Every GLSL member must start where
// a C# field starts, and the sizes must agree once rounded to 16 bytes. Pairs: SceneParam.cs / SceneParams.glsl
// (std140) and SceneLighting.cs / SceneLighting.glsl (std430; its members are whole 16 byte rows, so std140 agrees).
static class CSharpCheck
{
    public static void Run(string shadersDir)
    {
        var repo = Path.GetFullPath(Path.Combine(shadersDir, "..", "..", ".."));
        var structsDir = Path.Combine(repo, "Smithbox.Program", "Renderer", "Structs");

        var glsl = File.ReadAllText(Path.Combine(shadersDir, "SceneParams.glsl")) + "\n" +
                   File.ReadAllText(Path.Combine(shadersDir, "SceneLighting.glsl"));
        var structs = new Dictionary<string, (int Size, int Align)>();
        foreach (var helper in new[] { "sceneLight", "mapDirectionalLight" })
        {
            var l = Layout.Compute(Block.Find(glsl, helper).Members, structs);
            structs[helper] = (Layout.RoundUp(l.Size, 16), 16);
        }

        var ok = true;
        ok &= Compare(glsl, "sceneParams", Path.Combine(structsDir, "SceneParam.cs"), "SceneParam", structs);
        ok &= Compare(glsl, "sceneLighting", Path.Combine(structsDir, "SceneLighting.cs"), "SceneLightingData", structs);

        // And what the compilers emitted: every compiled shader's offsets against the computed layout
        foreach (var structName in new[] { "sceneParams", "sceneLighting" })
        {
            var expected = Layout.Compute(Block.Find(glsl, structName).Members, structs).Fields
                .ToDictionary(f => f.Name, f => f.Offset);
            int checkedFiles = 0, bad = 0;
            foreach (var spv in Directory.EnumerateFiles(shadersDir, "*.spv", SearchOption.AllDirectories))
            {
                var members = SpirvCheck.Members(File.ReadAllBytes(spv), structName);
                if (members == null)
                    continue;
                checkedFiles++;
                foreach (var (name, offset) in members)
                {
                    if (!expected.TryGetValue(name, out var e) || e != offset)
                    {
                        bad++;
                        Console.WriteLine($"  {Path.GetFileName(spv)}: {structName}.{name} at {offset}, expected {(expected.ContainsKey(name) ? e : "no such member")}");
                    }
                }
            }
            Console.WriteLine($"SPIR-V {structName}: {checkedFiles} shaders, {bad} mismatches");
            ok &= bad == 0;
        }

        Console.WriteLine(ok ? "C# and GLSL layouts agree." : "LAYOUT MISMATCH");
        Environment.ExitCode = ok ? 0 : 1;
    }

    static bool Compare(string glsl, string glslName, string csPath, string csName, Dictionary<string, (int, int)> structs)
    {
        var g = Layout.Compute(Block.Find(glsl, glslName).Members, structs);
        var cs = CSharpFields(File.ReadAllText(csPath), csName);
        var starts = new Dictionary<int, string>();
        var offset = 0;
        foreach (var (name, size) in cs)
        {
            starts.TryAdd(offset, name);
            offset += size;
        }

        var ok = true;
        foreach (var f in g.Fields)
        {
            if (!starts.ContainsKey(f.Offset))
            {
                Console.WriteLine($"  {glslName}.{f.Name} at {f.Offset}: no C# field of {csName} starts there");
                ok = false;
            }
        }

        var glslSize = Layout.RoundUp(g.Size, 16);
        if (offset != glslSize)
        {
            Console.WriteLine($"  {csName} is {offset} bytes; {glslName} is {g.Size} ({glslSize} rounded to 16)");
            ok = false;
        }

        Console.WriteLine($"{csName} ({offset} bytes) / {glslName} ({g.Fields.Count} members): {(ok ? "agree" : "DIFFER")}");
        return ok;
    }

    // The instance fields of a sequential struct, in order, with their sizes
    static List<(string Name, int Size)> CSharpFields(string source, string name)
    {
        var m = Regex.Match(source, @"struct\s+" + name + @"\b[^{]*\{");
        var depth = 1;
        var i = m.Index + m.Length;
        var start = i;
        while (depth > 0)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}') depth--;
            i++;
        }
        var body = source.Substring(start, i - start - 1);
        body = Regex.Replace(body, @"//[^\n]*", "");

        var constants = new Dictionary<string, int>
        {
            ["SceneLightList.MaxLights"] = 16,
            ["SceneLightData.FloatCount"] = 16
        };

        var fields = new List<(string, int)>();
        foreach (Match f in Regex.Matches(body, @"public\s+(fixed\s+)?(\w+)\s+(\w+)\s*(?:\[([^\]]+)\])?\s*;"))
        {
            var type = f.Groups[2].Value;
            var size = type switch
            {
                "Matrix4x4" => 64, "Vector4" => 16, "Vector3" => 12, "Vector2" => 8,
                "float" or "int" or "uint" => 4,
                "MapDirectionalLightData" => 48,
                _ => throw new Exception($"Unknown C# type {type} in {name}")
            };
            if (f.Groups[4].Success)
            {
                var count = 1;
                foreach (var factor in f.Groups[4].Value.Split('*'))
                {
                    var t = factor.Trim();
                    count *= int.TryParse(t, out var n) ? n : constants[t];
                }
                size *= count;
            }
            fields.Add((f.Groups[3].Value, size));
        }
        return fields;
    }
}
