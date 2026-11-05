# GPARAM byte-perfect round trip

Tool: `Tools/GparamRoundTrip` (run from `src/Smithbox.Data`):

```
GparamRoundTrip.exe <outDir> [GAME | GAME=path ...]
```

It finds files the way `ProjectFileLocator` does (`/param/**.gparam`, BB `/param/**.gparambnd`,
DS2/DS2S `/filter/**.fltparam`) through the same VFS as Smithbox (BHD archives + loose files).

Each file gets two checks:

- **raw**: decompressed GPARAM bytes == `GPARAM.Read(bytes).Write(DCX.Type.None)`. This tests the format code only.
- **save**: what `GparamBank` writes (`GPARAM.Read(file).Write()` with the compression it read, or the BND4
  rewrite for `.gparambnd`) == the original file. When the bytes differ, the tool also decompresses both and compares them.

## Results (after fix)

| Game  | Files | GPARAMs | Version | Raw        | Save (exact) | Save content |
|-------|------:|--------:|---------|------------|--------------|--------------|
| DS2   | 28    | 28      | V2      | 28/28      | 28/28        | same         |
| DS2S  | 28    | 28      | V2      | 28/28      | 28/28        | same         |
| BB    | 116   | 129     | V3      | 129/129    | 1/116        | all same     |
| DS3   | 194   | 194     | V3      | 194/194    | 1/194        | all same     |
| SDT   | 253   | 253     | V3, V5  | 253/253    | 0/253        | all same     |
| ER    | 448   | 448     | V5      | 448/448    | 32/448       | all same     |
| AC6   | 284   | 284     | V5      | 284/284    | 0/284        | all same     |
| NR    | 206   | 206     | V5, V6  | 206/206    | 206/206      | same         |
| DB_NT | 60    | 60      | V6      | 60/60      | 0/60         | all same     |
| DS1, DS1R, DES | 0 | 0 | — (no GPARAM) | | | |

All 1,630 GPARAMs read and write back byte-perfect. Every save mismatch is DCX recompression
(DFLT zlib output, Oodle KRAK level/version). The decompressed content always matches, so the game reads
the same data. That belongs to DCX, not GPARAM, and is expected across tools.

BB note: the `game_data.txt` root for BB is `...\Bloodborne`; the data is in `Loose\dvdroot_ps4`, so the run
used `BB=G:\Game Data Repository\Bloodborne\Loose\dvdroot_ps4`.

## Fix: V2 (DS2) trailing padding

Before the fix, 18/28 DS2 and 20/28 DS2S files came back 4–12 bytes short. The cause was not the ShiftJIS key padding
(the old `TODO`s): every V2 file is padded to 16 bytes after the last extra-ID array, and the padding is
uninitialised memory (e.g. `07 02 8D E3 12 07 93 1A CD 3C C4 DE`), not zeros.

`GPARAM.cs` changes:

- New `Padding` property (V2 only): the bytes after `ParamExtraIds + 4 * total IDs`.
- `Write` for V2 pads to 16 bytes, reusing `Padding` bytes and filling with zeros past them, so edited
  or newly made files stay 16-aligned as the game's files are.
- `Clone()` now copies `Padding` and also `Unk40`, which it dropped before.
