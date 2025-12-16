using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Editors.Viewport;

public interface IViewport
{
    public ViewportCamera ViewportCamera { get; }

    public int Width { get; }
    public int Height { get; }

    public float NearClip { get; }
    public float FarClip { get; }

    public bool IsViewportSelected { get; set; }

    public void OnGui();
    public void ResizeViewport(GraphicsDevice device, Rectangle newvp);
    public bool Update(Sdl2Window window, float dt);
    public void Draw(GraphicsDevice device, CommandList cl);
    public void SetEnvMap(uint index);
    public void FrameBox(BoundingBox box, Vector3 offset, float distance = 5);
    public void FramePosition(Vector3 pos, float dist);
}
