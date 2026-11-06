namespace Havok.Formats.Tagfile;

/// <summary>
/// What differs between the binary tagfiles of the SDK versions. Reading accepts every variant (section names,
/// padding, partial type bodies, member extra values, an absent PTCH), so the dialect mostly decides how files are
/// written.
///
/// The 2018 and 2019 HKLib serializers were two copies of one engine (see the Unified Havok Library Plan, Phase 2).
/// The true format differences are <see cref="SdkVersion" />, the string section names and their padding byte. The
/// rest are writer behaviours HKLib's 2019 copy gained to reproduce AC6 files byte for byte; they are switches here so
/// they can be measured on the other SDKs.
/// </summary>
public sealed record TagfileDialect
{
    /// <summary>
    /// The SDKV string, e.g. "20180100".
    /// </summary>
    public required string SdkVersion { get; init; }

    public required HavokSdk Sdk { get; init; }

    /// <summary>
    /// Type and field string section ids: TSTR/FSTR (2018) or TST1/FST1 (2019).
    /// </summary>
    public required string TypeStringsSection { get; init; }

    public required string FieldStringsSection { get; init; }

    /// <summary>
    /// Type name and type body section ids: TNA1/TBDY, or TNAM/TBOD in 2015 files.
    /// </summary>
    public string TypeNamesSection { get; init; } = "TNA1";

    public string TypeBodiesSection { get; init; } = "TBDY";

    /// <summary>
    /// The byte string sections are padded with: 0x00 (2018) or 0xFF (2019).
    /// </summary>
    public required byte StringPadding { get; init; }

    // ---- Writer behaviours ----------------------------------------------------------------------------------------

    /// <summary>
    /// Write a TCRF compendium reference when the loaded compendium holds every type needed, instead of a TYPE section.
    /// </summary>
    public bool WriteCompendiumReference { get; init; } = true;

    /// <summary>
    /// Write back the TYPE section a root object was read with, while it holds every type needed.
    /// </summary>
    public bool ReuseTypeSection { get; init; } = true;

    /// <summary>
    /// Number and write items in SDK order: pointed-to objects in the order found, each followed by the arrays and
    /// strings it owns breadth first, references numbered by member then array element with strings last.
    /// When false, items are written in the order they were enqueued (HKLib 2018).
    /// </summary>
    public bool SdkItemOrder { get; init; } = true;

    /// <summary>
    /// Align array items to at least 16 bytes (strings excepted).
    /// </summary>
    public bool AlignArraysTo16 { get; init; } = true;

    /// <summary>
    /// Pad the DATA section to 16 bytes.
    /// </summary>
    public bool PadDataTo16 { get; init; } = true;

    /// <summary>
    /// Leave PTCH out when there are no pointers, and order its entries by type index.
    /// </summary>
    public bool CompactPatches { get; init; } = true;

    /// <summary>
    /// Write equal strings once. When false, a string is written once per string object, and reading gives the strings
    /// of one item the same object, so a rewrite shares exactly the strings the original shared (2018 files store
    /// equal strings separately). Empty strings are always shared, since .NET has a single empty string.
    /// </summary>
    public bool MergeEqualStrings { get; init; } = true;

    /// <summary>
    /// With <see cref="MergeEqualStrings" /> false: whether empty strings share one item. .NET has a single empty
    /// string object, so reading can't tell shared empty strings from separate ones; this picks one for the SDK.
    /// </summary>
    public bool ShareEmptyStrings { get; init; } = true;

    /// <summary>
    /// Record hkRelArray pointers in PTCH (2018 does; 2019 only patches hkRelArray32).
    /// </summary>
    public bool PatchRelArrays { get; init; }

    /// <summary>
    /// Members flagged 0x80 carry an extra value after their flags (2019). Values are remembered per dialect and
    /// written back.
    /// </summary>
    public bool MemberExtraValues { get; init; }

    /// <summary>
    /// The 2018 dialect: the writer behaviours of HKLib's 2019 serializer, plus what the round-trip survey found 2018
    /// files do differently (separate equal strings, patched hkRelArrays).
    /// </summary>
    public static TagfileDialect Hk2018 { get; } = new()
    {
        SdkVersion = "20180100",
        Sdk = HavokSdk.Hk2018,
        TypeStringsSection = "TSTR",
        FieldStringsSection = "FSTR",
        StringPadding = 0x00,
        MergeEqualStrings = false,
        PatchRelArrays = true
    };

    /// <summary>
    /// The 2018 dialect exactly as HKLib's 2018 serializer writes it: always a TYPE section built from the registry,
    /// items in enqueue order, no extra alignment, PTCH always present. Kept to check parity with HKLib.
    /// </summary>
    public static TagfileDialect Hk2018Legacy { get; } = Hk2018 with
    {
        WriteCompendiumReference = false,
        ReuseTypeSection = false,
        SdkItemOrder = false,
        AlignArraysTo16 = false,
        PadDataTo16 = false,
        CompactPatches = false,
        MergeEqualStrings = true,
        PatchRelArrays = false
    };

    /// <summary>
    /// Sekiro's 2016 files: laid out like 2018 files (the survey's tagfile layouts), so the 2018 dialect with its SDKV.
    /// </summary>
    public static TagfileDialect Hk2016 { get; } = Hk2018 with
    {
        SdkVersion = "20160200",
        Sdk = HavokSdk.Hk2016,
        ShareEmptyStrings = false
    };

    /// <summary>
    /// DS1R's 2015 files: the 2018 layout with the type name and body sections called TNAM and TBOD.
    /// </summary>
    public static TagfileDialect Hk2015 { get; } = Hk2018 with
    {
        SdkVersion = "20150100",
        Sdk = HavokSdk.Hk2015,
        TypeNamesSection = "TNAM",
        TypeBodiesSection = "TBOD"
    };

    /// <summary>
    /// The 2019 dialect, as HKLib's 2019 serializer writes it (byte-identical on AC6).
    /// </summary>
    public static TagfileDialect Hk2019 { get; } = new()
    {
        SdkVersion = "20190100",
        Sdk = HavokSdk.Hk2019,
        TypeStringsSection = "TST1",
        FieldStringsSection = "FST1",
        StringPadding = 0xFF,
        MemberExtraValues = true
    };
}
