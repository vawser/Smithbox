using SoulsFormats;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace StudioCore.Editors.ParamEditor.META;

public class ParamFieldMeta
{
    private ParamMeta MetaParent;

    public ParamFieldMeta(ParamMeta parent, PARAMDEF.Field field)
    {
        MetaParent = parent;

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

    public ParamFieldMeta(ParamMeta parent, XmlNode fieldMeta, PARAMDEF.Field field)
    {
        MetaParent = parent;

        XmlAttribute tDefaultValue = fieldMeta.Attributes["DefaultValue"];
        if (tDefaultValue != null)
        {
            DefaultValue = tDefaultValue.InnerText;
        }

        XmlAttribute Ref = fieldMeta.Attributes["Refs"];
        if (Ref != null)
        {
            RefTypes = Ref.InnerText.Split(",").Select(x => new ParamRef(MetaParent, x)).ToList();
        }

        XmlAttribute VRef = fieldMeta.Attributes["VRef"];
        if (VRef != null)
        {
            VirtualRef = VRef.InnerText;
        }

        XmlAttribute FMGRef = fieldMeta.Attributes["FmgRef"];
        if (FMGRef != null)
        {
            FmgRef = FMGRef.InnerText.Split(",").Select(x => new FMGRef(MetaParent, x)).ToList();
        }

        XmlAttribute tMapFmgRef = fieldMeta.Attributes["MapFmgRef"];
        if (tMapFmgRef != null)
        {
            MapFmgRef = new List<FMGRef>
            {
                new FMGRef(MetaParent, "m10_02_00_00"),
                new FMGRef(MetaParent, "m10_04_00_00"),
                new FMGRef(MetaParent, "m10_10_00_00"),
                new FMGRef(MetaParent, "m10_14_00_00"),
                new FMGRef(MetaParent, "m10_15_00_00"),
                new FMGRef(MetaParent, "m10_16_00_00"),
                new FMGRef(MetaParent, "m10_17_00_00"),
                new FMGRef(MetaParent, "m10_18_00_00"),
                new FMGRef(MetaParent, "m10_19_00_00"),
                new FMGRef(MetaParent, "m10_23_00_00"),
                new FMGRef(MetaParent, "m10_25_00_00"),
                new FMGRef(MetaParent, "m10_27_00_00"),
                new FMGRef(MetaParent, "m10_29_00_00"),
                new FMGRef(MetaParent, "m10_31_00_00"),
                new FMGRef(MetaParent, "m10_32_00_00"),
                new FMGRef(MetaParent, "m10_33_00_00"),
                new FMGRef(MetaParent, "m10_34_00_00"),
                new FMGRef(MetaParent, "m20_10_00_00"),
                new FMGRef(MetaParent, "m20_11_00_00"),
                new FMGRef(MetaParent, "m20_21_00_00"),
                new FMGRef(MetaParent, "m20_24_00_00"),
                new FMGRef(MetaParent, "m50_35_00_00"),
                new FMGRef(MetaParent, "m50_36_00_00"),
                new FMGRef(MetaParent, "m50_37_00_00"),
                new FMGRef(MetaParent, "m50_38_00_00")
            };
        }

        XmlAttribute TexRef = fieldMeta.Attributes["TextureRef"];
        if (TexRef != null)
        {
            TextureRef = TexRef.InnerText.Split(",").Select(x => new TexRef(parent, x)).ToList();
        }

        XmlAttribute Enum = fieldMeta.Attributes["Enum"];
        if (Enum != null)
        {
            EnumType = parent.ParamEnums.GetValueOrDefault(Enum.InnerText, null);
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
            ExtRefs = ExRef.InnerText.Split(';').Select(x => new ExtRef(MetaParent, x)).ToList();
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
                foreach (var element in DeepCopyTarget.InnerText.Split(","))
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
    /// The default value for this field.
    /// </summary>
    public string DefaultValue { get; set; }

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

}