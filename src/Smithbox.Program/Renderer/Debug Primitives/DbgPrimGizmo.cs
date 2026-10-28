using System;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class DbgPrimGizmo : DbgPrim
{
    public int TriCount => Indices.Count / 3;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutPositionColorNormal;

    public override VertexLayoutDescription LayoutDescription => VertexPositionColorNormal.Layout;

    public override BoundingBox Bounds => new(new Vector3(-20, -20, -20), new Vector3(20, 20, 20));

    public override BoundingBox FramingBounds
    {
        get
        {
            return new(new Vector3(-20, -20, -20), new Vector3(20, 20, 20));
        }
    }

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

        var vertIndexA = AddOrReuseVertex(vertA);
        var vertIndexB = AddOrReuseVertex(vertB);
        var vertIndexC = AddOrReuseVertex(vertC);

        AddIndex((short)vertIndexC);
        AddIndex((short)vertIndexB);
        AddIndex((short)vertIndexA);
    }

    protected override void DisposeBuffers()
    {
    }
}
