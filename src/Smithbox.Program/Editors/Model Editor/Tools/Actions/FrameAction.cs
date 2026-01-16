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
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public FrameAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (Editor.ViewportSelection.IsSelection())
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
        var offset = CFG.Current.ModelEditor_FrameInViewport_Offset;
        var distance = CFG.Current.ModelEditor_FrameInViewport_Distance;

        if (Editor.ViewportSelection.IsSelection())
        {
            HashSet<Entity> selected = Editor.ViewportSelection.GetFilteredSelection<Entity>();

            var entity = selected.FirstOrDefault();

            if (entity != null && entity.RenderSceneMesh != null)
            {
                Editor.ModelViewportView.Viewport.FrameBox(entity.RenderSceneMesh.GetBounds(), offset, distance);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void FrameCurrentEntity(Entity entity)
    {
        var offset = CFG.Current.ModelEditor_FrameInViewport_Offset;
        var distance = CFG.Current.ModelEditor_FrameInViewport_Distance;

        if (entity != null && entity.RenderSceneMesh != null)
        {
            Editor.ModelViewportView.Viewport.FrameBox(entity.RenderSceneMesh.GetBounds(), offset, distance);
        }
    }
}
