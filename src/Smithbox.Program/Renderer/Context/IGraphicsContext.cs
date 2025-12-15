using StudioCore.Application;
using System;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Renderer;

/// <summary>
///     Abstraction for a graphics context, which handles window creation, frame updates, drawing, ImGui rendering,
///     and swapchain/presentation
/// </summary>
public interface IGraphicsContext : IDisposable
{
    public Sdl2Window Window { get; }

    public IImguiRenderer ImguiRenderer { get; }

    public GraphicsDevice Device { get; }

    public void Initialize();

    public void Draw(ProjectManager projectManager);
}
