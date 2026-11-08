# DES Model Save

FLVER0 write support and Model Editor saving for Demon's Souls projects (2026-11-08).

## FLVER0 writer fixes (SoulsFormats)

`FLVER0.Write` already existed but no DES model came back byte-identical. Checked with `Tools/Flver0RoundTrip`
(reads every DES FLVER0 from chr, obj, parts and map, writes it, compares bytes and re-reads it):

| Stage | Exact of 3519 |
|---|---|
| Writer before the fixes | 75 |
| After the fixes | 3272 |

Fixes:
- **Header face counts (0x40, 0x44).** 0x40 = strip faces without degenerate or restart triangles. 0x44 = `indices - 2` per mesh before 0x15, strip faces including degenerates from 0x15. The old writer included degenerates for 0x12–0x14 and wrote the raw index count.
- **Header 0x4A / 0x4B.** Were written as `bool` and `0`; now written as the bytes that were read.
- **Vertex index size.** The header byte was computed but the indices were written with `Header.VertexIndexSize`; one value is now used for both. The mesh's index byte length was always `Count * 2`.
- **Versions ≤ 0x10** (all 79 DES models of 0xE, 0xF, 0x10). Materials have no layout header (a single layout at the layouts offset), and meshes have no vertex buffer list. The writer always wrote both.
- **Texture param names.** Older models have none: the read keeps that as `null` with a zero offset, and the writer now writes the zero offset back (it used to write an empty string).
- **Float UVs** (`Vertex.Write`, which FLVER2 shares). Float2/3/4 UVs were multiplied by the UV factor on write but never divided on read. This affected 48 DES models, and FLVER2 models with float UVs too.
- **Clone constructors.** `Mesh(Mesh)` indexed into empty lists and threw. `Texture(Texture)` copied the wrong way round.

The other 247 models differ **only** in the 0x40 / 0x44 face counts, and only by a few faces (for example o3120: 72 against 69).
No rule tried matched them: equal indices, equal positions, or zero area. They are probably the exporter's counts from before stripping.
The game likely only uses these as statistics. That is **unverified**.

## Model Editor save

The editors hold DES models as a FLVER2 (`Flver0Converter.ToFlver2`). Saving converts the model back with `Flver0Converter.ToFlver0`:
- What FLVER2 can't hold is kept in a `ConditionalWeakTable` keyed by the FLVER2: the FLVER0 header (version, index size, unknowns), the DCX compression, and each material's FLVER2 layout indices.
- A mesh's layout becomes an index into its material's layout list. If a mesh's layout isn't in that list (for example, after its material changes), it is added.
- If the material or buffer-layout count changed, the original per-material lists are dropped and rebuilt from what the meshes use.
- A model with no FLVER0 origin gets the 0x15 DES defaults.
- Saving throws if a mesh uses more than 28 bones, or has no vertex buffer. FLVER0 has one face set per mesh: the first `FSFlags.None` set is written.

`ModelBank`:
- Writes through `Flver0Converter.WriteEditorFlver`.
- Matches the model file case-insensitively.
- Writes back to the single model of a binder whose model isn't named after the binder (`am_f_0000_l`), as the load does.

`ModelEditorScreen` no longer refuses DES saves. The Time Act Editor still does.

Tool check: `Flver0RoundTrip <USRDIR> out.md editor` sends every model through FLVER2 and back. It gives the same 3272 exact results as writing the FLVER0 directly.
