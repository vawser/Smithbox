using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public enum MeshLayoutType
{
    LayoutMinimal,
    LayoutSky,
    LayoutStandard,
    LayoutUV2,
    LayoutUV3,
    LayoutUV4,
    LayoutDeform,
    LayoutCollision,
    LayoutNavmesh,
    LayoutPositionColorNormal,
    LayoutPositionColor
}

public static class MeshLayoutUtils
{
    public static VertexLayoutDescription GetLayoutDescription(MeshLayoutType type)
    {
        switch (type)
        {
            case MeshLayoutType.LayoutMinimal:
                return FlverLayoutMinimal.Layout;
            case MeshLayoutType.LayoutSky:
                return FlverLayoutSky.Layout;
            case MeshLayoutType.LayoutStandard:
                return FlverLayout.Layout;
            case MeshLayoutType.LayoutUV2:
                return FlverLayoutUV2.Layout;
            case MeshLayoutType.LayoutUV3:
                return FlverLayoutUV3.Layout;
            case MeshLayoutType.LayoutUV4:
                return FlverLayoutUV4.Layout;
            case MeshLayoutType.LayoutDeform:
                return FlverLayoutDeform.Layout;
            case MeshLayoutType.LayoutCollision:
                return CollisionLayout.Layout;
            case MeshLayoutType.LayoutNavmesh:
                return NavmeshLayout.Layout;
            case MeshLayoutType.LayoutPositionColor:
                return PositionColor.Layout;
            case MeshLayoutType.LayoutPositionColorNormal:
                return VertexPositionColorNormal.Layout;
            default:
                throw new ArgumentException(LOC.Get("REND_Mesh_Layout_Invalid_Type"));
        }
    }

    public static unsafe uint GetLayoutVertexSize(MeshLayoutType type)
    {
        switch (type)
        {
            case MeshLayoutType.LayoutMinimal:
                return (uint)sizeof(FlverLayoutMinimal);
            case MeshLayoutType.LayoutSky:
                return (uint)sizeof(FlverLayoutSky);
            case MeshLayoutType.LayoutStandard:
                return (uint)sizeof(FlverLayout);
            case MeshLayoutType.LayoutUV2:
                return (uint)sizeof(FlverLayoutUV2);
            case MeshLayoutType.LayoutUV3:
                return (uint)sizeof(FlverLayoutUV3);
            case MeshLayoutType.LayoutUV4:
                return (uint)sizeof(FlverLayoutUV4);
            case MeshLayoutType.LayoutDeform:
                return (uint)sizeof(FlverLayoutDeform);
            case MeshLayoutType.LayoutCollision:
                return (uint)sizeof(CollisionLayout);
            case MeshLayoutType.LayoutNavmesh:
                return (uint)sizeof(NavmeshLayout);
            case MeshLayoutType.LayoutPositionColorNormal:
                return (uint)sizeof(VertexPositionColorNormal);
            case MeshLayoutType.LayoutPositionColor:
                return (uint)sizeof(PositionColor);
            default:
                throw new ArgumentException(LOC.Get("REND_Mesh_Layout_Invalid_Type"));
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutMinimal
{
    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed byte Color[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutSky
{
    public Vector3 Position;
    public fixed sbyte Normal[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayout
{
    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed sbyte Binormal[4];
    public fixed sbyte Bitangent[4];
    public fixed byte Color[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("binormal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("bitangent", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutUV2
{
    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed sbyte Binormal[4];
    public fixed sbyte Bitangent[4];
    public fixed byte Color[4];
    public fixed short Uv2[2];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("binormal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("bitangent", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("uv2", VkFormat.R16G16Sint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutUV3
{
    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed sbyte Binormal[4];
    public fixed sbyte Bitangent[4];
    public fixed byte Color[4];
    public fixed short Uv2[2];
    public fixed short Uv3[2];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("binormal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("bitangent", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("uv2", VkFormat.R16G16Sint),
        new VertexElementDescription("uv3", VkFormat.R16G16Sint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutUV4
{
    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed sbyte Binormal[4];
    public fixed sbyte Bitangent[4];
    public fixed byte Color[4];
    public fixed short Uv2[2];
    public fixed short Uv3[2];
    public fixed short Uv4[2];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("binormal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("bitangent", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("uv2", VkFormat.R16G16Sint),
        new VertexElementDescription("uv3", VkFormat.R16G16Sint),
        new VertexElementDescription("uv4", VkFormat.R16G16Sint));
}

/// <summary>
///     <see cref="FlverLayoutUV2" /> plus the TEXCOORD1-3 byte attributes, which ER era shaders use for blend
///     weights and vertex animation. Only materials that need them use this layout. It starts with the same
///     fields as <see cref="FlverLayoutUV2" />, so the two can share fill code.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FlverLayoutDeform
{
    public const int TexCoordSize = 4;
    public const int TexCoordCount = 3;

    public Vector3 Position;
    public fixed short Uv1[2];
    public fixed sbyte Normal[4];
    public fixed sbyte Binormal[4];
    public fixed sbyte Bitangent[4];
    public fixed byte Color[4];
    public fixed short Uv2[2];
    public fixed byte TexCoords[TexCoordSize * TexCoordCount];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("uv1", VkFormat.R16G16Sint),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("binormal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("bitangent", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("uv2", VkFormat.R16G16Sint),
        new VertexElementDescription("texcoord1", VkFormat.R8G8B8A8Unorm),
        new VertexElementDescription("texcoord2", VkFormat.R8G8B8A8Unorm),
        new VertexElementDescription("texcoord3", VkFormat.R8G8B8A8Unorm));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CollisionLayout
{
    public const uint SizeInBytes = 24;
    public Vector3 Position;
    public fixed sbyte Normal[4];
    public fixed byte Color[4];
    public fixed byte Barycentric[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("barycentric", VkFormat.R8G8B8A8Uint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct NavmeshLayout
{
    public const uint SizeInBytes = 24;
    public Vector3 Position;
    public fixed sbyte Normal[4];
    public fixed byte Color[4];
    public fixed byte Barycentric[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("normal", VkFormat.R8G8B8A8Sint),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("barycentric", VkFormat.R8G8B8A8Uint));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct PositionColor
{
    public Vector3 Position;
    public fixed byte Color[4];

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint));
}

//[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorNormal
{
    private struct _color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    public Vector3 Position;
    private _color _Color;
    public Vector3 Normal;

    public Color Color
    {
        set
        {
            _Color.r = value.R;
            _Color.g = value.G;
            _Color.b = value.B;
            _Color.a = value.A;
        }
    }

    public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
    {
        Position = position;
        _Color = new _color();
        _Color.r = color.R;
        _Color.g = color.G;
        _Color.b = color.B;
        _Color.a = color.A;
        Normal = normal;
    }

    public static VertexLayoutDescription Layout = new(
        new VertexElementDescription("position", VkFormat.R32G32B32Sfloat),
        new VertexElementDescription("color", VkFormat.R8G8B8A8Uint),
        new VertexElementDescription("normal", VkFormat.R32G32B32Sfloat));

    public bool Equals(VertexPositionColorNormal other)
    {
        return Position.Equals(other.Position)
            && Normal.Equals(other.Normal)
            && _Color.r == other._Color.r
            && _Color.g == other._Color.g
            && _Color.b == other._Color.b
            && _Color.a == other._Color.a;
    }

    public override bool Equals(object obj)
    {
        return obj is VertexPositionColorNormal other && Equals(other);
    }

    public override int GetHashCode()
    {
        var packedColor = _Color.r | (_Color.g << 8) | (_Color.b << 16) | (_Color.a << 24);
        return HashCode.Combine(Position, Normal, packedColor);
    }
}
