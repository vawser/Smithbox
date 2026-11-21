using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public class FlverMeshProvider : MeshProvider, IResourceEventListener
{
    private readonly ResourceHandle<FlverResource> _source;
    private BoundingBox _bounds;

    private int _referenceCount;
    private ResourceHandle<FlverResource> _resource;

    public List<int> ModelMasks = new();

    /// <summary>
    /// Leaves out the body hair meshes of a body model
    /// </summary>
    public bool HideBodyHair = false;

    /// <summary>
    /// Leaves out the head of a body model (its mask 3 mesh: in ER the skull a face part sits on)
    /// </summary>
    public bool HideHead = false;

    /// <summary>
    /// Leaves out a face mesh a body model carries itself (ER's fc_m_0100: a default face with a P[FG_...] material),
    /// which a face part replaces
    /// </summary>
    public bool HideFace = false;

    /// <summary>
    /// Leaves out meshes whose material has no definition (no MATBIN or MTD in the project's material bank), which
    /// the game can't draw either. ER's hair models hr_a_0100 and hr_a_0101 each carry a whole leftover female body
    /// (P[FC_F_0000]_SSStest, a material no MATBIN defines) that would sit on the real body. Only while the material
    /// bank is loaded, as without it no material has a definition.
    /// </summary>
    public bool HideUndefinedMaterials = false;

    // Every mesh of an Elden Ring body model has its part of the body as its mask, and this one is the head
    private const int HeadMask = 3;

    // Faces are meant to be the fg_ part models, so a body model that has one uses their material
    private const string FaceMaterialPrefix = "P[FG_";

    public List<FlverSubmeshProvider> _allSubmeshes = new();

    private List<FlverSubmeshProvider> _activeSubmeshes = new();

    public FlverMeshProvider(ResourceHandle<FlverResource> source)
    {
        _source = source;
        _resource = null;
    }
    private void UpdateModelMasks()
    {
        var hideUndefined = HideUndefinedMaterials &&
            Smithbox.Orchestrator.SelectedProject?.Handler?.MaterialData?.PrimaryBank != null;

        if (!ModelMasks.Any() && !HideBodyHair && !HideHead && !HideFace && !hideUndefined)
        {
            _activeSubmeshes = _allSubmeshes;
            return;
        }

        _activeSubmeshes = _allSubmeshes.Where((p, i) =>
        {
            var res = _resource.Get();

            if (res.GPUMeshes.Length > i)
            {
                var material = res.GPUMeshes[i].Material;

                if (HideBodyHair && material.IsBodyHair)
                {
                    return false;
                }

                if (HideHead && material.MaterialMask == HeadMask)
                {
                    return false;
                }

                if (HideFace && material.MaterialName?.StartsWith(FaceMaterialPrefix, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return false;
                }

                if (hideUndefined && material.Definition == null)
                {
                    return false;
                }

                if (!ModelMasks.Any())
                {
                    return true;
                }

                var mask = res.GPUMeshes[i].Material.MaterialMask;

                if (mask < ModelMasks.Count && mask > -1)
                {
                    return ModelMasks[mask] == 1;
                }
                else if(mask == -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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

    public string MeshName => _source.Name;

    public bool IsSpeedtree;

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        lock (StateLock)
        {
            if (_resource != null)
            {
                return;
            }

            // A provider nobody references any more must not come back to life when the resource is reloaded:
            // it would re-acquire the resource (leaking it) and notify listeners that were already disposed.
            if (_referenceCount <= 0)
            {
                return;
            }

            _resource = (ResourceHandle<FlverResource>)handle;
            _resource.Acquire();
            CreateSubmeshes();
        }

        NotifyAvailable();

        MeshProviderInspector.Add(_source.FullName, this);
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        List<FlverSubmeshProvider> detached;
        lock (StateLock)
        {
            detached = DetachResource();
        }

        FinishUnload(detached);
    }

    /// <summary>
    ///     Drops the resource and the submeshes built from it. Called under StateLock; the caller then passes the
    ///     submeshes to FinishUnload outside it.
    /// </summary>
    private List<FlverSubmeshProvider> DetachResource()
    {
        _resource?.Release();
        _resource = null;

        List<FlverSubmeshProvider> detached = _allSubmeshes;
        _allSubmeshes = new List<FlverSubmeshProvider>();
        _activeSubmeshes = _allSubmeshes;
        return detached;
    }

    private void FinishUnload(List<FlverSubmeshProvider> detached)
    {
        foreach (FlverSubmeshProvider submesh in detached)
        {
            submesh.Invalidate();
        }

        NotifyUnavailable();

        MeshProviderInspector.Remove(_source.FullName);
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

    public override IResourceHandle ResourceHandle => _source;

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
        List<FlverSubmeshProvider> detached;
        lock (StateLock)
        {
            _referenceCount--;
            if (_referenceCount > 0)
            {
                return;
            }

            _referenceCount = 0;
            detached = DetachResource();

            // Inside the lock, so an Acquire on another thread sees the provider either still held or fully released
            _source.RemoveResourceEventListener(this);
        }

        FinishUnload(detached);
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
