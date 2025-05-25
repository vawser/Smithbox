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

public class HavokNavmeshCostGraphProvider : MeshProvider, IResourceEventListener
{
    private readonly string _resourceName;
    private int _referenceCount;
    private ResourceHandle<HavokNavmeshResource> _resource;

    public HavokNavmeshCostGraphProvider(string resource)
    {
        _resourceName = resource;
        _resource = null;
    }

    public override BoundingBox Bounds => _resource.Get().Bounds;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutPositionColor;

    public override VertexLayoutDescription LayoutDescription => PositionColor.Layout;

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        _resource.Get().CostGraphGeomBuffer;

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => null;

    //public override uint MaterialIndex => MaterialBuffer.AllocationStart / (uint)sizeof(Material);

    public override string ShaderName => "NavWire";

    public override SpecializationConstant[] SpecializationConstants => new SpecializationConstant[0];

    public override VkCullModeFlags CullMode => VkCullModeFlags.None;

    public override VkFrontFace FrontFace => VkFrontFace.Clockwise;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.LineList;

    public override bool Is32Bit => true;

    public override int IndexOffset => 0;

    public override int IndexCount => _resource.Get().GraphIndexCount;

    public override uint VertexSize => MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);

    public override bool SelectedUseBackface => false;

    public override bool SelectedRenderBaseMesh => false;

    public override bool UseSelectedShader => false;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (_resource != null)
        {
            return;
        }

        _resource = (ResourceHandle<HavokNavmeshResource>)handle;
        _resource.Acquire();
        NotifyAvailable();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        _resource?.Release();
        _resource = null;
        NotifyUnavailable();
    }

    ~HavokNavmeshCostGraphProvider()
    {
        if (_resource != null)
        {
            _resource.Release();
        }
    }

    public override bool TryLock()
    {
        return true;
    }

    public override void Unlock()
    {
    }

    public override void Acquire()
    {
        if (_referenceCount == 0)
        {
            ResourceManager.AddResourceListener<HavokNavmeshResource>(_resourceName, this,
                AccessLevel.AccessGPUOptimizedOnly);
        }

        _referenceCount++;
    }

    public override void Release()
    {
        _referenceCount--;
        if (_referenceCount <= 0)
        {
            _referenceCount = 0;
            OnResourceUnloaded(_resource, 0);
        }
    }

    public override bool IsAvailable()
    {
        return _resource != null &&
               (_resource.AccessLevel == AccessLevel.AccessGPUOptimizedOnly ||
                _resource.AccessLevel == AccessLevel.AccessFull);
    }

    public override bool IsAtomic()
    {
        return true;
    }

    public override bool HasMeshData()
    {
        if (_resource != null && _resource.Get().VertexCount > 0)
        {
            return true;
        }

        return false;
    }
}
