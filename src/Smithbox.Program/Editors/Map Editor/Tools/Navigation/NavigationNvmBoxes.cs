using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The box tree of an NVM (DeS, DS1, DS1R), rebuilt the way the game files have it (Navigation Editor Other Games Plan,
/// Phase N6): a quadtree of depth 3 under the root box, every inner box split at its XZ midpoint into four children
/// (1 = min X min Z, 2 = max X min Z, 3 = max X max Z, 4 = min X max Z), every box spanning the root's Y, and each of the
/// 64 leaves listing the triangles whose XZ footprint touches it. Renderer-free, so Tools/NavSurvey checks it against
/// every game file.
/// </summary>
public static class NvmBoxRule
{
    /// <summary>The depth of every leaf in the game files.</summary>
    public const int Depth = 3;

    /// <summary>
    /// A new tree over the corners given (the game's root box), with the triangles placed in its leaves.
    /// </summary>
    public static NVM.Box Rebuild(NVM nvm, Vector3 min, Vector3 max)
    {
        var all = new List<int>(nvm.Triangles.Count);
        for (int t = 0; t < nvm.Triangles.Count; t++)
            all.Add(t);

        return Build(nvm, min.X, min.Z, max.X, max.Z, min.Y, max.Y, 0, all);
    }

    private static NVM.Box Build(NVM nvm, double minX, double minZ, double maxX, double maxZ, float minY, float maxY, int depth, List<int> candidates)
    {
        var min = new Vector3((float)minX, minY, (float)minZ);
        var max = new Vector3((float)maxX, maxY, (float)maxZ);
        var box = new NVM.Box { MinValueCorner = min, MaxValueCorner = max, TriangleIndices = new List<int>() };

        // Only the triangles touching the parent can touch a child
        var here = new List<int>();
        foreach (var t in candidates)
        {
            if (TriangleTouchesRect(nvm, t, min, max))
                here.Add(t);
        }

        if (depth == Depth)
        {
            box.TriangleIndices = here;
            return box;
        }

        // In float, from the stored corners, as the game's files have them: the split at min + (max - min) / 2 and the
        // far side at min + (max - min), which isn't always the parent's max once rounded ((min + max) / 2, the
        // parent's max, split + half and double precision each miss on some files)
        float sizeX = max.X - min.X, sizeZ = max.Z - min.Z;
        float midX = min.X + sizeX / 2, midZ = min.Z + sizeZ / 2;
        float farX = min.X + sizeX, farZ = min.Z + sizeZ;
        box.ChildBox1 = Build(nvm, min.X, min.Z, midX, midZ, minY, maxY, depth + 1, here);
        box.ChildBox2 = Build(nvm, midX, min.Z, farX, midZ, minY, maxY, depth + 1, here);
        box.ChildBox3 = Build(nvm, midX, midZ, farX, farZ, minY, maxY, depth + 1, here);
        box.ChildBox4 = Build(nvm, min.X, midZ, midX, farZ, minY, maxY, depth + 1, here);
        return box;
    }

    /// <summary>
    /// Whether a triangle's XZ footprint touches an XZ rectangle, edges included (a separating axis test over the
    /// rectangle's two axes and the triangle's three edge normals).
    /// </summary>
    public static bool TriangleTouchesRect(NVM nvm, int triangle, Vector3 min, Vector3 max)
    {
        var tri = nvm.Triangles[triangle];
        var a = new Vector2(nvm.Vertices[tri.VertexIndex1].X, nvm.Vertices[tri.VertexIndex1].Z);
        var b = new Vector2(nvm.Vertices[tri.VertexIndex2].X, nvm.Vertices[tri.VertexIndex2].Z);
        var c = new Vector2(nvm.Vertices[tri.VertexIndex3].X, nvm.Vertices[tri.VertexIndex3].Z);
        return TriangleTouchesRect(a, b, c, new Vector2(min.X, min.Z), new Vector2(max.X, max.Z));
    }

    public static bool TriangleTouchesRect(Vector2 a, Vector2 b, Vector2 c, Vector2 min, Vector2 max)
    {
        // The rectangle's axes
        if (Math.Max(a.X, Math.Max(b.X, c.X)) < min.X || Math.Min(a.X, Math.Min(b.X, c.X)) > max.X)
            return false;
        if (Math.Max(a.Y, Math.Max(b.Y, c.Y)) < min.Y || Math.Min(a.Y, Math.Min(b.Y, c.Y)) > max.Y)
            return false;

        // The triangle's edge normals
        Span<Vector2> corners = stackalloc Vector2[] { min, new Vector2(max.X, min.Y), max, new Vector2(min.X, max.Y) };
        Span<Vector2> tri = stackalloc Vector2[] { a, b, c };
        for (int i = 0; i < 3; i++)
        {
            var p = tri[i];
            var q = tri[(i + 1) % 3];
            var normal = new Vector2(q.Y - p.Y, p.X - q.X);
            if (normal == Vector2.Zero)
                continue;

            float t0 = Vector2.Dot(normal, a), t1 = Vector2.Dot(normal, b), t2 = Vector2.Dot(normal, c);
            float tMin = Math.Min(t0, Math.Min(t1, t2)), tMax = Math.Max(t0, Math.Max(t1, t2));

            float rMin = float.MaxValue, rMax = float.MinValue;
            foreach (var corner in corners)
            {
                float d = Vector2.Dot(normal, corner);
                rMin = Math.Min(rMin, d);
                rMax = Math.Max(rMax, d);
            }

            if (rMax < tMin || rMin > tMax)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Every leaf of a tree, in the order the reader walks them (child 1 to 4, depth first).
    /// </summary>
    public static List<NVM.Box> Leaves(NVM.Box root)
    {
        var leaves = new List<NVM.Box>();
        void Walk(NVM.Box box)
        {
            if (box == null)
                return;

            if (box.ChildBox1 == null && box.ChildBox2 == null && box.ChildBox3 == null && box.ChildBox4 == null)
            {
                leaves.Add(box);
                return;
            }

            Walk(box.ChildBox1);
            Walk(box.ChildBox2);
            Walk(box.ChildBox3);
            Walk(box.ChildBox4);
        }

        Walk(root);
        return leaves;
    }
}
