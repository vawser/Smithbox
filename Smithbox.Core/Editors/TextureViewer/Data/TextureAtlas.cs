using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;

public class TextureAtlas
{
    public string ImagePath { get; set; }

    public List<SubTexture> SubTextures = new List<SubTexture>();

    public TextureAtlas(XmlNode node)
    {
        ImagePath = node.Attributes["imagePath"].Value;

        foreach (XmlNode cNode in node.ChildNodes)
        {
            SubTextures.Add(new SubTexture(cNode));
        }
    }
}