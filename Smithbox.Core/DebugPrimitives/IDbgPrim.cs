using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Drawing;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.DebugPrimitives;

public interface IDbgPrim : IDisposable
{
    string Name { get; set; }
    Color NameColor { get; set; }

    DbgPrimCategory Category { get; set; }

    /// <summary>
    ///     Underlying layout type of the mesh data
    /// </summary>
    public MeshLayoutType LayoutType { get; }

    public VertexLayoutDescription LayoutDescription { get; }

    public BoundingBox Bounds { get; }

    /// <summary>
    ///     Get handle to the GPU allocated geometry
    /// </summary>
    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeometryBuffer { get; }

    // Pipeline state
    public string ShaderName { get; }

    public SpecializationConstant[] SpecializationConstants { get; }

    public VkCullModeFlags CullMode { get; }

    public VkPolygonMode FillMode { get; }

    public VkFrontFace FrontFace => VkFrontFace.CounterClockwise;

    public VkPrimitiveTopology Topology { get; }

    // Mesh data
    public bool Is32Bit => false;

    public int IndexOffset => 0;

    public int IndexCount { get; }

    public uint VertexSize { get; }
}
