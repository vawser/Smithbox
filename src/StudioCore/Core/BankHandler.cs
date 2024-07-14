using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Banks.SelectionGroupBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Banks.TextureBlockBank;
using StudioCore.Banks.TextureCorrectionBank;
using StudioCore.Editors.MapEditor.LightmapAtlasEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
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

    public FormatBank MSB_Info;
    public FormatBank FLVER_Info;
    public FormatBank GPARAM_Info;

    public TextureCorrectionBank CorrectedTextureInfo;
    public TextureAdditionBank AdditionalTextureInfo;
    public TextureBlockBank BlockedTextureInfo;

    public GameOffsetBank GameOffsets;
    public SelectionGroupBank SelectionGroups;

    public ProjectEnumBank ProjectEnums;

    public FMGBank FMGBank;
    public LightmapAtlasBank LightmapAtlasBank;
    public MaterialResourceBank MaterialBank;

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

        MSB_Info = new FormatBank("MSB", true);
        FLVER_Info = new FormatBank("FLVER", false);
        GPARAM_Info = new FormatBank("GPARAM", true);

        AdditionalTextureInfo = new TextureAdditionBank();
        BlockedTextureInfo = new TextureBlockBank();
        CorrectedTextureInfo = new TextureCorrectionBank();

        GameOffsets = new GameOffsetBank();
        SelectionGroups = new SelectionGroupBank();

        ProjectEnums = new ProjectEnumBank("Project Enums");

        FMGBank = new FMGBank();
        LightmapAtlasBank = new LightmapAtlasBank();
        MaterialBank = new MaterialResourceBank();
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

        MSB_Info.LoadBank();
        FLVER_Info.LoadBank();
        GPARAM_Info.LoadBank();

        AdditionalTextureInfo.LoadBank();
        BlockedTextureInfo.LoadBank();
        CorrectedTextureInfo.LoadBank();

        GameOffsets.LoadBank();
        SelectionGroups.LoadBank();

        ProjectEnums.LoadBank();

        FMGBank.SetDefaultLanguagePath();

        FMGBank.LanguageFolder = Smithbox.ProjectHandler.CurrentProject.Config.LastFmgLanguageUsed;
        FMGBank.LoadFMGs();

        LightmapAtlasBank.LoadBank();
        MaterialBank.LoadBank();

        ParamBank.ReloadParams();
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
        }
    }
}
