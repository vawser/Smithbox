//using Microsoft.Xna.Framework;

using ImGuiNET;
using Microsoft.Win32;
using SoulsFormats;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore;

public static class Utils
{
    public enum RayCastCull
    {
        CullNone,
        CullFront,
        CullBack
    }

    public const float Pi = (float)Math.PI;
    public const float PiOver2 = (float)Math.PI / 2.0f;
    public const float Rad2Deg = 180.0f / Pi;
    public const float Deg2Rad = Pi / 180.0f;

    private static readonly char[] _dirSep = { '\\', '/' };

    public static float DegToRadians(float deg)
    {
        return deg * Pi / 180.0f;
    }

    public static float RadiansToDeg(float rad)
    {
        return rad * 180.0f / Pi;
    }

    public static float Clamp(float f, float min, float max)
    {
        if (f < min)
        {
            return min;
        }

        if (f > max)
        {
            return max;
        }

        return f;
    }

    public static float Lerp(float a, float b, float d)
    {
        return (a * (1.0f - d)) + (b * d);
    }

    public static bool IsPowerTwo(uint a)
    {
        if (a > 0)
        {
            while (a % 2 == 0)
            {
                a >>= 1;
            }

            if (a == 1)
            {
                return true;
            }
        }

        return false;
    }

    public static Matrix4x4 Inverse(this Matrix4x4 src)
    {
        Matrix4x4.Invert(src, out Matrix4x4 result);
        return result;
    }


    // Vector rotation functions from John Alexiou at https://stackoverflow.com/questions/69245724/rotate-a-vector-around-an-axis-in-3d-space
    /// <summary>
    ///     Rotates a vector using the Rodriguez rotation formula
    ///     about an arbitrary axis.
    /// </summary>
    public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float angle)
    {
        Vector3 vxp = Vector3.Cross(axis, vector);
        Vector3 vxvxp = Vector3.Cross(axis, vxp);
        return vector + ((float)Math.Sin(angle) * vxp) + ((1 - (float)Math.Cos(angle)) * vxvxp);
    }

    /// <summary>
    ///     Rotates a vector about a point in space.
    /// </summary>
    /// <returns>The rotated vector</returns>
    public static Vector3 RotateVectorAboutPoint(Vector3 vector, Vector3 pivot, Vector3 axis, float angle)
    {
        return pivot + RotateVector(vector - pivot, axis, angle);
    }


    private static double GetColorComponent(double temp1, double temp2, double temp3)
    {
        double num;
        temp3 = MoveIntoRange(temp3);
        if (temp3 < 0.166666666666667)
        {
            num = temp1 + ((temp2 - temp1) * 6 * temp3);
        }
        else if (temp3 >= 0.5)
        {
            num = temp3 >= 0.666666666666667 ? temp1 : temp1 + ((temp2 - temp1) * (0.666666666666667 - temp3) * 6);
        }
        else
        {
            num = temp2;
        }

        return num;
    }

    private static double GetTemp2(float H, float S, float L)
    {
        double temp2;
        temp2 = L >= 0.5 ? L + S - (L * S) : L * (1 + (double)S);
        return temp2;
    }

    /// <summary>
    ///     Derived from https://stackoverflow.com/a/1626232
    /// </summary>
    public static Vector3 ColorToHSV(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        var hue = color.GetHue();
        var saturation = max == 0 ? 0 : 1.0f - (1.0f * min / max);
        var value = max / 255.0f;

        return new Vector3(hue, saturation, value);
    }

    /// <summary>
    ///     Derived from https://stackoverflow.com/a/1626232
    /// </summary>
    public static Color ColorFromHSV(Vector3 hsv)
    {
        var hue = hsv.X;
        var saturation = hsv.Y;
        var value = hsv.Z;

        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = (hue / 60) - (float)Math.Floor(hue / 60);

        value *= 255.0f;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - (f * saturation)));
        var t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

        if (hi == 0)
        {
            return Color.FromArgb(255, v, t, p);
        }

        if (hi == 1)
        {
            return Color.FromArgb(255, q, v, p);
        }

        if (hi == 2)
        {
            return Color.FromArgb(255, p, v, t);
        }

        if (hi == 3)
        {
            return Color.FromArgb(255, p, q, v);
        }

        if (hi == 4)
        {
            return Color.FromArgb(255, t, p, v);
        }

        return Color.FromArgb(255, v, p, q);
    }

    private static double MoveIntoRange(double temp3)
    {
        if (temp3 < 0)
        {
            temp3 += 1;
        }
        else if (temp3 > 1)
        {
            temp3 -= 1;
        }

        return temp3;
    }

    public static string Frankenpath(params string[] pathParts)
    {
        StringBuilder sb = new();

        for (var i = 0; i < pathParts.Length; i++)
        {
            sb.Append(pathParts[i].Trim('\\'));
            if (i < pathParts.Length - 1)
            {
                sb.Append('\\');
            }
        }

        return sb.ToString();
    }

    public static string GetShortIngameFileName(string fileName)
    {
        return GetFileNameWithoutAnyExtensions(GetFileNameWithoutDirectoryOrExtension(fileName));
    }

    public static string GetFileNameWithoutDirectoryOrExtension(string fileName)
    {
        if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
        {
            fileName = fileName.TrimEnd(_dirSep);
        }

        if (fileName.Contains("\\") || fileName.Contains("/"))
        {
            fileName = fileName.Substring(fileName.LastIndexOfAny(_dirSep) + 1);
        }

        if (fileName.Contains("."))
        {
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
        }

        return fileName;
    }

    public static string GetFileNameWithoutAnyExtensions(string fileName)
    {
        var dirSepIndex = fileName.LastIndexOfAny(_dirSep);
        if (dirSepIndex >= 0)
        {
            var dotIndex = -1;
            var doContinue = true;
            do
            {
                dotIndex = fileName.LastIndexOf('.');
                doContinue = dotIndex > dirSepIndex;
                if (doContinue)
                {
                    fileName = fileName.Substring(0, dotIndex);
                }
            } while (doContinue);
        }
        else
        {
            var dotIndex = -1;
            var doContinue = true;
            do
            {
                dotIndex = fileName.LastIndexOf('.');
                doContinue = dotIndex >= 0;
                if (doContinue)
                {
                    fileName = fileName.Substring(0, dotIndex);
                }
            } while (doContinue);
        }

        return fileName;
    }

    public static string GetLocalAssetPath(AssetLocator assetLocator, string assetPath)
    {
        if (assetPath.StartsWith(assetLocator.GameModDirectory))
        {
            return assetPath.Replace(assetLocator.GameModDirectory, "");
        }

        if (assetPath.StartsWith(assetLocator.GameRootDirectory))
        {
            return assetPath.Replace(assetLocator.GameRootDirectory, "");
        }

        throw new DirectoryNotFoundException(
            $"Asset path did not start with game or project directory: {assetPath}");
    }

    public static void WriteWithBackup<T>(AssetLocator assetLocator, string assetPath, T item,
        params object[] writeparms) where T : SoulsFile<T>, new()
    {
        WriteWithBackup(assetLocator.GameRootDirectory, assetLocator.GameModDirectory, assetPath, item,
            assetLocator.Type, writeparms);
    }

    public static void WriteWithBackup<T>(string gamedir, string moddir, string assetpath, T item,
        GameType gameType = GameType.Undefined, params object[] writeparms) where T : SoulsFile<T>, new()
    {
        var assetgamepath = $@"{gamedir}\{assetpath}";
        var assetmodpath = $@"{moddir}\{assetpath}";

        try
        {
            if (moddir != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(assetmodpath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(assetmodpath));
                }
            }

            // Make a backup of the original file if a mod path doesn't exist
            if (moddir == null && !File.Exists($@"{assetgamepath}.bak") && File.Exists(assetgamepath))
            {
                File.Copy(assetgamepath, $@"{assetgamepath}.bak", true);
            }

            var writepath = moddir == null ? assetgamepath : assetmodpath;

            if (File.Exists(writepath + ".temp"))
            {
                File.Delete(writepath + ".temp");
            }

            if (gameType == GameType.DarkSoulsIII && item is BND4 bndDS3)
            {
                SFUtil.EncryptDS3Regulation(writepath + ".temp", bndDS3);
            }
            else if (gameType == GameType.EldenRing && item is BND4 bndER)
            {
                SFUtil.EncryptERRegulation(writepath + ".temp", bndER);
            }
            else if (gameType == GameType.ArmoredCoreVI && item is BND4 bndAC6)
            {
                SFUtil.EncryptAC6Regulation(writepath + ".temp", bndAC6);
            }
            else if (item is BXF3 or BXF4)
            {
                var bhdPath = $@"{moddir}\{(string)writeparms[0]}";
                if (item is BXF3 bxf3)
                {
                    bxf3.Write(bhdPath + ".temp", writepath + ".temp");

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf3.Dispose();
                }
                else if (item is BXF4 bxf4)
                {
                    bxf4.Write(bhdPath + ".temp", writepath + ".temp");

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf4.Dispose();
                }

                if (File.Exists(writepath))
                {
                    File.Copy(writepath, writepath + ".prev", true);
                    File.Delete(writepath);
                }

                if (File.Exists(bhdPath))
                {
                    File.Copy(bhdPath, bhdPath + ".prev", true);
                    File.Delete(bhdPath);
                }

                File.Move(writepath + ".temp", writepath);
                File.Move(bhdPath + ".temp", bhdPath);

                return;
            }
            else
            {
                item.Write(writepath + ".temp");
            }

            // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
            if (item is IDisposable d)
            {
                d.Dispose();
            }

            if (File.Exists(writepath))
            {
                File.Copy(writepath, writepath + ".prev", true);
                File.Delete(writepath);
            }

            File.Move(writepath + ".temp", writepath);
        }
        catch (Exception e)
        {
            throw new SavingFailedException(Path.GetFileName(assetmodpath), e);
        }
    }

    public static void WriteStringWithBackup(string gamedir, string moddir, string assetpath, string item)
    {
        var assetgamepath = $@"{gamedir}\{assetpath}";
        var assetmodpath = $@"{moddir}\{assetpath}";

        if (moddir != null)
        {
            if (!Directory.Exists(Path.GetDirectoryName(assetmodpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetmodpath));
            }
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (moddir == null && !File.Exists($@"{assetgamepath}.bak") && File.Exists(assetgamepath))
        {
            File.Copy(assetgamepath, $@"{assetgamepath}.bak", true);
        }

        var writepath = moddir == null ? assetgamepath : assetmodpath;

        if (File.Exists(writepath + ".temp"))
        {
            File.Delete(writepath + ".temp");
        }

        File.WriteAllText(writepath + ".temp", item);

        if (File.Exists(writepath))
        {
            File.Copy(writepath, writepath + ".prev", true);
            File.Delete(writepath);
        }

        File.Move(writepath + ".temp", writepath);
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

    public static Matrix4x4 GetBoneObjectMatrix(FLVER.Bone bone, List<FLVER.Bone> bones)
    {
        Matrix4x4 res = Matrix4x4.Identity;
        FLVER.Bone parentBone = bone;
        do
        {
            res *= parentBone.ComputeLocalTransform();
            if (parentBone.ParentIndex >= 0)
            {
                parentBone = bones[parentBone.ParentIndex];
            }
            else
            {
                parentBone = null;
            }
        } while (parentBone != null);

        return res;
    }

    public static void setRegistry(string name, string value)
    {
        RegistryKey rkey = Registry.CurrentUser.CreateSubKey(@"Software\DSMapStudio");
        rkey.SetValue(name, value);
    }

    public static string readRegistry(string name)
    {
        RegistryKey rkey = Registry.CurrentUser.CreateSubKey(@"Software\DSMapStudio");
        var v = rkey.GetValue(name);
        return v == null ? null : v.ToString();
    }

    /// <summary>
    ///     Replace # with fullwidth # to prevent ImGui from hiding text when detecting ## and ###.
    ///     Optionally replaces %, which is only an issue with certain imgui elements.
    /// </summary>
    public static string ImGuiEscape(string str, string nullStr = "", bool percent = false)
    {
        if (str == null)
        {
            return nullStr;
        }

        str = str.Replace("#", "\xFF03"); // FF03 is eastern block #

        if (percent)
        {
            str = str.Replace("%", "%%");
        }

        return str;
    }

    public static bool EnumEditor(Array enumVals, string[] enumNames, object oldval, out object val, int[] intVals)
    {
        val = null;

        for (var i = 0; i < enumNames.Length; i++)
        {
            enumNames[i] = $"{intVals[i]}: {enumNames[i]}";
        }

        int index = Array.IndexOf(enumVals, oldval);

        if (ImGui.Combo("##", ref index, enumNames, enumNames.Length))
        {
            val = enumVals.GetValue(index);
            return true;
        }

        return false;
    }

    public static void ImGuiGenericHelpPopup(string buttonText, string imguiID, string displayText)
    {
        if (ImGui.Button(buttonText + "##" + imguiID))
        {
            ImGui.OpenPopup(imguiID);
        }

        if (ImGui.BeginPopup(imguiID))
        {
            ImGui.Text(displayText);
            ImGui.EndPopup();
        }
    }

    public static void ImGui_InputUint(string text, ref uint val)
    {
        var strval = $@"{val}";
        if (ImGui.InputText(text, ref strval, 16))
        {
            var res = uint.TryParse(strval, out var refval);
            if (res)
            {
                val = refval;
            }
        }
    }

    /// <summary>
    ///     Inserts new lines into a string to make it fit in the specified UI width.
    /// </summary>
    public static string ImGui_WordWrapString(string text, float uiWidth, int maxLines = 3)
    {
        var textWidth = ImGui.CalcTextSize(text).X;

        // Determine how many line breaks are needed
        var rowNum = float.Ceiling(textWidth / uiWidth);
        if (rowNum > maxLines)
        {
            rowNum = maxLines;
        }

        // Insert line breaks into text
        for (float iRow = 1; iRow < rowNum; iRow++)
        {
            var pos_default = (int)(text.Length * (iRow / rowNum));
            int pos_final;
            var iPos = 0;
            var sign = 1;
            while (true)
            {
                // Find position in string to insert new line without interrupting any words
                pos_final = pos_default + (iPos * sign);
                if (pos_final <= pos_default * 0.7f || pos_final >= pos_default * 1.3f)
                {
                    // Couldn't find empty position within limited range, insert at fractional position instead.
                    text = text.Insert(pos_default, "-\n ");
                    break;
                }

                if (text[pos_final] is ' ' or '-')
                {
                    text = text.Insert(pos_final, "\n");
                    break;
                }

                sign *= -1;
                if (sign == -1)
                {
                    iPos++;
                }
            }
        }

        return text;
    }

    /// <summary>
    ///     Generates display format for ImGui float input.
    ///     Made to display trailing zeroes even if value is an integer,
    ///     and limit number of decimals to appropriate values.
    /// </summary>
    public static string ImGui_InputFloatFormat(float f)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, 3, 6)}f";
    }

    /// <summary>
    ///     Returns string representing version of param or regulation.
    /// </summary>
    public static string ParseParamVersion(ulong version)
    {
        string verStr = version.ToString();
        if (verStr.Length == 7 || verStr.Length == 8)
        {
            char major = verStr[0];
            string minor = verStr[1..3];
            char patch = verStr[3];
            string rev = verStr[4..];
            return $"{major}.{minor}.{patch}.{rev}";
        }

        return "Unknown version format";
    }

    public static void EntitySelectionHandler(Selection selection, Entity entity,
        bool itemSelected, bool isItemFocused, List<WeakReference<Entity>> filteredEntityList = null)
    {
        // Up/Down arrow mass selection
        var arrowKeySelect = false;
        if (isItemFocused && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
        {
            itemSelected = true;
            arrowKeySelect = true;
        }

        if (itemSelected)
        {
            if (arrowKeySelect)
            {
                if (InputTracker.GetKey(Key.ControlLeft)
                    || InputTracker.GetKey(Key.ControlRight)
                    || InputTracker.GetKey(Key.ShiftLeft)
                    || InputTracker.GetKey(Key.ShiftRight))
                {
                    selection.AddSelection(entity);
                }
                else
                {
                    selection.ClearSelection();
                    selection.AddSelection(entity);
                }
            }
            else if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Toggle Selection
                if (selection.GetSelection().Contains(entity))
                {
                    selection.RemoveSelection(entity);
                }
                else
                {
                    selection.AddSelection(entity);
                }
            }
            else if (selection.GetSelection().Count > 0
                     && (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight)))
            {
                // Select Range
                List<Entity> entList;
                if (filteredEntityList != null)
                {
                    entList = new();
                    foreach (WeakReference<Entity> ent in filteredEntityList)
                    {
                        if (ent.TryGetTarget(out Entity e))
                        {
                            entList.Add(e);
                        }
                    }
                }
                else
                {
                    entList = entity.Container.Objects;
                }

                var i1 = entList.IndexOf(selection.GetFilteredSelection<MapEntity>()
                    .FirstOrDefault(fe => fe.Container == entity.Container && fe != entity.Container.RootObject));
                var i2 = entList.IndexOf((MapEntity)entity);

                if (i1 != -1 && i2 != -1)
                {
                    var iStart = i1;
                    var iEnd = i2;
                    if (i2 < i1)
                    {
                        iStart = i2;
                        iEnd = i1;
                    }

                    for (var i = iStart; i <= iEnd; i++)
                    {
                        selection.AddSelection(entList[i]);
                    }
                }
                else
                {
                    selection.AddSelection(entity);
                }
            }
            else
            {
                // Exclusive Selection
                selection.ClearSelection();
                selection.AddSelection(entity);
            }
        }
    }

    public static int ParseHexFromString(string str)
    {
        return int.Parse(str.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
    }

    public static string ParseRegulationVersion(ulong version)
    {
        string verStr = version.ToString();
        if (verStr.Length != 8)
        {
            return "Unknown Version";
        }
        char major = verStr[0];
        string minor = verStr[1..3];
        char patch = verStr[3];
        string rev = verStr[4..];

        return $"{major}.{minor}.{patch}.{rev}";
    }

    public static Vector3 GetDecimalColor(Color color)
    {
        float r = Convert.ToSingle(color.R);
        float g = Convert.ToSingle(color.G);
        float b = Convert.ToSingle(color.B);
        Vector3 vec = new Vector3((r / 255), (g / 255), (b / 255));

        //throw new NotImplementedException($"{vec}");

        return vec;
    }
}
