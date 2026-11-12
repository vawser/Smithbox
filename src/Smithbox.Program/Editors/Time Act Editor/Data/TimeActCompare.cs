using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// How a time act, animation or event compares with the game's version of it
/// </summary>
public enum TimeActVanillaStatus
{
    /// <summary>
    /// Not known yet, as the game's version is still being read, or couldn't be read
    /// </summary>
    Unknown,

    /// <summary>
    /// The same as the game's version
    /// </summary>
    Same,

    /// <summary>
    /// The game has it, but it has been changed
    /// </summary>
    Modified,

    /// <summary>
    /// The game doesn't have it
    /// </summary>
    Added
}

/// <summary>
/// The events of an animation matched with the game's. Events have no ID, so they are lined up as a diff of two
/// lists is: the longest run of identical events in order anchors the match, and between anchors the remaining events
/// pair up in order with game events of the same type. These are the modified ones. Events left over are added,
/// and game events left over were removed.
/// </summary>
public class TimeActEventMatch
{
    /// <summary>
    /// Each matched event, with the game's version of it
    /// </summary>
    public Dictionary<TAE.Event, TAE.Event> Matches = new();

    /// <summary>
    /// The matched events that are the same as the game's
    /// </summary>
    public HashSet<TAE.Event> Same = new();

    public List<TAE.Event> Added = new();

    /// <summary>
    /// The game's events that have no match
    /// </summary>
    public List<TAE.Event> Removed = new();

    public int ModifiedCount => Matches.Count - Same.Count;

    public TimeActVanillaStatus GetStatus(TAE.Event ev)
    {
        if (!Matches.ContainsKey(ev))
            return TimeActVanillaStatus.Added;

        return Same.Contains(ev) ? TimeActVanillaStatus.Same : TimeActVanillaStatus.Modified;
    }
}

/// <summary>
/// Compares time act data with the game's. Only depends on SoulsFormats, so the TaeTemplateSurvey tool builds this
/// same file to check it against the game's files.
/// </summary>
public static class TimeActCompare
{
    public static bool HeadersEqual(TAE a, TAE b)
    {
        return a.ID == b.ID &&
            a.EventBank == b.EventBank &&
            (a.SkeletonName ?? "") == (b.SkeletonName ?? "") &&
            (a.SibName ?? "") == (b.SibName ?? "") &&
            (a.Flags ?? []).SequenceEqual(b.Flags ?? []);
    }

    public static bool AnimationsEqual(TAE.Animation a, TAE.Animation b, bool bigEndian)
    {
        return MiniHeadersEqual(a.MiniHeader, b.MiniHeader) &&
            (a.AnimFileName ?? "") == (b.AnimFileName ?? "") &&
            EventListsEqual(a, b, bigEndian) &&
            GroupsEqual(a, b);
    }

    public static bool MiniHeadersEqual(TAE.Animation.AnimMiniHeader a, TAE.Animation.AnimMiniHeader b)
    {
        return (a, b) switch
        {
            (null, null) => true,
            (TAE.Animation.AnimMiniHeader.Standard x, TAE.Animation.AnimMiniHeader.Standard y) =>
                x.IsLoopByDefault == y.IsLoopByDefault &&
                x.ImportsHKX == y.ImportsHKX &&
                x.AllowDelayLoad == y.AllowDelayLoad &&
                x.ImportHKXSourceAnimID == y.ImportHKXSourceAnimID,
            (TAE.Animation.AnimMiniHeader.ImportOtherAnim x, TAE.Animation.AnimMiniHeader.ImportOtherAnim y) =>
                x.ImportFromAnimID == y.ImportFromAnimID && x.Unknown == y.Unknown,
            _ => false
        };
    }

    public static bool EventListsEqual(TAE.Animation a, TAE.Animation b, bool bigEndian)
    {
        if (a.Events.Count != b.Events.Count)
            return false;

        for (int i = 0; i < a.Events.Count; i++)
        {
            if (!EventsEqual(a.Events[i], b.Events[i], bigEndian))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The groups match, and each event is in the group at the same position as its game counterpart
    /// </summary>
    public static bool GroupsEqual(TAE.Animation a, TAE.Animation b)
    {
        if (a.EventGroups.Count != b.EventGroups.Count || a.Events.Count != b.Events.Count)
            return false;

        for (int i = 0; i < a.EventGroups.Count; i++)
        {
            var x = a.EventGroups[i];
            var y = b.EventGroups[i];

            if (x.GroupType != y.GroupType || !x.GroupData.Equals(y.GroupData))
                return false;
        }

        var aIndex = IndexGroups(a);
        var bIndex = IndexGroups(b);

        for (int i = 0; i < a.Events.Count; i++)
        {
            if (GetGroupIndex(aIndex, a.Events[i]) != GetGroupIndex(bIndex, b.Events[i]))
                return false;
        }

        return true;
    }

    private static Dictionary<TAE.EventGroup, int> IndexGroups(TAE.Animation animation)
    {
        var index = new Dictionary<TAE.EventGroup, int>();

        for (int i = 0; i < animation.EventGroups.Count; i++)
        {
            index.TryAdd(animation.EventGroups[i], i);
        }

        return index;
    }

    private static int GetGroupIndex(Dictionary<TAE.EventGroup, int> index, TAE.Event ev)
    {
        return ev.Group != null && index.TryGetValue(ev.Group, out var i) ? i : -1;
    }

    /// <summary>
    /// Whether two events are the same, apart from their group. Groups are compared with the animation, so that
    /// reverting an event never leaves it marked as changed.
    /// </summary>
    public static bool EventsEqual(TAE.Event a, TAE.Event b, bool bigEndian)
    {
        return a.Type == b.Type &&
            a.StartTime == b.StartTime &&
            GetEnd(a) == GetEnd(b) &&
            a.Unk04 == b.Unk04 &&
            ParametersEqual(a.GetParameterBytes(bigEndian), b.GetParameterBytes(bigEndian));
    }

    /// <summary>
    /// Whether two events end at the same time, counting every end past the placeholder as the same
    /// </summary>
    public static bool EndsEqual(TAE.Event a, TAE.Event b) => GetEnd(a) == GetEnd(b);

    /// <summary>
    /// Any end at or past the placeholder means the event lasts to the end of the animation. The writer stores
    /// every such end as the placeholder (a Sekiro event ending at 133.33 is written as 100), so they compare as one.
    /// </summary>
    private static float GetEnd(TAE.Event ev)
    {
        return ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder
            ? TAE.Event.EldenRingInfiniteLengthEventPlaceholder
            : ev.EndTime;
    }

    /// <summary>
    /// The lengths can differ by unused padding, which is always zero, as when a template is checked against an event
    /// </summary>
    public static bool ParametersEqual(byte[] a, byte[] b)
    {
        for (int i = 0; i < Math.Max(a.Length, b.Length); i++)
        {
            var x = i < a.Length ? a[i] : 0;
            var y = i < b.Length ? b[i] : 0;

            if (x != y)
                return false;
        }

        return true;
    }

    public static TimeActEventMatch MatchEvents(List<TAE.Event> current, List<TAE.Event> vanilla, bool bigEndian)
    {
        var match = new TimeActEventMatch();

        var currentKeys = current.Select(e => GetEventKey(e, bigEndian)).ToArray();
        var vanillaKeys = vanilla.Select(e => GetEventKey(e, bigEndian)).ToArray();

        var n = current.Count;
        var m = vanilla.Count;

        // Longest common subsequence of identical events, filled from the end
        var lengths = new int[n + 1, m + 1];

        for (int i = n - 1; i >= 0; i--)
        {
            for (int j = m - 1; j >= 0; j--)
            {
                lengths[i, j] = currentKeys[i] == vanillaKeys[j]
                    ? lengths[i + 1, j + 1] + 1
                    : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        var anchors = new List<(int Current, int Vanilla)>();

        for (int i = 0, j = 0; i < n && j < m;)
        {
            if (currentKeys[i] == vanillaKeys[j])
            {
                anchors.Add((i, j));
                i++;
                j++;
            }
            else if (lengths[i + 1, j] >= lengths[i, j + 1])
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        // The ends of the lists close off the last gap
        anchors.Add((n, m));

        var usedVanilla = new bool[m];
        int previousCurrent = 0, previousVanilla = 0;

        foreach (var (anchorCurrent, anchorVanilla) in anchors)
        {
            PairGap(current, vanilla, previousCurrent, anchorCurrent, previousVanilla, anchorVanilla, bigEndian,
                match, usedVanilla);

            if (anchorCurrent < n)
            {
                match.Matches[current[anchorCurrent]] = vanilla[anchorVanilla];
                match.Same.Add(current[anchorCurrent]);
                usedVanilla[anchorVanilla] = true;
            }

            previousCurrent = anchorCurrent + 1;
            previousVanilla = anchorVanilla + 1;
        }

        for (int j = 0; j < m; j++)
        {
            if (!usedVanilla[j])
            {
                match.Removed.Add(vanilla[j]);
            }
        }

        return match;
    }

    /// <summary>
    /// The events between two anchors pair with game events of the same type between them, the most alike first:
    /// an edit usually changes one value, so the game event it came from shares the rest. Ties go to the closest
    /// position. Taking them in order instead pairs an edited event with a deleted neighbour of its type.
    /// </summary>
    private static void PairGap(List<TAE.Event> current, List<TAE.Event> vanilla, int currentStart, int currentEnd,
        int vanillaStart, int vanillaEnd, bool bigEndian, TimeActEventMatch match, bool[] usedVanilla)
    {
        var candidates = new List<(int Current, int Vanilla, double Score, int Distance)>();

        for (int i = currentStart; i < currentEnd; i++)
        {
            for (int j = vanillaStart; j < vanillaEnd; j++)
            {
                if (vanilla[j].Type != current[i].Type)
                    continue;

                var distance = Math.Abs((i - currentStart) - (j - vanillaStart));

                candidates.Add((i, j, GetSimilarity(current[i], vanilla[j], bigEndian), distance));
            }
        }

        var pairedCurrent = new HashSet<int>();

        foreach (var (i, j, _, _) in candidates.OrderByDescending(e => e.Score).ThenBy(e => e.Distance))
        {
            if (usedVanilla[j] || pairedCurrent.Contains(i))
                continue;

            match.Matches[current[i]] = vanilla[j];
            usedVanilla[j] = true;
            pairedCurrent.Add(i);
        }

        for (int i = currentStart; i < currentEnd; i++)
        {
            if (!pairedCurrent.Contains(i))
            {
                match.Added.Add(current[i]);
            }
        }
    }

    /// <summary>
    /// How alike two events of one type are: one point for each of start, end and Unk04 that match, two for the
    /// parameters as a whole, and up to one more for the share of parameter bytes that match
    /// </summary>
    private static double GetSimilarity(TAE.Event a, TAE.Event b, bool bigEndian)
    {
        var aBytes = a.GetParameterBytes(bigEndian);
        var bBytes = b.GetParameterBytes(bigEndian);

        var score = 0.0;

        if (a.StartTime == b.StartTime)
            score++;

        if (GetEnd(a) == GetEnd(b))
            score++;

        if (a.Unk04 == b.Unk04)
            score++;

        var length = Math.Max(aBytes.Length, bBytes.Length);

        if (length == 0)
            return score + 3;

        var same = 0;

        for (int i = 0; i < length; i++)
        {
            var x = i < aBytes.Length ? aBytes[i] : 0;
            var y = i < bBytes.Length ? bBytes[i] : 0;

            if (x == y)
                same++;
        }

        return score + (same == length ? 2 : 0) + (double)same / length;
    }

    private static string GetEventKey(TAE.Event ev, bool bigEndian)
    {
        var bytes = ev.GetParameterBytes(bigEndian);

        // Trailing zeros are padding, so lengths that differ only by it give the same key
        var length = bytes.Length;

        while (length > 0 && bytes[length - 1] == 0)
        {
            length--;
        }

        return $"{ev.Type}|{BitConverter.SingleToInt32Bits(ev.StartTime)}|{BitConverter.SingleToInt32Bits(GetEnd(ev))}" +
            $"|{ev.Unk04}|{Convert.ToHexString(bytes, 0, length)}";
    }
}
