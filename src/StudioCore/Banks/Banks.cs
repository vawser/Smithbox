using StudioCore.Banks.AliasBank;
using StudioCore.Banks.ChrLinkBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Banks.CorrectedTextureBank;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.MapGroup;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.UserProject;
using System.Collections.Generic;
using StudioCore.Banks.BlockedTextureBank;
using StudioCore.Memory;

namespace StudioCore.BanksMain;

public static class BankUtils
{
    public static void SetupBanks()
    {
        // Alias
        MapAliasBank.Bank = new AliasBank(AliasBankType.Map);
        ModelAliasBank.Bank = new AliasBank(AliasBankType.Model);
        FlagAliasBank.Bank = new AliasBank(AliasBankType.EventFlag);
        ParticleAliasBank.Bank = new AliasBank(AliasBankType.Particle);
        GparamAliasBank.Bank = new AliasBank(AliasBankType.Gparam);
        SoundAliasBank.Bank = new AliasBank(AliasBankType.Sound);
        CutsceneAliasBank.Bank = new AliasBank(AliasBankType.Cutscene);
        MovieAliasBank.Bank = new AliasBank(AliasBankType.Movie);

        // Format
        MsbFormatBank.Bank = new FormatBank(FormatBankType.MSB, true);
        FlverFormatBank.Bank = new FormatBank(FormatBankType.FLVER, false);
        GparamFormatBank.Bank = new FormatBank(FormatBankType.GPARAM, true);

        // Data
        MaterialResourceBank.Setup();
        MapGroupsBank.Bank = new MapGroupBank();
        GameOffsetsBank.Bank = new GameOffsetBank();

        // Mappings
        CorrectedTextures.Bank = new CorrectedTextureBank();
        AdditionalTextures.Bank = new AdditionalTextureBank();
        BlockedTextures.Bank = new BlockedTextureBank();
    }

    public static void ReloadBanks()
    {
        // Alias
        ModelAliasBank.Bank.ReloadAliasBank();
        FlagAliasBank.Bank.ReloadAliasBank();
        ParticleAliasBank.Bank.ReloadAliasBank();
        MapAliasBank.Bank.ReloadAliasBank();
        GparamAliasBank.Bank.ReloadAliasBank();
        SoundAliasBank.Bank.ReloadAliasBank();
        CutsceneAliasBank.Bank.ReloadAliasBank();
        MovieAliasBank.Bank.ReloadAliasBank();

        // Format
        MsbFormatBank.Bank.ReloadFormatBank();
        FlverFormatBank.Bank.ReloadFormatBank();
        GparamFormatBank.Bank.ReloadFormatBank();

        // Data
        ParamBank.ReloadParams();
        MaterialResourceBank.Setup();
        MapGroupsBank.Bank.ReloadMapGroupBank();
        GameOffsetsBank.Bank.ReloadBank();

        // Mappings
        CorrectedTextures.Bank.ReloadBank();
        AdditionalTextures.Bank.ReloadBank();
        BlockedTextures.Bank.ReloadBank();
    }
}

// Alias
public static class ModelAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class FlagAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class ParticleAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class MapAliasBank
{
    public static AliasBank Bank { get; set; }

    public static Dictionary<string, string> MapNames;

    public static void ReloadMapNames()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Alias Bank - Load Map Names", TaskManager.RequeueType.None, false,
        () =>
        {
            if (Project.Type != ProjectType.Undefined)
            {
                if (Bank.AliasNames != null)
                {
                    if (Bank.aliasType is AliasBankType.Map)
                    {
                        var _mapNames = new Dictionary<string, string>();
                        var entries = Bank.AliasNames.GetEntries("Maps");

                        if (entries != null)
                        {
                            foreach (var entry in Bank.AliasNames.GetEntries("Maps"))
                            {
                                if (!CFG.Current.MapNameAtlas_ShowUnused)
                                {
                                    if (entry.tags[0] != "unused")
                                    {
                                        _mapNames.Add(entry.id, entry.name);
                                    }
                                    else
                                    {
                                        _mapNames.Add(entry.id, entry.name);
                                    }
                                }
                            }

                            MapNames = _mapNames;
                        }
                    }
                }
            }
        }));
    }

    public static string GetMapName(string mapId)
    {
        if (MapNames == null)
            return "";

        if (MapNames.ContainsKey(mapId))
        {
            return $"{MapNames[mapId]}";
        }

        return $"";
    }

    public static string GetMapNameFromFilename(string filename)
    {
        if (MapNames == null)
            return "";

        // Map-specific names will be 11 characters or more
        if (filename.Length >= 11)
        {
            // Check to see if it is a map or cutscene specific file
            if (filename[0] is 's' || filename[0] is 'm')
            {
                var map1 = $"{filename[1]}{filename[2]}";
                var map2 = $"{filename[4]}{filename[5]}";

                foreach (var entry in MapNames)
                {
                    var id = entry.Key;

                    if (id.Length >= 11)
                    {
                        var eMap1 = $"{id[1]}{id[2]}";
                        var eMap2 = $"{id[4]}{id[5]}";

                        if (map1 == eMap1 && map2 == eMap2)
                        {
                            return $"{entry.Value}";
                        }
                    }
                }
            }
        }

        return $"";
    }
}

public static class GparamAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class SoundAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class CutsceneAliasBank
{
    public static AliasBank Bank { get; set; }
}

public static class MovieAliasBank
{
    public static AliasBank Bank { get; set; }
}

// Format
public static class MsbFormatBank
{
    public static FormatBank Bank { get; set; }
}

public static class FlverFormatBank
{
    public static FormatBank Bank { get; set; }
}

public static class GparamFormatBank
{
    public static FormatBank Bank { get; set; }
}

// Data
public static class MapGroupsBank
{
    public static MapGroupBank Bank { get; set; }
}
public static class GameOffsetsBank
{
    public static GameOffsetBank Bank { get; set; }
}

// Mappings
public static class CorrectedTextures
{
    public static CorrectedTextureBank Bank { get; set; }
}
public static class AdditionalTextures
{
    public static AdditionalTextureBank Bank { get; set; }
}
public static class BlockedTextures
{
    public static BlockedTextureBank Bank { get; set; }
}

