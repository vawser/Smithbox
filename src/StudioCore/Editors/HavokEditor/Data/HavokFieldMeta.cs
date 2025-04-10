using System.Xml;

namespace StudioCore.Editors.HavokEditor.Data;

public class HavokFieldMeta
{
    public HavokClassMeta ParentClass;

    // Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HavokFieldMeta(HavokClassMeta parent)
    {
        ParentClass = parent;
    }

    public HavokFieldMeta(HavokClassMeta parent, XmlNode entry)
    {
        ParentClass = parent;

        // Name
        XmlAttribute tName = entry.Attributes["Name"];
        if (tName != null)
        {
            Name = tName.InnerText;
        }

        // Description
        XmlAttribute tDescription = entry.Attributes["Description"];
        if (tDescription != null)
        {
            Description = tDescription.InnerText.Replace("\\n", "\n");
        }
    }
}
