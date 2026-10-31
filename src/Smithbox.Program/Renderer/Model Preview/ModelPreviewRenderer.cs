using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public enum ModelPreviewProgress
{
    Working,
    Finished,
    Failed
}

/// <summary>
///     Renders one model at a time into a texture, with a scene of its own, and unloads the model afterwards.
///     <see cref="Advance" /> moves the current model along once per frame.
///     <para>
///         Models are loaded for this purpose alone: only the model itself is read from its container, without
///         textures, and it is drawn with the flat shader. Rendering has to wait for its turn, as there is one set
///         of scene parameters, but loading doesn't: <see cref="Prefetch" /> loads what is coming up in parallel.
///     </para>
/// </summary>
public sealed class ModelPreviewRenderer
{
    /// <summary>
    ///     The size of a preview's texture. This is fixed rather than following the display size, so that the size
    ///     can be adjusted without every preview having to be rendered again.
    /// </summary>
    public const uint Resolution = 256;

    private const float FieldOfView = MathF.PI / 3.0f;

    // Looks at the front of the model, which faces -Z, from above and to the side
    private static readonly Quaternion ViewRotation = Quaternion.CreateFromYawPitchRoll(-0.6f, 0.35f, 0.0f);

    // Gives up on a model after this many frames
    private const int LoadTimeout = 600;

    // Frames given to the resources of a finished load to appear, as they are registered on the main thread
    private const int ResourceArrivalFrames = 30;

    // More than the frames in flight, which may still be using a model after it has been rendered
    private const int RetireFrames = 4;

    // The most models to hold ahead of the one being rendered
    private const int PrefetchLimit = 12;

    private readonly GraphicsDevice Device;
    private readonly RenderScene Scene = new();
    private readonly SceneRenderPipeline Pipeline;
    private readonly ResourcePool Pool = new("model preview") { ModelsOnly = true };

    // Rendering happens in here, and is copied to the preview's own texture. This way there is
    // only the one framebuffer and depth buffer, however many previews are being kept.
    private readonly Texture ColorTarget;
    private readonly Texture DepthTarget;
    private readonly Framebuffer Framebuffer;

    private readonly List<RetiredModel> Retired = new();
    private readonly List<PrefetchedModel> Prefetched = new();

    private Stage CurrentStage = Stage.Idle;
    private ModelPreviewEntry Entry;
    private ResourceHandle<FlverResource> Handle;
    private MeshRenderableProxy Mesh;
    private string Container;
    private Task ModelLoad;

    private TexturePool.TextureHandle Preview;
    private Texture PreviewTexture;

    // Set for the frame in which the queue has the model's draws, read by the queue's setup actions
    private bool CaptureThisFrame;

    private int FramesInStage;
    private int FramesLoaded;

    private enum Stage
    {
        Idle,
        Loading,
        Capture,
        Publish
    }

    private sealed record RetiredModel(IResourceHandle Handle, string Container)
    {
        public int Age { get; set; }
    }

    private sealed record PrefetchedModel(string ID, string Container, Task Load);

    public ModelPreviewRenderer(GraphicsDevice device)
    {
        Device = device;

        OutputDescription outputs = device.SwapchainFramebuffer.OutputDescription;

        // Mesh pipelines are built for the swapchain, so matching its formats lets them be used here as they are
        TextureDescription colorDescription = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            outputs.ColorAttachments[0].Format, VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        TextureDescription depthDescription = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            outputs.DepthAttachment.Value.Format, VkImageUsageFlags.DepthStencilAttachment,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        ColorTarget = device.ResourceFactory.CreateTexture(ref colorDescription);
        DepthTarget = device.ResourceFactory.CreateTexture(ref depthDescription);
        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(DepthTarget, ColorTarget));

        Pipeline = new SceneRenderPipeline(Scene, device, (int)Resolution, (int)Resolution);
        Pipeline.SetOffscreen();

        Pipeline.SetViewportSetupAction((d, cl) =>
        {
            cl.SetFramebuffer(Framebuffer);

            if (CaptureThisFrame)
            {
                Vector3 background = CFG.Current.Viewport_Bg_Color;

                cl.ClearColorTarget(0, new RgbaFloat(background.X, background.Y, background.Z, 1.0f));
                cl.ClearDepthStencil(0);
            }
        });

        // The overlay queue runs after the model has been drawn, and this scene has no overlays of its own
        Pipeline.SetOverlayViewportSetupAction((d, cl) =>
        {
            if (CaptureThisFrame)
            {
                CaptureThisFrame = false;
                cl.CopyTexture(ColorTarget, PreviewTexture);
            }
        });
    }

    /// <summary>
    ///     Starts on a model, replacing any model in progress.
    /// </summary>
    public void Begin(ProjectEntry project, ModelPreviewEntry entry)
    {
        Cancel();

        ResourceDescriptor asset = GetModelAsset(project, entry);

        if (!TryGetContainer(project, entry, asset, out Container))
        {
            return;
        }

        Entry = entry;

        // A load restricted to one name isn't remembered by the pool, so asking again would read the container again
        PrefetchedModel prefetched = Prefetched.Find(e => e.ID == entry.ID);

        if (prefetched != null)
        {
            Prefetched.Remove(prefetched);
        }

        Handle = Pool.Find<FlverResource>(asset.Name);
        Mesh = MeshRenderableProxy.FromProvider(Scene, MeshProviderCache.GetFlverMeshProvider(Handle));
        ModelLoad = prefetched?.Load ?? LoadModel(asset, Container);

        SetStage(Stage.Loading);
    }

    /// <summary>
    ///     Starts loading the models that are waiting, and drops any that aren't wanted anymore.
    /// </summary>
    /// <param name="entries">The models that are waiting, in the order they will be rendered.</param>
    public void Prefetch(ProjectEntry project, IReadOnlyList<ModelPreviewEntry> entries)
    {
        HashSet<string> wanted = new(StringComparer.OrdinalIgnoreCase);

        foreach (ModelPreviewEntry entry in entries)
        {
            wanted.Add(entry.ID);
        }

        // One that is still loading can't be taken back, so it is left until it arrives
        for (var i = Prefetched.Count - 1; i >= 0; i--)
        {
            if (Prefetched[i].Load.IsCompleted && !wanted.Contains(Prefetched[i].ID))
            {
                Pool.UnloadContainer(Prefetched[i].Container);
                Prefetched.RemoveAt(i);
            }
        }

        foreach (ModelPreviewEntry entry in entries)
        {
            if (Prefetched.Count >= PrefetchLimit)
            {
                return;
            }

            if (entry.ID == Entry?.ID || Prefetched.Exists(e => e.ID == entry.ID))
            {
                continue;
            }

            ResourceDescriptor asset = GetModelAsset(project, entry);

            if (TryGetContainer(project, entry, asset, out var container))
            {
                Prefetched.Add(new PrefetchedModel(entry.ID, container, LoadModel(asset, container)));
            }
        }
    }

    /// <summary>
    ///     Moves the model along by a frame. Must be called before the frame's render queues are executed.
    /// </summary>
    /// <param name="preview">The finished preview, which then belongs to the caller.</param>
    public ModelPreviewProgress Advance(out TexturePool.TextureHandle preview)
    {
        preview = null;

        FramesInStage++;

        switch (CurrentStage)
        {
            case Stage.Loading:
                return AdvanceLoading();

            case Stage.Capture:
                return Capture();

            case Stage.Publish:
                preview = Preview;
                Preview = null;
                PreviewTexture = null;

                FinishModel();

                return ModelPreviewProgress.Finished;

            default:
                return ModelPreviewProgress.Failed;
        }
    }

    /// <summary>
    ///     Abandons the model in progress, if there is one.
    /// </summary>
    public void Cancel()
    {
        CaptureThisFrame = false;

        Preview?.Dispose();
        Preview = null;
        PreviewTexture = null;

        FinishModel();
    }

    /// <summary>
    ///     Abandons the model in progress and everything loaded ahead of it.
    /// </summary>
    public void Reset()
    {
        Cancel();

        foreach (PrefetchedModel model in Prefetched)
        {
            Pool.UnloadContainer(model.Container);
        }

        Prefetched.Clear();
    }

    /// <summary>
    ///     Unloads the models that the GPU has finished with.
    /// </summary>
    public void RetireFinished()
    {
        for (var i = Retired.Count - 1; i >= 0; i--)
        {
            RetiredModel model = Retired[i];

            model.Age++;

            if (model.Age < RetireFrames)
            {
                continue;
            }

            Retired.RemoveAt(i);

            MeshProviderCache.InvalidateMeshProvider(model.Handle);
            Pool.UnloadContainer(model.Container);
        }
    }

    private Task LoadModel(ResourceDescriptor asset, string container)
    {
        // Containers hold plenty that isn't needed here, including other models
        return Pool.LoadContainer(container, new LoadOptions
        {
            JobName = "Model Previews",
            HideFromLoadWindow = true,
            Types = ResourceType.Flver,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            OnlyNames = new[] { asset.Name }
        });
    }

    private ModelPreviewProgress AdvanceLoading()
    {
        if (FramesInStage > LoadTimeout || ModelLoad.IsFaulted)
        {
            return Fail();
        }

        if (!ModelLoad.IsCompleted)
        {
            return ModelPreviewProgress.Working;
        }

        FramesLoaded++;

        if (!Handle.IsLoaded || !Mesh.HasRenderableGeometry())
        {
            return FramesLoaded > ResourceArrivalFrames ? Fail() : ModelPreviewProgress.Working;
        }

        if (!Mesh.AreRenderablesConstructed())
        {
            return ModelPreviewProgress.Working;
        }

        return Submit();
    }

    private ModelPreviewProgress Submit()
    {
        BoundingBox bounds = Mesh.GetBounds();

        // Slightly larger than the model so that it doesn't touch the edges
        var radius = Vector3.Distance(bounds.Max, bounds.Min) * 0.5f * 1.05f;

        if (radius <= 0.0f || float.IsNaN(radius))
        {
            return Fail();
        }

        var distance = radius / MathF.Sin(FieldOfView * 0.5f);
        Vector3 direction = Vector3.Transform(Vector3.UnitZ, ViewRotation);
        Vector3 eye = bounds.GetCenter() - direction * distance;

        Transform camera = new(eye, ViewRotation, Vector3.One);

        Matrix4x4 view = camera.CameraViewMatrixLH;
        Matrix4x4 projection = ViewportUtils.CreatePerspective(Device, true, FieldOfView, 1.0f,
            MathF.Max(distance - radius, distance * 0.01f), distance + radius);

        Pipeline.UpdateSceneParameters(projection, view, eye, 0, 0);
        Pipeline.RenderScene(new BoundingFrustum(view * projection));

        // Nothing to draw leaves the render pass that clears the framebuffer unstarted, so there is nothing to copy
        if (Pipeline.QueuedDrawCount == 0)
        {
            return Fail();
        }

        SetStage(Stage.Capture);

        return ModelPreviewProgress.Working;
    }

    private ModelPreviewProgress Capture()
    {
        // A queue encodes its draws in one frame and submits them in the next, which is this one
        Preview = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

        if (Preview == null)
        {
            return Fail();
        }

        TextureDescription description = TextureDescription.Texture2D(Resolution, Resolution, 1, 1, ColorTarget.Format,
            VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst, VkImageCreateFlags.None, VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);

        PreviewTexture = Device.ResourceFactory.CreateTexture(ref description);
        PreviewTexture.Name = $"Model Preview: {Entry.ID}";

        Preview.FillWithGPUTexture(PreviewTexture);

        CaptureThisFrame = true;

        SetStage(Stage.Publish);

        return ModelPreviewProgress.Working;
    }

    private ModelPreviewProgress Fail()
    {
        FinishModel();

        return ModelPreviewProgress.Failed;
    }

    private void FinishModel()
    {
        if (Mesh != null)
        {
            // Taken out of the scene now, but its resources have to outlast any frames that drew it
            Mesh.Dispose();

            Retired.Add(new RetiredModel(Handle, Container));
        }

        Mesh = null;
        Handle = null;
        Entry = null;
        Container = null;
        CurrentStage = Stage.Idle;
    }

    private void SetStage(Stage stage)
    {
        CurrentStage = stage;
        FramesInStage = 0;
        FramesLoaded = 0;
    }

    private static bool TryGetContainer(ProjectEntry project, ModelPreviewEntry entry, ResourceDescriptor asset, out string container)
    {
        container = asset.GetContainerPath(project);

        // Stale aliases are common, and the alternative is waiting for a load that won't deliver anything
        return container != null && (entry.Kind is ModelPreviewKind.MapPiece || project.VFS.FS.FileExists(container));
    }

    private static ResourceDescriptor GetModelAsset(ProjectEntry project, ModelPreviewEntry entry)
    {
        var name = GetModelName(entry);

        return entry.Kind switch
        {
            ModelPreviewKind.Character => ModelLocator.GetChrModel(project, name, name),
            ModelPreviewKind.Asset => ModelLocator.GetObjModel(project, name, name),
            _ => ModelLocator.GetMapModel(project, PathBuilder.GetAssetMapID(project, entry.MapID), name, name)
        };
    }

    // The alias lists hold asset names in lower case, but they are in upper case everywhere else
    private static string GetModelName(ModelPreviewEntry entry)
    {
        return entry.ID.Replace("aeg", "AEG");
    }
}
