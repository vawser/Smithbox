using System.Collections.Generic;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;

public class TextureAtlas
{
    public string ImagePath { get; set; }

    public List<SubTexture> SubTextures = new List<SubTexture>();

    // Custom
    public string Type { get; set; } = "";

    /// <summary>An atlas made in code (generated icon layouts).</summary>
    public TextureAtlas(string imagePath, string type, List<SubTexture> subTextures)
    {
        ImagePath = imagePath;
        Type = type ?? "";
        SubTextures = subTextures;
    }

    public TextureAtlas(XmlNode node)
    {
        ImagePath = node.Attributes["imagePath"].Value;

        foreach (XmlNode cNode in node.ChildNodes)
        {
            SubTextures.Add(new SubTexture(cNode));
        }

        if (node.Attributes["type"] != null)
        {
            Type = node.Attributes["type"].Value;
        }
    }
}