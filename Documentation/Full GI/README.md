# Full GI

Goal: light map models in the Map Editor viewport with the baked irradiance volumes of Armored Core VI, Elden Ring
and Nightreign, so indirect light varies through the map as it does in game instead of coming from one grey cubemap.

Status (2026-11-03): Phases 1-5 implemented, awaiting in-app checks (Plan.md). Map models are lit by the
volumes' diffuse light, reflections are normalised by IVINFO, two bakes can be blended, and volumes are skipped
by their world bounds. Left: NR `_01` mid-range volumes, and removing the Phase 1 load log once checked. Phase 3: the volume table in
the material buffer and the volume textures at set 3 binding 1, declared in FlverShader.frag but not read yet. Phase 2 code:
`TextureHandle.IsTPFVolume` and the 3D branch of `FillWithTPF`, `FillWithColorVolume`,
`SceneRenderer.GlobalVolumeTexturePool` (created and maintained, not bound yet), routing in `TextureResource`,
`TextureLocator.GetEnvMapTextureVirtualPath`, the `map/{id}/envmap/{file}` branch of `PathBuilder`, and the
bake's envmap files queued in `MapResourceHandler.SetupTextureLoadLists`. `SoulsFormats.IVINFO` exists and
round-trips every file. Phase 1 code: `IrradianceVolumeBank` (Editors/Map Editor/Data), `IrradianceVolumeIdTool`
(Editors/Map Editor/Tools/Validation), locator entries `IrradianceVolumeFiles` and `EnvMapTextureFiles`,
`CFG.Viewport_GI_Bake`, the Viewport Rendering tool section's Global Illumination tab, and
`Map-Editor-Tools-Irradiance_Volumes.json`. The per-map load log line is temporary diagnostics; remove it once
checked.

## Files in this folder

| File | What it holds |
|---|---|
| [Data.md](Data.md) | The game files: volume textures, IVINFO, the MSB region that places each volume, how they link per game |
| [Shaders.md](Shaders.md) | What the game's shaders do with the data, decoded from DXIL: texel decode, UVW, fade, SH evaluation, cubemap normalisation |
| [Renderer.md](Renderer.md) | How Smithbox's renderer works today and what has to change for this |
| [Plan.md](Plan.md) | The implementation plan in phases, with files, open questions and verification maps |
| [Tools.md](Tools.md) | How the analysis was done, and how to rerun it |
| [Gparam Lighting.md](Gparam%20Lighting.md) | Plan for lighting viewports from GPARAM LightSet and Tone Map (sun, hemisphere, exposure). Phases 1 (read only) and 2 (sun, exposure) built and checked 2026-11-03; Phase 3 (hemisphere ambient and reflections) checked; its BG colour, Phase 4 (model set for characters) and Phase 5 (area, weather, time, bakes following the time) built, awaiting checks |
| `tools/` | Source of the throwaway C# analysis tool (`ivtool`) |

## The short version

- Each volume is an `EnvMapEffectBox` MSB region with four 3D BC7 textures `GIIV####_##_W/X/Y/Z` in the map's
  envmap TPFs. The texture fills the region's box exactly.
- Smithbox links boxes to volumes exactly as the game does (`IrradianceVolumeBank.ResolveVolumeId`). AC6:
  `EnvMapEffectBox_UnkT32`. ER/NR (traced in eldenring.exe): skip unless a Box with a 10-character name; the ID is
  `UnkT0A` (ER) / `Unk0A` (NR) when >= 0, else the name's last three characters. Vanilla boxes leave the property
  at -1, so they link by name. The tool can fill the property from the name, as an option.
- The textures hold L0 (W) and L1 (X, Y, Z) spherical harmonics per RGB channel, with a per-texel exponent in alpha.
- The game rasterises each region box in a deferred pass, fades it in over `FadeDistance` metres inside its faces,
  and writes diffuse irradiance and a normalised cubemap reflection.
- Smithbox renders forward, so the plan is a per-pixel loop over the map's volumes in `FlverShader.frag`, reading
  3D textures from a new bindless pool, replacing `IndirectLight`'s diffuse term when a pixel is inside a volume.
