using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public unsafe class FlverSubmeshProvider : MeshProvider
{
    private static readonly BlendStateDescription SubtractBlend = new(RgbaFloat.Black, new BlendAttachmentDescription(
        true,
        VkBlendFactor.SrcAlpha, VkBlendFactor.One, VkBlendOp.ReverseSubtract,
        VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add));

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
        && _resource.Get().GPUMeshes[_meshIndex].Material.Definition?.CullNone != true
            ? VkCullModeFlags.Back
            : VkCullModeFlags.None;

    public override BlendStateDescription BlendState =>
        _resource.Get().GPUMeshes[_meshIndex].Material.Blending switch
        {
            MaterialBlending.Alpha => BlendStateDescription.SingleAlphaBlend,
            MaterialBlending.Add => BlendStateDescription.SingleAdditiveBlend,
            MaterialBlending.Subtract => SubtractBlend,
            _ => BlendStateDescription.SingleOverrideBlend
        };

    public override bool DepthWrite => _resource.Get().GPUMeshes[_meshIndex].Material.DepthWrite;

    public override bool IsTransparent =>
        _resource.Get().GPUMeshes[_meshIndex].Material.Blending != MaterialBlending.Opaque;

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

    public string MeshName => _resource.Name;

    /// <summary>
    ///     Index of this mesh within its FLVER.
    /// </summary>
    public int MeshIndex => _meshIndex;

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
