#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>How the Randomise tool changes a face</summary>
public sealed class FaceRandomiseOptions
{
    /// <summary>0 to 1: how far the values move</summary>
    public float Strength = 0.5f;

    /// <summary>True: around the face's own values (a variation). False: the FaceGen sliders around neutral.</summary>
    public bool AroundCurrent = true;

    /// <summary>Whether part fields (hair, beard, brows…) may change</summary>
    public bool Parts = true;

    /// <summary>Whether colours (skin, hair, eyes…) may change</summary>
    public bool Colours = true;

    /// <summary>The groups (by title) left as they are</summary>
    public HashSet<string> LockedGroups = new();
}

/// <summary>
/// Random values for a face row's fields, by the kinds the Properties window edits (<see cref="FaceGenLayout"/>):
/// <list type="bullet">
/// <item>FaceGen sliders (age, gender, caricature, the shape and texture controls; FaceGenParam's modes): normally
/// distributed bytes, around neutral (128) or around the face's own. Hidden sliders (in the creator's order but not its
/// menu) are left alone.</item>
/// <item>Other amounts (gloss, root darkness, makeup): around the face's own value, as they have no neutral.</item>
/// <item>Colours: hue, saturation and brightness moved a little around the face's own colour.</item>
/// <item>Parts: with a chance that grows with the strength, another of the values that name a model.</item>
/// </list>
/// IDs, flags and the other plain fields are never changed, nor are locked groups.
/// </summary>
public static class FaceRandomiser
{
    // How far the bytes move at full strength (one standard deviation)
    private const float NeutralSpread = 64f;
    private const float VariationSpread = 32f;
    private const float AmountSpread = 40f;

    // Colours at full strength: hue (0-1 around), saturation and value
    private const float HueSpread = 0.04f;
    private const float SaturationSpread = 0.12f;
    private const float ValueSpread = 0.12f;

    /// <summary>The fields to write, with their new values; only fields whose value changes</summary>
    public static List<(string Field, object Value)> Generate(ProjectEntry project, Param param, Param.Row row,
        IReadOnlyList<FaceFieldGroup> groups, FaceRandomiseOptions options, Random random, bool isFemale)
    {
        var values = new List<(string, object)>();
        var strength = Math.Clamp(options.Strength, 0f, 1f);

        if (strength <= 0)
            return values;

        var storesModes = FaceGenParams.StoresModes(param);

        foreach (var group in groups)
        {
            if (options.LockedGroups.Contains(group.Title))
                continue;

            foreach (var field in group.Fields)
            {
                switch (field.Kind)
                {
                    case FaceFieldKind.Slider:
                        if (SliderByte(project, param, row, field.Name, storesModes, options.AroundCurrent, strength, random) is { } b)
                            Add(values, row, field.Name, b);
                        break;

                    case FaceFieldKind.Color when options.Colours:
                        var (r, g, bl) = Colour(row, field, strength, random);
                        Add(values, row, field.Name, r);
                        Add(values, row, field.Green!, g);
                        Add(values, row, field.Blue!, bl);
                        break;

                    case FaceFieldKind.Field when options.Parts && FaceComposition.IsPartField(field.Name):
                        if (Part(project, row, field.Name, options.AroundCurrent, strength, random, isFemale) is { } part)
                            Add(values, row, field.Name, part);
                        break;
                }
            }
        }

        return values;
    }

    private static void Add(List<(string, object)> values, Param.Row row, string field, int value)
    {
        if (Int(row, field) != value)
            values.Add((field, value));
    }

    private static int Int(Param.Row row, string field) =>
        row[field]?.Value is IConvertible v ? v.ToInt32(null) : FaceGenParams.Neutral;

    /// <summary>A slider's new byte, or null to leave it (a hidden slider)</summary>
    private static int? SliderByte(ProjectEntry project, Param param, Param.Row row, string field, bool storesModes,
        bool aroundCurrent, float strength, Random random)
    {
        var isFaceGen = storesModes
            ? FaceGenParams.ParseFaceGenField(field) != null
            : FaceGenModel.SliderId(field) >= 0 || field.Equals("caricatureTexture", StringComparison.Ordinal);

        var current = Int(row, field);

        if (isFaceGen)
        {
            // Not in the creator's menu (hidden, or not in its order at all, as ER's faceGeoData60): the faces keep
            // these at one value, so they are left alone
            if (!storesModes && FaceGenModel.SliderId(field) is >= 0 and var id &&
                FaceGenSliderTable.Get(project) is { } table && (!table.Sequence.Contains(id) || table.Get(id) == null))
            {
                return null;
            }

            var centre = aroundCurrent ? current : FaceGenParams.Neutral;
            var spread = aroundCurrent ? VariationSpread : NeutralSpread;

            return ToByte(centre + Gaussian(random) * spread * strength);
        }

        // An amount: no neutral, so always around the face's own
        return ToByte(current + Gaussian(random) * AmountSpread * strength);
    }

    /// <summary>A colour moved a little in hue, saturation and value</summary>
    private static (int R, int G, int B) Colour(Param.Row row, FaceField field, float strength, Random random)
    {
        var rgb = new Vector3(Int(row, field.Name), Int(row, field.Green!), Int(row, field.Blue!)) / 255f;
        var (h, s, v) = ToHsv(rgb);

        h = (h + Gaussian(random) * HueSpread * strength + 1f) % 1f;
        s = Math.Clamp(s + Gaussian(random) * SaturationSpread * strength, 0f, 1f);
        v = Math.Clamp(v + Gaussian(random) * ValueSpread * strength, 0f, 1f);

        var result = FromHsv(h, s, v) * 255f;

        return (ToByte(result.X), ToByte(result.Y), ToByte(result.Z));
    }

    /// <summary>Another value that names a model, with a chance that grows with the strength; else null</summary>
    private static int? Part(ProjectEntry project, Param.Row row, string field, bool aroundCurrent, float strength,
        Random random, bool isFemale)
    {
        // A variation changes a part less often than a fresh face
        var chance = aroundCurrent ? strength * 0.5f : strength;

        if (random.NextSingle() >= chance)
            return null;

        var choices = FaceComposition.GetChoices(project, field, isFemale);

        return choices.Count == 0 ? null : choices[random.Next(choices.Count)].Value;
    }

    private static int ToByte(float value) => Math.Clamp((int)MathF.Round(value), 0, 255);

    /// <summary>A standard normal sample (Box-Muller)</summary>
    private static float Gaussian(Random random)
    {
        var u1 = 1.0 - random.NextDouble();
        var u2 = random.NextDouble();

        return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
    }

    private static (float H, float S, float V) ToHsv(Vector3 c)
    {
        var max = MathF.Max(c.X, MathF.Max(c.Y, c.Z));
        var min = MathF.Min(c.X, MathF.Min(c.Y, c.Z));
        var delta = max - min;

        float h = 0;

        if (delta > 1e-6f)
        {
            if (max == c.X)
                h = (c.Y - c.Z) / delta / 6f;
            else if (max == c.Y)
                h = ((c.Z - c.X) / delta + 2f) / 6f;
            else
                h = ((c.X - c.Y) / delta + 4f) / 6f;

            h = (h + 1f) % 1f;
        }

        return (h, max > 1e-6f ? delta / max : 0f, max);
    }

    private static Vector3 FromHsv(float h, float s, float v)
    {
        var i = (int)MathF.Floor(h * 6f) % 6;
        var f = h * 6f - MathF.Floor(h * 6f);
        var p = v * (1f - s);
        var q = v * (1f - f * s);
        var t = v * (1f - (1f - f) * s);

        return i switch
        {
            0 => new Vector3(v, t, p),
            1 => new Vector3(q, v, p),
            2 => new Vector3(p, v, t),
            3 => new Vector3(p, q, v),
            4 => new Vector3(t, p, v),
            _ => new Vector3(v, p, q)
        };
    }
}
