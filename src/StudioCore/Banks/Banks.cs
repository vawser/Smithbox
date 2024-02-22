using SoulsFormats.KF4;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Format
        MsbFormatBank.Bank = new FormatBank(FormatBankType.MSB, true);
        FlverFormatBank.Bank = new FormatBank(FormatBankType.FLVER, false);
        GparamFormatBank.Bank = new FormatBank(FormatBankType.GPARAM, false);

        // Data
        MaterialResourceBank.Setup();
    }

    public static void ReloadBanks(ProjectSettings projectSettings, NewProjectOptions projectOptions)
    {
        // Alias
        ModelAliasBank.Bank.ReloadAliasBank();
        FlagAliasBank.Bank.ReloadAliasBank();
        ParticleAliasBank.Bank.ReloadAliasBank();
        MapAliasBank.Bank.ReloadAliasBank();

        // Format
        MsbFormatBank.Bank.ReloadFormatBank();
        FlverFormatBank.Bank.ReloadFormatBank();
        GparamFormatBank.Bank.ReloadFormatBank();

        // Data
        ParamBank.ReloadParams(projectSettings, projectOptions);
        MaterialResourceBank.Setup();
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

                        foreach (var entry in Bank.AliasNames.GetEntries("Maps"))
                        {
                            if (!CFG.Current.MapAliases_ShowUnusedNames)
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
        }));
    }

    public static string GetMapName(string mapId, string baseName)
    {
        if (MapNames == null)
            return baseName;

        if (MapNames.ContainsKey(mapId))
        {
            return $"{baseName}{MapNames[mapId]}";
        }

        return $"{baseName}";
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