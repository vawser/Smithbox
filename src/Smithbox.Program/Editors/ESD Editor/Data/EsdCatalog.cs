using StudioCore.Application;
using System.Text.Json;

namespace StudioCore.Editors.EsdEditor;

/// <summary>One documented argument of an ESD command or function</summary>
public class EsdArgDef
{
    public string Name;
    /// int, bool, float, string or enum
    public string Type;
    /// What the value refers to (ESDLang's namespace): Talk, EventFlag, Shop, Item, ItemType, Machine, Lot...
    public string Role;
    public string Enum;
    public bool Optional;
    public string Comment;
}

/// <summary>A command (bank 1) or function of one script kind</summary>
public class EsdCallDef
{
    public int Bank;
    public int Id;
    public string Name;
    public List<string> OldNames = new();
    public List<EsdArgDef> Args = new();
    public string Comment;
    /// Used by a vanilla file of this game (ESDLang's games_used)
    public bool Seen;

    // Functions only
    public string ReturnName;
    public string ReturnType;
    public string ReturnEnum;
    public bool BinaryReturn;

    public int RequiredArgs => Args.TakeWhile(a => !a.Optional).Count();
}

public class EsdEnumDef
{
    public string Name;
    public Dictionary<long, string> Values = new();

    public string ValueName(long value) => Values.TryGetValue(value, out var n) ? n : null;
}

/// <summary>The commands, functions and enums of one script kind (Talk, Chr, Event, AI)</summary>
public class EsdKindCatalog
{
    public string Kind;
    public Dictionary<(int Bank, int Id), EsdCallDef> Commands = new();
    public Dictionary<int, EsdCallDef> Functions = new();
    public Dictionary<string, EsdEnumDef> Enums = new();

    public EsdCallDef Command(int bank, int id) => Commands.TryGetValue((bank, id), out var c) ? c : null;
    public EsdCallDef Function(long id) => id is >= int.MinValue and <= int.MaxValue && Functions.TryGetValue((int)id, out var f) ? f : null;
    public EsdEnumDef FindEnum(string name) => name != null && Enums.TryGetValue(name, out var e) ? e : null;
}

/// <summary>
/// A game's ESD catalogue: Assets/ESD/Catalogues/&lt;GAME&gt;.json, generated from ESDLang's scripting documentation
/// (used with permission of thefifthmatt) by Claude/Tools/EsdSurvey catalog, and checked against every vanilla file by
/// EsdSurvey catalogcheck.
/// </summary>
public class EsdCatalog
{
    public string Game;
    public string Source;
    public Dictionary<string, EsdKindCatalog> Kinds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The catalogue for a script kind. DS2's talk ESDs have no docs of their own; they use the Event ids
    /// completely (EsdSurvey catalogcheck: 2,615/2,615 commands, 4,156/4,156 functions), so they get the Event kind.
    /// </summary>
    public EsdKindCatalog For(EsdScriptKind kind)
    {
        var name = kind.ToString();
        if (Kinds.TryGetValue(name, out var k))
            return k;
        if (kind == EsdScriptKind.Talk && Kinds.TryGetValue("Event", out var ev))
            return ev;
        return null;
    }

    public static string FileName(ProjectType type) => type switch
    {
        ProjectType.DES => "DES",
        ProjectType.DS1 => "DS1",
        ProjectType.DS1R => "DS1R",
        ProjectType.DS2 => "DS2",
        ProjectType.DS2S => "DS2S",
        ProjectType.BB => "BB",
        ProjectType.DS3 => "DS3",
        ProjectType.SDT => "SDT",
        ProjectType.ER => "ER",
        ProjectType.NR => "NR",
        ProjectType.AC6 => "AC6",
        _ => null
    };

    public static EsdCatalog Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;

        var cat = new EsdCatalog
        {
            Game = Str(root, "Game"),
            Source = Str(root, "Source"),
        };

        if (!root.TryGetProperty("Kinds", out var kinds))
            return cat;

        foreach (var k in kinds.EnumerateArray())
        {
            var kind = new EsdKindCatalog { Kind = Str(k, "Kind") };

            if (k.TryGetProperty("Commands", out var cmds))
                foreach (var c in cmds.EnumerateArray())
                {
                    var def = Call(c);
                    kind.Commands[(def.Bank, def.Id)] = def;
                }

            if (k.TryGetProperty("Functions", out var funcs))
                foreach (var f in funcs.EnumerateArray())
                {
                    var def = Call(f);
                    if (f.TryGetProperty("Returns", out var r) && r.ValueKind == JsonValueKind.Object)
                    {
                        def.ReturnName = Str(r, "Name");
                        def.ReturnType = Str(r, "Type");
                        def.ReturnEnum = Str(r, "Enum");
                    }
                    def.BinaryReturn = Bool(f, "BinaryReturn");
                    kind.Functions[def.Id] = def;
                }

            if (k.TryGetProperty("Enums", out var enums))
                foreach (var e in enums.EnumerateArray())
                {
                    var en = new EsdEnumDef { Name = Str(e, "Name") };
                    if (e.TryGetProperty("Values", out var vals))
                        foreach (var v in vals.EnumerateArray())
                            en.Values.TryAdd(v.TryGetProperty("Value", out var val) && val.ValueKind == JsonValueKind.Number ? val.GetInt64() : 0, Str(v, "Name"));
                    if (en.Name != null)
                        kind.Enums[en.Name] = en;
                }

            if (kind.Kind != null)
                cat.Kinds[kind.Kind] = kind;
        }

        return cat;
    }

    private static EsdCallDef Call(JsonElement e)
    {
        var def = new EsdCallDef
        {
            // The generator leaves out zero values (EsdSurvey wrote with WhenWritingDefault): a missing number is 0
            Bank = Int(e, "Bank"),
            Id = Int(e, "Id"),
            Name = Str(e, "Name"),
            Comment = Str(e, "Comment"),
            Seen = Bool(e, "Seen"),
        };

        if (e.TryGetProperty("OldNames", out var old) && old.ValueKind == JsonValueKind.Array)
            foreach (var o in old.EnumerateArray())
                def.OldNames.Add(o.GetString());

        if (e.TryGetProperty("Args", out var args) && args.ValueKind == JsonValueKind.Array)
            foreach (var a in args.EnumerateArray())
                def.Args.Add(new EsdArgDef
                {
                    Name = Str(a, "Name"),
                    Type = Str(a, "Type"),
                    Role = Str(a, "Role"),
                    Enum = Str(a, "Enum"),
                    Optional = Bool(a, "Optional"),
                    Comment = Str(a, "Comment"),
                });

        def.Name ??= def.OldNames.FirstOrDefault();
        return def;
    }

    private static string Str(JsonElement e, string name)
    {
        return e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    }

    private static int Int(JsonElement e, string name)
    {
        return e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;
    }

    private static bool Bool(JsonElement e, string name)
    {
        return e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
    }
}
