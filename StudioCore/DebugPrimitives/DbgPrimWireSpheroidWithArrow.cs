using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireSpheroidWithArrow : DbgPrimWire
{
    private static readonly DbgPrimGeometryData GeometryData = null;

    public DbgPrimWireSpheroidWithArrow(Transform location, float radius, Color color, int numVerticalSegments = 11,
        int numSidesPerSegment = 12, bool flatBottom = false, bool reverseDirection = false)
    {
        NameColor = color;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            #region Sphere

            Vector3 topPoint;
            Vector3 bottomPoint;
            if (flatBottom)
            {
                topPoint = Vector3.UnitY * radius;
                bottomPoint = Vector3.Zero;
            }
            else
            {
                topPoint = Vector3.UnitY * radius;
                bottomPoint = -Vector3.UnitY * radius;
            }

            var points = new Vector3[numVerticalSegments, numSidesPerSegment];

            for (var i = 0; i < numVerticalSegments; i++)
            {
                for (var j = 0; j < numSidesPerSegment; j++)
                {
                    var horizontalAngle = 1.0f * j / numSidesPerSegment * Utils.Pi * 2.0f;
                    var verticalAngle = (1.0f * (i + 1) / (numVerticalSegments + 1) * Utils.Pi) - Utils.PiOver2;
                    var altitude = (float)Math.Sin(verticalAngle);
                    if (flatBottom && altitude < 0)
                    {
                        altitude = 0f;
                    }

                    var horizontalDist = (float)Math.Cos(verticalAngle);
                    points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude,
                        (float)Math.Sin(horizontalAngle) * horizontalDist) * radius;
                }
            }

            for (var i = 0; i < numVerticalSegments; i++)
            {
                for (var j = 0; j < numSidesPerSegment; j++)
                {
                    // On the bottom, we must connect each to the bottom point
                    if (i == 0)
                    {
                        AddLine(points[i, j], bottomPoint, color);
                    }

                    // On the top, we must connect each point to the top
                    // Note: this isn't "else if" because with 2 segments, 
                    // these are both true for the only ring
                    if (i == numVerticalSegments - 1)
                    {
                        AddLine(points[i, j], topPoint, color);
                    }

                    // Make vertical lines that connect from this 
                    // horizontal ring to the one above
                    // Since we are connecting 
                    // (current) -> (the one above current)
                    // we dont need to do this for the very last one.
                    if (i < numVerticalSegments - 1)
                    {
                        AddLine(points[i, j], points[i + 1, j], color);
                    }


                    // Make lines that connect points horizontally
                    //---- if we reach end, we must wrap around, 
                    //---- otherwise, simply make line to next one
                    if (j == numSidesPerSegment - 1)
                    {
                        AddLine(points[i, j], points[i, 0], color);
                    }
                    else
                    {
                        AddLine(points[i, j], points[i, j + 1], color);
                    }
                }
            }

            #endregion

            #region Arrow

            var Segments = 8;

            var TipRadius = 0.10f;

            Vector3 GetPoint(int segmentIndex, float radius, float depth)
            {
                var horizontalAngle = 1.0f * segmentIndex / Segments * Utils.Pi * 2.0f;
                var x = (float)Math.Cos(horizontalAngle);
                var y = (float)Math.Sin(horizontalAngle);
                return new Vector3(x * radius, y * radius, depth);
            }

            //AddLine(Vector3.Zero, Vector3.UnitY * OrientingSpinesLength);
            //AddLine(Vector3.Zero, -Vector3.UnitX * OrientingSpinesLength * 0.5f);

            Vector3 tip = -Vector3.UnitZ;
            if (reverseDirection)
            {
                tip = Vector3.UnitZ;
            }

            for (var i = 0; i <= Segments; i++)
            {
                Vector3 ptTipStartOuter = GetPoint(i, TipRadius, 0);

                AddLine(ptTipStartOuter, tip, color);

                // Connect this vertical spline to the previous one with horizontal lines.
                if (i > 0)
                {
                    Vector3 prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 0);

                    AddLine(prevPtTipStartOuter, ptTipStartOuter, color);
                }
            }

            #endregion


            //FinalizeBuffers(true);

            /* //crashes
            GeometryData = new DbgPrimGeometryData()
            {
                GeomBuffer = GeometryBuffer
            };*/
            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                UpdatePerFrameResources(d, cl, null);
            });
        }
    }

    public bool RayCast(Ray ray, Matrix4x4 transform, out float dist)
    {
        var radius = Vector3.TransformNormal(Vector3.UnitX, transform).Length();
        Vector3 pos = Vector3.Transform(Vector3.Zero, transform);
        return Utils.RaySphereIntersection(ref ray, pos, radius, out dist);
    }
}
