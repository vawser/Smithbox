using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Making and removing EDGE entries (EDGE Editor M4). Every operation returns a changed copy of the file, or null when
/// it can't be done, and leaves the file it is given alone. Uses only SoulsFormats, so EdgeSurvey's check compiles this
/// same file.
///
/// What the game's map files do (EdgeSurvey authoring, 689 files), which new entries follow:
/// - The file's ID header is the collision's number (689/689: h000500 → 500).
/// - A chain's segments follow each other in the file, head to tail (3076 of 3193 chains), so new segments go next to
///   their neighbours. Hang and Hug chains can share a chain ID (151 files), and grapples share them too; a new chain
///   or grapple takes one higher than any in the file, which is never ambiguous.
/// - Grapples come before the segments (3/3 files with both).
/// - A grapple's direction point is ~4 cm straight up from it (direction Y = 1, median 0.0435 m).
/// - Map grapples have no pull point (12/12, Unk2C 0); object files use one (188 of 267 with Unk2C 1), offered separately.
/// - Variation: Hang 0 (3421 of 4283), Hug 1 (2237 of 4871), Grapple 0 (11 of 12); Unk36 0.
/// </summary>
public static class EdgeAuthoring
{
    public const float GrappleDirectionLength = 0.04f;
    public const float PullPointHeight = 0.5f;

    public static byte DefaultVariation(EDGE.EdgeType type) => type switch
    {
        EDGE.EdgeType.Hug => 1,
        _ => 0
    };

    /// <summary>
    /// What the game's object EDGE files do (EdgeSurvey objects, 182 objbnds), which the Model Editor's new entries
    /// follow: the file is e{number}.edge next to the FLVER, binder ID 1300, Flag1, Zlib; its ID header is the object
    /// number (151/182); grapples have a pull point (188/255) and Variation 1 (171/255); their direction point is
    /// horizontal (202/255 with Y 0), not up as in maps; ledges and walls use Variation 0 (71/71, 146/273).
    /// </summary>
    public static class ObjectDefaults
    {
        public const int BinderFileId = 1300;
        public const byte GrappleVariation = 1;
        public const byte HangVariation = 0;
        public const byte HugVariation = 0;

        public static byte Variation(EDGE.EdgeType type) => type switch
        {
            EDGE.EdgeType.Grapple => GrappleVariation,
            EDGE.EdgeType.Hang => HangVariation,
            _ => HugVariation
        };

        /// <summary>The binder name of an object's EDGE file, e.g. e001000.edge for o001000. Null without a number.</summary>
        public static string FileName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || objectName.Length < 2 || !int.TryParse(objectName.Substring(1), out var number))
                return null;

            return $"e{number:D6}.edge";
        }

        /// <summary>
        /// Puts an object's EDGE file into its objbnd as the game's are: beside the FLVER named after the object (or the
        /// first FLVER), binder ID 1300 unless taken (then the next free one), Flag1. An existing file of that name gets
        /// the new bytes. Returns the binder file, or null if the object name has no number.
        /// </summary>
        public static BinderFile AddToBinder(IBinder binder, string objectName, byte[] data)
        {
            var fileName = FileName(objectName);

            if (fileName == null)
                return null;

            static string Stem(string name)
            {
                var file = name.Substring(name.LastIndexOf('\\') + 1);
                var dot = file.IndexOf('.');
                return dot < 0 ? file : file.Substring(0, dot);
            }

            var flver = binder.Files.FirstOrDefault(f => f.Name != null && f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(Stem(f.Name), objectName, StringComparison.OrdinalIgnoreCase))
                ?? binder.Files.FirstOrDefault(f => f.Name != null && f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase));

            var directory = flver != null ? flver.Name.Substring(0, flver.Name.LastIndexOf('\\') + 1) : "";
            var name = directory + fileName;

            var existing = binder.Files.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Bytes = data;
                return existing;
            }

            var id = BinderFileId;
            while (binder.Files.Any(f => f.ID == id))
                id++;

            var file = new BinderFile(Binder.FileFlags.Flag1, id, name, data) { CompressionType = DCX.Type.Zlib };

            binder.Files.Add(file);
            binder.Files.Sort((a, b) => a.ID.CompareTo(b.ID));

            return file;
        }
    }

    /// <summary>An empty file for a collision, e.g. 500 for h000500. Null if the name has no number.</summary>
    public static EDGE CreateFile(string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2 || !int.TryParse(modelName.Substring(1), out var number))
            return null;

        return new EDGE { ID = number };
    }

    public static EDGE Copy(EDGE edge) => new() { ID = edge.ID, Edges = edge.Edges.Select(e => e.Clone()).ToList() };

    /// <summary>One higher than any chain or group ID of the file, from 1.</summary>
    public static int NextChainId(EDGE edge) => edge.Edges.Count == 0 ? 1 : Math.Max(1, edge.Edges.Max(e => e.Unk30) + 1);

    #region Grapples

    /// <summary>
    /// A new grapple point, after the file's other grapples. <paramref name="direction"/> is the way its direction
    /// point lies from it in the part's space: up for map grapples; object grapples point sideways (see
    /// <see cref="ObjectDefaults"/>).
    /// </summary>
    public static EDGE AddGrapple(EDGE edge, Vector3 position, Vector3 direction, out int index, byte? variation = null)
    {
        var after = Copy(edge);

        var grapple = new EDGE.Edge
        {
            Type = EDGE.EdgeType.Grapple,
            V1 = position,
            V2 = position + Vector3.Normalize(direction) * GrappleDirectionLength,
            V3 = Vector3.Zero,
            Unk2C = 0,
            Unk30 = NextChainId(edge),
            VariationID = variation ?? DefaultVariation(EDGE.EdgeType.Grapple),
            Unk36 = 0
        };

        // After the last grapple; at the start when there are none, as the game's files keep grapples first
        var last = after.Edges.FindLastIndex(e => e.Type == EDGE.EdgeType.Grapple);
        index = last + 1;
        after.Edges.Insert(index, grapple);

        return after;
    }

    /// <summary>Gives a grapple a pull point above it (and sets Has Pull Point), or takes it away.</summary>
    public static EDGE SetPullPoint(EDGE edge, int index, bool has, Vector3 up)
    {
        if (index < 0 || index >= edge.Edges.Count || edge.Edges[index].Type != EDGE.EdgeType.Grapple)
            return null;

        var after = Copy(edge);
        var entry = after.Edges[index];

        if (has)
        {
            entry.V3 = entry.V1 + Vector3.Normalize(up) * PullPointHeight;
            entry.Unk2C = 1;
        }
        else
        {
            entry.V3 = Vector3.Zero;
            entry.Unk2C = 0;
        }

        return after;
    }

    #endregion

    #region Chains

    /// <summary>Whether an entry is a ledge or wall segment.</summary>
    public static bool IsSegment(EDGE.Edge entry) => entry.Type is EDGE.EdgeType.Hang or EDGE.EdgeType.Hug;

    /// <summary>The indices of a segment's chain (same type and chain ID), in file order.</summary>
    public static List<int> ChainOf(EDGE edge, int index)
    {
        if (index < 0 || index >= edge.Edges.Count || !IsSegment(edge.Edges[index]))
            return new List<int>();

        var entry = edge.Edges[index];

        return Enumerable.Range(0, edge.Edges.Count)
            .Where(i => IsSegment(edge.Edges[i]) && edge.Edges[i].Type == entry.Type && edge.Edges[i].Unk30 == entry.Unk30)
            .ToList();
    }

    /// <summary>A new chain of one segment, added at the end of the file. Index is the new segment's.</summary>
    public static EDGE AddChain(EDGE edge, EDGE.EdgeType type, Vector3 start, Vector3 end, out int index, byte? variation = null)
    {
        index = -1;

        if (type == EDGE.EdgeType.Grapple || start == end)
            return null;

        var after = Copy(edge);

        after.Edges.Add(new EDGE.Edge
        {
            Type = type,
            V1 = start,
            V2 = end,
            Unk30 = NextChainId(edge),
            VariationID = variation ?? DefaultVariation(type),
            Unk36 = 0
        });

        index = after.Edges.Count - 1;
        return after;
    }

    /// <summary>
    /// Which end of its chain a chain point is: its only segment end is a V2 (the tail, drawn on from), a V1 (the
    /// head, drawn on backwards), or neither (inside the chain, or where it branches).
    /// </summary>
    public static ChainEnd GetChainEnd(EdgeHandle handle)
    {
        if (handle.Kind != EdgeHandleKind.Vertex || handle.Points.Count != 1)
            return ChainEnd.None;

        return handle.Points[0].Point == 2 ? ChainEnd.Tail : ChainEnd.Head;
    }

    /// <summary>
    /// A new segment from a chain's end to a point, with the fields of the segment it joins: after it from the tail,
    /// before it from the head, so the chain stays in head-to-tail order. Index is the new segment's.
    /// </summary>
    public static EDGE Extend(EDGE edge, EdgeHandle end, Vector3 position, out int index)
    {
        index = -1;

        var which = GetChainEnd(end);

        if (which == ChainEnd.None || end.Position == position)
            return null;

        var at = end.Points[0].Entry;

        if (at < 0 || at >= edge.Edges.Count)
            return null;

        var after = Copy(edge);
        var segment = after.Edges[at].Clone();

        if (which == ChainEnd.Tail)
        {
            segment.V1 = end.Position;
            segment.V2 = position;
            index = at + 1;
        }
        else
        {
            segment.V1 = position;
            segment.V2 = end.Position;
            index = at;
        }

        segment.V3 = Vector3.Zero;
        after.Edges.Insert(index, segment);

        return after;
    }

    /// <summary>
    /// Splits a segment in two at a point along it (0-1, clamped away from its ends). The second half follows the first.
    /// </summary>
    public static EDGE Split(EDGE edge, int index, float at = 0.5f)
    {
        if (index < 0 || index >= edge.Edges.Count || !IsSegment(edge.Edges[index]) || edge.Edges[index].V1 == edge.Edges[index].V2)
            return null;

        var after = Copy(edge);
        var first = after.Edges[index];
        var second = first.Clone();
        var middle = Vector3.Lerp(first.V1, first.V2, Math.Clamp(at, 0.01f, 0.99f));

        first.V2 = middle;
        second.V1 = middle;
        after.Edges.Insert(index + 1, second);

        return after;
    }

    /// <summary>
    /// Joins a chain's tail to another chain's head of the same type: a segment between them (unless they meet already)
    /// and the second chain renumbered into the first, its segments moved to follow. The tail's own chain's head closes
    /// it into a loop instead. Null if the ends aren't a tail and a head of the same type.
    /// </summary>
    public static EDGE Join(EDGE edge, EdgeHandle tail, EdgeHandle head, out int index)
    {
        index = -1;

        if (GetChainEnd(tail) != ChainEnd.Tail || GetChainEnd(head) != ChainEnd.Head)
            return null;

        var tailEntry = tail.Points[0].Entry;
        var headEntry = head.Points[0].Entry;

        if (tailEntry < 0 || tailEntry >= edge.Edges.Count || headEntry < 0 || headEntry >= edge.Edges.Count)
            return null;

        var a = edge.Edges[tailEntry];
        var b = edge.Edges[headEntry];

        if (a.Type != b.Type)
            return null;

        var sameChain = a.Unk30 == b.Unk30;

        // The tail's chain, then the head's chain, in file order; everything else stays where it is
        var tailChain = ChainOf(edge, tailEntry);
        var headChain = sameChain ? new List<int>() : ChainOf(edge, headEntry);

        var moved = new HashSet<int>(headChain);
        var result = new EDGE { ID = edge.ID };

        for (var i = 0; i < edge.Edges.Count; i++)
        {
            if (moved.Contains(i))
                continue;

            result.Edges.Add(edge.Edges[i].Clone());

            if (i != tailEntry)
                continue;

            if (tail.Position != head.Position)
            {
                var bridge = a.Clone();
                bridge.V1 = tail.Position;
                bridge.V2 = head.Position;
                bridge.V3 = Vector3.Zero;
                result.Edges.Add(bridge);
            }

            index = result.Edges.Count - 1;

            foreach (var j in headChain)
            {
                var segment = edge.Edges[j].Clone();
                segment.Unk30 = a.Unk30;
                result.Edges.Add(segment);
            }
        }

        // Nothing to do: the ends already meet in one chain
        if (sameChain && tail.Position == head.Position)
            return null;

        return result;
    }

    #endregion

    #region Removing

    /// <summary>
    /// Takes a point away. A grapple goes; a pull point is removed from its grapple; a chain end shortens its chain by
    /// its segment; a point inside a chain, where one segment ends and the next starts, joins the two into one.
    /// Null for a point where a chain branches.
    /// </summary>
    public static EDGE DeletePoint(EDGE edge, EdgeHandle handle)
    {
        switch (handle.Kind)
        {
            case EdgeHandleKind.Grapple:
                return DeleteEntries(edge, new[] { handle.Entry });

            case EdgeHandleKind.Pull:
                return SetPullPoint(edge, handle.Entry, false, Vector3.UnitY);
        }

        if (handle.Points.Count == 1)
            return DeleteEntries(edge, new[] { handle.Entry });

        if (handle.Points.Count != 2)
            return null;

        var ending = handle.Points.FirstOrDefault(p => p.Point == 2);
        var starting = handle.Points.FirstOrDefault(p => p.Point == 1);

        if (!handle.Points.Contains(ending) || !handle.Points.Contains(starting) || ending.Entry == starting.Entry)
            return null;

        var after = Copy(edge);
        after.Edges[ending.Entry].V2 = after.Edges[starting.Entry].V2;
        after.Edges.RemoveAt(starting.Entry);

        return after;
    }

    /// <summary>Removes a segment's whole chain.</summary>
    public static EDGE DeleteChain(EDGE edge, int index)
    {
        var chain = ChainOf(edge, index);

        return chain.Count == 0 ? null : DeleteEntries(edge, chain);
    }

    public static EDGE DeleteEntries(EDGE edge, IEnumerable<int> indices)
    {
        var remove = new HashSet<int>(indices.Where(i => i >= 0 && i < edge.Edges.Count));

        if (remove.Count == 0)
            return null;

        return new EDGE
        {
            ID = edge.ID,
            Edges = edge.Edges.Where((_, i) => !remove.Contains(i)).Select(e => e.Clone()).ToList()
        };
    }

    #endregion
}

public enum ChainEnd
{
    None,
    Head,
    Tail
}
