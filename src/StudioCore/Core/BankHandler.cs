using StudioCore.Banks.AliasBank;
using StudioCore.Banks.EntitySelectionGroupBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Banks.HavokAliasBank;
using StudioCore.Banks.MapTransformBank;
using StudioCore.Banks.ParamCategoryBank;
using StudioCore.Banks.ParamCommutativeBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Banks.SpawnStateBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Banks.TextureBlockBank;
using StudioCore.Banks.TextureCorrectionBank;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

/// <summary>
/// Handler class that holds all of the documentation banks used to:
/// - Create name aliases (Alias Bank)
/// - Populate property information. (Format Bank)
/// - Store help documentation (Help Bank)
/// - Store filter strings used in conditional checks (X Texture Bank)
/// </summary>
public class BankHandler
{
    public AliasBank MapAliases;
    public AliasBank GparamAliases;

    public AliasBank CharacterAliases;
    public AliasBank AssetAliases;
    public AliasBank PartAliases;
    public AliasBank MapPieceAliases;

    public AliasBank EventFlagAliases;
    public AliasBank ParticleAliases;
    public AliasBank SoundAliases;
    public AliasBank CutsceneAliases;
    public AliasBank MovieAliases;
    public AliasBank TalkAliases;

    public AliasBank TimeActAliases;
    public HavokGeneratorAliasBank HavokGeneratorAliases;

    public FormatBank MSB_Info;
    public FormatBank FLVER_Info;
    public FormatBank GPARAM_Info;

    public TextureCorrectionBank CorrectedTextureInfo;
    public TextureAdditionBank AdditionalTextureInfo;
    public TextureBlockBank BlockedTextureInfo;

    public GameOffsetBank GameOffsets;

    public ProjectEnumBank ProjectEnums;
    public ParamCategoryBank ParamCategories;
    public ParamCommutativeBank ParamCommutativeGroups;
    public SpawnStateBank SpawnStates;
    public LightmapAtlasBank LightmapAtlasBank;
    public MaterialResourceBank MaterialBank;
    public EntitySelectionGroupBank EntitySelectionGroups;

    public MapTransformBank MapTransforms;

    public bool ReloadAliasBanks;

    public BankHandler()
    {
        MapAliases = new AliasBank("Maps", "Maps", "Map Name");
        GparamAliases = new AliasBank("Gparams", "Gparams", "Graphics Param");

        CharacterAliases = new AliasBank("Character", "Characters", "Character Name");
        AssetAliases = new AliasBank("Asset", "Assets", "Asset Name");
        PartAliases = new AliasBank("Part", "Parts", "Part Name");
        MapPieceAliases = new AliasBank("MapPiece", "MapPieces", "Map Piece Name");

        EventFlagAliases = new AliasBank("EventFlag", "Flags", "Event Flag");
        ParticleAliases = new AliasBank("Fxr", "Particles", "Particle");
        SoundAliases = new AliasBank("Sound", "Sounds", "Sound");
        CutsceneAliases = new AliasBank("Cutscene", "Cutscenes", "Cutscene");
        MovieAliases = new AliasBank("Movie", "Movies", "Movie");
        TalkAliases = new AliasBank("Talk", "Talks", "Talk");

        TimeActAliases = new AliasBank("TimeActs", "TimeActs", "TimeAct");

        HavokGeneratorAliases = new HavokGeneratorAliasBank("Generators", "Generator");

        MSB_Info = new FormatBank("MSB", true);
        FLVER_Info = new FormatBank("FLVER", false);
        GPARAM_Info = new FormatBank("GPARAM", true);

        AdditionalTextureInfo = new TextureAdditionBank();
        BlockedTextureInfo = new TextureBlockBank();
        CorrectedTextureInfo = new TextureCorrectionBank();

        GameOffsets = new GameOffsetBank();

        ProjectEnums = new ProjectEnumBank("Project Enums");
        ParamCategories = new ParamCategoryBank();
        ParamCommutativeGroups = new ParamCommutativeBank();
        LightmapAtlasBank = new LightmapAtlasBank();
        MaterialBank = new MaterialResourceBank();
        EntitySelectionGroups = new EntitySelectionGroupBank();

        MapTransforms = new MapTransformBank();

        SpawnStates = new SpawnStateBank();
    }

    public void UpdateBanks()
    {
        MapAliases.LoadBank();
        GparamAliases.LoadBank();

        CharacterAliases.LoadBank();
        AssetAliases.LoadBank();
        PartAliases.LoadBank();
        MapPieceAliases.LoadBank();

        EventFlagAliases.LoadBank();
        ParticleAliases.LoadBank();
        SoundAliases.LoadBank();
        CutsceneAliases.LoadBank();
        MovieAliases.LoadBank();
        TalkAliases.LoadBank();

        TimeActAliases.LoadBank();
        HavokGeneratorAliases.LoadBank();

        MSB_Info.LoadBank();
        FLVER_Info.LoadBank();
        GPARAM_Info.LoadBank();

        AdditionalTextureInfo.LoadBank();
        BlockedTextureInfo.LoadBank();
        CorrectedTextureInfo.LoadBank();

        GameOffsets.LoadBank();

        ProjectEnums.LoadBank();
        ParamCategories.LoadBank();
        ParamCommutativeGroups.LoadBank();
        SpawnStates.LoadBank();
        LightmapAtlasBank.LoadBank();
        MaterialBank.LoadBank();
        EntitySelectionGroups.LoadBank();

        MapTransforms.LoadBank();

        ParamBank.ReloadParams();

        MsbMeta.SetupMeta();
    }

    /// <summary>
    /// Update loop, used to spin until trigger variables occur
    /// </summary>
    public void OnGui()
    {
        if(ReloadAliasBanks)
        {
            ReloadAliasBanks = false;

            MapAliases.LoadBank();
            GparamAliases.LoadBank();

            CharacterAliases.LoadBank();
            AssetAliases.LoadBank();
            PartAliases.LoadBank();
            MapPieceAliases.LoadBank();

            EventFlagAliases.LoadBank();
            ParticleAliases.LoadBank();
            SoundAliases.LoadBank();
            CutsceneAliases.LoadBank();
            MovieAliases.LoadBank();
            TalkAliases.LoadBank();

            TimeActAliases.LoadBank();
        }
    }
}
