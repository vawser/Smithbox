using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The fields of a GrassTypeParam row that determine how it is laid out.
/// </summary>
public record GrassType(
    string ModelName,
    float BaseDensity,
    float NormalInfluence,
    float InclinationMax,
    float InclinationJitter,
    float ScaleBaseMin,
    float ScaleBaseMax,
    float ScaleHeightMin,
    float ScaleHeightMax,
    float OrientationAngle,
    float OrientationRange,
    float Spacing);

/// <summary>
/// Spreads the instances of a grass type across the faces of a GRASS mesh.
/// The game places grass at runtime, so this is an approximation built from the param fields, not a match for it.
/// </summary>
public static class GrassPlacement
{
    /// <summary>
    /// Instances per square metre of a face with a density of 1, for a type with a base density of 1.
    /// Not known from the game: chosen as the grass models are roughly 2 metres wide.
    /// </summary>
    public const float InstancesPerSquareMetre = 0.5f;

    /// <summary>
    /// Gets the transforms of a grass type, relative to the parent model.
    /// The seed keeps the result the same between runs, so grass doesn't shift whenever it is rebuilt.
    /// </summary>
    public static Matrix4x4[] Scatter(GRASS grass, int slot, GrassType type, float densityScale, int seed)
    {
        var random = new Random(seed);
        var transforms = new List<Matrix4x4>();
        var placed = new Dictionary<(int, int, int), List<Vector3>>();

        foreach (GRASS.Face face in grass.Faces)
        {
            GRASS.Vertex a = grass.Vertices[face.VertexIndexA];
            GRASS.Vertex b = grass.Vertices[face.VertexIndexB];
            GRASS.Vertex c = grass.Vertices[face.VertexIndexC];

            var densityA = a.GrassDensities[slot];
            var densityB = b.GrassDensities[slot];
            var densityC = c.GrassDensities[slot];
            var maxDensity = MathF.Max(densityA, MathF.Max(densityB, densityC));

            if (maxDensity <= 0)
                continue;

            Vector3 cross = Vector3.Cross(b.Position - a.Position, c.Position - a.Position);
            var area = cross.Length() * 0.5f;

            if (area <= 0)
                continue;

            // The stored normals aren't always unit length, so it is only used for which way the face points
            Vector3 normal = Vector3.Normalize(cross);
            if (Vector3.Dot(normal, face.Normal) < 0)
            {
                normal = -normal;
            }

            var expected = area * maxDensity * type.BaseDensity * densityScale * InstancesPerSquareMetre;
            var count = (int)expected;

            if (random.NextSingle() < expected - count)
            {
                count++;
            }

            for (var i = 0; i < count; i++)
            {
                // Uniform point within the triangle
                var root = MathF.Sqrt(random.NextSingle());
                var weight = random.NextSingle();
                var u = 1 - root;
                var v = root * (1 - weight);
                var w = root * weight;

                // Candidates are generated at the highest density of the face, which keeps the density gradient
                var density = u * densityA + v * densityB + w * densityC;
                if (random.NextSingle() * maxDensity > density)
                    continue;

                Vector3 position = a.Position * u + b.Position * v + c.Position * w;

                if (type.Spacing > 0 && !TryReserve(placed, position, type.Spacing))
                    continue;

                transforms.Add(CreateTransform(position, normal, type, random));
            }
        }

        return transforms.ToArray();
    }

    /// <summary>
    /// Claims a position unless it is within the spacing of one already placed. The positions are
    /// grouped into cells the size of the spacing, so only the neighbouring cells need checking.
    /// </summary>
    private static bool TryReserve(Dictionary<(int, int, int), List<Vector3>> placed, Vector3 position, float spacing)
    {
        var cellX = (int)MathF.Floor(position.X / spacing);
        var cellY = (int)MathF.Floor(position.Y / spacing);
        var cellZ = (int)MathF.Floor(position.Z / spacing);

        for (var x = cellX - 1; x <= cellX + 1; x++)
        {
            for (var y = cellY - 1; y <= cellY + 1; y++)
            {
                for (var z = cellZ - 1; z <= cellZ + 1; z++)
                {
                    if (!placed.TryGetValue((x, y, z), out var cell))
                        continue;

                    foreach (Vector3 other in cell)
                    {
                        if (Vector3.DistanceSquared(position, other) < spacing * spacing)
                            return false;
                    }
                }
            }
        }

        if (!placed.TryGetValue((cellX, cellY, cellZ), out var own))
        {
            own = new List<Vector3>();
            placed[(cellX, cellY, cellZ)] = own;
        }

        own.Add(position);
        return true;
    }

    private static Matrix4x4 CreateTransform(Vector3 position, Vector3 normal, GrassType type, Random random)
    {
        var width = Lerp(type.ScaleBaseMin, type.ScaleBaseMax, random.NextSingle()) / 100f;
        var height = Lerp(type.ScaleHeightMin, type.ScaleHeightMax, random.NextSingle()) / 100f;

        var yaw = type.OrientationAngle < 0
            ? random.NextSingle() * MathF.Tau
            : ToRadians(type.OrientationAngle + MathF.Max(type.OrientationRange, 0) * (random.NextSingle() * 2 - 1));

        var jitterAxis = random.NextSingle() * MathF.Tau;
        var jitterAngle = ToRadians(type.InclinationJitter * random.NextSingle());
        var jitter = Quaternion.CreateFromAxisAngle(new Vector3(MathF.Cos(jitterAxis), 0, MathF.Sin(jitterAxis)), jitterAngle);

        return Matrix4x4.CreateScale(width, height, width)
            * Matrix4x4.CreateRotationY(yaw)
            * Matrix4x4.CreateFromQuaternion(jitter * GetTilt(normal, type))
            * Matrix4x4.CreateTranslation(position);
    }

    /// <summary>
    /// The rotation that leans a model towards the surface: as much as the normal influence asks for,
    /// and no further than the maximum inclination.
    /// </summary>
    private static Quaternion GetTilt(Vector3 normal, GrassType type)
    {
        Vector3 leaned = Vector3.Lerp(Vector3.UnitY, normal, type.NormalInfluence / 100f);
        Vector3 axis = Vector3.Cross(Vector3.UnitY, leaned);

        if (axis.LengthSquared() < 1e-8f)
            return Quaternion.Identity;

        var angle = MathF.Atan2(axis.Length(), Vector3.Dot(Vector3.UnitY, leaned));

        return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.Min(angle, ToRadians(type.InclinationMax)));
    }

    private static float Lerp(float min, float max, float amount)
    {
        return min + (max - min) * amount;
    }

    private static float ToRadians(float degrees)
    {
        return degrees * MathF.PI / 180f;
    }
}
