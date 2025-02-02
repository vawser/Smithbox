using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimSolidSpotLight : DbgPrimSolid
{
    private readonly DbgPrimGeometryData GeometryData;

    public override BoundingBox Bounds => new BoundingBox(new Vector3(-1, 0, -1), new Vector3(1, 2, 1));

    public DbgPrimSolidSpotLight(Transform location, float radius, float height, float beamAngle, Color color, int numVerticalSegments = 8, int numSidesPerSegment = 8)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            NameColor = color;

            // Create the spotlight base (a sphere representing the light source)
            Vector3 topPoint = Vector3.UnitY * radius;
            Vector3 bottomPoint = -Vector3.UnitY * radius;
            var vertices = new Vector3[numVerticalSegments + 2, numSidesPerSegment];

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

            for (int j = 0; j < numSidesPerSegment; j++)
            {
                int nextJ = (j + 1) % numSidesPerSegment;
                AddTri(vertices[0, j], bottomPoint, vertices[0, nextJ], color);
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

            // Create the spotlight beam (a cone-like cylinder)
            float beamRadius = (float)(Math.Tan(beamAngle / 2) * height);
            var topY = height;
            var bottomY = 0;
            var topCenter = new Vector3(0, topY, 0);
            var bottomCenter = new Vector3(0, bottomY, 0);

            List<Vector3> topVertices = new List<Vector3>();
            List<Vector3> bottomVertices = new List<Vector3>();
            int numSegments = 16;

            for (int i = 0; i < numSegments; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / numSegments);
                float x = beamRadius * (float)Math.Cos(angle);
                float z = beamRadius * (float)Math.Sin(angle);

                topVertices.Add(new Vector3(x, topY, z));
                bottomVertices.Add(new Vector3(x, bottomY, z));
            }

            for (int i = 0; i < numSegments; i++)
            {
                int next = (i + 1) % numSegments;
                AddTri(topCenter, topVertices[next], topVertices[i], color);
                AddTri(bottomCenter, bottomVertices[i], bottomVertices[next], color);
                AddTri(topVertices[i], bottomVertices[i], bottomVertices[next], color);
                AddTri(topVertices[i], bottomVertices[next], topVertices[next], color);
            }

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}
