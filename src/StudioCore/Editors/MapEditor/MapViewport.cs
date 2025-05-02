using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Scene;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.MapEditorNS;

public class MapViewport
{
    public MapEditor Editor;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public MapViewport(MapEditor editor)
    {
        Editor = editor;

        Rect = Editor.BaseEditor.GraphicsContext.Window.Bounds;

        if (Editor.BaseEditor.GraphicsContext.Device != null)
        {
            Viewport = new Viewport((IEditor)this, "MapEditorViewport", Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport((IEditor)this, "MapEditorNullViewport", Rect.Width, Rect.Height);
        }
    }

    public void OnGui()
    {
        Viewport.OnGui();
    }

    public void Update(float deltatime)
    {
        ViewportUsingKeyboard = Viewport.Update(Editor.Window, deltatime);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Editor.Window = window;
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
