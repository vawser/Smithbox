using StudioCore.Resource;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.DebugPrimitives;

public class DbgPrimGizmo : DbgPrim
{
    public int TriCount => Indices.Length / 3;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutPositionColorNormal;

    public override VertexLayoutDescription LayoutDescription => VertexPositionColorNormal.Layout;

    public override BoundingBox Bounds => new(new Vector3(-20, -20, -20), new Vector3(20, 20, 20));

    public override string ShaderName => "DebugWire";

    public override SpecializationConstant[] SpecializationConstants => new SpecializationConstant[0];

    public override VkCullModeFlags CullMode => VkCullModeFlags.None;

    public override VkPolygonMode FillMode => VkPolygonMode.Fill;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.TriangleList;

    public override uint VertexSize =>
        MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColorNormal);

    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
    {
        AddTri(a, b, c, color);
        AddTri(a, c, d, color);
    }

    public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        AddTri(a, b, c, color, color, color);
    }

    public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color colorA, Color colorB, Color colorC)
    {
        Vector3 dir = Vector3.Cross(b - a, c - a);
        Vector3 norm = Vector3.Normalize(dir);

        var vertA = new VertexPositionColorNormal(a, colorA, norm);
        var vertB = new VertexPositionColorNormal(b, colorB, norm);
        var vertC = new VertexPositionColorNormal(c, colorC, norm);

        var vertIndexA = Array.IndexOf(Vertices, vertA);
        var vertIndexB = Array.IndexOf(Vertices, vertB);
        var vertIndexC = Array.IndexOf(Vertices, vertC);

        //If vertex A can't be recycled from an old one, make a new one.
        if (vertIndexA == -1)
        {
            AddVertex(vertA);
            vertIndexA = Vertices.Length - 1;
        }

        //If vertex B can't be recycled from an old one, make a new one.
        if (vertIndexB == -1)
        {
            AddVertex(vertB);
            vertIndexB = Vertices.Length - 1;
        }

        //If vertex C can't be recycled from an old one, make a new one.
        if (vertIndexC == -1)
        {
            AddVertex(vertC);
            vertIndexC = Vertices.Length - 1;
        }

        AddIndex((short)vertIndexC);
        AddIndex((short)vertIndexB);
        AddIndex((short)vertIndexA);
    }

    protected override void DisposeBuffers()
    {
    }
}
