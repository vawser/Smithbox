using StudioCore.Resource;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWire : DbgPrim
{
    public int LineCount => Indices.Length / 2;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutPositionColorNormal;

    public override VertexLayoutDescription LayoutDescription => VertexPositionColorNormal.Layout;

    public override BoundingBox Bounds => new(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

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
        //int startIndex = Array.IndexOf(Vertices, startVert);
        //int endIndex = Array.IndexOf(Vertices, endVert);
        var startIndex = -1;
        var endIndex = -1;

        //If start vertex can't be recycled from an old one, make a new one.
        if (startIndex == -1)
        {
            AddVertex(startVert);
            startIndex = Vertices.Length - 1;
        }

        //If end vertex can't be recycled from an old one, make a new one.
        if (endIndex == -1)
        {
            AddVertex(endVert);
            endIndex = Vertices.Length - 1;
        }

        for (var i = 0; i < Indices.Length; i += 2)
        {
            int lineStart = Indices[i];
            if (i + 1 < Indices.Length)
            {
                int lineEnd = Indices[i + 1];

                if (lineStart == startIndex && lineEnd == endIndex)
                {
                    // Line literally already exists lmao
                    return;
                }
            }
        }

        AddIndex((short)startIndex);
        AddIndex((short)endIndex);
    }

    protected override void DisposeBuffers()
    {
        //VertBuffer?.Dispose();
    }
}
