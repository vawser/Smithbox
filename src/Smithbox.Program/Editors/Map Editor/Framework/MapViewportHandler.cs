using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class MapViewportHandler
{
    public MapEditorView View;

    public List<ViewportWrapper> Viewports = new();
    public ViewportWrapper ActiveViewport;

    public ViewportWrapper ViewportToClose = null;

    public RenderScene RenderScene;

    public MapViewportHandler(MapEditorView view)
    {
        View = view;

        RenderScene = new();

        var viewportWrapper = new ViewportWrapper(view, RenderScene, 0);
        viewportWrapper.Setup();
        viewportWrapper.RenderScene.DrawFilter = CFG.Current.LastSceneFilter;

        Viewports = [viewportWrapper];
        ActiveViewport = viewportWrapper;
    }


    public void DisplayMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_Viewport")}##addNewViewport", false))
        {
            AddView();
        }

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_Axis_Viewports")}##addAxisViewports", false))
        {
            AddAxisViews();
        }
        GUI.Tooltip(LOC.Get("EDITOR_Add_Axis_Viewports_TT"));

        var canClose = CountViews() > 1;
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Close_Viewport")}##closeCurrentViewport", false, canClose))
        {
            if (CountViews() > 1)
            {
                RemoveView(ActiveViewport);
            }
        }
    }

    public ViewportWrapper AddView()
    {
        var index = 0;
        while (index < Viewports.Count)
        {
            if (Viewports[index] == null)
            {
                break;
            }

            index++;
        }

        ViewportWrapper view = new ViewportWrapper(View, RenderScene, index);
        view.Setup();

        if (index < Viewports.Count)
        {
            Viewports[index] = view;
        }
        else
        {
            Viewports.Add(view);
        }

        ActiveViewport = view;

        return view;
    }

    /// <summary>
    ///     Adds orthographic Top, Front and Right viewports looking at what the active one is focused on, at the same
    ///     scale, so with the active viewport they make the usual four-view layout. They open as windows of their own
    ///     to dock where wanted; the active viewport stays active.
    /// </summary>
    public void AddAxisViews()
    {
        var source = ActiveViewport;

        if (source?.Viewport is not VulkanViewport sourceViewport)
            return;

        var sourceCamera = sourceViewport.ViewportCamera;

        foreach (var preset in new[] { ViewportPresetView.Top, ViewportPresetView.Front, ViewportPresetView.Right })
        {
            var added = AddView();

            if (added.Viewport is not VulkanViewport viewport)
                continue;

            added.RenderScene.DrawFilter = source.RenderScene.DrawFilter;

            var camera = viewport.ViewportCamera;
            camera.CameraTransform = sourceCamera.CameraTransform;
            camera.FocusDistance = sourceCamera.FocusDistance;
            camera.OrthographicSize = sourceCamera.OrthographicSize;
            camera.ViewMode = sourceCamera.ViewMode;

            // Orthographic by hand, so rotating one of these doesn't hand it back to perspective
            camera.SetProjectionType(ViewMode.Orthographic);
            camera.SetPresetView(preset);
        }

        ActiveViewport = source;
    }

    public bool RemoveView(ViewportWrapper view)
    {
        if (!Viewports.Contains(view))
        {
            return false;
        }

        Viewports[view.ID] = null;

        // Its pipeline's queues would otherwise keep running and its targets stay allocated
        (view.Viewport as VulkanViewport)?.Dispose();

        if (view == ActiveViewport || ActiveViewport == null)
        {
            ActiveViewport = Viewports.FindLast(e => e != null);
        }

        return true;
    }

    /// <summary>
    /// Releases every viewport of the view, for a view that is being closed
    /// </summary>
    public void Dispose()
    {
        foreach (var wrapper in Viewports)
        {
            (wrapper?.Viewport as VulkanViewport)?.Dispose();
        }
    }

    public int CountViews()
    {
        return Viewports.Where(e => e != null).Count();
    }
}

public class ViewportWrapper
{
    public MapEditorView View;
    public RenderScene RenderScene;
    public IViewport Viewport;

    public int ID;

    public ViewportWrapper(MapEditorView view, RenderScene scene, int id)
    {
        View = view;
        ID = id;
        RenderScene = scene;
    }

    public void Setup()
    {
        if (View.Device != null)
        {
            var rect = View.Window.Bounds;

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(View.Universe, $"MapViewport_{View.ViewIndex}_{ID}", rect.Width, rect.Height, RenderScene);

            }
            else
            {
                Viewport = new NullViewport(View.Universe, $"MapViewport_{View.ViewIndex}_{ID}", rect.Width, rect.Height, RenderScene);
            }
        }
    }
}