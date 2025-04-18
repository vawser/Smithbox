using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.DebugPrimitives;

public class DbgPrimSkybox : DbgPrim
{
    //protected override PrimitiveType PrimType => PrimitiveType.TriangleList;

    private static readonly float Radius = 100;

    public DbgPrimSkybox()
    {
        Category = DbgPrimCategory.Skybox;

        Vector3 min = -Vector3.One * Radius;
        Vector3 max = Vector3.One * Radius;

        // 3 Letters of below names: 
        // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight
        var tfl = new Vector3(min.X, max.Y, max.Z);
        var tfr = new Vector3(max.X, max.Y, max.Z);
        var bfr = new Vector3(max.X, min.Y, max.Z);
        var bfl = new Vector3(min.X, min.Y, max.Z);
        var tbl = new Vector3(min.X, max.Y, min.Z);
        var tbr = new Vector3(max.X, max.Y, min.Z);
        var bbr = new Vector3(max.X, min.Y, min.Z);
        var bbl = new Vector3(min.X, min.Y, min.Z);

        //front face
        AddTri(bfl, tfl, tfr);
        AddTri(bfl, tfr, bfr);

        // top face
        AddTri(tfl, tbl, tbr);
        AddTri(tfl, tbr, tfr);

        // back face
        AddTri(bbl, tbl, tbr);
        AddTri(bbl, tbr, bbr);

        // bottom face
        AddTri(bfl, bbl, bbr);
        AddTri(bfl, bbr, bfr);

        // left face
        AddTri(bbl, tbl, tfl);
        AddTri(bbl, tfl, bfl);

        // right face
        AddTri(bbr, tbr, tfr);
        AddTri(bbr, tfr, bfr);
    }

    public override MeshLayoutType LayoutType => throw new NotImplementedException();

    public override VertexLayoutDescription LayoutDescription => throw new NotImplementedException();

    public override BoundingBox Bounds => throw new NotImplementedException();

    public override string ShaderName => throw new NotImplementedException();

    public override SpecializationConstant[] SpecializationConstants => throw new NotImplementedException();

    public override VkCullModeFlags CullMode => VkCullModeFlags.None;

    public override VkPolygonMode FillMode => VkPolygonMode.Fill;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.TriangleList;

    public override uint VertexSize => throw new NotImplementedException();

    public void AddTri(Vector3 a, Vector3 b, Vector3 c)
    {
        //var dir = Vector3.Cross(b - a, c - a);
        //var norm = Vector3.Normalize(dir);

        var vertA = new VertexPositionColorNormal(a, Color.White, Vector3.Zero);
        var vertB = new VertexPositionColorNormal(b, Color.White, Vector3.Zero);
        var vertC = new VertexPositionColorNormal(c, Color.White, Vector3.Zero);

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

        AddIndex((short)vertIndexA);
        AddIndex((short)vertIndexB);
        AddIndex((short)vertIndexC);
    }

    protected override void DisposeBuffers()
    {
        //VertBuffer?.Dispose();
        //IndexBuffer?.Dispose();
    }

    public void Render(Renderer.IndirectDrawEncoder encoder, SceneRenderPipeline sp)
    {
        throw new NotImplementedException();
    }

    public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
        throw new NotImplementedException();
    }
}
