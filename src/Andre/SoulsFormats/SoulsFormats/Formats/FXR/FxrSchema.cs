using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SoulsFormats
{
    /// <summary>Games covered by the FXR schema.</summary>
    public enum FxrGame
    {
        DS3,
        SDT,
        ER,
        AC6,
        NR,
    }

    public enum FxrFieldType
    {
        Bool,
        Int,
        Float,
        Vec2,
        Vec3,
        Vec4,
    }

    /// <summary>
    /// Named, typed layout of every FXR action per game, generated from fxr-main's YAML by
    /// Tools/FxrSchemaGen (Smithbox.Data/Assets/FXR/fxr_schema.json). See FXR Format Plan, Layer 2.
    /// </summary>
    public class FxrSchema
    {
        public string Source { get; private set; }

        public IReadOnlyDictionary<int, FxrActionDef> Actions => actions;
        private readonly Dictionary<int, FxrActionDef> actions = new();

        public IReadOnlyDictionary<string, FxrEnumDef> Enums => enums;
        private readonly Dictionary<string, FxrEnumDef> enums = new();

        /// <summary>
        /// fxr-main's ConfigActionSlots, by config type (1004 Basic, 1005 NodeEmitter): the config's fixed action
        /// slots in file order, each with the action types that may fill it. Configs not listed have no fixed slots.
        /// </summary>
        public IReadOnlyDictionary<int, List<int[]>> ConfigSlots => configSlots;
        private readonly Dictionary<int, List<int[]>> configSlots = new();

        /// <summary>Which slot of a config type an action type fills, -1 when none (or the config has no fixed slots).</summary>
        public int SlotOf(int configType, int actionType)
        {
            if (!configSlots.TryGetValue(configType, out var slots))
                return -1;
            for (int i = 0; i < slots.Count; i++)
                if (Array.IndexOf(slots[i], actionType) >= 0)
                    return i;
            return -1;
        }

        public static FxrSchema Load(string path) => Parse(File.ReadAllText(path));

        public static FxrSchema Parse(string json)
        {
            var schema = new FxrSchema();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            schema.Source = root.TryGetProperty("source", out var src) ? src.GetString() : null;

            foreach (var e in root.GetProperty("enums").EnumerateObject())
            {
                var def = new FxrEnumDef { Name = e.Name, Description = Str(e.Value, "desc") };
                foreach (var m in e.Value.GetProperty("members").EnumerateObject())
                    def.Members.Add(new FxrEnumMember { Name = m.Name, Value = m.Value.GetProperty("value").GetInt32(), Description = Str(m.Value, "desc") });
                schema.enums[def.Name] = def;
            }

            foreach (var a in root.GetProperty("actions").EnumerateArray())
            {
                var def = new FxrActionDef
                {
                    Type = a.GetProperty("type").GetInt32(),
                    Name = a.GetProperty("name").GetString(),
                    Slot = Str(a, "slot"),
                    IsAppearance = a.GetProperty("isAppearance").GetBoolean(),
                    IsParticle = a.GetProperty("isParticle").GetBoolean(),
                    Description = Str(a, "desc"),
                };
                foreach (var p in a.GetProperty("props").EnumerateObject())
                    def.Props[p.Name] = ParseProp(p.Name, p.Value);

                foreach (var g in a.GetProperty("games").EnumerateObject())
                {
                    if (!Enum.TryParse<FxrGame>(g.Name, out var game)) continue;
                    var layout = new FxrActionLayout { Action = def, Game = game };
                    if (g.Value.TryGetProperty("fallback", out var fb) && fb.GetBoolean())
                        layout.Fallback = true;
                    else
                    {
                        layout.Fields1 = ParseFields(g.Value.GetProperty("fields1"), def);
                        layout.Fields2 = ParseFields(g.Value.GetProperty("fields2"), def);
                        layout.Properties1 = ParseNames(g.Value.GetProperty("properties1"));
                        layout.Properties2 = ParseNames(g.Value.GetProperty("properties2"));
                        layout.Section10s = ParseNames(g.Value.GetProperty("section10s"));
                    }
                    def.Layouts[game] = layout;
                }
                schema.actions[def.Type] = def;
            }

            if (root.TryGetProperty("configSlots", out var cs))
            {
                foreach (var c in cs.EnumerateObject())
                {
                    if (!int.TryParse(c.Name, out var configType)) continue;
                    var list = new List<int[]>();
                    foreach (var slot in c.Value.GetProperty("slots").EnumerateArray())
                        list.Add(slot.EnumerateArray().Select(t => t.GetInt32()).ToArray());
                    schema.configSlots[configType] = list;
                }
            }
            return schema;
        }

        /// <summary>The action's layout in a game, or null when the schema has none (unknown type or game).</summary>
        public FxrActionLayout GetLayout(int type, FxrGame game) =>
            actions.TryGetValue(type, out var def) && def.Layouts.TryGetValue(game, out var layout) && !layout.Fallback ? layout : null;

        /// <summary>Resolves a schema default ("BlendMode.Normal", numbers, bools) to a raw word.</summary>
        internal uint DefaultWord(FxrPropDef prop, FxrFieldType type, int component)
        {
            var d = prop?.Default;
            if (d is JsonElement e)
            {
                if (e.ValueKind == JsonValueKind.Array)
                {
                    int i = 0;
                    foreach (var item in e.EnumerateArray())
                        if (i++ == component) return Word(item, type);
                    return 0;
                }
                return Word(e, type);
            }
            return 0;
        }

        private uint Word(JsonElement e, FxrFieldType type)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.True: return 1;
                case JsonValueKind.False: return 0;
                case JsonValueKind.Number:
                    if (type is FxrFieldType.Int or FxrFieldType.Bool)
                        return (uint)(e.TryGetInt64(out var l) ? l : (long)e.GetDouble());
                    return BitConverter.SingleToUInt32Bits((float)e.GetDouble());
                case JsonValueKind.String:
                    // "Enum.Member"
                    var s = e.GetString();
                    int dot = s.IndexOf('.');
                    if (dot > 0 && enums.TryGetValue(s[..dot], out var en))
                        foreach (var m in en.Members)
                            if (m.Name == s[(dot + 1)..])
                                return type == FxrFieldType.Float ? BitConverter.SingleToUInt32Bits(m.Value) : (uint)m.Value;
                    return 0;
                default: return 0;
            }
        }

        private static FxrPropDef ParseProp(string name, JsonElement p)
        {
            var def = new FxrPropDef
            {
                Name = name,
                TypeScriptType = Str(p, "type"),
                Enum = Str(p, "enum"),
                Resource = Str(p, "resource"),
                TextureType = Str(p, "textureType"),
                Argument = Str(p, "argument"),
                Scale = Str(p, "scale"),
                Time = Str(p, "time"),
                Color = Str(p, "color"),
                Description = Str(p, "desc"),
                Components = p.TryGetProperty("components", out var c) ? c.GetInt32() : 1,
                IsSection10 = p.TryGetProperty("s10", out var s) && s.GetBoolean(),
            };
            if (p.TryGetProperty("default", out var d)) def.Default = d.Clone();
            return def;
        }

        private static List<FxrFieldSlot> ParseFields(JsonElement list, FxrActionDef def)
        {
            var slots = new List<FxrFieldSlot>();
            int word = 0;
            foreach (var f in list.EnumerateArray())
            {
                var type = ParseType(f.GetProperty("type").GetString());
                var name = f.GetProperty("name").GetString();
                var slot = new FxrFieldSlot { Name = name, Type = type, Word = word, Prop = def.Props.GetValueOrDefault(name) };
                slots.Add(slot);
                word += slot.Words;
            }
            return slots;
        }

        private static List<string> ParseNames(JsonElement list)
        {
            var names = new List<string>();
            foreach (var n in list.EnumerateArray()) names.Add(n.GetString());
            return names;
        }

        private static FxrFieldType ParseType(string t) => t switch
        {
            "bool" => FxrFieldType.Bool,
            "int" => FxrFieldType.Int,
            "float" => FxrFieldType.Float,
            "vec2" => FxrFieldType.Vec2,
            "vec3" => FxrFieldType.Vec3,
            "vec4" => FxrFieldType.Vec4,
            _ => throw new FormatException($"Unknown FXR field type '{t}'."),
        };

        private static string Str(JsonElement e, string name)
        {
            var s = e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
            return name == "desc" ? PlainLinks(s) : s;
        }

        // {@link target} and {@link target shown text}, from fxr-main's JSDoc
        private static readonly System.Text.RegularExpressions.Regex LinkPattern =
            new(@"\{@link\s+([^\s}]+)(?:\s+([^}]*?))?\s*\}", System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// The descriptions come from fxr-main's JSDoc, whose {@link} tags Smithbox doesn't render: a link becomes
        /// its shown text, or else the target's last part (ActionType.Line to Line), which is the name the editor shows.
        /// </summary>
        public static string PlainLinks(string s)
        {
            if (string.IsNullOrEmpty(s) || !s.Contains("{@link"))
                return s;

            return LinkPattern.Replace(s, m =>
            {
                if (m.Groups[2].Success && m.Groups[2].Value.Length > 0)
                    return m.Groups[2].Value;
                var target = m.Groups[1].Value;
                return target.Substring(target.LastIndexOf('.') + 1);
            });
        }
    }

    public class FxrEnumDef
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<FxrEnumMember> Members { get; } = new();
    }

    public class FxrEnumMember
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
    }

    public class FxrActionDef
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string Slot { get; set; }
        public bool IsAppearance { get; set; }
        public bool IsParticle { get; set; }
        public string Description { get; set; }
        public Dictionary<string, FxrPropDef> Props { get; } = new();
        public Dictionary<FxrGame, FxrActionLayout> Layouts { get; } = new();
    }

    /// <summary>One named value of an action: a field, a property (curve) or a section10 list.</summary>
    public class FxrPropDef
    {
        public string Name { get; set; }
        /// <summary>fxr-main's TypeScript type, e.g. "ScalarValue", "Vector4Value", "BlendMode | ScalarProperty".</summary>
        public string TypeScriptType { get; set; }
        public string Enum { get; set; }
        /// <summary>texture, model, anibnd or sound when the value is a resource id.</summary>
        public string Resource { get; set; }
        public string TextureType { get; set; }
        /// <summary>What a property's curve is driven by (PropertyArgument name, e.g. "ParticleAge").</summary>
        public string Argument { get; set; }
        public string Scale { get; set; }
        public string Time { get; set; }
        public string Color { get; set; }
        public int Components { get; set; } = 1;
        public bool IsSection10 { get; set; }
        public string Description { get; set; }
        /// <summary>Raw JSON default (number, bool, array or "Enum.Member").</summary>
        public object Default { get; set; }
        public bool IsUnknown => Name.StartsWith("unk", StringComparison.Ordinal);
    }

    public class FxrFieldSlot
    {
        public string Name { get; set; }
        public FxrFieldType Type { get; set; }
        /// <summary>Word index inside its list (fields1 or fields2).</summary>
        public int Word { get; set; }
        public int Words => Type switch { FxrFieldType.Vec2 => 2, FxrFieldType.Vec3 => 3, FxrFieldType.Vec4 => 4, _ => 1 };
        public FxrPropDef Prop { get; set; }
    }

    public class FxrActionLayout
    {
        public FxrActionDef Action { get; set; }
        public FxrGame Game { get; set; }
        /// <summary>The game has no such action; fxr-main writes a replacement instead.</summary>
        public bool Fallback { get; set; }
        public List<FxrFieldSlot> Fields1 { get; set; } = new();
        public List<FxrFieldSlot> Fields2 { get; set; } = new();
        public List<string> Properties1 { get; set; } = new();
        public List<string> Properties2 { get; set; } = new();
        public List<string> Section10s { get; set; } = new();

        public int Words1 => Fields1.Count == 0 ? 0 : Fields1[^1].Word + Fields1[^1].Words;
        public int Words2 => Fields2.Count == 0 ? 0 : Fields2[^1].Word + Fields2[^1].Words;

        /// <summary>
        /// fxr-main's rule: the schema applies when every count is at most the schema's count.
        /// Otherwise the action is shown as raw words.
        /// </summary>
        public bool Fits(FxrAction a)
        {
            int f2 = a.Fields2.Count - (FxrActionView.HasDs3TracerQuirk(a, Game) ? 1 : 0);
            return a.Fields1.Count <= Words1 && f2 <= Words2
                && a.Properties1.Count <= Properties1.Count && a.Properties2.Count <= Properties2.Count
                && a.Section10s.Count <= Section10s.Count;
        }
    }
}
