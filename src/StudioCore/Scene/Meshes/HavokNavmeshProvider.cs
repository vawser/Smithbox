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

public class HavokNavmeshProvider : MeshProvider, IResourceEventListener
{
    private readonly HavokNavmeshCostGraphProvider _costGraphProvider;
    private readonly string _resourceName;

    private int _referenceCount;
    private ResourceHandle<HavokNavmeshResource> _resource;

    public HavokNavmeshProvider(string resource)
    {
        _resourceName = resource;
        _resource = null;
        _costGraphProvider = new HavokNavmeshCostGraphProvider(resource);
    }

    public override int ChildCount => 1;

    public override BoundingBox Bounds => _resource.Get().Bounds;

    public override MeshLayoutType LayoutType => MeshLayoutType.LayoutNavmesh;

    public override VertexLayoutDescription LayoutDescription => NavmeshLayout.Layout;

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer => _resource.Get().GeomBuffer;

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => null;

    //public override uint MaterialIndex => MaterialBuffer.AllocationStart / (uint)sizeof(Material);

    public override string ShaderName => "NavSolid";

    public override SpecializationConstant[] SpecializationConstants => new SpecializationConstant[0];

    public override VkCullModeFlags CullMode => VkCullModeFlags.Back;

    public override VkFrontFace FrontFace => VkFrontFace.Clockwise;

    public override VkPrimitiveTopology Topology => VkPrimitiveTopology.TriangleList;

    public override bool Is32Bit => true;

    public override int IndexOffset => 0;

    public override int IndexCount => _resource.Get().IndexCount;

    public override uint VertexSize => NavmeshLayout.SizeInBytes;

    public override bool SelectedUseBackface => false;

    public override bool SelectedRenderBaseMesh => false;

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
        _costGraphProvider.Acquire();
    }

    public override void Release()
    {
        _referenceCount--;
        if (_referenceCount <= 0)
        {
            _referenceCount = 0;
            OnResourceUnloaded(_resource, 0);
        }

        _costGraphProvider.Release();
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

    public override MeshProvider GetChildProvider(int index)
    {
        return _costGraphProvider;
    }
}
