using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace SoulsFormats
{
    /// <summary>Games covered by the FFX schema (the pre-FXR3 effect formats).</summary>
    public enum FfxGame
    {
        DES,
        DS2,
        DS2S,
        DS1,
        DS1R,
    }

    /// <summary>
    /// Layout and names of the FFX actions per game, generated from vanilla data by Claude/Tools/FfxSurvey --schema
    /// (Smithbox.Data/Assets/FXR/ffx_schema.json). Value type ids use DS2's numbering; DeS file ids are mapped
    /// through <see cref="DesValueTypes"/>. See Claude/Documentation/FFX Older Games Plan.md §10.
    /// </summary>
    public class FfxSchema
    {
        public IReadOnlyDictionary<int, FfxValueTypeDef> ValueTypes => valueTypes;
        private readonly Dictionary<int, FfxValueTypeDef> valueTypes = new();

        /// <summary>DeS value type id → DS2-scheme id.</summary>
        public IReadOnlyDictionary<int, int> DesValueTypes => desValueTypes;
        private readonly Dictionary<int, int> desValueTypes = new();

        /// <summary>Evaluator opcode → name.</summary>
        public IReadOnlyDictionary<int, string> Evaluators => evaluators;
        private readonly Dictionary<int, string> evaluators = new();

        public IReadOnlyDictionary<int, FfxActionDef> Actions => actions;
        private readonly Dictionary<int, FfxActionDef> actions = new();

        /// <summary>The effect-level param lists: "effect1" (ParamList1) and "effect2" (ParamList2).</summary>
        public IReadOnlyDictionary<string, FfxActionDef> EffectParams => effectParams;
        private readonly Dictionary<string, FfxActionDef> effectParams = new();

        public static FfxSchema Load(string path) => Parse(File.ReadAllText(path));

        public static FfxSchema Parse(string json)
        {
            var schema = new FfxSchema();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("valueTypes", out var vts))
                foreach (var p in vts.EnumerateObject())
                    schema.valueTypes[int.Parse(p.Name)] = new FfxValueTypeDef
                    {
                        Id = int.Parse(p.Name),
                        Name = Str(p.Value, "name"),
                        Kind = Str(p.Value, "kind"),
                        Source = Str(p.Value, "source"),
                        Desc = Str(p.Value, "desc"),
                    };

            if (root.TryGetProperty("desValueTypes", out var des))
                foreach (var p in des.EnumerateObject())
                    schema.desValueTypes[int.Parse(p.Name)] = p.Value.GetInt32();

            if (root.TryGetProperty("evaluators", out var evs))
                foreach (var p in evs.EnumerateObject())
                    schema.evaluators[int.Parse(p.Name)] = Str(p.Value, "name");

            if (root.TryGetProperty("actions", out var acts))
                foreach (var p in acts.EnumerateObject())
                    schema.actions[int.Parse(p.Name)] = ReadAction(p.Name, p.Value);

            if (root.TryGetProperty("effectParams", out var eps))
                foreach (var p in eps.EnumerateObject())
                    schema.effectParams[p.Name] = ReadAction(p.Name, p.Value);

            return schema;
        }

        private static FfxActionDef ReadAction(string key, JsonElement e)
        {
            var def = new FfxActionDef
            {
                Key = key,
                Id = int.TryParse(key, out int id) ? id : -1,
                Name = Str(e, "name") ?? key,
                Category = Str(e, "category") ?? "",
                Source = Str(e, "source"),
                Desc = Str(e, "desc"),
            };
            if (!e.TryGetProperty("games", out var games))
                return def;

            // Aliases ("DS2S": "DS2") are resolved after every concrete layout is read
            var aliases = new List<(FfxGame Game, FfxGame Base)>();
            foreach (var g in games.EnumerateObject())
            {
                if (!Enum.TryParse(g.Name, out FfxGame game))
                    continue;
                if (g.Value.ValueKind == JsonValueKind.String)
                {
                    if (Enum.TryParse(g.Value.GetString(), out FfxGame b))
                        aliases.Add((game, b));
                    continue;
                }
                var layout = new FfxActionLayout
                {
                    ParamCounts = g.Value.TryGetProperty("paramCounts", out var pc) ? pc.EnumerateArray().Select(x => x.GetInt32()).ToArray() : Array.Empty<int>(),
                };
                if (g.Value.TryGetProperty("params", out var ps))
                {
                    int i = 0;
                    foreach (var p in ps.EnumerateArray())
                    {
                        layout.Params.Add(new FfxParamDef
                        {
                            Index = i++,
                            Name = Str(p, "name"),
                            Types = p.TryGetProperty("types", out var t) ? t.EnumerateArray().Select(x => x.GetInt32()).ToArray() : Array.Empty<int>(),
                            Min = p.TryGetProperty("min", out var mn) ? mn.GetDouble() : null,
                            Max = p.TryGetProperty("max", out var mx) ? mx.GetDouble() : null,
                            Seen = p.TryGetProperty("seen", out var sn) ? sn.EnumerateArray().Select(x => x.GetDouble()).ToArray() : null,
                            Desc = Str(p, "desc"),
                        });
                    }
                }
                def.Games[game] = layout;
            }
            foreach (var (game, b) in aliases)
                if (def.Games.TryGetValue(b, out var bl))
                    def.Games[game] = bl;
            return def;
        }

        private static string Str(JsonElement e, string prop) =>
            e.ValueKind == JsonValueKind.Object && e.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        /// <summary>The DS2-scheme value type id for a param's file type id in this game.</summary>
        public int SchemeType(int fileType, FfxGame game) =>
            game == FfxGame.DES && desValueTypes.TryGetValue(fileType, out int t) ? t : fileType;

        /// <summary>The value type of a param's file type id in this game, or null when unknown.</summary>
        public FfxValueTypeDef ValueType(int fileType, FfxGame game) =>
            valueTypes.TryGetValue(SchemeType(fileType, game), out var vt) ? vt : null;

        /// <summary>An action's layout in a game, or null when the schema has none.</summary>
        public FfxActionLayout Layout(int actionId, FfxGame game) =>
            actions.TryGetValue(actionId, out var a) && a.Games.TryGetValue(game, out var l) ? l : null;

        /// <summary>The game an FFXDLSE file belongs to, from its stream format; DS2 files can't tell DS2 from DS2S.</summary>
        public static FfxGame GameOf(FFXDLSE ffx) => ffx.Format == FFXDLSE.StreamFormat.DES ? FfxGame.DES : FfxGame.DS2;
    }

    public class FfxValueTypeDef
    {
        /// <summary>DS2-scheme id.</summary>
        public int Id;
        public string Name;
        /// <summary>Body kind: int, float, tick, rgba, int2, float2, tick2, rgba2, intSequence, floatSequence, …, effectCall, actionCall, ref, ref3, paramFloat, paramRgba, unknown.</summary>
        public string Kind;
        public string Source;
        public string Desc;
    }

    public class FfxActionDef
    {
        /// <summary>The schema key: the action id, or "effect1"/"effect2".</summary>
        public string Key;
        /// <summary>Action id, -1 for the effect-level lists.</summary>
        public int Id;
        public string Name;
        public string Category;
        public string Source;
        public string Desc;
        public Dictionary<FfxGame, FfxActionLayout> Games = new();
    }

    public class FfxActionLayout
    {
        /// <summary>Every param count seen in vanilla.</summary>
        public int[] ParamCounts;
        /// <summary>One entry per slot up to the largest count.</summary>
        public List<FfxParamDef> Params = new();
    }

    public class FfxParamDef
    {
        public int Index;
        public string Name;
        /// <summary>The file type ids seen in this slot (DeS ids for DeS).</summary>
        public int[] Types;
        /// <summary>Range of the constant values seen, when the slot held constants.</summary>
        public double? Min, Max;
        /// <summary>The distinct constant values seen, when there were only a few.</summary>
        public double[] Seen;
        public string Desc;
    }

    /// <summary>
    /// Named access to an FFX action's (or an effect's) param list through the schema.
    /// When the list doesn't fit the schema (<see cref="Fits"/> false), an editor should show the raw params instead.
    /// </summary>
    public class FfxActionView
    {
        public FfxSchema Schema { get; }
        public FfxGame Game { get; }
        public FfxActionDef Def { get; }
        public FfxActionLayout Layout { get; }
        public FFXDLSE.ParamList List { get; }

        public FfxActionView(FfxSchema schema, FfxGame game, int actionId, FFXDLSE.ParamList list)
        {
            Schema = schema;
            Game = game;
            List = list;
            if (schema.Actions.TryGetValue(actionId, out var def))
            {
                Def = def;
                def.Games.TryGetValue(game, out var layout);
                Layout = layout;
            }
        }

        private FfxActionView(FfxSchema schema, FfxGame game, FfxActionDef def, FFXDLSE.ParamList list)
        {
            Schema = schema;
            Game = game;
            List = list;
            Def = def;
            if (def != null && def.Games.TryGetValue(game, out var layout))
                Layout = layout;
        }

        /// <summary>A view of an effect's ParamList1 (index 1) or ParamList2 (index 2).</summary>
        public static FfxActionView ForEffect(FfxSchema schema, FfxGame game, FFXDLSE.FXEffect effect, int index)
        {
            schema.EffectParams.TryGetValue($"effect{index}", out var def);
            return new FfxActionView(schema, game, def, index == 1 ? effect.ParamList1 : effect.ParamList2);
        }

        /// <summary>A view of a state action.</summary>
        public static FfxActionView ForAction(FfxSchema schema, FfxGame game, FFXDLSE.Action action) =>
            new FfxActionView(schema, game, action.ID, action.ParamList);

        /// <summary>A view of a nested action call.</summary>
        public static FfxActionView ForCall(FfxSchema schema, FfxGame game, FFXDLSE.Param38 call) =>
            new FfxActionView(schema, game, call.ActionID, call.ParamList);

        public int Count => List.Params.Count;

        /// <summary>
        /// True when the schema knows this action in this game, the param count is one seen in vanilla, and every
        /// param's type is one seen in its slot.
        /// </summary>
        public bool Fits
        {
            get
            {
                if (Layout == null || Array.IndexOf(Layout.ParamCounts, Count) < 0)
                    return false;
                for (int i = 0; i < Count; i++)
                    if (i >= Layout.Params.Count || Array.IndexOf(Layout.Params[i].Types, List.Params[i].FileType) < 0)
                        return false;
                return true;
            }
        }

        /// <summary>Why <see cref="Fits"/> is false, or null when it fits.</summary>
        public string Misfit()
        {
            if (Def == null) return "unknown action";
            if (Layout == null) return $"no layout for {Game}";
            if (Array.IndexOf(Layout.ParamCounts, Count) < 0) return $"{Count} params, schema has {string.Join("/", Layout.ParamCounts)}";
            for (int i = 0; i < Count; i++)
                if (i >= Layout.Params.Count || Array.IndexOf(Layout.Params[i].Types, List.Params[i].FileType) < 0)
                    return $"param {i} type {List.Params[i].FileType} not in schema";
            return null;
        }

        public FfxParamDef ParamDef(int index) => Layout != null && index < Layout.Params.Count ? Layout.Params[index] : null;

        /// <summary>The slot name, or "param{index}" when the schema has none.</summary>
        public string NameOf(int index) => ParamDef(index)?.Name ?? $"param{index:00}";

        public int IndexOf(string name)
        {
            if (Layout != null)
                for (int i = 0; i < Math.Min(Count, Layout.Params.Count); i++)
                    if (Layout.Params[i].Name == name)
                        return i;
            return -1;
        }

        public FFXDLSE.Param Get(int index) => index >= 0 && index < Count ? List.Params[index] : null;

        public FFXDLSE.Param Get(string name) => Get(IndexOf(name));

        public FfxValueTypeDef TypeOf(int index) => Get(index) is FFXDLSE.Param p ? Schema.ValueType(p.FileType, Game) : null;

        /// <summary>
        /// A constant param's value: int, float (also ticks), Vector4 (colour), Vector2 (2 floats / 2 ticks) or
        /// int[2]; null for sequences, calls, refs and other non-constant bodies.
        /// </summary>
        public object GetValue(int index) => Get(index) switch
        {
            FFXDLSE.Param1 p => p.Int,
            FFXDLSE.Param7 p => p.Float,
            FFXDLSE.Param15 p => new Vector4(p.Color.R, p.Color.G, p.Color.B, p.Color.A),
            FFXDLSE.Param40 p => p.TextureID,
            FFXDLSE.Param41 p => p.Unk04,
            FFXDLSE.Param68 p => p.SoundID,
            FFXDLSE.Param69 p => p.Unk04,
            FFXDLSE.Param70 p => p.Tick,
            FFXDLSE.Param79 p => new[] { p.Int1, p.Int2 },
            FFXDLSE.Param81 p => new Vector2(p.Float1, p.Float2),
            FFXDLSE.Param85 p => new Vector2(p.Tick1, p.Tick2),
            _ => null,
        };

        public object GetValue(string name) => GetValue(IndexOf(name));

        /// <summary>Sets a constant param's value (same types as <see cref="GetValue(int)"/>); false when the param isn't a constant of that type.</summary>
        public bool SetValue(int index, object value)
        {
            switch (Get(index), value)
            {
                case (FFXDLSE.Param1 p, int v): p.Int = v; return true;
                case (FFXDLSE.Param7 p, float v): p.Float = v; return true;
                case (FFXDLSE.Param15 p, Vector4 v): p.Color = new FFXDLSE.PrimitiveColor(v.X, v.Y, v.Z, v.W); return true;
                case (FFXDLSE.Param40 p, int v): p.TextureID = v; return true;
                case (FFXDLSE.Param41 p, int v): p.Unk04 = v; return true;
                case (FFXDLSE.Param68 p, int v): p.SoundID = v; return true;
                case (FFXDLSE.Param69 p, int v): p.Unk04 = v; return true;
                case (FFXDLSE.Param70 p, float v): p.Tick = v; return true;
                case (FFXDLSE.Param79 p, int[] v) when v.Length == 2: p.Int1 = v[0]; p.Int2 = v[1]; return true;
                case (FFXDLSE.Param81 p, Vector2 v): p.Float1 = v.X; p.Float2 = v.Y; return true;
                case (FFXDLSE.Param85 p, Vector2 v): p.Tick1 = v.X; p.Tick2 = v.Y; return true;
                default: return false;
            }
        }

        public bool SetValue(string name, object value) => SetValue(IndexOf(name), value);
    }
}
