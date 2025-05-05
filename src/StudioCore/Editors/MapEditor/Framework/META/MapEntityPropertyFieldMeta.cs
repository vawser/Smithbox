using Octokit;
using SoulsFormats;
using StudioCore.Editors.ParamEditor.META;
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
public class MapEntityPropertyFieldMeta
{
    public bool IsEmpty { get; set; } = false;

    public MapEntityPropertyMeta _parent;

    // Map-specific
    public string SpecialHandling { get; set; } = string.Empty;

    public bool ArrayProperty { get; set; } = false;
    public bool IndexProperty { get; set; } = false;
    public bool PositionProperty { get; set; } = false;
    public bool RotationProperty { get; set; } = false;
    public bool ScaleProperty { get; set; } = false;
    public bool EntityIdentifierProperty { get; set; } = false;

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

    public bool ShowTalkList { get; set; } = false;

    public bool ShowModelLinkButton { get; set; } = false;

    public bool ShowSpawnStateList { get; set; } = false;

    // Empty default
    public MapEntityPropertyFieldMeta()
    {
        IsEmpty = true;
    }

    public MapEntityPropertyFieldMeta(MapEntityPropertyMeta parent)
    {
        _parent = parent;
    }

    public MapEntityPropertyFieldMeta(MapEntityPropertyMeta parent, XmlNode entry)
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
            ParamRef = tParamRef.InnerText.Split(",").Select(x => new ParamRef(null, x)).ToList();
        }

        // FmgRef
        XmlAttribute tFmgRef = entry.Attributes["FmgRef"];
        if (tFmgRef != null)
        {
            FmgRef = tFmgRef.InnerText.Split(",").Select(x => new FMGRef(null, x)).ToList();
        }

        // Enum
        XmlAttribute tEnum = entry.Attributes["Enum"];
        if (tEnum != null)
        {
            EnumType = parent.EnumList.GetValueOrDefault(tEnum.InnerText, null);
        }

        // MapRef
        XmlAttribute tMapRef = entry.Attributes["MapRef"];
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

        // Talk List
        XmlAttribute tShowTalkList = entry.Attributes["TalkAlias"];
        if (tShowTalkList != null)
        {
            ShowTalkList = true;
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

        // Entity ID Property
        XmlAttribute EntityIdentifier = entry.Attributes["EntityIdentifier"];
        if (EntityIdentifier != null)
        {
            EntityIdentifierProperty = true;
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
