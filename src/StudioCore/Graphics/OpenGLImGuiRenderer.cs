using ImGuiNET;
using Silk.NET.OpenGL;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore.Graphics;

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

    private int _firstFrame = 0;
    private uint _fontTexture;

    private bool _frameBegun;
    private uint _indexBufferSize;
    private bool _shiftDown;

    private uint _vertexArray;
    private uint _vertexBufferSize;
    private int _windowHeight;

    private int _windowWidth;

    public OpenGLImGuiRenderer(GL gl, int width, int height, ColorSpaceHandling colorSpaceHandling)
    {
        GL = gl;
        _assembly = typeof(VulkanImGuiRenderer).GetTypeInfo().Assembly;
        _colorSpaceHandling = colorSpaceHandling;
        _windowWidth = width;
        _windowHeight = height;

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        ImGui.GetIO().Fonts.AddFontDefault();

        SetOpenTKKeyMappings();

        SetPerFrameImGuiData(1f / 60f);

        _vertexBufferSize = 10000;
        _indexBufferSize = 3000;

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

    public void OnSetupDone()
    {
        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        // Build
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height, out var bytesPerPixel);

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
        io.Fonts.SetTexID((IntPtr)_fontTexture);
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
            Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
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
            Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
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

    private void UpdateImGuiInput(InputSnapshot snapshot)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.
        var leftPressed = false;
        var middlePressed = false;
        var rightPressed = false;
        for (var i = 0; i < snapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = snapshot.MouseEvents[i];
            if (me.Down)
            {
                switch (me.MouseButton)
                {
                    case MouseButton.Left:
                        leftPressed = true;
                        break;
                    case MouseButton.Middle:
                        middlePressed = true;
                        break;
                    case MouseButton.Right:
                        rightPressed = true;
                        break;
                }
            }
        }

        io.MouseDown[0] = leftPressed || snapshot.IsMouseDown(MouseButton.Left);
        io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
        io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
        io.MousePos = snapshot.MousePosition;
        io.MouseWheel = snapshot.WheelDelta;

        IReadOnlyList<char> keyCharPresses = snapshot.KeyCharPresses;
        for (var i = 0; i < keyCharPresses.Count; i++)
        {
            var c = keyCharPresses[i];
            ImGui.GetIO().AddInputCharacter(c);
        }

        IReadOnlyList<KeyEvent> keyEvents = snapshot.KeyEvents;
        for (var i = 0; i < keyEvents.Count; i++)
        {
            KeyEvent keyEvent = keyEvents[i];
            io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
            if (keyEvent.Key == Key.ControlLeft || keyEvent.Key == Key.ControlRight)
            {
                _controlDown = keyEvent.Down;
            }

            if (keyEvent.Key == Key.ShiftLeft || keyEvent.Key == Key.ShiftRight)
            {
                _shiftDown = keyEvent.Down;
            }

            if (keyEvent.Key == Key.AltLeft || keyEvent.Key == Key.AltRight)
            {
                _altDown = keyEvent.Down;
            }
        }

        io.KeyCtrl = _controlDown;
        io.KeyAlt = _altDown;
        io.KeyShift = _shiftDown;
    }

    private static void SetOpenTKKeyMappings()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
        io.KeyMap[(int)ImGuiKey.KeypadEnter] = (int)Key.KeypadEnter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
        io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        io.KeyMap[(int)ImGuiKey.Space] = (int)Key.Space;
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
            ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

            GL.BufferData(GLEnum.ArrayBuffer, (nuint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)),
                (void*)cmd_list.VtxBuffer.Data, GLEnum.StreamDraw);
            GL.BufferData(GLEnum.ElementArrayBuffer, (nuint)(cmd_list.IdxBuffer.Size * sizeof(ushort)),
                (void*)cmd_list.IdxBuffer.Data, GLEnum.StreamDraw);

            for (var cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
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
                    GL.BindTexture(TextureTarget.Texture2D, (uint)pcmd.TextureId);

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
