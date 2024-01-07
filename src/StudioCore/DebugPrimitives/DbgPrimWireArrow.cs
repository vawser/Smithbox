using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireArrow : DbgPrimWire
{
    public const int Segments = 6;

    public const float TipLength = 0.25f;
    public const float TipRadius = 0.10f;
    public const float StemRadius = 0.04f;

    public const float OrientingSpinesLength = 0.5f;

    private readonly DbgPrimGeometryData GeometryData;

    public DbgPrimWireArrow(string name, Transform location, Color color)
    {
        Category = DbgPrimCategory.HkxBone;

        NameColor = color;
        Name = name;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            Vector3 GetPoint(int segmentIndex, float radius, float depth)
            {
                var horizontalAngle = 1.0f * segmentIndex / Segments * Utils.Pi * 2.0f;
                var x = (float)Math.Cos(horizontalAngle);
                var y = (float)Math.Sin(horizontalAngle);
                return new Vector3(x * radius, y * radius, depth);
            }

            AddLine(Vector3.Zero, Vector3.UnitY * OrientingSpinesLength);
            AddLine(Vector3.Zero, -Vector3.UnitX * OrientingSpinesLength * 0.5f);

            Vector3 tip = Vector3.UnitZ;

            for (var i = 0; i <= Segments; i++)
            {
                Vector3 ptBase = GetPoint(i, StemRadius, 0);
                Vector3 ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);
                Vector3 ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);

                AddLine(ptBase, ptTipStartInner, color);
                AddLine(ptTipStartInner, ptTipStartOuter, color);
                AddLine(ptTipStartOuter, tip, color);

                // Connect this vertical spline to the previous one with horizontal lines.
                if (i > 0)
                {
                    Vector3 prevPtBase = GetPoint(i - 1, StemRadius, 0);
                    Vector3 prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);
                    Vector3 prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                    AddLine(prevPtBase, ptBase, color);
                    AddLine(prevPtTipStartInner, ptTipStartInner, color);
                    AddLine(prevPtTipStartOuter, ptTipStartOuter, color);
                }
            }

            //FinalizeBuffers(true);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                UpdatePerFrameResources(d, cl, null);
            });
        }
    }
}
