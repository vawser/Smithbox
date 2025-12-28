using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using Silk.NET.OpenGL;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace StudioCore.Renderer;

public unsafe class OpenGLImGuiRenderer : IImguiRenderer
{
    private readonly Assembly _assembly;
    private readonly uint _indexBuffer;
    private readonly Vector2 _scaleFactor = Vector2.One;
    private readonly uint _shader;
    private readonly int _shaderFontTextureLocation;
    private readonly int _shaderProjectionMatrixLocation;
    private readonly uint _vertexBuffer;
    private readonly GL GL;
    private bool _altDown;
    private ColorSpaceHandling _colorSpaceHandling;

    private bool _controlDown;

    //private int _firstFrame = 0;
    private uint _fontTexture;

    private bool _frameBegun;
    //private uint _indexBufferSize;
    private bool _shiftDown;

    private uint _vertexArray;
    //private uint _vertexBufferSize;
    private int _windowHeight;

    private int _windowWidth;

    private ImGuiContextPtr guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr plotContext;

    public OpenGLImGuiRenderer(GL gl, int width, int height, ColorSpaceHandling colorSpaceHandling)
    {
        GL = gl;
        _assembly = typeof(VulkanImGuiRenderer).GetTypeInfo().Assembly;
        _colorSpaceHandling = colorSpaceHandling;
        _windowWidth = width;
        _windowHeight = height;

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
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        io.ConfigDebugHighlightIdConflicts = false;
#if DEBUG
        io.ConfigDebugHighlightIdConflicts = true;
#endif

        unsafe
        {
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            platformIO.PlatformSetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_setClipboardTextCallback);
            platformIO.PlatformGetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(_getClipboardTextCallback);
            platformIO.PlatformClipboardUserData = (void*)GCHandle.ToIntPtr(_clipboardUserDataHandle);
        }

        ImGui.GetIO().Fonts.AddFontDefault();

        SetPerFrameImGuiData(1f / 60f);

        //_vertexBufferSize = 10000;
        //_indexBufferSize = 3000;

        _vertexBuffer = GL.GenBuffer();
        _indexBuffer = GL.GenBuffer();

        var vertexSource = @"#version 330 core
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
uniform mat4 projection_matrix;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";

        var fragmentSource = @"#version 330 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
layout (location = 0) out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

        _shader = CreateProgram("ImGui", vertexSource, fragmentSource);
        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");
    }


    private static SetClipboardTextCallback _setClipboardTextCallback = SetClipboardText;
    private static GetClipboardTextCallback _getClipboardTextCallback = GetClipboardText;
    private static readonly GCHandle _clipboardUserDataHandle = GCHandle.Alloc(new object());

    public void OnSetupDone()
    {
        ImGui.NewFrame();
        _frameBegun = true;
    }

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

    public void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        byte* pixels;
        int width;
        int height;
        int bytesPerPixel;

        // Build
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height, &bytesPerPixel);

        var mips = (uint)Math.Floor(Math.Log(Math.Max(width, height), 2));
        GL.ActiveTexture(TextureUnit.Texture0);
        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexStorage2D(TextureTarget.Texture2D, mips, SizedInternalFormat.Rgba8, (uint)width, (uint)height);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)width, (uint)height, PixelFormat.Bgra,
            PixelType.UnsignedByte, pixels);
        GL.GenerateTextureMipmap(_fontTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        // Store our identifier
        io.Fonts.SetTexID(_fontTexture);
        io.Fonts.ClearTexData();
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

    private uint CreateProgram(string name, string vertexSource, string fragmentSource)
    {
        var program = GL.CreateProgram();

        var vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSource);

        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);

        GL.LinkProgram(program);

        GL.GetProgram(program, GLEnum.LinkStatus, out var success);
        if (success == 0)
        {
            var info = GL.GetProgramInfoLog(program);
        }

        GL.DetachShader(program, vertex);
        GL.DetachShader(program, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }

    private uint CompileShader(string name, ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, GLEnum.CompileStatus, out var success);
        if (success == 0)
        {
            var info = GL.GetShaderInfoLog(shader);
        }

        return shader;
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    /// <summary>
    ///     Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
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
            Key.WinLeft => ImGuiKey.LeftShift,
            Key.WinRight => ImGuiKey.RightShift,
            Key.F1 => ImGuiKey.F1,
            Key.F2 => ImGuiKey.F2,
            Key.F3 => ImGuiKey.F3,
            Key.F4 => ImGuiKey.F4,
            Key.F5 => ImGuiKey.F5,
            Key.F6 => ImGuiKey.F6,
            Key.F7 => ImGuiKey.F7,
            Key.F8 => ImGuiKey.F8,
            Key.F9 => ImGuiKey.F9,
            Key.F10 => ImGuiKey.F10,
            Key.F11 => ImGuiKey.F11,
            Key.F12 => ImGuiKey.F12,
            Key.F13 => ImGuiKey.F13,
            Key.F14 => ImGuiKey.F14,
            Key.F15 => ImGuiKey.F15,
            Key.F16 => ImGuiKey.F16,
            Key.F17 => ImGuiKey.F17,
            Key.F18 => ImGuiKey.F18,
            Key.F19 => ImGuiKey.F19,
            Key.F20 => ImGuiKey.F20,
            Key.F21 => ImGuiKey.F21,
            Key.F22 => ImGuiKey.F22,
            Key.F23 => ImGuiKey.F23,
            Key.F24 => ImGuiKey.F24,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.NumLock => ImGuiKey.NumLock,
            Key.Keypad0 => ImGuiKey.Keypad0,
            Key.Keypad1 => ImGuiKey.Keypad1,
            Key.Keypad2 => ImGuiKey.Keypad2,
            Key.Keypad3 => ImGuiKey.Keypad3,
            Key.Keypad4 => ImGuiKey.Keypad4,
            Key.Keypad5 => ImGuiKey.Keypad5,
            Key.Keypad6 => ImGuiKey.Keypad6,
            Key.Keypad7 => ImGuiKey.Keypad7,
            Key.Keypad8 => ImGuiKey.Keypad8,
            Key.Keypad9 => ImGuiKey.Keypad9,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            _ => ImGuiKey.None,
        };
    }

    private void RenderImDrawData(ImDrawDataPtr draw_data)
    {
        if (draw_data.CmdListsCount == 0)
        {
            return;
        }

        var framebufferWidth = (int)(draw_data.DisplaySize.X * draw_data.FramebufferScale.X);
        var framebufferHeight = (int)(draw_data.DisplaySize.Y * draw_data.FramebufferScale.Y);
        if (framebufferWidth <= 0 || framebufferHeight <= 0)
        {
            return;
        }

        GL.Enable(GLEnum.Blend);
        GL.Enable(GLEnum.ScissorTest);
        GL.BlendEquation(GLEnum.FuncAdd);
        GL.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.StencilTest);

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        GL.Viewport(0, 0, (uint)framebufferWidth, (uint)framebufferHeight);
        var L = draw_data.DisplayPos.X;
        var R = draw_data.DisplayPos.X + draw_data.DisplaySize.X;
        var T = draw_data.DisplayPos.Y;
        var B = draw_data.DisplayPos.Y + draw_data.DisplaySize.Y;
        var mvp = stackalloc float[]
        {
            2.0f / (R - L), 0.0f, 0.0f, 0.0f, 0.0f, 2.0f / (T - B), 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f,
            (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f
        };
        GL.UseProgram(_shader);
        GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, new Span<float>(mvp, 16));
        GL.Uniform1(_shaderFontTextureLocation, 0);
        GL.BindSampler(0, 0);

        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);

        GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBuffer);
        GL.BindBuffer(GLEnum.ElementArrayBuffer, _indexBuffer);
        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);
        var stride = (uint)sizeof(ImDrawVert);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, (void*)16);

        Vector2 clipOffset = draw_data.DisplayPos;
        Vector2 clipScale = draw_data.FramebufferScale;

        // Render command lists
        for (var n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            GL.BufferData(GLEnum.ArrayBuffer, (nuint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)),
                (void*)cmd_list.VtxBuffer.Data, GLEnum.StreamDraw);
            GL.BufferData(GLEnum.ElementArrayBuffer, (nuint)(cmd_list.IdxBuffer.Size * sizeof(ushort)),
                (void*)cmd_list.IdxBuffer.Data, GLEnum.StreamDraw);

            for (var cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmd pcmd = cmd_list.CmdBuffer.Data[cmd_i];
                if (pcmd.UserCallback != null)
                {
                    throw new NotImplementedException();
                }

                Vector4 clipRect;
                clipRect.X = (pcmd.ClipRect.X - clipOffset.X) * clipScale.X;
                clipRect.Y = (pcmd.ClipRect.Y - clipOffset.Y) * clipScale.Y;
                clipRect.Z = (pcmd.ClipRect.Z - clipOffset.X) * clipScale.X;
                clipRect.W = (pcmd.ClipRect.W - clipOffset.Y) * clipScale.Y;

                if (clipRect.X < framebufferWidth && clipRect.Y < framebufferHeight && clipRect.Z >= 0.0f &&
                    clipRect.W >= 0.0f)
                {
                    GL.Scissor((int)clipRect.X, framebufferHeight - (int)clipRect.W,
                        (uint)(clipRect.Z - clipRect.X), (uint)(clipRect.W - clipRect.Y));

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, ((uint)pcmd.TextureId.Handle));

                    if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                    {
                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, pcmd.ElemCount,
                            DrawElementsType.UnsignedShort, (void*)(pcmd.IdxOffset * sizeof(ushort)),
                            (int)pcmd.VtxOffset);
                    }
                    else
                    {
                        GL.DrawElements(PrimitiveType.Triangles, pcmd.ElemCount, DrawElementsType.UnsignedShort,
                            (void*)(pcmd.IdxOffset * sizeof(ushort)));
                    }
                }
            }
        }

        GL.DeleteVertexArray(_vertexArray);
        _vertexArray = 0;
    }
}
