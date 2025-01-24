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

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class MsbMeta
{
    private static Dictionary<string, MsbParamMeta> _MsbMetas = new();

    public static void SetupMeta()
    {
        _MsbMetas = new();

        var metaPath = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\Meta";

        //TaskLogs.AddLog($"metaPath: {metaPath}");

        if (Path.Exists(metaPath))
        {
            foreach (var folder in Directory.EnumerateDirectories(metaPath))
            {
                //TaskLogs.AddLog($"folder: {folder}");

                var rootType = new DirectoryInfo(folder).Name;

                //TaskLogs.AddLog($"rootType: {rootType}");

                var typeMetaPath = $"{metaPath}\\{rootType}";
                //TaskLogs.AddLog($"typeMetaPath: {typeMetaPath}");

                if (Path.Exists(typeMetaPath))
                {
                    foreach (var file in Directory.EnumerateFiles(typeMetaPath))
                    {
                        var currentPath = file;
                        var specificType = Path.GetFileNameWithoutExtension(file);

                        //TaskLogs.AddLog($"currentPath: {currentPath}");

                        var newMeta = new MsbParamMeta(currentPath);
                        _MsbMetas.Add($"{rootType}_{specificType}", newMeta);
                    }
                }
            }
        }
    }

    public static MsbParamMeta GetMeta(Type type, bool sharedMeta)
    {
        // Get the strings from the passed type
        var typeString = $"{type}";

        var typeParts = typeString.Split(".");

        if (typeParts.Length > 1)
        {
            var typeSegments = typeParts[1].Split("+");

            if (typeSegments.Length > 2)
            {
                var rootType = typeSegments[1];
                var specificType = typeSegments[2];

                if (sharedMeta)
                    specificType = rootType;

                var key = $"{rootType}_{specificType}";

                if (_MsbMetas.ContainsKey(key))
                    return _MsbMetas[key];
            }
            else if (typeSegments.Length > 1)
            {
                var rootType = typeSegments[1];

                var key = $"{rootType}_{rootType}";

                if (_MsbMetas.ContainsKey(key))
                    return _MsbMetas[key];
            }
        }

        return new MsbParamMeta();
    }

    /// <summary>
    /// For DS2 MSB params
    /// </summary>
    public static MsbParamMeta GetParamMeta(string paramName)
    {
        if (_MsbMetas.ContainsKey(paramName))
            return _MsbMetas[paramName];

        return new MsbParamMeta();
    }

    public static MsbFieldMetaData GetFieldMeta(string field, Type type)
    {
        var rootMeta = GetMeta(type, true);
        var specificMeta = GetMeta(type, false);

        if (specificMeta != null)
        {
            if (specificMeta.Fields.ContainsKey(field))
            {
                return specificMeta.Fields[field];
            }
        }

        if (rootMeta != null)
        {
            if(rootMeta.Fields.ContainsKey(field))
            {
                return rootMeta.Fields[field];
            }
        }

        return new MsbFieldMetaData();
    }

    /// <summary>
    /// For DS2 MSB params
    /// </summary>
    public static MsbFieldMetaData GetParamFieldMeta(string field, string paramName)
    {
        var rootMeta = GetParamMeta(paramName);

        if (rootMeta != null)
        {
            if (rootMeta.Fields.ContainsKey(field))
            {
                return rootMeta.Fields[field];
            }
        }

        return new MsbFieldMetaData();
    }
}

public class MsbParamMeta
{
    public bool IsEmpty { get; set; } = false;

    internal XmlDocument _xml;

    private int XML_VERSION = 0;

    public Dictionary<string, MapParamEnum> EnumList { get; set; } = new Dictionary<string, MapParamEnum>();

    public Dictionary<string, MsbFieldMetaData> Fields { get; set; } = new Dictionary<string, MsbFieldMetaData>();

    public string Wiki { get; set; } = string.Empty;

    // Empty default
    public MsbParamMeta()
    {
        IsEmpty = true;
    }

    public MsbParamMeta(string path)
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
                Fields.Add(entry.Name, new MsbFieldMetaData(this, entry));
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

public class MsbFieldMetaData
{
    public bool IsEmpty { get; set; } = false;

    public MsbParamMeta _parent;

    // Map-specific
    public string SpecialHandling { get; set; } = string.Empty;

    public bool ArrayProperty { get; set; } = false;
    public bool IndexProperty { get; set; } = false;
    public bool PositionProperty { get; set; } = false;
    public bool RotationProperty { get; set; } = false;
    public bool ScaleProperty { get; set; } = false;

    // Meta
    public List<ParamRef> ParamRef { get; set; } = new List<ParamRef>();

    public List<FMGRef> FmgRef { get; set; } = new List<FMGRef>();

    public List<string> MapRef { get; set; } = new List<string>();

    public MapParamEnum EnumType { get; set; }

    public string AltName { get; set; } = string.Empty;

    public string Wiki { get; set; } = string.Empty;

    public bool IsBool { get; set; } = false;

    public bool IsPadding { get; set; } = false;

    public bool IsObsolete { get; set; } = false;

    public bool ShowParticleList { get; set; } = false;

    public bool ShowSoundList { get; set; } = false;

    public bool ShowEventFlagList { get; set; } = false;

    public bool ShowModelLinkButton { get; set; } = false;

    public bool ShowSpawnStateList { get; set; } = false;

    // Empty default
    public MsbFieldMetaData()
    {
        IsEmpty = true;
    }

    public MsbFieldMetaData(MsbParamMeta parent)
    {
        _parent = parent;
    }

    public MsbFieldMetaData(MsbParamMeta parent, XmlNode entry)
    {
        _parent = parent;

        // AltName
        XmlAttribute tAltName = entry.Attributes["AltName"];
        if (tAltName != null)
        {
            AltName = tAltName.InnerText;
        }

        // Wiki
        XmlAttribute tWiki = entry.Attributes["Wiki"];
        if (tWiki != null)
        {
            Wiki = tWiki.InnerText.Replace("\\n", "\n");
        }

        // ParamRef
        XmlAttribute tParamRef = entry.Attributes["ParamRef"];
        if (tParamRef != null)
        {
            ParamRef = tParamRef.InnerText.Split(",").Select(x => new ParamRef(x)).ToList();
        }

        // FmgRef
        XmlAttribute tFmgRef = entry.Attributes["FmgRef"];
        if (tFmgRef != null)
        {
            FmgRef = tFmgRef.InnerText.Split(",").Select(x => new FMGRef(x)).ToList();
        }

        // Enum
        XmlAttribute tEnum = entry.Attributes["Enum"];
        if (tEnum != null)
        {
            EnumType = parent.EnumList.GetValueOrDefault(tEnum.InnerText, null);
        }

        // MapRef
        XmlAttribute tMapRef = entry.Attributes["FmgRef"];
        if (tMapRef != null)
        {
            MapRef = tMapRef.InnerText.Split("-").ToList();
        }

        // IsBool
        XmlAttribute tIsBool = entry.Attributes["IsBool"];
        if (tIsBool != null)
        {
            IsBool = true;
        }

        // Padding
        XmlAttribute tIsPadding = entry.Attributes["Padding"];
        if (tIsPadding != null)
        {
            IsPadding = true;
        }

        // Obsolete
        XmlAttribute tIsObsolete = entry.Attributes["Obsolete"];
        if (tIsObsolete != null)
        {
            IsObsolete = true;
        }

        // Particle List
        XmlAttribute tShowParticleList = entry.Attributes["ParticleAlias"];
        if (tShowParticleList != null)
        {
            ShowParticleList = true;
        }

        // Sound List
        XmlAttribute tShowSoundList = entry.Attributes["SoundAlias"];
        if (tShowSoundList != null)
        {
            ShowSoundList = true;
        }

        // Event Flag List
        XmlAttribute tEventFlagList = entry.Attributes["FlagAlias"];
        if (tEventFlagList != null)
        {
            ShowEventFlagList = true;
        }

        // Spawn State List
        XmlAttribute tSpawnStateList = entry.Attributes["SpawnStates"];
        if (tSpawnStateList != null)
        {
            ShowSpawnStateList = true;
        }

        // Model Link Button
        XmlAttribute tModelLinkButton = entry.Attributes["ModelNameLink"];
        if (tModelLinkButton != null)
        {
            ShowModelLinkButton = true;
        }

        // Array Property
        XmlAttribute IsArray = entry.Attributes["ArrayProperty"];
        if (IsArray != null)
        {
            ArrayProperty = true;
        }

        // Index Property
        XmlAttribute IsIndex = entry.Attributes["IndexProperty"];
        if (IsIndex != null)
        {
            IndexProperty = true;
        }

        // Position Property
        XmlAttribute IsPosition = entry.Attributes["PositionProperty"];
        if (IsPosition != null)
        {
            PositionProperty = true;
        }

        // Rotation Property
        XmlAttribute IsRotation = entry.Attributes["RotationProperty"];
        if (IsRotation != null)
        {
            RotationProperty = true;
        }

        // Scale Property
        XmlAttribute IsScale = entry.Attributes["ScaleProperty"];
        if (IsScale != null)
        {
            ScaleProperty = true;
        }

        // SpecialHandling
        XmlAttribute tSpecialHandling = entry.Attributes["SpecialHandling"];
        if (tSpecialHandling != null)
        {
            SpecialHandling = tSpecialHandling.InnerText;
        }
    }
}

public class MapParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new();

    public MapParamEnum(XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
        }

        if (enumNode.Attributes["Name"] != null)
        {
            TaskLogs.AddLog("");
        }
    }
}