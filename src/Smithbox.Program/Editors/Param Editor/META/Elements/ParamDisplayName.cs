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
            Smithbox.Log(this,
                LOC.Get("PARAM_Meta_ParamDisplayName_Missing_Param_Property", parent.Name, Param));
        }

        if (node.Attributes["Name"] != null)
        {
            Name = node.Attributes["Name"].InnerText;
        }
        else
        {
            Smithbox.Log(this,
                LOC.Get("PARAM_Meta_ParamDisplayName_Missing_Name_Property", parent.Name, Name));
        }
    }
}