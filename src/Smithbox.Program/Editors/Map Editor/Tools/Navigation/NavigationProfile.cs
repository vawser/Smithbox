using System;

namespace StudioCore.Editors.MapEditor;

// What differs between the games the Navigation tool works on, so its model, validator and edits ask a profile rather
// than test for a game. Kept free of the rest of Smithbox, so Tools/NavSurvey can link it.
// See Documentation/Claude/Navigation Editor Other Games Plan.md, sections 4.1 and Phases N2 and N3.

/// <summary>What a game's n NameIDs are built from.</summary>
public enum NavNameIdKind
{
    /// <summary>The map's digits and the entry's position in the list (ER, NR, DB_NT, AC6).</summary>
    Position,

    /// <summary>The map's AA and BB digits and the entry's ModelID (DS3, SDT).</summary>
    ModelId,

    /// <summary>No one rule the tool can follow (BB).</summary>
    None,
}

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

    /// <summary>
    /// Whether the game's jump and drop links are user edges in the mesh (ER, NR, DB_NT, AC6), rather than user edge
    /// setups the game turns into edges when it loads the mesh (DS3, BB, SDT: no file of theirs has a user edge).
    /// </summary>
    public bool HasMeshUserEdges { get; private init; } = true;

    /// <summary>The navmesh sets the game's maps have: n and o, or n alone.</summary>
    public NavmeshSet[] Sets => HasOSet ? new[] { NavmeshSet.N, NavmeshSet.O } : new[] { NavmeshSet.N };

    /// <summary>
    /// The most gates a navmesh can hold: 16 where each gate has 16 cost slots (ER, NR, AC6, DS3, BB: never more in the
    /// game files), no limit where it has one per gate (SDT has navmeshes with 19).
    /// </summary>
    public int MaxGates => GateCostsPerGate ? int.MaxValue : NavGates.MaxGates;

    /// <summary>
    /// Whether the game's navmesh files with faces have a face tree the tool keeps (hk2018's query mediator, hk2019's
    /// cached face iterator). DS3 and BB keep a bare hkcdStaticAabbTree next to the mesh (NavSurvey treecheck: 1,254 and 678 decode, every face in its leaf); SDT has none.
    /// </summary>
    public bool HasFaceTree { get; private init; } = true;

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
        NewEntryUnk50 = 1,
        _nameId = DecimalNameId,
    };

    /// <summary>
    /// The older Havok games (family B: DS3 and BB on hk2014, SDT on hk2016), from NavSurvey nameids and names (Phase N5):
    /// flat map folders, the n set only, no quarters, NVC, map UID or readable ONAV; files named n{map}_{ModelID:D6} as
    /// the binders have them; the running FaceDataIndex. NameIDs follow the ModelID, not the position. Unk4C = 1 entries
    /// can have faces (DS3 137, SDT 72, BB 7,354).
    /// </summary>
    private static NavGameProfile FamilyB(string game, NavNameIdKind nameIds, bool gateCostsPerGate, bool faceTree) => new()
    {
        Game = game,
        MirrorOffset = 2_000_000_000u,
        HasOSet = false,
        HasNvc = false,
        HasReadableOnav = false,
        HasQuarters = false,
        AreaFolders = false,
        HasMapUid = false,
        Unk4CMeansNoFaces = false,
        GateCostsPerGate = gateCostsPerGate,
        NameIdKind = nameIds,
        HasFaceTree = faceTree,
        HasMeshUserEdges = false,
        NewEntryUnk50 = 0,
        _nameId = DecimalNameId,
    };

    /// <summary>Dark Souls III: NameID (AA * 10 + BB) * 1,000,000 + ModelID (1,527 of 1,529), 16 gate cost slots.</summary>
    public static readonly NavGameProfile DS3 = FamilyB("DS3", NavNameIdKind.ModelId, false, true);

    /// <summary>
    /// Bloodborne: no one NameID rule. m21 has ModelID * 10 (+ a digit), the chalice maps (m29_..) ModelID * 1000 and
    /// NameIDs repeated across maps. 16 gate cost slots.
    /// </summary>
    public static readonly NavGameProfile BB = FamilyB("BB", NavNameIdKind.None, false, true);

    /// <summary>Sekiro: DS3's NameID rule (1,301 of 1,304), and gate costs one per gate (2,955 of 2,955), as in DB_NT.</summary>
    public static readonly NavGameProfile SDT = FamilyB("SDT", NavNameIdKind.ModelId, true, false);

    /// <summary>The profile of a game by its short name; Elden Ring's for a game without one of its own.</summary>
    public static NavGameProfile For(string game) => game switch
    {
        "NR" => NR,
        "DB_NT" => DB_NT,
        "AC6" => AC6,
        "DS3" => DS3,
        "BB" => BB,
        "SDT" => SDT,
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

    /// <summary>What a game's n NameIDs are built from.</summary>
    public NavNameIdKind NameIdKind { get; private init; } = NavNameIdKind.Position;

    /// <summary>
    /// The Unk50 a new entry gets: ER and NR 0xFF01 (no quarter), AC6 1 (every entry with a ModelID of 0 or more), DS3,
    /// BB and SDT 0 (all of them). DB_NT's compact entries keep the connected list's offset there, which the writer sets.
    /// </summary>
    public int NewEntryUnk50 { get; private init; } = 0xFF01;

    /// <summary>
    /// The NameID the game files give an n entry, from its position or its ModelID by the game's rule; null in a game
    /// without one rule (BB) or for a ModelID the rule can't hold.
    /// </summary>
    public uint? ExpectedNameId(string nameBase, int position, int modelId)
    {
        switch (NameIdKind)
        {
            case NavNameIdKind.Position:
                return ExpectedNameId(nameBase, position);
            case NavNameIdKind.ModelId:
                if (modelId < 0 || modelId > 999_999)
                    return null;
                var digits = nameBase.Substring(1).Replace("_", "");
                int aa = int.Parse(digits.Substring(0, 2)), bb = int.Parse(digits.Substring(2, 2));
                return (uint)((aa * 10 + bb) * 1_000_000 + modelId);
            default:
                return null;
        }
    }

    /// <summary>
    /// The NameID the game gives placeholders that have no NameID of their own: -1 (DS3 2, SDT 3, BB 21 entries, none
    /// with faces). The NameID rule doesn't apply to them.
    /// </summary>
    public const uint NoNameId = 0xFFFF_FFFFu;

    /// <summary>The NameID of an n entry's o entry.</summary>
    public uint MirrorOf(uint nNameId) => nNameId + MirrorOffset;

    /// <summary>The NameID of an o entry's n entry.</summary>
    public uint UnmirrorOf(uint oNameId) => oNameId - MirrorOffset;
}
