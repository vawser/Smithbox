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

## Decisions (2026-11-08)

1. **No Phase 0.** Go straight to the new design. It avoids every candidate trigger: no struct copies from buffers, its
   own binding, aligned params. If it crashes anyway, bisect then.
2. **A new instance lane.** `InstanceData` grows to 96 bytes with a second `uvec4`, and the material ID goes back to a
   plain index plus the no-shadow flag.
3. **The `SceneParams.glsl` include comes first**, as Phase 1, so the later `SceneParam` changes happen in one place.

### Phase 1 findings

- **No shader uses `#include` today.** Shaders load only as prebuilt `.spv` (`ShaderHelper.CreateFromSpirv`), so
  includes only matter at build time.
- **Two compilers:**
  - `glslangValidator`, used for the shaders outside the FlverShader set, supports `#include` through
    `#extension GL_GOOGLE_include_directive : require`.
  - `Veldrid.SPIRV.VariantCompiler` passes source text to libveldrid-spirv, which has no include callback. It needs a
    textual `#include "..."` expansion step in `CompilationSet.cs` before compiling (the project is in this repo).
- **The 68 copies are not all alike.** They run from 5 to 103 lines: 17 have the full 103 lines, and most of the others
  are presumably truncated prefixes. Each one must be checked to be a true prefix of the full struct before it is
  replaced. The very short ones (5-8 lines) may be different structs that only share the name, bound to other buffers.
  Any copy that isn't a prefix is either renamed or left alone, and only true prefixes switch to the include.
- **Bound buffer size:** with the include, every shader declares the full struct. Every buffer bound as scene params
  must be `sizeof(SceneParam)` (true today for the pipeline, overlay, shadow cascade and rain buffers). The audit must
  confirm there are no others.

### Phase 1 as built (2026-11-08; checked in the app by the user: all viewports render as before)

Correction to the findings above: there are **36** source copies, not 68. The 68 counted the compiled `.spv` files,
which contain the struct name.

- **`Shaders/SceneParams.glsl`:** `sceneLight`, `mapDirectionalLight` and `sceneParams`, generated from
  FlverShader.frag's definitions, with an include guard and a default `MAX_SCENE_LIGHTS 16`.
- **`Tools/SceneParamsAudit`:**
  - The default run checks each copy by std140 layout. The rule: every member a shader reads (`sceneparam.x` outside
    the struct) must be a canonical member at the same offset with the same type. Unread members are padding.
    Results go to `report.md`.
  - `--apply` writes the include and rewrites the compatible copies. It removes their struct and helper-struct blocks,
    adds `#include` and the `GL_GOOGLE_include_directive` extension, and renames reads to the canonical names.
  - `--spirv` compares each `.spv`'s `sceneParams` member offsets against git HEAD, writing `spirv-report.md`.
- **Results:**
  - 33 copies were switched automatically. Four of them had renames: `SimpleFlver_Brightness`/`_Saturation` →
    `simpleFlverBrightness`/`Saturation` in the three SimpleFlver vertex shaders, and `OutlineColor` → `outlineColor`
    in SimpleFlver_Outline.frag.
  - **FlverShader.vert** was switched by hand. Its copy had an `#ifdef MATERIAL_DEFORM` inside the struct, there only
    to reach `materialTime`.
  - **FlverBB and FlverDS3 (.vert/.frag) are dead.** Nothing in the C# code or the variant sets references them. Their
    `.frag` copies leave out `curserPosition`, so they would read `envmap` and the light multipliers 16 bytes early.
    The `.frag` files were left as they are; their `.vert` files were switched, since those never read the bad members.
- **The SPIR-V check:** every member of every old copy starts at the same offset in the new code, except the renames
  above and `envmap` in FlverBB.vert, FlverDS3.vert and SimpleFlver_Outline.vert, which the audit found those shaders
  never read.
- **Bindings:** every scene-param binding is a full `sizeof(SceneParam)` buffer: the pipeline, overlay, rain and
  shadow cascade buffers, and `VolumetricFog.comp`'s compute set (`pipeline.SceneParamBuffer`).
- **Variant compiler:** `CompilationSet.ExpandIncludes` replaces `#include "file"` lines (relative to the including
  file) with the file's text, followed by a `#line` so errors keep the shader's own line numbers. It also comments out
  the include extension line. The rebuilt exe is in `bin/Release/net8.0`; the old `net8.0-windows` build can't expand
  includes.
- **Rule from now on:** a field added to `SceneParam.cs` goes in `SceneParams.glsl` once, keeping the 16-byte rows.
  Shaders read it without any copying.

### Phase 2 as built (2026-11-08; checked in the app by the user: lighting looks the same)

The plan said `set 4, binding 1`; it went to **set 0, binding 1** instead. The lighting is per view, not global:
- Model Light1 turns with the camera's yaw.
- The Model Editor's viewport uses the viewport sun, while the Map Editor uses the GPARAMs.

Set 0 is already each pipeline's own set, and every scene batch binds it (`SceneRenderPipeline.BindResources`), so
each view gets its own buffer. A single table in the shared material buffer, as the old `LightSetTable` was, can't
do that.

- **`SceneLightingData`** (`Renderer/Structs/SceneLighting.cs`, 304 bytes): the sun (direction, colour, highlights),
  the hemisphere, the four model lights and the model hemisphere. These are exactly the fields a per-part set
  overrides. Its GLSL twin is `sceneLighting` in `Shaders/SceneLighting.glsl` (std430, readonly), read as
  `LIGHTING.<field>`, where `LIGHTING` is `lightingSets[LIGHTING_SET_INDEX]` and the index is `0u` for now.
- **`SceneRenderPipeline.SceneLightingBuffer`:** 128 entries (38,912 bytes), zeroed at creation. Only entry 0, the
  view's own lighting, is uploaded each frame: the map's sun, hemisphere and model lights when the GPARAMs light the
  scene, otherwise the viewport sun (`ViewportSunDirection`/`ViewportSunColor`, now pipeline fields instead of
  `SceneParam` fields).
- **The set-0 layout** (`StaticResourceCache.SceneParamLayoutDescription`) gained the storage buffer. All four places
  that create set-0 resource sets pass it: the pipeline's main and overlay sets, `ShadowCascades` and `RainOcclusion`.
  The fog passes bind the pipeline's own set.
- **Removed from `SceneParam`/`sceneParams`** (now 1968 bytes, down from 2272): `LightDirection`, `SunColor`,
  `SunSpecColor`, `HemiUp`, `HemiDown`, `ModelLight0-3`, `ModelHemiUp`, `ModelHemiDown`. Kept, as scene-wide:
  `MapLightingFlags`, `EnvMapScale`, `EnvMapSaturate`.
- **Shader reads:**
  - `FlverShader.frag`'s `SunTravel`, `SunColor`, `SunSpecColor` and `Hemisphere` read `LIGHTING`.
  - `ModelLight(i)`, which returned a struct, is now `ModelLightDirection`/`Color`/`SpecColor(i)`, and its loop reads
    the fields.
  - `FogVolume.frag` reads `LIGHTING.sun.direction`.
  - No shader copies a lighting struct out of the buffer.
- **The checks** (`SceneParamsAudit --csharp`):
  - `SceneParam`/`sceneParams` and `SceneLightingData`/`sceneLighting` agree. Every GLSL member starts where a C#
    field does, and the sizes match.
  - The compiled `.spv` offsets match the computed layouts in all 62 shaders with `sceneParams` and all 17 with
    `sceneLighting`.
- Between phases the user removed `FlverBB.*` and `FlverDS3.*`, the dead shaders.

### Phase 3 as built (2026-11-08; checked in the app by the user: env lighting is back)

- **`SceneLightingData` grows to 368 bytes** with four rows taken from Phase 5a's `LightSetData`: `EnvDiffuseScale`,
  `EnvSpecularScale`, then the five cube pool indices (`EnvDiffuseTexture`, `EnvSpecular0-3`) and three pads. In
  GLSL they are `envDiffuseScale`, `envSpecularScale`, `envTextures` and `envTextures2`.
- **`SceneLightingData.SetEnv(env, exposure)`** carries over `LightSetTable.FromLighting`'s env logic: on only when
  the lighting has env cubemaps (`GparamEnvLighting.Enabled`, DES only) and `Viewport_Map_DrawParam_Env_Lighting` is
  on. The textures are looked up each frame (`CubeTexture`), so they appear once they've loaded.
- **The pipeline** calls it for entry 0 with the map's `Env` and exposure. The viewport-sun lighting sets it off.
  When it's off, `envDiffuseScale.w` is 0, so no material is env lit.
- **`FlverShader.frag`:**
  - `EnvLit()` is back: a map material with `MATERIAL_ENV_LIT` and `LIGHTING.envDiffuseScale.w > 0.5`.
  - `EnvLight()` reads `LIGHTING.envTextures*` and the scales field by field, where the old version copied
    `lightSetData set = ...`.
- **The Rendering tool's** "cubemaps loaded" count uses `SetEnv`, so nothing reads `LightSetTable` any more.
  It can be archived in Phase 4.
- **Checks:** `SceneParamsAudit --csharp` agrees at 368 bytes, and the compiled offsets match in all 17
  `sceneLighting` shaders.

### Phase 4 as built (2026-11-08; not yet checked in the app)

- **`Shaders/InstanceData.glsl`:** the one `instanceData` struct and `WorldBuffer` declaration (set 1, binding 0),
  in place of 16 identical copies (`SceneParamsAudit --instance-apply`). Its new lane is `uvec4 lighting`, where
  `x` is the lighting set and `yzw` are reserved.
- **`InstanceData.cs`** is 96 bytes, up from 80: `LightingSet` plus three pads. Every C# site sizes and aligns by
  `sizeof(InstanceData)`, including `FirstInstance` and the world/grass allocators. No GPU code writes instances.
  The 64 MB world budget now holds about 699k instances (the densest maps take about 40k).
- **`MeshRenderableProxy.LightSet`:** a set of 128 or more falls back to 0, and the set is written to
  `InstanceData.LightingSet` for flver-shader meshes. The material ID is a plain index plus the no-shadow flag again.
- **`FlverShader.vert`** passes `idata[i].lighting.x` as `fsin_lightingSet`, flat, at location 17. The
  `FlverShader.frag` side:
  - It sets `g_lightingSet = MapLighting() ? fsin_lightingSet : 0u` at the start of `main`, and
    `#define LIGHTING_SET_INDEX g_lightingSet` makes every `LIGHTING.<field>` read the instance's set.
  - The sun, hemisphere, model lights and env cubemaps all follow the set without any other change.
- **`SceneRenderPipeline.FoldLighting(lighting, exposure, cameraYaw, withModels)`:** one fold for entry 0 and for the
  per-part sets, which `LightSetTable` used to fold separately and without Light1's camera yaw.
  - The per-part sets (`SceneRenderer.MapLightSets`) are folded at the scene's exposure into
    `PartLighting[0..n)`.
  - They are uploaded each frame after entry 0, in the same upload task.
  - With no map lighting there are no part sets.
- **Map Editor:** `UpdatePartLighting` is called again, capped at `SceneLightingData.MaxEntries - 1` (127) sets, and
  the "Light Parts by Their Own LightID" checkbox is back.
- **Archived:** `Renderer/Scene/LightSetTable.cs` → `.archive/LightSetTable.cs` (with `git mv`, so the rename is
  staged), along with `SceneRenderer.LightSets`. Nothing referenced either any more.
- **Checks** (`SceneParamsAudit --csharp`):
  - `InstanceData`/`instanceData` agree at 96 bytes.
  - The compiled offsets match in all 31 shaders with `instanceData`, which are all 31 that bind `WorldBuffer`.
  - `sceneParams` (62 shaders) and `sceneLighting` (17) still match.

### Phased plan

| Phase | Work | Check |
|---|---|---|
| 1 | `SceneParams.glsl` include: include expansion in the variant compiler; a prefix audit of all 68 copies; switch the true prefixes over; recompile everything. | Every viewport (map, model, TAE, gparam, collision, nav) renders as before, on ER, DS3 and DES. |
| 2 | Lighting buffer on its own binding (for example `set 4, binding 1`, `std430`), with the scene's set as entry 0. Sun, hemisphere and model lights move out of `SceneParam`; the shader reads fields, never whole structs. | ER/DS3/DES map lighting looks the same as now. |
| 3 | Re-enable DES env (HemEnv) lighting from entry 0. | Phase 5a's checks in `DES DrawParam Lighting Plan.md`. |
| 4 | `InstanceData` grows to 96 bytes with the set index in the new lane (every `idata` reader updated); re-enable `UpdatePartLighting` and the checkbox. | DES parts with differing LightIDs lit by their own rows; non-DES unchanged. |
