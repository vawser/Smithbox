using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Scene;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.MapEditor.Core;

public class MapViewportView
{
    public Smithbox BaseEditor;
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private Sdl2Window Window;
    private GraphicsDevice Device;
    public RenderScene RenderScene;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public MapViewportView(MapEditorScreen editor, ProjectEntry project, Smithbox baseEditor)
    {
        Editor = editor;
        Project = project;
        BaseEditor = baseEditor;

        Window = baseEditor._context.Window;
        Device = baseEditor._context.Device;

        Rect = Window.Bounds;

        if (Device != null)
        {
            RenderScene = new RenderScene();

            Viewport = new Viewport(BaseEditor, Editor, null, ViewportType.MapEditor, "Mapeditvp", Rect.Width, Rect.Height);

            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport(BaseEditor, Editor, null, ViewportType.MapEditor, "Mapeditvp", Rect.Width, Rect.Height);
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
