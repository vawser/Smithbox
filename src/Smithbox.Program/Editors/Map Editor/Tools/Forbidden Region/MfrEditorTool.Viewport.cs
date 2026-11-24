using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Forbidden Region Editor's viewport side: while Edit in Viewport is on, the box under the cursor is outlined and
/// a click selects it (making its map the current one); the gizmo moves, rotates and scales the selected box. Place on
/// Surface puts a new box where the next click lands, through the viewport's picking depth (as the Ruler does).
/// </summary>
public partial class MfrEditorTool
{
    public MfrGizmoTarget GizmoTarget;

    private bool Editing;
    private int LastDrawnFrame = -10;

    /// <summary>
    /// True while Edit in Viewport is on and the tool is drawn: it stops by itself two frames after the tool isn't, so a
    /// closed tool never keeps the viewport's clicks.
    /// </summary>
    public bool IsEditing => Editing && CanUse() && ImGui.GetFrameCount() - LastDrawnFrame <= 2;

    // The box under the cursor
    private MapContainer HoveredMap;
    private int HoveredIndex = -1;

    // Placing a new box on a surface
    private bool Placing;
    private bool AwaitingPick;
    private Vector2 AwaitingScreen;
    private VulkanViewport AwaitingViewport;
    private bool SwallowUntilRelease;

    // A gizmo drag: the document before it, and the box as it started
    private MFR DragBefore;
    private MFR.Entry DragStart;
    private MapContainer DragMap;
    private int DragIndex = -1;

    private void DisplayViewportEditing()
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Viewport_Header"), LOC.Get("MAP_MfrEditor_Viewport_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MfrEditor_Edit_In_Viewport")}##mfrEditInViewport", ref Editing);
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Edit_In_Viewport_TT"));

        if (!Editing)
            return;

        if (Placing)
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Placing"));
        }
        else if (HoveredMap != null && HoveredIndex >= 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Hovered", HoveredIndex, HoveredMap.Name));
        }
        else
        {
            // Always draw the line, so the panel below doesn't jump as the cursor moves on and off boxes
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Hovered_None"));
        }
    }

    private void DisplayPicked()
    {
        if (IsEditing && GizmoTarget.HasTarget)
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Gizmo_Hint"));
    }

    private void TogglePlacing()
    {
        Placing = !Placing;
        AwaitingPick = false;

        // Placing works through the viewport, so it turns editing on
        if (Placing)
            Editing = true;
    }

    /// <summary>
    /// Hover, click-to-select and placing. Called each frame by the active viewport; returns true while the
    /// viewport's clicks are taken, so they don't select MSB objects. A click on no box is left to the viewport.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        if (!IsEditing)
        {
            HoveredMap = null;
            HoveredIndex = -1;
            Placing = false;
            AwaitingPick = false;
            FinishAbandonedDrag();
            return false;
        }

        // The gizmo commits on release before this runs; a drag still open with the button up was dropped by it
        if (DragBefore != null && !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
            FinishAbandonedDrag();

        // A placing click's result, from the viewport it was made in
        if (AwaitingPick)
        {
            var pipeline = AwaitingViewport.ViewPipeline;

            if (pipeline.PickingResultsReady)
            {
                pipeline.PickingResultsReady = false;
                AwaitingPick = false;
                FinishPlacing(AwaitingViewport, pipeline.PickingDepth);
            }
            else if (!pipeline.IsPickingPending)
            {
                // The request was taken by something else
                AwaitingPick = false;
            }
        }

        var taken = false;

        if (SwallowUntilRelease)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) || InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
                SwallowUntilRelease = false;

            taken = true;
        }

        var pressed = canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);

        if (Placing)
        {
            HoveredMap = null;
            HoveredIndex = -1;

            if (pressed && !AwaitingPick && !viewport.ViewPipeline.IsPickingPending)
            {
                var mouse = InputManager.MousePosition;

                // The centre of the pixel the picking pass tests
                AwaitingScreen = new Vector2(MathF.Floor(mouse.X - viewport.X) + 0.5f, MathF.Floor(mouse.Y - viewport.Y) + 0.5f);
                AwaitingViewport = viewport;
                AwaitingPick = true;

                viewport.ViewPipeline.PickingResultsReady = false;
                viewport.ViewPipeline.CreateAsyncPickingRequest();

                SwallowUntilRelease = true;
            }

            return true;
        }

        if (AwaitingPick)
            return true;

        // While the gizmo is dragged, keep the hover where it was
        if (DragBefore == null)
            Pick(ray, out HoveredMap, out HoveredIndex);

        if (pressed && HoveredMap != null && HoveredIndex >= 0)
        {
            SetCurrentMap(HoveredMap);
            Select(HoveredIndex);
            ScrollTarget = HoveredIndex;
            SwallowUntilRelease = true;
            return true;
        }

        return taken;
    }

    /// <summary>The nearest drawn box the ray enters, over every map the overlay draws.</summary>
    private void Pick(Ray ray, out MapContainer hitMap, out int hitIndex)
    {
        hitMap = null;
        hitIndex = -1;
        var best = float.MaxValue;

        // The same rule as the overlay: Show Boxes and the type toggles, plus the selected box
        var showAll = CFG.Current.MapEditor.MapEditor_Display_Forbidden_Regions;
        var showSave = showAll && CFG.Current.MapEditor.MapEditor_Forbidden_Region_Show_Save_Limit;
        var showMulti = showAll && CFG.Current.MapEditor.MapEditor_Forbidden_Region_Show_Multiplay_Limit;

        foreach (var map in GetDrawnMaps())
        {
            var document = View.MfrBank.Get(map);

            if (document == null || document.Entries.Count == 0)
                continue;

            var root = map.RootObject.GetLocalTransform().WorldMatrix;

            if (!Matrix4x4.Invert(root, out var toMap))
                continue;

            var origin = Vector3.Transform(ray.Origin, toMap);
            var direction = Vector3.Normalize(Vector3.TransformNormal(ray.Direction, toMap));

            for (var i = 0; i < document.Entries.Count; i++)
            {
                var e = document.Entries[i];

                // Only what is drawn: the shown types, plus the selected box of the current map
                var drawn = (map == CurrentMap && i == SelectedIndex)
                    || (e.Type == MFR.EntryType.SaveLimit ? showSave : showMulti);

                if (!drawn)
                    continue;

                if (MfrBoxMath.RayHit(e, origin, direction) is float t && t < best)
                {
                    // A ray that starts inside a box (the camera in it) would always pick it: skip those
                    if (t <= 0)
                        continue;

                    best = t;
                    hitMap = map;
                    hitIndex = i;
                }
            }
        }
    }

    private void FinishPlacing(VulkanViewport viewport, float depth)
    {
        if (depth <= 0 || !Unproject(viewport, AwaitingScreen, depth, out var point))
        {
            Smithbox.Log(this, LOC.Get("MAP_MfrEditor_Place_Nothing_Hit"));
            return;
        }

        // Into the map under the click: the current map, or the one whose root is nearest
        var map = CurrentMap;

        if (map == null)
            return;

        Placing = false;

        // Sitting on the surface: the box's bottom at the point
        var half = new Vector3(2);
        var local = ToMap(map, point) + new Vector3(0, half.Y, 0);

        AddBox(new MFR.Entry { Type = NewType, HalfExtents = half, Center = local },
            LOC.Get("MAP_MfrEditor_Action_Add", map.Name));
    }

    /// <summary>
    /// The world position at a viewport position and a picking depth, through the inverse of the view projection the
    /// scene was drawn with (the same as the Ruler's).
    /// </summary>
    private static bool Unproject(VulkanViewport viewport, Vector2 screen, float depth, out Vector3 point)
    {
        point = default;

        if (viewport.Width <= 0 || viewport.Height <= 0)
            return false;

        float x = 2.0f * screen.X / viewport.Width - 1.0f;
        float y = 1.0f - 2.0f * screen.Y / viewport.Height;
        float z = viewport.Device.IsDepthRangeZeroToOne ? depth : depth * 2.0f - 1.0f;

        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        var world = Vector4.Transform(new Vector4(x, y, z, 1.0f), inverse);

        if (MathF.Abs(world.W) < 1e-12f)
            return false;

        point = new Vector3(world.X, world.Y, world.Z) / world.W;

        return float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z);
    }

    #region Gizmo

    /// <summary>The selected box, if the gizmo should be on it.</summary>
    private bool TryGetSelected(out MapContainer map, out MFR.Entry entry)
    {
        map = CurrentMap;
        entry = null;

        if (!IsEditing || Placing || map?.RootObject == null)
            return false;

        var document = View.MfrBank.Get(map);

        if (document == null || SelectedIndex < 0 || SelectedIndex >= document.Entries.Count)
            return false;

        entry = document.Entries[SelectedIndex];
        return true;
    }

    public bool HasGizmoTarget => TryGetSelected(out _, out _);

    // The map root's rotation (tiles have none, but a map can be placed rotated)
    private static Quaternion RootRotation(Matrix4x4 root)
    {
        Matrix4x4.Decompose(root, out _, out var rotation, out _);
        return rotation;
    }

    public Transform GetGizmoTransform()
    {
        if (!TryGetSelected(out var map, out var entry))
            return Transform.Default;

        var root = map.RootObject.GetLocalTransform().WorldMatrix;
        var position = Vector3.Transform(entry.Center, root);

        // Box rotation, then the root's
        var rotation = GizmoState.Space == GizmoState.GizmosSpace.World
            ? Quaternion.Identity
            : RootRotation(root) * MfrBoxMath.GetRotation(entry);

        return new Transform(position, rotation, entry.HalfExtents * 2);
    }

    /// <summary>
    /// Applies a gizmo drag to the selected box, from where it was when the drag started. Preview frames change the
    /// document in place; the last frame records the whole drag as one action.
    /// </summary>
    public void GizmoApply(Transform original, Transform current, GizmoState.GizmosMode mode, bool commit)
    {
        if (!TryGetSelected(out var map, out var entry))
            return;

        if (DragBefore == null)
        {
            DragBefore = MfrBank.Copy(View.MfrBank.Get(map));
            DragStart = MfrBoxMath.Copy(entry);
            DragMap = map;
            DragIndex = SelectedIndex;
        }

        if (map != DragMap || SelectedIndex != DragIndex)
        {
            DragBefore = null;
            return;
        }

        var root = map.RootObject.GetLocalTransform().WorldMatrix;
        Matrix4x4.Invert(root, out var toMap);
        var rootRotation = RootRotation(root);

        switch (mode)
        {
            case GizmoState.GizmosMode.Translate:
            {
                var startWorld = Vector3.Transform(DragStart.Center, root);
                entry.Center = Vector3.Transform(startWorld + (current.Position - original.Position), toMap);
                break;
            }
            case GizmoState.GizmosMode.Rotate:
            {
                // The drag's world rotation, applied after the box's own world rotation
                var delta = current.Rotation * Quaternion.Inverse(original.Rotation);
                var world = delta * rootRotation * MfrBoxMath.GetRotation(DragStart);
                var local = Quaternion.Inverse(rootRotation) * world;

                MfrBoxMath.SetRotation(entry, local, MfrBoxMath.IsMirrored(DragStart));
                entry.Center = DragStart.Center;
                break;
            }
            case GizmoState.GizmosMode.Scale:
            {
                var factor = new Vector3(
                    original.Scale.X != 0 ? current.Scale.X / original.Scale.X : 1,
                    original.Scale.Y != 0 ? current.Scale.Y / original.Scale.Y : 1,
                    original.Scale.Z != 0 ? current.Scale.Z / original.Scale.Z : 1);

                entry.HalfExtents = Vector3.Max(DragStart.HalfExtents * factor, new Vector3(0.005f));
                break;
            }
        }

        if (!commit)
        {
            View.MfrBank.Touch(map);
            return;
        }

        var before = DragBefore;
        var after = MfrBank.Copy(View.MfrBank.Get(map));
        DragBefore = null;

        var verb = mode switch
        {
            GizmoState.GizmosMode.Rotate => "MAP_MfrEditor_Action_Rotate",
            GizmoState.GizmosMode.Scale => "MAP_MfrEditor_Action_Scale",
            _ => "MAP_MfrEditor_Action_Move"
        };

        Commit(map, before, after, LOC.Get(verb, SelectedIndex, map.Name));
    }

    /// <summary>
    /// Records a drag the gizmo never committed (its preview frames changed the document in place), so every change
    /// is still one undoable action.
    /// </summary>
    private void FinishAbandonedDrag()
    {
        if (DragBefore == null)
            return;

        var before = DragBefore;
        var map = DragMap;
        DragBefore = null;

        var current = View.MfrBank.Get(map);

        if (map == null || current == null || current.Write().AsSpan().SequenceEqual(before.Write()))
            return;

        Commit(map, before, MfrBank.Copy(current), LOC.Get("MAP_MfrEditor_Action_Move", DragIndex, map.Name));
    }

    #endregion
}

/// <summary>
/// The gizmo on the Forbidden Region Editor's selected box: moves, rotates and scales it.
/// </summary>
public class MfrGizmoTarget : IGizmoTarget
{
    private readonly MfrEditorTool Tool;

    public MfrGizmoTarget(MfrEditorTool tool)
    {
        Tool = tool;
    }

    public bool HasTarget => Tool.HasGizmoTarget;

    public bool AllowScale => true;

    public Transform GetTransform() => Tool.GetGizmoTransform();

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Tool.GizmoApply(original, current, mode, false);
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Tool.GizmoApply(original, current, mode, true);
    }
}
