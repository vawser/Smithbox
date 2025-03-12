using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace StudioCore.Editors.ParamEditor;

public class ParamMetaData
{
    private const int XML_VERSION = 0;
    private static readonly Dictionary<PARAMDEF, ParamMetaData> _ParamMetas = new();
    private readonly string _path;
    internal XmlDocument _xml;

    internal Dictionary<string, ParamEnum> enums = new();

    internal List<ParamDisplayName> DisplayNames = new();

    internal List<ParamColorEdit> ColorEditors = new();

    public static string CurrentMetaFile = "";

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

        CurrentMetaFile = _path;

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

        // Enums
        foreach (XmlNode node in root.SelectNodes("Enums/Enum"))
        {
            ParamEnum en = new(node);
            enums.Add(en.Name, en);
        }

        // Color Edits
        foreach (XmlNode node in root.SelectNodes("ColorEdit/ColorEditor"))
        {
            ParamColorEdit colorEditor = new(node);
            ColorEditors.Add(colorEditor);
        }

        // Display Names
        foreach (XmlNode node in root.SelectNodes("DisplayNames/NameEntry"))
        {
            ParamDisplayName displayNames = new(node);
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
    ///     Provides a reordering of rows for display purposes only
    /// </summary>
    public Dictionary<string, List<string>> AlternateRowOrders { get; set; }

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
            GetXmlAttribute(xml, node, property).InnerText = value.Name;
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
                ? string.Join(',', value).Replace(eolPattern, eolPattern + "\n")
                : string.Join(',', value);
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
        if (AlternateRowOrders != null)
        {
            foreach (var (k, v) in AlternateRowOrders)
            {
                SetStringListXmlProperty("Order", v, x => x, "-,", _xml, "PARAMMETA", "AlternateRowOrders", k);
            }
        }
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

            using var writer = XmlWriter.Create(_path, writeSettings);
            _xml.Save(writer);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog("Unable to save editor mode changes to file",
                LogLevel.Warning, LogPriority.High, e);
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

        ShowParticleEnumList = false;
        ShowSoundEnumList = false;
        ShowFlagEnumList = false;
        ShowCutsceneEnumList = false;
        ShowMovieEnumList = false;
        ShowProjectEnumList = false;

        FlagAliasEnum_ConditionalField = "";
        FlagAliasEnum_ConditionalValue = "";
        MovieAliasEnum_ConditionalField = "";
        MovieAliasEnum_ConditionalValue = "";
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

        XmlAttribute tMapFmgRef = fieldMeta.Attributes["MapFmgRef"];
        if (tMapFmgRef != null)
        {
            MapFmgRef = new List<FMGRef>
            {
                new FMGRef("m10_02_00_00"),
                new FMGRef("m10_04_00_00"),
                new FMGRef("m10_10_00_00"),
                new FMGRef("m10_14_00_00"),
                new FMGRef("m10_15_00_00"),
                new FMGRef("m10_16_00_00"),
                new FMGRef("m10_17_00_00"),
                new FMGRef("m10_18_00_00"),
                new FMGRef("m10_19_00_00"),
                new FMGRef("m10_23_00_00"),
                new FMGRef("m10_25_00_00"),
                new FMGRef("m10_27_00_00"),
                new FMGRef("m10_29_00_00"),
                new FMGRef("m10_31_00_00"),
                new FMGRef("m10_32_00_00"),
                new FMGRef("m10_33_00_00"),
                new FMGRef("m10_34_00_00"),
                new FMGRef("m20_10_00_00"),
                new FMGRef("m20_11_00_00"),
                new FMGRef("m20_21_00_00"),
                new FMGRef("m20_24_00_00"),
                new FMGRef("m50_35_00_00"),
                new FMGRef("m50_36_00_00"),
                new FMGRef("m50_37_00_00"),
                new FMGRef("m50_38_00_00")
            };
        }

        XmlAttribute TexRef = fieldMeta.Attributes["TextureRef"];
        if (TexRef != null)
        {
            TextureRef = TexRef.InnerText.Split(",").Select(x => new TexRef(x)).ToList();
        }

        XmlAttribute Enum = fieldMeta.Attributes["Enum"];
        if (Enum != null)
        {
            EnumType = parent.enums.GetValueOrDefault(Enum.InnerText, null);
        }

        XmlAttribute ProjectEnum = fieldMeta.Attributes["ProjectEnum"];
        if (ProjectEnum != null)
        {
            ShowProjectEnumList = true;
            ProjectEnumType = ProjectEnum.InnerText;
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

        XmlAttribute IsInvertedFloat = fieldMeta.Attributes["IsInvertedPercentage"];
        if (IsInvertedFloat != null)
        {
            IsInvertedPercentage = true;
        }

        XmlAttribute IsPadding = fieldMeta.Attributes["Padding"];
        if (IsPadding != null)
        {
            IsPaddingField = true;
        }

        XmlAttribute AddSeparator = fieldMeta.Attributes["Separator"];
        if (AddSeparator != null)
        {
            AddSeparatorNextLine = true;
        }

        XmlAttribute Obsolete = fieldMeta.Attributes["Obsolete"];
        if (Obsolete != null)
        {
            IsObsoleteField = true;
        }

        XmlAttribute ParticleAlias = fieldMeta.Attributes["ParticleAlias"];
        if (ParticleAlias != null)
        {
            ShowParticleEnumList = true;
        }

        XmlAttribute SoundAlias = fieldMeta.Attributes["SoundAlias"];
        if (SoundAlias != null)
        {
            ShowSoundEnumList = true;
        }

        XmlAttribute FlagAlias = fieldMeta.Attributes["FlagAlias"];
        if (FlagAlias != null)
        {
            ShowFlagEnumList = true;
            if (FlagAlias.InnerText != "")
            {
                FlagAliasEnum_ConditionalField = FlagAlias.InnerText.Split("=")[0];
                FlagAliasEnum_ConditionalValue = FlagAlias.InnerText.Split("=")[1];
            }
        }

        XmlAttribute CutsceneAlias = fieldMeta.Attributes["CutsceneAlias"];
        if (CutsceneAlias != null)
        {
            ShowCutsceneEnumList = true;
        }

        XmlAttribute MovieAlias = fieldMeta.Attributes["MovieAlias"];
        if (MovieAlias != null)
        {
            ShowMovieEnumList = true;
            if (MovieAlias.InnerText != "")
            {
                MovieAliasEnum_ConditionalField = MovieAlias.InnerText.Split("=")[0];
                MovieAliasEnum_ConditionalValue = MovieAlias.InnerText.Split("=")[1];
            }
        }

        XmlAttribute ParamFieldOffset = fieldMeta.Attributes["ParamFieldOffset"];
        if (ParamFieldOffset != null)
        {
            ShowParamFieldOffset = true;
            ParamFieldOffsetIndex = ParamFieldOffset.InnerText;
        }

        XmlAttribute DeepCopyTarget = fieldMeta.Attributes["DeepCopyTarget"];
        if (DeepCopyTarget != null)
        {
            DeepCopyTargetType = new List<string>();

            if (DeepCopyTarget.InnerText.Contains(","))
            {
                foreach(var element in DeepCopyTarget.InnerText.Split(","))
                {
                    DeepCopyTargetType.Add(element);
                }
            }
            else
            {
                DeepCopyTargetType.Add(DeepCopyTarget.InnerText);
            }
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
    ///     DS2 Map FMG Refs
    /// </summary>
    public List<FMGRef> MapFmgRef { get; set; }

    /// <summary>
    ///     Name of an Texture Container and File that a Field may refer to.
    /// </summary>
    public List<TexRef> TextureRef { get; set; }

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
    ///     Is this field considered padding?
    /// </summary>
    public bool IsPaddingField { get; set; }

    /// <summary>
    ///     Is a ImGui seperator applied after this line?
    /// </summary>
    public bool AddSeparatorNextLine { get; set; }

    /// <summary>
    ///     Is this field considered obsolete (unused)?
    /// </summary>
    public bool IsObsoleteField { get; set; }

    /// <summary>
    ///     Is this float displayed as an inverted percentage
    /// </summary>
    public bool IsInvertedPercentage { get; set; }

    /// <summary>
    /// Boolean for display the Particle alias derived Enum list
    /// </summary>
    public bool ShowParticleEnumList { get; set; }

    /// <summary>
    /// Boolean for display the Sound alias derived Enum list
    /// </summary>
    public bool ShowSoundEnumList { get; set; }

    /// <summary>
    /// Boolean for display the Event Flag alias derived Enum list
    /// </summary>
    public bool ShowFlagEnumList { get; set; }
    public string FlagAliasEnum_ConditionalField { get; set; }
    public string FlagAliasEnum_ConditionalValue { get; set; }

    /// <summary>
    /// Boolean for display the Cutscene alias derived Enum list
    /// </summary>
    public bool ShowCutsceneEnumList { get; set; }

    /// <summary>
    /// Boolean for display the Movie alias derived Enum list
    /// </summary>
    public bool ShowMovieEnumList { get; set; }
    public string MovieAliasEnum_ConditionalField { get; set; }
    public string MovieAliasEnum_ConditionalValue { get; set; }

    public bool ShowProjectEnumList { get; set; }
    public string ProjectEnumType { get; set; }

    public bool ShowParamFieldOffset { get; set; }
    public string ParamFieldOffsetIndex { get; set; }

    public List<string> DeepCopyTargetType { get; set; }

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
            var pdef = ParamMetaData.Get(def.Parent);
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
        ParamMetaData.SetStringListXmlProperty("FmgRef", FmgRef, x => x.getStringForm(), null, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringListXmlProperty("TextureRef", TextureRef, x => x.getStringForm(), null, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetEnumXmlProperty("Enum", EnumType, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringXmlProperty("AltName", AltName, false, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringXmlProperty("Wiki", Wiki, true, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetBoolXmlProperty("IsBool", IsBool, _parent._xml, "PARAMMETA", "Field", field);
        ParamMetaData.SetStringListXmlProperty("ExtRefs", ExtRefs, x => x.getStringForm(), null, _parent._xml,
            "PARAMMETA", "Field", field);

        XmlNode thisNode = ParamMetaData.GetXmlNode(_parent._xml, "PARAMMETA", "Field", field);
        if (thisNode.Attributes.Count == 0 && thisNode.ChildNodes.Count == 0)
            ParamMetaData.GetXmlNode(_parent._xml, "PARAMMETA", "Field").RemoveChild(thisNode);
    }
}
public class ParamDisplayName
{
    public string Param;
    public string Name;

    public ParamDisplayName(XmlNode node)
    {
        Param = "";
        Name = "";

        if (node.Attributes["Param"] != null)
        {
            Param = node.Attributes["Param"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamColorEdit Name property for {Param}");
        }

        if (node.Attributes["Name"] != null)
        {
            Name = node.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamColorEdit Name property for {Name}");
        }
    }
}

public class ParamColorEdit
{
    public string Name;
    public string Fields;
    public string PlacedField;

    public ParamColorEdit(XmlNode colorEditNode)
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
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamColorEdit Name property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["Fields"] != null)
        {
            Fields = colorEditNode.Attributes["Fields"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamColorEdit Fields property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["PlacedField"] != null)
        {
            PlacedField = colorEditNode.Attributes["PlacedField"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamColorEdit PlacedField property for {colorEditNode.Name}");
        }
    }
}

public class ParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new(); // using string as an intermediate type. first string is value, second is name.

    public ParamEnum(XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }
        else
        {
            //TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamEnum Name property for {enumNode.Name}", LogLevel.Error);
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
            else
            {
                //TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - Unable to populate ParamEnum Option Attribute Value property for {enumNode.Name}", LogLevel.Error);
            }
        }
    }
}

public class ParamRef
{
    public string ConditionField;
    public uint ConditionValue;
    public int Offset;
    public string ParamName;

    internal ParamRef(string refString)
    {
        if(refString == "")
        {
            TaskLogs.AddLog($"PARAM META: {ParamMetaData.CurrentMetaFile} - ParamRef string is empty.");
            return;
        }

        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        ParamName = offsetSplit[0];
        if (offsetSplit.Length > 1)
        {
            Offset = int.Parse(offsetSplit[1]);
        }

        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            ConditionField = condition[0];
            ConditionValue = uint.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return ConditionField != null ? ParamName + '(' + ConditionField + '=' + ConditionValue + ')' : ParamName;
    }
}

public class FMGRef
{
    public string conditionField;
    public int conditionValue;
    public int offset;
    public string fmg;

    internal FMGRef(string refString)
    {
        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        fmg = offsetSplit[0];
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
        return conditionField != null ? fmg + '(' + conditionField + '=' + conditionValue + ')' : fmg;
    }
}

public class TexRef
{
    /// <summary>
    /// The lookup process to use.
    /// </summary>
    public string LookupType = "";

    /// <summary>
    /// The name of the texture container.
    /// </summary>
    public string TextureContainer = "";

    /// <summary>
    /// The name of the texture file within the texture container.
    /// </summary>
    public string TextureFile = "";

    /// <summary>
    /// The param row field that the image index is taken from.
    /// </summary>
    public string TargetField = "";

    /// <summary>
    /// The initial part of the subtexture filename to match with.
    /// </summary>
    public string SubTexturePrefix = "";

    internal TexRef(string refString)
    {
        var refSplit = refString.Split('/');

        LookupType = refSplit[0];

        if (refSplit.Length > 1)
        {
            TextureContainer = refSplit[1];
        }
        if (refSplit.Length > 2)
        {
            TextureFile = refSplit[2];
        }
        if (refSplit.Length > 3)
        {
            TargetField = refSplit[3];
        }

        if (LookupType == "Direct")
        {
            if (refSplit.Length > 4)
            {
                SubTexturePrefix = refSplit[4];
            }
        }
    }

    internal string getStringForm()
    {
        return TextureFile;
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
