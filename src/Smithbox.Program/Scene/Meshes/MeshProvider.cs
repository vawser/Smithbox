using StudioCore.Resource;
using StudioCore.Resource.Types;
using StudioCore.Scene.Interfaces;
using StudioCore.Scene.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;
using static SoulsFormats.DRB.Shape;

namespace StudioCore.Scene.Meshes;

/// <summary>
///     Common interface to a family of providers that can be used to supply mesh
///     data to renderable proxies and renderables. For instance, many individual
///     resources are capable of supplying render meshes, and they may or may not be
///     loaded
/// </summary>
public abstract class MeshProvider
{
    public List<WeakReference<IMeshProviderEventListener>> _listeners = new();

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







