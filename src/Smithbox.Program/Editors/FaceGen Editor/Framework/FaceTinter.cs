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


    private readonly ProjectEntry Project;

    // The face's FaceGen texture (DS3), made on first use
    private FaceTextureBuilder? FaceTextureBuilder;

    // The materials that have been coloured, with their own values
    private sealed record Own(Vector4 DiffuseAlpha, MaterialCustomize? Customize, MaterialCustomizeExtra Extra, MaterialMakeup Makeup);
    private readonly Dictionary<FlverResource.FlverMaterial, Own> Tinted = new();

    private Own OwnOf(FlverResource.FlverMaterial material)
    {
        if (!Tinted.TryGetValue(material, out var own))
        {
            Tinted[material] = own = new Own(material.Shading.DiffuseAlpha, material.Customize, material.CustomizeExtra,
                material.CustomizeMakeup);
        }

        return own;
    }

    /// <summary>
    /// A skin's makeup from the face: each layer's amount from its field (face_cheek) and colour from the field's
    /// colour (face_cheekColor_R/G/B). Layers whose fields the row lacks keep the material's own values.
    /// </summary>
    private MaterialMakeup MakeupOf(FlverResource.FlverMaterial material, MaterialMakeup own, Param.Row row,
        IReadOnlyDictionary<string, int>? overrides)
    {
        if (material.Definition?.Slots?.Makeup is not { } recipe)
            return own;

        var makeup = own;

        for (var i = 0; i < (int)own.Count && i < MaterialMakeup.MaxLayers; i++)
        {
            if (recipe.Fields[i] is not { } name || FindField(row, name, "", "") is not { } amountField)
                continue;

            var layer = own[i];
            var color = GetLook(row, name, overrides)?.Color ?? new Vector3(layer.X, layer.Y, layer.Z);
            makeup[i] = new Vector4(color, ToValue(amountField, Byte(row, amountField, overrides)));
        }

        return makeup;
    }

    /// <summary>
    /// An eye material's iris colour, sclera colour, cataract and its colour, and iris scale, from the side's fields
    /// (eyeR_irisColor_R, eyeR_scleraColor_R, eyeR_cataract, eyeR_cataractColor_R, eyeR_irisScale; or eyeL_). Fields
    /// the row doesn't have keep the material's own values, and a row with no fields for the side leaves it as it is.
    /// </summary>
    private void TintEye(FlverResource.FlverMaterial material, Param.Row row, IReadOnlyDictionary<string, int>? overrides)
    {
        var name = $"{material.MaterialName} {material.FlverMaterialName}";
        var side = name.Contains("Left", StringComparison.OrdinalIgnoreCase) ? "eyeL"
            : name.Contains("Right", StringComparison.OrdinalIgnoreCase) ? "eyeR"
            : null;

        if (side == null || OwnOf(material) is not { Customize: { } own } original)
            return;

        Vector3? ColorOf(string snake, string pascal) =>
            FindField(row, side, snake + "_R", pascal + "_R") is { } red ? new Vector3(
                ToValue(red, Byte(row, red, overrides)),
                ToValue(red[..^1] + "G", Byte(row, red[..^1] + "G", overrides)),
                ToValue(red[..^1] + "B", Byte(row, red[..^1] + "B", overrides))) : null;

        float? ValueOf(string snake, string pascal) =>
            FindField(row, side, snake, pascal) is { } field ? ToValue(field, Byte(row, field, overrides)) : null;

        var iris = ColorOf("irisColor", "IrisColor");
        var sclera = ColorOf("scleraColor", "ScleraColor");
        var cataractColor = ColorOf("cataractColor", "CataractColor");
        var cataract = ValueOf("cataract", "Cataract");
        var irisScale = ValueOf("irisScale", "IrisScale");

        if (iris == null && sclera == null && cataract == null)
            return;

        // The sclera multiplies the whole eye texture (Color.rgb, float3_5); the iris colour is laid over the mask's x
        // (Params.rgb, float3_0)
        var wanted = own;
        var ownSclera = new Vector3(own.Color.X, own.Color.Y, own.Color.Z);
        var ownIris = new Vector3(own.Params.X, own.Params.Y, own.Params.Z);
        wanted.Color = new Vector4(sclera ?? ownSclera, cataract ?? own.Color.W);
        wanted.Params = new Vector4(iris ?? ownIris, own.Params.W);

        var extra = original.Extra;
        extra.A = new Vector4(cataractColor ?? new Vector3(extra.A.X, extra.A.Y, extra.A.Z), irisScale ?? extra.A.W);

        if (material.Customize is { } current && current.Color == wanted.Color && current.Params == wanted.Params &&
            material.CustomizeExtra.A == extra.A)
        {
            return;
        }

        material.Customize = wanted;
        material.CustomizeExtra = extra;
        material.UpdateMaterial();
    }

    public FaceTinter(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>The red field of a colour (green and blue by the same name), e.g. "hair" → hair_color_R, or null</summary>
    public static string? ColorField(Param.Row row, string name) => FindField(row, name, "color_R", "Color_R");

    /// <summary>
    /// The colour name of a part field: its name without "_partsId" / "PartsId" (or "Style": DS1's hairStyle is
    /// coloured by hairColor_*), or null
    /// </summary>
    public static string? PartColorName(string partField)
    {
        return partField.EndsWith("_partsId", StringComparison.OrdinalIgnoreCase) ? partField[..^"_partsId".Length]
            : partField.EndsWith("PartsId", StringComparison.OrdinalIgnoreCase) ? partField[..^"PartsId".Length]
            : partField.EndsWith("Style", StringComparison.OrdinalIgnoreCase) ? partField[..^"Style".Length]
            : null;
    }

    /// <summary>
    /// Colours the materials of each part of <paramref name="mesh"/> (the parts are <paramref name="appearance"/>'s,
    /// in order) that take a character colour, from <paramref name="row"/>, with <paramref name="overrides"/> (a field
    /// being edited) in place of the stored values. False while a part isn't loaded yet, so it is to be called again.
    /// Called every frame: only materials whose values differ are written.
    /// </summary>
    public bool Apply(MeshRenderableProxy mesh, CharacterAppearance appearance, Param.Row row,
        IReadOnlyList<FacePart> parts, IReadOnlyDictionary<string, int>? overrides, Param? param = null)
    {
        var done = true;

        // The face's FaceGen texture (DS3), built when a material first wants it this frame, and rebuilt only when the
        // texture coefficients or the base image change
        uint? faceTexture = null;

        uint FaceTexture()
        {
            if (faceTexture is { } built)
                return built;

            faceTexture = 0;

            if (param != null && FaceGenSliderTable.Get(Project) is { } table &&
                table.TextureBase is { } baseImage &&
                FaceGenModel.TextureCoefficients(Project, param, row, overrides) is { } coefficients)
            {
                FaceTextureBuilder ??= new FaceTextureBuilder(Project);

                if (FaceTextureBuilder.Update(coefficients, baseImage))
                    faceTexture = FaceTextureBuilder.Index;
            }

            return faceTexture.Value;
        }

        // A woman wearing the shared head (DS1), by her body (fc_f_*). Only the shared head: the other games' heads
        // name their body's skin (DS3 FC_M_0000_a), and whether they swap it for a woman isn't known
        var female = appearance.Parts.Any(p => p.Name.StartsWith("fc_f_", StringComparison.OrdinalIgnoreCase)) &&
                     appearance.Parts.Any(p => p.Name.Equals(FaceComposition.SharedHead, StringComparison.OrdinalIgnoreCase));

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
                continue;
            }

            // The face field that chose this model, for the part's own colour (none for the body's parts)
            var partColorName = parts.FirstOrDefault(p => string.Equals(p.Model, name, StringComparison.OrdinalIgnoreCase)) is { } part
                ? PartColorName(part.Field)
                : null;

            foreach (var material in materials)
            {
                if (material.Definition?.Slots is not { CharacterColor: { } wants } slots)
                    continue;

                // Eyes: by the side the material is (P[FG_A_1500]_eye_Right / _eye_Left), from eyeR_* / eyeL_*
                if (FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeEye)
                {
                    TintEye(material, row, overrides);
                    continue;
                }

                var colorName = wants.Equals(PartColor, StringComparison.OrdinalIgnoreCase) ? partColorName : wants;
                var look = colorName != null ? LookOf(colorName) : null;

                // A FaceGen face draws the face's texture even where the row has no colour for it (DS1's FaceGenParam
                // has no skin colour): its own colour stands
                if (look == null && FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeFaceGen &&
                    slots.BodyType != null && OwnOf(material).Customize is { } plain)
                {
                    look = new Look(new Vector3(plain.Color.X, plain.Color.Y, plain.Color.Z), null, null, null);
                }

                if (look == null)
                    continue;

                var own = OwnOf(material);

                if (own.Customize is { } customize)
                {
                    // The colour, and for hair the root darkness and white density where the face has them
                    var wanted = customize;

                    if (FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeBodyHair)
                    {
                        // Body hair: the colour (body_hairColor_*), and the amount from the field of the colour's own
                        // name (body_hair)
                        wanted.Color = new Vector4(look.Color, customize.Color.W);
                        wanted.Params.X = FindField(row, colorName!, "", "") is { } amountField
                            ? ToValue(amountField, Byte(row, amountField, overrides))
                            : customize.Params.X;
                    }
                    else if (FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeSkin)
                    {
                        // Skin: the colour, and the beard stubble (face_beard / faceBeard)
                        wanted.Color = new Vector4(look.Color, customize.Color.W);
                        wanted.Params.X = StubbleOf(row, overrides) ?? customize.Params.X;
                    }
                    else if (FlverResource.CustomizeMode(material.Definition) is MaterialCustomize.ModeFaceGen or
                             MaterialCustomize.ModeHairTint or MaterialCustomize.ModeEyeTint)
                    {
                        // DS3: the colour is the part model's FC_SkinMatchColor, the field's FaceRangeParam value as it
                        // stands (skin −1…1 added, eyes added on the iris, hair multiplied). Params.x, the eye's alpha
                        // mask, is kept
                        wanted.Color = new Vector4(look.Color, customize.Color.W);
                    }
                    else
                    {
                        wanted.Color = new Vector4(look.Color, look.RootBlack ?? customize.Color.W);
                        wanted.Params.X = look.WhiteDensity ?? customize.Params.X;
                    }

                    // A FaceGen skin with a body texture is a face: it draws the face's FaceGen texture
                    var isFace = FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeFaceGen &&
                                 slots.BodyType != null;
                    var faceTex = isFace ? FaceTexture() : material.CustomizeFaceTexture;

                    // On a woman, the face's male skin and normal (DS1's shared head names FG_m_face_org_d and
                    // FG_m_face_n) give way to the female ones beside them (FG_f_*), where those are loaded
                    if (isFace && female)
                    {
                        SwapToFemale(material, FlverResource.FlverMaterial.TextureType.CustomizeStubbleTextureResource);
                        SwapToFemale(material, FlverResource.FlverMaterial.TextureType.NormalTextureResource);
                    }

                    // Hair: the shininess (hair_shininess), where the material has a gloss to scale
                    var extra = own.Extra;
                    if (FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeHair &&
                        extra.B.X >= 0 && look.Shininess is { } shininess)
                    {
                        extra.B.X = shininess;
                    }

                    // Skin: the makeup layers, and the decal (its texture, placement and colour)
                    var isSkin = FlverResource.CustomizeMode(material.Definition) == MaterialCustomize.ModeSkin;
                    var makeup = isSkin ? MakeupOf(material, own.Makeup, row, overrides) : own.Makeup;

                    if (isSkin && slots.Decal != null)
                    {
                        material.SetCustomizeDecal(DecalOf(row, overrides, ref extra));
                    }

                    // The body's burn scar: burn_scar (burnScar) as its amount
                    if (isSkin && slots.Burn != null && FindField(row, "burn", "scar", "Scar") is { } burnField)
                    {
                        extra.B.Y = ToValue(burnField, Byte(row, burnField, overrides));
                    }

                    if (material.Customize is { } current && current.Color == wanted.Color && current.Params == wanted.Params &&
                        material.CustomizeExtra.A == extra.A && material.CustomizeExtra.B == extra.B &&
                        SameLayers(material.CustomizeMakeup, makeup) && material.CustomizeFaceTexture == faceTex)
                    {
                        continue;
                    }

                    // The masks' textures and tilings are filled in by the upload
                    material.CustomizeFaceTexture = faceTex;
                    material.Customize = wanted;
                    material.CustomizeExtra = extra;
                    material.CustomizeMakeup = makeup;
                    material.UpdateMaterial();
                }
                else if (material.AlbedoFromColor && slots.Color is { Length: >= 3 } materialColor)
                {
                    // A colour-only material: its tint goes from the material's colour to the face's
                    var scale = new Vector3(
                        Ratio(look.Color.X, materialColor[0]), Ratio(look.Color.Y, materialColor[1]), Ratio(look.Color.Z, materialColor[2]));
                    var wanted = new Vector4(new Vector3(own.DiffuseAlpha.X, own.DiffuseAlpha.Y, own.DiffuseAlpha.Z) * scale, own.DiffuseAlpha.W);

                    if (material.Shading.DiffuseAlpha == wanted)
                        continue;

                    material.Shading.DiffuseAlpha = wanted;
                    material.UpdateMaterial();
                }
                else if (slots.Customize == 0)
                {
                    // A plain textured material (DS1's Ps_Hair: the game's DL_FREG_139 multiplies the albedo): the
                    // face's colour multiplies the material's diffuse colour
                    var wanted = new Vector4(new Vector3(own.DiffuseAlpha.X, own.DiffuseAlpha.Y, own.DiffuseAlpha.Z) * look.Color,
                        own.DiffuseAlpha.W);

                    if (material.Shading.DiffuseAlpha == wanted)
                        continue;

                    material.Shading.DiffuseAlpha = wanted;
                    material.UpdateMaterial();
                }
            }
        }

        return done;
    }

    // The textures swapped for a woman's, with the material's own, to put back
    private readonly Dictionary<(FlverResource.FlverMaterial Material, FlverResource.FlverMaterial.TextureType Slot),
        ResourceHandle<TextureResource>?> Swapped = new();

    /// <summary>
    /// Puts the female counterpart of a slot's texture in its place: the name with _m_ for _f_ (FG_m_face_n →
    /// FG_f_face_n), if one is loaded. Nothing for a texture with no _m_ in its name, or none loaded yet.
    /// </summary>
    private void SwapToFemale(FlverResource.FlverMaterial material, FlverResource.FlverMaterial.TextureType slot)
    {
        var key = (material, slot);
        var current = material.GetTexture(slot);

        if (current == null)
            return;

        // The female texture this put there stays; anything else in the slot (the material's own, or its own come back
        // after loading late) is the one to name the female texture by
        if (Swapped.TryGetValue(key, out var kept) && !ReferenceEquals(current, kept) &&
            current.Name.Contains("_f_", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var index = current.Name.IndexOf("_m_", StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return;

        var femaleName = current.Name[..index] + "_f_" + current.Name[(index + 3)..];

        if (!ResourceManager.DefaultPool.TryFind<TextureResource>(femaleName, out var femaleTexture) ||
            femaleTexture is not { IsLoaded: true })
        {
            return;
        }

        // Held while swapped out, so it is still there to put back
        if (kept == null || !ReferenceEquals(kept, current))
        {
            current.Acquire();
            kept?.Release();
            Swapped[key] = current;
        }

        material.SetTexture(slot, femaleTexture);
    }

    /// <summary>Puts back the values of everything that was coloured</summary>
    public void Restore()
    {
        foreach (var ((material, slot), own) in Swapped)
        {
            material.SetTexture(slot, own);
            own?.Release();
        }

        Swapped.Clear();

        foreach (var (material, own) in Tinted)
        {
            material.Shading.DiffuseAlpha = own.DiffuseAlpha;
            material.Customize = own.Customize;

            // No material names a decal or a FaceGen texture: they are only ever the face's
            material.SetCustomizeDecal(null);
            material.CustomizeFaceTexture = 0;
            material.CustomizeExtra = own.Extra;
            material.CustomizeMakeup = own.Makeup;
            material.UpdateMaterial();
        }

        Tinted.Clear();
    }

    /// <summary>A face's colour, and the hair values that go with it (null where the row has no such field)</summary>
    private sealed record Look(Vector3 Color, float? RootBlack, float? WhiteDensity, float? Shininess);

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
            ValueOf(FindField(row, name, "whiteDensity", "WhiteDensity")),
            ValueOf(FindField(row, name, "shininess", "Shininess")));
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

    /// <summary>The face's beard stubble on the skin (face_beard, faceBeard), or null if the row has none</summary>
    private float? StubbleOf(Param.Row row, IReadOnlyDictionary<string, int>? overrides) =>
        FindField(row, "face", "beard", "Beard") is { } field ? ToValue(field, Byte(row, field, overrides)) : null;

    // The decal parts whose textures have been asked for
    private readonly HashSet<string> RequestedDecals = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The decal parts are fg_a_60NN, NN the value of decal_partsId; 0 is none</summary>
    private const int DecalPartBase = 6000;

    /// <summary>
    /// A skin's decal from the face: the part's texture (fg_a_60NN's FG_A_60NN_n, its alpha the shape, loaded on first
    /// use), and its placement and colour into <paramref name="extra"/> (A colour and scale, B offset, angle and mirror).
    /// Null texture when the face has no decal or it hasn't loaded yet.
    /// </summary>
    private ResourceHandle<TextureResource>? DecalOf(Param.Row row, IReadOnlyDictionary<string, int>? overrides,
        ref MaterialCustomizeExtra extra)
    {
        float? ValueOf(string snake, string pascal) =>
            FindField(row, "decal", snake, pascal) is { } field ? ToValue(field, Byte(row, field, overrides)) : null;

        if (FindField(row, "decal", "partsId", "PartsId") is not { } partsField)
            return null;

        var value = Byte(row, partsField, overrides);

        if (value <= 0)
            return null;

        var color = GetLook(row, "decal", overrides)?.Color ?? new Vector3(extra.A.X, extra.A.Y, extra.A.Z);
        extra.A = new Vector4(color, ValueOf("scale", "Scale") ?? extra.A.W);
        extra.B = new Vector4(
            ValueOf("posX", "PosX") ?? extra.B.X,
            ValueOf("posY", "PosY") ?? extra.B.Y,
            ValueOf("angle", "Angle") ?? extra.B.Z,
            ValueOf("mirror", "Mirror") is { } mirror ? (mirror > 0.5f ? -1 : 1) : extra.B.W);

        var part = $"fg_a_{DecalPartBase + value:D4}";
        var texture = $"{part.ToUpperInvariant()}_n";

        RequestDecal(part);

        return ResourceManager.DefaultPool.TryFind<TextureResource>(texture, out var handle) && handle is { IsLoaded: true }
            ? handle
            : null;
    }

    /// <summary>Loads a decal part's textures (its binder holds only a TPF), once</summary>
    private void RequestDecal(string part)
    {
        if (!RequestedDecals.Add(part) ||
            !Project.Locator.PartFiles.Entries.Any(e => e.Filename.Equals(part, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _ = LoadDecalAsync(ModelLocator.GetPartsModel(Project, part, part));
    }

    private async System.Threading.Tasks.Task LoadDecalAsync(ResourceDescriptor asset)
    {
        try
        {
            var options = new LoadOptions
            {
                JobName = LOC.Get("FACEGEN_Load_Job_Textures"),
                Types = ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            };

            await ResourceManager.DefaultPool.LoadAssets(Project, new HashSet<ResourceDescriptor> { asset }, options);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Load_Textures_Failed"), e);
        }
    }

    private static bool SameLayers(MaterialMakeup a, MaterialMakeup b)
    {
        for (var i = 0; i < MaterialMakeup.MaxLayers; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    private static float Ratio(float wanted, float own) => own > 1e-4f ? wanted / own : wanted;

    private static int Byte(Param.Row row, string field, IReadOnlyDictionary<string, int>? overrides)
    {
        if (overrides != null && overrides.TryGetValue(field, out var value))
            return value;

        return FaceGenParams.CellValue(row, field) is IConvertible stored ? stored.ToInt32(null) : FaceGenParams.Neutral;
    }

    /// <summary>A byte as the shader's value: across FaceRangeParam's range for the field, else 0 to 1</summary>
    private float ToValue(string field, int value)
    {
        var t = Math.Clamp(value, 0, 255) / 255f;

        if (FaceGenParams.Range(Project, field) is { } range)
            return range.Min + (range.Max - range.Min) * t;

        return t;
    }
}
