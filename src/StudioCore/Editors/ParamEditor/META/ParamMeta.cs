using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace StudioCore.Editors.ParamEditor.META;

public class ParamMeta
{
    public ParamData DataParent;

    public const int XML_VERSION = 0;

    public string Name;

    public string _path;

    public XmlDocument _xml;

    public Dictionary<string, ParamEnum> ParamEnums = new();

    public List<ParamDisplayName> DisplayNames = new();

    public List<ParamColorEdit> ColorEditors = new();

    public Dictionary<PARAMDEF.Field, ParamFieldMeta> Fields = new();

    /// <summary>
    /// Provides a brief description of the param's usage and behaviour
    /// </summary>
    public string Wiki { get; set; }

    /// <summary>
    /// Range of grouped rows (eg weapon infusions, itemlot groups)
    /// </summary>
    public int BlockSize { get; set; }

    /// <summary>
    /// ID at which grouping begins in a param
    /// </summary>
    public int BlockStart { get; set; }

    /// <summary>
    /// Indicates the param uses consecutive IDs and thus rows with consecutive IDs should be kept together if moved
    /// </summary>
    public bool ConsecutiveIDs { get; set; }

    /// <summary>
    /// Max value of trailing digits used for offset, +1
    /// </summary>
    public int OffsetSize { get; set; }

    /// <summary>
    /// Value to offset references by
    /// </summary>
    public int FixedOffset { get; set; }

    /// <summary>
    /// Whether row 0 is a dummy to be ignored
    /// </summary>
    public bool Row0Dummy { get; set; }

    /// <summary>
    /// Provides a reordering of fields for display purposes only
    /// </summary>
    public List<string> AlternateOrder { get; set; }

    /// <summary>
    /// Provides a reordering of rows for display purposes only
    /// </summary>
    public Dictionary<string, List<string>> AlternateRowOrders { get; set; }

    /// <summary>
    /// Provides a set of fields the define a CalcCorrectGraph
    /// </summary>
    public CalcCorrectDefinition CalcCorrectDef { get; set; }

    /// <summary>
    /// Provides a set of fields the define a CalcCorrectGraph for soul cost
    /// </summary>
    public SoulCostDefinition SoulCostDef { get; set; }

    public ParamMeta(ParamData parent)
    {
        DataParent = parent;
    }

    public void XmlDeserialize(string path, PARAMDEF def)
    {
        Name = Path.GetFileName(path);

        // Blank
        if (!File.Exists(path))
        {
            foreach (PARAMDEF.Field f in def.Fields)
            {
                new ParamFieldMeta(this, f);
            }

            // Blank Metadata
            try
            {
                XmlDocument xml = new();

                XmlElement root = xml.CreateElement("PARAMMETA");
                SetStringXmlProperty("XmlVersion", "" + XML_VERSION, false, xml, "PARAMMETA");

                XmlNode self = xml.CreateElement("Self");
                root.AppendChild(self);

                XmlNode field = xml.CreateElement("Field");
                root.AppendChild(field);

                _xml = xml;
                _path = path;
            }
            catch
            {
            }
        }
        // Filled
        else
        {
            XmlDocument mxml = new();

            mxml.Load(path);

            _xml = mxml;
            _path = path;

            XmlNode root = mxml.SelectSingleNode("PARAMMETA");
            var xmlVersion = int.Parse(root.Attributes["XmlVersion"].InnerText);
            if (xmlVersion != XML_VERSION)
            {
                throw new InvalidDataException(
                    $"Mismatched XML version; current version: {XML_VERSION}, file version: {xmlVersion}");
            }

            XmlNode self = root.SelectSingleNode("Self");
            if (self != null)
            {
                XmlAttribute WikiEntry = self.Attributes["Wiki"];
                if (WikiEntry != null)
                {
                    Wiki = WikiEntry.InnerText.Replace("\\n", "\n");
                }

                XmlAttribute GroupSize = self.Attributes["BlockSize"];
                if (GroupSize != null)
                {
                    BlockSize = int.Parse(GroupSize.InnerText);
                }

                XmlAttribute GroupStart = self.Attributes["BlockStart"];
                if (GroupStart != null)
                {
                    BlockStart = int.Parse(GroupStart.InnerText);
                }

                XmlAttribute CIDs = self.Attributes["ConsecutiveIDs"];
                if (CIDs != null)
                {
                    ConsecutiveIDs = true;
                }

                XmlAttribute Off = self.Attributes["OffsetSize"];
                if (Off != null)
                {
                    OffsetSize = int.Parse(Off.InnerText);
                }

                XmlAttribute FixOff = self.Attributes["FixedOffset"];
                if (FixOff != null)
                {
                    FixedOffset = int.Parse(FixOff.InnerText);
                }

                XmlAttribute R0 = self.Attributes["Row0Dummy"];
                if (R0 != null)
                {
                    Row0Dummy = true;
                }

                XmlAttribute AltOrd = self.Attributes["AlternativeOrder"];
                if (AltOrd != null)
                {
                    AlternateOrder = new List<string>(AltOrd.InnerText.Replace("\n", "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries));

                    for (var i = 0; i < AlternateOrder.Count; i++)
                    {
                        AlternateOrder[i] = AlternateOrder[i].Trim();
                    }
                }

                XmlAttribute CCD = self.Attributes["CalcCorrectDef"];
                if (CCD != null)
                {
                    CalcCorrectDef = new CalcCorrectDefinition(this, CCD.InnerText);
                }

                XmlAttribute SCD = self.Attributes["SoulCostDef"];
                if (SCD != null)
                {
                    SoulCostDef = new SoulCostDefinition(this, SCD.InnerText);
                }
            }

            // Enums
            foreach (XmlNode node in root.SelectNodes("Enums/Enum"))
            {
                ParamEnum en = new(this, node);
                ParamEnums.Add(en.Name, en);
            }

            // Color Edits
            foreach (XmlNode node in root.SelectNodes("ColorEdit/ColorEditor"))
            {
                ParamColorEdit colorEditor = new(this, node);
                ColorEditors.Add(colorEditor);
            }

            // Display Names
            foreach (XmlNode node in root.SelectNodes("DisplayNames/NameEntry"))
            {
                ParamDisplayName displayNames = new(this, node);
                DisplayNames.Add(displayNames);
            }

            // Fields
            Dictionary<string, int> nameCount = new();
            foreach (PARAMDEF.Field f in def.Fields)
            {
                try
                {
                    var name = FixName(f.InternalName);
                    var c = nameCount.GetValueOrDefault(name, 0);
                    XmlNodeList nodes = root.SelectNodes($"Field/{name}");
                    //XmlNode pairedNode = root.SelectSingleNode($"Field/{}");
                    XmlNode pairedNode = nodes[c];
                    nameCount[name] = c + 1;

                    if (pairedNode == null)
                    {
                        Fields.Add(f, new ParamFieldMeta(this, f));
                        continue;
                    }

                    Fields.Add(f, new ParamFieldMeta(this, pairedNode, f));
                }
                catch
                {
                    Fields.Add(f, new ParamFieldMeta(this, f));
                }
            }

            //Row orders
            var orders = root.SelectSingleNode("AlternateRowOrders");
            if (orders != null)
            {
                foreach (XmlNode node in orders.ChildNodes)
                {
                    if (AlternateRowOrders == null) AlternateRowOrders = [];
                    XmlAttribute altOrd = node.Attributes["Order"];
                    if (altOrd != null)
                    {
                        List<string> ord =
                        [
                            ..altOrd.InnerText.Replace("\n", "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        ];

                        for (var i = 0; i < ord.Count; i++)
                        {
                            ord[i] = ord[i].Trim();
                        }

                        AlternateRowOrders.Add(node.Name, ord);
                    }
                }
            }
        }
    }

    public ParamFieldMeta GetField(PARAMDEF.Field def)
    {
        if (Fields.ContainsKey(def))
        {
            ParamFieldMeta fieldMeta = Fields[def];

            if (fieldMeta == null)
            {
                if (DataParent.ParamMeta.ContainsKey(def.Parent))
                {
                    var pdef = DataParent.ParamMeta[def.Parent];
                    fieldMeta = new ParamFieldMeta(pdef, def);
                }
            }

            return fieldMeta;
        }

        return null;
    }
    private void SetStringXmlProperty(string property, string value, bool sanitise, XmlDocument xml,
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
    private XmlNode GetXmlNode(XmlDocument xml, XmlNode parent, string child)
    {
        XmlNode node = parent.SelectSingleNode(child);

        if (node == null)
        {
            node = parent.AppendChild(xml.CreateElement(child));
        }

        return node;
    }

    private XmlAttribute GetXmlAttribute(XmlDocument xml, XmlNode node, string name)
    {
        XmlAttribute attribute = node.Attributes[name];

        if (attribute == null)
        {
            attribute = node.Attributes.Append(xml.CreateAttribute(name));
        }

        return attribute;
    }

    private XmlNode GetXmlNode(XmlDocument xml, params string[] path)
    {
        XmlNode currentNode = xml;

        foreach (var s in path)
        {
            currentNode = GetXmlNode(xml, currentNode, s);
        }

        return currentNode;
    }
    private string FixName(string internalName)
    {
        var name = Regex.Replace(internalName, @"[^a-zA-Z0-9_]", "");
        if (Regex.IsMatch(name, @"^\d"))
        {
            name = "_" + name;
        }

        return name;
    }
}
