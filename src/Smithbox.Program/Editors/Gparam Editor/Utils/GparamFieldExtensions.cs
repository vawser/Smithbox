using Google.Protobuf.WellKnownTypes;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public static class GparamFieldExtensions
{
    // -------------------------------------------------------------------------
    // GPARAM.Param extensions
    // -------------------------------------------------------------------------

    public static GPARAM.IField CloneField(this GPARAM.Param param, GPARAM.IField field)
    {
        return field switch
        {
            GPARAM.SbyteField f => CloneTypedField(f),
            GPARAM.ShortField f => CloneTypedField(f),
            GPARAM.IntField f => CloneTypedField(f),
            GPARAM.LongField f => CloneTypedField(f),
            GPARAM.ByteField f => CloneTypedField(f),
            GPARAM.UshortField f => CloneTypedField(f),
            GPARAM.UintField f => CloneTypedField(f),
            GPARAM.UlongField f => CloneTypedField(f),
            GPARAM.FloatField f => CloneTypedField(f),
            GPARAM.DoubleField f => CloneTypedField(f),
            GPARAM.BoolField f => CloneTypedField(f),
            GPARAM.Vector2Field f => CloneTypedField(f),
            GPARAM.Vector3Field f => CloneTypedField(f),
            GPARAM.Vector4Field f => CloneTypedField(f),
            GPARAM.ColorField f => CloneTypedField(f),
            GPARAM.StringField f => CloneTypedField(f),
            _ => throw new NotSupportedException($"Cannot clone unknown field type: {field.GetType().Name}")
        };
    }

    // -------------------------------------------------------------------------
    // GPARAM.IField extensions
    // -------------------------------------------------------------------------
    public static int IndexOfValue(this GPARAM.IField field, GPARAM.IFieldValue value)
    {
        var list = field.Values;
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], value))
                return i;
        }
        return -1;
    }

    public static GPARAM.IFieldValue CloneValue(this GPARAM.IField field, GPARAM.IFieldValue value)
    {
        return field switch
        {
            GPARAM.SbyteField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<sbyte>)value)].Clone(),
            GPARAM.ShortField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<short>)value)].Clone(),
            GPARAM.IntField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<int>)value)].Clone(),
            GPARAM.LongField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<long>)value)].Clone(),
            GPARAM.ByteField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<byte>)value)].Clone(),
            GPARAM.UshortField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<ushort>)value)].Clone(),
            GPARAM.UintField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<uint>)value)].Clone(),
            GPARAM.UlongField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<ulong>)value)].Clone(),
            GPARAM.FloatField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<float>)value)].Clone(),
            GPARAM.DoubleField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<double>)value)].Clone(),
            GPARAM.BoolField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<bool>)value)].Clone(),
            GPARAM.Vector2Field f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<System.Numerics.Vector2>)value)].Clone(),
            GPARAM.Vector3Field f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<System.Numerics.Vector3>)value)].Clone(),
            GPARAM.Vector4Field f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<System.Numerics.Vector4>)value)].Clone(),
            GPARAM.ColorField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<System.Drawing.Color>)value)].Clone(),
            GPARAM.StringField f => f.Values[f.Values.IndexOf((GPARAM.FieldValue<string>)value)].Clone(),
            _ => throw new NotSupportedException($"Cannot clone value for unknown field type: {field.GetType().Name}")
        };
    }

    public static void SetValue(this GPARAM.IField field, int index, GPARAM.IFieldValue newValue)
    {
        switch (field)
        {
            case GPARAM.SbyteField f: f.Values[index] = (GPARAM.FieldValue<sbyte>)newValue; break;
            case GPARAM.ShortField f: f.Values[index] = (GPARAM.FieldValue<short>)newValue; break;
            case GPARAM.IntField f: f.Values[index] = (GPARAM.FieldValue<int>)newValue; break;
            case GPARAM.LongField f: f.Values[index] = (GPARAM.FieldValue<long>)newValue; break;
            case GPARAM.ByteField f: f.Values[index] = (GPARAM.FieldValue<byte>)newValue; break;
            case GPARAM.UshortField f: f.Values[index] = (GPARAM.FieldValue<ushort>)newValue; break;
            case GPARAM.UintField f: f.Values[index] = (GPARAM.FieldValue<uint>)newValue; break;
            case GPARAM.UlongField f: f.Values[index] = (GPARAM.FieldValue<ulong>)newValue; break;
            case GPARAM.FloatField f: f.Values[index] = (GPARAM.FieldValue<float>)newValue; break;
            case GPARAM.DoubleField f: f.Values[index] = (GPARAM.FieldValue<double>)newValue; break;
            case GPARAM.BoolField f: f.Values[index] = (GPARAM.FieldValue<bool>)newValue; break;
            case GPARAM.Vector2Field f: f.Values[index] = (GPARAM.FieldValue<System.Numerics.Vector2>)newValue; break;
            case GPARAM.Vector3Field f: f.Values[index] = (GPARAM.FieldValue<System.Numerics.Vector3>)newValue; break;
            case GPARAM.Vector4Field f: f.Values[index] = (GPARAM.FieldValue<System.Numerics.Vector4>)newValue; break;
            case GPARAM.ColorField f: f.Values[index] = (GPARAM.FieldValue<System.Drawing.Color>)newValue; break;
            case GPARAM.StringField f: f.Values[index] = (GPARAM.FieldValue<string>)newValue; break;
            default:
                throw new NotSupportedException($"Cannot set value for unknown field type: {field.GetType().Name}");
        }
    }

    public static void AddValue(this GPARAM.IField field, int index, GPARAM.IFieldValue newValue)
    {
        switch (field)
        {
            case GPARAM.SbyteField f:
                var val_sbyte = (GPARAM.FieldValue<sbyte>)newValue;
                f.Values.Add(val_sbyte.Clone()); 
                break;
            case GPARAM.ShortField f:
                var val_short = (GPARAM.FieldValue<short>)newValue;
                f.Values.Add(val_short.Clone());
                break;
            case GPARAM.IntField f:
                var val_int = (GPARAM.FieldValue<int>)newValue;
                f.Values.Add(val_int.Clone());
                break;
            case GPARAM.LongField f:
                var val_long = (GPARAM.FieldValue<long>)newValue;
                f.Values.Add(val_long.Clone());
                break;
            case GPARAM.ByteField f:
                var val_byte = (GPARAM.FieldValue<byte>)newValue;
                f.Values.Add(val_byte.Clone());
                break;
            case GPARAM.UshortField f:
                var val_ushort = (GPARAM.FieldValue<ushort>)newValue;
                f.Values.Add(val_ushort.Clone());
                break;
            case GPARAM.UintField f:
                var val_uint = (GPARAM.FieldValue<uint>)newValue;
                f.Values.Add(val_uint.Clone());
                break;
            case GPARAM.UlongField f:
                var val_ulong = (GPARAM.FieldValue<ulong>)newValue;
                f.Values.Add(val_ulong.Clone());
                break;
            case GPARAM.FloatField f:
                var val_float = (GPARAM.FieldValue<float>)newValue;
                f.Values.Add(val_float.Clone());
                break;
            case GPARAM.DoubleField f:
                var val_double = (GPARAM.FieldValue<double>)newValue;
                f.Values.Add(val_double.Clone());
                break;
            case GPARAM.BoolField f:
                var val_bool = (GPARAM.FieldValue<bool>)newValue;
                f.Values.Add(val_bool.Clone());
                break;
            case GPARAM.Vector2Field f:
                var val_vec2 = (GPARAM.FieldValue<Vector2>)newValue;
                f.Values.Add(val_vec2.Clone());
                break;
            case GPARAM.Vector3Field f:
                var val_vec3 = (GPARAM.FieldValue<Vector3>)newValue;
                f.Values.Add(val_vec3.Clone());
                break;
            case GPARAM.Vector4Field f:
                var val_vec4 = (GPARAM.FieldValue<Vector4>)newValue;
                f.Values.Add(val_vec4.Clone());
                break;
            case GPARAM.ColorField f:
                var val_color = (GPARAM.FieldValue<Color>)newValue;
                f.Values.Add(val_color.Clone());
                break;
            case GPARAM.StringField f:
                var val_str = (GPARAM.FieldValue<string>)newValue;
                f.Values.Add(val_str.Clone());
                break;
            default:
                throw new NotSupportedException($"Cannot set value for unknown field type: {field.GetType().Name}");
        }
    }

    private static GPARAM.Field<T> CloneTypedField<T>(GPARAM.Field<T> source)
    {
        // Construct via the public default ctor that every concrete Field<T>
        // subclass exposes (e.g. new FloatField()).
        var clone = (GPARAM.Field<T>)Activator.CreateInstance(source.GetType())!;

        clone.Key = source.Key;
        clone.Name = source.Name;
        clone.Capacity = source.Capacity;
        clone.Unk = source.Unk;

        clone.Values = new System.Collections.Generic.List<GPARAM.FieldValue<T>>(source.Values.Count);
        foreach (var v in source.Values)
        {
            clone.Values.Add(v.Clone());
        }

        return clone;
    }
}
