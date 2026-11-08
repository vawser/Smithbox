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
