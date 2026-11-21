using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
///     Per material table read by the flver shader. Every value is the index of a texture in the global texture pool.
///     The shader reads this as 12 32-bit words, two values to a word, so the order and size here
///     must match FlverShader.frag exactly: 24 values in six rows of four, 48 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Material
{
    /// <summary>The most texture layers a material can blend</summary>
    public const int MaxLayers = 4;

    /// <summary>
    ///     Set in <see cref="uvSet" />, above the UV set itself: the material belongs to a character, which the map's
    ///     GPARAM lights with its model set, see GparamLighting
    /// </summary>
    public const ushort ModelLightingFlag = 0x100;

    /// <summary>
    ///     Set in <see cref="uvSet" />: a Demon's Souls material lit as the game's HemEnv shaders light it (its MTD's
    ///     g_LightingType 3, every map material): the hemisphere and the LightBank row's env cubemaps, and no sun.
    ///     The env specular map it reads is in <see cref="EnvSpecularSlotMask" />, the share of the light its
    ///     shadows take in <see cref="EnvShadowMask" />.
    /// </summary>
    public const ushort EnvLightingFlag = 0x200;

    /// <summary>
    ///     Set in <see cref="uvSet2" />, above the second layer's UV set: a material that blends by a blend mask without a
    ///     recipe has the blend entries (<see cref="MaterialWithBlend" />) for the mask's tiling alone, which the shader
    ///     reads from <see cref="MaterialLayerTiling.MaskTiling" />. DS2 tiles its masks in the FLVER (FlverMaterial.FlverTiling)
    /// </summary>
    public const ushort MaskTilingFlag = 0x100;

    /// <summary>
    ///     Set in <see cref="uvSet2" /> with <see cref="MaskTilingFlag" />: a GXFlver mask blend (MaterialResolvedMaskBlend),
    ///     weighed by the vertex alpha pushed by the mask, the mask on the third UV slot. FC_BlendMaskParam's x and z are
    ///     the blend entries' Sharpness.x and Detail.x (FlverMaterial.MaskBlendUvIndex)
    /// </summary>
    public const ushort MaskBlendFlag = 0x200;

    /// <summary>
    ///     Set in <see cref="uvSet2" />: a double face material (MaterialDefinition.DoubleFace), whose front faces are the
    ///     first layer and back faces the second
    /// </summary>
    public const ushort DoubleFaceFlag = 0x400;

    /// <summary>
    ///     Set in <see cref="uvSet3" />, above the third layer's UV set: an opacity blend (FlverMaterial.OpacityFadeUvIndex),
    ///     whose alpha is multiplied by the red of the blend mask slot (second UV slot, MaskTiling.xy), of the detail mask
    ///     in the blend entries (third UV slot, MaskTiling.zw) and by the vertex alpha
    /// </summary>
    public const ushort OpacityFlag = 0x100;

    /// <summary>The MTD's g_EnvSpcSlotNo (0-3), which of the four env specular maps the material reads</summary>
    public const ushort EnvSpecularSlotMask = 0x0C00;
    public const int EnvSpecularSlotShift = 10;

    /// <summary>The MTD's g_ShadowPowMul (0-1) in fifteenths</summary>
    public const ushort EnvShadowMask = 0xF000;
    public const int EnvShadowShift = 12;

    // Row 0: albedo, layers 0-3
    public ushort colorTex;
    public ushort colorTex2;
    public ushort colorTex3;
    public ushort colorTex4;

    // Row 1: normal, layers 0-3
    public ushort normalTex;
    public ushort normalTex2;
    public ushort normalTex3;
    public ushort normalTex4;

    // Row 2: specular, layers 0-3
    public ushort specTex;
    public ushort specTex2;
    public ushort specTex3;
    public ushort specTex4;

    // Row 3: shininess, layers 0-3
    public ushort shininessTex;
    public ushort shininessTex2;
    public ushort shininessTex3;
    public ushort shininessTex4;

    // Row 4: other textures
    public ushort blendMaskTex;
    public ushort emissiveTex;
    public ushort lightmapTex;
    public ushort lightmapTex2;

    // Row 5: the UV set (0 = first, 1 = second) that each layer's textures use, in the low 8 bits.
    // The first also holds the material's flags above them, see ModelLightingFlag.
    public ushort uvSet;
    public ushort uvSet2;
    public ushort uvSet3;
    public ushort uvSet4;
}

/// <summary>
///     The values of a material, as opposed to its textures. Read by the flver fragment shader as the
///     entry straight after the material's <see cref="Material" />, so it has the same 48 byte size.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialShading
{
    /// <summary>The alpha test used when there is no material to ask</summary>
    public const float DefaultAlphaRef = 0.5f;

    /// <summary>xyz multiplies the albedo, w is the alpha below which a pixel is discarded</summary>
    public Vector4 DiffuseAlpha;

    /// <summary>xy tiles the albedo of the first layer, zw its normal</summary>
    public Vector4 Tiling;

    /// <summary>xy tiles the albedo of the second layer, zw its normal</summary>
    public Vector4 Tiling2;

    public static MaterialShading Default => new()
    {
        DiffuseAlpha = new Vector4(1, 1, 1, DefaultAlphaRef),
        Tiling = Vector4.One,
        Tiling2 = Vector4.One
    };
}

/// <summary>
///     What a material does over time and adds to its lighting. Read by the flver fragment shader
///     as the entry straight after the material's <see cref="MaterialShading" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialEffects
{
    /// <summary>rgb multiplies the emissive texture, w is unused</summary>
    public Vector4 EmissiveColor;

    /// <summary>xy tiles the emissive texture, zw is how far its UVs move each second</summary>
    public Vector4 EmissiveTiling;

    /// <summary>xy is how far the albedo's UVs of the first layer move each second, zw its normal's</summary>
    public Vector4 Scroll;

    public static MaterialEffects Default => new()
    {
        EmissiveColor = new Vector4(1, 1, 1, 0),
        EmissiveTiling = new Vector4(1, 1, 0, 0)
    };
}

/// <summary>
///     The tiling that doesn't fit in <see cref="MaterialShading" />, read by the flver fragment
///     shader as the entry after the deform entry. Only materials with a blend recipe have this and the next three.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayerTiling
{
    /// <summary>xy tiles the albedo of layer 2, zw its normal</summary>
    public Vector4 Tiling3;

    /// <summary>xy tiles the albedo of layer 3, zw its normal</summary>
    public Vector4 Tiling4;

    /// <summary>xy tiles the blend mask, zw the detail mask</summary>
    public Vector4 MaskTiling;

    public static MaterialLayerTiling Default => new()
    {
        Tiling3 = Vector4.One,
        Tiling4 = Vector4.One,
        MaskTiling = Vector4.One
    };
}

/// <summary>How the layers are weighed, see <see cref="MaterialBlendRecipe" /></summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialBlend
{
    /// <summary>xyz is the sharpness of layers 1 to 3, w the bias of the detail mask</summary>
    public Vector4 Sharpness;

    /// <summary>xyz is how much of the detail mask layers 1 to 3 use, w the scale of the weights</summary>
    public Vector4 Detail;

    /// <summary>Where the value of layers 1 to 3 comes from</summary>
    public uint Source1, Source2, Source3;

    /// <summary>The texture index of the detail mask</summary>
    public uint DetailTex;
}

/// <summary>The rgb of each is the tint of layers 1 to 3, and the three w are the tint of layer 0</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayerTint
{
    public Vector4 Tint2, Tint3, Tint4;
}

/// <summary>How far the UVs of layers 1 to 3 move each second: xy for the albedo, zw for the normal</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayerScroll
{
    public Vector4 Scroll2, Scroll3, Scroll4;
}

/// <summary>
///     How a material moves its vertices. Read by the flver vertex shader as the entry straight after the
///     material's <see cref="MaterialEffects" />, which is the only reason it has the same 48 byte size.
///     Only materials with a deformer allocate it, so static materials don't pay for it.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MaterialDeform
{
    public float Amplitude;

    /// <summary>Frames a second for a vertex animation, cycles a second for sway, phase spread for wind</summary>
    public float Rate;

    /// <summary>Further values of the deformers that need them, see FlverShader.vert</summary>
    public float Extra0, Extra1;

    /// <summary>The direction of sway, unused by vertex animation</summary>
    public Vector4 Direction;

    /// <summary>Global texture index of the positions and normals of a vertex animation. 0 until they load</summary>
    public uint VatPositionTex;
    public uint VatNormalTex;

    public float Extra2, Extra3;
}

/// <summary>
///     The character-creator colouring of a ChrCustomize material (ER's hair-type and skin shaders), read by the flver
///     fragment shader as the entry straight after <see cref="MaterialEffects" />, in the deform entry's place: these
///     shaders never deform. Its meaning depends on <see cref="MaterialSlotRecipe.Customize" />:
///     <list type="bullet">
///     <item>Hair: the albedo is <c>sat(1 + root · vertex colour − Color.w) · Color.rgb</c>, lerped to a grey-white by
///     <c>sat(pow(Params.x · 2.5 · mask.x, 5))</c>. root is the mask's y (Params.y 1) or z (2), or 1 if Params.z is 0</item>
///     <item>Skin: the albedo texture is re-tinted to Color.rgb (divided by the texture's own skin tone) where the
///     mask's z is 0, then darkened by the beard stubble: <c>mix(1, stubble.rgb, sat(2 · Params.x) · mask.y)</c>, the
///     stubble texture (<see cref="SecondTex" />) tiled by Params.yz</item>
///     </list>
///     A face's own colours are written here by the FaceGen Editor's FaceTinter.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialCustomize
{
    /// <summary>rgb: the colour (hair color_4, skin color_3); w: the hair's root darkness (float_14)</summary>
    public Vector4 Color;

    /// <summary>
    ///     Hair: x the white density (float_0), y the root mask channel (1 y, 2 z), z 1 if the root is masked.
    ///     Skin: x the stubble amount (float_14), yz the stubble texture's tiling. w: the mode
    /// </summary>
    public Vector4 Params;

    /// <summary>The tiling of the mask (Mask3Map)</summary>
    public Vector2 MaskTiling;

    /// <summary>The mask's texture index, 0 until it loads (then read as white)</summary>
    public uint MaskTex;

    /// <summary>Skin: the stubble texture's index, 0 until it loads (then no stubble)</summary>
    public uint SecondTex;

    public const uint ModeHair = 1;
    public const uint ModeSkin = 2;

    /// <summary>
    ///     Eye (ER's [Eye]): Color.rgb the sclera colour (multiplies the texture), Color.w the cataract amount, Params.rgb
    ///     the iris colour (where the mask's x is), Params.w the pupil's value (where the mask's y is), and in
    ///     <see cref="MaterialCustomizeExtra" /> the cataract colour (A.rgb, over the pupil) and the iris scale (A.w,
    ///     scaling every UV about the centre)
    /// </summary>
    public const uint ModeEye = 3;

    /// <summary>Shade (ER's [EyeAO]): the albedo becomes mix(Color.rgb, 1, albedo), a multiplier no darker than Color</summary>
    public const uint ModeShade = 4;

    /// <summary>
    ///     Body hair (ER's [BodyHair_Base], [BodyHair]): the albedo is Color.rgb, and the cut-out the normal map's alpha
    ///     times a density from the mask's x and Params.x (the body_hair amount): m·(1 + 4f) below 0, m + f above
    /// </summary>
    public const uint ModeBodyHair = 5;

    /// <summary>
    ///     Eye patch (ER's [EyePatch]): <c>sat(detail · 4.55 · mix(albedo, albedo · 3 · Color.rgb, mask.x))</c>, the mask's
    ///     x the cloth or leather that takes the colour, the detail (<see cref="SecondTex" />, tiled by Params.yz) the
    ///     material's cloth or leather albedo. Without the detail it is 1
    /// </summary>
    public const uint ModeEyePatch = 6;

    /// <summary>
    ///     FaceGen skin (DS3's GXFlver_FgFace* shaders): <c>mix(f(face), f(body), vertex colour alpha)</c> with
    ///     <c>f(x) = sat(sqrt(x) + Color.rgb)²</c> (FC_SkinMatchColor, the skin colour). The face is the FaceGen texture
    ///     built for the face (<see cref="MaskTex" />, set at run time; UV 0), the body the skin texture
    ///     (<see cref="SecondTex" />, g_DiffuseTexture2; UV 1, tiled by Params.yz). Without a face texture the material's
    ///     own albedo stands in. The same shaders' plain skins have neither, which leaves <c>f(albedo)</c>, and DS3's eyes
    ///     (GXFlver_EyeIbl) add the colour only where the albedo's alpha is (Params.x 1): <c>sat(sqrt(x) + c · a)²</c>
    /// </summary>
    public const uint ModeFaceGen = 7;

    /// <summary>
    ///     Hair tint (DS3's SAT_HairIbl2): <c>sat(sqrt(albedo) · Color.rgb)²</c>, the hair colour multiplying the texture in
    ///     square-root space (FC_SkinMatchColor on a hair part)
    /// </summary>
    public const uint ModeHairTint = 8;

    /// <summary>
    ///     Eye tint (DS1's FRPG_Phn_FaceEye, gFC_FaceEyeCol): <c>mix(albedo, albedo · Color.rgb, albedo.a)</c>, the eye
    ///     colour multiplying the texture where its alpha (the iris) is
    /// </summary>
    public const uint ModeEyeTint = 9;
}

/// <summary>
///     What doesn't fit in <see cref="MaterialCustomize" />, the entry after it (in the blend tiling entry's place)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialCustomizeExtra
{
    /// <summary>Eye: rgb the cataract colour, w the iris scale. Skin: rgb the decal's colour, w its scale</summary>
    public Vector4 A;

    /// <summary>
    ///     Hair: x the shininess, the gloss the mask allows (float_23, FaceParam hair_shininess); below 0 for none.
    ///     Eye: y the iris parallax depth, zw its height map's tiling. Skin: xy the decal's offset, z its angle, w -1 if
    ///     mirrored (else 1)
    /// </summary>
    public Vector4 B;

    /// <summary>
    ///     Skin: xy the decal mask's tiling, zw the decal's. A skin with a burn scar instead (the body, which has no
    ///     decal): B.y its amount, C.xy its albedo's tiling, C.zw its normal's
    /// </summary>
    public Vector4 C;

    public static MaterialCustomizeExtra Default => new() { A = new Vector4(1, 1, 1, 1), B = new Vector4(-1, 0, 0, 0) };
}

/// <summary>
///     The makeup of a skin colouring (ER's [Skin] face permutation), entries 5 to 7 after the material (in the blend
///     entries' places). Up to six layers, applied in order over the re-tinted skin, each weighed by a channel of one of
///     two masks (Texture2D_13, Texture2D_15) times its amount: multiplied (<see cref="KindMultiply" />, around the eyes
///     and the cheeks), mixed (<see cref="KindMix" />, eye shadows and lips) or mixed by the eighth power
///     (<see cref="KindLine" />, the eyeliner).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialMakeup
{
    public const int MaxLayers = 6;

    public const uint KindMultiply = 0;
    public const uint KindMix = 1;
    public const uint KindLine = 2;

    /// <summary>Each layer: rgb its colour, w its amount</summary>
    public Vector4 Layer0, Layer1, Layer2, Layer3, Layer4, Layer5;

    /// <summary>The two masks' texture indices, 0 until they load (then no makeup)</summary>
    public uint MaskTex0, MaskTex1;

    /// <summary>Per layer, 5 bits from bit 5·i: bit 0 the mask (0 or 1), bits 1-2 the channel, bits 3-4 the kind</summary>
    public uint Layout;

    /// <summary>The number of layers</summary>
    public uint Count;

    /// <summary>xy the first mask's tiling, zw the second's</summary>
    public Vector4 MaskTiling;

    /// <summary>
    ///     The decal (a fg_a_60NN part's texture, set at run time: alpha its shape) and its mask (Mask1Map's x), 0 for
    ///     none. Placed on the second UV set by <see cref="MaterialCustomizeExtra" /> A, B and C
    /// </summary>
    public uint DecalTex, DecalMaskTex;

    /// <summary>
    ///     The body's burn scar (ER's [Skin] body permutation): its albedo and the normal map whose alpha is its shape,
    ///     both on the second UV set, 0 for none. Its amount is <see cref="MaterialCustomizeExtra" /> B.y, the tilings C
    /// </summary>
    public uint BurnTex, BurnNormalTex;

    public Vector4 this[int i]
    {
        get => i switch { 0 => Layer0, 1 => Layer1, 2 => Layer2, 3 => Layer3, 4 => Layer4, _ => Layer5 };
        set
        {
            switch (i)
            {
                case 0: Layer0 = value; break;
                case 1: Layer1 = value; break;
                case 2: Layer2 = value; break;
                case 3: Layer3 = value; break;
                case 4: Layer4 = value; break;
                default: Layer5 = value; break;
            }
        }
    }

    public static MaterialMakeup Default => new() { MaskTiling = Vector4.One };
}

/// <summary>
///     What is uploaded for a material with a <see cref="MaterialCustomize" /> entry
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithCustomize
{
    public Material Material;
    public MaterialShading Shading;
    public MaterialEffects Effects;
    public MaterialCustomize Customize;
    public MaterialCustomizeExtra Extra;
    public MaterialMakeup Makeup;
}

/// <summary>
///     What is uploaded for a material
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithShading
{
    public Material Material;
    public MaterialShading Shading;
    public MaterialEffects Effects;
}

/// <summary>
///     What is uploaded for a material with a deformer
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithDeform
{
    public Material Material;
    public MaterialShading Shading;
    public MaterialEffects Effects;
    public MaterialDeform Deform;
}

/// <summary>
///     The overlay of a material. Read by the flver fragment shader as the entry after <see cref="MaterialLayerScroll" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialDetail
{
    /// <summary>xy tiles the albedo, zw the normal</summary>
    public Vector4 Tiling;

    /// <summary>rgb multiplies the albedo, w is the strength of the normal</summary>
    public Vector4 ColorStrength;

    /// <summary>
    ///     The texture indices of the albedo and normal, 0 if there is none. Pad0 is how the albedo is laid over the
    ///     material's (0 multiplied, <see cref="AlbedoOverlay" />), Pad1 the strength of an overlay as float bits
    /// </summary>
    public uint AlbedoTex, NormalTex, Pad0, Pad1;

    /// <summary>The [Ov_AN] overlay, see <see cref="MaterialDetailRecipe.AlbedoBlend" /></summary>
    public const uint AlbedoOverlay = 1;

    /// <summary>
    ///     The low byte of Pad0 is the albedo mode, bits 8-10 the number of stack layers, and <see cref="HasStackEntries" />
    ///     says the stack entries are there (a material with only a back face has them and no layers)
    /// </summary>
    public const uint AlbedoModeMask = 0xFF;
    public const int StackCountShift = 8;
    public const uint HasStackEntries = 1u << 11;
}

/// <summary>
///     One layer of a detail stack (C[DetailBlend], <see cref="MaterialDetailRecipe.Stack" />), read by the flver
///     fragment shader as entries 15 to 18, after <see cref="MaterialLayerDetailExtra" />. The layer is lerped over
///     the details below it by sat(pow(Weight.x * map[Weight.w] + Weight.y, Weight.z)).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialDetailStackEntry
{
    /// <summary>rgb multiplies the layer's albedo, w is unused</summary>
    public Vector4 Color;

    /// <summary>
    ///     x the scale, y the bias, z the power, w the channel (0-3) of the weight map, plus <see cref="Inverted" /> and
    ///     <see cref="Replacing" />
    /// </summary>
    public Vector4 Weight;

    /// <summary>Added to Weight.w: the layer takes one minus its weight</summary>
    public const float Inverted = 4;

    /// <summary>Added to Weight.w: the layer is lerped over the finished albedo, see MaterialDetailStackLayer.Replace</summary>
    public const float Replacing = 8;

    /// <summary>Added to Weight.w: the layer is weighed by its own albedo's alpha, see MaterialDetailStackLayer.OwnAlpha</summary>
    public const float OwnAlpha = 16;

    /// <summary>Tiles the layer's albedo and normal, which share a UV group</summary>
    public Vector2 Tiling;

    /// <summary>The albedo's texture index in the low 16 bits, the normal's in the high, 0 for none</summary>
    public uint Textures;

    /// <summary>The weight map's texture index (the same in every layer), sampled with the base normal's tiling</summary>
    public uint WeightTex;

    public static MaterialDetailStackEntry Default => new() { Color = Vector4.One, Weight = new Vector4(0, 0, 1, 0), Tiling = Vector2.One };
}

/// <summary>
///     Everything of a material with a detail stack, which has every entry before it. <see cref="Back" /> (entry 19)
///     is what back faces are drawn with, its weight unused, 0 textures for none
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithDetailStack
{
    public MaterialWithLayerDetail Layered;
    public MaterialDetailStackEntry Layer1, Layer2, Layer3, Layer4;
    public MaterialDetailStackEntry Back;
}

/// <summary>
///     The glow of a [magma] material, read by the flver fragment shader as the two entries after
///     <see cref="MaterialDetail" />. See <see cref="MaterialMagmaRecipe" /> for what these do.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialMagma
{
    /// <summary>xy moves the first sample's UVs each second, zw scales them</summary>
    public Vector4 Flow0;

    /// <summary>xy moves the second sample's UVs each second, zw scales them</summary>
    public Vector4 Flow1;

    /// <summary>xy tiles the texture, z is the strength, w the power</summary>
    public Vector4 Tiling;

    /// <summary>xy bends the third sample's UVs, zw is unused</summary>
    public Vector4 Distort;

    /// <summary>rgb is the color, w the limit</summary>
    public Vector4 Color;

    /// <summary>The texture index, and which sample has the red</summary>
    public uint Tex, RedSample, Pad0, Pad1;
}

/// <summary>
///     The ripples and volume of water, read by the flver fragment shader as the two entries
///     after <see cref="MaterialMagma" />. See <see cref="MaterialResolvedWater" /> for what these do.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWater
{
    /// <summary>xy tiles the second normal, zw is how far its UVs move each second</summary>
    public Vector4 DetailTiling;

    /// <summary>xy is how far the first normal's blue bends the second's UVs, z its strength, w the gloss</summary>
    public Vector4 DetailBend;

    /// <summary>rgb multiplies the light the water scatters, w is 1 if it has a volume to color</summary>
    public Vector4 Tint;

    /// <summary>rgb is the scattering, w what depth is multiplied by to fade in at the shore</summary>
    public Vector4 Scatter;

    /// <summary>rgb is the absorption, w is unused</summary>
    public Vector4 Absorption;

    /// <summary>Unused, to fill out the second entry</summary>
    public Vector4 Pad;
}

/// <summary>
///     The snow of a material (<see cref="MaterialResolvedSnow" />), in the two entries of <see cref="MaterialWater" />,
///     whose size it has.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialSnow
{
    /// <summary>rgb is the snow's color, linear, w its roughness</summary>
    public Vector4 Color;

    /// <summary>x is how much of a metal it is, y its F0, z the vertex alpha the cover starts at, w what multiplies the alpha above it</summary>
    public Vector4 Surface;

    /// <summary>xy tiles the finer normal, z is its strength</summary>
    public Vector4 Detail;

    /// <summary>The second entry</summary>
    public MaterialLayeredHeight Height;
}

/// <summary>
///     A height made of one texture's RGB average at three tilings, weighed and summed, as DS1R's snow and water
///     height passes draw it (FRPG_Snow_HeightMap, FRPG_Water_HeightMap). One entry.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayeredHeight
{
    /// <summary>The three tilings</summary>
    public Vector3 Tiling;

    /// <summary>How far the first sample's UVs move each second, as two halves (BitConverter.HalfToUInt16Bits)</summary>
    public uint Scroll0;

    /// <summary>The three weights</summary>
    public Vector3 Blend;

    /// <summary>The second sample's movement, as two halves</summary>
    public uint Scroll1;

    /// <summary>The third sample's movement, as two halves</summary>
    public uint Scroll2;

    /// <summary>Whatever the surface wants to put here</summary>
    public float Extra0, Extra1, Extra2;

    public static uint PackScroll(float u, float v) =>
        BitConverter.HalfToUInt16Bits((Half)u) | ((uint)BitConverter.HalfToUInt16Bits((Half)v) << 16);

    public static MaterialLayeredHeight Of(float[] tiling, float[] blend, float[] scroll) => new()
    {
        Tiling = new Vector3(tiling[0], tiling[1], tiling[2]),
        Blend = new Vector3(blend[0], blend[1], blend[2]),
        Scroll0 = PackScroll(scroll[0], scroll[1]),
        Scroll1 = PackScroll(scroll[2], scroll[3]),
        Scroll2 = PackScroll(scroll[4], scroll[5])
    };
}

/// <summary>
///     DS1R's FRPG_Water_Reflect and FRPG_Water_Env (<see cref="MaterialResolvedReflectWater" />), in the two entries of
///     <see cref="MaterialWater" />, whose size it has.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialReflectWater
{
    /// <summary>rgb is g_WaterColor, linear, w its alpha: how much of it the scene behind takes on</summary>
    public Vector4 Color;

    /// <summary>x is g_FresnelPow, y g_FresnelBias, z g_FresnelScale, w g_SpecularPower</summary>
    public Vector4 Fresnel;

    /// <summary>rgb is g_FresnelColor, linear, w g_WaterWaveHeight</summary>
    public Vector4 FresnelColor;

    /// <summary>The second entry: the waves. Extra0 is g_WaterFadeBegin</summary>
    public MaterialLayeredHeight Height;
}

/// <summary>
///     Everything of a material with a blend recipe. The deform entry is there
///     even if it doesn't deform, so that the entries always have the same offsets.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithBlend
{
    public Material Material;
    public MaterialShading Shading;
    public MaterialEffects Effects;
    public MaterialDeform Deform;
    public MaterialLayerTiling LayerTiling;
    public MaterialBlend Blend;
    public MaterialLayerTint LayerTint;
    public MaterialLayerScroll LayerScroll;
}

/// <summary>
///     What multiplies each channel of the detail mask of layers 1 to 3, for the extended blend form (AC6's [MS]
///     blends, see <see cref="MaterialBlendRecipeLayer.DetailChannels" />). Read by the flver fragment shader as the
///     entry after <see cref="MaterialWater" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayerDetail
{
    public Vector4 Channels2, Channels3, Channels4;
}

/// <summary>For layers 1 to 3 of the extended form: x the detail bias, y what multiplies a soft weight, zw the detail mask's tiling</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialLayerDetailExtra
{
    public Vector4 Layer2, Layer3, Layer4;

    public static MaterialLayerDetailExtra Default => new()
    {
        Layer2 = new Vector4(0, 1, 1, 1),
        Layer3 = new Vector4(0, 1, 1, 1),
        Layer4 = new Vector4(0, 1, 1, 1)
    };
}

/// <summary>Everything of a material with the extended blend form, which has the entries of water in front of it</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithLayerDetail
{
    public MaterialWithWater Watery;
    public MaterialLayerDetail Detail;
    public MaterialLayerDetailExtra Extra;
}

/// <summary>Everything of a material of water, which has the entries of a glow in front of it</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithWater
{
    public MaterialWithMagma Glowing;
    public MaterialWater Water;
}

/// <summary>Everything of a material with a glow, which has the entries of an overlay in front of it</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithMagma
{
    public MaterialWithDetail Detailed;
    public MaterialMagma Magma;
}

/// <summary>Everything of a material with an overlay, which has the entries of a blend recipe in front of it</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithDetail
{
    public MaterialWithBlend Blended;
    public MaterialDetail Detail;
}
