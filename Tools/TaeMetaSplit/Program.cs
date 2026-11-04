using SoulsFormats;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

// Splits the TAE.Template.*.xml files and TimeAct-Meta.json into the per-game layout the param metadata uses:
//   TAE/<GAME>/Defs/<Def>.xml                   event layouts, with enums named by an 'enum' attribute
//   TAE/<GAME>/Enums/<Key>.json                 the enum options, shared by the Defs of the game
//   TAE/<GAME>/Meta/<Def>.json                  field name -> editor feature (param refs, aliases, bools...)
//   TAE/<GAME>/Annotations/English/<Def>.json   field name -> annotated name and description
// Then reads each Def back with its enums and checks it matches the old template exactly.
//
// Usage: TaeMetaSplit <Assets/TAE folder>

if (args.Length > 0 && args[0] == "per-event")
    return PerEvent.Run(args.Length > 1 ? args[1] : @"..\..\src\Smithbox.Data\Assets\TAE");

var root = args.Length > 0 ? args[0] : @"..\..\src\Smithbox.Data\Assets\TAE";
root = Path.GetFullPath(root);

var sources = new (string File, string Game, string Def)[]
{
    ("TAE.Template.DES.xml", "DES", "Character"),
    ("TAE.Template.DESR.xml", "DES", "Character-DESR"),
    ("TAE.Template.DS1.xml", "DS1", "Character"),
    ("TAE.Template.DS1.OBJ.xml", "DS1", "Object"),
    ("TAE.Template.DS1.REMO.xml", "DS1", "Cutscene"),
    ("TAE.Template.SOTFS.xml", "DS2S", "Character"),
    ("TAE.Template.BB.xml", "BB", "Character"),
    ("TAE.Template.DS3.xml", "DS3", "Character"),
    ("TAE.Template.SDT.xml", "SDT", "Character"),
    ("TAE.Template.ER.xml", "ER", "Character"),
    ("TAE.Template.AC6.xml", "AC6", "Character"),
    ("TAE.Template.ERNR.xml", "NR", "Character"),
    ("TAE.Template.DB.xml", "DB", "Character"),
};

var json = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var metaPath = Path.Join(root, "TimeAct-Meta.json");
var metaFields = JsonNode.Parse(File.ReadAllText(metaPath))!["Fields"]!.AsArray();

var failures = 0;

foreach (var game in sources.GroupBy(s => s.Game))
{
    var gameDir = Path.Join(root, game.Key);

    // Every inline enum of the game: the field it is on, the event, and its options in order
    var docs = new List<(string Def, string File, XDocument Doc)>();
    var uses = new List<(XElement Param, string Field, string Event, List<(string Name, string Value)> Options)>();

    foreach (var source in game)
    {
        var doc = XDocument.Load(Path.Join(root, source.File));
        docs.Add((source.Def, source.File, doc));

        foreach (var eventNode in doc.Root!.Descendants().Where(e => e.Name == "event" || e.Name == "action"))
        {
            var eventName = (string)eventNode.Attribute("name") ?? $"Event{(string)eventNode.Attribute("id")}";

            foreach (var param in eventNode.Elements())
            {
                var entries = param.Elements("entry").ToList();

                if (entries.Count == 0)
                    continue;

                var options = entries.Select(e => ((string)e.Attribute("name"), (string)e.Attribute("value"))).ToList();
                var field = (string)param.Attribute("name") ?? "Unnamed";

                uses.Add((param, field, eventName, options));
            }
        }
    }

    // Identical option lists share an enum, named after the fields that use it (Mask0..Mask31 -> Mask).
    // A name that more than one list would take is told apart by the event the list first appears in.
    static string Signature(List<(string Name, string Value)> options) =>
        string.Join("\n", options.Select(o => $"{o.Value}\t{o.Name}"));

    var enumKeys = new Dictionary<string, string>();
    var enums = new Dictionary<string, List<(string Name, string Value)>>();

    var lists = uses.GroupBy(u => Signature(u.Options))
        .Select(g => (Signature: g.Key, Uses: g.ToList(), Name: CommonName(g.Select(u => u.Field).Distinct().ToList())))
        .ToList();

    var nameCounts = lists.GroupBy(l => l.Name).ToDictionary(g => g.Key, g => g.Count());

    foreach (var list in lists)
    {
        var baseKey = nameCounts[list.Name] == 1 ? list.Name : $"{Sanitize(list.Uses[0].Event)}_{list.Name}";
        var key = baseKey;

        for (var i = 2; enums.ContainsKey(key); i++)
            key = $"{baseKey}_{i}";

        enums[key] = list.Uses[0].Options;
        enumKeys[list.Signature] = key;
    }

    foreach (var use in uses)
    {
        use.Param.Elements("entry").Remove();
        use.Param.SetAttributeValue("enum", enumKeys[Signature(use.Options)]);

        // Self-closing once the entries are gone
        if (!use.Param.Nodes().Any())
            use.Param.RemoveNodes();
    }

    // Defs
    Directory.CreateDirectory(Path.Join(gameDir, "Defs"));

    foreach (var (def, _, doc) in docs)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(false)
        };

        doc.Declaration = new XDeclaration("1.0", "utf-8", null);

        using var writer = XmlWriter.Create(Path.Join(gameDir, "Defs", $"{def}.xml"), settings);
        doc.Save(writer);
    }

    // Enums, in the shape of the param enums
    var enumDir = Path.Join(gameDir, "Enums");
    Directory.CreateDirectory(enumDir);

    foreach (var (key, options) in enums.OrderBy(e => e.Key))
    {
        var node = new JsonObject
        {
            ["Key"] = key,
            ["Names"] = Names(key),
            ["Options"] = new JsonArray(options.Select(o => (JsonNode)new JsonObject
            {
                ["Key"] = o.Value,
                ["Names"] = Names(o.Name)
            }).ToArray())
        };

        File.WriteAllText(Path.Join(enumDir, $"{key}.json"), node.ToJsonString(json));
    }

    // Meta and Annotations, split out of the shared meta. They are matched by field name, as before.
    Directory.CreateDirectory(Path.Join(gameDir, "Meta"));
    Directory.CreateDirectory(Path.Join(gameDir, "Annotations", "English"));

    foreach (var (def, _, _) in docs)
    {
        var meta = new JsonArray();
        var annotations = new JsonArray();

        foreach (var field in metaFields)
        {
            var entry = field!.AsObject();

            var metaEntry = new JsonObject();
            var annotation = new JsonObject();

            foreach (var (name, value) in entry)
            {
                switch (name)
                {
                    case "AltName":
                        annotation["Name"] = value?.DeepClone();
                        break;
                    case "Description":
                        annotation["Description"] = value?.DeepClone();
                        break;
                    case "Field" or "IsPrefix" or "EventTypes":
                        metaEntry[name] = value?.DeepClone();
                        annotation[name] = value?.DeepClone();
                        break;
                    default:
                        metaEntry[name] = value?.DeepClone();
                        break;
                }
            }

            meta.Add(metaEntry);

            if (annotation.ContainsKey("Name") || annotation.ContainsKey("Description"))
            {
                annotation["Name"] ??= "";
                annotation["Description"] ??= "";
                annotations.Add(Reorder(annotation, "Field", "IsPrefix", "EventTypes", "Name", "Description"));
            }
        }

        File.WriteAllText(Path.Join(gameDir, "Meta", $"{def}.json"),
            new JsonObject { ["Fields"] = meta }.ToJsonString(json));

        File.WriteAllText(Path.Join(gameDir, "Annotations", "English", $"{def}.json"),
            new JsonObject { ["Fields"] = annotations }.ToJsonString(json));
    }

    // Check: the Def read back with the enums from disk matches the old template
    var loadedEnums = Directory.EnumerateFiles(enumDir, "*.json")
        .Select(f => JsonNode.Parse(File.ReadAllText(f))!)
        .ToDictionary(
            n => (string)n["Key"]!,
            n => n["Options"]!.AsArray()
                .Select(o => new KeyValuePair<string, string>((string)o!["Names"]![0]!["Text"]!, (string)o["Key"]!))
                .ToList());

    foreach (var (def, file, _) in docs)
    {
        var before = TAE.Template.ReadXMLFile(Path.Join(root, file));
        var after = TAE.Template.ReadXMLFile(Path.Join(gameDir, "Defs", $"{def}.xml"),
            key => loadedEnums.TryGetValue(key, out var o) ? o : null);

        var problems = Compare(before, after);

        Console.WriteLine($"{game.Key}/{def} <- {file}: {(problems.Count == 0 ? "identical" : $"{problems.Count} differences")}");

        foreach (var p in problems.Take(20))
            Console.WriteLine($"    {p}");

        failures += problems.Count;
    }

    Console.WriteLine($"{game.Key}: {enums.Count} enums from {uses.Count} inline lists");
}

Console.WriteLine(failures == 0 ? "All templates identical." : $"{failures} differences.");
return failures == 0 ? 0 : 1;

static JsonArray Names(string text) => new(new JsonObject { ["Language"] = "English", ["Text"] = text });

static JsonObject Reorder(JsonObject o, params string[] order)
{
    var result = new JsonObject();

    foreach (var key in order)
        if (o.TryGetPropertyValue(key, out var v))
            result[key] = v?.DeepClone();

    return result;
}

// One field gives its own name. Several give their shared start without the numbering, or else the first.
static string CommonName(List<string> fields)
{
    if (fields.Count == 1)
        return Sanitize(fields[0]);

    var prefix = fields.Aggregate((a, b) =>
        new string(a.Zip(b).TakeWhile(p => p.First == p.Second).Select(p => p.First).ToArray()));

    prefix = Sanitize(prefix.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_', ' '));

    return prefix.Length >= 3 && prefix != "Unnamed" ? prefix : Sanitize(fields[0]);
}

static string Sanitize(string name)
{
    var s = Regex.Replace(name, "[^A-Za-z0-9_]+", "_").Trim('_');
    return s.Length == 0 ? "Unnamed" : s;
}

static List<string> Compare(TAE.Template before, TAE.Template after)
{
    var problems = new List<string>();

    string Text(object v) => v switch
    {
        null => "null",
        byte[] b => Convert.ToHexString(b),
        _ => $"{v.GetType().Name}:{v}"
    };

    if (before.Game != after.Game)
        problems.Add($"game {before.Game} != {after.Game}");

    foreach (var (bankId, bank) in before)
    {
        if (!after.TryGetValue(bankId, out var newBank))
        {
            problems.Add($"bank {bankId} missing");
            continue;
        }

        if (bank.Count != newBank.Count)
            problems.Add($"bank {bankId}: {bank.Count} events != {newBank.Count}");

        foreach (var (eventId, ev) in bank)
        {
            if (!newBank.TryGetValue(eventId, out var newEv))
            {
                problems.Add($"event {eventId} missing");
                continue;
            }

            if (ev.Name != newEv.Name)
                problems.Add($"event {eventId}: name {ev.Name} != {newEv.Name}");

            var oldParams = ev.ToList();
            var newParams = newEv.ToList();

            if (oldParams.Count != newParams.Count)
            {
                problems.Add($"event {eventId}: {oldParams.Count} params != {newParams.Count}");
                continue;
            }

            for (var i = 0; i < oldParams.Count; i++)
            {
                var (k1, p1) = oldParams[i];
                var (k2, p2) = newParams[i];
                var where = $"event {eventId} '{ev.Name}' param {i} '{k1}'";

                if (k1 != k2) problems.Add($"{where}: key != '{k2}'");
                if (p1.Type != p2.Type) problems.Add($"{where}: type {p1.Type} != {p2.Type}");
                if (p1.NameGroup != p2.NameGroup) problems.Add($"{where}: group differs");
                if (p1.AobLength != p2.AobLength) problems.Add($"{where}: aob length differs");
                if (Text(p1.ValueToAssert) != Text(p2.ValueToAssert)) problems.Add($"{where}: assert {Text(p1.ValueToAssert)} != {Text(p2.ValueToAssert)}");
                if (Text(p1.DefaultValue) != Text(p2.DefaultValue)) problems.Add($"{where}: default {Text(p1.DefaultValue)} != {Text(p2.DefaultValue)}");

                var e1 = p1.EnumEntries?.Select(e => $"{e.Key}={Text(e.Value)}").ToList();
                var e2 = p2.EnumEntries?.Select(e => $"{e.Key}={Text(e.Value)}").ToList();

                if ((e1 == null) != (e2 == null) || (e1 != null && !e1.SequenceEqual(e2)))
                    problems.Add($"{where}: enum entries differ ({e1?.Count ?? 0} vs {e2?.Count ?? 0})");
            }
        }
    }

    return problems;
}
