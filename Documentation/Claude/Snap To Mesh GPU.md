# Snap to Mesh: GPU depth pass

The Map Editor's Snap to Mesh (Tools > Transform) moves the selection along a world axis until its bounding box
meets the nearest mesh. The first version ray cast on the CPU against FLVER triangles kept in memory and decoded
collision; it was too heavy to preview live. It now finds the surface on the GPU. FLVER meshes only (the tool is
mainly for assets), so collision is no longer a target.

## Pass (`Renderer/Scene/SnapDepthPass.cs`)

- One per `SceneRenderPipeline`, built like `RainOcclusion`: an offscreen `RenderQueue` with its own `SceneParam`
  buffer, an orthographic view looking along the snap direction, reversed depth (cleared to 0, 1 at the near plane).
- The view covers the union footprint of the request's groups. The near plane goes through the nearest group's box
  centre, the far plane reaches each group's leading face plus Max Distance.
- Drawn with `MeshRenderables.cDepthPipelines`: the meshes' shadow-map pipelines, kept whether or not the mesh
  casts the sun's shadow (`cShadowPipelines` is null for parts with IsShadowSrc off). Only opaque FlverShader meshes
  have one. `SubmitDepthCasters` keeps to the scene's draw filter and display groups, and leaves out renderables
  whose selectable is in the request's excluded set (the selection and its children).
- Timing, as RainOcclusion: a queue draws what was put in it a frame later (or the same frame), so a request
  submits for three frames and clears and draws on the middle two. On the fourth, before any framebuffer is bound,
  `SnapDepth.comp` reduces each group's texel rectangle to the largest depth no nearer than the group's centre plane,
  with `atomicMax` on the float bits, and the result buffer is read back asynchronously. Stray draws go to a 1 x 1
  sink.
- Texel rectangles are found on the CPU through the same lookup matrix (view x projection, Y flipped when clip
  space Y isn't inverted), so they match the drawn texels whatever the Y convention.

## Tool (`SnapToMeshAction`)

- While the tool section is open it requests a snap for the selection every time the pass is free (latest wins),
  so the arrow follows a drag a few frames behind. Results hold the surface plane along the direction for each
  group, so the arrow is drawn from the current face to that plane even while the result is a little old.
- Applying requests a snap for the current state and moves the selection when a result for exactly that state
  arrives (it works from the menus with the tool closed too).

## Limits

- "Snap Each Object Separately" shares one view: the depth texels are 512 across the union footprint, so a widely
  spread selection is coarser, and a group sees surfaces only below its own centre plane, so a surface between the
  nearest group's centre and its own (a bridge over it) hides what lies beneath in those texels.
- At most `SnapDepthPass.MaxGroups` groups per request.
- Transparent meshes and non-FLVER geometry (collision, navmesh) aren't targets.
