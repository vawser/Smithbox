#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>What an import takes from a face file</summary>
public sealed class FaceImportOptions
{
    /// <summary>The FaceGen shape and texture (sliders, or FaceGenParam's modes), and the asymmetry</summary>
    public bool Shape = true;

    /// <summary>Colours, and amounts such as gloss, makeup and root darkness</summary>
    public bool Colours = true;

    /// <summary>Part fields (hair, beard, head…): the same game only, since part numbers differ between games</summary>
    public bool Parts = true;

    /// <summary>Every other field (flags, body scales…): the same game and param only</summary>
    public bool Other = false;
}

/// <summary>
/// Turns a <see cref="FaceFile"/> into the field values of a face param row, in this project's game:
/// <list type="bullet">
/// <item><b>Same game and param:</b> each chosen field's stored value, as it is.</item>
/// <item><b>Another game, or the other face param:</b> the shape (and texture, where the target uses it) is fitted to
/// the target's sliders from the file's coefficients; the asymmetry (the same CTL controls everywhere) is copied by
/// name; colours and amounts go through their game values and the target's ranges; parts and other fields are left,
/// since their numbers mean other things there.</item>
/// </list>
/// Field names are matched ignoring case and underscores (ER's hair_color_R is DS3's hairColor_R). The plan says what
/// it did, for the tool to show.
/// </summary>
public static class FaceImport
{
    public sealed record Plan(List<(string Field, object Value)> Values, List<string> Notes, bool CrossGame);

    private static readonly Regex AsymmetryField = new(@"^faceGeoAsymData\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>A FaceGen slider or mode (shape, texture, age, gender, caricature): taken from the coefficients across games</summary>
    public static bool IsFaceGenField(string field) =>
        FaceGenParams.ParseFaceGenField(field) != null || FaceGenModel.SliderId(field) >= 0 ||
        field.Equals("caricatureTexture", StringComparison.OrdinalIgnoreCase);

    /// <summary>A field name with case and underscores dropped, for matching between games</summary>
    public static string Normalise(string field) => field.Replace("_", "").ToLowerInvariant();

    /// <summary>A colour's or amount's byte as its game value: across FaceRangeParam's range, else 0 to 1</summary>
    public static float ToValue(ProjectEntry project, string field, int b) =>
        FaceGenParams.Range(project, field) is { } r && r.Min != r.Max
            ? FaceGenModel.ToGameValue(r, b)
            : Math.Clamp(b, 0, 255) / 255f;

    /// <summary>A game value as a byte of this project's field: across its range, else 0 to 1</summary>
    private static int ToByte(ProjectEntry project, string field, float value) =>
        FaceGenParams.Range(project, field) is { } r && r.Min != r.Max
            ? FaceGenModel.ToByte(r, value)
            : Math.Clamp((int)MathF.Round(value * 255f), 0, 255);

    public static Plan Build(ProjectEntry project, string paramName, Param param, Param.Row target, FaceFile file, FaceImportOptions options)
    {
        var values = new List<(string, object)>();
        var notes = new List<string>();

        var game = ProjectUtils.GetGameDirectory(project);
        var sameGame = file.Game.Equals(game, StringComparison.OrdinalIgnoreCase);
        var sameParam = sameGame && file.Param.Equals(paramName, StringComparison.OrdinalIgnoreCase);

        // The file's fields by normalised name
        var byName = new Dictionary<string, string>();
        foreach (var name in file.Fields.Keys)
            byName.TryAdd(Normalise(name), name);

        string? Source(string field) => byName.GetValueOrDefault(Normalise(field));

        var groups = FaceGenLayout.Build(project, param);
        var kinds = groups.SelectMany(g => g.Fields).ToList();
        var colourFields = kinds.Where(f => f.Kind == FaceFieldKind.Color).SelectMany(f => new[] { f.Name, f.Green!, f.Blue! }).ToHashSet();
        var amountFields = kinds.Where(f => f.Kind == FaceFieldKind.Slider && !IsFaceGenField(f.Name) && !AsymmetryField.IsMatch(f.Name))
            .Select(f => f.Name).ToHashSet();

        var copied = 0;
        var skippedParts = false;
        var skippedOther = false;

        foreach (var cell in target.Cells)
        {
            var field = cell.Def.InternalName;

            if (Source(field) is not { } source)
                continue;

            var stored = file.Fields[source];
            var isFaceGen = IsFaceGenField(field);
            var isAsymmetry = AsymmetryField.IsMatch(field);
            var isColour = colourFields.Contains(field) || amountFields.Contains(field);
            var isPart = FaceComposition.IsPartField(field);

            object? value = null;

            if (isFaceGen)
            {
                // The same param of the same game: the stored values are exact. Otherwise fitted below.
                if (options.Shape && sameParam)
                    value = stored;
            }
            else if (isAsymmetry)
            {
                // The CTL's asymmetric controls, the same in every game's FaceParam
                if (options.Shape)
                    value = stored;
            }
            else if (isColour)
            {
                if (options.Colours)
                {
                    value = sameGame
                        ? stored
                        : file.Values.TryGetValue(source, out var gameValue) ? ToByte(project, field, gameValue) : stored;
                }
            }
            else if (isPart)
            {
                if (options.Parts && sameGame)
                    value = stored;
                else if (options.Parts)
                    skippedParts = true;
            }
            else if (options.Other)
            {
                if (sameParam)
                    value = stored;
                else
                    skippedOther = true;
            }

            if (value == null)
                continue;

            values.Add((field, value));
            copied++;
        }

        // The shape from the coefficients, where the stored sliders don't fit this param
        if (options.Shape && !sameParam)
            FitShape(project, param, file, values, notes);

        if (copied > 0)
            notes.Add(LOC.Get("FACEGEN_Import_Note_Copied", copied));

        if (skippedParts)
            notes.Add(LOC.Get("FACEGEN_Import_Note_Parts_Skipped", file.Game));

        if (skippedOther)
            notes.Add(LOC.Get("FACEGEN_Import_Note_Other_Skipped"));

        if (!sameGame && options.Colours)
            notes.Add(LOC.Get("FACEGEN_Import_Note_Colours_Converted", file.Game));

        return new Plan(values, notes, !sameParam);
    }

    /// <summary>The target's FaceGen sliders (or modes) fitted to the file's coefficients</summary>
    private static void FitShape(ProjectEntry project, Param param, FaceFile file, List<(string, object)> values, List<string> notes)
    {
        var ctl = FaceGenModelData.Get(project).Controls;

        if (file.Shape == null || ctl == null)
        {
            notes.Add(LOC.Get("FACEGEN_Import_Note_No_Shape"));
            return;
        }

        if (file.GeometryBasis != 0 && file.GeometryBasis != ctl.GeometryBasisVersion)
        {
            notes.Add(LOC.Get("FACEGEN_Import_Note_Other_Model"));
            return;
        }

        // Texture only where this param uses the texture controls: FaceGenParam's modes, or a creator that applies them
        var usesTexture = FaceGenParams.StoresModes(param) ||
                          FaceGenSliderTable.Get(project)?.Sequence.Any(id => FaceGenSolver.IsControl(id, true)) == true;

        var fit = FaceGenModel.SlidersFromCoefficients(project, param, file.Shape, usesTexture ? file.Texture : null);

        if (fit == null)
        {
            notes.Add(LOC.Get("FACEGEN_Import_Note_No_Shape"));
            return;
        }

        foreach (var (field, b) in fit.Fields)
            values.Add((field, b));

        notes.Add(fit.TextureRms is { } textureRms
            ? LOC.Get("FACEGEN_Import_Note_Fitted_Texture", file.Game, fit.ShapeRms.ToString("0.000"), textureRms.ToString("0.000"))
            : LOC.Get("FACEGEN_Import_Note_Fitted", file.Game, fit.ShapeRms.ToString("0.000")));

        // More than a few byte steps off: this game's sliders don't reach the face (BB's texture sliders are narrower
        // than DS3's, for one), so the nearest face they can make was taken
        if (fit.ShapeRms > PoorFit || fit.TextureRms > PoorFit)
            notes.Add(LOC.Get("FACEGEN_Import_Note_Poor_Fit"));
    }

    /// <summary>An RMS error above which a fit is reported as not reaching the face: about three byte steps</summary>
    private const float PoorFit = 0.25f;
}
