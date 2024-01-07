using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene;

public interface IMeshProviderEventListener
{
    public void OnProviderAvailable();

    public void OnProviderUnavailable();
}

public static class MeshProviderCache
{
    private static readonly Dictionary<string, MeshProvider> _cache = new();

    public static FlverMeshProvider GetFlverMeshProvider(string virtualResourcePath)
    {
        if (_cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is FlverMeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        FlverMeshProvider nfmp = new(virtualResourcePath);
        _cache.Add(virtualResourcePath, nfmp);
        return nfmp;
    }

    public static CollisionMeshProvider GetCollisionMeshProvider(string virtualResourcePath)
    {
        if (_cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is CollisionMeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        CollisionMeshProvider nfmp = new(virtualResourcePath);
        _cache.Add(virtualResourcePath, nfmp);
        return nfmp;
    }

    public static NavmeshProvider GetNVMMeshProvider(string virtualResourcePath)
    {
        if (_cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is NavmeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        NavmeshProvider nfmp = new(virtualResourcePath);
        _cache.Add(virtualResourcePath, nfmp);
        return nfmp;
    }

    public static HavokNavmeshProvider GetHavokNavMeshProvider(string virtualResourcePath, bool temp = false)
    {
        if (!temp && _cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is HavokNavmeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        HavokNavmeshProvider nfmp = new(virtualResourcePath);
        if (!temp)
        {
            _cache.Add(virtualResourcePath, nfmp);
        }

        return nfmp;
    }

    public static void InvalidateMeshProvider(IResourceHandle handle)
    {
        if (_cache.ContainsKey(handle.AssetVirtualPath))
        {
            _cache.Remove(handle.AssetVirtualPath);
        }
    }
}

/// <summary>
///     Common interface to a family of providers that can be used to supply mesh
///     data to renderable proxies and renderables. For instance, many individual
///     resources are capable of supplying render meshes, and they may or may not be
///     loaded
/// </summary>
public abstract class MeshProvider
{
    protected List<WeakReference<IMeshProviderEventListener>> _listeners = new();

    /// <summary>
    ///     This mesh provider has child mesh providers. For example, a FLVER mesh
    ///     provider will have submesh providers that provide the actual mesh data
    /// </summary>
    public virtual int ChildCount => 0;

    /// <summary>
    ///     Underlying layout type of the mesh data
    /// </summary>
    public abstract MeshLayoutType LayoutType { get; }

    public abstract VertexLayoutDescription LayoutDescription { get; }

    public abstract BoundingBox Bounds { get; }

    /// <summary>
    ///     Object space transform of the mesh
    /// </summary>
    public virtual Matrix4x4 ObjectTransform => Matrix4x4.Identity;

    /// <summary>
    ///     Get handle to the GPU allocated geometry
    /// </summary>
    public abstract VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer { get; }

    /// <summary>
    ///     Get handle to the material data
    /// </summary>
    public abstract GPUBufferAllocator.GPUBufferHandle MaterialBuffer { get; }

    public virtual uint MaterialIndex => 0;

    /// <summary>
    ///     Get handle to GPU bone transforms
    /// </summary>
    public virtual GPUBufferAllocator.GPUBufferHandle BoneBuffer => null;

    // Pipeline state
    public abstract string ShaderName { get; }

    public abstract SpecializationConstant[] SpecializationConstants { get; }

    public virtual VkCullModeFlags CullMode => VkCullModeFlags.None;

    public virtual VkPolygonMode FillMode => VkPolygonMode.Fill;

    public virtual VkFrontFace FrontFace => VkFrontFace.CounterClockwise;

    public virtual VkPrimitiveTopology Topology => VkPrimitiveTopology.TriangleList;

    // Mesh data
    public virtual bool Is32Bit => false;

    public virtual int IndexOffset => 0;

    public virtual int IndexCount => 0;

    public virtual uint VertexSize => 0;

    // Original resource
    public virtual IResourceHandle ResourceHandle => null;

    // Selection properties
    public virtual bool UseSelectedShader => true;
    public virtual bool SelectedUseBackface => true;
    public virtual bool SelectedRenderBaseMesh => true;

    public void AddEventListener(IMeshProviderEventListener listener)
    {
        if (IsAvailable())
        {
            listener.OnProviderAvailable();
        }
        else
        {
            listener.OnProviderUnavailable();
        }

        _listeners.Add(new WeakReference<IMeshProviderEventListener>(listener));
    }

    protected void NotifyAvailable()
    {
        foreach (WeakReference<IMeshProviderEventListener> listener in _listeners)
        {
            IMeshProviderEventListener l;
            var succ = listener.TryGetTarget(out l);
            if (succ)
            {
                l.OnProviderAvailable();
            }
        }
    }

    protected void NotifyUnavailable()
    {
        foreach (WeakReference<IMeshProviderEventListener> listener in _listeners)
        {
            IMeshProviderEventListener l;
            var succ = listener.TryGetTarget(out l);
            if (succ)
            {
                l.OnProviderUnavailable();
            }
        }
    }

    /// <summary>
    ///     Attempts to lock the underlying resource such that it can't be unloaded
    ///     while the lock is active
    /// </summary>
    /// <returns>If the resource was loaded and locked</returns>
    public virtual bool TryLock() { return true; }

    /// <summary>
    ///     Unlocks the underlying resource allowing it to be locked by another thread
    ///     or unloaded
    /// </summary>
    public virtual void Unlock() { }

    public virtual void Acquire() { }
    public virtual void Release() { }

    /// <summary>
    ///     This mesh provider is capable of supplying mesh data at the moment.
    ///     For example, this may return true if the underlying resource is loaded,
    ///     and false if it is not.
    /// </summary>
    /// <returns>If the resource is available</returns>
    public virtual bool IsAvailable() { return true; }

    /// <summary>
    ///     This mesh provider is atomic with respect to the underlying resource.
    ///     This means that this represents an entire mesh that is able to be loaded and
    ///     unloaded at once. If this is not atomic, this means that it may be a submesh
    ///     of a larger resource, and this provider may not be valid if the resource is
    ///     unloaded because its existence depends on the parent resource
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAtomic() { return true; }

    /// <summary>
    ///     This mesh provider has mesh data. If it does not have mesh data, child
    ///     providers probably do have mesh data.
    /// </summary>
    /// <returns></returns>
    public virtual bool HasMeshData() { return false; }

    /// <summary>
    ///     Get the child provider at the supplied index
    /// </summary>
    /// <param name="index">index to the child provider</param>
    /// <returns>The child provider</returns>
    public virtual MeshProvider GetChildProvider(int index) { return null; }
}

public class FlverMeshProvider : MeshProvider, IResourceEventListener
{
    private readonly string _resourceName;
    private BoundingBox _bounds;

    private int _referenceCount;
    private ResourceHandle<FlverResource> _resource;

    private List<FlverSubmeshProvider> _submeshes = new();

    public FlverMeshProvider(string resource)
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

    public string MeshName => Path.GetFileNameWithoutExtension(_resourceName);

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
        foreach (FlverSubmeshProvider submesh in _submeshes)
        {
            submesh.Invalidate();
        }

        _submeshes.Clear();
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
        _submeshes = new List<FlverSubmeshProvider>();
        FlverResource res = _resource.Get();
        _bounds = res.Bounds;
        for (var i = 0; i < res.GPUMeshes.Length; i++)
        {
            var sm = new FlverSubmeshProvider(_resource, i);
            _submeshes.Add(sm);
        }
    }
}

public unsafe class FlverSubmeshProvider : MeshProvider
{
    private readonly int _meshIndex;

    private readonly ResourceHandle<FlverResource> _resource;
    private bool _isValid = true;

    public FlverSubmeshProvider(ResourceHandle<FlverResource> handle, int idx)
    {
        _resource = handle;
        _meshIndex = idx;
    }

    public override BoundingBox Bounds => _resource.Get().GPUMeshes[_meshIndex].Bounds;

    public override Matrix4x4 ObjectTransform => _resource.Get().GPUMeshes[_meshIndex].LocalTransform;

    public override MeshLayoutType LayoutType => _resource.Get().GPUMeshes[_meshIndex].Material.LayoutType;

    public override VertexLayoutDescription LayoutDescription =>
        _resource.Get().GPUMeshes[_meshIndex].Material.VertexLayout;

    public override VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer =>
        _resource.Get().GPUMeshes[_meshIndex].GeomBuffer;

    public override GPUBufferAllocator.GPUBufferHandle MaterialBuffer =>
        _resource.Get().GPUMeshes[_meshIndex].Material.MaterialBuffer;

    public override uint MaterialIndex => MaterialBuffer.AllocationStart / (uint)sizeof(Material);

    public override GPUBufferAllocator.GPUBufferHandle BoneBuffer => _resource.Get().StaticBoneBuffer;

    public override string ShaderName => _resource.Get().GPUMeshes[_meshIndex].Material.ShaderName;

    public override SpecializationConstant[] SpecializationConstants =>
        _resource.Get().GPUMeshes[_meshIndex].Material.SpecializationConstants.ToArray();

    public override VkCullModeFlags CullMode =>
        _resource.Get().GPUMeshes[_meshIndex].MeshFacesets[0].BackfaceCulling
            ? VkCullModeFlags.Back
            : VkCullModeFlags.None;

    public override VkFrontFace FrontFace => VkFrontFace.CounterClockwise;

    public override VkPrimitiveTopology Topology =>
        _resource.Get().GPUMeshes[_meshIndex].MeshFacesets[0].IsTriangleStrip
            ? VkPrimitiveTopology.TriangleStrip
            : VkPrimitiveTopology.TriangleList;

    public override bool Is32Bit => _resource.Get().GPUMeshes[_meshIndex].MeshFacesets[0].Is32Bit;

    public override int IndexOffset => _resource.Get().GPUMeshes[_meshIndex].MeshFacesets[0].IndexOffset;

    public override int IndexCount => _resource.Get().GPUMeshes[_meshIndex].MeshFacesets.Count > 0
        ? _resource.Get().GPUMeshes[_meshIndex].MeshFacesets[0].IndexCount
        : 0;

    public override uint VertexSize => _resource.Get().GPUMeshes[_meshIndex].Material.VertexSize;

    public string MeshName => Path.GetFileNameWithoutExtension(_resource.AssetVirtualPath);

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
        if (_resource.Get().GPUMeshes[_meshIndex].MeshFacesets.Count == 0)
        {
            return false;
        }

        return true;
    }
}

public class CollisionMeshProvider : MeshProvider, IResourceEventListener
{
    private readonly string _resourceName;
    private BoundingBox _bounds;

    private int _referenceCount;
    private ResourceHandle<HavokCollisionResource> _resource;

    private List<CollisionSubmeshProvider> _submeshes = new();

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

public class NavmeshProvider : MeshProvider, IResourceEventListener
{
    private readonly string _resourceName;
    private int _referenceCount;
    private ResourceHandle<NVMNavmeshResource> _resource;

    public NavmeshProvider(string resource)
    {
        _resourceName = resource;
        _resource = null;
    }

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

        _resource = (ResourceHandle<NVMNavmeshResource>)handle;
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
            ResourceManager.AddResourceListener<NVMNavmeshResource>(_resourceName, this,
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
