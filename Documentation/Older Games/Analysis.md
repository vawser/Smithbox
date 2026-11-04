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
