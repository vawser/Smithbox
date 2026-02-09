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
    public bool IsBoxSelecting()
    {
        return _isDragging && _mouseDragStarted;
    }

    public void Update()
    {
        if (CFG.Current.Viewport_Enable_Box_Selection && !Parent.Gizmos.IsMouseBusy())
        {
            Vector2 mousePos = InputManager.MousePosition;

            if (InputManager.IsMouseDown(MouseButton.Left) && InputManager.HasAltDown() && Parent.MouseInViewport() && !_isDragging)
            {
                _isDragging = true;
                _mouseDragStarted = false;
                _dragStart = mousePos;
                _dragEnd = mousePos;
            }
            else if (InputManager.IsMouseDown(MouseButton.Left) && InputManager.HasAltDown() && _isDragging)
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

                // Add a cooldown to normal picking so
                // the user doesn't accidently clear the box selection immediately
                Parent.ClickSelection.TriggerCooldown();
            }
        }

        if (_isDragging && _mouseDragStarted)
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

    private void UpdateSelection(WeakReference<ISelectable> obj)
    {
        if (!obj.TryGetTarget(out ISelectable target))
            return;

        Parent.ViewportSelection.AddSelection(target);
    }

    private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
    {
        Vector2 winPos = ImGui.GetWindowPos();

        float minX = MathF.Min(start.X, end.X) - winPos.X;
        float minY = MathF.Min(start.Y, end.Y) - winPos.Y;
        float maxX = MathF.Max(start.X, end.X) - winPos.X;
        float maxY = MathF.Max(start.Y, end.Y) - winPos.Y;

        bool shift = InputManager.HasShiftDown();
        bool alt = InputManager.HasAltDown();

        // Clear selection unless Shift (add) or Ctrl (toggle) is held
        if (!shift && !alt)
        {
            Parent.ViewportSelection.ClearSelection();
        }

        List<WeakReference<ISelectable>> selectableObjects = new();

        for (int i = 0; i < Parent.RenderScene.OpaqueRenderables.cBounds.Length; i++)
        {
            VisibleValidComponent visibleValidComponent = Parent.RenderScene.OpaqueRenderables.cVisible[i];

            if(!visibleValidComponent._visible)
                continue;

            if (!visibleValidComponent._valid)
                continue;

            BoundingBox obj = Parent.RenderScene.OpaqueRenderables.cBounds[i];
            WeakReference<ISelectable> selectable = Parent.RenderScene.OpaqueRenderables.cSelectables[i];

            if (selectable == null)
                continue;

            // Check if object's screen-space bounding box intersects selection box
            if (IsObjectInSelectionBox(obj, minX, minY, maxX, maxY))
            {
                ISelectable curEnt;
                selectable.TryGetTarget(out curEnt);

                var add = true;

                if (curEnt is MsbEntity ent)
                {
                    if(!CFG.Current.Viewport_Enable_Box_Selection_MapPiece)
                    {
                        if(ent.IsPartMapPiece())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Asset)
                    {
                        if (ent.IsPartAsset() || ent.IsPartDummyAsset())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Enemy)
                    {
                        if (ent.IsPartEnemy() || ent.IsPartDummyEnemy())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Player)
                    {
                        if (ent.IsPartPlayer())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Collision)
                    {
                        if (ent.IsPartCollision() || ent.IsPartConnectCollision())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Region)
                    {
                        if (ent.IsRegion())
                        {
                            add = false;
                        }
                    }

                    if (!CFG.Current.Viewport_Enable_Box_Selection_Light)
                    {
                        if (ent.IsLight())
                        {
                            add = false;
                        }
                    }
                }

                if (add)
                {
                    selectableObjects.Add(selectable);
                }
            }
        }

        // Select all objects that intersect the box
        // No distance-based filtering - if it's in the box, select it
        foreach (WeakReference<ISelectable> obj in selectableObjects)
        {
            UpdateSelection(obj);
        }
    }

    /// <summary>
    /// Checks if an object's bounding box intersects with the selection box in screen space
    /// Works correctly for both perspective and orthographic projections
    /// </summary>
    private bool IsObjectInSelectionBox(BoundingBox worldBounds, float minX, float minY, float maxX, float maxY)
    {
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

        // Track if any corner is in front of the camera
        bool anyInFront = false;

        // Calculate screen-space bounding box
        float objMinX = float.MaxValue;
        float objMinY = float.MaxValue;
        float objMaxX = float.MinValue;
        float objMaxY = float.MinValue;

        for (int i = 0; i < 8; i++)
        {
            Vector2 screenPos = WorldToScreen(corners[i], out bool inFront);

            if (inFront)
            {
                anyInFront = true;

                // Expand screen-space bounding box
                objMinX = Math.Min(objMinX, screenPos.X);
                objMinY = Math.Min(objMinY, screenPos.Y);
                objMaxX = Math.Max(objMaxX, screenPos.X);
                objMaxY = Math.Max(objMaxY, screenPos.Y);
            }
        }

        // If no corners are in front of camera, object is not selectable
        if (!anyInFront)
        {
            return false;
        }

        // Clamp screen bounds to viewport to handle objects partially off-screen
        objMinX = Math.Max(objMinX, 0);
        objMinY = Math.Max(objMinY, 0);
        objMaxX = Math.Min(objMaxX, Parent.Width);
        objMaxY = Math.Min(objMaxY, Parent.Height);

        // Check if screen-space bounding boxes intersect
        // This is an AABB intersection test
        bool intersects = !(objMaxX < minX || objMinX > maxX || objMaxY < minY || objMinY > maxY);

        return intersects;
    }

    /// <summary>
    /// Converts a world position to screen space
    /// Works correctly for perspective, orthographic, and oblique projections
    /// Returns whether the point is in front of the camera
    /// </summary>
    private Vector2 WorldToScreen(Vector3 worldPos, out bool inFront)
    {
        Vector4 world = new(worldPos, 1.0f);

        // Get the view-projection matrix
        Matrix4x4 viewProjection = Parent.ViewportCamera.CameraTransform.CameraViewMatrixLH * Parent.ViewportCamera.ProjectionMatrix;
        Vector4 clip = Vector4.Transform(world, viewProjection);

        bool isOrthographic = Parent.ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique;

        if (isOrthographic)
        {
            // In orthographic projection, check if Z is within valid range
            // Use a more lenient check to catch objects near the clipping planes
            inFront = clip.Z >= -1.5f && clip.Z <= 1.5f;

            // No perspective divide needed for orthographic
            Vector2 ndc = new Vector2(clip.X, clip.Y);

            return new Vector2(
                (ndc.X + 1f) / 2f * Parent.Width,
                (1f - ndc.Y) / 2f * Parent.Height
            );
        }
        else
        {
            // Perspective projection
            // Point is in front if W > 0
            inFront = clip.W > 0.001f; // Small epsilon to avoid division issues

            if (!inFront)
            {
                // Return position far off-screen
                return new Vector2(-100000, -100000);
            }

            // Perspective divide
            Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);

            return new Vector2(
                (ndc.X + 1f) / 2f * Parent.Width,
                (1f - ndc.Y) / 2f * Parent.Height
            );
        }
    }

    /// <summary>
    /// Alternative selection method: check if object center is in selection box
    /// Useful for small objects or as a fallback
    /// </summary>
    private bool IsObjectCenterInSelectionBox(Vector3 center, float minX, float minY, float maxX, float maxY)
    {
        Vector2 screenPos = WorldToScreen(center, out bool inFront);

        if (!inFront)
            return false;

        return screenPos.X >= minX && screenPos.X <= maxX &&
               screenPos.Y >= minY && screenPos.Y <= maxY;
    }
}