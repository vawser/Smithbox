using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Scene.Interfaces;
using System;
using System.Linq;
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

    private readonly float _dragThreshold = 5f;
    private float _selectionTolerance = 5f;
    public float SelectionTolerance
    {
        get => _selectionTolerance;
        set => _selectionTolerance = MathF.Max(0, value);
    }

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
                if (!_mouseDragStarted && Vector2.Distance(_dragStart, _dragEnd) > _dragThreshold)
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

    // TODO: Implement far clip and the selection tolerance slider...
    private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
    {
        EditorScreen targetEditor = null;
        if (Parent.ViewportType is ViewportType.MapEditor)
        {
            targetEditor = Parent.MapEditor;
        }
        if (Parent.ViewportType is ViewportType.ModelEditor)
        {
            targetEditor = Parent.ModelEditor;
        }

        float minX = MathF.Min(start.X, end.X);
        float minY = MathF.Min(start.Y, end.Y);
        float maxX = MathF.Max(start.X, end.X);
        float maxY = MathF.Max(start.Y, end.Y);
        minX -= Parent.X;
        maxX -= Parent.X;
        minY -= Parent.Y;
        maxY -= Parent.Y;
        bool shift = InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight);
        bool ctrl = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);
        if (!shift && !ctrl)
        {
            if (targetEditor != null)
            {
                Parent.ViewportSelection.ClearSelection(targetEditor);
            }
        }
        for (int i = 0; i < Parent.RenderScene.OpaqueRenderables.cBounds.Length; i++)
        {
            BoundingBox obj = Parent.RenderScene.OpaqueRenderables.cBounds[i];
            if (Parent.Frustum.Contains(obj) == ContainmentType.Disjoint)
            {
                continue;
            }
            Vector3 center = obj.GetCenter();
            Vector2 screenPos = WorldToScreen(center);
            if (screenPos.X >= minX - _selectionTolerance
                && screenPos.X <= maxX + _selectionTolerance
                && screenPos.Y >= minY - _selectionTolerance
                && screenPos.Y <= maxY + _selectionTolerance)
            {
                WeakReference<ISelectable> selectable = Parent.RenderScene.OpaqueRenderables.cSelectables[i];
                if (selectable == null)
                {
                    continue;
                }
                if (selectable.TryGetTarget(out ISelectable target))
                {
                    if (targetEditor != null)
                    {
                        if (ctrl)
                        {
                            Parent.ViewportSelection.RemoveSelection(targetEditor, target);
                        }
                        else
                        {
                            Parent.ViewportSelection.AddSelection(targetEditor, target);
                        }
                    }
                }
            }
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
