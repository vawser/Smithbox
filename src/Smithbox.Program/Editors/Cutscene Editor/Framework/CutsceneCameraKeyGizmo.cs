#nullable enable
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The viewport gizmo on the selected camera key: it sits where the camera is at the key's frame, turned like its
/// view (+Z forward, +Y up), and moves or turns that pose. The pose is taken back into the cut's space and made a key
/// with <see cref="CutsceneCameraKeys.KeyFor" />, so it works for Offset and Keyed keys alike. Translation changes
/// only the key's position and rotation only its angles; FOV and interpolation stay. A drag shows live and is one
/// undoable <see cref="CutsceneCameraKeysAction" /> when it ends. Not shown while looking through the camera, whose
/// eye would be the gizmo's.
/// </summary>
public class CutsceneCameraKeyGizmoTarget : IGizmoTarget
{
    private readonly CutsceneEditorScreen Editor;

    // The camera's keys as they were when the drag started, and which key
    private CutsceneCameraKeys? DragStart;
    private CutsceneCamera? DragCamera;
    private int DragIndex = -1;

    public CutsceneCameraKeyGizmoTarget(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    private (CutsceneScene Scene, CutsceneCamera Camera, CutsceneCameraKey Key)? Target
    {
        get
        {
            if (CFG.Current.CutsceneEditor_Look_Through_Camera || Editor.Scene is not { } scene ||
                Editor.SelectedCamera is not { PlaysKeys: true } camera || string.IsNullOrEmpty(camera.KeyId))
                return null;

            var index = Editor.SelectedCameraKey;
            var keys = camera.Keys!.Keys;
            return index >= 0 && index < keys.Count ? (scene, camera, keys[index]) : null;
        }
    }

    public bool HasTarget => Target != null && Editor.SelectedFile != null;

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        if (Target is not { } target)
            return Transform.Default;

        var (scene, camera, key) = target;
        CutsceneCamera.GetView(camera.World(key.Frame, scene.RootOf(camera.Resource, key.Frame)), out var eye, out var forward, out var up);
        return new Transform(eye, CutsceneCamera.ViewportRotation(forward, up), Vector3.One);
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (Target is not { } target)
            return;

        var (scene, camera, key) = target;
        if (DragStart == null || DragCamera != camera || DragIndex != Editor.SelectedCameraKey)
        {
            DragStart = camera.Keys!.Clone();
            DragCamera = camera;
            DragIndex = Editor.SelectedCameraKey;
        }

        Apply(scene, camera, key, DragStart.Keys[DragIndex], current, mode);
        Editor.ViewportWindow.InvalidateLines();
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        var start = DragStart;
        var index = DragIndex;
        var dragCamera = DragCamera;
        DragStart = null;
        DragCamera = null;
        DragIndex = -1;

        if (Target is not { } target || Editor.SelectedFile is not { } file)
            return;

        var (scene, camera, key) = target;

        // A click with no drag never previewed
        if (start == null || dragCamera != camera || index != Editor.SelectedCameraKey)
            start = camera.Keys!.Clone();

        Apply(scene, camera, key, start.Keys[Editor.SelectedCameraKey], current, mode);

        var after = camera.Keys!.Clone();
        if (start.SameAs(after))
            return;

        // The live keys go back as they were; the action puts the new ones in, so undo has both
        CopyValues(start.Keys[Editor.SelectedCameraKey], key);
        Editor.Execute(new CutsceneCameraKeysAction(file, camera.KeyId, start, after));
    }

    /// <summary>Sets the key (from its values at the drag's start) so the camera has the gizmo's pose at its frame</summary>
    private static void Apply(CutsceneScene scene, CutsceneCamera camera, CutsceneCameraKey key,
        CutsceneCameraKey startKey, Transform current, GizmoState.GizmosMode mode)
    {
        // The pose with the key as it was, which the drag changes one part of
        CopyValues(startKey, key);
        var frame = key.Frame;
        var (bone, fov) = camera.Pose(frame);

        Matrix4x4.Invert(scene.RootOf(camera.Resource, frame), out var toCut);

        switch (mode)
        {
            case GizmoState.GizmosMode.Translate:
                bone.Translation = Vector3.Transform(current.Position, toCut);
                break;

            case GizmoState.GizmosMode.Rotate:
            {
                var forward = Vector3.Transform(Vector3.UnitZ, current.Rotation);
                var up = Vector3.Transform(Vector3.UnitY, current.Rotation);
                bone.Rotation = CutsceneCameraRotation.FromView(Vector3.Normalize(Vector3.TransformNormal(forward, toCut)),
                    Vector3.Normalize(Vector3.TransformNormal(up, toCut)));
                break;
            }

            default:
                return;
        }

        var made = camera.Keys!.KeyFor(frame, bone, fov, camera.BaseBone(frame), camera.BaseFov(frame), startKey);
        if (mode == GizmoState.GizmosMode.Translate)
            key.Position = made.Position;
        else
            key.Rotation = made.Rotation;
    }

    private static void CopyValues(CutsceneCameraKey from, CutsceneCameraKey to)
    {
        to.Position = from.Position;
        to.Rotation = from.Rotation;
        to.FovDegrees = from.FovDegrees;
        to.Interpolation = from.Interpolation;
    }
}
