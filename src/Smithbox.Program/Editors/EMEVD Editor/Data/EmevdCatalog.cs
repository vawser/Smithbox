using StudioCore.Application;
using System.Text.Json;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>An arg's value type, numbered as SoulsFormats' EMEVD.Instruction.ArgType</summary>
public enum EmevdArgType { U8 = 0, U16 = 1, U32 = 2, S8 = 3, S16 = 4, S32 = 5, F32 = 6 }

public class EmevdArgDef
{
    public int Offset;
    public EmevdArgType Type;
    public string Name;
    public string JapaneseName;
    public string EnumKey;
    public double Default;
    public string Format;
    /// <summary>Some vanilla uses stop before this arg</summary>
    public bool Optional;
    /// <summary>Not in a shipped definition: inferred from the data</summary>
    public bool Inferred;

    /// <summary>The English name, else the shipped name, else the offset</summary>
    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name
        : !string.IsNullOrEmpty(JapaneseName) ? JapaneseName
        : $"+{Offset:X2}";
}

public class EmevdInstrDef
{
    public EmevdClassDef Class;
    public int ID;
    public string Key;
    public string Name;
    public string JapaneseName;
    /// <summary>Own, Shared, Borrowed, Exe or Inferred (Claude/Documentation/EMEVD Layouts.md)</summary>
    public string Confidence;
    public string Source;
    public string Exe;
    public string NameSource;
    public string Warnings;
    public int Uses;
    public List<int> Lengths = new();
    /// <summary>The last arg repeats to fill the instruction (Initialize Event's args)</summary>
    public bool RepeatLastArg;
    public List<EmevdArgDef> Args = new();

    public int Bank => Class.Bank;

    public bool HasName => !string.IsNullOrEmpty(Name);

    /// <summary>The English name, or the key for unnamed (inferred or exe-built) layouts</summary>
    public string DisplayName => HasName ? Name : Key;
}

public class EmevdClassDef
{
    public int Bank;
    public string Name;
    public string JapaneseName;
    public Dictionary<int, EmevdInstrDef> Instructions = new();

    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : $"Bank {Bank}";
}

public class EmevdEnumDef
{
    public string Key;
    public string Name;
    public Dictionary<long, string> Values = new();
    public Dictionary<long, string> JapaneseValues = new();

    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : Key;

    public string ValueName(long value)
    {
        if (Values.TryGetValue(value, out var name) && !string.IsNullOrEmpty(name))
            return name;

        return JapaneseValues.TryGetValue(value, out var jp) ? jp : null;
    }
}

/// <summary>
/// Smithbox's own instruction layouts for one game: Assets/EMEVD/Layouts/&lt;GAME&gt;.json, built by
/// Claude/Tools/EmedfSurvey from the games' shipped common.emedf files and every vanilla EMEVD.
/// </summary>
public class EmevdCatalog
{
    public string Game;
    public Dictionary<int, EmevdClassDef> Classes = new();
    public Dictionary<string, EmevdEnumDef> Enums = new();

    /// <summary>The layout file name for a project type, or null when the game has none</summary>
    public static string LayoutName(ProjectType type) => type switch
    {
        ProjectType.DS1 => "DS1",
        ProjectType.DS1R => "DS1R",
        ProjectType.BB => "BB",
        ProjectType.DS3 => "DS3",
        ProjectType.SDT => "SDT",
        ProjectType.ER => "ER",
        ProjectType.NR => "NR",
        ProjectType.AC6 => "AC6",
        ProjectType.DB_NT => "DB_NT",
        _ => null
    };

    public EmevdInstrDef Find(int bank, int id)
    {
        return Classes.TryGetValue(bank, out var c) && c.Instructions.TryGetValue(id, out var i) ? i : null;
    }

    public EmevdEnumDef FindEnum(string key)
    {
        return key != null && Enums.TryGetValue(key, out var e) ? e : null;
    }

    public static EmevdCatalog Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        var catalog = new EmevdCatalog { Game = Str(root, "Game") };

        foreach (var c in root.GetProperty("Classes").EnumerateArray())
        {
            var cls = new EmevdClassDef
            {
                Bank = c.GetProperty("Bank").GetInt32(),
                Name = Str(c, "Name"),
                JapaneseName = Str(c, "JapaneseName"),
            };

            foreach (var i in c.GetProperty("Instructions").EnumerateArray())
            {
                var instr = new EmevdInstrDef
                {
                    Class = cls,
                    ID = i.GetProperty("ID").GetInt32(),
                    Key = Str(i, "Key"),
                    Name = Str(i, "Name"),
                    JapaneseName = Str(i, "JapaneseName"),
                    Confidence = Str(i, "Confidence"),
                    Source = Str(i, "Source"),
                    Exe = Str(i, "Exe"),
                    NameSource = Str(i, "NameSource"),
                    Warnings = Str(i, "Warnings"),
                    Uses = i.TryGetProperty("Uses", out var u) ? u.GetInt32() : 0,
                    RepeatLastArg = Bool(i, "RepeatLastArg"),
                };

                if (i.TryGetProperty("Lengths", out var lengths))
                {
                    foreach (var l in lengths.EnumerateArray())
                        instr.Lengths.Add(l.GetInt32());
                }

                foreach (var a in i.GetProperty("Args").EnumerateArray())
                {
                    instr.Args.Add(new EmevdArgDef
                    {
                        Offset = a.GetProperty("Offset").GetInt32(),
                        Type = ParseType(Str(a, "Type")),
                        Name = Str(a, "Name"),
                        JapaneseName = Str(a, "JapaneseName"),
                        EnumKey = Str(a, "Enum"),
                        Default = a.TryGetProperty("Default", out var d) && d.ValueKind == JsonValueKind.Number ? d.GetDouble() : 0,
                        Format = Str(a, "Format"),
                        Optional = Bool(a, "Optional"),
                        Inferred = Bool(a, "Inferred"),
                    });
                }

                cls.Instructions[instr.ID] = instr;
            }

            catalog.Classes[cls.Bank] = cls;
        }

        if (root.TryGetProperty("Enums", out var enums))
        {
            foreach (var e in enums.EnumerateArray())
            {
                var def = new EmevdEnumDef { Key = Str(e, "Key"), Name = Str(e, "Name") };

                foreach (var v in e.GetProperty("Values").EnumerateArray())
                {
                    var value = v.GetProperty("Value").GetInt64();
                    def.Values[value] = Str(v, "Name");
                    def.JapaneseValues[value] = Str(v, "JapaneseName");
                }

                catalog.Enums[def.Key] = def;
            }
        }

        return catalog;
    }

    private static EmevdArgType ParseType(string type) => type switch
    {
        "u8" => EmevdArgType.U8,
        "u16" => EmevdArgType.U16,
        "u32" => EmevdArgType.U32,
        "s8" => EmevdArgType.S8,
        "s16" => EmevdArgType.S16,
        "s32" => EmevdArgType.S32,
        "f32" => EmevdArgType.F32,
        _ => throw new InvalidDataException($"Unknown EMEVD arg type '{type}'")
    };

    private static string Str(JsonElement e, string name)
    {
        return e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    }

    private static bool Bool(JsonElement e, string name)
    {
        return e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
    }
}
