using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class PullToCameraAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public PullToCameraAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && Editor.ViewportSelection.IsSelection())
        {
            ApplyMoveToCamera();
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ent.WrappedObject is IMsbPart or IMsbRegion)
        {
            if (ImGui.Selectable("Pull to Camera"))
            {
                ApplyMoveToCamera();
            }
            UIHelper.Tooltip($"Move the current selection to the camera position.\n\nShortcut: {KeyBindings.Current.MAP_MoveToCamera.HintText}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Move Selected to Camera", KeyBindings.Current.MAP_MoveToCamera.HintText))
        {
            ApplyMoveToCamera();
        }
        UIHelper.Tooltip("Move the current selection to the camera position.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Pull to Camera"))
        {
            UIHelper.SimpleHeader("Camera Offset Distance", "Camera Offset Distance", "", UI.Current.ImGui_Default_Text_Color);

            DPI.ApplyInputWidth(windowWidth);
            if (ImGui.SliderFloat("##Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100))
            {
                if (CFG.Current.Toolbar_Move_to_Camera_Offset < 0)
                    CFG.Current.Toolbar_Move_to_Camera_Offset = 0;

                if (CFG.Current.Toolbar_Move_to_Camera_Offset > 100)
                    CFG.Current.Toolbar_Move_to_Camera_Offset = 100;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");

            UIHelper.WrappedText("");

            if (ImGui.Button("Pull Selection", DPI.WholeWidthButton(windowWidth, 24)))
            {
                ApplyMoveToCamera();
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyMoveToCamera()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = Editor.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, Editor.MapViewportView.Viewport.ViewportCamera.CameraTransform.RotationMatrix);
            Vector3 camPos = Editor.MapViewportView.Viewport.ViewportCamera.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

            // Get the accumulated center position of all selections
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in sels)
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
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

            // Offset selection positions to place accumulated center in front of camera
            foreach (Entity sel in sels)
            {
                Transform localT = sel.GetLocalTransform();
                Transform rootT = sel.GetRootTransform();

                // Get new localized position by applying reversed root offsets to target camera position.  
                Vector3 newPos = Vector3.Transform(targetCamPos, Quaternion.Inverse(rootT.Rotation))
                                 - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));

                // Offset from center of multiple selections.
                Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                          - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
                Vector3 offsetFromCenter = localCenter - localT.Position;
                newPos -= offsetFromCenter;

                Transform newT = new(newPos, localT.EulerRotation);

                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            if (actlist.Any())
            {
                Actions.Viewport.CompoundAction action = new(actlist);
                Editor.EditorActionManager.ExecuteAction(action);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}