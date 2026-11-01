using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.TimeActEditor;
public class TimeActViewportWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    // The model to frame once its meshes have loaded
    public TimeActContainer PendingFrameContainer;

    public TimeActViewportWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Rect = view.Window.Bounds;

        if (view.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(View.Universe, $"TimeActViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
            else
            {
                Viewport = new NullViewport(View.Universe, $"TimeActViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
        }

    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
        {
            vulkanViewport.Display();

            UpdatePendingFrame();
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
        if (Viewport != null)
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
        Viewport.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport.IsViewportSelected;
    }

    public void UpdateDisplayNodes()
    {
        var wrapper = View.Selection.SelectedTimeActModelWrapper;

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
