# Data

All findings below were checked against real files; counts are from full surveys unless a map is named.

## Files per map

Example: NR `map/m10/m10_00_00_00/`, ER the same paths through the VFS, AC6 the same on disk.

| File | Content |
|---|---|
| `mAA_BB_CC_DD_{high,middle,low}.ivinfobnd.dcx` | BND4 of `.ivInfo` files, one per volume and bake (see IVINFO below) |
| ER/NR: `mAA_BB_CC_DD_envmap_NN_{high,middle,low}_00.tpfbnd.dcx` | BND4 of TPFs. Each TPF is one texture |
| AC6: `mAA_BB_CC_DD_envmap_NN_{high,middle,low}_{w,x,y,z,rem}.tpf.dcx` | One TPF per channel |

`NN` in the envmap file is the **bake** index, and it equals the `_##` suffix of the texture names inside.
Bakes are probably times of day or weather states: AC6 m00_10 has 6 (`_00`-`_05`), ER has 7 (`_00`-`_06`),
NR mostly 1 (`_00`), AC6 m03_20 has `_00`, `_03`, `_06`. Values for the same volume differ only slightly between bakes.

Quality levels shrink the voxel grid (AC6 m00_10 volume 500: high 56x52x104, low 12x12x24). Use `high`.

## Volume textures

Names: `{map}_GIIV{id:4}_{bake:2}_{W|X|Y|Z}`, e.g. `m10_00_00_00_GIIV0203_00_W`.

- TPF format 102, texture type `Volume`, 1 mip. DDS is DX10 with DXGI 98 (`BC7_UNORM`), dimension 4 (3D).
  Smithbox maps DXGI 98 to `VkFormat.Bc7UnormBlock` already (not sRGB, which is correct).
- Dimensions are multiples of 4 (BC7 blocks). DDS header: height at 0x0C, width at 0x10, depth at 0x18.
- Texture width is box X (Width), height is box Y (Height), depth is box Z (Depth).
- Density varies: 1 voxel/m (NR Env_Box203 60x60x48 box, 60x60x48 texels), 2/m (AC6 m00_10: 28x26x52 box,
  56x52x104 texels), down to ~40 m (NR Env_Box000 3400x300x2100 box, 84x4x52 texels). Texels are not
  necessarily cubic; always map the whole box to UVW 0..1.
- Decoded texel values centre on 0.5 for X/Y/Z (signed) and ~0.5-1.0 for W. Alpha is an exponent; see Shaders.md.

AC6 also has `GILM{id}_{bake}_rem`: a 512x512 BC6H cubemap with 7 mips, the region's reflection cubemap.
ER/NR keep reflection cubemaps elsewhere (not traced; the deferred pass binds one as `g_IBLTexture`).

## IVINFO (`.ivInfo`, 0x1C bytes)

Class: `src/Andre/SoulsFormats/SoulsFormats/Formats/IVINFO.cs`. Round-trips 18,225 AC6, 66,417 ER and 21,058 NR files.

| Offset | Type | Field |
|---|---|---|
| 0x00 | int | Version, 1 |
| 0x04 | int | 0 |
| 0x08 | Vector4 | `ShLuminance`: volume-average luminance SH, L1 in XYZ, L0 in W |
| 0x18 | float | `Unk18`: 0 in AC6, 1 in ER/NR; not passed to shaders |

`ShLuminance` is only used to normalise the reflection cubemap (Shaders.md). Diffuse GI does not need it.

## Placement: the EnvMapEffectBox region

The volume's world box is an MSB region of type EnvMapEffectBox (`MSBE.Region.EnvironmentMapEffectBox`,
`MSB_NR.Region.EnvMapEffectBox`, `MSB_AC6.Region.EnvMapEffectBox`). The region's shape is a Box.

How a box links to its volume ID. Smithbox implements the game's behaviour exactly (decided 2026-11-03, after
"property only" and "name first" were tried): `IrradianceVolumeBank.ResolveVolumeId`.

| Game | Link | Checked |
|---|---|---|
| AC6 | Property `EnvMapEffectBox_UnkT32` (names are placeholders like `{23}`) | data: m00_10, m03_20, m10_00, 0 unmatched |
| ER | Property `UnkT0A` (type data +0x0A, short) when >= 0; otherwise the game falls back to the name (below) | traced in eldenring.exe |
| NR | Same type-data layout as ER; assumed the same rule | data only; exe code is Arxan-split, not traced |

### What eldenring.exe does (traced 2026-11-03)

Two functions build the volume names, `140B391D0` and `140B3A6C0` (ER exe of 2026-10-21, 86,973,008 bytes).
Both loop over the MSB's regions of type 0x11 (EnvMapEffectBox) and, for each one:

```
if (shape != Box (5))           skip
if (wcslen(name) != 10)         skip          // "Env_Box203" is exactly 10 characters
id = name[7]*100 + name[8]*10 + name[9] - '0'*111
if (typeData.Unk0A >= 0)        id = typeData.Unk0A     // movzx [typeData+0Ah]; cmp 0, ax; jg; movsx
-> formats "%s_GIIV%04d_%02d" (id, bake) and "%s.ivInfo"
```

So `Unk0A` is the volume ID property, and the name digits are only the game's fallback when it is -1.
Every vanilla ER and NR box has `Unk0A = -1`, so the vanilla data relies on the fallback. Consequences:

- Following the property rule strictly, vanilla ER/NR boxes have no link. Writing the name-derived ID into
  `Unk0A` changes nothing in game (same ID either way) and makes the link a property.
  **Decision (2026-11-03, final):** Smithbox links as the game does, so vanilla boxes link by name. The
  undoable Fill Volume IDs action stays as an option, to make links survive renames.
- The name must still be exactly 10 characters and the shape a Box, even when `Unk0A` is set, or the game skips
  the region entirely.
- The name fallback can only express IDs 0-999 (three digits); `Unk0A` can hold up to 32767.
- SoulsFormats names the field `UnkT0A` in `MSBE.Region.EnvironmentMapEffectBox` and `Unk0A` in
  `MSB_NR.Region.EnvMapEffectBox` (this doc says `Unk0A` for both); renaming it (e.g. `IrradianceVolumeID`) would also need Smithbox's MSB meta.

The same pointer is read at +0x2E and +0x36 in that code, matching the `Unk2E` and `Unk36` type-data fields,
which confirms it is the region's type data.

In AC6 the matching EnvMapPoint has the same ID in `EnvMapPoint_UnkT20`. AC6's type data has 0xFFFF at +0x0A
too (`UnkT0A`/`UnkT0B` = 255), so it may share ER's override slot; the AC6 exe was not traced.
Points place the reflection cubemap captures and are not needed for diffuse GI.

Several boxes can share one ID (AC6 m03_20 has many boxes with UnkT32 0 pointing at the coarse GIIV0000).

Box conventions: the region position is the **bottom centre** of the box (NR Env_Box200 at y=-34.4 with height 70
contains Env_Point200 at y=22.6; centred it would not). Rotation is the usual MSB Euler in degrees. Smithbox already
draws these boxes, so reuse its region transform rather than re-deriving it.

Relevant region fields (ER/NR names; AC6 names in brackets):

| Field | Seen | Probable shader input |
|---|---|---|
| `FadeDistance` [`TransitionDist`] | 0.5 indoor, 1.5-10 outdoor | `g_envMapFadeDistance` (metres faded in from each face) |
| `Priority` [`EnvMapEffectBox_UnkT09`] | 10 typical; AC6 5-100 | draw order between overlapping boxes (unverified) |
| `AmbientDiffuseIntensity` | 1, sometimes 0 | `g_envMapDiffuseColorScale` (unverified) |
| `AmbientSpecularIntensity` [`SpecularLightMult`] | 0.5-8 | `g_envMapColorScale` (unverified) |

A huge box with ID 0 usually covers the whole map at coarse resolution (ER Env_Box000 4500x3500x4500,
NR 3400x300x2100, AC6 35000x7000x35000), with finer boxes nested inside it.

## Not covered

- NR has ~60 extra `_01` volumes in `envmap_01..06` with no IVINFO and no box, probably for the NR-only
  `MidRangeEnvMapOutput` regions. Skip them at first.
- DS3/SDT use BTAB/BTPB lightmaps and probes instead; Smithbox already has `LightAtlasBank` and `LightProbeBank`.
- Duskbloods has EnvMapEffectBox regions too but was not checked.
