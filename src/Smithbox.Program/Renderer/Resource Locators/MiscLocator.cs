using StudioCore.Application;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Renderer;
public static class MiscLocator
{
    // TAE
    public static List<string> GetCharacterTimeActBinders(ProjectEntry project, bool ignoreProject = false)
    {
        List<string> ret = new List<string>();

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"chr";
        var paramExt = @".anibnd.dcx";

        if (project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            paramDir = Path.Join("timeact", "chr");
            paramExt = @".tae";
        }

        ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt, ignoreProject);

        return ret;
    }

    // AC6
    public static List<string> GetCharacterBehaviorTimeActBinders(ProjectEntry project, bool ignoreProject = false)
    {
        List<string> ret = new List<string>();

        var paramDir = @"chr";
        var paramExt = @".behbnd.dcx";

        ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt, ignoreProject);

        return ret;
    }

    public static List<string> GetObjectTimeActBinders(ProjectEntry project, bool ignoreProject = false)
    {
        List<string> ret = new List<string>();

        var paramDir = @"obj";
        var paramExt = @".objbnd.dcx";

        ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt, ignoreProject);

        if (project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            paramDir = Path.Join("timeact", "obj");
            paramExt = @".tae";

            ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt, ignoreProject);
        }

        return ret;
    }

    public static Dictionary<string, List<string>> GetAssetTimeActBinders_ER(ProjectEntry project, bool ignoreProject = false)
    {
        Dictionary<string, List<string>> assetDict = new();

        var paramDir = Path.Join("asset", "aeg");
        var paramExt = @".geombnd.dcx";

        List<string> ret = new List<string>();

        var searchDir = Path.Join(project.DataPath, paramDir);
        foreach (var folderPath in Directory.EnumerateDirectories(searchDir))
        {
            var folderName = folderPath.Substring(folderPath.Length - 6);

            ret = LocatorUtils.GetAssetFiles(project, Path.Join(paramDir, folderName), paramExt, ignoreProject);
            assetDict.Add(folderName, ret);
        }

        return assetDict;
    }

    public static List<string> GetAssetTimeActBinders_AC6(ProjectEntry project, bool ignoreProject = false)
    {
        List<string> ret = new List<string>();

        var paramDir = Path.Join("asset", "environment", "geometry");
        var paramExt = @".geombnd.dcx";

        ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt, ignoreProject);

        return ret;
    }

    public static List<string> GetHavokBehaviorBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS3 + Sekiro + ER + AC6
        var paramDir = @"chr";
        var paramExt = @".behbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;

    }

    public static List<string> GetHavokCollisionBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.AC6)
        {
            return new List<string>();
        }

        // ER
        var baseDir = Path.Join(project.DataPath, "map");
        var targetExt = @".hkxbhd";

        List<string> combinedList = new();

        if (Directory.Exists(baseDir))
        {
            foreach (var folder in Directory.GetDirectories(baseDir))
            {
                if (Directory.Exists(folder))
                {
                    foreach (var subFolder in Directory.GetDirectories(folder))
                    {
                        if (Directory.Exists(subFolder))
                        {
                            foreach (var file in Directory.GetFiles(subFolder))
                            {
                                if (file.Contains(targetExt))
                                {
                                    combinedList.Add(file);
                                }
                            }
                        }
                    }
                }
            }
        }

        return combinedList;
    }

    public static List<string> GetCharacterBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"chr";
        var paramExt = @".chrbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }

    // Cutscene
    public static List<string> GetCutsceneBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3
        var paramDir = @"remo";
        var paramExt = @".remobnd.dcx";

        // Sekiro + ER + AC6
        if (project.ProjectType is ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            paramDir = @"cutscene";
            paramExt = @".cutscenebnd.dcx";
        }

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }

    // Material
    public static List<string> GetMaterialBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro
        var paramDir = @"mtd";
        var paramExt = @".mtdbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }

    public static List<string> GetMaterialBinBinders(ProjectEntry project)
    {
        if (project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            var paramDir = @"material";
            var paramExt = @".matbinbnd.dcx";

            List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);
            return ret;

        }

        return new List<string>();
    }

    // Particle 
    public static List<string> GetParticleBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"sfx";
        var paramExt = @".ffxbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }

    // Scipt
    public static List<string> GetEventBinders(ProjectEntry project)
    {
        // Not supported
        if (project.ProjectType is ProjectType.DS2S
            or ProjectType.DS2
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"event";
        var paramExt = @".emevd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }

    // Talk
    public static List<string> GetTalkBinders(ProjectEntry project)
    {
        // Not supported + Sekiro
        if (project.ProjectType is ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = Path.Join("script", "talk");
        var paramExt = @".talkesdbnd.dcx";

        if(project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            paramDir = @"ezstate";
            paramExt = @".esd";
        }

        List<string> ret = LocatorUtils.GetAssetFiles(project, paramDir, paramExt);

        return ret;
    }
}
