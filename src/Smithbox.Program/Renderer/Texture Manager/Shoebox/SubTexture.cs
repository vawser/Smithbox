using System.Xml;

namespace StudioCore.Editors.TextureViewer;

public class SubTexture
{
    public string Name { get; set; } = "";
    public string X { get; set; } = "";
    public string Y { get; set; } = "";
    public string Width { get; set; } = "";
    public string Height { get; set; } = "";
    public string Half { get; set; } = "";

    public SubTexture(XmlNode node)
    {
        if (node.Attributes != null)
        {
            if (node.Attributes["name"] != null)
            {
                Name = node.Attributes["name"].Value;
            }

            if (node.Attributes["x"] != null)
            {
                X = node.Attributes["x"].Value;
            }

            if (node.Attributes["y"] != null)
            {
                Y = node.Attributes["y"].Value;
            }

            if (node.Attributes["width"] != null)
            {
                Width = node.Attributes["width"].Value;
            }

            if (node.Attributes["height"] != null)
            {
                Height = node.Attributes["height"].Value;
            }

            if (node.Attributes["half"] != null)
            {
                Half = node.Attributes["half"].Value;
            }
        }
    }
}
