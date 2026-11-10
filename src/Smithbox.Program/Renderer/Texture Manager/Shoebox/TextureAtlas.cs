using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// A &lt;TextureAtlas&gt; of a shoebox layout: the sheet it describes and its cells. Cells are added and removed
/// through this class so the document and the SubTextures list stay in step (Texture Editor plan, T4).
/// </summary>
public class TextureAtlas
{
    public LayoutElement Element { get; }

    /// <summary>The layout the atlas is in (null for generated atlases).</summary>
    public ShoeboxLayout Layout { get; internal set; }

    public List<SubTexture> SubTextures = new();

    public string ImagePath { get => Element.Get("imagePath") ?? ""; set => Set("imagePath", value); }

    // Custom (Smithbox's bundled layouts): the Icon Configuration group a sheet belongs to
    public string Type { get => Element.Get("type") ?? ""; set => Set("type", string.IsNullOrEmpty(value) ? null : value); }

    /// <summary>The atlas' own width/height attributes (NR, AC6, bundled layouts), 0 when absent.</summary>
    public int WidthValue => int.TryParse(Element.Get("width"), out var w) ? w : 0;
    public int HeightValue => int.TryParse(Element.Get("height"), out var h) ? h : 0;

    public TextureAtlas(LayoutElement element)
    {
        Element = element;

        foreach (var child in element.ChildElements.Where(e => e.Name == "SubTexture"))
        {
            SubTextures.Add(new SubTexture(child) { Atlas = this });
        }
    }

    /// <summary>An atlas made in code (generated icon layouts).</summary>
    public TextureAtlas(string imagePath, string type, List<SubTexture> subTextures)
    {
        Element = LayoutElement.Create("TextureAtlas", ("imagePath", imagePath));
        Element.SelfClosing = false;
        if (!string.IsNullOrEmpty(type))
            Element.Set("type", type);

        SubTextures = subTextures;
        foreach (var sub in subTextures)
            sub.Atlas = this;
    }

    public void Set(string attribute, string value)
    {
        if (Element.Get(attribute) == value)
            return;

        Element.Set(attribute, value);
        Layout?.MarkModified();
    }

    /// <summary>
    /// A new cell formatted like an existing one (its attribute set, order and spacing), with the given values;
    /// attributes the template has beyond name/x/y/width/height keep the template's values. Not yet added.
    /// </summary>
    public SubTexture CreateLike(SubTexture template, string name, int x, int y, int width, int height)
    {
        SubTexture sub;

        if (template != null)
        {
            sub = new SubTexture(template.Element.CloneElement());
        }
        else
        {
            sub = new SubTexture(name, x, y, width, height);
        }

        sub.Element.Set("name", name);
        sub.Element.Set("x", $"{x}");
        sub.Element.Set("y", $"{y}");
        sub.Element.Set("width", $"{width}");
        sub.Element.Set("height", $"{height}");

        // Kept in step with the size, except on half cells, where the original size is twice it
        if (sub.Element.Get("originalWidth") != null && sub.Element.Get("half") != "1")
        {
            sub.Element.Set("originalWidth", $"{width}");
            sub.Element.Set("originalHeight", $"{height}");
        }

        return sub;
    }

    /// <summary>Adds a cell at an index of SubTextures (and at the matching place in the file).</summary>
    public void Insert(SubTexture sub, int index)
    {
        index = System.Math.Clamp(index, 0, SubTextures.Count);

        // A cell put back by an undo gets the whitespace it had, so the file returns to its bytes
        if (index < SubTextures.Count)
            Element.InsertChildBefore(sub.Element, SubTextures[index].Element, sub.RemovedLead);
        else
            Element.InsertChild(sub.Element, SubTextures.LastOrDefault()?.Element, sub.RemovedLead);

        sub.RemovedLead = null;
        SubTextures.Insert(index, sub);
        sub.Atlas = this;
        Layout?.MarkModified();
    }

    public void Remove(SubTexture sub)
    {
        if (!SubTextures.Remove(sub))
            return;

        sub.RemovedLead = Element.RemoveChild(sub.Element);
        Layout?.MarkModified();
    }
}
