using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace StudioCore.Developer;

public static class XmlPrettyPrinter
{
    /// <summary>
    /// Pretty prints XML with each attribute on its own line.
    /// </summary>
    /// <param name="xmlString">The XML string to format</param>
    /// <param name="indentString">The string to use for indentation (default: two spaces)</param>
    /// <returns>Formatted XML string</returns>
    public static string PrettyPrint(string xmlString, string indentString = "  ")
    {
        var doc = XDocument.Parse(xmlString);
        var sb = new StringBuilder();

        using (var writer = new CustomXmlWriter(sb, indentString))
        {
            doc.Save(writer);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Pretty prints an XML file and saves it to the specified output path.
    /// </summary>
    /// <param name="inputPath">Path to the input XML file</param>
    /// <param name="outputPath">Path to save the formatted XML file</param>
    /// <param name="indentString">The string to use for indentation (default: two spaces)</param>
    public static void PrettyPrintFile(string inputPath, string outputPath, string indentString = "  ")
    {
        var xmlContent = File.ReadAllText(inputPath);
        var formatted = PrettyPrint(xmlContent, indentString);
        File.WriteAllText(outputPath, formatted);
    }
}

internal class CustomXmlWriter : XmlTextWriter
{
    private readonly string _indentString;
    private bool _hasAttributes;
    private int _currentIndentLevel;

    public CustomXmlWriter(StringBuilder sb, string indentString)
        : base(new StringWriter(sb))
    {
        _indentString = indentString;
        Formatting = Formatting.Indented;
        IndentChar = ' ';
        Indentation = indentString.Length;
        _currentIndentLevel = 0;
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
        base.WriteStartElement(prefix, localName, ns);
        _hasAttributes = false;
        _currentIndentLevel++;
    }

    public override void WriteEndElement()
    {
        _currentIndentLevel--;
        base.WriteEndElement();
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
        if (!_hasAttributes)
        {
            // First attribute - add newline and indent
            _hasAttributes = true;
            WriteRaw("\r\n" + GetIndent());
        }
        else
        {
            // Subsequent attributes - add newline and indent
            WriteRaw(" \r\n" + GetIndent());
        }

        base.WriteStartAttribute(prefix, localName, ns);
    }

    private string GetIndent()
    {
        return new string(' ', _currentIndentLevel * Indentation);
    }
}
