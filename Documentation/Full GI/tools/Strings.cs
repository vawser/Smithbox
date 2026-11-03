using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
// Prints ASCII and UTF-16 strings matching a regex, with their file offsets
public static class Strings
{
    public static void Run(string path, string pattern)
    {
        var b = File.ReadAllBytes(path);
        var re = new Regex(pattern, RegexOptions.IgnoreCase);
        var sb = new StringBuilder(); int start = 0;
        for (int i = 0; i <= b.Length; i++)
        {
            byte c = i < b.Length ? b[i] : (byte)0;
            if (c >= 0x20 && c < 0x7F) { if (sb.Length == 0) start = i; sb.Append((char)c); }
            else { if (sb.Length >= 4 && re.IsMatch(sb.ToString())) Console.WriteLine($"A 0x{start:X}: {sb}"); sb.Clear(); }
        }
        for (int align = 0; align < 2; align++)
        {
            sb.Clear();
            for (int i = align; i + 1 <= b.Length; i += 2)
            {
                char c = i + 1 < b.Length ? (char)(b[i] | (b[i + 1] << 8)) : '\0';
                if (c >= 0x20 && c < 0x7F) { if (sb.Length == 0) start = i; sb.Append(c); }
                else { if (sb.Length >= 4 && re.IsMatch(sb.ToString())) Console.WriteLine($"W 0x{start:X}: {sb}"); sb.Clear(); }
            }
        }
    }
}
