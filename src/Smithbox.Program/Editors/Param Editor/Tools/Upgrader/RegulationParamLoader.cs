using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>
/// The games the Param Upgrader supports. Kept separate from ProjectType so this code has no
/// dependency on the rest of Smithbox (the diff migration tool compiles these files on their own).
/// </summary>
public enum UpgraderGame
{
    ER,
    AC6,
    NR
}

/// <summary>
/// What is needed to turn a regulation's .param files into Params with a paramdef applied.
/// </summary>
public sealed class RegulationLoadContext
{
    public UpgraderGame Game { get; init; }

    /// <summary>Param type -> paramdef (version aware)</summary>
    public IReadOnlyDictionary<string, PARAMDEF> ParamDefs { get; init; }

    /// <summary>Param file name -> param type, for games whose files don't carry a usable param type (AC6)</summary>
    public IReadOnlyDictionary<string, string> TypeMapping { get; init; } = new Dictionary<string, string>();

    /// <summary>Param file names that always use the mapping, even when their own param type has a paramdef</summary>
    public ICollection<string> TypeExceptions { get; init; } = new HashSet<string>();
}

/// <summary>
/// Reads regulation.bin files into Params. Shared by the Param Upgrader, the Regulation Auto Merge and the
/// upgrade diff builder, so that all of them apply the same param type mapping and the same fixups.
/// </summary>
public static class RegulationParamLoader
{
    public static BND4 ReadBinder(UpgraderGame game, byte[] data)
    {
        return game switch
        {
            UpgraderGame.ER => SFUtil.DecryptERRegulation(data),
            UpgraderGame.AC6 => SFUtil.DecryptAC6Regulation(data),
            UpgraderGame.NR => SFUtil.DecryptNightreignRegulation(data),
            _ => throw new NotSupportedException(game.ToString())
        };
    }

    public static ulong ReadVersion(IBinder binder)
    {
        if (!ulong.TryParse(binder.Version, out var version))
            throw new InvalidDataException($"The regulation version \"{binder.Version}\" is not a number.");

        return version;
    }

    /// <summary>
    /// Loads every .param in the binder. A param that can't be loaded (no paramdef, or a paramdef that
    /// doesn't fit the row size) is left out and described in <paramref name="warnings"/>.
    /// </summary>
    public static Dictionary<string, Param> LoadParams(
        IBinder binder, RegulationLoadContext context, List<string> warnings, out ulong version)
    {
        version = ReadVersion(binder);

        var result = new Dictionary<string, Param>(StringComparer.Ordinal);

        foreach (BinderFile file in binder.Files)
        {
            if (!file.Name.EndsWith(".PARAM", StringComparison.OrdinalIgnoreCase))
                continue;

            var paramName = Path.GetFileNameWithoutExtension(file.Name.Replace('\\', '/').Split('/')[^1]);

            if (result.ContainsKey(paramName))
                continue;

            var param = LoadParam(paramName, file.Bytes.ToArray(), context, version, warnings);
            if (param != null)
                result.Add(paramName, param);
        }

        return result;
    }

    /// <summary>
    /// Loads one .param file. Returns null (with a warning) when it can't be loaded.
    /// </summary>
    public static Param LoadParam(
        string paramName, byte[] data, RegulationLoadContext context, ulong version, List<string> warnings)
    {
        Param param;

        try
        {
            param = Param.ReadIgnoreCompression(data);
        }
        catch (Exception e)
        {
            warnings?.Add($"{paramName}: could not be read ({e.Message}).");
            return null;
        }

        if (!ResolveParamType(paramName, param, context))
        {
            warnings?.Add($"{paramName}: no paramdef for param type \"{param.ParamType}\".");
            return null;
        }

        ApplyFixups(context.Game, param, version);

        try
        {
            param.ApplyParamdef(context.ParamDefs[param.ParamType], version, paramName);
        }
        catch (Exception e)
        {
            warnings?.Add($"{paramName}: the paramdef does not fit ({e.Message}).");
            return null;
        }

        return param;
    }

    private static bool ResolveParamType(string paramName, Param param, RegulationLoadContext context)
    {
        if (context.Game == UpgraderGame.AC6)
        {
            var mustMap = string.IsNullOrEmpty(param.ParamType) ||
                !context.ParamDefs.ContainsKey(param.ParamType) ||
                paramName == "EquipParamWeapon_Npc" ||
                context.TypeExceptions.Contains(paramName);

            if (mustMap)
            {
                if (!context.TypeMapping.TryGetValue(paramName, out var mapped))
                    return false;

                param.ParamType = mapped;
            }
        }

        return !string.IsNullOrEmpty(param.ParamType) && context.ParamDefs.ContainsKey(param.ParamType);
    }

    /// <summary>
    /// Row size fixups for params that earlier tools saved at the wrong size.
    /// </summary>
    public static void ApplyFixups(UpgraderGame game, Param param, ulong version)
    {
        if (game != UpgraderGame.ER)
            return;

        if (version >= 10601000 && param.ParamType == "CHR_MODEL_PARAM_ST")
            param.ExpandParamSize(12, 16);

        if (version >= 11210015)
        {
            if (param.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
                param.ExpandParamSize(880, 1024);
            if (param.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
                param.ExpandParamSize(112, 144);
            if (param.ParamType == "SIGN_PUDDLE_PARAM_ST")
                param.ExpandParamSize(32, 48);
        }
    }
}
