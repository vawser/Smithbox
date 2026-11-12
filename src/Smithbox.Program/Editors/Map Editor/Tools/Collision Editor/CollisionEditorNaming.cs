using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// How each game names a map collision: the model ID the Create tab asks for (the model name without its h), the
/// files and bodies of each layer, and the collision part. As the games' files and MSBs have them (MoppSurvey naming):
/// <list type="bullet">
/// <item>ER, NR, AC6, DB_NT, DS3, SDT, BB: model h001234, files h10_00_00_00_001234.hkx.dcx, part h001234_0000</item>
/// <item>DES: model h0000B0, loose file h0000b0.hkx (lower-case b), body h0000B0, part named as its model</item>
/// <item>DS1 / DS1R: model h1000B1, file h1000B1A10.hkx (DS1R .hkx.dcx in its binder), body h1000B1, part as its model</item>
/// <item>DS2S: model h01_0200, file h01_0200.hkx.dcx, body h01_0200, part h01_0200_0000</item>
/// </list>
/// Takes the game as its project type's name, so the survey tools link it without Smithbox's types.
/// </summary>
public sealed class CollisionEditorNaming
{
    private readonly string Game;

    private CollisionEditorNaming(string game)
    {
        Game = game;
    }

    public static CollisionEditorNaming For(string game) => new(game);

    private bool IsBlockNamed => Game is "DES" or "DS1" or "DS1R";

    /// <summary>The ID's form, for messages: 001234, 1234B1 or 01_0200.</summary>
    public string Example => IsBlockNamed ? "1234B1" : Game is "DS2S" ? "01_0200" : "001234";

    /// <summary>The longest ID typed.</summary>
    public int MaxLength => IsBlockNamed || Game is "DS2S" ? 7 : 6;

    /// <summary>Whether a typed ID has the game's form.</summary>
    public bool IsValid(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return IsBlockNamed ? Regex.IsMatch(id, @"^\d{4}[Bb]\d$")
            : Game is "DS2S" ? Regex.IsMatch(id, @"^\d\d_\d{4}$")
            : Regex.IsMatch(id, @"^\d{6}$");
    }

    /// <summary>An ID as the game writes it in model names (DES's and DS1's B upper case).</summary>
    public string Normalize(string id) => IsBlockNamed ? id.ToUpperInvariant() : id;

    public string ModelName(string id) => $"h{Normalize(id)}";

    /// <summary>The file of a layer (h, l) of a model of a map.</summary>
    public string FileName(string type, string mapId, string id)
    {
        id = Normalize(id);

        return Game switch
        {
            "DES" => $"{type}{id.ToLowerInvariant()}.hkx",
            "DS1" => $"{type}{id}A{mapId.Substring(1, 2)}.hkx",
            "DS1R" => $"{type}{id}A{mapId.Substring(1, 2)}.hkx.dcx",
            "DS2S" => $"{type}{id}.hkx.dcx",
            _ => $"{type}{mapId.Substring(1)}_{id}.hkx.dcx"
        };
    }

    /// <summary>The name a file's body has: the model, with the file's layer letter.</summary>
    public string BodyName(string type, string id) => $"{type}{Normalize(id)}";

    /// <summary>The model ID of a collision file, or null if it isn't named as the game names them.</summary>
    public string IdFromFile(string fileName)
    {
        var match = IsBlockNamed ? Regex.Match(fileName, @"^[hlf](\d{4}[Bb]\d)(A\d\d)?\.hkx", RegexOptions.IgnoreCase)
            : Game is "DS2S" ? Regex.Match(fileName, @"^[hlf](\d\d_\d{4})\.hkx", RegexOptions.IgnoreCase)
            : Regex.Match(fileName, @"_(\d{6})\.hkx", RegexOptions.IgnoreCase);

        return match.Success ? Normalize(match.Groups[1].Value) : null;
    }

    /// <summary>The model ID of a model name (h and the ID), or null.</summary>
    public string IdFromModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2 || char.ToLowerInvariant(modelName[0]) != 'h')
            return null;

        var id = modelName.Substring(1);

        return IsValid(id) ? Normalize(id) : null;
    }

    /// <summary>
    /// A new ID after the map's used ones: the next free number after the highest (below 900000 for six digits, 9000
    /// for the older games' four, those numbers being kept apart), in the block (DES, DS1: the digit after B) or group (DS2S: the digits before
    /// the _) the map uses most. Empty if none is free.
    /// </summary>
    public string Suggest(IReadOnlyCollection<string> used)
    {
        var taken = new HashSet<string>(used.Select(Normalize), StringComparer.OrdinalIgnoreCase);

        if (IsBlockNamed || Game is "DS2S")
        {
            // The block or group: B1 in h1000B1, 01 in h01_0200
            string Group(string id) => IsBlockNamed ? id.Substring(5, 1) : id.Substring(0, 2);
            string Number(string id) => IsBlockNamed ? id.Substring(0, 4) : id.Substring(3, 4);

            var valid = taken.Where(IsValid).ToList();
            var group = valid.GroupBy(Group).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).FirstOrDefault()?.Key
                ?? (IsBlockNamed ? "0" : "00");
            // Below 9000, as the maps keep their 9xxx models apart (DES m01_00_00_00 has h9999B0); if nothing is free
            // past the highest, the lowest free number
            var highest = valid.Where(i => Group(i) == group).Select(i => int.Parse(Number(i))).Where(n => n < 9000).DefaultIfEmpty(-1).Max();

            string Make(int n) => IsBlockNamed ? $"{n:D4}B{group}" : $"{group}_{n:D4}";

            for (var n = highest + 1; n < 9000; n++)
            {
                if (!taken.Contains(Make(n)))
                    return Make(n);
            }

            for (var n = 0; n < 10000; n++)
            {
                if (!taken.Contains(Make(n)))
                    return Make(n);
            }

            return "";
        }

        var numbers = taken.Where(IsValid).Select(int.Parse).ToHashSet();
        var top = numbers.Where(n => n < 900000).DefaultIfEmpty(0).Max();

        for (var n = top + 1; n < 1000000; n++)
        {
            if (!numbers.Contains(n))
                return n.ToString("D6");
        }

        return "";
    }

    /// <summary>
    /// The collision part's name: DES and DS1 name a part as its model (a second one with an instance number);
    /// the others its model and the first free instance number.
    /// </summary>
    public string PartName(string modelName, ISet<string> taken, out int instance)
    {
        instance = 0;

        if (IsBlockNamed && !taken.Contains(modelName))
            return modelName;

        while (taken.Contains($"{modelName}_{instance:D4}"))
            instance++;

        return $"{modelName}_{instance:D4}";
    }

    /// <summary>The entity ID of a part with no entity: -1 in DES and DS1, 0 in the later games.</summary>
    public int NoEntityId => IsBlockNamed ? -1 : 0;
}
