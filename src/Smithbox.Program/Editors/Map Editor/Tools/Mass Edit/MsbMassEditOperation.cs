using StudioCore.Application;
using System;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// What a mass edit's find goes through, from narrowest to widest
/// </summary>
public enum MsbMassEditScope
{
    SelectedObjects,
    SelectedMap,
    LoadedMaps,
    AllMaps
}

public enum MsbMassEditAction
{
    Edit,
    Delete
}

public enum MsbMassEditOperator
{
    Set,
    Add,
    Subtract,
    MultiplyBy,
    DivideBy,
    AtLeast,
    AtMost,
    SetRandom,
    AddRandom,
    Toggle,
    Prefix,
    Suffix,
    Replace
}

/// <summary>
/// The kinds of value a step can change, which decide the operators it offers
/// </summary>
public enum MsbMassEditFieldKind
{
    Unsupported,
    Number,
    Vector,
    Bool,
    Enum,
    Text
}

/// <summary>
/// A condition on the fields of a map object: some field at the path must compare with the value
/// </summary>
public record MsbMassEditCondition(string Field, MsbQueryComparison Comparison, string Value, string Value2)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Field) && string.IsNullOrWhiteSpace(Value) &&
        string.IsNullOrWhiteSpace(Value2);
}

/// <summary>
/// One step of an edit: Set [field] [operator] [value]. Value2 is the upper end of a random range, or what Replace
/// puts in place of Value.
/// </summary>
public record MsbMassEditStep(string Field, MsbMassEditOperator Operator, string Value, string Value2)
{
    public static readonly MsbMassEditOperator[] NumberOperators =
    [
        MsbMassEditOperator.Set, MsbMassEditOperator.Add, MsbMassEditOperator.Subtract,
        MsbMassEditOperator.MultiplyBy, MsbMassEditOperator.DivideBy,
        MsbMassEditOperator.AtLeast, MsbMassEditOperator.AtMost,
        MsbMassEditOperator.SetRandom, MsbMassEditOperator.AddRandom
    ];

    public static readonly MsbMassEditOperator[] BoolOperators = [MsbMassEditOperator.Set, MsbMassEditOperator.Toggle];

    public static readonly MsbMassEditOperator[] EnumOperators = [MsbMassEditOperator.Set];

    public static readonly MsbMassEditOperator[] TextOperators =
    [
        MsbMassEditOperator.Set, MsbMassEditOperator.Prefix, MsbMassEditOperator.Suffix, MsbMassEditOperator.Replace
    ];

    public static MsbMassEditOperator[] GetOperators(MsbMassEditFieldKind kind) => kind switch
    {
        MsbMassEditFieldKind.Number or MsbMassEditFieldKind.Vector => NumberOperators,
        MsbMassEditFieldKind.Bool => BoolOperators,
        MsbMassEditFieldKind.Enum => EnumOperators,
        MsbMassEditFieldKind.Text => TextOperators,
        _ => [MsbMassEditOperator.Set]
    };

    public static bool TakesValue(MsbMassEditOperator op) => op is not MsbMassEditOperator.Toggle;

    public static bool TakesTwoValues(MsbMassEditOperator op) =>
        op is MsbMassEditOperator.SetRandom or MsbMassEditOperator.AddRandom or MsbMassEditOperator.Replace;

    public static MsbMassEditFieldKind GetKind(Type type)
    {
        if (type == null)
            return MsbMassEditFieldKind.Unsupported;

        if (type.IsEnum)
            return MsbMassEditFieldKind.Enum;

        if (type == typeof(bool))
            return MsbMassEditFieldKind.Bool;

        if (type == typeof(string))
            return MsbMassEditFieldKind.Text;

        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4))
            return MsbMassEditFieldKind.Vector;

        return Type.GetTypeCode(type) switch
        {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or
            TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double => MsbMassEditFieldKind.Number,
            _ => MsbMassEditFieldKind.Unsupported
        };
    }

    /// <summary>
    /// Works out the value after this step. False with a reason if the step can't apply to the value.
    /// Random values are drawn from the given generator.
    /// </summary>
    public bool TryApply(Type type, object value, Random random, out object result, out string reason)
    {
        result = value;
        reason = null;

        var kind = GetKind(type);

        if (Array.IndexOf(GetOperators(kind), Operator) < 0)
        {
            reason = LOC.Get("MAP_MassEditTool_Skip_Operator", type?.Name ?? "?");
            return false;
        }

        switch (kind)
        {
            case MsbMassEditFieldKind.Number:
                return TryApplyNumber(type, value, random, out result, out reason);

            case MsbMassEditFieldKind.Vector:
                return TryApplyVector(type, value, random, out result, out reason);

            case MsbMassEditFieldKind.Bool:
                if (Operator is MsbMassEditOperator.Toggle)
                {
                    result = !(bool)value;
                    return true;
                }

                if (!TryParseBool(Value, out var flag))
                {
                    reason = LOC.Get("MAP_MassEditTool_Skip_Not_Bool", Value);
                    return false;
                }

                result = flag;
                return true;

            case MsbMassEditFieldKind.Enum:
                if (!TryParseEnum(type, Value, out result))
                {
                    reason = LOC.Get("MAP_MassEditTool_Skip_Not_Enum", Value, type.Name);
                    return false;
                }

                return true;

            case MsbMassEditFieldKind.Text:
                var text = (string)value ?? "";

                switch (Operator)
                {
                    case MsbMassEditOperator.Set:
                        result = Value ?? "";
                        return true;
                    case MsbMassEditOperator.Prefix:
                        result = $"{Value}{text}";
                        return true;
                    case MsbMassEditOperator.Suffix:
                        result = $"{text}{Value}";
                        return true;
                    case MsbMassEditOperator.Replace:
                        if (string.IsNullOrEmpty(Value))
                        {
                            reason = LOC.Get("MAP_MassEditTool_Skip_Replace_Empty");
                            return false;
                        }

                        result = text.Replace(Value, Value2 ?? "", StringComparison.Ordinal);
                        return true;
                }

                break;
        }

        reason = LOC.Get("MAP_MassEditTool_Skip_Unsupported", type?.Name ?? "?");
        return false;
    }

    private bool TryApplyNumber(Type type, object value, Random random, out object result, out string reason)
    {
        result = value;
        reason = null;

        var current = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        var isWhole = IsWholeNumber(type);

        double next;

        if (Operator is MsbMassEditOperator.SetRandom or MsbMassEditOperator.AddRandom)
        {
            if (!TryGetRange(out var low, out var high, out reason))
                return false;

            // Whole numbers are drawn from the whole numbers in the range, both ends included
            var drawn = isWhole
                ? Math.Floor(Math.Ceiling(low) + random.NextDouble() * (Math.Floor(high) - Math.Ceiling(low) + 1))
                : low + random.NextDouble() * (high - low);

            if (isWhole)
                drawn = Math.Min(drawn, Math.Floor(high));

            next = Operator is MsbMassEditOperator.SetRandom ? drawn : current + drawn;
        }
        else
        {
            if (!MsbQueryMatcher.TryParseNumber(Value, out var operand))
            {
                reason = LOC.Get("MAP_MassEditTool_Skip_Not_Number", Value);
                return false;
            }

            if (Operator is MsbMassEditOperator.DivideBy && operand == 0)
            {
                reason = LOC.Get("MAP_MassEditTool_Skip_Divide_By_Zero");
                return false;
            }

            next = Calculate(current, operand);
        }

        return TryConvertNumber(type, next, out result, out reason);
    }

    private bool TryApplyVector(Type type, object value, Random random, out object result, out string reason)
    {
        result = value;
        reason = null;

        var components = GetComponents(value);

        if (Operator is MsbMassEditOperator.SetRandom or MsbMassEditOperator.AddRandom)
        {
            if (!TryGetRange(out var low, out var high, out reason))
                return false;

            for (int i = 0; i < components.Length; i++)
            {
                var drawn = low + random.NextDouble() * (high - low);

                components[i] = Operator is MsbMassEditOperator.SetRandom ? drawn : components[i] + drawn;
            }
        }
        else
        {
            var operands = MsbQueryMatcher.ParseNumbers(Value);

            // One number applies to every component, or one is given per component
            if (operands == null || (operands.Length != 1 && operands.Length != components.Length))
            {
                reason = LOC.Get("MAP_MassEditTool_Skip_Not_Vector", Value, components.Length);
                return false;
            }

            for (int i = 0; i < components.Length; i++)
            {
                var operand = operands.Length == 1 ? operands[0] : operands[i];

                if (Operator is MsbMassEditOperator.DivideBy && operand == 0)
                {
                    reason = LOC.Get("MAP_MassEditTool_Skip_Divide_By_Zero");
                    return false;
                }

                components[i] = Calculate(components[i], operand);
            }
        }

        foreach (var component in components)
        {
            if (double.IsNaN(component) || double.IsInfinity(component) || Math.Abs(component) > float.MaxValue)
            {
                reason = LOC.Get("MAP_MassEditTool_Skip_Out_Of_Range", type.Name);
                return false;
            }
        }

        result = type == typeof(Vector2) ? new Vector2((float)components[0], (float)components[1])
            : type == typeof(Vector3) ? new Vector3((float)components[0], (float)components[1], (float)components[2])
            : new Vector4((float)components[0], (float)components[1], (float)components[2], (float)components[3]);

        return true;
    }

    private double Calculate(double current, double operand) => Operator switch
    {
        MsbMassEditOperator.Set => operand,
        MsbMassEditOperator.Add => current + operand,
        MsbMassEditOperator.Subtract => current - operand,
        MsbMassEditOperator.MultiplyBy => current * operand,
        MsbMassEditOperator.DivideBy => current / operand,
        MsbMassEditOperator.AtLeast => Math.Max(current, operand),
        MsbMassEditOperator.AtMost => Math.Min(current, operand),
        _ => current
    };

    /// <summary>
    /// The range of a random operator. Its ends can be given either way round.
    /// </summary>
    private bool TryGetRange(out double low, out double high, out string reason)
    {
        reason = null;
        high = 0;

        if (!MsbQueryMatcher.TryParseNumber(Value, out low) || !MsbQueryMatcher.TryParseNumber(Value2, out high))
        {
            reason = LOC.Get("MAP_MassEditTool_Skip_Not_Range", Value, Value2);
            return false;
        }

        if (low > high)
            (low, high) = (high, low);

        return true;
    }

    private static double[] GetComponents(object value) => value switch
    {
        Vector2 v => [v.X, v.Y],
        Vector3 v => [v.X, v.Y, v.Z],
        Vector4 v => [v.X, v.Y, v.Z, v.W],
        _ => []
    };

    private static bool IsWholeNumber(Type type) =>
        Type.GetTypeCode(type) is not (TypeCode.Single or TypeCode.Double);

    /// <summary>
    /// Converts a worked-out number to the field's type. Whole numbers are rounded; a value outside the type's range
    /// is refused rather than wrapped.
    /// </summary>
    public static bool TryConvertNumber(Type type, double value, out object result, out string reason)
    {
        result = null;
        reason = null;

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            reason = LOC.Get("MAP_MassEditTool_Skip_Out_Of_Range", type.Name);
            return false;
        }

        if (IsWholeNumber(type))
        {
            value = Math.Round(value, MidpointRounding.AwayFromZero);
        }

        var (min, max) = Type.GetTypeCode(type) switch
        {
            TypeCode.SByte => Range(sbyte.MinValue, sbyte.MaxValue),
            TypeCode.Byte => Range(byte.MinValue, byte.MaxValue),
            TypeCode.Int16 => Range(short.MinValue, short.MaxValue),
            TypeCode.UInt16 => Range(ushort.MinValue, ushort.MaxValue),
            TypeCode.Int32 => Range(int.MinValue, int.MaxValue),
            TypeCode.UInt32 => Range(uint.MinValue, uint.MaxValue),
            TypeCode.Int64 => Range(long.MinValue, long.MaxValue),
            TypeCode.UInt64 => Range(ulong.MinValue, ulong.MaxValue),
            TypeCode.Single => Range(float.MinValue, float.MaxValue),
            _ => Range(double.MinValue, double.MaxValue)
        };

        if (value < min || value > max)
        {
            reason = LOC.Get("MAP_MassEditTool_Skip_Out_Of_Range", type.Name);
            return false;
        }

        result = Type.GetTypeCode(type) switch
        {
            TypeCode.SByte => (sbyte)value,
            TypeCode.Byte => (byte)value,
            TypeCode.Int16 => (short)value,
            TypeCode.UInt16 => (ushort)value,
            TypeCode.Int32 => (int)value,
            TypeCode.UInt32 => (uint)value,
            TypeCode.Int64 => (long)value,
            TypeCode.UInt64 => (ulong)value,
            TypeCode.Single => (float)value,
            _ => (object)value
        };

        return true;
    }

    private static (double Min, double Max) Range(double min, double max) => (min, max);

    private static bool TryParseBool(string text, out bool value)
    {
        value = false;
        text = text?.Trim() ?? "";

        if (bool.TryParse(text, out value))
            return true;

        if (text == "1")
        {
            value = true;
            return true;
        }

        return text == "0";
    }

    /// <summary>
    /// An enum value by its name, or by the number it stands for
    /// </summary>
    public static bool TryParseEnum(Type type, string text, out object value)
    {
        value = null;
        text = text?.Trim() ?? "";

        if (text == "")
            return false;

        if (Enum.TryParse(type, text, true, out value) && value != null)
            return true;

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            value = Enum.ToObject(type, number);
            return true;
        }

        return false;
    }
}
