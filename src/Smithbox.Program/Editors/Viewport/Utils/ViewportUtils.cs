using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Editors.Viewport;

public static class ViewportUtils
{
    public enum RayCastCull
    {
        CullNone,
        CullFront,
        CullBack
    }

    // From Veldrid Neo Demo
    public static Matrix4x4 CreatePerspective(
        GraphicsDevice gd,
        bool useReverseDepth,
        float fov,
        float aspectRatio,
        float near, float far)
    {
        Matrix4x4 persp;
        if (useReverseDepth)
        {
            persp = CreatePerspective(fov, aspectRatio, far, near);
        }
        else
        {
            persp = CreatePerspective(fov, aspectRatio, near, far);
        }

        if (gd.IsClipSpaceYInverted)
        {
            persp *= new Matrix4x4(
                1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        }
        /*persp = new System.Numerics.Matrix4x4(
            -1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1) * persp;*/

        return persp;
    }

    private static Matrix4x4 CreatePerspective(float fov, float aspectRatio, float near, float far)
    {
        if (fov <= 0.0f || fov >= Math.PI)
        {
            throw new ArgumentOutOfRangeException(nameof(fov));
        }

        if (near <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(near));
        }

        if (far <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(far));
        }

        var yScale = 1.0f / (float)Math.Tan((double)fov * 0.5f);
        var xScale = yScale / aspectRatio;

        Matrix4x4 result;

        result.M11 = xScale;
        result.M12 = result.M13 = result.M14 = 0.0f;

        result.M22 = yScale;
        result.M21 = result.M23 = result.M24 = 0.0f;

        result.M31 = result.M32 = 0.0f;
        var negFarRange = float.IsPositiveInfinity(far) ? -1.0f : far / (near - far);
        result.M33 = -negFarRange;
        result.M34 = 1.0f;

        result.M41 = result.M42 = result.M44 = 0.0f;
        result.M43 = near * negFarRange;

        return result;
    }

    public static void ExtractScale(Matrix4x4 mat, out Vector3 scale, out Matrix4x4 post)
    {
        post = mat;
        var sx = new Vector3(post.M11, post.M12, post.M13).Length();
        var sy = new Vector3(post.M21, post.M22, post.M23).Length();
        var sz = new Vector3(post.M31, post.M32, post.M33).Length();
        scale = new Vector3(sx, sy, sz);
        post.M11 /= sx;
        post.M12 /= sx;
        post.M13 /= sx;
        post.M21 /= sy;
        post.M22 /= sy;
        post.M23 /= sy;
        post.M31 /= sz;
        post.M32 /= sz;
        post.M33 /= sz;
    }

    public static bool RayMeshIntersection(Ray ray,
        Vector3[] verts,
        int[] indices,
        RayCastCull cull,
        out float dist)
    {
        var hit = false;
        var mindist = float.MaxValue;

        for (var index = 0; index < indices.Length; index += 3)
        {
            if (cull != RayCastCull.CullNone)
            {
                // Get the face normal
                Vector3 normal = Vector3.Normalize(Vector3.Cross(
                    verts[indices[index + 1]] - verts[indices[index]],
                    verts[indices[index + 2]] - verts[indices[index]]));
                var ratio = Vector3.Dot(ray.Direction, normal);
                if (cull == RayCastCull.CullBack && ratio < 0.0f)
                {
                    continue;
                }

                if (cull == RayCastCull.CullFront && ratio > 0.0f)
                {
                    continue;
                }
            }

            float locdist;
            if (ray.Intersects(ref verts[indices[index]],
                    ref verts[indices[index + 1]],
                    ref verts[indices[index + 2]],
                    out locdist))
            {
                hit = true;
                if (locdist < mindist)
                {
                    mindist = locdist;
                }
            }
        }

        dist = mindist;
        return hit;
    }

    public static bool RayMeshIntersection(Ray ray,
        Span<Vector3> verts,
        Span<int> indices,
        RayCastCull cull,
        out float dist)
    {
        var hit = false;
        var mindist = float.MaxValue;

        for (var index = 0; index < indices.Length; index += 3)
        {
            if (cull != RayCastCull.CullNone)
            {
                // Get the face normal
                Vector3 normal = Vector3.Normalize(Vector3.Cross(
                    verts[indices[index + 1]] - verts[indices[index]],
                    verts[indices[index + 2]] - verts[indices[index]]));
                var ratio = Vector3.Dot(ray.Direction, normal);
                if (cull == RayCastCull.CullBack && ratio < 0.0f)
                {
                    continue;
                }

                if (cull == RayCastCull.CullFront && ratio > 0.0f)
                {
                    continue;
                }
            }

            float locdist;
            if (ray.Intersects(ref verts[indices[index]],
                    ref verts[indices[index + 1]],
                    ref verts[indices[index + 2]],
                    out locdist))
            {
                hit = true;
                if (locdist < mindist)
                {
                    mindist = locdist;
                }
            }
        }

        dist = mindist;
        return hit;
    }

    public static void Swap(ref float a, ref float b)
    {
        var temp = a;
        a = b;
        b = temp;
    }

    public static bool RayBoxIntersection(ref Ray ray, ref BoundingBox box, out float dist)
    {
        var tmin = (box.Min.X - ray.Origin.X) / ray.Direction.X;
        var tmax = (box.Max.X - ray.Origin.X) / ray.Direction.X;

        if (tmin > tmax)
        {
            Swap(ref tmin, ref tmax);
        }

        var tymin = (box.Min.Y - ray.Origin.Y) / ray.Direction.Y;
        var tymax = (box.Max.Y - ray.Origin.Y) / ray.Direction.Y;

        if (tymin > tymax)
        {
            Swap(ref tymin, ref tymax);
        }

        if (tmin > tymax || tymin > tmax)
        {
            dist = float.MaxValue;
            return false;
        }

        if (tymin > tmin)
        {
            tmin = tymin;
        }

        if (tymax < tmax)
        {
            tmax = tymax;
        }

        var tzmin = (box.Min.Z - ray.Origin.Z) / ray.Direction.Z;
        var tzmax = (box.Max.Z - ray.Origin.Z) / ray.Direction.Z;

        if (tzmin > tzmax)
        {
            Swap(ref tzmin, ref tzmax);
        }

        if (tmin > tzmax || tzmin > tmax)
        {
            dist = float.MaxValue;
            return false;
        }

        if (tzmin > tmin)
        {
            tmin = tzmin;
        }

        if (tzmax < tmax)
        {
            tmax = tzmax;
        }

        dist = tmin < 0.0f ? tmax : tmin;
        return true;
    }

    public static bool RaySphereIntersection(ref Ray ray,
        Vector3 pos, float radius, out float dist)
    {
        Vector3 oc = ray.Origin - pos;
        var a = Vector3.Dot(ray.Direction, ray.Direction);
        var b = 2.0f * Vector3.Dot(oc, ray.Direction);
        var c = Vector3.Dot(oc, oc) - (radius * radius);
        var discriminant = (b * b) - (4.0f * a * c);
        if (discriminant < 0)
        {
            dist = float.MaxValue;
            return false;
        }

        dist = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
        if (dist < 0)
        {
            dist = float.MaxValue;
            return false;
        }

        return true;
    }

    public static bool RayPlaneIntersection(Vector3 origin,
        Vector3 direction,
        Vector3 planePoint,
        Vector3 normal,
        out float dist)
    {
        var d = Vector3.Dot(direction, normal);
        if (d == 0)
        {
            dist = float.PositiveInfinity;
            return false;
        }

        dist = Vector3.Dot(planePoint - origin, normal) / d;
        return true;
    }

    public static bool RayCylinderIntersection(Vector3 origin,
        Vector3 direction,
        Vector3 center,
        float height,
        float radius,
        out float dist)
    {
        dist = float.MaxValue;
        Vector3 torigin = origin - center;
        var a = (direction.X * direction.X) + (direction.Z * direction.Z);
        var b = (direction.X * torigin.X) + (direction.Z * torigin.Z);
        var c = (torigin.X * torigin.X) + (torigin.Z * torigin.Z) - (radius * radius);

        var delta = (b * b) - (a * c);

        if (delta < 0.0000001)
        {
            return false;
        }

        dist = (-b - MathF.Sqrt(delta)) / a;
        if (dist < 0.0000001)
        {
            return false;
        }

        var y = torigin.Y + (dist * direction.Y);
        if (y > height || y < 0)
        {
            return false;
        }

        return true;
    }

    public static float[] GetFullScreenQuadVerts(GraphicsDevice gd)
    {
        if (gd.IsClipSpaceYInverted)
        {
            return new float[] { -1, -1, 0, 0, 1, -1, 1, 0, 1, 1, 1, 1, -1, 1, 0, 1 };
        }

        return new float[] { -1, 1, 0, 0, 1, 1, 1, 0, 1, -1, 1, 1, -1, -1, 0, 1 };
    }

}
