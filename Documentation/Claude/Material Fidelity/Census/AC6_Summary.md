# AC6 material census

Generated 2026-11-08 by `Tools/MaterialSurvey`. Placed triangles = LOD0 triangles x MSB placements;
characters and parts count only where an MSB places them (enemies), so the unplaced columns cover the rest.

| | Count |
|---|---|
| MATBINs in the game | 5554 |
| MATBINs used by a mesh | 3420 |
| Shaders used | 199 (0 with a shader table entry) |
| Meshes | 85395 |
| Triangles (LOD0, each model once) | 154,080,751 |
| Placed triangles | 956,600,926 |
| Meshes whose material is not in the MATBIN binders | 706 (100 names) |
| Read failures | 0 |

## How many shaders cover the game

| Share | Shaders by placed triangles | Shaders by triangles |
|---|---|---|
| 50% | 1 | 2 |
| 80% | 3 | 6 |
| 90% | 6 | 13 |
| 95% | 11 | 32 |
| 99% | 30 | 199 |
| 100% | 146 | 199 |

## Where textures go

Every sampler with a file, routed as `FlverResource.ProcessMaterialTexture` would route it (census copy of that logic).

| Route | Textures | Placed triangles of meshes with one |
|---|---|---|
| Normal | 172403 | 948,915,935 |
| Albedo | 130174 | 953,774,538 |
| Metallic | 63796 | 662,627,740 |
| Dropped | 59841 | 567,249,448 |
| MetallicOtherLayer | 20790 | 221,543,212 |
| Overlay | 12039 | 165,310,643 |
| Emissive | 7016 | 20,165,951 |
| Metallic+Offset | 690 | 6,530,742 |
| BlendLayer | 417 | 199,922 |
| Water | 274 | 454,040 |
| MetallicLayer0 | 71 | 191,144 |
| MetallicNoLayer | 62 | 116,478 |
| MetallicLayer1 | 26 | 74,666 |
| BlendDetailMask | 19 | 83,444 |
| BlendMask | 19 | 0 |
| MetallicLayer2 | 18 | 0 |
| MetallicLayer3 | 6 | 0 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| AlbedoMap | Albedo:129723 Overlay:12029 BlendLayer:202 | 154 | 82651 |
| NormalMap | Normal:171952 Water:274 BlendLayer:215 Overlay:10 | 118 | 78910 |
| MetallicMap | Metallic:63796 MetallicOtherLayer:20790 Metallic+Offset:690 MetallicLayer0:71 MetallicNoLayer:62 MetallicLayer1:26 MetallicLayer2:18 MetallicLayer3:6 BlendDetailMask:3 | 60 | 64558 |
| Mask1Map | Dropped:32675 BlendDetailMask:14 | 68 | 26976 |
| Mask3Map | Dropped:26432 BlendMask:19 | 54 | 23030 |
| EmissiveMap | Emissive:7012 | 76 | 5780 |
| C_Enemy__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:440 | 1 | 440 |
| C_Enemy__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:440 | 1 | 440 |
| DisplacementMap | Dropped:299 | 5 | 295 |
| P_ACParts__Marking_snp_Texture2DArray_0_ | Dropped:218 | 1 | 218 |
| Metalness__Base | Dropped:116 | 3 | 116 |
| Generic__AtmosphereHazeColorLUT | Dropped:26 | 3 | 26 |
| Generic__Cloud | Dropped:22 | 1 | 22 |
| Generic__Moon | Dropped:22 | 1 | 22 |
| Generic__Turbulence_Localise | Dropped:19 | 7 | 19 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_0_AlbedoMap | Albedo:7 | 1 | 7 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_1_NormalMap | Normal:7 | 1 | 7 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_2_MetallicMap | Dropped:7 | 1 | 7 |
| C_Enemy__ColorMaterial_Emissive_Glow_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Emissive:2 | 1 | 2 |
| C_Enemy__ColorMaterial_Emissive_Glow_snp_Texture2DArray_1_GSBlendMap_NormalMap | Emissive:2 | 1 | 2 |
| FlowMap | Dropped:2 | 1 | 2 |
| Generic__Turbulence_Localise_Flow | Dropped:2 | 1 | 2 |
| BlendEdge | BlendDetailMask:2 | 1 | 2 |
| P_ACParts__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:2 | 1 | 2 |
| P_ACParts__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:2 | 1 | 2 |
| VectorMap | Dropped:1 | 1 | 1 |
| C_Enemy__ColorMaterial_PF_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:1 | 1 | 1 |
| C_Enemy__ColorMaterial_PF_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:1 | 1 | 1 |
| C_Cutscene_Enemy_Alpha__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:1 | 1 | 1 |
| C_Cutscene_Enemy_Alpha__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `M[MS][MB][M1]` |  |  | 55.198 | 48.808 | 41614 | Mask1Map Mask3Map | aeg120_046 |
| `M[MS][MB][M0]` |  |  | 21.497 | 13.623 | 9069 | Mask3Map Mask1Map | aeg410_010 |
| `M[AMSN][M0][M1]_Edge` |  | AlphaTest | 6.301 | 7.410 | 13211 |  | aeg224_400 |
| `C[Enemy]_ColorMaterial` |  |  | 3.376 | 1.476 | 443 | Mask3Map Mask1Map | c3020 |
| `M[MS][Mask][M1]` |  | Overlay | 3.125 | 4.408 | 4660 | Mask1Map Mask3Map | c7400 |
| `M[AMSN][M0][M1]` |  | AlphaTest | 2.131 | 3.737 | 1516 |  | aeg233_800 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2_Child` |  | AlphaTest | 1.425 | 0.011 | 8 | Mask1Map | aeg501_911 |
| `C[AMSN][M0][M1]` |  |  | 0.656 | 0.951 | 306 |  | c7400 |
| `M[AMSN][M1]_Emissive` |  | AlphaTest | 0.620 | 0.690 | 192 |  | aeg203_094 |
| `M[AMSN][M0][M1][CV]_Edge` |  | AlphaTest | 0.590 | 0.715 | 2642 | Mask1Map | aeg039_441 |
| `M[AMSN][M0][M1]_ContactFade` |  | BlendMode2 | 0.567 | 0.085 | 114 |  | aeg411_020 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2` |  | AlphaTest | 0.560 | 0.004 | 10 | Mask1Map | aeg501_911 |
| `M[Interior]` |  |  | 0.484 | 0.183 | 399 | Mask1Map | aeg223_010 |
| `M[A][M0]_Emissive_Glow` |  |  | 0.467 | 0.669 | 3189 |  | aeg059_630 |
| `M[Crystal][M1]` |  | Overlay | 0.327 | 0.910 | 98 | DisplacementMap | aeg039_790 |
| `M[AMSN][M1][Vatex]_Alpha` |  | BlendMode2 AlphaTest | 0.288 | 0.149 | 335 |  | aeg036_380 |
| `M[AMSN][M0][M1]_Edge_Translucency` |  | AlphaTest | 0.227 | 0.023 | 22 | Mask1Map | aeg501_910 |
| `M[AMSN][M0][Mb2][Far_AN][Ov_N][Snow]` |  | Layers2Unweighted | 0.216 | 0.126 | 24 | Mask1Map | aeg501_203 |
| `C[3Mask][Detail][M1]` |  |  | 0.126 | 0.475 | 89 | Mask1Map Mask3Map | c6400 |
| `C[AMSN][M1][Detail]` |  |  | 0.115 | 0.005 | 1 | Metalness__Base | c5100 |
| `M[AMSN][M0][M1][Vatex]_Alpha_BlendEdge` |  | BlendMode2 AlphaTest | 0.112 | 0.231 | 404 | Mask1Map | aeg030_600 |
| `C[AMSN][M1][Detail2]` |  |  | 0.108 | 0.828 | 112 | Metalness__Base Mask1Map | aeg840_310 |
| `C[Enemy][Wepon]` |  |  | 0.092 | 0.170 | 105 |  | c3300 |
| `C[DetailBlend1][Detail]_Emissive_Glow` |  |  | 0.079 | 0.009 | 1 |  | c5500 |
| `M[AMSN][M1]_Emissive_Glow` |  |  | 0.072 | 0.078 | 53 |  | aeg059_600 |
| `M[AMSN][M0][M1][Vatex]_Edge_BlendEdge` |  | BlendMode2 AlphaTest | 0.063 | 0.114 | 180 | Mask1Map | aeg501_110 |
| `M[AMSN_V][Mb2][Far_AN][Ov_N]` |  | Layers2Unweighted | 0.061 | 0.188 | 8 |  | aeg030_161 |
| `M[AMSN][M1]` |  |  | 0.057 | 0.195 | 16 |  | aeg030_160 |
| `M[AMSN][POM][M1]_AO_Edge` |  | AlphaTest | 0.057 | 0.122 | 163 | DisplacementMap | aeg204_407 |
| `M[AMSN]` |  |  | 0.051 | 2.287 | 359 |  | aeg022_200 |
| `P[BS][M1]_Emissive_Glow` |  |  | 0.048 | 0.049 | 28 |  | aeg840_900 |
| `M[MS][Mask][M0]` |  | Overlay Scroll | 0.047 | 0.191 | 361 | Mask3Map Mask1Map | aeg065_600 |
| `C[c4210]_illusion` |  | Scroll BlendMode2 AlphaTest | 0.047 | 0.073 | 1 | Mask1Map Mask3Map | c4210 |
| `M[AMSN][M0][M1]_decal_Alpha` |  | BlendMode2 | 0.045 | 0.121 | 300 | Mask1Map | aeg411_122 |
| `M[MS][MB][M1]_PF` |  |  | 0.043 | 0.113 | 22 | Mask3Map Mask1Map | aeg000_520 |
| `C[AMSN][M1][Detail][DM1][OvA]` |  |  | 0.042 | 0.069 | 3 | Mask1Map | c6250 |
| `P[LaserBlade]_001` |  | Scroll BlendMode4 | 0.035 | 0.123 | 43 | Mask3Map | c5300 |
| `M[Planet][GroundSurface]_004` |  | Overlay AlphaTest | 0.034 | 0.159 | 6 | Mask1Map Mask3Map | aeg860_010 |
| `M[E]_Heatingwire` |  | Scroll BlendMode4 | 0.034 | 0.035 | 4 |  | aeg410_860 |
| `M[AMSN][M1][2Blend]_Combustion` |  | Scroll | 0.033 | 0.149 | 49 | Mask1Map | aeg065_600 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `M[MS][MB][M1]` | Mask3Map | 26 | 22.777 | 10393 | aeg120_046 |
| `M[MS][MB][M0]` | Mask3Map | 26 | 15.807 | 6243 | aeg410_010 |
| `M[MS][MB][M1]` | Mask1Map | 19 | 15.263 | 11068 | aeg410_183 |
| `M[MS][MB][M0]` | Mask1Map | 19 | 13.472 | 5360 | aeg034_095 |
| `C[Enemy]_ColorMaterial` | Mask3Map | 14 | 3.256 | 313 | c3020 |
| `M[MS][MB][M0]` | Mask1Map | 29 | 3.154 | 3446 | aeg020_145 |
| `M[MS][Mask][M1]` | Mask3Map | 6 | 2.470 | 4470 | c7400 |
| `M[MS][Mask][M1]` | Mask1Map | 19 | 2.079 | 3054 | c7400 |
| `M[MS][Mask][M1]` | Mask1Map | 29 | 1.508 | 2371 | aeg411_450 |
| `M[MS][Mask][M1]` | Mask3Map | 26 | 1.474 | 3159 | aeg120_695 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2_Child` | Mask1Map | 3 | 1.339 | 6 | aeg501_911 |
| `C[Enemy]_ColorMaterial` | Mask1Map | 23 | 0.982 | 26 | c3020 |
| `M[AMSN][M0][M1][CV]_Edge` | Mask1Map | 4 | 0.590 | 2638 | aeg039_441 |
| `M[MS][MB][M1]` | Mask1Map | 29 | 0.489 | 1437 | aeg039_622 |
| `M[Interior]` | Mask1Map | 9 | 0.484 | 395 | aeg223_010 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2` | Mask1Map | 3 | 0.436 | 8 | aeg501_911 |
| `M[Crystal][M1]` | DisplacementMap | 3 | 0.327 | 98 | aeg039_790 |
| `M[AMSN][M0][Mb2][Far_AN][Ov_N][Snow]` | Mask1Map | 4 | 0.216 | 24 | aeg501_203 |
| `M[AMSN][M0][M1]_Edge_Translucency` | Mask1Map | 3 | 0.133 | 18 | aeg501_910 |
| `C[3Mask][Detail][M1]` | Mask3Map | 0 | 0.126 | 89 | c6400 |

## Materials not found (top 15)

- `m[m10_00]330_paintedmetal`: 21 meshes, e.g. m10_00_00_00_000019
- `m[m10_00]026_aet101_033`: 21 meshes, e.g. m10_00_00_00_000011
- `m[m10_00]025_aet101_034`: 21 meshes, e.g. m10_00_00_00_000027
- `m[m10_00]107_aet001_042`: 20 meshes, e.g. m10_00_00_00_000017
- `m[m10_00]081_aet101_124`: 19 meshes, e.g. m10_00_00_00_000030
- `m[m10_00]023_aet101_019`: 17 meshes, e.g. m10_00_00_00_000011
- `m[m10_00]051_aet001_051`: 16 meshes, e.g. m10_00_00_00_000010
- `m[m10_00]001_aet999_852`: 16 meshes, e.g. m10_00_00_00_000025
- `m[m10_00]302_aet101_306`: 15 meshes, e.g. m10_00_00_00_000017
- `m[m10_00]086_aet101_127`: 15 meshes, e.g. m10_00_00_00_000025
- `m[m10_00]701_emissive_red`: 14 meshes, e.g. m10_00_00_00_000030
- `m[m10_00]327_101_154`: 14 meshes, e.g. m10_00_00_00_000010
- `m[m10_00]033_aet001_020`: 14 meshes, e.g. m10_00_00_00_000027
- `m[m10_00]029_aet001_026`: 13 meshes, e.g. m10_00_00_00_000030
- `m[m10_00]006_aet999_854`: 13 meshes, e.g. m10_00_00_00_000007

