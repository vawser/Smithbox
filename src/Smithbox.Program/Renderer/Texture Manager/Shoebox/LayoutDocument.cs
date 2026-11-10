using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// A shoebox .layout file as text that keeps every byte (Texture Editor plan, T4). The games' layouts are written by
/// a tool whose formatting XmlWriter can't reproduce (TextureSurvey §7.3): "/>" with no space before it, two spaces
/// between some attributes, LF lines with a single CRLF at the end, no declaration. So the file is kept as a tree of
/// raw text and elements, where an element keeps each attribute's leading whitespace, quote and raw value, and the
/// text before its "&gt;". Writing an unedited document gives back its exact bytes; an edit changes only the
/// attribute values it touches, and a new element copies the formatting of a sibling.
/// The supported XML is what the layouts use: elements, attributes, text, comments and processing instructions
/// (kept as raw text). Anything else fails to parse, and the layout is then read-only.
/// </summary>
public class LayoutDocument
{
    /// <summary>The top-level nodes: text, and the root element(s).</summary>
    public List<LayoutNode> Nodes = new();

    public bool HasUtf8Bom;

    public IEnumerable<LayoutElement> Elements => Nodes.OfType<LayoutElement>();

    public static LayoutDocument Parse(byte[] bytes)
    {
        var doc = new LayoutDocument();
        int start = 0;

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            doc.HasUtf8Bom = true;
            start = 3;
        }

        var text = Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
        var parser = new Parser(text);
        parser.ParseContent(doc.Nodes, null);

        if (!parser.AtEnd)
            throw new FormatException($"Unexpected text at {parser.Position}");

        return doc;
    }

    public byte[] ToBytes()
    {
        var sb = new StringBuilder();
        foreach (var node in Nodes)
            node.Write(sb);

        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return HasUtf8Bom ? [0xEF, 0xBB, 0xBF, .. body] : body;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var node in Nodes)
            node.Write(sb);
        return sb.ToString();
    }

    private sealed class Parser
    {
        private readonly string Text;
        public int Position;

        public Parser(string text) { Text = text; }

        public bool AtEnd => Position >= Text.Length;

        /// <summary>Reads nodes until the end tag of 'parent' (consumed and stored on it) or the end of the text.</summary>
        public void ParseContent(List<LayoutNode> into, LayoutElement parent)
        {
            while (!AtEnd)
            {
                int lt = Text.IndexOf('<', Position);

                if (lt < 0)
                {
                    into.Add(new LayoutText(Text[Position..]));
                    Position = Text.Length;
                    return;
                }

                if (lt > Position)
                {
                    into.Add(new LayoutText(Text[Position..lt]));
                    Position = lt;
                }

                if (Starts("</"))
                {
                    if (parent == null)
                        throw new FormatException($"Unexpected end tag at {Position}");

                    int gt = Text.IndexOf('>', Position);
                    if (gt < 0)
                        throw new FormatException("Unclosed end tag");

                    parent.EndTag = Text[Position..(gt + 1)];
                    Position = gt + 1;
                    return;
                }

                if (Starts("<!--"))
                {
                    int end = Text.IndexOf("-->", Position, StringComparison.Ordinal);
                    if (end < 0)
                        throw new FormatException("Unclosed comment");
                    into.Add(new LayoutText(Text[Position..(end + 3)]));
                    Position = end + 3;
                    continue;
                }

                if (Starts("<?"))
                {
                    int end = Text.IndexOf("?>", Position, StringComparison.Ordinal);
                    if (end < 0)
                        throw new FormatException("Unclosed processing instruction");
                    into.Add(new LayoutText(Text[Position..(end + 2)]));
                    Position = end + 2;
                    continue;
                }

                if (Starts("<!"))
                    throw new FormatException($"Unsupported markup at {Position}");

                into.Add(ParseElement());
            }

            if (parent != null)
                throw new FormatException($"Missing end tag of {parent.Name}");
        }

        private LayoutElement ParseElement()
        {
            Position++; // <
            var element = new LayoutElement { Name = ReadName() };

            while (true)
            {
                int wsStart = Position;
                while (!AtEnd && char.IsWhiteSpace(Text[Position]))
                    Position++;
                var ws = Text[wsStart..Position];

                if (AtEnd)
                    throw new FormatException($"Unclosed tag {element.Name}");

                if (Starts("/>"))
                {
                    element.TagTail = ws;
                    element.SelfClosing = true;
                    Position += 2;
                    return element;
                }

                if (Text[Position] == '>')
                {
                    element.TagTail = ws;
                    Position++;
                    ParseContent(element.Children, element);
                    return element;
                }

                if (ws.Length == 0)
                    throw new FormatException($"Expected whitespace before an attribute at {Position}");

                var attr = new LayoutAttribute { LeadingWhitespace = ws, Name = ReadName() };

                int eqStart = Position;
                while (!AtEnd && char.IsWhiteSpace(Text[Position])) Position++;
                if (AtEnd || Text[Position] != '=')
                    throw new FormatException($"Expected = after {attr.Name}");
                Position++;
                while (!AtEnd && char.IsWhiteSpace(Text[Position])) Position++;
                attr.EqualsText = Text[eqStart..Position];

                if (AtEnd || (Text[Position] != '"' && Text[Position] != '\''))
                    throw new FormatException($"Expected a quote for {attr.Name}");

                attr.Quote = Text[Position];
                int close = Text.IndexOf(attr.Quote, Position + 1);
                if (close < 0)
                    throw new FormatException($"Unclosed value of {attr.Name}");

                attr.RawValue = Text[(Position + 1)..close];
                Position = close + 1;
                element.Attributes.Add(attr);
            }
        }

        private string ReadName()
        {
            int start = Position;
            while (!AtEnd && (char.IsLetterOrDigit(Text[Position]) || Text[Position] is '_' or ':' or '-' or '.'))
                Position++;

            if (start == Position)
                throw new FormatException($"Expected a name at {Position}");

            return Text[start..Position];
        }

        private bool Starts(string s) => string.CompareOrdinal(Text, Position, s, 0, s.Length) == 0;
    }
}

public abstract class LayoutNode
{
    public abstract void Write(StringBuilder sb);
    public abstract LayoutNode Clone();
}

/// <summary>Text between elements (whitespace, comments, declarations) as it is in the file.</summary>
public sealed class LayoutText : LayoutNode
{
    public string Raw;

    public LayoutText(string raw) { Raw = raw; }

    public bool IsWhitespace => Raw.All(char.IsWhiteSpace);

    public override void Write(StringBuilder sb) => sb.Append(Raw);

    public override LayoutNode Clone() => new LayoutText(Raw);
}

public sealed class LayoutAttribute
{
    public string LeadingWhitespace = " ";
    public string Name;
    public string EqualsText = "=";
    public char Quote = '"';
    public string RawValue = "";

    public string Value
    {
        get => Unescape(RawValue);
        set => RawValue = Escape(value ?? "", Quote);
    }

    public LayoutAttribute Clone() => (LayoutAttribute)MemberwiseClone();

    private static string Unescape(string raw)
    {
        if (raw.IndexOf('&') < 0)
            return raw;

        return raw.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
    }

    private static string Escape(string value, char quote)
    {
        var s = value.Replace("&", "&amp;").Replace("<", "&lt;");
        return quote == '"' ? s.Replace("\"", "&quot;") : s.Replace("'", "&apos;");
    }
}

public sealed class LayoutElement : LayoutNode
{
    public string Name;
    public List<LayoutAttribute> Attributes = new();

    /// <summary>The whitespace between the last attribute and "&gt;" or "/&gt;".</summary>
    public string TagTail = "";

    public bool SelfClosing;
    public List<LayoutNode> Children = new();

    /// <summary>The end tag as written ("&lt;/TextureAtlas&gt;"), when not self-closing.</summary>
    public string EndTag;

    public IEnumerable<LayoutElement> ChildElements => Children.OfType<LayoutElement>();

    public string Get(string name) => Attributes.FirstOrDefault(a => a.Name == name)?.Value;

    /// <summary>
    /// Sets an attribute's value. A missing attribute is added after the others, spaced like the last one;
    /// null removes it.
    /// </summary>
    public void Set(string name, string value)
    {
        var attr = Attributes.FirstOrDefault(a => a.Name == name);

        if (value == null)
        {
            if (attr != null)
                Attributes.Remove(attr);
            return;
        }

        if (attr != null)
        {
            attr.Value = value;
            return;
        }

        var like = Attributes.LastOrDefault();
        attr = new LayoutAttribute
        {
            LeadingWhitespace = like?.LeadingWhitespace is { Length: > 0 } ws && !ws.Contains('\n') ? ws : " ",
            Name = name,
            EqualsText = like?.EqualsText ?? "=",
            Quote = like?.Quote ?? '"',
        };
        attr.Value = value;
        Attributes.Add(attr);
    }

    /// <summary>
    /// Adds a child after 'after' (or at the end), with the whitespace that precedes 'after' (or the last child
    /// element) in front of it, so it sits on its own line with the same indent.
    /// </summary>
    /// <param name="lead">The whitespace to put in front of it (e.g. what it had before it was removed); null copies it.</param>
    public void InsertChild(LayoutElement child, LayoutElement after = null, string lead = null)
    {
        var reference = after ?? ChildElements.LastOrDefault();

        if (reference == null)
        {
            // First child: before the text that precedes the end tag
            int at = Children.Count > 0 && Children[^1] is LayoutText t && t.IsWhitespace ? Children.Count - 1 : Children.Count;
            Children.Insert(at, child);
            Children.Insert(at, new LayoutText(lead ?? "\n\t"));
            return;
        }

        int index = Children.IndexOf(reference);
        lead ??= LeadOf(index) ?? "\n\t";

        Children.Insert(index + 1, child);
        Children.Insert(index + 1, new LayoutText(lead));
    }

    /// <summary>Adds a child in front of 'before', on its own line: [lead][child][before's lead][before].</summary>
    public void InsertChildBefore(LayoutElement child, LayoutElement before, string lead = null)
    {
        int index = Children.IndexOf(before);
        if (index < 0)
        {
            InsertChild(child, null, lead);
            return;
        }

        var beforeLead = LeadOf(index);
        lead ??= beforeLead ?? "\n\t";
        int at = beforeLead != null ? index - 1 : index;

        Children.Insert(at, child);
        Children.Insert(at, new LayoutText(lead));

        // 'before' had no whitespace of its own: give it the same, so it stays on its own line
        if (beforeLead == null)
            Children.Insert(at + 2, new LayoutText(lead));
    }

    /// <summary>Removes a child and the whitespace in front of it, and returns that whitespace (for undo).</summary>
    public string RemoveChild(LayoutElement child)
    {
        int index = Children.IndexOf(child);
        if (index < 0)
            return null;

        Children.RemoveAt(index);

        if (index > 0 && Children[index - 1] is LayoutText text && text.IsWhitespace)
        {
            Children.RemoveAt(index - 1);
            return text.Raw;
        }

        return null;
    }

    /// <summary>The whitespace text right before the child at index, or null.</summary>
    private string LeadOf(int index)
    {
        return index > 0 && Children[index - 1] is LayoutText text && text.IsWhitespace ? text.Raw : null;
    }

    public override void Write(StringBuilder sb)
    {
        sb.Append('<').Append(Name);

        foreach (var a in Attributes)
            sb.Append(a.LeadingWhitespace).Append(a.Name).Append(a.EqualsText).Append(a.Quote).Append(a.RawValue).Append(a.Quote);

        sb.Append(TagTail);

        if (SelfClosing)
        {
            sb.Append("/>");
            return;
        }

        sb.Append('>');

        foreach (var c in Children)
            c.Write(sb);

        sb.Append(EndTag ?? $"</{Name}>");
    }

    public override LayoutNode Clone() => CloneElement();

    public LayoutElement CloneElement()
    {
        return new LayoutElement
        {
            Name = Name,
            Attributes = Attributes.Select(a => a.Clone()).ToList(),
            TagTail = TagTail,
            SelfClosing = SelfClosing,
            Children = Children.Select(c => c.Clone()).ToList(),
            EndTag = EndTag,
        };
    }

    /// <summary>A detached element, e.g. for generated layouts: &lt;Name a="1" b="2"/&gt;.</summary>
    public static LayoutElement Create(string name, params (string Name, string Value)[] attributes)
    {
        var element = new LayoutElement { Name = name, SelfClosing = true };
        foreach (var (n, v) in attributes)
            element.Set(n, v);
        return element;
    }
}
