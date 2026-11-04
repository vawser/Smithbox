# Applying the ER/AC6/NR renderer features to SDT, BB, DS3, DS2 and DS1R

Written 2026-11-04. Follows `Documentation/Full GI` (irradiance volumes, GPARAM lighting) and
`Documentation/Post Processing` (the screen effect phases). DES is out of scope. Nothing here has been built.

## What already applies to every game

Every FLVER goes through `FlverShader` (`FlverDS3.*` and `FlverBB.*` in `Assets/Shaders` aren't referenced
anywhere), so the shader-side work is game-agnostic already:

| Feature | Older games today |
|---|---|
| BTL lights (`MapLightCollector`) | Works wherever a BTL loads: DS3, SDT, BB, DS2. DS1R has no BTL |
| SSAO, surface views, FXAA | Run without GPARAM (`SsaoPass.IsActive` only reads CFG) |
| SSR, TAA | Run on **ER's defaults** when there is no GPARAM (`GparamSsr.Enabled = true`, `GparamAntiAlias.Type = 5`). DS3 and BB have no SSR fields and no TAA fields, so they get effects the games never had |
| Sun shadows, HDR target, tone map | Run, with the viewport's own sun and exposure |

Everything that reads `GparamLighting` is off, because `GparamLightingBank.CanUse()` is AC6/ER/NR only:
map sun and hemisphere, exposure and auto exposure, bloom, glow, light shafts, volumetric fog, colour grading, wet.
`IrradianceVolumeBank` and `FogVolumeBank` are gated the same way.

## What each game has (surveyed 2026-11-04)

Surveyed on disk and with ivtool (`tpf`, `msb` with SDT/DS3/BB added to its switch in the scratch copy), plus the
GPARAM annotations in `Assets/GPARAM/<game>`.

### GPARAM files

| Game | Where | Names | Shared files |
|---|---|---|---|
| SDT | `param/drawparam/*.gparam.dcx` | `mAA_BB_CCWW`, as ER (`m11_00_0000`, `_0020`, `_0120`…), plus `sAA_*` | `mapbase`, `default` |
| DS3 | `param/drawparam/*.gparam.dcx` | `mAA_BB_CCWW` (`m30_00_0000`, `_0010`, `_0100`, `_0110`…), plus `sAA_*` | `mapbase`, `mapbase_x`, `mapoverride_*`, `default` |
| BB | `param/drawparam` | `mAA_BB_00NN` (`m24_01_0000-0003`), plus `sAA_*` | `default` (no `mapbase` seen) |
| DS2 | Different GPARAM layout (BGR colours, `Graphics Param Editor.txt`) | Not surveyed | |
| DS1R | No GPARAM. `param/DrawParam/aXX_DrawParam.parambnd.dcx` (PARAMs, already loaded by `ParamBank`) | | |

So the file chain in `GparamLightingBank` (`mAA_BB_0000` -> `mapbase` -> `default`) carries over to SDT and DS3 as is,
and to BB minus `mapbase`.

### GPARAM fields the resolver reads

Every string `GparamLightingBank` looks up, checked against each game's annotations:

| Group | SDT | DS3 | BB |
|---|---|---|---|
| LightSet (sun, Light1, LocalLight1/2, hemisphere, SSAO weights) | Same IDs | Same IDs | Same IDs |
| Shadows | Same | Cascade distances and fades; no depth bias fields | As DS3 |
| Tone Map | Same, including `GS Presents - ToneMap-Exposure`, but in **stops** (see "Step 1 as built") | Exposure is `Yebis-ToneMapExposure`, auto exposure `Yebis-AutoExposure`/`Yebis-MiddleGray`/`Yebis-Min/MaxExposure`. The files also have `GS Presents - ToneMap-Exposure`, at 0 (the annotations leave it out) | As DS3 |
| Bloom, Glow | Same | Same (no `Glow NumLevels`, `Blur Smoothness`) | As DS3, no `FinalColorScale` |
| SSAO | Same | Same (plus an older AO in `Other`: `FilterOther-fAOScale`…) | Core fields only |
| Light Shaft | Same | **Yebis light shaft** (`Yebis-LightShaft Enable`, `ShaftLength`, `GlareRatio`…), not ER's | As DS3 |
| Volumetric Fog | Same | None | None |
| SSR | Same | None | None |
| Anti-Alias | FXAA and TAA fields | FXAA only | FXAA only |
| Color Grading | `LutSourceId`, no `FadeTime` | Same | Same |
| Wet | Same | None | None |
| EnvironmentConfig, BG colour | No EnvironmentConfig; BG colour yes | BG colour yes | Neither |

Groups the older games have that ER doesn't, and the renderer doesn't draw:

- **Fog** (`FogParamEditor`): `Fog-Color`, `Fog-Dist`, `Fog-DistOffset`, `Fog-heightColor`, `Fog-heightDist`,
  `Fog-heightFogDensity`, `Fog-heightFogBase`… Classic distance and height fog. DS3, BB, SDT.
- **Light Scattering** (`LightScattering : ParamEditor`, `LsParamEditor`): sun angle, sun colour, Rayleigh and Mie
  coefficients, blend factor, ground reflectance. Aerial perspective. DS3, BB, SDT.
- **Environment Map** (`Env Map:Editor`): `EnvMapId`, `ColorScale`. Picks the map's reflection cubemap. DS3, BB, SDT.
- **Dominant Light Map**: sharpness and SH colour scales. Belongs with the lightmaps. DS3, BB, SDT.

### Per-part value IDs

ER takes value ID 0 for the map (the selecting code wasn't traced, but the data fits). DS3, SDT and BB parts carry
their own IDs instead: `Part.GparamConfig` has `LightSetID`, `FogParamID`, `LightScatteringID`, `EnvMapID`, and DS3
collisions add `SceneGparamConfig` (`ToneMapID`, `BloomID`, `ColorGradingID`, `ShadowParamID`…). SDT's
`SceneGparamConfig` holds `EventIDs` and `TransitionTime` instead. DS1R parts have byte `LightID`, `FogID`,
`ScatterID`, `ToneMapID`, `ToneCorrectID`, `ShadowID` into the DrawParam banks.

### Map lighting data

| Game | Indirect light | Reflections | Colour grading LUTs |
|---|---|---|---|
| **SDT** | **The same GIIV irradiance volumes as ER**: `map/mAA_BB_CC_DD/mAA_BB_CC_DD_envmap_0N.tpf.dcx`, N = 1-5, one TPF per bake holding every texture. `GIIV####_0N_W/X/Y/Z`, TPF format 102, Volume, BC7 3D, e.g. 132x16x132. **No `.ivinfobnd`** | One `GILM####_0N` cubemap per volume in the same TPF (128², 7 mips, DXGI 95 BC6H) | `map/mAA/mAA_cgrading.tpf.dcx`, `mAA_BB_CGradingNNNN`, 16x256: ER's layout, but **not** under `common/` |
| **DS3** | Lightmaps on map pieces (BTAB atlas per part), BTPB SH probes for characters and objects | `map/mAA/mAA_envmap.tpf.dcx`: `mAA_BB_GILM####` cubemaps (128², 7 mips, BC6H). MSB: 37 `EnvironmentMapPoint` `Env_Point###` and 38 `EnvironmentMapEffectBox` `Env_Box###` in m30_00 | `map/mAA/mAA_cgrading.tpf.dcx`, same as SDT. `gi_env_mAA.tpfbhd` also exists, unread |
| **BB** | BTAB + BTPB, as DS3 | Not surveyed | Not surveyed |
| **DS2** | BTAB + BTPB (DS2 versions) | Not surveyed | |
| **DS1R** | Lightmaps; `EnvLightTexBank` (DrawParam); parts' `EnvLightMapSpotIndex` | `EnvLightTexBank` resource IDs | None |

**SDT volumes link to boxes by number.** m11_00_00_00 has 55 `EnvironmentMapEffectBox` and 55 GIIV volumes:
`Env_Box000_…` <-> `GIIV0000`, `Env_Box010_…` <-> `GIIV0010`, `Env_Box200_…` <-> `GIIV0200`. The names have a
suffix after the number (`Env_Box210_??????`), so ER's rule (a 10-character name, last three characters) would
reject every one. `UnkT0A` is -1 on all of them, as in vanilla ER. The boxes also hold `AmbientDiffuseIntensity`
(1) and `AmbientSpecularIntensity` (0.25), which ER's don't. Untraced in sekiro.exe: a hypothesis from names and
counts.

**What the renderer does with lightmaps today: nothing.** `Material.lightmapTex/lightmapTex2` exist, and the
shader's material layout reserves row 4 z/w for them, but `FlverResource` never matches a lightmap sampler and
`FlverShader.frag` never reads one. `LightAtlasBank` (DS3, BB, DS2, DS2S) and `LightProbeBank` parse BTAB and BTPB
for editing only.

## Approach

### 1. A per-game profile instead of `ProjectType is AC6 or ER or NR`

`GparamLightingBank`, `FogVolumeBank`, `IrradianceVolumeBank`, `ColorGradingLuts` and the passes' defaults all ask
the same question differently. One profile object per game, owned by the lighting bank, would hold:

- Field aliases: e.g. Tone Map exposure is `GS Presents - ToneMap-Exposure` or `Yebis-ToneMapExposure`; light shaft
  is the ER fields or the Yebis ones. `FieldReader.Get` tries the aliases in order.
- Shared files (`mapbase`, `default` or just `default`).
- Defaults when a group is absent: **SSR and TAA off for DS3 and BB**, since they have no such fields.
- Paths: the LUT TPF (`common/` or not), the envmap TPFs, the volume link rule.
- Value ID selection (map ID 0, or per part, see 4).

### 2. SDT: nearly free

SDT's fields match ER's almost one for one, so most of ER's work applies once the gates let it in:

1. Map lighting, bloom, glow, light shafts, volumetric fog, SSAO, SSR, TAA, wet: enable SDT in
   `GparamLightingBank.CanUse`. `GetArea`'s `GparamRefSettings` lookup is ER's param; it already falls back to
   `mAA_BB` when the row is missing, to be confirmed for a project without the param. `GetSceneSubId` reads
   `GparamSubID_Base` or `Unk18`, neither of which SDT has, so the collision button needs SDT's field or stays off.
2. Colour grading: `ColorGradingLuts` with SDT's path, `/map/mAA/mAA_cgrading.tpf.dcx`.
3. Irradiance volumes: the bigger part.
   - Locator: `mAA_BB_CC_DD_envmap_0N.tpf.dcx`, one TPF per bake (ER has a TPF per texture inside a tpfbnd).
   - Link: `Env_Box(\d{3})` -> `GIIV0\1`.
   - No IVINFO, so no reflection normalisation and no `ShLuminance`. Use the `GILM` cubemap of the same ID raw,
     scaled by the box's `AmbientSpecularIntensity`, and scale the diffuse by `AmbientDiffuseIntensity`.
   - Before shading: check SDT's texel encoding is ER's (L0 in W, L1 in X/Y/Z, exponent in alpha). Decode one
     volume with ivtool and compare ranges with ER's. The Sekiro shaders are DXBC, not DXIL.
   - Bakes 1-5: SDT has no continuous time of day. Check whether its gparams key `TimeOfDay` at all, or whether the
     bakes follow the weather/progression files (`_0020`, `_0120`…). Until then, a bake combo, as AC6 has.

### 3. DS3 and BB: GPARAM with aliases, plus the groups ER doesn't have

1. Lighting, bloom, glow, SSAO and colour grading through the aliases in 1. BB gains nothing from `mapbase`.
2. **Reflection cubemaps**: load `mAA_envmap.tpf.dcx`'s `GILM` cubes into the cube pool and set `sceneparam.envmap`
   from `Env Map: EnvMapId` instead of the flat grey cube. Cheap, and it is the indirect specular for every surface.
   How `EnvMapId` and `Env_Point###` relate to `GILM0000-0005` needs one map's data side by side.
3. **Fog**: the `Fog` group is distance and height fog. `VolumetricFogPass` has an analytic fog path; feed it from
   these fields, or write a small fullscreen pass.
4. **Light Scattering**: an aerial perspective pass, Rayleigh plus Henyey-Greenstein Mie, from the sun angle and
   coefficients. New shader, applied where Fog is.
5. Light shafts: the Yebis fields (`ShaftLength`, `GlareRatio`, `AngleAttenuation`) mapped onto `LightShaftPass`'s
   parameters. They're close, not identical.

### 4. Per-part value IDs (DS3, SDT, BB, DS1R)

These games pick lighting per part. A first step that keeps one lighting per viewport: resolve with the IDs of the
selected part, or the most common `LightSetID` among the loaded map pieces, shown in Map Lighting. Doing it properly
means a small table of resolved LightSets in the material buffer (as `IrradianceVolumeTable` is) and a LightSet index
in the per-instance data, so the shader picks the sun and hemisphere per draw. Fog and scattering per part would
follow the same way.

### 5. Lightmaps and probes (DS3, BB, DS2, DS1R): their GI

The equivalent of the irradiance volume work for these games:

1. `FlverResource`: match the lightmap sampler (`g_Lightmap` / role `Lightmap`) into `lightmapTex`, and note its UV
   set (normally the second).
2. Per instance: BTAB's `UVOffset`/`UVScale` and `AtlasID` for the part and material (`LightAtlasBank` already has
   them), as `LightAtlasResolver` names them. Pieces without an entry use the FLVER's UVs as they are.
3. `FlverShader.frag`: when a lightmap is bound, its colour replaces `IndirectLight`'s diffuse term, as the volumes
   do. The encoding (BC6H HDR, RGBM, or a scale from Dominant Light Map's fields) has to be read off one texture
   and, ideally, the game's shader.
4. Characters and objects: the nearest BTPB probes' L1 SH (`Probe.Coefficients`, R0G0B0R1G1B1…) evaluated as the
   volumes' SH is, with `LightMask` scaling the sun. One probe per object (nearest, or a few blended) is enough.

### 6. DS1R: an adapter from DrawParam

No GPARAM, but `aXX_DrawParam` has the same ideas in PARAM rows, already loaded by `ParamBank`:

| DrawParam bank | Becomes |
|---|---|
| `LightBank`: three directional lights (`degRotX/Y_N`, `colR/G/B/A_N`), hemisphere (`_u`, `_d`), specular (`_s`), `envDif`/`envSpc` colours | `GparamLighting` sun, fills, hemisphere. Colours 0-255, `colA` an intensity in % |
| `ToneMapBank`: bloom begin/mul, `grayKeyValue`, adapted luminance min/max, `adaptSpeed`, light shaft power | Exposure, auto exposure, bloom, light shaft |
| `ToneCorrectBank`: brightness, contrast, saturation, hue | A colour matrix in `ToneMap.frag`, in place of a LUT |
| `FogBank`: begin/end Z, colour | The distance fog of 3.3 |
| `LightScatteringBank` | The scattering pass of 3.4 |
| `ShadowBank`: light angle, density, begin/end distance | Shadow cascades |
| `EnvLightTexBank` | Reflection and diffuse env textures |

Selected per part by `LightID`, `FogID`, `ScatterID`, `ToneMapID`, `ToneCorrectID`, `ShadowID`, so 4 applies.
`PointLightBank` holds falloffs only, not positions, so DS1R has nothing for `MapLightCollector`.

### 7. DS2: last

Its own GPARAM layout and groups (`Parallel Light`, `Fog Volume`, `Dynamic Adapation`, `Tone Map`, `Bloom`, `SSAO`,
`Light Shaft`, `Color Grading`, `Cascade Shadow`, `FXAA`), BGR colours. The same adapter idea as DS1R, from GPARAM
fields this time. It gets lightmaps and probes from 5.

## Step 1 as built (2026-11-04; not yet checked in the app)

The profile, and map lighting plus screen effects for SDT, DS3 and BB. No volumes, no reflection cubemaps, no Fog or
Light Scattering groups yet.

### What the data said (gpdump over every drawparam file, 2026-11-04)

The annotations turned out to be incomplete, so the fields were surveyed in the files themselves:

| | SDT | DS3 | BB |
|---|---|---|---|
| `GS Presents - ToneMap-Exposure` | -8 to 1.5, 42 files at 0: **stops** | 0 everywhere but one file (4.6, mode 7) | 0 everywhere |
| `Yebis-ToneMapExposure` | Not in the files | 1.5-2.3: linear, like ER's GS exposure | 1.5-5.7 |
| Auto exposure | GS: on in ~70 files, Middle Gray -2 to -8 (log2, as ER) | `Yebis-AutoExposure` on in 16, `Yebis-MiddleGray` 0.01-0.028 (linear: log2 -6.6 to -5.2, ER's range) | Off everywhere; Middle Gray 0.18 |
| Tone curve | GS ToneMapMode 7 and 8, as ER | GS mode 0, `Yebis-ToneType` 2 | As DS3 (four files mode 5) |
| `AntiAliasType` | 4 and 5 (TAA in 16) | 4 only | 4 (one 0) |
| TimeOfDay keys | 0, 2, 4, 6, 12, 18 in mapbase and m11 | One key (t0) | Not surveyed |
| LightSet | Sun w 0.2-3 | Sun w 0.2-16; many value IDs (0-320, 31000), the parts' `LightSetID`s | |
| `mapbase` | Has values | Present but every field empty | Absent |

LUTs: DS3's and SDT's `mAA_cgrading.tpf.dcx` textures are byte for byte ER's layout (DDS, 16 x 256, 32-bit, R mask
0xFF). BB's are PS4 textures, 16384 bytes with no DDS header and possibly tiled, so `ColorGradingLuts` rejects them.

### Built

- `Renderer/Scene/GparamProfile.cs`: per game, `Supported`, `SharedFiles`, `Exposure` (`GsLinear` AC6/ER/NR,
  `GsStops` SDT, `Yebis` DS3/BB), `HasSsr`/`HasVolumetricFog`/`HasGsLightShaft`/`HasTaa` (false for DS3, BB and the
  games before them), `HasTimezones` (AC6/ER/NR), `LutPathFormat` (ER's `common/`; SDT/DS3 the area folder; BB none).
  `Defaults` (shared) and `CreateDefaults()` are a `GparamPostEffects` with the missing effects off and, without
  TAA, AA type 4. `GparamProfile.Current` is the selected project's.
- `GparamLightingBank`: `CanUse()` is `Profile.Supported`; the shared files come from the profile; `Resolve` starts
  `PostEffects` from `CreateDefaults()`, so a group missing from the files leaves its effect off. `ReadExposure`:
  - GsLinear: as before.
  - GsStops: `Exposure = 2^x`.
  - Yebis: `Yebis-ToneMapExposure`, `Yebis-AutoExposure`, `MiddleGray = log2(Yebis-MiddleGray)`,
    `Yebis-ToneType` 2 -> ToneMapMode 7 (filmic), else 0 (clamp). Then `ReadYebisAdaption`: Adaption Min/Max =
    log2(`Yebis-Min/MaxExposure` / exposure), e.g. -5.5 to +2.1 EV in DS3 m30. Adapt speeds stay the GS fields.
  - LUTs loaded from `Profile.LutPathFormat`, none if it is null.
- `ColorGradingLuts.Load` takes the path format.
- `SsrPass`, `TaaPass`, `FxaaPass`: their defaults without a GPARAM are `GparamProfile.Current.Defaults`, so DS1R,
  DS2, DES and the older games' Model Editor viewports no longer get SSR or TAA.
- Viewport Rendering > Map Lighting: the exposure and auto exposure rows show the source of the profile's field; the
  seven timezone buttons only for AC6/ER/NR.
- `ViewportEnvironment.CanUse` also needs the volume bank, so the Model and Time Act Editors' borrowed map lighting
  stays AC6/ER/NR as before.

Hypotheses: SDT's exposure is in stops; DS3's `Yebis-ToneType` 2 is a filmic curve; the Yebis Middle Gray is a linear
key comparable with the GS one after log2. DS3 and SDT use LightSet ID 0 for the whole viewport, although their parts
pick their own IDs (approach 4).

### Check

| Where | Do | Expect | If wrong |
|---|---|---|---|
| SDT m11_00_00_00 (Ashina Castle) | Map Lighting on; Map Lighting tab | A table with values from `m11_00_0000` / `mapbase`; Exposure near 1 (0 stops); no timezone buttons | "default" sources everywhere: the files aren't found |
| Same | Drag the time 0 -> 18 | The sun's colour and strength change through keys 2, 4, 6, 12 (warm at 4), and the exposure follows | |
| Same | Post Effects tab | Auto exposure on with middle gray about -3 to -7; Bloom, SSAO; Color Grading row names `m11_00_CGradingNNNN` | Colour grading "not found": the LUT path |
| Same | Toggle Map Lighting | Similar brightness on and off (Exposure Scale 1) | Far too dark: exposure isn't stops after all, report the Exposure Scale that looks right |
| DS3 m30_00_00_00 (High Wall) | Map Lighting on | Sun (1, 0.94, 0.87) x 1.23 from `m30_00_0000 #0`; Exposure 2.3 from `Yebis-ToneMapExposure`; auto exposure off | Exposure 0 or black: the Yebis field isn't read |
| Same | Post Effects tab | No SSR, Volumetric Fog, Light Shafts or TAA among the passes; FXAA runs; Color Grading from `m30_00_CGradingNNNN` if the file enables it | SSR or TAA listed: the defaults aren't the profile's |
| DS3, a map whose file has `Yebis-AutoExposure` 1 | Post Effects tab | Auto exposure on, middle gray about -5 to -6.6, adaption a few EV either side | |
| BB m24_01_00_00 (Hunter's Dream) | Map Lighting on | Values from `m24_01_0000` / `default`, exposure from `Yebis-ToneMapExposure`; no colour grading | |
| ER m10_00_00_00, AC6, NR | Everything | As before: timezone buttons, the GS exposure, TAA in ER's mapbase | |
| DS1R or DS2 Map Editor; any older game's Model Editor | Post Effects | No SSR or TAA | |

### SDT fog too dense, bloom too strong (reported 2026-11-04; fixed, not yet checked in the app)

SDT's values sit far from ER's: m11_00_0000 has `HeightFogDensity Scale` 0.075-1 (ER m10: 0.004), `HeightFogOffset`
-85 to -150 with Ashina's valley below it, `FixedDensity` up to 0.015, and Bloom `Glare Threshold` 0-0.075 with
`Alpha` 0.4-1 (ER: 1 and 0.25). The game's shaders were read (Sekiro `shader/*.shaderbnd.dcx` with ivtool `shaders`,
`fxc -dumpbin` for its DXBC; Nightreign's DXIL with `dxc -dumpbin` to check the later engine):

- **Height fog** (`GXVolumetricFog_WriteInScattering`): `z = g_HeightFogParam.x x dot(position, g_HeightFactor)`,
  then `density = Global.x + local + g_HeightFogParam.y x (z >= 0 ? exp(-z) : 2 - exp(z))`. Below the offset the
  fog levels off at twice the scale. Smithbox grew it as `exp(-z)` without limit (clamped at 4 per metre), so every
  froxel under Ashina's offset was opaque. The same code is in Nightreign, so the fix applies to every game.
- **Bloom bright pass** (`ToneMap_BrightPassPS`): `L = dot(c, (0.29891, 0.58661, 0.11448))`,
  `c x max(L - T, 0) / (L + 0.0001)`, then a mix towards luma by `BloomBoostColor.w` and x `BloomBoostColor.rgb`. No
  knee. Smithbox measured the brightest channel with a soft knee, which blooms more, above all on saturated colours.
  Identical in Nightreign.
- Not settled: SDT's `VolumetricFog GameScale` (1-12 through the day; not in ER) isn't a shader constant, so it is
  applied on the CPU in a way not traced, and is still ignored. The composite weights the levels 1, 1/2, 1/4 and x
  `g_vBloomFinalColor`, also set on the CPU; Smithbox still averages its levels x Alpha.

Changed: `Density` in `VolumetricFog.comp` and `VolumetricFog.frag`; `Prefilter` in `Bloom.frag` (BloomPass passes
no knee). SPIR-V rebuilt with glslangValidator.

| Where | Do | Expect | If wrong |
|---|---|---|---|
| SDT m11_00_00_00 | Look down into the valley at a few times of day | Mist in the valley that thins with height, through which the far side shows; not a solid wall | Still solid: GameScale probably divides the density; report the time and how it looks |
| Same | Toggle Bloom | Softer than before; bright skies and lit surfaces glow, coloured lamps less than before | Still too strong: the composite's CPU-side scale; say roughly how much stronger than the game |
| ER m10_00_00_00, m60 tiles | Fog and bloom on | Much as before; less fog only where the camera looks below a map's height fog offset, slightly less bloom on saturated lights | |

Result (2026-11-04): fog still too dense, bloom still too strong in SDT.

### Second pass: GameScale, level weights, calibration sliders (2026-11-04; not yet checked in the app)

Read further:
- `GXVolumetricFog_SolveVolumetricFog`: each slice's density and light are scaled by `exp(log depth of the slice) x
  PreComputed.x` (slice thickness), and the transmittance is `exp(-sum)`. In-scatter is attenuated by
  `exp(-sum x ExtinctionScale)` per channel. `PreComputed`, `Global`, `g_HeightFactor` and `g_HeightFogParam` are
  CPU-set. Nothing named GameScale is in any fog shader.
- `GameScale`: Nightreign's mapbase has 1. In SDT it is 1 in 27 files, and keyed through the day in the others
  (m11_00_0000: 12, 1.7, 1.25, 2.5, 1, 1 at t0-t22). Never below 1.
- `ToneMap_BloomCompositePS` (Sekiro and Nightreign): `(A + B / 2 + C / 4) x g_vBloomFinalColor`. The tone map adds
  the glare x `g_GlareLuminance.x` to the exposed scene. The CPU-set scale is unknown.
- The fog strings in sekiro.exe have neither RIP-relative references nor absolute pointers (searched at 4-byte
  steps): the code that reads them is in its protected sections, as with Nightreign. Not traceable with ivtool.

Built (decided with the user: the likeliest reading plus sliders):
- `GparamVolumetricFog.GameScale` (`VolumetricFog GameScale`, default 1). `VolumetricFogPass` multiplies
  FixedDensity and HeightFogDensity Scale by `Viewport_Post_Fog_Density_Scale / GameScale`. Hypothesis.
- Bloom: the upsample adds each coarser level at `Settings.z` of the one above, 0.5 for the bloom (the game's
  1, 1/2, 1/4) and 1 for the glow (as matched by eye before). The composite divides by the weights' sum, so a flat
  bright area still blooms by Alpha. Alpha x `Viewport_Post_Bloom_Strength`.
- Post Effects: "Strength" under Bloom and "Density Scale" under Volumetric Fog (0-2, default 1). The Height Fog row
  shows the game scale.

| Where | Do | Report |
|---|---|---|
| SDT m11_00_00_00, a view you know from the game | Set the time to match it; move Density Scale until the fog matches | The Density Scale, the time, and the Height Fog row (its game scale) |
| Same, at another time with a different game scale (t0: 12, t6: 1.25) | The same | Whether the same Density Scale fits: if it does, dividing by GameScale is right; if it changes with the game scale, it isn't |
| Same | Move Bloom Strength until the bloom matches | The value; whether it differs between times (Alpha is 1 at t0-t4, 0.4 at t6) |
| ER m10_00_00_00 | Both sliders at 1 | As before; if anything less wide a bloom haze round very bright areas | 

Result (2026-11-04, SDT m11_00_00_00): matched with Bloom Strength 0.5 and Fog Density Scale 0.1. Only one time of
day was reported, so whether dividing by GameScale is right is still open (a second time with a different game scale
would show it).

Built: `GparamProfile.FogDensityScale` and `BloomScale`, 0.1 and 0.5 for SDT, 1 elsewhere, applied in
`VolumetricFogPass` and `BloomPass` before the viewport's sliders, so the sliders at 1 now mean "as matched".

| Where | Do | Expect | If wrong |
|---|---|---|---|
| SDT m11_00_00_00 | Set both sliders back to 1 (they are saved in the settings at 0.5 and 0.1) | The fog and bloom as matched | |
| Same, at t0 (game scale 12) and t6 (1.25) | Compare with the game | Both still match | One is off: report the Density Scale that fits it, and the GameScale rule changes |
| ER, NR, AC6 | | Unchanged | |

### DS3 too bright: tone curve and light probes (reported 2026-11-04; built, not yet checked in the app)

What the game's shaders say (DS3 `shader/*.shaderbnd.dcx`, ivtool `shaders`, `fxc -dumpbin`):
- **Tone curve.** `ToneMap_WriteFilmicToneMappingTablePS` writes Hejl and Burgess-Dawson's filmic curve,
  `x' = max(x - 0.004, 0)`, `x'(6.2x' + 0.5) / (x'(6.2x' + 1.7) + 0.06)`, raised to 2.2 / `Yebis-Gamma` (2.2 in the
  files, so 1). Its output is display values. `ToneMap_TabledToneMapLUTPS` indexes it with `x / (x + 0.2)` of the
  scene x `g_ToneMapSceneLumScale` x the adapted exposure (1 with auto exposure off: `ToneMapVS` writes 1), plus the
  glare x `g_GlareLuminance`. `g_ToneMapSceneLumScale` is set by the game's code, so how `Yebis-ToneMapExposure` is
  turned into it is not known.
- **No hemisphere.** No DS3 shader has a hemisphere constant. The forward FLVER shaders light with: ambient
  `max(dot((n, 1), aSh[c]), 0) x albedo` (per instance, `cbInstanceData.aSh[3]`), the IBL cubemap, sun x
  `ShLightMask.x`, local lights 1-2 as squared half-Lambert. The deferred `GXEnvMapRegion_IvDiff` reads
  `g_ShVolumeR/G/B/A` (3D, plain float) the same way, x `g_envMapDiffuseColorScale`. So the GPARAM hemisphere, which
  Smithbox used as all the ambient (0.5 x 1 x exposure 2.3), is not what lights DS3 surfaces.
- **BTPB groups are grids.** `Unk10/14/18` probe counts (31 x 4 x 7 = 868, 18 x 3 x 10 = 540), `Unk1C/20/24`
  spacing (2 m; 1.5 or 5 in some), `Unk28` the first probe. Coefficients are integers (L0 0-2800), `LightMask`
  0-1022 of 1024.
- **Map pieces have lightmaps too**, named differently: `map/mAA/gi_env_mAA.tpfbhd` (10,016 files in m30) holds
  `<area>_m<piece>_gi_NNNN_00_dol_00/01` BC7 textures, read as `g_DOLTexture`/`g_DOLTexture2` by the 348
  `GXFlver_*DOL*` shaders: DOL 1 is the colour (squared, x `FC_DominantRgbScale` x `ColorScale`), DOL 2 a dominant
  light direction (rotated by the instance's `vDominantRotateParam`) and a sharpness (+ `DominantSharpnesOffset`)
  splitting the light into a directional part (x N.L) and an ambient part. UVs are the second set, through
  `atlasUvScaleOffset` (BTAB). Not built: the follow-up for map pieces.

Built (decided with the user: the probes first):
- `ToneMapOperator.YebisFilmic` (`ToneMap.frag`, no square root after it), chosen by `GparamLighting.YebisFilmicMode`,
  which `ReadExposure` sets for `Yebis-ToneType` 2 (DS3, BB).
- `LightProbeGridBank` (on `IrradianceVolumeBank.ProbeGrids`): per loaded map, the groups of its `_0000` BTPB
  (the first otherwise) as four R32G32B32A32 3D textures each in the volume pool, probes placed by position. W = L0
  rgb and LightMask, Y/Z/X = coefficients 3-5, 6-8, 9-11, all / 1024. Entries in the irradiance volume table with
  `Params.y` = 1, a box from half a spacing before the first probe to half after the last, fading over one spacing,
  coarse grids first.
- `FlverShader.frag`: probe entries are read linearly, `max(W + X n.x + Y n.y + Z n.z, 0)`; reflections are scaled
  by the probes' luminance over the hemisphere's (0-2), so they darken indoors; `g_probeSunMask` (W's alpha, faded)
  scales the sun, for which `IndirectLight` now runs before the sun. `GparamProfile.HasLightProbes` (DS3, BB) keeps
  the hemisphere out of the volumes; outside every grid it is still the ambient.

Hypotheses: the coefficients' 1/1024 scale (L0 ~500 outdoors gives 0.49, close to the hemisphere's 0.5); bands
1-3 being Y (up), Z, X with no sign flips; the `_0000` file being the one to use (m30_00 also has `_0001`).

| Where | Do | Expect | If wrong |
|---|---|---|---|
| DS3 m30_00_00_00 (High Wall), map lighting on | Look at interiors and covered walkways | Darker than outside, and no sunlight inside (LightMask) | Interiors as bright as before: the grids aren't loaded; check the light probes are visible in the Map Editor |
| Same | Outdoors, compare with the game | Shading from above brighter than from below, overall nearer the game | Lit from below or one side: a band's axis or sign is wrong (say which way); too bright or dark overall: report the Exposure Scale that matches |
| Same | Walk a probe group's edge | A soft change over about 2 m, no hard seam | A hard edge: the box or fade is off |
| Same | Highlights and bright sky | Highlights roll off softer than before (Hejl) | |
| ER, AC6, NR, SDT | | Unchanged (no probes, their own curves) | |

### DS3 map piece lightmaps (DOL) (2026-11-04; built, not yet checked in the app)

The data (gi_env_m30, m30_00 BTAB and FLVERs, DS3 DXBC):
- **Names.** `AABBCCDD_mXXXXXX_gi_NNNN_00_dol_0K[_atlasIII][_l]`. AABBCCDD is the map (30000000 = m30_00_00_00),
  mXXXXXX the part's model, NNNN the bake (the map's `_0000` / `_0001` BTAB and BTPB), K 0 or 1 for DOL 1 / 2, III
  the BTAB `AtlasID` of an instanced piece, `_l` a low-detail copy. The binder holds every map of the area.
- **BTAB.** 2,370 entries in m30_00 `_0000`: `PartName` (m000518_1000), `MaterialName` (the FLVER material, o001290),
  `AtlasID` 0-7, `UVScale`/`UVOffset`. m000518 only has `_atlas000` textures. A piece without entries uses its own,
  whole.
- **FLVERs.** Map piece materials (`M[ARSN]_l.mtd`) list `g_DOLTexture1`/`g_DOLTexture2` with paths naming those
  textures (`...\30000000_m000518_gi_0000_00_dol_00.tif`).
- **Vertex shader.** `uv = UV1 x atlasUvScaleOffset.xy + .zw` (the instance's).
- **Pixel shader** (`GXFlver_ColDifSpcBumpDOL_Fwd`): DOL 1 rgb squared x `FC_TexHdrScaleParam` (the TPF entry's
  FloatStruct: m000500 0.274, 0.295, 0.398) x `FC_DominantRgbScale` x `FC_DominantColorScale`; DOL 2 rgb x 2 - 1 is
  a direction in the model's space, turned by the instance's quaternion `vDominantRotateParam`; sharpness =
  saturate(DOL 2 alpha + `FC_DominantSharpnesOffset`); light = color x sharpness x N.L + color x (1 - sharpness),
  times (1 - specular color) for energy. The sun is scaled by DOL 1 alpha x `ShLightMask.x`. Final: albedo x (lights
  + DOL light x AO) + specular.
- **GPARAM** `DominantLightMap:Editor`, m30_00_0000: Sharpness Min 0.5, Offset 0.55, Exponant 1.25, RGB Scale 0.5,
  R/G/B 1; SH Color Scale 0.5, SH R/G/B 1 (m40: Offset 1, RGB Scale 1.2, R/G/B 0.6/0.8/1). `cb9[12]` = (Min, Offset,
  Exponent, RGB Scale), `cb9[13]` = R/G/B. Min and Exponent aren't used by this shader. SH Color Scale is taken to
  scale the probes' SH, as `aSh` is the only other SH.

Built:
- Loading: `PathBuilder` `map/tex/{mAA}/gienv` -> `map/mAA/gi_env_mAA.tpfbhd`; `TextureLocator.GetDolLightmapVirtualPath`;
  `MapResourceHandler.SetupTextureLoadLists` queues it with the map's textures (DS3, `Viewport_Map_Lightmaps`). The
  whole binder loads, every map and bake of the area (about 40 MB for m30).
- Materials: `FlverMaterial.UsesLightmap` from a `DOLTexture` sampler (the FLVER's or the MTD's); such a material
  is drawn with `FlverShader_blend` and the UV2 layout, with `c_layerCount` 1, for its second UV set.
- `LightmapTable` (material buffer, 16,384 x 48 bytes; entry 0 the Dominant values) and `LightmapBinding` (a part's
  entries by material name). `SceneParam.LightmapStart` takes the old `MapLightPad0`.
- `MeshRenderableProxy.Lightmap`, passed to its submeshes; a static instance of a lightmapped material puts
  `InstanceData.LightmapFlag | entry` in `SkinReference` (animated ones use 1 or the high bit).
- `DolLightmapBank` (on MapData, run by `MapEditorScreen.Update` after the lighting): reads each area's binder index
  once in the background (names, and DOL 1's FloatStruct HDR scales); per loaded MSB3 map piece, BTAB's `_0000`
  entries by part give an atlas per material, else the piece's own textures; once both textures are on the GPU an
  entry is made and the part's mesh gets the binding. Parts no longer loaded give theirs back.
- `GparamLighting.DominantSharpnessOffset`, `DominantColor`, `ShColor`. The probe grids are now x `ShColor`
  (halving them in m30).
- `FlverShader.vert`: `IsAnimated()` ignores the lightmap flag; the UV2 variants pass `fsin_lightmap` and
  `fsin_modelToWorld`. `FlverShader.frag`: `LightmapIrradiance` as above (without the 1 - specular factors, which
  the PBR model handles its own way), replacing the probes or hemisphere for the pixel, its DOL 1 alpha the sun's
  mask, reflections scaled by its luminance against the hemisphere's.

Hypotheses: SH Color Scale scales the probes; the low bits of the material texture rows are untouched (the lightmap
fields of `Material` stay unused); bake 0000.

| Where | Do | Expect | If wrong |
|---|---|---|---|
| DS3 m30_00_00_00, map lighting on | Load the map, wait for its textures | Map pieces lit unevenly: darker in corners and under overhangs, brighter facing open sky, no sunlight indoors | Flat as before: the lightmaps aren't found. Report the Post Effects / log for "lightmaps", and whether Viewport_Map_Lightmaps is on |
| Same | A courtyard or street | Shadows baked on the ground where buildings block the sun, matching the cascades | Lit everywhere or nowhere: DOL 1 alpha is read the other way |
| Same | Bookshelves and repeated props (BTAB atlas pieces) | Each copy lit for where it stands, not all the same | Smeared or wrong patches: the atlas UV scale/offset or V flip is off |
| Same | Faces turned towards a window | Brighter on the side facing it | Brighter on the far side: DOL 2's direction is flipped or in the wrong space |
| Same | Characters and objects | Still lit by the probes (half as bright as before, from SH Color Scale 0.5) | Too dark now: SH Color Scale doesn't apply to the probes |
| ER, SDT, BB | | Unchanged | |

## Suggested order

| Step | Games | Cost | Visible gain |
|---|---|---|---|
| Profile, SSR/TAA off where the game lacks them | All | Small | Removes effects the games never had |
| SDT map lighting and post effects, LUT path | SDT | Small | Sun, exposure, bloom, fog, grading as ER |
| DS3/BB aliases, reflection cubemaps | DS3, BB | Small-medium | Map sun/exposure, real reflections |
| SDT irradiance volumes | SDT | Medium (most of ER's code reused) | Indirect light through the map |
| Fog and Light Scattering groups | DS3, BB, SDT, DS1R | Medium | The look of DS3/BB's distance haze |
| Lightmaps + BTAB, then BTPB probes | DS3, BB, DS2, DS1R | Large (format research) | Their GI |
| Per-part value IDs | DS3, SDT, BB, DS1R | Medium-large | Correct lighting where a map mixes LightSets |
| DS1R DrawParam adapter | DS1R | Medium | DS1R map lighting |
| DS2 adapter | DS2 | Medium | DS2 map lighting |

## Open questions

| Question | How to settle |
|---|---|
| SDT volume texel encoding = ER's? | ivtool `decode` on an SDT GIIV (needs a path without IVINFO), compare with ER; Sekiro's GI shader |
| SDT's link rule `Env_Box(\d{3})` -> `GIIV0###` | Every SDT map with ivtool `msb` + `tpf`; then sekiro.exe strings near `GIIV` |
| What SDT's five bakes mean | Survey `TimeOfDay` keys in SDT gparams; compare bake brightness |
| DS3 `EnvMapId` / `Env_Point` -> `GILM####` | Dump m30_00's points, boxes and the GparamConfig `EnvMapID`s beside the GILM names |
| Lightmap texture encoding and UV set | One DS3 map piece: its FLVER sampler list and the lightmap DDS format |
| Is DS3 `Yebis-ToneMapExposure` linear, as ER's is | Check in the app once aliased |
| BB's shared files and envmap/LUT paths | Survey BB `map/` and `param/drawparam` |
