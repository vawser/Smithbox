using Havok;
using CoreType = Havok.Reflection.HavokType;

/// <summary>
/// Phase 1 checks for Havok.Core: its registries must describe exactly the types HKLib's do, and its header sniffing
/// must agree with this tool's independent sniffer.
/// </summary>
public static class CoreCheck
{
    /// <summary>
    /// Compares Core's registry for each tagfile profile with HKLib's. Returns report lines.
    /// </summary>
    public static List<string> CompareRegistries()
    {
        var lines = new List<string>();

        void Compare(HavokProfile profile, IEnumerable<HKLibTypeView> hklib)
        {
            var core = profile.GetRegistry();
            var hk = hklib.ToList();
            var errors = new List<string>();
            if (core == null) { lines.Add($"- **{profile.Game}**: no Core registry"); return; }
            if (core.Count != hk.Count) errors.Add($"count {core.Count} vs HKLib {hk.Count}");

            foreach (var t in hk)
            {
                var c = core.GetType(t.Identity);
                if (c == null) { errors.Add($"`{t.Identity}` missing"); continue; }
                var d = Diff(c, t);
                if (d != null) errors.Add($"`{t.Identity}`: {d}");
            }

            lines.Add(errors.Count == 0
                ? $"- **{profile.Game}** ({core.SdkVersion}{(core.Overlays.Count > 0 ? " + " + string.Join(", ", core.Overlays) : "")}): {core.Count} types, all identical to HKLib"
                : $"- **{profile.Game}**: {errors.Count} differences: {string.Join("; ", errors.Take(10))}");
        }

        Compare(HavokProfile.ER, HKLib.Reflection.hk2018.HavokTypeRegistry.Instance.Types.Select(HKLibTypeView.From2018));
        Compare(HavokProfile.NR, HKLib.Reflection.hk2018.HavokTypeRegistry.Nightreign.Types.Select(HKLibTypeView.From2018));
        Compare(HavokProfile.AC6, HKLib.Reflection.hk2019.HavokTypeRegistry.Instance.Types.Select(HKLibTypeView.From2019));
        return lines;
    }

    /// <summary>
    /// The parts of a type that serialization depends on, flattened to strings so the 2018/2019 HKLib types and the
    /// Core type compare the same way.
    /// </summary>
    public sealed record HKLibTypeView(string Identity, string Summary)
    {
        public static HKLibTypeView From2018(HKLib.Reflection.hk2018.HavokType t) => new(t.Identity, Summarise(
            t.Name, t.Size, t.Alignment, (int)t.Flags, (int)t.Optionals, t.Format, t.Version, t.Hash,
            t.Parent?.Identity, t.SubType?.Identity,
            t.TemplateParameters.Select(p => $"{p.Name}={p.Value?.ToString() ?? p.Type?.Identity}"),
            t.Fields.Select(f => $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            t.Properties.Select(f => f == null ? "null" : $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            t.Interfaces.Select(i => $"{i.Type.Identity}@{i.Offset}"), t.Presets));

        public static HKLibTypeView From2019(HKLib.Reflection.hk2019.HavokType t) => new(t.Identity, Summarise(
            t.Name, t.Size, t.Alignment, (int)t.Flags, (int)t.Optionals, t.Format, t.Version, t.Hash,
            t.Parent?.Identity, t.SubType?.Identity,
            t.TemplateParameters.Select(p => $"{p.Name}={p.Value?.ToString() ?? p.Type?.Identity}"),
            t.Fields.Select(f => $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            t.Properties.Select(f => f == null ? "null" : $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            t.Interfaces.Select(i => $"{i.Type.Identity}@{i.Offset}"), t.Presets));
    }

    private static string Summarise(string name, int size, int align, int flags, int optionals, int format, ulong version,
        uint? hash, string parent, string subType, IEnumerable<string> template, IEnumerable<string> fields,
        IEnumerable<string> properties, IEnumerable<string> interfaces, IEnumerable<string> presets) =>
        $"name={name} size={size} align={align} flags={flags} opt={optionals} format={format} version={version} hash={hash} " +
        $"parent={parent} sub={subType} template=[{string.Join(",", template)}] fields=[{string.Join(",", fields)}] " +
        $"props=[{string.Join(",", properties)}] ifaces=[{string.Join(",", interfaces)}] presets=[{string.Join(",", presets)}]";

    private static string Diff(CoreType c, HKLibTypeView hk)
    {
        var core = Summarise(c.Name, c.Size, c.Alignment, (int)c.Flags, (int)c.Optionals, c.Format, c.Version, c.Hash,
            c.Parent?.Identity, c.SubType?.Identity,
            c.TemplateParameters.Select(p => $"{p.Name}={p.Value?.ToString() ?? p.Type?.Identity}"),
            c.Fields.Select(f => $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            c.Properties.Select(f => f == null ? "null" : $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"),
            c.Interfaces.Select(i => $"{i.Type.Identity}@{i.Offset}"), c.Presets);
        if (core == hk.Summary) return null;

        // First differing token, for the report
        var a = core.Split(' ');
        var b = hk.Summary.Split(' ');
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
            if (a[i] != b[i]) return $"Core `{Trim(a[i])}` vs HKLib `{Trim(b[i])}`";
        return "length differs";
    }

    private static string Trim(string s) => s.Length > 120 ? s[..120] + "…" : s;

    /// <summary>
    /// Null if Core's sniff agrees with the survey's, otherwise what differs.
    /// </summary>
    public static string CompareSniff(byte[] data, SniffInfo survey)
    {
        var core = HavokFileInfo.Read(data);
        var errors = new List<string>();

        var expected = survey.Container switch
        {
            "Packfile" => HavokContainer.Packfile,
            "Tagfile" => HavokContainer.Tagfile,
            "Compendium" => HavokContainer.Compendium,
            "OldTagfile" => HavokContainer.LegacyTagfile,
            _ => HavokContainer.Unknown
        };
        if (core.Container != expected && !(expected == HavokContainer.Unknown && core.Container == HavokContainer.Xml))
            errors.Add($"container {core.Container} vs {survey.Container}");
        if (core.SdkVersion != survey.Sdk) errors.Add($"sdk '{core.SdkVersion}' vs '{survey.Sdk}'");
        if (expected == HavokContainer.Packfile)
        {
            if (core.HeaderVersion != survey.HeaderVersion) errors.Add($"header v{core.HeaderVersion} vs v{survey.HeaderVersion}");
            if (core.PointerSize != survey.PointerSize) errors.Add($"pointer {core.PointerSize} vs {survey.PointerSize}");
            if (core.BigEndian != survey.BigEndian) errors.Add("endian");
            if (!core.ClassNames.SequenceEqual(survey.Classes)) errors.Add($"class names {core.ClassNames.Count} vs {survey.Classes.Count}");
        }
        if (core.UsesCompendium != survey.UsesCompendium) errors.Add("uses compendium");
        if (core.CompendiumRef != survey.CompendiumRef) errors.Add("compendium ref");
        if (!core.CompendiumIds.SequenceEqual(survey.CompendiumIds)) errors.Add("compendium ids");
        if (expected != HavokContainer.Unknown && core.Sdk == HavokSdk.Unknown && survey.Sdk != "") errors.Add($"SDK '{survey.Sdk}' not mapped");

        return errors.Count == 0 ? null : string.Join(", ", errors);
    }
}
