using System.Collections.Generic;
using System.Xml;

namespace StudioCore.Editors.MapEditor.Framework.META;

public class MapParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new();

    public MapParamEnum(XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
        }
    }
}