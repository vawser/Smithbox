#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The Cutscene Editor's viewport: the cut's actors, its camera's path and frustum, and markers for what has no model.
/// While looking through the camera, the viewport takes the camera's place and field of view every frame and ignores
/// the fly and orbit input; turning it off leaves a free camera where the cutscene camera was.
/// </summary>
public class CutsceneViewportWindow
{
    public CutsceneEditorView Editor;
    public ProjectEntry Project;

    public IViewport? Viewport;
    public Rectangle Rect;

    public bool ViewportUsingKeyboard;

    private readonly CutsceneDebugLines Lines = new();

    // What the lines were last built for, to rebuild them only when it changes
    private (CutsceneScene? Scene, int Frame, bool Path, bool Camera, bool Markers, bool LookThrough, int Forward, int Up, bool Horizontal, float Aspect, float Placement) BuiltKey;

    // Changes when a placement is moved, so the lines follow it
    private static float PlacementSignature(CutsceneScene? scene)
    {
        if (scene == null)
            return 0;

        var sum = 0f;
        foreach (var p in scene.Placements.Values)
        {
            foreach (var k in p.Keys)
                sum += k.Translation.X + k.Translation.Y * 3 + k.Translation.Z * 7 + k.Rotation.X + k.Rotation.Y * 3 + k.Rotation.Z * 7;
        }

        return sum;
    }
    private DateTime LastBuild;
    private static readonly TimeSpan BuildInterval = TimeSpan.FromMilliseconds(50);

    private bool WasLookingThrough;

    public CutsceneViewportWindow(CutsceneEditorView editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        // Without a renderer the viewport is still shown, as a disabled placeholder
        if (Smithbox.Instance.HasRenderer)
            Viewport = new VulkanViewport(editor.Universe, $"CutsceneViewport_{editor.ViewIndex}", Rect.Width, Rect.Height, editor.RenderScene);
        else
            Viewport = new NullViewport(editor.Universe, $"CutsceneViewport_{editor.ViewIndex}", Rect.Width, Rect.Height, editor.RenderScene);
    }

    /// <summary>Where the viewport camera is and which way it looks (the cut's camera while looking through it)</summary>
    public (Vector3 Eye, Vector3 Forward) CameraEyeAndForward()
    {
        if (Viewport is not VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);

        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_CutsceneEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
            vulkanViewport.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    /// <summary>
    /// Where the free viewport camera is and looks (it looks along its rotation's +Z, with +Y up), for setting a camera
    /// key from the view. <paramref name="fovRadians" /> is the viewport's field of view as a cutscene camera's file
    /// would store it (see <see cref="CutsceneCamera.FileFovRadians" />). False while looking through the cutscene
    /// camera, which then is the view.
    /// </summary>
    public bool TryGetFreeView(out Vector3 eye, out Vector3 forward, out Vector3 up, out float fovRadians)
    {
        eye = forward = up = default;
        fovRadians = 0;
        if (Viewport is not VulkanViewport vk || CFG.Current.CutsceneEditor_Look_Through_Camera)
            return false;

        var transform = vk.ViewportCamera.CameraTransform;
        eye = transform.Position;
        forward = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, transform.Rotation));
        up = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, transform.Rotation));

        var vertical = (vk.ViewportCamera.FovOverrideDegrees ?? CFG.Current.Viewport_Camera_FOV) * MathF.PI / 180f;
        fovRadians = CutsceneCamera.FileFovRadians(vertical, Aspect);
        return true;
    }

    /// <summary>The same cut with changed data (a camera written back): its lines are built again.</summary>
    public void InvalidateLines() => BuiltKey = default;

    /// <summary>A new cut: its lines are built again, and the free camera starts where its camera does.</summary>
    public void OnSceneChanged()
    {
        BuiltKey = default;

        if (Viewport is VulkanViewport vk && Editor.Scene is { } scene && scene.ActiveCamera(0) is { Skeleton: not null } camera)
        {
            CutsceneCamera.GetView(camera.World(0, scene.RootOf(camera.Resource, 0)), out var eye, out var forward, out var up);
            vk.ViewportCamera.CameraTransform.Position = eye;
            vk.ViewportCamera.CameraTransform.Rotation = CutsceneCamera.ViewportRotation(forward, up);
        }
    }

    public void Update(float deltatime)
    {
        if (Viewport != null)
            ViewportUsingKeyboard = Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        UpdateCamera();
        UpdateLines();
        Lines.Update();
    }

    private float Aspect => Viewport is VulkanViewport vk && vk.Height > 0 ? vk.Width / (float)vk.Height : 16f / 9f;

    private void UpdateCamera()
    {
        if (Viewport is not VulkanViewport vk)
            return;

        var scene = Editor.Scene;
        var frame = Editor.Playback.Frame;
        var camera = scene?.ActiveCamera(frame);

        var lookThrough = CFG.Current.CutsceneEditor_Look_Through_Camera && scene != null && camera?.Skeleton != null;

        if (lookThrough)
        {
            CutsceneCamera.GetView(camera!.World(frame, scene!.RootOf(camera.Resource, frame)), out var eye, out var forward, out var up);

            vk.ViewportCamera.CameraTransform.Position = eye;
            vk.ViewportCamera.CameraTransform.Rotation = CutsceneCamera.ViewportRotation(forward, up);
            vk.ViewportCamera.FovOverrideDegrees = camera.VerticalFovRadians(frame, Aspect) * 180f / MathF.PI;
            vk.CameraInputLocked = true;
            vk.ViewportCamera.UpdateProjectionMatrix();
        }
        else if (WasLookingThrough)
        {
            // Back to a free camera, from where the cutscene camera was
            vk.ViewportCamera.FovOverrideDegrees = null;
            vk.CameraInputLocked = false;
            vk.ViewportCamera.UpdateProjectionMatrix();
        }

        WasLookingThrough = lookThrough;
    }

    private void UpdateLines()
    {
        if (Editor.RenderScene == null || Lines.IsUpdating)
            return;

        var scene = Editor.Scene;
        var frame = (int)MathF.Round(Editor.Playback.Frame);
        var key = (scene, frame, CFG.Current.CutsceneEditor_Display_Camera_Path, CFG.Current.CutsceneEditor_Display_Camera,
            CFG.Current.CutsceneEditor_Display_Markers, CFG.Current.CutsceneEditor_Look_Through_Camera,
            CFG.Current.CutsceneEditor_Camera_Forward_Axis, CFG.Current.CutsceneEditor_Camera_Up_Axis,
            CFG.Current.CutsceneEditor_Camera_Fov_Is_Horizontal, MathF.Round(Aspect, 2), PlacementSignature(scene));

        if (key == BuiltKey)
            return;

        // A new cut shows at once; a moving frame at most every BuildInterval
        if (scene == BuiltKey.Scene && DateTime.UtcNow - LastBuild < BuildInterval)
            return;

        BuiltKey = key;
        LastBuild = DateTime.UtcNow;

        if (scene == null)
        {
            Lines.Clear();
            return;
        }

        var lines = new Dictionary<CutsceneLineLayer, List<(Vector3, Vector3)>>
        {
            [CutsceneLineLayer.CameraPath] = new(),
            [CutsceneLineLayer.Camera] = new(),
            [CutsceneLineLayer.Markers] = new(),
            [CutsceneLineLayer.Root] = new()
        };

        // The animations' roots (each HKT's placement): markers like the actors' below
        if (CFG.Current.CutsceneEditor_Display_Markers)
        {
            foreach (var placement in scene.Placements.Values)
                CutsceneDebugLines.AddAxes(lines[CutsceneLineLayer.Root], placement.At(frame), 1.0f);
        }

        if (CFG.Current.CutsceneEditor_Display_Camera_Path)
        {
            foreach (var camera in scene.Cameras)
            {
                if (camera.Skeleton == null)
                    continue;

                Vector3? last = null;
                for (var f = 0; f < scene.FrameCount; f++)
                {
                    if (!camera.IsActiveAt(f))
                    {
                        last = null;
                        continue;
                    }

                    var p = camera.World(f, scene.RootOf(camera.Resource, f)).Translation;
                    if (last is { } l)
                        lines[CutsceneLineLayer.CameraPath].Add((l, p));
                    last = p;
                }

                // Its keys, as axes where the camera is at each
                if (camera.PlaysKeys)
                {
                    foreach (var cameraKey in camera.Keys!.Keys)
                        CutsceneDebugLines.AddAxes(lines[CutsceneLineLayer.CameraPath],
                            camera.World(cameraKey.Frame, scene.RootOf(camera.Resource, cameraKey.Frame)), 0.4f);
                }
            }
        }

        // The camera at this frame; not while looking through it, as it would fill the view
        var active = scene.ActiveCamera(frame);
        if (CFG.Current.CutsceneEditor_Display_Camera && active?.Skeleton != null && !CFG.Current.CutsceneEditor_Look_Through_Camera)
        {
            CutsceneCamera.GetView(active.World(frame, scene.RootOf(active.Resource, frame)), out var eye, out var forward, out var up);
            CutsceneDebugLines.AddFrustum(lines[CutsceneLineLayer.Camera], eye, forward, up,
                active.VerticalFovRadians(frame, Aspect), Aspect, 1.0f);
        }

        if (CFG.Current.CutsceneEditor_Display_Markers)
        {
            foreach (var actor in scene.Actors)
            {
                if (!actor.ShowsMarker || !actor.IsVisibleAt(frame))
                    continue;

                CutsceneDebugLines.AddAxes(lines[CutsceneLineLayer.Markers],
                    actor.RootWorld(frame, scene.RootOf(actor.Resource, frame)), 0.5f);
            }
        }

        Lines.Show(Editor.RenderScene, lines);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport?.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport != null && Viewport.IsViewportSelected;
    }

    public void Dispose()
    {
        Lines.Clear();
        (Viewport as VulkanViewport)?.Dispose();
    }
}
