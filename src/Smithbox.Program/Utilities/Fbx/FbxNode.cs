#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StudioCore.Utilities.Fbx;

/// <summary>
///     The type code of an FBX property, as stored in binary FBX (one ASCII character).
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public enum FbxPropertyType : byte
{
    /// <summary>One byte. Written as 0 / 1 by Blender and as 'T' / 'Y' / 'N' characters by the FBX SDK.</summary>
    Bool = (byte)'C',
    Int16 = (byte)'Y',
    Int32 = (byte)'I',
    Int64 = (byte)'L',
    Float = (byte)'F',
    Double = (byte)'D',
    String = (byte)'S',
    Raw = (byte)'R',
    BoolArray = (byte)'b',
    Int32Array = (byte)'i',
    Int64Array = (byte)'l',
    FloatArray = (byte)'f',
    DoubleArray = (byte)'d',
}

/// <summary>
///     One value on an FBX node. <see cref="Value" /> is a byte (Bool), short, int, long, float, double, string,
///     byte[] (Raw, BoolArray), int[], long[], float[] or double[], matching <see cref="Type" />.
/// </summary>
public sealed class FbxProperty
{
    /// <summary>Separator between an object's name and its class in binary FBX ("Hips\0\x01Model").</summary>
    public const string NameClassSeparator = "\0\u0001";

    public FbxPropertyType Type { get; }
    public object Value { get; }

    /// <summary>
    ///     Arrays only: write zlib-compressed. Kept from the file when read; null lets the writer decide (compressed
    ///     from 128 bytes up).
    /// </summary>
    public bool? Compressed;

    private FbxProperty(FbxPropertyType type, object value)
    {
        Type = type;
        Value = value;
    }

    public static FbxProperty Bool(bool v) => new(FbxPropertyType.Bool, (byte)(v ? 1 : 0));

    /// <summary>A Bool property holding a character ('Y', 'T', 'N'...), as the FBX SDK writes them.</summary>
    public static FbxProperty Char(byte c) => new(FbxPropertyType.Bool, c);

    public static FbxProperty Int16(short v) => new(FbxPropertyType.Int16, v);
    public static FbxProperty Int32(int v) => new(FbxPropertyType.Int32, v);
    public static FbxProperty Int64(long v) => new(FbxPropertyType.Int64, v);
    public static FbxProperty Float(float v) => new(FbxPropertyType.Float, v);
    public static FbxProperty Double(double v) => new(FbxPropertyType.Double, v);
    public static FbxProperty String(string v) => new(FbxPropertyType.String, v);

    /// <summary>An object name with its class, in the binary form ("Hips\0\x01Model").</summary>
    public static FbxProperty ObjectName(string name, string cls) =>
        new(FbxPropertyType.String, name + NameClassSeparator + cls);

    public static FbxProperty Raw(byte[] v) => new(FbxPropertyType.Raw, v);

    public static FbxProperty BoolArray(bool[] v) =>
        new(FbxPropertyType.BoolArray, v.Select(b => (byte)(b ? 1 : 0)).ToArray());

    /// <summary>A bool array from its stored bytes (kept as they are, so a round trip is exact).</summary>
    public static FbxProperty BoolArrayBytes(byte[] v) => new(FbxPropertyType.BoolArray, v);

    public static FbxProperty Int32Array(int[] v) => new(FbxPropertyType.Int32Array, v);
    public static FbxProperty Int64Array(long[] v) => new(FbxPropertyType.Int64Array, v);
    public static FbxProperty FloatArray(float[] v) => new(FbxPropertyType.FloatArray, v);
    public static FbxProperty DoubleArray(double[] v) => new(FbxPropertyType.DoubleArray, v);

    public bool IsArray => Type is FbxPropertyType.BoolArray or FbxPropertyType.Int32Array
        or FbxPropertyType.Int64Array or FbxPropertyType.FloatArray or FbxPropertyType.DoubleArray;

    public bool IsNumber => Type is FbxPropertyType.Bool or FbxPropertyType.Int16 or FbxPropertyType.Int32
        or FbxPropertyType.Int64 or FbxPropertyType.Float or FbxPropertyType.Double;

    public int ArrayLength => Value switch
    {
        byte[] b when Type == FbxPropertyType.BoolArray => b.Length,
        int[] a => a.Length,
        long[] a => a.Length,
        float[] a => a.Length,
        double[] a => a.Length,
        _ => 0,
    };

    /// <summary>Any scalar number as a long (floating values truncated).</summary>
    public long AsLong() => Value switch
    {
        byte b => b,
        short s => s,
        int i => i,
        long l => l,
        float f => (long)f,
        double d => (long)d,
        _ => throw new InvalidOperationException($"FBX property {Type} is not a number"),
    };

    public int AsInt() => (int)AsLong();

    /// <summary>Any scalar number as a double.</summary>
    public double AsDouble() => Value switch
    {
        byte b => b,
        short s => s,
        int i => i,
        long l => l,
        float f => f,
        double d => d,
        _ => throw new InvalidOperationException($"FBX property {Type} is not a number"),
    };

    /// <summary>A Bool as written by either Blender (0 / 1) or the FBX SDK ('T' / 'Y' vs 'F' / 'N').</summary>
    public bool AsBool() => Value switch
    {
        byte b => b != 0 && b != (byte)'F' && b != (byte)'N',
        _ => AsLong() != 0,
    };

    public string AsString() => Value switch
    {
        string s => s,
        byte[] b when Type == FbxPropertyType.Raw => Encoding.UTF8.GetString(b),
        _ => Convert.ToString(Value, CultureInfo.InvariantCulture) ?? "",
    };

    /// <summary>An object's name and class ("Hips\0\x01Model" → ("Hips", "Model")); a plain string has no class.</summary>
    public (string Name, string Class) AsObjectName()
    {
        var s = AsString();
        var i = s.IndexOf(NameClassSeparator, StringComparison.Ordinal);
        return i < 0 ? (s, "") : (s[..i], s[(i + 2)..]);
    }

    public byte[] AsRaw() => Value as byte[] ?? Encoding.UTF8.GetBytes(AsString());

    /// <summary>Any numeric array (or a scalar, as one element) as doubles.</summary>
    public double[] AsDoubleArray() => Value switch
    {
        double[] a => a,
        float[] a => Array.ConvertAll(a, v => (double)v),
        int[] a => Array.ConvertAll(a, v => (double)v),
        long[] a => Array.ConvertAll(a, v => (double)v),
        byte[] a when Type == FbxPropertyType.BoolArray => Array.ConvertAll(a, v => (double)v),
        _ => new[] { AsDouble() },
    };

    public float[] AsFloatArray() => Value switch
    {
        float[] a => a,
        _ => Array.ConvertAll(AsDoubleArray(), v => (float)v),
    };

    /// <summary>Any integer array (or a scalar, as one element) as longs.</summary>
    public long[] AsLongArray() => Value switch
    {
        long[] a => a,
        int[] a => Array.ConvertAll(a, v => (long)v),
        byte[] a when Type == FbxPropertyType.BoolArray => Array.ConvertAll(a, v => (long)v),
        float[] a => Array.ConvertAll(a, v => (long)v),
        double[] a => Array.ConvertAll(a, v => (long)v),
        _ => new[] { AsLong() },
    };

    public int[] AsIntArray() => Value switch
    {
        int[] a => a,
        _ => Array.ConvertAll(AsLongArray(), v => (int)v),
    };

    /// <summary>Same type and the same value; floating values compared bit for bit (so NaN equals NaN).</summary>
    public bool ContentEquals(FbxProperty other)
    {
        if (Type != other.Type)
            return false;

        return (Value, other.Value) switch
        {
            (byte a, byte b) => a == b,
            (short a, short b) => a == b,
            (int a, int b) => a == b,
            (long a, long b) => a == b,
            (float a, float b) => BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b),
            (double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b),
            (string a, string b) => string.Equals(a, b, StringComparison.Ordinal),
            (byte[] a, byte[] b) => a.AsSpan().SequenceEqual(b),
            (int[] a, int[] b) => a.AsSpan().SequenceEqual(b),
            (long[] a, long[] b) => a.AsSpan().SequenceEqual(b),
            (float[] a, float[] b) => a.Length == b.Length && a.Zip(b).All(p =>
                BitConverter.SingleToInt32Bits(p.First) == BitConverter.SingleToInt32Bits(p.Second)),
            (double[] a, double[] b) => a.Length == b.Length && a.Zip(b).All(p =>
                BitConverter.DoubleToInt64Bits(p.First) == BitConverter.DoubleToInt64Bits(p.Second)),
            _ => false,
        };
    }

    public override string ToString()
    {
        var t = (char)Type;
        if (IsArray)
            return $"{t}[{ArrayLength}]";
        return Value switch
        {
            string s => $"S \"{s.Replace(NameClassSeparator, "\\0\\1")}\"",
            byte[] b => $"R [{b.Length} bytes]",
            float f => $"F {f.ToString("R", CultureInfo.InvariantCulture)}",
            double d => $"D {d.ToString("R", CultureInfo.InvariantCulture)}",
            _ => $"{t} {Convert.ToString(Value, CultureInfo.InvariantCulture)}",
        };
    }
}

/// <summary>
///     One node of an FBX document: a name, its values and its child nodes. Binary and ASCII FBX share this tree
///     (<see cref="FbxDocument" />).
/// </summary>
public sealed class FbxNode
{
    public string Name;
    public List<FbxProperty> Properties = new();
    public List<FbxNode> Children = new();

    /// <summary>
    ///     Written with a body even when it has no children (an empty <c>{ }</c>; in binary, an end marker). The FBX SDK
    ///     writes <c>AnimationLayer</c> this way. Nodes with children or no values always get one.
    /// </summary>
    public bool EmptyBody;

    public FbxNode(string name, params FbxProperty[] properties)
    {
        Name = name;
        Properties.AddRange(properties);
    }

    /// <summary>The first child with this name, or null.</summary>
    public FbxNode? Child(string name) => Children.FirstOrDefault(c => c.Name == name);

    public IEnumerable<FbxNode> ChildrenNamed(string name) => Children.Where(c => c.Name == name);

    /// <summary>Adds a child and returns it.</summary>
    public FbxNode Add(FbxNode child)
    {
        Children.Add(child);
        return child;
    }

    public FbxNode Add(string name, params FbxProperty[] properties) => Add(new FbxNode(name, properties));

    public FbxProperty? Prop(int index) => index >= 0 && index < Properties.Count ? Properties[index] : null;

    /// <summary>
    ///     The node's numbers as doubles: its array value, or all its scalar values (ASCII FBX 6.x writes arrays as
    ///     plain value lists).
    /// </summary>
    public double[] Doubles() =>
        Properties.Count == 1 && Properties[0].IsArray
            ? Properties[0].AsDoubleArray()
            : Properties.Where(p => p.IsNumber).Select(p => p.AsDouble()).ToArray();

    /// <summary>The node's integers as longs (see <see cref="Doubles" />).</summary>
    public long[] Longs() =>
        Properties.Count == 1 && Properties[0].IsArray
            ? Properties[0].AsLongArray()
            : Properties.Where(p => p.IsNumber).Select(p => p.AsLong()).ToArray();

    /// <summary>This node and all its descendants.</summary>
    public int CountNodes() => 1 + Children.Sum(c => c.CountNodes());

    public override string ToString() => $"{Name}: {string.Join(", ", Properties)}" +
                                         (Children.Count > 0 ? $" {{{Children.Count}}}" : "");
}
