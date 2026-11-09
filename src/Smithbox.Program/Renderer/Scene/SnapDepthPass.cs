using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>A request to <see cref="SnapDepthPass" />: where groups of the selection would land along a direction.</summary>
public sealed class SnapDepthRequest
{
    /// <summary>The unit world direction the groups move along</summary>
    public Vector3 Direction;

    /// <summary>How far past its leading face a group looks for a surface</summary>
    public float MaxDistance;

    /// <summary>The world bounds of each group</summary>
    public List<BoundingBox> Groups = new();

    /// <summary>Selectables whose meshes aren't drawn: the selection itself, so it doesn't land on itself</summary>
    public IReadOnlySet<ISelectable> Excluded;

    /// <summary>The caller's own data, handed back with the result</summary>
    public object Tag;
}

/// <summary>The answer to a <see cref="SnapDepthRequest" /></summary>
public sealed class SnapDepthResult
{
    public SnapDepthRequest Request;

    /// <summary>
    ///     For each group, where along the direction (as a dot product with it) the nearest surface past the group's
    ///     centre plane lies, or null if there is none within range.
    /// </summary>
    public float?[] SurfacePlanes;
}

/// <summary>
///     Finds where the Map Editor's Snap to Mesh lands, for one <see cref="SceneRenderPipeline" />: the scene's depth
///     drawn orthographically along the snap direction over the selection's footprint, with the selection left out,
///     then reduced per group by SnapDepth.comp and read back. See Documentation/Claude/Snap To Mesh GPU.md.
///     <para>
///         As <see cref="RainOcclusion" />, the meshes are drawn with their depth pipelines, and a queue draws what was
///         put in it a frame later than it was put there (or in the same frame), so a request puts the meshes in for
///         three frames and clears and draws the map in the middle two. On the fourth the map holds the request's
///         view, and it is reduced before anything is drawn. Draws on the frames after go to a 1 x 1 sink, as binding
///         the map at all would leave it holding something else.
///     </para>
/// </summary>
public class SnapDepthPass
{
    /// <summary>The map's side in texels</summary>
    public const uint Resolution = 512;

    /// <summary>The most groups one request reduces</summary>
    public const int MaxGroups = 256;

    // Frames a request puts the meshes in the queue for
    private const int SubmitFrames = 3;

    [StructLayout(LayoutKind.Sequential)]
    private struct GpuGroup
    {
        public int X0, Y0, X1, Y1;
        public Vector4 Limit;
    }

    private readonly GraphicsDevice Device;
    private readonly SceneRenderer.RenderQueue Queue;
    private readonly DeviceBuffer ParamBuffer;

    private Texture Map;
    private Framebuffer MapFramebuffer;
    private Texture Sink;
    private Framebuffer SinkFramebuffer;

    private ResourceLayout ComputeLayout;
    private ComputePass Compute;
    private ResourceSet ComputeSet;
    private DeviceBuffer GroupBuffer;
    private DeviceBuffer ResultBuffer;
    private DeviceBuffer ReadbackBuffer;

    private readonly GpuGroup[] Groups = new GpuGroup[MaxGroups];
    private readonly uint[] Zeros = new uint[MaxGroups];

    // The latest request waiting to be drawn, which replaces any older one
    private SnapDepthRequest Pending;

    // The request being drawn, its view, and what turns a group's depth back into a distance
    private SnapDepthRequest Active;
    private Matrix4x4 View;
    private Matrix4x4 Projection;
    private float NearPlane;
    private float Range;
    private int GroupCount;

    // Which frame of the request this is, from 0, or -1 for none; and whether its result is being read back
    private int Phase = -1;
    private bool Reading;

    // Frames left in which the queue can still hold meshes from a request
    private int StrayFrames;

    // Set by Update for this frame
    private bool SubmitThisFrame;
    private bool ClearThisFrame;
    private bool ReduceThisFrame;

    /// <summary>The last result read back, or null. Set on the render thread when its readback finishes.</summary>
    public SnapDepthResult LatestResult { get; private set; }

    /// <summary>True while no request is being drawn or read back, so a new one starts on the next frame</summary>
    public bool IsIdle => Phase < 0 && !Reading;

    public unsafe SnapDepthPass(GraphicsDevice device, SceneRenderPipeline pipeline)
    {
        Device = device;

        ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(device.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);

        ParamBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(SceneParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        Queue = new SceneRenderer.RenderQueue("Snap Depth", device, pipeline)
        {
            IsOffscreen = true,
            SceneParamSet = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(sceneParamLayout, ParamBuffer, pipeline.SceneLightingBuffer))
        };
        Queue.SetPredrawSetupAction((d, cl) => Begin(cl));
        SceneRenderer.RegisterRenderQueue(Queue);
    }

    /// <summary>Asks for a snap. It starts once the pass is idle; a request not yet started is replaced.</summary>
    public void Request(SnapDepthRequest request)
    {
        Pending = request;
    }

    /// <summary>Moves the request on by a frame. Called once a frame, before the scene is submitted and uploaded.</summary>
    public void Update()
    {
        StrayFrames = Math.Max(StrayFrames - 1, 0);
        SubmitThisFrame = false;
        ClearThisFrame = false;
        ReduceThisFrame = false;

        if (Phase < 0 && !Reading && Pending != null)
        {
            var request = Pending;
            Pending = null;

            if (!Start(request))
            {
                LatestResult = new SnapDepthResult { Request = request, SurfacePlanes = new float?[request.Groups.Count] };
            }
        }

        if (Phase < 0)
        {
            return;
        }

        SubmitThisFrame = Phase < SubmitFrames;
        ClearThisFrame = Phase >= 1 && Phase < SubmitFrames;
        ReduceThisFrame = Phase == SubmitFrames;
        StrayFrames = 2;

        if (ReduceThisFrame)
        {
            Reading = true;
            Phase = -1;
        }
        else
        {
            Phase++;
        }
    }

    /// <summary>Sets up the view of a request: false if it has nothing to look at</summary>
    private bool Start(SnapDepthRequest request)
    {
        GroupCount = Math.Min(request.Groups.Count, MaxGroups);

        if (GroupCount == 0 || request.Direction.LengthSquared() < 1e-8f)
        {
            return false;
        }

        var travel = Vector3.Normalize(request.Direction);
        var helper = MathF.Abs(travel.Y) > 0.9f ? Vector3.UnitZ : Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(helper, travel));
        var up = Vector3.Cross(travel, right);

        // The footprint of all the groups across the direction, the nearest centre plane, and the farthest reach
        float minR = float.MaxValue, maxR = float.MinValue, minU = float.MaxValue, maxU = float.MinValue;
        var near = float.MaxValue;

        for (var g = 0; g < GroupCount; g++)
        {
            var box = request.Groups[g];

            foreach (var corner in Corners(box))
            {
                var r = Vector3.Dot(right, corner);
                var u = Vector3.Dot(up, corner);
                minR = MathF.Min(minR, r);
                maxR = MathF.Max(maxR, r);
                minU = MathF.Min(minU, u);
                maxU = MathF.Max(maxU, u);
            }

            near = MathF.Min(near, Vector3.Dot(travel, box.GetCenter()));
        }

        var far = near;

        for (var g = 0; g < GroupCount; g++)
        {
            var box = request.Groups[g];
            far = MathF.Max(far, Vector3.Dot(travel, box.GetCenter()) + FaceDistance(box, travel) + request.MaxDistance);
        }

        // A little past the edges, so the groups' outer texels are inside the map
        var halfR = MathF.Max((maxR - minR) * 0.5f * 1.02f, 0.01f);
        var halfU = MathF.Max((maxU - minU) * 0.5f * 1.02f, 0.01f);
        var midR = (minR + maxR) * 0.5f;
        var midU = (minU + maxU) * 0.5f;

        var center = right * midR + up * midU + travel * near;

        View = new Matrix4x4(
            right.X, up.X, travel.X, 0.0f,
            right.Y, up.Y, travel.Y, 0.0f,
            right.Z, up.Z, travel.Z, 0.0f,
            -Vector3.Dot(right, center), -Vector3.Dot(up, center), -Vector3.Dot(travel, center), 1.0f);

        // Reversed depth as the viewport's: 1 at the near plane, through the nearest group's centre, 0 at the far
        Range = MathF.Max(far - near, 0.01f);

        Projection = new Matrix4x4(
            1.0f / halfR, 0.0f, 0.0f, 0.0f,
            0.0f, (Device.IsClipSpaceYInverted ? -1.0f : 1.0f) / halfU, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f / Range, 0.0f,
            0.0f, 0.0f, 1.0f, 1.0f);

        NearPlane = near;

        // World to texels, as the rain map's lookup: with the standard clip Y, Veldrid flips the viewport
        Matrix4x4 lookup = View * Projection;
        if (!Device.IsClipSpaceYInverted)
        {
            lookup *= Matrix4x4.CreateScale(1.0f, -1.0f, 1.0f);
        }

        for (var g = 0; g < GroupCount; g++)
        {
            var box = request.Groups[g];

            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;

            foreach (var corner in Corners(box))
            {
                var p = Vector3.Transform(corner, lookup);
                var tx = (p.X * 0.5f + 0.5f) * Resolution;
                var ty = (p.Y * 0.5f + 0.5f) * Resolution;
                x0 = MathF.Min(x0, tx);
                y0 = MathF.Min(y0, ty);
                x1 = MathF.Max(x1, tx);
                y1 = MathF.Max(y1, ty);
            }

            var rect = new GpuGroup
            {
                X0 = Math.Clamp((int)MathF.Floor(x0), 0, (int)Resolution - 1),
                Y0 = Math.Clamp((int)MathF.Floor(y0), 0, (int)Resolution - 1),
                X1 = Math.Clamp((int)MathF.Ceiling(x1), 0, (int)Resolution),
                Y1 = Math.Clamp((int)MathF.Ceiling(y1), 0, (int)Resolution),
            };

            // At least one texel, for a point or a box edge on to the direction
            rect.X1 = Math.Max(rect.X1, rect.X0 + 1);
            rect.Y1 = Math.Max(rect.Y1, rect.Y0 + 1);

            // Only surfaces past the group's own centre plane count
            var centerDepth = Vector3.Dot(travel, box.GetCenter()) - near;
            rect.Limit = new Vector4(1.0f - centerDepth / Range, 0.0f, 0.0f, 0.0f);

            Groups[g] = rect;
        }

        Active = request;
        Phase = 0;
        return true;
    }

    private static IEnumerable<Vector3> Corners(BoundingBox box)
    {
        for (var i = 0; i < 8; i++)
        {
            yield return new Vector3(
                (i & 1) == 0 ? box.Min.X : box.Max.X,
                (i & 2) == 0 ? box.Min.Y : box.Max.Y,
                (i & 4) == 0 ? box.Min.Z : box.Max.Z);
        }
    }

    /// <summary>How far the face of a box that leads along a direction is from its centre</summary>
    public static float FaceDistance(BoundingBox box, Vector3 direction)
    {
        var half = (box.Max - box.Min) * 0.5f;
        return Vector3.Dot(half, Vector3.Abs(direction));
    }

    /// <summary>Puts the meshes in the request's view in the queue, on the frames it is drawn</summary>
    public void Submit(MeshRenderables renderables, RenderFilter filter, DrawGroup displayGroup)
    {
        if (SubmitThisFrame && Active != null)
        {
            renderables.SubmitDepthCasters(Queue, new BoundingFrustum(View * Projection), filter, displayGroup,
                Active.Excluded);
        }

        Queue.SetDrawParameters(renderables.cDrawParameters, renderables.cDepthPipelines);
    }

    /// <summary>Uploads the request's scene parameters while it is being drawn: the viewport's, seen along it</summary>
    public unsafe void Upload(CommandList cl, SceneParam sceneParams)
    {
        if (!SubmitThisFrame)
        {
            return;
        }

        SceneParam snap = sceneParams;
        snap.View = View;
        snap.Projection = Projection;
        snap.ShadowTexture = 0;
        snap.RainTexture = 0;
        snap.CameraClip = Vector4.Zero;

        cl.UpdateBuffer(ParamBuffer, 0, ref snap, (uint)sizeof(SceneParam));
    }

    private void Begin(CommandList cl)
    {
        // Before any framebuffer is bound: the map holds the request's view from the last two frames
        if (ReduceThisFrame)
        {
            if (Map != null)
            {
                Reduce(cl);
            }
            else
            {
                Reading = false;
            }
        }

        if (ClearThisFrame)
        {
            EnsureMap();

            cl.SetFramebuffer(MapFramebuffer);
            cl.ClearDepthStencil(0);
            cl.SetViewport(0, new Veldrid.Viewport(0, 0, Resolution, Resolution, 0.0f, 1.0f));
            cl.SetScissorRect(0, 0, 0, Resolution, Resolution);
            return;
        }

        // Meshes can still be in the queue from a request's last frames
        if (StrayFrames > 0)
        {
            EnsureSink();

            cl.SetFramebuffer(SinkFramebuffer);
            cl.SetViewport(0, new Veldrid.Viewport(0, 0, 1, 1, 0.0f, 1.0f));
            cl.SetScissorRect(0, 0, 0, 1, 1);
        }
    }

    private void Reduce(CommandList cl)
    {
        EnsureCompute();

        var request = Active;
        var count = GroupCount;
        var near = NearPlane;
        var range = Range;

        // Copied through staging buffers, which also ends any render pass, so the dispatch is outside one
        cl.UpdateBuffer(GroupBuffer, 0, Groups);
        cl.UpdateBuffer(ResultBuffer, 0, Zeros);

        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderRead | VkAccessFlags2.ShaderWrite);

        // One work group per group of the selection
        Compute.Dispatch(Device, cl, new[] { ComputeSet }, (uint)count * Compute.GroupSize.X, Compute.GroupSize.Y);

        cl.Barrier(VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderWrite,
            VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferRead);

        SceneRenderer.AddAsyncReadback(ReadbackBuffer, ResultBuffer, d =>
        {
            var planes = new float?[request.Groups.Count];

            unsafe
            {
                MappedResourceView<uint> mapped = d.Map<uint>(ReadbackBuffer, MapMode.Read);
                var bits = new Span<uint>(mapped.MappedResource.Data.ToPointer(), MaxGroups);

                for (var g = 0; g < count; g++)
                {
                    if (bits[g] == 0)
                    {
                        continue;
                    }

                    var depth = BitConverter.UInt32BitsToSingle(bits[g]);
                    planes[g] = near + (1.0f - depth) * range;
                }
            }

            LatestResult = new SnapDepthResult { Request = request, SurfacePlanes = planes };
            Reading = false;
        });
    }

    private void EnsureCompute()
    {
        if (Compute != null)
        {
            return;
        }

        ResourceFactory factory = Device.ResourceFactory;

        ComputeLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("SnapDepth", VkDescriptorType.SampledImage, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SnapSampler", VkDescriptorType.Sampler, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SnapGroups", VkDescriptorType.StorageBuffer, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SnapResults", VkDescriptorType.StorageBuffer, VkShaderStageFlags.Compute)));

        Compute = new ComputePass("SnapDepth", new[] { ComputeLayout }, 16, 16);

        var groupSize = (uint)(Marshal.SizeOf<GpuGroup>() * MaxGroups);
        GroupBuffer = factory.CreateBuffer(new BufferDescription(groupSize,
            VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0,
            (uint)Marshal.SizeOf<GpuGroup>()));
        GroupBuffer.Name = "Snap Depth Groups";

        ResultBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * MaxGroups,
            VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto, 0, sizeof(uint)));
        ResultBuffer.Name = "Snap Depth Results";

        ReadbackBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * MaxGroups,
            VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, VmaAllocationCreateFlags.Mapped));

        ComputeSet = factory.CreateResourceSet(new ResourceSetDescription(ComputeLayout, Map, Device.PointSampler,
            GroupBuffer, ResultBuffer));
    }

    private void EnsureMap()
    {
        if (Map != null)
        {
            return;
        }

        TextureDescription description = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            ShadowCascades.DepthFormat, VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Map = Device.ResourceFactory.CreateTexture(ref description);
        Map.Name = "Snap Depth Map";
        MapFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Map));
    }

    private void EnsureSink()
    {
        if (Sink != null)
        {
            return;
        }

        TextureDescription description = TextureDescription.Texture2D(1, 1, 1, 1, ShadowCascades.DepthFormat,
            VkImageUsageFlags.DepthStencilAttachment, VkImageCreateFlags.None, VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);

        Sink = Device.ResourceFactory.CreateTexture(ref description);
        Sink.Name = "Snap Depth Sink";
        SinkFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Sink));
    }
}
