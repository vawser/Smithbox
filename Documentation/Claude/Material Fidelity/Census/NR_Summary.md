# NR material census

Generated 2026-11-08 by `Tools/MaterialSurvey`. Placed triangles = LOD0 triangles x MSB placements;
characters and parts count only where an MSB places them (enemies), so the unplaced columns cover the rest.

| | Count |
|---|---|
| MATBINs in the game | 20063 |
| MATBINs used by a mesh | 7389 |
| Shaders used | 560 (17 with a shader table entry) |
| Meshes | 29792 |
| Triangles (LOD0, each model once) | 136,940,146 |
| Placed triangles | 1,894,299,959 |
| Meshes whose material is not in the MATBIN binders | 0 (0 names) |
| Read failures | 0 |

## How many shaders cover the game

| Share | Shaders by placed triangles | Shaders by triangles |
|---|---|---|
| 50% | 8 | 7 |
| 80% | 29 | 36 |
| 90% | 52 | 76 |
| 95% | 84 | 138 |
| 99% | 186 | 277 |
| 100% | 507 | 560 |

## Where textures go

Every sampler with a file, routed as `FlverResource.ProcessMaterialTexture` would route it (census copy of that logic).

| Route | Textures | Placed triangles of meshes with one |
|---|---|---|
| Normal | 36001 | 1,504,817,431 |
| Albedo | 34454 | 1,483,895,641 |
| BlendLayer | 25512 | 335,865,807 |
| Dropped | 21365 | 745,509,657 |
| Overlay | 15524 | 904,873,637 |
| BlendDetailMask | 4403 | 328,866,351 |
| Fur | 2304 | 32,466,392 |
| BlendMask | 1206 | 253,708,324 |
| Emissive | 688 | 50,529,630 |
| Water | 173 | 166,154 |
| Specular | 157 | 10,768,917 |
| Deformer | 65 | 317,060 |
| Magma | 13 | 4,307,046 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| AlbedoMap | Albedo:33840 BlendLayer:12546 Overlay:1677 Fur:1276 | 428 | 28474 |
| NormalMap | Normal:35411 Overlay:13847 BlendLayer:12966 Fur:1028 Water:173 Deformer:1 | 417 | 28316 |
| MetallicMap | Dropped:10183 | 113 | 9800 |
| Mask1Map | Dropped:8525 BlendDetailMask:3446 BlendMask:117 | 235 | 8920 |
| Mask3Map | Dropped:1596 BlendMask:1089 BlendDetailMask:35 Magma:13 | 115 | 2553 |
| BlendEdge | BlendDetailMask:922 | 2 | 922 |
| EmissiveMap | Emissive:613 | 91 | 585 |
| g_DiffuseTexture | Albedo:460 | 37 | 460 |
| g_BumpmapTexture | Normal:456 | 36 | 456 |
| g_TranslucencyTexture | Dropped:307 | 32 | 307 |
| DistortionDepth | Dropped:223 | 23 | 223 |
| VectorMap | Dropped:174 Deformer:64 | 30 | 175 |
| Albedo | Albedo:134 | 5 | 134 |
| Normal_Shininess | Normal:134 | 5 | 134 |
| g_SpecularTexture | Specular:110 | 3 | 110 |
| FlowMap | Dropped:100 | 5 | 99 |
| g_DiffuseTexture2 | Dropped:47 | 1 | 47 |
| g_SpecularTexture2 | Specular:47 | 1 | 47 |
| g_BumpmapTexture2 | Dropped:47 | 1 | 47 |
| g_BumpmapTexture3 | Dropped:47 | 1 | 47 |
| Emissive__Base | Emissive:52 | 5 | 26 |
| AmbientOcculusionMap | Dropped:22 | 5 | 22 |
| Emissive__BaseDetail | Emissive:20 | 3 | 20 |
| Albedo__Inside | Albedo:20 | 5 | 20 |
| PseudoTransparency_Anger | Dropped:18 | 1 | 18 |
| Mask__BlendEdgeTexture | Dropped:15 | 3 | 15 |
| Generic__AtmosphereHazeColorLUT | Dropped:14 | 6 | 14 |
| Fade_and_FallOff_Noise | Dropped:10 | 1 | 10 |
| DisplacementMap | Dropped:10 | 2 | 10 |
| MaskTexture | Dropped:5 | 2 | 5 |
| Flowmap_ | Dropped:4 | 4 | 4 |
| FadeOut_Noise | Dropped:3 | 2 | 3 |
| Generic__Flowmap | Dropped:3 | 1 | 3 |
| Emissive_Flicker | Emissive:3 | 2 | 3 |
| Generic_ | Dropped:7 | 1 | 3 |
| ReflectanceMap | Dropped:2 | 2 | 2 |
| Fade__Line | Dropped:2 | 1 | 2 |
| DistortionDepth__Base | Dropped:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `C[DetailBlend]` |  | Overlay | 13.814 | 13.655 | 3331 | MetallicMap | aeg099_610 |
| `M[AMSN_V][Ov_N]` |  | Overlay | 8.244 | 10.832 | 3362 |  | aeg599_890 |
| `M[AMSN_V]_Vesitation` | yes | WindPivot AlphaTest | 7.860 | 0.588 | 229 |  | aeg001_290 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` |  | BlendRecipe | 6.212 | 0.643 | 110 |  | aeg004_114 |
| `M[AMSN_V]` |  |  | 4.778 | 8.177 | 2700 |  | aeg004_045 |
| `M[AMSN][Ov_N]` |  | Overlay | 3.890 | 7.011 | 1442 | MetallicMap | aeg571_117 |
| `M[AMSN_V][Ov_N][Crystal]` |  | Overlay | 3.644 | 5.074 | 2716 | Mask1Map MetallicMap | aeg399_890 |
| `M[AMSN_V][Ov_AN]` |  |  | 2.455 | 3.323 | 924 |  | aeg562_002 |
| `M[AMSN]` |  |  | 2.104 | 2.735 | 764 | MetallicMap | aeg570_051 |
| `M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 2.047 | 1.700 | 267 |  | aeg355_019 |
| `M[Crystal]` |  | Overlay | 1.930 | 0.675 | 169 | Mask1Map MetallicMap | aeg355_058 |
| `M[AMSN_V][Ov_N][Snow]` |  | Overlay | 1.828 | 1.081 | 156 |  | aeg550_319 |
| `M[AMSN_V][Mb2][Ov_N]` |  | BlendRecipe Overlay | 1.727 | 1.540 | 694 |  | aeg599_045 |
| `M[AMSN_V][Ov_N][Mb4_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 1.715 | 1.109 | 119 |  | aeg355_001 |
| `SpeedTree_5010fdmo_w32187` |  |  | 1.651 | 0.052 | 39 | g_TranslucencyTexture | aeg801_250 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` |  | Overlay | 1.543 | 0.307 | 49 | Mask1Map MetallicMap | aeg355_151 |
| `C[AMSN]` |  | AlphaTest | 1.514 | 2.786 | 430 | MetallicMap | c2271 |
| `M[Crystal][CrossFade]` |  | Overlay | 1.402 | 0.270 | 41 | Mask1Map MetallicMap | aeg355_068 |
| `C[AMSN]_Shadow` |  | AlphaTest | 1.323 | 0.122 | 58 | MetallicMap | c5240 |
| `SpeedTree_50777_w3636187` |  | AlphaTest | 1.235 | 0.008 | 13 | g_TranslucencyTexture | aeg801_006 |
| `C[DetailBlend]_SSS` |  | Overlay | 1.188 | 1.843 | 212 | Mask1Map MetallicMap | c3450 |
| `C[Fur]` |  | Fur | 1.179 | 1.377 | 277 | MetallicMap | c3661 |
| `M[AMSN_V]_Vesitation_Emissive_Glow` | yes | WindPivot AlphaTest | 1.175 | 0.773 | 83 |  | aeg599_853 |
| `M[AMSN_V]_Grass2` | yes | WindPivot AlphaTest | 1.083 | 0.234 | 278 | Mask1Map | aeg355_019 |
| `M[AMSN_V][Ov_N][Mb2_ColorAdjust][1Mask_ch18]` |  | BlendRecipe Overlay | 1.081 | 0.543 | 90 |  | aeg355_007 |
| `C[DetailBlend][S2]` |  | AlphaTest | 0.968 | 0.784 | 205 | MetallicMap | c4300 |
| `M[SpiritTree_Trunk]` |  | Overlay AlphaTest | 0.932 | 0.096 | 7 | Mask3Map | aeg594_105 |
| `M[AMSN_V]_Edge` |  | AlphaTest | 0.905 | 1.040 | 505 |  | aeg399_890 |
| `SpeedTree_5077f_w34636187` |  | AlphaTest | 0.847 | 0.034 | 24 | g_TranslucencyTexture | aeg801_250 |
| `M[AMSN_V][Mb2]` | yes | BlendRecipe | 0.780 | 0.818 | 851 |  | aeg030_903 |
| `M[AMSN_V][Mb2][1mask][Ov_N]` |  | BlendRecipe Overlay | 0.621 | 0.167 | 24 |  | aeg004_290 |
| `M[AMSN_V][Ov_N]_Edge` |  | Overlay AlphaTest | 0.616 | 0.380 | 138 |  | aeg599_890 |
| `M[AMSN_V][Mb2][Ov_AN]` |  | BlendRecipe BlendMode2 | 0.590 | 1.146 | 437 |  | aeg007_094 |
| `M[AMSN_V][Mb3][3Mask][Ov_N2]` |  | BlendRecipe Overlay | 0.589 | 0.180 | 19 |  | aeg552_224 |
| `GXFlver_ColDifSpcBumpIbl` |  |  | 0.570 | 0.105 | 62 |  | aeg003_054 |
| `M[AMSN_V]_Grass2_LOD1` | yes | WindPivot AlphaTest | 0.567 | 0.029 | 67 | Mask1Map | aeg004_114 |
| `M[AMSN]_Edge` |  | AlphaTest | 0.465 | 0.795 | 238 | MetallicMap | aeg361_187 |
| `C[AMSN]_ContactFade` |  | BlendMode2 | 0.454 | 0.240 | 65 | MetallicMap | c3661 |
| `M[AMSN_V][CL][Ov_n2][1Mask][Snow_VtxMask]` |  | Overlay | 0.441 | 0.054 | 14 | Mask1Map | aeg552_116 |
| `SpeedTree_5010fdmo_w329a7` |  |  | 0.431 | 0.017 | 30 | g_TranslucencyTexture | aeg801_016 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `C[DetailBlend]` | MetallicMap | 3 | 6.780 | 2337 | aeg099_610 |
| `M[AMSN][Ov_N]` | MetallicMap | 2 | 3.890 | 1442 | aeg571_117 |
| `M[AMSN_V][Ov_N][Crystal]` | Mask1Map | 0 | 3.644 | 2716 | aeg399_890 |
| `M[AMSN_V][Ov_N][Crystal]` | MetallicMap | 2 | 3.644 | 2716 | aeg399_890 |
| `M[AMSN_V][Ov_N][Crystal]` | Mask1Map | 5 | 3.565 | 2701 | aeg399_890 |
| `M[AMSN]` | MetallicMap | 2 | 2.096 | 749 | aeg570_051 |
| `M[Crystal]` | Mask1Map | 0 | 1.925 | 164 | aeg355_058 |
| `M[Crystal]` | MetallicMap | 2 | 1.884 | 135 | aeg355_058 |
| `SpeedTree_5010fdmo_w32187` | g_TranslucencyTexture | -1 | 1.651 | 39 | aeg801_250 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | Mask1Map | 0 | 1.543 | 49 | aeg355_151 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | MetallicMap | 2 | 1.543 | 49 | aeg355_151 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | Mask1Map | 5 | 1.543 | 49 | aeg355_151 |
| `M[Crystal][CrossFade]` | Mask1Map | 0 | 1.402 | 41 | aeg355_068 |
| `M[Crystal][CrossFade]` | MetallicMap | 2 | 1.402 | 41 | aeg355_068 |
| `SpeedTree_50777_w3636187` | g_TranslucencyTexture | -1 | 1.235 | 13 | aeg801_006 |
| `M[SpiritTree_Trunk]` | Mask3Map | 1 | 0.932 | 7 | aeg594_105 |
| `M[SpiritTree_Trunk]` | Mask3Map | 7 | 0.932 | 7 | aeg594_105 |
| `M[AMSN_V]_Grass2` | Mask1Map | 3 | 0.914 | 188 | aeg355_019 |
| `SpeedTree_5077f_w34636187` | g_TranslucencyTexture | -1 | 0.847 | 24 | aeg801_250 |
| `C[DetailBlend]_SSS` | Mask1Map | 8 | 0.744 | 134 | c3450 |

