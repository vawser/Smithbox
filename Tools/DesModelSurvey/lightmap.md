# DES lightmap survey

## Lightmapped MTD texture/UV signatures

- 26: `g_Diffuse:uv1 g_Specular:uv1 g_Bumpmap:uv1 g_Lightmap:uv3`
- 13: `g_Diffuse:uv1 g_Specular:uv1 g_Bumpmap:uv1 g_Diffuse_2:uv2 g_Specular_2:uv2 g_Bumpmap_2:uv2 g_Lightmap:uv3`
- 13: `g_Diffuse:uv1 g_Lightmap:uv3`
- 9: `g_Diffuse:uv1 g_Specular:uv1 g_Lightmap:uv3`
- 5: `g_Diffuse:uv1 g_Bumpmap:uv1 g_Lightmap:uv3`
- 5: `g_Diffuse:uv1 g_Specular:uv1 g_Diffuse_2:uv2 g_Specular_2:uv2 g_Lightmap:uv3`
- 2: `g_Diffuse:uv1 g_Diffuse_2:uv2 g_Lightmap:uv3`
- 1: `g_Diffuse:uv1 g_Bumpmap:uv1 g_Diffuse_2:uv2 g_Bumpmap_2:uv2 g_Lightmap:uv3`

## Their shaders

- 20: `DS_Phn_ColDifSpcBmpLit.spx`
- 13: `DS_Phn_ColDifLit.spx`
- 12: `DS_Phn_ColDifSpcBmpMulLit.spx`
- 9: `DS_Phn_ColDifSpcLit.spx`
- 5: `DS_Phn_ColDifSpcBmpLit_Skin.spx`
- 5: `DS_Phn_ColDifBmpLit.spx`
- 5: `DS_Phn_ColDifSpcMulLit.spx`
- 2: `DS_Phn_ColDifMulLit.spx`
- 1: `DS_Phn_ColDifSpcBmpMulLit_Skin.spx`
- 1: `DS_Phn_ColDifBmpMulLit.spx`
- 1: `DS_Phn_ColDifSpcBmpLit_skin.spx`

## Their params

- 74: `g_DiffuseMapColor = 1,1,1`
- 74: `g_TexScroll_1 = 0,0`
- 74: `g_IsNew = True`
- 73: `g_SpecularMapColor = 1,1,1`
- 72: `g_TexScroll_0 = 0,0`
- 70: `g_TexScroll_2 = 0,0`
- 68: `g_TexScrollType = 0`
- 67: `g_DiffuseMapColorPower = 0.6`
- 64: `g_LightingType = 3`
- 43: `g_BlendMode = 0`
- 35: `g_SpecularMapColorPower = 1`
- 35: `g_EnvSpcSlotNo = 0`
- 33: `g_SpecularPower = 4`
- 23: `g_SpecularMapColorPower = 2`
- 22: `g_FaceGenType = 0`
- 17: `g_BlendMode = 2`
- 15: `g_SpecularPower = 2`
- 13: `g_BlendMode = 1`
- 13: `g_SpecularMapColorPower = 1.5`
- 13: `g_EnvSpcSlotNo = 1`
- 13: `g_EnvSpcSlotNo = 2`
- 12: `g_SpecularPower = 60`
- 12: `g_EnvSpcSlotNo = 3`
- 11: `g_SpecularPower = 7`
- 6: `g_LightingType = 1`
- 6: `g_TexScrollType = 1`
- 5: `g_DiffuseMapColorPower = 0.5`
- 4: `g_ShadowPowMul = 0.8`
- 4: `g_useHairColor = 0`
- 4: `g_LightingType = 0`
- 4: `g_TexScroll_2 = -0.01,-0.005`
- 2: `g_DiffuseMapColorPower = 0.7`
- 2: `g_SpecularMapColorPower = 30`
- 2: `g_SpecularPower = 128`
- 1: `g_SpecularMapColor = 0.961,0.922,0.922`
- 1: `g_SpecularMapColorPower = 2.5`
- 1: `g_SpecularPower = 70`
- 1: `g_BlendMode = 5`
- 1: `g_TexScroll_0 = 0.05,0.15`
- 1: `g_TexScroll_0 = 0,0.03`

## Lightmapped map meshes by UV count

- 1525: `uvs 2, diffuse_2 0, mtd M[D][L]`
- 1327: `uvs 2, diffuse_2 0, mtd M_4Stone[DSB][L]`
- 1202: `uvs 2, diffuse_2 0, mtd M[DB][L]`
- 484: `uvs 2, diffuse_2 0, mtd M_4Stone[DS][L]`
- 461: `uvs 2, diffuse_2 0, mtd M_7Metal[DSB][L]`
- 325: `uvs 3, diffuse_2 1, mtd M_4Stone[DSB][ML]`
- 237: `uvs 2, diffuse_2 0, mtd M_1Wood[DSB][L]`
- 163: `uvs 3, diffuse_2 1, mtd M[D][ML]`
- 146: `uvs 3, diffuse_2 1, mtd M_7Metal[DSB][ML]`
- 139: `uvs 2, diffuse_2 0, mtd A04_blood[DS][L]_Alp`
- 137: `uvs 2, diffuse_2 0, mtd A03_4Stone[DSB][L]`
- 134: `uvs 2, diffuse_2 0, mtd M_7Metal[DS][L]`
- 112: `uvs 2, diffuse_2 0, mtd M_1Wood[DSB][L]_Edge`
- 101: `uvs 3, diffuse_2 1, mtd M[DB][ML]`
- 99: `uvs 2, diffuse_2 0, mtd M[D][L]_Edge`
- 98: `uvs 2, diffuse_2 0, mtd M[D][L]_Alp`
- 74: `uvs 2, diffuse_2 0, mtd M_7Metal[DSB][L]_Edge`
- 56: `uvs 3, diffuse_2 1, mtd A03_4Stone[DSB][ML]`
- 55: `uvs 3, diffuse_2 1, mtd M_4Stone[DS][ML]`
- 49: `uvs 2, diffuse_2 0, mtd M_9Glass[DSB][L]`
- 38: `uvs 2, diffuse_2 0, mtd M_4Stone[DSB][L]_Alp`
- 33: `uvs 2, diffuse_2 0, mtd M[DB][L]_Edge`
- 24: `uvs 2, diffuse_2 0, mtd A03_1Wood[DSB][L]`
- 15: `uvs 2, diffuse_2 0, mtd A03_[D][L]_Edge`
- 15: `uvs 2, diffuse_2 0, mtd M_7Metal[DSB][L]_Alp`
- 13: `uvs 2, diffuse_2 0, mtd M_4Stone[DSB][L]_Edge`
- 12: `uvs 2, diffuse_2 0, mtd M_1Wood[DS][L]`
- 11: `uvs 2, diffuse_2 0, mtd M_9Glass[DS][L]`
- 9: `uvs 2, diffuse_2 0, mtd A05_4Stone[DSB][L]`
- 7: `uvs 2, diffuse_2 0, mtd A05[D][L]`
- 7: `uvs 2, diffuse_2 0, mtd M[DB][L]_Alp`
- 6: `uvs 2, diffuse_2 0, mtd A05[DB][L]`
- 6: `uvs 2, diffuse_2 0, mtd M_9Glass[DSB][L]_Edge`
- 6: `uvs 2, diffuse_2 0, mtd A07_yuki[D][L]Alp`
- 6: `uvs 2, diffuse_2 0, mtd A07_yuki[D][L]`
- 6: `uvs 2, diffuse_2 0, mtd M99_lava12b[D][L]`
- 5: `uvs 2, diffuse_2 0, mtd M_4Stone[DS][L]_Alp`
- 5: `uvs 2, diffuse_2 0, mtd M_7Metal[DS][L]_Alp`
- 4: `uvs 2, diffuse_2 0, mtd A03_7Metal[DSB][L]`
- 4: `uvs 3, diffuse_2 1, mtd A05_4Stone[DSB][ML]`

## UV ranges (min/max of u and v together)

- `uvs 2 diffuse_2 0 set 0`: -16.00 .. 16.00 over 3480 meshes
- `uvs 2 diffuse_2 0 set 1`: -16.00 .. 16.00 over 3480 meshes
- `uvs 3 diffuse_2 1 set 0`: -16.00 .. 16.00 over 521 meshes
- `uvs 3 diffuse_2 1 set 1`: -16.00 .. 16.00 over 521 meshes
- `uvs 3 diffuse_2 1 set 2`: -16.00 .. 10.13 over 521 meshes

## Lightmap textures: 802 names, 1020 found in area TPFs

- 680: `format 0 type Texture mips 0 size 256x256`
- 197: `format 0 type Texture mips 0 size 128x128`
- 104: `format 0 type Texture mips 0 size 512x512`
- 22: `format 0 type Texture mips 0 size 64x64`
- 12: `format 0 type Texture mips 0 size 1024x1024`
- 2: `format 0 type Texture mips 0 size 32x32`
- 1: `format 0 type Texture mips 0 size 2048x2048`
- 1: `format 0 type Texture mips 0 size 1024x256`
- 1: `format 5 type Texture mips 0 size 1024x1024`

Sample names: m01_lit_B0m0000, m01_lit_B0m2000, m01_lit_B0m3040, m01_lit_B0m2100, m01_lit_B0m2101, m01_lit_B0m2110, m01_lit_B0m2111, m01_lit_b0m3010, m01_lit_B0m3020, m01_lit_B0m3030, m01_lit_b0m3050, m01_lit_B0m3051, m01_lit_B0m3052, m01_lit_B0m3060, m01_lit_B0m3070
