using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

public class ViewportOverlay
{
    // Keeps a label clear of the entity it is attached to
    private static readonly Vector2 LabelOffset = new(6f, -6f);

    public VulkanViewport Parent;

    public ViewportOverlay(VulkanViewport parent)
    {
        Parent = parent;
    }

    public void Draw()
    {
        if(CFG.Current.Viewport_DisplayControls)
        {
            if (Parent.ViewportCamera.ViewMode is ViewMode.Perspective)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Overlay_Perspective_Controls"));
            }

            if (Parent.ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Overlay_OrthoOblique_Controls"));
            }
        }

        var camera = Parent.ViewportCamera;

        if (camera.IsSlicing)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Slice", $"{camera.SliceHalfDepth * 2.0f:0.##}", $"{camera.FocusDistance:0.##}"));
        }

        if (camera.ViewMode is not ViewMode.Perspective && CFG.Current.Viewport_DisplayScaleBar)
        {
            DrawScaleBar(camera);
        }

        // Map Editor
        if (Parent.Owner is MapUniverse mapUniverse)
        {
            // The map that the content tree shows and new objects go into
            var activeMap = mapUniverse.View.Selection.SelectedMapContainer;

            if (CFG.Current.Viewport_DisplayActiveMap && activeMap != null)
            {
                var alias = AliasHelper.GetMapNameAlias(mapUniverse.View.Project, activeMap.Name);
                var mapText = string.IsNullOrEmpty(alias) ? activeMap.Name : $"{activeMap.Name}: {alias}";

                ImGui.Text(LOC.Get("VIEWPORT_Overlay_Active_Map", mapText));
            }

            if (CFG.Current.Viewport_DisplayRotationIncrement)
            {
                mapUniverse.View.RotateAction.DisplayViewportHint();
            }
            if (CFG.Current.Viewport_DisplayTranslationIncrement)
            {
                mapUniverse.View.TranslateAction.DisplayViewportHint();
            }

            // The Navigation tool's gate numbers and costs
            mapUniverse.View.NavigationTool.DrawGateLabels(Parent);

            // The Ruler tool's distance
            mapUniverse.View.RulerTool.DrawLabels(Parent);

            // The tile outlines' map IDs and names
            mapUniverse.View.TileOutlineTool.DrawLabels(Parent);

            // The Create at Point marker
            mapUniverse.View.CreateAction.CreateAtPoint.DrawOverlay(Parent);

            // First person: crosshair and controls
            mapUniverse.View.FirstPersonTool.DrawOverlay(Parent);
        }

        // Model Editor
        if (Parent.Owner is ModelUniverse modelUniverse)
        {
            var container = modelUniverse.View.Selection.SelectedModelWrapper?.Container;
            var drawFilter = modelUniverse.View.RenderScene.DrawFilter;

            if (container != null)
            {
                // Optionally only the selected ones, as a character has hundreds of dummies and bones
                var selection = modelUniverse.View.ViewportSelection.GetSelection();

                IEnumerable<Entity> Labelled(List<Entity> entities) => CFG.Current.ModelEditor_Labels_Selected_Only
                    ? entities.Where(selection.Contains)
                    : entities;

                if (CFG.Current.ModelEditor_DisplayDummyLabels && drawFilter.HasFlag(RenderFilter.Dummies))
                {
                    DrawEntityLabels(Labelled(container.Dummies), CFG.Current.Viewport_Dummy_Polygon_Base_Color);
                }

                if (CFG.Current.ModelEditor_DisplayNodeLabels && drawFilter.HasFlag(RenderFilter.Nodes))
                {
                    DrawEntityLabels(Labelled(container.Nodes), CFG.Current.Viewport_Bone_Marker_Base_Color);
                }
            }
        }

        // Time Act Editor
        if (Parent.Owner is TimeActUniverse timeActUniverse)
        {
            var container = timeActUniverse.View.Selection.SelectedTimeActModelWrapper?.Container;
            var drawFilter = timeActUniverse.View.RenderScene.DrawFilter;

            if (container != null)
            {
                if (CFG.Current.TimeActEditor_DisplayDummyLabels && drawFilter.HasFlag(RenderFilter.Dummies))
                {
                    DrawEntityLabels(container.Dummies, CFG.Current.Viewport_Dummy_Polygon_Base_Color);
                }

                if (CFG.Current.TimeActEditor_DisplayNodeLabels && drawFilter.HasFlag(RenderFilter.Nodes))
                {
                    DrawEntityLabels(container.Nodes, CFG.Current.Viewport_Bone_Marker_Base_Color);
                }
            }
        }

        // Profiling
        if (CFG.Current.Viewport_Display_Profiling)
        {
            var vertexBufferSize = Renderer.SceneRenderer.GeometryBufferAllocator.TotalVertexFootprint / 1024 / 1024;
            var indexBufferSize = Renderer.SceneRenderer.GeometryBufferAllocator.TotalIndexFootprint / 1024 / 1024;

            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Cull_Time", Parent.RenderScene.OctreeCullTime));
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Work_Creation_Time", Parent.RenderScene.CPUDrawTime));
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Scene_Render_CPU_Time", Parent.ViewPipeline.CPURenderTime));
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Visible_Objects", Parent.RenderScene.RenderObjectCount));
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Vertex_Buffers_Size", vertexBufferSize));
            ImGui.Text(LOC.Get("VIEWPORT_Overlay_Index_Buffers_Size", indexBufferSize));
        }
    }

    /// <summary>
    ///     A bar in the bottom left corner a round number of metres long, which an orthographic view can show since a
    ///     metre is the same number of pixels everywhere in it. Its length is the largest 1, 2 or 5 x 10^n that fits
    ///     in about 150 pixels. Measured on the focus plane, where an oblique view is unsheared.
    /// </summary>
    private void DrawScaleBar(ViewportCamera camera)
    {
        if (Parent.Height <= 0 || camera.OrthographicSize <= 0)
            return;

        var pixelsPerMetre = Parent.Height / camera.OrthographicSize;
        var target = 150.0f / pixelsPerMetre;

        var magnitude = MathF.Pow(10.0f, MathF.Floor(MathF.Log10(target)));
        var length = magnitude;

        foreach (var step in new[] { 2.0f, 5.0f, 10.0f })
        {
            if (magnitude * step <= target)
                length = magnitude * step;
        }

        var barPixels = length * pixelsPerMetre;
        var start = new Vector2(Parent.X + 16.0f, Parent.Y + Parent.Height - 24.0f);
        var end = start + new Vector2(barPixels, 0.0f);

        var drawList = ImGui.GetWindowDrawList();
        var shadow = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.8f));
        var color = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));
        var tick = new Vector2(0.0f, 6.0f);

        foreach (var (offset, col, thickness) in new[] { (Vector2.One, shadow, 3.0f), (Vector2.Zero, color, 2.0f) })
        {
            drawList.AddLine(start + offset, end + offset, col, thickness);
            drawList.AddLine(start - tick + offset, start + offset, col, thickness);
            drawList.AddLine(end - tick + offset, end + offset, col, thickness);
        }

        var label = length >= 1000.0f ? $"{length / 1000.0f:0.##} km"
            : length >= 1.0f ? $"{length:0.##} m"
            : $"{length * 100.0f:0.##} cm";

        var textPosition = start + new Vector2(0.0f, -6.0f - ImGui.GetFontSize() - 2.0f);
        drawList.AddText(textPosition + Vector2.One, shadow, label);
        drawList.AddText(textPosition, color, label);
    }

    /// <summary>
    /// Draws the viewport label of each visible entity at the entity's on-screen position.
    /// </summary>
    private unsafe void DrawEntityLabels(IEnumerable<Entity> entities, Vector3 color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var font = ImGui.GetFont();
        var fontSize = ImGui.GetFontSize() * CFG.Current.ModelEditor_LabelScale;

        var origin = new Vector2(Parent.X, Parent.Y);
        var textColor = ImGui.GetColorU32(new Vector4(color, 1f));
        var shadowColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f));

        foreach (var entity in entities)
        {
            var label = entity.ViewportLabel;

            if (label == null || entity.RenderSceneMesh == null || !entity.EditorVisible)
                continue;

            // The render mesh rather than the entity, since animation moves the mesh alone
            var position = Parent.WorldToScreen(entity.RenderSceneMesh.World.Translation, out bool inFront);

            if (!inFront || position.X < 0 || position.X > Parent.Width ||
                position.Y < 0 || position.Y > Parent.Height)
                continue;

            var textPosition = origin + position + LabelOffset;

            drawList.AddText(font, fontSize, textPosition + Vector2.One, shadowColor, label);
            drawList.AddText(font, fontSize, textPosition, textColor, label);
        }
    }
}

