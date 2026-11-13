using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class PullToCameraAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public PullToCameraAction(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (CanPull())
        {
            if (InputManager.IsPressed(KeybindID.Pull))
            {
                ApplyMoveToCamera();
            }
        }
    }

    /// <summary>
    /// Only dummies and bones have a position of their own to move
    /// </summary>
    private bool CanPull()
    {
        return View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform).Count > 0;
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Pull to Camera
        var flags = CanPull() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled;

        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Pull_Title")}##pullAction", false, flags))
        {
            ApplyMoveToCamera();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Pull_Context_TT", InputManager.GetHint(KeybindID.Pull)));
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Pull to Camera
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Pull_Title")}##pullAction", InputManager.GetHint(KeybindID.Pull),
            false, CanPull()))
        {
            ApplyMoveToCamera();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Pull_Menu_TT"));
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // NOT USED
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyMoveToCamera()
    {
        if (View.ViewportWindow.Viewport == null)
            return;

        if (CanPull())
        {
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, View.ViewportWindow.Viewport.ViewportCamera.CameraTransform.RotationMatrix);
            Vector3 camPos = View.ViewportWindow.Viewport.ViewportCamera.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

            // An object whose parent is also selected follows its parent, so it isn't moved itself
            var moved = sels.Where(sel =>
            {
                for (Entity p = sel.Parent; p != null; p = p.Parent)
                {
                    if (sels.Contains(p))
                        return false;
                }

                return true;
            }).ToList();

            // Get the accumulated center position of all selections, where they are drawn
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in moved)
            {
                if (GizmoState.Origin == GizmoState.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null &&
                    sel.WrappedObject is not SoulsFormats.FLVER2.Mesh)
                {
                    // Use bounding box origin as center
                    accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                }
                else
                {
                    // Use actual position as center (the middle of the geometry, for a mesh)
                    accumPos += sel.GetRootLocalTransform().Position;
                }
            }

            var center = accumPos / moved.Count;

            // The whole selection moves by the same step, as a gizmo drag from its centre to the camera would.
            // Each object carries the step into its own parent's space.
            var from = new Transform(center, Quaternion.Identity, Vector3.One);
            var to = new Transform(targetCamPos, Quaternion.Identity, Vector3.One);

            foreach (Entity sel in moved)
            {
                actlist.Add(sel.GetUpdateTransformAction(sel.ProjectGizmoDelta(from, to)));
            }

            if (actlist.Any())
            {
                ViewportCompoundAction action = new(actlist);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
        else
        {
            Smithbox.LogError<PullToCameraAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}
