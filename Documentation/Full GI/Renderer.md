# Smithbox renderer: current state and what has to change

## Indirect light today

`src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader.frag`, `IndirectLight(Surface s)` (around line 720):
diffuse and specular both come from one cubemap, `globalTexturesCube[sceneparam.envmap]`, mip 5 for diffuse.
`sceneparam.envmap` is index 0 by default, a flat 0.5 grey cube made in `SceneRenderer` (constructor,
`FillWithColorCube`). `VulkanViewport.SetEnvMap` can change it; `ViewportMenu` only sets 0. The sun and
`ambientLightMult` / `indirectLightMult` are viewport settings. No map lighting data is loaded at all.

## Descriptor sets used by the FLVER pipeline

| Set | Content |
|---|---|
| 0 | `SceneParamBuffer` (`Renderer/Structs/SceneParam.cs`, mirrored as `sceneParams` in the shader) |
| 1 | per-object instance data |
| 2 | `globalTextures[]` 2D bindless pool (`SceneRenderer.GlobalTexturePool`, 32768) |
| 3 | `globalTexturesCube[]` (`GlobalCubeTexturePool`, 500) |
| 4 | material storage buffers (several views of one buffer) |
| 5 | samplers (`linearSampler`, `anisoLinearSampler`) |
| 6 | picking buffer |
| 7 | bones / skin buffer (vertex) |

Since Phase 3: set 3 also has binding 1, the volume textures, and set 4's buffer holds the volume table.
No set 8: some GPUs allow only 8 bound descriptor sets.
Every pipeline built with the FLVER shader must get the new layout, including `_selected`, `_focus`, `_blend`,
`_deform` variants (`FlverShader.json` lists them all) and `FlverResource` pipeline construction.

## Textures

- `Renderer/Scene/TexturePool.cs`: `TexturePool` is a bindless array of `SampledImage`, so a third pool,
  `globalVolumeTextures`, works the same way as the cube pool (`new TexturePool(device, "globalVolumeTextures", N)`,
  bind in `SceneRenderer` next to `GlobalCubeTexturePool.BindTexturePool(cl, 3)`, regenerate dirty tables, clean).
- `TextureHandle.FillWithDDS` (around line 700) always builds `VkImageType.Image2D` with `Depth = 1`. A volume
  needs a variant: `Type = Image3D`, `Depth = dds.dwDepth`, `ArrayLayers = 1`, no cube flag, and copy
  `rowPitch * rowsOfBlocks * depth` bytes per mip.
- Veldrid supports it: `TextureDescription.Texture3D`, `TextureView` makes `VkImageViewType.Image3D`, and
  "linear" staging textures are backed by a `VkBuffer`, so the copy goes through `vkCmdCopyBufferToImage` with a
  depth pitch (`CommandList.cs` around line 2190). BC7 3D therefore needs no linear image.
- `TextureResource.LoadVulkanTexture` picks the pool with `TextureHandle.IsTPFCube`; add an `IsTPFVolume`
  (`TPF.TexType.Volume`, or DDS `DDSCAPS2.VOLUME` / DX10 dimension 4) and route to the new pool.
- Texture loading for maps goes through `MapResourceHandler` load lists (see how `FogVolumeBank.GetTextureNames`
  adds asset textures in commit e874c4247). Envmap TPFs are not located by anything yet.

## Existing pattern to copy: fog volumes (commit e874c4247)

`FogVolumeBank` (Editors/Map Editor/Data), `ProjectFileLocator.FogVolumeFiles`, `MapContainer.LoadFVB`,
`FogVolumeRenderableProxy`, `FogVolumeMaterial` struct, `RenderFilter.FogVolume`, `ViewportPrefs` toggles,
`CFG` fields, localisation in `Map-Editor-Data.json` / `Preferences-Viewport.json`. GI does not need entities
(the boxes are already MSB regions), but needs the bank, locator entries, prefs and filter plumbing.

## Build and shader notes (from memory)

- Rebuild SPIR-V with the VariantCompiler command in the `shader-rebuild` memory; `compile.ps1` is stale.
- Build `Smithbox.Vulkan`, not the solution (building the sln rewrites tracked `Smithbox.Release/Output`).
- Probes that read game data must run from a folder with `Assets`, and need the game's `oo2core_*.dll` in the
  working directory.
