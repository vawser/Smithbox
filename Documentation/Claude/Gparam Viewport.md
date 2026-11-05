# Gparam Editor Viewport

The Gparam Editor has a 3D viewport, like the Map, Model and Time Act Editors. It shows one map, lit by the
GPARAM selected in the editor, including unsaved edits.

## Pieces

| File | Role |
| --- | --- |
| `Editors/Gparam Editor/GparamUniverse.cs` | The viewport's `IUniverse`. Reads a map's MSB and builds a `MeshRenderableProxy` for each MapPiece and Asset part. Then loads their models and textures and frames the camera on the map pieces. |
| `Editors/Gparam Editor/GUI/GparamViewportWindow.cs` | The `VulkanViewport`, plus a `GparamLightingBank` whose `FileOverride` returns the editor's loaded GPARAM objects. Resolves `Lighting` (scene set 0, models set 100). |
| `Editors/Gparam Editor/Tools/GparamViewportPreviewTool.cs` | Tool window section: status, map list, load/frame/unload, options, time of day. |
| `VulkanViewport` | `GparamUniverse` owner: scene, gizmos, grids (`ViewportGridEditor.Gparam`), focus, and `MapLightingSource = () => ViewportWindow.Lighting`. |

## Behaviour

- **Which map**: for `mAA_BB_CCWW` the viewport uses `mAA_BB_00_00`. Failing that, the first `mAA_BB_*_00` map, then
  any map whose `GparamLightingBank.GetArea` is the area. Shared files (mapbase, default, ...) map to no map, so a
  map has to be picked in the list.
- **Isolation**: the MSB is the Map Editor's copy if it is already read, and only read from. Otherwise the viewport
  reads its own copy, so `MapWrapper.MSB` is never replaced. Nothing becomes an entity, so the map can't be edited
  here. The map sits at the origin, without ER/NR tile offsets.
- **Threading**: the MSB is read on the thread pool. The meshes are built on the main thread in
  `GparamUniverse.Update`. A generation counter drops loads that a newer load or an unload has overtaken.
- **Lighting refresh**: the lighting is resolved again when the file, map, hours or weather changes. It is also
  resolved again on any `ActionManager.ActionPerformed` (edit, undo, redo), and every 100 ms while an ImGui item is
  active, because colour drags only commit when they are released.
- **Same settings as the Map Editor**: the time of day (`Viewport_Map_Lighting_Hours`) and weather
  (`Viewport_Map_Lighting_Weather`, `_Weather_Outdoor`) are the Map Editor's, as the Model and Time Act viewports'
  are. The other map lighting, GI and BTL settings are shared too. They are set in the Viewport Rendering section,
  which has Map Lighting, Global Illumination and BTL Lights tabs for the Gparam owner
  (`GparamLightingTab`, `GparamGlobalIlluminationTab`). The one difference is the file: the GPARAM selected in the
  editor is used as it is, while the Map Editor uses its chosen file, swapped for the weather's version when there
  is one.
- **Time of day override**: with `GparamEditor_Viewport_Follow_Value_Time` (off by default), the viewport follows
  the selected value's TimeOfDay instead, but only when its field has more than one time key.

## Previewable fields

`GparamLightingBank.FieldReader` records every (group key, field key) it is asked for, per bank. `IsRenderedField`
and `IsRenderedGroup` resolve the lighting once, with no map or weather, the first time they are called. Every read
is unconditional, so that one resolve lists all the fields the game's profile reads. The group and field lists draw
those in `UI.ImGui_GparamPreviewable_Text`, with a tooltip line, behind
`CFG.GparamEditor_Highlight_Viewport_Fields`. MSB fog volume fields (`FogVolumeBank`) aren't included, as the
Gparam viewport doesn't draw fog volumes.

## Baked and placed light (`Editors/Gparam Editor/Framework/GparamMapEnvironment.cs`)

Each view has its own `IrradianceVolumeBank`, `IrradianceVolumeTable` and `DolLightmapBank`. The Map Editor's
copies are never touched, so the same map can be open in both editors.

- **Irradiance volumes** (AC6/ER/NR): the map's `EnvMapEffectBox` regions are linked to volumes by
  `IrradianceVolumeBank.ResolveVolumeId`. The volume textures are queued with the map. They go into the
  pipeline's own `VolumeTable` through `IrradianceVolumeBank.UpdateTable` (a generalised `UpdateGpuTable`). ER's
  bakes follow the viewport's own time of day (`ResolveBakes(..., hours)`).
- **Light probe grids** (DS3/BB): the BTPB `_0000` file from the Map Editor's `LightProbeBank.Files`. Its groups go
  through `LightProbeGridBank.AppendEntries(groups, found, shColor)`, lit by the viewport's GPARAM `ShColor`.
- **Light atlases** (DS3): the area's DOL binder is queued with the map. The BTAB `_0000` entries come from
  `LightAtlasBank.Files`, and the pieces are lit through `DolLightmapBank.Update(LightmapMap...)`, with parts keyed
  by their MSB object.
- **BTL lights**: read off the main thread with `MapUniverse.ReadBTL`. Each frame they are picked by distance from
  the camera, using `MapLightCollector`'s helpers, and uploaded to the shared `SceneRenderer.MapLights`. That table
  and `SceneRenderer.Lightmaps` are shared by every viewport, but only the focused editor updates them and only its
  viewports are drawn, so the editors don't fight over them.

The Map Editor's own call paths are kept as thin wrappers over the generalised methods, so its behaviour is
unchanged.

## Not done (possible follow-ups)

- No characters or collisions.
- No ER/NR tile transform, so a map that is normally offset shows at the origin.
- No viewport filter menu entries for the Gparam owner.
- Volume and lightmap textures aren't held. If another editor's `ClearUnusedResources` frees them while the Gparam
  Editor is out of focus, they don't come back until the map is reloaded. The Map Editor behaves the same way.
- Temporary `[Gparam Viewport]` log lines in `GparamUniverse` (map built, camera framed): remove them once the viewport is checked in the app.
