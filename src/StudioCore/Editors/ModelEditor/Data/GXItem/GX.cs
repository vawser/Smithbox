using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public static class GX
{
    public static GXIdType ParseGXId(this string id) => ParseGXId(id.AsSpan());
    public static GXIdType ParseGXId(this ReadOnlySpan<char> id)
    {
        if (id == "GXMD")
            return GXIdType.GXMD;
        if (id.StartsWith("GX") && id.Length == 4 && char.IsDigit(id[2]) && char.IsDigit(id[3]))
            return GXIdType.GX00;
        return GXIdType.Unknown;
    }

    public static bool IsGXMD(string id) => id.ParseGXId() == GXIdType.GXMD;
    public static bool IsGX00(string id) => id.ParseGXId() == GXIdType.GX00;

    public static GXValue[] ToGxValues(this byte[] bytes) => ToGxValues(bytes.AsSpan());
    public static GXValue[] ToGxValues(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % 4 != 0)
            throw new ArgumentException("The number of bytes has to be divisible by 4.");

        var values = new GXValue[bytes.Length / 4];
        for (int i = 0; i < values.Length; i++)
            values[i] = new GXValue(i: BitConverter.ToInt32(bytes.Slice(i * 4, 4)));
        return values;
    }
    public static byte[] ToGxDataBytes(this IEnumerable<GXValue> values)
    {
        var bytes = new List<byte>();
        foreach (var value in values)
            bytes.AddRange(value.GetBytes());
        return bytes.ToArray();
    }
}