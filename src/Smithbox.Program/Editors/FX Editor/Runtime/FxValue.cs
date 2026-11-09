using SoulsFormats;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.FxEditor;

// The FX runtime (FX Editor Plan §10.2–10.3). Everything in Runtime/ uses only SoulsFormats and the BCL, so
// Claude/Tools/FxRuntimeSurvey compiles the same files.

/// <summary>What a property's curve is evaluated at (the schema's <c>argument</c>).</summary>
public enum FxArgument
{
    Constant0,
    ActiveTime,
    ParticleAge,
    EmissionTime,
}

/// <summary>The times a property can be driven by, and the external values its modifiers read.</summary>
public struct FxEvalContext
{
    /// <summary>The node's active time (seconds since its delay ended).</summary>
    public float ActiveTime;

    /// <summary>The particle's age, for particle properties.</summary>
    public float ParticleAge;

    /// <summary>The node's active time when the particle (or node) was emitted.</summary>
    public float EmissionTime;

    /// <summary>The per-instance random seed the random modifiers are keyed by.</summary>
    public uint Seed;

    public Func<int, float> External;

    public float Argument(FxArgument a) => a switch
    {
        FxArgument.ActiveTime => ActiveTime,
        FxArgument.ParticleAge => ParticleAge,
        FxArgument.EmissionTime => EmissionTime,
        _ => 0,
    };
}

/// <summary>
/// A property ready to evaluate: its stored curve, the display-unit factor per component (degrees, mirrored X, ...),
/// its argument and its modifiers. Values come out in fxr-main's units, like the inspector shows them.
/// </summary>
public sealed class FxValue
{
    public readonly FxrCurve Curve;
    public readonly float[] Factors;
    public readonly FxArgument Argument;
    public readonly List<Modifier> Modifiers = new();

    public int Components => Curve.Components;

    /// <summary>True when the value never changes: a constant with no modifiers.</summary>
    public bool IsConstant => Curve.Function <= FxrPropertyFunction.Constant && Modifiers.Count == 0;

    public sealed class Modifier
    {
        public int Type;
        public uint[] Seeds;
        public float[] A, B;
        public int ExternalId;
        public FxrCurve Factor;
    }

    private FxValue(FxrCurve curve, float[] factors, FxArgument argument)
    {
        Curve = curve;
        Factors = factors;
        Argument = argument;
    }

    public static FxValue Constant(params float[] value)
    {
        var type = (FxrValueType)Math.Clamp(value.Length - 1, 0, 3);
        var curve = new FxrCurve { Function = FxrPropertyFunction.Constant, ValueType = type, Constant = (float[])value.Clone() };
        var factors = new float[value.Length];
        Array.Fill(factors, 1f);
        return new FxValue(curve, factors, FxArgument.Constant0);
    }

    /// <summary>A property of an action (its schema default when it isn't stored), null when it can't be read.</summary>
    public static FxValue From(FxrActionView view, string name)
    {
        try
        {
            var prop = view.GetProperty(name) ?? view.DefaultProperty(name);
            if (prop == null)
                return null;

            var curve = FxrCurve.Decode(prop);
            var conversions = view.ConversionsOf(name);
            var factors = new float[curve.Components];
            for (int i = 0; i < factors.Length; i++)
                factors[i] = (float)FxrConversions.CurveFactor(conversions, i);

            var argument = Enum.TryParse<FxArgument>(view.Describe(name)?.Argument, out var a) ? a : FxArgument.Constant0;
            var value = new FxValue(curve, factors, argument);

            foreach (var m in prop.Modifiers)
                value.Modifiers.Add(ReadModifier(m, curve.Components));

            return value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Modifier fields (fxr-main): RandomDelta and RandomFraction are seeds then max; RandomRange is seeds, min, max;
    /// ExternalValue1/2 are the external value id and a factor property.
    /// </summary>
    private static Modifier ReadModifier(FxrModifier m, int c)
    {
        var mod = new Modifier { Type = m.ModifierType };
        var w = m.Fields;
        float F(int i) => i < w.Count ? BitConverter.UInt32BitsToSingle(w[i]) : 0;

        switch (mod.Type)
        {
            case 21: // RandomDelta
            case 53: // RandomFraction
                mod.Seeds = new uint[c]; mod.A = new float[c];
                for (int i = 0; i < c; i++) { mod.Seeds[i] = i < w.Count ? w[i] : 0; mod.A[i] = F(c + i); }
                break;
            case 24: // RandomRange
                mod.Seeds = new uint[c]; mod.A = new float[c]; mod.B = new float[c];
                for (int i = 0; i < c; i++) { mod.Seeds[i] = i < w.Count ? w[i] : 0; mod.A[i] = F(c + i); mod.B[i] = F(2 * c + i); }
                break;
            case 38: // ExternalValue1
            case 39: // ExternalValue2
                mod.ExternalId = w.Count > 0 ? (int)w[0] : 0;
                if (m.Properties.Count > 0)
                    mod.Factor = FxrCurve.Decode(m.Properties[0]);
                break;
        }
        return mod;
    }

    // ---------------------------------------------------------------- evaluation

    public float Scalar(in FxEvalContext ctx)
    {
        Span<float> v = stackalloc float[4];
        Evaluate(ctx, v);
        return v[0];
    }

    public System.Numerics.Vector3 Vector3(in FxEvalContext ctx)
    {
        Span<float> v = stackalloc float[4];
        Evaluate(ctx, v);
        return new System.Numerics.Vector3(v[0], v[1], v[2]);
    }

    public System.Numerics.Vector4 Vector4(in FxEvalContext ctx)
    {
        Span<float> v = stackalloc float[4];
        Evaluate(ctx, v);
        return new System.Numerics.Vector4(v[0], v[1], v[2], v[3]);
    }

    [ThreadStatic] private static float[] _scratch;
    [ThreadStatic] private static float[] _factorScratch;

    /// <summary>The value in display units. <paramref name="result"/> must hold at least 4 floats.</summary>
    public void Evaluate(in FxEvalContext ctx, Span<float> result)
    {
        _scratch ??= new float[4];
        int c = Components;

        Curve.EvaluateInto(ctx.Argument(Argument), _scratch);
        for (int i = 0; i < 4; i++)
            result[i] = i < c ? _scratch[i] : 0;

        foreach (var m in Modifiers)
        {
            switch (m.Type)
            {
                case 21: // RandomDelta: base ± max
                    for (int i = 0; i < c; i++)
                        result[i] += m.A[i] * Signed(ctx.Seed, m.Seeds[i]);
                    break;
                case 24: // RandomRange: base + [min, max]
                    for (int i = 0; i < c; i++)
                        result[i] += m.A[i] + (m.B[i] - m.A[i]) * Unit(ctx.Seed, m.Seeds[i]);
                    break;
                case 53: // RandomFraction: base ± base × max
                    for (int i = 0; i < c; i++)
                        result[i] += result[i] * m.A[i] * Signed(ctx.Seed, m.Seeds[i]);
                    break;
                case 38: // ExternalValue1/2: × factor(external value)
                case 39:
                    if (m.Factor != null && ctx.External != null)
                    {
                        _factorScratch ??= new float[4];
                        m.Factor.EvaluateInto(ctx.External(m.ExternalId), _factorScratch);
                        for (int i = 0; i < c; i++)
                            result[i] *= m.Factor.Components == 1 ? _factorScratch[0] : _factorScratch[i];
                    }
                    break;
            }
        }

        // Display units last: the random ranges are stored in the same units as the curve
        for (int i = 0; i < c; i++)
            result[i] *= Factors[i];
    }

    // ---------------------------------------------------------------- randoms

    /// <summary>A stable hash of the instance seed and a modifier seed, in [0, 1).</summary>
    public static float Unit(uint instance, uint seed)
    {
        uint h = Hash(instance * 0x9E3779B9u ^ seed);
        return (h >> 8) * (1f / 16777216f);
    }

    /// <summary>The same, in [-1, 1).</summary>
    public static float Signed(uint instance, uint seed) => Unit(instance, seed) * 2 - 1;

    public static uint Hash(uint x)
    {
        x ^= x >> 16; x *= 0x7FEB352Du;
        x ^= x >> 15; x *= 0x846CA68Bu;
        x ^= x >> 16;
        return x;
    }
}

/// <summary>A small deterministic random source (xorshift), so a replay from time 0 gives the same effect.</summary>
public sealed class FxRandom
{
    private uint _state;

    public FxRandom(uint seed) { _state = seed == 0 ? 0x6D2B79F5u : seed; }

    public uint NextUInt()
    {
        uint x = _state;
        x ^= x << 13; x ^= x >> 17; x ^= x << 5;
        return _state = x;
    }

    /// <summary>[0, 1).</summary>
    public float Next() => (NextUInt() >> 8) * (1f / 16777216f);

    /// <summary>[-1, 1).</summary>
    public float NextSigned() => Next() * 2 - 1;
}
