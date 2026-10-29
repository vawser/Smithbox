using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public class CollisionMeshProvider : MeshProvider, IResourceEventListener
{
    private readonly ResourceHandle<HavokCollisionResource> _source;
    public BoundingBox _bounds;

    public int _referenceCount;
    public ResourceHandle<HavokCollisionResource> _resource;

    public List<CollisionSubmeshProvider> _submeshes = new();

    public CollisionMeshProvider(ResourceHandle<HavokCollisionResource> source)
    {
        _source = source;
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

    public override IResourceHandle ResourceHandle => _source;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (_resource != null)
        {
            return;
        }

        // Released providers must not revive on a later reload (see FlverMeshProvider)
        if (_referenceCount <= 0)
        {
            return;
        }

        _resource = (ResourceHandle<HavokCollisionResource>)handle;
        _resource.Acquire();
        CreateSubmeshes();
        NotifyAvailable();

        MeshProviderInspector.Add(_source.FullName, this);
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

        MeshProviderInspector.Remove(_source.FullName);
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
        var first = _referenceCount == 0;
        _referenceCount++;

        if (first)
        {
            ResourceManager.AddResourceListener(_source, this, AccessLevel.AccessGPUOptimizedOnly);
        }
    }

    public override void Release()
    {
        _referenceCount--;
        if (_referenceCount <= 0)
        {
            _referenceCount = 0;
            OnResourceUnloaded(_resource, 0);
            _source.RemoveResourceEventListener(this);
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
