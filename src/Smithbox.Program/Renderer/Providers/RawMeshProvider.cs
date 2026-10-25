#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     Mesh data supplied directly by the caller instead of coming from a loaded resource.
///     No pool or name is involved:
///     <code>
///     var mesh = RawMeshProvider.FromTriangles(positions, indices, Color.Orange);
///     var proxy = MeshRenderableProxy.FromProvider(scene, mesh);
///     proxy.World = Matrix4x4.CreateTranslation(0, 1, 0);
///     </code>
///     The GPU buffers are freed when the last renderable using the provider is released. If a provider is created but
///     never given to a renderable, call <see cref="Dispose" />.
/// </summary>
public sealed unsafe class RawMeshProvider : MeshProvider, IDisposable
{
    private readonly VertexIndexBufferAllocator.VertexIndexBufferHandle _geometry;
    private readonly bool _isLines;
    private readonly BoundingBox _bounds;
    private readonly int _indexCount;

    private bool _disposed;
    private bool _everAcquired;
    private int _referenceCount;

    private RawMeshProvider(VertexIndexBufferAllocator.VertexIndexBufferHandle geometry, int indexCount,
        BoundingBox bounds, bool isLines)
    {
        _geometry = geometry;
        _indexCount = indexCount;
        _bounds = bounds;
        _isLines = isLines;
    }

    public override BoundingBox Bounds => _bounds;

    public override MeshLayoutType LayoutType =>
        _isLines ? MeshLayoutType.LayoutPositionColor : MeshLayoutType.LayoutNavmesh;

    public override VertexLayoutDescription LayoutDescription =>
        _isLines ? PositionColor.Layout : NavmeshLayout.Layout;

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer => _geometry;

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => null!;

    public override string ShaderName => _isLines ? "NavWire" : "NavSolid";

    public override SpecializationConstant[] SpecializationConstants => Array.Empty<SpecializationConstant>();

    // Raw triangles are drawn from both sides so the winding order of the caller's data doesn't matter
    public override VkCullModeFlags CullMode => VkCullModeFlags.None;

    public override VkFrontFace FrontFace => VkFrontFace.CounterClockwise;

    public override VkPrimitiveTopology Topology =>
        _isLines ? VkPrimitiveTopology.LineList : VkPrimitiveTopology.TriangleList;

    public override bool Is32Bit => true;

    public override int IndexOffset => 0;

    public override int IndexCount => _indexCount;

    public override uint VertexSize =>
        _isLines ? MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor) : NavmeshLayout.SizeInBytes;

    public override bool SelectedUseBackface => false;

    public override bool SelectedRenderBaseMesh => false;

    public override bool UseSelectedShader => !_isLines;

    public override bool HasMeshData()
    {
        return _indexCount > 0;
    }

    public override bool IsAvailable()
    {
        return !_disposed;
    }

    public override void Acquire()
    {
        _everAcquired = true;
        _referenceCount++;
    }

    public override void Release()
    {
        if (_referenceCount > 0)
        {
            _referenceCount--;
        }

        if (_referenceCount == 0 && _everAcquired)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _geometry.Dispose();
        NotifyUnavailable();
    }

    /// <summary>
    ///     Solid, flat shaded triangles in one colour.
    /// </summary>
    /// <param name="positions">Vertex positions</param>
    /// <param name="indices">Three indices into <paramref name="positions" /> per triangle</param>
    public static RawMeshProvider FromTriangles(IReadOnlyList<Vector3> positions, IReadOnlyList<int> indices,
        Color color)
    {
        if (indices.Count % 3 != 0)
        {
            throw new ArgumentException("Triangle indices must come in groups of three.", nameof(indices));
        }

        // Every triangle gets its own three vertices so that it can have its own flat normal
        var count = indices.Count;
        var vsize = (uint)count * NavmeshLayout.SizeInBytes;
        var isize = (uint)count * 4u;

        VertexIndexBufferAllocator.VertexIndexBufferHandle geometry =
            SceneRenderer.GeometryBufferAllocator.Allocate(vsize, isize, (int)NavmeshLayout.SizeInBytes, 4);

        var outIndices = new Span<int>(geometry.MapIBuffer().ToPointer(), count);
        var outVertices = new Span<NavmeshLayout>(geometry.MapVBuffer().ToPointer(), count);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var t = 0; t < count; t += 3)
        {
            Vector3 p0 = positions[indices[t]];
            Vector3 p1 = positions[indices[t + 1]];
            Vector3 p2 = positions[indices[t + 2]];

            Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
            n = n.LengthSquared() > 0 ? Vector3.Normalize(n) : Vector3.UnitY;

            for (var v = 0; v < 3; v++)
            {
                Vector3 p = v == 0 ? p0 : v == 1 ? p1 : p2;
                ref NavmeshLayout vert = ref outVertices[t + v];

                vert = new NavmeshLayout();
                vert.Position = p;
                vert.Normal[0] = (sbyte)(n.X * 127.0f);
                vert.Normal[1] = (sbyte)(n.Y * 127.0f);
                vert.Normal[2] = (sbyte)(n.Z * 127.0f);
                vert.Color[0] = color.R;
                vert.Color[1] = color.G;
                vert.Color[2] = color.B;
                vert.Color[3] = color.A;
                vert.Barycentric[0] = (byte)(v == 1 ? 1 : 0);
                vert.Barycentric[1] = (byte)(v == 2 ? 1 : 0);

                outIndices[t + v] = t + v;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }

        geometry.UnmapIBuffer();
        geometry.UnmapVBuffer();

        return new RawMeshProvider(geometry, count, count > 0 ? new BoundingBox(min, max) : new BoundingBox(), false);
    }

    /// <summary>
    ///     Line segments in one colour.
    /// </summary>
    /// <param name="positions">Vertex positions</param>
    /// <param name="indices">Two indices into <paramref name="positions" /> per line</param>
    public static RawMeshProvider FromLines(IReadOnlyList<Vector3> positions, IReadOnlyList<int> indices, Color color)
    {
        if (indices.Count % 2 != 0)
        {
            throw new ArgumentException("Line indices must come in groups of two.", nameof(indices));
        }

        var vertexCount = positions.Count;
        var count = indices.Count;
        var vertexSize = MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);

        VertexIndexBufferAllocator.VertexIndexBufferHandle geometry =
            SceneRenderer.GeometryBufferAllocator.Allocate((uint)vertexCount * vertexSize, (uint)count * 4u,
                (int)vertexSize, 4);

        var outIndices = new Span<int>(geometry.MapIBuffer().ToPointer(), count);
        var outVertices = new Span<PositionColor>(geometry.MapVBuffer().ToPointer(), vertexCount);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var v = 0; v < vertexCount; v++)
        {
            ref PositionColor vert = ref outVertices[v];
            vert = new PositionColor();
            vert.Position = positions[v];
            vert.Color[0] = color.R;
            vert.Color[1] = color.G;
            vert.Color[2] = color.B;
            vert.Color[3] = color.A;
            min = Vector3.Min(min, positions[v]);
            max = Vector3.Max(max, positions[v]);
        }

        for (var i = 0; i < count; i++)
        {
            outIndices[i] = indices[i];
        }

        geometry.UnmapIBuffer();
        geometry.UnmapVBuffer();

        return new RawMeshProvider(geometry, count, vertexCount > 0 ? new BoundingBox(min, max) : new BoundingBox(),
            true);
    }
}
