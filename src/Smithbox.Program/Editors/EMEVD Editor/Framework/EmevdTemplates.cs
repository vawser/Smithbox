using SoulsFormats;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>One place a template field's value is written: a line's arg at a byte offset, with the arg's type</summary>
public class EmevdTemplateUse
{
    public int Line { get; set; }
    public int Offset { get; set; }
    public EmevdArgType Type { get; set; }
}

/// <summary>A fill-in: a value written into one or more args (an entity ID used on five lines is one field)</summary>
public class EmevdTemplateField
{
    public string Name { get; set; }
    public EmevdArgRole Role { get; set; }
    /// <summary>What the role refers to: entity kinds, param name…, as the layouts give it</summary>
    public string RoleType { get; set; }
    /// <summary>The value the source holds; a double so f32 fields (a time, a distance) fit too (u32 IDs are exact)</summary>
    public double Default { get; set; }

    /// <summary>The field writes f32 args (all its uses are one kind: integer or float)</summary>
    [JsonIgnore]
    public bool IsFloat => Uses.Count > 0 && Uses[0].Type == EmevdArgType.F32;
    public List<EmevdTemplateUse> Uses { get; set; } = new();
}

public class EmevdTemplateLine
{
    public int Bank { get; set; }
    public int ID { get; set; }
    /// <summary>The arg bytes as hex</summary>
    public string Args { get; set; }
    public uint? Layer { get; set; }
}

public class EmevdTemplateParam
{
    public int Line { get; set; }
    public long Target { get; set; }
    public long Source { get; set; }
    public int Size { get; set; }
    public int UnkID { get; set; }
}

/// <summary>
/// A ready-made event pattern (P6 templates, Claude/Documentation/EMEVD Block Editor Analysis.md): lines with their
/// event parameters, and fill-in fields. Inserted as lines into the selected event (like a paste) or as a new event.
/// </summary>
public class EmevdTemplate
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    /// <summary>Built-ins: the vanilla event it was taken from ("m10_00_00_00 10002800")</summary>
    public string Source { get; set; }
    /// <summary>Built-ins: how many vanilla events share the pattern</summary>
    public int Copies { get; set; }
    public int RestBehavior { get; set; }
    public List<EmevdTemplateLine> Lines { get; set; } = new();
    public List<EmevdTemplateParam> Parameters { get; set; } = new();
    public List<EmevdTemplateField> Fields { get; set; } = new();

    /// <summary>Shipped with Smithbox (not saved in the project)</summary>
    [JsonIgnore]
    public bool BuiltIn { get; set; }

    /// <summary>Project templates: the file it was read from</summary>
    [JsonIgnore]
    public string FilePath { get; set; }
}

/// <summary>A game's built-in templates, as Assets/EMEVD/Templates/&lt;GAME&gt;.json holds them</summary>
public class EmevdTemplateSet
{
    public string Game { get; set; }
    public string Notes { get; set; }
    public List<EmevdTemplate> Templates { get; set; } = new();
}

/// <summary>
/// Builds template lines from field values, and templates from events: from a group of vanilla events of one shape
/// (the args that differ between them are the fields; EmevdEditorCheck templates writes the built-ins this way), or
/// from lines the user selects (args with a role are the fields). Pure, so EmevdEditorCheck runs the same code.
/// </summary>
public static class EmevdTemplates
{
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static EmevdTemplateSet LoadSet(string path)
    {
        var set = JsonSerializer.Deserialize<EmevdTemplateSet>(File.ReadAllText(path), Json) ?? new();
        foreach (var t in set.Templates)
            t.BuiltIn = true;
        return set;
    }

    public static EmevdTemplate Load(string path)
    {
        var t = JsonSerializer.Deserialize<EmevdTemplate>(File.ReadAllText(path), Json);
        if (t != null)
            t.FilePath = path;
        return t;
    }

    public static string Write(EmevdTemplate t) => JsonSerializer.Serialize(t, Json);

    public static string Write(EmevdTemplateSet set) => JsonSerializer.Serialize(set, Json);

    // ---------------------------------------------------------------- building

    /// <summary>
    /// The template's lines with the given field values written in (a missing value takes the field's default), with
    /// their parameters, ready to insert like pasted lines. Null when a value doesn't fit an arg it's written to; the
    /// problem says which.
    /// </summary>
    public static List<EmevdClipLine> Build(EmevdTemplate t, IReadOnlyList<double> values, out string problem)
    {
        problem = null;

        var lines = t.Lines.Select(l => new EmevdClipLine
        {
            Bank = l.Bank,
            ID = l.ID,
            Args = Convert.FromHexString(l.Args ?? ""),
            Layer = l.Layer,
        }).ToList();

        for (var f = 0; f < t.Fields.Count; f++)
        {
            var field = t.Fields[f];
            var value = values != null && f < values.Count ? values[f] : field.Default;

            foreach (var use in field.Uses)
            {
                if (use.Line < 0 || use.Line >= lines.Count || use.Offset + EmevdDecoded.SizeOf(use.Type) > lines[use.Line].Args.Length)
                {
                    problem = $"{field.Name}: use outside line {use.Line}";
                    return null;
                }

                byte[] bytes;
                if (use.Type == EmevdArgType.F32)
                {
                    bytes = EmevdDecoded.EncodeFloat((float)value);
                }
                else
                {
                    // A u32 takes a negative value by its 32-bit form, as everywhere else in the editor
                    var whole = (long)value;
                    var v = use.Type == EmevdArgType.U32 && whole < 0 && whole >= int.MinValue ? unchecked((uint)(int)whole) : whole;
                    if (whole != value || !EmevdDecoded.EncodeInt(use.Type, v, out bytes))
                    {
                        problem = $"{field.Name}: {value} doesn't fit {EmevdDecoded.TypeName(use.Type)}";
                        return null;
                    }
                }

                bytes.CopyTo(lines[use.Line].Args, use.Offset);
            }
        }

        foreach (var p in t.Parameters)
        {
            if (p.Line >= 0 && p.Line < lines.Count)
                lines[p.Line].Params.Add((p.Target, p.Source, p.Size, p.UnkID));
        }

        return lines;
    }

    /// <summary>The template as a whole new event</summary>
    public static EMEVD.Event MakeEvent(EmevdTemplate t, long id, IReadOnlyList<double> values, out string problem)
    {
        var lines = Build(t, values, out problem);
        if (lines == null)
            return null;

        var ev = new EMEVD.Event(id, (EMEVD.Event.RestBehaviorType)t.RestBehavior)
        {
            Instructions = lines.Select(l => l.Make()).ToList(),
        };

        for (var i = 0; i < lines.Count; i++)
        {
            foreach (var (target, source, size, unk) in lines[i].Params)
                ev.Parameters.Add(new EMEVD.Parameter(i, target, source, size) { UnkID = unk });
        }

        return ev;
    }

    // ---------------------------------------------------------------- making

    /// <summary>
    /// A built-in from vanilla events of one shape (same instructions, arg lengths and parameter layout; the first is
    /// the source): every decoded arg whose value differs between them is a field, and args whose values agree in
    /// every event are one field. Args a parameter writes, float args and the structure's own args (skip counts, goto
    /// labels) aren't fields.
    /// </summary>
    public static EmevdTemplate FromGroup(EmevdCatalog catalog, List<(string File, EMEVD.Event Ev)> members)
    {
        var (file, ev) = members[0];
        var t = FromEvent(ev, Enumerable.Range(0, ev.Instructions.Count).ToList());
        var script = EmevdScript.Build(catalog, ev);
        var scripts = members.Select(m => EmevdScript.Build(catalog, m.Ev)).ToList();

        // Candidate args, with their value in each member
        // Floats compare by their bits, so -0 and NaN payloads stay apart
        var columns = new List<(EmevdLine Line, EmevdArg Arg, long[] Values)>();
        foreach (var line in script.Lines.Where(l => l.Decoded.Fits))
        {
            foreach (var arg in line.Decoded.Args)
            {
                if (!Fillable(line, arg, floats: true))
                    continue;

                var values = new long[scripts.Count];
                var ok = true;
                for (var m = 0; m < scripts.Count && ok; m++)
                {
                    var other = scripts[m].Lines[line.Index];
                    var a = other.Decoded.Fits ? other.Decoded.Args.FirstOrDefault(x => x.Offset == arg.Offset) : default;
                    if (a.Def == null || a.Type != arg.Type) ok = false;
                    else values[m] = a.IsFloat ? BitConverter.SingleToInt32Bits(a.Float) : a.Int;
                }

                if (ok && values.Distinct().Count() > 1)
                    columns.Add((line, arg, values));
            }
        }

        // Args varying together (and of one kind, integer or float) are one field
        foreach (var group in columns.GroupBy(c => $"{c.Arg.IsFloat}|{string.Join(",", c.Values)}"))
            t.Fields.Add(Field(script, group.Select(c => (c.Line, c.Arg)).ToList()));

        t.Category = CategoryOf(script);
        t.Source = $"{file} {ev.ID}";
        t.Copies = members.Count;
        return t;
    }

    /// <summary>
    /// A template from some lines of an event (in this order), for Save as Template: their parameters come along, and
    /// every arg with a role (entity, flag, param row, text, map) is a field; args holding the same value with the
    /// same role are one field.
    /// </summary>
    public static EmevdTemplate FromLines(EmevdCatalog catalog, EmevdScript script, List<int> lines, string name)
    {
        var t = FromEvent(script.Event, lines);
        t.Name = name;
        t.Category = CategoryOf(script, lines);

        var picked = new List<(EmevdLine Line, EmevdArg Arg)>();
        foreach (var l in lines)
        {
            var line = script.Lines[l];
            if (!line.Decoded.Fits)
                continue;

            foreach (var arg in line.Decoded.Args)
            {
                if (Fillable(line, arg) && arg.Def.Role != EmevdArgRole.None)
                    picked.Add((line, arg));
            }
        }

        foreach (var group in picked.GroupBy(p => (p.Arg.Def.Role, p.Arg.Int)))
        {
            var field = Field(script, group.ToList(), lines);
            t.Fields.Add(field);
        }

        return t;
    }

    /// <summary>The lines (with their parameters, renumbered) of an event, no fields yet</summary>
    private static EmevdTemplate FromEvent(EMEVD.Event ev, List<int> lines)
    {
        var t = new EmevdTemplate { RestBehavior = (int)ev.RestBehavior };
        var index = new Dictionary<int, int>();

        foreach (var l in lines)
        {
            var instr = ev.Instructions[l];
            index[l] = t.Lines.Count;
            t.Lines.Add(new EmevdTemplateLine
            {
                Bank = instr.Bank,
                ID = instr.ID,
                Args = Convert.ToHexString(instr.ArgData ?? Array.Empty<byte>()),
                Layer = instr.Layer,
            });
        }

        foreach (var p in ev.Parameters)
        {
            if (index.TryGetValue((int)p.InstructionIndex, out var at))
                t.Parameters.Add(new EmevdTemplateParam { Line = at, Target = p.TargetStartByte, Source = p.SourceStartByte, Size = p.ByteCount, UnkID = p.UnkID });
        }

        return t;
    }

    /// <summary>An arg a field may write: decoded, not written by a parameter, not a skip count or goto label (and integer, unless floats)</summary>
    private static bool Fillable(EmevdLine line, EmevdArg arg, bool floats = false)
    {
        if ((arg.IsFloat && !floats) || line.Params.ContainsKey(arg.Offset))
            return false;

        return !(arg.Offset == line.FlowArgOffset && line.Kind is EmevdBlockKind.Skip or EmevdBlockKind.Goto);
    }

    /// <param name="order">The template's lines in the event (null: all of them, in order)</param>
    private static EmevdTemplateField Field(EmevdScript script, List<(EmevdLine Line, EmevdArg Arg)> uses, List<int> order = null)
    {
        var (first, arg) = uses[0];
        var withRole = uses.FirstOrDefault(u => u.Arg.Def.Role != EmevdArgRole.None);
        var named = withRole.Arg.Def != null ? withRole : uses[0];

        var names = uses.Select(u => u.Arg.Def.DisplayName).Distinct().ToList();
        var name = names.Count == 1 ? names[0] : $"{names[0]} / {names[1]}{(names.Count > 2 ? "…" : "")}";

        if (first.Kind == EmevdBlockKind.Call && arg.Offset == first.FlowArgOffset)
            name = $"{name} (called event)";

        return new EmevdTemplateField
        {
            Name = name,
            Role = named.Arg.Def.Role,
            RoleType = named.Arg.Def.RoleType,
            // Both arms as double: "IsFloat ? Float : Int" would be typed float and round IDs past 2^24
            Default = arg.IsFloat ? (double)arg.Float : (double)arg.Int,
            Uses = uses.Select(u => new EmevdTemplateUse
            {
                Line = order == null ? u.Line.Index : order.IndexOf(u.Line.Index),
                Offset = u.Arg.Offset,
                Type = u.Arg.Type,
            }).ToList(),
        };
    }

    /// <summary>
    /// A category from the instructions' names, for grouping the list (only display: nothing depends on it). Bosses,
    /// items, cutscenes, objects, characters, effects, events (calls only), or other.
    /// </summary>
    public static string CategoryOf(EmevdScript script, List<int> lines = null)
    {
        var names = (lines ?? Enumerable.Range(0, script.Lines.Count).ToList())
            .Select(l => script.Lines[l])
            .Where(l => l.Kind is EmevdBlockKind.Command or EmevdBlockKind.Call)
            .Select(l => l.Name)
            .ToList();

        bool Any(params string[] words) => names.Any(n => words.Any(w => n.Contains(w, StringComparison.OrdinalIgnoreCase)));

        if (Any("Boss"))
            return "Bosses";
        if (Any("Item", "Treasure", "Award"))
            return "Items";
        if (Any("Cutscene"))
            return "Cutscenes";
        if (Any("Bullet", "SFX", "SpEffect"))
            return "Effects";
        if (Any("Object", "Asset", "ObjAct"))
            return "Objects";
        if (Any("Character", "AI ", "AI", "Animation"))
            return "Characters";
        if (names.Count > 0 && names.All(n => n.StartsWith("Initialize", StringComparison.OrdinalIgnoreCase)))
            return "Event Calls";
        return "Other";
    }

    /// <summary>
    /// A name for a built-in: its most distinctive command (the one the game uses least, by the layouts' use counts),
    /// with the line count; e.g. "Handle Boss Defeat and Display Banner (10 lines)"
    /// </summary>
    public static string NameOf(EmevdScript script)
    {
        var commands = script.Lines
            .Where(l => l.Kind is EmevdBlockKind.Command && l.Decoded.Def != null && l.Decoded.Def.HasName)
            .OrderBy(l => l.Decoded.Def.Uses)
            .ThenBy(l => l.Index)
            .ToList();

        var main = commands.Count > 0 ? commands[0].Name
            : script.Lines.FirstOrDefault(l => l.Kind == EmevdBlockKind.Call)?.Name
            ?? script.Lines.FirstOrDefault()?.Name ?? "Event";

        return $"{main} ({script.Lines.Count} lines)";
    }

    /// <summary>The instructions of a template in brief, for its description: the commands, in order, without repeats</summary>
    public static string Outline(EmevdScript script)
    {
        return string.Join(", ", script.Lines
            .Where(l => l.Kind is EmevdBlockKind.Command or EmevdBlockKind.Call or EmevdBlockKind.Wait)
            .Select(l => l.Name)
            .Distinct()
            .Take(10));
    }
}
