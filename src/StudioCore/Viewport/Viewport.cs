using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Interface
{
    public enum ViewportType
    {
        MapEditor,
        ModelEditor,
        TimeActEditor
    }

    /// <summary>
    ///     A viewport is a virtual (i.e. render to texture/render target) view of a scene. It can receive input events to
    ///     transform
    ///     the view within a virtual canvas, or it can be manually configured for say rendering thumbnails
    /// </summary>
    public class Viewport : IViewport
    {
        public Smithbox BaseEditor;
        public MapEditorScreen MapEditor;
        public ModelEditorScreen ModelEditor;

        private readonly ViewportActionManager _actionManager;

        private readonly FullScreenQuad _clearQuad;

        private readonly GraphicsDevice _device;
        private readonly float _dragThreshold = 5f;

        //private DebugPrimitives.DbgPrimGizmoTranslate TranslateGizmo = null;
        private readonly Gizmos _gizmos;

        private readonly MapViewportGrid _mapEditor_Viewport_Grid;
        private readonly ModelViewGrid _modelEditor_Viewport_Grid;

        private readonly DbgPrimWire _rayDebug = null;

        private readonly RenderScene _renderScene;
        private readonly ViewportSelection _selection;
        private readonly SceneRenderPipeline _viewPipeline;

        private readonly string _vpid = "";

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

        //public RenderTarget2D SceneRenderTarget = null;

        private bool _vpvisible;
        private bool DebugRayCastDraw = false;

        public int X;
        public int Y;

        public ViewportType ViewportType;

        public Viewport(Smithbox baseEditor, MapEditorScreen mapEditor, ModelEditorScreen modelEditor, ViewportType viewportType, string id, int width, int height)
        {
            BaseEditor = baseEditor;
            MapEditor = mapEditor;
            ModelEditor = modelEditor;
            ViewportType = viewportType;

            _vpid = id;
            Width = width;
            Height = height;
            _device = BaseEditor._context.Device;

            float depth = _device.IsDepthRangeZeroToOne ? 1 : 0;

            _renderViewport = new Veldrid.Viewport(0, 0, Width, Height, depth, 1.0f - depth);
            WorldView = new WorldView(new Rectangle(0, 0, Width, Height));

            if (viewportType is ViewportType.MapEditor)
            {
                _renderScene = mapEditor.MapViewportView.RenderScene;
                _selection = mapEditor.Selection;
                _actionManager = mapEditor.EditorActionManager;
            }

            if (viewportType is ViewportType.ModelEditor)
            {
                _renderScene = modelEditor.RenderScene;
                _selection = modelEditor._selection;
                _actionManager = modelEditor.EditorActionManager;
            }

            if (_renderScene != null && _device != null)
            {
                _viewPipeline = new SceneRenderPipeline(_renderScene, _device, width, height);

                _projectionMat = Utils.CreatePerspective(_device, false,
                    CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f, width / (float)height, NearClip, FarClip);

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
                if (ViewportType is ViewportType.MapEditor)
                {
                    _gizmos = new Gizmos(MapEditor, _actionManager, _selection, _renderScene.OverlayRenderables);
                    _mapEditor_Viewport_Grid = new MapViewportGrid(MapEditor, _renderScene.OpaqueRenderables);
                }

                if (ViewportType is ViewportType.ModelEditor)
                {
                    _gizmos = new Gizmos(ModelEditor, _actionManager, _selection, _renderScene.OverlayRenderables);
                    _modelEditor_Viewport_Grid = new ModelViewGrid(ModelEditor, _renderScene.OpaqueRenderables);
                }

                _clearQuad = new FullScreenQuad();

                Scene.Renderer.AddBackgroundUploadTask((gd, cl) =>
                {
                    _clearQuad.CreateDeviceObjects(gd, cl);
                });
            }
        }

        public float SelectionTolerance
        {
            get => _selectionTolerance;
            set => _selectionTolerance = MathF.Max(0, value);
        }

        public bool DrawGrid { get; set; } = true;

        /// <summary>
        ///     The camera in this scene
        /// </summary>
        public WorldView WorldView { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public float NearClip { get; set; } = 0.1f;
        public float FarClip => CFG.Current.Viewport_RenderDistance_Max;

        public bool ViewportSelected { get; private set; }

        public void OnGui()
        {
            Shortcuts();
            if (UI.Current.Interface_Editor_Viewport)
            {
                if (ImGui.Begin($@"Viewport##{_vpid}", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav))
                {
                    if(ViewportType is ViewportType.MapEditor)
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

                    //ImGui.DrawGrid(ref view.M11, ref proj.M11, ref identity.M11, 100.0f);
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
            }
            if (UI.Current.Interface_Editor_Profiling)
            {
                if (ImGui.Begin($@"Profiling##{_vpid}"))
                {
                    ImGui.Text($@"Cull time: {_renderScene.OctreeCullTime} ms");
                    ImGui.Text($@"Work creation time: {_renderScene.CPUDrawTime} ms");
                    ImGui.Text($@"Scene Render CPU time: {_viewPipeline.CPURenderTime} ms");
                    ImGui.Text($@"Visible objects: {_renderScene.RenderObjectCount}");
                    ImGui.Text(
                        $@"Vertex Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalVertexFootprint / 1024 / 1024} MB");
                    ImGui.Text(
                        $@"Index Buffers Size: {Scene.Renderer.GeometryBufferAllocator.TotalIndexFootprint / 1024 / 1024} MB");
                    //ImGui.Text($@"Selected renderable:  { _viewPipeline._pickingEntity }");
                }
                ImGui.End();
            }
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

        public bool Update(Sdl2Window window, float dt)
        {
            Vector2 pos = InputTracker.MousePosition;
            Ray ray = GetRay(pos.X - X, pos.Y - Y);
            _cursorX = (int)pos.X; // - X;
            _cursorY = (int)pos.Y; // - Y;
            _gizmos.Update(ray, _canInteract && MouseInViewport());
            _mapEditor_Viewport_Grid.Update(ray);
            _modelEditor_Viewport_Grid.Update(ray);
            _viewPipeline.SceneParams.SimpleFlver_Brightness = CFG.Current.Viewport_DefaultRender_Brightness;
            _viewPipeline.SceneParams.SimpleFlver_Saturation = CFG.Current.Viewport_DefaultRender_Saturation;
            _viewPipeline.SceneParams.SelectionColor = new Vector4(CFG.Current.Viewport_DefaultRender_SelectColor.X, CFG.Current.Viewport_DefaultRender_SelectColor.Y,
                CFG.Current.Viewport_DefaultRender_SelectColor.Z, 1.0f);
            bool kbbusy = false;

            if (!_gizmos.IsMouseBusy() && _canInteract && MouseInViewport())
            {
                kbbusy = WorldView.UpdateInput(window, dt);
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
                        ISelectable sel = _viewPipeline.GetSelection();
                        if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
                        {
                            if (sel != null)
                            {
                                if (_selection.GetSelection().Contains(sel))
                                {
                                    _selection.RemoveSelection(targetEditor, sel);
                                }
                                else
                                {
                                    _selection.AddSelection(targetEditor, sel);
                                }
                            }
                        }
                        else if (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight))
                        {
                            if (sel != null)
                            {
                                _selection.AddSelection(targetEditor, sel);
                            }
                        }
                        else
                        {
                            _selection.ClearSelection(targetEditor);
                            if (sel != null)
                            {
                                _selection.AddSelection(targetEditor, sel);
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
            _projectionMat = Utils.CreatePerspective(device, true, CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f,
                Width / (float)Height, NearClip, FarClip);
            _frustum = new BoundingFrustum(WorldView.CameraTransform.CameraViewMatrixLH * _projectionMat);
            _viewPipeline.TestUpdateView(_projectionMat, WorldView.CameraTransform.CameraViewMatrixLH,
                WorldView.CameraTransform.Position, _cursorX, _cursorY);
            _viewPipeline.RenderScene(_frustum);
            if (_rayDebug != null)
            {
                //TODO:_debugRenderer.Add(_rayDebug, new Scene.RenderKey(0));
            }
            if (DrawGrid)
            {
                //DebugRenderer.Add(ViewportGrid, new Scene.RenderKey(0));
                //ViewportGrid.UpdatePerFrameResources(device, cl, ViewPipeline);
                //ViewportGrid.Render(device, cl, ViewPipeline);
            }
            _gizmos.CameraPosition = WorldView.CameraTransform.Position;
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
            // Only display in Map Editor
            if (ViewportType is ViewportType.MapEditor)
            {
                if (CFG.Current.Viewport_Enable_ViewportInfoPanel)
                {
                    if (CFG.Current.Viewport_ViewportInfoPanel_Display_DegreeIncrement)
                    {
                        MapEditor.RotationIncrement.DisplayViewportRotateIncrement();
                    }
                    if (CFG.Current.Viewport_ViewportInfoPanel_Display_MovementIncrement)
                    {
                        MapEditor.KeyboardMovement.DisplayViewportMovementIncrement();
                    }
                }
            }
        }

        // TODO: Implement far clip and the selection tolerance slider...
        private void SelectObjectsInDragArea(Vector2 start, Vector2 end)
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
                if (targetEditor != null)
                {
                    _selection.ClearSelection(targetEditor);
                }
            }
            for (int i = 0; i < _renderScene.OpaqueRenderables.cBounds.Length; i++)
            {
                BoundingBox obj = _renderScene.OpaqueRenderables.cBounds[i];
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
                    WeakReference<ISelectable> selectable = _renderScene.OpaqueRenderables.cSelectables[i];
                    if (selectable == null)
                    {
                        continue;
                    }
                    if (selectable.TryGetTarget(out ISelectable target))
                    {
                        if (targetEditor != null)
                        {
                            if (ctrl)
                            {
                                _selection.RemoveSelection(targetEditor, target);
                            }
                            else
                            {
                                _selection.AddSelection(targetEditor, target);
                            }
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
    }
}