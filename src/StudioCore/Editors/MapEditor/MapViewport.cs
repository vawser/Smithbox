using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using static StudioCore.Resource.ResourceManager;
using Rectangle = Veldrid.Rectangle;

namespace StudioCore.Editors.MapEditorNS;

public class MapViewport
{
    public MapEditor Editor;

    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    private readonly FullScreenQuad _clearQuad;
    private readonly GraphicsDevice _device;
    private readonly float _dragThreshold = 5f;

    private readonly Gizmos _gizmos;

    private readonly DbgPrimWire _rayDebug = null;

    private readonly SceneRenderPipeline _viewPipeline;

    private bool _canInteract;

    private int _cursorX;
    private int _cursorY;
    private Vector2 _dragEnd;
    private Vector2 _dragStart;

    private BoundingFrustum _frustum;

    private bool _isDragging;

    private bool _mouseDragStarted;

    private Matrix4x4 _projectionMat;

    private Veldrid.Viewport _renderViewport;

    private float _selectionTolerance = 5f;

    private bool _vpvisible;
    private bool DebugRayCastDraw = false;

    public int X;
    public int Y;
    public bool DrawGrid { get; set; } = true;

    /// <summary>
    ///     The camera in this scene
    /// </summary>
    public WorldView WorldView { get; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public float NearClip { get; set; } = 0.1f;
    public float FarClip => CFG.Current.Viewport_RenderDistance_Max;

    private DbgPrimWireGrid WireGrid;

    private DebugPrimitiveRenderableProxy ViewportGridProxy;

    public bool ViewportSelected { get; private set; }
    public float SelectionTolerance
    {
        get => _selectionTolerance;
        set => _selectionTolerance = MathF.Max(0, value);
    }

    private string imguiID = "mapEditorViewport";

    public MapViewport(MapEditor editor)
    {
        Editor = editor;

        _device = editor.BaseEditor.GraphicsContext.Device;

        Rect = Editor.BaseEditor.GraphicsContext.Window.Bounds;
        Width = Rect.Width;
        Height = Rect.Height;

        if (Editor.BaseEditor.GraphicsContext.Device != null)
        {
            float depth = _device.IsDepthRangeZeroToOne ? 1 : 0;

            _renderViewport = new Veldrid.Viewport(0, 0, Width, Height, depth, 1.0f - depth);

            WorldView = new WorldView(new Rectangle(0, 0, Width, Height));

            _viewPipeline = new SceneRenderPipeline(editor.RenderScene, _device, Width, Height);

            _projectionMat = Utils.CreatePerspective(_device, false,
                CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f, Width / (float)Height, NearClip, FarClip);

            _frustum = new BoundingFrustum(_projectionMat);

            _viewPipeline.SetViewportSetupAction((d, cl) =>
            {
                cl.SetFramebuffer(_device.SwapchainFramebuffer);
                cl.SetViewport(0, _renderViewport);
                if (_vpvisible)
                {
                    _clearQuad.Render(d, cl);
                }
                _vpvisible = false;
            });

            _viewPipeline.SetOverlayViewportSetupAction((d, cl) =>
            {
                cl.SetFramebuffer(_device.SwapchainFramebuffer);
                cl.SetViewport(0, _renderViewport);
                cl.ClearDepthStencil(0);
            });

            // Create gizmos
            _gizmos = new Gizmos(editor.EditorActionManager, editor.Selection, editor.RenderScene.OverlayRenderables);

            // Create view grid
            WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

            ViewportGridProxy = new DebugPrimitiveRenderableProxy(editor.RenderScene.OverlayRenderables, WireGrid);
            ViewportGridProxy.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);

            _clearQuad = new FullScreenQuad();

            Scene.Renderer.AddBackgroundUploadTask((gd, cl) =>
            {
                _clearQuad.CreateDeviceObjects(gd, cl);
            });
        }
    }

    public void Display()
    {
        if (Editor.BaseEditor.GraphicsContext.Device == null)
            return;

        if(!UI.Current.Interface_Editor_Viewport)
            return;

        Shortcuts();

        // Viewport
        UIHelper.ApplyChildStyle();

        if (ImGui.Begin($@"Viewport##{imguiID}", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav))
        {
            Vector2 p = ImGui.GetWindowPos();
            Vector2 s = ImGui.GetWindowSize();
            Rectangle newvp = new((int)p.X, (int)p.Y + 3, (int)s.X, (int)s.Y - 3);
            ResizeViewport(_device, newvp);
            if (InputTracker.GetMouseButtonDown(MouseButton.Right) && MouseInViewport())
            {
                ImGui.SetWindowFocus();
                ViewportSelected = true;
            }
            else if (!InputTracker.GetMouseButton(MouseButton.Right))
            {
                ViewportSelected = false;
            }
            _canInteract = ImGui.IsWindowFocused();
            if (CFG.Current.Viewport_Enable_BoxSelection && !_gizmos.IsMouseBusy())
            {
                Vector2 mousePos = InputTracker.MousePosition;
                if (InputTracker.GetMouseButtonDown(MouseButton.Left) && MouseInViewport())
                {
                    _isDragging = true;
                    _mouseDragStarted = false;
                    _dragStart = mousePos;
                    _dragEnd = mousePos;
                }
                else if (InputTracker.GetMouseButton(MouseButton.Left) && _isDragging)
                {
                    _dragEnd = mousePos;

                    // Check if the drag threshold has been exceeded
                    if (!_mouseDragStarted && Vector2.Distance(_dragStart, _dragEnd) > _dragThreshold)
                    {
                        _mouseDragStarted = true;
                    }
                }
                else if (_isDragging && !InputTracker.GetMouseButton(MouseButton.Left))
                {
                    if (_mouseDragStarted)
                    {
                        // Drag was confirmed and released — perform selection
                        SelectObjectsInDragArea(_dragStart, _dragEnd);
                    }

                    // Reset drag state
                    _isDragging = false;
                    _mouseDragStarted = false;
                }
            }

            _vpvisible = true;

            Matrix4x4 proj = Matrix4x4.Transpose(_projectionMat);
            Matrix4x4 view = Matrix4x4.Transpose(WorldView.CameraTransform.CameraViewMatrixLH);
            Matrix4x4 identity = Matrix4x4.Identity;

            ViewportInformationPanel();

            // Display profiling text in the viewport
            if (UI.Current.Interface_Editor_Profiling)
            {
                ImGui.Text($@"Cull time: {Editor.RenderScene.OctreeCullTime} ms");
                ImGui.Text($@"Work creation time: {Editor.RenderScene.CPUDrawTime} ms");
                ImGui.Text($@"Scene Render CPU time: {_viewPipeline.CPURenderTime} ms");
                ImGui.Text($@"Visible objects: {Editor.RenderScene.RenderObjectCount}");
                ImGui.Text(
                    $@"Vertex Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalVertexFootprint / 1024 / 1024} MB");
                ImGui.Text(
                    $@"Index Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalIndexFootprint / 1024 / 1024} MB");
            }
        }

        if (_isDragging)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 start = _dragStart;
            Vector2 end = _dragEnd;

            // Clamp coordinates to window area
            start = Vector2.Clamp(start, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());
            end = Vector2.Clamp(end, ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());
            drawList.AddRect(start, end, ImGui.GetColorU32(new Vector4(0f, 0.5f, 1f, 1f)), 0f, ImDrawFlags.None, 2f);
            drawList.AddRectFilled(start, end, ImGui.GetColorU32(new Vector4(0f, 0.5f, 1f, 0.15f)));
        }

        ImGui.End();
        UIHelper.UnapplyChildStyle();
    }


    public void SceneParamsGui()
    {
        ImGui.SliderFloat4("Light Direction", ref _viewPipeline.SceneParams.LightDirection, -1, 1);
        ImGui.SliderFloat("Direct Light Mult", ref _viewPipeline.SceneParams.DirectLightMult, 0, 3);
        ImGui.SliderFloat("Indirect Light Mult", ref _viewPipeline.SceneParams.IndirectLightMult, 0, 3);
        ImGui.SliderFloat("Brightness", ref _viewPipeline.SceneParams.SceneBrightness, 0, 5);
    }

    public void ResizeViewport(GraphicsDevice device, Rectangle newvp)
    {
        Width = newvp.Width;
        Height = newvp.Height;
        X = newvp.X;
        Y = newvp.Y;
        WorldView.UpdateBounds(newvp);
        float depth = device.IsDepthRangeZeroToOne ? 0 : 1;
        _renderViewport = new Veldrid.Viewport(newvp.X, newvp.Y, Width, Height, depth, 1.0f - depth);
    }

    public void Update(float deltatime)
    {
        if (Editor.BaseEditor.GraphicsContext.Device == null)
            return;

        Vector2 pos = InputTracker.MousePosition;
        Ray ray = GetRay(pos.X - X, pos.Y - Y);
        _cursorX = (int)pos.X; // - X;
        _cursorY = (int)pos.Y; // - Y;
        _gizmos.Update(ray, _canInteract && MouseInViewport());

        if (CFG.Current.MapEditor_Viewport_RegenerateMapGrid)
        {
            CFG.Current.MapEditor_Viewport_RegenerateMapGrid = false;

            RegenerateGrid();
        }

        if (UI.Current.Interface_MapEditor_Viewport_Grid)
        {
            ViewportGridProxy.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
            ViewportGridProxy.Visible = true;
            ViewportGridProxy.World = new Transform(0, CFG.Current.MapEditor_Viewport_Grid_Height, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            ViewportGridProxy.Visible = false;
        }

        _viewPipeline.SceneParams.SimpleFlver_Brightness = CFG.Current.Viewport_DefaultRender_Brightness;
        _viewPipeline.SceneParams.SimpleFlver_Saturation = CFG.Current.Viewport_DefaultRender_Saturation;
        _viewPipeline.SceneParams.SelectionColor = new Vector4(CFG.Current.Viewport_DefaultRender_SelectColor.X, CFG.Current.Viewport_DefaultRender_SelectColor.Y,
            CFG.Current.Viewport_DefaultRender_SelectColor.Z, 1.0f);
        bool kbbusy = false;
        if (!_gizmos.IsMouseBusy() && _canInteract && MouseInViewport())
        {
            kbbusy = WorldView.UpdateInput(Editor.Window, deltatime);

            if (InputTracker.GetMouseButtonDown(MouseButton.Left))
            {
                _viewPipeline.CreateAsyncPickingRequest();
            }
            if (InputTracker.GetMouseButton(MouseButton.Left) && InputTracker.GetKeyDown(Key.AltLeft))
            {
                _viewPipeline.CreateAsyncPickingRequest();
            }
            if (_viewPipeline.PickingResultsReady)
            {
                ISelectable sel = _viewPipeline.GetSelection();
                if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
                {
                    if (sel != null)
                    {
                        if (Editor.Selection.GetSelection().Contains(sel))
                        {
                            Editor.Selection.RemoveSelection(sel);
                        }
                        else
                        {
                            Editor.Selection.AddSelection(sel);
                        }
                    }
                }
                else if (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight))
                {
                    if (sel != null)
                    {
                        Editor.Selection.AddSelection(sel);
                    }
                }
                else
                {
                    Editor.Selection.ClearSelection();

                    if (sel != null)
                    {
                        Editor.Selection.AddSelection(sel);
                    }
                }
            }
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Editor.BaseEditor.GraphicsContext.Device == null)
            return;

        _projectionMat = Utils.CreatePerspective(device, true, CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f,
                Width / (float)Height, NearClip, FarClip);

        _frustum = new BoundingFrustum(WorldView.CameraTransform.CameraViewMatrixLH * _projectionMat);

        _viewPipeline.TestUpdateView(_projectionMat, WorldView.CameraTransform.CameraViewMatrixLH,
            WorldView.CameraTransform.Position, _cursorX, _cursorY);

        _viewPipeline.RenderScene(_frustum);
       
        _gizmos.CameraPosition = WorldView.CameraTransform.Position;
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Editor.Window = window;
        Rect = window.Bounds;
    }

    public void SetEnvMap(uint index)
    {
        _viewPipeline.EnvMapTexture = index;
    }

    /// <summary>
    ///     Moves the camera position such that it is directly looking at the center of a
    ///     bounding box. Camera will face the same direction as before.
    /// </summary>
    /// <param name="box">The bounding box to frame</param>
    public void FrameBox(BoundingBox box)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, WorldView.CameraTransform.RotationMatrix);
        Vector3 pos = box.GetCenter();
        float radius = Vector3.Distance(box.Max, box.Min);
        WorldView.CameraTransform.Position = pos - camdir * radius;
    }

    /// <summary>
    ///     Moves the camera position such that it is directly looking at a position.
    public void FramePosition(Vector3 pos, float dist)
    {
        Vector3 camdir = Vector3.Transform(Vector3.UnitZ, WorldView.CameraTransform.RotationMatrix);
        WorldView.CameraTransform.Position = pos - camdir * dist;
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_DisplayInformationPanel))
        {
            CFG.Current.Viewport_Enable_ViewportInfoPanel = !CFG.Current.Viewport_Enable_ViewportInfoPanel;
        }
    }

    public void ViewportInformationPanel()
    {
        if (CFG.Current.Viewport_Enable_ViewportInfoPanel)
        {
            if (CFG.Current.Viewport_ViewportInfoPanel_Display_DegreeIncrement)
            {
                Editor.RotationIncrement.DisplayViewportRotateIncrement();
            }
            if (CFG.Current.Viewport_ViewportInfoPanel_Display_MovementIncrement)
            {
                Editor.KeyboardMovement.DisplayViewportMovementIncrement();
            }
        }
    }

    // TODO: Implement far clip and the selection tolerance slider...
    private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
    {
        float minX = MathF.Min(start.X, end.X);
        float minY = MathF.Min(start.Y, end.Y);
        float maxX = MathF.Max(start.X, end.X);
        float maxY = MathF.Max(start.Y, end.Y);
        minX -= X;
        maxX -= X;
        minY -= Y;
        maxY -= Y;
        bool shift = InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight);
        bool ctrl = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);
        if (!shift && !ctrl)
        {
            Editor.Selection.ClearSelection();
        }
        for (int i = 0; i < Editor.RenderScene.OpaqueRenderables.cBounds.Length; i++)
        {
            BoundingBox obj = Editor.RenderScene.OpaqueRenderables.cBounds[i];
            if (_frustum.Contains(obj) == ContainmentType.Disjoint)
            {
                continue;
            }
            Vector3 center = obj.GetCenter();
            Vector2 screenPos = WorldToScreen(center);
            if (screenPos.X >= minX - _selectionTolerance
                && screenPos.X <= maxX + _selectionTolerance
                && screenPos.Y >= minY - _selectionTolerance
                && screenPos.Y <= maxY + _selectionTolerance)
            {
                WeakReference<ISelectable> selectable = Editor.RenderScene.OpaqueRenderables.cSelectables[i];
                if (selectable == null)
                {
                    continue;
                }
                if (selectable.TryGetTarget(out ISelectable target))
                {
                    if (ctrl)
                    {
                        Editor.Selection.RemoveSelection(target);
                    }
                    else
                    {
                        Editor.Selection.AddSelection(target);
                    }
                }
            }
        }
    }

    private Vector2 WorldToScreen(Vector3 worldPos)
    {
        Vector4 world = new(worldPos, 1.0f);
        Vector4 clip = Vector4.Transform(world, WorldView.CameraTransform.CameraViewMatrixLH * _projectionMat);
        if (clip.W <= 0.0f)
        {
            return new Vector2(-10000, -10000);
        }
        Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);
        return new Vector2(
            (ndc.X + 1f) / 2f * Width,
            (1f - ndc.Y) / 2f * Height
        );
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
        Matrix4x4.Invert(_projectionMat, out invProj);
        Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);
        viewCoords.Z = 1.0f;
        viewCoords.W = 0.0f;
        Matrix4x4 invView;
        Matrix4x4.Invert(WorldView.CameraTransform.CameraViewMatrixLH, out invView);
        Vector3 worldCoords = Vector4.Transform(viewCoords, invView).XYZ();
        worldCoords = Vector3.Normalize(worldCoords);
        //worldCoords.X = -worldCoords.X;
        return new Ray(WorldView.CameraTransform.Position, worldCoords);
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
    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void RegenerateGrid()
    {
        WireGrid.Dispose();
        ViewportGridProxy.Dispose();

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

        ViewportGridProxy = new DebugPrimitiveRenderableProxy(Editor.RenderScene.OpaqueRenderables, WireGrid);
        ViewportGridProxy.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
    }

    private bool TaskWindowOpen = true;

    public void DisplayResourceLoadWindow()
    {
        var scale = DPI.Scale;

        if (GetActiveJobProgress().Count > 0)
        {
            ImGui.SetNextWindowSize(new Vector2(400, 310) * scale);
            ImGui.SetNextWindowPos(new Vector2(Width - (100 * scale), Height - (300 * scale)));
            if (!ImGui.Begin("Resource Loading Tasks", ref TaskWindowOpen, ImGuiWindowFlags.NoDecoration))
            {
                ImGui.End();
                return;
            }

            foreach (KeyValuePair<ResourceJob, int> job in GetActiveJobProgress())
            {
                if (!job.Key.Finished)
                {
                    var completed = job.Key.Progress;
                    var size = job.Key.GetEstimateTaskSize();
                    ImGui.Text(job.Key.Name);

                    if (size == 0)
                    {
                        ImGui.ProgressBar(0.0f);
                    }
                    else
                    {
                        ImGui.ProgressBar(completed / (float)size, new Vector2(386.0f, 20.0f) * scale);
                    }
                }
            }

            ImGui.End();
        }
    }

    public string SelectedResource = "";
    public IResourceHandle SelectedResourceHandle = null;
    public string ResourceFilter = "";

    public void DisplayResourceList()
    {
        if (!UI.Current.Interface_MapEditor_ResourceList)
            return;

        if (!ImGui.Begin($"Resource List##mapResourceList"))
        {
            ImGui.End();
            return;
        }

        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();
        var tableSize = new Vector2(width * 0.98f, height * 0.98f);

        ImGui.SetNextItemWidth(width * 0.98f);
        ImGui.InputText("##resourceTableFilter", ref ResourceFilter, 255);

        // Table
        //ImGui.BeginChild("resourceTableSection", tableSize);

        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        var imguiId = 0;

        if (ImGui.BeginTable($"resourceListTable", 6, tableFlags))
        {
            ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Load State", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Access Level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Reference Count", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Unload", ImGuiTableColumnFlags.WidthStretch);

            // Header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TableSetColumnIndex(1);

            ImGui.Text("Name");
            UIHelper.Tooltip("Name of this resource.");

            ImGui.TableSetColumnIndex(2);

            ImGui.Text("Load State");
            UIHelper.Tooltip("The load state of this resource.");

            ImGui.TableSetColumnIndex(3);

            // Access Level
            ImGui.Text("Access Level");
            UIHelper.Tooltip("The access level of this resource.");

            ImGui.TableSetColumnIndex(4);

            // Reference Count
            ImGui.Text("Reference Count");
            UIHelper.Tooltip("The reference count for this resource.");

            ImGui.TableSetColumnIndex(5);

            // Unload

            // Contents
            foreach (var item in resDatabase)
            {
                var resName = item.Key;
                var resHandle = item.Value;

                if (ResourceFilter != "" && !resName.Contains(ResourceFilter))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if (ImGui.Button($"{ForkAwesome.Bars}##{imguiId}_select"))
                {
                    SelectedResource = resName;
                }
                UIHelper.Tooltip("Select this resource.");

                ImGui.TableSetColumnIndex(1);

                // Name
                ImGui.AlignTextToFramePadding();
                if (SelectedResource == resName)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{resName}");
                }
                else
                {
                    ImGui.Text(resName);
                }

                ImGui.TableSetColumnIndex(2);

                // Load State
                if (resHandle.IsLoaded())
                {
                    ImGui.Text("Loaded");
                }
                else
                {
                    ImGui.Text("Unloaded");
                }

                ImGui.TableSetColumnIndex(3);

                // Access Level
                ImGui.Text($"{resHandle.AccessLevel}");
                ImGui.TableSetColumnIndex(4);

                // Reference Count
                ImGui.Text($"{resHandle.GetReferenceCounts()}");
                ImGui.TableSetColumnIndex(5);

                // Unload
                if (ImGui.Button($"{ForkAwesome.Times}##{imguiId}_unload"))
                {
                    resHandle.Release(true);
                }
                UIHelper.Tooltip("Unload this resource.");

                imguiId++;
            }

            ImGui.EndTable();
        }

        //ImGui.EndChild();

        /*
        ImGui.BeginChild("resourceDetailsSection");

        if(SelectedResource == "")
        {
            ImGui.Text($"No resource selected.");
        }
        else if(SelectedResourceHandle != null)
        {
            var resHandle = SelectedResourceHandle;

            // FLVER
            if(resHandle is ResourceHandle<FlverResource>)
            {
                var resource = (ResourceHandle<FlverResource>)resHandle;
                var res = resource.Get();

            }

            // Havok Collision
            if (resHandle is ResourceHandle<HavokCollisionResource>)
            {
                var resource = (ResourceHandle<HavokCollisionResource>)resHandle;
                var res = resource.Get();

            }

            // Havok Navmesh
            if (resHandle is ResourceHandle<HavokNavmeshResource>)
            {
                var resource = (ResourceHandle<HavokNavmeshResource>)resHandle;
                var res = resource.Get();

            }

            // NVM Navmesh
            if (resHandle is ResourceHandle<NVMNavmeshResource>)
            {
                var resource = (ResourceHandle<NVMNavmeshResource>)resHandle;
                var res = resource.Get();

            }

            // Texture
            if (resHandle is ResourceHandle<TextureResource>)
            {
                var resource = (ResourceHandle<TextureResource>)resHandle;
                var res = resource.Get();

            }
        }

        ImGui.EndChild();
        */

        ImGui.End();
    }
}
