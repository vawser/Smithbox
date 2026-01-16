using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public class FrameAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public FrameAction(MapEditorScreen editor, ProjectEntry project)
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
            if (InputManager.IsPressed(InputAction.Frame))
            {
                ApplyViewportFrame();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ent.WrappedObject is IMsbPart or IMsbRegion)
        {
            if (ImGui.Selectable("Frame Selection"))
            {
                ApplyViewportFrame();
            }
            UIHelper.Tooltip($"Frames the current selection in the viewport.\n\nShortcut: {InputManager.GetHint(InputAction.Frame)}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Frame Selection", InputManager.GetHint(InputAction.Frame)))
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
        if (Editor.ViewportSelection.IsSelection())
        {
            HashSet<Entity> selected = Editor.ViewportSelection.GetFilteredSelection<Entity>();
            var first = false;
            BoundingBox box = new();

            foreach (Entity s in selected)
            {
                if (s.RenderSceneMesh != null)
                {
                    var framingBounds = BoundingBoxHelper.GetDerivedBoundingBox(s);

                    if (!first)
                    {
                        box = framingBounds;
                        first = true;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, framingBounds);
                    }
                }
                else if (s.Container.RootObject == s)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = s.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    if (!first)
                    {
                        first = true;
                        box = nodeBox;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, nodeBox);
                    }
                }
            }

            if (first)
            {
                Editor.MapViewportView.Viewport.FrameBox(box, new Vector3());
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void ApplyViewportFrameWithBox(BoundingBox box)
    {
        Editor.MapViewportView.Viewport.FrameBox(box, new Vector3());
    }
}