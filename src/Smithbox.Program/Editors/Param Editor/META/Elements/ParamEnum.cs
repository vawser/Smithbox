using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.ParamEditor;

public class ParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new(); // using string as an intermediate type. first string is value, second is name.

    public ParamEnum(ParamMeta parent, XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }
        else
        {
            //TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamEnum Name property for {enumNode.Name}", LogLevel.Error);
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
            else
            {
                //TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamEnum Option Attribute Value property for {enumNode.Name}", LogLevel.Error);
            }
        }
    }
}