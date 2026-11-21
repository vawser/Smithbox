using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class HavokNavmeshCostGraphProvider : MeshProvider, IResourceEventListener
{
    private readonly ResourceHandle<HavokNavmeshResource> _source;
    private int _referenceCount;
    private ResourceHandle<HavokNavmeshResource> _resource;

    public HavokNavmeshCostGraphProvider(ResourceHandle<HavokNavmeshResource> source)
    {
        _source = source;
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
        lock (StateLock)
        {
            if (_resource != null)
            {
                return;
            }

            _resource = (ResourceHandle<HavokNavmeshResource>)handle;
            _resource.Acquire();
        }

        NotifyAvailable();

        MeshProviderInspector.Add(_source.FullName, this);
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        lock (StateLock)
        {
            _resource?.Release();
            _resource = null;
        }

        NotifyUnavailable();

        MeshProviderInspector.Remove(_source.FullName);
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
        lock (StateLock)
        {
            _referenceCount--;
            if (_referenceCount > 0)
            {
                return;
            }

            _referenceCount = 0;

            // Dropped inside the lock, so it can't undo a load that a new Acquire started meanwhile
            _resource?.Release();
            _resource = null;
        }

        NotifyUnavailable();

        MeshProviderInspector.Remove(_source.FullName);
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
