using Havok;

/// <summary>
/// Phase 1 check for Havok.Core: its header sniffing must agree with this tool's independent sniffer. (Its registries
/// were also compared type for type with HKLib's until Phase 7 retired HKLib; that code is in .archive/Havok Phase 7.)
/// </summary>
public static class CoreCheck
{
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
