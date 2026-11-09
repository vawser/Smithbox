using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public class FxrKeyframe
    {
        public float Position { get; set; }

        /// <summary>One value per component.</summary>
        public float[] Value { get; set; }

        /// <summary>Bezier: first control point. Hermite: incoming tangent. Null otherwise.</summary>
        public float[] In { get; set; }

        /// <summary>Bezier: second control point. Hermite: outgoing tangent. Null otherwise.</summary>
        public float[] Out { get; set; }
    }

    /// <summary>
    /// A property's value decoded from its raw fields (FXR Format Plan §2.4): a constant, or keyframes
    /// for Stepped/Linear/Bezier/Hermite, or one scalar keyframe list per component for AC6's
    /// ComponentHermite.
    /// </summary>
    public partial class FxrCurve
    {
        public FxrPropertyFunction Function { get; set; }
        public FxrValueType ValueType { get; set; }
        public bool Loop { get; set; }

        public int Components => (int)ValueType + 1;

        /// <summary>Zero, One and Constant: the value.</summary>
        public float[] Constant { get; set; }

        /// <summary>Stepped, Linear, Bezier and Hermite.</summary>
        public List<FxrKeyframe> Keyframes { get; set; } = new();

        /// <summary>ComponentHermite: one scalar keyframe list per component.</summary>
        public List<FxrKeyframe>[] ComponentKeyframes { get; set; }

        /// <summary>ComponentHermite: the curve's duration (first field).</summary>
        public float Duration { get; set; }

        /// <summary>The stored per-component bounds (vanilla: min/max of the keyframe values).</summary>
        public float[] Min { get; set; }
        public float[] Max { get; set; }

        private static float F(List<uint> w, int i) => BitConverter.UInt32BitsToSingle(w[i]);
        private static uint W(float f) => BitConverter.SingleToUInt32Bits(f);

        /// <summary>True when both curves have the same function, type, loop flag and keyframes (bounds ignored).</summary>
        public bool SameShape(FxrCurve o)
        {
            if (o == null || Function != o.Function || ValueType != o.ValueType || Loop != o.Loop || Duration != o.Duration) return false;
            static bool Arr(float[] a, float[] b) => a == null ? b == null : b != null && a.AsSpan().SequenceEqual(b);
            static bool List(List<FxrKeyframe> a, List<FxrKeyframe> b)
            {
                if (a.Count != b.Count) return false;
                for (int i = 0; i < a.Count; i++)
                    if (a[i].Position != b[i].Position || !Arr(a[i].Value, b[i].Value) || !Arr(a[i].In, b[i].In) || !Arr(a[i].Out, b[i].Out)) return false;
                return true;
            }
            if (!Arr(Constant, o.Constant) || !List(Keyframes, o.Keyframes)) return false;
            if (ComponentKeyframes == null || o.ComponentKeyframes == null) return ComponentKeyframes == o.ComponentKeyframes;
            if (ComponentKeyframes.Length != o.ComponentKeyframes.Length) return false;
            for (int i = 0; i < ComponentKeyframes.Length; i++)
                if (!List(ComponentKeyframes[i], o.ComponentKeyframes[i])) return false;
            return true;
        }

        public static FxrCurve Decode(FxrProperty p)
        {
            var curve = new FxrCurve { Function = p.Function, ValueType = p.ValueType, Loop = p.Loop };
            int c = curve.Components;
            var w = p.Fields;
            switch (p.Function)
            {
                case FxrPropertyFunction.Zero:
                    curve.Constant = new float[c];
                    break;
                case FxrPropertyFunction.One:
                    curve.Constant = new float[c];
                    Array.Fill(curve.Constant, 1f);
                    break;
                case FxrPropertyFunction.Constant:
                    curve.Constant = new float[c];
                    for (int i = 0; i < c; i++) curve.Constant[i] = F(w, i);
                    break;
                case FxrPropertyFunction.Stepped:
                case FxrPropertyFunction.Linear:
                case FxrPropertyFunction.Bezier:
                case FxrPropertyFunction.Hermite:
                {
                    int n = (int)w[0];
                    curve.Min = Slice(w, 1, c);
                    curve.Max = Slice(w, 1 + c, c);
                    int pos = 1 + 2 * c, val = pos + n, a = val + n * c, b = a + n * c;
                    bool extra = p.Function is FxrPropertyFunction.Bezier or FxrPropertyFunction.Hermite;
                    for (int k = 0; k < n; k++)
                        curve.Keyframes.Add(new FxrKeyframe
                        {
                            Position = F(w, pos + k),
                            Value = Slice(w, val + k * c, c),
                            In = extra ? Slice(w, a + k * c, c) : null,
                            Out = extra ? Slice(w, b + k * c, c) : null,
                        });
                    break;
                }
                case FxrPropertyFunction.ComponentHermite:
                {
                    curve.Duration = F(w, 0);
                    curve.Min = Slice(w, 1 + c, c);
                    curve.Max = Slice(w, 1 + 2 * c, c);
                    curve.ComponentKeyframes = new List<FxrKeyframe>[c];
                    int at = 1 + 3 * c;
                    for (int i = 0; i < c; i++)
                    {
                        int n = (int)w[1 + i];
                        var list = new List<FxrKeyframe>(n);
                        for (int k = 0; k < n; k++)
                            list.Add(new FxrKeyframe
                            {
                                Position = F(w, at + k),
                                Value = new[] { F(w, at + n + k) },
                                In = new[] { F(w, at + 2 * n + k) },
                                Out = new[] { F(w, at + 3 * n + k) },
                            });
                        curve.ComponentKeyframes[i] = list;
                        at += 4 * n;
                    }
                    break;
                }
                default:
                    throw new NotSupportedException($"Unknown property function {(int)p.Function}.");
            }
            return curve;
        }

        private static float[] Slice(List<uint> w, int start, int count)
        {
            var r = new float[count];
            for (int i = 0; i < count; i++) r[i] = F(w, start + i);
            return r;
        }

        /// <summary>
        /// Writes this curve into the property's function, type, loop flag and fields. With
        /// <paramref name="recomputeBounds"/> the stored min/max are recomputed from the keyframes, as
        /// fxr-main and (per the Phase 3 survey) the game files do; otherwise the decoded bounds are kept.
        /// TypeEnumB is not touched.
        /// </summary>
        public void EncodeInto(FxrProperty p, bool recomputeBounds = true)
        {
            int c = Components;
            p.Function = Function;
            p.ValueType = ValueType;
            p.Loop = Loop;
            var w = p.Fields;
            w.Clear();
            switch (Function)
            {
                case FxrPropertyFunction.Zero:
                case FxrPropertyFunction.One:
                    break;
                case FxrPropertyFunction.Constant:
                    for (int i = 0; i < c; i++) w.Add(W(Constant[i]));
                    break;
                case FxrPropertyFunction.Stepped:
                case FxrPropertyFunction.Linear:
                case FxrPropertyFunction.Bezier:
                case FxrPropertyFunction.Hermite:
                {
                    if (recomputeBounds || Min == null || Max == null) ComputeBounds();
                    w.Add((uint)Keyframes.Count);
                    foreach (var v in Min) w.Add(W(v));
                    foreach (var v in Max) w.Add(W(v));
                    foreach (var k in Keyframes) w.Add(W(k.Position));
                    foreach (var k in Keyframes) foreach (var v in k.Value) w.Add(W(v));
                    if (Function is FxrPropertyFunction.Bezier or FxrPropertyFunction.Hermite)
                    {
                        foreach (var k in Keyframes) for (int i = 0; i < c; i++) w.Add(W(k.In?[i] ?? 0));
                        foreach (var k in Keyframes) for (int i = 0; i < c; i++) w.Add(W(k.Out?[i] ?? 0));
                    }
                    break;
                }
                case FxrPropertyFunction.ComponentHermite:
                {
                    if (recomputeBounds || Min == null || Max == null) ComputeBounds();
                    w.Add(W(Duration));
                    foreach (var list in ComponentKeyframes) w.Add((uint)list.Count);
                    foreach (var v in Min) w.Add(W(v));
                    foreach (var v in Max) w.Add(W(v));
                    foreach (var list in ComponentKeyframes)
                    {
                        foreach (var k in list) w.Add(W(k.Position));
                        foreach (var k in list) w.Add(W(k.Value[0]));
                        foreach (var k in list) w.Add(W(k.In?[0] ?? 0));
                        foreach (var k in list) w.Add(W(k.Out?[0] ?? 0));
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// ComponentHermite bounds: min = max = each component's last keyframe value (all AC6 files, most NR
        /// files) instead of the true min/max (other NR files). FxrActionView sets this from the file.
        /// </summary>
        public bool ComponentBoundsFromLast { get; set; } = true;

        /// <summary>
        /// The bounds as the game files store them (Phase 3 survey):
        /// - Stepped/Linear/Hermite, NR ComponentHermite: per-component min/max of the keyframe values,
        ///   starting from the first value and replaced only on a strict comparison (so the sign of a
        ///   zero is whichever came first, as in vanilla).
        /// - ComponentHermite: per file, see <see cref="ComponentBoundsFromLast"/> (all AC6 and most NR files use the last value).
        /// - Bezier: vanilla stores something else (curve extents, sometimes stale); the min/max rule is
        ///   the best known approximation.
        /// </summary>
        public void ComputeBounds()
        {
            int c = Components;
            Min = new float[c];
            Max = new float[c];
            for (int i = 0; i < c; i++)
            {
                List<FxrKeyframe> keys = Function == FxrPropertyFunction.ComponentHermite ? ComponentKeyframes[i] : Keyframes;
                int idx = Function == FxrPropertyFunction.ComponentHermite ? 0 : i;
                if (keys.Count == 0) continue;
                if (Function == FxrPropertyFunction.ComponentHermite && ComponentBoundsFromLast)
                {
                    Min[i] = Max[i] = keys[^1].Value[0];
                    continue;
                }
                float min = keys[0].Value[idx], max = min;
                for (int k = 1; k < keys.Count; k++)
                {
                    float v = keys[k].Value[idx];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
                Min[i] = min;
                Max[i] = max;
            }
        }

        /// <summary>
        /// Multiplies every value of a component (keyframe values, tangents/control points, bounds and
        /// constants) by a factor. Used for display conversions; positions are not touched.
        /// </summary>
        public void ScaleComponent(int component, double factor)
        {
            float f = (float)factor;
            if (Constant != null) Constant[component] *= f;
            if (Function == FxrPropertyFunction.ComponentHermite)
            {
                foreach (var k in ComponentKeyframes[component]) ScaleKey(k, 0, f);
            }
            else
            {
                foreach (var k in Keyframes) ScaleKey(k, component, f);
            }
            if (Min != null && Max != null)
            {
                float a = Min[component] * f, b = Max[component] * f;
                Min[component] = Math.Min(a, b);
                Max[component] = Math.Max(a, b);
            }
        }

        private static void ScaleKey(FxrKeyframe k, int i, float f)
        {
            k.Value[i] *= f;
            if (k.In != null) k.In[i] *= f;
            if (k.Out != null) k.Out[i] *= f;
        }
    }
}
