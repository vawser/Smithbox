# ER GPARAM effects missing from the renderer: findings and phased plan

Written 2026-11-03. Follows on from `Documentation/Full GI/Gparam Lighting.md` (LightSet, Tone Map, Shadows,
EnvironmentConfig, FogVolume and BG colour are already rendered). Each phase ends in something to check by eye;
Smithbox is not launched to verify (CLAUDE.md).

## What ER's files actually hold

Surveyed with ivtool `ergp` through the VFS: `m10_00_0000`, `m60_00_0000` and `mapbase` in full, then every
drawparam file for the fields below.

| Group (annotation name) | In ER's files | Notes |
|---|---|---|
| Fog | **No fields** | Annotation marked obsolete. ER's distance and height fog is VolumetricFog's `Global *` fields |
| Volumetric Fog | Every map file | m10: 96 x ? x 24 froxels, near 0.1, far 300, `Global HeightFogDensity Scale` 0.004, `Distribution` 0.06, `ScatteringG` 0.2; `SkyLight SkyColor`, `DirLight DirectColor`/`OppositeColor` keyed by time; `DirLight LightBankId` 160000000 |
| Cloud Shadow | **No fields** | Nothing to drive it in ER |
| SSAO | mapbase + 43 files | Scale 1.5, Offset -0.25, kernel 2-5 px over base distance 100, fade at 3800 m. LightSet has `Directional Light SsaoWeight0-3` per light (m60 ID 0: 1, 0.5, 0, 1 at t0) |
| Ambient Occlusion, Model AO | **No fields** | SSAO is ER's only AO |
| SSR | mapbase + 48 files (21 disable it) | Shininess reject 0.35, Fresnel reject 0.15, fade-to-viewer 0.1 |
| Light Shaft | Most map files | On in m60 (overworld) all day, off in m10. `Angle` is its own sun angle; `EnableRefLight` + `LightBankId` 160000000 point at a light instead; sky mask by `MaskDepthThreshold` 1000 |
| Bloom (Glare) | mapbase + 110 files | `GlareNumLevels` 5, `Luminance` 1, `Threshold` 1, `Bloom Alpha` 0.25. Mostly on/off per value ID |
| Glow Filter | mapbase + 28 files | 7 levels, `FinalColorScale` 5, `Blur RgbScale` 1.5, `Blur AlphaScale` 1: likely blurs an emissive mask in alpha |
| Anti-Alias | mapbase, default, s-maps | `AntiAliasType` 5 in mapbase (TAA fields set: jitter 8, feedback 0.95, sharpen 0.25), 4 in `default` and s-maps. No enum file yet |
| Color Grading | 162 files | `LutSourceId` changes through the day in m60 (1, 2, 2, 4, 4, 5, 5, 5); off (-1) in mapbase. Where the LUT textures live is unknown |
| Wet | 28 files, **all `m00_00_WWWW_weatherbase/weatheroutdoor`** | Albedo rate 0.25-0.6, shininess rate 0.25-0.5 in rain weathers. These are global weather overlays, which `GparamLightingBank` does not read yet |
| Sea Waves | **Only `mdlvw_*` files, all disabled** | No ER map uses it |

Not in the list but worth having: **auto exposure**. Every ER map file sets `ToneMap-Enable AutoExposure` 1
(Middle Gray -6 to -8, adapt min/max), and the viewport still uses the fixed `Exposure`. The HDR target that
Phase 2 of the lighting plan was waiting for now exists. Lens Simulation (DoF), Motion Blur and Feedback Blur are
left out: they hide detail in an editor.

So the list becomes: **Volumetric/height fog, SSAO, SSR, Light Shafts, Bloom, Glow, AA, Colour Grading, Wet, plus
auto exposure**. Fog, Cloud Shadow, AO, Model AO and Sea Waves are dropped for ER (no data to drive them).

## What the renderer has and lacks

Has: per-pipeline HDR `SceneTarget` (RGBA16F + D32), `ToneMapPass` drawn straight to the swapchain in the overlay
setup action (`VulkanViewport.cs:198`), a scene depth copy after opaques (`SceneDepth`, captured mid batch list in
`SceneRenderer.SubmitBatches`), sun shadow cascades, FogVolume boxes ray-marched against scene depth, BTL light table,
and a generic per-field GPARAM resolver (`GparamLightingBank.TryGetValue`).

Lacks, and what needs it:

| Gap | Needed by |
|---|---|
| A post chain between the scene and tone mapping (ping-pong HDR textures, fullscreen passes) | Everything below |
| Compute passes (Veldrid has `CreateComputePipeline`/`Dispatch`; Smithbox.Program never uses them) | Auto exposure, volumetric fog, Hi-Z |
| Normals and roughness on screen: the renderer is forward and writes colour only | SSAO, SSR |
| Indirect light kept apart from direct, so AO darkens only ambient | SSAO |
| A colour copy after opaques, and a Hi-Z depth chain | SSR |
| Previous frame's view-projection and colour, sub-pixel jitter | TAA (and SSAO/SSR reprojection, optional) |
| Weather overlay files (`m00_00_WWWW_weatheroutdoor` / `_weatherbase`) in the resolve chain | Wet |
| The other groups resolved into `GparamLighting` | Everything below |

## Phases

Ordered so that what needs no new scene outputs comes first, and the most invasive change (extra render targets
in every scene shader) lands once, before the two effects that need it.

### Phase 0: post chain and resolving the groups (no visible change)

1. `Renderer/Scene/PostProcessChain.cs`, owned by `SceneRenderPipeline` beside `ToneMap`: two RGBA16F textures the
   size of `SceneTarget` (recreated on its `Version`), a `Run(cl, ...)` that applies the enabled passes in order and
   hands `ToneMapPass` the last one written. Fullscreen passes follow `ToneMapPass`'s pattern (pipeline per
   `OutputDescription`, one resource set per target version). A small `ComputePass` helper for the compute ones.
2. `GparamPostEffects` (new, in `GparamLighting.cs` or its own file) on `GparamLighting`: one class per group above,
   resolved in `GparamLightingBank.Resolve` with the same file -> `mapbase` -> `default` chain and time
   interpolation. Value ID 0 throughout, except Bloom, which has per-weather IDs (to confirm in Phase 1).
3. Viewport Rendering > Post Effects tab: a toggle per effect (CFG `Viewport_Post_*`, each on by default only once
   checked), and a read-only table of the resolved values with their source, like Map Lighting's.
4. Survey with ivtool: every group in the `weatheroutdoor`/`weatherbase` files, and the `AntiAliasType` values the
   exe accepts (strings near `AntiAliasType`).

Check: Post Effects lists, for ER m60_42_36_00 at t4: Light Shaft on, LUT 2, Glare threshold 1; for m10_00_00_00:
Light Shaft off, height fog density 0.004. The image is unchanged.

### Phase 0 as built (2026-11-03; checked in the app 2026-11-03)

- `Renderer/Scene/PostProcessChain.cs`: `PostProcessPass` (`Name`, `IsActive(lighting)`, `Run(gd, cl, frame)`),
  `PostProcessFrame` (target, source, destination texture and its colour-only framebuffer, lighting),
  `PostProcessChain` (`Add`, `Run` returning the texture to tone map, `Version`, `LastActive`, `Outputs` for
  fullscreen pipelines). Two RGBA16F textures (colour attachment, sampled, storage) made the first time a pass is
  active. No passes are added yet. Owned by `SceneRenderPipeline.PostProcess`.
- `VulkanViewport`'s overlay setup action runs the chain before binding the swapchain, and `ToneMapPass.Draw` takes a
  texture instead of the `SceneTarget` (its resource set is rebuilt when the texture object changes). The model
  previews tone map their target directly, without the chain.
- `Renderer/Scene/ComputePass.cs` (shader `name.comp.spv`, group size, `Dispatch` rounding up) and
  `ShaderHelper.LoadComputeSPIRV` / `LoadBytecode` knowing the `comp` stage. Unused until Phase 1.
- `Renderer/Scene/GparamPostEffects.cs` on `GparamLighting.PostEffects`, filled by `GparamLightingBank.ReadPostEffects`
  at ID 0 (SSAO light weights at the LightSet ID). Group keys are the annotations' IDs (`Bloom[Yebis]`,
  `LightShaft[Yebis]`, `VolumetricFog ParamEditor`, `SSAO Editor`, `SSR ParamEditor`, `Anti-Alias-Editor`,
  `ColorGrading[Yebis]`, `Wet ParamEditor`, `Glow Filter-Editor`); the bloom switch's key has full-width brackets
  (`Gs Presents【Bloom】 Enable`). Auto exposure's speeds, centre weight and adaption range are read from Tone Map.
- Viewport Rendering > Post Effects tab (Map Editor, and the Model/Time Act Editors' borrowed map): "Draw Screen
  Effects" (CFG `Viewport_Post_Effects`, on), the passes that ran, and per map a table of each effect's values and the
  file they came from. Changed from the plan: the per-effect switches are added with their effects, so none does
  nothing in the meantime.

Survey results (ivtool `ergp`, `strings`, 2026-11-03):

- **Weather overlays are big.** 85 files: `m00_00_0000_weatherbase` and 84 `m00_00_WWWW_weatheroutdoor`
  (WW = `WeatherParam.GparamId`, 0001-0881). Nearly all of them set Tone Map (77, including exposure and auto
  exposure), Volumetric Fog (77), LightSet (71: sun, hemisphere, SSAO weights), Light Shaft (57), FogVolume (42),
  `LutSourceId` (28) and Wet (28). So they change the existing map lighting too, not only Wet: wiring them into the
  chain (Phase 8 step 1) should move up as soon as the weather combo is used for outdoor maps.
- **AntiAliasType.** The exe lists `FXAA2_GS`, `FXAA3_GS`, `FXAA3_HQ_GS`, `DLAA_GS` together, beside `FXAA.ppo`,
  `FXAA3.ppo`, `FXAA3HQ.ppo`, `DLAA.ppo`, and the TAA fields follow the DLAA ones. Hypothesis: 0 none, 1-3 the FXAA
  kinds, 4 DLAA, 5 TAA. So mapbase uses TAA, and `default` and the s-maps DLAA.
- **Tone Map has its own colour controls**: `GS Presents - ColorH/S/V`, `Contrast`, `Tone Map Color Grading
  ColorShift/ColorScale` (Vector4, rgb with an unused w), vignetting and chromatic aberration. All neutral in
  mapbase and m60_00_0000 (H 0, S 1, V 1, Contrast 1, Shift/Scale (1, 1, 1)); vignetting power 0.4-0.5. They belong
  with Phase 7.

Check (no image change expected):

| Where | Expect |
|---|---|
| ER m10_00_00_00, Post Effects tab | Light Shafts off; Height Fog "density 1E-05 + 0.004 x exp(-0.06 x (y - 0)), scattering g 0.2", from m10_00_0000 #0; Bloom from mapbase #0; Auto Exposure "middle gray -6.4" |
| ER m60_42_36_00 at time 4 | Light Shafts on; Color Grading "LUT 2"; Auto Exposure middle gray -6.2 |
| Any viewport, toggle Draw Screen Effects | No change at all; "No screen effects are drawn yet" |
| Model Editor with a borrowed map | The same tab, for that map |
| Resize a viewport, open the model previews | Still drawn as before (the tone map now reads the texture it is handed) |

### Phase 1: auto exposure, bloom, FXAA

The cheap, screen-only effects; they prove the chain and set the brightness the later effects are judged against.

1. Auto exposure (compute): log-luminance histogram of the exposed scene, centre-weighted by `Center Weight`, mean
   compared to `Middle Gray` (log2), clamped by `Adaption Min/Max` (EV around `Exposure`), smoothed with
   `LightAdapt`/`DarkAdapt Speed` per second. Result fed to `ToneMapPass` as a multiplier. Off when the file's
   `Enable AutoExposure` is 0 (mapbase).
2. Bloom: bright pass `max(L - Threshold, 0) x Luminance` on the exposed colour, `GlareNumLevels` half-res mips,
   Gaussian blur scaled by `GaussianRadiusScale`, summed and added at `Bloom Alpha` before tone mapping;
   `Sturate Ctrl` lerps towards luminance.
3. FXAA after tone mapping, on the swapchain draw, with `fFxaaAlpha`, `ReduceMin/Mul`, `SpanMax`. Used whatever
   `AntiAliasType` says until TAA exists.

Check: ER m60_42_36_00 at t4 looking at the sun-lit sky then into a cave: exposure brightens over a second or two.
Bright sky and lamps get a soft halo, nothing else does. Edges of a fence or banner stop crawling.

### Phase 1 as built (2026-11-03; checked in the app 2026-11-03)

- **Auto exposure** (`AutoExposurePass.cs`, `AutoExposure.comp`): the chain's first pass, measuring only
  (`PostProcessPass.WritesImage` false; the chain now also calls `BeginFrame` on every pass). One 16 x 16 group takes
  a 64 x 64 grid of the scene, divides the GPARAM exposure back out (so it measures game units), and averages log2
  luminance weighted by `exp(-CenterWeight x r²)` from the centre. Target = `clamp(MiddleGray - mean, AdaptionMin,
  AdaptionMax)` EV; the GPARAM's Exposure is taken to be right for a scene whose mean is Middle Gray. The adapt
  speeds are read as the share of the gap closed per 1/30 s (LightAdapt when going darker). The state stays in a
  storage buffer on the GPU from frame to frame; the tone map multiplies by `exp2(ev)` (`ToneMapParam.Settings.w`),
  and a neutral buffer is bound when the pass didn't run (model previews, auto exposure off). A readback each few
  frames feeds the menu. Changed from the plan: a weighted mean, not a histogram.
  Runs only with map lighting and the file's `Enable AutoExposure` on.
- **Bloom** (`BloomPass.cs`, `Bloom.vert/frag`): prefilter at half size (13-tap with Karis weights, then the
  threshold on the auto-exposed colour with a soft knee of `Threshold x 0.5 x Smooth`, x Luminance, desaturated by
  `Sturate Ctrl`), `NumLevels` (1-8) half-size downsamples, then upsamples where each level is its own texture
  (down level + 3x3 tent of the level below, spread by `GaussianRadiusScale`), and a composite of
  `scene + sum x Alpha / NumLevels`. The bloom is divided back by the auto exposure so the tone map exposes it with
  the scene. Not used: `FeedbackWeight` (temporal). Only value ID 0 is read, so files that switch bloom on by weather
  ID and off at 0 show none.
- **FXAA** (`FxaaPass.cs`, `Fxaa.vert/frag`): Lottes' original FXAA, whose Reduce Min/Mul and Span Max are exactly
  the GPARAM's fields; Alpha blends it in. When active, the tone map draws into an RGBA8 image the size of the scene
  (`FxaaPass.ImageOutputs`) and FXAA draws that to the swapchain at the viewport's offset. Runs with or without map
  lighting (mapbase's values then), whatever `AntiAliasType` says.
- Veldrid quirks met: binding a framebuffer begins its render pass with the contents discarded, so bloom can't blend
  into a level; `UpdateBuffer` is a staging copy with no barrier, and ends any open render pass, so passes update
  their parameters first and then put one barrier (transfer -> uniform read) before drawing or dispatching.
- CFG `Viewport_Post_Auto_Exposure`, `Viewport_Post_Bloom`, `Viewport_Post_Fxaa` (all on), switches in Post Effects,
  plus "Auto exposure: scene mean X (log2), adjusted Y EV towards Z".
- Shaders compiled with `glslangValidator -V`; the app is built from Smithbox.Vulkan.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m10_00_00_00, time 4 | Look around the courtyard | Post Effects says "Drawing: Auto Exposure, Bloom, FXAA"; the status line shows a mean around -3 to -6 and an adjustment within -0.5..+0.4 EV | Mean far outside that: report it; the scene isn't in the units assumed |
| Same | Look at the bright sky, then into a dark doorway | The image dims a little against the sky and brightens a little in the dark, easing over about a second, never more than half a stop | Jumps: speeds are not per 1/30 s. Too strong or the wrong way: the formula's sign |
| Same | Toggle Auto Exposure | At most half a stop of difference | |
| A lamp or the sun-lit sky | | A soft glow round the brightest parts only; dull surfaces unchanged | Everything glows: the threshold is in the wrong units |
| Same | Toggle Bloom | Only the glow goes | Image shifts or flips: a texture is read mirrored |
| Fences, banners, railings | Toggle FXAA | Stair-stepped edges smoothed, text and gizmo lines (drawn after) unchanged | Image offset or doubled in a split viewport: the FXAA offset |
| Map lighting off, or the Model Editor without a borrowed map | | Only FXAA in "Drawing:"; the image exactly as before apart from edges | |
| Resize the viewport, use two viewports | | No black frames, no stretched image | |

### Phase 2: height fog, then volumetric fog

1. Analytic height fog (fullscreen, at the depth capture point so it lands before transparents): density
   `FixedDensity + DensityScale x exp(-Distribution x (y - HeightFogOffset))` integrated along the view ray,
   in-scatter from `SkyLight SkyColor` plus `DirLight DirectColor`/`OppositeColor` by a Henyey-Greenstein phase with
   `ScatteringG`, both x exposure, starting at `StartDist`. Transparent shaders apply the same function per pixel
   (a shared GLSL include), so water and glass are fogged too.
2. Froxel volume (compute): `VolumeWidth` x (width x aspect) x `VolumeDepth` slices between `NearDepth` and
   `FarDepth`, inject density and light, sample the sun cascades (`ShadowSampleCount`) and the BTL table scaled by
   `LocalLightScale`; integrate front to back; the fullscreen pass and transparents sample the 3D texture instead of
   the analytic integral within `FarDepth`. Temporal reprojection is left out (editor frames are not noisy enough to
   need it if the sample counts are kept).
3. Later, optional: inject the FogVolume boxes into the froxels instead of ray-marching each box.

Check: ER m10_00_00_00 at t4: distant towers hazed with a faint green-grey (SkyColor 0.49, 0.55, 0.51), the haze
thinning with height. Looking towards the sun the haze is brighter than away from it. With volumetric fog, shafts of
shadow through the courtyard archways.

### Phase 2 as built (2026-11-03; checked in the app 2026-11-03)

Steps 1 and 2 were built as one: the froxel volume is the fog, and the analytic part only covers what lies past it.

- **Volumetric fog** (`VolumetricFogPass.cs`, `VolumetricFog.comp`, `VolumetricFog.vert/frag`), one per
  `SceneRenderPipeline` (`Fog`). Not a `PostProcessChain` pass: it runs inside the scene, in
  `SceneRenderPipeline.CaptureSceneDepth`, right after the depth copy that the transparent draws read.
  - `Prepare` (in `UpdateSceneParameters`) works out the parameters from the camera and the map lighting: colours x
    intensity (w) x GPARAM exposure, as the map lights are.
  - `Apply` updates them (which ends the scene's render pass), dispatches the compute, then draws a fullscreen
    triangle into the HDR target with blend `scene x a + rgb`. The draw carries on with the target's contents (the
    depth copy leaves the framebuffer bound), and Veldrid moves the volume to a sampled layout before its render pass
    resumes.
  - `SubmitBatches` now copies depth after the last draw when no transparent draw caused it, so scenes without
    transparents get fog too.
- **The volume:** `VolumeWidth` cells across, as many down as keep them square (54 at 16:9), `VolumeDepth` slices
  spaced evenly in log depth from `NearDepth` to `FarDepth`. RGBA16F: light scattered so far, and transmittance.
  - One thread per column marches out from the camera, `SampleCount` steps per slice (jittered per column), and
    stores the totals at each slice's far side.
  - The fragment reads it at the pixel's depth, fading in from the camera before the first slice.
- **How the values are read** (a reading, not decoded from the game):
  - density = `FixedDensity + HeightFogDensity Scale x exp(-HeightFogDistribution x (y - HeightFogOffset))` per
    metre, capped at 4; `AbsorptionScale` (its average) adds to extinction only.
  - `SkyColor` is the same from every direction.
  - The sun's light is `mix(OppositeColor, DirectColor, HG(view.sun, ScatteringG) / HG(1))`, shadowed by one texel
    of the cascade the point falls in.
  - `HeightAtten` fades a light above `HeightFogOffset`; `StartDist` keeps it off the first metres.
  - Map lights: the BTL table near the camera (`SceneRenderer.MapLights`), x `LocalLightScale`, range x
    `LocalLightDistScale`, evaluated once per slice.
  - Past `FarDepth`, with `EnableAnalyticalFog`, 8 steps out to `FogEndDepth`, sky and unshadowed sun only. Pixels
    with no depth (sky) take the whole range.
- The compute binds its own set (the scene params buffer, the shadow map itself via the new
  `ShadowCascades.MapTexture`, the material buffer for the map lights, and the volume as a storage image), because
  the texture pools and the material buffer layout are only visible to the vertex and fragment stages.
- CFG `Viewport_Post_Volumetric_Fog`, `Viewport_Post_Fog_Shadows`, `Viewport_Post_Fog_Local_Lights` (all on),
  switches in Post Effects; "Drawing:" lists Volumetric Fog first.
- Not done, from the plan:
  - Transparents (water, glass) are drawn over the fog unfogged. The 3D texture could go in
    `globalTexturesVolume` (set 3, binding 1) for FlverShader to read, but that pool is fragment-only, which fits.
  - `DirLight LightBankId/LightId` are ignored: the scene sun is used.
  - `ShadowSampleCount` is unused: `SampleCount` steps are shadowed.
  - Step 3 (FogVolume boxes into the froxels) remains.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m10_00_00_00, time 4, map lighting on | Look across the castle | "Drawing:" starts with Volumetric Fog; distant towers hazed green-grey, near walls clear | Whole screen grey or black: report HeightFogOffset and the camera's height (density formula or units). Nothing at all: the fog switch, or ER's `VolumetricFog Enable` |
| Same | Fly up 50-100 m, then down into a low courtyard | Haze thins higher up, thickens lower | The other way round: the sign of Distribution |
| Same | Look towards the sun, then away | Brighter and warmer towards it (DirectColor), dimmer away (OppositeColor) | Same both ways: the phase isn't applied |
| Same, Shadows on | Look through archways towards the sun | Darker streaks of fog in the shadows of walls and arches | Shadows that slide with the camera, or upside down: the Y flip in the compute's clip coordinates |
| Same | Toggle Sun Shadows in Fog | Only the streaks go | |
| An interior with torches (m10 or a catacomb) | Toggle Map Lights in Fog | A glow in the air round lights | Blown out: the BTL units differ from the fog's, report Local Light Scale |
| A pool or window | | Water and glass not hazed (known gap); no seam or halo round them | |
| Toggle Volumetric Fog | | Only the haze changes; frame time drops a little | |
| Resize, split viewports, Model Editor with a borrowed map | | Fog matches the view in each; no stretching | |

### Phase 3: light shafts

Screen-space radial blur from the light's screen position: sky mask where depth > `MaskDepthThreshold`, shaped by
`SkyMaskSizeAngle`/`CenterPower`, tinted by `Sky Multiply/Add Color`, blurred `ShaftBlurCount` times over
`ShaftBlurRange`, faded by `Fade Out Start/End Angle`. The direction is `LightShaft Angle` unless `EnableRefLight`,
then the LightSet sun (bank 160000000 is taken to mean the scene LightSet; to confirm). `Parallel Mode` angles
switch to a directional blur when the light is far off screen.

Check: ER m60_42_36_00 at t8 facing the sun through trees: rays fanning from the sun disc; none in m10_00_00_00
(disabled there).

### Phase 3 as built (2026-11-03; checked in the app 2026-11-03)

**Survey.** Every m60 tile resolves to `m60_00_0000`. There:
- `EnableRefLight` is on with bank 160000000 at every time, so the scene sun is used.
- `Sky Multiply Color` and `Sky Add Color` have intensity (w) **0 at t4, t8, t12 and from t15 on**. Their w is 1 only
  at t0 (Add) and t11.5 (both).
- So m60 shows shafts only towards t11.5 (rising from t8, gone again at t12) and around t0. The plan's "t8" check
  would show nothing.
- `capturemap` and the 34 files with `<1, 0.85, 0.85, 3>` have stronger multipliers.
- m10 has the group off.

**The pass** (`LightShaftPass.cs`, `LightShaft.vert/frag`) runs in the chain after auto exposure and before bloom,
so the brightest shafts also glow. Its steps:
1. **Mask** at half size, written out in Mask(). A pixel counts as sky where the scene depth is 0 or past
   `MaskDepthThreshold`. It is weighted by `((cos(ray, sun) - cos(SizeAngle)) / (1 - cos(SizeAngle)))^CenterPower`.
   Its colour is the sky over `SkyColorThreshold` (judged after the auto exposure), greyed by `SkyColorSaturate`,
   times Multiply.rgb × w, plus Add.rgb × w.
2. **Blur**, `ShaftBlurCount` (1-4) passes of 16 taps each:
   - Each pass covers 1/16 of the one before.
   - The taps run towards the sun's point on screen, over `min(ShaftBlurRange / 4, 1)` of the way.
   - The weights are `exp(-ShaftBlurFallOff × t)` along the whole length.
   - Between the Parallel mode angles (the sun's angle from the camera's forward), the blur turns to a fixed direction
     across the screen, `ShaftBlurRange for Parallel` long. It is always parallel with the sun behind the camera.
3. **Composite**: scene + shafts × `1 - smoothstep(Fade out begin, end, angle)`.

**How the values are read** (a reading, not decoded from the game):
- `Sky Add Color` takes `1 + (exposure - 1) × ExposureInfluenceRate AddColor` of the GPARAM exposure. The Multiply
  colour scales the sky, which already has the exposure in it.
- Not used: `HighQualitySkyMask`, `SkyMaskMode` (0 everywhere), and `Angle` while `EnableRefLight` is set.
- Weather overlays, which override Light Shaft in 57 files, are still not in the resolve chain.

CFG `Viewport_Post_Light_Shafts` (on), with a switch in Post Effects.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m60_42_36_00 at t11 (between t8 and t11.5), map lighting on | Face the sun with trees or a ruin in front of it | "Drawing:" lists Light Shafts; pale rays fan out from the sun round the silhouettes | Nothing: Multiply/Add w read as intensity is wrong, report the Post Effects table's Light Shaft row. Rays pointing away from the sun: the screen Y flip in `ToScreen` |
| Same, t8 or t15 | | No visible rays (w is 0) | Visible: the colours aren't scaled by w |
| Same, t11 | Turn until the sun is 40-80° off to the side | The rays turn into parallel streaks across the screen | Streaks pointing the wrong way: the parallel direction's sign |
| Same | Turn the sun behind you (past 90-120°) | The shafts fade out completely | |
| Same | Toggle Light Shafts | Only the rays change | Whole image shifts or gets brighter: the composite reads the wrong texture |
| m10_00_00_00 | | Light Shafts not listed (group off) | |
| Resize, split viewports | | Rays stay on the sun in each viewport | |

### Phase 4: screen outputs for normals and indirect light

The one invasive step. `SceneTarget` gains two colour attachments: normal (octahedral) + roughness/metalness
(RGBA16F or RGB10A2), and indirect diffuse (R11G11B10F). `FlverShader.frag` writes them; every other shader that
draws into `SceneTarget.Outputs` (collision, nav, debug wire, grid, FogVolume, SimpleFlver) gets matching outputs
with write masks off, so their pipelines stay valid. Blend states need one entry per attachment. Transparents write
none of them.

Check: a debug view (Post Effects > Show: Normals / Roughness / Indirect) that draws each attachment. Nothing else
changes; nav meshes and collisions still draw.

### Phase 4 as built (2026-11-03; checked in the app 2026-11-03)

- **The attachments** (`SceneTarget.cs`):
  - `Surface`, RGBA16F: xy the world normal (octahedral, -1..1), z roughness, w specular (the largest of F0's
    channels; the flver shader is specular/F0, not metalness).
  - `Indirect`, B10G11R11 float: the indirect diffuse light in the colour's units, x `indirectLightMult` and scene
    brightness.
  - `BeginScene` clears Surface to `(0, 0, -1, 0)`, so roughness < 0 marks pixels no flver drew, and Indirect to black.
- **Veldrid fork:** `BlendAttachmentDescription.ColorWriteDisabled` (new, default false, part of equality and the
  hash) sets a colour write mask of 0, with a `NoWrite` preset. Veldrid always wrote RGBA before, and a shader with no
  output for an attachment would leave undefined values in it.
- **Every scene pipeline** pads its blend to three entries with `SceneTarget.BlendFor(set, blend, writesSurface)`, or
  `WithSurface` for the fog pass. Overlay pipelines, which draw to the swapchain, are left as they were.
  - Only `MeshRenderableProxy`'s main and picking pipelines for opaque FlverShader meshes write the two attachments
    (`UsesFlverShader && !IsTransparent`).
  - Every other pipeline drawing into the target has `NoWrite` for both: collision, nav, SimpleFlver, the selection
    outline, debug primitives, FogVolume boxes, volumetric fog, and transparent flvers.
  - The shadow pipeline keeps no colour attachments.
- **FlverShader.frag** has two new outputs, `fsout_surface` (location 1) and `fsout_indirect` (location 2):
  - `IndirectLight` keeps its diffuse term in `g_indirectDiffuse`, for both the Phong and PBR paths.
  - Unlit surfaces write no indirect light.
  - All 16 variants were rebuilt with the variant compiler.
- **Debug view** (`SurfaceViewPass.cs`, `SurfaceView.vert/frag`): the chain's last pass, CFG `Viewport_Post_Surface_View`
  (Post Effects > Show: Image / Normals / Roughness / Specular / Indirect Light). It needs Post Effects on, but not map
  lighting.
  - Normals, roughness and specular are shown as display values: `ToneMapPass.Draw(passThrough)` drops the exposure,
    curve and auto exposure.
  - Indirect light is tone mapped as the scene is.
  - Pixels with no surface are dark grey.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| Any ER map, Show: Image | Look around | Exactly as before, including collisions, nav meshes, the grid, gizmos, FogVolume boxes, fog, shafts, selection outlines | Black or missing meshes, or a validation error: a pipeline left with one blend entry |
| Show: Normals | | Smooth pastel colours: floors facing up the same colour everywhere, walls by facing, normal-map detail visible; sky and collision dark grey | Flat single colour: the output isn't written. Speckled noise: a pipeline writing without the outputs |
| Show: Roughness | | Polished stone and metal darker, cloth and dirt lighter | |
| Show: Specular | | Mostly dark (F0 near 0.04), metals bright | |
| Show: Indirect Light | m10 interior and outdoors, map lighting on | The ambient on surfaces without the sun's direct light: shadowed sides keep their light, sunlit faces show only the fill | Black everywhere: the attachment isn't reached |
| Water, glass, foliage cards with transparency | Show: Normals | The scene behind shows through (transparents don't write) | |
| Select an object | Show: Image | Outline and tint as before | |
| Model Editor, Time Act Editor viewports | | Draw as before | |

### Phase 5: SSAO

Half-res, from depth and the normal attachment: hemisphere samples with kernel radius `Kernel Scale Min/Max` pixels
over `Base Dist`, `AO = saturate(raw x Scale + Offset)`, faded out over `DepthFadeStart/Range`, bilateral blur, then
the composite subtracts `indirect x (1 - AO)` from the scene colour. The LightSet's `SsaoWeight0-3` scale how much
AO also darkens each directional light's contribution; first pass ignores them (0 for the sun in m10 and m60 at t0 is
the common case), then add a second attachment term if the game clearly darkens sunlit creases.

Check: ER m10_00_00_00: dark contact shadows where walls meet floors and under ledges, in volume-lit interiors as
well as outside; sunlit faces not darkened. Toggle off: exactly Phase 4's image.

### Phase 5 as built (2026-11-03; checked in the app 2026-11-03)

**Survey.** Only `mapbase`, `default` and 43 others have SSAO; m10, m30 and m60 don't, so ER maps take mapbase:
Sample Offset 5, AO Scale 1.5, AO Offset -0.25, Kernel Scale Base Dist 100, Min 2, Max 5, DepthFadeStart 3800 over
100, PrecisionDepth 4000, Depth Reject Rate 0.02, UseNormal on. The files that do have it mostly fill IDs 10-130, not
0 (LightSet-like); only ID 0 is read, as before.

**The pass** (`SsaoPass.cs`, `Ssao.vert/frag`) is the chain's first pass, before auto exposure, so the exposure is
measured on the darkened scene. It also runs without map lighting, on mapbase's values. Its steps:
1. **Occlusion**, at half size: 16 taps on a golden-angle spiral, turned per pixel by interleaved gradient noise.
   - Positions come from the scene depth.
   - The normal comes from `Surface`, or from the depth's slope where no flver drew or UseNormal is off.
   - A tap occludes by `max(dot(v̂, N) - 0.05, 0)`, falling off with distance out to the kernel's world radius.
   - raw = `2 × sum / 16`; the occlusion is `saturate(raw × AO Scale + AO Offset) × the viewport's Strength`.
   - It fades out from DepthFadeStart over DepthFadeRange and is gone past PrecisionDepth.
2. **Blur**, across then down: 9 taps, weighted down where the depth differs by more than Depth Reject Rate × depth.
3. **Composite**: `scene - Indirect × occlusion`, upsampled with depth-aware weights. Only the ambient is taken
   away.

**How the values are read** (a reading, not decoded from the game):
- The kernel radius is `Sample Offset × clamp(Base Dist / depth, Min, Max)` pixels at 1080p, scaled to the viewport's
  height. With mapbase that is 25 px up to 20 m and 10 px from 50 m on.
- Depth Reject Rate is taken as the blur's depth tolerance; in the game it is probably the temporal history's.
- Not used: UseReProj, PrevFrame Strength/Depth Aware (temporal), Geom Bent Offset, EnableUpScale, PreventGhost, and
  the LightSet's `SsaoWeight0-3`. The directional lights are not darkened.

**Known interplay:** the volumetric fog is already in the image, so a fogged pixel loses its ambient as if it weren't
fogged. This is small near the camera, where the fog is thin.

CFG `Viewport_Post_Ssao` (on) and `Viewport_Post_Ssao_Strength` (1), in Post Effects. Show: Ambient Occlusion displays
the blurred visibility.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m10_00_00_00, map lighting on, Show: Ambient Occlusion | Look at a courtyard corner and under a ledge | White open surfaces, soft grey where walls meet floors, in corners and under overhangs; no banding or dotted pattern | Everything grey: the occlusion is too strong, report it. Speckles: the blur's depth tolerance. Shadows offset from the geometry: the Y flip in WorldPosition |
| Same, Show: Image | Toggle SSAO | Corners and creases darken a little; sunlit faces don't change | Sunlit faces darken: the Indirect attachment holds direct light |
| Same | Walk close to a wall, then back off | The darkening keeps its size on the wall, not getting huge up close | Halos round near objects: the radius at short range |
| A volume-lit interior (m10 or a catacomb) | Toggle SSAO | Clear darkening in corners, since ambient is most of the light there | |
| Edges of a building against the sky | Show: Ambient Occlusion | No dark halo on the sky side | |
| Strength 0 | | Exactly as with SSAO off | |
| Map lighting off, Model Editor | | Contact shading on the model where parts meet | |

### Phase 6: SSR

Copy the HDR colour at the depth capture point; build a Hi-Z mip chain of depth (compute). For pixels whose
roughness passes `Shininess Reject Threshold` and Fresnel passes `Fresnel Reject Threshold`, march the reflected
ray (`Ray Trace Step Scale Min/Max`), sample the colour copy at a mip from `Display Mip Scale/Offset` x roughness,
fade at screen edges, near the viewer (`Fade To Viewer Start`) and over `Depth Fade Range`, and blend over the
hemisphere/envmap reflection already in the image (needs the specular term in an attachment, or redo it here from
the normal attachment).

Check: ER m10_00_00_00 courtyard puddles and polished floors reflect nearby walls, falling back to the sky colour at
screen edges. In a file with `SSR Enable` false, none.

### Phase 6 as built (2026-11-03; checked in the app 2026-11-03, in m14)

**Survey.** As with SSAO, m10 and m60 have no SSR, so ER maps take mapbase:
- Shininess Reject 0.35, Fresnel Reject 0.15, Fade To Viewer Start 0.1.
- Step Scale 6-10, Depth Fade Range 10, Crack Detect 1.02.
- Display Mip Scale 1 / Offset 0, Blur Sigma 0, Blur Sigma for Mip 4.
- `GparamSsr`'s defaults were not mapbase's (steps 1/1, fade 1, crack 0) and are now.

**A fifth attachment:** `SceneTarget.Reflection` (B10G11R11 float), the indirect specular that FlverShader adds (the
hemisphere, or the env map without map lighting, times its Fresnel term). Written as `fsout_reflection`, location 3,
by opaque flvers only; `WithSurface` gives every pipeline a fourth entry. All 16 variants were rebuilt.
Five attachments overran `VkFramebuffer`'s 256 byte `StackList<VkAttachmentDescription2>` (56 bytes each, so four
fit); it is now `StackList<..., Size512Bytes>`, as Pipeline.cs already had for its own list.

**The pass** (`SsrPass.cs`, `Ssr.vert/frag`) runs after SSAO and before auto exposure; without map lighting it uses
mapbase's values. Its steps:
1. **Copy** the image into `SSR Color Mips` (RGBA16F, up to 8 mips) and `GenerateMipmaps`. The chain's textures gained
   `TransferSrc` for this.
2. **Trace**, at half size:
   - A pixel is left out if shininess < 0.35, Schlick's Fresnel `F0 + (1 - F0)(1 - N·V)⁵` (F0 = the largest
     channel) < 0.15, or the depth > Depth Reject Threshold. The flver shader's own term, which has a gloss² on it,
     was tried first: that counts roughness twice and rejected nearly everything.
   - The camera's forward, which the ray's depths are measured along, is found in the shader from `clipToWorld`, so
     it agrees with the depth buffer whatever the view matrix's handedness.
   - The reflected ray is Depth Fade Range (10 m) long, cut at 0.2 m in front of the camera.
   - It is marched in screen pixels, with steps growing from Step Scale Min to Max (at 1080p, scaled to the viewport),
     at most 64 of them.
   - A hit is where the ray goes behind the scene by less than `depth × Crack Detect Factor + 5 cm`, refined 5 times by
     halving.
   - Fades: `1 - smoothstep(Fade To Viewer Start, +0.25, R·V)` for rays coming back at the camera, the last 8% of the
     screen at its edges, and the second half of the ray's length.
   - The colour is read at mip `Offset + Scale × 6.5 × roughness`, the flver shader's own env map mip curve. It is
     stored premultiplied by its weight, so the bilinear upsample doesn't darken.
3. **Composite**: `scene + (traced × Fresnel - Reflection) × weight × Strength`, with the flver shader's Fresnel term
   (gloss² and all), as the Reflection it takes out was weighted with it. A hit swaps the environment's reflection for
   the screen's; a miss leaves it as it was.

**How the values are read** (a reading, not decoded from the game):
- Unused: TraverseMode (Hi-Z, 0 in mapbase; the trace is linear), Blur Sigma / Blur Sigma for Mip (the mips are the
  hardware's box chain).
- Depth Fade Range is read as the ray's length. Crack Detect is read as the hit's depth tolerance.

**Known gaps:**
- Water and glass are transparent, so they write no surface and get no SSR.
- With F0 near 0.04, Fresnel Reject 0.15 leaves most dielectric surfaces out except at grazing angles (about 70° or
  more from the normal). This is probably what the game does too.
- m10's floors are dirt and rough flagstone, past Shininess Reject, so Stormveil shows almost no SSR until Wet
  (Phase 8) raises the shininess in rain. A temporary per-pixel diagnostic confirmed it (floors rejected as too rough,
  walls seen head on as Fresnel too low), and was removed once m14's polished floors reflected correctly.

CFG `Viewport_Post_Ssr` (on) and `Viewport_Post_Ssr_Strength` (1), in Post Effects. Show gains Environment Reflection
(the attachment) and Screen Reflections (the trace), both tone mapped.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m14_00_00_00 (Raya Lucaria; m10's floors are too rough), Show: Screen Reflections | Look along a polished floor towards walls, low and grazing | The walls appear mirrored in the floor, softer on rougher stone; black where nothing is hit | Reflections offset, sliding or upside down: the Y flip in `ToPixels`. Speckled streaks: the hit tolerance |
| Same, Show: Image | Toggle SSR | Grazing floors show the walls in place of the sky/hemisphere tint; surfaces seen head on barely change | Whole image brighter: the environment's reflection isn't taken off |
| Same | Pan so the reflected wall leaves the screen | The reflection fades back to the environment's at the edge, without a hard line | |
| Show: Environment Reflection | | Brighter on grazing, glossy surfaces; dark on rough ones | Black: the attachment isn't written |
| Strength 0 | | Exactly as with SSR off | |
| Model Editor without map lighting | | Glossy armour reflects nearby parts at grazing angles, the env map elsewhere | |
| Resize, split viewports | | Reflections stay on their surfaces | |

### Phase 7: colour grading

Research first: where the Yebis LUTs for `LutSourceId` 0-5 are (search the VFS for tpfs or raw volumes in
`/other`, `/menu`, `/param/drawparam`; strings near `LutSourceId` in the exe). Then a 3D LUT applied after tone
mapping in `ToneMap.frag`, the two LUTs either side of the time blended like the GI bakes. `FadeTime` is a gameplay
transition and is ignored.

Check: ER m60_42_36_00 dragging the time 4 -> 12 -> 15: the grade shifts warmer towards evening and cooler at night,
on top of the sun's colour change.

### Phase 7 as built (2026-11-03; checked in the app 2026-11-03)

**Where the LUTs are.** `/map/mAA/common/mAA_cgrading.tpf.dcx`, one TPF per area (20 areas, 101 LUTs):
- Each texture is named `mAA_BB_CGradingNNNN`, where NNNN is the LutSourceId.
- Each is an uncompressed 16 x 256 RGBA8 DDS (R mask 0xFF): a 16³ cube with red across, green down each 16 x 16
  slice, and blue from slice to slice.
- `m60_00_CGrading0000` isn't an identity. Red runs 0, 15, 30, 46 … 240, 255, a slight curve.

**The IDs are shared between areas**, so every area's TPF is loaded. Testing "own area only" left 121 of 199 file/ID
pairs unresolved:
- `m10_00_0900` uses 1, 2, 4 and 5, which exist only as `m60_00_*`.
- `m20_00` uses 5000-5003, from `m61_00`.
- `m11_00_0900` and `m16_00` use 2002, from `m60_00`.
- The IDs still missing after that are the weather overlays' (0201, 0110…), which aren't in the resolve chain.

**Survey.**
- mapbase and default have the group off (-1). m10 and m14 use 0.
- m60 uses 1 (t0), 2 (t4, t8), 4 (t11.5, t12) and 5 (t15 on). m11 uses 101.
- The Tone Map group's ColorH/S/V and Color Grading ColorShift/ColorScale are neutral (H 0, S 1, V 1, `<1,1,1,0>`)
  in nearly every file. About 15 key ColorH to 1 at t8 and about 14 key ColorV to 1.2 at t4. **Not implemented.**

**Built:**
- `ColorGradingLuts.cs` loads m00-m99's TPFs once through the project VFS, from `GparamLightingBank.GetLuts`, and
  packs every LUT side by side into a 16N x 256 RGBA8 atlas. `Find(area, id)` prefers the area's own LUT, then the
  same mAA, then any.
- `FieldReader.GetKeys` returns the keys either side of the time and the blend. `Interpolate` now shares
  `Neighbours` with it. `GparamColorGrading` gains NextLutSourceId, Blend, Luts, Slot and NextSlot. The area looked
  up is the resolved file's mAA_BB.
- `ToneMapPass`/`ToneMap.frag`: after the square root, the display colour is looked up trilinearly in both slots
  and blended. The atlas is uploaded once, as a static, and replaced if the bank's LUTs change; a 1 x 1 texture is
  bound without them. No grading in pass-through debug views.
- CFG `Viewport_Post_Color_Grading` (on), in Post Effects. The table's Color Grading row shows each ID with the
  texture it resolved to, e.g. `2 (m60_00_CGrading0002) to 4 (m60_00_CGrading0004), 0.88 of the way`, or "not
  found".
- Hypothesis: the LUTs are indexed by the display colour Smithbox writes (the tone map's square root). If the game
  indexes in sRGB or before its own curve, the grade will look a little off in the darks.
- `FadeTime` (a gameplay transition) is ignored.

Check:

| Where | Do | Expect | If wrong |
|---|---|---|---|
| ER m60_42_36_00, map lighting on | Post Effects table | Color Grading row: at t8 `2 (m60_00_CGrading0002)`, at t10 `2 (...) to 4 (...), 0.57 of the way`, at t16 `5 (...)` | "not found": the atlas didn't load or the name pattern differs |
| Same | Toggle Color Grading at t8, t12, t16 | A tint and contrast change that differs between the times; no banding in skies | Strong colour cast or posterised gradients: the slice order or the index space is wrong |
| Same | Drag the time slowly from t8 to t11.5 | The grade shifts smoothly, no jump | A jump: the blend isn't applied |
| ER m10_00_00_00 | Toggle | A subtle change from `m10_00_CGrading0000` | |
| ER m11_00_00_00 (Leyndell) | | The row reads `101 (m11_00_CGrading0101)`, and toggling changes the image | |
| Map lighting off, Model Editor | | No change | |

### Phase 8: Wet

1. Resolve chain: the chosen weather (`WeatherParam.GparamId`) adds `m00_00_WWWW_weatheroutdoor` (outdoor maps) or
   `_weatherbase` above the map file. Which one applies where is a hypothesis to check against the exe's
   `m%02u_%02u_%04u_WeatherBase` / `_WeatherOutdoor` formats.
2. `FlverShader.frag`: albedo lerped towards `OverrideAlbedo` by `AlbedoRate`, roughness towards
   `1 - OverrideShininess` by `ShininessRate`, on up-facing outdoor surfaces (N.y and not under a roof: use the sun
   shadow map's top-down depth as a rain mask, or skip occlusion at first).

Check: ER m60 tile with a rain weather (WW 20 or 30): ground darker and glossier, more so with weather 30; sheltered
interiors unchanged once the rain mask is in.

### Phase 9: TAA

Jitter the projection by `JitterCount` Halton offsets, reproject the history with the previous view-projection from
depth (map geometry is static, so camera motion covers it; animated characters ghost and get clamped by
neighbourhood colour clipping), blend by `FeedbackWeight` scaled by the depth bands (`FBWeightBorderDepth`,
`FBWeightCancelByDepth`), sharpen by `SharpenIntensity`. Replaces FXAA when `AntiAliasType` is the TAA value.
Jitter off while picking, so selection IDs stay stable.

Check: ER m60 grass and fences stable while the camera is still, without smearing when it moves; clicking still
selects the right object.

## Verification maps

- ER m60_42_36_00 (Limgrave tile): light shafts, LUT changes through the day, bloom, rain weathers.
- ER m10_00_00_00 (Stormveil): height fog, SSAO in volume-lit interiors, SSR on wet stone, no light shafts.
- ER m30_xx (a catacomb, via `GparamRefSettings` -> m30_00): auto exposure in the dark, glow on emissives.
