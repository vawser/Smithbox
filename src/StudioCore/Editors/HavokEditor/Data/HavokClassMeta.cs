using StudioCore.Editors.MapEditor.Framework.META;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.HavokEditor.Data;

public class HavokClassMeta
{
    internal XmlDocument _xml;

    private int XML_VERSION = 0;
    public Dictionary<string, HavokFieldMeta> Fields { get; set; } = new();
    public Dictionary<string, HavokMetaEnum> Enums { get; set; } = new();

    // Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HavokClassMeta(string path)
    {
        _xml = new XmlDocument();
        _xml.Load(path);

        XmlNode root = _xml.SelectSingleNode("HAVOKMETA");
        var xmlVersion = int.Parse(root.Attributes["XmlVersion"].InnerText);
        if (xmlVersion != XML_VERSION)
        {
            throw new InvalidDataException(
                $"Mismatched XML version; current version: {XML_VERSION}, file version: {xmlVersion}");
        }

        // Class
        XmlNode havokClass = root.SelectSingleNode("Class");
        if (havokClass != null)
        {
            XmlAttribute tName = havokClass.Attributes["Name"];
            if (tName != null)
            {
                Name = tName.InnerText;
            }

            XmlAttribute tDescription = havokClass.Attributes["Description"];
            if (tDescription != null)
            {
                Description = tDescription.InnerText.Replace("\\n", "\n");
            }
        }

        // Fields
        XmlNode fields = root.SelectSingleNode("Field");
        if (fields != null)
        {
            foreach (XmlNode entry in fields.ChildNodes)
            {
                Fields.Add(entry.Name, new HavokFieldMeta(this, entry));
            }
        }

        // Enums
        XmlNode enums = root.SelectSingleNode("Enums");
        if (enums != null)
        {
            foreach (XmlNode entry in enums.ChildNodes)
            {
                var newEnum = new HavokMetaEnum(entry);
                Enums.Add(newEnum.Name, newEnum);
            }
        }
    }

    internal static XmlNode GetXmlNode(XmlDocument xml, XmlNode parent, string child)
    {
        XmlNode node = parent.SelectSingleNode(child);

        if (node == null)
        {
            node = parent.AppendChild(xml.CreateElement(child));
        }

        return node;
    }

    internal static XmlAttribute GetXmlAttribute(XmlDocument xml, XmlNode node, string name)
    {
        XmlAttribute attribute = node.Attributes[name];

        if (attribute == null)
        {
            attribute = node.Attributes.Append(xml.CreateAttribute(name));
        }

        return attribute;
    }

    internal static XmlNode GetXmlNode(XmlDocument xml, params string[] path)
    {
        XmlNode currentNode = xml;

        foreach (var s in path)
        {
            currentNode = GetXmlNode(xml, currentNode, s);
        }

        return currentNode;
    }

    internal static void SetStringXmlProperty(string property, string value, bool sanitise, XmlDocument xml,
        params string[] path)
    {
        XmlNode node = GetXmlNode(xml, path);
        if (value != null)
        {
            GetXmlAttribute(xml, node, property).InnerText = sanitise ? value.Replace("\n", "\\n") : value;
        }
        else
        {
            node.Attributes.RemoveNamedItem(property);
        }
    }
}
