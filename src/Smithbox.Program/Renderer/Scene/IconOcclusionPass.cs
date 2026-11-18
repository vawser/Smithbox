using System;
using System.Numerics;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>The answer to an <see cref="IconOcclusionPass" /> request</summary>
public sealed class IconOcclusionResult
{
    /// <summary>The caller's own data, handed in with the request</summary>
    public object Tag;

    /// <summary>For each point of the request, whether the scene is not in front of it</summary>
    public bool[] Visible;
}

/// <summary>
///     Tests screen-space icons drawn over a viewport (the Map Editor's region icons) against the scene's depth, so
///     that what stands in front of them hides them as it would a mesh. Each point is tested by IconOcclusion.comp
///     against <see cref="SceneRenderPipeline.SceneDepth" />, the copy taken after the opaque draws, and the results
///     are read back. One request is in flight at a time, so the answer is a few frames behind the camera.
/// </summary>
public sealed class IconOcclusionPass
{
    /// <summary>The most points one request tests</summary>
    public const int MaxPoints = 4096;

    private const uint GroupSize = 64;

    private readonly SceneRenderPipeline Pipeline;

    private ResourceLayout Layout;
    private ComputePass Compute;
    private ResourceSet Set;
    private Texture SetTexture;
    private DeviceBuffer PointBuffer;
    private DeviceBuffer ResultBuffer;
    private DeviceBuffer ReadbackBuffer;

    private readonly object Lock = new();

    // The latest request waiting to be tested: the count header, then the points
    private readonly Vector4[] PendingPoints = new Vector4[MaxPoints + 1];
    private int PendingCount;
    private object PendingTag;
    private bool HasPending;

    // The points being tested, as uploaded
    private readonly Vector4[] ActivePoints = new Vector4[MaxPoints + 1];

    private volatile bool Reading;

    /// <summary>The last result read back, or null. Set on the render thread when its readback finishes.</summary>
    public IconOcclusionResult LatestResult { get; private set; }

    public IconOcclusionPass(SceneRenderPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    /// <summary>
    ///     Asks for the points to be tested on the next frame drawn. Each point is the position in the viewport (0 to
    ///     1 across and down), the point's window depth (reversed, as the scene's), and how far from the centre, in
    ///     texels, the four other taps are. A request not yet started is replaced.
    /// </summary>
    public void Request(object tag, ReadOnlySpan<Vector4> points)
    {
        lock (Lock)
        {
            var count = Math.Min(points.Length, MaxPoints);

            points[..count].CopyTo(PendingPoints.AsSpan(1));
            PendingCount = count;
            PendingTag = tag;
            HasPending = true;
        }
    }

    /// <summary>
    ///     Tests the pending request against this frame's scene depth. Called on the draw list once the scene has been
    ///     drawn, outside a render pass.
    /// </summary>
    public void Run(GraphicsDevice gd, CommandList cl)
    {
        if (Reading)
        {
            return;
        }

        var depth = Pipeline.SceneDepth.Texture;

        if (depth == null)
        {
            return;
        }

        int count;
        object tag;

        lock (Lock)
        {
            if (!HasPending)
            {
                return;
            }

            count = PendingCount;
            tag = PendingTag;
            HasPending = false;

            PendingPoints.AsSpan(1, count).CopyTo(ActivePoints.AsSpan(1));
        }

        if (count == 0)
        {
            LatestResult = new IconOcclusionResult { Tag = tag, Visible = Array.Empty<bool>() };
            return;
        }

        EnsureResources(gd, depth);

        ActivePoints[0] = new Vector4(BitConverter.Int32BitsToSingle(count), 0, 0, 0);

        // Copied through a staging buffer, which also ends any render pass, so the dispatch is outside one
        cl.UpdateBuffer(PointBuffer, 0, ActivePoints);

        // The points' copy has to land, and so does the scene depth's
        cl.Barrier(VkPipelineStageFlags2.Transfer | VkPipelineStageFlags2.LateFragmentTests,
            VkAccessFlags2.TransferWrite | VkAccessFlags2.DepthStencilAttachmentWrite,
            VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderRead | VkAccessFlags2.ShaderWrite);

        Compute.Dispatch(gd, cl, new[] { Set }, (uint)count);

        cl.Barrier(VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderWrite,
            VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferRead);

        Reading = true;

        SceneRenderer.AddAsyncReadback(ReadbackBuffer, ResultBuffer, d =>
        {
            var visible = new bool[count];

            unsafe
            {
                MappedResourceView<uint> mapped = d.Map<uint>(ReadbackBuffer, MapMode.Read);
                var bits = new Span<uint>(mapped.MappedResource.Data.ToPointer(), MaxPoints);

                for (var i = 0; i < count; i++)
                {
                    visible[i] = bits[i] != 0;
                }
            }

            LatestResult = new IconOcclusionResult { Tag = tag, Visible = visible };
            Reading = false;
        });
    }

    private void EnsureResources(GraphicsDevice gd, Texture depth)
    {
        ResourceFactory factory = gd.ResourceFactory;

        if (Compute == null)
        {
            Layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SceneDepth", VkDescriptorType.SampledImage, VkShaderStageFlags.Compute),
                new ResourceLayoutElementDescription("DepthSampler", VkDescriptorType.Sampler, VkShaderStageFlags.Compute),
                new ResourceLayoutElementDescription("IconPoints", VkDescriptorType.StorageBuffer, VkShaderStageFlags.Compute),
                new ResourceLayoutElementDescription("IconResults", VkDescriptorType.StorageBuffer, VkShaderStageFlags.Compute)));

            Compute = new ComputePass("IconOcclusion", new[] { Layout }, GroupSize);

            var pointSize = (uint)(16 * (MaxPoints + 1));
            PointBuffer = factory.CreateBuffer(new BufferDescription(pointSize,
                VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0, 16));
            PointBuffer.Name = "Icon Occlusion Points";

            ResultBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * MaxPoints,
                VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto, 0, sizeof(uint)));
            ResultBuffer.Name = "Icon Occlusion Results";

            ReadbackBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * MaxPoints,
                VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, VmaAllocationCreateFlags.Mapped));
            ReadbackBuffer.Name = "Icon Occlusion Readback";
        }

        // The depth copy is made again when the viewport is resized
        if (Set == null || SetTexture != depth)
        {
            if (Set != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(Set);
            }

            Set = factory.CreateResourceSet(new ResourceSetDescription(Layout, depth, gd.PointSampler,
                PointBuffer, ResultBuffer));
            SetTexture = depth;
        }
    }

    /// <summary>
    ///     Releases the buffers, layout and set once frames in flight are done with them. A test still being read back
    ///     never completes.
    /// </summary>
    public void Dispose()
    {
        if (Set != null)
            SceneRenderer.DisposeAllAfterFramesInFlight(Set);

        if (PointBuffer != null)
            SceneRenderer.DisposeAllAfterFramesInFlight(PointBuffer);

        // The result is read back, and a readback can still be queued, so these are kept longer
        if (ResultBuffer != null)
            SceneRenderer.DisposeAfterFramesInFlight(ResultBuffer, SceneRenderer.ReadbackReleaseFrames);

        if (ReadbackBuffer != null)
            SceneRenderer.DisposeAfterFramesInFlight(ReadbackBuffer, SceneRenderer.ReadbackReleaseFrames);

        // After the set made from it
        if (Layout != null)
            SceneRenderer.DisposeAfterFramesInFlight(Layout, SceneRenderer.ReadbackReleaseFrames);

        Set = null;
        SetTexture = null;
        PointBuffer = null;
        ResultBuffer = null;
        ReadbackBuffer = null;
        Layout = null;
    }
}
