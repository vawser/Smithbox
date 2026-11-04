using SoulsFormats;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// survey: lists the fields that look like they hold an ID, with their games, types and current meta.
/// tag-aliases: adds an Alias to the per-event meta of the fields that match a rule, leaving fields that
/// already have one as they are.
///
/// Usage: TaeMetaSplit survey|tag-aliases &lt;Assets/TAE folder&gt;
/// </summary>
static class Aliases
{
    static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static readonly Regex IdLike = new("(?i)(id|ffx|sfx|chr|char|anim|flag|sound|cutscene|movie|gparam|npc|model)");

    /// <summary>
    /// Field name pattern -> meta key and value. Every rule that matches is applied, each to its own key.
    /// Only integer fields without an inline enum are tagged.
    /// </summary>
    static readonly (Regex Field, string Key, string Value)[] Rules =
    [
        // Visual effects: every FFX ID, whichever character type it is for
        (new("^FFXID"), "Alias", "Particles"),

        // Sounds played by the event. The sound type picks the bank, the alias names the ID.
        (new("^(Override)?SoundID$"), "Alias", "Sounds"),

        // Animations of the character the time act belongs to
        (new("(?i)^(AnimationID|AnimID|AimUpAnimID|AimDownAnimID|Destination Anim ID|Transition Anim ID)$"), "Alias", "Animations"),

        // The SpEffect state the event is limited to, from the enum the param editor shares
        (new(@"^StateInfo( \([0-9A-F]+\))?$"), "ProjectEnum", "SP_EFFECT_TYPE"),
    ];

    public static int Survey(string root)
    {
        var rows = new List<(string Field, string Game, string Type, string Event, string Meta)>();

        foreach (var (game, def, bank, meta) in Load(root))
        {
            foreach (var (eventId, ev) in bank)
            {
                foreach (var p in ev.Values.Where(p => p.ValueToAssert == null && IdLike.IsMatch(p.Name)))
                {
                    var m = FindField(meta, eventId, p.Name);
                    var metaText = m == null ? "" : string.Join(" ", m.Where(kv => kv.Key != "Field").Select(kv => $"{kv.Key}={kv.Value?.ToJsonString()}"));

                    rows.Add((p.Name, $"{game}/{def}", p.EnumEntries != null ? $"{p.Type}+enum" : p.Type.ToString(), ev.Name, metaText));
                }
            }
        }

        foreach (var g in rows.GroupBy(r => r.Field).OrderBy(g => g.Key))
        {
            Console.WriteLine($"{g.Key} ({string.Join("/", g.Select(r => r.Type).Distinct())}) " +
                $"[{string.Join(",", g.Select(r => r.Game).Distinct())}] " +
                $"e.g. {string.Join(", ", g.Select(r => r.Event).Distinct().Take(3))} " +
                $"meta: {string.Join(" | ", g.Select(r => r.Meta).Distinct())}");
        }

        return 0;
    }

    public static int Tag(string root)
    {
        foreach (var (game, def, bank, meta) in Load(root))
        {
            var added = new Dictionary<string, List<string>>();

            foreach (var (eventId, ev) in bank.OrderBy(e => e.Key))
            {
                foreach (var p in ev.Values)
                {
                    if (p.ValueToAssert != null || p.EnumEntries != null || !IsInteger(p.Type))
                        continue;

                    foreach (var rule in Rules.Where(r => r.Field.IsMatch(p.Name)))
                    {
                        var field = FindField(meta, eventId, p.Name);

                        // A value set by hand stays
                        if (field != null && field[rule.Key] is not null)
                            continue;

                        if (field == null)
                        {
                            var evNode = FindEvent(meta, eventId) ?? AddEvent(meta, eventId, ev.Name);
                            field = new JsonObject { ["Field"] = p.Name };
                            evNode["Fields"]!.AsArray().Add(field);
                        }

                        field[rule.Key] = rule.Value;

                        var tag = $"{rule.Key}={rule.Value}";

                        if (!added.TryGetValue(tag, out var list))
                            added[tag] = list = [];

                        list.Add($"{eventId}:{p.Name}");
                    }
                }
            }

            File.WriteAllText(Path.Join(root, game, "Meta", $"{def}.json"), meta.ToJsonString(Json));

            Console.WriteLine($"{game}/{def}: " + (added.Count == 0 ? "nothing to tag" :
                string.Join("; ", added.Select(a => $"{a.Key} +{a.Value.Count}"))));

            foreach (var (alias, fields) in added)
                Console.WriteLine($"    {alias}: {string.Join(", ", fields)}");
        }

        return 0;
    }

    static bool IsInteger(TAE.Template.ParamType type) => type is
        TAE.Template.ParamType.s8 or TAE.Template.ParamType.u8 or TAE.Template.ParamType.s16 or
        TAE.Template.ParamType.u16 or TAE.Template.ParamType.s32 or TAE.Template.ParamType.u32 or
        TAE.Template.ParamType.s64 or TAE.Template.ParamType.u64;

    static IEnumerable<(string Game, string Def, TAE.Template.BankTemplate Bank, JsonObject Meta)> Load(string root)
    {
        root = Path.GetFullPath(root);

        foreach (var gameDir in Directory.EnumerateDirectories(root))
        {
            var enums = Directory.EnumerateFiles(Path.Join(gameDir, "Enums"), "*.json")
                .Select(f => JsonNode.Parse(File.ReadAllText(f))!)
                .ToDictionary(
                    n => (string)n["Key"]!,
                    n => n["Options"]!.AsArray()
                        .Select(o => new KeyValuePair<string, string>((string)o!["Names"]![0]!["Text"]!, (string)o["Key"]!))
                        .ToList());

            foreach (var defPath in Directory.EnumerateFiles(Path.Join(gameDir, "Defs"), "*.xml"))
            {
                var def = Path.GetFileNameWithoutExtension(defPath);
                var bank = TAE.Template.ReadXMLFile(defPath, k => enums.TryGetValue(k, out var o) ? o : null)[0];
                var metaPath = Path.Join(gameDir, "Meta", $"{def}.json");
                var meta = File.Exists(metaPath)
                    ? JsonNode.Parse(File.ReadAllText(metaPath))!.AsObject()
                    : new JsonObject { ["Events"] = new JsonArray() };

                yield return (Path.GetFileName(gameDir), def, bank, meta);
            }
        }
    }

    static JsonObject FindEvent(JsonObject meta, int eventId) =>
        meta["Events"]!.AsArray().Select(e => e!.AsObject()).FirstOrDefault(e => (int)e["ID"]! == eventId);

    static JsonObject FindField(JsonObject meta, int eventId, string name) =>
        FindEvent(meta, eventId)?["Fields"]!.AsArray().Select(f => f!.AsObject()).FirstOrDefault(f => (string)f["Field"] == name);

    // Kept in event ID order, as the converter wrote them
    static JsonObject AddEvent(JsonObject meta, int eventId, string name)
    {
        var events = meta["Events"]!.AsArray();
        var node = new JsonObject { ["ID"] = eventId, ["Event"] = name, ["Fields"] = new JsonArray() };

        var index = 0;
        while (index < events.Count && (int)events[index]!["ID"]! < eventId)
            index++;

        events.Insert(index, node);
        return node;
    }
}
