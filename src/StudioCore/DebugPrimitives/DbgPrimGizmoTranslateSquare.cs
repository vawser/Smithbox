using StudioCore.MsbEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.DebugPrimitives;

public class DbgPrimGizmoTranslateSquare : DbgPrimGizmo
{
    public enum Axis
    {
        None,
        PosX,
        PosY,
        PosZ
    }

    public const float StartOffset = 2.25f;
    public const float Length = 0.75f;

    private readonly DbgPrimGeometryData GeometryData;

    private readonly List<Vector3> Tris = new();

    public DbgPrimGizmoTranslateSquare(Gizmos.Axis axis)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            Vector3 a, b, c, d;
            switch (axis)
            {
                case Gizmos.Axis.PosX:
                    a = new Vector3(0.0f, StartOffset, StartOffset);
                    b = new Vector3(0.0f, StartOffset + Length, StartOffset);
                    c = new Vector3(0.0f, StartOffset, StartOffset + Length);
                    d = new Vector3(0.0f, StartOffset + Length, StartOffset + Length);
                    break;
                case Gizmos.Axis.PosY:
                    a = new Vector3(StartOffset, 0.0f, StartOffset);
                    b = new Vector3(StartOffset + Length, 0.0f, StartOffset);
                    c = new Vector3(StartOffset, 0.0f, StartOffset + Length);
                    d = new Vector3(StartOffset + Length, 0.0f, StartOffset + Length);
                    break;
                case Gizmos.Axis.PosZ:
                    a = new Vector3(StartOffset, StartOffset, 0.0f);
                    b = new Vector3(StartOffset + Length, StartOffset, 0.0f);
                    c = new Vector3(StartOffset, StartOffset + Length, 0.0f);
                    d = new Vector3(StartOffset + Length, StartOffset + Length, 0.0f);
                    break;
                default:
                    throw new ArgumentException("Select valid axis");
            }

            Color color = Color.FromArgb(0x86, 0xC8, 0x15);
            AddQuad(a, b, d, c, color);
            AddColQuad(Tris, a, b, d, c);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };

            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                UpdatePerFrameResources(d, cl, null);
            });
        }
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
