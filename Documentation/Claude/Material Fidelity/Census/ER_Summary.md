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
| Dropped | 114544 | 3,154,070,052 |
| Albedo | 111694 | 7,983,369,133 |
| Normal | 109029 | 7,921,246,892 |
| OverlayNormal | 66052 | 3,869,320,633 |
| Albedo1 | 26577 | 2,348,172,150 |
| BlendDetailMask | 25097 | 2,267,760,831 |
| Normal1 | 24160 | 2,255,043,684 |
| OverlayAlbedo | 15159 | 894,972,514 |
| Metallic+Offset | 13752 | 448,453,778 |
| Albedo2 | 12629 | 1,339,028,051 |
| Normal2 | 12550 | 1,334,368,242 |
| Metallic | 9942 | 543,414,528 |
| BlendMask | 8248 | 1,602,356,642 |
| Albedo3 | 2432 | 580,615,151 |
| Normal3 | 2432 | 580,615,151 |
| FurAlbedo | 1990 | 113,238,479 |
| FurNormal | 1898 | 111,865,217 |
| Emissive | 1017 | 119,507,101 |
| Metallic1 | 914 | 45,180,199 |
| MagmaGlow | 486 | 26,076,146 |
| WaterNormal | 381 | 2,018,367 |
| Deformer | 266 | 10,909,563 |
| Metallic2 | 250 | 11,261,548 |
| Specular | 150 | 8,068,209 |
| Shininess | 2 | 1,923,012 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| NormalMap | Normal:106952 OverlayNormal:66052 Dropped:54264 Normal1:24160 Normal2:12550 Normal3:2432 FurNormal:1898 WaterNormal:381 Deformer:25 Albedo:2 Albedo1:2 Albedo2:2 | 473 | 111791 |
| AlbedoMap | Albedo:109577 Dropped:40473 Albedo1:26575 OverlayAlbedo:15159 Albedo2:12627 Albedo3:2432 FurAlbedo:1990 | 486 | 111639 |
| Mask1Map | BlendDetailMask:21901 Dropped:8982 BlendMask:922 Deformer:37 | 262 | 30550 |
| MetallicMap | Metallic+Offset:13752 Metallic:9942 Metallic1:914 Dropped:642 Metallic2:250 | 100 | 23847 |
| Mask3Map | BlendMask:7326 Dropped:6960 MagmaGlow:486 BlendDetailMask:88 | 146 | 14382 |
| BlendEdge | BlendDetailMask:3102 Dropped:19 | 6 | 3121 |
| g_DiffuseTexture | Albedo:2100 | 69 | 2100 |
| g_BumpmapTexture | Normal:2077 | 67 | 2077 |
| g_TranslucencyTexture | Dropped:1688 | 60 | 1688 |
| EmissiveMap | Emissive:1014 Dropped:38 | 86 | 1016 |
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
| Emissive_Flicker | Dropped:7 Emissive:2 | 1 | 9 |
| Fire___Light_Edge | Dropped:8 | 2 | 8 |
| Fire___Black_Edge | Dropped:8 | 2 | 8 |
| Fire___Pattern | Dropped:7 | 1 | 7 |
| Fire___Mask | Dropped:7 | 1 | 7 |
| ReflectanceMap | Dropped:13 | 2 | 7 |
| BlendEdgeTexture | BlendDetailMask:6 | 1 | 6 |
| Alpha_Subtract | Dropped:6 | 1 | 6 |
| DistortionDepth__Base | Dropped:4 | 1 | 4 |
| Translucency__Base | Dropped:4 | 1 | 4 |
| Emissive__Noise | Dropped:3 | 2 | 3 |
| ShininessMap | Shininess:2 Dropped:2 | 1 | 2 |
| AlphaWing | Dropped:2 | 1 | 2 |
| Generic__Cloud | Dropped:2 | 2 | 2 |
| Generic__Galaxy | Dropped:1 | 1 | 1 |
| g_EmissiveTexture | Emissive:1 | 1 | 1 |
| Alpha_Inside_Base | Dropped:1 | 1 | 1 |
| Fade__Base | Dropped:1 | 1 | 1 |
| g_BloodMaskTexture | Dropped:1 | 1 | 1 |
| g_ScatteringMaskTexture | Dropped:1 | 1 | 1 |
| Mud__Base | Albedo:1 | 1 | 1 |
| Finger___em__Edge | Dropped:1 | 1 | 1 |
| Crack__Edge | Dropped:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `M[AMSN_V][Ov_N]` |  | Overlay | 12.420 | 22.856 | 22085 |  | aeg040_067 |
| `C[DetailBlend]` |  | Overlay | 8.886 | 7.554 | 12675 | AlbedoMap NormalMap MetallicMap | c3661 |
| `M[AMSN_V]` |  |  | 6.289 | 7.185 | 8943 |  | aeg004_045 |
| `M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 5.769 | 2.994 | 2394 |  | aeg052_013 |
| `M[AMSN_V]_Vesitation` | yes | WindPivot AlphaTest | 4.791 | 0.309 | 388 |  | aeg001_290 |
| `M[AMSN_V][Mb2][Ov_AN]` |  | BlendRecipe Overlay | 4.057 | 1.190 | 2080 | NormalMap | aeg004_162 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` |  | BlendRecipe | 3.666 | 0.221 | 117 | AlbedoMap NormalMap | aeg004_114 |
| `M[AMSN_V][Ov_N][Mb4_ColorAdjust][3Mask_ch18]` |  | BlendRecipe Overlay | 3.056 | 1.426 | 552 |  | aeg050_000 |
| `M[AMSN][Ov_N]` |  | Overlay | 2.407 | 4.933 | 4048 |  | aeg230_883 |
| `M[AMSN_V][Ov_AN]` |  | Overlay | 2.340 | 4.052 | 3672 |  | aeg231_231 |
| `M[AMSN_V][Ov_N][Mb2_ColorAdjust][1Mask_ch18]` |  | BlendRecipe Overlay | 2.087 | 0.883 | 725 |  | aeg052_473 |
| `SpeedTree_5010fdmo_w32187` |  |  | 2.005 | 0.084 | 202 | g_TranslucencyTexture | aeg801_250 |
| `C[AMSN]_Shadow` |  | AlphaTest | 2.001 | 0.033 | 59 |  | c5240 |
| `M[AMSN_V][Mb2][Ov_N]` |  | BlendRecipe Overlay AlphaTest | 1.931 | 3.408 | 3942 |  | aeg052_326 |
| `M[AMSN]` |  |  | 1.740 | 2.315 | 2480 |  | aeg099_620 |
| `M[AMSN_V][Mb3][3Mask][Ov_N]` |  | BlendRecipe Overlay | 1.720 | 3.395 | 1627 |  | aeg450_015 |
| `M[AMSN_V][Mb2][1mask][Ov_N]` |  | BlendRecipe Overlay | 1.628 | 0.182 | 193 |  | aeg007_110 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` |  | Overlay | 1.375 | 1.961 | 1451 | AlbedoMap NormalMap Mask1Map | aeg001_420 |
| `SpeedTree_50777_w3636187` |  | AlphaTest | 1.284 | 0.012 | 73 | g_TranslucencyTexture | aeg490_000 |
| `M[AMSN_V][Ov_N][Snow]` |  |  | 1.228 | 1.395 | 759 | AlbedoMap NormalMap | aeg220_102 |
| `C[AMSN]` |  |  | 1.178 | 0.485 | 1012 |  | c5401 |
| `M[AMSN_V]_Grass2` | yes | WindPivot AlphaTest | 1.055 | 0.224 | 583 | Mask1Map | aeg004_114 |
| `SpeedTree_5010f_w321a7` |  | AlphaTest | 1.046 | 0.073 | 207 | g_TranslucencyTexture | aeg490_057 |
| `C[DetailBlend]_FurBlur` |  | Overlay | 0.997 | 0.044 | 37 | VectorMap Mask1Map AlbedoMap NormalMap | c5410 |
| `C[Fur]` |  | Fur AlphaTest | 0.980 | 0.588 | 868 | AlbedoMap NormalMap DisplacementMap | c5450 |
| `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` |  | BlendRecipe Overlay | 0.980 | 0.937 | 2095 | AlbedoMap NormalMap | aeg007_042 |
| `C[DetailBlend]_SSS` |  | Overlay | 0.865 | 0.466 | 277 | Mask1Map AlbedoMap NormalMap | c4080 |
| `C[Shell]_FurBlur` |  | Fur AlphaTest | 0.835 | 0.076 | 82 | AlbedoMap VectorMap Mask1Map | c5410 |
| `M[AMSN_V]_Edge` |  | AlphaTest | 0.764 | 1.386 | 2162 |  | aeg231_090 |
| `M[AMSN_V]_Vesitation_Emissive_Glow` | yes | WindPivot AlphaTest | 0.761 | 0.078 | 287 |  | aeg099_720 |
| `M[AMSN_V][Mb3][3Mask][Ov_AN][Snow_VtxMask]` |  | BlendRecipe Overlay | 0.696 | 0.786 | 515 | AlbedoMap NormalMap | aeg007_000 |
| `SpeedTree_5010fdmo_w329a7` |  |  | 0.687 | 0.037 | 185 | g_TranslucencyTexture | aeg801_096 |
| `M[AMSN]_Edge` |  | AlphaTest | 0.661 | 1.004 | 797 |  | aeg411_096 |
| `SpeedTree_5077f_w34636187` |  | AlphaTest | 0.656 | 0.027 | 86 | g_TranslucencyTexture | aeg801_250 |
| `SpeedTree_5010f_w312187` |  | AlphaTest | 0.592 | 0.059 | 162 | g_TranslucencyTexture | aeg801_210 |
| `M[AMSN_V][Snow]` |  |  | 0.591 | 0.488 | 249 | AlbedoMap NormalMap | aeg040_010 |
| `M[AMSN][Ov_N]_Edge` |  | Overlay AlphaTest | 0.563 | 0.786 | 801 |  | aeg110_305 |
| `M[AMSN_V][Mb3][VC][3Mask][Ov_N][Cover]` |  | Layers3Unweighted | 0.550 | 0.234 | 84 | AlbedoMap NormalMap Mask3Map | aeg040_021 |
| `C[DetailBlend][S2]` |  | Overlay | 0.520 | 0.245 | 562 | NormalMap AlbedoMap MetallicMap | c4300 |
| `M[AMSN_V][Ov_N]_Edge` |  | Overlay AlphaTest | 0.454 | 1.006 | 696 |  | aeg052_751 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `C[DetailBlend]` | NormalMap | 12 | 5.210 | 6789 | c3661 |
| `M[AMSN_V][Mb2][Ov_AN]` | NormalMap | 5 | 4.057 | 2076 | aeg004_162 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | NormalMap | 7 | 3.666 | 117 | aeg004_114 |
| `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | AlbedoMap | 13 | 3.596 | 115 | aeg004_114 |
| `C[DetailBlend]` | AlbedoMap | 10 | 3.537 | 5190 | aeg099_610 |
| `C[DetailBlend]` | NormalMap | 9 | 2.520 | 3861 | c3661 |
| `SpeedTree_5010fdmo_w32187` | g_TranslucencyTexture | -1 | 2.005 | 202 | aeg801_250 |
| `C[DetailBlend]` | AlbedoMap | 4 | 1.947 | 2904 | c3661 |
| `C[DetailBlend]` | NormalMap | 5 | 1.433 | 1877 | c3500 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` | AlbedoMap | 2 | 1.375 | 1451 | aeg001_420 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` | NormalMap | 3 | 1.375 | 1450 | aeg001_420 |
| `SpeedTree_50777_w3636187` | g_TranslucencyTexture | -1 | 1.284 | 73 | aeg490_000 |
| `C[DetailBlend]` | AlbedoMap | 2 | 1.260 | 1557 | c3500 |
| `M[AMSN_V][Ov_N][Snow]` | AlbedoMap | 2 | 1.228 | 759 | aeg220_102 |
| `M[AMSN_V][Ov_N][Snow]` | NormalMap | 3 | 1.228 | 759 | aeg220_102 |
| `M[AMSN_V][Ov_N][Snow]` | NormalMap | 4 | 1.158 | 752 | aeg220_102 |
| `SpeedTree_5010f_w321a7` | g_TranslucencyTexture | -1 | 1.046 | 207 | aeg490_057 |
| `M[AMSN_V][Ov_N][Snow_VtxMask]` | Mask1Map | 4 | 1.030 | 1378 | aeg430_074 |
| `C[DetailBlend]_FurBlur` | Mask1Map | 5 | 0.997 | 31 | c5410 |
| `C[DetailBlend]_FurBlur` | VectorMap | 2 | 0.996 | 30 | c5410 |

## Materials not found (top 15)

- `p[fc_f_0000]_ssstest`: 48 meshes, e.g. hr_a_0101_l
- ``: 15 meshes, e.g. m60_46_38_10_463810
- `p[fc_m_0105]_cloth`: 4 meshes, e.g. fc_m_0105_l
- `aeg004_010_id00`: 3 meshes, e.g. m16_00_00_99_000123
- `p[fc_m_0105]`: 2 meshes, e.g. fc_m_0105_l
- `p[fc_m_0105]_body`: 2 meshes, e.g. fc_m_0105_l
- `p[fg_a_0100]_sss`: 2 meshes, e.g. fc_m_0100_l
- `aeg004_010_id02`: 1 meshes, e.g. m16_00_00_99_000123
- `aeg004_010_id03`: 1 meshes, e.g. m16_00_00_99_000123
- `aeg004_010_id01`: 1 meshes, e.g. m16_00_00_99_000123

## Binding audit

Each texture's route (`MaterialBinding.Route`, the viewport's own code) against what the game's shader does with
that slot in the material's permutation (`Signatures/{GAME}_SlotMeanings.json`). Placed triangles count a mesh once
per texture, so a column can add up to more than the game.

| Status | Textures | Placed triangles |
|---|---|---|
| Ok | 440834 | 32,993,337,259 |
| Dropped | 106032 | 5,454,038,886 |
| Unverified | 6070 | 2,243,245,061 |
| Approximate | 4269 | 227,122,063 |
| Misrouted | 3727 | 139,394,524 |
| Extra | 362 | 18,614,057 |
| UvMismatch | 91 | 3,824,425 |

### Top 30 issues by placed triangles

| Status | Shader | Slot | Role | Game | Viewport | Placed % | Meshes | Example |
|---|---|---|---|---|---|---|---|---|
| Dropped | `C[DetailBlend]` | 12 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 4.726 | 6209 | c3661 |
| Dropped | `M[AMSN_V][Mb2][Ov_AN]` | 5 | NormalMap | Normal | Dropped (a layer the blend doesn't use) | 4.057 | 2076 | aeg004_162 |
| Dropped | `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | 7 | NormalMap | Normal | Dropped (a layer the blend doesn't use) | 3.666 | 117 | aeg004_114 |
| Dropped | `M[AMSN_V][Mb4_Ch2][3Mask][Ov_AN]` | 13 | AlbedoMap | Albedo | Dropped (a layer the blend doesn't use) | 3.596 | 115 | aeg004_114 |
| Dropped | `C[DetailBlend]` | 10 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 3.085 | 4743 | aeg099_610 |
| Dropped | `C[DetailBlend]` | 9 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 2.106 | 3190 | c3661 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_DiffuseTexture | (no slot) | Albedo | 2.005 | 202 | aeg801_250 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_BumpmapTexture | (no slot) | Normal | 2.005 | 202 | aeg801_250 |
| Unverified | `SpeedTree_5010fdmo_w32187` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 2.005 | 202 | aeg801_250 |
| Dropped | `C[DetailBlend]` | 4 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.693 | 2462 | c3661 |
| Dropped | `M[AMSN_V][Ov_N][Snow_VtxMask]` | 2 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.375 | 1451 | aeg001_420 |
| Dropped | `M[AMSN_V][Ov_N][Snow_VtxMask]` | 3 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.375 | 1450 | aeg001_420 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_DiffuseTexture | (no slot) | Albedo | 1.284 | 73 | aeg490_000 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_BumpmapTexture | (no slot) | Normal | 1.284 | 73 | aeg490_000 |
| Unverified | `SpeedTree_50777_w3636187` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 1.284 | 73 | aeg490_000 |
| Dropped | `M[AMSN_V][Ov_N][Snow]` | 2 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 1.228 | 759 | aeg220_102 |
| Dropped | `M[AMSN_V][Ov_N][Snow]` | 3 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.228 | 759 | aeg220_102 |
| Dropped | `M[AMSN_V][Ov_N][Snow]` | 4 | NormalMap | Normal | Dropped (its Normal layer 0 slot already holds another texture) | 1.158 | 752 | aeg220_102 |
| Unverified | `SpeedTree_5010f_w321a7` | -1 | g_DiffuseTexture | (no slot) | Albedo | 1.046 | 207 | aeg490_057 |
| Unverified | `SpeedTree_5010f_w321a7` | -1 | g_BumpmapTexture | (no slot) | Normal | 1.046 | 207 | aeg490_057 |
| Unverified | `SpeedTree_5010f_w321a7` | -1 | g_TranslucencyTexture | (no slot) | Dropped (no route for the role g_TranslucencyTexture) | 1.046 | 207 | aeg490_057 |
| Dropped | `M[AMSN_V][Ov_N][Snow_VtxMask]` | 4 | Mask1Map | Mask | Dropped (no route for the role Mask1Map) | 1.030 | 1378 | aeg430_074 |
| Dropped | `C[DetailBlend]` | 5 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 1.011 | 1232 | c3500 |
| Dropped | `C[DetailBlend]_FurBlur` | 5 | Mask1Map | Other | Dropped (no route for the role Mask1Map) | 0.997 | 31 | c5410 |
| Dropped | `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` | 6 | AlbedoMap | Albedo | Dropped (a layer the blend doesn't use) | 0.980 | 2095 | aeg007_042 |
| Dropped | `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` | 7 | NormalMap | NormalGloss | Dropped (a layer the blend doesn't use) | 0.980 | 2095 | aeg007_042 |
| Dropped | `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` | 0 | NormalMap | NormalGloss | Dropped (a layer the blend doesn't use) | 0.980 | 2095 | aeg007_042 |
| Dropped | `M[AMSN_V][Mb2][Ov_N][Snow_VtxMask]` | 1 | NormalMap | NormalGloss | Dropped (a layer the blend doesn't use) | 0.980 | 2095 | aeg007_042 |
| Dropped | `C[DetailBlend]` | 2 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 0.903 | 991 | c3500 |
| Dropped | `C[Shell]_FurBlur` | 7 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 0.833 | 44 | c5410 |

