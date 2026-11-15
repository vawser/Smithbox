#nullable enable
using StudioCore.Editors.Common;
using System;
using System.Numerics;
using System.Threading;
using Tracy;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// Proxy class that represents a connection between the logical scene
/// heirarchy and the actual underlying renderable representation. Used to handle
/// things like renderable construction, selection, hiding/showing, etc
/// </summary>
public abstract class RenderableProxy : SceneRenderer.IRendererUpdatable, IDisposable
{
    public bool _autoregister = true;

    public bool _registered;
    public bool disposedValue;

    public abstract bool RenderSelectionOutline { set; get; }

    /// <summary>
    /// Draws this as part of a map that is out of focus: semi-transparent and grey.
    /// A selection outline takes priority, so a selected object always looks selected.
    /// </summary>
    public virtual bool RenderFocusGhost { get; set; }

    public abstract Matrix4x4 World { get; set; }

    public abstract bool Visible { get; set; }

    public abstract RenderFilter DrawFilter { get; set; }
    public abstract DrawGroup DrawGroups { get; set; }
    public virtual bool AutoRegister { get => _autoregister; set => _autoregister = value; }

    public virtual void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp);
    public abstract void DestroyRenderables();
    public abstract void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp);

    public abstract void SetSelectable(ISelectable sel);

    public abstract BoundingBox GetBounds();

    public abstract BoundingBox GetFramingBounds();

    public abstract BoundingBox GetLocalBounds();

    // 1 while a construction / update of this proxy is waiting in the upload queue. Both read the proxy's state
    // when they run, not when they were asked for, so a second request made before then would only repeat the work.
    // A map load asks several times for each submesh (its constructor, the parent's state copied onto it, the
    // parent's own construction), which put hundreds of thousands of tasks in the queue for large maps.
    private int _constructionQueued;
    private int _updateQueued;

    /// <summary>
    ///     Whether <see cref="ConstructRenderables" /> also does all of what <see cref="UpdateRenderables" /> does,
    ///     so an update asked for while a construction is waiting can be left to it.
    /// </summary>
    protected virtual bool ConstructionCoversUpdate => false;

    /// <summary>
    ///     Whether a construction of this proxy is waiting in the upload queue.
    /// </summary>
    protected bool IsConstructionQueued => Volatile.Read(ref _constructionQueued) != 0;

    internal void ScheduleRenderableConstruction()
    {
        // Already waiting: that one will see the current state
        if (Interlocked.Exchange(ref _constructionQueued, 1) != 0)
        {
            return;
        }

        SceneRenderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            // Cleared before running, so that a construction asked for while this one runs (e.g. it finds its
            // geometry not on the GPU yet and asks again) is queued rather than dropped
            Volatile.Write(ref _constructionQueued, 0);

            Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"Renderable construction");
            ConstructRenderables(gd, cl, null);
            Profiler.TracyCZoneEnd(ctx);
        });
    }

    /// <summary>
    ///     For a construction that found its geometry not on the GPU yet: constructs again once the geometry's buffer
    ///     is resident. A buffer is only uploaded once it is full or loading pauses, so asking again straight away
    ///     sent every waiting renderable round the upload queue on every pass until then. Asks again straight away if
    ///     there is no buffer to wait on.
    /// </summary>
    protected void ScheduleConstructionWhenResident(VertexIndexBufferAllocator.VertexIndexBufferHandle? geometry)
    {
        if (geometry == null || !geometry.RunWhenResident(ScheduleRenderableConstruction))
        {
            ScheduleRenderableConstruction();
        }
    }

    internal void ScheduleRenderableUpdate()
    {
        // A construction still to run will apply the current state anyway
        if (ConstructionCoversUpdate && IsConstructionQueued)
        {
            return;
        }

        if (Interlocked.Exchange(ref _updateQueued, 1) != 0)
        {
            return;
        }

        SceneRenderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            Volatile.Write(ref _updateQueued, 0);

            Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"Renderable update");
            UpdateRenderables(gd, cl, null);
            Profiler.TracyCZoneEnd(ctx);
        });
    }

    public virtual void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        ScheduleRenderableConstruction();
    }

    public virtual void UnregisterWithScene()
    {
        if (_registered)
        {
            _registered = false;
            DestroyRenderables();
        }
    }

    public virtual void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }
        /*if (Resource != null)
        {
            Resource.Release();
        }
        Resource = null;
        Created = false;
        Submeshes = null;*/
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            UnregisterAndRelease();
            disposedValue = true;
        }
    }

    protected uint GetPackedEntityID(int system, int index)
    {
        return (uint)system << 30 | (uint)index & 0x3FFFFFFF;
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RenderableProxy()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }
}
