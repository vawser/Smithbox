using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportView
{
    public Smithbox BaseEditor;

    public ModelEditorScreen Editor;
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

    public ModelViewportView(ModelEditorScreen editor, ProjectEntry project, Smithbox baseEditor)
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
        }
    }

    public void Setup()
    {
        if (Device != null && !Smithbox.LowRequirementsMode)
        {
            Viewport = new StudioCore.Editors.Viewport.Viewport(BaseEditor, null, Editor, ViewportType.ModelEditor, 
                "Modeleditvp", Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(BaseEditor, null, Editor, ViewportType.ModelEditor, 
                "Modeleditvp", Rect.Width, Rect.Height);
        }
    }

    public void OnGui()
    {
        Viewport.OnGui();
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
        return Viewport.IsViewportSelected;
    }
}
