# DES DrawParam lighting: plan

Written 2026-11-08. Nothing here has been built yet.

**Goal:** light the Demon's Souls Map Editor viewport from the game's own DrawParam, as the AC6/ER/NR/SDT/DS3/BB
viewports are lit from their GPARAMs. The design also serves DS1R, whose DrawParam has the same banks
(`Analysis.md` step 6), and whose adapter isn't built either.

The evidence is in `Tools/DesDrawParamSurvey/survey.md`, made by `Tools/DesDrawParamSurvey`.

## What exists today

- **What the renderer reads:** everything comes through one `GparamLighting` (`Renderer/Scene/GparamLighting.cs`):
  - the sun, `Light1` and two local lights, each with diffuse and specular (`.w` = HDR intensity);
  - the hemisphere up and down colours;
  - exposure, auto exposure and the tone map mode;
  - shadow cascades, the env map scale, the background colour and `PostEffects` (bloom, glow, light shaft, volumetric fog, SSAO, SSR, AA, grading).

  `GparamLighting.Models` is a second set used to light characters.
- **The source:** `GparamLightingBank` builds that from GPARAM files, and `GparamProfile.For(DES)` is `Older` with `Supported = false`. So the DES Map Editor viewport uses the viewport's own sun and exposure. Only the DES lightmap toggle (`DES Model Display.md`, Phase 4) reads game data.
- **Model and Time Act viewports:** `ViewportEnvironment` lights them only for games with irradiance volumes (AC6, ER, NR), so they stay as they are for DES.
- **What the renderer can't draw yet:** classic distance fog (only `VolumetricFogPass` exists), a brightness/contrast/saturation colour correction (only LUT grading), and Rayleigh/Mie light scattering.

## What DES has (survey)

- **Banks:** `param/drawparam/aAA_drawparam.parambnd` for areas a01-a08 and a99. Each holds `mAA_<Bank>.param` for the eleven banks:
  - `LightBank`, `FogBank`, `LightScatteringBank`, `ShadowBank`, `DofBank`, `ToneMapBank`, `ToneCorrectBank` and `PointLightBank` have 64 rows (IDs 0-63);
  - `LensFlareBank` has 20 and `EnvLightTexBank` 8;
  - `LensFlareExBank` exists too.

  `ParamBank` already loads them all (DrawParam load in `LoadParameters_DES`). A map `mAA_BB_CC_DD` uses area aAA.
- **Part IDs:** every MSBD part carries byte IDs into the banks: `LightID`, `FogID`, `ScatterID`, `LensFlareID`, `ShadowID`, `DofID`, `ToneMapID` and `ToneCorrectID`.
  - Nearly every ID has a row. Only `255` turns up as a missing ID, a handful of times (m01 LightID, m04 FogID).
  - Map pieces mostly use low IDs (0-11), and one ID covers most of a map: m01 LightID 0 (26 of 61), m02 0 (67 of 125), m04 0 (116 of 126).
- **Map pieces against collisions:** they agree on the most-used ID for most banks, but **not for LightBank** (24 of 42 maps differ). Collisions and objects use high IDs (m01 collisions 58-63, objects 58; m02 and m04 collisions and objects 5-7). That fits the DS1 behaviour: characters take the lighting of the collision they stand on, while map pieces keep their own.
- **Field differences from DS1:** the field names match DS1's, but DES stores many as `s32` where DS1 has `s16`. `ToneMapBank` lacks DS1's far bloom and light shaft fields. `LightBank` has an extra hemisphere pair `colR/G/B/A_du` and `_dd`. `ToneCorrectBank` and `EnvLightTexBank` are identical.
- **Units, from the value ranges:**
  - colours are 0-255 with `colA` an intensity percentage (`colA_0` up to 800, `envDif_colA` up to 1000);
  - rotations are degrees;
  - fog `fogBeginZ` and `fogEndZ` are metres (-370 to 3279), `degRotW` looks like the fog's maximum density in % (0-200) and `degRotZ` is always 0;
  - `ShadowBank` has its own light direction, `densityRatio` (0-100), a shadow colour (0-80) and distances;
  - `ToneMapBank` has Reinhard-style `grayKeyValue` (mostly 0.18), adapted luminance limits and `adaptSpeed`.
- **`envDif` / `envSpc_0-3`:** they come in runs of four (0/0-3, 50/50-53, 100/100-103), so they look like IDs of env textures, probably in `other/envlight.tpf`. Unverified.
- **Point lights:** 814 `Light` events, each naming a part (candlesticks, `燭台`) and no region.
  - Their `PointLightID` (up to ~500) is beyond `PointLightBank`'s 64 rows, so it isn't a row ID.
  - It doesn't match any FLVER dummy either: no DES map piece has dummies.
  - Where their positions come from is unknown.

## How the banks map onto `GparamLighting`

| DrawParam | `GparamLighting` | Conversion |
|---|---|---|
| `LightBank` light 0 (`degRotX_0/Y_0`, `colR..A_0`) | `Sun` diffuse | colour /255, `.w` = `colA`/100; direction from degrees (see Phase 1 check) |
| light 1, light 2 | `Light1`, `LocalLight1` | as light 0 |
| `_s` light (`degRotX_s/Y_s`, `colR..A_s`) | `Sun.Specular`, or a specular-only light | DES gives specular its own direction; open question |
| `_u`, `_d` | `HemiUp`, `HemiDown` | /255, `.w` = `colA`/100 |
| `_du`, `_dd` (DES only) | `Models.HemiUp/Down` (hypothesis: the second hemisphere is for characters) | as above; open question |
| `envDif_col*`, `envSpc_col*` | `EnvMapScale` (diffuse, specular) | /255 × `colA`/100 |
| `envDif`, `envSpc_0-3` | env cubemaps | Phase 5 |
| `ToneMapBank` `grayKeyValue`, `minAdaptedLum`, `maxAdapredLum`, `adaptSpeed` | `MiddleGray`, auto exposure bounds and speed | a new `GparamExposureModel.Reinhard` (key / average luminance, clamped) |
| `bloomBegin`, `bloomMul` | `PostEffects.Bloom` threshold and intensity | % |
| `ShadowBank` `beginDist`, `endDist`, `fadeBeginDist`, `fadeDist`, `depthOffset` | `GparamShadow` near, far, fade | metres |
| `ShadowBank` `lightDegRotX/Y`, `densityRatio`, `colR/G/B` | shadow light direction, shadow strength and colour | new fields |
| `ToneCorrectBank` | a colour correction in the tone map | new: brightness × RGB, contrast about mid grey, saturation, hue |
| `FogBank` | classic distance fog | new pass (Analysis.md 3.3) |
| `LightScatteringBank`, `DofBank`, `LensFlareBank` | — | not planned |

## Phases

### Phase 1: one lighting per map (the adapter)

- **The adapter:** a `DrawParamLightingSource` that fills a `GparamLighting` from one row of each bank of the map's area, read through `ParamBank` by **field name**, with values converted to float. That way the same code reads DES's `s32` and DS1's `s16`.
- **Wiring:** `GparamProfile` gets a `DrawParam` source kind for DES (and later DS1R), and `GparamLightingBank.ResolveForViewport` hands DES to the adapter.
- **Which rows:** the most-used ID among the map's map pieces, per bank. That is m01 Light 0, m05 Light 1, m06 Light 1 and m07 Light 20 (the survey's example rows).
- **Choosing rows:** an override picker in the viewport's lighting menu (bank and row, shown by row name), like the GPARAM file chooser.
- **Covered here:** sun, fills, hemisphere, env scale and exposure (the new Reinhard model), plus bloom.
- **Check:**
  - m01_00_00_00 (the Nexus; its LightBank 0 is `神殿部分`, "temple part"): warm sun from the south-east (`degRotY_0` -160), weak fills.
  - m06_00_00_00: orange cave light, `LightBank` 1 `洞窟内`.

  Also check the rotation convention: a shadow should fall where the game's does at the m01 starting area.

#### Phase 1 as built (2026-11-08; not yet checked in the app)

- **The adapter:** `Editors/Map Editor/Data/DrawParamLightingBank.cs`, held by `MapData` and `MapEditorView`.
  - `GparamProfile` has a new `DrawParamLighting` flag; DES now has its own profile with it set (`Supported` stays false, so every GPARAM path stays off).
  - `MapEditorScreen` takes `SceneRenderer.MapLighting` from this bank when it can be used.
  - It caches on the map, the two row IDs and a hash of the rows' values, so Param Editor edits show at once.
- **Lights:** light 0 is the sun, lights 1 and 2 are `Light1` and `LocalLight1` (on when `colA` > 0), and `_s` is the sun's specular colour. `_u`/`_d` are the hemisphere for both the scene and the model set; `_du`/`_dd` aren't used yet.
- **Rotation:** **the pitch is negated.** DrawParam's pitch is the light's elevation: outdoor suns are 10-50, ShadowBank 35-70, and m06's lava light is -60, from below. GPARAM's pitch is the direction of travel. The yaw is used as it is.
- **Exposure:** base = key / √(min·max), auto exposure on, `MiddleGray` = log₂(key / base), and the ev bounds are log₂(key / max / base) and log₂(key / min / base). The adapt speed goes from k per second to AutoExposure.comp's per-1/30-s speed, 1 − e^(−k/30).
- **Bloom:** threshold = `bloomBegin` / 20, alpha = `bloomMul` / 200 (so 20/50 gives the GPARAM defaults), and off when `bloomMul` is 0. Glow is off. This is a guess, to match by eye.
- **Env map scale:** **moved to Phase 5.** Without DES's own env cubemaps, scaling the viewport's stand-in by DES's 200-650 % would only distort it.
- **Menu:** the viewport's Map Lighting section has a DES tab with the shared settings and two row pickers (LightBank and ToneMapBank: "most used by map pieces", or any row by ID and name). A "Use Selected Part's Rows" button takes the selected part's LightID and ToneMapID. The Resolved Values table names each value's row. The overrides are `CFG.Viewport_Map_DrawParam_Light_Row` and `_ToneMap_Row` (-1 = most used).

Expected values for m01_00_00_00 (LightBank 0 `神殿部分`, ToneMapBank 0):
- **Sun:** (1, 0.937, 0.718) × 2.6, elevation 25°, yaw −160°.
- **Fills:** both off (`colA` 0).
- **Hemisphere:** white × 0.1, up and down.
- **Specular:** (1, 0.937, 0.914) × 0.89.
- **Exposure:** 1.42 base, ±0.66 EV adaptation, speed 0.154.
- **Bloom:** threshold 1.0, alpha 0.4.

### Phase 2: shadows and tone correction

- **Shadows:** from `ShadowBank`, with its own light direction, density and colour (DES shadows are tinted; m07 uses 80/80/80).
- **Colour correction:** `ToneCorrectBank` as a colour step in `ToneMap.frag`, which DS1R can reuse.
- **Check:** m01 (`ToneCorrect` 0 darkens to 0.8/0.87/0.83 with contrast 1.1) against m03 (neutral).

#### Phase 2 as built (2026-11-08; not yet checked in the app)

- **Rows:** the adapter reads four banks, LightBank, ToneMapBank, ShadowBank and ToneCorrectBank (`DrawParamRows`). Each bank's row is the most-used by map pieces (`ShadowID`, `ToneCorrectID`) or the menu's override (`CFG.Viewport_Map_DrawParam_Shadow_Row` / `_ToneCorrect_Row`).
- **Shadow direction:** `GparamShadow` has a new `LightAngle`; `SceneRenderPipeline` casts the cascades along it instead of the sun's direction when it's set. Only DES sets it, as `lightDegRotX/Y` with the same elevation/yaw conversion as LightBank.
- **Shadow darkness:** `GparamShadow.Density` = `densityRatio` / 100, which multiplies the viewport's shadow strength.
- **Shadow range:** shadows start at `beginDist` (at least 0.1) and end at `endDist`, fading from `fadeBeginDist`. Where that leaves no fade (m01: end 10, fade begin 10, fade 5), the fade starts `fadeDist` before the end. A row with `endDist` ≤ the start has no shadows. The cascades split the range at 10 %, 30 % and 60 %, since ER's split distances are in kilometres.
- **Shadow colour:** `colR/G/B` (0-80) is **not used**. How the game uses it isn't known.
- **Tone correction:** a new `GparamToneCorrection` on `GparamLighting`, drawn by `ToneMap.frag` on the display colour after the curve and before the LUT:
  - brightness × RGB, then contrast about 0.5 per channel, saturation about the luminance, and a hue rotation about the grey axis;
  - this is a reading of the field names, not the game's shader;
  - with the identity values it changes nothing, so the other games are unaffected;
  - `CFG.Viewport_Post_Tone_Correction` and a menu checkbox switch it.
- **Menu:** the DES tab offers all four row pickers, "Use Selected Part's Rows" (all four) and "Use Most Used Rows". Resolved Values adds a shadow line (light, density, range) and a tone-correction line.

Expected values for m01_00_00_00:
- **ShadowBank 0 `神殿`:** light at 50° elevation and yaw 0, density 0.7, shadows from 0.1 m, fading from 5 m to 10 m.
- **ToneCorrectBank 0:** brightness (0.8, 0.87, 0.83), contrast 1.1, saturation 0.9, hue 0. That is slightly darker, cooler, punchier and less saturated than m03, whose ToneCorrect 0 is neutral.

### Phase 3: distance fog

- **The pass:** a classic fog pass from `FogBank` (begin and end Z, colour, density %), shared with the DS3/BB/SDT `Fog` group (Analysis.md 3.3).
- **Check:** m04_00 (Tower of Latria, fog 0-50 m, dark green-grey at 95 %).

#### Phase 3 as built (2026-11-08; not yet checked in the app)

- **Map names, corrected:** the earlier notes had m01 as Boletarian Palace and m04 as the Valley. Smithbox's DES map aliases give m01 = the Nexus, m02 = Boletarian Palace, m03 = Shrine of Storms, m04 = Tower of Latria, m05 = Valley of Defilement, m06 = Stonefang Tunnel (so the lava reading of its −60° light stands), m07 = Northern Limit and m08 = the tutorial.
- **The pass:** `Renderer/Scene/DistanceFogPass.cs` with `DistanceFog.vert/.frag`, settings in a new `GparamDistanceFog` (on `GparamPostEffects`, off unless a game sets it), switched by `CFG.Viewport_Post_Distance_Fog`.
  - It is a fullscreen draw in `SceneRenderPipeline.CaptureSceneDepth`. It reads the copied depth, so it lands under the transparent draws, and blends as the volumetric fog does (scene × (1 − f) + colour × f).
  - It runs before the volumetric fog draw, because its uniform update has to happen outside a render pass. The two are never on together.
  - Pixels with nothing drawn (the viewport background) aren't fogged.
- **The ramp:** f = clamp((distance − `fogBeginZ`) / (`fogEndZ` − `fogBeginZ`)) × `degRotW` / 100, with the distance along the camera's axis.
- **The colour:** taken as a display colour, so squared into linear light, × `colA` / 100 × the exposure.
- **Adapter and menu:** the adapter reads a fifth bank (`FogID`, `CFG.Viewport_Map_DrawParam_Fog_Row`). The menu has the Fog Bank row picker, a Distance Fog checkbox and a fog line under Resolved Values.

Expected values:
- **m01 FogBank 0:** 10 m to 220 m, density 1.0, colour (54, 50, 120) → (0.045, 0.038, 0.22) × 1.0.
- **m04 FogBank 0:** 0 m to 50 m, density 0.95, colour (50, 70, 62) × 0.05, so very dark.

Open:
- **`colA` and `degRotW`:** which of them is the fog's density and which the colour's intensity isn't settled. m08's `colA` 0 makes its 30 % fog black, and m02's `colA` 10 makes Boletaria's 50 % fog nearly black. If those look wrong, `colA` is probably an opacity, not an intensity.
- **Sky domes:** they are distant map pieces, so they get fully fogged. The game probably exempts its sky shaders.


### Phase 4: per-part lighting

- **The problem:** map pieces and the parts standing in a collision take different rows; m01's palace and its interior use LightBank 0, 3 and 11. Phase 1 picks one row per map.
- **The work:** per-instance lighting, a light-set index per draw into a small table of resolved `GparamLighting`s. That's needed anyway for the DS3/SDT/BB per-part value IDs (Analysis.md step 4), so build it once for all of them.
- **Characters and objects:** take the LightID of the collision under them, which needs a hit test, or their own LightID as a first step.

#### Phase 4 as built (2026-11-08; not yet checked in the app)

Each part is lit by the LightBank row its own `LightID` picks. Characters standing in a collision still use their own `LightID` rather than the collision's; that comes next if needed.

- **The table:** `Renderer/Scene/LightSetTable.cs` holds up to 127 light sets in the material buffer, entry 0 unused.
  - Each set is a `LightSetData` of 320 bytes: the sun, the hemisphere, the model set's four lights and its hemisphere.
  - Each is folded with intensity and exposure exactly as `SceneRenderPipeline` folds the scene's, so a part lit by its own set looks as the whole scene would under that row.
  - It is filled every frame from `SceneRenderer.MapLightSets` at the scene's exposure, and uploads only when something changed.
  - `SceneParam.LightSetStart` is **appended at the end**, so the shaders that copy only the start of the struct are unchanged.
- **The index:** `MeshRenderableProxy.LightSet` (0 = the scene's lighting) is set on a part's proxy and copied to its submeshes, as `Lightmap` is.
  - It is written into bits 24-30 of the instance's `MaterialID`, only for meshes drawn with the flver shader. Material IDs are entry indices in a 32 MB buffer, far below 2^24.
  - `FlverShader.vert` now masks the material ID with `0x00FFFFFF` (it was `0x7FFFFFFF`) and passes the set in bits 8-14 of the flat `fsin_flags`, so no new varying.
  - **No other shader changed:** nothing else reads `materialID.x`, and C# never reads `MaterialID` back.
- **The shader:** `FlverShader.frag` reads `lightsetdata` (set 4) through new helpers: `SunTravel`, `SunColor`, `SunSpecColor`, `ModelLight` and `Hemisphere`.
  - They use the set only under map lighting and when it isn't 0, so with no sets the shader does exactly what it did before.
  - The shadow cascades stay the scene's (ShadowBank's light), so a part's own sun shades it but doesn't change its shadows.
  - All 32 variants were recompiled with the VariantCompiler.
- **Assignment:** `DrawParamLightingBank.UpdatePartLighting`, called every frame from `MapEditorScreen` after the scene's lighting.
  - Each distinct (area, LightID) other than the scene's own row gets a set (rows resolved once and re-resolved when their values change; each row is hashed once per frame).
  - Each part entity whose `RenderSceneMesh` is a `MeshRenderableProxy` gets its set. Parts that leave go back to 0.
  - Missing rows (LightID 255) and composite characters (the player, built from part models) keep the scene's lighting.
- **Off-switches:** everything goes back to the scene's lighting when the "Light Parts by Their Own LightID" checkbox (`CFG.Viewport_Map_DrawParam_Per_Part`) is off, when map lighting is off, and when a specific LightBank row is chosen in the menu, which then lights everything.
- **Menu:** the checkbox shows how many parts are lit by how many rows of their own.

Expected for m01 (the Nexus):
- The scene's row is LightBank 0, so 26 of its 61 map pieces keep it.
- The others get sets for LightBank 3 (22 pieces), 11 (10) and 1 (2).
- Objects use 58 (71), 11, 0 and 6.
- Enemies use 0, so they stay on the scene's lighting.

### Phase 5: env textures, then point lights

- **Env textures:** link `envDif` and `envSpc_0-3` to textures (survey `other/envlight.tpf` and `EnvLightTexBank`).
- **Point lights:** find where the `Light` events' positions and `PointLightID`s resolve before feeding `MapLightCollector`. Neither a region nor a dummy is involved; the EBOOT is encrypted, so this is data-only research.

#### Phase 5 research (2026-11-08)

The evidence is from `Tools/DesDrawParamSurvey` modes `envtex` (writes `envtex.md`) and `envexport` (writes `envexport.md` and decodes every env cubemap to `envtex/*.png`, six faces in a strip), and from `Tools/DesEnvProbe` (modes `shaders`, `mtds`, `mtdcorr`, `peaks`, `lightpeaks`).

- **Where the textures are:** `envDif` = N names `EnvDif_mAA_NNN` and `envSpc_k` = N names `EnvSpc_mAA_NNN`, cubemaps in `map/mAA/mAA_9999.tpf`. Names compare case-insensitively (`envdif_m08_100`, `Envspc_m06_014`). **Every ID of every LightBank row in every area resolves** (320 of 320 per bank; both `mAA_` and `sAA_LightBank`). `other/envlight.tpf` holds a default set, `EnvDif_def_000` and `EnvSpc_def_000-003`.
- **The textures:** 32² to 128² cubemaps, DXT1 (TPF format 0) or A8R8G8B8 (format 10), with or without mips.
- **`EnvSpc_0-3` are four specular powers of the same light rig.** Peaks rise and averages fall from 0 to 3 (m01: peak 115 → 191 → 255 → 165, average 26 → 30 → 19 → 12); `_003` is a few pinpoints, `_000` broad blobs. `EnvDif` is the same rig convolved for diffuse.
- **The MTD picks the map:** `g_EnvSpcSlotNo` (0-3) is in every DES MTD (slot 0: 134, 1: 40, 2: 52, 3: 39). It loosely follows `g_SpecularPower`: slot 1 is power 4-8, slot 2 is 5-70 (mostly 7), slot 3 is 60 (35 of 39); slot 0 is the default for everything.
- **The lighting model is per material, from the MTD's `g_LightingType`:**

  | `g_LightingType` | Materials | Pixel shader family (`ds_flver.shaderbnd`) |
  |---|---|---|
  | 3 | all 91 `map` MTDs, the area MTDs (stone, wood, water, lava) | `…_HemEnv`, `…_HemEnvLerp`: hemisphere + env cubemaps |
  | 1 | characters (40), parts/armour (21), damage, most SFX | `…_HemDir3`: hemisphere + three directional lights |
  | 0 | sky, clouds, light shafts, additive SFX | unlit |

  Every variant also comes with `Sdw`/`Csd` (shadowed), `Lit` (lightmapped) and `PntS`/`PntSS`/`PntSSSS` (1, 2 or 4 point lights). So in the game **map pieces aren't lit by LightBank's three directional lights at all**; their light comes from the env cubemaps, scaled by LightBank's `envDif_col*` and `envSpc_col*`. Only characters and parts use the three lights.
- **`_du`/`_dd`, a reading:** with the split above, `_du`/`_dd` ("Dir up/down") is probably the hemisphere of the `HemDir3` shaders (characters) and `_u`/`_d` that of `HemEnv` (map). The magnitudes fit: m01 row 0 has `_u`/`_d` at 0.1 beside an `envDif` scale of 4.0, and `_du`/`_dd` at 1.6/1.44 beside a 2.6 sun.
- **`EnvLightTexBank` is unused at run time:** every row in every area (loose `mAA_envlighttexbank.param` and in the parambnd) has `isUse` 0 and default values. Its fields (ten directional lights, `resNameId_Dif0/Spc0-3`, `sepcPow` per map) describe the authoring tool that baked these cubemaps.
- **The rigs don't follow LightBank:** identical cubemaps are reused across areas (m05's `_000-003` are m01's, m08's `_010-013` are m02's, m99's are m08's). Their peaks don't line up with the rows' lights, so they can't settle the yaw convention. m01's brightest pinpoint is at 28° elevation, which agrees with the elevation reading, but that alone proves little.
- **Loader bug:** PS3 TPF format 10 (`EnvDif/EnvSpc` of m02, m04, m06, m08 and m99) is 4 bytes a pixel in A, R, G, B byte order after deswizzling. The Headerizer's DDS header calls it 24-bit RGB, so `TexturePool.FillWithTPF` takes its "24-bit formats" branch and **drops it**. This affects every DES format-10 texture, not just the env maps.

#### Phase 5a as built: env lighting (2026-11-08; not yet checked in the app)

The user chose the game's split by `g_LightingType`.

- **Loader fix:** `TexturePool.FillWithTPF` reorders PS3 format-10 data from A,R,G,B to R,G,B,A (`Ps3ArgbToRgba`) and uploads it as `R8G8B8A8Srgb`. That covers every DES format-10 texture, env map or not.
- **Material flags** (`Material.uvSet`, above the UV set; `FlverResource.MarkModelLighting`, DES only, from the MTD):
  - `g_LightingType` 1 sets `ModelLightingFlag`, whatever the model's name.
  - 3 sets `EnvLightingFlag` (0x200), with `g_EnvSpcSlotNo` in bits 10-11 and `g_ShadowPowMul` in fifteenths in bits 12-15 (default 0.8).
  - Any other value, or no MTD, is lit as before.
- **Adapter:** `ApplyEnv` fills a new `GparamLighting.Env` (`GparamEnvLighting`) from the LightBank row: `EnvDif_mAA_NNN`, `EnvSpc_mAA_NNN` ×4, and the scales `envDif_col*`/`envSpc_col*` (rgb/255, colA/100). The model set's hemisphere is now **`_du`/`_dd`**; the scene keeps `_u`/`_d`.
- **Light sets:** `LightSetData` grew from 320 to 384 bytes, with the env scales (folded with the exposure) and five cube-pool indices (`NoTexture` = not loaded, looked up again each frame). **Entry 0 is now the scene's own lighting**, so the shader reads env from `lightsetdata[start + set]` for every part.
- **Shader** (`FlverShader.frag`, all variants recompiled): `EnvLit()` is true for flagged materials whose set has env on. For them:
  - **No sun:** the sun's direct light is skipped.
  - **Diffuse:** `EnvDif(N) × scale × shadow` is added to the hemisphere, unless a DES lightmap already supplies the light from around.
  - **Specular:** `specular map × EnvSpc[slot](R) × scale × shadow`.
  - **Shadow:** `mix(1, sunShadow, g_ShadowPowMul)`, with the cascades still along ShadowBank's light.
  - **Cube lookup:** `D × (1, 1, −1)`, the convention the other games' env maps use; unverified for DES.
- **Menu:** the DES tab has a checkbox, "Light Map Materials by Env Cubemaps" (`CFG.Viewport_Map_DrawParam_Env_Lighting`). Off, map materials take the sun as before. Resolved Values has an Env line naming the cubemaps, their scales, and how many of the five are loaded.
- **Not done:** `g_LightingType` 0 (unlit: sky, clouds, light shafts) is still lit as before; `HemEnvLerp` (probably a blend between two sets) isn't modelled.

Check in the app:
- **m01_00_00_00 (the Nexus), map pieces:** they lose the hard sun and show the env light; the Env line reads `EnvDif_m01_000 … 5 of 5 loaded`. Shiny materials (slot 3, power 60) show small sharp highlights; slot 0 shows broad ones.
- **m02_00_00_00 (Boletaria):** its cubemaps are format 10. Before this fix they never loaded, so 5 of 5 shows the loader fix working.
- **Characters on m01:** still lit by LightBank's three lights, now with the brighter `_du`/`_dd` hemisphere (1.6 / 1.44).
- **Toggling the checkbox** should switch between the env look and the old sun look.
- **If the env light looks upside down or mirrored**, the cube convention is wrong; say which way.

## Open questions

| Question | How to settle |
|---|---|
| Rotation convention of `degRotX/Y` | Pitch settled from the data as the light's elevation (negated for GPARAM, see Phase 1 as built). The yaw's zero and sign are still open: compare the shadow direction in the Nexus (m01) with the game |
| `_s` light: separate specular direction, or the sun's specular | Compare in the app on shiny materials (armour, m01 floor) |
| `_du` / `_dd`: characters' hemisphere? | Light a character with and without; DS1 has no such pair |
| `colA` as % of intensity, or a scale whose 100 means 1.0 in HDR | Phase 1: m01's `colA_0` 260 against m04's 0 (lightmapped "焼付けあり" rows have low direct light) |
| Fog `degRotW` = density % | Phase 3 in the app on m04 |
| `envDif`/`envSpc` IDs → which textures | Survey `other/envlight.tpf` names against the IDs |
| `PointLightID` and point light positions | Survey every Light event's part kind and model; look for a per-map light file |
| Which part's IDs the game uses for the camera/scene (map pieces, or the collision the player is in) | Phase 4; the data says collisions differ for LightBank in 24 maps |
