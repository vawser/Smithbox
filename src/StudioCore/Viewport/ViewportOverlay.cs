using Hexa.NET.ImGui;

namespace StudioCore.ViewportNS;

public class ViewportOverlay
{
    public Viewport Parent;
    public Smithbox BaseEditor;

    public ViewportOverlay(Smithbox baseEditor, Viewport parent)
    {
        this.BaseEditor = baseEditor;
        Parent = parent;
    }

    public void Draw()
    {
        if(CFG.Current.Viewport_DisplayControls)
        {
            ImGui.Text("Holding click on the viewport will enable camera controls.");
            ImGui.Text("Use WASD to navigate.");
            ImGui.Text("Use right click to rotate the camera.");
            ImGui.Text("Hold Shift to temporarily speed up and Ctrl to temporarily slow down.");
            ImGui.Text("Scroll the mouse wheel to adjust overall speed.");
        }

        // Map Editor
        if (Parent.ViewportType is ViewportType.MapEditor)
        {
            if (CFG.Current.Viewport_DisplayRotationIncrement)
            {
                Parent.MapEditor.RotationIncrement.DisplayViewportRotateIncrement();
            }
            if (CFG.Current.Viewport_DisplayMovementIncrement)
            {
                Parent.MapEditor.KeyboardMovement.DisplayViewportMovementIncrement();
            }
        }

        // Profiling
        if (CFG.Current.Viewport_Profiling)
        {
            ImGui.Text($@"Cull time: {Parent.RenderScene.OctreeCullTime} ms");
            ImGui.Text($@"Work creation time: {Parent.RenderScene.CPUDrawTime} ms");
            ImGui.Text($@"Scene Render CPU time: {Parent.ViewPipeline.CPURenderTime} ms");
            ImGui.Text($@"Visible objects: {Parent.RenderScene.RenderObjectCount}");
            ImGui.Text($@"Vertex Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalVertexFootprint / 1024 / 1024} MB");
            ImGui.Text($@"Index Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalIndexFootprint / 1024 / 1024} MB");
        }
    }
}

