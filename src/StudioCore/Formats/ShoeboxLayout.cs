using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Formats;

public class ShoeboxLayoutContainer
{
    public string ContainerName = "";

    public Dictionary<string, ShoeboxLayout> Layouts = new Dictionary<string, ShoeboxLayout>();

    public Dictionary<string, List<SubTexture>> Textures = new Dictionary<string, List<SubTexture>>();

    public ShoeboxLayoutContainer(string filepath)
    {
        var binder = BND4.Read(filepath);

        ContainerName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filepath));

        foreach (var file in binder.Files)
        {
            if(file.Name.Contains(".layout"))
            {
                ShoeboxLayout newLayout = new ShoeboxLayout(file);

                if(!Layouts.ContainsKey(file.Name))
                {
                    Layouts.Add(file.Name, newLayout);
                }
            }
        }
    }

    public void BuildTextureDictionary()
    {
        foreach(var entry in Layouts)
        {
            foreach(var tex in entry.Value.TextureAtlases)
            {
                TaskLogs.AddLog($"TextureAtlas: {tex.ImagePath}");

                var path = Path.GetFileNameWithoutExtension(tex.ImagePath);
                string Name = path;

                if(!Textures.ContainsKey(Name))
                {
                    TaskLogs.AddLog($"Name: {Name}");
                    Textures.Add(Name, tex.SubTextures);
                }
            }
        }
    }
}

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
}

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
