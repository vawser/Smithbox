#nullable enable
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The character creator's maths, with no tie to a project or a param, so a tool can use the same code
/// (Claude/Tools/FaceGenSurvey links this file):
/// <list type="bullet">
/// <item><see cref="Build"/>: slider values → the FaceGen model's coefficients, as the game builds a face.</item>
/// <item><see cref="Fit"/>: coefficients → slider values, the other way. Used to turn a face into another param's or
/// game's sliders (blending, conversion, fitting).</item>
/// </list>
/// The forward maths follows FaceSouls (github.com/lexlayer93/FaceSouls), MIT licence, Copyright (c) lexlayer93: a
/// control moves the face along its direction to its value; age and gender ("neutral") each keep the other where it
/// is; caricature scales how far the face is from the average, outside the age/gender plane. Race "All", as the
/// creators use.
/// <para>
/// Slider IDs, as the slider tables use them: 10 age, 20 gender, 30 caricature, 1NN shape control NN, 2NN texture
/// control NN.
/// </para>
/// </summary>
public static class FaceGenSolver
{
    public const int AgeSlider = 10;
    public const int GenderSlider = 20;
    public const int CaricatureSlider = 30;

    /// <summary>Whether a slider ID is a linear control of the given side: 1NN for shape, 2NN for texture</summary>
    public static bool IsControl(int id, bool texture) => texture ? id is >= 200 and < 300 : id is >= 100 and < 200;

    // ---- forward

    /// <summary>
    /// The coefficients of one side (shape or texture) of a face, from its slider values: from zero, each slider of
    /// <paramref name="sequence"/> is applied in order, then the coefficients are clamped to ±<paramref name="clamp"/>.
    /// <paramref name="value"/> gives each slider's value (age in years, a control's −10…10 and so on); a hidden
    /// slider's is 0. Null if the CTL has no race statistics.
    /// </summary>
    public static float[]? Build(CTL ctl, bool texture, IReadOnlyList<int> sequence, Func<int, float> value, float clamp)
    {
        if (ctl.AllRaces is not { } race)
            return null;

        var side = Side.Of(ctl, race, texture);
        var s = new float[side.Modes];

        foreach (var id in sequence)
            Apply(side, s, id, value(id));

        for (var i = 0; i < s.Length; i++)
            s[i] = Math.Clamp(s[i], -clamp, clamp);

        return s;
    }

    /// <summary>The CTL's terms for one side of the model</summary>
    private sealed record Side(int Modes, List<CTL.LinearControl> Controls, float[] Age, float[] Gender,
        float AgeOffset, float GenderOffset, float[] Mean, float[] Density, bool Texture)
    {
        public static Side Of(CTL ctl, CTL.Race race, bool texture) => texture
            ? new Side(ctl.TextureSymmetricModes, ctl.TextureSymmetricControls, race.Age.TextureCoefficients,
                race.Gender.TextureCoefficients, race.Age.TextureOffset, race.Gender.TextureOffset,
                race.TextureMean, race.TextureDensity, true)
            : new Side(ctl.GeometrySymmetricModes, ctl.GeometrySymmetricControls, race.Age.GeometryCoefficients,
                race.Gender.GeometryCoefficients, race.Age.GeometryOffset, race.Gender.GeometryOffset,
                race.GeometryMean, race.GeometryDensity, false);
    }

    private static void Apply(Side side, float[] s, int id, float value)
    {
        switch (id)
        {
            case AgeSlider:
                SetAgeGender(s, side.Age, side.Gender, side.AgeOffset, side.GenderOffset, value, age: true);
                break;
            case GenderSlider:
                SetAgeGender(s, side.Age, side.Gender, side.AgeOffset, side.GenderOffset, value, age: false);
                break;
            case CaricatureSlider:
                SetCaricature(s, side.Age, side.Gender, side.Mean, side.Density, value);
                break;
            default:
                if (IsControl(id, side.Texture) && id % 100 < side.Controls.Count)
                    SetControl(s, side.Controls[id % 100].Coefficients, value);
                break;
        }
    }

    /// <summary>The apparent age (or gender) of a face: how the age (gender) control reads it</summary>
    public static float? Read(CTL ctl, bool texture, float[] s, bool age)
    {
        if (ctl.AllRaces is not { } race)
            return null;

        var side = Side.Of(ctl, race, texture);

        return age ? Dot(side.Age, s) + side.AgeOffset : Dot(side.Gender, s) + side.GenderOffset;
    }

    // ---- inverse

    /// <summary>One side's fit: each slider's value, and how far the face built from them is from the target</summary>
    /// <param name="Values">Each slider's value by ID, snapped to what a byte holds (hidden sliders 0). On the texture
    /// side, 30 is the texture caricature.</param>
    /// <param name="Rms">RMS distance of the built face's coefficients from the target's</param>
    /// <param name="TargetRms">The target's RMS distance from zero, for scale</param>
    public sealed record FitResult(Dictionary<int, float> Values, float Rms, float TargetRms);

    /// <summary>A face's fit: the shape side, and the texture side if one was asked for</summary>
    public sealed record FaceFit(FitResult Shape, FitResult? Texture);

    /// <summary>
    /// Slider values that build (by <see cref="Build"/>) the face closest to <paramref name="shape"/> and, if given,
    /// <paramref name="texture"/>.
    /// <para>
    /// Every creator step is affine in the face, and its linear part doesn't depend on the slider's value
    /// (<c>SetControl</c> is <c>(I − ccᵀ)s + q·c</c>). Age, gender and caricature come first in every game's order, so
    /// each side is <c>base(age, gender, caricature) + A·q</c> for its controls' values q, with A fixed. So:
    /// </para>
    /// <list type="bullet">
    /// <item>For given age, gender and caricature, each side's q is a box-bounded least squares: Cholesky, and if that
    /// leaves a range, coordinate descent within the ranges. There are more controls than modes, so a small pull
    /// towards 0 (neutral) picks the smallest of the settings that fit.</item>
    /// <item>Age and gender, which both sides share, and each side's caricature are searched over their ranges (golden
    /// section, a few rounds) for the lowest summed cost, starting from how the shape reads.</item>
    /// </list>
    /// Values are snapped to what a byte holds before the result is scored. <paramref name="range"/> gives a slider's
    /// range (caricature's for both sides), or null for a hidden one (left at 0). Null if the CTL has no race
    /// statistics.
    /// </summary>
    public static FaceFit? Fit(CTL ctl, IReadOnlyList<int> sequence, Func<int, (float Min, float Max)?> range,
        float shapeClamp, float textureClamp, float[] shape, float[]? texture = null)
    {
        if (ctl.AllRaces == null)
            return null;

        var sides = new List<SideFit> { new(ctl, false, sequence, range, shape, shapeClamp) };

        if (texture != null)
            sides.Add(new SideFit(ctl, true, sequence, range, texture, textureClamp));

        // The sliders searched: (ID, the sides it is set on, bounds)
        var searched = new List<(int Id, List<SideFit> On, float Lo, float Hi)>();

        void Search(int id, List<SideFit> on, float? start)
        {
            if (!sequence.Contains(id) || range(id) is not { } r || r.Min == r.Max)
                return;

            var lo = MathF.Min(r.Min, r.Max);
            var hi = MathF.Max(r.Min, r.Max);
            var value = Math.Clamp(start ?? (lo + hi) * 0.5f, lo, hi);

            foreach (var side in on)
                side.Values[id] = value;

            searched.Add((id, on, lo, hi));
        }

        Search(AgeSlider, sides, Read(ctl, false, shape, age: true));
        Search(GenderSlider, sides, Read(ctl, false, shape, age: false));

        foreach (var side in sides)
            Search(CaricatureSlider, new List<SideFit> { side }, 1f);

        float Cost() => sides.Sum(side => side.Cost());

        // A few rounds of one slider at a time
        const int Rounds = 3;

        for (var round = 0; round < Rounds && searched.Count > 0; round++)
        {
            foreach (var (id, on, lo, hi) in searched)
            {
                var best = GoldenSection(v =>
                {
                    foreach (var side in on)
                        side.Values[id] = v;
                    return Cost();
                }, lo, hi);

                foreach (var side in on)
                    side.Values[id] = best;
            }
        }

        var results = sides.Select(side => side.Finish(range)).ToList();

        return new FaceFit(results[0], results.Count > 1 ? results[1] : null);
    }

    /// <summary>One side of a fit: its target, its control problem, and the slider values being searched</summary>
    private sealed class SideFit
    {
        private readonly CTL Ctl;
        private readonly bool Texture;
        private readonly IReadOnlyList<int> Sequence;
        private readonly float[] Target;
        private readonly float Clamp;
        private readonly List<int> Controls;
        private readonly ControlProblem Problem;
        private float[] Q;

        public readonly Dictionary<int, float> Values = new();

        public SideFit(CTL ctl, bool texture, IReadOnlyList<int> sequence, Func<int, (float Min, float Max)?> range,
            float[] target, float clamp)
        {
            Ctl = ctl;
            Texture = texture;
            Sequence = sequence;
            Target = target;
            Clamp = clamp;
            Controls = sequence.Where(id => IsControl(id, texture) && range(id) is { } r && r.Min != r.Max).Distinct().ToList();
            Problem = new ControlProblem(ctl, texture, sequence, Controls, range);
            Q = new float[Controls.Count];

            foreach (var id in sequence)
                Values[id] = 0;
        }

        /// <summary>The cost for the current age, gender and caricature, with the controls solved for them</summary>
        public float Cost()
        {
            var (q, cost) = Problem.Solve(Values, Target, Q);
            Q = q;
            return cost;
        }

        /// <summary>Snaps the searched sliders, solves the controls for them, snaps those, and scores the face</summary>
        public FitResult Finish(Func<int, (float Min, float Max)?> range)
        {
            foreach (var id in new[] { AgeSlider, GenderSlider, CaricatureSlider })
            {
                if (Values.ContainsKey(id))
                    Values[id] = Snap(range, id, Values[id]);
            }

            var (q, _) = Problem.Solve(Values, Target, Q);

            for (var k = 0; k < Controls.Count; k++)
                Values[Controls[k]] = Snap(range, Controls[k], q[k]);

            var built = Build(Ctl, Texture, Sequence, id => Values.GetValueOrDefault(id), Clamp)!;

            return new FitResult(new Dictionary<int, float>(Values), Rms(built, Target), Rms(new float[Target.Length], Target));
        }
    }

    /// <summary>A value as the nearest a byte holds on its slider; 0 for a hidden one</summary>
    private static float Snap(Func<int, (float Min, float Max)?> range, int id, float v)
    {
        if (range(id) is not { } r || r.Min == r.Max)
            return 0;

        var b = MathF.Round(Math.Clamp((v - r.Min) / (r.Max - r.Min), 0f, 1f) * 255f);

        return r.Min + (r.Max - r.Min) * b / 255f;
    }

    /// <summary>
    /// The controls' part of a fit: the fixed matrix A (each control's effect on the finished face), and the
    /// box-bounded least squares for q given the other sliders
    /// </summary>
    private sealed class ControlProblem
    {
        private const double Ridge = 1e-3;
        private const int MaxSweeps = 400;

        private readonly CTL Ctl;
        private readonly bool Texture;
        private readonly IReadOnlyList<int> Sequence;
        private readonly List<int> Controls;
        private readonly double[] Lo;
        private readonly double[] Hi;

        // A's columns (one per control), and AᵀA + λI with its Cholesky factor
        private readonly float[][] Columns;
        private readonly double[,] Normal;
        private readonly double[,]? Factor;

        public ControlProblem(CTL ctl, bool texture, IReadOnlyList<int> sequence, List<int> controls,
            Func<int, (float Min, float Max)?> range)
        {
            Ctl = ctl;
            Texture = texture;
            Sequence = sequence;
            Controls = controls;

            var n = controls.Count;
            Lo = new double[n];
            Hi = new double[n];

            for (var k = 0; k < n; k++)
            {
                var r = range(controls[k])!.Value;
                Lo[k] = Math.Min(r.Min, r.Max);
                Hi[k] = Math.Max(r.Min, r.Max);
            }

            // A doesn't depend on the other sliders: measured once, from all zero
            var zero = new Dictionary<int, float>();
            var at = BuildWith(zero, new float[n]);
            Columns = new float[n][];

            for (var k = 0; k < n; k++)
            {
                var unit = new float[n];
                unit[k] = 1f;
                var moved = BuildWith(zero, unit);
                Columns[k] = new float[at.Length];
                for (var i = 0; i < at.Length; i++)
                    Columns[k][i] = moved[i] - at[i];
            }

            Normal = new double[n, n];

            for (var a = 0; a < n; a++)
            {
                for (var b = a; b < n; b++)
                {
                    double sum = 0;
                    for (var i = 0; i < at.Length; i++)
                        sum += Columns[a][i] * Columns[b][i];
                    Normal[a, b] = Normal[b, a] = sum;
                }

                Normal[a, a] += Ridge;
            }

            Factor = Cholesky(Normal);
        }

        /// <summary>The unclamped face for the other sliders' values and the controls' values</summary>
        private float[] BuildWith(Dictionary<int, float> others, float[] q)
        {
            var map = new Dictionary<int, float>(others);
            for (var k = 0; k < Controls.Count; k++)
                map[Controls[k]] = q[k];

            return Build(Ctl, Texture, Sequence, id => map.GetValueOrDefault(id), float.MaxValue)!;
        }

        /// <summary>
        /// The controls' values for the other sliders' values (controls in <paramref name="others"/> are ignored),
        /// and the cost |A·q − r|² + λ|q|². <paramref name="start"/> is where the bounded search starts.
        /// </summary>
        public (float[] Q, float Cost) Solve(Dictionary<int, float> others, float[] target, float[] start)
        {
            var n = Controls.Count;
            var baseFace = BuildWith(others, new float[n]);
            var m = baseFace.Length;

            var residual = new double[m];
            for (var i = 0; i < m; i++)
                residual[i] = target[i] - baseFace[i];

            var g = new double[n];
            for (var k = 0; k < n; k++)
            {
                double sum = 0;
                for (var i = 0; i < m; i++)
                    sum += Columns[k][i] * residual[i];
                g[k] = sum;
            }

            // Unbounded first; kept if every control is within its range
            var q = Factor != null ? CholeskySolve(Factor, g) : new double[n];
            var inside = Factor != null && Enumerable.Range(0, n).All(k => q[k] >= Lo[k] && q[k] <= Hi[k]);

            if (!inside)
            {
                // Coordinate descent on ½qᵀNq − gᵀq within the box, from the given start
                for (var k = 0; k < n; k++)
                    q[k] = Math.Clamp(k < start.Length ? start[k] : 0, Lo[k], Hi[k]);

                for (var sweep = 0; sweep < MaxSweeps; sweep++)
                {
                    double largest = 0;

                    for (var k = 0; k < n; k++)
                    {
                        var sum = g[k];
                        for (var j = 0; j < n; j++)
                        {
                            if (j != k)
                                sum -= Normal[k, j] * q[j];
                        }

                        var next = Math.Clamp(sum / Normal[k, k], Lo[k], Hi[k]);
                        largest = Math.Max(largest, Math.Abs(next - q[k]));
                        q[k] = next;
                    }

                    if (largest < 1e-5)
                        break;
                }
            }

            // |A·q − r|² + λ|q|²
            double cost = 0;
            for (var i = 0; i < m; i++)
            {
                var e = -residual[i];
                for (var k = 0; k < n; k++)
                    e += Columns[k][i] * q[k];
                cost += e * e;
            }

            for (var k = 0; k < n; k++)
                cost += Ridge * q[k] * q[k];

            return (q.Select(v => (float)v).ToArray(), (float)cost);
        }
    }

    /// <summary>The Cholesky factor of a symmetric positive definite matrix, or null if it isn't</summary>
    private static double[,]? Cholesky(double[,] a)
    {
        var n = a.GetLength(0);
        var l = new double[n, n];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                var sum = a[i, j];
                for (var k = 0; k < j; k++)
                    sum -= l[i, k] * l[j, k];

                if (i == j)
                {
                    if (sum <= 0)
                        return null;
                    l[i, i] = Math.Sqrt(sum);
                }
                else
                {
                    l[i, j] = sum / l[j, j];
                }
            }
        }

        return l;
    }

    /// <summary>Solves L·Lᵀ·x = b</summary>
    private static double[] CholeskySolve(double[,] l, double[] b)
    {
        var n = b.Length;
        var y = new double[n];

        for (var i = 0; i < n; i++)
        {
            var sum = b[i];
            for (var k = 0; k < i; k++)
                sum -= l[i, k] * y[k];
            y[i] = sum / l[i, i];
        }

        var x = new double[n];

        for (var i = n - 1; i >= 0; i--)
        {
            var sum = y[i];
            for (var k = i + 1; k < n; k++)
                sum -= l[k, i] * x[k];
            x[i] = sum / l[i, i];
        }

        return x;
    }

    /// <summary>The minimum of a function over [lo, hi], by golden-section search</summary>
    private static float GoldenSection(Func<float, float> f, float lo, float hi, int iterations = 24)
    {
        const float InvPhi = 0.618034f;

        var a = lo;
        var b = hi;
        var c = b - InvPhi * (b - a);
        var d = a + InvPhi * (b - a);
        var fc = f(c);
        var fd = f(d);

        for (var i = 0; i < iterations; i++)
        {
            if (fc < fd)
            {
                b = d;
                d = c;
                fd = fc;
                c = b - InvPhi * (b - a);
                fc = f(c);
            }
            else
            {
                a = c;
                c = d;
                fc = fd;
                d = a + InvPhi * (b - a);
                fd = f(d);
            }
        }

        return (a + b) * 0.5f;
    }

    private static float Rms(float[] a, float[] b)
    {
        var sum = 0f;
        for (var i = 0; i < a.Length && i < b.Length; i++)
            sum += (a[i] - b[i]) * (a[i] - b[i]);
        return MathF.Sqrt(sum / Math.Max(1, a.Length));
    }

    // ---- the creator's steps

    private static float Dot(float[] a, float[] b)
    {
        var sum = 0f;
        for (var i = 0; i < a.Length && i < b.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    /// <summary>Moves the face along a control's direction until the control reads <paramref name="value"/></summary>
    private static void SetControl(float[] s, float[] c, float value)
    {
        var delta = value - Dot(c, s);

        for (var i = 0; i < s.Length && i < c.Length; i++)
            s[i] += delta * c[i];
    }

    /// <summary>
    /// Sets the apparent age (or gender) without changing the other: solves the age/gender covariance for the
    /// amounts of each direction to move by
    /// </summary>
    private static void SetAgeGender(float[] s, float[] a, float[] g, float ageOffset, float genderOffset, float value, bool age)
    {
        float aa = Dot(a, a), ag = Dot(a, g), gg = Dot(g, g);
        var det = aa * gg - ag * ag;

        if (MathF.Abs(det) < 1e-12f)
            return;

        var current = age ? Dot(a, s) + ageOffset : Dot(g, s) + genderOffset;
        var change = value - current;

        // The inverse of [[aa, ag], [ag, gg]] times (change, 0) or (0, change)
        float af, gf;

        if (age)
        {
            af = gg / det * change;
            gf = -ag / det * change;
        }
        else
        {
            af = -ag / det * change;
            gf = aa / det * change;
        }

        for (var i = 0; i < s.Length && i < a.Length && i < g.Length; i++)
            s[i] += af * a[i] + gf * g[i];
    }

    /// <summary>
    /// Sets how far the face is from the average, outside the age/gender plane (measured through the race's density
    /// matrix), keeping its age and gender
    /// </summary>
    private static void SetCaricature(float[] s, float[] age, float[] gender, float[] mean, float[] density, float value)
    {
        value = MathF.Max(0.01f, value);

        var n = s.Length;

        if (mean.Length < n || density.Length < n * n || n < 3 || age.Length < n || gender.Length < n)
            return;

        // An orthonormal pair spanning age and gender
        var ua = Normalize(age.Take(n).ToArray());
        var ug = gender.Take(n).ToArray();
        var proj = Dot(ua, ug);
        for (var i = 0; i < n; i++)
            ug[i] -= ua[i] * proj;
        ug = Normalize(ug);

        var dev = new float[n];
        for (var i = 0; i < n; i++)
            dev[i] = s[i] - mean[i];

        float da = Dot(ua, dev), dg = Dot(ug, dev);
        var agDev = new float[n];
        var rest = new float[n];

        for (var i = 0; i < n; i++)
        {
            agDev[i] = ua[i] * da + ug[i] * dg;
            rest[i] = dev[i] - agDev[i];
        }

        // |density · rest| / sqrt(n - 2)
        var norm = 0f;

        for (var r = 0; r < n; r++)
        {
            var q = 0f;
            for (var c = 0; c < n; c++)
                q += density[r * n + c] * rest[c];
            norm += q * q;
        }

        var current = MathF.Sqrt(norm) / MathF.Sqrt(n - 2);

        if (current <= 0)
            return;

        var scale = value / current;

        for (var i = 0; i < n; i++)
            s[i] = mean[i] + scale * rest[i] + agDev[i];
    }

    private static float[] Normalize(float[] v)
    {
        var length = MathF.Sqrt(Dot(v, v));

        if (length > 0)
        {
            for (var i = 0; i < v.Length; i++)
                v[i] /= length;
        }

        return v;
    }
}
