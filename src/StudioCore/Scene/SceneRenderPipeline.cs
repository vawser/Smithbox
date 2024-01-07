using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene;

/// <summary>
///     The "renderer" for a scene. This pipeline is instantiated for every real or virtual viewport, and will
///     render a scene into an internally maintained framebuffer.
/// </summary>
public class SceneRenderPipeline
{
    private readonly Renderer.RenderQueue _overlayQueue;

    private readonly Renderer.RenderQueue _renderQueue;
    private readonly RenderScene Scene;

    private bool _pickingEnabled;

    public uint EnvMapTexture = 0;


    public PickingResult PickingResult;

    public SceneParam SceneParams;

    public unsafe SceneRenderPipeline(RenderScene scene, GraphicsDevice device, int width, int height)
    {
        Scene = scene;

        ResourceFactory factory = device.ResourceFactory;

        // Setup scene param uniform buffer
        SceneParamBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(SceneParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));
        SceneParams = new SceneParam();
        SceneParams.Projection = Utils.CreatePerspective(device, true,
            CFG.Current.GFX_Camera_FOV * (float)Math.PI / 180.0f, width / (float)height, 0.1f, 2000.0f);
        SceneParams.View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 2.0f, 0.0f), new Vector3(1.0f, 2.0f, 0.0f),
            Vector3.UnitY);
        SceneParams.EyePosition = new Vector4(0.0f, 2.0f, 0.0f, 0.0f);
        SceneParams.LightDirection = new Vector4(1.0f, -0.5f, 0.0f, 0.0f);
        SceneParams.EnvMap = EnvMapTexture;
        SceneParams.CursorPosition[0] = 0;
        SceneParams.CursorPosition[1] = 0;
        SceneParams.CursorPosition[2] = 0;
        SceneParams.CursorPosition[3] = 0;
        SceneParams.AmbientLightMult = 1.0f;
        SceneParams.DirectLightMult = 1.0f;
        SceneParams.IndirectLightMult = 1.0f;
        SceneParams.EmissiveMapMult = 1.0f;
        SceneParams.SceneBrightness = 1.0f;
        device.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
        ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(
            device.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);
        SceneParamResourceSet = StaticResourceCache.GetResourceSet(device.ResourceFactory,
            new ResourceSetDescription(sceneParamLayout, SceneParamBuffer));

        // Setup picking uniform buffer
        PickingResultsBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(PickingResult),
                VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0,
                (uint)sizeof(PickingResult)
            ));
        PickingResult = new PickingResult();
        PickingResult.depth = 0; // int.MaxValue;
        PickingResult.entityID = ulong.MaxValue;
        device.UpdateBuffer(PickingResultsBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));
        ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(
            device.ResourceFactory,
            StaticResourceCache.PickingResultDescription);
        PickingResultResourceSet = StaticResourceCache.GetResourceSet(device.ResourceFactory,
            new ResourceSetDescription(pickingResultLayout,
                PickingResultsBuffer));

        PickingResultReadbackBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(PickingResult),
                VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped));
        device.UpdateBuffer(PickingResultReadbackBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));

        _renderQueue = new Renderer.RenderQueue("Viewport Render", device, this);
        Renderer.RegisterRenderQueue(_renderQueue);

        _overlayQueue = new Renderer.RenderQueue("Overlay Render", device, this);
        Renderer.RegisterRenderQueue(_overlayQueue);
    }

    public DeviceBuffer SceneParamBuffer { get; }

    public ResourceSet SceneParamResourceSet { get; }
    public DeviceBuffer PickingResultsBuffer { get; }
    public ResourceSet PickingResultResourceSet { get; }

    public DeviceBuffer PickingResultReadbackBuffer { get; }

    public Vector3 Eye { get; private set; }

    public float CPURenderTime => _renderQueue.CPURenderTime;
    public bool PickingResultsReady { get; set; }

    private int _pickingEntity { get; set; }

    public void SetViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _renderQueue.SetPredrawSetupAction(action);
    }

    public void SetOverlayViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _overlayQueue.SetPredrawSetupAction(action);
    }

    public unsafe void TestUpdateView(Matrix4x4 proj, Matrix4x4 view, Vector3 eye, int cursorx, int cursory)
    {
        Eye = eye;
        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            SceneParams.Projection = proj;
            SceneParams.View = view;
            SceneParams.EyePosition = new Vector4(eye, 0.0f);
            SceneParams.EnvMap = EnvMapTexture;
            SceneParams.CursorPosition[0] = cursorx;
            SceneParams.CursorPosition[1] = cursory;
            cl.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
        });
    }

    public void BindResources(CommandList cl)
    {
        cl.SetGraphicsResourceSet(0, SceneParamResourceSet);
        cl.SetGraphicsResourceSet(6, PickingResultResourceSet);
    }

    public void RenderScene(BoundingFrustum frustum)
    {
        Scene.Render(_renderQueue, _overlayQueue, frustum, this);
    }

    public unsafe void CreateAsyncPickingRequest()
    {
        if (_pickingEnabled)
        {
            return;
        }

        _pickingEnabled = true;
        Scene.SendGPUPickingRequest();
        Debug.WriteLine("Starting picking request");
        Renderer.AddAsyncReadback(PickingResultReadbackBuffer, PickingResultsBuffer, d =>
        {
            MappedResourceView<PickingResult> result =
                d.Map<PickingResult>(PickingResultReadbackBuffer, MapMode.Read);
            var results = new Span<PickingResult>(result.MappedResource.Data.ToPointer(), 1);
            _pickingEntity = results[0].entityID != ulong.MaxValue ? (int)results[0].entityID : -1;
            PickingResultsReady = true;
            _pickingEnabled = false;
            Debug.WriteLine($@"Got picking result: entity {results[0].entityID}, depth {results[0].depth}");
            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                cl.UpdateBuffer(PickingResultsBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));
            });
        });
    }

    public ISelectable GetSelection()
    {
        if (!PickingResultsReady)
        {
            throw new Exception("Can't get selection when picking results aren't ready");
        }

        PickingResultsReady = false;

        if (_pickingEntity == -1)
        {
            return null;
        }

        var renderableSystemIndex = (uint)_pickingEntity >> 30;
        WeakReference<ISelectable> sel;
        if (renderableSystemIndex == 0)
        {
            // TODO: Logging?
            if ((_pickingEntity & 0x3FFFFFFF) >= Scene.OpaqueRenderables.cSelectables.Length)
            {
                return null;
            }

            sel = Scene.OpaqueRenderables.cSelectables[_pickingEntity & 0x3FFFFFFF];
        }
        else
        {
            if ((_pickingEntity & 0x3FFFFFFF) >= Scene.OverlayRenderables.cSelectables.Length)
            {
                return null;
            }

            sel = Scene.OverlayRenderables.cSelectables[_pickingEntity & 0x3FFFFFFF];
        }

        ISelectable selected;
        if (sel != null && sel.TryGetTarget(out selected))
        {
            return selected;
        }

        return null;
    }
}
