using StudioCore.Interface;
using StudioCore.Scene;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.MapEditor.Core;

public class MapViewportView
{
    private MapEditorScreen Screen;
    private Sdl2Window Window;
    private GraphicsDevice Device;
    public RenderScene RenderScene;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public MapViewportView(MapEditorScreen screen, Sdl2Window window, GraphicsDevice device)
    {
        Screen = screen;
        Window = window;
        Device = device;

        Rect = window.Bounds;

        if (device != null)
        {
            RenderScene = new RenderScene();

            Viewport = new Viewport(ViewportType.MapEditor, "Mapeditvp", device, RenderScene, screen.EditorActionManager, screen.Selection, Rect.Width, Rect.Height);
            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport(ViewportType.MapEditor, "Mapeditvp", screen.EditorActionManager, screen.Selection, Rect.Width, Rect.Height);
        }
    }

    public void OnGui()
    {
        Viewport.OnGui();
    }

    public void OnProjectChanged()
    {

    }

    public void Update(float deltatime)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, deltatime);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport.Draw(device, cl);
    }
    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }
}
