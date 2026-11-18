#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// Colours a face's character by the face's own fields: hair, beard, eyebrows and eyelashes by their parts' colours
/// (and, for hair-type shaders, their root darkness and white density), and the skin of the head and body by the
/// skin colour.
/// <para>
/// The game sets these as the material's own params, per character: a face colour <b>replaces</b> the colour param the
/// shader is drawn with (ER: <c>[Hair]</c>/<c>[FaceHair]</c> <c>snp_0_color_4</c>, <c>[Skin]</c> <c>snp_0_color_3</c>,
/// <c>[EyeLash]</c> <c>snp_0_color_1</c>). The evidence: the MATBINs' values fall inside FaceRangeParam's ranges (hair
/// colour 0–0.8, MATBIN 0.22–0.33; skin 0.1–0.9, MATBIN 0.56/0.45/0.38; root darkness 0–1, MATBIN 0.7–1; white density
/// 0–1, MATBIN 0), and the hair shader works its specular out from <c>color_4</c>'s brightness, which only makes sense
/// if <c>color_4</c> is the chosen colour.
/// </para>
/// <para>
/// Which colour a shader takes is data: <see cref="MaterialSlotRecipe.CharacterColor"/> ({GAME}_ChrCustomize.json),
/// "part" for the part's own colour, or a colour's name ("skin"). Materials with a <see cref="MaterialCustomize"/>
/// entry get the colour (and the hair values) there; colour-only materials without one (eyelashes) by their diffuse
/// colour, scaled from the material's colour to the face's.
/// </para>
/// <para>
/// The fields are found by name, the same in every game. A part field's colour is its prefix's: <c>hair_partsId</c> →
/// <c>hair_color_R/G/B</c> (ER), <c>hairPartsId</c> → <c>hairColor_R/G/B</c> (BB, DS3, SDT); a named colour is
/// <c>skin_color_R</c> or <c>skinColor_R</c>; the hair values <c>hair_rootBlack</c> / <c>hairRootBlack</c> and
/// <c>hair_whiteDensity</c> / <c>hairWhiteDensity</c>. A byte goes to a value by FaceRangeParam's field of the same
/// name (row 1 the minimum, row 2 the maximum), else by byte / 255.
/// </para>
/// <para>
/// Models are shared by everything that shows them, so the materials' own values are put back by
/// <see cref="Restore"/> when the face is no longer shown, as <see cref="FaceMorpher"/> does with the vertices.
/// </para>
/// </summary>
public sealed class FaceTinter
{
    /// <summary>The <see cref="MaterialSlotRecipe.CharacterColor"/> for the colour of the part the material is on</summary>
    public const string PartColor = "part";

    private const string RangeParamName = "FaceRangeParam";
    private const int RangeMinRow = 1;
    private const int RangeMaxRow = 2;

    private readonly ProjectEntry Project;

    // The materials that have been coloured, with their own values
    private sealed record Own(Vector4 DiffuseAlpha, MaterialCustomize? Customize);
    private readonly Dictionary<FlverResource.FlverMaterial, Own> Tinted = new();

    public FaceTinter(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>TEMPORARY (FaceGen colour check): what the last <see cref="Apply"/> did, part by part</summary>
    public string LastReport { get; private set; } = "";

    /// <summary>The red field of a colour (green and blue by the same name), e.g. "hair" → hair_color_R, or null</summary>
    public static string? ColorField(Param.Row row, string name) => FindField(row, name, "color_R", "Color_R");

    /// <summary>The colour name of a part field: its name without "_partsId" / "PartsId", or null</summary>
    public static string? PartColorName(string partField)
    {
        return partField.EndsWith("_partsId", StringComparison.OrdinalIgnoreCase) ? partField[..^"_partsId".Length]
            : partField.EndsWith("PartsId", StringComparison.OrdinalIgnoreCase) ? partField[..^"PartsId".Length]
            : null;
    }

    /// <summary>
    /// Colours the materials of each part of <paramref name="mesh"/> (the parts are <paramref name="appearance"/>'s,
    /// in order) that take a character colour, from <paramref name="row"/>, with <paramref name="overrides"/> (a field
    /// being edited) in place of the stored values. False while a part isn't loaded yet, so it is to be called again.
    /// Called every frame: only materials whose values differ are written.
    /// </summary>
    public bool Apply(MeshRenderableProxy mesh, CharacterAppearance appearance, Param.Row row,
        IReadOnlyList<FacePart> parts, IReadOnlyDictionary<string, int>? overrides)
    {
        var done = true;
        var report = new List<string>();

        // Each name's colour and hair values, as first wanted (null: the row has no such colour)
        var looks = new Dictionary<string, Look?>(StringComparer.OrdinalIgnoreCase);

        Look? LookOf(string name)
        {
            if (!looks.TryGetValue(name, out var look))
                looks[name] = look = GetLook(row, name, overrides);

            return look;
        }

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            var name = appearance.Parts[i].Name;

            if (mesh.Submeshes[i]._meshProvider.ResourceHandle is not ResourceHandle<FlverResource> { IsLoaded: true } handle ||
                handle.Get()?.GPUMaterials is not { } materials)
            {
                done = false;
                report.Add($"{name}: not loaded");
                continue;
            }

            // The face field that chose this model, for the part's own colour (none for the body's parts)
            var partColorName = parts.FirstOrDefault(p => string.Equals(p.Model, name, StringComparison.OrdinalIgnoreCase)) is { } part
                ? PartColorName(part.Field)
                : null;

            var tinted = new List<string>();

            foreach (var material in materials)
            {
                if (material.Definition?.Slots is not { CharacterColor: { } wants } slots)
                    continue;

                var colorName = wants.Equals(PartColor, StringComparison.OrdinalIgnoreCase) ? partColorName : wants;

                if (colorName == null || LookOf(colorName) is not { } look)
                    continue;

                if (!Tinted.TryGetValue(material, out var own))
                    Tinted[material] = own = new Own(material.Shading.DiffuseAlpha, material.Customize);

                if (own.Customize is { } customize)
                {
                    // The colour, and for hair the root darkness and white density where the face has them
                    var wanted = customize;
                    wanted.Color = new Vector4(look.Color, look.RootBlack ?? customize.Color.W);
                    wanted.Params.X = look.WhiteDensity ?? customize.Params.X;

                    tinted.Add($"{material.FlverMaterialName} {colorName} {Describe(wanted)}");

                    if (material.Customize is { } current && current.Color == wanted.Color && current.Params == wanted.Params)
                        continue;

                    // The mask's texture and tiling are filled in by the upload
                    material.Customize = wanted;
                    material.UpdateMaterial();
                }
                else if (material.AlbedoFromColor && slots.Color is { Length: >= 3 } materialColor)
                {
                    // A colour-only material: its tint goes from the material's colour to the face's
                    var scale = new Vector3(
                        Ratio(look.Color.X, materialColor[0]), Ratio(look.Color.Y, materialColor[1]), Ratio(look.Color.Z, materialColor[2]));
                    var wanted = new Vector4(new Vector3(own.DiffuseAlpha.X, own.DiffuseAlpha.Y, own.DiffuseAlpha.Z) * scale, own.DiffuseAlpha.W);

                    tinted.Add($"{material.FlverMaterialName} {colorName} {look.Color.X:0.00} {look.Color.Y:0.00} {look.Color.Z:0.00} (diffuse)");

                    if (material.Shading.DiffuseAlpha == wanted)
                        continue;

                    material.Shading.DiffuseAlpha = wanted;
                    material.UpdateMaterial();
                }
            }

            if (tinted.Count > 0)
                report.Add($"{name}: {string.Join(", ", tinted)}");
        }

        // TEMPORARY (FaceGen colour check)
        LastReport = string.Join("; ", report);

        return done;
    }

    /// <summary>Puts back the values of everything that was coloured</summary>
    public void Restore()
    {
        foreach (var (material, own) in Tinted)
        {
            material.Shading.DiffuseAlpha = own.DiffuseAlpha;
            material.Customize = own.Customize;
            material.UpdateMaterial();
        }

        Tinted.Clear();
    }

    /// <summary>A face's colour, and the hair values that go with it (null where the row has no such field)</summary>
    private sealed record Look(Vector3 Color, float? RootBlack, float? WhiteDensity);

    private Look? GetLook(Param.Row row, string name, IReadOnlyDictionary<string, int>? overrides)
    {
        if (ColorField(row, name) is not { } red)
            return null;

        var green = red[..^1] + "G";
        var blue = red[..^1] + "B";

        var color = new Vector3(
            ToValue(red, Byte(row, red, overrides)),
            ToValue(green, Byte(row, green, overrides)),
            ToValue(blue, Byte(row, blue, overrides)));

        float? ValueOf(string? field) => field != null ? ToValue(field, Byte(row, field, overrides)) : null;

        return new Look(color,
            ValueOf(FindField(row, name, "rootBlack", "RootBlack")),
            ValueOf(FindField(row, name, "whiteDensity", "WhiteDensity")));
    }

    /// <summary>The field <c>{name}_{snake}</c> or <c>{name}{pascal}</c>, as the row names it, or null</summary>
    private static string? FindField(Param.Row row, string name, string snake, string pascal)
    {
        foreach (var candidate in new[] { $"{name}_{snake}", $"{name}{pascal}" })
        {
            foreach (var cell in row.Cells)
            {
                if (cell.Def.InternalName.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                    return cell.Def.InternalName;
            }
        }

        return null;
    }

    private static float Ratio(float wanted, float own) => own > 1e-4f ? wanted / own : wanted;

    private static string Describe(MaterialCustomize c) =>
        $"{c.Color.X:0.00} {c.Color.Y:0.00} {c.Color.Z:0.00}" +
        (c.Params.W == MaterialCustomize.ModeHair ? $" root {c.Color.W:0.00} white {c.Params.X:0.00}" : "");

    private static int Byte(Param.Row row, string field, IReadOnlyDictionary<string, int>? overrides)
    {
        if (overrides != null && overrides.TryGetValue(field, out var value))
            return value;

        return row.Cells.FirstOrDefault(c => c.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase)).Value
            is IConvertible stored ? stored.ToInt32(null) : FaceGenParams.Neutral;
    }

    /// <summary>A byte as the shader's value: across FaceRangeParam's range for the field, else 0 to 1</summary>
    private float ToValue(string field, int value)
    {
        var t = Math.Clamp(value, 0, 255) / 255f;

        if (FaceGenParams.GetParam(Project, RangeParamName) is { } ranges &&
            RangeValue(ranges, RangeMinRow, field) is { } min && RangeValue(ranges, RangeMaxRow, field) is { } max)
        {
            return min + (max - min) * t;
        }

        return t;
    }

    private static float? RangeValue(Param ranges, int rowId, string field)
    {
        var row = ranges.Rows.FirstOrDefault(r => r.ID == rowId);

        return row?.Cells.FirstOrDefault(c => c.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase)).Value
            is IConvertible value ? value.ToSingle(null) : null;
    }
}
