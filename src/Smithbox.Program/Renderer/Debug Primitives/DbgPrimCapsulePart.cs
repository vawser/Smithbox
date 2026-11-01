#nullable enable
using StudioCore.Utilities;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
///     The shape of a piece of a capsule: a unit dome over +Y, or a unit tube from Y 0 to 1.
///     A capsule of any size is two domes and a tube placed by their transforms, and a sphere is just the domes.
/// </summary>
internal static class CapsulePartShape
{
    public const int Sides = 16;
    public const int DomeRings = 5;

    public static readonly BoundingBox Bounds = new(new Vector3(-1, 0, -1), new Vector3(1, 1, 1));

    public static Vector3 DomePoint(int ring, int side)
    {
        var height = (float)ring / DomeRings * Utils.PiOver2;
        var angle = (float)side / Sides * Utils.Pi * 2;

        return new Vector3(MathF.Cos(angle) * MathF.Cos(height), MathF.Sin(height), MathF.Sin(angle) * MathF.Cos(height));
    }

    public static Vector3 TubePoint(int end, int side)
    {
        var angle = (float)side / Sides * Utils.Pi * 2;

        return new Vector3(MathF.Cos(angle), end, MathF.Sin(angle));
    }
}

public class DbgPrimSolidCapsulePart : DbgPrimSolid
{
    public override BoundingBox Bounds => CapsulePartShape.Bounds;
    public override BoundingBox FramingBounds => CapsulePartShape.Bounds;

    public DbgPrimSolidCapsulePart(bool isDome)
    {
        Func<int, int, Vector3> point = isDome ? CapsulePartShape.DomePoint : CapsulePartShape.TubePoint;
        var rings = isDome ? CapsulePartShape.DomeRings : 1;

        for (var ring = 0; ring < rings; ring++)
        {
            for (var side = 0; side < CapsulePartShape.Sides; side++)
            {
                var next = (side + 1) % CapsulePartShape.Sides;

                AddQuad(point(ring, side), point(ring + 1, side), point(ring + 1, next), point(ring, next), Color.White);
            }
        }

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}

public class DbgPrimWireCapsulePart : DbgPrimWire
{
    public override BoundingBox Bounds => CapsulePartShape.Bounds;
    public override BoundingBox FramingBounds => CapsulePartShape.Bounds;

    public DbgPrimWireCapsulePart(bool isDome)
    {
        Func<int, int, Vector3> point = isDome ? CapsulePartShape.DomePoint : CapsulePartShape.TubePoint;
        var rings = isDome ? CapsulePartShape.DomeRings : 1;

        for (var side = 0; side < CapsulePartShape.Sides; side++)
        {
            var next = (side + 1) % CapsulePartShape.Sides;

            for (var ring = 0; ring < rings; ring++)
            {
                AddLine(point(ring, side), point(ring + 1, side), Color.White);
            }

            for (var ring = 0; ring <= rings; ring++)
            {
                AddLine(point(ring, side), point(ring, next), Color.White);
            }
        }

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}
