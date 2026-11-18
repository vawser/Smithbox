#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// A game's character creator sliders (Assets/FACEGEN/&lt;game&gt;.json): each slider's range, the order the game applies
/// them in, and the clamps. Only the games whose faces store slider values (FaceParam) have one.
/// </summary>
public sealed class FaceGenSliderTable
{
    public sealed class Slider
    {
        public int Id { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
    }

    public List<Slider> Sliders { get; set; } = new();
    public List<int> Sequence { get; set; } = new();
    public float ShapeClamp { get; set; } = float.MaxValue;
    public float TextureClamp { get; set; } = float.MaxValue;

    /// <summary>
    /// The face part value names the body too (DS3: the hundreds digit is the musculature, the tens the body hair, the
    /// ones the age type, and fc_&lt;sex&gt;_&lt;value&gt; is that body), and the head is then fg_a_01NN by its last two
    /// digits, else by its ones digit alone (110 → fg_a_0100). See <see cref="StudioCore.Editors.Common.FaceComposition"/>
    /// </summary>
    public bool FacePartsNameBody { get; set; }

    /// <summary>
    /// The fgbnd image the face texture's modes are added to (DS3: "face.bmp", the mean), or null if the game draws no FaceGen
    /// texture. See <see cref="FaceTextureBuilder"/>
    /// </summary>
    public string? TextureBase { get; set; }

    private Dictionary<int, Slider>? ById;

    public Slider? Get(int id) => (ById ??= Sliders.ToDictionary(s => s.Id)).GetValueOrDefault(id);

    private static readonly ConditionalWeakTable<ProjectEntry, Holder> Cache = new();

    private sealed class Holder
    {
        public FaceGenSliderTable? Table;
    }

    public static FaceGenSliderTable? Get(ProjectEntry project) =>
        Cache.GetValue(project, p => new Holder { Table = Load(p) }).Table;

    private static FaceGenSliderTable? Load(ProjectEntry project)
    {
        var path = Path.Join(AppContext.BaseDirectory, "Assets", "FACEGEN", $"{ProjectUtils.GetGameDirectory(project)}.json");

        if (!File.Exists(path))
            return null;

        try
        {
            return JsonSerializer.Deserialize<FaceGenSliderTable>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(FaceGenSliderTable), LOC.Get("FACEGEN_Slider_Table_Failed", path), e);
            return null;
        }
    }
}

/// <summary>
/// Turns a face row into the coefficients of the FaceGen model's symmetric shape modes, as the game does:
/// <list type="bullet">
/// <item>FaceGenParam (modes stored): each byte is a coefficient, b × 20 / 255 − 10.</item>
/// <item>FaceParam (sliders stored): from zero, each slider is applied in the game's order (age, gender, caricature,
/// then the shape controls; hidden controls at 0), and the coefficients are clamped.</item>
/// </list>
/// The maths follows FaceSouls (github.com/lexlayer93/FaceSouls), MIT licence, Copyright (c) lexlayer93: a control
/// moves the face along its direction to its value; age and gender ("neutral") each keep the other where it is;
/// caricature scales how far the face is from the average, outside the age/gender plane. Race "All", as the creators use.
/// Only shape is computed: texture is drawn from the game's own colour fields.
/// </summary>
public static class FaceGenModel
{
    private const int AgeSlider = 10;
    private const int GenderSlider = 20;
    private const int CaricatureSlider = 30;

    /// <summary>
    /// The shape coefficients of a face row, or null if the game's model or slider table isn't available.
    /// <paramref name="overrides"/> stands in for fields by name, e.g. a slider being dragged that isn't written yet.
    /// </summary>
    public static float[]? ShapeCoefficients(ProjectEntry project, Param param, Param.Row row,
        IReadOnlyDictionary<string, int>? overrides = null)
    {
        var ctl = FaceGenModelData.Get(project).Controls;

        if (ctl == null)
            return null;

        var gs = ctl.GeometrySymmetricModes;

        // The shape bytes, by their number
        var shape = new SortedDictionary<int, int>();

        foreach (var cell in row.Cells)
        {
            if (FaceGenParams.ParseFaceGenField(cell.Def.InternalName) is { IsGeometry: true } field && cell.Value is IConvertible value)
            {
                shape[field.Index] = overrides != null && overrides.TryGetValue(cell.Def.InternalName, out var dragged)
                    ? dragged
                    : value.ToInt32(null);
            }
        }

        if (FaceGenParams.StoresModes(param))
        {
            var modes = new float[gs];
            var k = 0;

            foreach (var b in shape.Values)
            {
                if (k >= gs)
                    break;
                modes[k++] = FaceGenParams.ByteToCoefficient(b);
            }

            return modes;
        }

        var table = FaceGenSliderTable.Get(project);

        if (table == null || ctl.AllRaces is not { } race)
            return null;

        var s = new float[gs];

        float Value(int id, int b)
        {
            var slider = table.Get(id);

            // Hidden sliders: in the order, but not in the menu, so they stay at 0
            return slider == null ? 0 : slider.Min + (slider.Max - slider.Min) * b / 255f;
        }

        int Byte(string fieldName) =>
            overrides != null && overrides.TryGetValue(fieldName, out var dragged) ? dragged
            : row[fieldName]?.Value is IConvertible v ? v.ToInt32(null) : FaceGenParams.Neutral;

        foreach (var id in table.Sequence)
        {
            switch (id)
            {
                case AgeSlider:
                    SetAgeGender(s, race.Age.GeometryCoefficients, race.Gender.GeometryCoefficients,
                        race.Age.GeometryOffset, race.Gender.GeometryOffset, Value(id, Byte("age")), age: true);
                    break;
                case GenderSlider:
                    SetAgeGender(s, race.Age.GeometryCoefficients, race.Gender.GeometryCoefficients,
                        race.Age.GeometryOffset, race.Gender.GeometryOffset, Value(id, Byte("gender")), age: false);
                    break;
                case CaricatureSlider:
                    SetCaricature(s, race.Age.GeometryCoefficients, race.Gender.GeometryCoefficients,
                        race.GeometryMean, race.GeometryDensity, Value(id, Byte("caricatureGeometry")));
                    break;
                case >= 100 and < 200:
                {
                    var index = id - 100;

                    if (index < ctl.GeometrySymmetricControls.Count)
                        SetControl(s, ctl.GeometrySymmetricControls[index].Coefficients, Value(id, shape.GetValueOrDefault(index, FaceGenParams.Neutral)));
                    break;
                }
            }
        }

        for (var i = 0; i < gs; i++)
            s[i] = Math.Clamp(s[i], -table.ShapeClamp, table.ShapeClamp);

        return s;
    }

    /// <summary>
    /// The coefficients of the texture modes of a FaceParam row, as the game's creator applies its sliders: age,
    /// gender and caricature (caricatureTexture) on the texture side, then each texture control (2NN, faceTexDataNN),
    /// then the texture clamp. Null if the game has no model or table, or the param stores modes.
    /// </summary>
    public static float[]? TextureCoefficients(ProjectEntry project, Param param, Param.Row row,
        IReadOnlyDictionary<string, int>? overrides = null)
    {
        var ctl = FaceGenModelData.Get(project).Controls;
        var table = FaceGenSliderTable.Get(project);

        if (ctl == null || table == null || ctl.AllRaces is not { } race || FaceGenParams.StoresModes(param))
            return null;

        var t = new float[ctl.TextureSymmetricModes];

        float Value(int id, int b)
        {
            var slider = table.Get(id);
            return slider == null ? 0 : slider.Min + (slider.Max - slider.Min) * b / 255f;
        }

        int Byte(string fieldName) =>
            overrides != null && overrides.TryGetValue(fieldName, out var dragged) ? dragged
            : row[fieldName]?.Value is IConvertible v ? v.ToInt32(null) : FaceGenParams.Neutral;

        foreach (var id in table.Sequence)
        {
            switch (id)
            {
                case AgeSlider:
                    SetAgeGender(t, race.Age.TextureCoefficients, race.Gender.TextureCoefficients,
                        race.Age.TextureOffset, race.Gender.TextureOffset, Value(id, Byte("age")), age: true);
                    break;
                case GenderSlider:
                    SetAgeGender(t, race.Age.TextureCoefficients, race.Gender.TextureCoefficients,
                        race.Age.TextureOffset, race.Gender.TextureOffset, Value(id, Byte("gender")), age: false);
                    break;
                case CaricatureSlider:
                    SetCaricature(t, race.Age.TextureCoefficients, race.Gender.TextureCoefficients,
                        race.TextureMean, race.TextureDensity, Value(id, Byte("caricatureTexture")));
                    break;
                case >= 200 and < 300:
                {
                    var index = id - 200;

                    if (index < ctl.TextureSymmetricControls.Count)
                        SetControl(t, ctl.TextureSymmetricControls[index].Coefficients, Value(id, Byte($"faceTexData{index:D2}")));
                    break;
                }
            }
        }

        for (var i = 0; i < t.Length; i++)
            t[i] = Math.Clamp(t[i], -table.TextureClamp, table.TextureClamp);

        return t;
    }

    /// <summary>
    /// The value a FaceParam byte stands for on the game's slider (age in years, a control from −10 to 10 and so on),
    /// or null if the field isn't a slider of the game's table. Hidden sliders read 0, as the game applies them.
    /// </summary>
    public static float? SliderValue(ProjectEntry project, string field, int b)
    {
        if (FaceGenSliderTable.Get(project) is not { } table)
            return null;

        var id = field switch
        {
            "age" => AgeSlider,
            "gender" => GenderSlider,
            "caricatureGeometry" => CaricatureSlider,
            _ => FaceGenParams.ParseFaceGenField(field) is { } f ? (f.IsGeometry ? 100 : 200) + f.Index : -1
        };

        if (id < 0 || !table.Sequence.Contains(id))
            return null;

        return table.Get(id) is { } slider ? slider.Min + (slider.Max - slider.Min) * b / 255f : 0f;
    }

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
