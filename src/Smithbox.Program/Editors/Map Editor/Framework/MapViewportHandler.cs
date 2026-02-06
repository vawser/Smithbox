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
        if (ImGui.MenuItem("New Viewport"))
        {
            AddView();
        }

        if (ImGui.MenuItem("Close Current Viewport"))
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

    public bool RemoveView(ViewportWrapper view)
    {
        if (!Viewports.Contains(view))
        {
            return false;
        }

        Viewports[view.ID] = null;

        if (view == ActiveViewport || ActiveViewport == null)
        {
            ActiveViewport = Viewports.FindLast(e => e != null);
        }

        return true;
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