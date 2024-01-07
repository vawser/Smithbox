using StudioCore.MsbEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.DebugPrimitives;

public class DbgPrimGizmoRotateRing : DbgPrimGizmo
{
    public enum Axis
    {
        None,
        PosX,
        PosY,
        PosZ
    }

    public const int Segments = 12;
    public const float TotalLength = 5.0f;
    public const float TipLength = 1.25f;
    public const float TipRadius = 0.25f;
    public const float StemRadius = 0.075f;

    public const float Radius = 3.0f;
    public const float RingRadius = 0.06f;
    public const int RingCount = 30;
    public const int SideCount = 4;


    private readonly DbgPrimGeometryData GeometryData;

    private readonly List<Vector3> Tris = new();

    public DbgPrimGizmoRotateRing(Gizmos.Axis axis)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            Vector3 GetPoint(Gizmos.Axis axis, int segmentIndex, float radius, float depth)
            {
                var horizontalAngle = 1.0f * segmentIndex / Segments * Utils.Pi * 2.0f;
                var x = (float)Math.Cos(horizontalAngle);
                var y = (float)Math.Sin(horizontalAngle);
                if (axis == Gizmos.Axis.PosZ)
                {
                    return new Vector3(x * radius, y * radius, depth);
                }

                if (axis == Gizmos.Axis.PosX)
                {
                    return new Vector3(depth, x * radius, y * radius);
                }

                return new Vector3(x * radius, depth, y * radius);
            }

            // Algorithm from
            // http://apparat-engine.blogspot.com/2013/04/procedural-meshes-torus.html
            void Ring(Gizmos.Axis axis, Color color)
            {
                List<Vector3> collist = Tris;

                List<Vector3> vertices = new((RingCount + 1) * (SideCount + 1));
                List<Vector3> colvertices = new((RingCount + 1) * (SideCount + 1));
                var theta = 0.0f;
                var phi = 0.0f;
                var verticalStride = (float)(Math.PI * 2.0) / RingCount;
                var horizontalStride = (float)(Math.PI * 2.0f) / SideCount;
                for (var i = 0; i < RingCount + 1; i++)
                {
                    theta = verticalStride * i;
                    for (var j = 0; j < SideCount + 1; j++)
                    {
                        phi = horizontalStride * j;
                        var cosphi = (float)Math.Cos(phi);
                        var sinphi = (float)Math.Sin(phi);
                        var costheta = (float)Math.Cos(theta);
                        var sintheta = (float)Math.Sin(theta);
                        var a = costheta * (Radius + (RingRadius * cosphi));
                        var b = sintheta * (Radius + (RingRadius * cosphi));
                        var c = RingRadius * sinphi;
                        var ca = costheta * (Radius + (RingRadius * 7.0f * cosphi));
                        var cb = sintheta * (Radius + (RingRadius * 7.0f * cosphi));
                        var cc = RingRadius * 7.0f * sinphi;

                        if (axis == Gizmos.Axis.PosX)
                        {
                            vertices.Add(new Vector3(c, a, b));
                            colvertices.Add(new Vector3(cc, ca, cb));
                        }

                        if (axis == Gizmos.Axis.PosY)
                        {
                            vertices.Add(new Vector3(a, c, b));
                            colvertices.Add(new Vector3(ca, cc, cb));
                        }

                        if (axis == Gizmos.Axis.PosZ)
                        {
                            vertices.Add(new Vector3(a, b, c));
                            colvertices.Add(new Vector3(ca, cb, cc));
                        }
                    }
                }

                for (var i = 0; i < RingCount; i++)
                {
                    for (var j = 0; j < SideCount; j++)
                    {
                        var a = j + (i * (SideCount + 1));
                        var b = j + 1 + (i * (SideCount + 1));
                        var c = j + ((i + 1) * (SideCount + 1));
                        var d = j + 1 + ((i + 1) * (SideCount + 1));
                        //AddQuad(vertices[a], vertices[b], vertices[c], vertices[d], color);
                        AddTri(vertices[a], vertices[b], vertices[c], color);
                        AddTri(vertices[b], vertices[d], vertices[c], color);
                        AddColTri(collist, colvertices[a], colvertices[b], colvertices[c]);
                        AddColTri(collist, colvertices[b], colvertices[d], colvertices[c]);
                    }
                }
            }

            Ring(axis, Color.FromArgb(0x86, 0xC8, 0x15));

            //FinalizeBuffers(true);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }

    private void AddColTri(List<Vector3> list, Vector3 a, Vector3 b, Vector3 c)
    {
        list.Add(a);
        list.Add(b);
        list.Add(c);
    }

    private void AddColQuad(List<Vector3> list, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        AddColTri(list, a, b, c);
        AddColTri(list, a, c, d);
    }

    public bool GetRaycast(Ray ray, Matrix4x4 transform)
    {
        for (var i = 0; i < Tris.Count() / 3; i++)
        {
            Vector3 a = Vector3.Transform(Tris[i * 3], transform);
            Vector3 b = Vector3.Transform(Tris[(i * 3) + 1], transform);
            Vector3 c = Vector3.Transform(Tris[(i * 3) + 2], transform);
            float dist;
            if (ray.Intersects(ref a, ref b, ref c, out dist))
            {
                return true;
            }
        }

        return false;
    }
}
