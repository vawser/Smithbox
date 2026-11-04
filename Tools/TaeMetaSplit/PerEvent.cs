using SoulsFormats;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Turns the shared Meta and Annotations of each def, whose entries match fields by name (with prefixes and
/// event type limits), into per-event lists keyed by event ID and exact field name. Each field of each event
/// gets the entry the shared lists resolved to, and the result is checked against the old lookup.
///
/// Usage: TaeMetaSplit per-event &lt;Assets/TAE folder&gt;
/// </summary>
static class PerEvent
{
    static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static readonly string[] MatchKeys = ["Field", "IsPrefix", "EventTypes"];

    public static int Run(string root)
    {
        root = Path.GetFullPath(root);
        var failures = 0;

        foreach (var gameDir in Directory.EnumerateDirectories(root))
        {
            var enums = LoadEnums(Path.Join(gameDir, "Enums"));

            foreach (var defPath in Directory.EnumerateFiles(Path.Join(gameDir, "Defs"), "*.xml"))
            {
                var def = Path.GetFileNameWithoutExtension(defPath);
                var bank = TAE.Template.ReadXMLFile(defPath, k => enums.TryGetValue(k, out var o) ? o : null)[0];

                var metaPath = Path.Join(gameDir, "Meta", $"{def}.json");
                var annotationPath = Path.Join(gameDir, "Annotations", "English", $"{def}.json");

                var sharedMeta = ReadShared(metaPath);
                var sharedAnnotations = ReadShared(annotationPath);

                var metaEvents = new JsonArray();
                var annotationEvents = new JsonArray();

                foreach (var (eventId, ev) in bank.OrderBy(e => e.Key))
                {
                    var metaFields = new JsonArray();
                    var annotationFields = new JsonArray();

                    foreach (var parameter in ev.Values)
                    {
                        // Fixed values aren't shown, so they have nothing to describe
                        if (parameter.ValueToAssert != null)
                            continue;

                        var meta = Find(sharedMeta, eventId, parameter.Name);

                        if (meta != null)
                        {
                            var entry = new JsonObject { ["Field"] = parameter.Name };

                            foreach (var (k, v) in meta)
                                if (!MatchKeys.Contains(k))
                                    entry[k] = v?.DeepClone();

                            metaFields.Add(entry);
                        }

                        var annotation = Find(sharedAnnotations, eventId, parameter.Name);

                        annotationFields.Add(new JsonObject
                        {
                            ["Field"] = parameter.Name,
                            ["Name"] = (string)annotation?["Name"] ?? "",
                            ["Description"] = (string)annotation?["Description"] ?? ""
                        });
                    }

                    if (metaFields.Count > 0)
                        metaEvents.Add(new JsonObject { ["ID"] = eventId, ["Event"] = ev.Name, ["Fields"] = metaFields });

                    annotationEvents.Add(new JsonObject { ["ID"] = eventId, ["Event"] = ev.Name, ["Fields"] = annotationFields });
                }

                var metaOut = new JsonObject { ["Events"] = metaEvents };
                var annotationOut = new JsonObject { ["Events"] = annotationEvents };

                // Check every shown field resolves to the same values as before
                var problems = 0;

                foreach (var (eventId, ev) in bank)
                {
                    foreach (var parameter in ev.Values.Where(p => p.ValueToAssert == null))
                    {
                        var oldMeta = Strip(Find(sharedMeta, eventId, parameter.Name));
                        var newMeta = Strip(FindPerEvent(metaOut, eventId, parameter.Name));

                        if (oldMeta != newMeta)
                        {
                            problems++;
                            Console.WriteLine($"    meta {eventId} {parameter.Name}: {oldMeta} != {newMeta}");
                        }

                        var oldAnn = Find(sharedAnnotations, eventId, parameter.Name);
                        var newAnn = FindPerEvent(annotationOut, eventId, parameter.Name);

                        if (((string)oldAnn?["Name"] ?? "") != (string)newAnn?["Name"] ||
                            ((string)oldAnn?["Description"] ?? "") != (string)newAnn?["Description"])
                        {
                            problems++;
                            Console.WriteLine($"    annotation {eventId} {parameter.Name} differs");
                        }
                    }
                }

                File.WriteAllText(metaPath, metaOut.ToJsonString(Json));
                File.WriteAllText(annotationPath, annotationOut.ToJsonString(Json));

                var withMeta = metaEvents.Sum(e => e!["Fields"]!.AsArray().Count);
                var described = annotationEvents.Sum(e => e!["Fields"]!.AsArray().Count(f => ((string)f!["Description"]).Length > 0));

                Console.WriteLine($"{Path.GetFileName(gameDir)}/{def}: {bank.Count} events, {withMeta} fields with meta, " +
                    $"{described} described, {(problems == 0 ? "identical" : $"{problems} differences")}");

                failures += problems;
            }
        }

        Console.WriteLine(failures == 0 ? "All lookups identical." : $"{failures} differences.");
        return failures == 0 ? 0 : 1;
    }

    static List<JsonObject> ReadShared(string path)
    {
        if (!File.Exists(path))
            return [];

        var node = JsonNode.Parse(File.ReadAllText(path))!;

        if (node["Fields"] is not JsonArray fields)
            throw new InvalidOperationException($"{path} is not in the shared layout (already converted?)");

        return fields.Select(f => f!.AsObject()).ToList();
    }

    /// <summary>
    /// The old lookup: entries for the event type go first, and within each, an exact match before a prefix
    /// </summary>
    static JsonObject Find(List<JsonObject> fields, int eventType, string name)
    {
        bool IsExact(JsonObject e) => ((string)e["Field"]).Equals(name, StringComparison.OrdinalIgnoreCase);
        bool IsPrefix(JsonObject e) => (bool?)e["IsPrefix"] == true && name.StartsWith((string)e["Field"], StringComparison.OrdinalIgnoreCase);

        List<int> Types(JsonObject e) => e["EventTypes"] is JsonArray a ? a.Select(x => (int)x!).ToList() : [];
        bool IsForType(JsonObject e) => Types(e).Contains(eventType);
        bool IsForAll(JsonObject e) => Types(e).Count == 0;

        return fields.FirstOrDefault(e => IsForType(e) && IsExact(e))
            ?? fields.FirstOrDefault(e => IsForType(e) && IsPrefix(e))
            ?? fields.FirstOrDefault(e => IsForAll(e) && IsExact(e))
            ?? fields.FirstOrDefault(e => IsForAll(e) && IsPrefix(e));
    }

    static JsonObject FindPerEvent(JsonObject file, int eventId, string name)
    {
        var ev = file["Events"]!.AsArray().FirstOrDefault(e => (int)e!["ID"] == eventId);

        return ev?["Fields"]!.AsArray().Select(f => f!.AsObject()).FirstOrDefault(f => (string)f["Field"] == name);
    }

    // The values of an entry, without how it was matched
    static string Strip(JsonObject entry)
    {
        if (entry == null)
            return "null";

        return string.Join(";", entry.Where(kv => !MatchKeys.Contains(kv.Key))
            .OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}={kv.Value?.ToJsonString()}"));
    }

    static Dictionary<string, List<KeyValuePair<string, string>>> LoadEnums(string folder)
    {
        if (!Directory.Exists(folder))
            return new();

        return Directory.EnumerateFiles(folder, "*.json")
            .Select(f => JsonNode.Parse(File.ReadAllText(f))!)
            .ToDictionary(
                n => (string)n["Key"]!,
                n => n["Options"]!.AsArray()
                    .Select(o => new KeyValuePair<string, string>((string)o!["Names"]![0]!["Text"]!, (string)o["Key"]!))
                    .ToList());
    }
}
