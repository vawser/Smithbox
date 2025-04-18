using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireCylinder : DbgPrimWire
{
    public DbgPrimWireCylinder(Transform location, float range, float height, int numSegments, Color color)
    {
        NameColor = color;

        var top = height; //height / 2;
        var bottom = 0.0f; //-top;

        for (var i = 0; i < numSegments; i++)
        {
            var angle = 1.0f * i / numSegments * Utils.Pi * 2.0f;
            var x = (float)Math.Cos(angle);
            var z = (float)Math.Sin(angle);
            //Very last one wraps around to the first one
            if (i == numSegments - 1)
            {
                var x_next = (float)Math.Cos(0);
                var z_next = (float)Math.Sin(0);
                // Create line to wrap around to first point
                //---- Top
                AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                //---- Bottom
                AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
            }
            else
            {
                var angle_next = 1.0f * (i + 1) / numSegments * Utils.Pi * 2.0f;
                var x_next = (float)Math.Cos(angle_next);
                var z_next = (float)Math.Sin(angle_next);

                // Create line to next point in ring
                //---- Top
                AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                //---- Bottom
                AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
            }

            // Make pillar from top to bottom
            AddLine(new Vector3(x, bottom, z), new Vector3(x, top, z), color);
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }

    public bool RayCast(Ray ray, Matrix4x4 transform, out float dist)
    {
        // Use an AABB to approximate this I don't care cylinder intersections are scary
        BoundingBox bb = BoundingBox.Transform(new BoundingBox(new Vector3(-1.0f, 0.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f)), transform);
        return Utils.RayBoxIntersection(ref ray, ref bb, out dist);
    }
}
