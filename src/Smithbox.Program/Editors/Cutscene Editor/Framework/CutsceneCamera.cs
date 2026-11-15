#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>The local axes a cutscene camera bone can look or point up along.</summary>
public enum CutsceneCameraAxis
{
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveZ,
    NegativeZ
}

/// <summary>
/// A camera resource of the MQB (type 3) in one cut. It is a one-bone skeleton in the cut's Havok file, animated like
/// any actor, with a float slot named camNNNN_FOV for its field of view (radians). Which of the bone's axes it looks
/// along isn't known from the files, so it is an option of the editor (<see cref="CFG" />).
/// </summary>
public sealed class CutsceneCamera
{
    public int ResourceIndex;
    public MQB.Resource Resource;
    public List<MQB.Event> Events = new();

    public HavokSkeletonData? Skeleton;
    public HavokClip? Clip;

    /// <summary>The camNNNN_FOV slot, -1 if the skeleton has none</summary>
    public int FovSlot = -1;

    public float NearPlane = 0.1f;
    public float FarPlane = 10000f;

    public string Name => Resource.Name;

    public CutsceneCamera(int resourceIndex, MQB.Resource resource)
    {
        ResourceIndex = resourceIndex;
        Resource = resource;
    }

    public void ReadEventParameters()
    {
        foreach (var e in Events)
        {
            foreach (var p in e.Parameters)
            {
                if (p.Value is not float f)
                    continue;

                if (p.Name.Equals("nearPlane", StringComparison.OrdinalIgnoreCase))
                    NearPlane = f;
                else if (p.Name.Equals("farPlane", StringComparison.OrdinalIgnoreCase))
                    FarPlane = f;
            }
        }
    }

    public void FindFovSlot()
    {
        FovSlot = -1;
        if (Skeleton == null)
            return;

        for (var i = 0; i < Skeleton.FloatSlotCount; i++)
        {
            if (Skeleton.FloatSlots[i].EndsWith("_FOV", StringComparison.OrdinalIgnoreCase))
            {
                FovSlot = i;
                return;
            }
        }
    }

    /// <summary>Whether one of the camera's events covers the frame (true if this cut lists none)</summary>
    public bool IsActiveAt(float frame)
    {
        if (Events.Count == 0)
            return true;

        foreach (var e in Events)
        {
            if (frame >= e.StartFrame && frame < e.StartFrame + Math.Max(1, e.Duration))
                return true;
        }

        return false;
    }

    public Matrix4x4 World(float frame, Matrix4x4 cutRoot) => Skeleton == null
        ? cutRoot
        : CutsceneSkeletonPose.BoneWorld(Skeleton, Clip, 0, frame) * cutRoot;

    /// <summary>The field of view the file gives, in radians, or null if it gives none.</summary>
    public float? FovRadians(float frame)
    {
        if (Skeleton == null || Clip == null || FovSlot < 0)
            return null;

        var fov = Clip.SampleSlot(Skeleton, FovSlot, Math.Clamp(frame, 0, Math.Max(0, Clip.FrameCount - 1)));
        return fov > 0.001f && fov < MathF.PI ? fov : null;
    }

    /// <summary>The vertical field of view in radians for an aspect ratio, from the file's value and the options.</summary>
    public float VerticalFovRadians(float frame, float aspect)
    {
        var fov = FovRadians(frame) ?? CFG.Current.Viewport_Camera_FOV * MathF.PI / 180f;

        if (CFG.Current.CutsceneEditor_Camera_Fov_Is_Horizontal && aspect > 0)
            fov = 2 * MathF.Atan(MathF.Tan(fov / 2) / aspect);

        return fov;
    }

    /// <summary>The camera's eye, forward and up in the scene, by the axes chosen in the options.</summary>
    public static void GetView(Matrix4x4 world, out Vector3 eye, out Vector3 forward, out Vector3 up)
    {
        eye = world.Translation;
        forward = Vector3.Normalize(Vector3.TransformNormal(Axis((CutsceneCameraAxis)CFG.Current.CutsceneEditor_Camera_Forward_Axis), world));
        up = Vector3.Normalize(Vector3.TransformNormal(Axis((CutsceneCameraAxis)CFG.Current.CutsceneEditor_Camera_Up_Axis), world));

        if (!float.IsFinite(forward.X + forward.Y + forward.Z))
            forward = Vector3.UnitZ;

        // A forward and up along the same axis leave no right: take another up
        if (MathF.Abs(Vector3.Dot(forward, up)) > 0.999f || !float.IsFinite(up.X + up.Y + up.Z))
            up = MathF.Abs(forward.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitZ;
    }

    public static Vector3 Axis(CutsceneCameraAxis axis) => axis switch
    {
        CutsceneCameraAxis.PositiveX => Vector3.UnitX,
        CutsceneCameraAxis.NegativeX => -Vector3.UnitX,
        CutsceneCameraAxis.PositiveY => Vector3.UnitY,
        CutsceneCameraAxis.NegativeY => -Vector3.UnitY,
        CutsceneCameraAxis.PositiveZ => Vector3.UnitZ,
        _ => -Vector3.UnitZ
    };

    /// <summary>
    /// The rotation of a viewport camera that looks along <paramref name="forward" /> with <paramref name="up" /> up:
    /// the viewport looks along its rotation's +Z, with +Y up (Transform.CameraViewMatrixLH).
    /// </summary>
    public static Quaternion ViewportRotation(Vector3 forward, Vector3 up)
    {
        var z = Vector3.Normalize(forward);
        var x = Vector3.Normalize(Vector3.Cross(up, z));
        var y = Vector3.Cross(z, x);

        var basis = new Matrix4x4(
            x.X, x.Y, x.Z, 0,
            y.X, y.Y, y.Z, 0,
            z.X, z.Y, z.Z, 0,
            0, 0, 0, 1);

        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(basis));
    }
}
