# Shader signatures (Phase 2)

Generated 2026-11-08 by `Tools/ShaderSignature`. For each material shader of ER, NR and AC6, the tool records which
texture channels and parameters reach which G-buffer component. It gets this by disassembling the game's own pixel
shaders and walking the DXIL data flow back from every `storeOutput`. What each target means is in
[../GBuffer.md](../GBuffer.md).

## Rerunning

```bash
cd src/Smithbox.Data
../../Tools/ShaderSignature/bin/Release/net10.0/ShaderSignature.exe ER run <cacheDir> "../../Documentation/Claude/Material Fidelity/Signatures" "../../Documentation/Claude/Material Fidelity/Census"
```

`dxc` comes from the Windows SDK (`10.0.26100.0`). Disassembly is cached in `<cacheDir>`, so a rerun takes seconds.
The full per-shader JSON (`{GAME}_Signatures.json`, 4–11 MB) is written next to the TSVs; move it into the cache
dir rather than the docs.

Other modes:
- `list <bundleRegex>`, `extract <outDir> <bundleRegex>`: look inside `.shaderbdle` bundles.
- `one <cacheDir> <shader>`: print one shader's signature.
- `perms <cacheDir> <shader>`: each numbered permutation's slots.
- `params <shader>`: the shader's materials grouped by parameters.
- `checkrecipes <signatures.json>`: the checked-in recipe tables against the signatures.
- `bnd <vfsPath> <outDir>`: extract a plain binder. ER's `gx*.shaderbnd` aren't reachable through the VFS by name.

## Files

| File | What it holds |
|---|---|
| `{GAME}_ChannelMap.tsv` | Role + channel → set of outputs, totalled over shaders, weighted by the census' placed triangles |
| `{GAME}_ShaderTextures.tsv` | Per shader and texture slot: role, channel → outputs, and what the coordinates depend on |
| `{GAME}_Permutations.tsv` | Each numbered permutation's material texture slots and roles |
| `{GAME}_SignatureFailures.txt` | NR only: 190 `_cloth` bundles, which hold only vertex shaders |

## How a shader is read

- **Variant.** `_Gbuf.ppo` first, then `_Gbuf_[A]`, `_Fwd`, `_Fwd_[A]`. ER: 1063 shaders, all but 1 from a G-buffer
  variant. NR: 1120, 14 forward only. AC6: 455.
- **Permutations.** Many shaders come as numbered permutations (`{shader}_{N}_Gbuf.ppo`). ER has 103 such shaders
  with 554 permutations, NR 142 with 581, AC6 181 with 1366; `M[MS][MB][M1]` alone has 165. Each permutation turns on
  a set of texture slots, e.g. AC6 perm 1 = `[0 2 16]`, perm 2 adds slot 3, perm 3 adds 22. The signature is the
  union over all permutations, with each one's slots kept in `_Permutations.tsv`.
- **Which permutation a material uses** is the MATBIN parameter `GXFT_SATStaticSwitch`. The recipe tables are already
  keyed `shader@N` by it, and `MaterialDefinition` looks them up the same way.
- **Leaves.** A G-buffer output's leaves are texture channels (`<resource>.r`), cbuffer fields (`cb:FC_DifMapMulCol`,
  `cb:M_AMSN_snp_0_float_0_0`) and interpolants (`in:TEXCOORD0.z`). A texture whose channels feed another sample's
  coordinates is marked `uv` (flow, distortion, parallax).

## Findings

1. **The G-buffer layout is the same in all three games.**
   - RT0 = normal, RT1.x = metallic, RT1.y = gloss, RT2 = albedo, RT3.x = SSS, RT4 = emissive.
   - The common route covers 85–99% of placed triangles:
     - `AlbedoMap.rgb` → RT2
     - `NormalMap.rg` → RT0
     - `NormalMap.b` → RT1.y
     - `MetallicMap.r` → RT1.x
2. **Smithbox's F0 is 0 for these games** (see GBuffer.md). The game uses `mix(0.04, albedo, metallic)`, and diffuse
   is `albedo·(1 - metallic)`. This affects every surface, not only the 100 shaders with a MetallicMap.
3. **Masks are layer weights.** In the channel map, Mask1Map and Mask3Map channels feed every G-buffer output
   together, which is what a blend weight does. In AC6 that's ~60–80% of placed triangles, and in ER ~28% (Mask1)
   and ~17% (Mask3). AC6's `[MS][MB][M0/M1]` is a 4-layer blend: albedo, normal and metallic per layer, layers 2+ on
   the second UV set (`TEXCOORD0.zw`), one normal scrolling by `FC_GlobalTime`. AC6 has no shader table entry for it,
   so Smithbox draws only its first layer. That's AC6's single largest gap.
4. **The checked-in recipe tables agree with the shaders.** Once `@N` keys are resolved to their permutation, every
   slot a recipe names is sampled by that shader in 1258 / 1309 (ER), 1319 / 1405 (NR) and 918 / 996 (AC6) entries.
   The rest:
   - **Water (3–6 per game):** expected. The water recipes come from the forward shader, and the signature reads `_Gbuf`.
   - **`TextureInputs` (47–75):** mostly character shaders whose inputs were read from another permutation, e.g.
     `C[c2200]_Body`.
   - **Fur `_cloth` (NR 15):** these bundles have no pixel shader.

   The lost generators are therefore not needed to *trust* the tables. They would still be needed to *extend* them,
   for example to AC6's `[MS]` blends. Rebuilding the blend formulas (constants, curve shapes) is beyond data-flow
   tracing, and is the Phase 5 oracle's job.
5. **Signatures say which inputs reach which outputs, not the maths.** "Gloss, not roughness" and
   "`F0 = mix(0.04, …)`" came from reading the forward shader by hand. Phase 3 should treat each signature as "these
   inputs must be bound, to these meanings". It doesn't give the curves.
