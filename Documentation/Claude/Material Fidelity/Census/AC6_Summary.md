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
| Dropped | 193337 | 574,203,961 |
| Albedo | 82646 | 953,974,460 |
| Normal | 78506 | 940,246,945 |
| Metallic | 63622 | 657,381,032 |
| OverlayAlbedo | 12029 | 164,922,915 |
| Albedo1 | 7105 | 158,983,880 |
| Normal1 | 6962 | 157,893,928 |
| BlendDetailMask1 | 6024 | 146,646,433 |
| Emissive | 5780 | 20,165,951 |
| Metallic1 | 5266 | 106,812,806 |
| Normal2 | 2246 | 44,200,691 |
| BlendDetailMask2 | 2106 | 45,336,809 |
| Metallic+Offset | 777 | 7,587,570 |
| OverlayNormal | 522 | 34,210,210 |
| Albedo2 | 359 | 3,447,139 |
| BlendDetailMask | 205 | 1,481,447 |
| WaterNormal | 82 | 454,040 |
| Metallic2 | 34 | 37,156 |
| BlendMask | 19 | 0 |
| Normal3 | 14 | 29,956 |
| BlendDetailMask3 | 8 | 29,956 |
| Albedo3 | 6 | 0 |
| Metallic3 | 6 | 0 |

## Roles

| Role | Routes (mesh textures) | Shaders | Meshes |
|---|---|---|---|
| AlbedoMap | Albedo:82384 Dropped:40071 OverlayAlbedo:12029 Albedo1:7105 Albedo2:359 Albedo3:6 | 154 | 82651 |
| NormalMap | Dropped:84354 Normal:78271 Normal1:6962 Normal2:2246 OverlayNormal:522 WaterNormal:82 Normal3:14 | 118 | 78910 |
| MetallicMap | Metallic:63622 Dropped:15753 Metallic1:5266 Metallic+Offset:777 Metallic2:34 Metallic3:6 BlendDetailMask:3 Normal:1 | 60 | 64558 |
| Mask1Map | Dropped:30212 BlendDetailMask2:2106 BlendDetailMask:200 BlendDetailMask1:163 BlendDetailMask3:8 | 68 | 26976 |
| Mask3Map | Dropped:20571 BlendDetailMask1:5861 BlendMask:19 | 54 | 23030 |
| EmissiveMap | Emissive:5780 Dropped:1232 | 76 | 5780 |
| C_Enemy__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:258 Dropped:182 | 1 | 440 |
| C_Enemy__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:230 Dropped:210 | 1 | 440 |
| DisplacementMap | Dropped:299 | 5 | 295 |
| P_ACParts__Marking_snp_Texture2DArray_0_ | Dropped:218 | 1 | 218 |
| Metalness__Base | Dropped:116 | 3 | 116 |
| Generic__AtmosphereHazeColorLUT | Dropped:26 | 3 | 26 |
| Generic__Cloud | Dropped:22 | 1 | 22 |
| Generic__Moon | Dropped:22 | 1 | 22 |
| Generic__Turbulence_Localise | Dropped:19 | 7 | 19 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_0_AlbedoMap | Dropped:7 | 1 | 7 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_1_NormalMap | Dropped:7 | 1 | 7 |
| C_Enemy_DM_RT_PL_2_snp_Texture2DArray_2_MetallicMap | Dropped:7 | 1 | 7 |
| Generic__Turbulence_Localise_Flow | Dropped:2 | 1 | 2 |
| C_Enemy__ColorMaterial_Emissive_Glow_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Dropped:2 | 1 | 2 |
| C_Enemy__ColorMaterial_Emissive_Glow_snp_Texture2DArray_1_GSBlendMap_NormalMap | Dropped:2 | 1 | 2 |
| P_ACParts__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:2 | 1 | 2 |
| P_ACParts__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:2 | 1 | 2 |
| FlowMap | Dropped:2 | 1 | 2 |
| BlendEdge | BlendDetailMask:2 | 1 | 2 |
| C_Enemy__ColorMaterial_PF_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:1 | 1 | 1 |
| C_Enemy__ColorMaterial_PF_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:1 | 1 | 1 |
| C_Cutscene_Enemy_Alpha__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | Albedo:1 | 1 | 1 |
| C_Cutscene_Enemy_Alpha__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap | Normal:1 | 1 | 1 |
| VectorMap | Dropped:1 | 1 | 1 |

## Top 40 shaders by placed triangles

| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |
|---|---|---|---|---|---|---|---|
| `M[MS][MB][M1]` |  |  | 55.198 | 48.808 | 41614 | AlbedoMap MetallicMap NormalMap Mask1Map Mask3Map | aeg120_046 |
| `M[MS][MB][M0]` |  | Overlay | 21.497 | 13.623 | 9069 | NormalMap AlbedoMap Mask3Map Mask1Map MetallicMap | aeg410_010 |
| `M[AMSN][M0][M1]_Edge` |  | AlphaTest | 6.301 | 7.410 | 13211 |  | aeg224_400 |
| `C[Enemy]_ColorMaterial` |  | Overlay | 3.376 | 1.476 | 443 | Mask3Map AlbedoMap NormalMap C_Enemy__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap C_Enemy__ColorMaterial_snp_Texture2DArray_1_GSBlendMap_NormalMap Mask1Map | c3020 |
| `M[MS][Mask][M1]` |  | Overlay | 3.125 | 4.408 | 4660 | NormalMap AlbedoMap MetallicMap Mask1Map Mask3Map | c7400 |
| `M[AMSN][M0][M1]` |  | AlphaTest | 2.131 | 3.737 | 1516 |  | aeg233_800 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2_Child` |  | AlphaTest | 1.425 | 0.011 | 8 | Mask1Map | aeg501_911 |
| `C[AMSN][M0][M1]` |  |  | 0.656 | 0.951 | 306 |  | c7400 |
| `M[AMSN][M1]_Emissive` |  | AlphaTest | 0.620 | 0.690 | 192 |  | aeg203_094 |
| `M[AMSN][M0][M1][CV]_Edge` |  | AlphaTest | 0.590 | 0.715 | 2642 | Mask1Map NormalMap AlbedoMap | aeg039_441 |
| `M[AMSN][M0][M1]_ContactFade` |  | BlendMode2 | 0.567 | 0.085 | 114 |  | aeg411_020 |
| `M[AMSN][M0][M1]_Edge_Translucency_Pivot2` |  | AlphaTest | 0.560 | 0.004 | 10 | Mask1Map | aeg501_911 |
| `M[Interior]` |  |  | 0.484 | 0.183 | 399 | NormalMap Mask1Map EmissiveMap | aeg223_010 |
| `M[A][M0]_Emissive_Glow` |  |  | 0.467 | 0.669 | 3189 |  | aeg059_630 |
| `M[Crystal][M1]` |  | Overlay | 0.327 | 0.910 | 98 | DisplacementMap | aeg039_790 |
| `M[AMSN][M1][Vatex]_Alpha` |  |  | 0.288 | 0.149 | 335 |  | aeg036_380 |
| `M[AMSN][M0][M1]_Edge_Translucency` |  | AlphaTest | 0.227 | 0.023 | 22 | Mask1Map | aeg501_910 |
| `M[AMSN][M0][Mb2][Far_AN][Ov_N][Snow]` |  | Layers2Unweighted | 0.216 | 0.126 | 24 | AlbedoMap NormalMap Mask1Map | aeg501_203 |
| `C[3Mask][Detail][M1]` |  |  | 0.126 | 0.475 | 89 | AlbedoMap NormalMap Mask3Map MetallicMap Mask1Map | c6400 |
| `C[AMSN][M1][Detail]` |  | BlendRecipe | 0.115 | 0.005 | 1 | Metalness__Base NormalMap | c5100 |
| `M[AMSN][M0][M1][Vatex]_Alpha_BlendEdge` |  | BlendMode2 AlphaTest | 0.112 | 0.231 | 404 | Mask1Map | aeg030_600 |
| `C[AMSN][M1][Detail2]` |  | BlendRecipe | 0.108 | 0.828 | 112 | NormalMap Metalness__Base AlbedoMap Mask1Map | aeg840_310 |
| `C[Enemy][Wepon]` |  |  | 0.092 | 0.170 | 105 | NormalMap | c3300 |
| `C[DetailBlend1][Detail]_Emissive_Glow` |  | Overlay | 0.079 | 0.009 | 1 | AlbedoMap | c5500 |
| `M[AMSN][M1]_Emissive_Glow` |  |  | 0.072 | 0.078 | 53 |  | aeg059_600 |
| `M[AMSN][M0][M1][Vatex]_Edge_BlendEdge` |  | BlendMode2 AlphaTest | 0.063 | 0.114 | 180 | Mask1Map | aeg501_110 |
| `M[AMSN_V][Mb2][Far_AN][Ov_N]` |  | Layers2Unweighted | 0.061 | 0.188 | 8 | AlbedoMap NormalMap | aeg030_161 |
| `M[AMSN][M1]` |  |  | 0.057 | 0.195 | 16 |  | aeg030_160 |
| `M[AMSN][POM][M1]_AO_Edge` |  | AlphaTest | 0.057 | 0.122 | 163 | DisplacementMap | aeg204_407 |
| `M[AMSN]` |  |  | 0.051 | 2.287 | 359 |  | aeg022_200 |
| `P[BS][M1]_Emissive_Glow` |  | AlphaTest | 0.048 | 0.049 | 28 |  | aeg840_900 |
| `M[MS][Mask][M0]` |  | Overlay | 0.047 | 0.191 | 361 | NormalMap AlbedoMap Mask1Map Mask3Map MetallicMap | aeg065_600 |
| `C[c4210]_illusion` |  | Scroll BlendMode2 AlphaTest | 0.047 | 0.073 | 1 | EmissiveMap Mask1Map Mask3Map | c4210 |
| `M[AMSN][M0][M1]_decal_Alpha` |  | BlendMode2 | 0.045 | 0.121 | 300 | Mask1Map | aeg411_122 |
| `M[MS][MB][M1]_PF` |  |  | 0.043 | 0.113 | 22 | MetallicMap NormalMap Mask3Map Mask1Map | aeg000_520 |
| `C[AMSN][M1][Detail][DM1][OvA]` |  | Overlay | 0.042 | 0.069 | 3 | MetallicMap AlbedoMap NormalMap Mask1Map | c6250 |
| `P[LaserBlade]_001` |  | Scroll BlendMode4 | 0.035 | 0.123 | 43 | Mask3Map AlbedoMap | c5300 |
| `M[Planet][GroundSurface]_004` |  | Overlay AlphaTest | 0.034 | 0.159 | 6 | Mask1Map AlbedoMap NormalMap Mask3Map | aeg860_010 |
| `M[E]_Heatingwire` |  | Scroll BlendMode4 | 0.034 | 0.035 | 4 | EmissiveMap | aeg410_860 |
| `M[AMSN][M1][2Blend]_Combustion` |  | BlendRecipe Scroll | 0.033 | 0.149 | 49 | Mask1Map EmissiveMap NormalMap | aeg065_600 |

## Top 20 dropped textures by placed triangles

| Shader | Role | Slot | Placed % | Meshes | Example |
|---|---|---|---|---|---|
| `M[MS][MB][M1]` | NormalMap | 3 | 22.206 | 9527 | aeg120_046 |
| `M[MS][MB][M0]` | NormalMap | 3 | 13.734 | 6328 | aeg410_010 |
| `M[MS][MB][M0]` | Mask3Map | 26 | 12.426 | 4755 | aeg410_010 |
| `M[MS][MB][M0]` | AlbedoMap | 13 | 12.270 | 4413 | aeg410_010 |
| `M[MS][MB][M1]` | NormalMap | 24 | 12.229 | 10085 | aeg410_183 |
| `M[MS][MB][M1]` | Mask1Map | 19 | 12.206 | 10081 | aeg410_183 |
| `M[MS][MB][M0]` | NormalMap | 12 | 12.178 | 4578 | aeg410_010 |
| `M[MS][MB][M0]` | NormalMap | 24 | 11.392 | 3788 | aeg034_095 |
| `M[MS][MB][M1]` | Mask3Map | 26 | 11.389 | 6022 | aeg120_046 |
| `M[MS][MB][M0]` | Mask1Map | 19 | 11.226 | 4070 | aeg034_095 |
| `M[MS][MB][M0]` | NormalMap | 14 | 10.583 | 1901 | aeg410_010 |
| `M[MS][MB][M1]` | NormalMap | 12 | 10.554 | 5396 | aeg120_046 |
| `M[MS][MB][M1]` | AlbedoMap | 13 | 10.548 | 5342 | aeg120_046 |
| `M[MS][MB][M1]` | MetallicMap | 17 | 10.144 | 5050 | aeg120_046 |
| `M[MS][MB][M1]` | NormalMap | 14 | 6.263 | 1998 | aeg120_046 |
| `M[MS][MB][M0]` | AlbedoMap | 23 | 5.969 | 1118 | aeg020_200 |
| `M[MS][MB][M0]` | NormalMap | 27 | 5.657 | 3232 | aeg410_010 |
| `M[MS][MB][M1]` | AlbedoMap | 23 | 3.683 | 1613 | aeg039_082 |
| `C[Enemy]_ColorMaterial` | AlbedoMap | 1 | 3.376 | 443 | c3020 |
| `C[Enemy]_ColorMaterial` | NormalMap | 8 | 3.376 | 443 | c3020 |

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

## Binding audit

Each texture's route (`MaterialBinding.Route`, the viewport's own code) against what the game's shader does with
that slot in the material's permutation (`Signatures/{GAME}_SlotMeanings.json`). Placed triangles count a mesh once
per texture, so a column can add up to more than the game.

| Status | Textures | Placed triangles |
|---|---|---|
| Ok | 272691 | 3,581,005,623 |
| Dropped | 177990 | 2,482,326,980 |
| Unverified | 1283 | 65,505,906 |
| UvMismatch | 3473 | 53,243,853 |
| Misrouted | 5314 | 44,151,825 |
| Extra | 6828 | 24,593,243 |
| Approximate | 82 | 454,040 |

### Top 30 issues by placed triangles

| Status | Shader | Slot | Role | Game | Viewport | Placed % | Meshes | Example |
|---|---|---|---|---|---|---|---|---|
| Dropped | `M[MS][MB][M0]` | 13 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 12.108 | 4348 | aeg410_010 |
| Dropped | `M[MS][MB][M0]` | 12 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 12.108 | 4348 | aeg410_010 |
| Dropped | `M[MS][MB][M0]` | 26 | Mask3Map | Mask | Dropped (no route for the role Mask3Map) | 11.863 | 4126 | aeg410_010 |
| Dropped | `M[MS][MB][M1]` | 24 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 10.960 | 9400 | aeg410_183 |
| Dropped | `M[MS][MB][M1]` | 19 | Mask1Map | Mask | Dropped (no route for the role Mask1Map) | 10.937 | 9396 | aeg410_183 |
| Dropped | `M[MS][MB][M1]` | 3 | NormalMap | NormalGloss | Dropped (a layer the blend doesn't use) | 10.607 | 3452 | aeg101_270 |
| Dropped | `M[MS][MB][M0]` | 3 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 10.563 | 5108 | aeg410_010 |
| Dropped | `M[MS][MB][M0]` | 14 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 10.356 | 1865 | aeg410_010 |
| Dropped | `M[MS][MB][M1]` | 3 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 10.331 | 5682 | aeg120_046 |
| Dropped | `M[MS][MB][M1]` | 13 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 9.359 | 5248 | aeg120_046 |
| Dropped | `M[MS][MB][M1]` | 12 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 9.359 | 5248 | aeg120_046 |
| Dropped | `M[MS][MB][M1]` | 26 | Mask3Map | Mask | Dropped (no route for the role Mask3Map) | 9.202 | 5122 | aeg120_046 |
| Dropped | `M[MS][MB][M1]` | 17 | MetallicMap | Metallic | Dropped (a metallic map of a layer above the bottom one) | 8.954 | 4956 | aeg120_046 |
| Dropped | `M[MS][MB][M0]` | 19 | Mask1Map | Mask | Dropped (no route for the role Mask1Map) | 8.659 | 3706 | aeg034_943 |
| Dropped | `M[MS][MB][M0]` | 24 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 7.195 | 3444 | aeg020_200 |
| Dropped | `M[MS][MB][M0]` | 27 | NormalMap | Normal | Dropped (its Normal layer 0 slot already holds another texture) | 5.530 | 3088 | aeg410_010 |
| Dropped | `M[MS][MB][M1]` | 14 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 5.105 | 1906 | aeg120_046 |
| Misrouted | `C[Enemy]_ColorMaterial` | 0 | NormalMap | Albedo | OverlayNormal | 3.376 | 443 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 1 | AlbedoMap | NormalGloss | Dropped (its Albedo layer 0 slot already holds another texture) | 3.376 | 443 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 8 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 3.376 | 443 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 4 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 3.376 | 443 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 9 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 3.376 | 443 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 2 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 3.376 | 441 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 7 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 3.337 | 440 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 6 | NormalMap | NormalGloss | Dropped (its Normal layer 0 slot already holds another texture) | 3.337 | 440 | c3020 |
| Unverified | `C[Enemy]_ColorMaterial` | -1 | C_Enemy__ColorMaterial_snp_Texture2DArray_0_GSBlendMap_AlbedoMap | (no slot) | Albedo | 3.337 | 258 | c3020 |
| Dropped | `C[Enemy]_ColorMaterial` | 14 | Mask3Map | Other | Dropped (no route for the role Mask3Map) | 3.256 | 313 | c3020 |
| Dropped | `M[MS][MB][M0]` | 3 | NormalMap | NormalGloss | Dropped (a layer the blend doesn't use) | 3.158 | 1168 | aeg034_943 |
| Dropped | `M[MS][MB][M0]` | 23 | AlbedoMap | Albedo | Dropped (its Albedo layer 0 slot already holds another texture) | 3.115 | 790 | aeg020_200 |
| UvMismatch | `M[MS][MB][M1]` | 24 | NormalMap | NormalGloss | Normal | 3.012 | 883 | aeg120_256 |

