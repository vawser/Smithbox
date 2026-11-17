using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.Common;

// Known credits for original research used for character reloading:
// - horkrux
// - Meowmaritus

/// <summary>
/// The memory offsets used by the Character Reloader, one group per game executable version.
/// Read from Assets/HAVOK/Character Reloader/{game}/*.json.
/// </summary>
public class CharacterReloaderOffsets
{
    public List<CharacterReloaderOffsetEntry> Groups { get; set; } = new();

    /// <summary>
    /// The group for this executable version, or else the newest group (null if there are none).
    /// </summary>
    public CharacterReloaderOffsetEntry GetGroup(string exeVersion, out bool exactMatch)
    {
        var match = Groups.FirstOrDefault(e => e.exeVersion == exeVersion);
        exactMatch = match != null;

        if (match != null)
            return match;

        return Groups
            .OrderBy(e => Version.TryParse(e.exeVersion, out var v) ? v : new Version(0, 0))
            .LastOrDefault();
    }

    /// <summary>
    /// Parse a hex value, with or without a 0x prefix. Returns null if it can't be parsed.
    /// </summary>
    public static int? ParseHex(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        var hex = str.Trim();

        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex[2..];
        }

        if (int.TryParse(hex, NumberStyles.HexNumber, null, out int asInt))
        {
            return asInt;
        }

        return null;
    }

    /// <summary>
    /// Parse space-separated hex bytes ("48 31 D2"). Returns null if they can't be parsed.
    /// </summary>
    public static byte[] ParseBytes(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        try
        {
            return str.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => byte.Parse(x, NumberStyles.HexNumber))
                .ToArray();
        }
        catch (FormatException)
        {
            return null;
        }
    }
}

public class CharacterReloaderOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    // How the reload is requested:
    // "Call"  - call the game's own RequestReloadChr(WorldChrMan, name), which queues the request itself
    // "Queue" - append a request node to WorldChrMan's reload list (for games where that function was stripped)
    public string method { get; set; }

    // Instruction(s) that load the WorldChrMan global (space-separated bytes, ?? = any byte)
    public string worldChrManAob { get; set; }

    // Start and end of the rel32 in the AOB ("0x3/0x7"): the global is at end + rel32
    public string worldChrManAobRelativeOffset { get; set; }

    // Call: the game's RequestReloadChr, found through a call to it. The AOB defaults to worldChrManAob
    // (a call site that also loads WorldChrMan), with the call's rel32 at requestFunctionAobRelativeOffset.
    public string requestFunctionAob { get; set; }
    public string requestFunctionAobRelativeOffset { get; set; }

    // Call, optional: byte flag the game requires for player characters (c0000) to be reloaded.
    // Set around the call, then restored.
    public string playerReloadFlagAob { get; set; }
    public string playerReloadFlagAobRelativeOffset { get; set; }

    // Queue: WorldChrMan fields of the reload request list
    public string reloadListOffset { get; set; }
    public string reloadCountOffset { get; set; }
    public string reloadTimerOffset { get; set; }

    // Queue: instruction that hands the processed request node to the game's allocator to be freed;
    // patched so the game doesn't free the node, which Smithbox allocated
    public string crashPatchAob { get; set; }

    // Distance from the end of the AOB back to the patched instruction
    public string crashPatchDistFromEndOfAob { get; set; }
    public string crashPatchBytes { get; set; }
}
