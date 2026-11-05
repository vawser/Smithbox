using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The part of a value row an edit step changes
/// </summary>
public enum GparamMassEditField
{
    Value,
    Component1,
    Component2,
    Component3,
    Component4,
    ID,
    TimeOfDay
}

/// <summary>
/// What an edit step does to its field
/// </summary>
public enum GparamMassEditOperator
{
    Set,
    Add,
    Subtract,
    MultiplyBy,
    DivideBy,
    AtLeast,
    AtMost,
    Toggle
}

/// <summary>
/// One step of an edit: Set [Field] [Operator] [Value]. The steps are applied in order, each to the value the step
/// before it left.
/// </summary>
public record GparamMassEditStep(GparamMassEditField? Field, GparamMassEditOperator Operator, string Value)
{
    public bool HasField => Field != null;

    public static bool TakesValue(GparamMassEditOperator op) => op is not GparamMassEditOperator.Toggle;

    /// <summary>
    /// The operators that make sense for a field. The value itself can be of any type, so the planner skips the
    /// rows an operator doesn't suit, with the reason.
    /// </summary>
    public static IReadOnlyList<GparamMassEditOperator> GetOperators(GparamMassEditField field) => field switch
    {
        GparamMassEditField.Value or GparamMassEditField.Component1 or GparamMassEditField.Component2 or
            GparamMassEditField.Component3 or GparamMassEditField.Component4 => Enum.GetValues<GparamMassEditOperator>(),
        _ => Enum.GetValues<GparamMassEditOperator>().Where(e => e != GparamMassEditOperator.Toggle).ToList()
    };
}

/// <summary>
/// A value row as the plan will leave it, with what it was when the plan was made
/// </summary>
public class GparamPlannedValue
{
    public GparamQueryMatch Match;

    public object OldValue;
    public object NewValue;
    public int OldID;
    public int NewID;
    public float OldTimeOfDay;
    public float NewTimeOfDay;

    public bool ValueChanged => !Equals(OldValue, NewValue);
    public bool IDChanged => OldID != NewID;
    public bool TimeOfDayChanged => OldTimeOfDay != NewTimeOfDay;
    public bool Changed => ValueChanged || IDChanged || TimeOfDayChanged;
}

/// <summary>
/// One change to show in the preview
/// </summary>
public record GparamPlannedChange(GparamQueryMatch Match, string Field, string OldText, string NewText);

/// <summary>
/// A match the plan leaves alone, and why
/// </summary>
public record GparamPlannedSkip(GparamQueryMatch Match, string Reason);

public class GparamMassEditPlan
{
    public List<GparamPlannedValue> Values = new();
    public List<GparamPlannedChange> Changes = new();
    public List<GparamPlannedSkip> Skips = new();

    /// <summary>
    /// The matches every step left as they were
    /// </summary>
    public int Unchanged;

    public int FieldCount;
}

/// <summary>
/// A copy a duplicate will add, with the ID it is planned to take
/// </summary>
public record GparamPlannedDuplicate(GparamQueryMatch Match, int NewID);

public class GparamMassDuplicatePlan
{
    public List<GparamPlannedDuplicate> Duplicates = new();
    public bool InvalidOffset;
    public int Offset;
}

/// <summary>
/// Works out what a mass edit will do to each match, without changing anything, so it can be shown before it is
/// applied. A row is changed by every step or by none, so it is never left half edited.
/// </summary>
public class GparamMassEditPlanner
{
    public ProjectEntry Project;

    private readonly GparamQueryMatcher Matcher;

    public GparamMassEditPlanner(ProjectEntry project)
    {
        Project = project;
        Matcher = new GparamQueryMatcher(project);
    }

    #region Edit
    public GparamMassEditPlan Plan(IReadOnlyList<GparamQueryMatch> matches, IReadOnlyList<GparamMassEditStep> steps)
    {
        var plan = new GparamMassEditPlan();
        var activeSteps = steps.Where(e => e.HasField).ToList();

        if (activeSteps.Count == 0)
            return plan;

        foreach (var match in matches)
        {
            if (match.Value == null)
                continue;

            var planned = new GparamPlannedValue
            {
                Match = match,
                OldValue = match.Value.Value,
                NewValue = match.Value.Value,
                OldID = match.Value.ID,
                NewID = match.Value.ID,
                OldTimeOfDay = match.Value.TimeOfDay,
                NewTimeOfDay = match.Value.TimeOfDay
            };

            string reason = null;

            foreach (var step in activeSteps)
            {
                reason = ApplyStep(step, match, planned);

                if (reason != null)
                    break;
            }

            if (reason != null)
            {
                plan.Skips.Add(new GparamPlannedSkip(match, reason));
                continue;
            }

            if (!planned.Changed)
            {
                plan.Unchanged++;
                continue;
            }

            plan.Values.Add(planned);
        }

        SkipTakenIDs(plan);

        foreach (var planned in plan.Values)
        {
            AddChanges(plan, planned);
        }

        plan.FieldCount = plan.Values.Select(e => (e.Match.FileEntry, e.Match.GroupKey, e.Match.FieldKey)).Distinct().Count();

        return plan;
    }

    /// <summary>
    /// Two values of a field can't share an ID. A row whose new ID another value of its field would have once the
    /// plan is applied is skipped, which puts its old ID back, so this goes round until no ID is taken twice.
    /// </summary>
    private static void SkipTakenIDs(GparamMassEditPlan plan)
    {
        var byField = plan.Values
            .Where(e => e.IDChanged)
            .GroupBy(e => (e.Match.FileEntry, e.Match.GroupKey, e.Match.FieldKey))
            .ToList();

        var skipped = new HashSet<GparamPlannedValue>();

        foreach (var group in byField)
        {
            var field = group.First().Match.Field;
            var planned = plan.Values
                .Where(e => (e.Match.FileEntry, e.Match.GroupKey, e.Match.FieldKey) == group.Key)
                .ToDictionary(e => e.Match.Value);

            bool changed;

            do
            {
                changed = false;

                // The IDs the field will have: the planned ones, and the rest as they are
                var counts = new Dictionary<int, int>();

                foreach (var value in field.Values)
                {
                    var id = planned.TryGetValue(value, out var p) && !skipped.Contains(p) ? p.NewID : value.ID;
                    counts[id] = counts.GetValueOrDefault(id) + 1;
                }

                foreach (var p in group)
                {
                    if (skipped.Contains(p) || counts.GetValueOrDefault(p.NewID) <= 1)
                        continue;

                    skipped.Add(p);
                    changed = true;
                }
            }
            while (changed);
        }

        if (skipped.Count == 0)
            return;

        foreach (var p in skipped)
        {
            plan.Values.Remove(p);
            plan.Skips.Add(new GparamPlannedSkip(p.Match, LOC.Get("GPARAM_MassEditTool_Skip_ID_Taken", p.NewID)));
        }
    }

    private void AddChanges(GparamMassEditPlan plan, GparamPlannedValue planned)
    {
        var match = planned.Match;

        if (planned.ValueChanged)
        {
            plan.Changes.Add(new GparamPlannedChange(match, LOC.Get("GPARAM_MassEditTool_Field_Value"),
                FormatValue(match, planned.OldValue), FormatValue(match, planned.NewValue)));
        }

        if (planned.IDChanged)
        {
            plan.Changes.Add(new GparamPlannedChange(match, LOC.Get("GPARAM_MassEditTool_Field_ID"),
                GparamQueryMatcher.FormatValue(planned.OldID), GparamQueryMatcher.FormatValue(planned.NewID)));
        }

        if (planned.TimeOfDayChanged)
        {
            plan.Changes.Add(new GparamPlannedChange(match, LOC.Get("GPARAM_MassEditTool_Field_Time_Of_Day"),
                GparamQueryMatcher.FormatValue(planned.OldTimeOfDay), GparamQueryMatcher.FormatValue(planned.NewTimeOfDay)));
        }
    }

    /// <summary>
    /// A value as text, with the name of its enum option if it has one
    /// </summary>
    private string FormatValue(GparamQueryMatch match, object value)
    {
        var text = GparamQueryMatcher.FormatValue(value);
        var option = Matcher.GetEnumOptions(match.GroupKey, match.FieldKey)?.FirstOrDefault(e => e.Value == text).Name;

        return option != null ? $"{option} ({text})" : text;
    }

    /// <summary>
    /// Returns the reason the step can't be applied to the row, or null
    /// </summary>
    private string ApplyStep(GparamMassEditStep step, GparamQueryMatch match, GparamPlannedValue planned)
    {
        switch (step.Field)
        {
            case GparamMassEditField.ID:
            {
                if (!TryApplyScalar(step, planned.NewID, out var result, out var reason))
                    return reason;

                var rounded = Math.Round(result, MidpointRounding.AwayFromZero);

                if (rounded < int.MinValue || rounded > int.MaxValue)
                    return LOC.Get("GPARAM_MassEditTool_Skip_Out_Of_Range", FormatNumber(result), "int");

                planned.NewID = (int)rounded;
                return null;
            }

            case GparamMassEditField.TimeOfDay:
            {
                if (!TryApplyScalar(step, planned.NewTimeOfDay, out var result, out var reason))
                    return reason;

                if (result < float.MinValue || result > float.MaxValue)
                    return LOC.Get("GPARAM_MassEditTool_Skip_Out_Of_Range", FormatNumber(result), "float");

                planned.NewTimeOfDay = (float)result;
                return null;
            }

            case GparamMassEditField.Value:
            {
                var reason = ApplyValueStep(step, match, planned.NewValue, out var result);

                if (reason != null)
                    return reason;

                planned.NewValue = result;
                return null;
            }

            default:
            {
                var index = (int)step.Field.Value - (int)GparamMassEditField.Component1;
                var reason = ApplyComponentStep(step, planned.NewValue, index, out var result);

                if (reason != null)
                    return reason;

                planned.NewValue = result;
                return null;
            }
        }
    }

    /// <summary>
    /// An operator on a single number. Set takes the number; Toggle isn't offered for these fields.
    /// </summary>
    private static bool TryApplyScalar(GparamMassEditStep step, double current, out double result, out string reason)
    {
        result = 0;

        if (!TryParseNumber(step.Value, out var operand))
        {
            reason = LOC.Get("GPARAM_MassEditTool_Skip_Invalid_Number", step.Value);
            return false;
        }

        return TryApplyNumber(step.Operator, current, operand, out result, out reason);
    }

    private string ApplyValueStep(GparamMassEditStep step, GparamQueryMatch match, object current, out object result)
    {
        result = current;

        switch (step.Operator)
        {
            case GparamMassEditOperator.Set:
                return TryParseValue(match, current, step.Value, out result)
                    ? null
                    : LOC.Get("GPARAM_MassEditTool_Skip_Invalid_Value", step.Value, GparamUtils.GetReadableObjectTypeName(match.Field));

            case GparamMassEditOperator.Toggle:
                if (current is bool flag)
                {
                    result = !flag;
                    return null;
                }

                if (IsScalar(current) && TryToNumber(current, out var number) &&
                    TryFromNumber(number != 0 ? 0 : 1, current, out result))
                    return null;

                return LOC.Get("GPARAM_MassEditTool_Skip_Not_Toggle");
        }

        // Arithmetic: a number, or each component of a vector or colour
        if (!TryGetComponents(current, out var components))
            return LOC.Get("GPARAM_MassEditTool_Skip_Not_Number");

        var operands = GparamQueryMatcher.ParseNumbers(step.Value);

        if (operands == null)
            return LOC.Get("GPARAM_MassEditTool_Skip_Invalid_Number", step.Value);

        // One number applies to every component, or one per component applies to each in turn
        if (operands.Length != 1 && operands.Length != components.Length)
            return LOC.Get("GPARAM_MassEditTool_Skip_Component_Count", operands.Length, components.Length);

        var computed = new double[components.Length];

        for (int i = 0; i < components.Length; i++)
        {
            var operand = operands.Length == 1 ? operands[0] : operands[i];

            if (!TryApplyNumber(step.Operator, components[i], operand, out computed[i], out var reason))
                return reason;
        }

        return TrySetComponents(current, computed, out result)
            ? null
            : LOC.Get("GPARAM_MassEditTool_Skip_Out_Of_Range", string.Join(", ", computed.Select(FormatNumber)),
                GparamUtils.GetReadableObjectTypeName(match.Field));
    }

    private static string ApplyComponentStep(GparamMassEditStep step, object current, int index, out object result)
    {
        result = current;

        if (IsScalar(current) || !TryGetComponents(current, out var components))
            return LOC.Get("GPARAM_MassEditTool_Skip_Not_Vector");

        if (index >= components.Length)
            return LOC.Get("GPARAM_MassEditTool_Skip_No_Component", index + 1, components.Length);

        if (step.Operator is GparamMassEditOperator.Toggle)
            return LOC.Get("GPARAM_MassEditTool_Skip_Not_Toggle");

        if (!TryParseNumber(step.Value, out var operand))
            return LOC.Get("GPARAM_MassEditTool_Skip_Invalid_Number", step.Value);

        if (!TryApplyNumber(step.Operator, components[index], operand, out var computed, out var reason))
            return reason;

        components[index] = computed;

        return TrySetComponents(current, components, out result)
            ? null
            : LOC.Get("GPARAM_MassEditTool_Skip_Out_Of_Range", FormatNumber(computed), "component");
    }
    #endregion

    #region Duplicate
    /// <summary>
    /// The copies a duplicate adds: one of each matched value, with its ID moved by the offset, or up to the next
    /// free one if that is taken, as the add itself does
    /// </summary>
    public GparamMassDuplicatePlan PlanDuplicates(IReadOnlyList<GparamQueryMatch> matches, string offsetText)
    {
        var plan = new GparamMassDuplicatePlan();

        if (!int.TryParse(offsetText?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var offset))
        {
            plan.InvalidOffset = true;
            return plan;
        }

        plan.Offset = offset;

        foreach (var group in matches.Where(e => e.Value != null).GroupBy(e => (e.FileEntry, e.GroupKey, e.FieldKey)))
        {
            var taken = new HashSet<int>();

            foreach (var match in group)
            {
                var id = GparamUtils.NextFreeValueId(match.Field, match.Value.ID + offset, taken);
                taken.Add(id);

                plan.Duplicates.Add(new GparamPlannedDuplicate(match, id));
            }
        }

        return plan;
    }
    #endregion

    #region Values
    /// <summary>
    /// Reads typed text as a value of the same type as the current one. Vectors and colours take their components
    /// split by commas, colours as R, G, B, A. A field with an enum takes an option's name as well as its value.
    /// </summary>
    private bool TryParseValue(GparamQueryMatch match, object current, string text, out object value)
    {
        value = null;
        text = text?.Trim() ?? "";

        if (Matcher.TryGetEnumValue(match.GroupKey, match.FieldKey, text, out var enumValue))
        {
            text = enumValue;
        }

        if (current is string)
        {
            value = text;
            return true;
        }

        if (current is bool)
        {
            if (bool.TryParse(text, out var flag))
            {
                value = flag;
                return true;
            }

            if (TryParseNumber(text, out var number) && (number == 0 || number == 1))
            {
                value = number == 1;
                return true;
            }

            return false;
        }

        if (IsScalar(current))
        {
            return TryParseNumber(text, out var number) && TryFromNumber(number, current, out value);
        }

        if (!TryGetComponents(current, out var components))
            return false;

        var numbers = GparamQueryMatcher.ParseNumbers(text);

        if (numbers == null || numbers.Length != components.Length)
            return false;

        return TrySetComponents(current, numbers, out value);
    }

    private static bool IsScalar(object value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double;

    /// <summary>
    /// The numbers of a value: the value itself, or the components of a vector or colour
    /// </summary>
    private static bool TryGetComponents(object value, out double[] components)
    {
        switch (value)
        {
            case Vector2 v: components = [v.X, v.Y]; return true;
            case Vector3 v: components = [v.X, v.Y, v.Z]; return true;
            case Vector4 v: components = [v.X, v.Y, v.Z, v.W]; return true;
            case Color c: components = [c.R, c.G, c.B, c.A]; return true;
        }

        if (IsScalar(value) && TryToNumber(value, out var number))
        {
            components = [number];
            return true;
        }

        components = null;
        return false;
    }

    /// <summary>
    /// Puts numbers back into a value of the type of the one given. Colour components are rounded, and fail outside
    /// 0 to 255 rather than wrapping around.
    /// </summary>
    private static bool TrySetComponents(object like, double[] numbers, out object value)
    {
        value = null;

        switch (like)
        {
            case Vector2:
                if (!AllFloats(numbers)) return false;
                value = new Vector2((float)numbers[0], (float)numbers[1]);
                return true;

            case Vector3:
                if (!AllFloats(numbers)) return false;
                value = new Vector3((float)numbers[0], (float)numbers[1], (float)numbers[2]);
                return true;

            case Vector4:
                if (!AllFloats(numbers)) return false;
                value = new Vector4((float)numbers[0], (float)numbers[1], (float)numbers[2], (float)numbers[3]);
                return true;

            case Color:
                var bytes = new int[4];

                for (int i = 0; i < 4; i++)
                {
                    var rounded = Math.Round(numbers[i], MidpointRounding.AwayFromZero);

                    if (rounded < 0 || rounded > 255)
                        return false;

                    bytes[i] = (int)rounded;
                }

                // Stored as R, G, B, A, as it is shown
                value = Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
                return true;
        }

        return numbers.Length == 1 && TryFromNumber(numbers[0], like, out value);
    }

    private static bool AllFloats(double[] numbers) => numbers.All(e => e >= float.MinValue && e <= float.MaxValue);
    #endregion

    #region Numbers
    public static bool TryParseNumber(string text, out double number)
    {
        return GparamQueryMatcher.TryParseNumber(text, out number);
    }

    private static string FormatNumber(double number) => number.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Applies an arithmetic operator. Gives the reason it can't be applied, if it can't.
    /// </summary>
    private static bool TryApplyNumber(GparamMassEditOperator op, double current, double operand, out double result, out string reason)
    {
        reason = null;

        switch (op)
        {
            case GparamMassEditOperator.Set: result = operand; break;
            case GparamMassEditOperator.Add: result = current + operand; break;
            case GparamMassEditOperator.Subtract: result = current - operand; break;
            case GparamMassEditOperator.MultiplyBy: result = current * operand; break;
            case GparamMassEditOperator.AtLeast: result = Math.Max(current, operand); break;
            case GparamMassEditOperator.AtMost: result = Math.Min(current, operand); break;

            case GparamMassEditOperator.DivideBy:
                if (operand == 0)
                {
                    result = 0;
                    reason = LOC.Get("GPARAM_MassEditTool_Skip_Divide_By_Zero");
                    return false;
                }

                result = current / operand;
                break;

            default:
                result = 0;
                reason = LOC.Get("GPARAM_MassEditTool_Skip_Not_Toggle");
                return false;
        }

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            reason = LOC.Get("GPARAM_MassEditTool_Skip_Out_Of_Range", FormatNumber(result), "number");
            return false;
        }

        return true;
    }

    private static bool TryToNumber(object value, out double number)
    {
        if (IsScalar(value))
        {
            number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return true;
        }

        number = 0;
        return false;
    }

    /// <summary>
    /// Puts a number back into the type of the value it came from. Whole number types are rounded, and a number
    /// outside the range of the type fails rather than wrapping around.
    /// </summary>
    private static bool TryFromNumber(double number, object like, out object value)
    {
        value = null;

        double rounded = Math.Round(number, MidpointRounding.AwayFromZero);

        switch (like)
        {
            case byte: if (rounded < byte.MinValue || rounded > byte.MaxValue) return false; value = (byte)rounded; return true;
            case sbyte: if (rounded < sbyte.MinValue || rounded > sbyte.MaxValue) return false; value = (sbyte)rounded; return true;
            case short: if (rounded < short.MinValue || rounded > short.MaxValue) return false; value = (short)rounded; return true;
            case ushort: if (rounded < ushort.MinValue || rounded > ushort.MaxValue) return false; value = (ushort)rounded; return true;
            case int: if (rounded < int.MinValue || rounded > int.MaxValue) return false; value = (int)rounded; return true;
            case uint: if (rounded < uint.MinValue || rounded > uint.MaxValue) return false; value = (uint)rounded; return true;

            // The edges of the 64-bit ranges can't be told apart as doubles, so they are kept clear of
            case long: if (rounded < -9.2e18 || rounded > 9.2e18) return false; value = (long)rounded; return true;
            case ulong: if (rounded < 0 || rounded > 1.8e19) return false; value = (ulong)rounded; return true;

            case float:
                if (number < float.MinValue || number > float.MaxValue) return false;
                value = (float)number;
                return true;

            case double:
                value = number;
                return true;
        }

        return false;
    }
    #endregion
}
