using SoulsFormats;
using StudioCore.Editors.MapEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.Helpers;

public static class BoundingBoxHelper
{
    public static BoundingBox GetDerivedBoundingBox(Entity s)
    {
        var framingBounds = s.RenderSceneMesh.GetFramingBounds();

        // Calculate the bounding box for regions, as the debug primitives themselves do not.
        if (s.WrappedObject is IMsbRegion)
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

                    framingBounds = GenerateBoundingBox(position, width, height, depth);
                }

                // Cylinder
                if (shape is MSB.Shape.Cylinder)
                {
                    var cylinderShape = (MSB.Shape.Cylinder)shape;

                    var height = cylinderShape.Height;
                    var radius = cylinderShape.Radius;

                    framingBounds = GenerateBoundingBox(position, radius, height);
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
        else if(s.WrappedObject is BTL.Light)
        {
            var position = s.GetPropertyValue<Vector3>("Position");
            var radius = s.GetPropertyValue<float>("Radius");

            framingBounds = GenerateBoundingBox(position, radius);
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
