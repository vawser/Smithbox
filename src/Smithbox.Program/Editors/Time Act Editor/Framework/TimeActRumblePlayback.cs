#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Shakes the viewport camera with the rumble cams of the events the playback passes. A rumble cam is a short camera
/// animation in the rumble binder (other/rumble/camera_NNN): a Havok clip in Sekiro, Elden Ring, Nightreign and
/// Armored Core VI, and a SIBCAM in Demon's Souls, Dark Souls and Dark Souls III. How far the camera moves from where
/// the animation starts is added to the view while it plays. One with a dummy poly fades out with the camera's
/// distance from it.
/// </summary>
public class TimeActRumblePlayback
{
    // The binders the games keep their rumble cams in, the first one there being used
    private static readonly string[] BinderPaths =
    {
        "/other/default.rumblebnd.dcx",
        "/other/rumblehk.rumblehkbnd.dcx",
        "/other/default.rumblebnd"
    };

    // camera_000.hkx, camera_000_win32.sibcam, camera_000_daml_1.sibcam: the number is the rumble cam ID
    private static readonly Regex CameraName = new(@"^camera_(\d+)", RegexOptions.IgnoreCase);

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;

    private readonly Dictionary<int, RumbleCam?> _cams = new();
    private Dictionary<int, BinderFileHeader>? _entries;
    private byte[]? _binderData;
    private bool _binderRead;

    private readonly List<Rumble> _active = new();

    private TAE.Animation? _animation;
    private float _lastTime;

    /// <summary>
    /// A rumble cam: how long it lasts, and where the camera is at a time in it
    /// </summary>
    private sealed record RumbleCam(float Duration, Func<float, Matrix4x4> Sample);

    private sealed class Rumble(RumbleCam cam, Vector3? source, float falloffStart, float falloffEnd)
    {
        public RumbleCam Cam { get; } = cam;
        public Vector3? Source { get; } = source;
        public float FalloffStart { get; } = falloffStart;
        public float FalloffEnd { get; } = falloffEnd;
        public float Elapsed;
    }

    public TimeActRumblePlayback(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;
    }

    public int ActiveCount => _active.Count;

    /// <param name="time">The playback clock, in seconds</param>
    /// <param name="playing">Whether the clock is running, rather than being scrubbed or stepped</param>
    public void Update(ModelAnimator? animator, float time, bool playing)
    {
        var animation = _view.Selection.SelectedAnimation;

        if (animation != _animation)
        {
            _animation = animation;
            _lastTime = time;
        }

        if (CFG.Current.TimeActEditor_Sim_Rumble_Cam && playing && animation != null)
        {
            // A loop back to the start passes the events at the end, then those at the start
            if (time < _lastTime)
            {
                Trigger(animation, _lastTime, float.MaxValue, animator);
                Trigger(animation, -1, time, animator);
            }
            else if (time > _lastTime)
            {
                Trigger(animation, _lastTime, time, animator);
            }
        }

        _lastTime = time;

        if (!CFG.Current.TimeActEditor_Sim_Rumble_Cam)
        {
            _active.Clear();
        }

        ApplyShake(Hexa.NET.ImGui.ImGui.GetIO().DeltaTime);
    }

    public void StopAll()
    {
        _active.Clear();
        ApplyShake(0);
    }

    private void Trigger(TAE.Animation animation, float from, float to, ModelAnimator? animator)
    {
        var data = _project.Handler.TimeActData;
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        foreach (var ev in animation.Events)
        {
            if (ev.StartTime <= from || ev.StartTime > to ||
                data?.GetSimulationRole(ev) != TimeActSimulationRole.RumbleCam ||
                !_view.Playback.Simulation.IsEventEnabled(ev))
                continue;

            if (TimeActSimulation.GetFloat(ev, "RumbleCamID") is not { } id || id < 0 || GetCam((int)id) is not { } cam)
                continue;

            Vector3? source = null;
            var falloffStart = TimeActSimulation.GetFloat(ev, "FalloffStart") ?? -1;
            var falloffEnd = TimeActSimulation.GetFloat(ev, "FalloffEnd") ?? -1;

            // A local rumble comes from a dummy poly of the character, or its body if it hasn't that one
            if (TimeActSimulation.GetFloat(ev, "DummyPolyID") is { } dummy && container != null)
            {
                source = _view.Playback.EventDisplay.GetDummyPosition((int)dummy, container, animator)
                         ?? _view.Playback.EventDisplay.GetDummyPosition(-1, container, animator);
            }

            _active.Add(new Rumble(cam, source, falloffStart, falloffEnd));
        }
    }

    private void ApplyShake(float deltaTime)
    {
        if (_view.ViewportWindow?.Viewport is not StudioCore.Editors.Viewport.VulkanViewport vk)
            return;

        var camera = vk.ViewportCamera;
        var shake = Matrix4x4.Identity;

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            Rumble rumble = _active[i];
            rumble.Elapsed += deltaTime;

            if (rumble.Elapsed >= rumble.Cam.Duration)
            {
                _active.RemoveAt(i);
                continue;
            }

            var weight = GetWeight(rumble, camera.CameraTransform.Position);

            if (weight > 0)
            {
                shake *= Scale(GetMove(rumble), weight);
            }
        }

        camera.Shake = shake;
    }

    // How far the camera has moved from where the rumble cam starts
    private static Matrix4x4 GetMove(Rumble rumble)
    {
        Matrix4x4 now = rumble.Cam.Sample(rumble.Elapsed);
        Matrix4x4 start = rumble.Cam.Sample(0);

        return Matrix4x4.Invert(start, out Matrix4x4 inverseStart) ? now * inverseStart : Matrix4x4.Identity;
    }

    private static float GetWeight(Rumble rumble, Vector3 cameraPosition)
    {
        if (rumble.Source is not { } source || rumble.FalloffEnd <= 0 || rumble.FalloffEnd <= rumble.FalloffStart)
            return 1;

        var distance = Vector3.Distance(source, cameraPosition);

        if (distance <= rumble.FalloffStart)
            return 1;

        return Math.Clamp(1 - (distance - rumble.FalloffStart) / (rumble.FalloffEnd - rumble.FalloffStart), 0, 1);
    }

    private static Matrix4x4 Scale(Matrix4x4 move, float weight)
    {
        if (weight >= 1 || !Matrix4x4.Decompose(move, out _, out Quaternion rotation, out Vector3 translation))
            return move;

        return Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(Quaternion.Identity, rotation, weight)) *
               Matrix4x4.CreateTranslation(translation * weight);
    }

    /// <summary>
    /// A rumble cam by its ID, found by its file name as the binder IDs are offset in some games
    /// </summary>
    private RumbleCam? GetCam(int id)
    {
        if (_cams.TryGetValue(id, out var cam))
            return cam;

        cam = null;

        try
        {
            if (GetEntries() is { } entries && entries.TryGetValue(id, out var entry) && _binderData != null)
            {
                using var binder = OpenBinder(_binderData);
                var bytes = binder.ReadFile(entry);

                cam = entry.Name.EndsWith(".sibcam", StringComparison.OrdinalIgnoreCase) ? ReadSibcam(bytes) : ReadHavok(bytes);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Rumble_Failed_To_Read", id), e);
        }

        _cams[id] = cam;
        return cam;
    }

    private static RumbleCam? ReadSibcam(Memory<byte> bytes)
    {
        return SibcamData.Read(bytes) is { Duration: > 0 } sibcam ? new RumbleCam(sibcam.Duration, sibcam.Sample) : null;
    }

    private RumbleCam? ReadHavok(Memory<byte> bytes)
    {
        // One bone: the camera the clip moves
        var skeleton = new HavokSkeletonData
        {
            Name = "Camera01",
            Names = new[] { "Camera01" },
            Parents = new short[] { -1 },
            ReferencePose = new[]
            {
                new HavokTransformData { Translation = Vector3.Zero, Rotation = Quaternion.Identity, Scale = Vector3.One }
            }
        };

        if (HavokAnimationReader.ReadClip(_project.Descriptor.ProjectType, bytes.ToArray(), null, skeleton) is not
            { Duration: > 0 } clip)
        {
            return null;
        }

        var track = clip.BoneToTrack.Length > 0 && clip.BoneToTrack[0] >= 0 ? clip.BoneToTrack[0] : 0;
        var lastFrame = Math.Max(clip.FrameCount - 1, 0);

        return new RumbleCam(clip.Duration, time =>
        {
            var frame = clip.FrameDuration > 0 ? Math.Min(time / clip.FrameDuration, lastFrame) : 0;
            return clip.Sample(track, frame).GetMatrix();
        });
    }

    private Dictionary<int, BinderFileHeader>? GetEntries()
    {
        if (_binderRead)
            return _entries;

        _binderRead = true;

        foreach (var path in BinderPaths)
        {
            if (!_project.VFS.FS.FileExists(path) || _project.VFS.FS.ReadFile(path) is not { } data)
                continue;

            var bytes = data.ToArray();
            if (DCX.Is(bytes))
                bytes = DCX.Decompress(bytes).ToArray();

            _binderData = bytes;

            using var binder = OpenBinder(bytes);

            _entries = new Dictionary<int, BinderFileHeader>();

            foreach (var file in binder.Files)
            {
                var name = Path.GetFileName(file.Name.Replace('\\', '/'));

                if (!name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase) &&
                    !name.EndsWith(".sibcam", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (CameraName.Match(name) is { Success: true } match && int.TryParse(match.Groups[1].Value, out var id))
                {
                    _entries.TryAdd(id, file);
                }
            }

            return _entries;
        }

        return null;
    }

    private static BinderReader OpenBinder(byte[] bytes)
    {
        return bytes.AsSpan(0, 4).SequenceEqual("BND4"u8) ? new BND4Reader(bytes) : new BND3Reader(bytes);
    }
}
