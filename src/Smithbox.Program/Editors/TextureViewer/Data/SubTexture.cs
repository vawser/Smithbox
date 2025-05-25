using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;

public class SubTexture
{
    public string Name { get; set; }
    public string X { get; set; }
    public string Y { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public string Half { get; set; }

    public SubTexture(XmlNode node)
    {
        Name = node.Attributes["name"].Value;
        X = node.Attributes["x"].Value;
        Y = node.Attributes["y"].Value;
        Width = node.Attributes["width"].Value;
        Height = node.Attributes["height"].Value;

        if (node.Attributes["half"] != null)
        {
            Half = node.Attributes["half"].Value;
        }
        else
        {
            Half = "";
        }
    }
}
