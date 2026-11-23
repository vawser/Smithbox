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

    /// <summary>
    /// The FX Editor: FXR effects of the games the FXR schema covers, and the DLSE effects of DES, DS2 and DS2S
    /// (read-only for now: FFX Older Games Plan, F3a)
    /// </summary>
    public static bool SupportsFxEditor(ProjectType curType)
    {
        return SupportsFxPlayback(curType) || curType is ProjectType.DES or ProjectType.DS2 or ProjectType.DS2S;
    }

    /// <summary>
    /// Effect playback (the FX Editor's viewport, and effects shown in the Map, Time Act, Projectile, Cutscene and Lua
    /// editors): the FXR3 games, whose effects the runtime can build
    /// </summary>
    public static bool SupportsFxPlayback(ProjectType curType)
    {
        return curType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;
    }

    /// <summary>
    /// The Cutscene Editor: the games whose MQBs and cut Havok files are read in full (Cutscene Editor Analysis, phases
    /// 0 and 1). BB and DS2 aren't surveyed yet.
    /// </summary>
    public static bool SupportsCutsceneEditor(ProjectType curType)
    {
        return curType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;
    }

    /// <summary>
    /// The Projectile Editor: the games whose bullet params the simulation reads (BulletDefinition). Its effects also
    /// need FX playback (<see cref="SupportsFxPlayback"/>).
    /// </summary>
    public static bool SupportsProjectileEditor(ProjectType curType)
    {
        return curType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S
            or ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;
    }

    /// <summary>
    /// The FaceGen Editor: the games with face params (FaceGenParam or FaceParam). Within them, what it shows is
    /// decided by which of the params the game has.
    /// </summary>
    public static bool SupportsFaceGenEditor(ProjectType curType)
    {
        return curType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3
            or ProjectType.SDT or ProjectType.ER or ProjectType.NR;
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

    /// <summary>
    /// The Lua Editor: the games whose Lua binders Claude/Tools/LuaAiSurvey has checked (decompiled text faithful,
    /// .luagnl rebuilt from it). See Claude/Documentation/Lua Editor Game Expansion.md.
    /// NR: 472/472 decompile, 469 recompile byte-identical, every binder, .luagnl and .luainfo round trips.
    /// SDT: per-map binders; 672/672 decompile and recompile, 656/672 byte-identical (102010_battle's stray goto fixed
    /// by DSLuaDecompiler's RemoveFallthroughJumpsPass).
    /// BB: per-map binders; 846/846 decompile and recompile; battle goals in the older global form (X_Activate).
    /// DS3: per-map binders; 1106/1106 decompile and recompile; battle goals as tables, odds chains and direct goals.
    /// AC6: logic scripts, each the battle goal in aicommon's EZOP framework (TacCtrlFuncArr / OpSetArr; 297/305 read);
    /// 353/353 decompile and recompile, no function differing in more than register / layout opcodes once the Lua 5.0
    /// TEST, recursive-local and up-value-name fixes are in.
    /// DB_NT: one binder per goal (as ER); 202/202 decompile and recompile; battle goals as tables, including the
    /// player-companion Common_Battle_Activate_PCbot (55/70 read; the rest are aicommon act libraries).
    /// DS1R: per-map binders (as SDT / BB / DS3) plus eventCommon; 540/540 decompile and recompile, 539 byte-identical;
    /// battle goals in BB's global form (X_Activate), 445/455 read. Shipped .luagnl / .luainfo carry trailing slack the
    /// rewrite drops (content identical).
    /// In every game, scripts the decompiler can't fully turn into text are read-only (LuaScript.IsDecompileIncomplete).
    /// </summary>
    public static bool SupportsLuaEditor(ProjectType curType)
    {
        return curType is ProjectType.ER or ProjectType.NR or ProjectType.SDT or ProjectType.BB or ProjectType.DS3
            or ProjectType.AC6 or ProjectType.DB_NT or ProjectType.DS1R;
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
