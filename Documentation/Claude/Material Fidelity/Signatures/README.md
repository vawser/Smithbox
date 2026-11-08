# Shader signatures (Phase 2)

Generated 2026-11-08 by `Tools/ShaderSignature`. For each material shader of ER, NR and AC6, the tool records which
texture channels and parameters reach which G-buffer component. It gets this by disassembling the game's own pixel
shaders and walking the DXIL data flow back from every `storeOutput`. What each target means is in
[../GBuffer.md](../GBuffer.md).

## Rerunning

```bash
cd src/Smithbox.Data
../../Tools/ShaderSignature/bin/Release/net10.0/ShaderSignature.exe ER run <cacheDir> "../../Documentation/Claude/Material Fidelity/Signatures" "../../Documentation/Claude/Material Fidelity/Census"
```

`dxc` comes from the Windows SDK (`10.0.26100.0`). Disassembly is cached in `<cacheDir>`, so a rerun takes seconds.
The full per-shader JSON (`{GAME}_Signatures.json`, 4–11 MB) is written next to the TSVs; move it into the cache
dir rather than the docs.

Other modes:
- `list <bundleRegex>`, `extract <outDir> <bundleRegex>`: look inside `.shaderbdle` bundles.
- `one <cacheDir> <shader>`: print one shader's signature.
- `perms <cacheDir> <shader>`: each numbered permutation's slots.
- `params <shader>`: the shader's materials grouped by parameters.
- `checkrecipes <signatures.json>`: the checked-in recipe tables against the signatures.
- `bnd <vfsPath> <outDir>`: extract a plain binder. ER's `gx*.shaderbnd` aren't reachable through the VFS by name.

## Files

| File | What it holds |
|---|---|
| `{GAME}_ChannelMap.tsv` | Role + channel → set of outputs, totalled over shaders, weighted by the census' placed triangles |
| `{GAME}_ShaderTextures.tsv` | Per shader and texture slot: role, channel → outputs, and what the coordinates depend on |
| `{GAME}_Permutations.tsv` | Each numbered permutation's material texture slots and roles |
| `{GAME}_SignatureFailures.txt` | NR only: 190 `_cloth` bundles, which hold only vertex shaders |

## How a shader is read

- **Variant.** `_Gbuf.ppo` first, then `_Gbuf_[A]`, `_Fwd`, `_Fwd_[A]`. ER: 1063 shaders, all but 1 from a G-buffer
  variant. NR: 1120, 14 forward only. AC6: 455.
- **Permutations.** Many shaders come as numbered permutations (`{shader}_{N}_Gbuf.ppo`). ER has 103 such shaders
  with 554 permutations, NR 142 with 581, AC6 181 with 1366; `M[MS][MB][M1]` alone has 165. Each permutation turns on
  a set of texture slots, e.g. AC6 perm 1 = `[0 2 16]`, perm 2 adds slot 3, perm 3 adds 22. The signature is the
  union over all permutations, with each one's slots kept in `_Permutations.tsv`.
- **Which permutation a material uses** is the MATBIN parameter `GXFT_SATStaticSwitch`. The recipe tables are already
  keyed `shader@N` by it, and `MaterialDefinition` looks them up the same way.
- **Leaves.** A G-buffer output's leaves are texture channels (`<resource>.r`), cbuffer fields (`cb:FC_DifMapMulCol`,
  `cb:M_AMSN_snp_0_float_0_0`) and interpolants (`in:TEXCOORD0.z`). A texture whose channels feed another sample's
  coordinates is marked `uv` (flow, distortion, parallax).

## Blend recipe generator (2026-11-08)

`ShaderSignature <GAME> blends <cacheDir> <outJson> [shaderRegex]` (`Recipes.cs`) rebuilds the lost tool behind
`{GAME}_Blends.json`:

1. **Find the layers.** The albedo (RT2.x) is a chain of lerps, `(top - bottom) * w + bottom`, with the same SSA value
   as the bottom both times. Overlay lerps, whose weight is built from the overlay itself, are skipped.
2. **Read each weight.** It's expanded into a polynomial over its leaves and solved against the viewport's two forms:
   hard `1 - S·s·(2v - (D·m + bias))` and soft `v - s(1 - v)(D·m + bias + 0.01)`. A trailing `× (1 + …)` factor is
   left out, as the checked-in recipes do.
3. **Pair the normals.** A layer's normal is the top of the lerp in RT0.x that has the same weight.
4. **Name the sources.** Vertex values are traced through the vertex shader to the FLVER attribute they come from.
   For example, pixel input `TEXCOORD3.x` is vertex input `TEXCOORD1.x`, the name the recipes and the viewport use.

| | ER | NR | AC6 |
|---|---|---|---|
| Checked-in recipes reproduced field for field | 60 / 97 | 51 / 116 | 13 / 34 |
| New recipes, written to `Assets/Material Definitions/{GAME}_GeneratedBlends.json` | 36 | 46 | 46 |

The app and the census merge the generated recipes in where `_Blends.json` has none (`MaterialShaderTable.MergeBlends`).
A recipe is only written if the viewport draws all its sources.

Where it doesn't match the checked-in recipes:
- **It finds more.** Layer normals the checked-in recipes lack (6 ER, 9 NR, 6 AC6). In NR, `[Mb3]`/`[Mb4]` layers read
  `TEXCOORD1.y`/`.z` where the checked-in recipes say `.x` for every layer. Ours is the likelier reading: one vertex
  channel per layer.
- **Weights it can't read:**
  - `ColorAdjust` shaders lerp, by a param, between a hard weight and `sat(mask + vertex)` behind a bool. The old
    generator kept the hard part.
  - Snow weights use the surface's direction.
  - Some layers take two albedos.
- **AC6 `[MS][MB]`, the largest gap.** Built 2026-11-08, not yet checked in the app:
  - **Colour-only layers.** A recipe layer with `Albedo: -1` and a `Color` is drawn with a white texture tinted by
    the param (`MaterialBlendLayer.ColorOnly`, `FlverMaterial.ColorOnlyLayers`). The generator writes these when a
    lerp's top is params only.
  - **Any vertex `TEXCOORDk`.** A recipe whose vertex sources all read one `TEXCOORDk` gets that FLVER UV member packed
    where `TEXCOORD1` goes (`MaterialDefinition.BlendVertexAttribute`, `FlverResource.GetTexCoordIndex`), and
    `TEXCOORDk.x/y/z` read it as `TEXCOORD1.x/y/z` do. This already lets 2 ER, 3 NR and 6 AC6 recipes through that
    read `TEXCOORD2`/`3`. Only UByte4Norm members are moved.

  **Extended blend form, built 2026-11-08, not yet checked in the app.** `[MS][MB][M0/M1]` now produces 122
  recipes (AC6 185 generated in all, 181 extended layers, 18 colour-only). AC6 Ok rose from 47.2% to 57.3% of placed
  texture-triangles, and Dropped fell from 50.8% to 39.7%.
  - **Generator** (`Recipes.ReadExtended`, used when the classic form doesn't fit): it splits the weight as `A + B·v`
    and divides out the sharpness. The detail comes out as `Σ_c C_c·mask.c + bias`, with each C and the bias a
    polynomial in material params. A trailing params-only factor becomes the soft scale. The chain walk also passes
    through a detail overlay multiplied between layers (`color_7·4.55·T22.r × …`).
  - **Recipe** (`MaterialBlendRecipeLayer`): `DetailMask` (the layer's own slot), `DetailChannels` (4 sums of
    products), `DetailBias`, `SoftScale`. `MaterialDefinition.EvaluateSum` evaluates them per material.
  - **Viewport.**
    - Two more material entries after water: `MaterialLayerDetail` (+13, the channel weights per layer) and
      `MaterialLayerDetailExtra` (+14, bias, soft scale and tiling per layer), in `MaterialWithLayerDetail`.
    - Source flags: `SOURCE_EXTENDED` (32) and `SOURCE_DETAIL_SECOND_UV` (64). The layer's detail mask texture index is
      in the source's top 16 bits.
    - A second vertex attribute: TEXCOORD5 in `[MS]`, packed in the TEXCOORD2 slot and read as channel 7
      (`fsin_vertexBlend.w`).
    - Extended hard weights take their scale in the sharpness.

  What `[MS][MB]` needed, for reference. Its weights use three things the old blend data had no room for:
  1. **A detail mask per layer picked by a dot product.** Layers 11 and 13 use `dot(T26.rgba, snp_0_float3_N)`, so a
     param chooses which channel of one mask texture each layer uses.
  2. **A detail bias set by a param, per layer.** The soft layer's detail is `lerp(1 - T19.r, T19.r, snp_0_float_52)`,
     so its bias is `1 - f52`. The recipe has one numeric `DetailBias` for every layer.
  3. **A scale on the soft weight,** `× (snp_0_float_25 + 1)`, which the format leaves out.

  Supporting these means a wider per-layer blend entry: a channel selector, a bias and a soft scale for each of the 3
  upper layers. The material buffer would then carry one more 48-byte entry for blended materials.

## ColorAdjust blends (2026-11-08)

`ShaderSignature <GAME> coloradjust <cacheDir> <outJson> [shaderRegex]` (`ColorAdjust.cs`) writes
`Assets/Material Definitions/{GAME}_ColorAdjust.json`. Built 2026-11-08, not yet checked in the app.

**What the checked-in recipes miss.** In `M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]` and its relatives, the large
ER/NR terrain blends, each layer is weighted like this:

```
v      = sat(Mask.c + TEXCOORD1.c)  · COLOR1.w where bool_0 is set
hard   = sat(1 - 1.4·f6·(2(1 - v) - (1 - T3.r)))      the checked-in recipe's weight
weight = lerp(v, hard, f3)                              f3, f0, f5 per layer
```

The checked-in recipes have only `hard`, and use the mask without the vertex value added. Most materials set the mix
params to 0.1–0.2, so the game's weights are mostly the soft `v`. Each layer's albedo, the bottom's too, is
`bool_k ? lerp(T, color_k, f_k) : T·color_k`: with its bool set, it moves toward the colour rather than being tinted
by it.

**Recipe.** `MaterialColorAdjustRecipe`: albedo slot → `MaterialBlendRecipeLayer` extras (`AddSource`, `ScaleSource`,
`ScaleIf`, `HardMix`, `ColorAdjust`, `ColorAdjustIf`). `MaterialShaderTable.MergeColorAdjust` adds them to the recipe of
the same key by albedo slot. The bottom's adjust goes to `AlbedoColorAdjust`.

| | ER | NR | AC6 |
|---|---|---|---|
| Shaders found | 26 (22 with a weight mix) | 33 (30) | 4 (0) |
| Merged into a blend recipe | 10 | 17 | 0 |

**Recipes for the rest (2026-11-08, later).** `M[Cave_001]`, `M[Cliff_001]`, `M[Road_Ground]` and others had no blend
recipe at all, because the blend generator couldn't read `lerp(v, hard, K)`. It now can:
- **The mixed weight** (`Recipes.MixedWeight`): it reads `hard`, with `v` standing for its mask channel. The mix and
  the vertex value come from `_ColorAdjust.json`.
- **A weight scaled as a whole by a param**, `f16·sat(…)`: `MaterialBlendRecipeLayer.WeightScale`.
- **Layers of different scale or detail bias.** The recipe has one of each, so a hard layer's scale ratio folds into
  its `Sharpness`, and a differing bias becomes `DetailBiasOverride`.
- **Viewport:** `SourceScaled` (8192) and `SourceBias` (32768). The weight scale and bias go in `extra.y`/`.z` of
  entry +14.

Checked-in recipes reproduced are unchanged, and no generated recipe was lost.

| | ER | NR | AC6 |
|---|---|---|---|
| Generated blend recipes | 36 → 55 | 46 → 67 | 185 → 186 |
| Census Ok | 83.7% → 84.3% | 80.9% → 81.3% | unchanged |

ER's cave/cliff/road shaders went from about 2.9 points Dropped to 0.3. Still without a recipe: `M[Cave_Ore_001]`
and others whose weights have other forms. AC6's `M[Planet][GroundSurface]` shaders got one recipe (`_003`).

**Viewport.**
- Source bits: `SourceAdd` (128, its channel from bit 8), `SourceScaleAlpha` (2048), `SourceMixed` (4096), and
  `SourceAdjusted` (16384) on the first source.
- The mix factors and colour adjusts use entries +13/+14, the extended form's. They're only used when no layer is
  extended.
- `ComputeRecipeWeights` adds the vertex value, applies the alpha and lerps from the raw value to the hard weight.
- `SampleLayered` lerps toward the tint where the adjust is ≥ 0.
- The binding audit can't see this: it checks textures, not weights.

## Snow, fur and SSS masks (2026-11-08)

Built 2026-11-08, not yet checked in the app.

**Snow (`[Snow_VtxMask]`).** The snow layer's value is how much the surface faces a direction:
`v = sat(f0 + dot(N, float3_0)) · TEXCOORD1.z`, where N is the world normal of the base normal map. Its weight is the
usual hard form, with the Mask1Map as its detail mask.
- **Generator** (`Recipes.FacingValue`): it finds the `sat(bias + dot(N, float3 param))` (times a vertex value) inside
  a weight. That stands in as the source `FACING`, so the hard form reads as before.
  - Recipe fields: `FacingDirection`, `FacingBias`, `FacingVertex`.
  - New generated recipes: ER and NR, 5 shaders each, such as `M[AMSN_V][Ov_N][Snow_VtxMask]`.
- **Viewport:** `SourceFacing` (bit 16, with the vertex channel from bit 17). Those bits are the extended form's
  otherwise, so `FlverResource` only writes the extended detail masks there for extended recipes.
  - The direction is entry +13's second layer `.yzw`, and each layer's bias is `.w` of entry +14.
  - `ComputeRecipeWeights` turns layer 0's normal map into world space and takes the dot.
- **Not done:** shaders with a checked-in recipe that lacks the snow layer (`M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]`).
  The checked-in recipe wins, so their snow still isn't drawn.

**Fur (`C[Fur]_FurBlur`, `C[Shell]_FurBlur`).** These multiply the fur colour by `4.55·lerp(0.2196, T.r, Mask1Map.r)`:
a further layer, shown where the mask is set and neutral elsewhere. It isn't the cut strands.
- **Generator:** `ShaderSignature furmasks` writes `{GAME}_FurMasks.json` (ER 12 shaders, NR 5).
- **App:** `MaterialFurRecipe.Masked`/`Mask`. The masked texture goes in the third albedo slot and the mask in the blend
  mask slot, and `FlverShader.frag`'s fur path multiplies them in.
- **Limits:** only materials that resolve as fur get it. Most placed `C[Shell]_FurBlur` materials give no strand
  texture, so they aren't drawn as fur, and their mask stays dropped. ER fur Dropped went from 4.48 to 4.33 points.

**Not drawn:**
- **`C[DetailBlend]_SSS` Mask1Map:** it feeds only the subsurface amount (RT3.x), and the viewport has no subsurface
  term.
- **`M[AMSN_V]_Grass2` Mask1Map:** it's sampled with world-projected UVs (position and the inverse view-projection), a
  ground colour under the grass.
- **`C[DetailBlend]_FurBlur`:** no fur recipe, and its masked texture (slot 14) is almost never given.

Census after these: ER Ok 84.4% → 85.1% (Dropped 9.2% → 8.4%), NR 83.0% → 83.1%.

## Emissive masks (2026-11-08)

`ShaderSignature <GAME> emissivemasks <cacheDir> <outJson> [shaderRegex]` (`EmissiveMasks.cs`) writes
`{GAME}_EmissiveMasks.json`. Built 2026-11-08, not yet checked in the app.

- **What it finds:** shaders whose glow is a texture's red times params, where the texture isn't named as an
  emissive. `M[Crystal]` (NR): `RT4.rgb = Mask1Map.r · (1 - TEXCOORD1.y) · color_0.rgb · f3`.
- **How it reads them:** every term of RT4.x/y/z must have the same texture red and the same colour param. The float
  params all terms share become the factor.
- **Left out:** other factors are dropped, such as the vertex fade and, in `[Ov_N][Crystal]`, a blend-style
  `sat(1 - 1.4·f5·(…))`. Those glows can come out brighter than the game's.
- **Viewport:**
  - `MaterialDefinition.EmissiveMaskType`: `MaterialBinding.Route` sends that texture to the emissive slot. Its colour
    is the colour param times the factor.
  - `EmissiveColor.w = -1` has `FlverShader.frag` take the red for all three channels. Fur keeps its dither width
    in that w, which is never below 0.
- **Coverage:** ER 6 shaders, NR 19, AC6 0. NR Ok rose from 81.3% to 83.0% (Dropped 13.1% → 11.4%), ER from 84.3% to 84.4%.

**AC6's dropped masks are something else.** `[MS][MB][M0/M1]` slots 19, 26 and 29 (~43 of AC6's ~60 Dropped points)
already route as layer detail masks, in permutations that have a blend recipe. They drop where a permutation has none.
The generator rejects those for three reasons:
- a weight that reads two vertex values (`min(max(f48, -1), 0)·TEXCOORD8.x + TEXCOORD8.w`);
- facing weights that read the world normal;
- "a layer isn't one albedo".

## Detail stacks (2026-11-08)

`ShaderSignature <GAME> detailstacks <cacheDir> <outJson> [shaderRegex]` (`DetailStacks.cs`) writes
`Assets/Material Definitions/{GAME}_DetailStacks.json`. Run it with `STACK_DEBUG=1` to print each stack, or `=2` for
what uses a stack that was rejected. Built 2026-11-08, not yet checked in the app.

**What they are.** `C[DetailBlend]` permutations above 0 lerp up to four detail albedos together. The result then
multiplies the base albedo by 4.55, as a single detail does:

```
d      = lerp(lerp(lerp(c1·T2, c0·T1, w1), c5·T10, w2), c2·T4, w3)
albedo = sat(base · d · 4.55)
```

The normals follow the same weights: `lerp(lerp(T5, T6, w1), T12, w2) …`, and then `(n·2 - 1)·f10` is added to the base
normal. Each weight reads one channel of the base normal map (T0):

- `w1 = sat((T0.b·22)^260)`: a step at a blue of 1/22. Blue is gloss elsewhere; here, near-zero gloss marks T2.
- `w2 = sat((0.5 - f19(T0.a - 0.5))·2.05 - 1.05)`: a ramp up as alpha falls.
- `w3`: the same ramp with `+f20`, rising as alpha grows.

Each permutation wires in different slots. The UV groups pair them: (1,6), (2,5), (10,12) and (4,9).

**Recipe.** `MaterialDetailRecipe.Stack`: the bottom detail is the recipe's own `Albedo`/`Color`/`Normal`. Each layer has
`Albedo`, `Color` and `Normal`, plus `Channel`, `Scale`, `Bias` and `Power`, meaning `w = sat(pow(Scale·x + Bias, Power))`.
`StackWeightMap` is the slot of the map. The generator reads both weight forms as that, with `exp2(log2(X)·P)` as the
pow. `NormalStrength` is copied from the checked-in `_Details.json` entry (key, `@0`, or plain name). The app and the
census replace the `_Details` entry of a key that has a stack (`MaterialShaderTable.MergeDetailStacks`).

**Whole-albedo layers and back faces (2026-11-08, later).** The other permutations lerp some layers over the
*finished* albedo, so those layers replace the colour rather than detail it. Permutation 8, for example, is
`sat(lerp(lerp(base·c0·T1·4.55, c5·T10, w2), c2·T4, w3))`. A layer can also sit on the bottom of its lerp, as in
permutation 2, `lerp(c1·T2, base·d·4.55, w1)`, which is that layer weighted by `1 - w1`. Their normals still lerp
in the detail chain, with the same weights. Permutations 15 and up use `front ? that : c3·T8` for the albedo and
`T13` for the normal offset, at a strength of 1.
- **The walk** (`BuildStack`):
  1. The clamp under RT2.x.
  2. A `SV_IsFrontFace` select.
  3. Whole-albedo lerps, one albedo term against the rest.
  4. What the 4.55 multiplies: a detail stack or one detail.
- **Recipe:** `Replace` and `Invert` on a layer; `BackAlbedo`, `BackColor` and `BackNormal` on the recipe.
- **Viewport:**
  - `weight.w` adds 4 (inverted) and 8 (replacing).
  - Entry +18 is the back face. `MaterialDetail.HasStackEntries`, bit 10 of `Pad0`, says entries 15–18 exist.
  - Whole-albedo layers are mixed in after the multiply. The game clamps once, at the end, and so does the viewport.
    One with no texture is left out rather than painting a grey stand-in.
  - Back faces (`gl_FrontFacing`) skip the base and details.
- All 21 `C[DetailBlend]` permutations now have a recipe or the plain detail (0).

**`[S2]` and `_SSS` (2026-11-08, later still).**
- **Bare power weights.** `_SSS` weighs its alpha layers by `(1 - a)^8` and `[S2]` by `(0.5 - f21(a - 0.5))^8`, with
  no saturate around them. The weight reader now takes a bare `exp2(log2(X)·P)`.
- **Own-alpha layers.** `[S2]` lays slot 8 over the finished albedo by that texture's own alpha, like a decal:
  `lerp(X, c7·T8, T8.a)`. This is the layer flag `OwnAlpha` (weight.w +16). Where a whole-albedo lerp follows it, the
  compiler folds the two together as `((X - b) + (L - X)·a)·w + b`, which `Folded` matches.
  - Its normal lerps the detail offset toward a param (`color_3`) by the same alpha. That isn't drawn.
- **Up to four layers.** `[S2]` permutations 9–12 have four. Entries +15 to +18 are layers, +19 the back face; the
  count is bits 8–10 of `Pad0`, and `HasStackEntries` moved to bit 11.

| | ER | NR | AC6 |
|---|---|---|---|
| Stacks written | 232, in 37 shaders | 144, in 21 | 40, in 8 |

**Viewport.**
- The detail entry's `Pad0` holds the layer count from bit 8.
- Entries +15 to +17 are `MaterialDetailStackEntry`, one per layer: colour, weight (scale, bias, power, channel), tiling,
  albedo/normal texture indices and the weight map. They come after the extended-blend entries, so a stacked material
  allocates all 18 (`MaterialWithDetailStack`).
- `FlverShader.frag` mixes the layers' albedos (tinted) and normals by `DetailStackWeights` before the existing
  multiply and normal offset.
- The weight map is the base normal, loaded a second time and sampled with layer 0's normal tiling.
- A missing layer texture counts as neutral (`1/4.55`, flat normal).

**Not done:**
- **Weights of other forms.** "weight not understood": ER 21, NR 12 shader permutations.
- **The single-detail form of other shaders.** "isn't one albedo and color": ER 44.
- **`[S2]` slot 11.** A normal added to the detail offset on its own (ER 0.5 points), still dropped.
- **Layer metallic.** The layers also lerp metallic toward params (`f16`, `f8` and `f5` in `C[DetailBlend]`), which
  isn't drawn.
- **Mask1Map.** It has no route in `C[DetailBlend]_FurBlur` and `_SSS` (ER ~1.6 points).

## Parser fixes

Found while building the blend generator. All tables were regenerated after both:
- **Signature order.** Input/output element IDs follow the second (DXIL) signature table, not the first (register
  order). Some `in:` names had been wrong. NR's UV table changed (618 → 610 shaders).
- **Long field names.** A cbuffer field whose name runs into the comment (`name;; Offset:`) had been skipped, and its
  values were credited to the field before it (`FC_RimLightingParam`). Recovering them grew the metallic offsets
  (ER 143 → 230 shaders, NR 87 → 161, AC6 35 → 56) and the overlays (ER 40 → 82).

## Findings

1. **The G-buffer layout is the same in all three games.**
   - RT0 = normal, RT1.x = metallic, RT1.y = gloss, RT2 = albedo, RT3.x = SSS, RT4 = emissive.
   - The common route covers 85–99% of placed triangles:
     - `AlbedoMap.rgb` → RT2
     - `NormalMap.rg` → RT0
     - `NormalMap.b` → RT1.y
     - `MetallicMap.r` → RT1.x
2. **Smithbox's F0 is 0 for these games** (see GBuffer.md). The game uses `mix(0.04, albedo, metallic)`, and diffuse
   is `albedo·(1 - metallic)`. This affects every surface, not only the 100 shaders with a MetallicMap.
3. **Masks are layer weights.** In the channel map, Mask1Map and Mask3Map channels feed every G-buffer output
   together, which is what a blend weight does. In AC6 that's ~60–80% of placed triangles, and in ER ~28% (Mask1)
   and ~17% (Mask3). AC6's `[MS][MB][M0/M1]` is a 4-layer blend: albedo, normal and metallic per layer, layers 2+ on
   the second UV set (`TEXCOORD0.zw`), one normal scrolling by `FC_GlobalTime`. AC6 has no shader table entry for it,
   so Smithbox draws only its first layer. That's AC6's single largest gap.
4. **The checked-in recipe tables agree with the shaders.** Once `@N` keys are resolved to their permutation, every
   slot a recipe names is sampled by that shader in 1258 / 1309 (ER), 1319 / 1405 (NR) and 918 / 996 (AC6) entries.
   The rest:
   - **Water (3–6 per game):** expected. The water recipes come from the forward shader, and the signature reads `_Gbuf`.
   - **`TextureInputs` (47–75):** mostly character shaders whose inputs were read from another permutation, e.g.
     `C[c2200]_Body`.
   - **Fur `_cloth` (NR 15):** these bundles have no pixel shader.

   The lost generators are therefore not needed to *trust* the tables. They would still be needed to *extend* them,
   for example to AC6's `[MS]` blends. Rebuilding the blend formulas (constants, curve shapes) is beyond data-flow
   tracing, and is the Phase 5 oracle's job.
5. **Signatures say which inputs reach which outputs, not the maths.** "Gloss, not roughness" and
   "`F0 = mix(0.04, …)`" came from reading the forward shader by hand. Phase 3 should treat each signature as "these
   inputs must be bound, to these meanings". It doesn't give the curves.
