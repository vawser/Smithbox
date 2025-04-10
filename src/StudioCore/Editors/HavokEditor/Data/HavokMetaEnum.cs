using System.Collections.Generic;
using System.Xml;

namespace StudioCore.Editors.HavokEditor.Data;

public class HavokMetaEnum
{
    public string Name;

    public Dictionary<string, string> Values = new();

    public HavokMetaEnum(XmlNode enumNode)
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