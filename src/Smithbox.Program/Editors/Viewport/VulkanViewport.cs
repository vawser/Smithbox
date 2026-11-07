using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

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
    public GraphicsDevice Device;

    public Gizmos Gizmos;

    public ViewportGrid PrimaryGrid;
    public ViewportGrid SecondaryGrid;
    public ViewportGrid TertiaryGrid;

    /// <summary>
    /// Gizmo target for grid movement mode, see <see cref="ViewportGridMovement" />.
    /// </summary>
    private ViewportGridGizmoTarget GridGizmoTarget;

    public RenderScene RenderScene;
    public ViewportSelection ViewportSelection;
    public SceneRenderPipeline ViewPipeline;

    public Veldrid.Viewport RenderViewport;

    public ViewportMenu ViewportMenu;
    public ViewportOverlay ViewportOverlay;
    public ResourceLoadWindow ResourceLoadWindow;

    public ClickSelection ClickSelection;
    public BoxSelection BoxSelection;

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
    /// The dock node this viewport was last displayed in.
    /// </summary>
    public uint LastDockId { get; private set; }

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
        ResourceLoadWindow = new();

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

        if (owner is TimeActUniverse timeActUniverse)
        {
            RenderScene = timeActUniverse.View.RenderScene;
            ViewportSelection = timeActUniverse.View.ViewportSelection;
            ActionManager = timeActUniverse.View.ViewportActionManager;
        }

        if (owner is GparamUniverse gparamUniverse)
        {
            RenderScene = gparamUniverse.View.RenderScene;
            ViewportSelection = gparamUniverse.View.ViewportSelection;
            ActionManager = gparamUniverse.View.ViewportActionManager;
        }

        if (RenderScene != null && Device != null)
        {
            ViewPipeline = new SceneRenderPipeline(RenderScene, Device, width, height);

            // The screen effects run here, TAA among them, which the scene is jittered for
            ViewPipeline.AllowTaa = true;

            // The irradiance volume table holds the Map Editor's loaded maps
            ViewPipeline.UseIrradianceVolumes = Owner is MapUniverse;

            // So is the lighting of their GPARAMs, and their BTL lights
            ViewPipeline.UseMapLighting = Owner is MapUniverse;
            ViewPipeline.UseMapLights = Owner is MapUniverse;

            // The Model and Time Act Editors have no map, so they borrow the lighting and a volume of one
            var environmentProject = owner switch
            {
                ModelUniverse model => model.Project,
                TimeActUniverse timeAct => timeAct.Project,
                _ => null
            };

            if (environmentProject?.Handler.ViewportEnvironment is { } environment && environment.CanUse())
            {
                ViewPipeline.UseMapLighting = true;
                ViewPipeline.MapLightingSource = () => environment.ActiveLighting;

                if (environment.Table != null)
                {
                    ViewPipeline.UseIrradianceVolumes = true;
                    ViewPipeline.VolumeTable = environment.Table;
                }
            }

            // The Gparam Editor shows a map of its own, lit by the GPARAM it has selected, unsaved edits and all.
            // The viewport is made while its window is, so the window is looked up when the lighting is drawn.
            if (owner is GparamUniverse gparamOwner &&
                GparamProfile.For(gparamOwner.Project.Descriptor.ProjectType).Supported)
            {
                var gparamView = gparamOwner.View;

                ViewPipeline.UseMapLighting = true;
                ViewPipeline.MapLightingSource = () => gparamView.ViewportWindow?.Lighting;
            }

            // Its map's volumes go in a table of its own, and its BTL lights in the shared one, which the editor in
            // focus fills (see GparamMapEnvironment)
            if (owner is GparamUniverse gparamMap)
            {
                ViewPipeline.UseMapLights = true;

                if (gparamMap.Environment.VolumeTable != null)
                {
                    ViewPipeline.UseIrradianceVolumes = true;
                    ViewPipeline.VolumeTable = gparamMap.Environment.VolumeTable;
                }
            }

            ViewportCamera.UpdateProjectionMatrix();

            // The scene is drawn into the pipeline's HDR target, which is the size of the viewport, on the
            // offscreen list. Something has to be bound for its draws, so a hidden viewport keeps a 1x1 target.
            ViewPipeline.SetViewportSetupAction((d, cl) =>
            {
                ViewPipeline.Target.EnsureSize(d, (uint)Math.Max(Width, 1), (uint)Math.Max(Height, 1));
                ViewPipeline.Target.BeginScene(cl, ViewPipeline.BackgroundColor);
            });

            // The overlays are drawn on the swapchain, once the scene has been tone mapped onto it, so their colors
            // are kept as they are
            ViewPipeline.SetOverlayViewportSetupAction((d, cl) =>
            {
                // The screen effects run on the scene first, binding their own targets
                var frameLighting = ViewPipeline.ActiveMapLighting;
                var toneMapSource = IsViewportVisible
                    ? ViewPipeline.PostProcess.Run(d, cl, ViewPipeline.Target, frameLighting)
                    : null;

                // A debug view of display values is shown as it is, and so is Editor mode's fullbright albedo, which
                // no exposure or curve should change
                var passThrough = ViewPipeline.SurfaceView.ShowsDisplayValues || !RenderMode.IsGame;

                // With FXAA the tone map goes into its image first, and FXAA takes that to the screen
                var fxaa = IsViewportVisible && ViewPipeline.Fxaa.IsActive(frameLighting);

                if (fxaa)
                {
                    ViewPipeline.Fxaa.BeginImage(d, cl, ViewPipeline.Target);
                    ViewPipeline.ToneMap.Draw(d, cl, toneMapSource, FxaaPass.ImageOutputs, Vector2.Zero,
                        frameLighting, ViewPipeline.AutoExposure, passThrough);
                }

                cl.SetFramebuffer(Device.SwapchainFramebuffer);
                cl.SetViewport(0, RenderViewport);
                if (IsViewportVisible)
                {
                    var offset = new Vector2(RenderViewport.X, RenderViewport.Y);

                    if (fxaa)
                    {
                        ViewPipeline.Fxaa.Draw(d, cl, Device.SwapchainFramebuffer.OutputDescription, offset, frameLighting);
                    }
                    else
                    {
                        ViewPipeline.ToneMap.Draw(d, cl, toneMapSource, Device.SwapchainFramebuffer.OutputDescription,
                            offset, frameLighting, ViewPipeline.AutoExposure, passThrough);
                    }
                }
                IsViewportVisible = false;
                cl.ClearDepthStencil(0);
            });

            if (owner is MapUniverse || owner is ModelUniverse || owner is TimeActUniverse || owner is GparamUniverse)
            {
                Gizmos = new Gizmos(this, RenderScene.OverlayRenderables);

                PrimaryGrid = new ViewportGrid(RenderScene.OpaqueRenderables);
                SecondaryGrid = new ViewportGrid(RenderScene.OpaqueRenderables);
                TertiaryGrid = new ViewportGrid(RenderScene.OpaqueRenderables);
            }

            ClickSelection = new(this);
            BoxSelection = new(this);
        }

        Instantiated = true;
    }

    public void Display()
    {
        // The resource load panel ends at the very bottom of the window, which would otherwise make it scrollable
        var flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar
            | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0)); // Transparent

        if (ImGui.Begin($@"{LOC.Get("VIEWPORT_Window_Name")}###{ID}", flags))
        {
            Visible = true;

            LastDockId = ImGui.GetWindowDockID();

            HandleMapEditorFocus();
            HandleModelEditorFocus();
            HandleTimeActEditorFocus();
            HandleGparamEditorFocus();

            HandleCompass();

            ViewportMenu.Draw();
            ViewportOverlay.Draw();

            Vector2 p = ImGui.GetWindowPos();
            Vector2 s = ImGui.GetWindowSize();

            // The viewport gives up the space that the resource load panel needs
            var loadPanelHeight = (int)(IsFocusedViewport() ? ResourceLoadWindow.GetHeight(s) : 0);

            Rectangle newViewportSize = new((int)p.X, (int)p.Y + 3, (int)s.X, (int)s.Y - 3 - loadPanelHeight);

            ResizeViewport(Device, newViewportSize);

            // Inputs
            if (InputManager.IsMouseDown(MousebindID.Viewport_Enable_Viewport_Movement) && MouseInViewport())
            {
                ImGui.SetWindowFocus();
                IsViewportSelected = true;
            }
            else if (!InputManager.IsMouseDown(MousebindID.Viewport_Enable_Viewport_Movement))
            {
                IsViewportSelected = false;
            }

            CanInteract = ImGui.IsWindowFocused();

            ClickSelection.Update();

            // Dragging paints while walls are being painted, so it mustn't start a box selection
            if (!IsWallPainting())
            {
                BoxSelection.Update();
            }

            if (loadPanelHeight > 0)
            {
                ResourceLoadWindow.Display(p, s, loadPanelHeight);
            }

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

        // The scene's pixels start at the viewport's corner, in its own target; the overlays' are the window's
        ViewPipeline.UpdateSceneParameters(ViewportCamera.ProjectionMatrix, ViewportCamera.CameraTransform.CameraViewMatrixLH,
            ViewportCamera.CameraTransform.Position, CursorX, CursorY, (int)RenderViewport.X, (int)RenderViewport.Y);

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

        UpdateGizmoTarget();

        Gizmos.Update(ray, CanInteract && MouseInViewport(), IsActiveViewport);

        UpdateGrids();

        bool kbbusy = false;

        var canPick = !Gizmos.IsMouseBusy() && !BoxSelection.IsBoxSelecting() && CanInteract && MouseInViewport();

        // Only the active viewport is updated, so it is the one that paints
        var painting = Owner is MapUniverse mapUniverse &&
                       mapUniverse.View.WallPaintTool.HandleViewportInput(ray, canPick);

        // Dragging a collision vertex takes the clicks too
        if (Owner is MapUniverse collisionUniverse &&
            collisionUniverse.View.CollisionEditorTool.HandleViewportInput(ray, canPick))
        {
            painting = true;
        }

        // So does picking a navmesh face in the Navigation tool
        if (Owner is MapUniverse navigationUniverse &&
            navigationUniverse.View.NavigationTool.HandleViewportInput(ray, canPick))
        {
            painting = true;
        }

        if (canPick)
        {
            kbbusy = ViewportCamera.UpdateInput(window, dt);

            if (!painting)
            {
                ClickSelection.HandlePickingRequest();
            }
        }

        //Gizmos.DebugGui();
        return kbbusy;
    }

    /// <summary>
    /// True if this is the viewport the user last used, out of the viewports in all of the editor's views.
    /// Unlike <see cref="IsActiveViewport"/>, this stays true when the mouse leaves the viewport.
    /// </summary>
    private bool IsFocusedViewport()
    {
        if (Owner is MapUniverse mapUniverse)
        {
            var view = mapUniverse.View;

            return view.Editor.ViewHandler.ActiveView == view
                && view.ViewportHandler.ActiveViewport?.Viewport == this;
        }

        if (Owner is ModelUniverse modelUniverse)
        {
            var view = modelUniverse.View;

            return view.Editor.ViewHandler.ActiveView == view
                && view.ViewportWindow.Viewport == this;
        }

        if (Owner is TimeActUniverse timeActUniverse)
        {
            var view = timeActUniverse.View;

            return view.Editor.ViewHandler.ActiveView == view
                && view.ViewportWindow.Viewport == this;
        }

        if (Owner is GparamUniverse gparamUniverse)
        {
            var view = gparamUniverse.View;

            return view.Editor.ViewHandler.ActiveView == view
                && view.ViewportWindow.Viewport == this;
        }

        return false;
    }

    private bool IsWallPainting()
    {
        return Owner is MapUniverse mapUniverse && mapUniverse.View.WallPaintTool.IsPainting;
    }

    public void HandleCompass()
    {
        if (Owner is MapUniverse mapUniverse)
        {
            mapUniverse.View.CompassTool.DisplayCompass();
        }
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
                    if (wrapper == null)
                        continue;

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

                foreach (var wrapper in modelUniverse.View.Editor.ViewHandler.Views)
                {
                    if (wrapper == null)
                        continue;

                    if (wrapper.ViewportWindow.Viewport == this)
                    {
                        modelUniverse.View.Editor.ViewHandler.ActiveView = wrapper;
                        IsActiveViewport = true;
                    }
                }
            }
            else
            {
                IsActiveViewport = false;
            }
        }
    }
    public void HandleTimeActEditorFocus()
    {
        if (Owner is TimeActUniverse timeActUniverse)
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_Viewport);

                foreach (var wrapper in timeActUniverse.View.Editor.ViewHandler.Views)
                {
                    if (wrapper == null)
                        continue;

                    if (wrapper.ViewportWindow.Viewport == this)
                    {
                        timeActUniverse.View.Editor.ViewHandler.ActiveView = wrapper;
                        IsActiveViewport = true;
                    }
                }
            }
            else
            {
                IsActiveViewport = false;
            }
        }
    }

    public void HandleGparamEditorFocus()
    {
        if (Owner is GparamUniverse gparamUniverse)
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_Viewport);

                foreach (var view in gparamUniverse.View.Editor.ViewHandler.TexViews)
                {
                    if (view == null)
                        continue;

                    if (view.ViewportWindow.Viewport == this)
                    {
                        gparamUniverse.View.Editor.ViewHandler.ActiveView = view;
                        IsActiveViewport = true;
                    }
                }
            }
            else
            {
                IsActiveViewport = false;
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
    ///     Moves the camera such that the whole bounding box is visible, whichever way the camera
    ///     is facing. Camera will face the same direction as before.
    /// </summary>
    /// <param name="box">The bounding box to fit into the view</param>
    public void FitBox(BoundingBox box)
    {
        // Slightly larger than the box so the model doesn't touch the viewport edges
        float radius = Vector3.Distance(box.Max, box.Min) * 0.5f * 1.05f;

        if (radius <= 0 || Width <= 0 || Height <= 0)
        {
            return;
        }

        float aspectRatio = Width / (float)Height;
        float dist = radius;

        if (ViewportCamera.ViewMode is ViewMode.Perspective)
        {
            float halfFovY = CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 360.0f;
            float halfFovX = (float)Math.Atan(Math.Tan(halfFovY) * aspectRatio);

            // The near clip is added so that small models aren't cut by it
            dist = radius / (float)Math.Sin(Math.Min(halfFovX, halfFovY)) + CFG.Current.Viewport_Perspective_Near_Clip;
        }
        else
        {
            ViewportCamera.OrthographicSize = Math.Clamp(radius * 2.0f / Math.Min(aspectRatio, 1.0f),
                ViewportCamera.OrthographicMinSize, ViewportCamera.OrthographicMaxSize);

            ViewportCamera.UpdateProjectionMatrix();
        }

        FramePosition(box.GetCenter(), dist);
    }

    /// <summary>
    ///     Moves the camera position such that it is directly looking at a position.
    public void FramePosition(Vector3 pos, float dist)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, ViewportCamera.CameraTransform.RotationMatrix);
        ViewportCamera.CameraTransform.Position = pos - camdir * dist;
    }

    /// <summary>
    /// Converts a world position to screen space, relative to the viewport's top-left corner
    /// Works correctly for perspective, orthographic, and oblique projections
    /// Returns whether the point is in front of the camera
    /// </summary>
    public Vector2 WorldToScreen(Vector3 worldPos, out bool inFront)
    {
        Vector4 world = new(worldPos, 1.0f);

        // Get the view-projection matrix
        Matrix4x4 viewProjection = ViewportCamera.CameraTransform.CameraViewMatrixLH * ViewportCamera.ProjectionMatrix;
        Vector4 clip = Vector4.Transform(world, viewProjection);

        bool isOrthographic = ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique;

        if (isOrthographic)
        {
            // In orthographic projection, check if Z is within valid range
            // Use a more lenient check to catch objects near the clipping planes
            inFront = clip.Z >= -1.5f && clip.Z <= 1.5f;

            // No perspective divide needed for orthographic
            Vector2 ndc = new Vector2(clip.X, clip.Y);

            return new Vector2(
                (ndc.X + 1f) / 2f * Width,
                (1f - ndc.Y) / 2f * Height
            );
        }
        else
        {
            // Perspective projection
            // Point is in front if W > 0
            inFront = clip.W > 0.001f; // Small epsilon to avoid division issues

            if (!inFront)
            {
                // Return position far off-screen
                return new Vector2(-100000, -100000);
            }

            // Perspective divide
            Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);

            if (ndc.X < -1f || ndc.X > 1f ||
                ndc.Y < -1f || ndc.Y > 1f ||
                ndc.Z < 0f || ndc.Z > 1f)
            {
                // Return position far off-screen
                return new Vector2(-100000, -100000);
            }

            return new Vector2(
                (ndc.X + 1f) / 2f * Width,
                (1f - ndc.Y) / 2f * Height
            );
        }
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

    /// <summary>
    /// The grid editor this viewport belongs to, and whether that editor is focused. False if it has no grids.
    /// </summary>
    private bool TryGetGridEditor(out ViewportGridEditor editor, out bool isEditorFocused)
    {
        switch (Owner)
        {
            case MapUniverse mapUniverse:
                editor = ViewportGridEditor.Map;
                isEditorFocused = mapUniverse.Project.Handler.FocusedEditor is MapEditorScreen;
                return true;

            case ModelUniverse modelUniverse:
                editor = ViewportGridEditor.Model;
                isEditorFocused = modelUniverse.Project.Handler.FocusedEditor is ModelEditorScreen;
                return true;

            case TimeActUniverse timeActUniverse:
                editor = ViewportGridEditor.TimeAct;
                isEditorFocused = timeActUniverse.Project.Handler.FocusedEditor is TimeActEditorScreen;
                return true;

            case GparamUniverse gparamUniverse:
                editor = ViewportGridEditor.Gparam;
                isEditorFocused = gparamUniverse.Project.Handler.FocusedEditor is GparamEditorScreen;
                return true;
        }

        editor = default;
        isEditorFocused = false;
        return false;
    }

    /// <summary>
    /// Points the gizmo at the moving grid while grid movement mode is on, otherwise at the selection.
    /// </summary>
    private void UpdateGizmoTarget()
    {
        if (Gizmos == null)
        {
            return;
        }

        Gizmos.SnapGrid = null;

        if (TryGetGridEditor(out var editor, out var isEditorFocused))
        {
            GridGizmoTarget ??= new ViewportGridGizmoTarget(editor);

            // Snap to the grid shown in the grid tool, while it is drawn
            var snapGrid = ViewportGridConfig.Get(editor, ViewportGridConfig.GetCurrentGrid(editor));
            if (isEditorFocused && snapGrid.Display)
            {
                Gizmos.SnapGrid = snapGrid;
            }

            // The grids are only drawn while their editor is focused, see ViewportGrid.Update
            if (isEditorFocused && GridGizmoTarget.HasTarget)
            {
                Gizmos.Target = GridGizmoTarget;
                return;
            }
        }

        Gizmos.Target = null;
    }

    public void UpdateGrids()
    {
        if (TryGetGridEditor(out var editor, out var isEditorFocused))
        {
            UpdateGrids(editor, isEditorFocused);
        }
    }

    private void UpdateGrids(ViewportGridEditor editor, bool isEditorFocused)
    {
        PrimaryGrid.Update(isEditorFocused, ViewportGridConfig.Get(editor, ViewportGridType.Primary), ViewportCamera.CameraTransform.Position);
        SecondaryGrid.Update(isEditorFocused, ViewportGridConfig.Get(editor, ViewportGridType.Secondary), ViewportCamera.CameraTransform.Position);
        TertiaryGrid.Update(isEditorFocused, ViewportGridConfig.Get(editor, ViewportGridType.Tertiary), ViewportCamera.CameraTransform.Position);
    }

    public void DelayPicking()
    {
        ClickSelection.TriggerCooldown();
    }
}