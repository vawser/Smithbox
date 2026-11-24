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

    public override MeshLayoutType LayoutType => throw new NotSupportedException();

    public override VertexLayoutDescription LayoutDescription => throw new NotSupportedException();

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        throw new NotSupportedException();

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => throw new NotSupportedException();

    public override string ShaderName => throw new NotSupportedException();

    public override SpecializationConstant[] SpecializationConstants => throw new NotSupportedException();

    public override IResourceHandle ResourceHandle => _source;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        lock (StateLock)
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
        }

        NotifyAvailable();

        MeshProviderInspector.Add(_source.FullName, this);
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        List<CollisionSubmeshProvider> detached;
        lock (StateLock)
        {
            detached = DetachResource();
        }

        FinishUnload(detached);
    }

    // Called under StateLock (see FlverMeshProvider)
    private List<CollisionSubmeshProvider> DetachResource()
    {
        _resource?.Release();
        _resource = null;

        List<CollisionSubmeshProvider> detached = _submeshes;
        _submeshes = new List<CollisionSubmeshProvider>();
        return detached;
    }

    private void FinishUnload(List<CollisionSubmeshProvider> detached)
    {
        foreach (CollisionSubmeshProvider submesh in detached)
        {
            submesh.Invalidate();
        }

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
        bool first;
        lock (StateLock)
        {
            first = _referenceCount == 0;
            _referenceCount++;
        }

        if (first)
        {
            ResourceManager.AddResourceListener(_source, this, AccessLevel.AccessGPUOptimizedOnly);
        }
    }

    public override void Release()
    {
        List<CollisionSubmeshProvider> detached;
        lock (StateLock)
        {
            _referenceCount--;
            if (_referenceCount > 0)
            {
                return;
            }

            _referenceCount = 0;
            detached = DetachResource();
            _source.RemoveResourceEventListener(this);
        }

        FinishUnload(detached);
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
