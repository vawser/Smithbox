#nullable enable
using StudioCore;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Resource;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene.Framework;

/// <summary>
/// Proxy class that represents a connection between the logical scene
/// heirarchy and the actual underlying renderable representation. Used to handle
/// things like renderable construction, selection, hiding/showing, etc
/// </summary>
public abstract class RenderableProxy : Renderer.IRendererUpdatable, IDisposable
{
    public bool _autoregister = true;

    public bool _registered;
    public bool disposedValue;

    public abstract bool RenderSelectionOutline { set; get; }

    public abstract Matrix4x4 World { get; set; }

    public abstract bool Visible { get; set; }

    public abstract RenderFilter DrawFilter { get; set; }
    public abstract DrawGroup DrawGroups { get; set; }
    public virtual bool AutoRegister { get => _autoregister; set => _autoregister = value; }

    public void Dispose()
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

    internal void ScheduleRenderableConstruction()
    {
        Renderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"Renderable construction");
            ConstructRenderables(gd, cl, null);
            Tracy.TracyCZoneEnd(ctx);
        });
    }

    internal void ScheduleRenderableUpdate()
    {
        Renderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"Renderable update");
            UpdateRenderables(gd, cl, null);
            Tracy.TracyCZoneEnd(ctx);
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
