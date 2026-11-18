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
/// them in, and the clamps. The games whose faces store slider values (FaceParam) have sliders; DS1 and DS1R, whose
/// faces store the coefficients themselves, have a table only for its <see cref="TextureBase"/>.
/// </summary>
public sealed class FaceGenSliderTable
{
    public sealed class Slider
    {
        public int Id { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }

        /// <summary>
        /// The slider's slot in the character creator's menu: CharMakeMenuTopParam's faceParamId on the slider's row
        /// (ER: 0–86 for the shape controls, 96 age, 97 gender, 99 caricature). Null where the table doesn't say.
        /// Used for the slider's in-game caption (<see cref="FaceGenMenu"/>).
        /// </summary>
        public int? Slot { get; set; }
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
/// Turns a face row into the coefficients of the FaceGen model's symmetric modes, as the game does, and back:
/// <list type="bullet">
/// <item>FaceGenParam (modes stored): each byte is a coefficient, b × 20 / 255 − 10.</item>
/// <item>FaceParam (sliders stored): from zero, each slider is applied in the game's order (age, gender, caricature,
/// then the controls; hidden controls at 0), and the coefficients are clamped (<see cref="FaceGenSolver.Build"/>).</item>
/// </list>
/// The maths is <see cref="FaceGenSolver"/>'s (after FaceSouls, MIT licence, Copyright (c) lexlayer93).
/// </summary>
public static class FaceGenModel
{
    private const int AgeSlider = FaceGenSolver.AgeSlider;
    private const int GenderSlider = FaceGenSolver.GenderSlider;
    private const int CaricatureSlider = FaceGenSolver.CaricatureSlider;

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

        if (table == null || ctl.AllRaces == null)
            return null;

        int Byte(string fieldName) =>
            overrides != null && overrides.TryGetValue(fieldName, out var dragged) ? dragged
            : row[fieldName]?.Value is IConvertible v ? v.ToInt32(null) : FaceGenParams.Neutral;

        float Value(int id) => id switch
        {
            AgeSlider => ByteValue(table, id, Byte("age")),
            GenderSlider => ByteValue(table, id, Byte("gender")),
            CaricatureSlider => ByteValue(table, id, Byte("caricatureGeometry")),
            >= 100 and < 200 => ByteValue(table, id, shape.GetValueOrDefault(id - 100, FaceGenParams.Neutral)),
            _ => 0
        };

        return FaceGenSolver.Build(ctl, texture: false, table.Sequence, Value, table.ShapeClamp);
    }

    /// <summary>
    /// The coefficients of the texture modes of a face row: FaceGenParam's texture bytes, or a FaceParam row's
    /// sliders as the creator applies them (age, gender and caricatureTexture on the texture side, then each texture
    /// control, faceTexDataNN, then the texture clamp). Null if the game has no model or table.
    /// </summary>
    public static float[]? TextureCoefficients(ProjectEntry project, Param param, Param.Row row,
        IReadOnlyDictionary<string, int>? overrides = null)
    {
        var ctl = FaceGenModelData.Get(project).Controls;
        var table = FaceGenSliderTable.Get(project);

        if (ctl == null)
            return null;

        // FaceGenParam (DS1, DS1R): the texture bytes are the coefficients themselves, in order
        if (FaceGenParams.StoresModes(param))
        {
            var t = new float[ctl.TextureSymmetricModes];
            var texture = new SortedDictionary<int, int>();

            foreach (var cell in row.Cells)
            {
                if (FaceGenParams.ParseFaceGenField(cell.Def.InternalName) is { IsGeometry: false } field && cell.Value is IConvertible value)
                {
                    texture[field.Index] = overrides != null && overrides.TryGetValue(cell.Def.InternalName, out var dragged)
                        ? dragged
                        : value.ToInt32(null);
                }
            }

            var k = 0;

            foreach (var b in texture.Values)
            {
                if (k >= t.Length)
                    break;
                t[k++] = FaceGenParams.ByteToCoefficient(b);
            }

            return t;
        }

        if (table == null || ctl.AllRaces == null)
            return null;

        int Byte(string fieldName) =>
            overrides != null && overrides.TryGetValue(fieldName, out var dragged) ? dragged
            : row[fieldName]?.Value is IConvertible v ? v.ToInt32(null) : FaceGenParams.Neutral;

        float Value(int id) => id switch
        {
            AgeSlider => ByteValue(table, id, Byte("age")),
            GenderSlider => ByteValue(table, id, Byte("gender")),
            CaricatureSlider => ByteValue(table, id, Byte("caricatureTexture")),
            >= 200 and < 300 => ByteValue(table, id, Byte($"faceTexData{id - 200:D2}")),
            _ => 0
        };

        return FaceGenSolver.Build(ctl, texture: true, table.Sequence, Value, table.TextureClamp);
    }

    /// <summary>A byte's value on a slider of the table; 0 for a hidden slider (in the order, not in the menu)</summary>
    private static float ByteValue(FaceGenSliderTable table, int id, int b) =>
        table.Get(id) is { } slider ? slider.Min + (slider.Max - slider.Min) * b / 255f : 0;

    // ---- coefficients → bytes

    /// <summary>A face's bytes fitted from coefficients, and how close the face they build is</summary>
    /// <param name="Fields">Each field to write, by name, with its byte</param>
    /// <param name="ShapeRms">RMS error of the rebuilt shape coefficients (0 is exact)</param>
    /// <param name="TextureRms">The same for texture, or null if no texture was fitted</param>
    public sealed record SliderFit(Dictionary<string, int> Fields, float ShapeRms, float? TextureRms);

    /// <summary>
    /// The bytes of <paramref name="param"/>'s fields that make the face whose coefficients are
    /// <paramref name="shape"/> (and <paramref name="texture"/>, if given): the inverse of
    /// <see cref="ShapeCoefficients"/> and <see cref="TextureCoefficients"/>.
    /// <list type="bullet">
    /// <item>FaceGenParam: each coefficient as its byte, in field order.</item>
    /// <item>FaceParam: <see cref="FaceGenSolver.Fit"/> over the game's sliders, both sides at once, as they share
    /// age and gender.</item>
    /// </list>
    /// Null if the game's model or slider table isn't available.
    /// </summary>
    public static SliderFit? SlidersFromCoefficients(ProjectEntry project, Param param, float[] shape, float[]? texture = null)
    {
        var ctl = FaceGenModelData.Get(project).Controls;
        var fields = param.AppliedParamdef?.Fields.Select(f => f.InternalName).ToList();

        if (ctl == null || fields == null)
            return null;

        if (FaceGenParams.StoresModes(param))
            return ModesToBytes(fields, shape, texture);

        if (FaceGenSliderTable.Get(project) is not { } table)
            return null;

        (float Min, float Max)? Range(int id) => table.Get(id) is { } s ? (s.Min, s.Max) : null;

        var fit = FaceGenSolver.Fit(ctl, table.Sequence, Range, table.ShapeClamp, table.TextureClamp, shape, texture);

        if (fit == null)
            return null;

        var result = new Dictionary<string, int>();

        void Write(FaceGenSolver.FitResult fit, bool isTexture)
        {
            foreach (var (id, value) in fit.Values)
            {
                if (Range(id) is not { } r || FieldOf(id, isTexture) is not { } field || !fields.Contains(field))
                    continue;

                result[field] = ToByte(r, value);
            }
        }

        Write(fit.Shape, false);

        if (fit.Texture != null)
            Write(fit.Texture, true);

        return new SliderFit(result, fit.Shape.Rms, fit.Texture?.Rms);
    }

    /// <summary>The FaceParam field a slider ID writes: on the texture side, caricature is caricatureTexture</summary>
    private static string? FieldOf(int id, bool texture) => id switch
    {
        AgeSlider => texture ? null : "age",
        GenderSlider => texture ? null : "gender",
        CaricatureSlider => texture ? "caricatureTexture" : "caricatureGeometry",
        >= 100 and < 200 when !texture => $"faceGeoData{id - 100:D2}",
        >= 200 and < 300 when texture => $"faceTexData{id - 200:D2}",
        _ => null
    };

    /// <summary>FaceGenParam: each coefficient as its byte, the n-th mode in the n-th shape (texture) field by number</summary>
    private static SliderFit ModesToBytes(List<string> fields, float[] shape, float[]? texture)
    {
        var result = new Dictionary<string, int>();
        float shapeError = 0, textureError = 0;

        float Side(bool geometry, float[] coefficients)
        {
            var sideFields = fields
                .Select(f => (Field: f, Parsed: FaceGenParams.ParseFaceGenField(f)))
                .Where(f => f.Parsed is { } p && p.IsGeometry == geometry)
                .OrderBy(f => f.Parsed!.Value.Index)
                .Select(f => f.Field)
                .ToList();

            var error = 0f;

            for (var k = 0; k < sideFields.Count && k < coefficients.Length; k++)
            {
                var b = ToByte((-10f, 10f), coefficients[k]);
                result[sideFields[k]] = b;
                var back = FaceGenParams.ByteToCoefficient(b);
                error += (back - coefficients[k]) * (back - coefficients[k]);
            }

            return MathF.Sqrt(error / Math.Max(1, coefficients.Length));
        }

        shapeError = Side(true, shape);

        if (texture != null)
            textureError = Side(false, texture);

        return new SliderFit(result, shapeError, texture != null ? textureError : null);
    }

    // ---- slider values

    /// <summary>
    /// The value a FaceParam byte stands for on the game's slider (age in years, a control from −10 to 10 and so on),
    /// or null if the field isn't a slider of the game's table. Hidden sliders read 0, as the game applies them.
    /// </summary>
    public static float? SliderValue(ProjectEntry project, string field, int b)
    {
        if (FaceGenSliderTable.Get(project) is not { } table)
            return null;

        var id = SliderId(field);

        if (id < 0 || !table.Sequence.Contains(id))
            return null;

        return table.Get(id) is { } slider ? slider.Min + (slider.Max - slider.Min) * b / 255f : 0f;
    }

    /// <summary>
    /// The slider table's ID for a FaceParam field: 10 age, 20 gender, 30 caricature, 1NN / 2NN the shape / texture
    /// control NN. −1 for any other field.
    /// </summary>
    public static int SliderId(string field) => field switch
    {
        "age" => AgeSlider,
        "gender" => GenderSlider,
        "caricatureGeometry" => CaricatureSlider,
        _ => FaceGenParams.ParseFaceGenField(field) is { } f ? (f.IsGeometry ? 100 : 200) + f.Index : -1
    };

    /// <summary>
    /// The range of game values a field's byte spans (0 to 255 → Min to Max; Min may be the larger, as the game's
    /// inverted sliders are), or null if the field has none, so its byte is all there is to show:
    /// <list type="bullet">
    /// <item>FaceGenParam shape and texture fields: the mode coefficient, −10 to 10.</item>
    /// <item>FaceParam sliders in the game's slider table (age in years, controls ±10 or ±5). Hidden sliders have none.</item>
    /// <item>Other FaceParam amounts (gloss, root darkness, makeup): FaceRangeParam's range for the field.</item>
    /// </list>
    /// </summary>
    public static (float Min, float Max)? GameRange(ProjectEntry project, Param param, string field)
    {
        if (FaceGenParams.StoresModes(param))
            return FaceGenParams.ParseFaceGenField(field) != null ? (-10f, 10f) : null;

        var id = SliderId(field);

        if (id >= 0 && FaceGenSliderTable.Get(project) is { } table && table.Sequence.Contains(id))
            return table.Get(id) is { } slider ? (slider.Min, slider.Max) : null;

        return FaceGenParams.Range(project, field) is { } range && range.Min != range.Max ? range : null;
    }

    /// <summary>A byte as its game value across <paramref name="range"/></summary>
    public static float ToGameValue((float Min, float Max) range, int b) => range.Min + (range.Max - range.Min) * b / 255f;

    /// <summary>A game value across <paramref name="range"/> as the nearest byte</summary>
    public static int ToByte((float Min, float Max) range, float value) =>
        range.Max == range.Min ? FaceGenParams.Neutral : Math.Clamp((int)MathF.Round((value - range.Min) / (range.Max - range.Min) * 255f), 0, 255);
}
