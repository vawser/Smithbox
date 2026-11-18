#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>How a face field is edited</summary>
public enum FaceFieldKind
{
    /// <summary>A byte from 0 to 255: a FaceGen control or mode, or an amount (gloss, beard density and the like)</summary>
    Slider,

    /// <summary>Three bytes, X_R, X_G and X_B, edited together as one colour</summary>
    Color,

    /// <summary>Anything else (part IDs, flags), edited as the Param Editor edits it</summary>
    Field
}

/// <summary>A field of a face param, or for a colour the three fields of it (named by the red one)</summary>
public sealed record FaceField(string Name, FaceFieldKind Kind, string? Green = null, string? Blue = null);

/// <summary>A titled group of face fields</summary>
public sealed class FaceFieldGroup
{
    public string Title = "";
    public List<FaceField> Fields = new();
}

/// <summary>
/// The groups the Properties window shows a face param's fields in. Taken from the param's field layout when it has
/// one (the ER and NR layouts follow the character creator's menu); otherwise the fields are grouped by their names.
/// Every field of the param is in exactly one group.
/// </summary>
public static class FaceGenLayout
{
    public static List<FaceFieldGroup> Build(ProjectEntry project, Param param)
    {
        var def = param.AppliedParamdef;

        if (def == null)
            return new List<FaceFieldGroup>();

        var fields = def.Fields.Where(f => !IsPadding(f)).ToList();
        var names = new HashSet<string>(fields.Select(f => f.InternalName));
        var entries = new Dictionary<string, FaceField>();
        var colorPart = new Dictionary<string, string>();

        // Colours first: X_R with an X_G and an X_B beside it
        foreach (var field in fields)
        {
            var name = field.InternalName;

            if (!name.EndsWith("_R", StringComparison.Ordinal))
                continue;

            var stem = name[..^2];
            var green = stem + "_G";
            var blue = stem + "_B";

            if (!names.Contains(green) || !names.Contains(blue))
                continue;

            entries[name] = new FaceField(name, FaceFieldKind.Color, green, blue);
            colorPart[name] = name;
            colorPart[green] = name;
            colorPart[blue] = name;
        }

        foreach (var field in fields)
        {
            if (colorPart.ContainsKey(field.InternalName))
                continue;

            entries[field.InternalName] = new FaceField(field.InternalName, IsSlider(field) ? FaceFieldKind.Slider : FaceFieldKind.Field);
        }

        // The field list in param order, with each colour in place of its first field
        var ordered = new List<FaceField>();
        var placed = new HashSet<string>();

        foreach (var field in fields)
        {
            var key = colorPart.TryGetValue(field.InternalName, out var red) ? red : field.InternalName;

            if (placed.Add(key))
                ordered.Add(entries[key]);
        }

        return FromFieldLayout(project, param, ordered, colorPart) ?? ByName(ordered);
    }

    private static bool IsPadding(PARAMDEF.Field field) =>
        field.DisplayType is PARAMDEF.DefType.dummy8 ||
        field.InternalName.StartsWith("pad", StringComparison.OrdinalIgnoreCase) ||
        field.InternalName.StartsWith("reserved", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A whole byte that isn't an ID: the FaceGen bytes, and the amounts of the newer games. Part IDs, hair styles and
    /// bit flags are not sliders, as their values are choices rather than amounts.
    /// </summary>
    private static bool IsSlider(PARAMDEF.Field field)
    {
        if (field.DisplayType is not PARAMDEF.DefType.u8 || field.BitSize is not (-1 or 8))
            return false;

        var name = field.InternalName;

        return !name.Contains("partsId", StringComparison.OrdinalIgnoreCase) &&
               !name.Equals("hairStyle", StringComparison.OrdinalIgnoreCase) &&
               !name.EndsWith("Id", StringComparison.Ordinal) &&
               !name.EndsWith("_Base", StringComparison.Ordinal);
    }

    private static List<FaceFieldGroup>? FromFieldLayout(ProjectEntry project, Param param, List<FaceField> ordered,
        Dictionary<string, string> colorPart)
    {
        var paramData = project.Handler.ParamData;
        var meta = paramData.GetParamMeta(param.AppliedParamdef);

        if (meta == null || string.IsNullOrEmpty(meta.FieldLayout))
            return null;

        var layout = paramData.FieldLayouts?.Entries.FirstOrDefault(e => e.Name == meta.FieldLayout);

        if (layout == null || layout.Groups.Count == 0)
            return null;

        var byName = ordered.ToDictionary(f => f.Name);
        var placed = new HashSet<string>();
        var groups = new List<FaceFieldGroup>();

        foreach (var entry in layout.Groups)
        {
            var group = new FaceFieldGroup { Title = Title(entry) };

            foreach (var fieldName in entry.Fields ?? new List<string>())
            {
                var key = colorPart.TryGetValue(fieldName, out var red) ? red : fieldName;

                if (byName.TryGetValue(key, out var field) && placed.Add(key))
                    group.Fields.Add(field);
            }

            if (group.Fields.Count > 0)
                groups.Add(group);
        }

        var rest = ordered.Where(f => !placed.Contains(f.Name)).ToList();

        if (rest.Count > 0)
            groups.Add(new FaceFieldGroup { Title = LOC.Get("FACEGEN_Group_Other"), Fields = rest });

        return groups;
    }

    private static string Title(FieldLayoutEntry entry)
    {
        var name = entry.GetName();

        return string.IsNullOrEmpty(name) ? entry.Key : name;
    }

    // ---- grouped by name, for params without a field layout

    private static readonly (string LocKey, Func<string, bool> Match)[] NameGroups =
    {
        ("FACEGEN_Group_Template", n => n is "age" or "gender" or "face_partsId" ||
                                        n.StartsWith("caricature", StringComparison.OrdinalIgnoreCase)),
        ("FACEGEN_Group_Shape", n => FaceGenParams.ParseFaceGenField(n) is { IsGeometry: true }),
        ("FACEGEN_Group_Texture", n => FaceGenParams.ParseFaceGenField(n) is { IsGeometry: false }),
        ("FACEGEN_Group_Eyes", n => n.StartsWith("eye", StringComparison.OrdinalIgnoreCase)),
        ("FACEGEN_Group_Hair", n => n.StartsWith("hair", StringComparison.OrdinalIgnoreCase) ||
                                    n.StartsWith("beard", StringComparison.OrdinalIgnoreCase) ||
                                    n.StartsWith("eyebrow", StringComparison.OrdinalIgnoreCase) ||
                                    n.StartsWith("eyelash", StringComparison.OrdinalIgnoreCase) ||
                                    n.StartsWith("body_hair", StringComparison.OrdinalIgnoreCase)),
        ("FACEGEN_Group_Skin", n => n.StartsWith("skin", StringComparison.OrdinalIgnoreCase) ||
                                    n.StartsWith("face_", StringComparison.OrdinalIgnoreCase)),
        ("FACEGEN_Group_Accessories", n => n.StartsWith("accessories", StringComparison.OrdinalIgnoreCase) ||
                                           n.StartsWith("decal", StringComparison.OrdinalIgnoreCase)),
        ("FACEGEN_Group_Body", n => n.StartsWith("chrBodyScale", StringComparison.OrdinalIgnoreCase)),
    };

    private static List<FaceFieldGroup> ByName(List<FaceField> ordered)
    {
        var groups = NameGroups.Select(g => new FaceFieldGroup { Title = LOC.Get(g.LocKey) }).ToList();
        var other = new FaceFieldGroup { Title = LOC.Get("FACEGEN_Group_Other") };

        foreach (var field in ordered)
        {
            var index = Array.FindIndex(NameGroups, g => g.Match(field.Name));
            (index >= 0 ? groups[index] : other).Fields.Add(field);
        }

        groups.Add(other);
        return groups.Where(g => g.Fields.Count > 0).ToList();
    }
}
