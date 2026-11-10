using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// One .layout file: its atlases, and the lossless document they are read from (Texture Editor plan, T4).
/// An unedited layout writes back its original bytes; an edited one writes its document, which differs from the
/// original only where attributes or cells were changed.
/// </summary>
public class ShoeboxLayout
{
    public string FileName = "";

    public List<TextureAtlas> TextureAtlases = new List<TextureAtlas>();

    /// <summary>The parsed file; null for generated layouts and for files the lossless parser can't read.</summary>
    public LayoutDocument Document { get; private set; }

    /// <summary>The bytes the layout was read from.</summary>
    public byte[] OriginalBytes { get; private set; }

    /// <summary>The binder file it came from (sblytbnd), or null.</summary>
    public BinderFile BinderFile { get; }

    /// <summary>The file it came from on disk (Smithbox's bundled or the project's Icon Layouts), or null.</summary>
    public string SourcePath { get; }

    public bool IsModified { get; private set; }

    /// <summary>Generated in code, or not readable losslessly: shown, but can't be edited or saved.</summary>
    public bool IsReadOnly => Document == null;

    /// <summary>Why the layout is read-only, when the lossless parser failed.</summary>
    public string ParseError { get; private set; }

    /// <summary>A layout made in code (generated icon layouts).</summary>
    public ShoeboxLayout(string fileName, TextureAtlas atlas)
    {
        FileName = fileName;
        TextureAtlases.Add(atlas);
    }

    public ShoeboxLayout(BinderFile file)
    {
        FileName = file.Name;
        BinderFile = file;
        Load(file.Bytes.ToArray());
    }

    /// <summary>A new layout made from bytes, for a binder file or a project Icon Layouts path (T5 Create Layout).</summary>
    public ShoeboxLayout(string fileName, byte[] bytes, BinderFile binderFile, string sourcePath)
    {
        FileName = fileName;
        BinderFile = binderFile;
        SourcePath = sourcePath;
        Load(bytes);
    }

    public ShoeboxLayout(string filepath)
    {
        FileName = Path.GetFileNameWithoutExtension(filepath);
        SourcePath = filepath;
        Load(File.ReadAllBytes(filepath));
    }

    public void MarkModified()
    {
        if (!IsReadOnly)
            IsModified = true;
    }

    /// <summary>The bytes to save: the original ones until the layout is edited.</summary>
    public byte[] ToBytes()
    {
        return IsModified && Document != null ? Document.ToBytes() : OriginalBytes;
    }

    /// <summary>After a save: the written bytes become the original ones.</summary>
    public void MarkSaved()
    {
        OriginalBytes = ToBytes();
        IsModified = false;
    }

    private void Load(byte[] bytes)
    {
        OriginalBytes = bytes;

        try
        {
            Document = LayoutDocument.Parse(bytes);

            foreach (var element in Document.Elements.Where(e => e.Name == "TextureAtlas"))
            {
                TextureAtlases.Add(new TextureAtlas(element) { Layout = this });
            }
        }
        catch (Exception e)
        {
            // Read the cells the old way, read-only
            Document = null;
            ParseError = e.Message;
            TextureAtlases.Clear();

            var xml = new XmlDocument();
            using var stream = new MemoryStream(bytes);
            xml.Load(stream);

            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name != "TextureAtlas")
                    continue;

                var atlasElement = ToElement(node);
                atlasElement.SelfClosing = false;

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "SubTexture")
                        atlasElement.Children.Add(ToElement(child));
                }

                TextureAtlases.Add(new TextureAtlas(atlasElement));
            }
        }
    }

    private static LayoutElement ToElement(XmlNode node)
    {
        var element = new LayoutElement { Name = node.Name, SelfClosing = true };

        if (node.Attributes != null)
        {
            foreach (XmlAttribute a in node.Attributes)
                element.Set(a.Name, a.Value);
        }

        return element;
    }
}
