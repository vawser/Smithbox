# Tools and how the analysis was done

## ivtool (source in `tools/`)

A throwaway .NET 10 console app referencing `Andre.SoulsFormats.csproj` and `Andre.IO.csproj`, plus the
`DirectXTexNet` package for BC7 decoding. Build it outside the repo (it was in the session scratchpad), then:

- Copy the game's `oo2core_*.dll` (ER: `oo2core_6`, AC6/NR: `oo2core_8`) into the output folder and run from there;
  `Oodle` looks in the current directory.
- For ER, copy `src/Smithbox.Data/Assets/UXM Dictionaries/EldenRingDictionary.txt` into
  `<output>/Assets/UXM Dictionaries/`; the VFS loads it relative to the working directory.

| Command | Does |
|---|---|
| `extract <bnd.dcx> <outdir>` | Unpack a BND4 |
| `survey <AC6/NR> <mapdir>` | Field statistics for every IVINFO on disk |
| `er <ER Game dir>` | The same survey through the ER VFS |
| `rt <AC6/NR/ER> <dir>` | Round-trip every IVINFO through the class |
| `tpf <files...>` | Texture names, formats, DDS dimensions |
| `decode <ivinfobnd> <tpfs...>` | Decode BC7 volumes and print channel ranges |
| `shaders <shader dir> <outdir>` | Unpack shader binders (shaderbdle fails on a nested format; not needed) |
| `msb <AC6/NR/ER> <msb.dcx> <type filter>` | Dump region fields by reflection |
| `match <AC6/NR/ER> <Game dir> <mapId>` | Link EnvMapEffectBoxes to GIIV volumes and IVINFO (ER/NR by name: analysis only) |
| `raw <NR/ER> <Game dir> <mapId> <regionType>` | Raw header index and type-data words of regions, bypassing SoulsFormats |
| `strings <exe> <regex>` | ASCII and UTF-16 strings with file offsets |
| `xref <exe> <fileOffsets,...> <before> <after>` | Code that references those offsets RIP-relatively, with context |
| `callers <exe> <vas,...> <span> <before> <after>` | Calls or jumps into an address range |
| `func <exe> <va>` | Whole function containing an address, bounds from `.pdata` |
| `units <ivinfobnd> <tpfs...>` | Mean decoded L0 luminance of each volume (base 2 and base e) against its IVINFO |
| `ergp <ER Game dir> <file regex> [group] [field regex]` | List ER drawparam files through the VFS, or dump matching fields' value IDs and TimeOfDay keys |
| `erld <ER Game dir>` | Across ER map gparams, how often each LightSet field of each value ID differs from ID 0 |
| `erscene <ER Game dir> <msb regex>` | Per ER map, the collisions' scene GPARAM sub IDs, against the drawparam files of its area |
| `erbake <ER Game dir> <map regex>` | Mean IVINFO ShLuminance of each bake of the maps' high ivinfobnds |

## gpdump (source in `tools/gpdump/`)

Same setup as ivtool, for the loose NR/AC6 files (copy the game's `oo2core_*.dll` next to it).

| Command | Does |
|---|---|
| `<file.gparam.dcx> [group]` | Every field with all its value IDs and TimeOfDay keys |
| `diff <file.gparam.dcx>` | Per field, which value IDs differ from ID 0 |
| `msb <NR/AC6/ER> <msb.dcx>` | Counts of each part type's Gparam / SceneGparam struct values |
| `param <ER/NR/AC6> <regulation.bin> <def.xml> <param> [field regex]` | Rows of one regulation param (e.g. `WeatherParam`, `GparamRefSettings`) |
| `models <AC6 msb.dcx>` | Counts of the non-map parts by type and model, and the character models |

The exe commands use the `Iced` 1.21.0 package. They work on ER because the code that builds the names is
normal compiled code. NR's equivalent code is split by Arxan into jumps (`jmp 140C756C3`) and was not traced;
tracing it would need a binary with the protection removed, made with an existing community tool.

Chain used for ER: `strings` for `%s_GIIV%04d_%02d` -> `xref` -> `callers` -> `func` on the callers -> read
where the formatted integer is computed.

## Shader disassembly

Shader binders hold DXBC containers with DXIL. Disassemble with the Windows SDK:

```bash
"/c/Program Files (x86)/Windows Kits/10/bin/10.0.26100.0/x64/dxc.exe" -dumpbin "GXEnvMapRegion[Box][DiffSpcPcc][LerpDiff].ppo" > out.txt
```

Reflection is not stripped, so cbuffer layouts and resource names are in the header. To follow a field:
convert its offset to a row (`offset / 16`), find the `createHandle` for its cbuffer (resource class 2, range id
= the `CBn` number in the bindings table), then grep `cbufferLoadLegacy(..., %handle, i32 row)`.
Useful files: `gxrenderershader/GXEnvMapRegion*.ppo` (deferred), `gxflvershader/GXFlver_ColDif_Fwd.ppo` (forward).
Search names with `grep -raoh "[A-Za-z_0-9]*[Ii]rradiance[A-Za-z_0-9]*"` across the unpacked binders.

Op codes seen: 6 FAbs, 7 Saturate, 21 Exp, 23 Log, 24 Sqrt, 25 Rsqrt, 35 FMax, 36 FMin, 46 FMad,
55 Dot3, 56 Dot4, 59 CBufferLoadLegacy, 62 SampleLevel, 66 TextureLoad.
