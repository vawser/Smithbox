using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

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
        if (ImGui.Selectable("Frame in Viewport"))
        {
            ApplyViewportFrame();
        }
        UIHelper.Tooltip($"Frames the current selection in the viewport.\n\nShortcut: {InputManager.GetHint(KeybindID.Frame)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Frame Selected in Viewport", InputManager.GetHint(KeybindID.Frame)))
        {
            ApplyViewportFrame();
        }
        UIHelper.Tooltip("Frames the current selection in the viewport.");
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

            var entity = selected.FirstOrDefault();

            if (entity != null && entity.RenderSceneMesh != null)
            {
                View.ViewportWindow.Viewport.FrameBox(entity.RenderSceneMesh.GetBounds(), offset, distance);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void FrameCurrentEntity(Entity entity)
    {
        var offset = CFG.Current.Viewport_Frame_Offset;
        var distance = CFG.Current.Viewport_Frame_Distance;

        if (entity != null && entity.RenderSceneMesh != null)
        {
            View.ViewportWindow.Viewport.FrameBox(entity.RenderSceneMesh.GetBounds(), offset, distance);
        }
    }
}
