using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public class DbgPrimGizmoScaleCube : DbgPrimGizmo
{
    public const float AxisOffset = 5.0f;
    public const float CubeSize = 0.6f;

    private readonly DbgPrimGeometryData GeometryData;
    private readonly List<Vector3> Tris = new();

    public DbgPrimGizmoScaleCube(GizmoState.Axis axis)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            BuildCube(axis, Color.FromArgb(0x86, 0xC8, 0x15));

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };

            SceneRenderer.AddBackgroundUploadTask((d, cl) =>
            {
                UpdatePerFrameResources(d, cl, null);
            });
        }
    }

    // ------------------------------------------------------


    private void BuildCube(GizmoState.Axis axis, Color color)
    {
        Vector3 center = Vector3.Zero;

        switch (axis)
        {
            case GizmoState.Axis.PosX:
                center = new Vector3(AxisOffset, 0, 0);
                break;

            case GizmoState.Axis.PosY:
                center = new Vector3(0, AxisOffset, 0);
                break;

            case GizmoState.Axis.PosZ:
                center = new Vector3(0, 0, AxisOffset);
                break;
        }

        float h = CubeSize * 0.5f;

        Vector3[] v =
        {
            center + new Vector3(-h,-h,-h),
            center + new Vector3( h,-h,-h),
            center + new Vector3( h, h,-h),
            center + new Vector3(-h, h,-h),

            center + new Vector3(-h,-h, h),
            center + new Vector3( h,-h, h),
            center + new Vector3( h, h, h),
            center + new Vector3(-h, h, h),
        };

        // Faces
        AddFace(v[0], v[1], v[2], v[3], color); // back
        AddFace(v[4], v[5], v[6], v[7], color); // front
        AddFace(v[0], v[1], v[5], v[4], color); // bottom
        AddFace(v[2], v[3], v[7], v[6], color); // top
        AddFace(v[1], v[2], v[6], v[5], color); // right
        AddFace(v[3], v[0], v[4], v[7], color); // left
    }

    // ------------------------------------------------------

    private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
    {
        AddQuad(a, b, c, d, color);
        AddColQuad(Tris, a, b, c, d);
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

    // ------------------------------------------------------

    public bool GetRaycast(Ray ray, Matrix4x4 transform)
    {
        for (var i = 0; i < Tris.Count / 3; i++)
        {
            Vector3 a = Vector3.Transform(Tris[i * 3], transform);
            Vector3 b = Vector3.Transform(Tris[i * 3 + 1], transform);
            Vector3 c = Vector3.Transform(Tris[i * 3 + 2], transform);

            float dist;
            if (ray.Intersects(ref a, ref b, ref c, out dist))
                return true;
        }

        return false;
    }
}
