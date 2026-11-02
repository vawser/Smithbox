using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A LOADLIST file (worldloadlistlist, ingamestay.loadlist): Shift-JIS text listing one file per line as
/// device:/path, where everything from a '#' is a comment. Entries are disabled by commenting them out.
/// Lines are kept exactly as read, so an unchanged file is written back byte for byte.
/// </summary>
public partial class LoadList
{
    public List<LoadListLine> Lines = new();

    public string NewLine = "\r\n";

    // Whether the last line ends with a newline
    public bool TrailingNewLine = true;

    private static readonly Encoding ShiftJIS = GetShiftJIS();

    private static Encoding GetShiftJIS()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding("shift-jis");
    }

    // Some files (SDT, AC6) end in null bytes that pad them to a multiple of 16 bytes.
    // They are kept out of the text, as nothing after a null can be edited in a text box.
    private const int PaddingAlignment = 16;
    private bool Padded = false;
    private int TextLength = 0;
    private int PaddingLength = 0;

    public static LoadList Read(byte[] data)
    {
        var length = data.Length;

        while (length > 0 && data[length - 1] == 0)
            length--;

        var list = Parse(ShiftJIS.GetString(data, 0, length));

        list.Padded = length < data.Length;
        list.TextLength = length;
        list.PaddingLength = data.Length - length;

        return list;
    }

    public byte[] Write()
    {
        var text = ShiftJIS.GetBytes(ToText());

        if (!Padded)
            return text;

        // Text of the same length is padded as it was read, so an unchanged file is written as it was
        var padding = text.Length == TextLength
            ? PaddingLength
            : (PaddingAlignment - text.Length % PaddingAlignment) % PaddingAlignment;

        var data = new byte[text.Length + padding];
        text.CopyTo(data, 0);

        return data;
    }

    /// <summary>
    /// Takes the line endings and padding of the file this text replaces, so it is written the same way.
    /// </summary>
    public void CopyFormat(LoadList source)
    {
        Padded = source.Padded;
        TextLength = source.TextLength;
        PaddingLength = source.PaddingLength;
        NewLine = source.NewLine;
    }

    public static LoadList Parse(string text)
    {
        var list = new LoadList();

        if (!text.Contains("\r\n") && text.Contains('\n'))
            list.NewLine = "\n";

        var lines = text.Split('\n');

        // A file ending with a newline splits into a final empty string, which isn't a line
        list.TrailingNewLine = lines.Length > 1 && lines[^1] == "";
        var count = list.TrailingNewLine ? lines.Length - 1 : lines.Length;

        for (int i = 0; i < count; i++)
        {
            var line = lines[i];

            if (list.NewLine == "\r\n" && line.EndsWith('\r'))
                line = line[..^1];

            list.Lines.Add(LoadListLine.Parse(line));
        }

        return list;
    }

    public string ToText()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < Lines.Count; i++)
        {
            sb.Append(Lines[i].ToText());

            if (i < Lines.Count - 1 || TrailingNewLine)
                sb.Append(NewLine);
        }

        return sb.ToString();
    }
}

public partial class LoadListLine
{
    /// <summary>
    /// The file this line loads, or empty for a blank or comment-only line.
    /// </summary>
    public string Path = "";

    /// <summary>
    /// The whitespace between the path and the comment.
    /// </summary>
    public string Separator = "";

    /// <summary>
    /// The comment, from its '#' to the end of the line, or empty.
    /// </summary>
    public string Comment = "";

    public LoadListLine() { }

    public LoadListLine(string path, string comment = "")
    {
        Path = path;
        Comment = comment;
        Separator = comment != "" ? " " : "";
    }

    public static LoadListLine Parse(string line)
    {
        var commentStart = line.IndexOf('#');
        var before = commentStart < 0 ? line : line[..commentStart];
        var path = before.TrimEnd();

        return new LoadListLine
        {
            Path = path,
            Separator = before[path.Length..],
            Comment = commentStart < 0 ? "" : line[commentStart..]
        };
    }

    public string ToText() => Path + Separator + Comment;

    public bool IsEnabledEntry => Path != "";

    /// <summary>
    /// An entry that has been commented out, like #other:/envLight.tpf.
    /// </summary>
    public bool IsDisabledEntry => Path == "" && DisabledEntryRegex().IsMatch(Comment);

    public bool IsEntry => IsEnabledEntry || IsDisabledEntry;

    public bool IsBlank => Path == "" && Comment == "" && Separator.Trim() == "";

    /// <summary>
    /// Comments the entry out, or uncomments it. Disabling then enabling an entry gives back the same line.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        if (enabled && IsDisabledEntry)
        {
            var match = DisabledEntryRegex().Match(Comment);
            var rest = Comment[match.Length..];
            var comment = rest.TrimStart();

            Path = match.Groups[1].Value;
            Separator = rest[..(rest.Length - comment.Length)];
            Comment = comment;
        }
        else if (!enabled && IsEnabledEntry)
        {
            Comment = "#" + ToText();
            Path = "";
            Separator = "";
        }
    }

    /// <summary>
    /// Sets the comment, adding the '#' it needs and a space before it if the line has none.
    /// </summary>
    public void SetComment(string text)
    {
        if (text != "" && !text.StartsWith('#'))
            text = "#" + text;

        Comment = text;

        if (Comment != "" && Path != "" && Separator == "")
            Separator = " ";
        else if (Comment == "")
            Separator = "";
    }

    // '#' followed by a device:/path, with no space in it
    [GeneratedRegex(@"^#([A-Za-z0-9_]+:/[^\s#]+)")]
    private static partial Regex DisabledEntryRegex();
}
