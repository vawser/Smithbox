using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Scene.DebugPrimitives
{
    public class DbgPrimWireSphereForwardUp : DbgPrimWire
    {
        private static readonly DbgPrimGeometryData GeometryData = null;

        public DbgPrimWireSphereForwardUp(Transform location, float radius, Color color, Color forwardColor,
            Color upColor, int numVerticalSegments = 11, int numSidesPerSegment = 12)
        {
            NameColor = color;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.GeomBuffer);
            }
            else
            {
                Vector3 topPoint = Vector3.UnitY * radius;
                Vector3 bottomPoint = -Vector3.UnitY * radius;
                var points = new Vector3[numVerticalSegments, numSidesPerSegment];

                for (var i = 0; i < numVerticalSegments; i++)
                {
                    for (var j = 0; j < numSidesPerSegment; j++)
                    {
                        var horizontalAngle = 1.0f * j / numSidesPerSegment * Utils.Pi * 2.0f;
                        var verticalAngle = 1.0f * (i + 1) / (numVerticalSegments + 1) * Utils.Pi - Utils.PiOver2;
                        var altitude = (float)Math.Sin(verticalAngle);
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

                float m = 1.5f * radius;
                AddLine(Vector3.Zero, Vector3.UnitX * m, forwardColor);
                AddLine(Vector3.UnitX * m, new Vector3(1 - 0.2f, +0.2f, 0) * m, forwardColor);
                AddLine(Vector3.UnitX * m, new Vector3(1 - 0.2f, -0.2f, 0) * m, forwardColor);
                AddLine(Vector3.UnitX * m, new Vector3(1 - 0.2f, 0, +0.2f) * m, forwardColor);
                AddLine(Vector3.UnitX * m, new Vector3(1 - 0.2f, 0, -0.2f) * m, forwardColor);
                AddLine(Vector3.Zero, Vector3.UnitY * m, upColor);
                AddLine(Vector3.UnitY * m, new Vector3(0, 1 - 0.2f, +0.2f) * m, upColor);
                AddLine(Vector3.UnitY * m, new Vector3(0, 1 - 0.2f, -0.2f) * m, upColor);
                AddLine(Vector3.UnitY * m, new Vector3(+0.2f, 1 - 0.2f, 0f) * m, upColor);
                AddLine(Vector3.UnitY * m, new Vector3(-0.2f, 1 - 0.2f, 0f) * m, upColor);

                Renderer.AddBackgroundUploadTask((d, cl) =>
                {
                    UpdatePerFrameResources(d, cl, null);
                });
            }
        }
    }
}