# SPIR-V sceneParams offsets, HEAD vs working copy

| File | Old members | New members | Result |
|---|---|---|---|
| `src/Smithbox.Data/Assets/Shaders/Collision_focus.frag.spv` | 27 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/Collision_focus.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/Collision_selected.frag.spv` | 27 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/Collision_selected.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/Collision.frag.spv` | 27 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/Collision.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/DebugWire.frag.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/DebugWire.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/DistanceFog.frag.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverBB.frag.spv` | 10 | 10 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverBB.vert.spv` | 5 | 77 | `envmap`@160 is `curserPosition`; `envmap` moved 160 -> 176 |
| `src/Smithbox.Data/Assets/Shaders/FlverDS3.frag.spv` | 10 | 10 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverDS3.vert.spv` | 5 | 77 | `envmap`@160 is `curserPosition`; `envmap` moved 160 -> 176 |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend_focus.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend_focus.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend_selected.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend_selected.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blend.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask_focus.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask_focus.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask_selected.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask_selected.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_blendmask.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend_focus.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend_focus.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend_selected.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend_selected.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_blend.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_focus.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_focus.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_outline.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_outline.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_selected.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform_selected.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_deform.vert.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_focus.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_focus.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_selected.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader_selected.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader.frag.spv` | 77 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FlverShader/FlverShader.vert.spv` | 4 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FogVolume.frag.spv` | 25 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/FogVolume.vert.spv` | 3 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid_focus.frag.spv` | 29 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid_focus.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid_selected.frag.spv` | 29 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid_selected.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid.frag.spv` | 29 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavSolid.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavWire_focus.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/NavWire.vert.spv` | 11 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_focus.frag.spv` | 18 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_focus.vert.spv` | 13 | 77 | `SimpleFlver_Brightness`@200 is `simpleFlverBrightness`; `SimpleFlver_Saturation`@204 is `simpleFlverSaturation` |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_Outline.frag.spv` | 13 | 77 | `SimpleFlverSelectColor`@208 is `selectionColor`; `OutlineColor`@224 is `outlineColor` |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_Outline.vert.spv` | 5 | 77 | `envmap`@160 is `curserPosition`; `envmap` moved 160 -> 176 |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_selected.frag.spv` | 18 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver_selected.vert.spv` | 13 | 77 | `SimpleFlver_Brightness`@200 is `simpleFlverBrightness`; `SimpleFlver_Saturation`@204 is `simpleFlverSaturation` |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver.frag.spv` | 12 | 77 | `whiteTint`@200 is `simpleFlverBrightness` |
| `src/Smithbox.Data/Assets/Shaders/SimpleFlver.vert.spv` | 13 | 77 | `SimpleFlver_Brightness`@200 is `simpleFlverBrightness`; `SimpleFlver_Saturation`@204 is `simpleFlverSaturation` |
| `src/Smithbox.Data/Assets/Shaders/ViewportGrid.vert.spv` | 2 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/VolumetricFog.comp.spv` | 57 | 77 | same |
| `src/Smithbox.Data/Assets/Shaders/VolumetricFog.frag.spv` | 25 | 77 | same |
