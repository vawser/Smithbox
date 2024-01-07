using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireSpotLight : DbgPrimWire
{
    private static readonly DbgPrimGeometryData GeometryData = null;

    public DbgPrimWireSpotLight(Transform location, float radius, float angle, Color color)
    {
        NameColor = color;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            radius *= -1; // Spotlights are apparently backwards for some reason

            Vector3 TL = -Vector3.UnitZ * radius;
            Vector3 TR = -Vector3.UnitZ * radius;
            Vector3 BL = -Vector3.UnitZ * radius;
            Vector3 BR = -Vector3.UnitZ * radius;

            TL.X = -angle / 3f;
            BL.X = -angle / 3f;
            TR.X = angle / 3f;
            BR.X = angle / 3f;

            TL.Y = -angle / 3f;
            BL.Y = angle / 3f;
            TR.Y = -angle / 3f;
            BR.Y = angle / 3f;

            AddLine(Vector3.Zero, TL);
            AddLine(Vector3.Zero, TR);
            AddLine(Vector3.Zero, BL);
            AddLine(Vector3.Zero, BR);

            Vector3 T = -Vector3.UnitZ * radius;
            Vector3 B = -Vector3.UnitZ * radius;
            Vector3 L = -Vector3.UnitZ * radius;
            Vector3 R = -Vector3.UnitZ * radius;
            T.Y = -angle / 2;
            B.Y = angle / 2;
            L.X = -angle / 2;
            R.X = angle / 2;

            AddLine(Vector3.Zero, T);
            AddLine(Vector3.Zero, B);
            AddLine(Vector3.Zero, L);
            AddLine(Vector3.Zero, R);

            AddLine(TL, T);
            AddLine(T, TR);
            AddLine(TR, R);
            AddLine(R, BR);
            AddLine(BR, B);
            AddLine(B, BL);
            AddLine(BL, L);
            AddLine(L, TL);

            // sphere
            var sphereRadius = radius * 0.02f;
            var numVerticalSegments = 8;
            var numSidesPerSegment = 8;
            Vector3 topPoint = Vector3.UnitY * sphereRadius;
            Vector3 bottomPoint = -Vector3.UnitY * sphereRadius;
            var points = new Vector3[numVerticalSegments, numSidesPerSegment];

            for (var i = 0; i < numVerticalSegments; i++)
            {
                for (var j = 0; j < numSidesPerSegment; j++)
                {
                    var horizontalAngle = 1.0f * j / numSidesPerSegment * Utils.Pi * 2.0f;
                    var verticalAngle = (1.0f * (i + 1) / (numVerticalSegments + 1) * Utils.Pi) - Utils.PiOver2;
                    var altitude = (float)Math.Sin(verticalAngle);
                    var horizontalDist = (float)Math.Cos(verticalAngle);
                    points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude,
                        (float)Math.Sin(horizontalAngle) * horizontalDist) * sphereRadius;
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
