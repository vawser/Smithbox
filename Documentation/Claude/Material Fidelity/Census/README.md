# Material census (Phase 1)

Generated 2026-11-08 by `Tools/MaterialSurvey` over every map piece, asset, character and parts FLVER of ER, NR and
AC6. No read failures in any game.

## Rerunning

```bash
cd src/Smithbox.Data
../../Tools/MaterialSurvey/bin/Release/net10.0/MaterialSurvey.exe ER "../../Documentation/Claude/Material Fidelity/Census"
```

Arguments: `<ER|NR|AC6> <outDir> [binderPathRegex] [maxBinders]`. Takes about 4.5 min for ER, 1.5 for NR, 3.5 for AC6.
The tool compiles in the renderer's own `MaterialDefinition.cs` and reads the shader tables from
`Assets/Material Definitions`, so a table or resolver change shows up on the next run.

## Files

| File | What it holds |
|---|---|
| `{GAME}_Summary.md` | Totals, how many shaders cover the game, texture routes, roles, top shaders, top dropped textures, missing materials |
| `{GAME}_Shaders.tsv` | One row per shader: features, placed/total triangles, meshes, maps, categories, where each role goes, an example model |
| `{GAME}_Tokens.tsv` | The same per shader-name token (`[Mb2]`, `[Ov_N]`, …) |
| `{GAME}_Roles.tsv` | Per sampler role, where the viewport sends it |
| `{GAME}_Dropped.tsv` | Shader + role + slot the game samples that the viewport binds nowhere, ranked by placed triangles |
| `{GAME}_FlverOnlyTextures.tsv` | Textures a FLVER lists that its MATBIN has no sampler for |

**Placed triangles** = LOD0 triangles × the times the MSBs place the model. Map pieces, assets and enemies get
placements. Parts (equipment) only count in the unplaced triangle columns.

**Routes** copy the decision order of `FlverResource.ProcessMaterialTexture`. Phase 3 replaces that copy with
the renderer's own code. `Known` in the shader tables means only "has an entry in `{GAME}.json`". The recipe tables
(`_Blends`, `_Details`, …) are what the Features column shows.

## Binding audit (Phase 3, 2026-11-08)

`{GAME}_Audit.tsv` and the "Binding audit" section of each `_Summary.md` check every mesh texture. Each row puts the
viewport's route next to what the game's shader does with that slot, in the permutation the material picks
(`GXFT_SATStaticSwitch`).

- **Viewport side:** `MaterialBinding.Route` (`Renderer/Materials/MaterialBinding.cs`). This is now the only place
  routing is decided. `FlverResource.ProcessMaterialTexture` dispatches on it, and the census compiles the same file
  in, so the two can't drift.
- **Game side:** `Signatures/{GAME}_SlotMeanings.json`, made by `ShaderSignature run`. It holds each slot's meaning,
  worked out from where its channels go, and its UV set.

Statuses:

| Status | Meaning |
|---|---|
| Ok | The viewport draws it as what the game uses it for, or leaves out what the game doesn't sample |
| Dropped | The game samples it, the viewport binds it nowhere (the route's reason is in the Viewport column) |
| UvMismatch | Right meaning, other UV set |
| Misrouted | Bound as something else |
| Extra | Bound, but the material's permutation doesn't sample it |
| Approximate | Water and fur stand-ins |
| Unverified | `g_`-named samplers with no slot (SpeedTree), or slots the signature can't name |

Share of placed triangles × textures (after the per-layer UV fix and with slot occupancy, see below):

| | ER | NR | AC6 |
|---|---|---|---|
| Ok | 78.5% | 73.7% | 47.2% |
| Dropped | 15.1% | 20.7% | 50.8% |
| Unverified | 5.5% | 4.3% | 1.0% |
| Approximate | 0.6% | 0.7% | 0.0% |
| Misrouted | 0.3% | 0.6% | 0.2% |
| Extra | 0.0% | 0.1% | 0.5% |
| UvMismatch | 0.0% | 0.0% | 0.3% |

**Slot occupancy (2026-11-08).** The audit used to count a texture as bound whenever its route named a slot. But
`LookupTexture` keeps the first texture put in a slot, so later ones are never drawn. The census now follows the
viewport's order (the definition's textures first, then the model's in its order), and a texture whose slot is
already full is `Dropped (its … slot already holds another texture)`. This moved a lot from Ok/UvMismatch to
Dropped. These are the biggest:

| Shader | What isn't drawn | Placed share |
|---|---|---|
| `C[DetailBlend]` (ER) | Its extra albedo/normal layers (slots 4, 9, 10, 12). It has no known blend, so only the first of each is drawn | ~12% |
| `M[AMSN_V][Ov_AN]` | Its overlay albedo/normal (slots 5/2). The overlay recipe doesn't claim them, so they lose to the base textures | ~4% |
| AC6 `[MS][MB][M0/M1]` | Every layer above the first. These blends have no recipe, and every layer name-matches layer 0 | the bulk of AC6's 51% Dropped |

### Undrawn layers (2026-11-08)

- **`M[AMSN_V][Ov_AN]` overlay: fixed, not yet checked in the app.**
  - Why it was missing: its albedo overlay is a gamma-space blend, not the multiply form `_Details.json` describes, so
    the lost generator left it out:
    `x = A + s(g - 0.5)(O - A)`, then `sat(x + s(0.5 - g)(O - x))`, with `g = O^0.454` and `s = snp_0_float_5`.
  - The generator: `ShaderSignature overlays` finds this form, plus the `N + s(O·2 - 1)` normal overlay on top of an
    unscaled base normal, with material params only. It writes `Assets/Material Definitions/{GAME}_Overlays.json`:
    ER 40 (7 albedo), NR 32 (7), AC6 34 (1).
  - The app: `MaterialShaderTable.MergeOverlays` fills in what `_Details` lacks. `MaterialDetail.Pad0`/`Pad1` carry
    the mode and strength, and `FlverShader.frag` applies the overlay.
  - The result: ER's overlay normal (2.2%) and albedo (1.4%) are now drawn. ER Ok 78.5% → 80.1%, NR 73.7% → 74.5%.
    0.09% remain where the overlay's path is only in the FLVER, since `ResolveDetailRecipe` reads MATBIN paths only.
- **`C[DetailBlend]` detail stacks: partly done 2026-11-08, not yet checked in the app.**
  - What they are: permutations above 0 blend up to three detail albedos and normals before laying them over the
    base, with weights taken from the base normal map's blue and alpha. Perm 3, for example:
    `w1 = sat((T0.b·22)^260)`, `d = lerp(lerp(c1·T2, c0·T1, w1), c5·T10, w2)`, and
    `w2 = sat((0.5 - f19(T0.a - 0.5))·2.05 - 1.05)`.
  - The generator: `ShaderSignature detailstacks` writes `{GAME}_DetailStacks.json`. The format and the viewport path
    are in Signatures/README.md, Detail stacks.
  - The result: ER Ok 80.3% → 82.3% and Dropped 13.3% → 11.3%. NR Ok 76.8% → 79.9% and Dropped 17.5% → 14.5%.
    `C[DetailBlend]`'s Dropped share fell from 19.5 to 9.7 points (ER) and from 29.6 to 14.5 (NR). AC6 is unchanged.
  - Whole-albedo layers and back faces, added later the same day: ER Ok 82.3% → 83.2%, Dropped 11.3% → 10.4%.
    NR Ok 79.9% → 80.2%. `C[DetailBlend]` Dropped fell to 5.2 points (ER) and 12.9 (NR).
  - `[S2]` and `_SSS` (bare power weights, own-alpha layers, four layers): ER Ok 83.2% → 83.7%, Dropped 10.4% → 9.9%.
    NR Ok 80.2% → 80.9%. `C[DetailBlend]` Dropped is now 2.6 points (ER) and 9.6 (NR).
  - Still dropped: Mask1Map in `_FurBlur`/`_SSS` (no route), and `[S2]`'s slot 11 normal and upper metallic.
- **AC6 `[MS]` blends: not done.** These fit the existing recipe format. AC6 perm 13's layer weight is
  `sat((v - f23(1 - v)(mask + 0.01))·(f25 + 1))`, which is `MaterialBlendRecipeLayer`'s Soft form, with v = `TEXCOORD8.w`
  and the mask `T19.r` inverted by `f52`. Some layers are a flat colour instead of a texture. They're simply missing
  from `AC6_Blends.json`. Rebuilding a blend-recipe generator (the lost tool) would cover these, ER's "layer the blend
  doesn't use" cases, and possibly the detail stacks.

The largest issues by kind:

1. **Blend layers on the wrong UV set: fixed 2026-11-08.**
   - The problem: `ConfigureMaterialShader` read every layer of a known blend with UV0. But
     `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` (ER 3.7%, NR 6.2%, e.g. `aeg004_114`) samples all four layers with
     `TEXCOORD0.zw`, and only its masks with UV0.
   - The fix: `ShaderSignature uvsets` writes `Assets/Material Definitions/{GAME}_UvSets.json` (slot → UV set),
     `MaterialDefinition` sets `MaterialSampler.UvSet`, and each layer now uses its albedo's set
     (`FlverMaterial.LayerAlbedoTypes`).
   - The result: UV mismatch is now ~0 in ER/NR (ER 6.0% → 0.01%, NR 10.2% → 0.02%). AC6's mismatch turned out to be
     layers that aren't drawn at all (above). Not yet checked in the app.
2. **Layers a blend doesn't use (ER 4.7%, NR 3.4%).** `M[AMSN_V][Mb2][Ov_AN]` slots 5/6 (normal/albedo, ER 4%) and the
   4th layer of `[Mb4_Ch2]` (slots 7, 13) are dropped with "a layer the blend doesn't use". The recipe (or TexCoord1X)
   covers fewer layers than the shader samples.
3. **Masks with no route (Mask1Map/Mask3Map).** ER 1.4%, NR 4%, AC6 12%: masks that drive snow, crystal glow
   (`M[AMSN_V][Ov_N][Crystal]` Mask1Map → emissive) and AC6's `[MS]` layer weights.
4. **AC6's upper-layer metallic maps** (3.9%) are dropped, as its `[MS]` blends have no recipe.
5. **Misrouted is now small.**
   - The largest real-looking cases are overlay/normal slots that a blend uses as masks:
     `[Snow_VtxMask]` slot 11 and `[Cover]` slot 7.
   - `M[Cave_001]` slot 12 is an AlbedoMap that the shader reads as a normal.

How the classifier decides a slot's meaning:
- **Normal:** red and green reach the normal. Either the blue is gloss and nothing else, or red and green go only
  to the normal, or blue doesn't reach the normal and red and green don't reach gloss.
- **Mask:** a channel that reaches both the normal and the albedo.

Each rule came from a false positive found in this run: crystal normals that also colour the surface, a two-channel
mask, and AC6 normals that also weigh layers. The rest of `Misrouted` and `Unverified` may still contain
classifier errors.

## Coverage

| | ER | NR | AC6 |
|---|---|---|---|
| Shaders used | 643 | 560 | 199 |
| Shaders for 50% of placed triangles | 9 | 8 | 1 |
| … for 90% | 51 | 52 | 6 |
| … for 95% | 80 | 84 | 11 |
| … for 99% | 175 | 186 | 30 |
| Meshes | 116,882 | 29,792 | 85,395 |

The worklist is very top-heavy, AC6 most of all: `M[MS][MB][M1]` alone is 55% of its placed triangles.

## Findings

1. **MetallicMap is dropped in all three games.** ER: 25,020 textures on 23,847 meshes, in 100 shaders.
   NR: 10,063. AC6: 85,459, which is nearly every AC6 mesh. **Fixed 2026-11-08** (see GBuffer.md, Metallic path):
   in the rerun every ER mesh with a metallic map binds one (23,847), and 1,653 upper-layer maps are left out.
   AC6: 64,612 bound, 20,847 upper-layer maps left out. Not yet checked in the app.
   **Offsets and per-layer metallic, 2026-11-08** (GBuffer.md):
   - ER: 13,630 metallic maps get their shader's offset; 9,155 have none. In known blends, maps bound per layer are
     963 (layer 0), 860 (layer 1) and 250 (layer 2). 160 couldn't be paired with a layer, and 482 are other layers of
     unblended materials.
   - AC6: 690 get an offset, 121 are bound per layer, and 62 couldn't be paired. The `[MS]` blends have no recipe,
     so their 20,790 upper-layer maps are still left out. The top ER cases are `C[DetailBlend]` (4.1% of placed
   triangles, characters), `M[AMSN][Ov_N]` (2.4%) and `M[AMSN]` (1.7%).
2. **Some MetallicMaps are bound as albedo (a real misbinding).** The fallback name match runs `Contains("ALBEDO")` on the
   *whole* sampler name, which starts with the shader name. So in `M[AMSN][Mb2][Ov_N_AlbedoMask_MetallicMask]`
   (ER, 160 meshes, e.g. `aeg004_494`) and `C[Fur]_Albedo_FallOff` the metallic texture takes the albedo slot.
   480 ER and 120 NR textures. **Fixed 2026-11-08:** `ProcessMaterialTexture` now matches on `samplerName`
   (the part after `Texture2D_`), as the Emissive check already did. In the rerun, ER's 480 MetallicMaps and
   1 DisplacementMap went from Albedo to Dropped, and 1 NormalMap went from Albedo to Normal. NR's 120 went to
   Dropped. Nothing else moved. This hasn't been checked in the app yet.
3. **Mask1Map / Mask3Map are often dropped.** ER: 9,066 + 6,960. AC6: 32,675 + 26,432. They are bound only when a
   blend recipe claims them. The top ER case is `M[AMSN_V][Ov_N][Snow_VtxMask]` Mask1Map (1.0%). What these masks
   drive (snow, AO, colour adjust, AC6's `[M0][M1]` material IDs) is a Phase 2 question.
4. **AC6's base shaders have no table support at all.** The `[MS][MB][M0/M1]` family (≈80% of placed triangles)
   drops MetallicMap, Mask1Map and Mask3Map, and no AC6 shader has a `{GAME}.json` entry. AC6 is the best single
   target for Phase 2.
5. **SpeedTree `g_TranslucencyTexture` is dropped.** It is ~6% of ER placed triangles across the `SpeedTree_*`
   shaders (trees are placed many times).
6. **Texture2DArray samplers aren't parsed** (AC6 `C[Enemy]_ColorMaterial`, `P_ACParts__Marking`). Their role comes
   out as the whole name, because `MaterialDefinition.SlotName` only matches `Texture2D_N_`. The albedo/normal still
   bind through the name match; the marking array is dropped.
7. **Smaller dropped roles worth noting:** DistortionDepth (ER 493), VectorMap (fur flow, 359), FlowMap (260),
   DisplacementMap (139), and many one-off effect textures (fire, LUTs, clouds).
8. **Missing materials.**
   - ER: 79 meshes (face test materials and `aeg004_010_idNN`).
   - AC6: 706 meshes in 100 names, all `m[m10_00]…` map materials. These probably come from a per-map MATBIN binder
     the survey doesn't read yet. To check before Phase 3.
