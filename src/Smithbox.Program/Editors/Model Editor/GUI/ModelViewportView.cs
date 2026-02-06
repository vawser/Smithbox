using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportWindow
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public ModelViewportWindow(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Rect = view.Window.Bounds;

        if (view.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(View.Universe, $"ModelViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
            else
            {
                Viewport = new NullViewport(View.Universe, $"ModelViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
        }

    }

    public void Display()
    {
        if (Viewport is VulkanViewport vulkanViewport)
        {
            vulkanViewport.Display();
        }

        if (Viewport is NullViewport nullViewport)
        {
            nullViewport.Display();
        }
    }

    public void Update(float deltatime)
    {
        ViewportUsingKeyboard = Viewport.Update(View.Window, deltatime);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.Window = window;
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

    public void UpdateDisplayNodes()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper == null)
            return;

        var container = wrapper.Container;

        if (container == null)
            return;

        foreach (var obj in container.Dummies)
        {
            obj.RenderSceneMesh.Dispose();
            container.AssignDummyDrawable(obj, wrapper);
        }

        foreach (var obj in container.Nodes)
        {
            obj.RenderSceneMesh.Dispose();
            container.AssignNodeDrawable(obj, wrapper);
        }
    }
}
