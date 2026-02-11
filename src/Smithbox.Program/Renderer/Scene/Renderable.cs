using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
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
                $"Renderable index {renderable} is out of range [0, {_topIndex})");
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
    public BoundingBox[] cBounds;
    public bool[] cCulled;
    public MeshDrawParametersComponent[] cDrawParameters;
    public Pipeline[] cPipelines;
    public WeakReference<ISelectable>[] cSelectables;
    public Pipeline[] cSelectionPipelines;

    public MeshRenderables(int id) : base()
    {
        RenderableSystemIndex = id;

        // Initialize mesh-specific arrays
        cBounds = new BoundingBox[Capacity];
        cCulled = new bool[Capacity];
        cDrawParameters = new MeshDrawParametersComponent[Capacity];
        cPipelines = new Pipeline[Capacity];
        cSelectables = new WeakReference<ISelectable>[Capacity];
        cSelectionPipelines = new Pipeline[Capacity];
    }

    protected override void ResizeArrays(int newCapacity)
    {
        base.ResizeArrays(newCapacity);

        // Resize mesh-specific arrays
        Array.Resize(ref cBounds, newCapacity);
        Array.Resize(ref cCulled, newCapacity);
        Array.Resize(ref cDrawParameters, newCapacity);
        Array.Resize(ref cPipelines, newCapacity);
        Array.Resize(ref cSelectables, newCapacity);
        Array.Resize(ref cSelectionPipelines, newCapacity);
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

    public void SubmitRenderables(SceneRenderer.RenderQueue queue)
    {
        for (var i = 0; i < _topIndex; i++)
        {
            if (cVisible[i]._valid && !cCulled[i])
            {
                queue.Add(i, cRenderKeys[i]);
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
                    cBounds[writeIndex] = cBounds[readIndex];
                    cCulled[writeIndex] = cCulled[readIndex];
                    cDrawParameters[writeIndex] = cDrawParameters[readIndex];
                    cPipelines[writeIndex] = cPipelines[readIndex];
                    cSelectables[writeIndex] = cSelectables[readIndex];
                    cSelectionPipelines[writeIndex] = cSelectionPipelines[readIndex];
                }
                writeIndex++;
            }
            readIndex++;
        }

        _topIndex = writeIndex;
        _freeIndices.Clear();
    }
}