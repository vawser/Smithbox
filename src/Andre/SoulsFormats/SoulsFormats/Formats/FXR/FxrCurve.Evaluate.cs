using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Curve evaluation (FX Editor Plan §10.3), ported from fxr-main 32.1.3's <c>valueAt</c>: Stepped, Linear,
    /// Bezier, Hermite (fxr-main's approximation) and ComponentHermite, with loop. Modifiers are not applied here,
    /// as in fxr-main. Evaluate the curve as stored (<see cref="Decode"/>), not one scaled for display:
    /// <see cref="ScaleComponent"/> also scales Hermite tangents, which are angles.
    /// </summary>
    public partial class FxrCurve
    {
        /// <summary>The curve's length: the last keyframe position (0 for constants).</summary>
        public float SequenceDuration
        {
            get
            {
                float d = 0;
                if (Function == FxrPropertyFunction.ComponentHermite)
                {
                    if (ComponentKeyframes != null)
                        foreach (var list in ComponentKeyframes)
                            foreach (var k in list)
                                d = Math.Max(d, k.Position);
                }
                else
                {
                    foreach (var k in Keyframes)
                        d = Math.Max(d, k.Position);
                }
                return d;
            }
        }

        /// <summary>The value at <paramref name="arg"/>, one float per component.</summary>
        public float[] Evaluate(float arg)
        {
            var result = new float[Components];
            EvaluateInto(arg, result);
            return result;
        }

        /// <summary>The value at <paramref name="arg"/>, written into <paramref name="result"/> (Components long).</summary>
        public void EvaluateInto(float arg, float[] result)
        {
            int c = Components;
            switch (Function)
            {
                case FxrPropertyFunction.Zero:
                case FxrPropertyFunction.One:
                case FxrPropertyFunction.Constant:
                    for (int i = 0; i < c; i++)
                        result[i] = Constant != null && i < Constant.Length ? Constant[i] : 0;
                    return;

                case FxrPropertyFunction.ComponentHermite:
                {
                    if (Loop)
                    {
                        float duration = SequenceDuration;
                        if (duration == 0)
                        {
                            // As fxr-main and the games: a looping curve with no length is NaN
                            Array.Fill(result, float.NaN);
                            return;
                        }
                        arg %= duration;
                    }
                    for (int i = 0; i < c; i++)
                    {
                        var keys = ComponentKeyframes?[i];
                        if (keys == null || keys.Count == 0) { result[i] = 0; continue; }

                        // Each component is a non-looping Hermite curve of its own
                        float compDuration = 0;
                        foreach (var k in keys) compDuration = Math.Max(compDuration, k.Position);
                        result[i] = arg > compDuration ? keys[keys.Count - 1].Value[0] : Hermite(keys, arg, 0);
                    }
                    return;
                }

                default:
                {
                    if (Keyframes.Count == 0)
                    {
                        Array.Fill(result, 0);
                        return;
                    }

                    if (Loop)
                    {
                        float duration = SequenceDuration;
                        if (duration == 0)
                        {
                            Array.Fill(result, float.NaN);
                            return;
                        }
                        arg %= duration;
                    }
                    else if (arg > SequenceDuration)
                    {
                        Copy(Keyframes[Keyframes.Count - 1].Value, result, c);
                        return;
                    }

                    for (int i = 0; i < c; i++)
                    {
                        result[i] = Function switch
                        {
                            FxrPropertyFunction.Stepped => Stepped(Keyframes, arg, i),
                            FxrPropertyFunction.Linear => Linear(Keyframes, arg, i),
                            FxrPropertyFunction.Bezier => Bezier(Keyframes, arg, i),
                            FxrPropertyFunction.Hermite => Hermite(Keyframes, arg, i),
                            _ => 0,
                        };
                    }
                    return;
                }
            }
        }

        private static void Copy(float[] from, float[] to, int c)
        {
            for (int i = 0; i < c; i++)
                to[i] = from != null && i < from.Length ? from[i] : 0;
        }

        // ---------------------------------------------------------------- fxr-main's interpolation

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>
        /// The keyframes either side of a position. As fxr-main, a keyframe placed before an earlier one is skipped.
        /// </summary>
        private static void Surrounding(List<FxrKeyframe> keys, float position, out FxrKeyframe prev, out FxrKeyframe next)
        {
            prev = null;
            next = null;
            float max = float.NegativeInfinity;
            foreach (var k in keys)
            {
                if (k.Position < max)
                    continue;
                max = k.Position;

                if (k.Position <= position)
                {
                    prev = k;
                }
                else
                {
                    next = k;
                    break;
                }
            }
        }

        private static float Stepped(List<FxrKeyframe> keys, float position, int i)
        {
            if (keys.Count == 1)
                return keys[0].Value[i];

            FxrKeyframe nearest = null;
            foreach (var k in keys)
            {
                if (k.Position <= position) nearest = k;
                else break;
            }
            // Before the first key fxr-main has no value; the first key's is the sensible one
            return (nearest ?? keys[0]).Value[i];
        }

        private static float Linear(List<FxrKeyframe> keys, float position, int i)
        {
            if (keys.Count == 1)
                return keys[0].Value[i];

            Surrounding(keys, position, out var prev, out var next);
            if (prev == null) return next.Value[i];
            if (next == null) return prev.Value[i];

            float t = (position - prev.Position) / (next.Position - prev.Position);
            return Lerp(prev.Value[i], next.Value[i], t);
        }

        private static float CubicBezier(float p0, float p1, float p2, float p3, float t)
        {
            float k = 1 - t;
            return k * (k * k * p0 + 3 * t * (k * p1 + t * p2)) + t * t * t * p3;
        }

        /// <summary>The previous key's first control slope and the next key's second, each a third of the way.</summary>
        private static float Bezier(List<FxrKeyframe> keys, float position, int i)
        {
            if (keys.Count == 1)
                return keys[0].Value[i];

            Surrounding(keys, position, out var prev, out var next);
            if (prev == null) return next.Value[i];
            if (next == null) return prev.Value[i];

            float x = (position - prev.Position) / (next.Position - prev.Position);
            float v1 = prev.Value[i], v2 = next.Value[i];
            float t1 = prev.In?[i] ?? 0, t2 = next.Out?[i] ?? 0;
            return CubicBezier(v1, v1 + t1 / 3, v2 - t2 / 3, v2, x);
        }

        /// <summary>
        /// fxr-main's approximation of the games' "Curve2" interpolation: the previous key's two tangent angles pick a
        /// blend of nine easing curves. Both angles come from the previous key.
        /// </summary>
        private static float Hermite(List<FxrKeyframe> keys, float position, int i)
        {
            if (keys.Count == 1)
                return keys[0].Value[i];

            Surrounding(keys, position, out var prev, out var next);
            if (prev == null) return next.Value[i];
            if (next == null) return prev.Value[i];

            float x = (position - prev.Position) / (next.Position - prev.Position);
            float t1 = (prev.In?[i] ?? 0) / MathF.PI * 2;
            float t2 = (prev.Out?[i] ?? 0) / MathF.PI * 2;
            return Lerp(prev.Value[i], next.Value[i], ApproxHermite(t1, t2, x));
        }

        private static readonly Func<float, float>[] HermiteApprox =
        {
            CssCubicBezier(0.3f, 0.1f, 0.7f, 0.9f),     CssCubicBezier(0.135f, 0.135f, 0.525f, 1), CssCubicBezier(0.015f, 0.675f, 0.33f, 1),
            CssCubicBezier(0.475f, 0, 0.865f, 0.865f),  x => x,                                     CssCubicBezier(0.015f, 0.9f, 0.5f, 0.5f),
            CssCubicBezier(0.71f, 0, 0.985f, 0.37f),    CssCubicBezier(0.525f, 0.525f, 0.965f, 0.07f), CssCubicBezier(0.065f, 1.4f, 0.935f, -0.4f),
        };

        private static float ApproxHermite(float t1, float t2, float x)
        {
            float t1x2 = t1 * 2, t2x2 = t2 * 2;
            int ix = Math.Clamp((int)MathF.Floor(t1x2), 0, 1);
            int iy = Math.Clamp((int)MathF.Floor(t2x2), 0, 1);
            int n = ix + 3 * iy;
            float t1x2f = t1x2 - ix;
            return Lerp(
                Lerp(HermiteApprox[n](x), HermiteApprox[n + 1](x), t1x2f),
                Lerp(HermiteApprox[n + 3](x), HermiteApprox[n + 4](x), t1x2f),
                t2x2 - iy);
        }

        // A CSS-style cubic-bezier easing (x1, y1, x2, y2), solved for t by sampling, then Newton-Raphson or bisection

        private static float CalcBezier(float a, float b, float t) => t * ((t + 3 * (a - b) * t + (3 * b - 6 * a)) * t + 3 * a);
        private static float GetSlope(float a, float b, float t) => 3 * (t * (t + 3 * (a - b) * t + 2 * (b - 2 * a)) + a);

        private static Func<float, float> CssCubicBezier(float x1, float y1, float x2, float y2)
        {
            if (x1 == y1 && x2 == y2)
                return x => x;

            const int samplesCount = 11;
            const float step = 1f / (samplesCount - 1);
            const int last = samplesCount - 1;
            var samples = new float[samplesCount];
            for (int s = last; s >= 0; s--)
                samples[s] = CalcBezier(x1, x2, s * step);

            float GetTForX(float x)
            {
                float intervalStart = 0;
                int s = 1;
                for (; s != last && samples[s] <= x; s++)
                    intervalStart += step;
                s--;

                float t = intervalStart + (x - samples[s]) * step / (samples[s + 1] - samples[s]);
                float slope = GetSlope(x1, x2, t);
                if (slope == 0)
                    return t;
                if (slope >= 1e-3f)
                {
                    for (int n = 4; n > 0; n--)
                    {
                        float cur = GetSlope(x1, x2, t);
                        if (cur == 0) return t;
                        t -= (CalcBezier(x1, x2, t) - x) / cur;
                    }
                    return t;
                }

                // Binary subdivision
                float a = intervalStart, b = intervalStart + step, currentX;
                int iter = 0;
                do
                {
                    t = a + (b - a) / 2;
                    currentX = CalcBezier(x1, x2, t) - x;
                    if (currentX > 0) b = t; else a = t;
                } while (MathF.Abs(currentX) > 1e-7f && ++iter < 10);
                return t;
            }

            return x => x == 0 || x == 1 ? x : CalcBezier(y1, y2, GetTForX(x));
        }
    }
}
