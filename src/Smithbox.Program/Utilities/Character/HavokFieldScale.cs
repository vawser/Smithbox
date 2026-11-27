#nullable enable
using Havok;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Utilities;

/// <summary>How a Havok field changes when a character is scaled by s (Character Scale Plan.md §4.1)</summary>
public enum HavokScaleOp
{
    /// <summary>A float × s</summary>
    Length,

    /// <summary>A vector's X, Y, Z × s; W kept (a tag, a turn or padding)</summary>
    Xyz,

    /// <summary>All four components × s</summary>
    Xyzw,

    /// <summary>W × s only (a plane's distance, a sphere's radius); XYZ kept (a normal, or a centre scaled apart)</summary>
    W,

    /// <summary>A matrix's translation (M41, M42, M43) × s; the 3x3 kept</summary>
    Translation
}

/// <summary>One field to scale: on objects of a class (by its name in every SDK), a path of fields below it</summary>
/// <param name="Path">
///     Field names from the class, "[]" after a list or array to go through its items:
///     <c>m_referencePose[].m_translation</c>
/// </param>
public sealed record HavokScaleRule(string ClassName, string Path, HavokScaleOp Op);

/// <summary>
///     Scales Havok fields by rules, by reflection over the generated classes, so one rule covers every SDK
///     (hk2014 to hk2019) that names the field the same. Embedded structs and value types are written back to
///     their owner. Values that are not finite, or are placeholders (|v| ≥ 1e30, such as FLT_MAX limits), are left.
/// </summary>
public static class HavokFieldScale
{
    /// <summary>Every rule applied to every object of a document. Returns how many values changed.</summary>
    public static int Apply(HavokDocument document, IEnumerable<HavokScaleRule> rules, float scale)
    {
        var byClass = rules.GroupBy(r => r.ClassName).ToDictionary(g => g.Key, g => g.ToList());
        var changed = 0;
        foreach (var obj in document.Objects)
        {
            if (!byClass.TryGetValue(obj.GetType().Name, out var classRules))
                continue;

            foreach (var rule in classRules)
                changed += ApplyPath(obj, rule.Path.Split('.'), 0, rule.Op, scale);
        }

        return changed;
    }

    // Scales the value at path[at..] below owner, writing value types back. Returns the values changed.
    private static int ApplyPath(object owner, string[] path, int at, HavokScaleOp op, float scale)
    {
        var segment = path[at];
        var isList = segment.EndsWith("[]", StringComparison.Ordinal);
        var name = isList ? segment[..^2] : segment;
        var field = FindField(owner.GetType(), name);
        if (field == null)
            return 0;

        var value = field.GetValue(owner);
        if (value == null)
            return 0;

        var last = at == path.Length - 1;
        if (isList)
        {
            if (value is not IList list)
                return 0;

            var changed = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item == null)
                    continue;

                if (last)
                {
                    if (TryScale(item, op, scale, out var scaled))
                    {
                        list[i] = scaled;
                        changed++;
                    }
                }
                else
                {
                    changed += ApplyPath(item, path, at + 1, op, scale);
                    if (item.GetType().IsValueType)
                        list[i] = item;
                }
            }

            return changed;
        }

        if (last)
        {
            if (!TryScale(value, op, scale, out var scaled))
                return 0;

            if (!field.IsInitOnly)
                field.SetValue(owner, scaled);
            return 1;
        }

        var count = ApplyPath(value, path, at + 1, op, scale);
        if (value.GetType().IsValueType && !field.IsInitOnly)
            field.SetValue(owner, value);
        return count;
    }

    private static FieldInfo? FindField(Type type, string name) =>
        type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

    /// <summary>A value scaled by an op; false when the type doesn't fit the op</summary>
    public static bool TryScale(object value, HavokScaleOp op, float s, out object scaled)
    {
        scaled = value;
        switch (value)
        {
            case float f when op == HavokScaleOp.Length:
                scaled = Mul(f, s);
                return true;
            case Vector4 v:
                scaled = op switch
                {
                    HavokScaleOp.Xyz => new Vector4(Mul(v.X, s), Mul(v.Y, s), Mul(v.Z, s), v.W),
                    HavokScaleOp.Xyzw => new Vector4(Mul(v.X, s), Mul(v.Y, s), Mul(v.Z, s), Mul(v.W, s)),
                    HavokScaleOp.W => v with { W = Mul(v.W, s) },
                    _ => v
                };
                return op is HavokScaleOp.Xyz or HavokScaleOp.Xyzw or HavokScaleOp.W;
            case Vector3 v when op is HavokScaleOp.Xyz or HavokScaleOp.Xyzw:
                scaled = new Vector3(Mul(v.X, s), Mul(v.Y, s), Mul(v.Z, s));
                return true;
            case Matrix4x4 m when op == HavokScaleOp.Translation:
                m.M41 = Mul(m.M41, s);
                m.M42 = Mul(m.M42, s);
                m.M43 = Mul(m.M43, s);
                scaled = m;
                return true;
            default:
                return false;
        }
    }

    /// <summary>v × s, leaving values that are not lengths (NaN, infinities, FLT_MAX-style placeholders)</summary>
    public static float Mul(float v, float s) => float.IsFinite(v) && MathF.Abs(v) < 1e30f ? v * s : v;
}
