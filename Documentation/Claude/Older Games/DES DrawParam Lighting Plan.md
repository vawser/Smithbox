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
