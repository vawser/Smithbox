using StudioCore.AssetLocator;
using StudioCore.UserProject;
using System.Collections.Generic;

namespace StudioCore.Locators;
public static class FileAssetLocator
{
    // TAE
    public static List<string> GetAnimationBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".anibnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    public static List<string> GetBehaviorBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS1 
            or ProjectType.DS1R
            or ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".behbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;

    }
    public static List<string> GetCharacterBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\chr";
        var paramExt = @".chrbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Cutscene
    public static List<string> GetCutsceneBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3
        var paramDir = @"\remo";
        var paramExt = @".remobnd.dcx";

        // Sekiro + ER + AC6
        if (Project.Type is ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            paramDir = @"\cutscene";
            paramExt = @".cutscenebnd.dcx";
        }

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Material
    public static List<string> GetMaterialBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro
        var paramDir = @"\mtd";
        var paramExt = @".mtdbnd.dcx";

        if(Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            paramDir = @"\material";
            paramExt = @".matbinbnd.dcx";
            // Account for .devpatch in ER (e.g. matbinbnd.devpatch.dcx)
        }

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Particle 
    public static List<string> GetParticleBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\sfx";
        var paramExt = @".ffxbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Scipt
    public static List<string> GetEventBinders()
    {
        // Not supported
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }

    // Talk
    public static List<string> GetTalkBinders()
    {
        // Not supported + Sekiro
        if (Project.Type is ProjectType.DS2S
            or ProjectType.BB
            or ProjectType.DES)
        {
            return new List<string>();
        }

        // DS1R + DS3 + Sekiro + ER + AC6
        var paramDir = @"\script\talk";
        var paramExt = @".talkesdbnd.dcx";

        List<string> ret = LocatorUtils.GetAssetFiles(paramDir, paramExt);

        return ret;
    }
}
