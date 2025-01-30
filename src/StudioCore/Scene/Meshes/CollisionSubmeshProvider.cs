using StudioCore.Resource;
using StudioCore.Resource.Types;
using StudioCore.Scene.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene.Meshes;

public class CollisionSubmeshProvider : MeshProvider
{
    private readonly int _meshIndex;

    private readonly ResourceHandle<HavokCollisionResource> _resource;
    private bool _isValid = true;

    public CollisionSubmeshProvider(ResourceHandle<HavokCollisionResource> handle, int idx)
    {
        _resource = handle;
        _meshIndex = idx;
    }

    public override BoundingBox Bounds => _resource.Get().GPUMeshes[_meshIndex].Bounds;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutCollision;

    public override VertexLayoutDescription LayoutDescription => CollisionLayout.Layout;

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        _resource.Get().GPUMeshes[_meshIndex].GeomBuffer;

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => null;

    //public override uint MaterialIndex => MaterialBuffer.AllocationStart / (uint)sizeof(Material);

    public override string ShaderName => "Collision";

    public override SpecializationConstant[] SpecializationConstants => new SpecializationConstant[0];

    public override VkCullModeFlags CullMode => VkCullModeFlags.Back;

    public override VkFrontFace FrontFace => _resource.Get().FrontFace;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.TriangleList;

    public override bool Is32Bit => true;

    public override int IndexOffset => 0;

    public override int IndexCount => _resource.Get().GPUMeshes[_meshIndex].IndexCount;

    public override uint VertexSize => CollisionLayout.SizeInBytes;

    public override bool SelectedUseBackface => false;

    public override bool SelectedRenderBaseMesh => false;

    public override bool TryLock()
    {
        return true;
    }

    public override void Unlock()
    {
    }

    internal void Invalidate()
    {
        _isValid = false;
    }

    public override bool IsAvailable()
    {
        return _isValid;
    }

    public override bool IsAtomic()
    {
        return false;
    }

    public override bool HasMeshData()
    {
        if (_resource.Get().GPUMeshes[_meshIndex].VertexCount == 0)
        {
            return false;
        }

        return true;
    }
}
