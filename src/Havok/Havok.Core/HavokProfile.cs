using System.Collections.Concurrent;
using Havok.Reflection;

namespace Havok;

/// <summary>
/// How one game stores its Havok files: the SDK, the layout defaults used when writing a new file, and the type
/// registry plus overlays that describe its classes. The format of an existing file always comes from the file
/// itself (<see cref="HavokFileInfo" />); the profile only chooses the registry and the defaults.
/// Values come from the Phase 0 survey (Documentation/Claude/Havok Library Survey).
/// </summary>
public sealed class HavokProfile
{
    private static readonly ConcurrentDictionary<string, Lazy<HavokTypeRegistry?>> RegistryCache = new();

    private HavokProfile(string game, HavokSdk sdk, HavokContainer container, int pointerSize, bool bigEndian,
        string? registry = null, params string[] overlays)
    {
        Game = game;
        Sdk = sdk;
        DefaultContainer = container;
        PointerSize = pointerSize;
        BigEndian = bigEndian;
        RegistryFile = registry;
        OverlayFiles = overlays;
    }

    /// <summary>
    /// Smithbox's short game name (matches ProjectType and Andre.Core.Game).
    /// </summary>
    public string Game { get; }

    public HavokSdk Sdk { get; }

    public HavokContainer DefaultContainer { get; }

    public int PointerSize { get; }

    public bool BigEndian { get; }

    /// <summary>
    /// The base registry file name in the Res folder, or null while the SDK has no registry yet (hk2014, hk2015,
    /// hk2016 and the 32-bit packfile SDKs get theirs in Phases 3 to 5).
    /// </summary>
    public string? RegistryFile { get; }

    /// <summary>
    /// Overlay file names in the Res folder, applied in order on top of <see cref="RegistryFile" />.
    /// </summary>
    public IReadOnlyList<string> OverlayFiles { get; }

    public bool IsInScope => Sdk.IsInScope();

    public override string ToString() => $"{Game} ({Sdk})";

    // ---- Profiles, by game ------------------------------------------------------------------------------------

    public static readonly HavokProfile DES = new("DES", HavokSdk.Hk5x, HavokContainer.Packfile, 4, true);
    public static readonly HavokProfile DS1 = new("DS1", HavokSdk.Hk2010, HavokContainer.Packfile, 4, false);
    public static readonly HavokProfile DS1R = new("DS1R", HavokSdk.Hk2015, HavokContainer.Tagfile, 8, false);
    public static readonly HavokProfile DS2 = new("DS2", HavokSdk.Hk2012, HavokContainer.Packfile, 4, false);
    public static readonly HavokProfile DS2S = new("DS2S", HavokSdk.Hk2014, HavokContainer.Packfile, 8, false);
    public static readonly HavokProfile BB = new("BB", HavokSdk.Hk2014, HavokContainer.Packfile, 8, false);
    public static readonly HavokProfile DS3 = new("DS3", HavokSdk.Hk2014, HavokContainer.Packfile, 8, false);
    public static readonly HavokProfile SDT = new("SDT", HavokSdk.Hk2016, HavokContainer.Tagfile, 8, false);

    public static readonly HavokProfile ER = new("ER", HavokSdk.Hk2018, HavokContainer.Tagfile, 8, false,
        "HavokTypeRegistry20180100.xml");

    public static readonly HavokProfile NR = new("NR", HavokSdk.Hk2018, HavokContainer.Tagfile, 8, false,
        "HavokTypeRegistry20180100.xml", "HavokTypeRegistry20180100_NR.xml");

    // DB_NT needs an overlay of its own (survey: HKLib reads 123/710 files with the ER registry). Until it exists the
    // ER registry is used, as Smithbox does today.
    public static readonly HavokProfile DB_NT = new("DB_NT", HavokSdk.Hk2018, HavokContainer.Tagfile, 8, false,
        "HavokTypeRegistry20180100.xml");

    // Assumed to match DB_NT; no DB game data was surveyed. (verify)
    public static readonly HavokProfile DB = new("DB", HavokSdk.Hk2018, HavokContainer.Tagfile, 8, false,
        "HavokTypeRegistry20180100.xml");

    public static readonly HavokProfile AC6 = new("AC6", HavokSdk.Hk2019, HavokContainer.Tagfile, 8, false,
        "HavokTypeRegistry20190100.xml");

    public static IReadOnlyList<HavokProfile> All { get; } =
        [DES, DS1, DS1R, DS2, DS2S, BB, DS3, SDT, ER, NR, DB_NT, DB, AC6];

    /// <summary>
    /// The profile for a game's short name ("ER", "DS3"…), or null.
    /// </summary>
    public static HavokProfile? FromGame(string game) =>
        All.FirstOrDefault(p => p.Game.Equals(game, StringComparison.OrdinalIgnoreCase));

    // ---- Registry ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The folder holding the registry XMLs. Defaults to "Res" beside the running assembly.
    /// </summary>
    public static string ResourceFolder { get; set; } = Path.Join(AppContext.BaseDirectory, "Res");

    /// <summary>
    /// The type registry for this game, with its overlays applied and no C# type binding (Core has no class sets
    /// yet). Loaded once and shared between profiles with the same files. Null if the SDK has no registry yet.
    /// </summary>
    public HavokTypeRegistry? GetRegistry()
    {
        if (RegistryFile is null)
            return null;

        string key = string.Join("|", OverlayFiles.Prepend(RegistryFile));
        return RegistryCache.GetOrAdd(key, _ => new Lazy<HavokTypeRegistry?>(() => LoadRegistry(null))).Value;
    }

    /// <summary>
    /// Loads this game's registry and overlays afresh, binding types to C# classes with <paramref name="resolveType" />
    /// (a class set's identity → type map). Not cached: class sets keep the registry they load. Null if the SDK has no
    /// registry yet.
    /// </summary>
    public HavokTypeRegistry? LoadRegistry(Func<string, Type?>? resolveType)
    {
        if (RegistryFile is null)
            return null;

        return HavokTypeRegistry.Load(Path.Join(ResourceFolder, RegistryFile), resolveType,
            OverlayFiles.Select(o => Path.Join(ResourceFolder, o)).ToArray());
    }
}
