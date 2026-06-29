using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;

namespace StudioCore.Editors.Viewport;

public class ViewportOverlay
{
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
}

