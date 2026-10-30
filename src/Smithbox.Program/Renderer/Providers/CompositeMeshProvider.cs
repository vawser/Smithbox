using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
///     Presents several models as one, for things that are built from separate FLVERs (such as c0000).
///     A proxy of this provider gets a child proxy per model, in order, so everything set on the
///     proxy reaches all of the models.
/// </summary>
public class CompositeMeshProvider : MeshProvider
{
    private readonly List<MeshProvider> _parts;

    public CompositeMeshProvider(IEnumerable<MeshProvider> parts)
    {
        _parts = new List<MeshProvider>(parts);
    }

    public override int ChildCount => _parts.Count;

    public override MeshProvider GetChildProvider(int index)
    {
        return _parts[index];
    }

    public override BoundingBox Bounds => throw new NotImplementedException();

    public override MeshLayoutType LayoutType => throw new NotImplementedException();

    public override VertexLayoutDescription LayoutDescription => throw new NotImplementedException();

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        throw new NotImplementedException();

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => throw new NotImplementedException();

    public override string ShaderName => throw new NotImplementedException();

    public override SpecializationConstant[] SpecializationConstants => throw new NotImplementedException();
}
