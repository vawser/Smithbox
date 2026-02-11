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
                ImGui.Text("Holding click on the viewport will enable camera controls.");
                ImGui.Text("Use WASD to navigate.");
                ImGui.Text("Use right click to rotate the camera.");
                ImGui.Text("Hold Shift to temporarily speed up and Ctrl to temporarily slow down.");
                ImGui.Text("Scroll the mouse wheel to adjust overall speed.");
                ImGui.Text("Hold Ctrl, Alt and left click to make a box selection.");
            }

            if (Parent.ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique)
            {
                ImGui.Text("Holding click on the viewport will enable camera controls.");
                ImGui.Text("Pan the screen with the middle mouse button.");
                ImGui.Text("Use right click to rotate the camera.");
                ImGui.Text("Hold right click and scroll the mouse wheel to adjust the zoom.");
                ImGui.Text("Hold Ctrl, Alt and left click to make a box selection.");
            }
        }

        // Map Editor
        if (Parent.Owner is MapUniverse mapUniverse)
        {
            if (CFG.Current.Viewport_DisplayRotationIncrement)
            {
                mapUniverse.View.RotationIncrementTool.DisplayViewportRotateIncrement();
            }
            if (CFG.Current.Viewport_DisplayPositionIncrement)
            {
                mapUniverse.View.PositionIncrementTool.DisplayViewportMovementIncrement();
            }
        }

        // Profiling
        if (CFG.Current.Viewport_Display_Profiling)
        {
            ImGui.Text($@"Cull time: {Parent.RenderScene.OctreeCullTime} ms");
            ImGui.Text($@"Work creation time: {Parent.RenderScene.CPUDrawTime} ms");
            ImGui.Text($@"Scene Render CPU time: {Parent.ViewPipeline.CPURenderTime} ms");
            ImGui.Text($@"Visible objects: {Parent.RenderScene.RenderObjectCount}");
            ImGui.Text($@"Vertex Buffers Size: {Renderer.SceneRenderer.GeometryBufferAllocator.TotalVertexFootprint / 1024 / 1024} MB");
            ImGui.Text($@"Index Buffers Size: {Renderer.SceneRenderer.GeometryBufferAllocator.TotalIndexFootprint / 1024 / 1024} MB");
        }
    }
}

