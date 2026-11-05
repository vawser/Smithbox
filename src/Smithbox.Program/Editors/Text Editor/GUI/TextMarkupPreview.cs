using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Read-only, word-wrapped rendering of FMG text with its markup made visible.
/// ImGui's multiline input can't wrap or colour text, so long lines scroll sideways
/// and tags read as plain text. This draws:
/// - tags (anything in &lt;...&gt;, e.g. &lt;font ...&gt;, &lt;?keyicon@31?&gt;) in the disabled-text colour,
/// - text inside &lt;font color="#RRGGBB"&gt;...&lt;/font&gt; in that colour (nesting supported),
/// - everything else in the normal text colour, wrapped to the available width.
/// </summary>
public static class TextMarkupPreview
{
    private static readonly Regex TagPattern = new(@"<[^<>]*>", RegexOptions.Compiled);

    private static readonly Regex FontColorPattern = new(
        @"^<\s*font\b[^>]*\bcolou?r\s*=\s*[""']?#?(?<hex>[0-9A-Fa-f]{6})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FontClosePattern = new(@"^<\s*/\s*font\s*>$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly record struct Segment(string Text, Vector4 Color);

    // The text rarely changes between frames, so keep the parsed segments of the last few texts
    private static readonly Dictionary<string, List<Segment>> Cache = new();
    private const int MaxCacheEntries = 32;

    public static void Display(string id, string text)
    {
        text ??= "";

        ImGui.BeginChild($"##textPreview_{id}", new Vector2(-1, 0),
            ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        if (text.Length == 0)
        {
            ImGui.TextDisabled(" ");
        }
        else
        {
            DrawWrapped(GetSegments(text));
        }

        ImGui.EndChild();
    }

    private static List<Segment> GetSegments(string text)
    {
        // Colours are part of the cached result, so a theme change would need a clear;
        // keying on the current text colour handles that.
        var textColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
        var key = $"{textColor}|{text}";

        if (Cache.TryGetValue(key, out var cached))
            return cached;

        if (Cache.Count >= MaxCacheEntries)
            Cache.Clear();

        var segments = Parse(text, textColor, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        Cache[key] = segments;

        return segments;
    }

    private static List<Segment> Parse(string text, Vector4 textColor, Vector4 tagColor)
    {
        var segments = new List<Segment>();
        var colorStack = new Stack<Vector4>();
        var position = 0;

        Vector4 Current() => colorStack.Count > 0 ? colorStack.Peek() : textColor;

        foreach (Match tag in TagPattern.Matches(text))
        {
            if (tag.Index > position)
            {
                segments.Add(new Segment(text.Substring(position, tag.Index - position), Current()));
            }

            // The tag itself, dimmed, so the user can still see and edit around it
            segments.Add(new Segment(tag.Value, tagColor));

            var colorMatch = FontColorPattern.Match(tag.Value);

            if (colorMatch.Success)
            {
                colorStack.Push(ParseHex(colorMatch.Groups["hex"].Value, textColor.W));
            }
            else if (FontClosePattern.IsMatch(tag.Value) && colorStack.Count > 0)
            {
                colorStack.Pop();
            }

            position = tag.Index + tag.Length;
        }

        if (position < text.Length)
        {
            segments.Add(new Segment(text.Substring(position), Current()));
        }

        return segments;
    }

    private static Vector4 ParseHex(string hex, float alpha)
    {
        var value = int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return new Vector4(
            ((value >> 16) & 0xFF) / 255f,
            ((value >> 8) & 0xFF) / 255f,
            (value & 0xFF) / 255f,
            alpha);
    }

    /// <summary>
    /// Lay the segments out word by word, starting a new line when the next word doesn't fit.
    /// Each piece is drawn with its own colour and joined with SameLine(0, 0).
    /// </summary>
    private static void DrawWrapped(List<Segment> segments)
    {
        var maxWidth = Math.Max(1f, ImGui.GetContentRegionAvail().X);
        var lineWidth = 0f;
        var lineHasContent = false;

        void NewLine()
        {
            if (!lineHasContent)
            {
                // An empty line still needs its height
                ImGui.TextUnformatted(" ");
            }

            lineWidth = 0f;
            lineHasContent = false;
        }

        void DrawPiece(string piece, Vector4 color)
        {
            if (piece.Length == 0)
                return;

            var width = ImGui.CalcTextSize(piece).X;

            // Wrap before a piece that doesn't fit (unless it starts the line)
            if (lineHasContent && lineWidth + width > maxWidth)
            {
                lineWidth = 0f;
                lineHasContent = false;

                // A space that caused the wrap isn't drawn at the start of the next line
                if (string.IsNullOrWhiteSpace(piece))
                    return;
            }

            if (lineHasContent)
                ImGui.SameLine(0f, 0f);

            // TextUnformatted, not TextColored: the latter treats the text as a printf
            // format, so a '%' in game text would be misread
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(piece);
            ImGui.PopStyleColor();

            lineWidth += width;
            lineHasContent = true;
        }

        foreach (var segment in segments)
        {
            var lines = segment.Text.Replace("\r\n", "\n").Split('\n');

            for (int l = 0; l < lines.Length; l++)
            {
                if (l > 0)
                    NewLine();

                // Split into words and the spaces between them, keeping both
                foreach (var piece in Regex.Split(lines[l], @"(\s+)"))
                {
                    DrawPiece(piece, segment.Color);
                }
            }
        }
    }
}
