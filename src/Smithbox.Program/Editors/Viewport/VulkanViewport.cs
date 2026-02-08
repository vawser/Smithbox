using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// A viewport is a virtual (i.e. render to texture/render target) view of a scene. It can receive input events to
/// transform the view within a virtual canvas, or it can be manually configured for say rendering thumbnails
/// </summary>
public class VulkanViewport : IViewport
{
    public IUniverse Owner;

    public readonly string ID = "";

    public ViewportActionManager ActionManager;
    public FullScreenQuad ClearQuad;
    public GraphicsDevice Device;

    public Gizmos Gizmos;

    public MapGrid MapPrimaryGrid;
    public MapGrid MapSecondaryGrid;
    public MapGrid MapTertiaryGrid;

    public ModelGrid ModelPrimaryGrid;
    public ModelGrid ModelSecondaryGrid;
    public ModelGrid ModelTertiaryGrid;

    public RenderScene RenderScene;
    public ViewportSelection ViewportSelection;
    public SceneRenderPipeline ViewPipeline;

    public Veldrid.Viewport RenderViewport;

    public ViewportMenu ViewportMenu;
    public BoxSelection BoxSelection;
    public ViewportOverlay ViewportOverlay;

    /// <summary>
    /// If true, the user can interact with the viewport.
    /// </summary>
    private bool CanInteract;

    /// <summary>
    /// X coordinate of the user's cursor
    /// </summary>
    private int CursorX;

    /// <summary>
    /// Y coordinate of the user's cursor
    /// </summary>
    private int CursorY;

    /// <summary>
    /// If true, the viewport is visible.
    /// </summary>
    private bool IsViewportVisible;

    /// <summary>
    /// If true, the viewport window has been selected.
    /// </summary>
    public bool IsViewportSelected { get; set; }

    /// <summary>
    /// Viewport window position: X coordinate
    /// </summary>
    public int X;

    /// <summary>
    /// Viewport window position: Y coordinate
    /// </summary>
    public int Y;

    /// <summary>
    /// The viewport camera
    /// </summary>
    public ViewportCamera ViewportCamera { get; }

    /// <summary>
    /// The width of the viewport window.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the viewport window.
    /// </summary>
    public int Height { get; private set; }

    private bool Instantiated = false;

    public bool Visible = false;

    public bool IsActiveViewport = false;

    public VulkanViewport(IUniverse owner, string id, int width, int height, RenderScene scene)
    {
        Owner = owner;

        ID = id;

        ViewportMenu = new(this);
        ViewportOverlay = new(this);

        Width = width;
        Height = height;
        Device = Smithbox.Instance._context.Device;

        float depth = Device.IsDepthRangeZeroToOne ? 1 : 0;

        RenderViewport = new Veldrid.Viewport(0, 0, Width, Height, depth, 1.0f - depth);
        ViewportCamera = new ViewportCamera(this, new Rectangle(0, 0, Width, Height));

        if (owner is MapUniverse mapUniverse)
        {
            RenderScene = scene;
            ViewportSelection = mapUniverse.View.ViewportSelection;
            ActionManager = mapUniverse.View.ViewportActionManager;
        }

        if (owner is ModelUniverse modelUniverse)
        {
            RenderScene = modelUniverse.View.RenderScene;
            ViewportSelection = modelUniverse.View.ViewportSelection;
            ActionManager = modelUniverse.View.ViewportActionManager;
        }

        if (RenderScene != null && Device != null)
        {
            ViewPipeline = new SceneRenderPipeline(RenderScene, Device, width, height);

            ViewportCamera.UpdateProjectionMatrix();

            ViewPipeline.SetViewportSetupAction((d, cl) =>
            {
                cl.SetFramebuffer(Device.SwapchainFramebuffer);
                cl.SetViewport(0, RenderViewport);
                if (IsViewportVisible)
                {
                    ClearQuad.Render(d, cl);
                }
                IsViewportVisible = false;
            });

            ViewPipeline.SetOverlayViewportSetupAction((d, cl) =>
            {
                cl.SetFramebuffer(Device.SwapchainFramebuffer);
                cl.SetViewport(0, RenderViewport);
                cl.ClearDepthStencil(0);
            });

            if (owner is MapUniverse)
            {
                Gizmos = new Gizmos(owner, ActionManager, ViewportSelection, RenderScene.OverlayRenderables);

                MapPrimaryGrid = new MapGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.MapEditor_PrimaryGrid_Size,
                    CFG.Current.MapEditor_PrimaryGrid_SectionSize,
                    CFG.Current.MapEditor_PrimaryGrid_Color);

                MapSecondaryGrid = new MapGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.MapEditor_SecondaryGrid_Size,
                    CFG.Current.MapEditor_SecondaryGrid_SectionSize,
                    CFG.Current.MapEditor_SecondaryGrid_Color);

                MapTertiaryGrid = new MapGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.MapEditor_TertiaryGrid_Size,
                    CFG.Current.MapEditor_TertiaryGrid_SectionSize,
                    CFG.Current.MapEditor_TertiaryGrid_Color);
            }

            if (owner is ModelUniverse)
            {
                Gizmos = new Gizmos(owner, ActionManager, ViewportSelection, RenderScene.OverlayRenderables);

                ModelPrimaryGrid = new ModelGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.ModelEditor_PrimaryGrid_Size,
                    CFG.Current.ModelEditor_PrimaryGrid_SectionSize,
                    CFG.Current.ModelEditor_PrimaryGrid_Color);

                ModelSecondaryGrid = new ModelGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.ModelEditor_SecondaryGrid_Size,
                    CFG.Current.ModelEditor_SecondaryGrid_SectionSize,
                    CFG.Current.ModelEditor_SecondaryGrid_Color);

                ModelTertiaryGrid = new ModelGrid(owner, RenderScene.OpaqueRenderables,
                    CFG.Current.ModelEditor_TertiaryGrid_Size,
                    CFG.Current.ModelEditor_TertiaryGrid_SectionSize,
                    CFG.Current.ModelEditor_TertiaryGrid_Color);
            }

            BoxSelection = new(this);

            ClearQuad = new FullScreenQuad();

            SceneRenderer.AddBackgroundUploadTask((gd, cl) =>
            {
                ClearQuad.CreateDeviceObjects(gd, cl);
            });

        }

        Instantiated = true;
    }

    public void Display()
    {
        var flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0)); // Transparent

        if (ImGui.Begin($@"Viewport##{ID}", flags))
        {
            Visible = true;

            HandleMapEditorFocus();
            HandleModelEditorFocus();

            ViewportMenu.Draw();
            ViewportOverlay.Draw();

            Vector2 p = ImGui.GetWindowPos();
            Vector2 s = ImGui.GetWindowSize();
            Rectangle newViewportSize = new((int)p.X, (int)p.Y + 3, (int)s.X, (int)s.Y - 3);

            ResizeViewport(Device, newViewportSize);

            // Inputs
            if (InputManager.IsMouseDown(MouseButton.Right) && MouseInViewport())
            {
                ImGui.SetWindowFocus();
                IsViewportSelected = true;
            }
            else if (!InputManager.IsMouseDown(MouseButton.Right))
            {
                IsViewportSelected = false;
            }

            CanInteract = ImGui.IsWindowFocused();

            BoxSelection.Update();

            IsViewportVisible = true;
        }
        else
        {
            Visible = false;
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        ViewportCamera.UpdateProjectionMatrix(true);

        ViewPipeline.UpdateSceneParameters(ViewportCamera.ProjectionMatrix, ViewportCamera.CameraTransform.CameraViewMatrixLH,
            ViewportCamera.CameraTransform.Position, CursorX, CursorY);

        ViewPipeline.RenderScene(ViewportCamera.Frustum);

        Gizmos.CameraPosition = ViewportCamera.CameraTransform.Position;
    }

    public bool Update(Sdl2Window window, float dt)
    {
        if (!Instantiated)
            return false;

        Vector2 pos = InputManager.MousePosition;
        Ray ray = GetRay(pos.X - X, pos.Y - Y);
        CursorX = (int)pos.X; // - X;
        CursorY = (int)pos.Y; // - Y;

        Gizmos.Update(ray, CanInteract && MouseInViewport(), IsActiveViewport);

        UpdateGrids(ray);

        bool kbbusy = false;

        if (!Gizmos.IsMouseBusy() && !BoxSelection.IsBoxSelecting() && CanInteract && MouseInViewport())
        {
            kbbusy = ViewportCamera.UpdateInput(window, dt);

            HandlePickingRequest();
        }

        //Gizmos.DebugGui();
        return kbbusy;
    }

    public void HandleMapEditorFocus()
    {
        if (Owner is MapUniverse mapUniverse)
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_Viewport);

                // Need to loop through since we can't easily pass in the wrapper since this is a shared class with the Model Editor
                foreach (var wrapper in mapUniverse.View.ViewportHandler.Viewports)
                {
                    if (wrapper.Viewport == this)
                    {
                        mapUniverse.View.ViewportHandler.ActiveViewport = wrapper;
                        IsActiveViewport = true;
                    }
                }
            }
            else
            {
                IsActiveViewport = false;
            }

            if (CFG.Current.QuickView_DisplayTooltip)
            {
                mapUniverse.View.AutomaticPreviewTool.HandleQuickViewTooltip();
            }
        }
    }
    public void HandleModelEditorFocus()
    {
        if (Owner is ModelUniverse modelUniverse)
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_Viewport);
                modelUniverse.View.Editor.ViewHandler.ActiveView = modelUniverse.View;
            }
        }
    }

    public void ResizeViewport(GraphicsDevice device, Rectangle newViewportSize)
    {
        Width = newViewportSize.Width;
        Height = newViewportSize.Height;
        X = newViewportSize.X;
        Y = newViewportSize.Y;

        ViewportCamera.UpdateBounds(newViewportSize);

        float depth = device.IsDepthRangeZeroToOne ? 0 : 1;

        RenderViewport = new Veldrid.Viewport(
            newViewportSize.X, 
            newViewportSize.Y, 
            Width, 
            Height, 
            depth, 1.0f - depth);
    }


    public void HandlePickingRequest()
    {
        // Only handle picking if box selection is not active
        if (BoxSelection != null && BoxSelection.IsBoxSelecting())
        {
            return;
        }

        if (InputManager.IsMouseReleased(MouseButton.Left))
        {
            ViewPipeline.CreateAsyncPickingRequest();
        }

        //if (InputManager.IsMousePressed(MouseButton.Left) && InputManager.IsKeyDown(Key.AltLeft))
        //{
        //    ViewPipeline.CreateAsyncPickingRequest();
        //}

        if (ViewPipeline.PickingResultsReady)
        {
            ISelectable sel = ViewPipeline.GetSelection();

            if (InputManager.HasCtrlDown())
            {
                if (sel != null)
                {
                    var selection = ViewportSelection.GetSelection();

                    if (selection.Contains(sel))
                    {
                        ViewportSelection.RemoveSelection(sel);
                    }
                    else
                    {
                        ViewportSelection.AddSelection(sel);
                    }
                }
            }
            else if (InputManager.HasShiftDown())
            {
                if (sel != null)
                {
                    ViewportSelection.AddSelection(sel);
                }
            }
            else
            {
                ViewportSelection.ClearSelection();
                if (sel != null)
                {
                    ViewportSelection.AddSelection(sel);
                }
            }
        }
    }

    public void SetEnvMap(uint index)
    {
        ViewPipeline.EnvMapTexture = index;
    }

    /// <summary>
    ///     Moves the camera position such that it is directly looking at the center of a
    ///     bounding box. Camera will face the same direction as before.
    /// </summary>
    /// <param name="box">The bounding box to frame</param>
    public void FrameBox(BoundingBox box, Vector3 offset, float distance = 5)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, ViewportCamera.CameraTransform.RotationMatrix);
        Vector3 pos = box.GetCenter();
        float radius = Vector3.Distance(box.Max, box.Min);
        ViewportCamera.CameraTransform.Position = pos - camdir * (radius + distance) + offset; 
        // 5 here is a offset so entities with 0 radius have a decent framing
    }

    /// <summary>
    ///     Moves the camera position such that it is directly looking at a position.
    public void FramePosition(Vector3 pos, float dist)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, ViewportCamera.CameraTransform.RotationMatrix);
        ViewportCamera.CameraTransform.Position = pos - camdir * dist;
    }

    public Ray GetRay(float sx, float sy)
    {
        float x = 2.0f * sx / Width - 1.0f;
        float y = 1.0f - 2.0f * sy / Height;

        // Different ray calculation based on projection type
        if (ViewportCamera.ViewMode == ViewMode.Perspective)
        {
            return GetRayPerspective(x, y);
        }
        else
        {
            // For orthographic and oblique, rays are parallel
            return GetRayOrthographic(x, y);
        }
    }
    private Ray GetRayPerspective(float x, float y)
    {
        float z = 1.0f;
        Vector3 deviceCoords = new(x, y, z);

        // Clip Coordinates
        Vector4 clipCoords = new(deviceCoords.X, deviceCoords.Y, -1.0f, 1.0f);

        // View Coordinates
        Matrix4x4 invProj;
        Matrix4x4.Invert(ViewportCamera.ProjectionMatrix, out invProj);
        Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);
        viewCoords.Z = 1.0f;
        viewCoords.W = 0.0f;
        Matrix4x4 invView;
        Matrix4x4.Invert(ViewportCamera.CameraTransform.CameraViewMatrixLH, out invView);
        Vector3 worldCoords = Vector4.Transform(viewCoords, invView).XYZ();
        worldCoords = Vector3.Normalize(worldCoords);
        return new Ray(ViewportCamera.CameraTransform.Position, worldCoords);
    }

    private Ray GetRayOrthographic(float x, float y)
    {
        // For orthographic/oblique, all rays are parallel to the view direction
        Vector3 direction = Vector3.Transform(Vector3.UnitZ, ViewportCamera.CameraTransform.RotationMatrix);

        // Calculate world position at near plane
        Vector4 clipCoords = new Vector4(x, y, -1.0f, 1.0f);

        Matrix4x4 invProj;
        Matrix4x4.Invert(ViewportCamera.ProjectionMatrix, out invProj);
        Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);

        Matrix4x4 invView;
        Matrix4x4.Invert(ViewportCamera.CameraTransform.CameraViewMatrixLH, out invView);
        Vector3 worldPos = Vector4.Transform(viewCoords, invView).XYZ();

        return new Ray(worldPos, Vector3.Normalize(direction));
    }

    public bool MouseInViewport()
    {
        Vector2 mp = InputManager.MousePosition;
        if ((int)mp.X < X || (int)mp.X >= X + Width)
        {
            return false;
        }
        if ((int)mp.Y < Y || (int)mp.Y >= Y + Height)
        {
            return false;
        }
        return true;
    }

    public void UpdateGrids(Ray ray)
    {
        if (Owner is MapUniverse mapUniverse)
        {
            MapPrimaryGrid.Update(
                CFG.Current.MapEditor_DisplayPrimaryGrid,
                ray,
                CFG.Current.MapEditor_PrimaryGrid_Size,
                CFG.Current.MapEditor_PrimaryGrid_SectionSize,
                CFG.Current.MapEditor_PrimaryGrid_Color,
                CFG.Current.MapEditor_PrimaryGrid_Position_X,
                CFG.Current.MapEditor_PrimaryGrid_Position_Y,
                CFG.Current.MapEditor_PrimaryGrid_Position_Z,
                CFG.Current.MapEditor_PrimaryGrid_Rotation_X,
                CFG.Current.MapEditor_PrimaryGrid_Rotation_Y,
                CFG.Current.MapEditor_PrimaryGrid_Rotation_Z,
                ref CFG.Current.MapEditor_RegeneratePrimaryGrid);

            MapSecondaryGrid.Update(
                CFG.Current.MapEditor_DisplaySecondaryGrid,
                ray,
                CFG.Current.MapEditor_SecondaryGrid_Size,
                CFG.Current.MapEditor_SecondaryGrid_SectionSize,
                CFG.Current.MapEditor_SecondaryGrid_Color,
                CFG.Current.MapEditor_SecondaryGrid_Position_X,
                CFG.Current.MapEditor_SecondaryGrid_Position_Y,
                CFG.Current.MapEditor_SecondaryGrid_Position_Z,
                CFG.Current.MapEditor_SecondaryGrid_Rotation_X,
                CFG.Current.MapEditor_SecondaryGrid_Rotation_Y,
                CFG.Current.MapEditor_SecondaryGrid_Rotation_Z,
                ref CFG.Current.MapEditor_RegenerateSecondaryGrid);

            MapTertiaryGrid.Update(
                CFG.Current.MapEditor_DisplayTertiaryGrid,
                ray,
                CFG.Current.MapEditor_TertiaryGrid_Size,
                CFG.Current.MapEditor_TertiaryGrid_SectionSize,
                CFG.Current.MapEditor_TertiaryGrid_Color,
                CFG.Current.MapEditor_TertiaryGrid_Position_X,
                CFG.Current.MapEditor_TertiaryGrid_Position_Y,
                CFG.Current.MapEditor_TertiaryGrid_Position_Z,
                CFG.Current.MapEditor_TertiaryGrid_Rotation_X,
                CFG.Current.MapEditor_TertiaryGrid_Rotation_Y,
                CFG.Current.MapEditor_TertiaryGrid_Rotation_Z,
                ref CFG.Current.MapEditor_RegenerateTertiaryGrid);

        }

        if (Owner is ModelUniverse modelUniverse)
        {
            ModelPrimaryGrid.Update(
                CFG.Current.ModelEditor_DisplayPrimaryGrid,
                ray,
                CFG.Current.ModelEditor_PrimaryGrid_Size,
                CFG.Current.ModelEditor_PrimaryGrid_SectionSize,
                CFG.Current.ModelEditor_PrimaryGrid_Color,
                CFG.Current.ModelEditor_PrimaryGrid_Position_X,
                CFG.Current.ModelEditor_PrimaryGrid_Position_Y,
                CFG.Current.ModelEditor_PrimaryGrid_Position_Z,
                CFG.Current.ModelEditor_PrimaryGrid_Rotation_X,
                CFG.Current.ModelEditor_PrimaryGrid_Rotation_Y,
                CFG.Current.ModelEditor_PrimaryGrid_Rotation_Z,
                ref CFG.Current.ModelEditor_RegeneratePrimaryGrid);

            ModelSecondaryGrid.Update(
                CFG.Current.ModelEditor_DisplaySecondaryGrid,
                ray,
                CFG.Current.ModelEditor_SecondaryGrid_Size,
                CFG.Current.ModelEditor_SecondaryGrid_SectionSize,
                CFG.Current.ModelEditor_SecondaryGrid_Color,
                CFG.Current.ModelEditor_SecondaryGrid_Position_X,
                CFG.Current.ModelEditor_SecondaryGrid_Position_Y,
                CFG.Current.ModelEditor_SecondaryGrid_Position_Z,
                CFG.Current.ModelEditor_SecondaryGrid_Rotation_X,
                CFG.Current.ModelEditor_SecondaryGrid_Rotation_Y,
                CFG.Current.ModelEditor_SecondaryGrid_Rotation_Z,
                ref CFG.Current.ModelEditor_RegenerateSecondaryGrid);

            ModelTertiaryGrid.Update(
                CFG.Current.ModelEditor_DisplayTertiaryGrid,
                ray,
                CFG.Current.ModelEditor_TertiaryGrid_Size,
                CFG.Current.ModelEditor_TertiaryGrid_SectionSize,
                CFG.Current.ModelEditor_TertiaryGrid_Color,
                CFG.Current.ModelEditor_TertiaryGrid_Position_X,
                CFG.Current.ModelEditor_TertiaryGrid_Position_Y,
                CFG.Current.ModelEditor_TertiaryGrid_Position_Z,
                CFG.Current.ModelEditor_TertiaryGrid_Rotation_X,
                CFG.Current.ModelEditor_TertiaryGrid_Rotation_Y,
                CFG.Current.ModelEditor_TertiaryGrid_Rotation_Z,
                ref CFG.Current.ModelEditor_RegenerateTertiaryGrid);
        }

    }
}