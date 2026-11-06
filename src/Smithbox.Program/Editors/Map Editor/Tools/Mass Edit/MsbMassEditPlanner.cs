using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A field of a matched object, and what an edit will make of it
/// </summary>
public record MsbPlannedChange(MsbQueryMatch Match, string Path, object OldValue, object NewValue, string OldText,
    string NewText);

/// <summary>
/// A matched object an edit leaves alone, and why
/// </summary>
public record MsbPlannedSkip(MsbQueryMatch Match, string Reason);

/// <summary>
/// What an edit will do: each change to a field, the objects it leaves alone, and how many it changes
/// </summary>
public class MsbMassEditPlan
{
    public readonly List<MsbPlannedChange> Changes = new();
    public readonly List<MsbPlannedSkip> Skips = new();

    /// <summary>
    /// The objects with at least one change
    /// </summary>
    public readonly HashSet<MsbQueryMatch> Objects = new();

    /// <summary>
    /// The objects every step applied to that still came out as they were
    /// </summary>
    public int Unchanged;

    public int MapCount => Objects.Select(e => e.MapID).Distinct().Count();
}

/// <summary>
/// A field the matches have, as the steps list it: its path, its kind, and how many of the matches have it
/// </summary>
public record MsbMassEditFieldInfo(string Path, string DisplayName, Type ValueType, MsbMassEditFieldKind Kind, int Count);

/// <summary>
/// Works out what an edit does to the matches, without changing anything, so the preview and the apply are the same.
/// </summary>
public class MsbMassEditPlanner
{
    private readonly MsbQueryMatcher Matcher;

    public MsbMassEditPlanner(ProjectEntry project)
    {
        Matcher = new MsbQueryMatcher(project);
    }

    /// <summary>
    /// The fields of the matches, in the order they are first met, with how many of the matches have each
    /// </summary>
    public List<MsbMassEditFieldInfo> GetFields(List<MsbQueryMatch> matches)
    {
        var order = new List<string>();
        var infos = new Dictionary<string, (MsbQueryField Field, int Count)>();

        foreach (var match in matches)
        {
            foreach (var field in GetFields(match))
            {
                if (infos.TryGetValue(field.Path, out var info))
                {
                    infos[field.Path] = (info.Field, info.Count + 1);
                }
                else
                {
                    infos[field.Path] = (field, 1);
                    order.Add(field.Path);
                }
            }
        }

        return order.Select(path =>
        {
            var (field, count) = infos[path];

            return new MsbMassEditFieldInfo(path, field.DisplayName, field.ValueType,
                MsbMassEditStep.GetKind(field.ValueType), count);
        }).ToList();
    }

    private List<MsbQueryField> GetFields(MsbQueryMatch match)
    {
        return match.Fields ?? Matcher.GetFields(match.Entry.Object);
    }

    /// <summary>
    /// Applies the steps in order to the fields of each match, as they were found. An object is changed by every step
    /// or, if any can't apply to it, by none, and is then listed under the reason.
    /// </summary>
    public MsbMassEditPlan Plan(List<MsbQueryMatch> matches, List<MsbMassEditStep> steps)
    {
        var plan = new MsbMassEditPlan();

        steps = steps.Where(e => !string.IsNullOrWhiteSpace(e.Field)).ToList();

        if (steps.Count == 0)
            return plan;

        // Drawn once per plan, so the preview shows the values that are applied
        var random = new Random();

        foreach (var match in matches)
        {
            var fields = new Dictionary<string, MsbQueryField>();

            foreach (var field in GetFields(match))
            {
                fields.TryAdd(field.Path, field);
            }

            var values = new Dictionary<string, object>();
            string skip = null;

            foreach (var step in steps)
            {
                if (!fields.TryGetValue(step.Field, out var field))
                {
                    skip = LOC.Get("MAP_MassEditTool_Skip_No_Field", step.Field);
                    break;
                }

                var current = values.TryGetValue(step.Field, out var value) ? value : field.Value;

                if (!step.TryApply(field.ValueType, current, random, out var result, out var reason))
                {
                    skip = reason;
                    break;
                }

                values[step.Field] = result;
            }

            if (skip != null)
            {
                plan.Skips.Add(new MsbPlannedSkip(match, skip));
                continue;
            }

            var changed = false;

            foreach (var (path, value) in values)
            {
                var field = fields[path];

                if (Equals(field.Value, value))
                    continue;

                plan.Changes.Add(new MsbPlannedChange(match, path, field.Value, value, field.Shown, FormatValue(value)));
                changed = true;
            }

            if (changed)
                plan.Objects.Add(match);
            else
                plan.Unchanged++;
        }

        return plan;
    }

    /// <summary>
    /// A value as the changes list it, with its name if it is an enum
    /// </summary>
    public static string FormatValue(object value)
    {
        if (value is Enum enumValue)
        {
            var number = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), System.Globalization.CultureInfo.InvariantCulture);

            return $"{enumValue} ({MsbQueryMatcher.FormatValue(number)})";
        }

        return MsbQueryMatcher.FormatValue(value);
    }
}
