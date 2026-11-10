namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// One cell of a shoebox layout: a named rectangle of a sheet. Backed by its &lt;SubTexture&gt; element, so setting a
/// value changes only that attribute's text in the file (Texture Editor plan, T4). The string properties are the
/// attribute values as written; the *Value properties parse them.
/// </summary>
public class SubTexture
{
    /// <summary>The element in the layout document (detached for generated layouts).</summary>
    public LayoutElement Element { get; }

    /// <summary>The atlas the cell is in; null until it is added to one.</summary>
    public TextureAtlas Atlas { get; internal set; }

    /// <summary>The whitespace that was in front of the cell when it was removed, put back if it is inserted again.</summary>
    internal string RemovedLead;

    public string Name { get => Get("name"); set => Set("name", value); }
    public string X { get => Get("x"); set => Set("x", value); }
    public string Y { get => Get("y"); set => Set("y", value); }
    public string Width { get => Get("width"); set => Set("width", value); }
    public string Height { get => Get("height"); set => Set("height", value); }

    /// <summary>"1": the cell is stored at half resolution (NR: originalWidth/Height are twice width/height).</summary>
    public string Half { get => Get("half"); set => Set("half", value); }

    public string OriginalWidth { get => Get("originalWidth"); set => Set("originalWidth", value); }
    public string OriginalHeight { get => Get("originalHeight"); set => Set("originalHeight", value); }

    public int XValue => Int("x");
    public int YValue => Int("y");
    public int WidthValue => Int("width");
    public int HeightValue => Int("height");
    public bool IsHalf => Half == "1";

    public SubTexture(LayoutElement element)
    {
        Element = element;
    }

    /// <summary>A sub-texture made in code (generated icon layouts).</summary>
    public SubTexture(string name, int x, int y, int width, int height)
    {
        Element = LayoutElement.Create("SubTexture",
            ("name", name), ("x", $"{x}"), ("y", $"{y}"), ("width", $"{width}"), ("height", $"{height}"), ("half", "0"));
    }

    /// <summary>An attribute's value, "" when it is absent (as the old reader gave).</summary>
    public string Get(string attribute) => Element.Get(attribute) ?? "";

    public bool Has(string attribute) => Element.Get(attribute) != null;

    /// <summary>Sets an attribute (null removes it) and marks the layout modified.</summary>
    public void Set(string attribute, string value)
    {
        if (Element.Get(attribute) == value)
            return;

        Element.Set(attribute, value);
        Atlas?.Layout?.MarkModified();
    }

    private int Int(string attribute) => int.TryParse(Element.Get(attribute), out var v) ? v : 0;
}
