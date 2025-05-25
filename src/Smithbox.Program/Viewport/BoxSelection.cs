using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Scene;
using StudioCore.Scene.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.ViewportNS;

public class BoxSelection
{
    public Viewport Parent;
    public Smithbox BaseEditor;

    private Vector2 _dragEnd;
    private Vector2 _dragStart;

    private bool _isDragging;
    private bool _mouseDragStarted;

    private const float DragThreshold = 5f;

    public BoxSelection(Smithbox baseEditor, Viewport parent)
    {
        this.BaseEditor = baseEditor;
        Parent = parent;
    }

    public void Update()
    {

        if (CFG.Current.Viewport_Enable_BoxSelection && !Parent.Gizmos.IsMouseBusy())
        {
            Vector2 mousePos = InputTracker.MousePosition;
            if (InputTracker.GetMouseButtonDown(MouseButton.Left) && Parent.MouseInViewport())
            {
                _isDragging = true;
                _mouseDragStarted = false;
                _dragStart = mousePos;
                _dragEnd = mousePos;
            }
            else if (InputTracker.GetMouseButton(MouseButton.Left) && _isDragging)
            {
                _dragEnd = mousePos;

                // Check if the drag threshold has been exceeded
                if (!_mouseDragStarted && Vector2.Distance(_dragStart, _dragEnd) > DragThreshold)
                {
                    _mouseDragStarted = true;
                }
            }
            else if (_isDragging && !InputTracker.GetMouseButton(MouseButton.Left))
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

    private void UpdateSelection(WeakReference<ISelectable> obj, EditorScreen targetEditor, bool ctrl)
    {
        if (!obj.TryGetTarget(out ISelectable target) || targetEditor == null) return;
        if (ctrl)
        {
            Parent.ViewportSelection.RemoveSelection(targetEditor, target);
        }
        else
        {
            Parent.ViewportSelection.AddSelection(targetEditor, target);
        }
    }

    private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
    {
        EditorScreen targetEditor = Parent.ViewportType switch
        {
            ViewportType.MapEditor => Parent.MapEditor,
            ViewportType.ModelEditor => Parent.ModelEditor,
            _ => null
        };
        float minX = MathF.Min(start.X, end.X) - Parent.X;
        float minY = MathF.Min(start.Y, end.Y) - Parent.Y;
        float maxX = MathF.Max(start.X, end.X) - Parent.X;
        float maxY = MathF.Max(start.Y, end.Y) - Parent.Y;
        bool shift = InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight);
        bool ctrl = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);
        if (!shift && !ctrl && targetEditor != null)
            Parent.ViewportSelection.ClearSelection(targetEditor);
        List<(WeakReference<ISelectable> obj, float distance)> selectableObjects = new();
        for (int i = 0; i < Parent.RenderScene.OpaqueRenderables.cBounds.Length; i++)
        {
            VisibleValidComponent visibleValidComponent = Parent.RenderScene.OpaqueRenderables.cVisible[i];
            bool isCulled = Parent.RenderScene.OpaqueRenderables.cCulled[i];
            if (!(visibleValidComponent._valid && !isCulled)) continue;
            BoundingBox obj = Parent.RenderScene.OpaqueRenderables.cBounds[i];
            if (Parent.Frustum.Contains(obj) != ContainmentType.Contains) continue;
            Vector3 center = obj.GetCenter();
            float distanceToCamera = Vector3.Distance(center, Parent.ViewportCamera.CameraTransform.Position);
            WeakReference<ISelectable> selectable = Parent.RenderScene.OpaqueRenderables.cSelectables[i];
            if (selectable == null) continue;
            Vector2 screenPos = WorldToScreen(center);
            if (screenPos.X >= minX && screenPos.X <= maxX && screenPos.Y >= minY && screenPos.Y <= maxY)
                selectableObjects.Add((selectable, distanceToCamera));
        }
        selectableObjects.Sort((a, b) => a.distance.CompareTo(b.distance));
        float lastSelectedDistance = -1;
        foreach ((WeakReference<ISelectable> obj, float distanceToCamera) in selectableObjects)
        {
            if (lastSelectedDistance < 0)
            {
                lastSelectedDistance = distanceToCamera;
                UpdateSelection(obj, targetEditor, ctrl);
                continue;
            }
            if (distanceToCamera > lastSelectedDistance * CFG.Current.Viewport_BS_DistThresFactor) break;
            lastSelectedDistance = distanceToCamera;
            UpdateSelection(obj, targetEditor, ctrl);
        }
    }

    private Vector2 WorldToScreen(Vector3 worldPos)
    {
        Vector4 world = new(worldPos, 1.0f);
        Vector4 clip = Vector4.Transform(world, Parent.ViewportCamera.CameraTransform.CameraViewMatrixLH * Parent.ProjectionMatrix);
        if (clip.W <= 0.0f)
        {
            return new Vector2(-10000, -10000);
        }
        Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);
        return new Vector2(
            (ndc.X + 1f) / 2f * Parent.Width,
            (1f - ndc.Y) / 2f * Parent.Height
        );
    }
}
