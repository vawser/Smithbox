# DES Light Set Redesign

Status (2026-11-08): **light sets disabled; the user confirmed in-app that the crash is gone.** So the crash is in the
light-set code itself. Which candidate below triggers it is still open. The redesign below is proposed, not built.

Related: `DES DrawParam Lighting Plan.md` (Phases 4 and 5a, which this disables), `DES Model Display.md`.

## Why they were disabled

Commit `5733c94` (Phase 4, per-part lighting) made every render crash on this machine. The crash is a native abort
inside the NVIDIA driver: `nvoglv64.dll` 32.0.16.1060, exception `0xC0000409` (fast fail, code 7), at the same offset
(`0x10cbb2d`) every time. No Smithbox crash log is written, because this is not a managed exception. A fixed driver
offset points to the driver's shader compiler failing on a pipeline, and the commit's only GPU-side change is in
`FlverShader.frag`.

The cause is **not confirmed.** Candidates, in order of likelihood:

1. Whole storage-buffer structs loaded into locals: `lightSetData CurrentLightSet()` returned the 320+ byte entry by
   value, `Hemisphere` and `EnvLight` copied it into a local, and `ModelLight` picked between two structs from
   different storage classes (SSBO and UBO) with `?:`. glslang lowers these to member-by-member copies between
   types with explicit and implicit layouts, a known weak spot for drivers.
2. The scene-param block was 12 bytes larger than its buffer: `lightSetStart` broke the struct's 16-byte rows.
   Padding it (an earlier attempt) did **not** stop the crash.

A first step of the redesign is to find out which candidate it is (see Phase 0).

### What is disabled

| Where | Change |
|---|---|
| `FlverShader.frag` | No `lightSetData` buffer, no `lightSetStart`, no `g_lightSet`. Sun, model lights and hemisphere read only the scene params. `EnvLit()` returns false and `EnvLight()` returns zero. |
| `FlverShader.vert` | Material mask back to `0x7FFFFFFF`; `fsin_flags` holds only the no-shadow bit. |
| `SceneParam.cs` | `LightSetStart` (and its padding) removed. The struct ends on a 16-byte row again. |
| `SceneRenderer` | No `LightSetTable` is allocated. `LightSets` stays null. |
| `SceneRenderPipeline` | No table upload and no `LightSetStart`. |
| `MapEditorScreen` | `UpdatePartLighting` is no longer called; `MapLightSets` is empty. |
| `MeshRenderableProxy` | `LightSet` is still stored but no longer packed into the material ID. |
| `ViewportRenderingTool` | The "Light Parts by Their Own LightID" checkbox is hidden. |

This also turns off **Phase 5a, the DES env cubemap (HemEnv) lighting**. It read the scene's own set from entry 0 of the
same table. DES map materials go back to the hemisphere and sun.

Still in the code, unused: `LightSetTable`/`LightSetData` (the Rendering tool still calls `FromLighting` for its cubemap
readout), `DrawParamLightingBank.UpdatePartLighting`, `CFG.Viewport_Map_DrawParam_Per_Part` and the localization keys.

## What was squeezed in

The feature was fitted around the existing buffers rather than given its own place:

- **Bit packing into the material ID.** All four lanes of `InstanceData` were taken (material, bone start, skin
  reference or lightmap, entity ID). The set index went into bits 24-30 of the material ID, which shrank the material
  mask to 24 bits, and the vertex shader repacked it into bits 8-14 of `fsin_flags`. The skin reference already packs a
  lightmap index under `LightmapFlag` the same way.
- **One binding for everything.** The flver shader declares 18 buffer views at `set 4, binding 0`: materials,
  lightmaps, irradiance volumes, map lights, light sets and the material sub-tables. Each is a slice of one 32 MB
  allocator, indexed by `Start / stride`, with its start passed through the scene params.
- **An append-only scene-param struct.** There are 68 hand-copied `sceneParams` structs across the shaders. New fields
  are appended so the shorter copies stay valid, and every addition has to keep the 16-byte rows by hand.
- **Two paths for every light.** Each read was `g_lightSet != 0 ? set.x : sceneparam.x`. After Phase 5a made the scene's
  own set entry 0, the scene-param half of that was redundant, but it stayed.

## Proposed design

### Principles

- One source of lighting truth per fragment: a light-set index, with the scene's own lighting as set 0. No dual paths.
- Read fields from the buffer, never copy whole structs: `LS.sun.color`, not `lightSetData s = ...`.
- Per-instance data that no longer fits gets its own lane, not spare bits.
- Shared struct definitions live in one include file.

### 1. Lighting buffer: its own binding

Give lighting sets a dedicated binding, for example `set 4, binding 1`, as a plain `std430` array of `LightSetData`:

- The scene's set is always entry 0 and is uploaded every frame from what `SceneRenderPipeline` folds today.
- Per-part sets follow from 1.
- The sun, model lights, hemisphere and env cubemap values move **out** of `SceneParam` into entry 0. That shrinks
  `SceneParam` instead of growing it, and removes the second path in the shader.

The shader then has one accessor per field, all reading `lightsets[g_lightSet].<field>` (or a macro that does), with no
struct-valued functions.

### 2. Per-instance lighting index: a real lane

Grow `InstanceData` from 80 to 96 bytes with a second `uvec4`: `x` the lighting set, `yzw` reserved (the lightmap index
could move here from `SkinReference` later). The material ID goes back to a plain index plus the no-shadow flag.

This touches every shader that reads `idata`, since the instance stride changes. List them with
`grep -rl "uvec4 materialID" src/Smithbox.Data/Assets/Shaders`.

### 3. One `sceneParams` definition

Move `sceneParams` (and the shared light structs) into a `SceneParams.glsl` include, used by all 68 shaders. That removes
the "append so other copies stay valid" rule and makes layout mistakes compile errors in one place. This is the largest
change and can be done separately, before or after 1 and 2.

### Phased plan

| Phase | Work | Check |
|---|---|---|
| 0 | Confirm the crash trigger. Build the old shader (`5733c94`) with only candidate 1 changed: field reads, no struct copies. | Does it still crash on this NVIDIA driver? Decides whether the binding change alone is enough. |
| 1 | Lighting buffer on its own binding, scene set as entry 0, sun/hemisphere/model lights moved out of `SceneParam`. No per-part sets yet. | ER/DS3/DES map lighting looks the same as now. |
| 2 | Re-enable DES env (HemEnv) lighting from entry 0. | Phase 5a's checks in `DES DrawParam Lighting Plan.md`. |
| 3 | Instance lane for the set index; re-enable `UpdatePartLighting` and the checkbox. | DES parts with differing LightIDs lit by their own rows. |
| 4 (optional) | `SceneParams.glsl` include for all shaders. | All viewports render as before. |

## Open decisions

Asked one at a time before building:

1. Run Phase 0 first, or go straight to the new binding?
2. Instance lane (`InstanceData` grows to 96 bytes), or keep a packed index in the material ID with a properly reserved
   bit range?
3. Do the `SceneParams.glsl` include now, or leave it for later?
