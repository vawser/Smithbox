using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.ParamEditor;

public class ParamColorEdit
{
    public string Name;
    public string Fields;
    public string PlacedField;

    public ParamColorEdit(ParamMeta parent, XmlNode colorEditNode)
    {
        Name = "";
        Fields = "";
        PlacedField = "";

        if (colorEditNode.Attributes["Name"] != null)
        {
            Name = colorEditNode.Attributes["Name"].InnerText;
        }
        else
        {
            Smithbox.Log(this, $"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["Fields"] != null)
        {
            Fields = colorEditNode.Attributes["Fields"].InnerText;
        }
        else
        {
            Smithbox.Log(this, $"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Fields property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["PlacedField"] != null)
        {
            PlacedField = colorEditNode.Attributes["PlacedField"].InnerText;
        }
        else
        {
            Smithbox.Log(this, $"PARAM META: {parent.Name} - Unable to populate ParamColorEdit PlacedField property for {colorEditNode.Name}");
        }
    }
}
