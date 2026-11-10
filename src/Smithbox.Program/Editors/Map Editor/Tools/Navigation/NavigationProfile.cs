using System;

namespace StudioCore.Editors.MapEditor;

// What differs between the games the Navigation tool works on, so its model, validator and edits ask a profile rather
// than test for a game. Kept free of the rest of Smithbox, so Tools/NavSurvey can link it.
// See Documentation/Claude/Navigation Editor Other Games Plan.md, sections 4.1 and Phases N2 and N3.

/// <summary>A game's navigation rules.</summary>
public sealed class NavGameProfile
{
    /// <summary>The game's short name, as Havok profiles and project types name it (ER, NR, DB_NT).</summary>
    public string Game { get; private init; }

    /// <summary>What an o entry's NameID adds to its n entry's: 2,000,000,000 in ER and DB_NT, 2^31 in NR.</summary>
    public uint MirrorOffset { get; private init; }

    /// <summary>
    /// Whether a NameID names the exact map it was given for, _DD included. ER's do, so an open world _10 tile whose
    /// files carry the _00 tile's name can be told from its NameIDs alone. NR's don't: only the binder's file names can.
    /// </summary>
    public bool NameIdNamesTileVersion { get; private init; } = true;

    /// <summary>Whether the game has NVC files joining neighbouring maps.</summary>
    public bool HasNvc { get; private init; } = true;

    /// <summary>
    /// Whether a model's n mesh can be split into quarter (q) files, marked by the high byte of the entry's Unk50. DB_NT
    /// has no q files, and its entries keep the connected navmesh list's offset where ER has Unk50.
    /// </summary>
    public bool HasQuarters { get; private init; } = true;

    /// <summary>Whether map folders are grouped by area, /map/mAA/mAA_BB_CC_DD/ (ER, NR), rather than /map/mAA_BB_CC_DD/ (DB_NT).</summary>
    public bool AreaFolders { get; private init; } = true;

    /// <summary>
    /// Whether an entry's FaceDataIndex is the running total of FaceCount in list order (ER, NR). In DB_NT it is a 0/1
    /// value per model instead, the same in its n and o entries (NavSurvey nameids), whose meaning is unknown.
    /// </summary>
    public bool FaceDataIndexIsRunningTotal { get; private init; } = true;

    /// <summary>
    /// Whether a gate holds one cost per gate of its navmesh (DB_NT), rather than 16 slots, -1 past the navmesh's gates
    /// (ER, NR).
    /// </summary>
    public bool GateCostsPerGate { get; private init; }

    /// <summary>Whether the NVA has a map UID (ER's Entries11), which NVC files repeat. DB_NT's Section 11 is point lists.</summary>
    public bool HasMapUid { get; private init; } = true;

    /// <summary>
    /// Whether every model has an o mesh as well as its n mesh, listed in the second half of the NVA (ER, NR, DB_NT).
    /// AC6 has the n set only.
    /// </summary>
    public bool HasOSet { get; private init; } = true;

    /// <summary>Whether the game's ONAV files are the version SoulsFormats reads (v3: ER, NR). AC6's are v2 (Phase N0.4).</summary>
    public bool HasReadableOnav { get; private init; } = true;

    /// <summary>
    /// Whether Unk4C = 1 marks an entry without faces, as it does in every ER, NR and DB_NT entry (NavSurvey nameids).
    /// In AC6, 42 of 664 entries with Unk4C = 1 have faces, all of them ModelID 900000 and up.
    /// </summary>
    public bool Unk4CMeansNoFaces { get; private init; } = true;

    /// <summary>
    /// Whether gates store the walking cost to the other gates of their navmesh (ER: 21,436 of 59,038 such slots are
    /// costs, the rest -1). AC6 stores none: all 65,896 are -1, and only each gate's own slot is 0 (NavSurvey nameids).
    /// </summary>
    public bool StoresGateCosts { get; private init; } = true;

    /// <summary>The navmesh sets the game's maps have: n and o, or n alone.</summary>
    public NavmeshSet[] Sets => HasOSet ? new[] { NavmeshSet.N, NavmeshSet.O } : new[] { NavmeshSet.N };

    /// <summary>The number of costs each gate of a navmesh with this many gates holds.</summary>
    public int GateCostSlots(int gateCount) => GateCostsPerGate ? gateCount : NavGates.MaxGates;

    private Func<int, int, int, int, bool, int, uint> _nameId;

    private NavGameProfile()
    {
    }

    /// <summary>Decimal NameIDs: ER's, and DB_NT's (which has no open world).</summary>
    private static uint DecimalNameId(int aa, int bb, int cc, int dd, bool openWorld, int position)
        => (openWorld ? 1_000_000_000u : 0u) + (uint)(openWorld ? bb * 10000 + cc * 100 + dd : aa * 10000 + bb * 100 + cc) * 1_000u + (uint)position;

    /// <summary>
    /// Elden Ring: decimal NameIDs, prefix * 1,000,000,000 + map digits * 1,000 + position. The prefix is 0 in legacy
    /// maps, with the digits AABBCC, and 1 in the open world, with BBCCDD; the game tests the odd prefix (0x140BDA2A0).
    /// </summary>
    public static readonly NavGameProfile ER = new()
    {
        Game = "ER",
        MirrorOffset = 2_000_000_000u,
        _nameId = DecimalNameId,
    };

    /// <summary>
    /// Nightreign: NameIDs are bit fields (NavSurvey nameids, all 193 maps). Bit 31 marks the o set and bit 30 the open
    /// world; the low bits are the position in the entry's half:
    /// - open world: 1 &lt;&lt; 30 | (BB * 100 + CC) &lt;&lt; 8 | position, the same for every _DD version of a tile;
    /// - m10 to m19: AABBCC &lt;&lt; 8 | position;
    /// - other legacy maps: 1 &lt;&lt; 29 | (AA * 100 + BB) &lt;&lt; 12 | position.
    /// </summary>
    public static readonly NavGameProfile NR = new()
    {
        Game = "NR",
        MirrorOffset = 0x8000_0000u,
        NameIdNamesTileVersion = false,
        _nameId = (aa, bb, cc, dd, openWorld, position) => openWorld
            ? 1u << 30 | (uint)(bb * 100 + cc) << 8 | (uint)position
            : aa < 20
                ? (uint)(aa * 10000 + bb * 100 + cc) << 8 | (uint)position
                : 1u << 29 | (uint)(aa * 100 + bb) << 12 | (uint)position,
    };

    /// <summary>
    /// The Duskbloods (network test): ER's decimal NameIDs and o offset (NavSurvey nameids, all 9 maps), maps in flat
    /// folders, no quarter files, no NVC or ONAV files, no map UID, gate costs one per gate (36 of 36), and a
    /// FaceDataIndex that is a 0/1 value per model.
    /// </summary>
    public static readonly NavGameProfile DB_NT = new()
    {
        Game = "DB_NT",
        MirrorOffset = 2_000_000_000u,
        HasNvc = false,
        HasQuarters = false,
        AreaFolders = false,
        FaceDataIndexIsRunningTotal = false,
        GateCostsPerGate = true,
        HasMapUid = false,
        _nameId = DecimalNameId,
    };

    /// <summary>
    /// Armored Core VI: ER's decimal NameIDs (NavSurvey nameids, 664 of 664 entries, 20 maps), the running FaceDataIndex,
    /// 16 gate cost slots and a map UID, but the n set only, no NVC files, and ONAV v2. Its meshes are hk2019, whose
    /// face tree hangs off the mesh's cached face iterator rather than a query mediator. Placeholders (no faces) have
    /// Unk4C = 1, often with a negative ModelID.
    /// </summary>
    public static readonly NavGameProfile AC6 = new()
    {
        Game = "AC6",
        MirrorOffset = 2_000_000_000u,
        HasOSet = false,
        HasNvc = false,
        HasReadableOnav = false,
        // Unk50 is 0 or 1 (170 and 494 of 664 entries), never ER's 0xFF01 or quarter marks, and the binders hold no q files
        HasQuarters = false,
        Unk4CMeansNoFaces = false,
        StoresGateCosts = false,
        _nameId = DecimalNameId,
    };

    /// <summary>The profile of a game by its short name; Elden Ring's for a game without one of its own.</summary>
    public static NavGameProfile For(string game) => game switch
    {
        "NR" => NR,
        "DB_NT" => DB_NT,
        "AC6" => AC6,
        _ => ER,
    };

    /// <summary>The profile of the game a Havok profile reads files for.</summary>
    public static NavGameProfile For(Havok.HavokProfile profile) => For(profile?.Game);

    /// <summary>The folder of a map's files, e.g. /map/m10/m10_00_00_00 (ER) or /map/m10_00_00_00 (DB_NT).</summary>
    public string MapFolder(string mapId) => AreaFolders ? $"/map/{mapId.Substring(0, 3)}/{mapId}" : $"/map/{mapId}";

    /// <summary>The NameID the game files give the n entry at a position in a map's navmesh list.</summary>
    public uint ExpectedNameId(string nameBase, int position)
    {
        var digits = nameBase.Substring(1).Replace("_", "");
        int aa = int.Parse(digits.Substring(0, 2)), bb = int.Parse(digits.Substring(2, 2)), cc = int.Parse(digits.Substring(4, 2)), dd = int.Parse(digits.Substring(6, 2));
        return _nameId(aa, bb, cc, dd, NavigationMap.IsOpenWorld(nameBase), position);
    }

    /// <summary>The NameID of an n entry's o entry.</summary>
    public uint MirrorOf(uint nNameId) => nNameId + MirrorOffset;

    /// <summary>The NameID of an o entry's n entry.</summary>
    public uint UnmirrorOf(uint oNameId) => oNameId - MirrorOffset;
}
