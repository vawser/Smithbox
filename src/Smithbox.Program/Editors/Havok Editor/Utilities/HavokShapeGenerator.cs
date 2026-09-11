using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public static class HavokShapeGenerator
{
    public static (List<Vector3> Vertices, List<int> Indices) GenerateSquare(float width, float length)
    {
        if (width <= 0f) throw new ArgumentOutOfRangeException(nameof(width));
        if (length <= 0f) throw new ArgumentOutOfRangeException(nameof(length));

        float hw = width * 0.5f;
        float hl = length * 0.5f;

        var vertices = new List<Vector3>
        {
            new Vector3(-hw, 0f, -hl), // 0
            new Vector3(hw, 0f, -hl), // 1
            new Vector3(hw, 0f, hl), // 2
            new Vector3(-hw, 0f, hl), // 3
        };

        var indices = new List<int>
        {
            0, 1, 2,
            0, 2, 3
        };

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat equilateral triangle centered at the origin.
    /// </summary>
    /// <param name="size">Distance from the center to each corner (circumradius).</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateTriangle(float size)
    {
        if (size <= 0f) throw new ArgumentOutOfRangeException(nameof(size));

        var vertices = new List<Vector3>(3);
        for (int i = 0; i < 3; i++)
        {
            // Start pointing toward +Z, then sweep counter-clockwise (increasing angle).
            float angle = MathF.PI / 2f + i * (2f * MathF.PI / 3f);
            vertices.Add(new Vector3(size * MathF.Cos(angle), 0f, size * MathF.Sin(angle)));
        }

        var indices = new List<int> { 0, 1, 2 };

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat circle (approximated as a regular polygon fan) centered
    /// at the origin.
    /// </summary>
    /// <param name="radius">Circle radius.</param>
    /// <param name="segments">Number of edge segments approximating the circle. Higher = rounder.</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateCircle(float radius, int segments = 24)
    {
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius));
        if (segments < 3) throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 3 segments.");

        var vertices = new List<Vector3>(segments + 1)
        {
            Vector3.Zero // center, index 0
        };

        for (int i = 0; i < segments; i++)
        {
            float angle = i * (2f * MathF.PI / segments);
            vertices.Add(new Vector3(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
        }

        var indices = new List<int>(segments * 3);
        for (int i = 0; i < segments; i++)
        {
            int current = 1 + i;
            int next = 1 + (i + 1) % segments;
            indices.Add(0);
            indices.Add(current);
            indices.Add(next);
        }

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat half-disc (semicircle), centered at the origin, with its
    /// straight diameter edge along the X axis and the arc bulging toward +Z.
    /// </summary>
    /// <param name="radius">Circle radius.</param>
    /// <param name="segments">Number of edge segments approximating the arc. Higher = rounder.</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateSemiCircle(float radius, int segments = 12)
    {
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius));
        if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 1 segment.");

        // Center + (segments + 1) arc points running from (radius, 0, 0) around
        // to (-radius, 0, 0). The two radius edges from the center to the first
        // and last arc points are collinear (both along X), so together they
        // form the straight diameter edge closing the shape.
        var vertices = new List<Vector3>(segments + 2)
        {
            Vector3.Zero // center, index 0
        };

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (MathF.PI / segments); // 0 .. PI inclusive
            vertices.Add(new Vector3(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
        }

        var indices = new List<int>(segments * 3);
        for (int i = 0; i < segments; i++)
        {
            int current = 1 + i;
            int next = 1 + i + 1;
            indices.Add(0);
            indices.Add(current);
            indices.Add(next);
        }

        return (vertices, indices);
    }

    public static (List<Vector3> Vertices, List<int> Indices) GenerateFlverCollision(
        FLVER2 model,
        IReadOnlyList<int> meshIndices = null)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        if (model == null || model.Meshes == null || model.Meshes.Count == 0)
        {
            return (vertices, indices);
        }

        IEnumerable<int> targetIndices = meshIndices ?? Enumerable.Range(0, model.Meshes.Count);

        var weldMap = new Dictionary<Vector3, int>();

        foreach (int meshIndex in targetIndices)
        {
            if (meshIndex < 0 || meshIndex >= model.Meshes.Count)
                continue;

            var mesh = model.Meshes[meshIndex];
            var faces = mesh.GetFaces();

            foreach (var tri in faces)
            {
                Span<int> triIndices = stackalloc int[3];

                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = tri[i].Position;

                    if (!weldMap.TryGetValue(pos, out int vertIndex))
                    {
                        vertIndex = vertices.Count;
                        vertices.Add(pos);
                        weldMap[pos] = vertIndex;
                    }

                    triIndices[i] = vertIndex;
                }

                if (triIndices[0] == triIndices[1]
                    || triIndices[1] == triIndices[2]
                    || triIndices[2] == triIndices[0])
                {
                    continue;
                }

                indices.Add(triIndices[0]);
                indices.Add(triIndices[1]);
                indices.Add(triIndices[2]);
            }
        }

        return (vertices, indices);
    }

    public static void FlipTriangleWinding(List<int> indices)
    {
        for (int i = 0; i + 2 < indices.Count; i += 3)
        {
            (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
        }
    }
}