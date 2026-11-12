using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.Common;

public static class EditorConditions
{
    public static bool SupportsMapEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsModelEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsTimeActEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.DS2
            or ProjectType.DS2S)
        {
            return false;
        }

        return true;
    }

    public static bool SupportsTextEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsParamEditor(ProjectType curType)
    {
        return true;
    }

    /// <summary>The FX Editor: FXR effects of the games the FXR schema covers</summary>
    public static bool SupportsFxEditor(ProjectType curType)
    {
        return curType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;
    }

    /// <summary>The Behavior Editor: AC6 .bhv state machines in chr behbnds</summary>
    public static bool SupportsBehaviorEditor(ProjectType curType)
    {
        return curType is ProjectType.AC6;
    }

    /// <summary>The Sound Editor: FMOD games (DS1 to SDT) and Wwise games (ER, NR, AC6, DB_NT). Not DES (Sony MultiStream).</summary>
    public static bool SupportsSoundEditor(ProjectType curType)
    {
        return curType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT
            or ProjectType.ER or ProjectType.NR or ProjectType.AC6 or ProjectType.DB_NT;
    }

    /// <summary>The games whose sound is Wwise (.bnk / .wem); the others the Sound Editor supports are FMOD (.fsb)</summary>
    public static bool UsesWwise(ProjectType curType)
    {
        return curType is ProjectType.ER or ProjectType.NR or ProjectType.AC6 or ProjectType.DB_NT;
    }

    /// <summary>The EMEVD Editor: the games with instruction layouts in Assets/EMEVD/Layouts</summary>
    public static bool SupportsEmevdEditor(ProjectType curType)
    {
        return curType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT
            or ProjectType.ER or ProjectType.NR or ProjectType.AC6 or ProjectType.DB_NT;
    }

    /// <summary>
    /// The ESD Editor: the games whose vanilla ESDs SoulsFormats reads and rewrites byte-identical
    /// (Claude/Tools/EsdSurvey). DES is left out: its ESDs are big-endian (DLSE), which SoulsFormats can't read.
    /// DS1 (PTDE) is in but unchecked: its 32-bit files weren't surveyed.
    /// </summary>
    public static bool SupportsEsdEditor(ProjectType curType)
    {
        return curType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT
            or ProjectType.ER or ProjectType.NR or ProjectType.AC6 or ProjectType.DB_NT;
    }

    public static bool SupportsHavokEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DS3
            or ProjectType.SDT
            or ProjectType.BB
            or ProjectType.DS2S
            or ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DES
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            return true;
        }

        return false;
    }

    public static bool SupportsGraphicsParamEditor(ProjectType curType)
    {
        if (curType
            is ProjectType.DES
            or ProjectType.DS1
            or ProjectType.DS1R)
        {
            return false;
        }

        return true;
    }

    public static bool SupportsMaterialEditor(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsTextureViewer(ProjectType curType)
    {
        return true;
    }

    public static bool SupportsFileBrowser(ProjectType curType)
    {
        return true;
    }


}
