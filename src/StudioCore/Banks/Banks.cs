using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ParamEditor;
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
        MsbFormatBank.Bank = new FormatBank(FormatBankType.MSB);
        FlverFormatBank.Bank = new FormatBank(FormatBankType.FLVER);
        GparamFormatBank.Bank = new FormatBank(FormatBankType.GPARAM);

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