using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimEditor;

public class AnimViewportWindow
{
    public AnimEditorView View;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public AnimViewportWindow(AnimEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Rect = view.Window.Bounds;

        if (view.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(View.Universe, $"AnimViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
            else
            {
                Viewport = new NullViewport(View.Universe, $"AnimViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
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
}
