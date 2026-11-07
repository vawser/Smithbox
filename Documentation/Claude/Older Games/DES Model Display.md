# DES model display: what is wrong and how to fix it

Written 2026-11-07. Nothing here has been built yet.

## How this was checked

`Tools/DesModelSurvey` reads every Demon's Souls FLVER0 from the unpacked PS3 files (`chr/cXXXX/cXXXX.chrbnd`,
`obj/*.objbnd`, `parts/*.partsbnd`, `map/mAA_BB_CC_DD/*.flver`). For each one it checks the things the renderer's FLVER0
path (`FlverResource.LoadInternalDeS`, `ProcessMaterial`, `ProcessMesh(FLVER0.Mesh)`) does differently from what the file
asks for. The full report is `Tools/DesModelSurvey/survey.md`.

```
dotnet Tools/DesModelSurvey/bin/Release/net10.0/DesModelSurvey.dll "<DES USRDIR>" survey.md
```

Totals: 3519 models (132 chr, 1068 obj, 680 parts, 1639 map pieces). **Every model reads without an error**, so nothing is
missing outright. The problems are in placement, culling and textures.

## What is wrong

### 1. Rigid meshes put in the wrong place (worst)

DES has no NormalW binding. Its normals are `LayoutType.Color`, so `SetNormalWBoneTransform` (which checks for
`UByte4/Byte4E/Byte4`) never fires, and the NormalW byte isn't a node index (it is past the node count in 8352 of 8498
map meshes). A rigid mesh (`Mesh.Dynamic == 0`) binds each vertex to node `mesh.BoneIndices[vertex.BoneIndices[0]]`
instead.

The renderer handles this in `ProcessMesh(FLVER0.Mesh)` through `HasIndexNoWeightTransform`, but in three wrong ways:

| Bug | Where | Effect | Models |
|---|---|---|---|
| a. **Local, not world, node transform.** `BoneTransforms` holds `Bones[i].ComputeLocalTransform()`; the world version is commented out | `LoadInternalDeS` | Pieces under a nested node lose their parent offsets and end up near the origin or pointing the wrong way | chr 1, obj 26, map 4 (e.g. `o1100`, `o4800`, `o6110`, `c7050`, `m02_02_00_00/m4010b2`) |
| b. **Decided per material, not per mesh.** The flag comes from the material layout (indices but no weights), so bind-pose meshes (`Dynamic == 1`) that leave out weights get moved too | `ProcessMaterial` | Moved by the bone transform a second time | chr 6, obj 1, parts 20: **every player head and hair part** (`hd_f_0000`, `hd_m_0000`, `hr_f_0008`…), `c1020`, `c2010`, `c4000` |
| c. **Rigid meshes with weights left alone.** A `Dynamic == 0` mesh whose layout also has weights skips the transform | `ProcessMaterial` | Drawn at the origin of the node instead of where it is attached | chr 15, obj 7, map 3 (`c0200`, `c0300`, `c1200`, `c2030`, `o2201`, `m06_00_00_00/m0015b0`) |

On top of that, the transform only goes to position and normal. Tangents and bitangents keep their unrotated direction,
so normal mapping is wrong on every rotated piece even when the position is right.

25 models (chr 15, obj 8, parts 2) use the same material for both bind-pose and rigid meshes, so a per-material switch
can't be right for them. It has to be decided per mesh.

### 2. Double sided meshes are culled

`ProcessMesh(FLVER0.Mesh)` sets `BackfaceCulling = true` for every face set. The FLVER2 paths read the file's flag
(`faceset.CullBackfaces`). FLVER0 has `Mesh.CullBackfaces`, and it is false on 1391 meshes in 737 models:
chr 76, obj 211, parts 190, map 260. That covers cloth, capes, hair cards, foliage, banners and thin walls, which show
holes from behind. Sample targets: `c2000`, `c2010`, `c0200`.

### 3. Textures that never load

Textures are found by name in the pool, so a texture shows only if some loaded container has it. 2216 of 35,341
texture slots find nothing in the model's own container (chr 72, obj 1237, parts 598, map 309). Where they really are:

| Where the texture is | Count | Fix |
|---|---|---|
| The container the FLVER's own texture path names (`…\obj\o0000\tex\…`, `…\chr\c2000\tex\…`, `…\map\m02\tex\…`, `…\parts\Weapon\WP_A_1509\tex\…`, and `…\parts\tex\…` for the common TPFs) | 1492 | Load that container as well |
| The map's own TPF, `map/mAA_BB_CC_DD/mAA_BB_CC_DD.tpf`. `GetMapTextureVirtualPaths` only loads the area TPFs `map/mAA/mAA_NNNN.tpf` | 110 (m03 mostly) | Add it to the map texture list |
| Somewhere else in the game, but the path names the wrong container (e.g. `c6050` asks for `c1020_dragondemon_face` under `chr\c6050`) | 463 | Needs a name → container index |
| Nowhere in the shipped files (dev paths like `C:\Demon_works\…`, `小物tex` chairs, missing debug textures) | 151 | Nothing to do |

The common TPFs are also never loaded for DES. `parts/common_body.tpf` and `parts/common_ghost.tpf` hold the shared
body and ghost textures that every player part points at (`BD_F_body`, `Ghost_512x512_1`…). The AAT and SYSTEX blocks in
`MapResourceHandler.SetupTextureLoadLists` and `ModelUniverse.SetupTextureLoadList` skip DES. `PathBuilder`'s `aat`
branch would also look for `parts/common_body.tpf.dcx`, but DES has `.tpf`.

In the Model Editor, the parts prefix check (`am`, `lg`, `bd`, `hd`, `wp`) leaves out DES's `hr` (22 hair parts),
`as` (3) and `fc` (7), so their textures aren't loaded.

### 4. Smaller things

- **Lightmaps are ignored.** `g_Lightmap` is on 7394 texture slots (most map pieces). Map pieces have `UV0:Short4`, which
  SoulsFormats unpacks into two UV sets, the second one for the lightmap. The renderer only treats DS3's `g_DOLTexture`
  as a lightmap. The maps aren't wrong without it, just flatter than in the game.
- **MTDs missing from `mtd/mtd.mtdbnd`**: about 40 models, all ghosts, debug or test (`Cs_White_Ghost`, `DS_map[Dif]`,
  `DS_parts_*[…_Skin]`, `A05[D][ML]`). They fall back to default shading. Low priority.
- `PathBuilder` chr model has a second `ProjectType.DES` branch (`chr/{id}/{id}.chrbnd.dcx`) that can never be reached
  after the first one. It is harmless, but should be removed when this code is touched.
- The 0xFFFF check in `ProcessMesh(FLVER0.Mesh)` (`indices[i] == 0xFFFF && indices[i] > mesh.Vertices.Count`) does
  nothing after `Triangulate`. The survey found no out-of-range indices and no mesh over 65535 vertices.

## Plan

Phases are in order of how much they fix. Each one can be checked in the app on its own.

### Phase 1: rigid mesh placement

**Built 2026-11-07, not yet checked in the app.** `FlverResource.PlaceRigidVertices` and `GetRigidVertexNode` do the
placement; `ModelRig.BuildRigidFlver0Mesh` does the animation side. The material flag `HasIndexNoWeightTransform` is
gone. A rigid vertex whose bone index is out of range stays where it is (and gets no skin weight), as before.

In `FlverResource`:

1. `LoadInternalDeS`: fill `BoneTransforms` with **world** matrices (`Utils.GetBoneWorldMatrix(Bones[i], Bones)`,
   which the static bone buffer already uses a few lines down).
2. Drop the material-level `HasIndexNoWeightTransform` choice and decide per mesh in `ProcessMesh(FLVER0.Mesh)`:
   - `mesh.Dynamic == 1` (bind pose): no CPU transform, whatever the layout says. Fixes 1b.
   - `mesh.Dynamic == 0` (rigid): transform each vertex by the world matrix of
     `mesh.BoneIndices[vert.BoneIndices[0]]`, with or without weights in the layout. Fixes 1a and 1c. If the index is out of
     range, fall back to `mesh.NodeIndex`, then to identity.
3. Transform the tangent and bitangent along with the normal (`Vector3.TransformNormal`, keep the tangent's W sign).
4. Stop setting `SetNormalWBoneTransform` from the FLVER0 layout, since DES NormalW isn't a node index. Today it never
   fires for DES data anyway, so this only removes a trap.

Animation (`ModelRig.FromFlver(FLVER0)`) treats a rigid mesh as `RigidMesh` following `mesh.NodeIndex`, which is wrong
for meshes whose vertices bind to several nodes. Once the vertices are baked into the reference pose as above, the
correct rig is **skinned, one node per vertex at weight 1**: `NodeWorld * NodeInverseReferenceWorld` then moves them
right, using the same skin path as weighted meshes. So `BuildMesh` gets a DES rigid case that writes `SkinVertex` with
`mesh.BoneIndices[v.BoneIndices[0]]` and weight 1.

**Check**: `o1100`, `o4800`, `o6110` (objects whose parts sit under nested nodes) and the `m02_02_00_00` map
(`m4010b2`, `m4020b2`) should have every piece attached. The player head `hd_m_0000` and hair `hr_f_0008` should sit on
the head in the Model Editor, not float above or below it. `c0200` and `c1200` should no longer have pieces at their feet.

### Phase 2: per-mesh backface culling

**Built 2026-11-07, not yet checked in the app.**

`ProcessMesh(FLVER0.Mesh)`: `BackfaceCulling = mesh.CullBackfaces`. One line. `FlverSubmeshProvider.CullMode` reads it,
and a material whose definition says `CullNone` still overrides it.

**Check**: `c2000` (cloth), `c2010`, and the banners and foliage in `m01_00_00_00` from behind.

### Phase 3: texture coverage

**Built 2026-11-07, not yet checked in the app.** Where each part landed:

- Map's own TPF: `TextureLocator.GetMapTextureVirtualPaths` adds `map/tex/{mapId}/own` when the file exists;
  `PathBuilder` resolves it to `map/{mapId}/{mapId}.tpf`.
- Common TPFs: `TextureLocator.GetDesCommonTextureVirtualPaths` (common_body, common_ghost), loaded by the DES branch of
  the AAT blocks in `MapResourceHandler` and `ModelUniverse`. `PathBuilder`'s `aat` branch uses `.tpf` for DES.
  `facegen/facegen.tpf` was left out, as no model path points at it.
- Cross-container references: `TextureLocator.GetDesReferencedTextureContainers` applies the path rules (cached per
  project and container key). `FlverResource.RequestDesTextureContainers` loads each container into the model's pool,
  textures only, hidden from the load window, once per model, skipping the model's own container. The pool joins or
  skips containers that are loading or loaded already. These loads ignore the per-category texture toggles of the
  Map Editor.
- Parts prefixes: `ModelUniverse.IsPartModelName`, used for both the part model and part texture lists. DES also gets
  `hr`, `as` and `fc`, so its hair, accessory and face parts now load as models too, not only their textures.

1. **Map's own TPF**: in `TextureLocator.GetMapTextureVirtualPaths` (DES), also add `map/tex/{mapId}/own`, and make
   `PathBuilder` resolve it to `map/{mapId}/{mapId}.tpf`. Check `m03_00_00_00` (`m0000b0`, `m0000b1`).
2. **Common TPFs**: give DES `parts/common_body.tpf` and `parts/common_ghost.tpf` (and `facegen/facegen.tpf`) in the AAT
   path, and enable the AAT block for DES in `MapResourceHandler` and `ModelUniverse`. Check any player part
   (`am_f_0000`, `bd_f_0000`) and the ghost parts (`am_f_5900`).
3. **Cross-container references**: after a DES FLVER's materials are read, collect the containers its texture paths name
   and queue their TPFs into the same pool. The rules the survey uses are `chr\cXXXX\tex` → `chr/cXXXX/cXXXX.tpf`,
   `obj\oXXXX\tex` → `obj/oXXXX.objbnd`, `map\mAA…\tex` → the area TPFs of `mAA`, `parts\<Kind>\<ID>\tex` →
   `parts/<id>.partsbnd`, and `parts\tex` → the common TPFs. This covers 1492 of the 2216 misses. No name → container index
   is built for the remaining 463 (see the decision below).
4. **Model Editor parts prefixes**: add `hr`, `as`, `fc` (DES), so hair, accessories and faces load their textures.

**Check**: `o0050` (uses `c2000`'s body texture), `o0141` (uses `m02`'s barrel texture), `c2011` (holds weapon
`WP_A_1509`) should be textured in the Model Editor with no map loaded.

### Phase 4 (optional): DES lightmaps

**Built 2026-11-07, not yet checked in the app.** It went differently from the outline below. DS3's DOL path (instance
entries from `DolLightmapBank`) is per part and doesn't fit, so DES uses the material table's reserved lightmap slot
instead:

- `DesModelSurvey lightmap` mode (`Tools/DesModelSurvey/lightmap.md`). Every lightmapped DES MTD gives `g_Lightmap`
  UV 3 (diffuse UV 1, diffuse_2 UV 2), and in the meshes that is the **last** UV set: set 1 of the 2-UV `[L]` meshes
  (about 94% of lightmapped meshes), set 2 of the 3-UV `[ML]` layered ones. The lightmaps are DXT1 (TPF format 0),
  `mAA_lit_*`, and 1020 of 802 names are found in the area TPFs (some are in more than one).
- `FlverResource.LookupDesLightmap` loads `g_Lightmap` into the new `TextureType.LightmapTextureResource`, which
  `UpdateMaterial` writes to `Material.lightmapTex` (row 4 z, unused until now), and sets `UsesLightmap`, so the blend
  variant with `LayoutUV2` is chosen. Layered materials are skipped: the variant carries two UV sets and they need three.
- `FlverShader.frag`, `IndirectLight`: for `GAME_DES` with a lightmap texture, the ambient term is the lightmap squared
  (as DS3's DOL 1 is linearised) times `DES_LIGHTMAP_SCALE` (1.0). The game's own scale isn't traced. All variants
  were recompiled.
- The viewport's Lightmaps toggle shows for DES too (its own tooltip). The Intensity slider doesn't reach DES: it only
  feeds `DolLightmapBank`'s table.

Not done: the `[ML]` meshes' lightmaps, and matching the brightness against the game.

The original outline:

Treat `g_Lightmap` on a DES material like DS3's DOL lightmap: set `UsesLightmap`, choose `LayoutUV2` so the second UV set
reaches the shader, and look up the lightmap by its texture name, like any other texture of the material. The
survey counts `g_Lightmap` slots with the rest, so most of them already resolve from the area TPFs. This shares the existing `Viewport_Map_Lightmaps` toggle and blend variant, so no new shader work
should be needed. It has to be checked against how DS3's lightmap is combined (multiplied into the diffuse).

**Check**: Boletarian Palace `m02_00_00_00` with lightmaps toggled on and off.

## Decision: textures whose path names the wrong container

Decided 2026-11-07: **path rules only**, no name index.

463 texture slots exist in the game but not in the container their path names. Covering them would need an index of
which container holds each texture name, built by reading every TPF header once (chr, obj, parts, map areas) and cached
in the project. That startup cost isn't worth it for now. Those slots stay untextured in the Model Editor. In the Map
Editor they still show when the right container happens to be loaded for another model. If they turn out to matter, the
index can be added later on top of the path rules without changing them.

## Duplicate X / X.dcx files (2026-11-07)

3588 DES files exist both as `X` and `X.dcx`. `DesModelSurvey dcxpairs` decompressed every pair: 3585 are byte for byte
the same, and 3 objbnds (`o0052`, `o2195`, `o4219`) are 64 bytes longer uncompressed. 3365 of the `.dcx` are
`DCX_EDGE`, 223 `DCP_DFLT`. `EBOOT.BIN` is encrypted, so which copy the game reads can't be read from it; the
loadlists name files without `.dcx`.

Decision: list only the `.dcx`, and mirror saves.

- `ProjectFileLocator.RemoveDesUncompressedTwins` drops an entry whenever `Path + ".dcx"` is also in the dictionary,
  before the per-editor lists are compiled. Files that are only uncompressed stay.
- `VirtualFileSystem.FileWritten` (Andre.IO) is raised by `WriteFile`, and by `RealVirtualFileSystem.Copy`/`Move`, which
  `WriteWithBackup` uses to put `.temp` files in place. `ProjectVFS.MirrorDesDcxWrite` listens on the project file
  system for DES: a written relative `X.dcx` is decompressed and written to `X` when the game or project has `X`.
- `PathBuilder.DesPreferDcx`: map pieces and chrbnds now read the `.dcx` when it exists (objbnd, partsbnd and area
  TPFs already did).

Not covered: a write of only the uncompressed `X` (none of the editors list it any more) isn't mirrored to `X.dcx`.
