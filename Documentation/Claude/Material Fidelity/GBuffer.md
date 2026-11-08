# G-buffer layout (ER, NR, AC6)

Decoded 2026-11-08 from the material shaders' own G-buffer pixel shaders (`{shader}_Gbuf.ppo`) and the matching
forward shader (`{shader}_Fwd.ppo`), which does the lighting inline and so shows what each channel means.
`M[AMSN]` is the worked example. All three games write the same targets.

| Target | Component | Holds | Written from (`M[AMSN]`) |
|---|---|---|---|
| RT0 | xyz | World normal, `n * 0.5 + 0.5` | `NormalMap.rg * 2 - 1`, z rebuilt as `sqrt(1 - x² - y²)`, in the vertex tangent frame |
| RT0 | w | Instance or alpha data (same value written to every `.w`) | `FC_AlphaRef.z`, instance data |
| RT1 | x | **Metallic** | `saturate(MetallicMap.r + snp_0_float_0)` |
| RT1 | y | **Gloss** (smoothness, 1 = mirror) | `saturate(NormalMap.b)` |
| RT1 | z | Feature masks (snow, translucency) | 0 in `M[AMSN]`; `Mask1Map.r` in `[Snow_VtxMask]` and `_Translucency` shaders |
| RT1 | w | AO / vertex value | `TEXCOORD0.z` in ER `M[AMSN]`, `NormalMap.a` in AC6 `[MS]` |
| RT2 | xyz | **Albedo** | `saturate(AlbedoMap.rgb * snp_0_color_0 * FC_DifMapMulCol * aColor[slot])` |
| RT3 | x | Subsurface mask | `Mask1Map.r` in `C[DetailBlend]_SSS` |
| RT4 | xyz | **Emissive** | `EmissiveMap.rgb` × the material's emissive colour |

`AlbedoMap.a` goes to every target's `.w` through the alpha test, and also to `discard` in `_[A]` variants.

## How the forward shader uses them

From `M[AMSN]_Fwd.ppo` (constants are float hex in the disassembly):

- `F0 = mix(0.04, albedo, metallic)` (`(albedo - 0.04) * m + 0.04`).
- `diffuse = albedo * (1 - metallic)`.
- `g = sqrt(gloss)`. Environment BRDF approximation: `a = (1 - 0.999·N·V)^(5g) / (4 - 3g)`, and
  `envSpecular = (1 - a)(0.6g + 0.4)·F0 + a·g`. The Fresnel term grows with `g`, which is why `NormalMap.b` is gloss
  and not roughness.

## What Smithbox does today (FlverShader.frag)

| | Game | Smithbox (ER/NR/AC6 take the DS3 branches) |
|---|---|---|
| Normal | `NormalMap.rg` | Same |
| Roughness | `1 - NormalMap.b` | Same (`roughness = 1 - normalColor.z`) |
| F0 | `mix(0.04, albedo, metallic)` | `SampleLayer(ROW_SPECULAR)`, which is the **black default (0)** because these games have no specular map |
| Diffuse | `albedo * (1 - metallic)` | `albedo * (1 - F0)` = albedo |
| Metallic | `MetallicMap.r` + offset param | Never bound (Census finding 1) |

So every ER/NR/AC6 surface gets no base reflectance (only the grazing Fresnel term), and metals are drawn as
plain diffuse colour. Fixing this is a shader-path change for these games, not a per-shader one.

## Metallic path (built 2026-11-08, not yet checked in the app)

- `FlverResource.ChooseMetallic` picks one MetallicMap per material: the lowest-numbered (`_0` before `_1`, then
  slot) that has a file in the MATBIN or the FLVER. `ProcessMaterialTexture` binds it into the specular slot; any
  other MetallicMaps (upper blend layers) are left out.
- Specialisation constant 12 `c_metallic` is set for ER, AC6, NR, DB_NT and DB, unless the material binds an old
  `g_SpecularTexture` colour map. A material without a metallic map gets the black default, i.e. a non-metal.
- `FlverShader.frag`, after the wet-surface step: `F0 = mix(0.04, albedo, metallic.r)`, `albedo *= 1 - metallic`.
  Water keeps its own F0.

### Metallic offset and per-layer metallic (built 2026-11-08, not yet checked in the app)

- **Offsets.** `ShaderSignature metallic` finds every `sat(texture.r + cbuffer field)` that reaches RT1.x in every
  permutation. It writes `Assets/Material Definitions/{GAME}_Metallic.json`: shader → slot → `"param:component"`
  (`""` = saturated with nothing added). ER 256 shaders (143 with an offset), NR 187 (87), AC6 106 (35).
  `MaterialDefinition.ResolveMetallic` turns these into `MetallicOffsets` (sampler type → value) from the material's
  params. Some blends saturate the lerp of their layers rather than each layer (`M[AMSN][Mb2][Ov_AN]`); those have
  no per-layer offset and get none.
- **Per-layer metallic.** `ResolveMetallic` gives each blend layer the metallic map in its albedo's UV group (the
  metaparam groups in `_TextureGroups.json`), else the one with the same number. The shaders lerp a layer's albedo
  and metallic by the same weight. Checked by hand on `M[AMSN][Mb2][Ov_AN]`:
  albedo `lerp(T2, T3, w)` and metallic `lerp(T8, T7, w)`, so the pairs are T2↔T8 and T3↔T7, as the UV groups say.
  Over the recipes, the UV group gives a unique pair for 25 of 28 ER layers, 43 of 47 NR and 19 of 24 AC6.
- **In the viewport.** For a known blend (recipe or TexCoord1X) each layer's metallic goes in that layer's
  specular slot, and `c_blendSpecular` is set when a layer above the first has one. In the metallic path, row 3 of
  `Material` (the shininess textures, unused by these games) holds the four layer offsets as half floats
  (`FlverMaterial.MetallicOffsetBits`). `FlverShader.frag`'s `MetallicLayer(i)` is
  `sat(spec_i.r + offset_i)`, and the metallic is the sum of those weighted by `g_weight` as the albedo is.

Not done yet:
- AC6's `[MS][MB]` layers. They have no blend recipe, so only the bottom layer is drawn, metallic included.
- The lighting still uses Smithbox's `diffuse * (1 - F0)`, so there's a slight extra darkening (≤4% for non-metals)
  beyond the game's `albedo * (1 - metallic)`.
