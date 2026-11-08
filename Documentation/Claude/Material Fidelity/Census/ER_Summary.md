# ER material census

Generated 2026-11-08 by `Tools/MaterialSurvey`. Placed triangles = LOD0 triangles x MSB placements;
characters and parts count only where an MSB places them (enemies), so the unplaced columns cover the rest.

| | Count |
|---|---|
| MATBINs in the game | 20605 |
| MATBINs used by a mesh | 15664 |
| Shaders used | 643 (21 with a shader table entry) |
| Meshes | 116882 |
| Triangles (LOD0, each model once) | 505,890,774 |
| Placed triangles | 8,159,603,956 |
| Meshes whose material is not in the MATBIN binders | 79 (10 names) |
| Read failures | 0 |

## How many shaders cover the game

| Share | Shaders by placed triangles | Shaders by triangles |
|---|---|---|
| 50% | 9 | 7 |
| 80% | 30 | 27 |
| 90% | 51 | 54 |
| 95% | 80 | 101 |
| 99% | 175 | 246 |
| 100% | 582 | 643 |

## Where textures go

Every sampler with a file, routed as `FlverResource.ProcessMaterialTexture` would route it (census copy of that logic).

| Route | Textures | Placed triangles of meshes with one |
|---|---|---|
| BlendLayer | 149495 | 2,334,950,882 |
| Normal | 129445 | 5,680,001,133 |
| Albedo | 121793 | 5,648,418,251 |
| Overlay | 73612 | 3,770,465,932 |
| BlendDetailMask | 25007 | 2,254,982,871 |
| Dropped | 19206 | 1,435,117,580 |
| Metallic+Offset | 13630 | 446,834,812 |
| Metallic | 9155 | 501,251,412 |
| BlendMask | 8248 | 1,602,356,642 |
| Fur | 6733 | 113,238,479 |
| Emissive | 1065 | 119,507,101 |
| MetallicLayer0 | 963 | 47,980,375 |
| MetallicLayer1 | 860 | 40,981,906 |
| Water | 641 | 2,018,367 |
| Magma | 486 | 26,076,146 |
| MetallicOtherLayer | 482 | 19,860,566 |
| Deformer | 266 | 10,909,563 |
| MetallicLayer2 | 250 | 11,261,548 |
| MetallicNoLayer | 160 | 3,065,242 |
| Specular | 150 | 8,068,209 |
| Shininess | 4 | 1,923,012 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| NormalMap | Normal:127368 BlendLayer:75452 Overlay:62333 Fur:2901 Water:641 Deformer:25 | 473 | 111791 |
| AlbedoMap | Albedo:119679 BlendLayer:74043 Overlay:11279 Fur:3832 | 486 | 111639 |
| Mask1Map | BlendDetailMask:21817 Dropped:9066 BlendMask:922 Deformer:37 | 262 | 30550 |
| MetallicMap | Metallic+Offset:13630 Metallic:9155 MetallicLayer0:963 MetallicLayer1:860 MetallicOtherLayer:482 MetallicLayer2:250 MetallicNoLayer:160 | 100 | 23847 |
| Mask3Map | BlendMask:7326 Dropped:6960 Magma:486 BlendDetailMask:88 | 146 | 14382 |
| BlendEdge | BlendDetailMask:3102 Dropped:19 | 6 | 3121 |
| g_DiffuseTexture | Albedo:2100 | 69 | 2100 |
| g_BumpmapTexture | Normal:2077 | 67 | 2077 |
| g_TranslucencyTexture | Dropped:1688 | 60 | 1688 |
| EmissiveMap | Emissive:1052 | 86 | 1016 |
| DistortionDepth | Dropped:493 | 22 | 493 |
| VectorMap | Dropped:359 Deformer:204 | 33 | 440 |
| FlowMap | Dropped:260 | 15 | 250 |
| g_SpecularTexture | Specular:150 | 6 | 150 |
| DisplacementMap | Dropped:140 | 9 | 140 |
| Generic_ | Dropped:55 | 4 | 29 |
| Generic__LUT1 | Dropped:22 | 1 | 22 |
| 3Mask | Dropped:18 | 3 | 18 |
| PseudoTransparency_Anger | Dropped:18 | 1 | 18 |
| AlphaNoise | Dropped:17 | 3 | 17 |
| Generic__AtmosphereHazeColorLUT | Dropped:16 | 5 | 16 |
| Albedo__Inside | Albedo:14 | 3 | 14 |
| Emissive_Flicker | Emissive:9 | 1 | 9 |
| Fire___Light_Edge | Dropped:8 | 2 | 8 |
| Fire___Black_Edge | Dropped:8 | 2 | 8 |
| ReflectanceMap | Dropped:13 | 2 | 7 |
| Fire___Pattern | Dropped:7 | 1 | 7 |
| Fire___Mask | Dropped:7 | 1 | 7 |
| BlendEdgeTexture | Dropped:6 | 1 | 6 |
| Alpha_Subtract | Dropped:6 | 1 | 6 |
| DistortionDepth__Base | Dropped:4 | 1 | 4 |
| Translucency__Base | Dropped:4 | 1 | 4 |
| Emissive__Noise | Emissive:3 | 2 | 3 |
| Generic__Cloud | Dropped:2 | 2 | 2 |
| ShininessMap | Shininess:4 | 1 | 2 |
| AlphaWing | Dropped:2 | 1 | 2 |
| Generic__Galaxy | Dropped:1 | 1 | 1 |
| g_BloodMaskTexture | Dropped:1 | 1 | 1 |
| g_ScatteringMaskTexture | Dropped:1 | 1 | 1 |
| g_EmissiveTexture | Emissive:1 | 1 | 1 |
| Alpha_Inside_Base | Dropped:1 | 1 | 1 |
| Fade__Base | Dropped:1 | 1 | 1 |
| Finger___em__Edge | Dropped:1 | 1 | 1 |
| Mud__Base | Dropped:1 | 1 | 1 |
| Crack__Edge | Dropped:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `M[AMSN_V][Ov_N]` |  | Overlay | 12.420 | 22.856 | 22085 |  | aeg040_067 |
| `C[DetailBlend]` |  | AlphaTest | 8.886 | 7.554 | 12675 |  | c3661 |
| `M[AMSN_V]` |  |  | 6.289 | 7.185 | 8943 |  | aeg004_045 |
| `M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 5.769 | 2.994 | 2394 |  | aeg052_013 |
| `M[AMSN_V]_Vesitation` | yes | WindPivot AlphaTest | 4.791 | 0.309 | 388 |  | aeg001_290 |
| `M[AMSN_V][Mb2][Ov_AN]` |  | BlendRecipe | 4.057 | 1.190 | 2080 |  | aeg004_162 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` |  | BlendRecipe | 3.666 | 0.221 | 117 |  | aeg004_114 |
| `M[AMSN_V][Ov_N][Mb4_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 3.056 | 1.426 | 552 |  | aeg050_000 |
| `M[AMSN][Ov_N]` |  | Overlay | 2.407 | 4.933 | 4048 |  | aeg230_883 |
| `M[AMSN_V][Ov_AN]` |  |  | 2.340 | 4.052 | 3672 |  | aeg231_231 |
| `M[AMSN_V][Ov_N][Mb2_ColorAdjust][1Mask_ch18]` |  | BlendRecipe Overlay | 2.087 | 0.883 | 725 |  | aeg052_473 |
| `SpeedTree_5010fdmo_w32187` |  |  | 2.005 | 0.084 | 202 | g_TranslucencyTexture | aeg801_250 |
| `C[AMSN]_Shadow` |  | AlphaTest | 2.001 | 0.033 | 59 |  | c5240 |
| `M[AMSN_V][Mb2][Ov_N]` |  | BlendRecipe Overlay | 1.931 | 3.408 | 3942 |  | aeg052_326 |
| `M[AMSN]` |  |  | 1.740 | 2.315 | 2480 |  | aeg099_620 |
| `M[AMSN_V][Mb3][3Mask][Ov_N]` |  | BlendRecipe Overlay | 1.720 | 3.395 | 1627 |  | aeg450_015 |
| `M[AMSN_V][Mb2][1mask][Ov_N]` |  | BlendRecipe Overlay | 1.628 | 0.182 | 193 |  | aeg007_110 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` |  | Overlay | 1.375 | 1.961 | 1451 | Mask1Map | aeg001_420 |
| `SpeedTree_50777_w3636187` |  | AlphaTest | 1.284 | 0.012 | 73 | g_TranslucencyTexture | aeg490_000 |
| `M[AMSN_V][Ov_N][Snow]` |  |  | 1.228 | 1.395 | 759 |  | aeg220_102 |
| `C[AMSN]` |  |  | 1.178 | 0.485 | 1012 |  | c5401 |
| `M[AMSN_V]_Grass2` | yes | WindPivot AlphaTest | 1.055 | 0.224 | 583 | Mask1Map | aeg004_114 |
| `SpeedTree_5010f_w321a7` |  | AlphaTest | 1.046 | 0.073 | 207 | g_TranslucencyTexture | aeg490_057 |
| `C[DetailBlend]_FurBlur` |  | Overlay | 0.997 | 0.044 | 37 | VectorMap Mask1Map | c5410 |
| `C[Fur]` |  | Fur AlphaTest | 0.980 | 0.588 | 868 | DisplacementMap | c5450 |
| `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` |  | BlendRecipe Overlay | 0.980 | 0.937 | 2095 |  | aeg007_042 |
| `C[DetailBlend]_SSS` |  |  | 0.865 | 0.466 | 277 | Mask1Map | c4080 |
| `C[Shell]_FurBlur` |  | AlphaTest | 0.835 | 0.076 | 82 | Mask1Map VectorMap | c5410 |
| `M[AMSN_V]_Edge` |  | AlphaTest | 0.764 | 1.386 | 2162 |  | aeg231_090 |
| `M[AMSN_V]_Vesitation_Emissive_Glow` | yes | WindPivot AlphaTest | 0.761 | 0.078 | 287 |  | aeg099_720 |
| `M[AMSN_V][Mb3][3Mask][Ov_AN][Snow_VtxMask]` |  | BlendRecipe Overlay | 0.696 | 0.786 | 515 |  | aeg007_000 |
| `SpeedTree_5010fdmo_w329a7` |  |  | 0.687 | 0.037 | 185 | g_TranslucencyTexture | aeg801_096 |
| `M[AMSN]_Edge` |  | AlphaTest | 0.661 | 1.004 | 797 |  | aeg411_096 |
| `SpeedTree_5077f_w34636187` |  | AlphaTest | 0.656 | 0.027 | 86 | g_TranslucencyTexture | aeg801_250 |
| `SpeedTree_5010f_w312187` |  | AlphaTest | 0.592 | 0.059 | 162 | g_TranslucencyTexture | aeg801_210 |
| `M[AMSN_V][Snow]` |  |  | 0.591 | 0.488 | 249 |  | aeg040_010 |
| `M[AMSN][Ov_N]_Edge` |  | Overlay AlphaTest | 0.563 | 0.786 | 801 |  | aeg110_305 |
| `M[AMSN_V][Mb3][VC][3Mask][Ov_N][Cover]` |  | Layers3Unweighted | 0.550 | 0.234 | 84 | Mask3Map | aeg040_021 |
| `C[DetailBlend][S2]` |  | AlphaTest | 0.520 | 0.245 | 562 |  | c4300 |
| `M[AMSN_V][Ov_N]_Edge` |  | Overlay AlphaTest | 0.454 | 1.006 | 696 |  | aeg052_751 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `SpeedTree_5010fdmo_w32187` | g_TranslucencyTexture | -1 | 2.005 | 202 | aeg801_250 |
| `SpeedTree_50777_w3636187` | g_TranslucencyTexture | -1 | 1.284 | 73 | aeg490_000 |
| `SpeedTree_5010f_w321a7` | g_TranslucencyTexture | -1 | 1.046 | 207 | aeg490_057 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` | Mask1Map | 4 | 1.030 | 1378 | aeg430_074 |
| `C[DetailBlend]_FurBlur` | Mask1Map | 5 | 0.997 | 31 | c5410 |
| `C[DetailBlend]_FurBlur` | VectorMap | 2 | 0.996 | 30 | c5410 |
| `C[Shell]_FurBlur` | VectorMap | 1 | 0.835 | 62 | c5410 |
| `C[Shell]_FurBlur` | Mask1Map | 0 | 0.733 | 20 | c5410 |
| `SpeedTree_5010fdmo_w329a7` | g_TranslucencyTexture | -1 | 0.687 | 185 | aeg801_096 |
| `SpeedTree_5077f_w34636187` | g_TranslucencyTexture | -1 | 0.656 | 86 | aeg801_250 |
| `C[DetailBlend]_SSS` | Mask1Map | 8 | 0.614 | 190 | c4080 |
| `SpeedTree_5010f_w312187` | g_TranslucencyTexture | -1 | 0.592 | 162 | aeg801_210 |
| `M[AMSN_V]_Grass2` | Mask1Map | 3 | 0.559 | 323 | aeg004_114 |
| `M[AMSN_V][Mb3][VC][3Mask][Ov_N][Cover]` | Mask3Map | 0 | 0.550 | 84 | aeg040_021 |
| `M[Crystal]` | Mask1Map | 0 | 0.382 | 166 | aeg024_410 |
| `SpeedTree_50f1f_w321a7` | g_TranslucencyTexture | -1 | 0.380 | 183 | aeg490_055 |
| `C[Fur]` | DisplacementMap | 5 | 0.374 | 52 | c3661 |
| `SpeedTree_50f1f_w346369a7` | g_TranslucencyTexture | -1 | 0.344 | 73 | aeg801_016 |
| `SpeedTree_5077f_w3636187` | g_TranslucencyTexture | -1 | 0.328 | 20 | aeg801_086 |
| `SpeedTree_5010f_w32187` | g_TranslucencyTexture | -1 | 0.311 | 18 | aeg490_000 |

## Materials not found (top 15)

- `p[fc_f_0000]_ssstest`: 48 meshes, e.g. hr_a_0101_l
- ``: 15 meshes, e.g. m60_46_38_10_463810
- `p[fc_m_0105]_cloth`: 4 meshes, e.g. fc_m_0105_l
- `aeg004_010_id00`: 3 meshes, e.g. m16_00_00_99_000123
- `p[fg_a_0100]_sss`: 2 meshes, e.g. fc_m_0100_l
- `p[fc_m_0105]`: 2 meshes, e.g. fc_m_0105_l
- `p[fc_m_0105]_body`: 2 meshes, e.g. fc_m_0105_l
- `aeg004_010_id02`: 1 meshes, e.g. m16_00_00_99_000123
- `aeg004_010_id03`: 1 meshes, e.g. m16_00_00_99_000123
- `aeg004_010_id01`: 1 meshes, e.g. m16_00_00_99_000123

