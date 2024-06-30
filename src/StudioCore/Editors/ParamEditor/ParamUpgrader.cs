using Andre.Formats;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParamEditor.ParamBank;

namespace StudioCore.Editors.ParamEditor;

public static class ParamUpgrader
{
    public static string LatestDisplayVersion_AC6 = "1.06.1";
    public static ulong LatestVersion_AC6 = 10610279;

    public static string LatestDisplayVersion_ER = "1.12.2";
    public static ulong LatestVersion_ER = 11220021;

    public static string CurrentDisplayVersion;
    public static ulong CurrentVersion;

    public static bool IsUpgradingParams = false;

    public static bool ErrorDuringUpgrading = false;

    /// <summary>
    /// Return latest regulation.bin version in string form.
    /// </summary>
    /// <returns></returns>
    public static string GetLatestDisplayVersion()
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.ER: return LatestDisplayVersion_ER;
            case ProjectType.AC6: return LatestDisplayVersion_AC6;
            default: break;
        }

        return "";
    }

    /// <summary>
    /// Return latest regulation.bin version in ulong form.
    /// </summary>
    /// <returns></returns>
    public static ulong GetLatestVersion()
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.ER: return LatestVersion_ER;
            case ProjectType.AC6: return LatestVersion_AC6;
            default: break;
        }

        return 0;
    }

    /// <summary>
    /// Start the upgrade process
    /// </summary>
    public static void UpgradeRegulationVersion()
    {
        // Backup modded params
        var modRegulationPath = $@"{Smithbox.ProjectRoot}\regulation.bin";
        File.Copy(modRegulationPath, $@"{modRegulationPath}.upgrade.bak", true);

        IsUpgradingParams = true;
        ParamBank.PrimaryBank.SaveParams();
    }

    /// <summary>
    /// Called from ParamBank.SaveParams - Updates the regulation version.
    /// </summary>
    /// <param name="regParams"></param>
    /// <returns></returns>
    public static BND4 UpgradeRegulation(BND4 regParams)
    {
        var regulation = regParams;

        regulation.Version = GetLatestVersion().ToString();

        return regulation;
    }

    /// <summary>
    /// Called after reload of project - Injects the latest vanilla regulations rows
    /// </summary>
    public static void AddNewRegulationRows()
    {
        Dictionary<string, HashSet<int>> conflicts = new();
        
        string baseRegulationPath = GetBaseRegulationPath(CurrentVersion);

        if (!File.Exists(baseRegulationPath))
        {
            PlatformUtils.Instance.MessageBox("Failed to find old vanilla reguilation path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ErrorDuringUpgrading = true;
            return;
        }

        Dictionary<string, Param> oldVanillaRegulationParam = ParamBank.VanillaBank.GetOldVanillaParams(baseRegulationPath);

        // ParamBank.PrimaryBank;
        // ParamBank.VanillaBank;


        // Add new rows from latest vanilla regulation

    }

    /// <summary>
    /// Called after reload of project - Allows for the massedits to apply proper as the PARAMDEF will now be aligned with the latest version.
    /// </summary>
    public static void ApplyNewRegulationMassEdits()
    {
        // Skip if error occured during AddNewRegulationRows
        if (ErrorDuringUpgrading)
            return;



        // Apply mass edits
    }

    private static string GetBaseRegulationPath(ulong version)
    {
        var baseRegulationPath = "";
        var regulationFolder = "";
        var storedRegulationDirectory = AppContext.BaseDirectory + $"\\Assets\\Regulations\\{ResourceMiscLocator.GetGameIDForDir()}\\";

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            switch (version)
            {
                case 10210038: regulationFolder = "1.02.1 (10210038)"; break;
                case 10310059: regulationFolder = "1.03.1 (10310059)"; break;
                case 10320064: regulationFolder = "1.03.2 (10320064)"; break;
                case 10330078: regulationFolder = "1.03.3 (10330078)"; break;
                case 10410090: regulationFolder = "1.04.1 (10410090)"; break;
                case 10420097: regulationFolder = "1.04.2 (10420097)"; break;
                case 10501000: regulationFolder = "1.05 (10501000)"; break;
                case 10601000: regulationFolder = "1.06 (10601000)"; break;
                case 10701000: regulationFolder = "1.07 (10701000)"; break;
                case 10710188: regulationFolder = "1.07.1 (10710188)"; break;
                case 10801000: regulationFolder = "1.08 (10801000)"; break;
                case 10811000: regulationFolder = "1.08.1 (10811000)"; break;
                case 10901000: regulationFolder = "1.09 (10901000)"; break;
                case 10911000: regulationFolder = "1.09.1 (10911000)"; break;
                case 11001000: regulationFolder = "1.10 (11001000)"; break;
                case 11210015: regulationFolder = "1.12.1 (11210015)"; break;
                case 11220021: regulationFolder = "1.12.2 (11220021)"; break;
            }
        }

        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            switch (version)
            {
                case 10100129: regulationFolder = "1.01 (10100129)"; break;
                case 10210005: regulationFolder = "1.02.1 (10210005)"; break;
                case 10300151: regulationFolder = "1.03 (10300151)"; break;
                case 10310185: regulationFolder = "1.03.1 (10310185)"; break;
                case 10400193: regulationFolder = "1.04 (10400193)"; break;
                case 10410243: regulationFolder = "1.04.1 (10410243)"; break;
                case 10500262: regulationFolder = "1.05 (10500262)"; break;
                case 10600278: regulationFolder = "1.06 (10600278)"; break;
                case 10610279: regulationFolder = "1.06.1 (10610279)"; break;
            }
        }

        if (regulationFolder != "")
        {
            baseRegulationPath = $"{storedRegulationDirectory}\\{regulationFolder}\\regulation.bin";
        }

        return baseRegulationPath;
    }
}
