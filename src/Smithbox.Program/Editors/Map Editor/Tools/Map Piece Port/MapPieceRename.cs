using SoulsFormats;
using System;
using System.Text;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The byte level part of a map piece port. Only SoulsFormats is used, so Claude/Tools/MapPieceSurvey (port mode)
/// compiles this file to check it against the game files.
/// </summary>
public static class MapPieceRename
{
    /// <summary>
    /// A map piece file renamed: a binder's file names take the new stem and map, and each file (or a loose FLVER)
    /// has the old stem replaced where it appears whole, which is the FLVER's root node name. The stems are the same
    /// length, so everything else stays byte for byte. Compression is kept.
    /// </summary>
    public static byte[] Rename(Memory<byte> data, string oldStem, string newStem, string oldMap, string newMap)
    {
        var type = DCX.Type.None;
        var raw = data;

        if (DCX.Is(raw))
            raw = DCX.Decompress(raw, out type);

        var magic = raw.Length >= 4 ? Encoding.ASCII.GetString(raw.Span[..4]) : "";

        if (magic is "BND3" or "BND4")
        {
            // Read from the original bytes, so the binder writes back with the same DCX
            IBinder binder = magic == "BND3" ? BND3.Read(data) : BND4.Read(data);

            foreach (var file in binder.Files)
            {
                file.Name = ReplaceIgnoreCase(file.Name, oldStem, newStem);
                file.Name = ReplaceIgnoreCase(file.Name, oldMap, newMap);
                file.Bytes = ReplaceStem(file.Bytes, oldStem, newStem);
            }

            return binder is BND3 b3 ? b3.Write() : ((BND4)binder).Write();
        }

        var patched = ReplaceStem(raw, oldStem, newStem);
        return type == DCX.Type.None ? patched.ToArray() : DCX.Compress(patched.Span, type);
    }

    private static string ReplaceIgnoreCase(string text, string oldValue, string newValue)
    {
        return string.IsNullOrEmpty(text) ? text : text.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The stem replaced where it appears whole in a file (compressed files are left alone), as UTF-16 or single
    /// bytes, keeping the letter case of each match (DES names its root node m0000B0 for m0000b0)
    /// </summary>
    public static Memory<byte> ReplaceStem(Memory<byte> data, string oldStem, string newStem)
    {
        if (oldStem.Length != newStem.Length || data.Length == 0 || DCX.Is(data))
            return data;

        byte[] copy = null;

        foreach (var wide in new[] { true, false })
        {
            var width = wide ? 2 : 1;
            var span = copy != null ? copy.AsSpan() : data.Span;
            var length = oldStem.Length * width;

            for (var at = 0; at + length <= span.Length; at++)
            {
                if (!Matches(span, at, oldStem, wide))
                    continue;

                // A whole name: not part of a longer one (m30_00_00_00_000500 inside ..._000500_S is still the stem)
                if (!IsBoundary(span, at - width, wide) || !IsBoundary(span, at + length, wide))
                    continue;

                copy ??= data.ToArray();

                for (var c = 0; c < newStem.Length; c++)
                {
                    var old = (char)(wide ? copy[at + c * 2] | (copy[at + c * 2 + 1] << 8) : copy[at + c]);
                    var ch = char.IsUpper(old) ? char.ToUpperInvariant(newStem[c]) : char.ToLowerInvariant(newStem[c]);

                    if (wide)
                    {
                        copy[at + c * 2] = (byte)ch;
                        copy[at + c * 2 + 1] = 0;
                    }
                    else
                    {
                        copy[at + c] = (byte)ch;
                    }
                }

                span = copy.AsSpan();
                at += length - 1;
            }
        }

        return copy ?? data;
    }

    private static bool Matches(Span<byte> span, int at, string text, bool wide)
    {
        for (var c = 0; c < text.Length; c++)
        {
            int value = wide ? span[at + c * 2] | (span[at + c * 2 + 1] << 8) : span[at + c];

            if (value > 0x7F || char.ToLowerInvariant((char)value) != char.ToLowerInvariant(text[c]))
                return false;
        }

        return true;
    }

    /// <summary>Whether the character at a position (or outside the data) can't continue an identifier</summary>
    private static bool IsBoundary(Span<byte> span, int at, bool wide)
    {
        if (at < 0 || at + (wide ? 2 : 1) > span.Length)
            return true;

        int value = wide ? span[at] | (span[at + 1] << 8) : span[at];
        return !(value < 0x80 && char.IsAsciiLetterOrDigit((char)value));
    }
}
