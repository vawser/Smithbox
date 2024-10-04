using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActViewport
{
    private TimeActEditorScreen Screen;

    public ViewportActionManager ViewportEditorActionManager = new();
    public ViewportSelection Selection = new();

    public Rectangle Rect;
    public Sdl2Window Window;
    public RenderScene RenderScene;
    public IViewport Viewport;

    public Universe Universe;
    public bool ViewportUsingKeyboard;

    public TimeActViewport(TimeActEditorScreen Screen, Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        Universe = new Universe(RenderScene, Selection);

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.TimeActEditor, "TimeActEditor_ViewPort", device, RenderScene, ViewportEditorActionManager, Selection, Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(ViewportType.TimeActEditor, "TimeActEditor_ViewPort", ViewportEditorActionManager, Selection, Rect.Width, Rect.Height);
        }
    }

    public void OnProjectChanged()
    {
        Universe.UnloadAll(true);
    }

    public void Update(float dt)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, dt);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

    public void Display()
    {

    }
}
