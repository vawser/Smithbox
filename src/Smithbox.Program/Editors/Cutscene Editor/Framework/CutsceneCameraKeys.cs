#nullable enable
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>How a camera's keys make its motion.</summary>
public enum CutsceneCameraMode
{
    // The motion of the game's own camera: no keys apply
    Vanilla,

    // The keys are corrections added to the game's own camera
    Offset,

    // The keys are the camera: a curve through them
    Keyed
}

/// <summary>How a key leads into the next one.</summary>
public enum CutsceneKeyInterpolation
{
    // A curve through the neighbouring keys (cubic Hermite, Catmull-Rom tangents in frame time)
    Smooth,
    Linear,

    // Stays at this key until the next: a hard cut
    Hold
}

/// <summary>
/// One key of a camera. In <see cref="CutsceneCameraMode.Offset" /> its values are deltas: position in the cut's own
/// space, rotation about the camera's own axes, FOV in degrees added. In <see cref="CutsceneCameraMode.Keyed" />
/// they are the camera: position in the cut's space, yaw and pitch of its view against the cut's up (+Y), roll about
/// the view, FOV in degrees.
/// </summary>
public sealed class CutsceneCameraKey
{
    public int Frame;
    public Vector3 Position;

    // Degrees: X pitch (up is positive), Y yaw, Z roll
    public Vector3 Rotation;

    public float FovDegrees;
    public CutsceneKeyInterpolation Interpolation = CutsceneKeyInterpolation.Smooth;

    public CutsceneCameraKey Clone() => (CutsceneCameraKey)MemberwiseClone();

    public bool SameAs(CutsceneCameraKey other) =>
        Frame == other.Frame && Position == other.Position && Rotation == other.Rotation &&
        FovDegrees == other.FovDegrees && Interpolation == other.Interpolation;
}

/// <summary>
/// A camera's keys and mode: what camera authoring edits. The viewport plays the camera from them live; saving bakes
/// them into the cut's Havok file (<see cref="CutsceneCameraEdit" />) and keeps them in the cutscene's sidecar
/// (<see cref="CutsceneCameraKeyStore" />), so they stay editable.
/// </summary>
public sealed class CutsceneCameraKeys
{
    public CutsceneCameraMode Mode = CutsceneCameraMode.Vanilla;
    public HavokClipFormat Format = HavokClipFormat.Auto;

    /// <summary>Kept in frame order by <see cref="Sort" /></summary>
    public List<CutsceneCameraKey> Keys = new();

    public CutsceneCameraKeys Clone() => new()
    {
        Mode = Mode,
        Format = Format,
        Keys = Keys.Select(k => k.Clone()).ToList()
    };

    public bool SameAs(CutsceneCameraKeys other) =>
        Mode == other.Mode && Format == other.Format && Keys.Count == other.Keys.Count &&
        Keys.Zip(other.Keys).All(p => p.First.SameAs(p.Second));

    /// <summary>Puts the keys in frame order, a later key at the same frame replacing the earlier</summary>
    public void Sort()
    {
        Keys = Keys.GroupBy(k => k.Frame).Select(g => g.Last()).OrderBy(k => k.Frame).ToList();
    }

    public bool Applies => Mode != CutsceneCameraMode.Vanilla;

    /// <summary>
    /// The camera bone's local transform and FOV (radians) at a frame, from the base camera (the game's own) and the
    /// keys.
    /// </summary>
    public (HavokTransformData Bone, float Fov) Evaluate(float frame, HavokTransformData baseBone, float baseFov)
    {
        if (Mode == CutsceneCameraMode.Vanilla || (Mode == CutsceneCameraMode.Keyed && Keys.Count == 0))
            return (baseBone, baseFov);

        var (position, rotation, fov) = Interpolate(frame);

        if (Mode == CutsceneCameraMode.Offset)
        {
            var delta = CutsceneCameraRotation.FromLocalAngles(rotation);
            var bone = baseBone;
            bone.Translation = baseBone.Translation + position;
            bone.Rotation = Quaternion.Normalize(Quaternion.Normalize(baseBone.Rotation) * delta);
            return (bone, baseFov + fov * MathF.PI / 180f);
        }

        return (new HavokTransformData
        {
            Translation = position,
            Rotation = CutsceneCameraRotation.FromAngles(rotation),
            Scale = baseBone.Scale == Vector3.Zero ? Vector3.One : baseBone.Scale
        }, fov * MathF.PI / 180f);
    }

    /// <summary>The keys' values at a frame (no keys: zeros, which is no offset)</summary>
    public (Vector3 Position, Vector3 Rotation, float Fov) Interpolate(float frame)
    {
        if (Keys.Count == 0)
            return (Vector3.Zero, Vector3.Zero, 0);

        if (Keys.Count == 1 || frame <= Keys[0].Frame)
            return Values(Keys[0]);

        if (frame >= Keys[^1].Frame)
            return Values(Keys[^1]);

        var i = Keys.FindLastIndex(k => k.Frame <= frame);
        var a = Keys[i];
        var b = Keys[i + 1];
        var span = b.Frame - a.Frame;
        var u = span > 0 ? (frame - a.Frame) / span : 0;

        switch (a.Interpolation)
        {
            case CutsceneKeyInterpolation.Hold:
                return Values(a);

            case CutsceneKeyInterpolation.Linear:
                return (Vector3.Lerp(a.Position, b.Position, u), Vector3.Lerp(a.Rotation, b.Rotation, u),
                    a.FovDegrees + (b.FovDegrees - a.FovDegrees) * u);

            default:
            {
                // Catmull-Rom tangents in frame time, from the keys on either side (one-sided at the ends)
                var prev = i > 0 ? Keys[i - 1] : a;
                var next = i + 2 < Keys.Count ? Keys[i + 2] : b;
                float Rate(int f0, int f1) => f1 > f0 ? f1 - f0 : 1;

                var ta = Rate(prev.Frame, b.Frame);
                var tb = Rate(a.Frame, next.Frame);

                Vector3 V(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) =>
                    Hermite(p1, p2, (p2 - p0) / ta * span, (p3 - p1) / tb * span, u);

                float S(float p0, float p1, float p2, float p3) =>
                    Hermite(p1, p2, (p2 - p0) / ta * span, (p3 - p1) / tb * span, u);

                return (V(prev.Position, a.Position, b.Position, next.Position),
                    V(prev.Rotation, a.Rotation, b.Rotation, next.Rotation),
                    S(prev.FovDegrees, a.FovDegrees, b.FovDegrees, next.FovDegrees));
            }
        }
    }

    private static (Vector3, Vector3, float) Values(CutsceneCameraKey k) => (k.Position, k.Rotation, k.FovDegrees);

    private static Vector3 Hermite(Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float u)
    {
        float u2 = u * u, u3 = u2 * u;
        return (2 * u3 - 3 * u2 + 1) * p0 + (u3 - 2 * u2 + u) * m0 + (-2 * u3 + 3 * u2) * p1 + (u3 - u2) * m1;
    }

    private static float Hermite(float p0, float p1, float m0, float m1, float u)
    {
        float u2 = u * u, u3 = u2 * u;
        return (2 * u3 - 3 * u2 + 1) * p0 + (u3 - 2 * u2 + u) * m0 + (-2 * u3 + 3 * u2) * p1 + (u3 - u2) * m1;
    }

    /// <summary>
    /// A key that gives the camera this bone transform and FOV (radians) at its frame: an absolute key in Keyed
    /// mode, the delta from the base camera in Offset mode. <paramref name="near" /> is a neighbouring key, whose yaw
    /// the new one's is unwrapped to, so a smooth curve doesn't spin the long way.
    /// </summary>
    public CutsceneCameraKey KeyFor(int frame, HavokTransformData bone, float fovRadians, HavokTransformData baseBone,
        float baseFov, CutsceneCameraKey? near)
    {
        var key = new CutsceneCameraKey { Frame = frame };

        if (Mode == CutsceneCameraMode.Offset)
        {
            key.Position = bone.Translation - baseBone.Translation;
            var delta = Quaternion.Normalize(Quaternion.Conjugate(Quaternion.Normalize(baseBone.Rotation)) *
                                             Quaternion.Normalize(bone.Rotation));
            key.Rotation = CutsceneCameraRotation.ToLocalAngles(delta);
            key.FovDegrees = (fovRadians - baseFov) * 180f / MathF.PI;
        }
        else
        {
            key.Position = bone.Translation;
            key.Rotation = CutsceneCameraRotation.ToAngles(bone.Rotation);
            key.FovDegrees = fovRadians * 180f / MathF.PI;
        }

        if (near != null)
        {
            var r = key.Rotation;
            r.Y = CutsceneCameraRotation.Unwrap(r.Y, near.Rotation.Y);
            r.Z = CutsceneCameraRotation.Unwrap(r.Z, near.Rotation.Z);
            key.Rotation = r;
        }

        return key;
    }

    /// <summary>The key nearest a frame, to unwrap a new key's angles to</summary>
    public CutsceneCameraKey? Nearest(int frame) =>
        Keys.Count == 0 ? null : Keys.MinBy(k => Math.Abs(k.Frame - frame));
}

/// <summary>
/// The camera bone's rotation as pitch, yaw and roll (degrees). The camera looks along the bone's forward axis with
/// its up axis up, both from the options (−Y and +Z by default, see <see cref="CutsceneCamera" />).
/// <list type="bullet">
/// <item>Absolute (Keyed mode): the view direction is yaw about the cut's up (+Y, yaw 0 looking +Z, positive turning
/// toward +X) and pitch above the horizon; roll turns the up about the view.</item>
/// <item>Local (Offset mode): the same, against the camera's own forward, up and right axes.</item>
/// </list>
/// Checked with Claude/Tools/SplineRoundTrip --keymath.
/// </summary>
public static class CutsceneCameraRotation
{
    private static Vector3 Forward => CutsceneCamera.Axis((CutsceneCameraAxis)CFG.Current.CutsceneEditor.CutsceneEditor_Camera_Forward_Axis);
    private static Vector3 Up => CutsceneCamera.Axis((CutsceneCameraAxis)CFG.Current.CutsceneEditor.CutsceneEditor_Camera_Up_Axis);

    public static Quaternion FromAngles(Vector3 degrees) =>
        FromAngles(degrees, Forward, Up, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

    public static Vector3 ToAngles(Quaternion rotation) =>
        ToAngles(rotation, Forward, Up, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

    /// <summary>The bone rotation that looks along <paramref name="forward" /> with <paramref name="up" /> up</summary>
    public static Quaternion FromView(Vector3 forward, Vector3 up) => Mapping(Forward, Up, forward, up);

    public static Quaternion FromLocalAngles(Vector3 degrees)
    {
        var (f, u, r) = LocalFrame(Forward, Up);
        return FromAngles(degrees, f, u, r, u, f);
    }

    public static Vector3 ToLocalAngles(Quaternion delta)
    {
        var (f, u, r) = LocalFrame(Forward, Up);
        return ToAngles(delta, f, u, r, u, f);
    }

    // The bone's forward and up, made orthogonal, and the right that makes (right, up, forward) turn like (X, Y, Z)
    private static (Vector3 F, Vector3 U, Vector3 R) LocalFrame(Vector3 forward, Vector3 up)
    {
        var f = Vector3.Normalize(forward);
        var u = Vector3.Normalize(up - f * Vector3.Dot(up, f));
        if (!float.IsFinite(u.X + u.Y + u.Z))
            u = MathF.Abs(f.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitZ;
        return (f, u, Vector3.Cross(u, f));
    }

    /// <summary>
    /// The rotation that turns the bone's <paramref name="boneForward" /> and <paramref name="boneUp" /> to the view
    /// the angles give, against the reference axes (right, up, forward).
    /// </summary>
    public static Quaternion FromAngles(Vector3 degrees, Vector3 boneForward, Vector3 boneUp, Vector3 refRight,
        Vector3 refUp, Vector3 refForward)
    {
        var pitch = degrees.X * MathF.PI / 180f;
        var yaw = degrees.Y * MathF.PI / 180f;
        var roll = degrees.Z * MathF.PI / 180f;

        var d = Vector3.Normalize(refRight * (MathF.Sin(yaw) * MathF.Cos(pitch)) + refUp * MathF.Sin(pitch) +
                                  refForward * (MathF.Cos(yaw) * MathF.Cos(pitch)));
        var u0 = LevelUp(d, refUp, refForward, yaw);
        var u = u0 * MathF.Cos(roll) + Vector3.Cross(d, u0) * MathF.Sin(roll);

        return Mapping(boneForward, boneUp, d, u);
    }

    public static Vector3 ToAngles(Quaternion rotation, Vector3 boneForward, Vector3 boneUp, Vector3 refRight,
        Vector3 refUp, Vector3 refForward)
    {
        rotation = Quaternion.Normalize(rotation);
        var d = Vector3.Normalize(Vector3.Transform(boneForward, rotation));
        var u = Vector3.Normalize(Vector3.Transform(boneUp, rotation));

        var pitch = MathF.Asin(Math.Clamp(Vector3.Dot(d, refUp), -1, 1));
        var yaw = MathF.Atan2(Vector3.Dot(d, refRight), Vector3.Dot(d, refForward));
        var u0 = LevelUp(d, refUp, refForward, yaw);
        var roll = MathF.Atan2(Vector3.Dot(Vector3.Cross(d, u0), u), Vector3.Dot(u0, u));

        return new Vector3(pitch, yaw, roll) * (180f / MathF.PI);
    }

    // The up of a view with no roll: the reference up, square to the view. Looking straight up or down, where that
    // is undefined, the forward turned by the yaw stands in.
    private static Vector3 LevelUp(Vector3 d, Vector3 refUp, Vector3 refForward, float yaw)
    {
        var u0 = refUp - d * Vector3.Dot(d, refUp);
        if (u0.LengthSquared() < 1e-8f)
            u0 = -refForward * MathF.Cos(yaw) * MathF.Sign(Vector3.Dot(d, refUp));
        return Vector3.Normalize(u0);
    }

    /// <summary>The rotation taking a (forward, up) pair of the bone onto a (forward, up) pair of the view</summary>
    public static Quaternion Mapping(Vector3 boneForward, Vector3 boneUp, Vector3 forward, Vector3 up)
    {
        var (f0, u0, s0) = LocalFrame(boneForward, boneUp);
        var (f1, u1, s1) = LocalFrame(forward, up);

        // Row vectors (v' = v M): each bone axis goes to its view axis
        var m = new Matrix4x4(
            f0.X * f1.X + u0.X * u1.X + s0.X * s1.X, f0.X * f1.Y + u0.X * u1.Y + s0.X * s1.Y, f0.X * f1.Z + u0.X * u1.Z + s0.X * s1.Z, 0,
            f0.Y * f1.X + u0.Y * u1.X + s0.Y * s1.X, f0.Y * f1.Y + u0.Y * u1.Y + s0.Y * s1.Y, f0.Y * f1.Z + u0.Y * u1.Z + s0.Y * s1.Z, 0,
            f0.Z * f1.X + u0.Z * u1.X + s0.Z * s1.X, f0.Z * f1.Y + u0.Z * u1.Y + s0.Z * s1.Y, f0.Z * f1.Z + u0.Z * u1.Z + s0.Z * s1.Z, 0,
            0, 0, 0, 1);

        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(m));
    }

    /// <summary>An angle (degrees) moved by whole turns to be nearest another</summary>
    public static float Unwrap(float angle, float near)
    {
        while (angle - near > 180) angle -= 360;
        while (angle - near < -180) angle += 360;
        return angle;
    }
}

/// <summary>
/// The camera keys of one cutscene, kept beside the project in .smithbox/Cutscenes/&lt;cutscene&gt;.json, by
/// "cutNNNN/aNNNN/camNNNN" (the HKT's file and the camera's name). The hkx is what the game reads; this keeps the
/// keys it was baked from.
/// </summary>
public sealed class CutsceneCameraKeyStore
{
    public Dictionary<string, CutsceneCameraKeys> Cameras = new(StringComparer.OrdinalIgnoreCase);

    public static string PathFor(ProjectEntry project, string cutsceneName) =>
        Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Cutscenes", $"{cutsceneName}.json");

    public static CutsceneCameraKeyStore Load(ProjectEntry project, string cutsceneName) =>
        LoadFrom(PathFor(project, cutsceneName));

    public static CutsceneCameraKeyStore LoadFrom(string path)
    {
        var store = new CutsceneCameraKeyStore();
        if (!File.Exists(path))
            return store;

        try
        {
            var dto = JsonSerializer.Deserialize<StoreDto>(File.ReadAllText(path));
            foreach (var (id, cam) in dto?.Cameras ?? new())
            {
                var keys = new CutsceneCameraKeys
                {
                    Mode = Enum.TryParse<CutsceneCameraMode>(cam.Mode, out var m) ? m : CutsceneCameraMode.Vanilla,
                    Format = Enum.TryParse<HavokClipFormat>(cam.Format, out var f) ? f : HavokClipFormat.Auto,
                    Keys = cam.Keys.Select(k => new CutsceneCameraKey
                    {
                        Frame = k.Frame,
                        Position = V(k.Position),
                        Rotation = V(k.Rotation),
                        FovDegrees = k.Fov,
                        Interpolation = Enum.TryParse<CutsceneKeyInterpolation>(k.Interpolation, out var i) ? i : CutsceneKeyInterpolation.Smooth
                    }).ToList()
                };
                keys.Sort();
                store.Cameras[id] = keys;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(CutsceneCameraKeyStore), LOC.Get("CUTSCENE_Keys_Load_Failed", path), e);
        }

        return store;
    }

    /// <summary>Writes the store. With no camera keyed and no file yet, nothing is written.</summary>
    public void Save(ProjectEntry project, string cutsceneName) => SaveTo(PathFor(project, cutsceneName));

    public void SaveTo(string path)
    {
        if (Cameras.Count == 0 && !File.Exists(path))
            return;

        var dto = new StoreDto
        {
            Cameras = Cameras.ToDictionary(p => p.Key, p => new CameraDto
            {
                Mode = p.Value.Mode.ToString(),
                Format = p.Value.Format.ToString(),
                Keys = p.Value.Keys.Select(k => new KeyDto
                {
                    Frame = k.Frame,
                    Position = A(k.Position),
                    Rotation = A(k.Rotation),
                    Fov = k.FovDegrees,
                    Interpolation = k.Interpolation.ToString()
                }).ToList()
            })
        };

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static Vector3 V(float[]? a) => a is { Length: >= 3 } ? new Vector3(a[0], a[1], a[2]) : Vector3.Zero;
    private static float[] A(Vector3 v) => [v.X, v.Y, v.Z];

    private sealed class StoreDto
    {
        public int Version { get; set; } = 1;
        public Dictionary<string, CameraDto> Cameras { get; set; } = new();
    }

    private sealed class CameraDto
    {
        public string Mode { get; set; } = "";
        public string Format { get; set; } = "";
        public List<KeyDto> Keys { get; set; } = new();
    }

    private sealed class KeyDto
    {
        public int Frame { get; set; }
        public float[] Position { get; set; } = [];
        public float[] Rotation { get; set; } = [];
        public float Fov { get; set; }
        public string Interpolation { get; set; } = "";
    }
}
