using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimSolidArrow : DbgPrimSolid
{
    public const int Segments = 12;
    public const float TipLength = 0.25f;
    public const float TipRadius = 0.25f;
    public const float StemRadius = 0.1f;

    private readonly DbgPrimGeometryData GeometryData;

    public DbgPrimSolidArrow()
    {
        //BackfaceCulling = false;

        Category = DbgPrimCategory.HkxBone;

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

            Vector3 tip = Vector3.UnitZ;

            for (var i = 1; i <= Segments; i++)
            {
                Vector3 ptBase = GetPoint(i, StemRadius, 0);

                Vector3 prevPtBase = GetPoint(i - 1, StemRadius, 0);

                //Face: Part of Bottom
                AddTri(Vector3.Zero, prevPtBase, ptBase, Color.Green);
            }

            for (var i = 1; i <= Segments; i++)
            {
                Vector3 ptBase = GetPoint(i, StemRadius, 0);
                Vector3 ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);

                Vector3 prevPtBase = GetPoint(i - 1, StemRadius, 0);
                Vector3 prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);

                // Face: Base to Tip Inner
                AddQuad(prevPtBase, prevPtTipStartInner, ptTipStartInner, ptBase, Color.Green);
            }

            for (var i = 1; i <= Segments; i++)
            {
                Vector3 ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);
                Vector3 ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);

                Vector3 prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);
                Vector3 prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                // Face: Tip Start Inner to Tip Start Outer
                AddQuad(prevPtTipStartInner, prevPtTipStartOuter, ptTipStartOuter, ptTipStartInner, Color.Green);
            }

            for (var i = 1; i <= Segments; i++)
            {
                Vector3 ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);
                Vector3 prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                // Face: Tip Start to Tip
                AddTri(prevPtTipStartOuter, tip, ptTipStartOuter, Color.Green);
            }

            //FinalizeBuffers(true);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }
    }
}
