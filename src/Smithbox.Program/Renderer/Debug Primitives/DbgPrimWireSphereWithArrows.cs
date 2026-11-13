using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// A wire sphere with arrows out of it, showing which way the thing it marks faces. Used for the dummy and bone
/// markers of the Model and Time Act Editors.
/// </summary>
public class DbgPrimWireSphereWithArrows : DbgPrimWireSphere
{
    /// <summary>
    /// An arrow from the sphere's surface along <paramref name="Direction"/> (in the marker's own space), reaching
    /// <paramref name="Length"/> sphere radii from the centre, with a head <paramref name="HeadSize"/> radii long.
    /// </summary>
    public readonly record struct Arrow(Vector3 Direction, float Length, float HeadSize);

    public DbgPrimWireSphereWithArrows(Transform location, float radius, Color color, int numVerticalSegments,
        int numSidesPerSegment, params Arrow[] arrows)
        : base(location, radius, color, numVerticalSegments, numSidesPerSegment)
    {
        // The sphere's geometry is uploaded by a task the base queues for the next frame, which picks these up too
        foreach (var arrow in arrows)
        {
            AddArrow(arrow, radius, color);
        }
    }

    private void AddArrow(Arrow arrow, float radius, Color color)
    {
        var direction = Vector3.Normalize(arrow.Direction);

        var start = direction * radius;
        var tip = direction * (radius * arrow.Length);
        var headLength = radius * arrow.HeadSize;
        var headBase = tip - direction * headLength;

        // Two directions square to the arrow, for the corners of its head
        var helper = MathF.Abs(direction.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        var side = Vector3.Normalize(Vector3.Cross(direction, helper)) * headLength * 0.5f;
        var other = Vector3.Normalize(Vector3.Cross(direction, side)) * headLength * 0.5f;

        // Shaft
        AddLine(start, tip, color);

        // Head: four lines back from the tip, joined at their ends so it reads as a cone from any side
        Vector3[] corners = [headBase + side, headBase + other, headBase - side, headBase - other];

        for (var i = 0; i < corners.Length; i++)
        {
            AddLine(tip, corners[i], color);
            AddLine(corners[i], corners[(i + 1) % corners.Length], color);
        }
    }
}
