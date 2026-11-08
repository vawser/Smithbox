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

| | ER | NR | AC6 |
|---|---|---|---|
| Stacks written | 84, in 18 shaders | 82, in 13 | 12, in 4 |

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
- **Weights of other forms.** "weight not understood": ER 149, NR 74, AC6 28 shader permutations. These are in other
  shaders the walk reaches, such as `C[DetailBlend][S2]` and `_SSS`, which still drop slots 8, 10, 11 and 12.
- **The single-detail form of other shaders.** "isn't one albedo and color": ER 46.
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
