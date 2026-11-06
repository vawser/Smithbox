namespace Havok;

/// <summary>
/// Implemented by every generated Havok class, in every SDK's class set.
/// </summary>
public interface IHavokObject { }

/// <summary>
/// The container format of a Havok file.
/// </summary>
public enum HavokContainer
{
    Unknown,

    /// <summary>Packfile: magic 0x57E0E057, three sections with fixups. DeS to DS3.</summary>
    Packfile,

    /// <summary>Binary tagfile: TAG0 root section. DS1R, SDT, ER, NR, AC6, DB.</summary>
    Tagfile,

    /// <summary>Type compendium: TCM0 root section, holding the types that tagfiles refer to by id.</summary>
    Compendium,

    /// <summary>Old binary tagfile (magic 0xCAB00D1E or 0xD011FACE): DS3/SDT behbnd .hkt files. Not supported.</summary>
    LegacyTagfile,

    /// <summary>Havok XML (2018 only).</summary>
    Xml
}

/// <summary>
/// A Havok SDK version, as far as the file layout is concerned.
/// </summary>
public enum HavokSdk
{
    Unknown,

    /// <summary>Havok 4.5 (some DeS animations). No current library reads it.</summary>
    Hk4x,

    /// <summary>Havok 5.1 / 5.5 (DeS). 32-bit, big-endian.</summary>
    Hk5x,

    /// <summary>hk_2010.2.0 (DS1). 32-bit.</summary>
    Hk2010,

    /// <summary>hk_2012.1.0 (DS2). 32-bit. No current library reads it.</summary>
    Hk2012,

    /// <summary>hk_2014.1.0 (DS2S, BB, DS3, some SDT).</summary>
    Hk2014,

    /// <summary>20150100 tagfiles (DS1R).</summary>
    Hk2015,

    /// <summary>20160200 tagfiles (SDT, a few ER/AC6 files).</summary>
    Hk2016,

    /// <summary>20180100 tagfiles (ER, NR, DB_NT, DB).</summary>
    Hk2018,

    /// <summary>20190100 tagfiles (AC6).</summary>
    Hk2019
}

public static class HavokSdkExtensions
{
    /// <summary>
    /// Maps a packfile ContentsVersionString ("hk_2014.1.0-r1", "Havok-5.5.0-r1") or a tagfile SDKV ("20180100") to
    /// its SDK.
    /// </summary>
    public static HavokSdk FromVersionString(string version)
    {
        if (string.IsNullOrEmpty(version))
            return HavokSdk.Unknown;

        if (version.StartsWith("Havok-4.")) return HavokSdk.Hk4x;
        if (version.StartsWith("Havok-5.")) return HavokSdk.Hk5x;
        if (version.StartsWith("hk_2010")) return HavokSdk.Hk2010;
        if (version.StartsWith("hk_2012")) return HavokSdk.Hk2012;
        if (version.StartsWith("hk_2014")) return HavokSdk.Hk2014;

        return version switch
        {
            _ when version.StartsWith("2015") => HavokSdk.Hk2015,
            _ when version.StartsWith("2016") => HavokSdk.Hk2016,
            _ when version.StartsWith("2018") => HavokSdk.Hk2018,
            _ when version.StartsWith("2019") => HavokSdk.Hk2019,
            _ => HavokSdk.Unknown
        };
    }

    /// <summary>
    /// Whether one of the current libraries reads this SDK, so the unified library must too (plan §1.3).
    /// </summary>
    public static bool IsInScope(this HavokSdk sdk) =>
        sdk is not (HavokSdk.Unknown or HavokSdk.Hk4x or HavokSdk.Hk2012);
}
