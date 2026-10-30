using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
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

        // Map Editor
        if (Parent.Owner is MapUniverse mapUniverse)
        {
            if (CFG.Current.Viewport_DisplayRotationIncrement)
            {
                mapUniverse.View.RotateAction.DisplayViewportHint();
            }
            if (CFG.Current.Viewport_DisplayTranslationIncrement)
            {
                mapUniverse.View.TranslateAction.DisplayViewportHint();
            }
        }

        // Model Editor
        if (Parent.Owner is ModelUniverse modelUniverse)
        {
            var container = modelUniverse.View.Selection.SelectedModelWrapper?.Container;
            var drawFilter = modelUniverse.View.RenderScene.DrawFilter;

            if (container != null)
            {
                if (CFG.Current.ModelEditor_DisplayDummyLabels && drawFilter.HasFlag(RenderFilter.Dummies))
                {
                    DrawEntityLabels(container.Dummies, CFG.Current.Viewport_Dummy_Polygon_Base_Color);
                }

                if (CFG.Current.ModelEditor_DisplayNodeLabels && drawFilter.HasFlag(RenderFilter.Nodes))
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

