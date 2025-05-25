using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using StudioCore.Scene;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimWireChain : DbgPrimWire
{
    private readonly DbgPrimGeometryData GeometryData = null;

    public override BoundingBox Bounds => new(new Vector3(float.MinValue), new Vector3(float.MaxValue));

    public DbgPrimWireChain(List<Vector3> points, List<Vector3> looseStartingPoints, Color color, bool endAtStart = true, bool linkAll = false)
    {
        NameColor = color;
        BaseColor = color;
        HighlightedColor = color;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            if (linkAll)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    for (var i2 = 0; i2 < points.Count; i2++)
                    {
                        AddLine(points[i], points[i2]);
                    }
                }
            }
            else
            {
                var lastPoint = points[0];

                for (var i = 1; i < points.Count; i++)
                {
                    var point = points[i];
                    AddLine(lastPoint, point, color);
                    lastPoint = point;
                }

                if (endAtStart)
                    AddLine(lastPoint, points[0], color);
            }
            foreach (var looseStartPos in looseStartingPoints)
            {
                AddLine(looseStartPos, points[0]);
            }

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };

            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                UpdatePerFrameResources(d, cl, null);
            });
        }
    }

    public void UpdateTransform(Transform newTransform)
    {
    }
    public bool RayCast(Ray ray, Matrix4x4 transform, out float dist)
    {
        dist = 0;
        return false;
    }
}
