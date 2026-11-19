using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextureViewer;

public partial class MovieView
{
    private float Zoom = 0.5f;
    private Vector2 Pan;
    private bool FitPending = true;

    private MovieNode HoveredNode;
    private Vector2 DragMouseStart;
    private bool Panning;

    public bool ShowBounds = true;
    public bool ShowText = true;

    private readonly Dictionary<GFX.DefineShape, GFX.ShapeFill> Fills = new(ReferenceEqualityComparer.Instance);

    public string CanvasTitle => LOC.Get("TEXVIEW_Movie_Window_Stage");

    private GFX.ShapeFill GetFill(GFX.DefineShape shape)
    {
        if (!Fills.TryGetValue(shape, out var fill))
        {
            fill = GFX.ReadFirstFill(shape);
            Fills[shape] = fill;
        }
        return fill;
    }

    public void DisplayCanvas()
    {
        if (!IsActive)
        {
            NoMovie();
            return;
        }

        Scene.Update();

        Toolbar();

        var origin = ImGui.GetCursorScreenPos();
        var size = ImGui.GetContentRegionAvail();
        if (size.X < 8 || size.Y < 8)
            return;

        var stage = Contents.Movie.FrameSize;
        var stageMin = new Vector2(stage.XMin, stage.YMin) / GFX.TwipsPerPixel;
        var stageMax = new Vector2(stage.XMax, stage.YMax) / GFX.TwipsPerPixel;

        if (FitPending)
        {
            Fit(size, stageMin, stageMax);
            FitPending = false;
        }

        ImGui.InvisibleButton("##movieCanvas", size, ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonMiddle);
        var hovered = ImGui.IsItemHovered();
        var activated = ImGui.IsItemActivated();
        var active = ImGui.IsItemActive();
        var released = ImGui.IsItemDeactivated();
        var io = ImGui.GetIO();

        Vector2 ToScreen(Vector2 world) => origin + Pan + world * Zoom;
        Vector2 ToWorld(Vector2 screen) => (screen - origin - Pan) / Zoom;

        var mouseWorld = ToWorld(ImGui.GetMousePos());

        // Zoom about the mouse
        if (hovered && io.MouseWheel != 0)
        {
            var before = mouseWorld;
            Zoom = Math.Clamp(Zoom * (io.MouseWheel > 0 ? 1.15f : 1 / 1.15f), 0.05f, 16f);
            Pan = ImGui.GetMousePos() - origin - before * Zoom;
        }

        // Middle drag (or Space + left drag) pans
        if (activated)
        {
            Panning = ImGui.IsMouseDown(ImGuiMouseButton.Middle) || ImGui.IsKeyDown(ImGuiKey.Space);
            if (!Panning)
                BeginCanvasClick(mouseWorld, io.KeyCtrl);
            DragMouseStart = mouseWorld;
        }

        if (active)
        {
            if (Panning)
                Pan += io.MouseDelta;
            else if (IsDragging)
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                    CancelMove();
                else
                    UpdateMove(mouseWorld - DragMouseStart);
            }
        }

        if (released)
        {
            if (IsDragging)
                EndMove();
            Panning = false;
        }

        var hoveredLeaf = hovered && !active ? Scene.Pick(mouseWorld) : null;

        // Drawing
        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRect(origin, origin + size, true);
        drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(new Vector4(0.08f, 0.08f, 0.09f, 1)));
        drawList.AddRectFilled(ToScreen(stageMin), ToScreen(stageMax), ImGui.GetColorU32(new Vector4(0.16f, 0.16f, 0.18f, 1)));

        foreach (var node in Scene.DrawOrder)
            DrawNode(drawList, node, ToScreen);

        drawList.AddRect(ToScreen(stageMin), ToScreen(stageMax), ImGui.GetColorU32(new Vector4(0.55f, 0.55f, 0.6f, 1)));

        var hoverColor = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell_Hovered);
        var selectColor = ImGui.GetColorU32(UI.Current.ImGui_TextureEditor_Cell_Selected);

        var listHovered = HoveredNode;
        HoveredNode = null;
        foreach (var h in new[] { hoveredLeaf, listHovered })
        {
            if (h != null && h != Selected)
                Outline(drawList, h, ToScreen, hoverColor, 1.5f);
        }

        var selected = Selected;
        if (selected != null)
        {
            Outline(drawList, selected, ToScreen, selectColor, 2f);
            var p = ToScreen(Vector2.Transform(Vector2.Zero, selected.World));
            drawList.AddCircleFilled(p, 3.5f, selectColor);
        }

        drawList.PopClipRect();

        if (hoveredLeaf != null && !IsDragging)
        {
            var named = NamedOrSelf(hoveredLeaf);
            ImGui.SetTooltip(named == hoveredLeaf ? hoveredLeaf.NamePath : $"{named.NamePath}\n  {hoveredLeaf.Label}");
        }

        Keyboard(io);
    }

    private void Toolbar()
    {
        var frames = MovieScene.FrameCount(Contents.Movie.Tags);
        if (frames > 1)
        {
            ImGui.SetNextItemWidth(200 * DPI.UIScale());
            var frame = Scene.RootFrame;
            if (ImGui.SliderInt("##rootFrame", ref frame, 0, frames - 1, LOC.Get("TEXVIEW_Movie_Frame_Format")))
                Scene.RootFrame = frame;
            GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Root_Frame_TT"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(160 * DPI.UIScale());
            LabelPicker("rootLabels", Contents.Movie.Tags, f => Scene.RootFrame = f);
            ImGui.SameLine();
        }

        if (ImGui.Button(LOC.Get("TEXVIEW_Movie_Fit")))
            FitPending = true;
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Fit_TT"));
        ImGui.SameLine();
        if (ImGui.Button("1:1"))
        {
            var centre = ImGui.GetContentRegionAvail() / 2;
            var world = (centre - Pan) / Zoom;
            Zoom = 1;
            Pan = centre - world;
        }
        ImGui.SameLine();
        ImGui.Text($"{Zoom * 100:0}%");
        ImGui.SameLine();
        ImGui.Checkbox(LOC.Get("TEXVIEW_Movie_Show_Bounds"), ref ShowBounds);
        ImGui.SameLine();
        ImGui.Checkbox(LOC.Get("TEXVIEW_Movie_Show_Text"), ref ShowText);
        ImGui.SameLine();
        ImGui.Checkbox(LOC.Get("TEXVIEW_Movie_Move_Whole_Track"), ref MoveWholeTrack);
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Move_Whole_Track_TT"));
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Canvas_Help_TT"));
    }

    private void Fit(Vector2 size, Vector2 stageMin, Vector2 stageMax)
    {
        var stageSize = stageMax - stageMin;
        if (stageSize.X <= 0 || stageSize.Y <= 0)
            return;
        Zoom = Math.Clamp(Math.Min(size.X / stageSize.X, size.Y / stageSize.Y) * 0.95f, 0.05f, 16f);
        Pan = (size - stageSize * Zoom) / 2 - stageMin * Zoom;
    }

    private void BeginCanvasClick(Vector2 mouseWorld, bool ctrl)
    {
        var leaf = Scene.Pick(mouseWorld);
        var selected = Selected;

        if (leaf == null)
        {
            Select(null);
            return;
        }

        // A click inside the selection drags it; else it selects the element under the mouse (its nearest named
        // instance, the level the game addresses; Ctrl picks the image, text or shape itself)
        var inSelection = selected != null && (leaf == selected || leaf.SelfAndDescendantsOfAncestor(selected));
        if (!inSelection || ctrl)
        {
            selected = ctrl ? leaf : NamedOrSelf(leaf);
            Select(selected, true);
        }

        BeginMove(selected);
    }

    private void Keyboard(ImGuiIOPtr io)
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || io.WantTextInput || IsDragging)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            Select(null);

        var node = Selected;
        if (node == null)
            return;

        var step = io.KeyShift ? 10f : 1f;
        var delta = Vector2.Zero;
        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow)) delta.X -= step;
        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow)) delta.X += step;
        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow)) delta.Y -= step;
        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow)) delta.Y += step;
        if (delta != Vector2.Zero)
            Nudge(node, delta);

        // Up a level to the parent instance
        if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && node.Parent != null)
            Select(node.Parent, true);
    }

    #region Drawing
    private static uint Tint(MovieNode node, Vector4 baseColor)
    {
        var c = Vector4.Clamp(baseColor * node.Mult + node.Add, Vector4.Zero, Vector4.One);
        return ImGui.GetColorU32(c);
    }

    private static Vector4 ToVector(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

    private void DrawNode(ImDrawListPtr drawList, MovieNode node, Func<Vector2, Vector2> toScreen)
    {
        switch (node.Character)
        {
            case GFX.DefineExternalImage2 image:
                DrawImage(drawList, node, image, node.World, toScreen);
                break;
            case GFX.DefineShape shape:
                DrawShape(drawList, node, shape, toScreen);
                break;
            case GFX.DefineEditText text:
                DrawText(drawList, node, text, toScreen);
                break;
        }
    }

    private bool DrawImage(ImDrawListPtr drawList, MovieNode node, GFX.DefineExternalImage2 image, Matrix3x2 world, Func<Vector2, Vector2> toScreen)
    {
        var w = image.TargetWidth;
        var h = image.TargetHeight;
        var p0 = toScreen(Vector2.Transform(Vector2.Zero, world));
        var p1 = toScreen(Vector2.Transform(new Vector2(w, 0), world));
        var p2 = toScreen(Vector2.Transform(new Vector2(w, h), world));
        var p3 = toScreen(Vector2.Transform(new Vector2(0, h), world));

        var resolved = Bank.Images.Get(image.FileName, out _);
        if (resolved != null && resolved.IsReady && resolved.TextureId is ImTextureID id)
        {
            var uv0 = resolved.Uv0;
            var uv1 = resolved.Uv1;
            drawList.AddImageQuad(id, p0, p1, p2, p3, uv0, new Vector2(uv1.X, uv0.Y), uv1, new Vector2(uv0.X, uv1.Y), Tint(node, Vector4.One));
            return true;
        }

        if (ShowBounds)
            drawList.AddQuad(p0, p1, p2, p3, ImGui.GetColorU32(new Vector4(1, 0.3f, 0.8f, 0.5f)), 1f);
        return false;
    }

    private void DrawShape(ImDrawListPtr drawList, MovieNode node, GFX.DefineShape shape, Func<Vector2, Vector2> toScreen)
    {
        var fill = GetFill(shape);
        var corners = MovieScene.WorldCorners(node).Select(toScreen).ToArray();

        if (fill?.Kind == GFX.FillKind.Bitmap && fill.Matrix != null
            && Contents.Characters.GetValueOrDefault(fill.BitmapId) is GFX.DefineExternalImage2 image)
        {
            // The fill matrix maps bitmap pixels to shape twips
            var m = fill.Matrix.ToMatrix3x2();
            m.M11 /= GFX.TwipsPerPixel; m.M12 /= GFX.TwipsPerPixel;
            m.M21 /= GFX.TwipsPerPixel; m.M22 /= GFX.TwipsPerPixel;

            drawList.PushClipRect(Vector2.Min(Vector2.Min(corners[0], corners[1]), Vector2.Min(corners[2], corners[3])),
                Vector2.Max(Vector2.Max(corners[0], corners[1]), Vector2.Max(corners[2], corners[3])), true);
            var drawn = DrawImage(drawList, node, image, m * node.World, toScreen);
            drawList.PopClipRect();
            if (drawn)
                return;
        }
        else if (fill?.Kind is GFX.FillKind.Solid or GFX.FillKind.Gradient)
        {
            var color = ToVector(fill.Color);
            if (fill.Kind == GFX.FillKind.Gradient)
                color.W *= 0.6f;
            drawList.AddQuadFilled(corners[0], corners[1], corners[2], corners[3], Tint(node, color));
            return;
        }

        if (ShowBounds)
            drawList.AddQuad(corners[0], corners[1], corners[2], corners[3], ImGui.GetColorU32(new Vector4(0.4f, 0.8f, 1f, 0.4f)), 1f);
    }

    private static readonly Regex HtmlBreak = new(@"<br\s*/?>|</p>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex HtmlTag = new(@"<[^>]*>", RegexOptions.CultureInvariant);

    private static string PlainText(GFX.DefineEditText text)
    {
        var s = text.InitialText ?? "";
        if (text.Html)
        {
            s = HtmlBreak.Replace(s, "\n");
            s = HtmlTag.Replace(s, "");
            s = System.Net.WebUtility.HtmlDecode(s);
        }
        return s.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');
    }

    private unsafe void DrawText(ImDrawListPtr drawList, MovieNode node, GFX.DefineEditText text, Func<Vector2, Vector2> toScreen)
    {
        var corners = MovieScene.WorldCorners(node).Select(toScreen).ToArray();

        if (ShowBounds)
            drawList.AddQuad(corners[0], corners[1], corners[2], corners[3], ImGui.GetColorU32(new Vector4(1f, 0.85f, 0.3f, 0.35f)), 1f);

        if (!ShowText)
            return;

        var content = PlainText(text);
        if (content.Length == 0)
            return;

        // Text is drawn unrotated, at the world scale of the instance
        var axis = Vector2.TransformNormal(Vector2.UnitY, node.World).Length();
        var fontPx = (text.FontHeight > 0 ? text.FontHeight / GFX.TwipsPerPixel : 20f) * axis * Zoom;
        if (fontPx < 3)
            return;

        var font = ImGui.GetFont();
        var baseSize = ImGui.GetFontSize();
        var color = Tint(node, text.TextColor is Color c ? ToVector(c) : Vector4.One);

        var min = Vector2.Min(Vector2.Min(corners[0], corners[1]), Vector2.Min(corners[2], corners[3]));
        var max = Vector2.Max(Vector2.Max(corners[0], corners[1]), Vector2.Max(corners[2], corners[3]));
        var margin = text.Layout == null ? 0 : text.Layout.LeftMargin / GFX.TwipsPerPixel * axis * Zoom;
        var align = text.Layout?.Align ?? GFX.TextAlign.Left;

        drawList.PushClipRect(min - new Vector2(1), max + new Vector2(1, fontPx * 0.3f), true);
        var y = min.Y + 2 * axis * Zoom;
        foreach (var line in content.Split('\n'))
        {
            var width = ImGui.CalcTextSize(line).X * fontPx / baseSize;
            var x = align switch
            {
                GFX.TextAlign.Center => (min.X + max.X - width) / 2,
                GFX.TextAlign.Right => max.X - width - margin,
                _ => min.X + margin,
            };
            drawList.AddText(font, fontPx, new Vector2(x, y), color, line);
            y += fontPx * 1.15f;
        }
        drawList.PopClipRect();
    }

    private static void Outline(ImDrawListPtr drawList, MovieNode node, Func<Vector2, Vector2> toScreen, uint color, float thickness)
    {
        if (node.HasBounds && node.Children.Count == 0)
        {
            var c = MovieScene.WorldCorners(node).Select(toScreen).ToArray();
            drawList.AddQuad(c[0], c[1], c[2], c[3], color, thickness);
        }
        else if (node.WorldMin.X <= node.WorldMax.X)
        {
            drawList.AddRect(toScreen(node.WorldMin), toScreen(node.WorldMax), color, 0, ImDrawFlags.None, thickness);
        }
    }
    #endregion
}

public static class MovieNodeExtensions
{
    /// <summary>True when the node is the given node or inside it.</summary>
    public static bool SelfAndDescendantsOfAncestor(this MovieNode node, MovieNode ancestor)
    {
        for (var n = node; n != null; n = n.Parent)
            if (n == ancestor)
                return true;
        return false;
    }
}
