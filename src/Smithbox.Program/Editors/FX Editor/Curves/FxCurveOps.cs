using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Curve and modifier edits for the Curve Editor (FX Editor Plan, phase E3), on <see cref="FxrCurve"/>s in display
/// units as <see cref="FxrActionView.GetCurve"/> gives them. SoulsFormats and BCL only, so
/// Claude/Tools/FxCurveEditCheck can link it and check every edit against the writer.
/// </summary>
public static class FxCurveOps
{
    // ---------------------------------------------------------------- functions

    /// <summary>
    /// The functions a game's files use (FxrRoundTrip survey): Hermite in DS3, SDT, ER and AC6; ComponentHermite in
    /// AC6 and NR. Zero and One are written for constants of all 0 or all 1 by <see cref="Normalize"/>.
    /// </summary>
    public static IReadOnlyList<FxrPropertyFunction> Functions(FxrGame game)
    {
        var list = new List<FxrPropertyFunction>
        {
            FxrPropertyFunction.Constant, FxrPropertyFunction.Stepped, FxrPropertyFunction.Linear, FxrPropertyFunction.Bezier,
        };
        if (game is FxrGame.DS3 or FxrGame.SDT or FxrGame.ER or FxrGame.AC6)
            list.Add(FxrPropertyFunction.Hermite);
        if (game is FxrGame.AC6 or FxrGame.NR)
            list.Add(FxrPropertyFunction.ComponentHermite);
        return list;
    }

    public static bool IsKeyed(FxrPropertyFunction f) => f > FxrPropertyFunction.Constant;

    /// <summary>
    /// The curve with another function. Keys carry over (tangents start at 0 where the new function needs them);
    /// a constant becomes one key at 0; keys become a constant with the value at 0. ComponentHermite splits the
    /// keys per component, and leaves it merged back at every position any component had.
    /// </summary>
    public static FxrCurve Convert(FxrCurve curve, FxrPropertyFunction to)
    {
        int c = curve.Components;
        if (to == curve.Function || to <= FxrPropertyFunction.Constant && curve.Function <= FxrPropertyFunction.Constant)
        {
            var same = FxrActionView.Clone(curve);
            if (to <= FxrPropertyFunction.Constant) { same.Function = FxrPropertyFunction.Constant; same.Constant ??= Value(curve, 0); }
            return same;
        }

        var result = new FxrCurve { Function = to, ValueType = curve.ValueType, Loop = curve.Loop };

        if (to <= FxrPropertyFunction.Constant)
        {
            result.Function = FxrPropertyFunction.Constant;
            result.Constant = Value(curve, 0);
            result.Loop = false;
            return result;
        }

        // The keys as one list with every component
        List<FxrKeyframe> keys;
        if (curve.Function <= FxrPropertyFunction.Constant)
        {
            keys = new() { new FxrKeyframe { Position = 0, Value = Value(curve, 0) } };
        }
        else if (curve.Function == FxrPropertyFunction.ComponentHermite)
        {
            var positions = curve.ComponentKeyframes.SelectMany(l => l.Select(k => k.Position)).Distinct().OrderBy(p => p).ToList();
            if (positions.Count == 0) positions.Add(0);
            keys = positions.Select(p => new FxrKeyframe { Position = p, Value = curve.Evaluate(p) }).ToList();
        }
        else
        {
            keys = curve.Keyframes.Select(k => new FxrKeyframe
            {
                Position = k.Position,
                Value = (float[])k.Value.Clone(),
                In = (float[])k.In?.Clone(),
                Out = (float[])k.Out?.Clone(),
            }).ToList();
        }

        bool tangents = to is FxrPropertyFunction.Bezier or FxrPropertyFunction.Hermite;
        foreach (var k in keys)
        {
            // Bezier slopes and Hermite angles don't mean the same, so a change between them starts flat
            bool keepTangents = tangents && curve.Function == to;
            k.In = tangents ? keepTangents && k.In != null ? k.In : new float[c] : null;
            k.Out = tangents ? keepTangents && k.Out != null ? k.Out : new float[c] : null;
        }

        if (to == FxrPropertyFunction.ComponentHermite)
        {
            result.ComponentKeyframes = new List<FxrKeyframe>[c];
            for (int i = 0; i < c; i++)
                result.ComponentKeyframes[i] = keys.Select(k => new FxrKeyframe
                {
                    Position = k.Position,
                    Value = new[] { k.Value[i] },
                    In = new[] { 0f },
                    Out = new[] { 0f },
                }).ToList();
            result.Duration = keys.Max(k => k.Position);
        }
        else
        {
            result.Keyframes = keys;
        }
        return result;
    }

    /// <summary>The curve's value at a position (all components), modifiers left out.</summary>
    public static float[] Value(FxrCurve curve, float position)
    {
        if (curve.Function <= FxrPropertyFunction.Constant)
            return (float[])(curve.Constant?.Clone() ?? new float[curve.Components]);
        return curve.Evaluate(position);
    }

    /// <summary>Constants of all 0 or all 1 are stored as Zero or One, as vanilla does.</summary>
    public static void Normalize(FxrCurve curve)
    {
        if (curve.Function is not (FxrPropertyFunction.Constant or FxrPropertyFunction.Zero or FxrPropertyFunction.One) || curve.Constant == null)
            return;
        // Vanilla keeps Constant for a constant that happens to be 0 too; only an explicit Zero/One stays one
        if (curve.Function != FxrPropertyFunction.Constant)
        {
            float want = curve.Function == FxrPropertyFunction.Zero ? 0 : 1;
            if (curve.Constant.Any(v => v != want))
                curve.Function = FxrPropertyFunction.Constant;
        }
    }

    // ---------------------------------------------------------------- keys

    /// <summary>The key list of a component: the shared list, or ComponentHermite's own.</summary>
    public static List<FxrKeyframe> Keys(FxrCurve curve, int component) =>
        curve.Function == FxrPropertyFunction.ComponentHermite ? curve.ComponentKeyframes[component] : curve.Keyframes;

    /// <summary>Adds a key at a position with the curve's value there, keeping the keys in position order.</summary>
    public static void AddKey(FxrCurve curve, int component, float position)
    {
        var value = curve.Evaluate(position);
        bool tangents = curve.Function is FxrPropertyFunction.Bezier or FxrPropertyFunction.Hermite or FxrPropertyFunction.ComponentHermite;
        var list = Keys(curve, component);
        int width = curve.Function == FxrPropertyFunction.ComponentHermite ? 1 : curve.Components;
        var key = new FxrKeyframe
        {
            Position = position,
            Value = curve.Function == FxrPropertyFunction.ComponentHermite ? new[] { value[component] } : value,
            In = tangents ? new float[width] : null,
            Out = tangents ? new float[width] : null,
        };
        int at = list.FindIndex(k => k.Position > position);
        list.Insert(at < 0 ? list.Count : at, key);
        UpdateDuration(curve);
    }

    /// <summary>Removes a key; the last key of a list stays, as a curve needs one.</summary>
    public static bool RemoveKey(FxrCurve curve, int component, int index)
    {
        var list = Keys(curve, component);
        if (list.Count <= 1 || index < 0 || index >= list.Count)
            return false;
        list.RemoveAt(index);
        UpdateDuration(curve);
        return true;
    }

    /// <summary>Puts keys back in position order after a position was edited (stable for equal positions).</summary>
    public static void SortKeys(FxrCurve curve)
    {
        if (curve.Function == FxrPropertyFunction.ComponentHermite)
        {
            for (int i = 0; i < curve.ComponentKeyframes.Length; i++)
                curve.ComponentKeyframes[i] = curve.ComponentKeyframes[i].OrderBy(k => k.Position).ToList();
        }
        else if (IsKeyed(curve.Function))
        {
            curve.Keyframes = curve.Keyframes.OrderBy(k => k.Position).ToList();
        }
        UpdateDuration(curve);
    }

    /// <summary>ComponentHermite stores its length; it grows to cover the last key (it is never shortened).</summary>
    private static void UpdateDuration(FxrCurve curve)
    {
        if (curve.Function == FxrPropertyFunction.ComponentHermite)
            curve.Duration = Math.Max(curve.Duration, curve.SequenceDuration);
    }

    // ---------------------------------------------------------------- Hermite tangents

    /// <summary>
    /// Hermite and ComponentHermite tangents are angles, but <see cref="FxrActionView.GetCurve"/> scales them with
    /// the values. For editing and evaluating they're put back as stored; <see cref="ToDisplay"/> undoes this
    /// before <see cref="FxrActionView.SetCurve"/>.
    /// </summary>
    public static FxrCurve ToEditable(FxrCurve display, float[] factors)
    {
        var c = FxrActionView.Clone(display);
        ScaleTangents(c, factors, inverse: true);
        return c;
    }

    public static FxrCurve ToDisplay(FxrCurve editable, float[] factors)
    {
        var c = FxrActionView.Clone(editable);
        ScaleTangents(c, factors, inverse: false);
        return c;
    }

    private static void ScaleTangents(FxrCurve c, float[] factors, bool inverse)
    {
        if (c.Function is not (FxrPropertyFunction.Hermite or FxrPropertyFunction.ComponentHermite))
            return;
        for (int i = 0; i < c.Components; i++)
        {
            float f = i < factors.Length ? factors[i] : 1;
            if (f == 1 || f == 0) continue;
            float s = inverse ? 1 / f : f;
            foreach (var k in Keys(c, i))
            {
                int at = c.Function == FxrPropertyFunction.ComponentHermite ? 0 : i;
                if (k.In != null) k.In[at] *= s;
                if (k.Out != null) k.Out[at] *= s;
            }
        }
    }

    // ---------------------------------------------------------------- modifiers

    public const int RandomDelta = 21, RandomRange = 24, ExternalValue1 = 38, ExternalValue2 = 39, RandomFraction = 53;

    /// <summary>fxr-main's per-game TypeEnumB table (before the value type is added). DS3 has no RandomRange or ExternalValue2.</summary>
    private static readonly Dictionary<FxrGame, Dictionary<int, int>> EnumB = new()
    {
        [FxrGame.DS3] = new() { [RandomDelta] = 0, [ExternalValue1] = 4, [RandomFraction] = 8 },
        [FxrGame.SDT] = new() { [RandomDelta] = 0, [RandomRange] = 4, [ExternalValue1] = 8, [RandomFraction] = 12 },
        [FxrGame.ER] = new() { [RandomDelta] = 0, [RandomRange] = 4, [ExternalValue1] = 8, [ExternalValue2] = 12, [RandomFraction] = 16 },
        [FxrGame.AC6] = new() { [RandomDelta] = 0, [RandomRange] = 4, [ExternalValue1] = 8, [ExternalValue2] = 12, [RandomFraction] = 16 },
        [FxrGame.NR] = new() { [RandomDelta] = 0, [RandomRange] = 4, [ExternalValue1] = 8, [ExternalValue2] = 12, [RandomFraction] = 16 },
    };

    /// <summary>The modifier types a game supports, in fxr-main's order.</summary>
    public static IEnumerable<int> ModifierTypes(FxrGame game) =>
        new[] { RandomDelta, RandomRange, RandomFraction, ExternalValue1, ExternalValue2 }.Where(t => EnumB[game].ContainsKey(t));

    public static string ModifierName(int type) => type switch
    {
        RandomDelta => "RandomDelta",
        RandomRange => "RandomRange",
        RandomFraction => "RandomFraction",
        ExternalValue1 => "ExternalValue1",
        ExternalValue2 => "ExternalValue2",
        _ => type.ToString(),
    };

    public static bool IsRandom(int type) => type is RandomDelta or RandomRange or RandomFraction;

    /// <summary>
    /// A new modifier for a property of a value type, as fxr-main writes one. Random modifiers get seeds from
    /// <paramref name="random"/> and a range of 0 (no effect yet); external value modifiers read external value 0
    /// with a factor of 1 at 0 and 1 (no effect yet).
    /// </summary>
    public static FxrModifier CreateModifier(FxrGame game, int type, FxrValueType valueType, Random random)
    {
        int c = (int)valueType + 1;
        var mod = new FxrModifier
        {
            TypeEnumA = (ushort)(((type >> 4) | 0b1100) << 12 | (type & 0xF) << 4 | (int)valueType),
            TypeEnumB = EnumB[game][type] | (int)valueType,
        };

        uint F(float f) => BitConverter.SingleToUInt32Bits(f);
        switch (type)
        {
            case RandomDelta:
            case RandomFraction:
                for (int i = 0; i < c; i++) mod.Fields.Add((uint)random.Next());
                for (int i = 0; i < c; i++) mod.Fields.Add(F(0));
                break;
            case RandomRange:
                for (int i = 0; i < c; i++) mod.Fields.Add((uint)random.Next());
                for (int i = 0; i < c; i++) mod.Fields.Add(F(0));
                for (int i = 0; i < c; i++) mod.Fields.Add(F(0));
                break;
            case ExternalValue1:
            case ExternalValue2:
            {
                mod.Fields.Add(0);
                var factor = new FxrCurve
                {
                    Function = FxrPropertyFunction.Linear,
                    ValueType = valueType,
                    Keyframes =
                    {
                        new FxrKeyframe { Position = 0, Value = Enumerable.Repeat(1f, c).ToArray() },
                        new FxrKeyframe { Position = 1, Value = Enumerable.Repeat(1f, c).ToArray() },
                    },
                };
                var prop = new FxrProperty();
                factor.EncodeInto(prop);
                prop.TypeEnumB = game is FxrGame.DS3 or FxrGame.SDT or FxrGame.ER ? prop.PackTypeEnumB() : 0;
                mod.Properties.Add(prop);
                break;
            }
        }
        return mod;
    }

    /// <summary>
    /// A random modifier's ranges in display units: RandomDelta's max, RandomRange's min and max (scaled with the
    /// property, as they add to it); RandomFraction's max is a fraction and isn't scaled. Each array has one value
    /// per component.
    /// </summary>
    public static List<float[]> GetRanges(FxrModifier m, float[] factors)
    {
        int c = (int)m.ValueType + 1;
        int groups = m.ModifierType == RandomRange ? 2 : IsRandom(m.ModifierType) ? 1 : 0;
        bool scaled = m.ModifierType != RandomFraction;
        var result = new List<float[]>();
        for (int g = 0; g < groups; g++)
        {
            var v = new float[c];
            for (int i = 0; i < c; i++)
            {
                int at = c * (1 + g) + i;
                float raw = at < m.Fields.Count ? BitConverter.UInt32BitsToSingle(m.Fields[at]) : 0;
                v[i] = scaled ? raw * Factor(factors, i) : raw;
            }
            result.Add(v);
        }
        return result;
    }

    /// <summary>Sets a range group from display units (see <see cref="GetRanges"/>).</summary>
    public static void SetRange(FxrModifier m, int group, float[] display, float[] factors)
    {
        int c = (int)m.ValueType + 1;
        bool scaled = m.ModifierType != RandomFraction;
        for (int i = 0; i < c && i < display.Length; i++)
        {
            int at = c * (1 + group) + i;
            float f = Factor(factors, i);
            float raw = scaled && f != 0 ? display[i] / f : display[i];
            while (m.Fields.Count <= at) m.Fields.Add(0);
            m.Fields[at] = BitConverter.SingleToUInt32Bits(raw);
        }
    }

    public static uint[] GetSeeds(FxrModifier m)
    {
        int c = (int)m.ValueType + 1;
        return Enumerable.Range(0, c).Select(i => i < m.Fields.Count ? m.Fields[i] : 0u).ToArray();
    }

    public static void SetSeed(FxrModifier m, int component, uint seed)
    {
        while (m.Fields.Count <= component) m.Fields.Add(0);
        m.Fields[component] = seed;
    }

    public static int GetExternalId(FxrModifier m) => m.Fields.Count > 0 ? (int)m.Fields[0] : 0;

    public static void SetExternalId(FxrModifier m, int id)
    {
        if (m.Fields.Count == 0) m.Fields.Add(0);
        m.Fields[0] = (uint)id;
    }

    private static float Factor(float[] factors, int i) => factors != null && i < factors.Length ? factors[i] : 1;
}
