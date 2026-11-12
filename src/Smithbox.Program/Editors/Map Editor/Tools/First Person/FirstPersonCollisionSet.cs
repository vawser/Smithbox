using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>What a collision mesh of the first person mode was placed by.</summary>
public enum FirstPersonMeshKind
{
    /// <summary>A collision part's map collision file (h, l or f).</summary>
    Map,

    /// <summary>The collision of an asset or object, or of a prefab's member.</summary>
    Asset
}

/// <summary>
/// One placed collision mesh: a file's triangles in world space, under the part that places them.
/// </summary>
public sealed class FirstPersonMesh
{
    public FirstPersonMeshKind Kind;

    /// <summary>The map the placing part is in.</summary>
    public string MapId;

    /// <summary>The placing part's name, for status and diagnostics.</summary>
    public string PartName;

    /// <summary>The source the triangles came from: the collision file, or the asset's model and layer.</summary>
    public string Source;

    /// <summary>The world matrix the triangles were placed with, so a moved part is noticed.</summary>
    public Matrix4x4 World;

    public FirstPersonTriangleTree Tree;
}

/// <summary>A hit of a ray against the collision.</summary>
public struct FirstPersonHit
{
    public Vector3 Point;
    public Vector3 Normal;
    public float Distance;
    public FirstPersonMesh Mesh;
    public int Triangle;

    /// <summary>The triangle's collision material, or <see cref="FirstPersonTriangles.NoMaterial"/>.</summary>
    public uint Material;
}

/// <summary>
/// The placed collision meshes the first person mode walks on, and the queries over all of them. A plain list with
/// each mesh's bounds tested first: a map has at most a few thousand meshes, and the trees do the real work.
/// Depends only on System.Numerics and the tree, so the survey tool links it as it is.
/// </summary>
public sealed class FirstPersonCollisionSet
{
    public readonly List<FirstPersonMesh> Meshes = new();

    public int TriangleCount { get; private set; }

    public long MemoryBytes { get; private set; }

    public void Add(FirstPersonMesh mesh)
    {
        if (mesh?.Tree == null || mesh.Tree.TriangleCount == 0)
            return;

        Meshes.Add(mesh);
        TriangleCount += mesh.Tree.TriangleCount;
        MemoryBytes += mesh.Tree.MemoryBytes;
    }

    /// <summary>The nearest hit of the ray within maxDistance. The direction is normalised here.</summary>
    public bool RayCast(Vector3 origin, Vector3 direction, float maxDistance, out FirstPersonHit hit)
    {
        hit = default;

        var length = direction.Length();

        if (length < 1e-8f)
            return false;

        direction /= length;

        var inverse = new Vector3(1f / direction.X, 1f / direction.Y, 1f / direction.Z);
        var best = maxDistance;
        var found = false;

        foreach (var mesh in Meshes)
        {
            if (!FirstPersonTriangleTree.RayBox(origin, inverse, mesh.Tree.Min, mesh.Tree.Max, best, out _))
                continue;

            if (mesh.Tree.RayCast(origin, direction, best, out var distance, out var triangle))
            {
                best = distance;
                found = true;
                hit = new FirstPersonHit
                {
                    Distance = distance,
                    Point = origin + direction * distance,
                    Normal = mesh.Tree.Normal(triangle),
                    Mesh = mesh,
                    Triangle = triangle,
                    Material = mesh.Tree.Material(triangle)
                };
            }
        }

        // Facing the ray, as the triangles are two-sided
        if (found && Vector3.Dot(hit.Normal, direction) > 0)
            hit.Normal = -hit.Normal;

        return found;
    }

    /// <summary>Empties the set, keeping its list's capacity.</summary>
    public void Clear()
    {
        Meshes.Clear();
        TriangleCount = 0;
        MemoryBytes = 0;
    }

    /// <summary>Fills another set with the meshes whose bounds overlap the box (sharing their trees).</summary>
    public void CopyNearby(Vector3 min, Vector3 max, FirstPersonCollisionSet into)
    {
        into.Clear();

        foreach (var mesh in Meshes)
        {
            if (FirstPersonTriangleTree.BoxBox(min, max, mesh.Tree.Min, mesh.Tree.Max))
                into.Add(mesh);
        }
    }

    [ThreadStatic]
    private static List<int> OverlapScratch;

    /// <summary>Calls back for each triangle whose bounds overlap the box, with its corners.</summary>
    public void Overlap(Vector3 min, Vector3 max, Action<FirstPersonMesh, int, Vector3, Vector3, Vector3> visit)
    {
        var scratch = OverlapScratch ??= new List<int>();

        foreach (var mesh in Meshes)
        {
            if (!FirstPersonTriangleTree.BoxBox(min, max, mesh.Tree.Min, mesh.Tree.Max))
                continue;

            scratch.Clear();
            mesh.Tree.Overlap(min, max, scratch);

            var corners = mesh.Tree.Corners;

            foreach (var t in scratch)
                visit(mesh, t, corners[t * 3], corners[t * 3 + 1], corners[t * 3 + 2]);
        }
    }

    /// <summary>The bounds of everything, or false if the set is empty.</summary>
    public bool GetBounds(out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue);
        max = new Vector3(float.MinValue);

        foreach (var mesh in Meshes)
        {
            min = Vector3.Min(min, mesh.Tree.Min);
            max = Vector3.Max(max, mesh.Tree.Max);
        }

        return Meshes.Count > 0;
    }
}
