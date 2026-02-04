using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.ParamEditor;

public class ParamDisplayName
{
    public string Param;
    public string Name;

    public ParamDisplayName(ParamMeta parent, XmlNode node)
    {
        Param = "";
        Name = "";

        if (node.Attributes["Param"] != null)
        {
            Param = node.Attributes["Param"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {Param}");
        }

        if (node.Attributes["Name"] != null)
        {
            Name = node.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {Name}");
        }
    }
}