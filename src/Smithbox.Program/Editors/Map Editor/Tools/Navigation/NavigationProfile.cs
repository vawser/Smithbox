using System;

namespace StudioCore.Editors.MapEditor;

// What differs between the games the Navigation tool works on, so its model, validator and edits ask a profile rather
// than test for a game. Kept free of the rest of Smithbox, so Tools/NavSurvey can link it.
// See Documentation/Claude/Navigation Editor Other Games Plan.md, sections 4.1 and Phase N2.

/// <summary>A game's navigation rules.</summary>
public sealed class NavGameProfile
{
    /// <summary>The game's short name, as Havok profiles and project types name it (ER, NR).</summary>
    public string Game { get; }

    /// <summary>What an o entry's NameID adds to its n entry's: 2,000,000,000 in ER, 2^31 in NR.</summary>
    public uint MirrorOffset { get; }

    /// <summary>
    /// Whether a NameID names the exact map it was given for, _DD included. ER's do, so an open world _10 tile whose
    /// files carry the _00 tile's name can be told from its NameIDs alone. NR's don't: only the binder's file names can.
    /// </summary>
    public bool NameIdNamesTileVersion { get; }

    /// <summary>Whether the game has NVC files joining neighbouring maps.</summary>
    public bool HasNvc { get; }

    private readonly Func<int, int, int, int, bool, int, uint> _nameId;

    private NavGameProfile(string game, uint mirrorOffset, bool nameIdNamesTileVersion, bool hasNvc, Func<int, int, int, int, bool, int, uint> nameId)
    {
        Game = game;
        MirrorOffset = mirrorOffset;
        NameIdNamesTileVersion = nameIdNamesTileVersion;
        HasNvc = hasNvc;
        _nameId = nameId;
    }

    /// <summary>
    /// Elden Ring: decimal NameIDs, prefix * 1,000,000,000 + map digits * 1,000 + position. The prefix is 0 in legacy
    /// maps, with the digits AABBCC, and 1 in the open world, with BBCCDD; the game tests the odd prefix (0x140BDA2A0).
    /// </summary>
    public static readonly NavGameProfile ER = new("ER", 2_000_000_000u, true, true,
        (aa, bb, cc, dd, openWorld, position) => (openWorld ? 1_000_000_000u : 0u) + (uint)(openWorld ? bb * 10000 + cc * 100 + dd : aa * 10000 + bb * 100 + cc) * 1_000u + (uint)position);

    /// <summary>
    /// Nightreign: NameIDs are bit fields (NavSurvey nameids, all 193 maps). Bit 31 marks the o set and bit 30 the open
    /// world; the low bits are the position in the entry's half:
    /// - open world: 1 &lt;&lt; 30 | (BB * 100 + CC) &lt;&lt; 8 | position, the same for every _DD version of a tile;
    /// - m10 to m19: AABBCC &lt;&lt; 8 | position;
    /// - other legacy maps: 1 &lt;&lt; 29 | (AA * 100 + BB) &lt;&lt; 12 | position.
    /// </summary>
    public static readonly NavGameProfile NR = new("NR", 0x8000_0000u, false, true,
        (aa, bb, cc, dd, openWorld, position) => openWorld
            ? 1u << 30 | (uint)(bb * 100 + cc) << 8 | (uint)position
            : aa < 20
                ? (uint)(aa * 10000 + bb * 100 + cc) << 8 | (uint)position
                : 1u << 29 | (uint)(aa * 100 + bb) << 12 | (uint)position);

    /// <summary>The profile of a game by its short name; Elden Ring's for a game without one of its own.</summary>
    public static NavGameProfile For(string game) => game switch
    {
        "NR" => NR,
        _ => ER,
    };

    /// <summary>The profile of the game a Havok profile reads files for.</summary>
    public static NavGameProfile For(Havok.HavokProfile profile) => For(profile?.Game);

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
