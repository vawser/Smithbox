using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.ModelEditor;
using StudioCore.Scene;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.ViewportNS;

public enum ViewportType
{
    MapEditor,
    ModelEditor
}

/// <summary>
/// A viewport is a virtual (i.e. render to texture/render target) view of a scene. It can receive input events to
/// transform the view within a virtual canvas, or it can be manually configured for say rendering thumbnails
/// </summary>
public class Viewport : IViewport
{
    public Smithbox BaseEditor;
    public MapEditorScreen MapEditor;
    public ModelEditorScreen ModelEditor;

    public readonly string ID = "";

    public readonly ViewportActionManager ActionManager;
    public readonly FullScreenQuad ClearQuad;
    public readonly GraphicsDevice Device;

    public readonly Gizmos Gizmos;
    public readonly MapGrid MapGrid;
    public readonly ModelGrid ModelGrid;

    public readonly RenderScene RenderScene;
    public readonly ViewportSelection ViewportSelection;
    public readonly SceneRenderPipeline ViewPipeline;

    public BoundingFrustum Frustum;
    public Matrix4x4 ProjectionMatrix;
    public Veldrid.Viewport RenderViewport;
    public ViewMode ViewMode;

    public ViewportMenu ViewportMenu;
    public BoxSelection BoxSelection;
    public ViewportShortcuts Shortcuts;
    public ViewportOverlay ViewportOverlay;

    /// <summary>
    /// The editor this viewport is located in.
    /// </summary>
    public ViewportType ViewportType;

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

    /// <summary>
    /// The near clipping value for the projection matrix
    /// </summary>
    public float NearClip => CFG.Current.Viewport_RenderDistance_Min;

    /// <summary>
    /// The far clipping value for the projection matrix
    /// </summary>
    public float FarClip => CFG.Current.Viewport_RenderDistance_Max;

    public Viewport(Smithbox baseEditor, MapEditorScreen mapEditor, ModelEditorScreen modelEditor, ViewportType viewportType, string id, int width, int height)
    {
        BaseEditor = baseEditor;
        MapEditor = mapEditor;
        ModelEditor = modelEditor;
        ViewportType = viewportType;

        Shortcuts = new(baseEditor, this);
        BoxSelection = new(baseEditor, this);
        ViewportMenu = new(baseEditor, this);
        ViewportOverlay = new(baseEditor, this);

        ID = id;
        Width = width;
        Height = height;
        Device = BaseEditor._context.Device;

        float depth = Device.IsDepthRangeZeroToOne ? 1 : 0;

        RenderViewport = new Veldrid.Viewport(0, 0, Width, Height, depth, 1.0f - depth);
        ViewportCamera = new ViewportCamera(BaseEditor, this, ViewportType, new Rectangle(0, 0, Width, Height));

        if (viewportType is ViewportType.MapEditor)
        {
            RenderScene = mapEditor.MapViewportView.RenderScene;
            ViewportSelection = mapEditor.ViewportSelection;
            ActionManager = mapEditor.EditorActionManager;
        }

        if (viewportType is ViewportType.ModelEditor)
        {
            RenderScene = modelEditor.RenderScene;
            ViewportSelection = modelEditor._selection;
            ActionManager = modelEditor.EditorActionManager;
        }

        if (RenderScene != null && Device != null)
        {
            ViewPipeline = new SceneRenderPipeline(RenderScene, Device, width, height);

            ProjectionMatrix = Utils.CreatePerspective(Device, false,
                CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f, width / (float)height, NearClip, FarClip);

            Frustum = new BoundingFrustum(ProjectionMatrix);

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

            // Create gizmos
            if (ViewportType is ViewportType.MapEditor)
            {
                Gizmos = new Gizmos(MapEditor, ActionManager, ViewportSelection, RenderScene.OverlayRenderables);
                MapGrid = new MapGrid(MapEditor, RenderScene.OpaqueRenderables);
            }

            if (ViewportType is ViewportType.ModelEditor)
            {
                Gizmos = new Gizmos(ModelEditor, ActionManager, ViewportSelection, RenderScene.OverlayRenderables);
                ModelGrid = new ModelGrid(ModelEditor, RenderScene.OpaqueRenderables);
            }

            ClearQuad = new FullScreenQuad();

            Scene.Renderer.AddBackgroundUploadTask((gd, cl) =>
            {
                ClearQuad.CreateDeviceObjects(gd, cl);
            });
        }
    }

    public void OnGui()
    {
        var flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar;

        if (CFG.Current.Interface_Editor_Viewport)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0)); // Transparent

            if (ImGui.Begin($@"Viewport##{ID}", flags))
            {
                ViewportMenu.Draw();
                ViewportOverlay.Draw();
                Shortcuts.Update();

                if (ViewportType is ViewportType.MapEditor)
                {
                    MapEditor.FocusManager.SwitchWindowContext(MapEditorContext.MapViewport);
                }

                if (ViewportType is ViewportType.ModelEditor)
                {
                    ModelEditor.FocusManager.SwitchWindowContext(MapEditorContext.MapViewport);
                }

                Vector2 p = ImGui.GetWindowPos();
                Vector2 s = ImGui.GetWindowSize();
                Rectangle newvp = new((int)p.X, (int)p.Y + 3, (int)s.X, (int)s.Y - 3);

                ResizeViewport(Device, newvp);

                // Inputs
                if (InputTracker.GetMouseButtonDown(MouseButton.Right) && MouseInViewport())
                {
                    ImGui.SetWindowFocus();
                    IsViewportSelected = true;
                }
                else if (!InputTracker.GetMouseButton(MouseButton.Right))
                {
                    IsViewportSelected = false;
                }

                CanInteract = ImGui.IsWindowFocused();

                BoxSelection.Update();

                IsViewportVisible = true;

                Matrix4x4 proj = Matrix4x4.Transpose(ProjectionMatrix);
                Matrix4x4 view = Matrix4x4.Transpose(ViewportCamera.CameraTransform.CameraViewMatrixLH);
                Matrix4x4 identity = Matrix4x4.Identity;

                if (ViewportType is ViewportType.MapEditor)
                {
                    if (CFG.Current.QuickView_DisplayTooltip)
                    {
                        MapEditor.QuickView.HandleQuickViewTooltip();
                    }
                }
            }
            ImGui.End();
            ImGui.PopStyleColor();
        }
    }


    public void ResizeViewport(GraphicsDevice device, Rectangle newvp)
    {
        Width = newvp.Width;
        Height = newvp.Height;
        X = newvp.X;
        Y = newvp.Y;
        ViewportCamera.UpdateBounds(newvp);
        float depth = device.IsDepthRangeZeroToOne ? 0 : 1;
        RenderViewport = new Veldrid.Viewport(newvp.X, newvp.Y, Width, Height, depth, 1.0f - depth);
    }

    public bool Update(Sdl2Window window, float dt)
    {
        Vector2 pos = InputTracker.MousePosition;
        Ray ray = GetRay(pos.X - X, pos.Y - Y);
        CursorX = (int)pos.X; // - X;
        CursorY = (int)pos.Y; // - Y;
        Gizmos.Update(ray, CanInteract && MouseInViewport());

        if (ViewportType is ViewportType.MapEditor)
        {
            MapGrid.Update(ray);
        }

        if (ViewportType is ViewportType.ModelEditor)
        {
            ModelGrid.Update(ray);
        }

        ViewPipeline.SceneParams.SimpleFlver_Brightness = CFG.Current.Viewport_DefaultRender_Brightness;
        ViewPipeline.SceneParams.SimpleFlver_Saturation = CFG.Current.Viewport_DefaultRender_Saturation;
        ViewPipeline.SceneParams.SelectionColor = new Vector4(CFG.Current.Viewport_DefaultRender_SelectColor.X, CFG.Current.Viewport_DefaultRender_SelectColor.Y,
            CFG.Current.Viewport_DefaultRender_SelectColor.Z, 1.0f);
        bool kbbusy = false;

        if (!Gizmos.IsMouseBusy() && CanInteract && MouseInViewport())
        {
            kbbusy = ViewportCamera.UpdateInput(window, dt);
            if (InputTracker.GetMouseButtonDown(MouseButton.Left))
            {
                ViewPipeline.CreateAsyncPickingRequest();
            }
            if (InputTracker.GetMouseButton(MouseButton.Left) && InputTracker.GetKeyDown(Key.AltLeft))
            {
                ViewPipeline.CreateAsyncPickingRequest();
            }
            if (ViewPipeline.PickingResultsReady)
            {
                EditorScreen targetEditor = null;
                if (ViewportType is ViewportType.MapEditor)
                {
                    targetEditor = MapEditor;
                }
                if (ViewportType is ViewportType.ModelEditor)
                {
                    targetEditor = ModelEditor;
                }

                if (targetEditor != null)
                {
                    ISelectable sel = ViewPipeline.GetSelection();
                    if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
                    {
                        if (sel != null)
                        {
                            if (ViewportSelection.GetSelection().Contains(sel))
                            {
                                ViewportSelection.RemoveSelection(targetEditor, sel);
                            }
                            else
                            {
                                ViewportSelection.AddSelection(targetEditor, sel);
                            }
                        }
                    }
                    else if (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight))
                    {
                        if (sel != null)
                        {
                            ViewportSelection.AddSelection(targetEditor, sel);
                        }
                    }
                    else
                    {
                        ViewportSelection.ClearSelection(targetEditor);
                        if (sel != null)
                        {
                            ViewportSelection.AddSelection(targetEditor, sel);
                        }
                    }
                }
            }
        }

        //Gizmos.DebugGui();
        return kbbusy;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        ProjectionMatrix = Utils.CreatePerspective(device, true, CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f,
            Width / (float)Height, NearClip, FarClip);
        Frustum = new BoundingFrustum(ViewportCamera.CameraTransform.CameraViewMatrixLH * ProjectionMatrix);
        ViewPipeline.TestUpdateView(ProjectionMatrix, ViewportCamera.CameraTransform.CameraViewMatrixLH,
            ViewportCamera.CameraTransform.Position, CursorX, CursorY);
        ViewPipeline.RenderScene(Frustum);
        

        Gizmos.CameraPosition = ViewportCamera.CameraTransform.Position;
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
    public void FrameBox(BoundingBox box)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, ViewportCamera.CameraTransform.RotationMatrix);
        Vector3 pos = box.GetCenter();
        float radius = Vector3.Distance(box.Max, box.Min);
        ViewportCamera.CameraTransform.Position = pos - camdir * (radius + 5); 
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
        float z = 1.0f;
        Vector3 deviceCoords = new(x, y, z);

        // Clip Coordinates
        Vector4 clipCoords = new(deviceCoords.X, deviceCoords.Y, -1.0f, 1.0f);

        // View Coordinates
        Matrix4x4 invProj;
        Matrix4x4.Invert(ProjectionMatrix, out invProj);
        Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);
        viewCoords.Z = 1.0f;
        viewCoords.W = 0.0f;
        Matrix4x4 invView;
        Matrix4x4.Invert(ViewportCamera.CameraTransform.CameraViewMatrixLH, out invView);
        Vector3 worldCoords = Vector4.Transform(viewCoords, invView).XYZ();
        worldCoords = Vector3.Normalize(worldCoords);
        //worldCoords.X = -worldCoords.X;
        return new Ray(ViewportCamera.CameraTransform.Position, worldCoords);
    }

    public bool MouseInViewport()
    {
        Vector2 mp = InputTracker.MousePosition;
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
}