using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace StudioCore.ParamEditor;

public class ParamMetaData
{
    private const int XML_VERSION = 0;
    private static readonly Dictionary<PARAMDEF, ParamMetaData> _ParamMetas = new();
    private readonly string _path;
    internal XmlDocument _xml;

    internal Dictionary<string, ParamEnum> enums = new();

    private ParamMetaData(PARAMDEF def, string path)
    {
        Add(def, this);
        foreach (PARAMDEF.Field f in def.Fields)
        {
            new FieldMetaData(this, f);
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

    private ParamMetaData(XmlDocument xml, string path, PARAMDEF def)
    {
        _xml = xml;
        _path = path;
        XmlNode root = xml.SelectSingleNode("PARAMMETA");
        var xmlVersion = int.Parse(root.Attributes["XmlVersion"].InnerText);
        if (xmlVersion != XML_VERSION)
        {
            throw new InvalidDataException(
                $"Mismatched XML version; current version: {XML_VERSION}, file version: {xmlVersion}");
        }

        Add(def, this);

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
                CalcCorrectDef = new CalcCorrectDefinition(CCD.InnerText);
            }

            XmlAttribute SCD = self.Attributes["SoulCostDef"];
            if (SCD != null)
            {
                SoulCostDef = new SoulCostDefinition(SCD.InnerText);
            }
        }

        foreach (XmlNode node in root.SelectNodes("Enums/Enum"))
        {
            ParamEnum en = new(node);
            enums.Add(en.name, en);
        }

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
                    new FieldMetaData(this, f);
                    continue;
                }

                new FieldMetaData(this, pairedNode, f);
            }
            catch
            {
                new FieldMetaData(this, f);
            }
        }
    }

    /// <summary>
    ///     Provides a brief description of the param's usage and behaviour
    /// </summary>
    public string Wiki { get; set; }

    /// <summary>
    ///     Range of grouped rows (eg weapon infusions, itemlot groups)
    /// </summary>
    public int BlockSize { get; set; }

    /// <summary>
    ///     ID at which grouping begins in a param
    /// </summary>
    public int BlockStart { get; set; }

    /// <summary>
    ///     Indicates the param uses consecutive IDs and thus rows with consecutive IDs should be kept together if moved
    /// </summary>
    public bool ConsecutiveIDs { get; set; }

    /// <summary>
    ///     Max value of trailing digits used for offset, +1
    /// </summary>
    public int OffsetSize { get; set; }

    /// <summary>
    ///     Value to offset references by
    /// </summary>
    public int FixedOffset { get; set; }

    /// <summary>
    ///     Whether row 0 is a dummy to be ignored
    /// </summary>
    public bool Row0Dummy { get; set; }

    /// <summary>
    ///     Provides a reordering of fields for display purposes only
    /// </summary>
    public List<string> AlternateOrder { get; set; }

    /// <summary>
    ///     Provides a set of fields the define a CalcCorrectGraph
    /// </summary>
    public CalcCorrectDefinition CalcCorrectDef { get; set; }

    /// <summary>
    ///     Provides a set of fields the define a CalcCorrectGraph for soul cost
    /// </summary>
    public SoulCostDefinition SoulCostDef { get; set; }

    public static ParamMetaData Get(PARAMDEF def)
    {
        if (!ParamBank.IsMetaLoaded)
        {
            return null;
        }

        return _ParamMetas[def];
    }

    private static void Add(PARAMDEF key, ParamMetaData meta)
    {
        _ParamMetas.Add(key, meta);
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

    internal static void SetBoolXmlProperty(string property, bool value, XmlDocument xml, params string[] path)
    {
        XmlNode node = GetXmlNode(xml, path);
        if (value)
        {
            GetXmlAttribute(xml, node, property).InnerText = "";
        }
        else
        {
            node.Attributes.RemoveNamedItem(property);
        }
    }

    internal static void SetIntXmlProperty(string property, int value, XmlDocument xml, params string[] path)
    {
        XmlNode node = GetXmlNode(xml, path);
        if (value != 0)
        {
            GetXmlAttribute(xml, node, property).InnerText = value.ToString();
        }
        else
        {
            node.Attributes.RemoveNamedItem(property);
        }
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

    internal static void SetEnumXmlProperty(string property, ParamEnum value, XmlDocument xml, params string[] path)
    {
        XmlNode node = GetXmlNode(xml, path);
        if (value != null)
        {
            GetXmlAttribute(xml, node, property).InnerText = value.name;
        }
        else
        {
            node.Attributes.RemoveNamedItem(property);
        }
    }

    internal static void SetStringListXmlProperty<T>(string property, IEnumerable<T> list,
        Func<T, string> stringifier, string eolPattern, XmlDocument xml, params string[] path)
    {
        XmlNode node = GetXmlNode(xml, path);
        if (list != null)
        {
            IEnumerable<string> value = list.Select(stringifier);
            GetXmlAttribute(xml, node, property).InnerText = eolPattern != null
                ? String.Join(',', value).Replace(eolPattern, eolPattern + "\n")
                : String.Join(',', value);
        }
        else
        {
            node.Attributes.RemoveNamedItem(property);
        }
    }

    public void Commit()
    {
        if (_xml == null)
        {
            return;
        }

        SetStringXmlProperty("Wiki", Wiki, true, _xml, "PARAMMETA", "Self");
        SetIntXmlProperty("OffsetSize", OffsetSize, _xml, "PARAMMETA", "Self");
        SetIntXmlProperty("FixedOffset", FixedOffset, _xml, "PARAMMETA", "Self");
        SetBoolXmlProperty("Row0Dummy", Row0Dummy, _xml, "PARAMMETA", "Self");
        SetStringListXmlProperty("AlternativeOrder", AlternateOrder, x => x, "-,", _xml, "PARAMMETA", "Self");
        SetStringXmlProperty("CalcCorrectDef", CalcCorrectDef?.getStringForm(), false, _xml, "PARAMMETA", "Self");
        SetStringXmlProperty("SoulCostDef", SoulCostDef?.getStringForm(), false, _xml, "PARAMMETA", "Self");
    }

    public void Save()
    {
        if (_xml == null)
        {
            return;
        }

        try
        {
            XmlWriterSettings writeSettings = new();
            writeSettings.Indent = true;
            writeSettings.NewLineHandling = NewLineHandling.None;
            if (!File.Exists(_path))
            {
                File.WriteAllBytes(_path, new byte[0]);
            }

            _xml.Save(XmlWriter.Create(_path, writeSettings));
        }
        catch (Exception e)
        {
            TaskLogs.AddLog("Unable to save editor mode changes to file",
                LogLevel.Warning, TaskLogs.LogPriority.High, e);
        }
    }

    public static void SaveAll()
    {
        foreach (KeyValuePair<PARAMDEF.Field, FieldMetaData> field in FieldMetaData._FieldMetas)
        {
            field.Value.Commit(FixName(field.Key.InternalName)); //does not handle shared names
        }

        foreach (ParamMetaData param in _ParamMetas.Values)
        {
            param.Commit();
            param.Save();
        }
    }

    public static ParamMetaData XmlDeserialize(string path, PARAMDEF def)
    {
        if (!File.Exists(path))
        {
            return new ParamMetaData(def, path);
        }

        XmlDocument mxml = new();
        //try
        //{
        mxml.Load(path);
        return new ParamMetaData(mxml, path, def);
        //}
        //catch
        //{
        //    return new ParamMetaData(def, path);
        //}
    }

    internal static string FixName(string internalName)
    {
        var name = Regex.Replace(internalName, @"[^a-zA-Z0-9_]", "");
        if (Regex.IsMatch(name, @"^\d"))
        {
            name = "_" + name;
        }

        return name;
    }
}

public class FieldMetaData
{
    internal static ConcurrentDictionary<PARAMDEF.Field, FieldMetaData> _FieldMetas = new();

    private readonly ParamMetaData _parent;

    public FieldMetaData(ParamMetaData parent, PARAMDEF.Field field)
    {
        _parent = parent;
        Add(field, this);
        // Blank Metadata
    }

    public FieldMetaData(ParamMetaData parent, XmlNode fieldMeta, PARAMDEF.Field field)
    {
        _parent = parent;
        Add(field, this);
        XmlAttribute Ref = fieldMeta.Attributes["Refs"];
        if (Ref != null)
        {
            RefTypes = Ref.InnerText.Split(",").Select(x => new ParamRef(x)).ToList();
        }

        XmlAttribute VRef = fieldMeta.Attributes["VRef"];
        if (VRef != null)
        {
            VirtualRef = VRef.InnerText;
        }

        XmlAttribute FMGRef = fieldMeta.Attributes["FmgRef"];
        if (FMGRef != null)
        {
            FmgRef = FMGRef.InnerText.Split(",").Select(x => new FMGRef(x)).ToList();
        }

        ;
        XmlAttribute Enum = fieldMeta.Attributes["Enum"];
        if (Enum != null)
        {
            EnumType = parent.enums.GetValueOrDefault(Enum.InnerText, null);
        }

        XmlAttribute AlternateName = fieldMeta.Attributes["AltName"];
        if (AlternateName != null)
        {
            AltName = AlternateName.InnerText;
        }

        XmlAttribute WikiText = fieldMeta.Attributes["Wiki"];
        if (WikiText != null)
        {
            Wiki = WikiText.InnerText.Replace("\\n", "\n");
        }

        XmlAttribute IsBoolean = fieldMeta.Attributes["IsBool"];
        if (IsBoolean != null)
        {
            IsBool = true;
        }

        XmlAttribute ExRef = fieldMeta.Attributes["ExtRefs"];
        if (ExRef != null)
        {
            ExtRefs = ExRef.InnerText.Split(';').Select(x => new ExtRef(x)).ToList();
        }
    }

    /// <summary>
    ///     Name of another Param that a Field may refer to.
    /// </summary>
    public List<ParamRef> RefTypes { get; set; }

    /// <summary>
    ///     Name linking fields from multiple params that may share values.
    /// </summary>
    public string VirtualRef { get; set; }

    /// <summary>
    ///     Name of an FMG that a Field may refer to.
    /// </summary>
    public List<FMGRef> FmgRef { get; set; }

    /// <summary>
    ///     Set of generally acceptable values, named
    /// </summary>
    public ParamEnum EnumType { get; set; }

    /// <summary>
    ///     Alternate name for a field not provided by source defs or paramfiles.
    /// </summary>
    public string AltName { get; set; }

    /// <summary>
    ///     A big tooltip to explain the field to the user
    /// </summary>
    public string Wiki { get; set; }

    /// <summary>
    ///     Is this u8 field actually a boolean?
    /// </summary>
    public bool IsBool { get; set; }

    /// <summary>
    ///     Path (and subpath) filters for files linked by this field.
    /// </summary>
    public List<ExtRef> ExtRefs { get; set; }

    public static FieldMetaData Get(PARAMDEF.Field def)
    {
        if (!ParamBank.IsMetaLoaded)
        {
            return null;
        }

        FieldMetaData fieldMeta = _FieldMetas[def];
        if (fieldMeta == null)
        {
            ParamMetaData pdef = ParamMetaData.Get(def.Parent);
            fieldMeta = new FieldMetaData(pdef, def);
        }

        return fieldMeta;
    }

    private static void Add(PARAMDEF.Field key, FieldMetaData meta)
    {
        _FieldMetas[key] = meta;
    }

    public void Commit(string field)
    {
        if (_parent._xml == null)
        {
            return;
        }

        ParamMetaData.SetStringListXmlProperty("Refs", RefTypes, x => x.getStringForm(), null, _parent._xml,
            "PARAMMETA", "Field", field);
        ParamMetaData.SetStringXmlProperty("VRef", VirtualRef, false, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringListXmlProperty("FmgRef", FmgRef, x => x.getStringForm(), null, _parent._xml,
            "PARAMMETA", "Field", field);
        ParamMetaData.SetEnumXmlProperty("Enum", EnumType, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringXmlProperty("AltName", AltName, false, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringXmlProperty("Wiki", Wiki, true, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetBoolXmlProperty("IsBool", IsBool, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringListXmlProperty("ExtRefs", ExtRefs, x => x.getStringForm(), null, _parent._xml,
            "PARAMMETA", "Field", field);

        XmlNode thisNode = ParamMetaData.GetXmlNode(_parent._xml, "PARAMMETA", "Field", field);
        if (thisNode.Attributes.Count == 0 && thisNode.ChildNodes.Count == 0)
        {
            ParamMetaData.GetXmlNode(_parent._xml, "PARAMMETA", "Field").RemoveChild(thisNode);
        }
    }
}

public class ParamEnum
{
    public string name;

    public Dictionary<string, string>
        values = new(); // using string as an intermediate type. first string is value, second is name.

    public ParamEnum(XmlNode enumNode)
    {
        name = enumNode.Attributes["Name"].InnerText;
        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
        }
    }
}

public class ParamRef
{
    public string conditionField;
    public int conditionValue;
    public int offset;
    public string param;

    internal ParamRef(string refString)
    {
        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        param = offsetSplit[0];
        if (offsetSplit.Length > 1)
        {
            offset = int.Parse(offsetSplit[1]);
        }

        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            conditionField = condition[0];
            conditionValue = int.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return conditionField != null ? param + '(' + conditionField + '=' + conditionValue + ')' : param;
    }
}

public class FMGRef
{
    public string conditionField;
    public int conditionValue;
    public string fmg;

    internal FMGRef(string refString)
    {
        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        fmg = conditionSplit[0];
        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            conditionField = condition[0];
            conditionValue = int.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return conditionField != null ? fmg + '(' + conditionField + '=' + conditionValue + ')' : fmg;
    }
}

public class ExtRef
{
    public string name;
    public List<string> paths;

    internal ExtRef(string refString)
    {
        var parts = refString.Split(",");
        name = parts[0];
        paths = parts.Skip(1).ToList();
    }

    internal string getStringForm()
    {
        return name + ',' + string.Join(',', paths);
    }
}

public class CalcCorrectDefinition
{
    public string[] adjPoint_maxGrowVal;
    public string fcsMaxdist;
    public string[] stageMaxGrowVal;
    public string[] stageMaxVal;

    internal CalcCorrectDefinition(string ccd)
    {
        var parts = ccd.Split(',');
        if (parts.Length == 11)
        {
            // FCS param curve
            var cclength = 5;
            stageMaxVal = new string[cclength];
            stageMaxGrowVal = new string[cclength];
            Array.Copy(parts, 0, stageMaxVal, 0, cclength);
            Array.Copy(parts, cclength, stageMaxGrowVal, 0, cclength);
            adjPoint_maxGrowVal = null;
            fcsMaxdist = parts[10];
        }
        else
        {
            var cclength = (parts.Length + 1) / 3;
            stageMaxVal = new string[cclength];
            stageMaxGrowVal = new string[cclength];
            adjPoint_maxGrowVal = new string[cclength - 1];
            Array.Copy(parts, 0, stageMaxVal, 0, cclength);
            Array.Copy(parts, cclength, stageMaxGrowVal, 0, cclength);
            Array.Copy(parts, cclength * 2, adjPoint_maxGrowVal, 0, cclength - 1);
        }
    }

    internal string getStringForm()
    {
        var str = string.Join(',', stageMaxVal) + ',' + string.Join(',', stageMaxGrowVal) + ',';
        if (adjPoint_maxGrowVal != null)
        {
            str += string.Join(',', adjPoint_maxGrowVal);
        }

        if (fcsMaxdist != null)
        {
            str += string.Join(',', fcsMaxdist);
        }

        return str;
    }
}

public class SoulCostDefinition
{
    public string adjustment_value;
    public string boundry_inclination_soul;
    public string boundry_value;
    public int cost_row;
    public string init_inclination_soul;
    public int max_level_for_game;

    internal SoulCostDefinition(string ccd)
    {
        var parts = ccd.Split(',');
        init_inclination_soul = parts[0];
        adjustment_value = parts[1];
        boundry_inclination_soul = parts[2];
        boundry_value = parts[3];
        cost_row = int.Parse(parts[4]);
        max_level_for_game = int.Parse(parts[5]);
    }

    internal string getStringForm()
    {
        return
            $@"{init_inclination_soul},{adjustment_value},{boundry_inclination_soul},{boundry_value},{cost_row},{max_level_for_game}";
    }
}
