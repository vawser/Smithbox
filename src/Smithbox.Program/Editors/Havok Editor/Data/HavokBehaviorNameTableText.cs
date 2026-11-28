using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The text side of a game name table (action/*nameid.txt, DLProperties): a '#' comment line, "Num  = N", then
/// 'id = "name"' for 1..N. No editor dependencies, so offline tools can run it on the game's files.
/// </summary>
public static class HavokBehaviorNameTableText
{
    private static readonly Regex EntryLine = new(@"^\s*(-?\d+)\s*=\s*""(.*)""\s*$", RegexOptions.Compiled);
    private static readonly Regex NumLine = new(@"^(\s*Num\s*=\s*)(\d+)", RegexOptions.Compiled);

    // Latin-1 maps each byte to one char and back, so the Shift-JIS comment survives the round trip
    private static readonly Encoding Bytes = Encoding.Latin1;

    /// <summary>
    /// The table's names (UTF-8), or null if it isn't a name table
    /// </summary>
    public static HashSet<string> ReadNames(ReadOnlySpan<byte> raw)
    {
        var parsed = Parse(Bytes.GetString(raw));
        return parsed.Num < 0 ? null : parsed.Known;
    }

    /// <summary>
    /// The table with the names it lacks appended (next IDs, Num raised to the last one); every existing line is kept
    /// byte for byte. Null when nothing is missing or the file isn't a name table. Names with a quote are skipped.
    /// </summary>
    public static byte[] Append(ReadOnlySpan<byte> raw, IEnumerable<string> wanted, out List<string> missing)
    {
        missing = new();

        var text = Bytes.GetString(raw);
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        var (known, maxId, num, insertAt, numDigits, numLength) = Parse(text);

        // Not a name table this understands: leave it alone
        if (num < 0)
            return null;

        missing = wanted.Distinct(StringComparer.Ordinal).Where(n => !string.IsNullOrEmpty(n) && !known.Contains(n) && !n.Contains('"')).ToList();

        if (missing.Count == 0)
            return null;

        var nextId = Math.Max(num, maxId) + 1;
        var added = new StringBuilder();

        // The last entry may lack a line break
        if (insertAt < 0)
            insertAt = text.Length;

        if (insertAt == text.Length && text.Length > 0 && !text.EndsWith('\n'))
            added.Append(newline);

        foreach (var name in missing)
        {
            added.Append(nextId.ToString().PadRight(4)).Append(" = \"").Append(Bytes.GetString(Encoding.UTF8.GetBytes(name))).Append('"').Append(newline);
            nextId++;
        }

        // Num: the highest ID, as the game reads keys 1..Num. The later edit goes first, so the earlier offset holds.
        var numText = (nextId - 1).ToString();
        string result;

        if (numDigits < insertAt)
        {
            result = text.Insert(insertAt, added.ToString());
            result = result[..numDigits] + numText + result[(numDigits + numLength)..];
        }
        else
        {
            result = text[..numDigits] + numText + text[(numDigits + numLength)..];
            result = result.Insert(insertAt, added.ToString());
        }

        return Bytes.GetBytes(result);
    }

    private static (HashSet<string> Known, int MaxId, int Num, int InsertAt, int NumDigits, int NumLength) Parse(string text)
    {
        var known = new HashSet<string>(StringComparer.Ordinal);
        var maxId = 0;
        var num = -1;
        var insertAt = -1;   // char offset just past the last entry line
        var numDigits = -1;  // char offset and length of Num's value
        var numLength = 0;

        for (var pos = 0; pos < text.Length;)
        {
            var end = text.IndexOf('\n', pos);
            var next = end < 0 ? text.Length : end + 1;
            var line = text[pos..(end < 0 ? text.Length : end)].TrimEnd('\r');

            Match m;
            if ((m = EntryLine.Match(line)).Success)
            {
                known.Add(Encoding.UTF8.GetString(Bytes.GetBytes(m.Groups[2].Value)));
                maxId = Math.Max(maxId, int.Parse(m.Groups[1].Value));
                insertAt = next;
            }
            else if (num < 0 && (m = NumLine.Match(line)).Success)
            {
                num = int.Parse(m.Groups[2].Value);
                numDigits = pos + m.Groups[2].Index;
                numLength = m.Groups[2].Length;
            }

            pos = next;
        }

        return (known, maxId, num, insertAt, numDigits, numLength);
    }
}
