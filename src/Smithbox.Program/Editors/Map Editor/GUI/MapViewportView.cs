using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

public class MapViewportView
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public PlacementEntity PlacementOrb;

    public MapViewportView(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        editor.Window = Smithbox.Instance._context.Window;
        editor.Device = Smithbox.Instance._context.Device;

        Rect = editor.Window.Bounds;

        if (editor.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(Editor.Universe, "Mapeditvp", Rect.Width, Rect.Height);

                editor.RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
            }
            else
            {
                Viewport = new NullViewport(Editor.Universe, "Mapeditvp", Rect.Width, Rect.Height);
            }
        }
    }

    public Vector3 GetCameraPosition()
    {
        if (Editor.Device != null)
        {
            return Viewport.ViewportCamera.CameraTransform.Position;
        }

        return new Vector3();
    }

    /// <summary>
    /// Get the placement position. For usage with the tools.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPlacementPosition()
    {
        if (Editor.Device != null)
        {
            // Get the camera's view matrix and position
            var viewMatrix = Viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH;
            var cameraPosition = Viewport.ViewportCamera.CameraTransform.Position;

            // Invert the view matrix to get the camera's world matrix
            Matrix4x4.Invert(viewMatrix, out var cameraWorldMatrix);

            // Extract the forward direction from the world matrix (Z axis)
            Vector3 forward = new Vector3(cameraWorldMatrix.M31, cameraWorldMatrix.M32, cameraWorldMatrix.M33);
            Vector3 forwardNormalized = Vector3.Normalize(forward);

            // Calculate the position in front of the camera using configured distance
            Vector3 placementPosition = cameraPosition + forwardNormalized * CFG.Current.PlacementOrb_Distance;

            return placementPosition;
        }

        return Vector3.Zero;
    }

    /// <summary>
    /// Get the placement transform. For the placement of the orb in the screenspace.
    /// </summary>
    /// <returns></returns>
    public Matrix4x4 GetPlacementTransform()
    {
        if (Editor.Device != null)
        {
            // Get the camera's view matrix and position
            var viewMatrix = Viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH;
            var cameraPosition = Viewport.ViewportCamera.CameraTransform.Position;

            Matrix4x4.Invert(viewMatrix, out var cameraWorldMatrix);

            // Extract the forward direction from the view matrix
            Vector3 forward = new Vector3(cameraWorldMatrix.M31, cameraWorldMatrix.M32, cameraWorldMatrix.M33);
            Vector3 forwardNormalized = Vector3.Normalize(forward);

            // Calculate the placement position 5 units in front
            Vector3 placementPosition = cameraPosition + forwardNormalized * CFG.Current.PlacementOrb_Distance;

            // Reconstruct the rotation from the view matrix
            Vector3 up = new Vector3(cameraWorldMatrix.M21, cameraWorldMatrix.M22, cameraWorldMatrix.M23);
            Vector3 right = new Vector3(cameraWorldMatrix.M11, cameraWorldMatrix.M12, cameraWorldMatrix.M13);

            // Create world matrix using right, up, forward, and position
            Matrix4x4 placementTransform = new Matrix4x4(
                right.X, right.Y, right.Z, 0.0f,
                up.X, up.Y, up.Z, 0.0f,
                forward.X, forward.Y, forward.Z, 0.0f,
                placementPosition.X, placementPosition.Y, placementPosition.Z, 1.0f
            );

            return placementTransform;
        }

        return Matrix4x4.Identity;
    }

    public void OnGui()
    {
        Viewport.Display();

        if (Editor.Universe != null && PlacementOrb == null)
        {
            PlacementOrb = new PlacementEntity(Editor.Universe);
        }

        if (PlacementOrb != null)
        {
            PlacementOrb.UpdateRenderModel();
        }
    }

    public void Update(float deltatime)
    {
        ViewportUsingKeyboard = Viewport.Update(Editor.Window, deltatime);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Editor.Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport.IsViewportSelected;
    }
}
