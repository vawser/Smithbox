using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;

namespace StudioCore.Editors.MapEditor;

public class RenderTypeAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public RenderTypeAction(MapEditorScreen editor, ProjectEntry project)
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
            if (InputManager.IsPressed(InputAction.MapEditor_Cycle_Render_Type))
            {
                ApplyRenderTypeToggle();
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
            if (ent.WrappedObject is IMsbRegion or BTL.Light)
            {
                if (ImGui.Selectable("Toggle Render Type"))
                {
                    ApplyRenderTypeToggle();
                }
                UIHelper.Tooltip($"Toggles the rendering style for the current selection.\n\nShortcut: {InputManager.GetHint(InputAction.MapEditor_Cycle_Render_Type)}");
            }
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Toggle Render Type", InputManager.GetHint(InputAction.MapEditor_Cycle_Render_Type)))
        {
            ApplyRenderTypeToggle();
        }
        UIHelper.Tooltip("Toggle the render type of the current selection.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // Not shown here
    }


    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyRenderTypeToggle()
    {
        Editor.ViewportSelection.StoreSelection();
        var sel = Editor.ViewportSelection.GetSelection();

        foreach (var entry in sel)
        {
            var ent = (Entity)entry;

            if (ent is MsbEntity)
            {
                var mEnt = (MsbEntity)ent;

                if (!mEnt.IsSwitchingRenderType &&
                    (mEnt.Type is MsbEntityType.Region || mEnt.Type is MsbEntityType.Light))
                {
                    mEnt.SwitchRenderType();
                }
            }
        }

        Editor.ViewportSelection.ResetSelection(Editor);
    }
}
