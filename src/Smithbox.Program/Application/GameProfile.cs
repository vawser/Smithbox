using System.Collections.Generic;

namespace StudioCore.Application;

/// <summary>Where a game keeps its asset (object) geometry and textures</summary>
public enum GameAssetLayout
{
    /// <summary>Objects as obj/*.objbnd (or DS2's model/obj)</summary>
    None,

    /// <summary>asset/aeg/aegNNN/… and asset/aet/aetNNN/… (ER, NR and the Duskbloods)</summary>
    Aeg,

    /// <summary>asset/environment/geometry and asset/environment/texture (AC6)</summary>
    Ac6Environment
}

/// <summary>How a game stores map collision (the hit and connect collision of each map)</summary>
public enum MapCollisionLayout
{
    None,

    /// <summary>Loose .hkx files in the map folder (DES, DS1)</summary>
    LooseHkx,

    /// <summary>model/map/{l|h}AA_BB_CC_DD.hkxbhd (DS2)</summary>
    Ds2Binders,

    /// <summary>map/mAA_BB_CC_DD/{l|h}AA_BB_CC_DD.hkxbhd, low and high</summary>
    HkxBhd,

    /// <summary>{l|h|f}AA_BB_CC_DD.hkxbhd in the map's collision folder: low, high and the "fa" layer</summary>
    LayeredHkxBhd
}

/// <summary>How a game stores each map's navmesh</summary>
public enum MapNavmeshLayout
{
    /// <summary>No per-map navmesh binder is located (DS2)</summary>
    None,

    /// <summary>map/mAA_BB_CC_DD/mAA_BB_CC_DD.nvmbnd</summary>
    Nvmbnd,

    /// <summary>map/mAA_BB_CC_DD/mAA_BB_CC_DD.nvmbnd.dcx</summary>
    NvmbndDcx,

    /// <summary>mAA_BB_CC_DD.nvmhktbnd.dcx (Havok navmesh) in the map's collision folder</summary>
    NvmHktBnd
}

/// <summary>
///     What sets a game's files and data apart, named for what it means, so code asks the profile instead of
///     listing games. One per <see cref="ProjectType"/>, from <see cref="For"/> (or <see cref="ProjectEntry.Profile"/>).
/// </summary>
/// <remarks>
///     Add a property when a grouping of games recurs and has one meaning; a check for one game alone can stay
///     <c>profile.Type is ProjectType.X</c>. A new game is a new row in <see cref="Profiles"/>.
/// </remarks>
public sealed class GameProfile
{
    public ProjectType Type { get; }

    /// <summary>
    ///     The name of the game's file dictionary in Assets/File Dictionaries, or null if it has none
    /// </summary>
    public string FileDictionaryName { get; private init; }

    /// <summary>
    ///     DS2's file layout: models, textures and collision in BHD/BDT binders under model/ (model/map, model/chr,
    ///     model/obj, model/parts), graphics params as /filter/*.fltparam, and menu text as loose /menu/*.fmg
    /// </summary>
    public bool Ds2Layout { get; private init; }

    /// <summary>
    ///     The game ships low-detail copies of its files in sd archives, which the file lists leave out
    /// </summary>
    public bool LowDetailArchives { get; private init; }

    /// <summary>Map pieces are loose FLVER files in the map folder, not mapbnd binders</summary>
    public bool LooseMapPieces { get; private init; }

    /// <summary>
    ///     Each map's folder (models, collision, navmesh) sits in an area folder: map/m10/m10_00_00_00. Otherwise
    ///     directly under map: map/m10_00_00_00 (the Duskbloods, as DS3; their area folders only hold colour grading).
    /// </summary>
    public bool MapAreaFolders { get; private init; }

    /// <summary>How map collision is stored</summary>
    public MapCollisionLayout MapCollision { get; private init; }

    /// <summary>How map navmeshes are stored</summary>
    public MapNavmeshLayout Navmesh { get; private init; }

    /// <summary>Where asset geometry and textures are kept</summary>
    public GameAssetLayout AssetLayout { get; private init; }

    /// <summary>
    ///     Each map holds its own assets, so a map's asset map ID is the map ID. Otherwise the area's first map
    ///     (mAA_BB_00_00) holds them, with per-game exceptions.
    /// </summary>
    public bool AssetsPerMap { get; private init; }

    private GameProfile(ProjectType type)
    {
        Type = type;
    }

    private static readonly Dictionary<ProjectType, GameProfile> Profiles = new()
    {
        [ProjectType.Undefined] = new(ProjectType.Undefined),

        [ProjectType.DES] = new(ProjectType.DES)
        {
            FileDictionaryName = "DES-File-Dictionary.json", LooseMapPieces = true,
            MapCollision = MapCollisionLayout.LooseHkx, Navmesh = MapNavmeshLayout.Nvmbnd, AssetsPerMap = true
        },
        [ProjectType.DS1] = new(ProjectType.DS1)
        {
            FileDictionaryName = "DS1-File-Dictionary.json", LooseMapPieces = true,
            MapCollision = MapCollisionLayout.LooseHkx, Navmesh = MapNavmeshLayout.Nvmbnd
        },
        [ProjectType.DS1R] = new(ProjectType.DS1R)
        {
            FileDictionaryName = "DS1R-File-Dictionary.json", LooseMapPieces = true,
            MapCollision = MapCollisionLayout.HkxBhd, Navmesh = MapNavmeshLayout.NvmbndDcx
        },
        [ProjectType.DS2] = new(ProjectType.DS2)
        {
            FileDictionaryName = "DS2-File-Dictionary.json", Ds2Layout = true,
            MapCollision = MapCollisionLayout.Ds2Binders
        },
        [ProjectType.DS2S] = new(ProjectType.DS2S)
        {
            FileDictionaryName = "DS2S-File-Dictionary.json", Ds2Layout = true,
            MapCollision = MapCollisionLayout.Ds2Binders
        },
        [ProjectType.BB] = new(ProjectType.BB)
        {
            FileDictionaryName = "BB-File-Dictionary.json", LooseMapPieces = true,
            MapCollision = MapCollisionLayout.HkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd
        },
        [ProjectType.DS3] = new(ProjectType.DS3)
        {
            FileDictionaryName = "DS3-File-Dictionary.json",
            MapCollision = MapCollisionLayout.HkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd
        },
        [ProjectType.SDT] = new(ProjectType.SDT)
        {
            FileDictionaryName = "SDT-File-Dictionary.json",
            MapCollision = MapCollisionLayout.HkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd
        },

        [ProjectType.ER] = new(ProjectType.ER)
        {
            FileDictionaryName = "ER-File-Dictionary.json", LowDetailArchives = true, MapAreaFolders = true,
            MapCollision = MapCollisionLayout.LayeredHkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd,
            AssetLayout = GameAssetLayout.Aeg, AssetsPerMap = true
        },
        [ProjectType.NR] = new(ProjectType.NR)
        {
            FileDictionaryName = "NR-File-Dictionary.json", LowDetailArchives = true, MapAreaFolders = true,
            MapCollision = MapCollisionLayout.LayeredHkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd,
            AssetLayout = GameAssetLayout.Aeg, AssetsPerMap = true
        },
        [ProjectType.AC6] = new(ProjectType.AC6)
        {
            FileDictionaryName = "AC6-File-Dictionary.json", LowDetailArchives = true, MapAreaFolders = true,
            MapCollision = MapCollisionLayout.LayeredHkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd,
            AssetLayout = GameAssetLayout.Ac6Environment, AssetsPerMap = true
        },
        [ProjectType.DB_NT] = new(ProjectType.DB_NT)
        {
            FileDictionaryName = "DB-NT-File-Dictionary.json", LowDetailArchives = true,
            MapCollision = MapCollisionLayout.LayeredHkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd,
            AssetLayout = GameAssetLayout.Aeg, AssetsPerMap = true
        },
        [ProjectType.DB] = new(ProjectType.DB)
        {
            FileDictionaryName = "DB-File-Dictionary.json", LowDetailArchives = true,
            MapCollision = MapCollisionLayout.LayeredHkxBhd, Navmesh = MapNavmeshLayout.NvmHktBnd,
            AssetLayout = GameAssetLayout.Aeg, AssetsPerMap = true
        },
    };

    /// <summary>The game's profile. A type without a row gets an empty one (every property false).</summary>
    public static GameProfile For(ProjectType type)
    {
        return Profiles.TryGetValue(type, out var profile) ? profile : new GameProfile(type);
    }
}
