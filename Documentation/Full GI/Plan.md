# Implementation plan

Scope: AC6, ER, NR map models in the Map Editor viewport. Diffuse GI first; reflection normalisation later.
Each phase ends in something the user can check by eye (CLAUDE.md: no launching the program to verify).

## Phase 1: find and load the data (no rendering change)

1. `ProjectFileLocator`: add `IrradianceVolumeInfoFiles` (`.ivinfobnd`) and `EnvMapTextureFiles`
   (ER/NR `*_envmap_*_high_*.tpfbnd`, AC6 `*_envmap_*_high_{w,x,y,z}.tpf`), excluding `sd`, like `FogVolumeFiles`.
2. New `Editors/Map Editor/Data/IrradianceVolumeBank.cs`, modelled on `FogVolumeBank`:
   - `CanUse()` for AC6, ER, NR.
   - On map load, read the high ivinfobnd into `Dictionary<(int id, int bake), IVINFO>`.
   - Collect the map's EnvMapEffectBox regions and resolve each to an ID from properties only: AC6
     `EnvMapEffectBox_UnkT32`, ER/NR `Unk0A` when >= 0. Boxes with `Unk0A = -1` get no GI; GI code never
     reads names. Keep region, ID, fade, priority and intensities.
   - Count the ER/NR boxes left unlinked, for the offer below.
   - Expose the four texture names per volume for the chosen bake.
3. **Offer to fill `Unk0A`** (decided 2026-11-03). Built as `Tools/Validation/IrradianceVolumeIdTool.cs`, an
   "Irradiance Volumes" tab of the tool window's Validation section:
   - Shown for ER and NR only, when the loaded map has EnvMapEffectBoxes with `Unk0A < 0`. The GI viewport
     section shows "N environment boxes have no volume ID" with a button that opens it.
   - The tool window lists those boxes with the ID the game would use, and a **Fill volume IDs** button.
     It explains that this writes the ID the game already reads from the name, so the game is unaffected.
   - The only place in Smithbox that reads the name, and only as the user-approved source of a one-off write.
     It follows the game's own rule exactly: shape is Box, name is exactly 10 characters, characters 7-9 are
     digits, ID = their value.
   - Writes go through one `ViewportCompoundAction` of `PropChangeAction`s on `Unk0A`, so a single undo
     reverts them and the map is marked modified; nothing is saved until the user saves.
   - Reported, not changed: boxes the game skips (not a Box, or a name that is not 10 characters) and boxes
     whose last three characters are not digits. IDs with no `GIIV` volume are still filled (the game uses that
     ID either way) and flagged in the list.
   - After filling, the bank re-resolves links so GI picks the boxes up without reloading the map.
4. Pref `Viewport_GI_Bake` (default 0) and a read-only status per map in the viewport's
   Viewport Rendering > Global Illumination menu. `Viewport_GI_Enable` moved to Phase 4, where it has something to switch.
5. Temporary log line per map: volumes found, boxes linked, boxes unlinked. User passes the log back.

Check:
- AC6 m00_10_00_00: one box, linked to ID 500, nothing to fill.
- ER m10_00_00_00: 896 volumes over 7 bakes (128 per bake). Before filling, all 129 boxes are unlinked and the
  offer appears. After filling, 128 are linked and Env_Box902 is reported as having no volume. One undo
  returns all 129 to -1.
- NR m10_00_00_00: 36 boxes, all unlinked before and linked after filling, with no reports.
- Rename one filled ER box (e.g. Env_Box010 to Env_Box010_x): its link must not change in Smithbox. The tool
  should now report it, because at 14 characters the game skips that box.

## Phase 2: 3D textures on the GPU

1. `TexturePool.TextureHandle.FillWithDDS`: 3D branch (Image3D, depth from `dds.dwDepth`, per-mip size
   including depth). Keep 2D/cube unchanged.
2. `SceneRenderer`: `GlobalVolumeTexturePool` (start at 2048 slots: ER m10 has 128 volumes x 4 textures per bake),
   bind at set 8, rebuild dirty tables, clean with the others. Fill slot 0 with a 4x4x4 BC7 or RGBA8 texture of
   value (0.5, 0.5, 0.5, 0.5), which decodes to zero light.
3. `TextureResource.LoadVulkanTexture`: route volume TPFs to the new pool.
4. `MapResourceHandler`: queue the bank's envmap textures for the chosen bake (ER/NR open the tpfbnd and take
   only `GIIV` textures).

As built:
- The 3D upload only happens for handles of the volume pool. The texture viewer (`VulkanTextureHandle`) still
  puts volumes in the 2D pool, where only the first slice is uploaded, as before.
- `SceneRenderer` draws bind sets 2-7 for every batch, so the volume pool can't be bound until the pipeline
  layouts gain set 8 (Phase 3/4). Until then it is created, its tables regenerated and cleaned, but never bound.
- Volume textures go in the "Miscellaneous textures" load list, so they follow `MapEditor_TextureLoad_Misc`.
- The Global Illumination menu shows "Volume textures on the GPU: N" (excluding the default at slot 0).

Check: no crashes or validation errors loading the three maps, and the GPU count matches the volumes:
AC6 m00_10_00_00 bake 0 = 4, NR m10_00_00_00 bake 0 = 4 x 36 = 144 (plus any extra textures in that tpfbnd),
ER m10_00_00_00 bake 0 = 4 x 128 = 512. Unloading the map should lower it (unverified: textures nothing
listens to may only be freed by `ResourceManager.ClearUnusedResources`).

## Phase 3: the volume table (planned; see "Phase 3 as built" below, which replaced sets 8 and 9)

1. New `Renderer/Structs/IrradianceVolumeData.cs` (std430):
   `Matrix4x4 InvWorld` (world -> -1..1), `Vector4 ExtentsFade` (half extents xyz, 1/FadeDistance w),
   `Vector4 SafeJacobian` (xyz), `Vector4 VoxelCounts` (xyz), `Vector4 Params` (diffuse scale, priority, -, -),
   `uint4 Textures` (X, Y, Z, W pool indices).
2. A storage buffer of these plus a count, filled by the bank when textures become resident, sorted by
   Priority then volume descending, so the loop below blends coarse-to-fine like the game's draw order.
3. Add the count and buffer binding (set 8, binding 1) to the FLVER pipeline layouts.

### Linking changed to the game's behaviour (2026-11-03)

`ResolveVolumeId` replaces the property-only link of Phase 1: ER/NR skip boxes that aren't a Box or whose name
isn't 10 characters, then use the ID property if >= 0, else the name's characters 7-9 with the game's unchecked
arithmetic; AC6 uses `EnvMapEffectBox_UnkT32`. The summary, the shading table and the tool all use it. The tool
shows each box's property, resolved volume and how it was linked; the GI menu no longer prompts to fill, and
its "Volume Links..." item opens the tool. Vanilla ER/NR maps now light without filling.

### Phase 3 as built (2026-11-03)

Decided with the user: stay within 8 descriptor sets, as some GPUs allow no more, instead of adding sets 8 and 9.
- Volume textures: binding 1 of set 3. `TexturePool` takes an optional secondary pool that shares its
  descriptor set; `SceneRenderer.GlobalCubeTexturePool` owns it and `GlobalVolumeTexturePool` is its secondary.
  Shader: `layout(set = 3, binding = 1) uniform texture3D globalTexturesVolume[];`
- Fixed a Veldrid bug this exposed: `ResourceSet` stepped one resource too far after a sampled image array, so a
  second array binding started one slot late and wrote past the image info buffer. No existing layout had a
  binding after an array, so nothing else changes.
- Volume table: `IrradianceVolumeTable` (Renderer/Scene), 512 entries of `IrradianceVolumeData` (128 bytes) in one
  allocation of the material buffer (set 4), aligned so its start is an entry index. Shader view:
  `volumedata[sceneparam.irradianceVolumeStart + i]` for `i < sceneparam.irradianceVolumeCount`.
- `SceneParam` gains `IrradianceVolumeStart` and `IrradianceVolumeCount` at the end; `FlverShader.frag`'s copy of
  the struct now runs to them. The count is 0 unless the viewport's `SceneRenderPipeline.UseIrradianceVolumes` is
  set, which only Map Editor viewports do, and only counts entries already uploaded.
- `IrradianceVolumeBank.UpdateGpuTable` rebuilds the table every frame from `MapEditorScreen.Update`, from linked
  boxes of loaded maps whose four textures of the bake are resident in the volume pool, skipping boxes the game
  skips; sorted by priority, then largest first. It only uploads when the bytes change.
- World to local: `inverse(entity.GetWorldMatrix()) * scale(2) * translate(0, -1, 0)`, since the region's unit box
  spans -0.5..0.5 on X and Z and 0..1 on Y. Uploaded like `FogVolumeMaterial.InverseWorld` (C# row vectors,
  read in GLSL as `m * vec4(p, 1)`).
- AC6 fields used: `TransitionDist` as the fade distance and `EnvMapEffectBox_UnkT09` as the priority (the same
  offsets as ER's `FadeDistance` and `Priority`); diffuse scale 1, as AC6 has no AmbientDiffuseIntensity.
- The GI menu shows "Volumes in the shading table: N of 512".

Check: no validation errors or crashes; the table count equals the linked volumes of the bake (ER m10_00_00_00
after Fill Volume IDs: 128; NR m10_00_00_00: 36; AC6 m00_10_00_00: 1). Moving a box doesn't change the count.
Rendering must look exactly as before, as nothing reads the table yet.

## Phase 4: shading

Add `CFG.Viewport_GI_Enable` (moved here from Phase 1) to switch between the volumes and today's cubemap.

In `FlverShader.frag`:

```glsl
vec3 VolumeIrradiance(vec3 P, vec3 N, out float coverage)
{
    vec3 E = vec3(0); coverage = 0;
    for (uint i = 0; i < volumeCount; i++)            // already sorted coarse to fine
    {
        IrradianceVolume v = volumes[i];
        vec3 local = (v.invWorld * vec4(P, 1)).xyz;
        vec3 d3 = (1 - abs(local)) * v.extentsFade.xyz;
        float d = min(d3.x, min(d3.y, d3.z));
        if (d < 0) continue;
        float fade = smoothstep(0, 1, clamp(d * v.extentsFade.w, 0, 1));
        vec3 uvw = (local * 0.5) * v.safeJacobian.xyz + 0.5;
        uvw += normalize(mat3(v.invWorld) * N) / v.voxelCounts.xyz;
        vec3 Ev = EvaluateSH(sample X/Y/Z/W at uvw, N) * v.params.x;
        E = mix(E, Ev, fade);  coverage = mix(coverage, 1, fade);
    }
    return E;
}
```

`EvaluateSH` is the decode plus non-linear L1 from Shaders.md. In `IndirectLight`, use
`mix(cubemapDiffuse, volumeIrradiance, coverage)` for the diffuse term, behind a specialization constant or
`materialFeatures` bit so non-GI games keep today's path. Keep `ambientLightMult` as an overall multiplier.

The coordinate system needs care: Smithbox flips Z for the cubemap lookup (`N * vec3(1, 1, -1)`), so check whether
world space in the shader matches MSB space or is mirrored, and match the box transform used for region drawing.

Check: see the table below. A debug view mode that outputs `E` alone (no albedo) will make problems obvious.

### Phase 4 as built (2026-11-03)

- `FlverShader.frag`: `DecodeVolumeTexel`, `EvaluateSHChannel` (the game's non-linear L1), and `VolumeIrradiance`,
  which loops over the table, fades each box in from its faces, offsets one voxel along the normal and clamps
  to the outer voxel centres (the samplers repeat). `IndirectLight` mixes the cubemap's diffuse light towards
  the volumes' by their coverage; specular still comes from the cubemap.
- SH bands are in world axes, as in the game: the box only sets where to sample, not how to evaluate.
- Brightness: the game's light is in HDR units, about 0.01-0.03. `IrradianceVolumeTable.AutoExposure` scales the
  shaded volumes' average IVINFO `ShLuminance.w` to 0.5, the default ambient, and is multiplied by
  `CFG.Viewport_GI_Intensity`; with `Viewport_GI_Auto_Exposure` off the intensity is the exposure.
  `SceneParam.IrradianceVolumeExposure` carries it.
- `CFG.Viewport_GI_Enable` (default on) switches it off, sending a count of 0.
- Viewport Rendering > Global Illumination now has Light with Irradiance Volumes, Auto Exposure and Intensity.

Check (after Fill Volume IDs for ER and NR):

| Map | Where | Expect |
|---|---|---|
| ER m10_00_00_00 (Stormveil) | Interiors and the courtyard | Dark interiors and corridors, brighter open areas, instead of uniform grey ambient. Undersides darker than tops |
| NR m10_00_00_00 | Env_Box302-305 (5x9x5 m, fade 0.5) | Small rooms clearly darker than the area outside; a soft 0.5 m blend at the box faces |
| AC6 m00_10_00_00 | The hangar box (ID 500) | Lit inside the box; outside it, models keep the environment map's grey |
| Any | Toggle Light with Irradiance Volumes | Only indirect light changes; direct sun and specular stay the same |

Signs of a wrong assumption, and what to change:
- Light coming from the wrong horizontal side (e.g. walls facing away from openings are lit): Smithbox's world
  X or Z is mirrored relative to the game's; flip that component of N (and of L1) in `VolumeIrradiance`.
- Light shifted by a box's size or offset vertically: the bottom-centre origin is wrong; adjust the
  `translate(0, -1, 0)` in `IrradianceVolumeBank.TryBuildEntry`.
- Speckles or light leaking through thin walls: the normal offset is too big; try half a voxel.
- Seams between nested boxes: the drawing order; compare priorities in the tool tab.

### Phase 5 as built (2026-11-03)

- `IrradianceVolumeData` is now 256 bytes: world bounds, both bakes' textures and IVINFO `ShLuminance`, the
  blend ratio, and a reflection scale (ER/NR `AmbientSpecularIntensity`; 1 for AC6, whose `SpecularLightMult`
  reaches 8 and isn't confirmed to be the same thing).
- Reflection normalisation (game's GXEnvMapRegion): per bake, the luminance of the linear SH
  `max(0, W + L1 . N)` over `max(0, dot(ShLuminance, (0.5 N, 1)))`, 1 if the IVINFO L0 is 0 or less; blended
  across volumes like the light, starting at 1. Scales the cubemap specular. `Viewport_GI_Normalize_Reflections`.
- Bake blending: `Viewport_GI_Bake_Blend`, `Viewport_GI_Blend_Bake`, `Viewport_GI_Blend_Ratio`. The second bake's
  envmap files load with the map; volumes without that bake keep the first.
- Per-draw culling was not needed as such: each volume's world bounds are read and tested before anything else,
  so the loop costs two vec4 loads and six compares for every volume a pixel is outside.
- `SceneParam.IrradianceVolumeFlags` (bit 0: normalise reflections).
- Not done: NR's `_01` mid-range volumes (no IVINFO or box; placement unknown), and removing the temporary
  per-map load log line from Phase 1, which waits for the in-app checks.

Check:

| Feature | Where | Expect |
|---|---|---|
| Normalize Reflections | ER m10_00_00_00, a shiny floor running from a lit courtyard into a dark corridor | Reflections fade in the corridor and stay bright outside; toggling it off makes the corridor's floor glow evenly again |
| Bake blending | ER m10_00_00_00 with bake 0, Blend Bakes on, blend towards 3, reload the map | Dragging Blend Ratio from 0 to 1 changes the indirect light smoothly; "Volume textures on the GPU" doubles |
| Bounds test | ER m10_00_00_00 overview | Same image as before; frame time no worse than Phase 4 |

## Phase 5: refinements (as planned)

- Reflection normalisation with IVINFO `ShLuminance` (Shaders.md), scaling the cubemap specular.
- Bake selection UI and interpolation between two bakes (`InterpRatio`).
- Per-draw culling of the volume list (CPU picks volumes overlapping each model's bounds, like the game's A/B)
  if the per-pixel loop is too slow on ER maps with ~130 volumes.
- NR `_01` volumes and MidRangeEnvMapOutput.

## Risks

- Per-pixel loop cost on big maps: mitigate with the per-draw list or a coarse 3D lookup grid.
- Coordinate handedness and the bottom-centre box origin: get the transform from the existing region renderer.
- Unknown CPU constants (SafeVoxelJacobian, SamplingParam): the guesses only move samples by under a voxel, so
  errors show as slight shifts rather than broken lighting.
- Texture memory: load only one bake and the high quality; free on map unload.

## Verification maps

| Feature | Map | What to look for |
|---|---|---|
| Matching log | ER m10_00_00_00 (Stormveil) | 128 volumes per bake, no unmatched boxes |
| Fade and nesting | NR m10_00_00_00 | Env_Box203 (60x60x48, rotated -90) blends into Env_Box000 over 1.5 m at its faces |
| Interior darkening | NR m10_00_00_00 Env_Box302-305 | 5x9x5 m boxes, fade 0.5: small rooms darker than outside |
| AC6 field link | AC6 m00_10_00_00 | The single 28x26x52 box (ID 500) lights the hangar, no light outside it |
| Huge coarse box | AC6 m03_20_00_00 | ID 0 box, 35000 m wide at 92x16x92 voxels: soft gradients, no banding seams |
