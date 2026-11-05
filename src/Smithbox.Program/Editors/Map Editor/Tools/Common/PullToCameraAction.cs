using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class PullToCameraAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public PullToCameraAction(MapEditorView view, ProjectEntry project)
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
            if (InputManager.IsPressed(KeybindID.Pull))
            {
                ApplyMoveToCamera();
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
            if (ImGui.Selectable("Pull Selection"))
            {
                ApplyMoveToCamera();
            }
            GUI.Tooltip($"Move the current selection to the camera position.\n\nShortcut: {InputManager.GetHint(KeybindID.Pull)}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Pull Selection", InputManager.GetHint(KeybindID.Pull)))
        {
            ApplyMoveToCamera();
        }
        GUI.Tooltip("Move the current selection to the camera position.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        GUI.WrappedText("Configure how the pull to camera action works.");

        GUI.Spacer();
        GUI.SimpleHeader("Camera Offset Distance", "Camera Offset Distance", "", UI.Current.ImGui_Default_Text_Color);

        GUI.SetInputWidth();
        if (ImGui.SliderFloat("##Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100))
        {
            if (CFG.Current.Toolbar_Move_to_Camera_Offset < 0)
                CFG.Current.Toolbar_Move_to_Camera_Offset = 0;

            if (CFG.Current.Toolbar_Move_to_Camera_Offset > 100)
                CFG.Current.Toolbar_Move_to_Camera_Offset = 100;
        }
        GUI.Tooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("pullToCameraActions",
            "applyPull", "Pull Selection", "", ApplyMoveToCamera);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyMoveToCamera()
    {
        if (View.ViewportSelection.IsSelection())
        {
            HashSet<Entity> sels = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, View.ViewportHandler.ActiveViewport.Viewport.ViewportCamera.CameraTransform.RotationMatrix);
            Vector3 camPos = View.ViewportHandler.ActiveViewport.Viewport.ViewportCamera.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

            var action = BuildMoveToPointAction(sels, targetCamPos,
                GizmoState.Origin == GizmoState.GizmosOrigin.BoundingBox);

            if (action != null)
            {
                View.ViewportActionManager.ExecuteAction(action);
            }
        }
        else
        {
            Smithbox.LogError<PullToCameraAction>("No object selected.");
        }

        View.DelayPicking();
    }

    /// <summary>
    /// An action that moves the entities so the centre of the group lands on the world position, keeping their
    /// layout and rotations. The centre is their mean position, or the mean of their bounds' centres.
    /// Null if none of them has a transform.
    /// </summary>
    public static ViewportAction BuildMoveToPointAction(IEnumerable<Entity> entities, Vector3 targetWorld, bool useBoundsCenter = false)
    {
        var sels = entities.Where(e => e.HasTransform).ToList();

        if (sels.Count == 0)
            return null;

        // Get the accumulated center position of all selections
        Vector3 accumPos = Vector3.Zero;
        foreach (Entity sel in sels)
        {
            if (useBoundsCenter && sel.RenderSceneMesh != null)
            {
                // Use bounding box origin as center
                accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
            }
            else
            {
                // Use actual position as center
                accumPos += sel.GetRootLocalTransform().Position;
            }
        }

        Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

        List<ViewportAction> actlist = new();

        // Offset selection positions to place accumulated center at the target
        foreach (Entity sel in sels)
        {
            Transform localT = sel.GetLocalTransform();
            Transform rootT = sel.GetRootTransform();

            // Get new localized position by applying reversed root offsets to the target position.
            Vector3 newPos = Vector3.Transform(targetWorld, Quaternion.Inverse(rootT.Rotation))
                             - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));

            // Offset from center of multiple selections.
            Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                      - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
            Vector3 offsetFromCenter = localCenter - localT.Position;
            newPos -= offsetFromCenter;

            Transform newT = new(newPos, localT.EulerRotation);

            actlist.Add(sel.GetUpdateTransformAction(newT));
        }

        return new ViewportCompoundAction(actlist);
    }
}