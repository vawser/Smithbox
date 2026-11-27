using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public static class BoundingBoxHelper
{
    /// <summary>The smallest half-size a framed region gets, so a point region doesn't frame at a few centimetres</summary>
    private const float MinRegionHalfExtent = 1.0f;

    public static BoundingBox GetDerivedBoundingBox(Entity s)
    {
        var framingBounds = s.RenderSceneMesh.GetFramingBounds();

        if (s.WrappedObject is IMsbRegion)
        {
            // The region's primitive is a unit shape that the proxy's world matrix sizes, turns and places (shape
            // size, region rotation and position, and the map's own transform), so its world bounds are the region.
            // Building a box from the region's Position alone (below, as before) left out the map's transform, which
            // put the frame elsewhere for maps placed away from the origin, such as the m60 tiles.
            var worldBounds = s.RenderSceneMesh.GetBounds();

            if (IsUsable(worldBounds))
                return EnsureMinimumSize(worldBounds, MinRegionHalfExtent);

            framingBounds = GetLocalRegionBounds(s, framingBounds);

            // Into world space through the map (the parents' transforms), when the proxy has no world yet
            var parentWorld = s.Parent?.GetWorldMatrix() ?? Matrix4x4.Identity;
            return EnsureMinimumSize(BoundingBox.Transform(framingBounds, parentWorld), MinRegionHalfExtent);
        }
        else if (s.WrappedObject is BTL.Light)
        {
            var position = s.GetPropertyValue<Vector3>("Position");
            var radius = s.GetPropertyValue<float>("Radius");

            framingBounds = GenerateBoundingBox(position, radius);

            // BTL positions are in the map's space too
            var parentWorld = s.Parent?.GetWorldMatrix() ?? Matrix4x4.Identity;
            framingBounds = BoundingBox.Transform(framingBounds, parentWorld);
        }

        return framingBounds;
    }

    private static bool IsUsable(BoundingBox box)
    {
        var size = box.Max - box.Min;

        return float.IsFinite(size.X) && float.IsFinite(size.Y) && float.IsFinite(size.Z) &&
            size.X >= 0 && size.Y >= 0 && size.Z >= 0 && size.LengthSquared() > 0;
    }

    private static BoundingBox EnsureMinimumSize(BoundingBox box, float halfExtent)
    {
        var center = (box.Min + box.Max) * 0.5f;
        var half = Vector3.Max((box.Max - box.Min) * 0.5f, new Vector3(halfExtent));

        return new BoundingBox(center - half, center + half);
    }

    /// <summary>
    /// A region's bounds in its map's space from its shape, used when its primitive has no world matrix yet. Boxes
    /// and cylinders grow up from the region's position, as the region primitives are drawn.
    /// </summary>
    private static BoundingBox GetLocalRegionBounds(Entity s, BoundingBox framingBounds)
    {
        {
            var position = s.GetPropertyValue<Vector3>("Position");
            var shape = s.GetPropertyValue<MSB.Shape>("Shape");

            if (shape != null)
            {
                // Composite
                if (shape is MSB.Shape.Composite)
                {
                    var compositeShape = (MSB.Shape.Composite)shape;
                }

                // Box
                if (shape is MSB.Shape.Box)
                {
                    var boxShape = (MSB.Shape.Box)shape;

                    var width = boxShape.Width;
                    var depth = boxShape.Depth;
                    var height = boxShape.Height;

                    // From the base up, as the region box is drawn
                    framingBounds = GenerateBoundingBox(position + new Vector3(0, height / 2, 0), width, height, depth);
                }

                // Cylinder
                if (shape is MSB.Shape.Cylinder)
                {
                    var cylinderShape = (MSB.Shape.Cylinder)shape;

                    var height = cylinderShape.Height;
                    var radius = cylinderShape.Radius;

                    // From the base up, as the region cylinder is drawn
                    framingBounds = GenerateBoundingBox(position + new Vector3(0, height / 2, 0), radius, height);
                }

                // Sphere
                if (shape is MSB.Shape.Sphere)
                {
                    var sphereShape = (MSB.Shape.Sphere)shape;

                    var radius = sphereShape.Radius;

                    framingBounds = GenerateBoundingBox(position, radius);
                }

                // Circle
                if (shape is MSB.Shape.Circle)
                {
                    var circleShape = (MSB.Shape.Circle)shape;

                    var height = 1.0f;
                    var radius = circleShape.Radius;

                    framingBounds = GenerateBoundingBox(position, radius, height);
                }

                // Rectangle
                if (shape is MSB.Shape.Rectangle)
                {
                    var rectShape = (MSB.Shape.Rectangle)shape;

                    var width = rectShape.Width;
                    var depth = rectShape.Depth;
                    var height = 1.0f;

                    framingBounds = GenerateBoundingBox(position, width, height, depth);
                }

                // Point
                if (shape is MSB.Shape.Point)
                {
                    var pointShape = (MSB.Shape.Point)shape;

                    var radius = 1.0f;

                    framingBounds = GenerateBoundingBox(position, radius);
                }
            }
        }

        return framingBounds;
    }

    /// <summary>
    /// Generate a bounding box based of Sphere shape properties
    /// </summary>
    public static BoundingBox GenerateBoundingBox(Vector3 center, float radius)
    {
        Vector3 extents = new Vector3(radius, radius, radius);
        Vector3 min = center - extents;
        Vector3 max = center + extents;

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Generate a bounding box based of Cylinder shape properties
    /// </summary>
    public static BoundingBox GenerateBoundingBox(Vector3 center, float radius, float height)
    {
        Vector3 halfExtents = new Vector3(radius, height / 2, radius);
        Vector3 min = center - halfExtents;
        Vector3 max = center + halfExtents;

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Generate a bounding box based of Box shape properties
    /// </summary>
    public static BoundingBox GenerateBoundingBox(Vector3 center, float width, float height, float depth)
    {
        Vector3 halfExtents = new Vector3(width / 2, height / 2, depth / 2);
        Vector3 min = center - halfExtents;
        Vector3 max = center + halfExtents;

        return new BoundingBox(min, max);
    }

}
