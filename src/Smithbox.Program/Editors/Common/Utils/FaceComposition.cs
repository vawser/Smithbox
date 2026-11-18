#nullable enable
using Andre.Formats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.Common;

/// <summary>A part of a face: the field that chose it, the value in it, and the part model it names</summary>
/// <param name="Field">The face param field, e.g. hair_partsId</param>
/// <param name="Value">The value of the field</param>
/// <param name="Model">The part model shown, or null if there is none to show</param>
/// <param name="IsFallback">True when the value names no model, and <paramref name="Model"/> is the family's first one</param>
/// <param name="Body">The body model the value names as well (DS3's face parts value), or null</param>
public sealed record FacePart(string Field, int Value, string? Model, bool IsFallback, string? Body = null)
{
    /// <summary>The models, for a picker: "fc_m_0110 + fg_a_0100", or the model alone</summary>
    public string? Label => Body != null && Model != null ? $"{Body} + {Model}" : Body ?? Model;
}

/// <summary>A choice for a part field: a value, and the models it shows</summary>
public sealed record FacePartChoice(int Value, string Model);

/// <summary>
/// The part models a face row names. Each part field names a family of models in the parts folder by its number.
/// The families are told by what the models are (their material names: Eyebrow_Base, Beard_Hair, Eyelash_Plane,
/// Leather / Fabric eye patches), and the values by the character creator's own lists (CharMakeMenuListItemParam):
/// <list type="bullet">
/// <item>hair: hr_a_NNNN, the value itself (ER's hairstyle list: 0-10 and 100-125); hr_m_ / hr_f_ in the games with
/// per-sex hair</item>
/// <item>eyes: fg_a_15NN (one model for both eyes, or the left eye where eyeRPartsId names fg_a_16NN)</item>
/// <item>eyebrows: fg_a_20NN, beards: fg_a_30NN, accessories (eye patches, blindfolds): fg_a_50NN,
/// eyelashes: fg_a_70NN</item>
/// <item>the head: fg_a_01NN, where the hundreds of the value are not part of the model number (ER uses 0-52 and
/// 100-152 for the same 18 heads)</item>
/// </list>
/// The NN00 model of a family (and hr_a_0000) has no meshes: it is "none". Decals (6000s) are texture-only parts, and
/// are not shown. The rules go by field name, so a game shows the parts whose fields it has.
/// <para>
/// Where the game's creator data says the face parts value names the body
/// (<see cref="FaceGenEditor.FaceGenSliderTable.FacePartsNameBody"/>, DS3), the value is also the body fc_&lt;sex&gt;_NNNN
/// (hundreds the musculature, tens the body hair, ones the age type: 110 is muscular, hairy and young), and the head is
/// fg_a_01NN by the last two digits, else by the ones digit (fg_a_0100 young, 0101 mature, 0102 aged, 0103 none).
/// BB takes the same rule: its values are the age type (0-2: fc_&lt;sex&gt;_000N + fg_a_010N) and 100
/// (fc_m_0100 + fg_a_0200), which the first candidate, fg_a_(0100 + value), names directly. The heads' own
/// g_DiffuseTexture2 (the body skin they blend into) confirms each pairing.
/// </para>
/// </summary>
public static class FaceComposition
{
    private sealed record Rule(string Prefix, int Base, bool DropHundreds = false);

    private static readonly Dictionary<string, Rule> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["face_partsId"] = new("fg", 100, true),
        ["facePartsId"] = new("fg", 100, true),
        ["eye_partsId"] = new("fg", 1500),
        ["eyeLPartsId"] = new("fg", 1500),
        ["eyeRPartsId"] = new("fg", 1600),
        ["eyebrow_partsId"] = new("fg", 2000),
        ["eyeBrowPartsId"] = new("fg", 2000),
        ["beard_partsId"] = new("fg", 3000),
        ["beardPartsId"] = new("fg", 3000),
        ["eyelash_partsId"] = new("fg", 7000),
        ["eyelashPartsId"] = new("fg", 7000),
        ["accessories_partsId"] = new("fg", 5000),
        ["accessoriesPartsId"] = new("fg", 5000),
        ["hair_partsId"] = new("hr", 0),
        ["hairPartsId"] = new("hr", 0),
        ["hairStyle"] = new("hr", 0),
    };

    private static readonly Regex PartName = new(@"^(fg|hr|fc)_([a-z])_(\d{4})$", RegexOptions.Compiled);

    private const string BodyPrefix = "fc";

    /// <summary>Whether the game's face parts value names the body as well as the head (DS3)</summary>
    private static bool NamesBody(ProjectEntry project, Rule rule) =>
        rule.DropHundreds && FaceGenEditor.FaceGenSliderTable.Get(project)?.FacePartsNameBody == true;

    // The part models of each family, by number then by sex letter
    private static readonly ConditionalWeakTable<ProjectEntry, Dictionary<string, Dictionary<int, List<string>>>> Families = new();

    /// <summary>Whether a face param field chooses a part model</summary>
    public static bool IsPartField(string field) => Rules.ContainsKey(field);

    /// <summary>The parts a face row names, in the order of its fields</summary>
    public static List<FacePart> Resolve(ProjectEntry project, Param.Row row, bool isFemale)
    {
        var parts = new List<FacePart>();

        foreach (var cell in row.Cells)
        {
            if (!Rules.ContainsKey(cell.Def.InternalName) || cell.Value is not IConvertible value)
                continue;

            parts.Add(Resolve(project, cell.Def.InternalName, value.ToInt32(null), isFemale));
        }

        return parts;
    }

    /// <summary>The model a part field's value names</summary>
    public static FacePart Resolve(ProjectEntry project, string field, int value, bool isFemale)
    {
        if (!Rules.TryGetValue(field, out var rule))
            return new FacePart(field, value, null, false);

        var family = GetFamily(project, rule.Prefix);
        var sex = isFemale ? 'f' : 'm';
        var namesBody = NamesBody(project, rule);

        // DS3: the value is the body's number too
        var body = namesBody && value >= 0 && GetFamily(project, BodyPrefix).TryGetValue(value, out var bodies)
            ? PickSex(bodies, sex)
            : null;

        foreach (var number in Candidates(rule, value, namesBody))
        {
            if (family.TryGetValue(number, out var models) && Pick(models, sex) is { } model)
                return new FacePart(field, value, model, false, body);
        }

        // Faces always have a head, so a head the value doesn't name is shown as the first head. The other parts are
        // left out: a missing hair or beard is shown as none.
        if (rule.DropHundreds && family.Keys.Where(n => n >= rule.Base && n < rule.Base + 100).DefaultIfEmpty(-1).Min() is >= 0 and var first)
            return new FacePart(field, value, Pick(family[first], sex), true, body);

        return new FacePart(field, value, null, false, body);
    }

    /// <summary>The values a part field can take that name a model, for a picker</summary>
    public static List<FacePartChoice> GetChoices(ProjectEntry project, string field, bool isFemale)
    {
        if (!Rules.TryGetValue(field, out var rule))
            return new List<FacePartChoice>();

        var family = GetFamily(project, rule.Prefix);
        var sex = isFemale ? 'f' : 'm';
        var span = rule.Base == 0 ? int.MaxValue : 100;

        // DS3: every body number, with the head it gets
        if (NamesBody(project, rule))
        {
            return GetFamily(project, BodyPrefix).Keys
                .OrderBy(n => n)
                .Select(n => Resolve(project, field, n, isFemale))
                .Where(p => p.Body != null)
                .Select(p => new FacePartChoice(p.Value, p.Label!))
                .ToList();
        }

        return family.Keys
            .Where(n => n >= rule.Base && n - rule.Base < span)
            .OrderBy(n => n)
            .Select(n => new FacePartChoice(n - rule.Base, Pick(family[n], sex)!))
            .ToList();
    }

    private static IEnumerable<int> Candidates(Rule rule, int value, bool onesDigit = false)
    {
        if (value < 0)
            yield break;

        yield return rule.Base + value;

        if (rule.DropHundreds && value >= 100)
            yield return rule.Base + value % 100;

        // DS3: the head is the age type, the ones digit (the tens are the body hair)
        if (onesDigit && value % 100 >= 10)
            yield return rule.Base + value % 10;
    }

    /// <summary>A body of a sex (bodies have no unisex model): the sex's own, else the male one</summary>
    private static string? PickSex(List<string> models, char sex)
    {
        return models.FirstOrDefault(m => m[3] == sex) ?? models.FirstOrDefault(m => m[3] == 'm');
    }

    /// <summary>The model for a sex: the unisex one (a), else the sex's own, else the male one</summary>
    private static string? Pick(List<string> models, char sex)
    {
        return models.FirstOrDefault(m => m[3] == 'a')
            ?? models.FirstOrDefault(m => m[3] == sex)
            ?? models.FirstOrDefault(m => m[3] == 'm')
            ?? models.FirstOrDefault();
    }

    private static Dictionary<int, List<string>> GetFamily(ProjectEntry project, string prefix)
    {
        var families = Families.GetValue(project, p =>
        {
            var result = new Dictionary<string, Dictionary<int, List<string>>>();

            foreach (var entry in p.Locator.PartFiles.Entries)
            {
                var match = PartName.Match(entry.Filename.ToLowerInvariant());

                if (!match.Success)
                    continue;

                var family = result.TryGetValue(match.Groups[1].Value, out var f) ? f : result[match.Groups[1].Value] = new();
                var number = int.Parse(match.Groups[3].Value);

                if (!family.TryGetValue(number, out var models))
                    family[number] = models = new List<string>();

                if (!models.Contains(match.Value))
                    models.Add(match.Value);
            }

            return result;
        });

        return families.TryGetValue(prefix, out var found) ? found : new Dictionary<int, List<string>>();
    }
}
