# GPARAM lighting: implementation plan

Goal: light Map Editor viewports with the map's own GPARAM LightSet and Tone Map values (sun, hemisphere ambient,
exposure), in the game's units, so the sun and the irradiance volumes (Plan.md) share one scale and
`IrradianceVolumeTable.AutoExposure` is no longer needed.

Scope: AC6, ER, NR first (the games with volumes). DS3, SDT, BB share the LightSet layout and follow in Phase 6.
Each phase ends in something to check by eye; Smithbox is not launched to verify (CLAUDE.md).

## What the data looks like (dumped 2026-11-03)

From NR `default`, `mapbase`, `m10_00_0000..0400` and AC6 `m00_10_0000` (NR and AC6 drawparam files are loose
on disk; ER's go through the VFS).

- **Colours are Vector4: rgb colour, w intensity in game HDR units.** NR m10: sun `DiffColor0` = (0.69, 0.68, 0.67)
  x 0.2, `Hemi Color Up` = (1, 0.96, 0.88) x 0.1, `Hemi Color Down` = (0.28, 0.25, 0.23) x 0.01. The volumes'
  light is 0.01-0.03 in the same units. Tone Map `Exposure` is 4.5 there, so sun x exposure = 0.9: Exposure looks
  like a linear multiplier, not EV stops (to confirm in Phase 2).
- AC6 runs hotter: sun w 2-3, `Exposure` 1.75 with auto exposure on (`Middle Gray` -5.5, a log2 value).
  ER map files switch auto exposure on as well (m10_00_0000: Exposure 3.35-4.1 through the day, Middle Gray -6.4;
  m60_00_0000: Middle Gray -6 to -8); only `mapbase`/`default` leave it off.
- **Angles are Vector2 radians.** x is always negative in the samples (-0.17 to -1.05), so x is the elevation with
  negative meaning above the horizon, and y the yaw. Proposed direction the light travels:
  `(cos x * sin y, sin x, cos x * cos y)`, so negative x points down.
  `FogVolume.frag` uses the negation of that as the travel direction for `Light Overwrite`, which would send
  the light upwards; one of the two is wrong (Phase 2 check).
- **Map files are sparse.** NR m10_00_0000 lists 27 groups, most empty; empty groups and missing fields fall back
  to `mapbase` (ER, NR) or `default` (NR). `FogVolumeBank.GetSettings` already walks `mAA_BB_0000`, then
  `mapbase`, then the map's other files, for one field group; this generalises it per field.
- **Each field holds values keyed by ID, each with a TimeOfDay key.** Keys are interpolated through the day.
  ER map files key the sun at t = 0, 4, 8, 12, 15, 21, 22.5: seven keys, as `CutsceneGparamTimeParam` has seven
  timezones (morning, noon A, noon B, evening, night, deep night A, deep night B) and ER has seven GI bakes.
  t12 has a red sun and t15 turns it off for moonlight, so t0 is morning, not midnight (perhaps hours after 06:00).
  Some groups add other keys (Tone Map t11.5, sun t15.08/15.33).

### Value IDs (settled 2026-11-03, from data and exe strings; the selecting code is not traced)

- **IDs are numbered per group, not per file.** In NR m10_00_0000 FogVolume uses 1000-1093 (the FVB's fog IDs,
  as `FogVolumeBank` already reads them) while LightSet uses 0/100/1000/1100; AC6's Lens Simulation uses 700-830.
- **LightSet has two selections in the engine, one for the scene and one for models.** eldenring.exe's cutscene
  draw-param overrides list a begin/end bank and ID for every group (`beginShadowBank`, `beginToneMapId`, …),
  except LightSet, which has two: `LightSetAll` and `LightSetModel`.
- **LightSet IDs work as offsets on a base**, measured over all 231 ER map gparams with a LightSet:

  | ID | Differs from ID 0 in | Meaning |
  |---|---|---|
  | 0 | | Scene lighting: sun, hemisphere |
  | +100 | `Light1 FollowedCamera`, `Angle1`/`DiffColor1` (~115 files), LocalLight2's colours (~120) | Model lighting: camera-following fill on characters |
  | +1000 | `Enable LocalLight2` (127 files) | A further local light, still unknown |
  | 10, 11 | Sun and fills | The menu's offscreen render (`MenuOffscrRendParam.GparamID` = 10, 11 on PS4/XB1) |

  The sun (`Angle0`, `DiffColor0`, `SpecColor0`) and hemisphere colours are the same in 0/100/1000/1100 in all but
  5 files. AC6 m00_10_0000 (likely the garage) is the exception that fits: ID 0's sun is 0 and ID 100's is 2-3, so
  only models are sunlit there.
- **Rule for Smithbox:** map pieces, assets and collisions take LightSet ID 0. ID 100 is the model set; it matters only
  for the fill lights (Phase 4) and for maps like AC6's garage. Other groups (Tone Map, EnvironmentConfig) take ID 0.
- **Weather picks values too:** ER `WeatherParam.GparamId` gives each weather an ID (0 sunny, 1, 10, 20, 30, 31, 40,
  50, 60, 81-83, 99, and 600-800s for regional weathers), and ER files are named `mAA_BB_CCWW`
  (`m60_00_0010`, `_0020`, `_0030`, `_0031`, `_0040`), with `m%02u_%02u_%04u_WeatherBase` / `_WeatherOutdoor` formats in
  the exe. So ER's WW looks like the weather's GparamId and CC like the area sub ID. Weather selection is Phase 5.
- **Files per area.** NR m10 has `m10_00_0000..0400`. Collisions carry a scene GPARAM struct (AC6 names:
  `GparamSubID_Base`, `GparamSubID_Override1/2`; NR: `Unk18`, `Unk1D`, `Unk20`). In NR m10 `Unk18` is -1 or 0-3,
  which matches files 0000-0300 if sub ID N means `mAA_BB_0N00`. Untraced hypothesis.
- Part `GparamStruct.LightID` is -1 on all but one asset in NR m10 (LightID 10), so per-part overrides exist but
  are rare. Out of scope.

## Phase 1: read and resolve (no rendering change)

1. New `Editors/Map Editor/Data/GparamLightingBank.cs`, owned beside `IrradianceVolumeBank`:
   - `CanUse()`: AC6, ER, NR.
   - `Resolve(mapId, file, valueId, hours)` returns a `GparamLighting` (new, `Renderer/Scene/GparamLighting.cs`):
     sun angle, sun diffuse and specular colours, hemisphere up and down, Light1 / LocalLight1 / LocalLight2
     (angle, colours, enable, follow camera), Tone Map exposure, auto exposure on/off and middle grey,
     EnvironmentConfig envmap colour convert, BG colour. Plus, per field, the file and ID it came from.
   - Per field: the chosen file, then `mapbase`, then `default`. Within a file: the chosen ID, else ID 0, as
     `FogVolumeBank.ReadSettings` does. Two resolutions per frame: the scene set (LightSet ID 0) and the model set
     (LightSet ID 100), see "Value IDs".
   - TimeOfDay: values of one ID sorted by key, linear between neighbours, wrapping 24 -> 0. A single value applies
     all day. Vectors lerp per component; booleans and ints take the earlier key.
   - GPARAM reads cached per file name like `FogVolumeBank.GetFogGroup`, read through
     `MapData.PrimaryBank.TargetFS`, so ER works through the VFS and project overrides win.
   - `ListFiles(mapId)` (the map's `mAA_BB_*` files) for the UI.
2. Cache invalidation: hook the Gparam Editor's save to clear this bank's cache and `FogVolumeBank.ClearSettingsCache`,
   which has no caller today.
3. CFG: `Viewport_Map_Lighting` (default on), `Viewport_Map_Lighting_File` ("" = `mAA_BB_0000`),
   `Viewport_Map_Lighting_Hours` (4, the first daytime key). No value ID setting: the scene set is ID 0.
4. Viewport Rendering > Map Lighting gets a read-only "Map lighting" section: the file and ID in use, the sun's colour, intensity
   and angle, exposure, and which file each value came from.
5. Temporary log line per map: files found, value IDs, the resolved sun, hemisphere and exposure.

Which map: the first loaded map with a GPARAM. With several maps loaded, a "Lighting from" combo picks one.

Check (log and menu):
- NR m10_00_00_00, file 0000: scene sun (0.69, 0.68, 0.67) x 0.2, angle (-1.047, -0.698), hemisphere
  up (1, 0.96, 0.88) x 0.1, down x 0.01, exposure 4.3 with auto exposure on (middle gray -6), all from
  `m10_00_0000 #0`. Model set: the same sun, plus Light1 (camera-following) at 0.03.
- NR file 0400: sun intensity 0, exposure 4.5.
- AC6 m00_10_00_00: scene sun intensity 0; model sun x 3 at t0, x 2 at t4-t12, bluish (0.6, 0.74, 1) x 0.75 at
  t15-t22.5. At t13.5 the model sun is halfway between 2 and 0.75.
- ER m10_00_00_00 at t4: sun (0.98, 1, 0.99) x 0.4; at t12 (1, 0.94, 0.86) x 0.35; Exposure 3.5 at t4.
  An overworld tile (m60_42_36_00) resolves through the VFS without errors.

### Phase 1 as built (2026-11-03)

- `Renderer/Scene/GparamLighting.cs`: `GparamLighting` and `GparamDirectionalLight` (sun, Light1, LocalLight1/2),
  hemisphere, Tone Map, EnvironmentConfig, BG colour, and `Sources` (field key -> "file #id").
  `AngleToDirection` holds the proposed formula, unverified until Phase 2.
- `Editors/Map Editor/Data/GparamLightingBank.cs`, on `MapData` beside the GI bank (`MapEditorView.GparamLightingBank`):
  `Resolve(mapId, file, lightSetId, hours)`, `ChooseFile` (a chosen file only applies to maps of its area),
  `ListFiles`, `ClearCache`. Per field: the file, then `mapbase`, then `default`; within a file the ID, else ID 0.
  TimeOfDay is interpolated with wrap-around; angles take the short way round in yaw. Colours stored as `Color`
  fields are converted to Vector4 (0-1). Light1 has no switch of its own (`Enable LocalLight1/2` belong to the
  local lights), so its intensity decides.
- `GparamBank.SaveGraphicsParam` clears this bank's cache and `FogVolumeBank`'s on a successful save.
- CFG: `Viewport_Map_Lighting` (not read yet), `Viewport_Map_Lighting_File`, `Viewport_Map_Lighting_Hours` (4).
- Viewport Rendering > Map Lighting: file combo, time slider, and per loaded map a table of scene (#0) and model (#100) values with
  the scene value's source. The file combo and time slider moved here from Phase 5, as the checks need them.
- Temporary log line per map on load; removed after the in-app checks passed (2026-11-03).
- Nothing is rendered differently.

## Phase 2: sun and exposure

1. `SceneParam`: append `SunSpecColor` (Vector4) and `Exposure` (float), keeping the GLSL copies in
   `FlverShader.frag` (and `FogVolume.frag`, which reads `sceneparam`) in step; appended at the end, as before.
2. `SceneRenderPipeline.UseMapLighting` (set like `UseIrradianceVolumes`, Map Editor only). When it and the CFG
   are on, `UpdateSceneParameters` writes `LightDirection` from the angle, `SunColor` from `DiffColor0`,
   `SunSpecColor` from `SpecColor0` and `Exposure` from Tone Map. Otherwise `SunSpecColor = SunColor`, `Exposure = 1`:
   today's look, and the only path outside the Map Editor.
3. `FlverShader.frag`: `ShadeLight` takes separate diffuse and specular radiance (the BTL lights pass the same
   value for both). `litColor *= sceneparam.exposure` before `sqrt`.
4. GI: with map lighting on, `IrradianceVolumeExposure = Viewport_GI_Intensity` (game units, no auto exposure);
   the GI menu greys out Auto Exposure and says why.
5. `ViewportMenu.SceneParamsGui`: the sun direction, colour and intensity sliders are disabled while map lighting
   is on, with a note pointing at Viewport Rendering > Map Lighting.
6. Auto exposure: ER and AC6 map files switch it on, so the game's real exposure depends on what is on screen.
   Until there is an HDR target to measure, use `Exposure` as is, and offer `Viewport_Map_Lighting_Exposure_Scale`
   (default 1) for maps that look wrong. Record per game how far off it is, to size a later auto-exposure pass.

Check:

| Map | Expect | If wrong |
|---|---|---|
| NR m10_00_00_00 | Lit faces about as bright as before; slightly warm sun; interiors from the volumes dimmer relative to the sun than with auto exposure | Everything blown out or black: Exposure is EV, use `exp2(Exposure)` scaled |
| NR m10_00_00_00 | The sun comes from above, from the direction the user knows from the game | Undersides lit: flip the angle's sign. Wrong side horizontally: mirror yaw (Smithbox flips Z for the cubemap) |
| Same, toggle Map Lighting | Off returns exactly today's image | |
| ER fog volume with `Light Overwrite` | Its lit side matches the sun's | Fix `FogVolume.frag` to the same formula as the sun |
| AC6 m00_10_00_00 | Scene sun off (ID 0); the hangar lit by the volumes alone | Note the exposure scale that looks right |

### Phase 2 as built (2026-11-03; checked in the app by the user the same day)

Confirmed: `AngleToDirection`'s formula (negative elevation points down), the sun colours and the exposure.
Not reported separately: the fog volume `Light Overwrite` direction, which uses the negated formula in `FogVolume.frag`.

Changed from the plan: the exposure is applied on the CPU to the terms in game units (the sun and the volumes)
instead of to `litColor` in the shader. A shader-wide exposure of ~4 would have multiplied the grey cubemap's
ambient (0.5, not in game units) and the emissive maps too; they stay as they were until Phase 3 replaces the grey
ambient with the hemisphere and an HDR target makes a real exposure pass possible. So there is no `Exposure` in
`SceneParam`.

- `SceneParam`: `MapLightingFlags` (bit 0: the sun is from GPARAM) then `SunSpecColor`, appended so `SunSpecColor`
  lands on a 16-byte row, as std140 needs; `FlverShader.frag`'s copy matches. `SunSpecColor = SunColor` when map
  lighting is off.
- `SceneRenderer.MapLighting`, set every frame in `MapEditorScreen.Update` by `GparamLightingBank.ResolveForViewport`:
  the first loaded map by name with its own `mAA_BB_0000`, the menu's file and time, LightSet ID 0. Cached until
  one of those changes or the cache is cleared.
- `SceneRenderPipeline.UseMapLighting` (Map Editor viewports, set in `VulkanViewport`), `ActiveMapLighting`. On upload,
  a copy of `SceneParams` gets `LightDirection = AngleToDirection(Angle0)`, `SunColor = DiffColor0` and
  `SunSpecColor = SpecColor0` with w x exposure, and `IrradianceVolumeExposure = GI Intensity x exposure`.
  The viewport's own sun stays in `SceneParams`, so switching map lighting off brings it back.
  Exposure = Tone Map `Exposure` x `Viewport_Map_Lighting_Exposure_Scale`.
- `FlverShader.frag`: `ShadeLight(s, L, radiance, specularRadiance)`; scene lights pass the same value twice.
  SPIR-V rebuilt. The water volume's sun term and `FogVolume.frag` read the same `LightDirection`, so they follow
  the GPARAM sun.
- Menus: Map Lighting gets the toggle, the exposure scale and "Lit from X, exposure Y". Scene Lighting's sun
  direction, colour and intensity are disabled while map lighting is active. The GI menu's Auto Exposure is disabled
  then, with a tooltip saying why.

Check:

| Map | Expect | If wrong |
|---|---|---|
| ER m10_00_00_00 at time 4 | Sun (0.98, 1, 0.99) x 0.4 x 3.5 = 1.4: lit faces a little brighter than with the old white sun at 1. Map Lighting says "Lit from m10_00_0000, exposure 3.5" | |
| Same | The sun from above, from the side you know from the game (e.g. the courtyard's lit walls) | Undersides lit: negate the elevation in `AngleToDirection`. Wrong side horizontally: negate the yaw (or swap sin/cos), Smithbox may mirror Z |
| Same, GI on | Interiors much darker relative to the sun than with auto exposure (volumes x 3.5 instead of scaled to 0.5); outside the volumes, ambient is still the grey cube | Too dark to work in: raise Exposure Scale, and note the value per map |
| Same, drag Time of Day 4 -> 12 -> 15 | Sun reddens towards 12, then goes out at 15 (night), moonlight from about 15.3 | |
| NR m10_00_00_00 | Sun x 0.2 x 4.3 = 0.86, from angle (-1.047, -0.698) | |
| AC6 m00_10_00_00 | No sun at all (scene sun intensity 0); the hangar lit only by the volumes and the grey cube | |
| Shiny surface, any map | Highlights tinted by `SpecColor0` (very close to the diffuse colour in vanilla files) | |
| Toggle Light with the Map's GPARAMs off | Exactly the old image, the sun sliders enabled again with their old values | |
| ER fog volume with Light: Overwrite | Its lit side matches the sun's | `FogVolume.frag` uses the negated formula; fix whichever is wrong |

## Phase 3: hemisphere ambient and envmap colour

1. `SceneParam`: append `HemiUp`, `HemiDown` (Vector4, rgb x w).
2. `IndirectLight`: with map lighting on, ambient diffuse outside the volumes is `mix(hemiDown, hemiUp, N.y * 0.5 + 0.5)`
   instead of the grey cube's: `mix(hemi, volumeLight, coverage)`. Whether the game adds the hemisphere inside the
   volumes too is unknown; `Viewport_Map_Lighting_Hemi_Inside_Volumes` (default off) adds it, so the user can
   compare against the game.
3. Envmap specular: apply `EnvMapColorConvert` Saturate (lerp towards luminance) and Scale (rgb x w).
   Hue and Shift are left out until their formula is known (both are 1 in every sample, so Shift is not an offset).
4. BG colour as the viewport clear colour while map lighting is on.

Check: AC6 m00_10_00_00 outside the hangar box: ambient tinted (0.80, 0.86, 0.78), tops brighter than undersides,
instead of flat grey. AC6 envmap Saturate 0.35: reflections clearly less saturated. NR m10: no visible change
inside volume boxes with Hemi Inside Volumes off.

### Phase 3 as built (2026-11-03)

Decided with the user: with map lighting on, **reflections come from the hemisphere too**, evaluated along the
reflection vector, instead of the grey cube. The grey cube is not in game units, and desaturating it would do
nothing. The map's real envmap cubemaps replace this later.

- `SceneParam`: `HemiUp`, `HemiDown`, `EnvMapScale` (rgb with intensity, exposure or w folded in on the CPU),
  `EnvMapSaturate`, and three pad floats to fill the row. `MapLightingFlags` bit 1: add the hemisphere inside volumes.
- `FlverShader.frag`: `MapLighting()`, `Hemisphere(D)` = `mix(down, up, D.y * 0.5 + 0.5)`, and `AmbientBase(N)`
  (the hemisphere with map lighting, else the cube's mip 5). `IndirectLight` and the water's sky term use
  `AmbientBase`. Reflections with map lighting: `Hemisphere(reflect)`, lerped towards luminance by Saturate,
  times Scale, then the GI reflection normalisation as before. SPIR-V rebuilt.
- CFG `Viewport_Map_Lighting_Hemi_Inside_Volumes`, toggle in Map Lighting. Compared with the game by the user
  (2026-11-03): the hemisphere does add inside the volumes, so it defaults to on.
- BG colour (added after the checks): each viewport already fills its region with `VulkanViewport.ClearQuad`
  (a `FullScreenQuad` in `CFG.Viewport_Bg_Color`), so `FullScreenQuad.Render` takes an optional colour and
  `SceneRenderPipeline.MapBackgroundColor` gives it: `sqrt(rgb x w x exposure)`, clamped to 1, treating the BG colour
  as being in game units like the lights, with the shaders' gamma. CFG `Viewport_Map_Lighting_Background` (on),
  toggle in Map Lighting.
- Hue and Shift of EnvMapColorConvert are still left out.

Check:

| Map | Where | Expect |
|---|---|---|
| AC6 m00_10_00_00 | Outside the hangar box | Ambient (0.80, 0.86, 0.78) x 0.45 x 1.75 ≈ (0.63, 0.68, 0.62) on tops, x 0.2 ≈ (0.28, 0.30, 0.27) on undersides, instead of flat grey. Reflections faint and greyish (Saturate 0.35) |
| NR m10_00_00_00 | Anything outside the Env boxes | Tops warm, ≈ 0.43; undersides almost black (0.01 x 4.3): very contrasty, as NR's file says |
| ER m10_00_00_00, time 4 | Outside the volumes | Up (0.89, 0.92, 0.91) x 0.1 x 3.5 ≈ 0.31 |
| Any, inside a volume | Toggle Add the Hemisphere Inside Volumes | Off: same as Phase 2. On: brighter, the tops more than the undersides. Say which looks more like the game |
| Shiny floors outside volumes | | Reflect the sky colour; walls facing down reflect the darker ground colour |
| Map lighting off | | Exactly the old image (grey cube ambient and reflections) |
| NR m10_00_00_00 | Background | (0.035, 0.031, 0.024) x 1 x 4.3, square-rooted: about (0.39, 0.37, 0.32), a dark warm grey |
| ER/NR maps using `mapbase`'s BG | Background | (0.5, 0.5, 0.5) x 0.5 x 3.5 -> about 0.94, near white. If that looks wrong, w is not an intensity here: drop it from `MapBackgroundColor` |
| Toggle Use the GPARAM's Background Color off | Background | The settings' background colour again |

## Phase 4: extra directional lights

These come from the model set (LightSet ID 100), so they light characters and objects, not map pieces.
Light1 (`Angle1`, `DiffColor1`, `SpecColor1`, `Light1 FollowedCamera`) and LocalLight1/2 (with `Enable LocalLight*`)
are directional lights. FollowedCamera turns the angle relative to the camera's yaw. Needs a per-draw switch between
the scene and model sets (a `materialFeatures` bit or a second set of sun values in `SceneParam`), set for
character and enemy renderables. Default off, behind `Viewport_Map_Lighting_Model_Lights`.

Check: NR m10 has Light1 on with FollowedCamera at intensity 0.03: a faint fill on enemies that turns with the
camera, and none on map pieces. AC6 m00_10_00_00: ACs sunlit at x 2-3, the hangar not.

### Phase 4 as built (2026-11-03)

- Per-draw switch: a material flag. `InstanceData`'s four words are all used, so the flag is
  `Material.ModelLightingFlag` (0x100) in the first UV-set value, above the UV set itself (0/1); the shader masks
  it off where it reads UV sets. `FlverResource.MarkModelLighting` sets it, after `ConfigureMaterialShader` in both
  FLVER material paths, via `IsCharacterModel`: the resource's name is `c` + four digits
  (`ResourceLoadContext.Name`), or it comes from the parts folder (container `*.partsbnd*`, or a loose file under
  `parts/`). The second rule is for characters built from parts: AC6's ACs are `Enemy` parts of model c0000
  (m00_10_00_00 has c9980 and the player's c0000; m01_00_30_00 has two c0000 enemies), and c0000 is drawn by
  `CharacterComposition` from part FLVERs, never its own. The same covers players and player-like NPCs in the
  other games, and their weapons. The parts folder holds only bodies and equipment, so no map geometry is flagged.
- `GparamLighting.Models`: the ID 100 set, resolved with the scene's in `ResolveForViewport`.
- `SceneParam`: `ModelLight0-3` (`MapDirectionalLightData`: direction, colour, spec colour; the models' sun, Light1,
  LocalLight1, LocalLight2, with intensity x exposure, 0 when off) and `ModelHemiUp/Down`. `MapLightingFlags` bit 2.
- Light1's `FollowedCamera`: the angle's yaw plus the camera's yaw, `atan2(forward.x, forward.z)` with forward the
  view's -Z in world space, the same axes as `AngleToDirection`.
- `FlverShader.frag`: `g_modelLit = ModelLit()` per pixel. Model-lit pixels take the four model lights instead of
  the sun, and the model hemisphere in `Hemisphere()` (so their ambient and reflections too). BTL scene lights
  and the volumes are unchanged. SPIR-V rebuilt.
- CFG `Viewport_Map_Lighting_Model_Lights` (off), toggle "Light Characters with the Model Set".

Check (with the toggle on):

| Map | Expect | If wrong |
|---|---|---|
| NR m10_00_00_00, any enemy | Same sun as the scene; a faint extra fill (x 0.03 x 4.3) from the camera's side that moves as the camera turns; map pieces unchanged | Fill on the wrong side as the camera turns: negate `cameraYaw` |
| ER m10_00_00_00, an enemy | A fill light (Light1 is on in ~110 of ER's files for ID 100) | |
| Any, toggle off | Characters lit like map pieces again | |
| Model Editor | Unchanged (map lighting is Map Editor only) | |
| AC6 m00_10_00_00 (garage) | The AC (c0000, built from parts) sunlit by the model sun (x 2-3 x 1.75), the hangar around it not | Still unlit: check the part FLVERs' container path in the log, `IsCharacterModel` may not match it |
| AC6 m01_00_30_00 | The two c0000 enemy ACs lit like the c3020/c5000 enemies, including their weapons | |
| ER, a player-like NPC (c0000) | Body, armour and weapons all take the model set | |

## Phase 5: choosing the area, ID and time

1. Viewport Rendering > Map Lighting has the file combo and time slider since Phase 1; add the seven key times as stops
   labelled by `CutsceneGparamTimeParam`'s timezone names.
2. A "Use the selected collision's GPARAM" button: reads the selected collision's scene GPARAM base sub ID and
   picks the file, if the mapping holds (NR m10 collisions with `Unk18` 0-3 should name `m10_00_0000-0300`; ER's
   `mAA_BB_CCWW` names suggest CC is the area and WW the weather).
3. Weather (ER): a combo over `WeatherParam` rows, choosing the WW of the file name from `GparamId`, falling back to
   WW 00 when the map has no file for that weather.
4. Time and bakes (ER): the seven TimeOfDay keys and seven bakes suggest bake N = key N. If confirmed (compare bake
   brightness against the sun at each key, e.g. the night bakes darkest), the time slider drives
   `Viewport_GI_Blend_Bake/Ratio` as well: between keys N and N+1, bake N blended to N+1.

### Phase 5 research (2026-11-03, ivtool `erscene`, `erbake`; gpdump `param`)

- **Collision sub ID -> file: confirmed.** ER collisions' `GparamSubID_Base` against each area's files:
  m10 uses 1, 4, 5, 6, 8, 9 (files 01-06, 08, 09); m14 uses 1-5 (files exactly 01-05); m16 uses 1, 2, 4, 5, 6, 8, 9
  (files 01-06, 08, 09); m11 uses 8 (files 01, 02, 08, 09). No map uses 7 and no `07` file exists. Sub ID N picks
  `mAA_BB_0N00`, -1 the plain `0000`. Override1-3 are -1 almost everywhere.
- **Areas.** `GparamRefSettings` (row AABBCCDD -> `RefTargetMapId`) sends 70 maps elsewhere: every catacomb and
  cave (m30, m31, ...) to m30_00, m21_01/02 to m21_00, m40-m43's sub-maps to their _00. The open world has no rows;
  its tiles use the shared m60_00 / m61_00 files (tiles' sub IDs 4 and 9 match `m60_00_0400` and `_0900`).
- **Weather.** The open world's files add the weather in the last two digits (`m60_00_0010` ... `_0042`, `_0910`,
  `_0920`...), matching `WeatherParam.GparamId` (10, 20, 30, 31, 40, ...); m61's run 50-78 for the DLC's weathers.
- **Bake N = timezone N: confirmed.** Mean IVINFO L0 per bake, ER m10_00_00_00: 0.014, 0.024, 0.026, 0.013, 0.0095,
  0.0095, 0.0094. The two brightest are the daytime keys (t4, t8: sun 0.4), the evening (t12) dims, and the three
  night bakes are equal, as the three night keys' suns are (0.2 moonlight). m11, m60_42_36 and m60_43_31 follow the
  same shape; m60's bake 3 stays bright, as its evening sun is 0.6.

### Phase 5 as built (2026-11-03)

- `GparamLighting.TimeKeys` (0, 4, 8, 12, 15, 21, 22.5), `TimezoneNames`, `TimezonesAt(hours)` -> (from, to, ratio),
  wrapping 22.5 -> 0.
- `GparamLightingBank.GetArea` (GparamRefSettings, then m60/m61 -> mAA_00), `GetDefaultFile`, `ChooseFile` (now also
  swaps in the weather's `CCWW` file when the area has one), `GetSubIdFile`, `GetSceneSubId` (reflection:
  `GparamSubID_Base`, or NR's `Unk18`), `ListWeathers`, `GetWeatherName` (WeatherParam row names). The previously
  static helpers are instance methods now, as the area needs the params.
- Bakes following the time (decided with the user: on demand): `IrradianceVolumeBank.BakesFollowTime` (ER, map
  lighting on, `Viewport_Map_Lighting_Follow_Bakes`), `GetBakesToLoad` (the map load queues the two timezones'
  bakes instead of the menu's), `RequestBake` (queues one bake's envmap files for a loaded map, once, as its own
  load job), `IsBakeResident` (cached once true), and `ResolveBakes`, called by `MapEditorScreen.Update` before
  `UpdateGpuTable`: a new pair of bakes is only used once its textures are on the GPU; within a pair the ratio
  follows at once.
- Viewport Rendering > Map Lighting: "Use the Selected Collision's GPARAM", a Weather combo (when the area has weather files),
  seven timezone buttons under the time slider, "Irradiance Volumes Follow the Time of Day" (ER). The GI menu's bake
  settings are disabled while the bakes follow the time, and it shows the pair and ratio.
- CFG `Viewport_Map_Lighting_Weather` (0), `Viewport_Map_Lighting_Follow_Bakes` (on).

Check:

| Map | Do | Expect |
|---|---|---|
| ER m10_00_00_00 | Drag the time 0 -> 24 | The volumes' light brightens towards Noon A/B, dims at Evening and stays dark through the night, smoothly; the GI menu shows e.g. "bake 1 towards 2 by 0.5" at 6 |
| Same | First move into a new timezone | A load job "Irradiance volumes of m10_00_00_00: bake N"; the light changes once it finishes, without the volumes disappearing |
| Same | Select a collision with sub ID 4, press the collision button | GPARAM becomes m10_00_0400 and the lighting changes |
| ER m30_xx (a catacomb) | Open Map Lighting | "from m30_00_0000" instead of the default value |
| ER m60 tile | Weather combo | Entries such as "10: ..." from WeatherParam; picking one switches to m60_00_0010 (sun and exposure change) |
| NR m10_00_00_00 | Select a collision with Unk18 = 2 | The button picks m10_00_0200 |
| AC6 / NR | | No Follow checkbox; the GI bake settings work as before |

### BTL lights (2026-11-03)

- Before: only selected BTL lights shone (`SelectedLightPreview`, now in `.archive`), through the scene params'
  16-light array, at `DiffusePower x Viewport_Light_Intensity_Scale` (5).
- `Renderer/Scene/SceneLightTable.cs`: up to 1024 `SceneLightData` in one allocation of the material buffer, like
  `IrradianceVolumeTable`. `SceneParam.MapLightStart/Count` (+ two pads), `SceneRenderPipeline.UseMapLights`
  (Map Editor viewports). `FlverShader.frag`: `maplightdata[]` at set 4, a second loop after the scene lights that
  skips a point or spot light whose range doesn't reach the pixel before reading the rest of it. The 16-light
  array stays for the camera light.
- `Editors/Map Editor/Tools/Visibility/MapLightCollector.cs`, run by `MapViewportView.Update`: every frame, the
  visible BTL lights of the loaded maps within `Viewport_BTL_Light_Radius` (150 m) of the current viewport's camera,
  and, with `Viewport_BTL_Light_Use_Enable_Distance`, within each light's `EnableDist` (0 = no limit); selected
  lights always. Nearest first, up to `Viewport_BTL_Light_Max_Count` (256). Light entities are cached per map until
  its object count changes.
- Intensity: `DiffusePower x` the GPARAM exposure (x Exposure Scale) while map lighting lights the viewport, as BTL
  power is taken to be in the game's units like the GPARAM's lights; otherwise `Viewport_Light_Intensity_Scale`.
  Specular (added the same day): the table's entries are `MapLightData` (80 bytes: a `SceneLightData` and a
  `Specular` vec4), so the scene params' 16-light array, which every shader's copy lays out, is unchanged.
  `SceneLight.SpecularColor/SpecularIntensity` (null = the diffuse ones), `ToMapGpu`. The collector fills them from
  BTL `SpecularColor` (gamma 2.2, like the diffuse) and `SpecularPower x` the same intensity scale. Shader:
  `LightAttenuation` (direction and falloff, no colour) split out of `LightRadiance`; the map light loop shades
  diffuse with the diffuse colour and highlights with the specular colour, both at that attenuation.
- Viewport Rendering > BTL Lights tab: the toggle, radius, enable distance, most lights, intensity scale (disabled
  with map lighting), and "Lighting with N of the M lights in range".

Check:

| Where | Expect | If wrong |
|---|---|---|
| ER m10_00_00_00, nothing selected | Torches and lamps near the camera light their surroundings; flying away, distant ones turn off | |
| Same, map lighting on | Lights about as strong against the dark interiors as in game | Far too bright or dark: BTL power isn't in GPARAM units; report the Exposure Scale that looks right |
| Same, toggle "Use Each Light's Enable Distance" | Off: more lights shine at a distance | |
| A busy area | "Lighting with N of the M lights in range" with N up to the limit; lower Most Lights if the viewport slows | |
| Select a light far away | It still shines | |
| A light with a different SpecularColor (or SpecularPower 0) | Highlights on shiny surfaces take the specular colour; with power 0, no highlights but the diffuse light is unchanged | |

### All bakes loaded with the map (2026-11-03)

Changed from on demand at the user's request: while the bakes follow the time, `GetBakesToLoad(mapId)` returns every
bake the map has, so the map load queues all seven bakes' envmap files (about 7x the GI texture memory of one bake).
`ResolveBakes` still requests the pair it needs, which only does anything for a map loaded before following was
switched on.

Check: load ER m10_00_00_00 with "Irradiance Volumes Follow the Time of Day" on, then drag the time 0 -> 24: no
"Irradiance volumes of ...: bake N" load jobs appear and the light changes at once.

### Fog volumes follow the map lighting (2026-11-03)

- `FogVolumeBank.GetSettings` looks a FogID's row up in the map lighting's chain: the chosen file (with its
  weather), the area's own `mAA_BB_0000` (after `GparamRefSettings`), `mapbase`, `default`, then the area's other
  files. Per field, the first of those files with the row that has the field wins, else row 0 of the first file.
  Values are interpolated at the menu's time of day (`GparamLightingBank.TryGetValue`). With map lighting off, the
  area's own file is used, still at the menu's time.
- `FogVolumeBank.Update`, called by `MapEditorScreen.Update`, raises `SettingsVersion` when the file, weather, time
  or the map lighting toggle changes (and `ClearSettingsCache` raises it), and refreshes every loaded volume.
- Newly read: `RayMarching SteppingDistance` (taken as a fraction of the ray through the box, decided with the user:
  steps = min(MaxSampleCount, ceil(1 / SteppingDistance)), at least 4), `SuspendedTransmittance` (the march's
  early-out, which was a fixed 0.001), and, read but not drawn, `LightProcessingInterval(Threshold)` and
  `Light ShadowingIntensity` (no shadow map).

Check:

| Map | Do | Expect |
|---|---|---|
| ER m10_00_00_00, a fog volume | Drag the time of day | The fog changes colour/density where the row has several TimeOfDay keys; nothing changes where it has one |
| ER m60 tile with fog | Weather combo | A weather file with its own fog row changes the fog |
| ER catacomb (m30-m31) with fog | | Fog found from `m30_00_*` files, as the lighting is |
| Any fog volume | Compare with before | Similar look; slightly coarser steps where SteppingDistance 0.1 gives 10 steps instead of 16 |

## Phase 6: other games

DS3, SDT, BB: same LightSet fields, no volumes, so the hemisphere is the whole ambient, plus `giOcclusionRate`
(DS3, SDT). Their file names and fallbacks (no `mapbase`?) need checking. DS2's GPARAM has a different layout (and
BGR colours, Graphics Param Editor.txt); DS1 and DES have no GPARAM. Out of scope here.

## Open questions

| Question | Blocks | How to settle |
|---|---|---|
| Which value ID the game uses | Settled | Scene = LightSet ID 0, models = 100 (see "Value IDs"). The selecting code is not traced; what turns on +1000 is unknown |
| Exposure linear or EV | Phase 2 | First check in Phase 2 |
| Auto exposure | After an HDR target | ER and AC6 maps switch it on; Phase 2 uses the fixed value |
| Bake N = TimeOfDay key N (ER) | Settled | Bake brightness follows the sun at each key, see Phase 5 research |
| Angle sign and yaw axis | Settled | `(cos x sin y, sin x, cos x cos y)` is the travel direction, checked in Phase 2. `FogVolume.frag`'s `Light Overwrite` still negates it, unchecked |
| Collision sub ID -> file | Settled (ER) | Sub ID N -> mAA_BB_0N00, see Phase 5 research; NR/AC6 assumed the same |
| Hemisphere inside volumes | Settled | It adds inside them, compared with the game (2026-11-03) |
| BG colour's w and exposure | Phase 3 | Treated as an intensity with the exposure, like the lights; check against the game |
| What `Sharpness` does | Not planned | Unknown; not used |

## Files touched (expected)

New: `Editors/Map Editor/Data/GparamLightingBank.cs`, `Renderer/Scene/GparamLighting.cs`.
Changed: `Renderer/Structs/SceneParam.cs`, `Renderer/Scene/SceneRenderPipeline.cs`, `Editors/Viewport/VulkanViewport.cs`
(set `UseMapLighting`), `Editors/Viewport/GUI/ViewportMenu.cs`, `Editors/Map Editor/MapEditorScreen.cs` (resolve per
frame, as `UpdateGpuTable` is called), `Application/CFG.cs`, `FlverShader.frag` and its SPIR-V variants,
`FogVolume.frag` (struct copy, maybe the angle fix), the Gparam Editor's save path, `Localization/English/Viewport.json`.
Rebuild shaders with the `shader-rebuild` memory's commands; build `Smithbox.Vulkan`, not the solution.
