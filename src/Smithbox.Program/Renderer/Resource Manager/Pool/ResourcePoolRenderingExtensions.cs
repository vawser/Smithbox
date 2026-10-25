#nullable enable
using System;
using System.Collections.Generic;

namespace StudioCore.Renderer;

public static class ResourcePoolRenderingExtensions
{
    /// <summary>
    ///     Shorthand for finding a model by name and creating a renderable for it:
    ///     <c>pool.Find&lt;FlverResource&gt;(name)</c> followed by <see cref="MeshRenderableProxy.FromHandle" />.
    /// </summary>
    public static MeshRenderableProxy CreateRenderable<T>(this ResourcePool pool, RenderScene scene, string name,
        ModelMarkerType modelType = ModelMarkerType.None) where T : class, IResource, IDisposable, new()
    {
        return MeshRenderableProxy.FromHandle(scene, pool.Find<T>(name), modelType);
    }

    /// <summary>
    ///     Same as <see cref="CreateRenderable{T}" /> for models, with the model masks / unique id options.
    /// </summary>
    public static MeshRenderableProxy CreateModelRenderable(this ResourcePool pool, RenderScene scene, string name,
        ModelMarkerType modelType, string uid, IEnumerable<int>? masks)
    {
        return MeshRenderableProxy.MeshRenderableFromFlverResource(scene, pool.Find<FlverResource>(name), modelType,
            uid, masks);
    }
}
