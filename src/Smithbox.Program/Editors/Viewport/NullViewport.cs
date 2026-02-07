using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Editors.Viewport;

/// <summary>
///     A null viewport that doesn't actually do anything
/// </summary>
public class NullViewport : IViewport
{
    public IUniverse Owner;

    public int X;
    public int Y;

    public string ID { get; set; }

    public NullViewport(IUniverse owner, string id, int width, int height, RenderScene scene)
    {
        ID = id;

        Owner = owner;

        Width = width;
        Height = height;

        ViewportCamera = new ViewportCamera(this, new Rectangle(0, 0, Width, Height));
    }

    public ViewportCamera ViewportCamera { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public float NearClip { get; set; } = 0.1f;
    public float FarClip { get; set; } = CFG.Current.Viewport_Perspective_Far_Clip;

    public bool IsViewportSelected { get; set; }

    public void Display()
    {
        if (ImGui.Begin($@"Viewport##{ID}", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav))
        {
            Vector2 p = ImGui.GetWindowPos();
            Vector2 s = ImGui.GetWindowSize();
            var newvp = new Rectangle((int)p.X, (int)p.Y + 3, (int)s.X, (int)s.Y - 3);
            ResizeViewport(null, newvp);
            ImGui.Text("Disabled...");
        }

        ImGui.End();
    }

    public void SceneParamsGui()
    {
    }

    public void ResizeViewport(GraphicsDevice device, Rectangle newvp)
    {
        Width = newvp.Width;
        Height = newvp.Height;
        X = newvp.X;
        Y = newvp.Y;
        ViewportCamera.UpdateBounds(newvp);
    }

    public bool Update(Sdl2Window window, float dt)
    {
        return false;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
    }

    public void SetEnvMap(uint index)
    {
    }

    public void FrameBox(BoundingBox box, Vector3 offset, float distance = 5)
    {
    }

    public void FramePosition(Vector3 pos, float dist)
    {
    }
}
