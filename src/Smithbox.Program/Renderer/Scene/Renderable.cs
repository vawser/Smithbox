using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
/// Contains if the component is visible/valid. Valid means that there's an actual
/// renderable in this slot, while visible means it should be rendered
/// </summary>
public struct VisibleValidComponent
{
    public bool _valid;
    public bool _visible;
}

/// <summary>
/// All the components needed by the indirect draw encoder to render a mesh.
/// Batch them all together for locality
/// </summary>
public struct MeshDrawParametersComponent
{
    public SceneRenderer.IndirectDrawIndexedArgumentsPacked _indirectArgs;
    // The buffer holding the per-object data, and the layout it is read in. Its resource set is looked up at draw
    // time, as the buffer is replaced when it grows.
    public GPUBufferAllocator _objectBuffer;
    public ResourceLayout _objectLayout;
    public VkIndexType _indexFormat;
    public int _bufferIndex;
}

public struct SceneVisibilityComponent
{
    public RenderFilter _renderFilter;
    public DrawGroup _drawGroup;
}

/// <summary>
/// Data oriented structure that contains renderables. This is basically a structure
/// of arrays intended of containing all the renderables for a certain mesh. Management
/// of how this is populated is left to a higher level system
/// </summary>
public class Renderables
{
    private const int INITIAL_SIZE = 128;
    private const int GROWTH_FACTOR = 2;

    public readonly Stack<int> _freeIndices = new(100);

    public int _topIndex;
    public int _capacity;

    public RenderKey[] cRenderKeys;
    public SceneVisibilityComponent[] cSceneVis;

    /// <summary>
    /// Component for if the renderable is visible or active
    /// </summary>
    public VisibleValidComponent[] cVisible;

    public int RenderableSystemIndex { get; protected set; }

    /// <summary>
    /// Current capacity of the renderable arrays
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Number of active renderables (valid slots)
    /// </summary>
    public int Count => _topIndex - _freeIndices.Count;

    protected Renderables()
    {
        _capacity = INITIAL_SIZE;
        cRenderKeys = new RenderKey[_capacity];
        cSceneVis = new SceneVisibilityComponent[_capacity];
        cVisible = new VisibleValidComponent[_capacity];
    }

    /// <summary>
    /// Ensures the arrays have enough capacity. If not, resizes them.
    /// </summary>
    protected virtual void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _capacity)
        {
            return;
        }

        int newCapacity = _capacity;
        while (newCapacity < requiredCapacity)
        {
            newCapacity *= GROWTH_FACTOR;
        }

        ResizeArrays(newCapacity);
    }

    /// <summary>
    ///     Resizes all arrays to the new capacity
    /// </summary>
    protected virtual void ResizeArrays(int newCapacity)
    {
        Array.Resize(ref cRenderKeys, newCapacity);
        Array.Resize(ref cSceneVis, newCapacity);
        Array.Resize(ref cVisible, newCapacity);

        _capacity = newCapacity;
    }

    protected int GetNextInvalidIndex()
    {
        if (_freeIndices.Count > 0)
        {
            return _freeIndices.Pop();
        }

        // Ensure we have capacity for the new index
        EnsureCapacity(_topIndex + 1);

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
        if (renderable < 0 || renderable >= _topIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(renderable),
                LOC.Get("REND_Renderable_Out_of_Index_Range", renderable, _topIndex));
        }

        cVisible[renderable]._valid = false;
        _freeIndices.Push(renderable);
    }

    /// <summary>
    /// Compacts the arrays by removing gaps from freed indices.
    /// This is an expensive operation and should be called sparingly.
    /// </summary>
    public virtual void Compact()
    {
        if (_freeIndices.Count == 0)
        {
            return;
        }

        // Sort free indices in descending order
        var sortedFreeIndices = new List<int>(_freeIndices);
        sortedFreeIndices.Sort((a, b) => b.CompareTo(a));

        int writeIndex = 0;
        int readIndex = 0;
        var indexMapping = new Dictionary<int, int>();

        // Compact valid entries
        while (readIndex < _topIndex)
        {
            if (cVisible[readIndex]._valid)
            {
                if (writeIndex != readIndex)
                {
                    cVisible[writeIndex] = cVisible[readIndex];
                    cRenderKeys[writeIndex] = cRenderKeys[readIndex];
                    cSceneVis[writeIndex] = cSceneVis[readIndex];

                    indexMapping[readIndex] = writeIndex;
                }
                writeIndex++;
            }
            readIndex++;
        }

        _topIndex = writeIndex;
        _freeIndices.Clear();
    }
}

/// <summary>
///     Structure for mesh renderables and all the information needed to render a single static mesh
/// </summary>
public class MeshRenderables : Renderables
{
    public bool[] cBackToFront;
    public BoundingBox[] cBounds;
    public bool[] cCulled;
    public MeshDrawParametersComponent[] cDrawParameters;
    public Pipeline[] cPipelines;
    public WeakReference<ISelectable>[] cSelectables;
    public Pipeline[] cSelectionPipelines;

    /// <summary>The pipeline that draws the renderable into the sun's shadow map, or null if it casts no shadow</summary>
    public Pipeline[] cShadowPipelines;

    /// <summary>
    ///     The depth-only pipeline of the renderable (its shadow map pipeline), whether or not it casts the sun's
    ///     shadow, for depth views of the scene such as <see cref="SnapDepthPass" />. Null if it has none: only opaque
    ///     meshes drawn with the flver shader do.
    /// </summary>
    public Pipeline[] cDepthPipelines;

    /// <summary>
    ///     The pipeline that draws the renderable in the wireframe view (CFG.Viewport_Wireframe_Enable): its _wire shader
    ///     variant, with a geometry stage. Null for one drawn as usual there. Built the first time the view needs it, from
    ///     <see cref="cWireframeDescriptions" />, so meshes cost nothing extra to load while the view is off.
    /// </summary>
    public Pipeline[] cWireframePipelines;

    /// <summary>What <see cref="cWireframePipelines" /> is built from, while <see cref="cWireframePending" /></summary>
    public GraphicsPipelineDescription[] cWireframeDescriptions;

    public bool[] cWireframePending;

    /// <summary>Left out of the wireframe view, as the selection outline is: its lines show the selection there</summary>
    public bool[] cHiddenInWireframe;

    // Whether any wireframe pipeline is still to be built, so the view only looks for them after a mesh is added
    private bool _anyWireframePending;

    public MeshRenderables(int id) : base()
    {
        RenderableSystemIndex = id;

        // Initialize mesh-specific arrays
        cBackToFront = new bool[Capacity];
        cBounds = new BoundingBox[Capacity];
        cCulled = new bool[Capacity];
        cDrawParameters = new MeshDrawParametersComponent[Capacity];
        cPipelines = new Pipeline[Capacity];
        cSelectables = new WeakReference<ISelectable>[Capacity];
        cSelectionPipelines = new Pipeline[Capacity];
        cShadowPipelines = new Pipeline[Capacity];
        cDepthPipelines = new Pipeline[Capacity];
        cWireframePipelines = new Pipeline[Capacity];
        cWireframeDescriptions = new GraphicsPipelineDescription[Capacity];
        cWireframePending = new bool[Capacity];
        cHiddenInWireframe = new bool[Capacity];
    }

    protected override void ResizeArrays(int newCapacity)
    {
        base.ResizeArrays(newCapacity);

        // Resize mesh-specific arrays
        Array.Resize(ref cBackToFront, newCapacity);
        Array.Resize(ref cBounds, newCapacity);
        Array.Resize(ref cCulled, newCapacity);
        Array.Resize(ref cDrawParameters, newCapacity);
        Array.Resize(ref cPipelines, newCapacity);
        Array.Resize(ref cSelectables, newCapacity);
        Array.Resize(ref cSelectionPipelines, newCapacity);
        Array.Resize(ref cShadowPipelines, newCapacity);
        Array.Resize(ref cDepthPipelines, newCapacity);
        Array.Resize(ref cWireframePipelines, newCapacity);
        Array.Resize(ref cWireframeDescriptions, newCapacity);
        Array.Resize(ref cWireframePending, newCapacity);
        Array.Resize(ref cHiddenInWireframe, newCapacity);
    }

    public int CreateMesh(ref BoundingBox bounds, ref MeshDrawParametersComponent drawArgs)
    {
        var idx = AllocateValidAndVisibleRenderable();
        cBackToFront[idx] = false;
        cBounds[idx] = bounds;
        cDrawParameters[idx] = drawArgs;
        cShadowPipelines[idx] = null;
        cDepthPipelines[idx] = null;
        cWireframePipelines[idx] = null;
        cWireframeDescriptions[idx] = default;
        cWireframePending[idx] = false;
        cHiddenInWireframe[idx] = false;
        return idx;
    }

    /// <summary>Gives a renderable the pipeline it draws with in the wireframe view, built when the view first needs it</summary>
    public void SetWireframePipeline(int idx, ref GraphicsPipelineDescription description)
    {
        cWireframeDescriptions[idx] = description;
        cWireframePipelines[idx] = null;
        cWireframePending[idx] = true;
        _anyWireframePending = true;
    }

    /// <summary>
    ///     Builds the wireframe pipelines not built yet, for the wireframe view. Pipelines are shared between renderables
    ///     with the same description, so a scene builds only as many as it has kinds of material.
    /// </summary>
    public void BuildWireframePipelines(ResourceFactory factory)
    {
        if (!_anyWireframePending)
        {
            return;
        }

        _anyWireframePending = false;

        for (var i = 0; i < _topIndex; i++)
        {
            if (!cWireframePending[i])
            {
                continue;
            }

            cWireframePending[i] = false;

            if (!cVisible[i]._valid)
            {
                continue;
            }

            cWireframePipelines[i] = StaticResourceCache.GetPipeline(factory, ref cWireframeDescriptions[i]);
            cWireframeDescriptions[i] = default;
        }
    }

    /// <summary>
    ///     Adds the shadow casters inside <paramref name="frustum" /> to a shadow cascade's queue. It is culled on its
    ///     own, leaving <see cref="cCulled" /> as the view's culling set it, and keeps to the same filters and draw
    ///     groups, so what is hidden casts no shadow.
    /// </summary>
    public void SubmitShadowCasters(SceneRenderer.RenderQueue queue, BoundingFrustum frustum, RenderFilter filter,
        DrawGroup dispGroup)
    {
        var alwaysVis = dispGroup == null || dispGroup.AlwaysVisible;

        for (var i = 0; i < _topIndex; i++)
        {
            if (!cVisible[i]._valid || !cVisible[i]._visible || cShadowPipelines[i] == null)
            {
                continue;
            }

            if ((cSceneVis[i]._renderFilter & filter) == 0)
            {
                continue;
            }

            if (!alwaysVis && cSceneVis[i]._drawGroup != null && !cSceneVis[i]._drawGroup.IsInDisplayGroup(dispGroup))
            {
                continue;
            }

            if (CFG.Current.Viewport_Enable_Culling && frustum.Contains(ref cBounds[i]) == ContainmentType.Disjoint)
            {
                continue;
            }

            queue.Add(i, cRenderKeys[i]);
        }
    }

    /// <summary>
    ///     Adds the renderables with a depth pipeline inside <paramref name="frustum" /> to a depth view's queue, keeping
    ///     to the same filters and draw groups as the view, so only what is shown is drawn. Renderables whose selectable
    ///     is in <paramref name="excluded" /> are left out.
    /// </summary>
    public void SubmitDepthCasters(SceneRenderer.RenderQueue queue, BoundingFrustum frustum, RenderFilter filter,
        DrawGroup dispGroup, IReadOnlySet<ISelectable> excluded)
    {
        var alwaysVis = dispGroup == null || dispGroup.AlwaysVisible;

        for (var i = 0; i < _topIndex; i++)
        {
            if (!cVisible[i]._valid || !cVisible[i]._visible || cDepthPipelines[i] == null)
            {
                continue;
            }

            if ((cSceneVis[i]._renderFilter & filter) == 0)
            {
                continue;
            }

            if (!alwaysVis && cSceneVis[i]._drawGroup != null && !cSceneVis[i]._drawGroup.IsInDisplayGroup(dispGroup))
            {
                continue;
            }

            if (frustum.Contains(ref cBounds[i]) == ContainmentType.Disjoint)
            {
                continue;
            }

            if (excluded != null && excluded.Count > 0 && cSelectables[i] != null &&
                cSelectables[i].TryGetTarget(out ISelectable selectable) && excluded.Contains(selectable))
            {
                continue;
            }

            queue.Add(i, cRenderKeys[i]);
        }
    }

    /// <summary>
    ///     A hash of the shadow casters <see cref="SubmitShadowCasters" /> would take without culling, and their
    ///     bounds: it changes when one is added, removed, shown, hidden or moved. <see cref="RainOcclusion" /> draws
    ///     its map again when it does.
    /// </summary>
    public int ShadowCasterSignature(RenderFilter filter, DrawGroup dispGroup)
    {
        var alwaysVis = dispGroup == null || dispGroup.AlwaysVisible;
        HashCode hash = new();

        for (var i = 0; i < _topIndex; i++)
        {
            if (!cVisible[i]._valid || !cVisible[i]._visible || cShadowPipelines[i] == null)
            {
                continue;
            }

            if ((cSceneVis[i]._renderFilter & filter) == 0)
            {
                continue;
            }

            if (!alwaysVis && cSceneVis[i]._drawGroup != null && !cSceneVis[i]._drawGroup.IsInDisplayGroup(dispGroup))
            {
                continue;
            }

            hash.Add(i);
            hash.Add(cBounds[i].Min);
            hash.Add(cBounds[i].Max);
        }

        return hash.ToHashCode();
    }

    public void CullRenderables(BoundingFrustum frustum)
    {
        for (var i = 0; i < _topIndex; i++)
        {
            if (!cVisible[i]._valid)
            {
                continue;
            }

            ContainmentType intersect = frustum.Contains(ref cBounds[i]);
            if (!CFG.Current.Viewport_Enable_Culling)
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
        for (var i = 0; i < _topIndex; i++)
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

    public void SubmitRenderables(SceneRenderer.RenderQueue queue, Vector3 eye)
    {
        for (var i = 0; i < _topIndex; i++)
        {
            if (cVisible[i]._valid && !cCulled[i])
            {
                // These depend on where the camera is, so can't be stored
                var key = cBackToFront[i]
                    ? RenderKey.CreateBackToFront(Vector3.Distance(eye, cBounds[i].GetCenter()))
                    : cRenderKeys[i];

                queue.Add(i, key);
            }
        }
    }

    public override void Compact()
    {
        if (_freeIndices.Count == 0)
        {
            return;
        }

        int writeIndex = 0;
        int readIndex = 0;

        // Compact valid entries
        while (readIndex < _topIndex)
        {
            if (cVisible[readIndex]._valid)
            {
                if (writeIndex != readIndex)
                {
                    cVisible[writeIndex] = cVisible[readIndex];
                    cRenderKeys[writeIndex] = cRenderKeys[readIndex];
                    cSceneVis[writeIndex] = cSceneVis[readIndex];
                    cBackToFront[writeIndex] = cBackToFront[readIndex];
                    cBounds[writeIndex] = cBounds[readIndex];
                    cCulled[writeIndex] = cCulled[readIndex];
                    cDrawParameters[writeIndex] = cDrawParameters[readIndex];
                    cPipelines[writeIndex] = cPipelines[readIndex];
                    cSelectables[writeIndex] = cSelectables[readIndex];
                    cSelectionPipelines[writeIndex] = cSelectionPipelines[readIndex];
                    cShadowPipelines[writeIndex] = cShadowPipelines[readIndex];
                    cDepthPipelines[writeIndex] = cDepthPipelines[readIndex];
                    cWireframePipelines[writeIndex] = cWireframePipelines[readIndex];
                    cWireframeDescriptions[writeIndex] = cWireframeDescriptions[readIndex];
                    cWireframePending[writeIndex] = cWireframePending[readIndex];
                    cHiddenInWireframe[writeIndex] = cHiddenInWireframe[readIndex];
                }
                writeIndex++;
            }
            readIndex++;
        }

        _topIndex = writeIndex;
        _freeIndices.Clear();
    }
}