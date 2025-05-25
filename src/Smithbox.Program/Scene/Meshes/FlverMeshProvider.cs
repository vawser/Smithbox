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

public class FlverMeshProvider : MeshProvider, IResourceEventListener
{
    private readonly string _resourceName;
    private BoundingBox _bounds;

    private int _referenceCount;
    private ResourceHandle<FlverResource> _resource;

    public List<int> ModelMasks = new();
    List<FlverSubmeshProvider> _allSubmeshes = new();
    private List<FlverSubmeshProvider> _activeSubmeshes = new();

    public FlverMeshProvider(string resource)
    {
        _resourceName = resource;
        _resource = null;
    }
    private void UpdateModelMasks()
    {
        if (!ModelMasks.Any())
        {
            _activeSubmeshes = _allSubmeshes;
            return;
        }

        _activeSubmeshes = _allSubmeshes.Where((p, i) =>
        {
            var res = _resource.Get();

            if (res.GPUMeshes.Length > i)
            {
                var mask = res.GPUMeshes[i].Material.MaterialMask;
                return mask == -1 || ModelMasks[mask] == 1;
            }
            else
            {
                return true;
            }
        }).ToList();
    }

    public override int ChildCount => _activeSubmeshes.Count;

    public override BoundingBox Bounds => _bounds;

    public override MeshLayoutType LayoutType => throw new NotImplementedException();

    public override VertexLayoutDescription LayoutDescription => throw new NotImplementedException();

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        throw new NotImplementedException();

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer => throw new NotImplementedException();

    public override string ShaderName => throw new NotImplementedException();

    public override SpecializationConstant[] SpecializationConstants => throw new NotImplementedException();

    public string MeshName => Path.GetFileNameWithoutExtension(_resourceName);

    public bool IsSpeedtree;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (_resource != null)
        {
            return;
        }

        _resource = (ResourceHandle<FlverResource>)handle;
        _resource.Acquire();
        CreateSubmeshes();
        NotifyAvailable();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        _resource?.Release();
        _resource = null;
        foreach (FlverSubmeshProvider submesh in _allSubmeshes)
        {
            submesh.Invalidate();
        }

        _allSubmeshes.Clear();
        NotifyUnavailable();
    }

    ~FlverMeshProvider()
    {
        if (_resource != null)
        {
            _resource.Release();
        }
    }

    public override MeshProvider GetChildProvider(int index)
    {
        return _activeSubmeshes[index];
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
            ResourceManager.AddResourceListener<FlverResource>(_resourceName, this,
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
        return _resource != null && _referenceCount > 0 &&
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
        _allSubmeshes = new List<FlverSubmeshProvider>();
        FlverResource res = _resource.Get();
        IsSpeedtree = res.IsSpeedtree;
        _bounds = res.Bounds;
        for (var i = 0; i < res.GPUMeshes.Length; i++)
        {
            var sm = new FlverSubmeshProvider(_resource, i);
            _allSubmeshes.Add(sm);
        }
        UpdateModelMasks();
    }
}
