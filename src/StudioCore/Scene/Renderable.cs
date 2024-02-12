using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene;

/// <summary>
///     Contains if the component is visible/valid. Valid means that there's an actual
///     renderable in this slot, while visible means it should be rendered
/// </summary>
public struct VisibleValidComponent
{
    public bool _valid;
    public bool _visible;
}

/// <summary>
///     All the components needed by the indirect draw encoder to render a mesh.
///     Batch them all together for locality
/// </summary>
public struct MeshDrawParametersComponent
{
    public Renderer.IndirectDrawIndexedArgumentsPacked _indirectArgs;
    public ResourceSet _objectResourceSet;
    public VkIndexType _indexFormat;
    public int _bufferIndex;
}

public struct SceneVisibilityComponent
{
    public RenderFilter _renderFilter;
    public DrawGroup _drawGroup;
}

/// <summary>
///     Data oriented structure that contains renderables. This is basically a structure
///     of arrays intended of containing all the renderables for a certain mesh. Management
///     of how this is populated is left to a higher level system
/// </summary>
public class Renderables
{
    protected static readonly int SYSTEM_SIZE = CFG.Current.Viewport_Limit_Renderables;

    private readonly Stack<int> _freeIndices = new(100);

    private int _topIndex;
    public RenderKey[] cRenderKeys = new RenderKey[SYSTEM_SIZE];
    public SceneVisibilityComponent[] cSceneVis = new SceneVisibilityComponent[SYSTEM_SIZE];

    /// <summary>
    ///     Component for if the renderable is visible or active
    /// </summary>
    public VisibleValidComponent[] cVisible = new VisibleValidComponent[SYSTEM_SIZE];

    public int RenderableSystemIndex { get; protected set; }

    protected int GetNextInvalidIndex()
    {
        if (_freeIndices.Count > 0)
        {
            return _freeIndices.Pop();
        }

        if (_topIndex >= SYSTEM_SIZE)
        {
            throw new Exception("Renderable system full.\n\nTry increasing renderables limit in settings.\n");
        }

        return _topIndex++;
    }

    protected int AllocateValidAndVisibleRenderable()
    {
        var next = GetNextInvalidIndex();
        cVisible[next]._valid = true;
        cVisible[next]._visible = true;
        cRenderKeys[next] = new RenderKey(0);
        cSceneVis[next]._renderFilter = RenderFilter.All;
        cSceneVis[next]._drawGroup = new DrawGroup();
        return next;
    }

    public void RemoveRenderable(int renderable)
    {
        cVisible[renderable]._valid = false;
        _freeIndices.Push(renderable);
    }
}

/// <summary>
///     Structure for mesh renderables and all the information needed to render a single static mesh
/// </summary>
public class MeshRenderables : Renderables
{
    public BoundingBox[] cBounds = new BoundingBox[SYSTEM_SIZE];
    public bool[] cCulled = new bool[SYSTEM_SIZE];
    public MeshDrawParametersComponent[] cDrawParameters = new MeshDrawParametersComponent[SYSTEM_SIZE];
    public Pipeline[] cPipelines = new Pipeline[SYSTEM_SIZE];
    public WeakReference<ISelectable>[] cSelectables = new WeakReference<ISelectable>[SYSTEM_SIZE];

    public Pipeline[] cSelectionPipelines = new Pipeline[SYSTEM_SIZE];

    public MeshRenderables(int id)
    {
        RenderableSystemIndex = id;
    }

    public int CreateMesh(ref BoundingBox bounds, ref MeshDrawParametersComponent drawArgs)
    {
        var idx = AllocateValidAndVisibleRenderable();
        cBounds[idx] = bounds;
        cDrawParameters[idx] = drawArgs;
        return idx;
    }

    public void CullRenderables(BoundingFrustum frustum)
    {
        for (var i = 0; i < SYSTEM_SIZE; i++)
        {
            if (!cVisible[i]._valid)
            {
                continue;
            }

            ContainmentType intersect = frustum.Contains(ref cBounds[i]);
            if (!CFG.Current.Viewport_Frustum_Culling)
            {
                cCulled[i] = !cVisible[i]._valid || !cVisible[i]._visible;
            }
            else if (intersect == ContainmentType.Contains || intersect == ContainmentType.Intersects)
            {
                cCulled[i] = !cVisible[i]._valid || !cVisible[i]._visible;
            }
            else
            {
                cCulled[i] = true;
            }
        }
    }

    public void ProcessSceneVisibility(RenderFilter filter, DrawGroup dispGroup)
    {
        var alwaysVis = dispGroup != null ? dispGroup.AlwaysVisible : true;
        for (var i = 0; i < SYSTEM_SIZE; i++)
        {
            if (cCulled[i])
            {
                continue;
            }

            if ((cSceneVis[i]._renderFilter & filter) == 0)
            {
                cCulled[i] = true;
                continue;
            }

            if (!alwaysVis && cSceneVis[i]._drawGroup != null &&
                !cSceneVis[i]._drawGroup.IsInDisplayGroup(dispGroup))
            {
                cCulled[i] = true;
            }
        }
    }

    public void SubmitRenderables(Renderer.RenderQueue queue)
    {
        for (var i = 0; i < SYSTEM_SIZE; i++)
        {
            if (cVisible[i]._valid && !cCulled[i])
            {
                queue.Add(i, cRenderKeys[i]);
            }
        }
    }
}
