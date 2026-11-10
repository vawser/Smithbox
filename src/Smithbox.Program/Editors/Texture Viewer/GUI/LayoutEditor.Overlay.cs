using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The layout overlay on the texture viewport (T5): cell outlines over the image, hover and selection, and the
/// mouse edits — click to select (an alias group at once; Shift adds), drag to move, drag a corner to resize, drag in
/// Draw mode to add a cell — plus arrow keys to nudge (Shift: 10 px), Delete and Escape.
/// </summary>
public partial class LayoutEditor
{
    private enum DragMode { None, Move, Resize, Draw }

    private DragMode Drag = DragMode.None;
    private Vector2 DragStart;
    private int ResizeCorner;
    private List<(SubTexture Cell, CellRect Rect)> DragStartRects = new();
    private CellRect DrawRect;

    /// <summary>Draw mode: dragging on the image draws a new cell instead of selecting.</summary>
    public bool DrawMode;

    private const float HandleSize = 6f;

    /// <summary>
    /// Draws the overlay and handles its input. Called by the viewport right after the image, with the image's
    /// screen position and drawn size and the texture's pixel size.
    /// </summary>
    public void DrawOverlay(Vector2 imageMin, Vector2 displaySize, Vector2 textureSize)
    {
        Resolve();

        MouseSheetPosition = null;
        Hovered = null;

        if (Atlas == null || textureSize.X <= 0 || textureSize.Y <= 0 || displaySize.X <= 0 || displaySize.Y <= 0)
            return;

        var scale = displaySize / textureSize;
        var io = ImGui.GetIO();

        // Input: an invisible button over the image takes the clicks and drags. The cursor is not moved back
        // afterwards: the button covers the image's rect, so it already leaves the cursor where the image did, and
        // restoring it would leave a SetCursorScreenPos with no item after it (ImGui asserts on that at End)
        ImGui.SetCursorScreenPos(imageMin);
        ImGui.InvisibleButton("##layoutCanvas", displaySize);
        var hovered = ImGui.IsItemHovered();
        var activated = ImGui.IsItemActivated();
        var active = ImGui.IsItemActive();
        var released = ImGui.IsItemDeactivated();

        var mouse = (ImGui.GetMousePos() - imageMin) / scale;

        if (hovered || active)
        {
            MouseSheetPosition = mouse;
            Hovered = LayoutGeometry.CellsAt(Atlas, mouse.X, mouse.Y).FirstOrDefault();
        }

        if (activated)
            BeginDrag(mouse, imageMin, scale, io.KeyShift);

        if (active && Drag != DragMode.None)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                CancelChange();
                Drag = DragMode.None;
            }
            else
            {
                UpdateDrag(mouse);
            }
        }

        if (released)
            EndDrag();

        DrawCells(imageMin, scale);

        if (hovered && Hovered != null && Drag == DragMode.None)
            CellTooltip(Hovered);

        Keyboard(io);
        ScrollToRequested(imageMin, scale);
    }

    private int Snap(float v)
    {
        var s = Math.Max(1, CFG.Current.TextureViewerLayout_Snap);
        return (int)MathF.Round(v / s) * s;
    }

    #region Drag
    private void BeginDrag(Vector2 mouse, Vector2 imageMin, Vector2 scale, bool shift)
    {
        Drag = DragMode.None;

        // A corner handle of the selection
        if (IsEditable && SelectionIsOneRect)
        {
            var r = CellRect.Of(Selected.First());
            var corners = Corners(r, imageMin, scale);
            var screen = ImGui.GetMousePos();

            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(screen.X - corners[i].X) <= HandleSize + 2 && Math.Abs(screen.Y - corners[i].Y) <= HandleSize + 2)
                {
                    Drag = DragMode.Resize;
                    ResizeCorner = i;
                    DragStart = mouse;
                    DragStartRects = Selected.Select(c => (c, CellRect.Of(c))).ToList();
                    BeginChange(Selected);
                    return;
                }
            }
        }

        if (IsEditable && DrawMode)
        {
            Drag = DragMode.Draw;
            DragStart = new Vector2(Snap(mouse.X), Snap(mouse.Y));
            DrawRect = new CellRect((int)DragStart.X, (int)DragStart.Y, 0, 0);
            return;
        }

        if (Hovered != null)
        {
            var group = LayoutGeometry.AliasGroup(Atlas, Hovered);

            if (shift)
            {
                // Shift toggles the alias group
                if (Selected.Contains(Hovered))
                    foreach (var c in group) Selected.Remove(c);
                else
                    foreach (var c in group) Selected.Add(c);
                return;
            }

            if (!Selected.Contains(Hovered))
                Select(group);

            if (IsEditable)
            {
                Drag = DragMode.Move;
                DragStart = mouse;
                DragStartRects = Selected.Select(c => (c, CellRect.Of(c))).ToList();
                BeginChange(Selected);
            }

            return;
        }

        if (!shift)
            Selected.Clear();
    }

    private void UpdateDrag(Vector2 mouse)
    {
        switch (Drag)
        {
            case DragMode.Move:
                {
                    int dx = Snap(mouse.X - DragStart.X), dy = Snap(mouse.Y - DragStart.Y);
                    foreach (var (cell, r) in DragStartRects)
                    {
                        cell.X = $"{r.X + dx}";
                        cell.Y = $"{r.Y + dy}";
                    }
                    break;
                }
            case DragMode.Resize:
                {
                    var r = DragStartRects[0].Rect;
                    int x0 = r.X, y0 = r.Y, x1 = r.Right, y1 = r.Bottom;
                    int mx = Snap(mouse.X), my = Snap(mouse.Y);

                    // 0 top left, 1 top right, 2 bottom right, 3 bottom left
                    if (ResizeCorner is 0 or 3) x0 = Math.Min(mx, x1 - 1); else x1 = Math.Max(mx, x0 + 1);
                    if (ResizeCorner is 0 or 1) y0 = Math.Min(my, y1 - 1); else y1 = Math.Max(my, y0 + 1);

                    foreach (var (cell, _) in DragStartRects)
                    {
                        cell.X = $"{x0}";
                        cell.Y = $"{y0}";
                        cell.Width = $"{x1 - x0}";
                        cell.Height = $"{y1 - y0}";

                        // Kept in step with the size, except on half cells
                        if (cell.Has("originalWidth") && !cell.IsHalf)
                        {
                            cell.OriginalWidth = cell.Width;
                            cell.OriginalHeight = cell.Height;
                        }
                    }
                    break;
                }
            case DragMode.Draw:
                {
                    int x = Snap(mouse.X), y = Snap(mouse.Y);
                    int sx = (int)DragStart.X, sy = (int)DragStart.Y;
                    DrawRect = new CellRect(Math.Min(sx, x), Math.Min(sy, y), Math.Abs(x - sx), Math.Abs(y - sy));
                    break;
                }
        }
    }

    private void EndDrag()
    {
        switch (Drag)
        {
            case DragMode.Move:
            case DragMode.Resize:
                EndChange();
                break;
            case DragMode.Draw:
                if (DrawRect.W >= 1 && DrawRect.H >= 1)
                {
                    var template = Primary ?? Atlas.SubTextures.LastOrDefault();
                    AddCell(template, LayoutGeometry.NextName(Atlas, template), DrawRect);
                }
                break;
        }

        Drag = DragMode.None;
    }
    #endregion

    #region Drawing
    private static Vector2[] Corners(CellRect r, Vector2 imageMin, Vector2 scale)
    {
        var min = imageMin + new Vector2(r.X, r.Y) * scale;
        var max = imageMin + new Vector2(r.Right, r.Bottom) * scale;
        return [min, new Vector2(max.X, min.Y), max, new Vector2(min.X, max.Y)];
    }

    private void DrawCells(Vector2 imageMin, Vector2 scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var normal = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell);
        var half = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell_Half);
        var hoveredColor = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell_Hovered);
        var selectedColor = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell_Selected);

        void Outline(SubTexture c, uint color, float thickness)
        {
            var r = CellRect.Of(c);
            var min = imageMin + new Vector2(r.X, r.Y) * scale;
            var max = imageMin + new Vector2(r.Right, r.Bottom) * scale;
            drawList.AddRect(min, max, color, 0, ImDrawFlags.None, thickness);

            if (CFG.Current.TextureViewerLayout_ShowNames && max.X - min.X > 40 && max.Y - min.Y > 14)
            {
                var name = c.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? c.Name[..^4] : c.Name;
                drawList.AddText(min + new Vector2(2, 1), color, name);
            }
        }

        if (CFG.Current.TextureViewerLayout_ShowCells)
        {
            foreach (var c in Atlas.SubTextures)
            {
                if (!Selected.Contains(c) && c != Hovered)
                    Outline(c, c.IsHalf ? half : normal, 1f);
            }
        }

        if (Hovered != null && !Selected.Contains(Hovered))
            Outline(Hovered, hoveredColor, 2f);

        foreach (var c in Selected)
            Outline(c, selectedColor, 2f);

        // Corner handles of a single selected rectangle
        if (IsEditable && SelectionIsOneRect)
        {
            foreach (var p in Corners(CellRect.Of(Selected.First()), imageMin, scale))
                drawList.AddRectFilled(p - new Vector2(HandleSize / 2), p + new Vector2(HandleSize / 2), selectedColor);
        }

        if (Drag == DragMode.Draw && DrawRect.W > 0 && DrawRect.H > 0)
        {
            var min = imageMin + new Vector2(DrawRect.X, DrawRect.Y) * scale;
            var max = imageMin + new Vector2(DrawRect.Right, DrawRect.Bottom) * scale;
            drawList.AddRect(min, max, selectedColor, 0, ImDrawFlags.None, 2f);
            drawList.AddText(max + new Vector2(4, 0), selectedColor, $"{DrawRect.W} x {DrawRect.H}");
        }
    }

    private void CellTooltip(SubTexture c)
    {
        var r = CellRect.Of(c);
        var aliases = LayoutGeometry.AliasGroup(Atlas, c).Where(a => a != c).Select(a => a.Name).ToList();

        var text = $"{c.Name}\n{r.X}, {r.Y}  {r.W} x {r.H}";
        if (c.IsHalf)
            text += $"\n{LOC.Get("TEXVIEW_Layout_Half_Cell")}";
        if (aliases.Count > 0)
            text += $"\n{LOC.Get("TEXVIEW_Layout_Aliases", string.Join(", ", aliases.Take(6)))}{(aliases.Count > 6 ? " ..." : "")}";

        ImGui.SetTooltip(text);
    }
    #endregion

    private void Keyboard(ImGuiIOPtr io)
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || io.WantTextInput || Drag != DragMode.None)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            Selected.Clear();

        if (!IsEditable || Selected.Count == 0)
            return;

        var step = io.KeyShift ? 10 : 1;

        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow)) Nudge(-step, 0);
        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow)) Nudge(step, 0);
        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow)) Nudge(0, -step);
        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow)) Nudge(0, step);
        if (ImGui.IsKeyPressed(ImGuiKey.Delete)) DeleteSelected();
    }

    /// <summary>Scrolls the viewport window so the requested cell is in the middle.</summary>
    private void ScrollToRequested(Vector2 imageMin, Vector2 scale)
    {
        if (ScrollTo == null)
            return;

        var r = CellRect.Of(ScrollTo);
        ScrollTo = null;

        var scroll = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
        var contentImageMin = imageMin - ImGui.GetWindowPos() + scroll;
        var centre = contentImageMin + new Vector2(r.X + r.W / 2f, r.Y + r.H / 2f) * scale;

        ImGui.SetScrollX(Math.Max(0, centre.X - ImGui.GetWindowWidth() / 2));
        ImGui.SetScrollY(Math.Max(0, centre.Y - ImGui.GetWindowHeight() / 2));
    }
}
