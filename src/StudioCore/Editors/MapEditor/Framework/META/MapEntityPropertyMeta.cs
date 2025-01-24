using Octokit;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.MapEditor.Framework.META;


public class MapEntityPropertyMeta
{
    public bool IsEmpty { get; set; } = false;

    internal XmlDocument _xml;

    private int XML_VERSION = 0;

    public Dictionary<string, MapParamEnum> EnumList { get; set; } = new Dictionary<string, MapParamEnum>();

    public Dictionary<string, MapEntityPropertyFieldMeta> Fields { get; set; } = new Dictionary<string, MapEntityPropertyFieldMeta>();

    public string Wiki { get; set; } = string.Empty;

    // Empty default
    public MapEntityPropertyMeta()
    {
        IsEmpty = true;
    }

    public MapEntityPropertyMeta(string path)
    {
        _xml = new XmlDocument();
        _xml.Load(path);

        XmlNode root = _xml.SelectSingleNode("MSBMETA");
        var xmlVersion = int.Parse(root.Attributes["XmlVersion"].InnerText);
        if (xmlVersion != XML_VERSION)
        {
            throw new InvalidDataException(
                $"Mismatched XML version; current version: {XML_VERSION}, file version: {xmlVersion}");
        }

        // Self
        XmlNode self = root.SelectSingleNode("Self");
        if (self != null)
        {
            XmlAttribute WikiEntry = self.Attributes["Wiki"];
            if (WikiEntry != null)
            {
                Wiki = WikiEntry.InnerText.Replace("\\n", "\n");
            }
        }

        // Enums
        XmlNode enums = root.SelectSingleNode("Enums");
        if (enums != null)
        {
            foreach (XmlNode entry in enums.ChildNodes)
            {
                var newEnum = new MapParamEnum(entry);
                EnumList.Add(newEnum.Name, newEnum);
            }
        }

        // Fields
        XmlNode fields = root.SelectSingleNode("Field");
        if (fields != null)
        {
            foreach (XmlNode entry in fields.ChildNodes)
            {
                Fields.Add(entry.Name, new MapEntityPropertyFieldMeta(this, entry));
            }
        }

        // Color Editors
        XmlNode colorEditors = root.SelectSingleNode("ColorEditors");
        if (colorEditors != null)
        {

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
