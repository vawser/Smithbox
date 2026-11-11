using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Suggests groups for a map object from the map objects around it: a weighted vote of its nearest neighbours.
/// </summary>
public static class DisplayGroupNeighbours
{
    public class Candidate
    {
        public Entity Entity;
        public BoundingBox Bounds;
        public uint[] Groups;
    }

    public class Suggestion
    {
        public uint[] Groups;
        public int NeighbourCount;
        public float NearestDistance;
        public float FarthestDistance;
    }

    /// <summary>
    /// World-space bounds of a map object's mesh, or its position when no mesh is loaded (map pieces and
    /// collisions often sit at the map origin with their geometry baked in, so the position alone misleads).
    /// </summary>
    public static BoundingBox BoundsOf(Entity ent)
    {
        var box = ent.GetBounds();
        if (box.Min == box.Max)
        {
            var p = ent.GetWorldMatrix().Translation;
            box = new BoundingBox(p, p);
        }

        return box;
    }

    /// <summary>
    /// The gap between two boxes: 0 when they touch or overlap.
    /// </summary>
    public static float BoxDistance(BoundingBox a, BoundingBox b)
    {
        var gap = Vector3.Max(Vector3.Zero, Vector3.Max(a.Min - b.Max, b.Min - a.Max));
        return gap.Length();
    }

    private static Vector3 Center(BoundingBox box) => (box.Min + box.Max) * 0.5f;

    /// <summary>
    /// Votes over the nearest candidates. Each neighbour weighs 1 / (1 + gap in metres); a group is kept when the
    /// neighbours holding it carry at least <paramref name="agreement"/> of the total weight. When no group
    /// reaches that, the single best-supported group is kept, so a map object with neighbours always gets one.
    /// Returns null when there are no candidates.
    /// </summary>
    public static Suggestion Suggest(BoundingBox target, IReadOnlyList<Candidate> candidates, int neighbourCount,
        float agreement, int wordCount)
    {
        if (candidates.Count == 0)
            return null;

        var targetCenter = Center(target);

        var nearest = candidates
            .Select(c => (c, gap: BoxDistance(target, c.Bounds), centre: Vector3.DistanceSquared(targetCenter, Center(c.Bounds))))
            .OrderBy(x => x.gap)
            .ThenBy(x => x.centre)
            .Take(Math.Max(1, neighbourCount))
            .ToList();

        int bitCount = wordCount * 32;
        var scores = new float[bitCount];
        float total = 0;

        foreach (var (c, gap, _) in nearest)
        {
            float weight = 1.0f / (1.0f + gap);
            total += weight;

            for (int g = 0; g < wordCount && g < c.Groups.Length; g++)
            {
                uint word = c.Groups[g];
                while (word != 0)
                {
                    int bit = BitOperations.TrailingZeroCount(word);
                    scores[g * 32 + bit] += weight;
                    word &= word - 1;
                }
            }
        }

        var groups = new uint[wordCount];
        int best = -1;

        for (int flat = 0; flat < bitCount; flat++)
        {
            if (scores[flat] <= 0)
                continue;

            if (best < 0 || scores[flat] > scores[best])
                best = flat;

            if (scores[flat] >= agreement * total)
                groups[flat / 32] |= 1u << (flat % 32);
        }

        if (groups.All(w => w == 0) && best >= 0)
            groups[best / 32] |= 1u << (best % 32);

        return new Suggestion
        {
            Groups = groups,
            NeighbourCount = nearest.Count,
            NearestDistance = nearest.First().gap,
            FarthestDistance = nearest.Last().gap
        };
    }
}
