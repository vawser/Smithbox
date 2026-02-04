using System.Collections.Generic;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;

public class TextureAtlas
{
    public string ImagePath { get; set; }

    public List<SubTexture> SubTextures = new List<SubTexture>();

    // Custom
    public string Type { get; set; } = "";

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