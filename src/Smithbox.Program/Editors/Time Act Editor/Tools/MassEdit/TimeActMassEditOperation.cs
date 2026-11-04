using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What the mass edit does to its matches
/// </summary>
public enum TimeActMassEditAction
{
    Edit
}

/// <summary>
/// What an edit step does to its field
/// </summary>
public enum TimeActMassEditOperator
{
    Set,
    Add,
    Subtract,
    MultiplyBy,
    DivideBy,
    AtLeast,
    AtMost,
    ResetToDefault,
    Toggle,

    // Both times at once
    Shift,
    Scale,
    SetLength
}

/// <summary>
/// The kind of field a step edits, which decides the operators offered
/// </summary>
public enum TimeActMassEditFieldKind
{
    /// <summary>
    /// The start or end time
    /// </summary>
    Time,

    /// <summary>
    /// The start and end time together
    /// </summary>
    Times,

    Number,
    NumberBool,
    Bool,
    Enum,

    /// <summary>
    /// A value that isn't a single number, such as bytes or a gradient, which can only be set
    /// </summary>
    Other
}

/// <summary>
/// One step of an edit: Set [Field] [Operator] [Value]. The steps of an edit are applied in order, each to the
/// value the step before it left. Frames means the value of a time step is in frames rather than seconds.
/// </summary>
public record TimeActMassEditStep(string Field, TimeActMassEditOperator Operator, string Value, bool Frames)
{
    public bool HasField => !string.IsNullOrWhiteSpace(Field);
}

public static class TimeActMassEditFields
{
    public const string StartTime = "StartTime";
    public const string EndTime = "EndTime";

    /// <summary>
    /// The start and end time together, which can be shifted or scaled as one
    /// </summary>
    public const string Times = "Times";

    /// <summary>
    /// Event times are kept in seconds, at 30 frames to the second in every game
    /// </summary>
    public const float FrameDuration = 1f / 30f;

    public static bool IsTimeField(string field) => field is StartTime or EndTime or Times;

    public static TimeActMassEditFieldKind GetKind(string field, TAE.Template.ParameterTemplate parameter, TimeActField meta)
    {
        if (field is Times)
            return TimeActMassEditFieldKind.Times;

        if (field is StartTime or EndTime)
            return TimeActMassEditFieldKind.Time;

        if (parameter == null)
            return TimeActMassEditFieldKind.Number;

        if (parameter.EnumEntries != null)
            return TimeActMassEditFieldKind.Enum;

        switch (parameter.Type)
        {
            case TAE.Template.ParamType.b:
                return TimeActMassEditFieldKind.Bool;

            case TAE.Template.ParamType.aob:
            case TAE.Template.ParamType.f32grad:
                return TimeActMassEditFieldKind.Other;
        }

        return meta?.IsBool == true ? TimeActMassEditFieldKind.NumberBool : TimeActMassEditFieldKind.Number;
    }

    /// <summary>
    /// The operators that make sense for a kind of field
    /// </summary>
    public static IReadOnlyList<TimeActMassEditOperator> GetOperators(TimeActMassEditFieldKind kind) => kind switch
    {
        TimeActMassEditFieldKind.Time =>
        [
            TimeActMassEditOperator.Set, TimeActMassEditOperator.Add, TimeActMassEditOperator.Subtract,
            TimeActMassEditOperator.MultiplyBy, TimeActMassEditOperator.DivideBy,
            TimeActMassEditOperator.AtLeast, TimeActMassEditOperator.AtMost
        ],
        TimeActMassEditFieldKind.Times =>
        [
            TimeActMassEditOperator.Shift, TimeActMassEditOperator.Scale, TimeActMassEditOperator.SetLength
        ],
        TimeActMassEditFieldKind.Number =>
        [
            TimeActMassEditOperator.Set, TimeActMassEditOperator.Add, TimeActMassEditOperator.Subtract,
            TimeActMassEditOperator.MultiplyBy, TimeActMassEditOperator.DivideBy,
            TimeActMassEditOperator.AtLeast, TimeActMassEditOperator.AtMost,
            TimeActMassEditOperator.ResetToDefault
        ],
        TimeActMassEditFieldKind.NumberBool or TimeActMassEditFieldKind.Bool =>
        [
            TimeActMassEditOperator.Set, TimeActMassEditOperator.Toggle, TimeActMassEditOperator.ResetToDefault
        ],
        _ =>
        [
            TimeActMassEditOperator.Set, TimeActMassEditOperator.ResetToDefault
        ]
    };

    /// <summary>
    /// Whether an operator takes a value
    /// </summary>
    public static bool TakesValue(TimeActMassEditOperator op) =>
        op is not (TimeActMassEditOperator.ResetToDefault or TimeActMassEditOperator.Toggle);

    /// <summary>
    /// Whether the value of an operator is a time, which can be given in frames. Scaling takes a factor.
    /// </summary>
    public static bool TakesTime(TimeActMassEditOperator op) =>
        op is TimeActMassEditOperator.Set or TimeActMassEditOperator.Add or TimeActMassEditOperator.Subtract or
            TimeActMassEditOperator.AtLeast or TimeActMassEditOperator.AtMost or
            TimeActMassEditOperator.Shift or TimeActMassEditOperator.SetLength;
}
