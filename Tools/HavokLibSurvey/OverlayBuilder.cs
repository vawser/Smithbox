using System.Xml.Linq;
using Havok.Reflection;

/// <summary>
/// --buildoverlay: an overlay for a game whose files describe types that differ from its SDK's base registry (DB_NT on
/// the 2018 registry). Changed types replace the base type with the same Id; types the base lacks are added
/// (New="true", Ids continuing from the base's highest). Written in the base registry's runtime optionals encoding.
/// </summary>
public static class OverlayBuilder
{
    // serialized optionals bit -> 2018 runtime bit (the inverse of the registry loader's map)
    private static readonly int[] RuntimeBit2018 = [0, 1, 4, 23, 24, 26, 17, 28];

    /// <summary>
    /// The serialized optionals in runtime form, keeping the base type's runtime-only bits.
    /// </summary>
    private static int ToRuntime(int serialized, int baseRuntime)
    {
        int runtime = baseRuntime;
        for (int s = 0; s < RuntimeBit2018.Length; s++)
        {
            int bit = 1 << RuntimeBit2018[s];
            runtime = (serialized & (1 << s)) != 0 ? runtime | bit : runtime & ~bit;
        }
        return runtime;
    }

    public static List<string> Write(string path, string basePath, string game, IReadOnlyDictionary<string, HavokType> fileTypes,
        IReadOnlyDictionary<string, uint> hashes, Func<HavokType, string> describe, HavokTypeRegistry baseRegistry)
    {
        var lines = new List<string>();
        var baseXml = XElement.Load(basePath);
        var baseElements = baseXml.Elements().SelectMany(x => x.Elements()).ToDictionary(e => int.Parse(e.Attribute("Id")!.Value));

        // identity of each base element, computed as HavokTypeBuilder does: Name<arg, arg>
        var identities = new Dictionary<int, string>();
        string Identity(int id)
        {
            if (identities.TryGetValue(id, out var cached)) return cached;
            var e = baseElements[id];
            var name = e.Attribute("Name")!.Value;
            var ps = e.Element("Template")?.Elements().ToList();
            var identity = ps == null || ps.Count == 0
                ? name
                : name + "<" + string.Join(", ", ps.Select(p => p.Attribute("Kind")!.Value == "Type"
                    ? Identity(int.Parse(p.Attribute("Value")!.Value))
                    : p.Attribute("Value")!.Value)) + ">";
            identities[id] = identity;
            return identity;
        }

        var baseIds = new Dictionary<string, int>();
        foreach (var id in baseElements.Keys.OrderBy(x => x)) baseIds.TryAdd(Identity(id), id);

        // which file types differ from the base, and which the base lacks
        var changed = new List<HavokType>();
        var added = new List<HavokType>();
        foreach (var t in fileTypes.Values.OrderBy(t => t.Identity, StringComparer.Ordinal))
        {
            var reg = baseRegistry.GetType(t.Identity);
            if (reg == null)
            {
                added.Add(t);
                continue;
            }

            bool hashDiffers = hashes.TryGetValue(t.Identity, out var h) && reg.Hash != h;
            if (describe(t) != describe(reg) || hashDiffers) changed.Add(t);
        }

        // and any further new types those refer to
        var queue = new Queue<HavokType>(changed.Concat(added));
        var addedSet = added.Select(t => t.Identity).ToHashSet();
        void Refer(HavokType r)
        {
            if (r == null || baseIds.ContainsKey(r.Identity) || !addedSet.Add(r.Identity)) return;
            added.Add(r);
            queue.Enqueue(r);
        }

        while (queue.TryDequeue(out var t))
        {
            Refer(t.Parent);
            Refer(t.SubType);
            foreach (var p in t.TemplateParameters) Refer(p.Type);
            foreach (var f in t.Fields) Refer(f.Type);
            foreach (var i in t.Interfaces) Refer(i.Type);
        }

        int nextId = baseElements.Count + 1;
        var newIds = added.ToDictionary(t => t.Identity, _ => nextId++);
        int IdOf(HavokType t) => t == null ? 0 : baseIds.TryGetValue(t.Identity, out var b) ? b : newIds[t.Identity];

        XElement Element(HavokType t, XElement baseElement, bool isNew)
        {
            int baseOptionals = baseElement == null ? 0 : int.Parse(baseElement.Attribute("Optionals")!.Value);
            var e = new XElement("HavokType",
                new XAttribute("Id", IdOf(t)),
                new XAttribute("Name", t.Name),
                new XAttribute("Parent", IdOf(t.Parent)),
                new XAttribute("Optionals", ToRuntime((int)t.Optionals, baseOptionals)),
                new XAttribute("Format", t.Format),
                new XAttribute("SubType", IdOf(t.SubType)),
                new XAttribute("Size", t.Size),
                new XAttribute("Alignment", t.Alignment & 0xFFF),
                new XAttribute("Flags", (int)t.Flags),
                new XAttribute("Version", t.Version));
            if (isNew) e.Add(new XAttribute("New", "true"));

            // the files' hash, else the base's (files only store the hashes of top level types, so this may be stale)
            if (hashes.TryGetValue(t.Identity, out var hash)) e.Add(new XAttribute("Hash", hash));
            else if (baseElement?.Attribute("Hash") is { } baseHash) e.Add(new XAttribute("Hash", baseHash.Value));

            if (t.TemplateParameters.Count > 0)
                e.Add(new XElement("Template", t.TemplateParameters.Select(p => new XElement("Parameter",
                    new XAttribute("Name", p.Name),
                    new XAttribute("Kind", p.Type is null ? "Value" : "Type"),
                    new XAttribute("Value", p.Type is null ? p.Value ?? 0 : IdOf(p.Type))))));

            // fields from the file; the base's property members are kept when the file has as many properties
            var members = t.Fields.Skip(t.Parent?.Fields.Count ?? 0).Select(f => new XElement("Member",
                new XAttribute("Name", f.Name),
                new XAttribute("Offset", f.Offset),
                new XAttribute("Type", IdOf(f.Type)),
                new XAttribute("Flags", (int)f.Flags))).ToList();
            var baseProperties = baseElement?.Element("Members")?.Elements()
                .Where(m => (int.Parse(m.Attribute("Flags")!.Value) & (int)HavokType.Member.MemberFlags.Property) != 0)
                .ToList() ?? new List<XElement>();
            if (baseProperties.Count == t.Properties.Count) members.AddRange(baseProperties.Select(p => new XElement(p)));
            else if (t.Properties.Count > 0) e.Add(new XAttribute("PropertyCount", t.Properties.Count));
            if (members.Count > 0) e.Add(new XElement("Members", members));

            var interfaces = t.Interfaces.Skip(t.Parent?.Interfaces.Count ?? 0).ToList();
            if (interfaces.Count > 0)
                e.Add(new XElement("Interfaces", interfaces.Select(i => new XElement("Interface",
                    new XAttribute("Type", IdOf(i.Type)),
                    new XAttribute("Offset", i.Offset)))));

            // enum names: files don't store them, so keep the base's
            if (baseElement?.Element("Presets") is { } presets) e.Add(new XElement(presets));
            return e;
        }

        var root = new XElement("HavokTypeRegistryOverlay",
            new XAttribute("Base", baseXml.Attribute("Version")?.Value ?? ""),
            new XAttribute("Game", game),
            new XComment($" {game} changes to {Path.GetFileName(basePath)}, built by the buildoverlay mode of Tools/HavokLibSurvey from the type sections of {game}'s files. "));
        foreach (var t in changed) root.Add(Element(t, baseElements[baseIds[t.Identity]], false));
        foreach (var t in added.OrderBy(t => newIds[t.Identity])) root.Add(Element(t, null, true));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        root.Save(path);

        lines.Add($"- Wrote `{Path.GetFileName(path)}`: {changed.Count} changed types, {added.Count} new types (Ids {baseElements.Count + 1}–{nextId - 1}).");
        foreach (var t in added) lines.Add($"  - new: `{t.Identity}`");
        return lines;
    }
}
