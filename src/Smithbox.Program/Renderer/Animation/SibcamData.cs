#nullable enable
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
///     A SIBCAM camera animation, as the older games keep their rumble cams (other/rumble/camera_NNN.sibcam in
///     Dark Souls, Demon's Souls and Dark Souls III). Only the camera's motion is read; the field of view and the
///     structures nothing is known about are skipped. Layout from DSAnimStudio's SIBCAM2.
/// </summary>
public sealed class SibcamData
{
    private const float FrameDuration = 1f / 30f;

    // The frame data indices carry flags in their top bits
    private const int FrameIndexMask = 0x7FFFFFF;

    private const int CamsOffset = 0x84;
    private const int CamTrailingInts = 43;

    // The keys, by their frame: the files keep a key on every frame for most rumbles, and fewer for long ones
    private readonly List<(int Frame, Matrix4x4 Transform)> _keys = new();

    public int LastFrame { get; private set; }

    public float Duration => LastFrame * FrameDuration;

    public int KeyCount => _keys.Count;

    /// <summary>The frame of the last key</summary>
    public int LastKeyFrame => _keys.Count > 0 ? _keys[^1].Frame : 0;

    /// <summary>
    ///     Reads a SIBCAM, or returns null if the data isn't one.
    /// </summary>
    public static SibcamData? Read(Memory<byte> data)
    {
        var br = new BinaryReaderEx(false, data);

        var endian = br.ReadInt32();
        if (endian != 1 && endian != 0x1000000)
            return null;

        br.BigEndian = endian == 0x1000000;

        if (br.ReadInt32() != 8 || br.ReadInt32() != 0xC || br.ReadInt32() != 0x20051014)
            return null;

        br.ReadInt32();
        var sibcam = new SibcamData { LastFrame = br.ReadInt32() };

        var camsOffset = br.ReadInt32();
        var camsCount = br.ReadInt32();
        var frameDataOffset = br.ReadInt32();
        br.ReadInt32();
        var frameDataCount = br.ReadInt32();

        if (camsOffset != CamsOffset || camsCount < 0 || frameDataCount < 0)
            return null;

        var cams = new List<(Vector3 Translation, Vector3 Rotation, List<int[]> Keys)>(camsCount);

        br.Position = camsOffset;
        for (var i = 0; i < camsCount; i++)
        {
            cams.Add(ReadCam(br));
        }

        var frameData = new Vector3[frameDataCount];
        br.Position = frameDataOffset;
        for (var i = 0; i < frameDataCount; i++)
        {
            frameData[i] = br.ReadVector3();
        }

        // The cameras play one after another, each from where the last one ended
        var start = 0;

        foreach (var (translation, rotation, keys) in cams)
        {
            var end = start;

            foreach (var indices in keys)
            {
                Vector3 At(int index) => frameData[Math.Clamp(index & FrameIndexMask, 0, Math.Max(frameDataCount - 1, 0))];

                var frame = start + Math.Max(indices[0], 0);
                sibcam._keys.Add((frame, ToMatrix(At(indices[1]), At(indices[2]), translation, rotation)));
                end = Math.Max(end, frame);
            }

            start = end + 1;
        }

        sibcam._keys.Sort((a, b) => a.Frame.CompareTo(b.Frame));

        return sibcam;
    }

    // A camera: its rest transform, and per frame the indices of its position and rotation in the frame data
    private static (Vector3 Translation, Vector3 Rotation, List<int[]> Keys) ReadCam(BinaryReaderEx br)
    {
        br.ReadUInt32(); // name
        br.ReadInt32();
        br.ReadInt16(); // index
        br.ReadInt16();
        br.ReadInt32();
        br.ReadInt16(); // next index
        br.ReadInt16();
        Vector3 translation = br.ReadVector3();
        Vector3 rotation = br.ReadVector3();
        br.ReadVector3(); // scale
        var animationOffset = br.ReadUInt32();
        br.ReadInt32();
        br.ReadUInt32();
        br.ReadUInt32();

        for (var i = 0; i < CamTrailingInts; i++)
        {
            br.ReadInt32();
        }

        var frames = new List<int[]>();

        br.StepIn(animationOffset);
        var framesOffset = br.ReadUInt32();
        var framesCount = br.ReadInt32();

        br.StepIn(framesOffset);
        for (var i = 0; i < framesCount; i++)
        {
            var frame = br.ReadInt32();
            var position = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            var rotationIndex = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32(); // scale

            frames.Add(new[] { frame, position, rotationIndex });
        }
        br.StepOut();
        br.StepOut();

        return (translation, rotation, frames);
    }

    // As DSAnimStudio's SibcamPlayer.SibcamAnimFrameToTransform: Z flipped, Euler angles in X, Z, Y order
    private static Matrix4x4 ToMatrix(Vector3 position, Vector3 rotation, Vector3 camTranslation, Vector3 camRotation)
    {
        Vector3 translation = (position + camTranslation * new Vector3(1, 1, 0)) * new Vector3(1, 1, -1);

        Matrix4x4 turn = Matrix4x4.CreateRotationX(-(rotation.X - camRotation.X)) *
                         Matrix4x4.CreateRotationZ(rotation.Z + camRotation.Z) *
                         Matrix4x4.CreateRotationY(-rotation.Y - camRotation.Y);

        return turn * Matrix4x4.CreateTranslation(translation);
    }

    /// <summary>
    ///     Where the camera is at a time, between the keys either side of it.
    /// </summary>
    public Matrix4x4 Sample(float time)
    {
        if (_keys.Count == 0)
            return Matrix4x4.Identity;

        var frame = time / FrameDuration;

        if (frame <= _keys[0].Frame)
            return _keys[0].Transform;

        for (var i = 1; i < _keys.Count; i++)
        {
            if (frame <= _keys[i].Frame)
            {
                var (fromFrame, from) = _keys[i - 1];
                var span = _keys[i].Frame - fromFrame;

                return span > 0 ? Lerp(from, _keys[i].Transform, (frame - fromFrame) / span) : _keys[i].Transform;
            }
        }

        return _keys[^1].Transform;
    }

    private static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float t)
    {
        if (t <= 0 || !Matrix4x4.Decompose(a, out _, out Quaternion ra, out Vector3 ta) ||
            !Matrix4x4.Decompose(b, out _, out Quaternion rb, out Vector3 tb))
        {
            return a;
        }

        return Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(ra, rb, t)) * Matrix4x4.CreateTranslation(Vector3.Lerp(ta, tb, t));
    }
}
