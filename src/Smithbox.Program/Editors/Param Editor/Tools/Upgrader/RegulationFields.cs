using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>
/// The columns of a param in one version's layout, by field key. The key is the field's internal name; when a
/// paramdef has the same name more than once, later ones get "#2", "#3" and so on.
/// </summary>
public sealed class ParamLayout
{
    public readonly Param.Column[] Columns;
    public readonly string[] Keys;
    public readonly Dictionary<string, int> IndexByKey;
    public readonly int RowSize;

    public ParamLayout(Param param)
    {
        Columns = param.Columns.ToArray();
        Keys = new string[Columns.Length];
        IndexByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        RowSize = param.RowSize;

        var seen = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var i = 0; i < Columns.Length; i++)
        {
            var name = Columns[i].Def.InternalName;
            seen.TryGetValue(name, out var count);
            count++;
            seen[name] = count;

            var key = count == 1 ? name : $"{name}#{count}";
            Keys[i] = key;
            IndexByKey[key] = i;
        }
    }

    public Param.Column this[string key] => IndexByKey.TryGetValue(key, out var i) ? Columns[i] : null;

    /// <summary>Same fields, types and positions: rows can be copied as raw bytes</summary>
    public bool SameAs(ParamLayout other)
    {
        if (RowSize != other.RowSize || Columns.Length != other.Columns.Length)
            return false;

        for (var i = 0; i < Columns.Length; i++)
        {
            if (Keys[i] != other.Keys[i] || !RegulationFields.SameShape(Columns[i], other.Columns[i]) ||
                Columns[i].GetByteOffset() != other.Columns[i].GetByteOffset() ||
                Columns[i].GetBitOffset() != other.Columns[i].GetBitOffset())
                return false;
        }

        return true;
    }
}

/// <summary>
/// How fields of an older layout map to a newer layout for one step: same key and same shape, or a move.
/// </summary>
public sealed class FieldMap
{
    /// <summary>For each newer column, the older column its value comes from (-1 for a field new in this step)</summary>
    public readonly int[] OlderForNewer;

    /// <summary>For each older column, the newer column it gets its value from when rebuilding (-1 for none)</summary>
    public readonly int[] NewerForOlder;

    public readonly bool Identical;

    public FieldMap(ParamLayout older, ParamLayout newer, IEnumerable<FieldMove> moves)
    {
        Identical = older.SameAs(newer);
        OlderForNewer = Enumerable.Repeat(-1, newer.Columns.Length).ToArray();
        NewerForOlder = Enumerable.Repeat(-1, older.Columns.Length).ToArray();

        var moveList = moves?.ToList() ?? new List<FieldMove>();
        var moveTargets = moveList.Select(m => m.To).ToHashSet(StringComparer.Ordinal);

        // Same key and shape
        for (var n = 0; n < newer.Columns.Length; n++)
        {
            if (moveTargets.Contains(newer.Keys[n]))
                continue;

            if (older.IndexByKey.TryGetValue(newer.Keys[n], out var o) &&
                RegulationFields.SameShape(older.Columns[o], newer.Columns[n]))
            {
                OlderForNewer[n] = o;
                NewerForOlder[o] = n;
            }
        }

        // Moves
        foreach (var move in moveList)
        {
            if (!older.IndexByKey.TryGetValue(move.From, out var o) ||
                !newer.IndexByKey.TryGetValue(move.To, out var n))
                continue;

            if (!RegulationFields.SameShape(older.Columns[o], newer.Columns[n]))
                continue;

            OlderForNewer[n] = o;
            NewerForOlder[o] = n;
        }
    }
}

public static class RegulationFields
{
    public static bool SameShape(Param.Column a, Param.Column b)
    {
        return a.Def.DisplayType == b.Def.DisplayType &&
               a.Def.ArrayLength == b.Def.ArrayLength &&
               a.IsBitField == b.IsBitField &&
               a.Def.BitSize == b.Def.BitSize;
    }

    public static bool ValuesEqual(object a, object b)
    {
        switch (a)
        {
            case null:
                return b == null;
            case byte[] ab:
                return b is byte[] bb && ab.AsSpan().SequenceEqual(bb);
            case float af:
                return b is float bf && BitConverter.SingleToInt32Bits(af) == BitConverter.SingleToInt32Bits(bf);
            case double ad:
                return b is double bd && BitConverter.DoubleToInt64Bits(ad) == BitConverter.DoubleToInt64Bits(bd);
            default:
                return a.Equals(b);
        }
    }

    public static bool IsDefault(object value)
    {
        return value switch
        {
            null => true,
            byte[] b => b.All(x => x == 0),
            string s => s.Length == 0,
            float f => BitConverter.SingleToInt32Bits(f) == 0,
            double d => BitConverter.DoubleToInt64Bits(d) == 0,
            sbyte v => v == 0,
            byte v => v == 0,
            short v => v == 0,
            ushort v => v == 0,
            int v => v == 0,
            uint v => v == 0,
            _ => false
        };
    }

    public static string Format(object value)
    {
        return value switch
        {
            null => "",
            byte[] b => Convert.ToHexString(b),
            // NaN keeps its exact bits (some float fields hold FFFFFFFF), which "NaN" would lose
            float f when float.IsNaN(f) => $"NaN:{BitConverter.SingleToUInt32Bits(f):X8}",
            double d when double.IsNaN(d) => $"NaN:{BitConverter.DoubleToUInt64Bits(d):X16}",
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    public static object Parse(Param.Column column, string text)
    {
        var t = column.ValueType;
        var c = CultureInfo.InvariantCulture;

        if (t == typeof(byte[])) return Convert.FromHexString(text);
        if (t == typeof(string)) return text ?? "";
        if (t == typeof(float))
            return text.StartsWith("NaN:", StringComparison.Ordinal)
                ? BitConverter.UInt32BitsToSingle(uint.Parse(text.AsSpan(4), NumberStyles.HexNumber, c))
                : float.Parse(text, NumberStyles.Float, c);
        if (t == typeof(double))
            return text.StartsWith("NaN:", StringComparison.Ordinal)
                ? BitConverter.UInt64BitsToDouble(ulong.Parse(text.AsSpan(4), NumberStyles.HexNumber, c))
                : double.Parse(text, NumberStyles.Float, c);
        if (t == typeof(sbyte)) return sbyte.Parse(text, c);
        if (t == typeof(byte)) return byte.Parse(text, c);
        if (t == typeof(short)) return short.Parse(text, c);
        if (t == typeof(ushort)) return ushort.Parse(text, c);
        if (t == typeof(int)) return int.Parse(text, c);
        if (t == typeof(uint)) return uint.Parse(text, c);

        throw new NotSupportedException($"Field type {t} of {column.Def.InternalName}");
    }

    /// <summary>
    /// Copies the mapped fields of a newer row into an older-layout row (the rebuild direction).
    /// </summary>
    public static void CopyNewerToOlder(Param.Row newer, ParamLayout newerLayout, Param.Row older,
        ParamLayout olderLayout, FieldMap map)
    {
        if (map.Identical)
        {
            older.SetRawData(newer.GetRawData());
            return;
        }

        // Start from the newer bytes at the same offsets: this keeps bits no field names (spare bits of a
        // bitfield byte) wherever the layout didn't move them. The mapped fields then overwrite their own bits.
        older.SetRawData(ClippedCopy(newer.GetRawData(), olderLayout.RowSize));

        for (var o = 0; o < olderLayout.Columns.Length; o++)
        {
            var n = map.NewerForOlder[o];
            if (n < 0)
                continue;

            olderLayout.Columns[o].SetValue(older, newerLayout.Columns[n].GetValue(newer));
        }
    }

    /// <summary>The first <paramref name="size"/> bytes of the data, zero padded</summary>
    public static byte[] ClippedCopy(byte[] data, int size)
    {
        var result = new byte[size];
        Array.Copy(data, result, Math.Min(size, data.Length));
        return result;
    }

    /// <summary>The zero value of a column's type</summary>
    public static object ZeroValue(Param.Column column)
    {
        var t = column.ValueType;
        if (t == typeof(byte[])) return new byte[column.Def.ArrayLength];
        if (t == typeof(string)) return "";
        return Convert.ChangeType(0, t, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Hash of a param's rows: IDs, names, order and row bytes.
    /// </summary>
    public static string Hash(Param param)
    {
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> idBytes = stackalloc byte[4];

        foreach (var row in param.Rows)
        {
            BitConverter.TryWriteBytes(idBytes, row.ID);
            sha.AppendData(idBytes);
            sha.AppendData(Encoding.UTF8.GetBytes(row.Name ?? "\u0001null"));
            sha.AppendData(new byte[] { 0 });
            sha.AppendData(row.GetRawData());
        }

        return Convert.ToHexString(sha.GetHashAndReset())[..32];
    }

    /// <summary>
    /// The rows of a param with their keys (ID and occurrence index among rows with that ID).
    /// </summary>
    public static List<(RowKeyValue Key, Param.Row Row)> KeyedRows(Param param)
    {
        var result = new List<(RowKeyValue, Param.Row)>(param.Rows.Count);
        var counts = new Dictionary<int, int>();

        foreach (var row in param.Rows)
        {
            counts.TryGetValue(row.ID, out var index);
            counts[row.ID] = index + 1;
            result.Add((new RowKeyValue(row.ID, index), row));
        }

        return result;
    }

    /// <summary>
    /// Creates an empty param in the layout of <paramref name="def"/> at <paramref name="version"/>, with the
    /// header of <paramref name="headerSource"/>.
    /// </summary>
    public static Param CreateParam(PARAMDEF def, ulong version, Param headerSource)
    {
        var param = new Param(def, headerSource.BigEndian, version)
        {
            Format2D = headerSource.Format2D,
            Format2E = headerSource.Format2E,
            ParamdefFormatVersion = headerSource.ParamdefFormatVersion,
            Unk06 = headerSource.Unk06,
            ParamdefDataVersion = headerSource.ParamdefDataVersion,
            ParamType = def.ParamType
        };

        return param;
    }
}

public readonly record struct RowKeyValue(int ID, int Index);
