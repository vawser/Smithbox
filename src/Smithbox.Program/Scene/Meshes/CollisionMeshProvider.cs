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

public class CollisionMeshProvider : MeshProvider, IResourceEventListener
{
    public readonly string _resourceName;
    public BoundingBox _bounds;

    public int _referenceCount;
    public ResourceHandle<HavokCollisionResource> _resource;

    public List<CollisionSubmeshProvider> _submeshes = new();

    public CollisionMeshProvider(string resource)
    {
        _resourceName = resource;
        _resource = null;
    }

    public override int ChildCount => _submeshes.Count;

    public override BoundingBox Bounds => _bounds;

    public override MeshLayoutType LayoutType => throw new NotImplementedException();

    public override VertexLayoutDescription LayoutDescription => throw new NotImplementedException();

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        throw new NotImplementedException();

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => throw new NotImplementedException();

    public override string ShaderName => throw new NotImplementedException();

    public override SpecializationConstant[] SpecializationConstants => throw new NotImplementedException();

    public override IResourceHandle ResourceHandle => _resource;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (_resource != null)
        {
            return;
        }

        _resource = (ResourceHandle<HavokCollisionResource>)handle;
        _resource.Acquire();
        CreateSubmeshes();
        NotifyAvailable();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        _resource?.Release();
        _resource = null;
        foreach (CollisionSubmeshProvider submesh in _submeshes)
        {
            submesh.Invalidate();
        }

        _submeshes.Clear();
        NotifyUnavailable();
    }

    ~CollisionMeshProvider()
    {
        if (_resource != null)
        {
            _resource.Release();
        }
    }

    public override MeshProvider GetChildProvider(int index)
    {
        return _submeshes[index];
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
            ResourceManager.AddResourceListener<HavokCollisionResource>(_resourceName, this,
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
        return false;
    }

    private void CreateSubmeshes()
    {
        _submeshes = new List<CollisionSubmeshProvider>();
        HavokCollisionResource res = _resource.Get();
        _bounds = res.Bounds;
        if (res.GPUMeshes != null)
        {
            for (var i = 0; i < res.GPUMeshes.Length; i++)
            {
                var sm = new CollisionSubmeshProvider(_resource, i);
                _submeshes.Add(sm);
            }
        }
    }
}
