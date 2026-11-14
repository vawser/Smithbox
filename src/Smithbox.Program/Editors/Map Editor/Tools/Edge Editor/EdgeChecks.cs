using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>What is wrong with an EDGE entry or chain.</summary>
public enum EdgeIssueKind
{
    /// <summary>A ledge or wall segment whose ends are the same point.</summary>
    DegenerateSegment,

    /// <summary>An entry exactly the same as an earlier one.</summary>
    Duplicate,

    /// <summary>A ledge or wall with a pull point or Has Pull Point set, which only grapples use.</summary>
    PullOnSegment,

    /// <summary>A grapple whose Has Pull Point doesn't say whether it has a pull point.</summary>
    PullFlag,

    /// <summary>A grapple whose direction point is the grapple point itself.</summary>
    GrappleNoDirection,

    /// <summary>A chain end within a few centimetres of another start of its chain that it doesn't meet.</summary>
    ChainGap
}

/// <summary>An issue, at the entry it is shown and selected at.</summary>
public sealed record EdgeIssue(EdgeIssueKind Kind, int Entry, int Detail = 0);

/// <summary>
/// The checks of an EDGE file (EDGE Editor M5), and their fixes. Uses only SoulsFormats, so EdgeSurvey's check compiles
/// this same file. The game's own files have a few of these (EdgeSurvey checks), so they are warnings, not errors.
/// </summary>
public static class EdgeChecks
{
    public static List<EdgeIssue> Check(EDGE edge)
    {
        List<EdgeIssue> issues = new();

        for (var i = 0; i < edge.Edges.Count; i++)
        {
            var e = edge.Edges[i];

            if (EdgeAuthoring.IsSegment(e))
            {
                if (e.V1 == e.V2)
                    issues.Add(new EdgeIssue(EdgeIssueKind.DegenerateSegment, i));

                if (e.V3 != Vector3.Zero || e.Unk2C != 0)
                    issues.Add(new EdgeIssue(EdgeIssueKind.PullOnSegment, i));
            }
            else if (e.Type == EDGE.EdgeType.Grapple)
            {
                var hasPull = e.V3 != Vector3.Zero;

                if (e.Unk2C != (hasPull ? 1 : 0))
                    issues.Add(new EdgeIssue(EdgeIssueKind.PullFlag, i));

                if (e.V1 == e.V2)
                    issues.Add(new EdgeIssue(EdgeIssueKind.GrappleNoDirection, i));
            }

            for (var j = 0; j < i; j++)
            {
                if (Same(edge.Edges[j], e))
                {
                    issues.Add(new EdgeIssue(EdgeIssueKind.Duplicate, i, j));
                    break;
                }
            }
        }

        foreach (var (entry, distance) in ChainGaps(edge))
        {
            if (distance <= GapTolerance)
                issues.Add(new EdgeIssue(EdgeIssueKind.ChainGap, entry, (int)MathF.Round(distance * 1000)));
        }

        return issues.OrderBy(x => x.Entry).ThenBy(x => x.Kind).ToList();
    }

    /// <summary>
    /// How near a chain's loose ends come before they count as a gap, in metres. The game reuses a chain ID for
    /// separate pieces (165 chains in pieces, EdgeSurvey checks), but none of its loose ends is within 5 cm of
    /// another start (the nearest are 5-10 cm), so a nearer one is an end that was meant to meet and missed.
    /// </summary>
    public const float GapTolerance = 0.05f;

    /// <summary>
    /// For each chain with loose ends, the nearest any end comes to a start it doesn't meet (another piece's, or its
    /// own piece's, as a loop left open), at the entry of that end.
    /// </summary>
    public static IEnumerable<(int Entry, float Distance)> ChainGaps(EDGE edge)
    {
        var handles = EdgeHandles.Build(edge);

        foreach (var chain in handles
            .Where(h => EdgeAuthoring.GetChainEnd(h) != ChainEnd.None)
            .GroupBy(h => (edge.Edges[h.Entry].Type, edge.Edges[h.Entry].Unk30)))
        {
            var heads = chain.Where(h => EdgeAuthoring.GetChainEnd(h) == ChainEnd.Head).ToList();
            var tails = chain.Where(h => EdgeAuthoring.GetChainEnd(h) == ChainEnd.Tail).ToList();

            // A single piece counts too: its ends that nearly meet are a loop left open
            if (heads.Count == 0 || tails.Count == 0)
                continue;

            var best = float.MaxValue;
            var at = -1;

            foreach (var t in tails)
            {
                foreach (var h in heads)
                {
                    var d = Vector3.Distance(t.Position, h.Position);

                    if (d < best)
                    {
                        best = d;
                        at = t.Entry;
                    }
                }
            }

            if (at >= 0)
                yield return (at, best);
        }
    }

    private static bool Same(EDGE.Edge a, EDGE.Edge b)
    {
        return a.V1 == b.V1 && a.V2 == b.V2 && a.V3 == b.V3 && a.Unk2C == b.Unk2C && a.Unk30 == b.Unk30
            && a.Type == b.Type && a.VariationID == b.VariationID && a.Unk36 == b.Unk36;
    }

    /// <summary>
    /// A copy of the file with an issue fixed: degenerate and duplicate entries removed, a segment's pull point cleared,
    /// Has Pull Point set from the pull point, a direction point put back above a grapple, a gap closed by moving the
    /// loose end onto the start. Null if it can't be fixed.
    /// <paramref name="up"/> is up in the part's space.
    /// </summary>
    public static EDGE Fix(EDGE edge, EdgeIssue issue, Vector3 up)
    {
        if (issue.Entry < 0 || issue.Entry >= edge.Edges.Count)
            return null;

        switch (issue.Kind)
        {
            case EdgeIssueKind.DegenerateSegment:
            case EdgeIssueKind.Duplicate:
                return EdgeAuthoring.DeleteEntries(edge, new[] { issue.Entry });
        }

        var after = EdgeAuthoring.Copy(edge);
        var e = after.Edges[issue.Entry];

        switch (issue.Kind)
        {
            case EdgeIssueKind.PullOnSegment:
                e.V3 = Vector3.Zero;
                e.Unk2C = 0;
                return after;

            case EdgeIssueKind.PullFlag:
                e.Unk2C = e.V3 != Vector3.Zero ? 1 : 0;
                return after;

            case EdgeIssueKind.GrappleNoDirection:
                e.V2 = e.V1 + Vector3.Normalize(up) * EdgeAuthoring.GrappleDirectionLength;
                return after;

            case EdgeIssueKind.ChainGap:
                // The loose end goes onto the start it missed
                var handles = EdgeHandles.Build(edge);
                var tail = handles.FirstOrDefault(h => h.Entry == issue.Entry && EdgeAuthoring.GetChainEnd(h) == ChainEnd.Tail);

                if (tail == null)
                    return null;

                var head = handles
                    .Where(h => EdgeAuthoring.GetChainEnd(h) == ChainEnd.Head
                        && edge.Edges[h.Entry].Type == e.Type && edge.Edges[h.Entry].Unk30 == e.Unk30)
                    .OrderBy(h => Vector3.Distance(h.Position, tail.Position))
                    .FirstOrDefault();

                if (head == null || Vector3.Distance(head.Position, tail.Position) > GapTolerance)
                    return null;

                e.V2 = head.Position;
                return after;
        }

        return null;
    }

    /// <summary>Every fixable issue fixed, one at a time as the indices move.</summary>
    public static EDGE FixAll(EDGE edge, Vector3 up, out int fixedCount)
    {
        fixedCount = 0;
        var current = edge;

        // Each fix removes its issue, so this ends; the bound only guards against a fix that wouldn't
        for (var guard = 0; guard < edge.Edges.Count * 4 + 8; guard++)
        {
            var issue = Check(current).FirstOrDefault();

            if (issue == null)
                break;

            var next = Fix(current, issue, up);

            if (next == null)
                break;

            current = next;
            fixedCount++;
        }

        return current;
    }
}
