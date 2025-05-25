using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimSolidCylinder : DbgPrimSolid
{
    private readonly DbgPrimGeometryData GeometryData;

    public override BoundingBox Bounds => new(new Vector3(-1, 0, -1), new Vector3(1, 1, 1));

    public DbgPrimSolidCylinder(Transform location, float radius, float height, int numSegments, Color color)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            NameColor = color;

            var topY = height;
            var bottomY = 0;

            var topCenter = new Vector3(0, topY, 0);
            var bottomCenter = new Vector3(0, bottomY, 0);

            List<Vector3> topVertices = new List<Vector3>();
            List<Vector3> bottomVertices = new List<Vector3>();

            for (int i = 0; i < numSegments; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / numSegments);
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);

                topVertices.Add(new Vector3(x, topY, z));
                bottomVertices.Add(new Vector3(x, bottomY, z));
            }

            // Top and Bottom Faces (Triangle Fans)
            for (int i = 0; i < numSegments; i++)
            {
                int next = (i + 1) % numSegments;

                // Top cap
                AddTri(topCenter, topVertices[next], topVertices[i], color);

                // Bottom cap
                AddTri(bottomCenter, bottomVertices[i], bottomVertices[next], color);
            }

            // Side Faces (Quads -> Two Triangles)
            for (int i = 0; i < numSegments; i++)
            {
                int next = (i + 1) % numSegments;

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