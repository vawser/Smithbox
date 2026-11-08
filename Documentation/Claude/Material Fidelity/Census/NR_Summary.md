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
| Albedo | 28404 | 1,819,694,589 |
| Normal | 27752 | 1,834,187,016 |
| Dropped | 24442 | 752,032,109 |
| OverlayNormal | 14970 | 920,349,564 |
| Albedo1 | 7411 | 413,873,104 |
| BlendDetailMask | 7238 | 405,396,382 |
| Normal1 | 7058 | 407,376,882 |
| Metallic+Offset | 4084 | 223,660,189 |
| Metallic | 2982 | 197,974,277 |
| Metallic1 | 2944 | 75,282,796 |
| OverlayAlbedo | 2284 | 188,863,999 |
| Normal2 | 1799 | 230,210,469 |
| Albedo2 | 1761 | 233,277,635 |
| OverlayNormal1 | 1458 | 150,537,425 |
| BlendMask | 1302 | 259,142,827 |
| OverlayAlbedo1 | 879 | 100,344,867 |
| Albedo3 | 786 | 165,488,381 |
| Normal3 | 785 | 165,477,891 |
| FurAlbedo | 659 | 32,466,392 |
| FurNormal | 653 | 32,343,560 |
| OverlayNormal2 | 638 | 54,726,498 |
| Emissive | 611 | 50,529,630 |
| OverlayAlbedo2 | 385 | 42,416,500 |
| WaterNormal | 128 | 137,354 |
| Specular | 110 | 10,768,917 |
| OverlayNormal3 | 109 | 16,779,330 |
| OverlayAlbedo3 | 81 | 10,743,341 |
| Deformer | 65 | 317,060 |
| Specular1 | 47 | 0 |
| Metallic2 | 27 | 847,235 |
| MagmaGlow | 13 | 4,307,046 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| AlbedoMap | Albedo:27784 Albedo1:7358 Dropped:7311 OverlayAlbedo:2284 Albedo2:1755 OverlayAlbedo1:879 Albedo3:786 FurAlbedo:659 OverlayAlbedo2:385 OverlayAlbedo3:81 Normal3:57 | 428 | 28474 |
| NormalMap | Normal:27162 OverlayNormal:14970 Dropped:8798 Normal1:7011 Normal2:1752 OverlayNormal1:1458 Normal3:728 FurNormal:653 OverlayNormal2:638 WaterNormal:128 OverlayNormal3:109 Albedo:6 Albedo1:6 Albedo2:6 Deformer:1 | 417 | 28316 |
| MetallicMap | Metallic+Offset:4084 Metallic:2982 Metallic1:2944 Dropped:146 Metallic2:27 | 113 | 9800 |
| Mask1Map | BlendDetailMask:6281 Dropped:5690 BlendMask:117 | 235 | 8920 |
| Mask3Map | Dropped:1500 BlendMask:1185 BlendDetailMask:35 MagmaGlow:13 | 115 | 2553 |
| BlendEdge | BlendDetailMask:922 | 2 | 922 |
| EmissiveMap | Emissive:582 Dropped:31 | 91 | 585 |
| g_DiffuseTexture | Albedo:460 | 37 | 460 |
| g_BumpmapTexture | Normal:456 | 36 | 456 |
| g_TranslucencyTexture | Dropped:307 | 32 | 307 |
| DistortionDepth | Dropped:223 | 23 | 223 |
| VectorMap | Dropped:174 Deformer:64 | 30 | 175 |
| Albedo | Albedo:134 | 5 | 134 |
| Normal_Shininess | Normal:134 | 5 | 134 |
| g_SpecularTexture | Specular:110 | 3 | 110 |
| FlowMap | Dropped:100 | 5 | 99 |
| g_DiffuseTexture2 | Albedo1:47 | 1 | 47 |
| g_SpecularTexture2 | Specular1:47 | 1 | 47 |
| g_BumpmapTexture2 | Normal1:47 | 1 | 47 |
| g_BumpmapTexture3 | Normal2:47 | 1 | 47 |
| Emissive__Base | Emissive:26 Dropped:26 | 5 | 26 |
| AmbientOcculusionMap | Dropped:22 | 5 | 22 |
| Emissive__BaseDetail | Dropped:20 | 3 | 20 |
| Albedo__Inside | Albedo:20 | 5 | 20 |
| PseudoTransparency_Anger | Dropped:18 | 1 | 18 |
| Mask__BlendEdgeTexture | Dropped:15 | 3 | 15 |
| Generic__AtmosphereHazeColorLUT | Dropped:14 | 6 | 14 |
| DisplacementMap | Dropped:10 | 2 | 10 |
| Fade_and_FallOff_Noise | Dropped:10 | 1 | 10 |
| MaskTexture | Dropped:5 | 2 | 5 |
| Flowmap_ | Dropped:4 | 4 | 4 |
| FadeOut_Noise | Dropped:3 | 2 | 3 |
| Generic_ | Dropped:7 | 1 | 3 |
| Emissive_Flicker | Emissive:3 | 2 | 3 |
| Generic__Flowmap | Dropped:3 | 1 | 3 |
| ReflectanceMap | Dropped:2 | 2 | 2 |
| Fade__Line | Dropped:2 | 1 | 2 |
| DistortionDepth__Base | Dropped:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `C[DetailBlend]` |  | Overlay AlphaTest | 13.814 | 13.655 | 3331 | AlbedoMap NormalMap MetallicMap | aeg099_610 |
| `M[AMSN_V][Ov_N]` |  | Overlay | 8.244 | 10.832 | 3362 |  | aeg599_890 |
| `M[AMSN_V]_Vesitation` | yes | WindPivot AlphaTest | 7.860 | 0.588 | 229 |  | aeg001_290 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` |  | BlendRecipe | 6.212 | 0.643 | 110 | AlbedoMap NormalMap | aeg004_114 |
| `M[AMSN_V]` |  |  | 4.778 | 8.177 | 2700 |  | aeg004_045 |
| `M[AMSN][Ov_N]` |  | Overlay AlphaTest | 3.890 | 7.011 | 1442 |  | aeg571_117 |
| `M[AMSN_V][Ov_N][Crystal]` |  | BlendRecipe Overlay | 3.644 | 5.074 | 2716 | Mask1Map | aeg399_890 |
| `M[AMSN_V][Ov_AN]` |  | Overlay | 2.455 | 3.323 | 924 |  | aeg562_002 |
| `M[AMSN]` |  |  | 2.104 | 2.735 | 764 |  | aeg570_051 |
| `M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 2.047 | 1.700 | 267 |  | aeg355_019 |
| `M[Crystal]` |  | Overlay | 1.930 | 0.675 | 169 | Mask1Map | aeg355_058 |
| `M[AMSN_V][Ov_N][Snow]` |  | Overlay | 1.828 | 1.081 | 156 | AlbedoMap NormalMap | aeg550_319 |
| `M[AMSN_V][Mb2][Ov_N]` |  | BlendRecipe Overlay | 1.727 | 1.540 | 694 |  | aeg599_045 |
| `M[AMSN_V][Ov_N][Mb4_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 1.715 | 1.109 | 119 |  | aeg355_001 |
| `SpeedTree_5010fdmo_w32187` |  |  | 1.651 | 0.052 | 39 | g_TranslucencyTexture | aeg801_250 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` |  | Overlay | 1.543 | 0.307 | 49 | Mask1Map AlbedoMap NormalMap | aeg355_151 |
| `C[AMSN]` |  |  | 1.514 | 2.786 | 430 |  | c2271 |
| `M[Crystal][CrossFade]` |  | Overlay | 1.402 | 0.270 | 41 | Mask1Map | aeg355_068 |
| `C[AMSN]_Shadow` |  | AlphaTest | 1.323 | 0.122 | 58 |  | c5240 |
| `SpeedTree_50777_w3636187` |  | AlphaTest | 1.235 | 0.008 | 13 | g_TranslucencyTexture | aeg801_006 |
| `C[DetailBlend]_SSS` |  | Overlay | 1.188 | 1.843 | 212 | Mask1Map AlbedoMap NormalMap | c3450 |
| `C[Fur]` |  | Fur AlphaTest | 1.179 | 1.377 | 277 | AlbedoMap NormalMap | c3661 |
| `M[AMSN_V]_Vesitation_Emissive_Glow` | yes | WindPivot AlphaTest | 1.175 | 0.773 | 83 |  | aeg599_853 |
| `M[AMSN_V]_Grass2` | yes | WindPivot AlphaTest | 1.083 | 0.234 | 278 | Mask1Map | aeg355_019 |
| `M[AMSN_V][Ov_N][Mb2_ColorAdjust][1Mask_ch18]` |  | BlendRecipe Overlay | 1.081 | 0.543 | 90 |  | aeg355_007 |
| `C[DetailBlend][S2]` |  | Overlay | 0.968 | 0.784 | 205 | AlbedoMap NormalMap MetallicMap | c4300 |
| `M[SpiritTree_Trunk]` |  | Overlay AlphaTest | 0.932 | 0.096 | 7 | Mask3Map | aeg594_105 |
| `M[AMSN_V]_Edge` |  | AlphaTest | 0.905 | 1.040 | 505 |  | aeg399_890 |
| `SpeedTree_5077f_w34636187` |  | AlphaTest | 0.847 | 0.034 | 24 | g_TranslucencyTexture | aeg801_250 |
| `M[AMSN_V][Mb2]` | yes | BlendRecipe | 0.780 | 0.818 | 851 |  | aeg030_903 |
| `M[AMSN_V][Mb2][1mask][Ov_N]` |  | BlendRecipe Overlay | 0.621 | 0.167 | 24 |  | aeg004_290 |
| `M[AMSN_V][Ov_N]_Edge` |  | Overlay AlphaTest | 0.616 | 0.380 | 138 |  | aeg599_890 |
| `M[AMSN_V][Mb2][Ov_AN]` |  | BlendRecipe Overlay | 0.590 | 1.146 | 437 | NormalMap | aeg007_094 |
| `M[AMSN_V][Mb3][3Mask][Ov_N2]` |  | BlendRecipe Overlay | 0.589 | 0.180 | 19 | NormalMap | aeg552_224 |
| `GXFlver_ColDifSpcBumpIbl` |  |  | 0.570 | 0.105 | 62 |  | aeg003_054 |
| `M[AMSN_V]_Grass2_LOD1` | yes | WindPivot AlphaTest | 0.567 | 0.029 | 67 | Mask1Map | aeg004_114 |
| `M[AMSN]_Edge` |  | AlphaTest | 0.465 | 0.795 | 238 |  | aeg361_187 |
| `C[AMSN]_ContactFade` |  | BlendMode2 | 0.454 | 0.240 | 65 |  | c3661 |
| `M[AMSN_V][CL][Ov_n2][1Mask][Snow_VtxMask]` |  | Overlay | 0.441 | 0.054 | 14 | AlbedoMap NormalMap Mask1Map | aeg552_116 |
| `SpeedTree_5010fdmo_w329a7` |  |  | 0.431 | 0.017 | 30 | g_TranslucencyTexture | aeg801_016 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | AlbedoMap | 13 | 6.212 | 110 | aeg004_114 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | NormalMap | 7 | 6.212 | 110 | aeg004_114 |
| `M[AMSN_V][Ov_N][Crystal]` | Mask1Map | 0 | 3.644 | 2716 | aeg399_890 |
| `C[DetailBlend]` | NormalMap | 9 | 2.080 | 268 | c3661 |
| `M[Crystal]` | Mask1Map | 0 | 1.925 | 164 | aeg355_058 |
| `C[DetailBlend]` | AlbedoMap | 4 | 1.852 | 167 | c3661 |
| `M[AMSN_V][Ov_N][Snow]` | AlbedoMap | 2 | 1.828 | 156 | aeg550_319 |
| `M[AMSN_V][Ov_N][Snow]` | NormalMap | 3 | 1.828 | 156 | aeg550_319 |
| `SpeedTree_5010fdmo_w32187` | g_TranslucencyTexture | -1 | 1.651 | 39 | aeg801_250 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | Mask1Map | 0 | 1.543 | 49 | aeg355_151 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | AlbedoMap | 3 | 1.543 | 49 | aeg355_151 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | NormalMap | 4 | 1.543 | 49 | aeg355_151 |
| `M[AMSN_V][Ov_N][Crystal][CrossFade]` | Mask1Map | 5 | 1.543 | 49 | aeg355_151 |
| `C[DetailBlend]` | NormalMap | 12 | 1.475 | 263 | c4300 |
| `M[Crystal][CrossFade]` | Mask1Map | 0 | 1.402 | 41 | aeg355_068 |
| `C[DetailBlend]` | AlbedoMap | 10 | 1.362 | 158 | c4191 |
| `SpeedTree_50777_w3636187` | g_TranslucencyTexture | -1 | 1.235 | 13 | aeg801_006 |
| `C[DetailBlend][S2]` | NormalMap | 11 | 0.953 | 191 | c4300 |
| `C[DetailBlend]` | NormalMap | 5 | 0.948 | 177 | c3500 |
| `M[SpiritTree_Trunk]` | Mask3Map | 1 | 0.932 | 7 | aeg594_105 |

## Binding audit

Each texture's route (`MaterialBinding.Route`, the viewport's own code) against what the game's shader does with
that slot in the material's permutation (`Signatures/{GAME}_SlotMeanings.json`). Placed triangles count a mesh once
per texture, so a column can add up to more than the game.

| Status | Textures | Placed triangles |
|---|---|---|
| Ok | 113165 | 7,766,079,117 |
| Dropped | 19375 | 1,249,563,441 |
| Unverified | 7441 | 405,909,291 |
| Approximate | 967 | 63,027,612 |
| Misrouted | 577 | 56,145,009 |
| Extra | 215 | 8,097,914 |
| UvMismatch | 60 | 2,098,664 |

### Top 30 issues by placed triangles

| Status | Shader | Slot | Role | Game | Viewport | Placed % | Meshes | Example |
|---|---|---|---|---|---|---|---|---|
| Dropped | `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | 13 | AlbedoMap | Albedo | Dropped (a layer the blend doesn't use) | 6.212 | 110 | aeg004_114 |
| Dropped | `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | 7 | NormalMap | Normal | Dropped (a layer the blend doesn't use) | 6.212 | 110 | aeg004_114 |
| Dropped | `M[AMSN_V][Ov_N][Crystal]` | 0 | Mask1Map | Emissive | Dropped (no route for the role Mask1Map) | 3.644 | 2716 | aeg399_890 |
| Dropped | `M[Crystal]` | 0 | Mask1Map | Emissive | Dropped (no route for the role Mask1Map) | 1.925 | 164 | aeg355_058 |
| Dropped | `M[AMSN_V][Ov_N][Snow]` | 2 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.828 | 156 | aeg550_319 |
| Dropped | `M[AMSN_V][Ov_N][Snow]` | 3 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.828 | 156 | aeg550_319 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_DiffuseTexture | (no slot) | Albedo | 1.651 | 39 | aeg801_250 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_BumpmapTexture | (no slot) | Normal | 1.651 | 39 | aeg801_250 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 1.651 | 39 | aeg801_250 |
| Dropped | `M[AMSN_V][Ov_N][Crystal][CrossFade]` | 0 | Mask1Map | Emissive | Dropped (no route for the role Mask1Map) | 1.543 | 49 | aeg355_151 |
| Dropped | `M[AMSN_V][Ov_N][Crystal][CrossFade]` | 3 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.543 | 49 | aeg355_151 |
| Dropped | `M[AMSN_V][Ov_N][Crystal][CrossFade]` | 4 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.543 | 49 | aeg355_151 |
| Dropped | `M[AMSN_V][Ov_N][Crystal][CrossFade]` | 5 | Mask1Map | Mask | Dropped (no route for the role Mask1Map) | 1.543 | 49 | aeg355_151 |
| Dropped | `M[Crystal][CrossFade]` | 0 | Mask1Map | Emissive | Dropped (no route for the role Mask1Map) | 1.402 | 41 | aeg355_068 |
| Dropped | `C[DetailBlend]` | 9 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.401 | 71 | c3661 |
| Dropped | `C[DetailBlend]` | 4 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.400 | 70 | c3661 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_DiffuseTexture | (no slot) | Albedo | 1.235 | 13 | aeg801_006 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_BumpmapTexture | (no slot) | Normal | 1.235 | 13 | aeg801_006 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 1.235 | 13 | aeg801_006 |
| Dropped | `C[DetailBlend][S2]` | 11 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 0.953 | 191 | c4300 |
| Dropped | `M[SpiritTree_Trunk]` | 1 | Mask3Map | Other | Dropped (no route for the role Mask3Map) | 0.932 | 7 | aeg594_105 |
| Dropped | `M[SpiritTree_Trunk]` | 7 | Mask3Map | Emissive | Dropped (no route for the role Mask3Map) | 0.932 | 7 | aeg594_105 |
| Dropped | `M[AMSN_V]_Grass2` | 3 | Mask1Map | Other | Dropped (no route for the role Mask1Map) | 0.914 | 188 | aeg355_019 |
| Dropped | `C[DetailBlend]` | 6 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 0.870 | 83 | c3500 |
| Unverified | `SpeedTree_5077f_w34636187` | -1 | g_DiffuseTexture | (no slot) | Albedo | 0.847 | 24 | aeg801_250 |
| Unverified | `SpeedTree_5077f_w34636187` | -1 | g_BumpmapTexture | (no slot) | Normal | 0.847 | 24 | aeg801_250 |
| Unverified | `SpeedTree_5077f_w34636187` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 0.847 | 24 | aeg801_250 |
| Dropped | `C[DetailBlend][S2]` | 8 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 0.760 | 187 | c4300 |
| Dropped | `C[DetailBlend]_SSS` | 8 | Mask1Map | Subsurface | Dropped (no route for the role Mask1Map) | 0.744 | 134 | c3450 |
| Dropped | `C[DetailBlend]` | 12 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 0.714 | 84 | c4300 |

