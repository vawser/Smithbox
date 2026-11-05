using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

public class FrameAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public FrameAction(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Frame))
            {
                ApplyViewportFrame();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Frame
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Frame_Context_Title")}##frameAction"))
        {
            ApplyViewportFrame();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Frame_Context_TT", InputManager.GetHint(KeybindID.Frame)));
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Frame Selected in Viewport
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Frame_Menu_Title")}##frameAction", InputManager.GetHint(KeybindID.Frame)))
        {
            ApplyViewportFrame();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Frame_Menu_TT"));
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyViewportFrame()
    {
        var offset = CFG.Current.Viewport_Frame_Offset;
        var distance = CFG.Current.Viewport_Frame_Distance;

        if (View.ViewportSelection.IsSelection())
        {
            HashSet<Entity> selected = View.ViewportSelection.GetFilteredSelection<Entity>();

            // Frames all of the selection together
            BoundingBox? bounds = null;

            foreach (var entity in selected)
            {
                if (TryGetBounds(entity, out var entityBounds))
                {
                    bounds = bounds == null ? entityBounds : BoundingBox.Combine(bounds.Value, entityBounds);
                }
            }

            if (bounds != null)
            {
                View.ViewportWindow.Viewport?.FrameBox(bounds.Value, offset, distance);
            }
        }
        else
        {
            Smithbox.LogError<FrameAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }

    public void FrameCurrentEntity(Entity entity)
    {
        var offset = CFG.Current.Viewport_Frame_Offset;
        var distance = CFG.Current.Viewport_Frame_Distance;

        if (TryGetBounds(entity, out var bounds))
        {
            View.ViewportWindow.Viewport?.FrameBox(bounds, offset, distance);
        }
    }

    /// <summary>
    /// The bounds of an entity in the viewport. The model root has no mesh of its own, so it uses the bounds of the
    /// whole model, which are only known once its meshes have loaded.
    /// </summary>
    private bool TryGetBounds(Entity entity, out BoundingBox bounds)
    {
        bounds = default;

        if (entity == null)
            return false;

        if (entity.Container is ModelContainer container && entity == container.RootObject)
        {
            return container.TryGetMeshBounds(out bounds);
        }

        if (entity.RenderSceneMesh == null)
            return false;

        bounds = entity.RenderSceneMesh.GetBounds();

        return true;
    }
}
