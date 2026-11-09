# DB_NT Textures (Switch 2 TPF)

The Duskbloods network test ships its textures as TPFs with platform byte `0xA` (`TPFPlatform.NintendoSwitch2`).
Before this work the TPF reader threw on every one of them, and no renderer path handled the platform.

Survey tool: `Tools/TpfSurvey` (reads the romfs directly; run from `bin/Release/net10.0` so it finds oo2core).

| Command | What it does |
|---|---|
| `TpfSurvey` | Census of header fields and formats, parsed by hand (written to `census.txt`) |
| `TpfSurvey check` | Every texture through SoulsFormats: read, layout size, headerize, DirectXTex load, ASTC error texels (`check.txt`) |
| `TpfSurvey raw <tpf> <texture> <bytes>` | Hex dump of one texture's data |
| `TpfSurvey preview <tpf> <dir>` | Top mip of each texture as PNG |

## Header

Texture entries are 0x28 bytes (no float struct in any DB_NT file):

| Offset | Field | Notes |
|---|---|---|
| 0x00 | data offset, size | |
| 0x08 | format, type, mips, flags1 | format byte is game-specific (0xCF, 0x67, ...); flags1 always 0 |
| 0x0C | width, height (int16) | |
| 0x10 | `TextureCount` | layers of a `TextureArray` (1, 2 or 4); 1 for cubes |
| 0x14 | `Unk2` | **block linear block height of mip 0, log2 GOBs** (0..4) |
| 0x18 | `Unk3` | 0xC9 or 0x100C9, meaning unknown, no effect on layout |
| 0x1C | name offset | UTF-16 |
| 0x20 | float struct flag | always 0 |
| 0x24 | DXGI format | the real format, including the Windows ASTC values |

The old reader asserted `Unk2` in {0, 9, 0xD} and did not read `Unk3` or the DXGI field.

## Formats (6020 textures)

| DXGI | Format | Count | Typical use |
|---|---|---|---|
| 163 / 162 | ASTC 8x8 sRGB / UNORM | 1445 / 1177 | albedo, emissive / normals, masks |
| 151 / 150 | ASTC 6x6 sRGB / UNORM | 2 / 2 | |
| 186 | ASTC 12x12 UNORM | 2 | normal |
| 98, 99 | BC7 UNORM / sRGB | ~1260 | `_3m`, character, menu |
| 72, 71 | BC1 sRGB / UNORM | ~570 | albedo / normals |
| 80, 83 | BC4, BC5 | ~890 | `_m`, `_v` |
| 95 | BC6H UF16 | 1 | env cube |
| 28, 88, 93 | R8G8B8A8, B8G8R8X8 (sRGB) | ~18 | colour grading LUTs, `_g` |
| 10 | R16G16B16A16 float | 5 | `_vat`, irradiance cube |

Normal maps hold X and Y in R and G with B = 0, in both ASTC and BC1.

## Data layout

Tegra-style block linear, but with a **different GOB interior** from Maxwell (Switch 1):

- A GOB is still 64 bytes x 8 rows = 512 bytes. GOBs stack vertically into blocks of `2^n` GOBs, and blocks run
  left to right, then down.
- Inside a GOB the byte address bits are `x0-3, y0-1, x4, y2, x5`. Maxwell is `x0-3, y0, x4, y1-2, x5`.
  Found from the colour grading LUT `m10_CGrading0010`, which is a smooth RGBA gradient.
- Mip 0 uses the header's block height. Each later mip halves it while the mip's rows of blocks fit in half a block
  (`rows <= 8 << (n - 1)`).
- Mips follow one another with no padding. A layer (array slice or cube face) is its whole mip chain.
- Arrays and cubes pad every layer to the mip 0 block size (`512 << n`), even an array of one layer. A plain
  texture has no padding at the end.

With these rules the computed size matches the stored size for all 6020 textures.
The header's block height is "GOB count rounded up to a power of two" for power-of-two sizes, but not for every
other size (300x300 BC7 has 3, 352x352 has 4), so the decoder always uses the header.

## Implementation

- `TPF.cs`: reads and writes `TextureCount`, `Unk2`, the new `TexHeader.Unk3` and the DXGI field for platform 0xA.
  Building a new Switch 2 texture from a DDS throws `NotImplementedException`; that needs a swizzler and an ASTC
  encoder.
- `Switch2Headerizer.cs`: deswizzles each layer and mip into a linear DX10 DDS. `Headerizer.Headerize` sends
  platform 0xA here, so the Texture Viewer, model texture export and OBJ export all get it.
- `AstcDecoder.cs`: CPU decoder for 2D LDR ASTC, any footprint, to R8G8B8A8. ASTC textures headerize as
  `R8G8B8A8_UNORM_SRGB` or `R8G8B8A8_UNORM`, because desktop GPUs (NVIDIA, AMD) mostly cannot sample ASTC. It is
  parallel across block rows for large mips. HDR endpoint modes and reserved blocks decode to magenta. None were
  found in DB_NT (the magenta texels counted in three `_3m` masks are real data).
- `TextureResource.cs`: Switch 2 textures are headerized on the resource loader's thread, then uploaded through the
  PC path. Upload tasks run on the render thread with a 500 item minimum, so decoding there would stall frames.
- `VulkanTextureHandle.cs` (Texture Viewer): Switch 2 uses the PC upload path, which headerizes in the upload task.

The full `check` pass headerizes all 6020 textures in about 22 s on one loader thread.

## FLVER texture paths

`TpfSurvey overrides` compares every FLVER texture path with its MATBIN sampler. Of 47,927 FLVER texture entries,
1,602 have a path, and all of them are on albedo slots. 720 match the MATBIN and 882 differ. Of the 882, 810 name
the **normal map** (`AET001_037_n` where the MATBIN has `AET001_037_a`, e.g. `aeg103_245` material `base_441`),
70 another albedo and 2 an emissive. These look like exporter leftovers, so for DB_NT and DB
`FlverResource.LookupTexture` uses the MATBIN's path when it has one, and the FLVER path only when it does not.
`TpfSurvey materials <geombnd>` lists one model's FLVER entries next to its MATBIN samplers.

## Not covered

- R16G16B16A16_FLOAT is still skipped by `FillWithTPF` (existing BB workaround), so the 5 `_vat` and irradiance
  textures do not upload. BC3 is still blocked outside DS2, but DB_NT has none.
- Material and shader setup for DB_NT normal map channels (RG with B = 0) has not been checked in the viewport.
- Writing Switch 2 textures.
- The PS5 TPF writer does not write the DXGI field that the reader reads (seen in passing, not changed).
