#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     Tolerances and formats for <see cref="SplineCompressedEncoder" />. The defaults are tighter than Havok's own
///     (0.01 translation, 0.001 rotation), because a cutscene camera magnifies its errors over distance.
/// </summary>
public sealed class SplineEncodeSettings
{
    // Knots are bytes, so a block can't span more than 256 frames. Every vanilla ER cutscene clip uses 256.
    public int MaxFramesPerBlock = 256;

    // World units
    public float PositionTolerance = 0.0005f;

    // Radians, the angle between the sampled and the decoded rotation
    public float RotationTolerance = 0.0001f;

    public float ScaleTolerance = 0.0005f;

    // Float tracks (the camera FOV, radians)
    public float FloatTolerance = 0.00005f;

    // How often each frame is sampled for the fit and its check. The game plays at more frames per second than a
    // clip holds, so what lies between frames is seen too.
    public int SamplesPerFrame = 2;
}

/// <summary>
///     The fields of an hkaSplineCompressedAnimation that the encoder sets.
/// </summary>
public sealed class SplineEncodedClip
{
    public int TransformTrackCount;
    public int FloatTrackCount;
    public int FrameCount;
    public int BlockCount;
    public int MaxFramesPerBlock;
    public int MaskAndQuantizationSize;
    public float Duration;
    public float FrameDuration;
    public float BlockDuration;
    public float BlockInverseDuration;
    public byte[] Data = Array.Empty<byte>();
    public uint[] BlockOffsets = Array.Empty<uint>();
    public uint[] FloatBlockOffsets = Array.Empty<uint>();
    public List<SplineCompressedLayout.Block> Blocks = new();

    // Channel groups (a block's position, rotation, scale or float track) that couldn't be held within tolerance at
    // every sample, even with the shortest blocks. They are stored as closely as the format allows.
    public int MissedGroups;

    // The largest error at whole frames over its tolerance, across every channel group. Above 1 the spline form can't
    // hold the clip as asked (a camera moving metres per frame, whose 16-bit steps are too coarse), and an
    // uncompressed (interleaved) clip is the faithful choice.
    public double WorstErrorRatio;
}

/// <summary>
///     Encodes sampled transform and float tracks as an hkaSplineCompressedAnimation, in the form of the vanilla ER
///     cutscene clips: 16-bit positions, scales and float splines, uncompressed rotations. Each block's channels are
///     dropped when they keep their default (position 0, scale 1, identity rotation), stored once when they don't move,
///     and otherwise fitted with a clamped B-spline (degree 3, or 1 when that takes fewer control points) of as few
///     control points as keep every frame within tolerance. A block's last frame is the next block's first.
///     <para>
///         The fit uses the decoder's own basis (<see cref="SplineCompressedAnimation" />), so what is measured here
///         is what plays.
///     </para>
/// </summary>
public static class SplineCompressedEncoder
{
    private const int RotationQuantization = 5; // UNCOMPRESSED, as every ER cutscene clip

    /// <summary>
    ///     Encodes tracks given only at whole frames. Between frames the fit is free, so prefer the sampler overload
    ///     when the source can be sampled in between.
    /// </summary>
    /// <param name="transforms">[frame, track], local transforms</param>
    /// <param name="floats">[frame, track], or null without float tracks</param>
    public static SplineEncodedClip Encode(HavokTransformData[,] transforms, float[,]? floats, float frameDuration,
        SplineEncodeSettings? settings = null)
    {
        settings ??= new SplineEncodeSettings();
        if (floats != null && floats.GetLength(0) != transforms.GetLength(0))
        {
            throw new ArgumentException("The float tracks need a value for every frame.", nameof(floats));
        }

        var wholeFrames = new SplineEncodeSettings
        {
            MaxFramesPerBlock = settings.MaxFramesPerBlock,
            PositionTolerance = settings.PositionTolerance,
            RotationTolerance = settings.RotationTolerance,
            ScaleTolerance = settings.ScaleTolerance,
            FloatTolerance = settings.FloatTolerance,
            SamplesPerFrame = 1
        };

        return Encode(transforms.GetLength(0), transforms.GetLength(1), floats?.GetLength(1) ?? 0,
            (track, frame) => transforms[(int)frame, track], (track, frame) => floats![(int)frame, track],
            frameDuration, wholeFrames);
    }

    /// <summary>
    ///     Encodes tracks from samplers that can be asked for any time in frames from 0 to frameCount - 1; each frame
    ///     is sampled <see cref="SplineEncodeSettings.SamplesPerFrame" /> times, and the fit is held within tolerance
    ///     at every sample.
    /// </summary>
    /// <param name="sampleTransform">(track, frame) → the track's local transform</param>
    /// <param name="sampleFloat">(float track, frame) → its value; unused without float tracks</param>
    public static SplineEncodedClip Encode(int frameCount, int trackCount, int floatCount,
        Func<int, float, HavokTransformData> sampleTransform, Func<int, float, float>? sampleFloat,
        float frameDuration, SplineEncodeSettings? settings = null)
    {
        settings ??= new SplineEncodeSettings();
        if (settings.MaxFramesPerBlock < 2 || settings.MaxFramesPerBlock > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "MaxFramesPerBlock must be 2 to 256.");
        }

        if (frameCount < 1)
        {
            throw new ArgumentException("A clip needs at least one frame.", nameof(frameCount));
        }

        if (floatCount > 0 && sampleFloat == null)
        {
            throw new ArgumentNullException(nameof(sampleFloat));
        }

        // A block's 16-bit values are spread over the block's own range, so a channel that travels far within one
        // block can't be stored within tolerance (an open-world camera flying 190 m in 256 frames has ~3 mm steps).
        // Shorter blocks have shorter ranges: halve them until every channel fits.
        SplineEncodedClip? best = null;
        for (var size = settings.MaxFramesPerBlock;; size = Math.Max(MinFramesPerBlock, size / 2))
        {
            SplineEncodedClip clip = EncodeBlocks(frameCount, trackCount, floatCount, sampleTransform, sampleFloat,
                frameDuration, settings, size);
            if (best == null || clip.WorstErrorRatio < best.WorstErrorRatio)
            {
                best = clip;
            }

            if (clip.WorstErrorRatio <= 1 || size <= MinFramesPerBlock)
            {
                return best;
            }
        }
    }

    // Below this the block headers cost more than shorter ranges save
    private const int MinFramesPerBlock = 16;

    private static SplineEncodedClip EncodeBlocks(int frameCount, int trackCount, int floatCount,
        Func<int, float, HavokTransformData> sampleTransform, Func<int, float, float>? sampleFloat,
        float frameDuration, SplineEncodeSettings settings, int maxFramesPerBlock)
    {
        var perFrame = Math.Max(1, settings.SamplesPerFrame);
        var span = maxFramesPerBlock - 1;
        var blockCount = Math.Max(1, (frameCount - 1 + span - 1) / span);
        var stats = new EncodeStats();

        // The last rotation of each track, to keep its rotations in one hemisphere so that neighbouring keys blend
        // the short way (carried across blocks, which share a frame)
        var previous = new Quaternion?[trackCount];

        var clip = new SplineEncodedClip
        {
            TransformTrackCount = trackCount,
            FloatTrackCount = floatCount,
            FrameCount = frameCount,
            BlockCount = blockCount,
            MaxFramesPerBlock = maxFramesPerBlock,
            FrameDuration = frameDuration,
            Duration = (frameCount - 1) * frameDuration,
            BlockDuration = span * frameDuration,
            BlockInverseDuration = span * frameDuration > 0 ? 1 / (span * frameDuration) : 0
        };

        for (var b = 0; b < blockCount; b++)
        {
            var first = b * span;
            var frames = Math.Min(span + 1, frameCount - first);
            var block = new SplineCompressedLayout.Block
            {
                Transforms = new SplineCompressedLayout.TransformTrack[trackCount],
                Floats = new SplineCompressedLayout.FloatTrack[floatCount]
            };

            // Sample times from the block's start, in frames
            var n = (frames - 1) * perFrame + 1;
            var times = new double[n];
            for (var i = 0; i < n; i++)
            {
                times[i] = i / (double)perFrame;
            }

            for (var t = 0; t < trackCount; t++)
            {
                var px = new double[3][];
                var sx = new double[3][];
                var rx = new double[4][];
                for (var c = 0; c < 3; c++)
                {
                    px[c] = new double[n];
                    sx[c] = new double[n];
                }

                for (var c = 0; c < 4; c++)
                {
                    rx[c] = new double[n];
                }

                for (var i = 0; i < n; i++)
                {
                    HavokTransformData x = sampleTransform(t, (float)(first + times[i]));
                    Quaternion q = x.Rotation;
                    if (previous[t] is { } p && Quaternion.Dot(q, p) < 0)
                    {
                        q = -q;
                    }

                    previous[t] = q;
                    px[0][i] = x.Translation.X;
                    px[1][i] = x.Translation.Y;
                    px[2][i] = x.Translation.Z;
                    sx[0][i] = x.Scale.X;
                    sx[1][i] = x.Scale.Y;
                    sx[2][i] = x.Scale.Z;
                    rx[0][i] = q.X;
                    rx[1][i] = q.Y;
                    rx[2][i] = q.Z;
                    rx[3][i] = q.W;
                }

                block.Transforms[t] = new SplineCompressedLayout.TransformTrack
                {
                    Position = EncodeScalar(px, times, frames, 0, settings.PositionTolerance, stats),
                    Rotation = EncodeRotation(rx, times, frames, settings.RotationTolerance, stats),
                    Scale = EncodeScalar(sx, times, frames, 1, settings.ScaleTolerance, stats)
                };
            }

            for (var t = 0; t < floatCount; t++)
            {
                var values = new double[n];
                for (var i = 0; i < n; i++)
                {
                    values[i] = sampleFloat!(t, (float)(first + times[i]));
                }

                SplineCompressedLayout.ScalarGroup g =
                    EncodeScalar(new[] { values }, times, frames, double.NaN, settings.FloatTolerance, stats);

                // Vanilla float tracks always carry data: 0x03 a static value, 0x12 a 16-bit spline
                block.Floats[t] = g.IsSpline
                    ? new SplineCompressedLayout.FloatTrack
                    {
                        Mask = SplineCompressedLayout.FloatSpline | SplineCompressedLayout.FloatBits16, Spline = g
                    }
                    : new SplineCompressedLayout.FloatTrack
                    {
                        Mask = SplineCompressedLayout.FloatStatic | SplineCompressedLayout.FloatBits16,
                        Static = g.Static[0]
                    };
            }

            clip.Blocks.Add(block);
        }

        clip.MissedGroups = stats.Missed;
        clip.WorstErrorRatio = stats.WorstRatio;
        SplineCompressedLayout.Written written = SplineCompressedLayout.Write(clip.Blocks);
        clip.Data = written.Data;
        clip.BlockOffsets = written.BlockOffsets;
        clip.FloatBlockOffsets = written.FloatBlockOffsets;
        clip.MaskAndQuantizationSize = written.MaskAndQuantizationSize;
        return clip;
    }

    // ---------------------------------------------------------------- channel groups

    /// <param name="channels">One sample array per component (3 for position/scale, 1 for a float track)</param>
    /// <param name="defaultValue">What an absent component decodes as; NaN when every component must be stored</param>
    private static SplineCompressedLayout.ScalarGroup EncodeScalar(double[][] channels, double[] times, int frames,
        double defaultValue, double tolerance, EncodeStats stats)
    {
        var g = new SplineCompressedLayout.ScalarGroup { Bits16 = true };
        List<int> moving = new();

        for (var c = 0; c < channels.Length; c++)
        {
            double min = double.MaxValue, max = double.MinValue;
            foreach (var v in channels[c])
            {
                min = Math.Min(min, v);
                max = Math.Max(max, v);
            }

            if (max - min <= tolerance)
            {
                var value = (min + max) / 2;
                if (!double.IsNaN(defaultValue) && Math.Abs(value - defaultValue) <= tolerance)
                {
                    continue;
                }

                g.Flags |= (byte)(SplineCompressedLayout.StaticX << c);
                g.Static[c] = (float)value;
            }
            else
            {
                g.Flags |= (byte)(SplineCompressedLayout.SplineX << c);
                moving.Add(c);
            }
        }

        if (moving.Count == 0)
        {
            return g;
        }

        var fitted = FitGroup(channels, times, frames, moving, tolerance, QuantizeScalars, ScalarError);
        stats.Add(fitted, tolerance);
        g.Degree = (byte)fitted.Degree;
        g.Knots = fitted.Knots;
        var m = fitted.ControlPoints[0].Length;
        g.Values = new ushort[m, 3];
        for (var k = 0; k < moving.Count; k++)
        {
            var c = moving[k];
            Quantize16(fitted.ControlPoints[k], out var min, out var max, out var codes);
            g.Min[c] = min;
            g.Max[c] = max;
            for (var i = 0; i < m; i++)
            {
                g.Values[i, c] = codes[i];
            }
        }

        return g;
    }

    private static SplineCompressedLayout.RotationGroup EncodeRotation(double[][] q, double[] times, int frames,
        double tolerance, EncodeStats stats)
    {
        var n = q[0].Length;
        var g = new SplineCompressedLayout.RotationGroup { Quantization = RotationQuantization };

        // Static when every frame is within tolerance of the mean rotation; absent when that is the identity
        var mean = Quaternion.Normalize(new Quaternion((float)Average(q[0]), (float)Average(q[1]),
            (float)Average(q[2]), (float)Average(q[3])));
        var still = true;
        for (var i = 0; i < n && still; i++)
        {
            still = Angle(mean.X, mean.Y, mean.Z, mean.W, q[0][i], q[1][i], q[2][i], q[3][i]) <= tolerance;
        }

        if (still)
        {
            if (Angle(0, 0, 0, 1, mean.X, mean.Y, mean.Z, mean.W) <= tolerance)
            {
                return g;
            }

            g.Flags = 0x0F;
            g.Points.Add(QuaternionBytes(mean.X, mean.Y, mean.Z, mean.W));
            return g;
        }

        var fitted = FitGroup(q, times, frames, new List<int> { 0, 1, 2, 3 }, tolerance, (cps, _) => cps,
            RotationError);
        stats.Add(fitted, tolerance);
        g.Flags = 0xF0;
        g.Degree = (byte)fitted.Degree;
        g.Knots = fitted.Knots;
        for (var i = 0; i < fitted.ControlPoints[0].Length; i++)
        {
            g.Points.Add(QuaternionBytes((float)fitted.ControlPoints[0][i], (float)fitted.ControlPoints[1][i],
                (float)fitted.ControlPoints[2][i], (float)fitted.ControlPoints[3][i]));
        }

        return g;
    }

    // ---------------------------------------------------------------- fitting

    private sealed record Fit(int Degree, byte[] Knots, double[][] ControlPoints, double Error, bool Within = true);

    private sealed class EncodeStats
    {
        public int Missed;
        public double WorstRatio;

        public void Add(Fit fit, double tolerance)
        {
            Missed += fit.Within ? 0 : 1;
            WorstRatio = Math.Max(WorstRatio, tolerance > 0 ? fit.Error / tolerance : 0);
        }
    }

    // Turns fitted control points into what the file will hold (quantized and back), per moving component
    private delegate double[][] Storage(double[][] controlPoints, List<int> moving);

    // The largest error over the block of the stored spline against the samples
    private delegate double Error(double[][] samples, double[] times, List<int> moving, int degree, byte[] knots,
        double[][] stored);

    /// <summary>
    ///     The fewest control points, of degree 3 or 1, that keep the group within tolerance once stored. Degree 1
    ///     with a control point per frame interpolates the whole frames.
    /// </summary>
    /// <param name="frames">The frames of the block: knots fall on them, so there can be no more control points</param>
    private static Fit FitGroup(double[][] channels, double[] times, int frames, List<int> moving, double tolerance,
        Storage storage, Error error)
    {
        var n = frames;
        Fit? best = null;

        foreach (var degree in new[] { 3, 1 })
        {
            if (n < degree + 1)
            {
                continue;
            }

            Fit? Try(int m)
            {
                var knots = MakeKnots(n, m, degree);
                var cps = Solve(channels, times, moving, degree, knots, m);
                var stored = storage(cps, moving);
                var e = error(channels, times, moving, degree, knots, stored);
                return e <= tolerance ? new Fit(degree, knots, cps, e) : null;
            }

            // The error falls roughly as the control points grow: search for the first that fits, then walk up
            // from there in case the search landed on an unlucky count
            int low = degree + 1, high = n;
            Fit? found = null;
            while (low < high)
            {
                var mid = (low + high) / 2;
                if (Try(mid) is { } fit)
                {
                    found = fit;
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }

            if (found == null || found.ControlPoints[0].Length != low)
            {
                found = Try(low) ?? found;
            }

            for (var m = low + 1; found == null && m <= n; m++)
            {
                found = Try(m);
            }

            if (found != null && (best == null || found.ControlPoints[0].Length < best.ControlPoints[0].Length))
            {
                best = found;
            }
        }

        // Nothing within tolerance (quantization alone exceeds it, or the motion between frames is finer than a
        // spline with knots on frames can follow): a control point on each frame's own value, joined linearly, so
        // that at least every whole frame is as close as the storage allows
        if (best == null)
        {
            var degree = n >= 2 ? 1 : 0;
            var knots = MakeKnots(n, n, degree);
            var perFrame = n > 1 ? (times.Length - 1) / (n - 1) : 1;
            var cps = new double[moving.Count][];
            for (var k = 0; k < moving.Count; k++)
            {
                cps[k] = new double[n];
                for (var j = 0; j < n; j++)
                {
                    cps[k][j] = channels[moving[k]][j * perFrame];
                }
            }

            // Its error at whole frames, which is the storage's alone: shorter blocks can lower it, while nothing
            // in this format can follow detail between frames any closer
            var wholeTimes = new double[n];
            var wholeChannels = new double[channels.Length][];
            for (var c = 0; c < channels.Length; c++)
            {
                wholeChannels[c] = new double[n];
                for (var j = 0; j < n; j++)
                {
                    wholeChannels[c][j] = channels[c][j * perFrame];
                }
            }

            for (var j = 0; j < n; j++)
            {
                wholeTimes[j] = j;
            }

            var e = error(wholeChannels, wholeTimes, moving, degree, knots, storage(cps, moving));
            best = new Fit(degree, knots, cps, e, false);
        }

        return best;
    }

    /// <summary>
    ///     A clamped knot vector over frames 0 to n - 1 for m control points: degree + 1 knots at each end, the rest
    ///     spread evenly. With m ≤ n they fall on distinct frames.
    /// </summary>
    private static byte[] MakeKnots(int n, int m, int degree)
    {
        var knots = new byte[m + degree + 1];
        var last = (byte)(n - 1);
        for (var i = 0; i <= degree; i++)
        {
            knots[i] = 0;
            knots[knots.Length - 1 - i] = last;
        }

        var segments = m - degree;
        for (var i = 1; i < segments; i++)
        {
            knots[degree + i] = (byte)Math.Round(i * (n - 1) / (double)segments);
        }

        return knots;
    }

    /// <summary>
    ///     Least squares control points for each moving component over the samples, through the banded normal
    ///     equations (each sample touches degree + 1 control points).
    /// </summary>
    private static double[][] Solve(double[][] channels, double[] times, List<int> moving, int degree, byte[] knots,
        int m)
    {
        var n = times.Length;
        var band = degree;
        var normal = new double[m, 2 * band + 1]; // normal[i, band + (j - i)]
        var rhs = new double[moving.Count][];
        for (var k = 0; k < moving.Count; k++)
        {
            rhs[k] = new double[m];
        }

        var basis = new double[degree + 1];
        for (var f = 0; f < n; f++)
        {
            var span = Basis(degree, times[f], knots, m, basis);
            for (var a = 0; a <= degree; a++)
            {
                var i = span - a;
                for (var b = 0; b <= degree; b++)
                {
                    var j = span - b;
                    normal[i, band + j - i] += basis[a] * basis[b];
                }

                for (var k = 0; k < moving.Count; k++)
                {
                    rhs[k][i] += basis[a] * channels[moving[k]][f];
                }
            }
        }

        // A touch of damping keeps a control point no frame reaches from making the system singular
        for (var i = 0; i < m; i++)
        {
            normal[i, band] += 1e-12;
        }

        CholeskyBanded(normal, m, band);

        var result = new double[moving.Count][];
        for (var k = 0; k < moving.Count; k++)
        {
            result[k] = SolveBanded(normal, m, band, rhs[k]);
        }

        return result;
    }

    // In place: the lower factor L in the lower half of the band, so that normal = L Lᵀ
    private static void CholeskyBanded(double[,] a, int m, int band)
    {
        for (var i = 0; i < m; i++)
        {
            for (var j = Math.Max(0, i - band); j <= i; j++)
            {
                var sum = a[i, band + j - i];
                for (var k = Math.Max(0, i - band); k < j; k++)
                {
                    if (j - k <= band)
                    {
                        sum -= a[i, band + k - i] * a[j, band + k - j];
                    }
                }

                if (i == j)
                {
                    a[i, band] = Math.Sqrt(Math.Max(sum, 1e-300));
                }
                else
                {
                    a[i, band + j - i] = sum / a[j, band];
                }
            }
        }
    }

    private static double[] SolveBanded(double[,] l, int m, int band, double[] b)
    {
        var y = new double[m];
        for (var i = 0; i < m; i++)
        {
            var sum = b[i];
            for (var k = Math.Max(0, i - band); k < i; k++)
            {
                sum -= l[i, band + k - i] * y[k];
            }

            y[i] = sum / l[i, band];
        }

        var x = new double[m];
        for (var i = m - 1; i >= 0; i--)
        {
            var sum = y[i];
            for (var k = i + 1; k <= Math.Min(m - 1, i + band); k++)
            {
                sum -= l[k, band + i - k] * x[k];
            }

            x[i] = sum / l[i, band];
        }

        return x;
    }

    /// <summary>
    ///     The decoder's knot span and basis (SplineCompressedAnimation.FindKnotSpan and GetSinglePoint): the value at
    ///     a frame is Σ controlPoint[span - i] · basis[i].
    /// </summary>
    private static int Basis(int degree, double frame, byte[] knots, int m, double[] basis)
    {
        int span;
        if (frame >= knots[m])
        {
            span = m - 1;
        }
        else
        {
            int low = degree, high = m, mid = (low + high) / 2;
            while (frame < knots[mid] || frame >= knots[mid + 1])
            {
                if (frame < knots[mid])
                {
                    high = mid;
                }
                else
                {
                    low = mid;
                }

                mid = (low + high) / 2;
            }

            span = mid;
        }

        Array.Clear(basis);
        basis[0] = 1;
        for (var i = 1; i <= degree; i++)
        {
            for (var j = i - 1; j >= 0; j--)
            {
                double width = knots[span + i - j] - knots[span - j];
                var a = width != 0 ? (frame - knots[span - j]) / width : 0;
                var tmp = basis[j] * a;
                basis[j + 1] += basis[j] - tmp;
                basis[j] = tmp;
            }
        }

        return span;
    }

    /// <summary>
    ///     SplineCompressedAnimation's scalar evaluation, float for float (FindKnotSpan and GetSinglePoint), so the
    ///     error is the one playback shows.
    /// </summary>
    private static float EvaluateAsDecoder(int degree, float frame, byte[] knots, double[] controlPoints)
    {
        var m = controlPoints.Length;
        int span;
        if (frame >= knots[m])
        {
            span = m - 1;
        }
        else
        {
            int low = degree, high = m, mid = (low + high) / 2;
            while (frame < knots[mid] || frame >= knots[mid + 1])
            {
                if (frame < knots[mid])
                {
                    high = mid;
                }
                else
                {
                    low = mid;
                }

                mid = (low + high) / 2;
            }

            span = mid;
        }

        Span<float> n = stackalloc float[5];
        n.Clear();
        n[0] = 1;
        for (var i = 1; i <= degree; i++)
        {
            for (var j = i - 1; j >= 0; j--)
            {
                float width = knots[span + i - j] - knots[span - j];
                var a = width != 0 ? (frame - knots[span - j]) / width : 0;
                var tmp = n[j] * a;
                n[j + 1] += n[j] - tmp;
                n[j] = tmp;
            }
        }

        var value = 0.0f;
        for (var i = 0; i <= degree; i++)
        {
            value += (float)controlPoints[span - i] * n[i];
        }

        return value;
    }

    private static double Evaluate(int degree, double frame, byte[] knots, double[] controlPoints, double[] basis)
    {
        var span = Basis(degree, frame, knots, controlPoints.Length, basis);
        double value = 0;
        for (var i = 0; i <= degree; i++)
        {
            value += controlPoints[span - i] * basis[i];
        }

        return value;
    }

    // ---------------------------------------------------------------- storage and error

    private static double[][] QuantizeScalars(double[][] controlPoints, List<int> moving)
    {
        var result = new double[controlPoints.Length][];
        for (var k = 0; k < controlPoints.Length; k++)
        {
            Quantize16(controlPoints[k], out var min, out var max, out var codes);
            result[k] = new double[codes.Length];
            for (var i = 0; i < codes.Length; i++)
            {
                // As the decoder reads it: min + (max - min) * (code / 65535), in floats
                result[k][i] = min + (max - min) * (codes[i] / 65535.0f);
            }
        }

        return result;
    }

    private static void Quantize16(double[] values, out float min, out float max, out ushort[] codes)
    {
        double lo = double.MaxValue, hi = double.MinValue;
        foreach (var v in values)
        {
            lo = Math.Min(lo, v);
            hi = Math.Max(hi, v);
        }

        min = (float)lo;
        max = (float)hi;
        codes = new ushort[values.Length];
        var range = (double)max - min;
        for (var i = 0; i < values.Length; i++)
        {
            codes[i] = range > 0 ? (ushort)Math.Clamp(Math.Round((values[i] - min) / range * 65535), 0, 65535) : (ushort)0;
        }
    }

    private static double ScalarError(double[][] samples, double[] times, List<int> moving, int degree,
        byte[] knots, double[][] stored)
    {
        var basis = new double[degree + 1];
        double worst = 0;
        for (var k = 0; k < moving.Count; k++)
        {
            var s = samples[moving[k]];
            for (var f = 0; f < s.Length; f++)
            {
                // In the decoder's floats: at the thousands of units of an AC6 map a float's step is 0.0002 or
                // more, which alone can use up a sub-millimetre tolerance
                worst = Math.Max(worst, Math.Abs(EvaluateAsDecoder(degree, (float)times[f], knots, stored[k]) - s[f]));
            }
        }

        return worst;
    }

    private static double RotationError(double[][] samples, double[] times, List<int> moving, int degree,
        byte[] knots, double[][] stored)
    {
        var basis = new double[degree + 1];
        double worst = 0;
        for (var f = 0; f < samples[0].Length; f++)
        {
            var x = Evaluate(degree, times[f], knots, stored[0], basis);
            var y = Evaluate(degree, times[f], knots, stored[1], basis);
            var z = Evaluate(degree, times[f], knots, stored[2], basis);
            var w = Evaluate(degree, times[f], knots, stored[3], basis);
            // Only the direction counts: the decoder blends without normalizing, so even vanilla rotations are a
            // little short of unit length between their keys. A blend near zero, though, has no direction to trust.
            var length = Math.Sqrt(x * x + y * y + z * z + w * w);
            if (length < 0.5)
            {
                return double.MaxValue;
            }

            worst = Math.Max(worst, Angle(x, y, z, w, samples[0][f], samples[1][f], samples[2][f], samples[3][f]));
        }

        return worst;
    }

    /// <summary>
    ///     The angle between two rotations, in doubles: acos of a dot near 1 loses everything below ~1e-3 rad in
    ///     floats, so this takes 2·atan2(|a − b|, |a + b|) of the unit quaternions in the same hemisphere.
    /// </summary>
    public static double Angle(double ax, double ay, double az, double aw, double bx, double by, double bz, double bw)
    {
        var la = Math.Sqrt(ax * ax + ay * ay + az * az + aw * aw);
        var lb = Math.Sqrt(bx * bx + by * by + bz * bz + bw * bw);
        if (la < 1e-12 || lb < 1e-12)
        {
            return Math.PI;
        }

        ax /= la;
        ay /= la;
        az /= la;
        aw /= la;
        var sign = ax * bx + ay * by + az * bz + aw * bw < 0 ? -1.0 / lb : 1.0 / lb;
        bx *= sign;
        by *= sign;
        bz *= sign;
        bw *= sign;

        double dx = ax - bx, dy = ay - by, dz = az - bz, dw = aw - bw;
        double sx = ax + bx, sy = ay + by, sz = az + bz, sw = aw + bw;
        return 2 * Math.Atan2(Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw),
            Math.Sqrt(sx * sx + sy * sy + sz * sz + sw * sw));
    }

    private static double Average(double[] values)
    {
        double sum = 0;
        foreach (var v in values)
        {
            sum += v;
        }

        return sum / values.Length;
    }

    private static byte[] QuaternionBytes(float x, float y, float z, float w)
    {
        var bytes = new byte[16];
        BitConverter.TryWriteBytes(bytes.AsSpan(0), x);
        BitConverter.TryWriteBytes(bytes.AsSpan(4), y);
        BitConverter.TryWriteBytes(bytes.AsSpan(8), z);
        BitConverter.TryWriteBytes(bytes.AsSpan(12), w);
        return bytes;
    }
}
