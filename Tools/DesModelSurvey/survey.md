# DES model survey

| Category | Models | With issues | Meshes | Static | Double sided | Layout!=0 | Tex found | Tex missing | Tex only in map TPF |
|---|---|---|---|---|---|---|---|---|---|
| chr | 132 | 87 | 802 | 67 | 241 | 0 | 1415 | 72 | 0 |
| obj | 1068 | 267 | 2107 | 1861 | 352 | 0 | 3078 | 1237 | 0 |
| parts | 680 | 210 | 1274 | 312 | 330 | 0 | 2285 | 598 | 0 |
| map | 1639 | 281 | 8498 | 8498 | 468 | 0 | 26316 | 309 | 31 |

| Category | Static rigid nested (all verts) | Bind pose moved | Static weighted unplaced |
|---|---|---|---|
| chr | 2 | 8 | 23 |
| obj | 57 | 2 | 10 |
| parts | 0 | 20 | 0 |
| map | 46 | 0 | 8 |

| Category | Bind pose drawn by NormalW | NormalW >= used bones | NormalW >= nodes | NormalW needs remap | Static needs bone | Static bone ignored | Idx-no-weight | Idx-no-weight nested | Mesh node non-identity | Bad indices | Empty | >65535 verts |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| chr | 0 | 66 | 66 | 0 | 0 | 0 | 37 | 2 | 25 | 0 | 0 | 0 |
| obj | 0 | 1812 | 1803 | 9 | 39 | 1 | 1834 | 32 | 570 | 0 | 0 | 0 |
| parts | 0 | 312 | 312 | 0 | 0 | 0 | 302 | 0 | 90 | 0 | 0 | 0 |
| map | 0 | 8479 | 8352 | 0 | 127 | 0 | 8486 | 29 | 675 | 0 | 0 | 0 |

## issue

- `issue double sided mesh (renderer always culls)`: chr 76, obj 211, parts 190, map 260
- `issue static rigid mesh placed by local node transform, needs world`: chr 1, obj 26, map 4
- `issue bind-pose mesh without weights moved by the index-no-weight path`: chr 6, obj 1, parts 20
- `issue material shared by bind-pose and rigid meshes`: chr 15, obj 8, parts 2
- `issue static weighted mesh drawn without its node transform`: chr 15, obj 7, map 3
- `issue index-no-weight path uses local, not world, node transform`: chr 1, obj 11, map 4
- `issue mtd missing from mtd.mtdbnd: DS_map[Dif]`: obj 9, map 1
- `issue mtd missing from mtd.mtdbnd: DS_map[DifSpcLit]`: obj 6, map 3
- `issue texture only in the map's own TPF`: map 9
- `issue mtd missing from mtd.mtdbnd: DS_map[DifLit]`: obj 7
- `issue mtd missing from mtd.mtdbnd: DS_item[DifSpcBmp]`: parts 6
- `issue texture with empty path`: chr 1, obj 1, map 3
- `issue mtd missing from mtd.mtdbnd: DS_map_sky[Dcl]`: obj 1, map 4
- `issue mtd missing from mtd.mtdbnd: DS_map[DifSpc]`: obj 4
- `issue static mesh NormalW is local to the mesh bone list`: obj 4
- `issue mtd missing from mtd.mtdbnd: Cs_White_Ghost`: chr 3
- `issue mtd missing from mtd.mtdbnd: DS_character[DifSpcBmp_Skin]`: chr 1, obj 1
- `issue mtd missing from mtd.mtdbnd: Cs_Black_Ghost`: chr 2
- `issue mtd missing from mtd.mtdbnd: Cs_Wander_Ghost`: chr 2
- `issue mtd missing from mtd.mtdbnd: DS_map[Dif_Alp]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_character_Wander[DifSpcBmp_EdgeSkin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_character_WanderInv[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_character_Wander[DifSpcBmp_AlpSkin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_character_Ghost[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_parts[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_parts_Metal[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_parts_Lether[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_parts_Body[DifSpcBmp_Skin]`: chr 1
- `issue mtd missing from mtd.mtdbnd: DS_parts_Wet[DifSpcBmp_Skin]`: chr 1
- `issue static mesh needs node transform, renderer applies none`: obj 1
- `issue mtd missing from mtd.mtdbnd: A05[D][ML]`: map 1
- `issue mtd missing from mtd.mtdbnd: A05_4Stone[DSB][M]`: map 1
- `issue mtd missing from mtd.mtdbnd: A05[D]_Alp`: map 1
- `issue mtd missing from mtd.mtdbnd: DS_character_Ghost[DifSpcBmp]`: map 1

## missing tex

- `missing tex resolve path names container`: chr 62, obj 825, parts 572, map 33
- `missing tex -> map path, elsewhere in game`: obj 507, map 160
- `missing tex -> parts path, elsewhere in game`: chr 28, obj 37, parts 576
- `missing tex resolve elsewhere, path wrong`: chr 10, obj 383, parts 16, map 54
- `missing tex -> obj path, elsewhere in game`: chr 6, obj 419, map 28
- `missing tex -> chr path, elsewhere in game`: chr 38, obj 245, parts 12, map 8
- `missing tex resolve nowhere`: obj 29, parts 10, map 112
- `missing tex -> map path, nowhere`: obj 26, map 92
- `missing tex resolve in a map's own TPF`: map 110
- `missing tex -> obj path, nowhere`: obj 3, map 15
- `missing tex -> chr path, nowhere`: parts 6
- `missing tex -> other path, nowhere`: map 5
- `missing tex -> parts path, nowhere`: parts 4
- `missing tex -> other path, elsewhere in game`: map 1

## bind node

- `bind node non-identity dyn1 idx1 wgt1`: chr 717, obj 225, parts 934
- `bind node non-identity dyn0 idx1 wgt0`: chr 15, obj 806, parts 100, map 831
- `bind node non-identity dyn0 idx1 wgt1`: chr 23, obj 10, map 8
- `bind node non-identity dyn1 idx1 wgt0`: chr 8, obj 2, parts 20
- `bind node shared material models 25`: all 1

## read failure


## version

- `version 15`: all 2956
- `version 14`: all 237
- `version 12`: all 229
- `version F`: all 40
- `version 10`: all 32
- `version 13`: all 18
- `version E`: all 7

## dynamic

- `dynamic 0`: chr 67, obj 1861, parts 312, map 8498
- `dynamic 1`: chr 735, obj 246, parts 962

## normal

- `normal Color dyn0 idx1 wgt0`: chr 36, obj 1786, parts 302, map 8467
- `normal Color dyn1 idx1 wgt1`: chr 706, obj 226, parts 936
- `normal Color dyn0 idx1 wgt1`: chr 30, obj 26, parts 10, map 12
- `normal Float3 dyn0 idx1 wgt0`: chr 1, obj 47, map 14
- `normal Color dyn1 idx1 wgt0`: chr 8, obj 19, parts 20
- `normal Short4Norm dyn1 idx1 wgt1`: chr 21, obj 1
- `normal  dyn0 idx1 wgt0`: obj 1, map 5
- `normal Float3 dyn1 idx1 wgt0`: parts 6
- `normal Short4Norm dyn0 idx1 wgt1`: obj 1

## layout

- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color Tangent0:Color VertexColor0:Color UV0:Short4`: obj 160, map 3748
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color VertexColor0:Color UV0:Short4`: obj 45, map 2560
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color Tangent0:Color VertexColor0:Color UV0:Short2`: chr 38, obj 885, parts 296, map 341
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color VertexColor0:Color UV0:Short2`: chr 5, obj 551, parts 20, map 806
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Color Tangent0:Color VertexColor0:Color UV0:Short2`: chr 418, obj 139, parts 478, map 8
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color Tangent0:Color Bitangent0:Color VertexColor0:Color UV0:Short4 UV1:Short2`: obj 1, map 635
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Color VertexColor0:Color UV0:Short2`: chr 12, obj 30, parts 315
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color VertexColor0:Color UV0:Short4 UV1:Short2`: obj 6, map 219
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Color Tangent0:Color Bitangent0:Color VertexColor0:Color UV0:Short4`: obj 3, map 40
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Color Tangent0:Color VertexColor0:Color UV0:Short4`: chr 5, obj 22, map 3
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Float3 VertexColor0:Color UV0:Float2`: chr 1, obj 28, map 1
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Float3 VertexColor0:Color UV0:Float2 UV1:Float2`: obj 16, map 6
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Short4Norm Tangent0:Short4Norm VertexColor0:Color UV0:Float2`: chr 15, obj 1
- `layout Position0:Float3 BoneIndices0:Byte4E Normal0:Float3 Tangent0:Float4 VertexColor0:Color UV0:Float2`: parts 6, map 7
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Color Tangent0:Color Bitangent0:Color VertexColor0:Color UV0:Short4`: chr 6
- `layout Position0:Float3 BoneIndices0:Byte4E VertexColor0:Color UV0:Float2`: obj 1, map 5
- `layout Position0:Float3 BoneIndices0:Byte4E BoneWeights0:Short4Norm Normal0:Color Tangent0:Color Bitangent0:Color VertexColor0:Color UV0:Short4 UV1:Short2`: obj 3, map 1

## texparam

- `texparam g_Diffuse`: all 11735
- `texparam g_Lightmap`: all 7394
- `texparam g_Bumpmap`: all 7230
- `texparam g_Specular`: all 6323
- `texparam g_Diffuse_2`: all 1088
- `texparam g_Bumpmap_2`: all 689
- `texparam g_Specular_2`: all 637
- `texparam `: all 185
- `texparam g_Envmap`: all 65

## mtd

- `mtd M[D][L]`: obj 28, map 1519
- `mtd M_4Stone[DSB][L]`: obj 55, map 1325
- `mtd M[DB][L]`: obj 95, map 1191
- `mtd M[D]`: obj 438, map 455
- `mtd M_4Stone[DS][L]`: obj 3, map 484
- `mtd M_7Metal[DSB][L]`: obj 3, map 461
- `mtd M_4Stone[DSB][ML]`: obj 1, map 325
- `mtd M[DB]`: obj 218, map 70
- `mtd M_4Stone[DSB]`: chr 1, obj 170, parts 6, map 72
- `mtd M_1Wood[DSB][L]`: obj 4, map 236
- `mtd P_Metal[DSB]`: chr 10, obj 39, parts 178, map 5
- `mtd M[D]_Edge`: obj 25, map 183
- `mtd M[D][ML]`: obj 6, map 163
- `mtd M_7Metal[DSB][ML]`: map 146
- `mtd A04_blood[DS][L]_Alp`: map 139
- `mtd M_7Metal[DS][L]`: obj 4, map 134
- `mtd A03_4Stone[DSB][L]`: map 137
- `mtd Ps_Hair[DS]_Alp_Skin`: parts 126
- `mtd P_Metal[DSB]_Skin`: chr 4, parts 110
- `mtd C_Metal[DSB]_Skin`: chr 83, obj 24, parts 6
- `mtd M_1Wood[DSB][L]_Edge`: obj 1, map 112
- `mtd M_7Metal[DSB]`: obj 79, map 26
- `mtd M[D][L]_Edge`: obj 4, map 99
- `mtd M[DB][ML]`: map 100
- `mtd M[D][L]_Alp`: obj 1, map 98
- `mtd Ps_Body[DSB]_Skin`: chr 6, parts 92
- `mtd C_Leather[DSB]`: chr 3, obj 67, parts 20, map 1
- `mtd M_1Wood[DSB]`: obj 52, map 22
- `mtd M_7Metal[DSB][L]_Edge`: map 74
- `mtd Ps_Wander_Ghost_skin`: chr 1, parts 66
- `mtd C_Wet[DSB]_Skin`: chr 42, obj 21, map 3
- `mtd P_Leather[DSB]_Skin`: chr 6, parts 58
- `mtd C_Leather[DSB]_Skin`: chr 53, obj 9, map 1
- `mtd P_DullLeather[DSB]_Skin`: chr 2, parts 57
- `mtd Ps_Hair[D]_Alp_Skin`: parts 58
- `mtd A03_4Stone[DSB][ML]`: map 56
- `mtd M_4Stone[DS][ML]`: map 55
- `mtd P_DullLeather[DSB]_Edge_Skin`: chr 1, parts 50
- `mtd M_9Glass[DSB][L]`: map 49
- `mtd C_Leather[DSB]_Alp`: obj 47
- `mtd C_DullLeather[DSB]_Skin`: chr 35, obj 10
- `mtd C_Leather[DSB]_Edge_Skin`: chr 32, obj 12, map 1
- `mtd P_Leather[DSB]`: chr 5, obj 17, parts 20
- `mtd Ps_Wander_Ghost`: parts 42
- `mtd A07_Shadowman_shadow[Dn]Sub`: map 39
- `mtd M_4Stone[DSB][L]_Alp`: map 38
- `mtd M[DB][L]_Edge`: obj 1, map 33
- `mtd Ps_Hair[DS]_Skin`: parts 34
- `mtd C[Dn]_Skin`: chr 3, parts 29
- `mtd M_7Metal[DS]`: obj 30, map 2
- `mtd M_4Stone[DSB][M]`: obj 3, map 29
- `mtd Ps_Hair[DS]_Edge_Skin`: parts 32
- `mtd Ps_Hair[D]_Skin`: parts 30
- `mtd M[D]_Alp`: obj 14, map 14
- `mtd P_DullLeather[DSB]`: chr 1, obj 16, parts 10
- `mtd M_4Stone[DS]`: obj 25, map 2
- `mtd M[DB]_Edge`: obj 4, map 23
- `mtd C_DullLeather[DSB]_Edge_Skin`: chr 16, obj 4, parts 2, map 3
- `mtd DS_map[Dif]`: obj 24, map 1
- `mtd C_Wet[DSB]`: chr 1, obj 23
- `mtd A03_1Wood[DSB][L]`: map 24
- `mtd M[DB]_Skin`: obj 23
- `mtd C_Leather[DSB]_Edge`: chr 2, obj 20
- `mtd Cs_White_Ghost`: chr 21
- `mtd C_Metal[DSB]`: chr 6, obj 14
- `mtd P_DullLeather[DSB]_Edge`: obj 16, parts 4
- `mtd P_Metal[DSB]_Edge_Skin`: chr 2, parts 16
- `mtd A01[D]_Alp`: obj 4, map 14
- `mtd C_Metal[DSB]_Edge_Skin`: chr 16, obj 1
- `mtd P_Leather[DS]`: chr 1, parts 16
- `mtd C_Metal[DSB]_Alp_Skin`: chr 17
- `mtd C_Metal[DSB]_Alp`: obj 17
- `mtd P_Metal[DSB]_Edge`: obj 6, parts 10
- `mtd C_DullLeather[DSB]`: obj 16
- `mtd DS_map[DifSpcLit]`: obj 9, map 6
- `mtd M_7Metal[DSB]_Edge`: obj 5, map 10
- `mtd A03_[D][L]_Edge`: map 15
- `mtd M_7Metal[DSB][L]_Alp`: map 15
- `mtd Cs_Black_Ghost`: chr 14
- `mtd M_1Wood[DSB]_Edge`: obj 5, map 9

## Samples

### issue material shared by bind-pose and rigid meshes

- chr: c0100
- chr: c0200/c0200.flver
- chr: c0300/c0300.flver
- chr: c1080/c1080.flver
- chr: c1200/c1200.flver
- chr: c2030/c2030.flver
- chr: c2100/c2100.flver
- chr: c2101/c2101.flver
- chr: c3060/c3060.flver
- chr: c3140/c3140.flver
- chr: c7090/c7090.flver
- chr: c8020
- chr: c8100
- chr: c9950/c9950.flver
- chr: c9951/c9951.flver
- obj: o0000/o0000.flver
- obj: o2201/o2201.flver
- obj: o2999/o2999.flver
- obj: o4800/o4800.flver
- obj: o5001/o5001.flver
- obj: o9702/o9702.flver
- obj: o9993/o9993.flver
- obj: o9995/o9995.flver
- parts: wp_a_1390/WP_A_1390.flver
- parts: wp_a_1390_l/WP_A_1390.flver

### issue double sided mesh (renderer always culls)

- chr: c0100
- chr: c0200/c0200.flver
- chr: c0300/c0300.flver
- chr: c1090/c1090.flver
- chr: c1200/c1200.flver
- chr: c2000/c2000.flver
- chr: c2005/c2005.flver
- chr: c2010/c2010.flver
- chr: c2020/c2020.flver
- chr: c2030/c2030.flver
- chr: c2040/c2040.flver
- chr: c2050/c2050.flver
- chr: c2060/c2060.flver
- chr: c2070/c2070.flver
- chr: c2075/c2075.flver
- chr: c2080/c2080.flver
- chr: c2090/c2090.flver
- chr: c2100/c2100.flver
- chr: c2101/c2101.flver
- chr: c2110/c2110.flver
- chr: c2140/c2140.flver
- chr: c3010/c3010.flver
- chr: c3011
- chr: c3020/c3020.flver
- chr: c3030/c3030.flver
- chr: c3040/c3040.flver
- chr: c3060/c3060.flver
- chr: c3080/c3080.flver
- chr: c3090/c3090.flver
- chr: c3132/c3132.flver
- chr: c3180/c3180.flver
- chr: c4000/c4000.flver
- chr: c4010/c4010.flver
- chr: c4020/c4020.flver
- chr: c4021/c4021.flver
- chr: c4022/c4022.flver
- chr: c4030/c4030.flver
- chr: c4040/c4040.flver
- chr: c4050/c4050.flver
- chr: c4060/c4060.flver

### issue static weighted mesh drawn without its node transform

- chr: c0100
- chr: c0200/c0200.flver
- chr: c0300/c0300.flver
- chr: c1080/c1080.flver
- chr: c1200/c1200.flver
- chr: c2030/c2030.flver
- chr: c2100/c2100.flver
- chr: c2101/c2101.flver
- chr: c3060/c3060.flver
- chr: c3140/c3140.flver
- chr: c7090/c7090.flver
- chr: c8020
- chr: c8100
- chr: c9950/c9950.flver
- chr: c9951/c9951.flver
- obj: o1402/o1402.flver
- obj: o2201/o2201.flver
- obj: o2455/o2455.flver
- obj: o2999/o2999.flver
- obj: o4800/o4800.flver
- obj: o5001/o5001.flver
- obj: o5889/o5889.flver
- map: m04_01_00_00/m3107b1.flver
- map: m06_00_00_00/m0015b0.flver
- map: m06_00_00_00/m0103b0.flver

### missing texture (chr)

- c1020/c1020.flver: g_Diffuse N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01.tga
- c1020/c1020.flver: g_Specular N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_s.tga
- c1020/c1020.flver: g_Bumpmap N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_n.tga
- c2005/c2005.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\ghost_64x256_1.tga
- c2005/c2005.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_64x256_2.tga
- c2005/c2005.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c2005/c2005.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\ghost_256x256_2.tga
- c2011/c2011.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield.tga
- c2011/c2011.flver: g_Specular N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_s.tga
- c2011/c2011.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_n.tga
- c2011/c2011.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword.tga
- c2011/c2011.flver: g_Specular N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_s.tga
- c2011/c2011.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_n.tga
- c2095/c2095.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2095/c2095.flver: g_Bumpmap N:\DemonsSoul\data\Model\chr\c7130\tex\c7130_body_n.tga
- c2095/c2095.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c5002: g_Diffuse N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5002: g_Specular N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5002: g_Bumpmap N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: g_Diffuse N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: g_Specular N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: g_Bumpmap N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: g_Diffuse_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: g_Specular_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: g_Bumpmap_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: g_Diffuse N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: g_Specular N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: g_Bumpmap N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: g_Diffuse N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: g_Specular N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: g_Bumpmap N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: g_Diffuse_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000.tga
- c5009: g_Specular_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_s.tga
- c5009: g_Bumpmap_2 N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_n.tga
- c5009: g_Diffuse N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2.tga
- c5009: g_Specular N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2_s.tga

### issue bind-pose mesh without weights moved by the index-no-weight path

- chr: c1020/c1020.flver
- chr: c2010/c2010.flver
- chr: c2050/c2050.flver
- chr: c2070/c2070.flver
- chr: c2101/c2101.flver
- chr: c4000/c4000.flver
- obj: o4040/o4040.flver
- parts: hd_f_0000/HD_F_0000.flver
- parts: hd_f_0000_l/HD_F_0000.flver
- parts: hd_f_9000/HD_F_0000.flver
- parts: hd_f_9000_l/HD_F_0000.flver
- parts: hd_f_9010/HD_F_0000.flver
- parts: hd_f_9010_l/HD_F_0000.flver
- parts: hd_m_0000/HD_M_0000.flver
- parts: hd_m_0000_l/HD_M_0000.flver
- parts: hd_m_9000/HD_M_0000.flver
- parts: hd_m_9000_l/HD_M_0000.flver
- parts: hd_m_9010/HD_M_0000.flver
- parts: hd_m_9010_l/HD_M_0000.flver
- parts: hr_f_0008/HR_F_0008.flver
- parts: hr_f_0008_l/HR_F_0008.flver
- parts: hr_f_9000/HR_F_9000.flver
- parts: hr_f_9000_l/HR_F_9000.flver
- parts: hr_m_0009/HR_M_0009.flver
- parts: hr_m_0009_l/HR_M_0009.flver
- parts: hr_m_9000/HR_M_9000.flver
- parts: hr_m_9000_l/HR_M_9000.flver

### issue texture with empty path

- chr: c6032/c6032.flver
- obj: o6431/o6431.flver
- map: m03_01_00_99/m2010b1.flver
- map: m07_01_00_00/m07_0.flver
- map: m08_01_00_00/m9600b1.flver

### issue index-no-weight path uses local, not world, node transform

- chr: c7050/c7050.flver
- obj: o1100/o1100.flver
- obj: o2322/o2322.flver
- obj: o4400/o4400.flver
- obj: o4800/o4800.flver
- obj: o5170/o5170.flver
- obj: o6110/o6110.flver
- obj: o6111/o6111.flver
- obj: o6201/o6201.flver
- obj: o6203/o6203.flver
- obj: o6204/o6204.flver
- obj: o6701/o6701.flver
- map: m02_02_00_00/m4010b2.flver
- map: m02_02_00_00/m4020b2.flver
- map: m02_02_00_00/m4040b2.flver
- map: m02_02_00_00/m4060b2.flver

### issue static rigid mesh placed by local node transform, needs world

- chr: c7050/c7050.flver
- obj: o0205/o0205.flver
- obj: o0206/o0206.flver
- obj: o0207/o0207.flver
- obj: o1100/o1100.flver
- obj: o2190/o2190.flver
- obj: o2322/o2322.flver
- obj: o4070/o4070.flver
- obj: o4090/o4090.flver
- obj: o4110/o4110.flver
- obj: o4305/o4305.flver
- obj: o4306/o4306.flver
- obj: o4400/o4400.flver
- obj: o4800/o4800.flver
- obj: o4999/o4999.flver
- obj: o5170/o5170.flver
- obj: o6110/o6110.flver
- obj: o6111/o6111.flver
- obj: o6130/o6130.flver
- obj: o6140/o6140.flver
- obj: o6201/o6201.flver
- obj: o6203/o6203.flver
- obj: o6204/o6204.flver
- obj: o6701/o6701.flver
- obj: o9991/o9991.flver
- obj: o9992/o9992.flver
- obj: o9994/o9994.flver
- map: m02_02_00_00/m4010b2.flver
- map: m02_02_00_00/m4020b2.flver
- map: m02_02_00_00/m4040b2.flver
- map: m02_02_00_00/m4060b2.flver

### issue mtd missing from mtd.mtdbnd: DS_map[Dif_Alp]

- chr: c9500/c9500.flver

### issue mtd missing from mtd.mtdbnd: DS_character[DifSpcBmp_Skin]

- chr: c9501/c9501.flver
- obj: o9993/o9993.flver

### issue mtd missing from mtd.mtdbnd: Cs_White_Ghost

- chr: c9900/c9900.flver
- chr: c9995
- chr: c9997

### issue mtd missing from mtd.mtdbnd: Cs_Black_Ghost

- chr: c9901/c9901.flver
- chr: c9996

### issue mtd missing from mtd.mtdbnd: Cs_Wander_Ghost

- chr: c9980/c9980.flver
- chr: c9982

### issue mtd missing from mtd.mtdbnd: DS_character_Wander[DifSpcBmp_EdgeSkin]

- chr: c9981

### issue mtd missing from mtd.mtdbnd: DS_character_WanderInv[DifSpcBmp_Skin]

- chr: c9981

### issue mtd missing from mtd.mtdbnd: DS_character_Wander[DifSpcBmp_AlpSkin]

- chr: c9981

### issue mtd missing from mtd.mtdbnd: DS_character_Ghost[DifSpcBmp_Skin]

- chr: c9983

### issue mtd missing from mtd.mtdbnd: DS_parts[DifSpcBmp_Skin]

- chr: c9998/c9998.flver

### issue mtd missing from mtd.mtdbnd: DS_parts_Metal[DifSpcBmp_Skin]

- chr: c9999/c9999.flver

### issue mtd missing from mtd.mtdbnd: DS_parts_Lether[DifSpcBmp_Skin]

- chr: c9999/c9999.flver

### issue mtd missing from mtd.mtdbnd: DS_parts_Body[DifSpcBmp_Skin]

- chr: c9999/c9999.flver

### issue mtd missing from mtd.mtdbnd: DS_parts_Wet[DifSpcBmp_Skin]

- chr: c9999/c9999.flver

### missing texture (obj)

- o0050/o0050.flver: g_Diffuse N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body.tga
- o0050/o0050.flver: g_Specular N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body_s.tga
- o0050/o0050.flver: g_Bumpmap N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body_n.tga
- o0050/o0050.flver: g_Diffuse N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth.tga
- o0050/o0050.flver: g_Specular N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth_s.tga
- o0050/o0050.flver: g_Bumpmap N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth_n.tga
- o0141/o0141_1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Barrel.tga
- o0220/o0220.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0220/o0220.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0011.tga
- o0221/o0221.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0221/o0221.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0221/o0221.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0223/o0223.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_pillar_stone_00.tga
- o0223/o0223.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_pillar_stone_00_s.tga
- o0223/o0223.flver:  \\Server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m4700.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02_s.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0230/o0230.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02_s.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0231/o0231.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0240/o0240.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0240/o0240.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0241/o0241.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0241/o0241.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0242/o0242.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0242/o0242.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0244/o0244.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_01.tga
- o0244/o0244.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_01_s.tga
- o0244/o0244.flver:  \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m4300.tga
- o0258/o0258.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram.tga
- o0258/o0258.flver: g_Specular N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram_s.tga
- o0258/o0258.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram_n.tga
- o0504/o0504.flver: g_Diffuse N:\DemonsSoul\data\Model\obj\o4080\tex\o4080_Ballista.tga

### issue mtd missing from mtd.mtdbnd: DS_map[DifSpc]

- obj: o0104/o0104.flver
- obj: o0110/o0110.flver
- obj: o0130/o0130.flver
- obj: o0131/o0131.flver

### issue mtd missing from mtd.mtdbnd: DS_map[Dif]

- obj: o0107/o0107.flver
- obj: o0108/o0108.flver
- obj: o0109/o0109.flver
- obj: o0124/o0124.flver
- obj: o0125/o0125.flver
- obj: o9980/o9980.flver
- obj: o9981/o9981.flver
- obj: o9990/o9990.flver
- obj: o9992/o9992.flver
- map: m99_99_99_99/o0010.flver

### issue mtd missing from mtd.mtdbnd: DS_map[DifLit]

- obj: o0220/o0220.flver
- obj: o0240/o0240.flver
- obj: o0241/o0241.flver
- obj: o0242/o0242.flver
- obj: o9988/o9988.flver
- obj: o9989/o9989.flver
- obj: o9998/o9998.flver

### issue mtd missing from mtd.mtdbnd: DS_map[DifSpcLit]

- obj: o0221/o0221.flver
- obj: o0223/o0223.flver
- obj: o0230/o0230.flver
- obj: o0231/o0231.flver
- obj: o0244/o0244.flver
- obj: o9997/o9997.flver
- map: m99_99_98_04/m0000.flver
- map: m99_99_98_07/m0000.flver
- map: m99_99_98_09/m0000.flver

### issue static mesh NormalW is local to the mesh bone list

- obj: o0230/o0230.flver
- obj: o0231/o0231.flver
- obj: o9989/o9989.flver
- obj: o9997/o9997.flver

### issue static mesh needs node transform, renderer applies none

- obj: o4800/o4800.flver

### issue mtd missing from mtd.mtdbnd: DS_map_sky[Dcl]

- obj: o9996/o9996.flver
- map: m99_99_98_01/m9900.flver
- map: m99_99_98_04/m9900.flver
- map: m99_99_98_07/m9900.flver
- map: m99_99_98_09/m9900.flver

### missing texture (parts)

- am_f_0000/AM_F_0000.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_0000/AM_F_0000.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_0000/AM_F_0000.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_0000_l/AM_F_0000_L.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_0000_l/AM_F_0000_L.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_0000_l/AM_F_0000_L.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_5900/AM_F_5900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_5900/AM_F_5900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_5900_l/AM_F_5900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_5900_l/AM_F_5900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_6010/AM_F_6010.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010.tga
- am_f_6010/AM_F_6010.flver: g_Specular N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010_s.tga
- am_f_6010/AM_F_6010.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010_n.tga
- am_f_6010_l/AM_F_6010_L.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010.tga
- am_f_6010_l/AM_F_6010_L.flver: g_Specular N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010_s.tga
- am_f_6010_l/AM_F_6010_L.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\Body\BD_F_6010\tex\BD_F_6010_n.tga
- am_f_6900/AM_F_6900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_6900/AM_F_6900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_6900_l/AM_F_6900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_6900_l/AM_F_6900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_7100/AM_F_7100.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_7100/AM_F_7100.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_7100/AM_F_7100.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_7100_l/AM_F_7100_L.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_7100_l/AM_F_7100_L.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_7100_l/AM_F_7100_L.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_7900/AM_F_7900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_7900/AM_F_7900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_7900_l/AM_F_7900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_7900_l/AM_F_7900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_8100/AM_F_8100.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_8100/AM_F_8100.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_8100/AM_F_8100.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_8100_l/AM_F_8100_L.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\BD_F_body.tga
- am_f_8100_l/AM_F_8100_L.flver: g_Specular N:\DemonsSoul\data\Model\parts\tex\BD_F_body_s.tga
- am_f_8100_l/AM_F_8100_L.flver: g_Bumpmap N:\DemonsSoul\data\Model\parts\tex\BD_F_body_n.tga
- am_f_8900/AM_F_8900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_8900/AM_F_8900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- am_f_8900_l/AM_F_8900.flver: g_Diffuse N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- am_f_8900_l/AM_F_8900.flver: g_Diffuse_2 N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga

### issue mtd missing from mtd.mtdbnd: DS_item[DifSpcBmp]

- parts: as_a_0000/AS_A_0000.flver
- parts: as_a_0000_l/AS_A_0000.flver
- parts: as_a_0010/AS_A_0010.flver
- parts: as_a_0010_l/AS_A_0010.flver
- parts: as_a_0020/AS_A_0020.flver
- parts: as_a_0020_l/AS_A_0020.flver

### missing texture (map)

- m02_00_00_00/m2020b0l1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2020b0l1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2030b0l1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2030b0l1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2230b0.flver: g_Diffuse N:\DemonsSoul\data\Model\obj\o2121\tex\m02_obj_ironball.tga
- m02_00_00_00/m2230b0.flver: g_Bumpmap N:\DemonsSoul\data\Model\obj\o2121\tex\m02_obj_ironball_n.tga
- m02_00_00_00/m2250b0.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_building_wall_window.tga
- m02_00_00_00/m2310b0l1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2310b0l1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2613b0.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_building_wall.tga
- m02_00_00_00/m4020b0.flver: g_Diffuse N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body.tga
- m02_00_00_00/m4020b0.flver: g_Specular N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body_s.tga
- m02_00_00_00/m4020b0.flver: g_Bumpmap N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body_n.tga
- m02_00_00_00/m4020b0.flver: g_Diffuse N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth.tga
- m02_00_00_00/m4020b0.flver: g_Specular N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth_s.tga
- m02_00_00_00/m4020b0.flver: g_Bumpmap N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_cloth_n.tga
- m02_01_00_00/m0001b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0002b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0002b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_lava.tga
- m02_01_00_00/m0003b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_2.psd
- m02_01_00_00/m0004b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0004b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_00_00_wood.tga
- m02_01_00_00/m0005b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0005b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_lava.tga
- m02_01_00_00/m0005b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_00_00_wood.tga
- m02_01_00_00/m0009b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0011b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0011b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_lava.tga
- m02_01_00_00/m0011b1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m99_99_10_water_n.tga
- m02_01_00_00/m0011b1.flver: g_Envmap N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m99_99_10_env_01.tga
- m02_01_00_00/m0012b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0013b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_00_00_wood.tga
- m02_01_00_00/m2020b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Firewood.tga
- m02_01_00_00/m2020b1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Firewood_n.tga
- m02_01_00_00/m2020b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Tub.tga
- m02_01_00_00/m2020b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Cart_00.tga
- m02_01_00_00/m2020b1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Cart_00_n.tga
- m02_01_00_00/m2230b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Tub.tga
- m02_01_00_00/m5100b1.flver: g_Diffuse N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Firewood.tga
- m02_01_00_00/m5100b1.flver: g_Bumpmap N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Firewood_n.tga

### issue texture only in the map's own TPF

- map: m03_00_00_00/layou.flver
- map: m03_00_00_00/m0000b0.flver
- map: m03_00_00_00/m0000b1.flver
- map: m03_00_00_00/m0000b2.flver
- map: m03_00_00_00/m0001b4.flver
- map: m03_00_00_00/m9998b4.flver
- map: m03_03_00_00/m0001b4.flver
- map: m03_03_00_00/m9998b4.flver
- map: m03_03_00_00/m9999b0.flver

### issue mtd missing from mtd.mtdbnd: A05[D][ML]

- map: m07_01_00_00/m9000b1.flver

### issue mtd missing from mtd.mtdbnd: A05_4Stone[DSB][M]

- map: m07_01_00_00/m9000b1.flver

### issue mtd missing from mtd.mtdbnd: A05[D]_Alp

- map: m07_01_00_00/m9000b1.flver

### issue mtd missing from mtd.mtdbnd: DS_character_Ghost[DifSpcBmp]

- map: m99_99_11_00/m9999b1.flver

### missing tex -> obj path, elsewhere in game

- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01.tga
- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_s.tga
- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_s.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_n.tga
- o0504/o0504.flver: N:\DemonsSoul\data\Model\obj\o4080\tex\o4080_Ballista.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2760\tex\o2760_body_n.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750_weapons.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2760\tex\o2760_weapon_n.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750_plinth.tga
- o2761/o2761.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750_plinth_n.tga
- o2761/o2761_1.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750.tga
- o2761/o2761_1.flver: N:\DemonsSoul\data\Model\obj\o2760\tex\o2760_body_n.tga
- o2761/o2761_1.flver: N:\DemonsSoul\data\Model\obj\o2750\tex\o2750_plinth.tga

### missing tex resolve path names container

- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01.tga
- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_s.tga
- c1020/c1020.flver: N:\DemonsSoul\data\Model\obj\o0000\tex\o0000_01_n.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_64x256_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_64x256_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_256x256_2.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_s.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_n.tga
- c2095/c2095.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2095/c2095.flver: N:\DemonsSoul\data\Model\chr\c7130\tex\c7130_body_n.tga
- c2095/c2095.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_s.tga

### missing tex -> parts path, elsewhere in game

- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_64x256_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_64x256_2.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c2005/c2005.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_256x256_2.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_s.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1509\tex\WP_A_1509_shield_n.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_s.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_n.tga
- c2095/c2095.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c2095/c2095.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c7105/c7105.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c7105/c7105.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c7105/c7105.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c7105/c7105.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- c7115/c7115.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c7115/c7115.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c7115/c7115.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c7115/c7115.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_256x256_2.tga
- c7135/c7135.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_512x512_1.tga
- c7135/c7135.flver: N:\DemonsSoul\data\Model\parts\tex\ghost_512x512_2.tga
- c7135/c7135.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_1.tga
- c7135/c7135.flver: N:\DemonsSoul\data\Model\parts\tex\Ghost_256x256_2.tga
- o0258/o0258.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram.tga
- o0258/o0258.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram_s.tga
- o0258/o0258.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0250\tex\WP_A_0250_gram_n.tga
- o1504/o1504.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1503\tex\WP_A_1503_kiteshield.tga
- o1504/o1504.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1503\tex\WP_A_1503_kiteshield_s.tga
- o1504/o1504.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_1503\tex\WP_A_1503_kiteshield_n.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_face.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_face_s.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_face_n.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_eye.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_eye_s.tga
- o3998/o3998.flver: N:\DemonsSoul\data\Model\parts\tex\FC_M_eye_n.tga

### missing tex resolve elsewhere, path wrong

- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_s.tga
- c2011/c2011.flver: N:\DemonsSoul\data\Model\parts\Weapon\WP_A_0211\tex\WP_A_0211_sword_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_s.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_s.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_n.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1000/o1000.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1010/o1010.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1020/o1020.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1030/o1030.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1040/o1040.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_keystone.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_s.tga
- o1050/o1050.flver: N:\DemonsSoul\data\Model\obj\o1000\tex\o1000_KeyStone_n.tga
- o1605/o1605.flver: N:\DemonsSoul\data\Model\map\m01\tex\m01_etc_03.tga
- o1605/o1605_1.flver: N:\DemonsSoul\data\Model\map\m01\tex\m01_etc_03.tga
- o3120/o3120.flver: N:\DemonsSoul\data\Model\obj\o3120\tex\m03_wall_oi2.tga
- o3120/o3120.flver: N:\DemonsSoul\data\Model\obj\o3120\tex\m03_wall_oi2_s.tga
- o3120/o3120.flver: N:\DemonsSoul\data\Model\obj\o3120\tex\m03_wall_oi2_n.tga
- o3120/o3120.flver: N:\DemonsSoul\data\Model\obj\o3120\tex\m03_wall_oi4.tga

### missing tex -> chr path, elsewhere in game

- c2095/c2095.flver: N:\DemonsSoul\data\Model\chr\c7130\tex\c7130_body_n.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5002: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_slime2_n.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_s.tga
- c5009: N:\DemonsSoul\data\Model\chr\c5000\tex\c5000_n.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_011.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_011_s.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_011_n.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_05.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_05_s.tga
- c6031/c6031.flver: N:\DemonsSoul\data\Model\chr\c6030\tex\c6030_05_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_s.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_n.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_s.tga
- c6050/c6050.flver: N:\DemonsSoul\data\Model\chr\c6050\tex\c1020_dragondemon_face_bg_n.tga
- o0050/o0050.flver: N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body.tga
- o0050/o0050.flver: N:\DemonsSoul\data\Model\chr\c2000\tex\c2000_body_s.tga

### missing tex -> map path, elsewhere in game

- o0141/o0141_1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_obj_Barrel.tga
- o0220/o0220.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0220/o0220.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0011.tga
- o0221/o0221.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0221/o0221.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0221/o0221.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0223/o0223.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_pillar_stone_00.tga
- o0223/o0223.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_pillar_stone_00_s.tga
- o0223/o0223.flver: \\Server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m4700.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02_s.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0230/o0230.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_02_s.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_gnd_stone_00_s.tga
- o0231/o0231.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_00.tga
- o0240/o0240.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0240/o0240.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0241/o0241.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0241/o0241.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0242/o0242.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_etc_wood_01.tga
- o0242/o0242.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m0010.tga
- o0244/o0244.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_01.tga
- o0244/o0244.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_wall_stone_01_s.tga
- o0244/o0244.flver: \\server-dev4\DS\data\Model\map\m99_99_11_00\tex\m99_99_11_lightmap_m4300.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_gate_wall.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_gate_wall_n.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_lit_B3m2000.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_arch_wall_01.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_arch_wall_01_n.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_lit_B3m2000.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_arch_wall_02.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_arch_wall_02_n.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_lit_B3m2000.tga
- o0700/o0700.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_building_wall_01.tga

### missing tex -> map path, nowhere

- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00.tga
- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_chair_01.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_chair_01.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o3104/o3104.flver: N:\DemonsSoul\data\Model\map\m03\tex\m03_env_01.tga
- o8511/o8511.flver: N:\DemonsSoul\data\Model\map\m03\tex\m03_02_Wall_Cliff_00.tga
- o9010/o9010.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9120/o9120.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9121/o9121.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9122/o9122.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9123/o9123.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9124/o9124.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9125/o9125.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9140/o9140.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9141/o9141.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9150/o9150.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\o9150_01.tga
- o9150/o9150.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\o9150_01_s.tga
- o9160/o9160.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- m02_00_00_00/m2020b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2020b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2030b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2030b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2250b0.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_building_wall_window.tga
- m02_00_00_00/m2310b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga
- m02_00_00_00/m2310b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain_n.tga
- m02_00_00_00/m2613b0.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_building_wall.tga
- m02_01_00_00/m0001b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0002b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0002b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_lava.tga
- m02_01_00_00/m0003b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_gnd_2.psd
- m02_01_00_00/m0004b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_01_00_10.tga
- m02_01_00_00/m0004b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m02_00_00_wood.tga

### missing tex resolve nowhere

- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00.tga
- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2427/o2427.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2427/o2427_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_chair_01.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2431/o2431.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_chair_01.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_s.tga
- o2431/o2431_1.flver: N:\DemonsSoul\data\Model\map\小物データ\小物tex\m00_Chair_00_n.tga
- o3104/o3104.flver: N:\DemonsSoul\data\Model\map\m03\tex\m03_env_01.tga
- o8511/o8511.flver: N:\DemonsSoul\data\Model\map\m03\tex\m03_02_Wall_Cliff_00.tga
- o9010/o9010.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9120/o9120.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9121/o9121.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9122/o9122.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9123/o9123.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9124/o9124.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9125/o9125.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9140/o9140.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m1000_01.tga
- o9141/o9141.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9150/o9150.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\o9150_01.tga
- o9150/o9150.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\o9150_01_s.tga
- o9160/o9160.flver: \\Server-dev4\DS\data\Model\map\m99_99_00_00\tex\m0000_01.tga
- o9991/o9991.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- o9992/o9992.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- o9994/o9994.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- as_a_0000/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_s.tga
- as_a_0000/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_n.tga
- as_a_0000_l/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_s.tga
- as_a_0000_l/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_n.tga
- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150.tga
- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_s.tga
- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_n.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_s.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_n.tga
- m02_00_00_00/m2020b0l1.flver: N:\DemonsSoul\data\Model\map\m02\tex\m02_tower_wall_plain.tga

### missing tex -> obj path, nowhere

- o9991/o9991.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- o9992/o9992.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- o9994/o9994.flver: \\server-dev4\DS\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4010b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4010b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4030b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4030b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4050b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4060b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4060b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4070b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4070b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4080b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4080b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4090b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga
- m02_02_00_00/m4090b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4100b2.flver: N:\DemonsSoul\data\Model\obj\o0106\tex\m99_99_12_box.tga
- m02_02_00_00/m4100b2.flver: N:\DemonsSoul\data\Model\obj\o0105\tex\m99_99_12_barrel.tga

### missing tex -> parts path, nowhere

- as_a_0000/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_s.tga
- as_a_0000/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_n.tga
- as_a_0000_l/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_s.tga
- as_a_0000_l/AS_A_0000.flver: N:\DemonsSoul\data\Model\parts\Accessories\AS_A_0000\tex\i7007_arrow_n.tga

### missing tex -> chr path, nowhere

- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150.tga
- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_s.tga
- hr_f_0006/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_n.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_s.tga
- hr_f_0006_l/HR_F_0006.flver: N:\DemonsSoul\data\Model\chr\c7150\tex\c7150_n.tga

### missing tex resolve in a map's own TPF

- m02_01_00_00/m0011b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m99_99_10_water_n.tga
- m02_01_00_00/m0011b1.flver: N:\DemonsSoul\data\Model\map\m02_01_00_00\tex\m99_99_10_env_01.tga
- m03_01_00_99/m0001b4.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_water_n.tga
- m03_01_00_99/m0001b4.flver: N:\DemonsSoul\data\Model\map\m03_00_00_00\tex\m99_99_10_env_01.tga
- m03_01_00_99/m0003b1.flver: N:\DemonsSoul\data\Model\map\m03\tex\m99_99_10_tree_01.tga
- m03_01_00_99/m0006b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0007b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0007b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0008b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0008b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_ground.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m05_01_01_etc_wood.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_watergate.tga
- m03_01_00_99/m0009b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0010b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0010b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_tree_01.tga
- m03_01_00_99/m0011b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0011b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0011b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_ground.tga
- m03_01_00_99/m0011b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m05_01_01_etc_wood.tga
- m03_01_00_99/m0012b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0012b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_ground.tga
- m03_01_00_99/m0012b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0013b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0014b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0015b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0015b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0015b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0015b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_ground.tga
- m03_01_00_99/m0015b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m05_01_01_etc_wood.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_rock_02.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_broken_wall.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_temple_floor_01.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_tree_01.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_bridge_base_02.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_grass.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m99_99_10_ground.tga
- m03_01_00_99/m0016b1.flver: N:\DemonsSoul\data\Model\map\m03_01_00_00\tex\m05_01_01_etc_wood.tga

### missing tex -> other path, nowhere

- m03_01_00_99/m0026b1.flver: C:\Demon_works\m0026B1\tex\m03_lightmap.tga
- m03_01_00_99/m0026b1.flver: C:\Demon_works\m0026B1\tex\m03_lightmap.tga
- m03_01_00_99/m0026b1.flver: C:\Demon_works\m0026B1\tex\m03_lightmap.tga
- m03_01_00_99/m0026b1.flver: C:\Demon_works\m0026B1\tex\m03_lightmap.tga
- m07_01_00_00/m07_0.flver: C:\Demon_works\m07_00_00\tex\m07_01wall_out.tga

### missing tex -> other path, elsewhere in game

- m07_01_00_00/m07_0.flver: C:\Demon_works\m07_00_00\tex\m07_00_00_block1.tga


## Read failures

