using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

public enum BankAliasKind
{
    None,
    Character,
    Asset,
    Map,
}

/// <summary>
/// The naming rules behind the Sound Editor's aliases, without Smithbox dependencies (Claude/Tools/SoundSurvey links
/// this file to measure them on every game).
/// </summary>
public static class SoundNameRules
{
    // A sound ID inside a name: type letter + 6-10 digits, then optionally a suffix of letters (b, c, dummy) or anything
    // after an underscore (_01, _QU_v2, _stereo), and optionally a Play_/Stop_ prefix. Shorter numbers are character IDs
    // (c3471_attack_swing, c1010_v_attack1), not sound IDs.
    private static readonly Regex SoundIdPattern = new(@"^(?:(?:Play|Stop)_)?([a-z])(\d{6,10})(?:[a-z]+|_[A-Za-z0-9_]*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] BankPrefixes = { "cs_", "fdp_", "frpg2_", "frpg_", "sprj_", "fdlc_" };

    /// <summary>
    /// The sound ID a sample / media / event name carries: ("c", "107001001") for c107001001b or Play_c107001001.
    /// The number is as params and TAE events store it (no leading zeros): Play_s000806100 → ("s", "806100").
    /// </summary>
    public static (string Type, string Id)? ParseSoundId(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        var m = SoundIdPattern.Match(name);
        if (!m.Success || !long.TryParse(m.Groups[2].Value, out var number))
            return null;

        return (m.Groups[1].Value.ToLowerInvariant(), number.ToString());
    }

    /// <summary>
    /// Which alias a bank name points at: a character (cs_c2010, fdp_c1070 → "c2010"), an asset (aeg027_002), or a map
    /// (cs_m10, fdp_m30, s10_00_0030). For maps, <paramref name="exactId"/> is the full mAA_BB_CC_DD (missing parts 00)
    /// and <paramref name="prefix"/> what any matching map ID starts with.
    /// </summary>
    public static BankAliasKind BankAliasKey(string bankName, out string exactId, out string prefix)
    {
        exactId = prefix = null;
        var stem = bankName ?? "";
        foreach (var p in BankPrefixes)
            if (stem.StartsWith(p, StringComparison.OrdinalIgnoreCase)) { stem = stem[p.Length..]; break; }
        stem = stem.ToLowerInvariant();

        var chr = Regex.Match(stem, @"^c(\d{4})$");
        if (chr.Success)
        {
            exactId = $"c{chr.Groups[1].Value}";
            return BankAliasKind.Character;
        }

        if (Regex.IsMatch(stem, @"^aeg\d{3}_\d{3}$"))
        {
            exactId = stem;
            return BankAliasKind.Asset;
        }

        var map = Regex.Match(stem, @"^m(\d{2})(?:_(\d{2}))?(?:_(\d{2}))?(?:_(\d{2}))?$");
        var sMap = Regex.Match(stem, @"^s(\d{2})_(\d{2})_\d{4}$");
        if (map.Success || sMap.Success)
        {
            string aa = map.Success ? map.Groups[1].Value : sMap.Groups[1].Value;
            string bb = map.Success ? map.Groups[2].Value : sMap.Groups[2].Value;
            string cc = map.Success ? map.Groups[3].Value : "";
            string dd = map.Success ? map.Groups[4].Value : "";
            exactId = $"m{aa}_{Or00(bb)}_{Or00(cc)}_{Or00(dd)}";
            prefix = $"m{aa}_" + (bb.Length > 0 ? $"{bb}_" : "") + (cc.Length > 0 ? $"{cc}_" : "") + dd;
            return BankAliasKind.Map;
        }

        return BankAliasKind.None;
    }

    private static string Or00(string s) => string.IsNullOrEmpty(s) ? "00" : s;

    /// <summary>The Wwise event name of a sound ID: Play_ / Stop_ + type + the number padded to 9 digits (Play_s000806100).</summary>
    public static string EventName(string prefix, string type, string id)
        => long.TryParse(id, out var n) ? $"{prefix}_{type}{n:D9}" : null;

    /// <summary>
    /// Names a bank's unnamed events by the Play_/Stop_ + type + 9-digit rule (FNV-1 hash must equal the event ID):
    /// <list type="bullet">
    /// <item>from <paramref name="soundIds"/> (IDs found in the bank's own media names): confirmed names</item>
    /// <item>by trying the bank's ID range: cs_cXXXX / cXXXX → c XXXX ##### (10⁵), vcXXX → v XXX ###### (10⁶).
    /// These go to <paramref name="guessed"/>: SoundSurvey found ~9 false hits among 14k in ER's voice ranges.</item>
    /// </list>
    /// </summary>
    public static Dictionary<uint, string> MatchEventNames(HashSet<uint> unnamed, IEnumerable<(string Type, string Id)> soundIds,
        string bankName, out HashSet<uint> guessed)
    {
        var found = new Dictionary<uint, string>();
        guessed = new HashSet<uint>();
        if (unnamed.Count == 0)
            return found;

        foreach (var (type, id) in soundIds)
            foreach (var prefix in new[] { "Play", "Stop" })
            {
                var name = EventName(prefix, type, id);
                if (name == null) continue;
                var h = WwiseBank.Hash(name);
                if (unnamed.Contains(h)) found.TryAdd(h, name);
            }

        // the bank's ID range
        string letter = null, lead = null; int rest = 0;
        var stem = (bankName ?? "").ToLowerInvariant();
        var chr = Regex.Match(stem, @"^(?:cs_)?c(\d{4})$");
        var vc = Regex.Match(stem, @"^vc(\d{3})$");
        if (chr.Success) { letter = "c"; lead = chr.Groups[1].Value; rest = 5; }
        else if (vc.Success) { letter = "v"; lead = vc.Groups[1].Value; rest = 6; }

        if (letter != null && found.Count < unnamed.Count)
        {
            int max = rest == 5 ? 100000 : 1000000;
            var fmt = "D" + rest;
            Span<byte> digits = stackalloc byte[rest];
            foreach (var prefix in new[] { "Play_", "Stop_" })
            {
                // FNV-1 is incremental: hash the fixed head once, then only the digits per candidate
                var head = prefix + letter + lead;
                uint headHash = WwiseBank.Hash(head);
                for (int n = 0; n < max; n++)
                {
                    int v = n;
                    for (int k = rest - 1; k >= 0; k--) { digits[k] = (byte)('0' + v % 10); v /= 10; }
                    uint h = headHash;
                    for (int k = 0; k < rest; k++) { h *= 16777619; h ^= digits[k]; }
                    if (unnamed.Contains(h) && found.TryAdd(h, head + n.ToString(fmt)))
                        guessed.Add(h);
                }
            }
        }

        return found;
    }
}
