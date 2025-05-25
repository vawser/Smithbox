using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Scene;
using StudioCore.ViewportNS;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor.Core;

public class MapViewportView
{
    public Smithbox BaseEditor;
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private Sdl2Window Window;
    private GraphicsDevice Device;
    public RenderScene RenderScene;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public PlacementEntity PlacementOrb;

    public MapViewportView(MapEditorScreen editor, ProjectEntry project, Smithbox baseEditor)
    {
        Editor = editor;
        Project = project;
        BaseEditor = baseEditor;

        Window = baseEditor._context.Window;
        Device = baseEditor._context.Device;

        Rect = Window.Bounds;

        if (Device != null)
        {
            RenderScene = new RenderScene();
        }
    }

    public void Setup()
    {
        if (Device != null && !Smithbox.LowRequirementsMode)
        {
            Viewport = new ViewportNS.Viewport(BaseEditor, Editor, null, ViewportType.MapEditor, "Mapeditvp", Rect.Width, Rect.Height);

            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport(BaseEditor, Editor, null, ViewportType.MapEditor, "Mapeditvp", Rect.Width, Rect.Height);
        }
    }

    public Vector3 GetCameraPosition()
    {
        if (Device != null)
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
        if (Device != null)
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
        if (Device != null)
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
        Viewport.OnGui();

        if (!Smithbox.LowRequirementsMode)
        {
            if (Editor.Universe != null && PlacementOrb == null)
            {
                PlacementOrb = new PlacementEntity(Editor);
            }

            if (PlacementOrb != null)
            {
                PlacementOrb.UpdateRenderModel(Editor);
            }
        }
    }

    public void Update(float deltatime)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, deltatime);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
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
