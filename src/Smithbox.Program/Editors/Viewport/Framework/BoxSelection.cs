using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using StudioCore.Renderer;
using StudioCore.Keybinds;

namespace StudioCore.Editors.Viewport;

public class BoxSelection
{
    public VulkanViewport Parent;

    private Vector2 _dragEnd;
    private Vector2 _dragStart;

    private bool _isDragging;
    private bool _mouseDragStarted;

    private const float DragThreshold = 5f;

    public BoxSelection(VulkanViewport parent)
    {
        Parent = parent;
    }

    public void Update()
    {
        if (CFG.Current.Viewport_Enable_Box_Selection && !Parent.Gizmos.IsMouseBusy())
        {
            Vector2 mousePos = InputManager.MousePosition;

            if (InputManager.IsMouseDown(MouseButton.Left) && Parent.MouseInViewport() && !_isDragging)
            {
                _isDragging = true;
                _mouseDragStarted = false;
                _dragStart = mousePos;
                _dragEnd = mousePos;
            }
            else if (InputManager.IsMouseDown(MouseButton.Left) && _isDragging)
            {
                _dragEnd = mousePos;

                // Check if the drag threshold has been exceeded
                if (!_mouseDragStarted && Vector2.Distance(_dragStart, _dragEnd) > DragThreshold)
                {
                    _mouseDragStarted = true;
                }
            }
            else if (_isDragging && InputManager.IsMouseReleased(MouseButton.Left))
            {
                if (_mouseDragStarted)
                {
                    // Drag was confirmed and released — perform selection
                    SelectObjectsInDragArea(_dragStart, _dragEnd);
                }

                // Reset drag state
                _isDragging = false;
                _mouseDragStarted = false;
            }
        }

        if (_isDragging)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 start = _dragStart;
            Vector2 end = _dragEnd;

            // Clamp coordinates to window area
            start = Vector2.Clamp(start, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());
            end = Vector2.Clamp(end, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());

            drawList.AddRect(start, end, ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 1f, 1f)), 0f, ImDrawFlags.None, 2f);
            drawList.AddRectFilled(start, end, ImGui.GetColorU32(new Vector4(0f, 0.5f, 1f, 0.15f)));
        }
    }

    private void UpdateSelection(WeakReference<ISelectable> obj, bool ctrl)
    {
        if (!obj.TryGetTarget(out ISelectable target))
            return;

        if (ctrl)
        {
            // Toggle selection when Ctrl is held
            if (Parent.ViewportSelection.GetSelection().Contains(target))
            {
                Parent.ViewportSelection.RemoveSelection(target);
            }
            else
            {
                Parent.ViewportSelection.AddSelection(target);
            }
        }
        else
        {
            Parent.ViewportSelection.AddSelection(target);
        }
    }

    private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
    {
        Vector2 winPos = ImGui.GetWindowPos();

        float minX = MathF.Min(start.X, end.X) - winPos.X;
        float minY = MathF.Min(start.Y, end.Y) - winPos.Y;
        float maxX = MathF.Max(start.X, end.X) - winPos.X;
        float maxY = MathF.Max(start.Y, end.Y) - winPos.Y;

        bool shift = InputManager.HasShiftDown();
        bool ctrl = InputManager.HasCtrlDown();

        // Clear selection unless Shift (add) or Ctrl (toggle) is held
        if (!shift && !ctrl)
        {
            Parent.ViewportSelection.ClearSelection();
        }

        List<(WeakReference<ISelectable> obj, float distance)> selectableObjects = new();

        for (int i = 0; i < Parent.RenderScene.OpaqueRenderables.cBounds.Length; i++)
        {
            VisibleValidComponent visibleValidComponent = Parent.RenderScene.OpaqueRenderables.cVisible[i];
            bool isCulled = Parent.RenderScene.OpaqueRenderables.cCulled[i];

            if (!(visibleValidComponent._valid && !isCulled))
                continue;

            BoundingBox obj = Parent.RenderScene.OpaqueRenderables.cBounds[i];

            // Skip objects not in frustum
            if (Parent.ViewportCamera.Frustum.Contains(obj) == ContainmentType.Disjoint)
                continue;

            WeakReference<ISelectable> selectable = Parent.RenderScene.OpaqueRenderables.cSelectables[i];
            if (selectable == null)
                continue;

            // Check if object's screen-space bounding box intersects selection box
            if (IsObjectInSelectionBox(obj, minX, minY, maxX, maxY, out float distance))
            {
                selectableObjects.Add((selectable, distance));
            }
        }

        // Sort by distance (closest first)
        selectableObjects.Sort((a, b) => a.distance.CompareTo(b.distance));

        float lastSelectedDistance = -1;

        foreach ((WeakReference<ISelectable> obj, float distanceToCamera) in selectableObjects)
        {
            if (lastSelectedDistance < 0)
            {
                lastSelectedDistance = distanceToCamera;
                UpdateSelection(obj, ctrl);
                continue;
            }

            // Stop selecting if distance exceeds threshold
            if (distanceToCamera > lastSelectedDistance * CFG.Current.Viewport_Box_Selection_Distance_Threshold)
                break;

            lastSelectedDistance = distanceToCamera;
            UpdateSelection(obj, ctrl);
        }
    }

    /// <summary>
    /// Checks if an object's bounding box intersects with the selection box in screen space
    /// Works correctly for both perspective and orthographic projections
    /// </summary>
    private bool IsObjectInSelectionBox(BoundingBox worldBounds, float minX, float minY, float maxX, float maxY, out float distance)
    {
        distance = float.MaxValue;

        // Get the 8 corners of the bounding box
        Vector3[] corners = new Vector3[8]
        {
            new Vector3(worldBounds.Min.X, worldBounds.Min.Y, worldBounds.Min.Z),
            new Vector3(worldBounds.Max.X, worldBounds.Min.Y, worldBounds.Min.Z),
            new Vector3(worldBounds.Min.X, worldBounds.Max.Y, worldBounds.Min.Z),
            new Vector3(worldBounds.Max.X, worldBounds.Max.Y, worldBounds.Min.Z),
            new Vector3(worldBounds.Min.X, worldBounds.Min.Y, worldBounds.Max.Z),
            new Vector3(worldBounds.Max.X, worldBounds.Min.Y, worldBounds.Max.Z),
            new Vector3(worldBounds.Min.X, worldBounds.Max.Y, worldBounds.Max.Z),
            new Vector3(worldBounds.Max.X, worldBounds.Max.Y, worldBounds.Max.Z)
        };

        // Project all corners to screen space
        Vector2[] screenCorners = new Vector2[8];
        bool anyVisible = false;
        float minDepth = float.MaxValue;

        for (int i = 0; i < 8; i++)
        {
            screenCorners[i] = WorldToScreen(corners[i], out float depth, out bool visible);

            if (visible)
            {
                anyVisible = true;
                minDepth = Math.Min(minDepth, depth);
            }
        }

        // If no corners are visible (all behind camera), skip this object
        if (!anyVisible)
        {
            return false;
        }

        // Calculate screen-space bounding box of the object
        float objMinX = float.MaxValue;
        float objMinY = float.MaxValue;
        float objMaxX = float.MinValue;
        float objMaxY = float.MinValue;

        foreach (Vector2 screenPos in screenCorners)
        {
            objMinX = Math.Min(objMinX, screenPos.X);
            objMinY = Math.Min(objMinY, screenPos.Y);
            objMaxX = Math.Max(objMaxX, screenPos.X);
            objMaxY = Math.Max(objMaxY, screenPos.Y);
        }

        // Check if screen-space bounding boxes intersect
        bool intersects = !(objMaxX < minX || objMinX > maxX || objMaxY < minY || objMinY > maxY);

        if (intersects)
        {
            // For orthographic/oblique: use depth value directly
            // For perspective: depth is already calculated correctly
            distance = minDepth;
        }

        return intersects;
    }

    /// <summary>
    /// Converts a world position to screen space
    /// Works correctly for perspective, orthographic, and oblique projections
    /// </summary>
    private Vector2 WorldToScreen(Vector3 worldPos, out float depth, out bool visible)
    {
        Vector4 world = new(worldPos, 1.0f);

        // Get the view-projection matrix
        Matrix4x4 viewProjection = Parent.ViewportCamera.CameraTransform.CameraViewMatrixLH * Parent.ViewportCamera.ProjectionMatrix;
        Vector4 clip = Vector4.Transform(world, viewProjection);

        // For orthographic/oblique projections, W is always 1
        // For perspective, W varies with distance
        bool isOrthographic = Parent.ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique;

        if (isOrthographic)
        {
            // In orthographic projection, clip.W should be 1.0
            // Depth is stored in clip.Z
            depth = clip.Z;
            visible = clip.Z >= -1.0f && clip.Z <= 1.0f; // Within NDC range

            Vector3 ndc = new(clip.X, clip.Y, clip.Z);

            return new Vector2(
                (ndc.X + 1f) / 2f * Parent.Width,
                (1f - ndc.Y) / 2f * Parent.Height
            );
        }
        else
        {
            // Perspective projection
            if (clip.W <= 0.0f)
            {
                // Behind camera
                depth = float.MaxValue;
                visible = false;
                return new Vector2(-10000, -10000);
            }

            depth = clip.W; // Use W as depth for perspective
            visible = true;

            Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);

            return new Vector2(
                (ndc.X + 1f) / 2f * Parent.Width,
                (1f - ndc.Y) / 2f * Parent.Height
            );
        }
    }
}