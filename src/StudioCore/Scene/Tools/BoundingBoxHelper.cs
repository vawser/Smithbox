using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.Tools;

public static class BoundingBoxHelper
{
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
