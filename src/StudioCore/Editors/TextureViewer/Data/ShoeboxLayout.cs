using SoulsFormats;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.TextureViewer;


public class ShoeboxLayout
{
    public string FileName = "";

    public List<TextureAtlas> TextureAtlases = new List<TextureAtlas>();

    public ShoeboxLayout(BinderFile file)
    {
        FileName = file.Name;

        XmlDocument xmlDocument = new();
        Stream stream = new MemoryStream(file.Bytes.ToArray());
        xmlDocument.Load(stream);

        // TextureAltas
        foreach (XmlNode node in xmlDocument.ChildNodes)
        {
            TextureAtlases.Add(new TextureAtlas(node));
        }
    }

    public ShoeboxLayout(string filepath)
    {
        FileName = Path.GetFileNameWithoutExtension(filepath);
        byte[] data = File.ReadAllBytes(filepath);

        XmlDocument xmlDocument = new();
        Stream stream = new MemoryStream(data);
        xmlDocument.Load(stream);

        // TextureAltas
        foreach (XmlNode node in xmlDocument.ChildNodes)
        {
            TextureAtlases.Add(new TextureAtlas(node));
        }
    }
}



