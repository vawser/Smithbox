using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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

    // The model to frame once its meshes have loaded
    public ModelContainer PendingFrameContainer;

    // Most models are a few metres across (a character is about 2 m), so the shared speeds (20 m/s normal) are slowed to about 5 m/s
    private const float MoveSpeedFactor = 0.25f;

    public ModelViewportWindow(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Rect = view.Window.Bounds;

        // Without a renderer the viewport is still shown, as a disabled placeholder
        if (Smithbox.Instance.HasRenderer)
        {
            var vk = new VulkanViewport(View.Universe, $"ModelViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            vk.ViewportCamera.MoveSpeedFactor = MoveSpeedFactor;
            Viewport = vk;
        }
        else
        {
            Viewport = new NullViewport(View.Universe, $"ModelViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
        }
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
        {
            vulkanViewport.Display();

            UpdatePendingFrame();

            // The loaded model's EDGE file, drawn live (EDGE Editor)
            View.EdgeEditorTool.UpdateMarkers();
        }

        if (Viewport is NullViewport nullViewport)
        {
            nullViewport.Display();
        }
    }

    /// <summary>
    /// Mesh bounds are only known once the resource manager has committed the model,
    /// which happens on the main thread some frames after the load was requested.
    /// </summary>
    private void UpdatePendingFrame()
    {
        if (PendingFrameContainer == null)
            return;

        if (PendingFrameContainer.TryGetMeshBounds(out var bounds))
        {
            Viewport.FitBox(bounds);

            PendingFrameContainer = null;
        }
    }

    public void Update(float deltatime)
    {
        if(Viewport != null)
        {
            ViewportUsingKeyboard = Viewport.Update(View.Window, deltatime);
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport?.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport?.IsViewportSelected ?? false;
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
            obj.RenderSceneMesh = null;
            container.AssignDummyDrawable(obj, wrapper);
        }

        foreach (var obj in container.Nodes)
        {
            obj.RenderSceneMesh = null;
            container.AssignNodeDrawable(obj, wrapper);
        }
    }
}
