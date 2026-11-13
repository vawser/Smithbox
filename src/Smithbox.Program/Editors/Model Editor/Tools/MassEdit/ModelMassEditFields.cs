using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// What an edit step does to its field
/// </summary>
public enum ModelMassEditOperator
{
    Set,
    Add,
    Subtract,
    MultiplyBy,
    DivideBy,
    AtLeast,
    AtMost,
    Toggle,
    Replace
}

/// <summary>
/// The kind of field a step edits, which decides the operators offered
/// </summary>
public enum ModelMassEditFieldKind
{
    Number,
    Bool,
    Enum,
    Vector,
    Text,
    Colour,

    /// <summary>
    /// A field the search reads but the mass edit can't set, such as a list of indices
    /// </summary>
    ReadOnly
}

/// <summary>
/// One step of an edit: Set [Field] [Operator] [Value]. The steps are applied in order, each to the value the step
/// before it left. Value2 is the replacement text of Replace.
/// </summary>
public record ModelMassEditStep(string Field, ModelMassEditOperator Operator, string Value, string Value2)
{
    public bool HasField => !string.IsNullOrWhiteSpace(Field);
}

/// <summary>
/// A field of a FLVER object found by its path (Textures[0].Path): the object holding it and its property
/// </summary>
public readonly record struct ModelFieldTarget(object Owner, PropertyInfo Property)
{
    public object Get() => Property.GetValue(Owner);
}

/// <summary>
/// The fields the mass edit can set, found by the paths the search names them by, and the operators on their values
/// </summary>
public static class ModelMassEditFields
{
    #region Paths
    /// <summary>
    /// Finds the property a path names, walking nested objects and list elements. False if the path doesn't lead to
    /// a property of this object (a material with fewer textures, say).
    /// </summary>
    public static bool TryResolve(object root, string path, out ModelFieldTarget target)
    {
        target = default;

        if (root == null || string.IsNullOrEmpty(path))
            return false;

        var segments = path.Split('.');
        var current = root;

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var index = -1;

            var bracket = segment.IndexOf('[');

            if (bracket >= 0)
            {
                if (!int.TryParse(segment[(bracket + 1)..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                    return false;

                segment = segment[..bracket];
            }

            var property = current.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance);

            if (property == null || !property.CanRead)
                return false;

            var last = i == segments.Length - 1;

            if (last && index < 0)
            {
                target = new ModelFieldTarget(current, property);
                return true;
            }

            var value = property.GetValue(current);

            if (index >= 0)
            {
                if (value is not IList list || index >= list.Count)
                    return false;

                value = list[index];
            }

            // An element of a list of numbers is not a field of its own
            if (last || value == null)
                return false;

            current = value;
        }

        return false;
    }

    public static ModelMassEditFieldKind GetKind(PropertyInfo property)
    {
        if (property.SetMethod is not { IsPublic: true })
            return ModelMassEditFieldKind.ReadOnly;

        var type = property.PropertyType;

        if (type == typeof(bool))
            return ModelMassEditFieldKind.Bool;

        if (type.IsEnum)
            return ModelMassEditFieldKind.Enum;

        if (type == typeof(string))
            return ModelMassEditFieldKind.Text;

        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Quaternion))
            return ModelMassEditFieldKind.Vector;

        if (type == typeof(System.Drawing.Color))
            return ModelMassEditFieldKind.Colour;

        if (IsNumberType(type))
            return ModelMassEditFieldKind.Number;

        return ModelMassEditFieldKind.ReadOnly;
    }

    private static bool IsNumberType(Type type) => type == typeof(byte) || type == typeof(sbyte) ||
        type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) ||
        type == typeof(long) || type == typeof(ulong) || type == typeof(float) || type == typeof(double);
    #endregion

    #region Operators
    public static IReadOnlyList<ModelMassEditOperator> GetOperators(ModelMassEditFieldKind kind) => kind switch
    {
        ModelMassEditFieldKind.Number =>
        [
            ModelMassEditOperator.Set, ModelMassEditOperator.Add, ModelMassEditOperator.Subtract,
            ModelMassEditOperator.MultiplyBy, ModelMassEditOperator.DivideBy,
            ModelMassEditOperator.AtLeast, ModelMassEditOperator.AtMost
        ],
        ModelMassEditFieldKind.Vector =>
        [
            ModelMassEditOperator.Set, ModelMassEditOperator.Add, ModelMassEditOperator.Subtract,
            ModelMassEditOperator.MultiplyBy, ModelMassEditOperator.DivideBy
        ],
        ModelMassEditFieldKind.Bool => [ModelMassEditOperator.Set, ModelMassEditOperator.Toggle],
        ModelMassEditFieldKind.Text => [ModelMassEditOperator.Set, ModelMassEditOperator.Replace],
        ModelMassEditFieldKind.Enum or ModelMassEditFieldKind.Colour => [ModelMassEditOperator.Set],
        _ => []
    };

    public static bool TakesValue(ModelMassEditOperator op) => op is not ModelMassEditOperator.Toggle;

    /// <summary>
    /// Works out the new value of a field. Returns the reason the step can't be applied, or null.
    /// </summary>
    public static string TryApply(ModelMassEditOperator op, Type type, object current, string text, string text2,
        out object result)
    {
        result = null;
        text = text?.Trim() ?? "";

        switch (GetKindOfType(type))
        {
            case ModelMassEditFieldKind.Bool:
                if (op is ModelMassEditOperator.Toggle)
                {
                    result = !(bool)current;
                    return null;
                }

                if (TryParseBool(text, out var flag))
                {
                    result = flag;
                    return null;
                }

                return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Value", text, "true / false");

            case ModelMassEditFieldKind.Enum:
                if (TryParseEnum(type, text, out var enumValue))
                {
                    result = enumValue;
                    return null;
                }

                return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Value", text, type.Name);

            case ModelMassEditFieldKind.Text:
                if (op is ModelMassEditOperator.Replace)
                {
                    if (string.IsNullOrEmpty(text))
                        return LOC.Get("MODEL_MassEditTool_Skip_Replace_Empty");

                    result = (current as string ?? "").Replace(text, text2 ?? "", StringComparison.OrdinalIgnoreCase);
                    return null;
                }

                result = text;
                return null;

            case ModelMassEditFieldKind.Colour:
                if (TryParseNumbers(text, out var parts) && parts.Length is 3 or 4 &&
                    Array.TrueForAll(parts, e => e >= 0 && e <= 255 && e == Math.Floor(e)))
                {
                    var alpha = parts.Length == 4 ? (int)parts[3] : ((System.Drawing.Color)current).A;
                    result = System.Drawing.Color.FromArgb(alpha, (int)parts[0], (int)parts[1], (int)parts[2]);
                    return null;
                }

                return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Value", text, "R, G, B, A");

            case ModelMassEditFieldKind.Vector:
                return TryApplyVector(op, type, current, text, out result);

            case ModelMassEditFieldKind.Number:
                if (!ModelQueryMatcher.TryParseNumber(text, out var operand))
                    return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Value", text, type.Name);

                var number = Convert.ToDouble(current, CultureInfo.InvariantCulture);

                var reason = TryApplyNumber(op, number, operand, out var applied);

                if (reason != null)
                    return reason;

                if (!TryFromNumber(applied, type, out result))
                    return LOC.Get("MODEL_MassEditTool_Skip_Out_Of_Range", FormatNumber(applied), type.Name);

                return null;
        }

        return LOC.Get("MODEL_MassEditTool_Skip_Read_Only");
    }

    private static ModelMassEditFieldKind GetKindOfType(Type type)
    {
        if (type == typeof(bool)) return ModelMassEditFieldKind.Bool;
        if (type.IsEnum) return ModelMassEditFieldKind.Enum;
        if (type == typeof(string)) return ModelMassEditFieldKind.Text;
        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Quaternion))
            return ModelMassEditFieldKind.Vector;
        if (type == typeof(System.Drawing.Color)) return ModelMassEditFieldKind.Colour;
        if (IsNumberType(type)) return ModelMassEditFieldKind.Number;
        return ModelMassEditFieldKind.ReadOnly;
    }

    /// <summary>
    /// Vectors are set from x, y, z; arithmetic takes either one number for every component, or one per component
    /// </summary>
    private static string TryApplyVector(ModelMassEditOperator op, Type type, object current, string text, out object result)
    {
        result = null;

        var components = ToComponents(current);

        if (!TryParseNumbers(text, out var operands) ||
            (operands.Length != components.Length && (op is ModelMassEditOperator.Set || operands.Length != 1)))
        {
            return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Value", text,
                string.Join(", ", new[] { "x", "y", "z", "w" }[..components.Length]));
        }

        var values = new double[components.Length];

        for (int i = 0; i < components.Length; i++)
        {
            var operand = operands.Length == 1 ? operands[0] : operands[i];
            var reason = TryApplyNumber(op, components[i], operand, out values[i]);

            if (reason != null)
                return reason;

            if (values[i] < float.MinValue || values[i] > float.MaxValue)
                return LOC.Get("MODEL_MassEditTool_Skip_Out_Of_Range", FormatNumber(values[i]), "float");
        }

        result = FromComponents(type, values);
        return null;
    }

    private static double[] ToComponents(object value) => value switch
    {
        Vector2 v => [v.X, v.Y],
        Vector3 v => [v.X, v.Y, v.Z],
        Vector4 v => [v.X, v.Y, v.Z, v.W],
        Quaternion q => [q.X, q.Y, q.Z, q.W],
        _ => []
    };

    private static object FromComponents(Type type, double[] c)
    {
        if (type == typeof(Vector2)) return new Vector2((float)c[0], (float)c[1]);
        if (type == typeof(Vector3)) return new Vector3((float)c[0], (float)c[1], (float)c[2]);
        if (type == typeof(Vector4)) return new Vector4((float)c[0], (float)c[1], (float)c[2], (float)c[3]);
        return new Quaternion((float)c[0], (float)c[1], (float)c[2], (float)c[3]);
    }
    #endregion

    #region Parsing
    private static bool TryParseBool(string text, out bool value)
    {
        if (bool.TryParse(text, out value))
            return true;

        if (text == "1" || text == "0")
        {
            value = text == "1";
            return true;
        }

        return false;
    }

    /// <summary>
    /// An enum value by its name, or by its number (so flag combinations can be given)
    /// </summary>
    private static bool TryParseEnum(Type type, string text, out object value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            try
            {
                value = Enum.ToObject(type, number);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return Enum.TryParse(type, text, true, out value);
    }

    private static bool TryParseNumbers(string text, out double[] numbers)
    {
        numbers = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var parts = text.Trim('<', '>', '(', ')', ' ').Split(',');
        var result = new double[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (!ModelQueryMatcher.TryParseNumber(parts[i].Trim(), out result[i]))
                return false;
        }

        numbers = result;
        return true;
    }

    public static string FormatNumber(double number) => number.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// A value as the preview shows it: enums by name and number
    /// </summary>
    public static string FormatValue(object value) => value is Enum e
        ? $"{e} ({Convert.ToInt64(e, CultureInfo.InvariantCulture)})"
        : ModelQueryMatcher.FormatValue(value);
    #endregion

    #region Numbers
    private static string TryApplyNumber(ModelMassEditOperator op, double current, double operand, out double result)
    {
        switch (op)
        {
            case ModelMassEditOperator.Set: result = operand; break;
            case ModelMassEditOperator.Add: result = current + operand; break;
            case ModelMassEditOperator.Subtract: result = current - operand; break;
            case ModelMassEditOperator.MultiplyBy: result = current * operand; break;
            case ModelMassEditOperator.AtLeast: result = Math.Max(current, operand); break;
            case ModelMassEditOperator.AtMost: result = Math.Min(current, operand); break;

            case ModelMassEditOperator.DivideBy:
                if (operand == 0)
                {
                    result = 0;
                    return LOC.Get("MODEL_MassEditTool_Skip_Divide_By_Zero");
                }

                result = current / operand;
                break;

            default:
                result = 0;
                return LOC.Get("MODEL_MassEditTool_Skip_Invalid_Operator");
        }

        if (double.IsNaN(result) || double.IsInfinity(result))
            return LOC.Get("MODEL_MassEditTool_Skip_Out_Of_Range", FormatNumber(result), "number");

        return null;
    }

    /// <summary>
    /// Puts a number back into the type of the field. Whole number types are rounded, and a number outside the range
    /// of the type fails rather than wrapping around.
    /// </summary>
    private static bool TryFromNumber(double number, Type type, out object value)
    {
        value = null;

        double rounded = Math.Round(number, MidpointRounding.AwayFromZero);

        if (type == typeof(byte)) { if (rounded < byte.MinValue || rounded > byte.MaxValue) return false; value = (byte)rounded; return true; }
        if (type == typeof(sbyte)) { if (rounded < sbyte.MinValue || rounded > sbyte.MaxValue) return false; value = (sbyte)rounded; return true; }
        if (type == typeof(short)) { if (rounded < short.MinValue || rounded > short.MaxValue) return false; value = (short)rounded; return true; }
        if (type == typeof(ushort)) { if (rounded < ushort.MinValue || rounded > ushort.MaxValue) return false; value = (ushort)rounded; return true; }
        if (type == typeof(int)) { if (rounded < int.MinValue || rounded > int.MaxValue) return false; value = (int)rounded; return true; }
        if (type == typeof(uint)) { if (rounded < uint.MinValue || rounded > uint.MaxValue) return false; value = (uint)rounded; return true; }

        // The edges of the 64-bit ranges can't be told apart as doubles, so they are kept clear of
        if (type == typeof(long)) { if (rounded < -9.2e18 || rounded > 9.2e18) return false; value = (long)rounded; return true; }
        if (type == typeof(ulong)) { if (rounded < 0 || rounded > 1.8e19) return false; value = (ulong)rounded; return true; }

        if (type == typeof(float))
        {
            if (number < float.MinValue || number > float.MaxValue) return false;
            value = (float)number;
            return true;
        }

        if (type == typeof(double))
        {
            value = number;
            return true;
        }

        return false;
    }
    #endregion
}
