# SceneParams audit

Canonical: `FlverShader/FlverShader.frag`, 77 members, 2272 bytes (std140).

| File | Members | Outcome | Notes |
|---|---|---|---|
| `Collision_focus.frag` | 27 | switch | 25 unread |
| `Collision_focus.vert` | 11 | switch | 8 unread |
| `Collision_selected.frag` | 27 | switch | 25 unread |
| `Collision_selected.vert` | 11 | switch | 8 unread |
| `Collision.frag` | 27 | switch | 25 unread |
| `Collision.vert` | 11 | switch | 8 unread |
| `DebugWire.frag` | 11 | switch | 10 unread |
| `DebugWire.vert` | 11 | switch | 9 unread |
| `DistanceFog.frag` | 25 | switch | 23 unread |
| `FlverBB.frag` | 10 | MANUAL | reads `uint envmap` at 160, which is `ivec4 curserPosition` in the canonical struct; reads `float ambientLightMult` at 164, where the canonical struct has no member start; reads `float directLightMult` at 168, where the canonical struct has no member start; reads `float indirectLightMult` at 172, where the canonical struct has no member start; `sceneBrightness` -> `ambientLightMult`; 4 unread |
| `FlverBB.vert` | 5 | switch | 2 unread |
| `FlverDS3.frag` | 10 | MANUAL | reads `uint envmap` at 160, which is `ivec4 curserPosition` in the canonical struct; reads `float ambientLightMult` at 164, where the canonical struct has no member start; reads `float directLightMult` at 168, where the canonical struct has no member start; reads `float indirectLightMult` at 172, where the canonical struct has no member start; `sceneBrightness` -> `ambientLightMult`; 4 unread |
| `FlverDS3.vert` | 5 | switch | 2 unread |
| `FlverShader/FlverShader.frag` | 77 | switch | 7 unread |
| `FlverShader/FlverShader.vert` | ? | MANUAL | preprocessor line inside sceneParams: `#ifdef MATERIAL_DEFORM`; its layout depends on the variant |
| `FogVolume.frag` | 25 | switch | 19 unread |
| `FogVolume.vert` | 3 | switch | 1 unread |
| `NavSolid_focus.frag` | 29 | switch | 24 unread |
| `NavSolid_focus.vert` | 11 | switch | 8 unread |
| `NavSolid_selected.frag` | 29 | switch | 24 unread |
| `NavSolid_selected.vert` | 11 | switch | 8 unread |
| `NavSolid.frag` | 29 | switch | 24 unread |
| `NavSolid.vert` | 11 | switch | 8 unread |
| `NavWire_focus.vert` | 11 | switch | 8 unread |
| `NavWire.vert` | 11 | switch | 8 unread |
| `SimpleFlver_focus.frag` | 18 | switch | 17 unread |
| `SimpleFlver_focus.vert` | 13 | switch, renames | `SimpleFlver_Brightness` -> `simpleFlverBrightness`; `SimpleFlver_Saturation` -> `simpleFlverSaturation`; 9 unread |
| `SimpleFlver_Outline.frag` | 13 | switch, renames | `OutlineColor` -> `outlineColor`; 12 unread |
| `SimpleFlver_Outline.vert` | 5 | switch | 3 unread |
| `SimpleFlver_selected.frag` | 18 | switch | 13 unread |
| `SimpleFlver_selected.vert` | 13 | switch, renames | `SimpleFlver_Brightness` -> `simpleFlverBrightness`; `SimpleFlver_Saturation` -> `simpleFlverSaturation`; 9 unread |
| `SimpleFlver.frag` | 12 | switch | 11 unread |
| `SimpleFlver.vert` | 13 | switch, renames | `SimpleFlver_Brightness` -> `simpleFlverBrightness`; `SimpleFlver_Saturation` -> `simpleFlverSaturation`; 9 unread |
| `ViewportGrid.vert` | 2 | switch |  |
| `VolumetricFog.comp` | 57 | switch | 43 unread |
| `VolumetricFog.frag` | 25 | switch | 22 unread |

Switchable: 33. Manual: 3.

Applied: wrote `SceneParams.glsl` and rewrote 33 files.
