using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Scene.Framework;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Graphics;

/// <summary>
///     Can render draw lists produced by ImGui.
///     Also provides functions for updating ImGui input.
/// </summary>
public class VulkanImGuiRenderer : IImguiRenderer, IDisposable
{
    private readonly Assembly _assembly;

    private readonly Dictionary<Texture, TextureView> _autoViewsByTexture = new();

    //private Texture _fontTexture;
    private readonly TexturePool.TextureHandle _fontTexture;

    private readonly List<IDisposable> _ownedResources = new();
    private readonly Vector2 _scaleFactor = Vector2.One;

    // Image trackers
    private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView = new();

    private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new();
    private bool _altDown;

    private ColorSpaceHandling _colorSpaceHandling;

    //private ResourceSet _fontTextureResourceSet;
    //private IntPtr _fontAtlasID = (IntPtr)1;
    private bool _controlDown;

    private int _firstFrame;
    private Shader _fragmentShader;
    private bool _frameBegun;
    private GraphicsDevice _gd;
    private DeviceBuffer _indexBuffer;
    private int _lastAssignedID = 100;
    private ResourceLayout _layout;
    private ResourceSet _mainResourceSet;
    private Pipeline _pipeline;
    private DeviceBuffer _projMatrixBuffer;
    private bool _shiftDown;
    private ResourceLayout _textureLayout;

    // Device objects
    private DeviceBuffer _vertexBuffer;
    private Shader _vertexShader;
    private int _windowHeight;

    private int _windowWidth;

    private ImGuiContextPtr guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr plotContext;

    /// <summary>
    ///     Constructs a new ImGuiRenderer.
    /// </summary>
    /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
    /// <param name="outputDescription">The output format.</param>
    /// <param name="width">The initial width of the rendering target. Can be resized.</param>
    /// <param name="height">The initial height of the rendering target. Can be resized.</param>
    public VulkanImGuiRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height)
        : this(gd, outputDescription, width, height, ColorSpaceHandling.Legacy)
    {
    }

    /// <summary>
    ///     Constructs a new ImGuiRenderer.
    /// </summary>
    /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
    /// <param name="outputDescription">The output format.</param>
    /// <param name="width">The initial width of the rendering target. Can be resized.</param>
    /// <param name="height">The initial height of the rendering target. Can be resized.</param>
    /// <param name="colorSpaceHandling">Identifies how the renderer should treat vertex colors.</param>
    public VulkanImGuiRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height,
        ColorSpaceHandling colorSpaceHandling)
    {
        _gd = gd;
        _assembly = typeof(VulkanImGuiRenderer).GetTypeInfo().Assembly;
        _colorSpaceHandling = colorSpaceHandling;
        _windowWidth = width;
        _windowHeight = height;

        _fontTexture = Renderer.GlobalTexturePool.AllocateTextureDescriptor();

        // Create ImGui context
        guiContext = ImGui.CreateContext(null);

        // Set ImGui context
        ImGui.SetCurrentContext(guiContext);

        // Set ImGui context for ImGuizmo
        ImGuizmo.SetImGuiContext(guiContext);

        // Set ImGui context for ImPlot
        ImPlot.SetImGuiContext(guiContext);

        // Set ImGui context for ImNodes
        ImNodes.SetImGuiContext(guiContext);

        // Create and set ImNodes context and set style
        nodesContext = ImNodes.CreateContext();
        ImNodes.SetCurrentContext(nodesContext);
        ImNodes.StyleColorsDark(ImNodes.GetStyle());

        // Create and set ImPlot context and set style
        plotContext = ImPlot.CreateContext();
        ImPlot.SetCurrentContext(plotContext);
        ImPlot.StyleColorsDark(ImPlot.GetStyle());

        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        unsafe
        {
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            platformIO.PlatformSetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_setClipboardTextCallback);
            platformIO.PlatformGetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_getClipboardTextCallback);
            platformIO.PlatformClipboardUserData = (void*)GCHandle.ToIntPtr(_clipboardUserDataHandle);
        }

        ImGui.GetIO().Fonts.AddFontDefault();

        CreateDeviceResources(gd, outputDescription);

        SetPerFrameImGuiData(1f / 60f);
    }

    private static SetClipboardTextCallback _setClipboardTextCallback = SetClipboardText;
    private static GetClipboardTextCallback _getClipboardTextCallback = GetClipboardText;
    private static readonly GCHandle _clipboardUserDataHandle = GCHandle.Alloc(new object());

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void SetClipboardTextCallback(IntPtr userData, IntPtr text);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate IntPtr GetClipboardTextCallback(IntPtr userData);

    private static void SetClipboardText(IntPtr userData, IntPtr text)
    {
        string managedText = Marshal.PtrToStringUTF8(text);
        PlatformUtils.Instance.SetClipboardText(managedText);
    }

    private static IntPtr GetClipboardText(IntPtr userData)
    {
        string clipboardText = PlatformUtils.Instance.GetClipboardText();
        return Marshal.StringToCoTaskMemUTF8(clipboardText);
    }

    /// <summary>
    ///     Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _projMatrixBuffer.Dispose();
        _fontTexture.Dispose();
        _vertexShader.Dispose();
        _fragmentShader.Dispose();
        _layout.Dispose();
        _textureLayout.Dispose();
        _pipeline.Dispose();
        _mainResourceSet.Dispose();

        foreach (IDisposable resource in _ownedResources)
        {
            resource.Dispose();
        }
    }

    public void OnSetupDone()
    {
        ImGui.NewFrame();
        _frameBegun = true;
    }

    /// <summary>
    ///     Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture()
    {
        RecreateFontDeviceTexture(_gd);
    }

    /// <summary>
    ///     Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaSeconds, InputSnapshot snapshot, Action updateFontAction)
    {
        BeginUpdate(deltaSeconds);
        if (updateFontAction != null)
        {
            updateFontAction.Invoke();
        }

        UpdateImGuiInput(snapshot);
        EndUpdate();
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    public void DestroyDeviceObjects()
    {
        Dispose();
    }

    public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
    {
        CreateDeviceResources(gd, outputDescription, _colorSpaceHandling);
    }

    public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription,
        ColorSpaceHandling colorSpaceHandling)
    {
        _gd = gd;
        _colorSpaceHandling = colorSpaceHandling;
        ResourceFactory factory = gd.ResourceFactory;
        _vertexBuffer = factory.CreateBuffer(
            new BufferDescription(
                10000,
                VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));
        _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
        _indexBuffer = factory.CreateBuffer(
            new BufferDescription(
                2000,
                VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));
        _indexBuffer.Name = "ImGui.NET Index Buffer";

        _projMatrixBuffer = factory.CreateBuffer(
            new BufferDescription(
                64,
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));
        _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

        Tuple<Shader, Shader> res = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "imgui").ToTuple();
        _vertexShader = res.Item1;
        _fragmentShader = res.Item2;

        VertexLayoutDescription[] vertexLayouts =
        {
            new(
                new VertexElementDescription("in_position", VkFormat.R32G32Sfloat),
                new VertexElementDescription("in_texCoord", VkFormat.R32G32Sfloat),
                new VertexElementDescription("in_color", VkFormat.R8G8B8A8Unorm))
        };

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionMatrixBuffer", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Vertex),
            new ResourceLayoutElementDescription("MainSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment)));
        _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment)));

        GraphicsPipelineDescription pd = new(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(false, false, VkCompareOp.Always),
            new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true,
                true),
            VkPrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                vertexLayouts,
                new[] { _vertexShader, _fragmentShader },
                new[]
                {
                    new SpecializationConstant(0, gd.IsClipSpaceYInverted),
                    new SpecializationConstant(1, _colorSpaceHandling == ColorSpaceHandling.Legacy)
                }),
            new[] { _layout, Renderer.GlobalTexturePool.GetLayout() },
            outputDescription);
        _pipeline = factory.CreateGraphicsPipeline(ref pd);
        _pipeline.Name = "ImGuiPipeline";

        _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout,
            _projMatrixBuffer,
            gd.PointSampler));

        RecreateFontDeviceTexture(gd);
    }

    /// <summary>
    ///     Gets or creates a handle for a texture to be drawn with ImGui.
    ///     Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        if (!_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
        {
            ResourceSet resourceSet =
                factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
            rsi = new ResourceSetInfo(GetNextImGuiBindingID(), resourceSet);

            _setsByView.Add(textureView, rsi);
            _viewsById.Add(rsi.ImGuiBinding, rsi);
            _ownedResources.Add(resourceSet);
        }

        return rsi.ImGuiBinding;
    }

    public void RemoveImGuiBinding(TextureView textureView)
    {
        if (_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
        {
            _setsByView.Remove(textureView);
            _viewsById.Remove(rsi.ImGuiBinding);
            _ownedResources.Remove(rsi.ResourceSet);
            rsi.ResourceSet.Dispose();
        }
    }

    private IntPtr GetNextImGuiBindingID()
    {
        var newID = _lastAssignedID++;
        return newID;
    }

    /// <summary>
    ///     Gets or creates a handle for a texture to be drawn with ImGui.
    ///     Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        if (!_autoViewsByTexture.TryGetValue(texture, out TextureView textureView))
        {
            textureView = factory.CreateTextureView(texture);
            _autoViewsByTexture.Add(texture, textureView);
            _ownedResources.Add(textureView);
        }

        return GetOrCreateImGuiBinding(factory, textureView);
    }

    public void RemoveImGuiBinding(Texture texture)
    {
        if (_autoViewsByTexture.TryGetValue(texture, out TextureView textureView))
        {
            _autoViewsByTexture.Remove(texture);
            _ownedResources.Remove(textureView);
            textureView.Dispose();
            RemoveImGuiBinding(textureView);
        }
    }

    /// <summary>
    ///     Retrieves the shader texture binding for the given helper handle.
    /// </summary>
    public ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
    {
        if (!_viewsById.TryGetValue(imGuiBinding, out ResourceSetInfo rsi))
        {
            throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding);
        }

        return rsi.ResourceSet;
    }

    public void ClearCachedImageResources()
    {
        foreach (IDisposable resource in _ownedResources)
        {
            resource.Dispose();
        }

        _ownedResources.Clear();
        _setsByView.Clear();
        _viewsById.Clear();
        _autoViewsByTexture.Clear();
        _lastAssignedID = 100;
    }

    private string GetEmbeddedResourceText(string resourceName)
    {
        using (StreamReader sr = new(_assembly.GetManifestResourceStream(resourceName)))
        {
            return sr.ReadToEnd();
        }
    }

    private byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        using (Stream s = _assembly.GetManifestResourceStream(resourceName))
        {
            var ret = new byte[s.Length];
            s.ReadExactly(ret, 0, (int)s.Length);
            return ret;
        }
    }

    /// <summary>
    ///     Recreates the device texture used to render text.
    /// </summary>
    public unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        byte* pixels;
        int width;
        int height;
        int bytesPerPixel;

        // Build
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height, &bytesPerPixel);

        // Store our identifier
        io.Fonts.SetTexID(_fontTexture.TexHandle);

        //_fontTexture?.Dispose();
        Texture tex = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint)width,
            (uint)height,
            1,
            1,
            VkFormat.R8G8B8A8Unorm,
            VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None,
            VkImageTiling.Optimal));
        tex.Name = "ImGui.NET Font Texture";
        gd.UpdateTexture(
            tex,
            (IntPtr)pixels,
            (uint)(bytesPerPixel * width * height),
            0,
            0,
            0,
            (uint)width,
            (uint)height,
            1,
            0,
            0);
        _fontTexture.FillWithGPUTexture(tex);

        //_fontTextureResourceSet?.Dispose();
        //_fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTexture));

        io.Fonts.ClearTexData();
    }

    /// <summary>
    ///     Renders the ImGui draw list data.
    /// </summary>
    public void Render(GraphicsDevice gd, CommandList cl)
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), gd, cl);
        }
    }

    /// <summary>
    ///     Called before we handle the input in <see cref="Update(float, InputSnapshot)" />.
    ///     This render ImGui and update the state.
    /// </summary>
    protected void BeginUpdate(float deltaSeconds)
    {
        if (_frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
    }

    /// <summary>
    ///     Called at the end of <see cref="Update(float, InputSnapshot)" />.
    ///     This tells ImGui that we are on the next frame.
    /// </summary>
    protected void EndUpdate()
    {
        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    ///     Sets per-frame data based on the associated window.
    ///     This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private readonly Dictionary<Key, bool> _keyStates = new();

    private void UpdateImGuiInput(InputSnapshot snapshot)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        // Mouse input
        var leftPressed = false;
        var middlePressed = false;
        var rightPressed = false;
        for (int i = 0; i < snapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = snapshot.MouseEvents[i];
            if (me.Down)
            {
                switch (me.MouseButton)
                {
                    case MouseButton.Left: leftPressed = true; break;
                    case MouseButton.Middle: middlePressed = true; break;
                    case MouseButton.Right: rightPressed = true; break;
                }
            }
        }

        io.MouseDown[0] = leftPressed || snapshot.IsMouseDown(MouseButton.Left);
        io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
        io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
        io.MousePos = snapshot.MousePosition;
        io.MouseWheel = snapshot.WheelDelta;

        // Text input
        foreach (char c in snapshot.KeyCharPresses)
        {
            io.AddInputCharacter(c);
        }

        // Key events
        foreach (KeyEvent keyEvent in snapshot.KeyEvents)
        {
            _keyStates[keyEvent.Key] = keyEvent.Down;
        }

        foreach ((Key key, bool isDown) in _keyStates)
        {
            ImGuiKey imguiKey = MapToImGuiKey(key);
            if (imguiKey != ImGuiKey.None)
            {
                io.AddKeyEvent(imguiKey, isDown);
            }
        }

        io.AddKeyEvent(ImGuiKey.ModCtrl, _keyStates.GetValueOrDefault(Key.ControlLeft) || _keyStates.GetValueOrDefault(Key.ControlRight));
        io.AddKeyEvent(ImGuiKey.ModShift, _keyStates.GetValueOrDefault(Key.ShiftLeft) || _keyStates.GetValueOrDefault(Key.ShiftRight));
        io.AddKeyEvent(ImGuiKey.ModAlt, _keyStates.GetValueOrDefault(Key.AltLeft) || _keyStates.GetValueOrDefault(Key.AltRight));

        // Optional legacy fields
        io.KeyCtrl = _keyStates.GetValueOrDefault(Key.ControlLeft) || _keyStates.GetValueOrDefault(Key.ControlRight);
        io.KeyShift = _keyStates.GetValueOrDefault(Key.ShiftLeft) || _keyStates.GetValueOrDefault(Key.ShiftRight);
        io.KeyAlt = _keyStates.GetValueOrDefault(Key.AltLeft) || _keyStates.GetValueOrDefault(Key.AltRight);
    }

    private ImGuiKey MapToImGuiKey(Key key)
    {
        return key switch
        {
            Key.Tab => ImGuiKey.Tab,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Right => ImGuiKey.RightArrow,
            Key.Up => ImGuiKey.UpArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.BackSpace => ImGuiKey.Backspace,
            Key.Space => ImGuiKey.Space,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.A => ImGuiKey.A,
            Key.B => ImGuiKey.B,
            Key.C => ImGuiKey.C,
            Key.D => ImGuiKey.D,
            Key.E => ImGuiKey.E,
            Key.F => ImGuiKey.F,
            Key.G => ImGuiKey.G,
            Key.H => ImGuiKey.H,
            Key.I => ImGuiKey.I,
            Key.J => ImGuiKey.J,
            Key.K => ImGuiKey.K,
            Key.L => ImGuiKey.L,
            Key.M => ImGuiKey.M,
            Key.N => ImGuiKey.N,
            Key.O => ImGuiKey.O,
            Key.P => ImGuiKey.P,
            Key.Q => ImGuiKey.Q,
            Key.R => ImGuiKey.R,
            Key.S => ImGuiKey.S,
            Key.T => ImGuiKey.T,
            Key.U => ImGuiKey.U,
            Key.V => ImGuiKey.V,
            Key.W => ImGuiKey.W,
            Key.X => ImGuiKey.X,
            Key.Y => ImGuiKey.Y,
            Key.Z => ImGuiKey.Z,
            Key.ControlLeft => ImGuiKey.LeftCtrl,
            Key.ControlRight => ImGuiKey.RightCtrl,
            Key.ShiftLeft => ImGuiKey.LeftShift,
            Key.ShiftRight => ImGuiKey.RightShift,
            Key.AltLeft => ImGuiKey.LeftAlt,
            Key.AltRight => ImGuiKey.RightAlt,
            _ => ImGuiKey.None,
        };
    }

    private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandList cl)
    {
        if (_firstFrame < 30)
        {
            _firstFrame++;
            return;
        }

        uint vertexOffsetInVertices = 0;
        uint indexOffsetInElements = 0;

        if (draw_data.CmdListsCount == 0)
        {
            return;
        }

        var totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVBSize > _vertexBuffer.SizeInBytes)
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = gd.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(totalVBSize * 1.5f),
                    VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst,
                    VmaMemoryUsage.Auto,
                    0));
        }

        var totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
        if (totalIBSize > _indexBuffer.SizeInBytes)
        {
            _indexBuffer.Dispose();
            _indexBuffer = gd.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(totalIBSize * 1.5f),
                    VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst,
                    VmaMemoryUsage.Auto,
                    0));
        }

        for (var i = 0; i < draw_data.CmdListsCount; i++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[i];

            void* vtxPtr = (void*)cmd_list.VtxBuffer.Data;
            void* idxPtr = (void*)cmd_list.IdxBuffer.Data;

            uint vtxSize = (uint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert));
            uint idxSize = (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort));

            cl.UpdateBuffer(
                _vertexBuffer,
                vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                (IntPtr)vtxPtr,
                vtxSize);

            cl.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                (IntPtr)idxPtr,
                idxSize);

            vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
            indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
        }

        if (draw_data.CmdListsCount > 0)
        {
            cl.Barrier(VkPipelineStageFlags2.Transfer,
                VkAccessFlags2.TransferWrite,
                VkPipelineStageFlags2.VertexInput,
                VkAccessFlags2.VertexAttributeRead | VkAccessFlags2.IndexRead);
        }

        // Setup orthographic projection matrix into our constant buffer
        {
            ImGuiIOPtr io = ImGui.GetIO();

            var mvp = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            _gd.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);
        }

        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, VkIndexType.Uint16);
        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, _mainResourceSet);

        draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        // Render command lists
        var vtx_offset = 0;
        var idx_offset = 0;
        for (var n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            for (var cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmd pcmd = cmd_list.CmdBuffer.Data[cmd_i];

                if (pcmd.UserCallback != null)
                {
                    throw new NotImplementedException();
                }

                //cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                /*if (pcmd.TextureId != IntPtr.Zero)
                    {
                        if (pcmd.TextureId == _fontAtlasID)
                        {
                            cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                        }
                        else
                        {
                            cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                        }
                    }*/
                Renderer.GlobalTexturePool.BindTexturePool(cl, 1);

                cl.SetScissorRect(
                    0,
                    (uint)Math.Max(pcmd.ClipRect.X, 0),
                    (uint)Math.Max(pcmd.ClipRect.Y, 0),
                    (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                    (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                cl.DrawIndexed(pcmd.ElemCount, 1, (uint)idx_offset + pcmd.IdxOffset, vtx_offset,
                    (uint)pcmd.TextureId.Handle);
            }

            idx_offset += cmd_list.IdxBuffer.Size;
            vtx_offset += cmd_list.VtxBuffer.Size;
        }
    }

    private struct ResourceSetInfo
    {
        public readonly IntPtr ImGuiBinding;
        public readonly ResourceSet ResourceSet;

        public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
        {
            ImGuiBinding = imGuiBinding;
            ResourceSet = resourceSet;
        }
    }
}
