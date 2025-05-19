using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimSolidSphere : DbgPrimSolid
{
    private DbgPrimGeometryData GeometryData;

    public override BoundingBox Bounds => new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

    private Transform Transform;
    private float Radius;
    private Color Color;
    private int VerticalSegments;
    private int SidesPerSegment;

    public DbgPrimSolidSphere(Transform location, float radius, Color color, int numVerticalSegments = 11, int numSidesPerSegment = 12)
    {
        Transform = location;
        Radius = radius;
        Color = color;
        VerticalSegments = numVerticalSegments;
        SidesPerSegment = numSidesPerSegment;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            NameColor = color;
            var vertices = new Vector3[numVerticalSegments + 2, numSidesPerSegment];

            Vector3 topPoint = Vector3.UnitY * radius;
            Vector3 bottomPoint = -Vector3.UnitY * radius;

            for (int i = 0; i < numVerticalSegments; i++)
            {
                for (int j = 0; j < numSidesPerSegment; j++)
                {
                    float horizontalAngle = j / (float)numSidesPerSegment * Utils.Pi * 2.0f;
                    float verticalAngle = (i + 1) / (float)(numVerticalSegments + 1) * Utils.Pi - Utils.PiOver2;
                    float altitude = (float)Math.Sin(verticalAngle);
                    float horizontalDist = (float)Math.Cos(verticalAngle);

                    vertices[i, j] = new Vector3(
                        (float)Math.Cos(horizontalAngle) * horizontalDist,
                        altitude,
                        (float)Math.Sin(horizontalAngle) * horizontalDist) * radius;
                }
            }

            // Generate faces
            for (int j = 0; j < numSidesPerSegment; j++)
            {
                int nextJ = (j + 1) % numSidesPerSegment;
                // Bottom cap
                AddTri(vertices[0, j], bottomPoint, vertices[0, nextJ], color);
                // Top cap
                AddTri(topPoint, vertices[numVerticalSegments - 1, j], vertices[numVerticalSegments - 1, nextJ], color);
            }

            for (int i = 0; i < numVerticalSegments - 1; i++)
            {
                for (int j = 0; j < numSidesPerSegment; j++)
                {
                    int nextJ = (j + 1) % numSidesPerSegment;

                    // Two triangles forming a quad
                    AddTri(vertices[i, j], vertices[i + 1, j], vertices[i, nextJ], color);
                    AddTri(vertices[i + 1, j], vertices[i + 1, nextJ], vertices[i, nextJ], color);
                }
            }

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}