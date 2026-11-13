using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public class Gizmos
{
    public readonly TranslationGizmo TranslationGizmo;
    public readonly RotationGizmo RotationGizmo;
    public readonly ScaleGizmo ScaleGizmo;

    private Transform CurrentTransform = Transform.Default;

    private bool IsTransforming;
    public Vector3 CameraPosition { get; set; }

    private Transform OriginalTransform = Transform.Default;
    private Vector3 OriginProjection;
    private GizmoState.Axis TransformAxis = GizmoState.Axis.None;

    private float OriginalScaleProjection;

    private VulkanViewport Viewport;

    /// <summary>
    /// The default target, the viewport selection.
    /// </summary>
    public readonly SelectionGizmoTarget SelectionTarget;

    /// <summary>
    /// The target the gizmo moves. Set to null to move the selection.
    /// </summary>
    public IGizmoTarget Target { get; set; }

    // The target and mode a drag started with, kept until the drag ends
    private IGizmoTarget DragTarget;
    private GizmoState.GizmosMode DragMode;

    /// <summary>
    /// The grid that translation snaps to while snapping is held, or null for none.
    /// </summary>
    public ViewportGridSettings SnapGrid { get; set; }

    /// <summary>Takes the gizmo meshes out of the scene and releases them, for a viewport that is going away</summary>
    public void Dispose()
    {
        foreach (var proxy in new RenderableProxy[]
                 {
                     TranslationGizmo.X_Arrow_Proxy, TranslationGizmo.Y_Arrow_Proxy, TranslationGizmo.Z_Arrow_Proxy,
                     TranslationGizmo.X_Square_Proxy, TranslationGizmo.Y_Square_Proxy, TranslationGizmo.Z_Square_Proxy,
                     RotationGizmo.X_Ring_Proxy, RotationGizmo.Y_Ring_Proxy, RotationGizmo.Z_Ring_Proxy,
                     ScaleGizmo.X_Cube_Proxy, ScaleGizmo.Y_Cube_Proxy, ScaleGizmo.Z_Cube_Proxy
                 })
        {
            proxy?.Dispose();
        }
    }

    // The snap grid as it was when the drag started. A grid moving itself snaps to where it started.
    private Transform DragSnapGrid;
    private float DragSnapSize;

    public Gizmos(VulkanViewport viewport, MeshRenderables renderlist)
    {
        Viewport = viewport;
        SelectionTarget = new SelectionGizmoTarget(viewport);

        TranslationGizmo = new(renderlist);
        RotationGizmo = new(renderlist);
        ScaleGizmo = new(renderlist);
    }
    public bool IsMouseBusy()
    {
        return IsTransforming;
    }

    /// <summary>
    ///     The gizmo's world scale at a point. A perspective camera grows it with distance so it keeps its size on
    ///     screen. An orthographic or oblique one draws the same size at every distance, so it grows with the view's
    ///     height instead, matched to the share of the screen it takes in perspective.
    /// </summary>
    private float GetGizmoScale(Vector3 center)
    {
        var camera = Viewport?.ViewportCamera;

        if (camera != null && camera.ViewMode is not ViewMode.Perspective)
        {
            var halfFov = CFG.Current.Viewport_Camera_FOV * MathF.PI / 360.0f;
            return camera.OrthographicSize * CFG.Current.Viewport_Gizmo_Size_Distance_Scale /
                   (2.0f * MathF.Tan(halfFov));
        }

        return (center - CameraPosition).Length() * CFG.Current.Viewport_Gizmo_Size_Distance_Scale;
    }

    private IGizmoTarget ActiveTarget => Target ?? SelectionTarget;

    /// <summary>
    /// The gizmo mode for a target. Scale falls back to translate on targets that can't be scaled.
    /// </summary>
    private static GizmoState.GizmosMode GetMode(IGizmoTarget target)
    {
        if (GizmoState.Mode == GizmoState.GizmosMode.Scale && !target.AllowScale)
        {
            return GizmoState.GizmosMode.Translate;
        }

        return GizmoState.Mode;
    }

    /// <summary>
    /// Snapping is held while dragging: translation snaps to the snap grid's squares, rotation to
    /// <see cref="CFG.Viewport_Gizmo_Snap_Rotation_Step" />.
    /// </summary>
    private static bool IsSnapping()
    {
        return InputManager.HasCtrlDown();
    }

    /// <summary>
    /// Snaps a position to the corners of the snap grid's squares, in the grid's own space. Only the grid axes the
    /// dragged gizmo axes run along are snapped, so dragging along one axis doesn't move the others.
    /// </summary>
    private Vector3 SnapTranslation(Vector3 position)
    {
        var toGrid = Quaternion.Inverse(DragSnapGrid.Rotation);
        var local = Vector3.Transform(position - DragSnapGrid.Position, toGrid);

        var snapX = false;
        var snapY = false;
        var snapZ = false;

        void MarkDragAxis(Vector3 gizmoAxis)
        {
            var dir = Vector3.Transform(Vector3.Transform(gizmoAxis, OriginalTransform.Rotation), toGrid);
            const float threshold = 0.01f;

            snapX |= MathF.Abs(dir.X) > threshold;
            snapY |= MathF.Abs(dir.Y) > threshold;
            snapZ |= MathF.Abs(dir.Z) > threshold;
        }

        switch (TransformAxis)
        {
            case GizmoState.Axis.PosX: MarkDragAxis(Vector3.UnitX); break;
            case GizmoState.Axis.PosY: MarkDragAxis(Vector3.UnitY); break;
            case GizmoState.Axis.PosZ: MarkDragAxis(Vector3.UnitZ); break;
            case GizmoState.Axis.PosXY: MarkDragAxis(Vector3.UnitX); MarkDragAxis(Vector3.UnitY); break;
            case GizmoState.Axis.PosXZ: MarkDragAxis(Vector3.UnitX); MarkDragAxis(Vector3.UnitZ); break;
            case GizmoState.Axis.PosYZ: MarkDragAxis(Vector3.UnitY); MarkDragAxis(Vector3.UnitZ); break;
        }

        float Snap(float value) => MathF.Round(value / DragSnapSize) * DragSnapSize;

        if (snapX) local.X = Snap(local.X);
        if (snapY) local.Y = Snap(local.Y);
        if (snapZ) local.Z = Snap(local.Z);

        return DragSnapGrid.Position + Vector3.Transform(local, DragSnapGrid.Rotation);
    }

    public void Update(Ray ray, bool canCaptureMouse, bool isActiveViewport = true)
    {
        if (!isActiveViewport || !CFG.Current.Viewport_Render_Gizmos)
        {
            TranslationGizmo.SetVisibility(false);
            RotationGizmo.SetVisibility(false);
            ScaleGizmo.SetVisibility(false);

            return;
        }

        var target = IsTransforming ? DragTarget : ActiveTarget;
        var mode = IsTransforming ? DragMode : GetMode(target);

        TranslationGizmo.UpdateColor();
        RotationGizmo.UpdateColor();
        ScaleGizmo.UpdateColor();

        if (IsTransforming)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Gizmo_Interaction_Action))
            {
                IsTransforming = false;

                DragTarget.Commit(OriginalTransform, CurrentTransform, DragMode);
                DragTarget = null;

                // Add a cooldown to normal picking so
                // the user doesn't accidently clear their selection
                Viewport.ClickSelection.TriggerCooldown();
            }
            else
            {
                // Translate
                if (mode == GizmoState.GizmosMode.Translate)
                {
                    Vector3 delta;
                    if (TransformAxis == GizmoState.Axis.PosXY || TransformAxis == GizmoState.Axis.PosXZ || TransformAxis == GizmoState.Axis.PosYZ)
                    {
                        delta = GizmoHelper.GetDoubleAxisProjection(ray, OriginalTransform, TransformAxis) - OriginProjection;
                    }
                    else
                    {
                        delta = GizmoHelper.GetSingleAxisProjection(ray, OriginalTransform, TransformAxis) - OriginProjection;
                    }

                    CurrentTransform.Position = OriginalTransform.Position + delta;

                    if (IsSnapping() && DragSnapSize > 0f)
                    {
                        CurrentTransform.Position = SnapTranslation(CurrentTransform.Position);
                    }
                }
                // Rotation
                else if (mode == GizmoState.GizmosMode.Rotate)
                {
                    Vector3 axis = Vector3.Zero;
                    switch (TransformAxis)
                    {
                        case GizmoState.Axis.PosX:
                            axis = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), OriginalTransform.Rotation);
                            break;
                        case GizmoState.Axis.PosY:
                            axis = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), OriginalTransform.Rotation);
                            break;
                        case GizmoState.Axis.PosZ:
                            axis = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), OriginalTransform.Rotation);
                            break;
                    }

                    axis = Vector3.Normalize(axis);
                    Vector3 newproj = GizmoHelper.GetAxisPlaneProjection(ray, OriginalTransform, TransformAxis);
                    Vector3 delta = Vector3.Normalize(newproj - OriginalTransform.Position);
                    Vector3 deltaorig = Vector3.Normalize(OriginProjection - OriginalTransform.Position);
                    Vector3 side = Vector3.Cross(axis, deltaorig);
                    var y = Math.Max(-1.0f, Math.Min(1.0f, Vector3.Dot(delta, deltaorig)));
                    var x = Math.Max(-1.0f, Math.Min(1.0f, Vector3.Dot(delta, side)));
                    var angle = (float)Math.Atan2(x, y);

                    var step = CFG.Current.Viewport_Gizmo_Snap_Rotation_Step;
                    if (IsSnapping() && step > 0f)
                    {
                        var stepRadians = Utils.DegToRadians(step);
                        angle = MathF.Round(angle / stepRadians) * stepRadians;
                    }

                    CurrentTransform.Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * OriginalTransform.Rotation);
                }
                // Scale
                else if (mode == GizmoState.GizmosMode.Scale)
                {
                    float newProj = GizmoHelper.GetAxisScaleProjection(ray, OriginalTransform, TransformAxis);
                    float ratio = newProj / OriginalScaleProjection;

                    Vector3 scale = OriginalTransform.Scale;

                    switch (TransformAxis)
                    {
                        case GizmoState.Axis.PosX: scale.X *= ratio; break;
                        case GizmoState.Axis.PosY: scale.Y *= ratio; break;
                        case GizmoState.Axis.PosZ: scale.Z *= ratio; break;
                    }

                    CurrentTransform.Scale = scale;
                }

                DragTarget.Preview(OriginalTransform, CurrentTransform, DragMode);
            }
        }

        var hasTarget = IsTransforming || target.HasTarget;

        if (!IsTransforming && hasTarget)
        {
            OriginalTransform = target.GetTransform();

            var hoveredAxis = GizmoState.Axis.None;
            switch (mode)
            {
                case GizmoState.GizmosMode.Translate:
                    hoveredAxis = TranslationGizmo.HandleHoveredAxis(ray);
                    break;
                case GizmoState.GizmosMode.Rotate:
                    hoveredAxis = RotationGizmo.HandleHoveredAxis(ray);
                    break;
                case GizmoState.GizmosMode.Scale:
                    hoveredAxis = ScaleGizmo.HandleHoveredAxis(ray);
                    break;
            }

            if (canCaptureMouse && InputManager.IsMouseDown(MousebindID.Viewport_Gizmo_Interaction_Action))
            {
                if (hoveredAxis != GizmoState.Axis.None)
                {
                    IsTransforming = true;
                    DragTarget = target;
                    DragMode = mode;
                    TransformAxis = hoveredAxis;
                    CurrentTransform = OriginalTransform;

                    if (SnapGrid != null)
                    {
                        DragSnapGrid = ViewportGridMovement.GetGridTransform(SnapGrid);
                        DragSnapSize = Math.Max(SnapGrid.SectionSize, 0f);
                    }
                    else
                    {
                        DragSnapSize = 0f;
                    }

                    switch (mode)
                    {
                        case GizmoState.GizmosMode.Translate:
                            {
                                if (TransformAxis == GizmoState.Axis.PosXY || TransformAxis == GizmoState.Axis.PosXZ ||
                                    TransformAxis == GizmoState.Axis.PosYZ)
                                {
                                    OriginProjection = GizmoHelper.GetDoubleAxisProjection(ray, OriginalTransform, TransformAxis);
                                }
                                else
                                {
                                    OriginProjection = GizmoHelper.GetSingleAxisProjection(ray, OriginalTransform, TransformAxis);
                                }
                            }
                            break;
                        case GizmoState.GizmosMode.Rotate:
                            {
                                OriginProjection = GizmoHelper.GetAxisPlaneProjection(ray, OriginalTransform, TransformAxis);
                            }
                            break;
                        case GizmoState.GizmosMode.Scale:
                            {
                                OriginalScaleProjection = GizmoHelper.GetAxisScaleProjection(ray, OriginalTransform, TransformAxis);
                            }
                            break;
                    }
                }
            }
        }

        // Update gizmos transform and visibility
        if (hasTarget)
        {
            Vector3 center;
            Quaternion rot;
            if (IsTransforming)
            {
                center = CurrentTransform.Position;
                rot = CurrentTransform.Rotation;
            }
            else
            {
                center = OriginalTransform.Position;
                rot = OriginalTransform.Rotation;
            }

            Vector3 scale = new(GetGizmoScale(center));

            TranslationGizmo.UpdateTransform(center, rot, scale);
            RotationGizmo.UpdateTransform(center, rot, scale);
            ScaleGizmo.UpdateTransform(center, rot, scale);

            TranslationGizmo.SetVisibility(mode == GizmoState.GizmosMode.Translate);
            RotationGizmo.SetVisibility(mode == GizmoState.GizmosMode.Rotate);
            ScaleGizmo.SetVisibility(mode == GizmoState.GizmosMode.Scale);
        }
        else
        {
            TranslationGizmo.SetVisibility(false);
            RotationGizmo.SetVisibility(false);
            ScaleGizmo.SetVisibility(false);
        }
    }
}

public class TranslationGizmo
{
    public readonly DbgPrimGizmoTranslateArrow X_Arrow;
    public readonly DebugPrimitiveRenderableProxy X_Arrow_Proxy;
    public readonly DbgPrimGizmoTranslateArrow Y_Arrow;
    public readonly DebugPrimitiveRenderableProxy Y_Arrow_Proxy;
    public readonly DbgPrimGizmoTranslateArrow Z_Arrow;
    public readonly DebugPrimitiveRenderableProxy Z_Arrow_Proxy;

    public readonly DbgPrimGizmoTranslateSquare X_Square;
    public readonly DebugPrimitiveRenderableProxy X_Square_Proxy;
    public readonly DbgPrimGizmoTranslateSquare Y_Square;
    public readonly DebugPrimitiveRenderableProxy Y_Square_Proxy;
    public readonly DbgPrimGizmoTranslateSquare Z_Square;
    public readonly DebugPrimitiveRenderableProxy Z_Square_Proxy;

    public TranslationGizmo(MeshRenderables renderList)
    {
        X_Arrow = new DbgPrimGizmoTranslateArrow(GizmoState.Axis.PosX);
        Y_Arrow = new DbgPrimGizmoTranslateArrow(GizmoState.Axis.PosY);
        Z_Arrow = new DbgPrimGizmoTranslateArrow(GizmoState.Axis.PosZ);
        X_Square = new DbgPrimGizmoTranslateSquare(GizmoState.Axis.PosX);
        Y_Square = new DbgPrimGizmoTranslateSquare(GizmoState.Axis.PosY);
        Z_Square = new DbgPrimGizmoTranslateSquare(GizmoState.Axis.PosZ);

        X_Arrow_Proxy = new DebugPrimitiveRenderableProxy(renderList, X_Arrow);
        Y_Arrow_Proxy = new DebugPrimitiveRenderableProxy(renderList, Y_Arrow);
        Z_Arrow_Proxy = new DebugPrimitiveRenderableProxy(renderList, Z_Arrow);
        X_Square_Proxy = new DebugPrimitiveRenderableProxy(renderList, X_Square);
        Y_Square_Proxy = new DebugPrimitiveRenderableProxy(renderList, Y_Square);
        Z_Square_Proxy = new DebugPrimitiveRenderableProxy(renderList, Z_Square);

        UpdateColor();

    }
    public void SetVisibility(bool visible)
    {
        X_Arrow_Proxy.Visible = visible;
        Y_Arrow_Proxy.Visible = visible;
        Z_Arrow_Proxy.Visible = visible;
        X_Square_Proxy.Visible = visible;
        Y_Square_Proxy.Visible = visible;
        Z_Square_Proxy.Visible = visible;
    }

    public void UpdateColor()
    {
        X_Arrow_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Base_Color);
        X_Arrow_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Highlight_Color);

        Y_Arrow_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Base_Color);
        Y_Arrow_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

        Z_Arrow_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Base_Color);
        Z_Arrow_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Highlight_Color);

        X_Square_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Base_Color);
        X_Square_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Highlight_Color);

        Y_Square_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Base_Color);
        Y_Square_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

        Z_Square_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Base_Color);
        Z_Square_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Highlight_Color);
    }

    public GizmoState.Axis HandleHoveredAxis(Ray ray)
    {
        var hoveredAxis = GizmoState.Axis.None;

        if (X_Arrow.GetRaycast(ray, X_Arrow_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosX;
        }
        else if (Y_Arrow.GetRaycast(ray, Y_Arrow_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosY;
        }
        else if (Z_Arrow.GetRaycast(ray, Z_Arrow_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosZ;
        }

        if (X_Square.GetRaycast(ray, X_Square_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosYZ;
        }
        else if (Y_Square.GetRaycast(ray, Y_Square_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosXZ;
        }
        else if (Z_Square.GetRaycast(ray, Z_Square_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosXY;
        }

        X_Arrow_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosX;
        Y_Arrow_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosY;
        Z_Arrow_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosZ;

        X_Square_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosYZ;
        Y_Square_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosXZ;
        Z_Square_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosXY;

        return hoveredAxis;
    }

    public void UpdateTransform(Vector3 center, Quaternion rot, Vector3 scale)
    {
        X_Arrow_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Y_Arrow_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Z_Arrow_Proxy.World = new Transform(center, rot, scale).WorldMatrix;

        X_Square_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Y_Square_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Z_Square_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
    }
}

public class RotationGizmo
{
    public readonly DbgPrimGizmoRotateRing X_Ring;
    public readonly DebugPrimitiveRenderableProxy X_Ring_Proxy;
    public readonly DbgPrimGizmoRotateRing Y_Ring;
    public readonly DebugPrimitiveRenderableProxy Y_Ring_Proxy;
    public readonly DbgPrimGizmoRotateRing Z_Ring;
    public readonly DebugPrimitiveRenderableProxy Z_Ring_Proxy;

    public RotationGizmo(MeshRenderables renderList)
    {
        X_Ring = new DbgPrimGizmoRotateRing(GizmoState.Axis.PosX);
        Y_Ring = new DbgPrimGizmoRotateRing(GizmoState.Axis.PosY);
        Z_Ring = new DbgPrimGizmoRotateRing(GizmoState.Axis.PosZ);

        X_Ring_Proxy = new DebugPrimitiveRenderableProxy(renderList, X_Ring);
        Y_Ring_Proxy = new DebugPrimitiveRenderableProxy(renderList, Y_Ring);
        Z_Ring_Proxy = new DebugPrimitiveRenderableProxy(renderList, Z_Ring);

        UpdateColor();
    }

    public void SetVisibility(bool visible)
    {
        X_Ring_Proxy.Visible = visible;
        Y_Ring_Proxy.Visible = visible;
        Z_Ring_Proxy.Visible = visible;
    }

    public void UpdateColor()
    {
        X_Ring_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Base_Color);
        X_Ring_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Highlight_Color);

        Y_Ring_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Base_Color);
        Y_Ring_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

        Z_Ring_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Base_Color);
        Z_Ring_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Highlight_Color);
    }

    public GizmoState.Axis HandleHoveredAxis(Ray ray)
    {
        var hoveredAxis = GizmoState.Axis.None;

        if (X_Ring.GetRaycast(ray, X_Ring_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosX;
        }
        else if (Y_Ring.GetRaycast(ray, Y_Ring_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosY;
        }
        else if (Z_Ring.GetRaycast(ray, Z_Ring_Proxy.World))
        {
            hoveredAxis = GizmoState.Axis.PosZ;
        }

        X_Ring_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosX;
        Y_Ring_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosY;
        Z_Ring_Proxy.RenderSelectionOutline = hoveredAxis == GizmoState.Axis.PosZ;

        return hoveredAxis;
    }
    public void UpdateTransform(Vector3 center, Quaternion rot, Vector3 scale)
    {
        X_Ring_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Y_Ring_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Z_Ring_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
    }
}

public class ScaleGizmo
{
    public readonly DbgPrimGizmoScaleCube X_Cube;
    public readonly DebugPrimitiveRenderableProxy X_Cube_Proxy;
    public readonly DbgPrimGizmoScaleCube Y_Cube;
    public readonly DebugPrimitiveRenderableProxy Y_Cube_Proxy;
    public readonly DbgPrimGizmoScaleCube Z_Cube;
    public readonly DebugPrimitiveRenderableProxy Z_Cube_Proxy;

    public ScaleGizmo(MeshRenderables renderList)
    {
        X_Cube = new DbgPrimGizmoScaleCube(GizmoState.Axis.PosX);
        Y_Cube = new DbgPrimGizmoScaleCube(GizmoState.Axis.PosY);
        Z_Cube = new DbgPrimGizmoScaleCube(GizmoState.Axis.PosZ);

        X_Cube_Proxy = new DebugPrimitiveRenderableProxy(renderList, X_Cube);
        Y_Cube_Proxy = new DebugPrimitiveRenderableProxy(renderList, Y_Cube);
        Z_Cube_Proxy = new DebugPrimitiveRenderableProxy(renderList, Z_Cube);

        UpdateColor();
    }
    public void SetVisibility(bool visible)
    {
        X_Cube_Proxy.Visible = visible;
        Y_Cube_Proxy.Visible = visible;
        Z_Cube_Proxy.Visible = visible;
    }

    public void UpdateColor()
    {
        X_Cube_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Base_Color);
        X_Cube_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_X_Highlight_Color);

        Y_Cube_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Base_Color);
        Y_Cube_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

        Z_Cube_Proxy.BaseColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Base_Color);
        Z_Cube_Proxy.HighlightedColor = GizmoHelper.GetGizmoColor(
            CFG.Current.Viewport_Gizmo_Z_Highlight_Color);
    }

    public GizmoState.Axis HandleHoveredAxis(Ray ray)
    {
        var hovered = GizmoState.Axis.None;

        if (X_Cube.GetRaycast(ray, X_Cube_Proxy.World))
            hovered = GizmoState.Axis.PosX;
        else if (Y_Cube.GetRaycast(ray, Y_Cube_Proxy.World))
            hovered = GizmoState.Axis.PosY;
        else if (Z_Cube.GetRaycast(ray, Z_Cube_Proxy.World))
            hovered = GizmoState.Axis.PosZ;

        X_Cube_Proxy.RenderSelectionOutline = hovered == GizmoState.Axis.PosX;
        Y_Cube_Proxy.RenderSelectionOutline = hovered == GizmoState.Axis.PosY;
        Z_Cube_Proxy.RenderSelectionOutline = hovered == GizmoState.Axis.PosZ;

        return hovered;
    }
    public void UpdateTransform(Vector3 center, Quaternion rot, Vector3 scale)
    {
        X_Cube_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Y_Cube_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
        Z_Cube_Proxy.World = new Transform(center, rot, scale).WorldMatrix;
    }
}

public static class GizmoHelper
{
    public static Vector3 GetSingleAxisProjection(Ray ray, Transform t, GizmoState.Axis axis)
    {
        Vector3 axisvec = Vector3.Zero;
        switch (axis)
        {
            case GizmoState.Axis.PosX:
                axisvec = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosY:
                axisvec = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosZ:
                axisvec = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
        }

        Vector3 pos = t.Position;
        Vector3 normal = Vector3.Cross(Vector3.Cross(ray.Direction, axisvec), ray.Direction);
        var d = Vector3.Dot(ray.Origin - pos, normal) / Vector3.Dot(axisvec, normal);
        return pos + axisvec * d;
    }

    public static Vector3 GetDoubleAxisProjection(Ray ray, Transform t, GizmoState.Axis axis)
    {
        Vector3 planeNormal = Vector3.Zero;
        switch (axis)
        {
            case GizmoState.Axis.PosXY:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosYZ:
                planeNormal = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosXZ:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
        }

        float dist;
        Vector3 relorigin = ray.Origin - t.Position;
        if (ViewportUtils.RayPlaneIntersection(relorigin, ray.Direction, Vector3.Zero, planeNormal, out dist))
        {
            return ray.Origin + ray.Direction * dist;
        }

        return ray.Origin;
    }

    public static Vector3 GetAxisPlaneProjection(Ray ray, Transform t, GizmoState.Axis axis)
    {
        Vector3 planeNormal = Vector3.Zero;
        switch (axis)
        {
            case GizmoState.Axis.PosX:
                planeNormal = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosY:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
            case GizmoState.Axis.PosZ:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
        }

        float dist;
        Vector3 relorigin = ray.Origin - t.Position;
        if (ViewportUtils.RayPlaneIntersection(relorigin, ray.Direction, Vector3.Zero, planeNormal, out dist))
        {
            return ray.Origin + ray.Direction * dist;
        }

        return ray.Origin;
    }
    public static float GetAxisScaleProjection(Ray ray, Transform t, GizmoState.Axis axis)
    {
        Vector3 axisVec = axis switch
        {
            GizmoState.Axis.PosX => Vector3.Transform(Vector3.UnitX, t.Rotation),
            GizmoState.Axis.PosY => Vector3.Transform(Vector3.UnitY, t.Rotation),
            GizmoState.Axis.PosZ => Vector3.Transform(Vector3.UnitZ, t.Rotation),
            _ => Vector3.UnitX
        };

        Vector3 proj = GetSingleAxisProjection(ray, t, axis);
        return Vector3.Dot(proj - t.Position, axisVec);
    }

    public static Color GetGizmoColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }
}