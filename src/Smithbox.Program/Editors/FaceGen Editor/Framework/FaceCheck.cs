#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>What a Face Check finding is</summary>
public enum FaceIssueKind
{
    /// <summary>A character row points at a face that doesn't exist. The game shows no face, or a default one.</summary>
    MissingFace,

    /// <summary>A part field's value names no model: the game shows none (or, for a head, something else)</summary>
    UnknownPart,

    /// <summary>
    /// A slider the creator's menu doesn't show (hidden in its order, or not in it) holds another value than most faces.
    /// Smithbox shows it as the game builds it, but no face made in the creator would have it. A note, not an error:
    /// NPC faces are made outside the creator.
    /// </summary>
    HiddenSlider
}

/// <summary>A finding: the row it is on (a face row, or for a missing face the character row), and what it says</summary>
public sealed record FaceIssue(FaceIssueKind Kind, string ParamName, Param.Row Row, string Detail, int FaceId = -1);

/// <summary>
/// Checks a project's faces, as a Map Check does a map: characters pointing at missing faces, part values that name no
/// model, and hidden sliders with unusual values. Also counts the faces no character wears.
/// </summary>
public static class FaceCheck
{
    public sealed record Result(List<FaceIssue> Issues, int UnusedFaces, int Faces);

    public static Result Run(ProjectEntry project)
    {
        var issues = new List<FaceIssue>();
        var faceParamName = FaceGenParams.GetReferencedFaceParam(project);

        if (faceParamName == null || FaceGenParams.GetParam(project, faceParamName) is not { } faceParam)
            return new Result(issues, 0, 0);

        var users = FaceGenParams.GetUsers(project);

        // Characters pointing at missing faces
        foreach (var user in users.Missing.OrderBy(u => u.ParamName).ThenBy(u => u.Row.ID))
            issues.Add(new FaceIssue(FaceIssueKind.MissingFace, user.ParamName, user.Row, user.FaceId.ToString(), user.FaceId));

        // Every face param the project has: parts and hidden sliders
        foreach (var paramName in FaceGenParams.GetFaceParams(project))
        {
            if (FaceGenParams.GetParam(project, paramName) is not { } param)
                continue;

            CheckParts(project, paramName, param, issues);
            CheckHiddenSliders(project, paramName, param, issues);
        }

        var unused = faceParam.Rows.Count(r => users.Get(r.ID).Count == 0);

        return new Result(issues, unused, faceParam.Rows.Count);
    }

    /// <summary>Part values that resolve to no model, or a head that isn't there (shown as the first head)</summary>
    private static void CheckParts(ProjectEntry project, string paramName, Param param, List<FaceIssue> issues)
    {
        var partFields = param.AppliedParamdef?.Fields
            .Select(f => f.InternalName)
            .Where(FaceComposition.IsPartField)
            .ToList() ?? new List<string>();

        if (partFields.Count == 0)
            return;

        // The values every field's choices offer, once per field (a choice is a value that names a model)
        var known = partFields.ToDictionary(f => f, f =>
            FaceComposition.GetChoices(project, f, false).Select(c => c.Value)
                .Concat(FaceComposition.GetChoices(project, f, true).Select(c => c.Value))
                .ToHashSet());

        foreach (var row in param.Rows)
        {
            foreach (var field in partFields)
            {
                if (row[field]?.Value is not IConvertible v)
                    continue;

                var value = v.ToInt32(null);

                // A family with no models at all (a game without that kind of part) says nothing about the value
                if (known[field].Count == 0 || known[field].Contains(value))
                    continue;

                var part = FaceComposition.Resolve(project, field, value, false);

                if (part.Model != null && !part.IsFallback)
                    continue;

                var detail = part.IsFallback ? $"{field} = {value} → {part.Model}" : $"{field} = {value}";
                issues.Add(new FaceIssue(FaceIssueKind.UnknownPart, paramName, row, detail));
            }
        }
    }

    /// <summary>
    /// Sliders the creator's menu doesn't show, whose value on a row isn't the one most of the param's rows hold
    /// </summary>
    private static void CheckHiddenSliders(ProjectEntry project, string paramName, Param param, List<FaceIssue> issues)
    {
        if (FaceGenParams.StoresModes(param) || FaceGenSliderTable.Get(project) is not { } table)
            return;

        var hidden = param.AppliedParamdef?.Fields
            .Select(f => f.InternalName)
            .Where(f => FaceGenModel.SliderId(f) is >= 0 and var id &&
                        // Texture controls only count where the game applies them
                        (id < 200 || table.Sequence.Any(s => s is >= 200 and < 300)) &&
                        (!table.Sequence.Contains(id) || table.Get(id) == null))
            .ToList() ?? new List<string>();

        foreach (var field in hidden)
        {
            var values = param.Rows
                .Select(r => (Row: r, Value: r[field]?.Value is IConvertible v ? v.ToInt32(null) : (int?)null))
                .Where(x => x.Value != null)
                .ToList();

            if (values.Count == 0)
                continue;

            var usual = values.GroupBy(x => x.Value).OrderByDescending(g => g.Count()).First().Key!.Value;

            foreach (var (row, value) in values)
            {
                // Within a byte of the usual value (127 against 128) is the same
                if (Math.Abs(value!.Value - usual) <= 1)
                    continue;

                issues.Add(new FaceIssue(FaceIssueKind.HiddenSlider, paramName, row, $"{field} = {value} ({usual})"));
            }
        }
    }
}
