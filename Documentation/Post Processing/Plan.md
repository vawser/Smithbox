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

### Phase 0 as built (2026-11-03; not yet checked in the app)

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

### Phase 3: light shafts

Screen-space radial blur from the light's screen position: sky mask where depth > `MaskDepthThreshold`, shaped by
`SkyMaskSizeAngle`/`CenterPower`, tinted by `Sky Multiply/Add Color`, blurred `ShaftBlurCount` times over
`ShaftBlurRange`, faded by `Fade Out Start/End Angle`. The direction is `LightShaft Angle` unless `EnableRefLight`,
then the LightSet sun (bank 160000000 is taken to mean the scene LightSet; to confirm). `Parallel Mode` angles
switch to a directional blur when the light is far off screen.

Check: ER m60_42_36_00 at t8 facing the sun through trees: rays fanning from the sun disc; none in m10_00_00_00
(disabled there).

### Phase 4: screen outputs for normals and indirect light

The one invasive step. `SceneTarget` gains two colour attachments: normal (octahedral) + roughness/metalness
(RGBA16F or RGB10A2), and indirect diffuse (R11G11B10F). `FlverShader.frag` writes them; every other shader that
draws into `SceneTarget.Outputs` (collision, nav, debug wire, grid, FogVolume, SimpleFlver) gets matching outputs
with write masks off, so their pipelines stay valid. Blend states need one entry per attachment. Transparents write
none of them.

Check: a debug view (Post Effects > Show: Normals / Roughness / Indirect) that draws each attachment. Nothing else
changes; nav meshes and collisions still draw.

### Phase 5: SSAO

Half-res, from depth and the normal attachment: hemisphere samples with kernel radius `Kernel Scale Min/Max` pixels
over `Base Dist`, `AO = saturate(raw x Scale + Offset)`, faded out over `DepthFadeStart/Range`, bilateral blur, then
the composite subtracts `indirect x (1 - AO)` from the scene colour. The LightSet's `SsaoWeight0-3` scale how much
AO also darkens each directional light's contribution; first pass ignores them (0 for the sun in m10 and m60 at t0 is
the common case), then add a second attachment term if the game clearly darkens sunlit creases.

Check: ER m10_00_00_00: dark contact shadows where walls meet floors and under ledges, in volume-lit interiors as
well as outside; sunlit faces not darkened. Toggle off: exactly Phase 4's image.

### Phase 6: SSR

Copy the HDR colour at the depth capture point; build a Hi-Z mip chain of depth (compute). For pixels whose
roughness passes `Shininess Reject Threshold` and Fresnel passes `Fresnel Reject Threshold`, march the reflected
ray (`Ray Trace Step Scale Min/Max`), sample the colour copy at a mip from `Display Mip Scale/Offset` x roughness,
fade at screen edges, near the viewer (`Fade To Viewer Start`) and over `Depth Fade Range`, and blend over the
hemisphere/envmap reflection already in the image (needs the specular term in an attachment, or redo it here from
the normal attachment).

Check: ER m10_00_00_00 courtyard puddles and polished floors reflect nearby walls, falling back to the sky colour at
screen edges. In a file with `SSR Enable` false, none.

### Phase 7: colour grading

Research first: where the Yebis LUTs for `LutSourceId` 0-5 are (search the VFS for tpfs or raw volumes in
`/other`, `/menu`, `/param/drawparam`; strings near `LutSourceId` in the exe). Then a 3D LUT applied after tone
mapping in `ToneMap.frag`, the two LUTs either side of the time blended like the GI bakes. `FadeTime` is a gameplay
transition and is ignored.

Check: ER m60_42_36_00 dragging the time 4 -> 12 -> 15: the grade shifts warmer towards evening and cooler at night,
on top of the sun's colour change.

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
