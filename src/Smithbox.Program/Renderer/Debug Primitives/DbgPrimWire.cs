using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class DbgPrimWire : DbgPrim
{
    public int LineCount => Indices.Count / 2;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutPositionColorNormal;

    public override VertexLayoutDescription LayoutDescription => VertexPositionColorNormal.Layout;

    public override BoundingBox Bounds => new(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

    public override BoundingBox FramingBounds => new(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

    public override string ShaderName => "DebugWire";

    public override SpecializationConstant[] SpecializationConstants => new SpecializationConstant[0];

    public override VkCullModeFlags CullMode => VkCullModeFlags.None;

    public override VkPolygonMode FillMode => VkPolygonMode.Line;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.LineList;

    public override uint VertexSize =>
        MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColorNormal);

    public void AddLine(Vector3 start, Vector3 end)
    {
        AddLine(start, end, Color.White);
    }

    public void AddLine(Vector3 start, Vector3 end, Color color)
    {
        AddLine(start, end, color, color);
    }

    public void AddLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
    {
        var startVert = new VertexPositionColorNormal(start, startColor, Vector3.Zero);
        var endVert = new VertexPositionColorNormal(end, endColor, Vector3.Zero);
        // Wire vertices are never shared, so each line gets its own pair.
        AddVertex(startVert);
        AddVertex(endVert);
        var startIndex = Vertices.Count - 2;
        var endIndex = Vertices.Count - 1;

        AddIndex((short)startIndex);
        AddIndex((short)endIndex);
    }

    protected override void DisposeBuffers()
    {
        //VertBuffer?.Dispose();
    }
}
