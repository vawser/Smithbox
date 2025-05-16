#nullable enable
using StudioCore;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Resource;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Helpers;
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

public class SkeletonBoneRenderableProxy : RenderableProxy
{
    /// <summary>
    ///     Renderable for the actual bone
    /// </summary>
    private readonly DebugPrimitiveRenderableProxy _bonePointRenderable;

    /// <summary>
    ///     Renderables for the bones to child joints
    /// </summary>
    private readonly List<DebugPrimitiveRenderableProxy> _boneRenderables = new();

    /// <summary>
    ///     Child renderables that this bone is connected to
    /// </summary>
    private List<SkeletonBoneRenderableProxy> _childBones = new();

    private RenderFilter _drawfilter = RenderFilter.All;

    private DrawGroup _drawgroups = new();

    private bool _renderOutline;

    private WeakReference<ISelectable>? _selectable;
    private bool _visible = true;

    private Matrix4x4 _world = Matrix4x4.Identity;

    public SkeletonBoneRenderableProxy(RenderScene scene)
    {
        _bonePointRenderable = RenderableHelper.GetBonePointProxy(scene);
        ScheduleRenderableConstruction();
        AutoRegister = true;
        _registered = true;
    }

    public override bool AutoRegister
    {
        get => _autoregister;
        set
        {
            _autoregister = value;
            _bonePointRenderable.AutoRegister = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.AutoRegister = value;
            }
        }
    }

    public override bool RenderSelectionOutline
    {
        get => _renderOutline;
        set
        {
            _renderOutline = value;
            _bonePointRenderable.RenderSelectionOutline = _renderOutline;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.RenderSelectionOutline = value;
            }
        }
    }

    public override Matrix4x4 World
    {
        get => _world;
        set
        {
            _world = value;
            ScheduleRenderableUpdate();
            _bonePointRenderable.World = _world;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.World = _world;
            }
        }
    }

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            _bonePointRenderable.Visible = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.Visible = _visible;
            }
        }
    }

    public override RenderFilter DrawFilter
    {
        get => _drawfilter;
        set
        {
            _drawfilter = value;
            _bonePointRenderable.DrawFilter = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.DrawFilter = _drawfilter;
            }
        }
    }

    public override DrawGroup DrawGroups
    {
        get => _drawgroups;
        set
        {
            _drawgroups = value;
            _bonePointRenderable.DrawGroups = _drawgroups;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.DrawGroups = _drawgroups;
            }
        }
    }

    public override void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        _bonePointRenderable.ScheduleRenderableConstruction();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.ScheduleRenderableConstruction();
        }
    }

    public override void DestroyRenderables()
    {
        _bonePointRenderable.DestroyRenderables();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.DestroyRenderables();
        }
    }

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }
    public override BoundingBox GetFramingBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }
    public override BoundingBox GetLocalBounds()
    {
        BoundingBox b = _bonePointRenderable.GetLocalBounds();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            b = BoundingBox.Combine(b, c.GetLocalBounds());
        }

        return b;
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        _bonePointRenderable.SetSelectable(sel);
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.SetSelectable(sel);
        }
    }

    public override void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
    }
}
