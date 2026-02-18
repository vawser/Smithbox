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
    public MapEditorView View;
    public ProjectEntry Project;

    public RenderTypeAction(MapEditorView view, ProjectEntry project)
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
            if (InputManager.IsPressed(KeybindID.MapEditor_Cycle_Render_Type))
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
                UIHelper.Tooltip($"Toggles the rendering style for the current selection.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Cycle_Render_Type)}");
            }
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Toggle Render Type", InputManager.GetHint(KeybindID.MapEditor_Cycle_Render_Type)))
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
        View.ViewportSelection.StoreSelection();
        var sel = View.ViewportSelection.GetSelection();

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

        View.ViewportSelection.ResetSelection();

        View.DelayPicking();
    }
}
