using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimBulb : DbgPrimSolid
{
    private readonly DbgPrimGeometryData GeometryData;

    public override BoundingBox Bounds => new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 2, 1));

    public DbgPrimBulb(Transform location)
    {
        var scale = 0.2f;

        var trunkHeight = 0.25f * scale;
        var trunkWidth = 0.125f * scale;
        var clusterRadius = 0.3f * scale;
        var trunkColor = Color.Yellow;
        var clusterColor = Color.Yellow;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            NameColor = trunkColor;

            // Solid Trunk (Box)
            Vector3 trunkMin = new Vector3(-trunkWidth / 2, 0, -trunkWidth / 2);
            Vector3 trunkMax = new Vector3(trunkWidth / 2, trunkHeight, trunkWidth / 2);
            AddSolidBox(trunkMin, trunkMax, trunkColor);

            // Cluster (Solid Sphere on top of trunk)
            Vector3 clusterPos = new Vector3(0, trunkHeight + (clusterRadius * 0.8f), 0);
            AddSolidSphere(clusterPos, clusterRadius, clusterColor);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }

    private void AddSolidBox(Vector3 min, Vector3 max, Color color)
    {
        var tfl = new Vector3(min.X, max.Y, max.Z);
        var tfr = new Vector3(max.X, max.Y, max.Z);
        var bfr = new Vector3(max.X, min.Y, max.Z);
        var bfl = new Vector3(min.X, min.Y, max.Z);
        var tbl = new Vector3(min.X, max.Y, min.Z);
        var tbr = new Vector3(max.X, max.Y, min.Z);
        var bbr = new Vector3(max.X, min.Y, min.Z);
        var bbl = new Vector3(min.X, min.Y, min.Z);

        AddTri(tfl, tbl, tbr, color);
        AddTri(tfl, tbr, tfr, color);
        AddTri(bbl, bfl, bbr, color);
        AddTri(bfl, bfr, bbr, color);
        AddTri(bfl, tfl, tfr, color);
        AddTri(bfl, tfr, bfr, color);
        AddTri(bbr, tbr, tbl, color);
        AddTri(bbr, tbl, bbl, color);
        AddTri(bbl, tbl, tfl, color);
        AddTri(bbl, tfl, bfl, color);
        AddTri(bfr, tfr, tbr, color);
        AddTri(bfr, tbr, bbr, color);
    }

    private void AddSolidSphere(Vector3 position, float radius, Color color, int numVerticalSegments = 11, int numSidesPerSegment = 12)
    {
        var vertices = new Vector3[numVerticalSegments + 2, numSidesPerSegment];

        Vector3 topPoint = Vector3.UnitY * radius + position;
        Vector3 bottomPoint = -Vector3.UnitY * radius + position;

        for (int i = 0; i < numVerticalSegments; i++)
        {
            for (int j = 0; j < numSidesPerSegment; j++)
            {
                float horizontalAngle = j / (float)numSidesPerSegment * MathF.PI * 2.0f;
                float verticalAngle = (i + 1) / (float)(numVerticalSegments + 1) * MathF.PI - MathF.PI / 2;
                float altitude = MathF.Sin(verticalAngle);
                float horizontalDist = MathF.Cos(verticalAngle);

                vertices[i, j] = new Vector3(
                    MathF.Cos(horizontalAngle) * horizontalDist,
                    altitude,
                    MathF.Sin(horizontalAngle) * horizontalDist) * radius + position;
            }
        }

        for (int j = 0; j < numSidesPerSegment; j++)
        {
            int nextJ = (j + 1) % numSidesPerSegment;
            AddTri(bottomPoint, vertices[0, j], vertices[0, nextJ], color);
            AddTri(topPoint, vertices[numVerticalSegments - 1, j], vertices[numVerticalSegments - 1, nextJ], color);
        }

        for (int i = 0; i < numVerticalSegments - 1; i++)
        {
            for (int j = 0; j < numSidesPerSegment; j++)
            {
                int nextJ = (j + 1) % numSidesPerSegment;
                AddTri(vertices[i, j], vertices[i + 1, j], vertices[i, nextJ], color);
                AddTri(vertices[i + 1, j], vertices[i + 1, nextJ], vertices[i, nextJ], color);
            }
        }
    }
}

