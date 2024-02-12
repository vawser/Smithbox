using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Gui;

public interface IViewport
{
    public WorldView WorldView { get; }

    public int Width { get; }
    public int Height { get; }

    public float NearClip { get; set; }
    public float FarClip { get; }

    public bool ViewportSelected { get; }

    public void OnGui();
    public void SceneParamsGui();
    public void ResizeViewport(GraphicsDevice device, Rectangle newvp);
    public bool Update(Sdl2Window window, float dt);
    public void Draw(GraphicsDevice device, CommandList cl);
    public void SetEnvMap(uint index);
    public void FrameBox(BoundingBox box);
    public void FramePosition(Vector3 pos, float dist);
}
