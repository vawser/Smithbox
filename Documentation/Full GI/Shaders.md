# What the game's shaders do

Traced from Nightreign's DXIL (`shader/gxrenderershader.shaderbnd.dcx`, `gxflvershader.shaderbnd.dcx`). AC6 and ER
were not disassembled but use the same files and region data. Reflection names survive in the DXIL, so the names
below are the game's own.

## Two paths

1. **Deferred, the main one**: `GXEnvMapRegion[Box][DiffSpcPcc][Lerp | LerpDiff | PreLerp].ppo` in gxrenderershader.
   A pass per EnvMapEffectBox, drawn as a box over the G-buffer. Writes `SV_Target0` = reflection (specular) and
   `SV_Target1` = diffuse irradiance, both with alpha = fade, blended over the previous regions.
2. **Forward** (transparent and some special materials): `GXFlver_*_Fwd*`, `*_Velo_[A]`, decals in gxflvershader /
   gxdecal. Each draw gets up to two volumes, A and B, chosen on the CPU, in `cbModelParam`
   (`FC_IrradianceVolumeAInvObb`, `...SamplingParam`, `...Enable`, `...Scale`, `...SafeVoxeJacobian`).
   Forward shaders declare `FC_IrradianceVolume{A,B}ShLuminance` but never read it.

Smithbox renders forward, so the plan follows the forward math with the deferred pass's fade and blending.

## CB_ENVMAP_REGION (deferred pass, b3, 576 bytes)

```
float4x4 g_envMapWorld;            //   0
float4x4 g_envMapInvWorld;         //  64  world -> box local, box spans -1..1
float4   g_envMapDrawAreaExtents;  // 128  box half extents in metres
float    g_envMapColorScale;       // 144  scales the reflection
float    g_envMapFadeDistance;     // 148
float    g_envMapInvFadeDistance;  // 152
float4x4 g_envMapMatPccObb;        // 160  parallax-corrected cubemap box
float4x4 g_envMapMatInvPccObb;     // 224
float4   g_envMapRegionPccOrigin;  // 288
float4   g_envMapDebugOutputColor; // 304
float    g_envMapFadeRate;         // 320  multiplies the fade for the output alpha
float    g_envMapDiffuseColorScale;// 324
float4x4 g_envMapInvIv;            // 336  world -> volume local (used for the normal offset)
float4   g_envMapIvSamplingParam;  // 400  xyz divide the normal offset (probably voxel counts)
float    g_envMapInterpRatio;      // 416  blend from bake slot 0 to slot 1
float    g_ivTempIvSize;           // 420
float4x4 g_envMapViewToLocal;      // 432
float    g_envMapMipmapLevelNum0;  // 496
float    g_envMapMipmapLevelNum1;  // 500
float4   g_ivSafeVoxelJacobian0;   // 512  xyz shrink UVW around 0.5
float4   g_ivSafeVoxelJacobian1;   // 528
float4   g_ivShLuminance0;         // 544  IVINFO of bake slot 0
float4   g_ivShLuminance1;         // 560  IVINFO of bake slot 1
```

Textures: `g_IrradianceVolume{X,Y,Z,W}` (t27-t30) = bake slot 0, `g_IrradianceVolume2{X,Y,Z,W}` (t31-t34) = slot 1,
`g_IBLTexture` (t25, cube) = reflection. Samplers: `SS_ClampLinear` for the volumes (trilinear, clamp).

## Step by step (deferred, per pixel)

```hlsl
// 1. World position from depth (FC_MatrixInvViewProj); normal from GBuffer1 (rgb*2-1, normalised)
float3 local = mul(g_envMapInvWorld, float4(worldPos, 1)).xyz;          // -1..1 inside the box

// 2. Fade in from the faces: distance to the nearest face in metres
float3 d3   = (1 - abs(local)) * g_envMapDrawAreaExtents.xyz;
float  d    = min(d3.x, min(d3.y, d3.z));
if (d < 0) discard;                                                      // outside
float  fade = smoothstep(0, 1, saturate(d * g_envMapInvFadeDistance));

// 3. UVW, pulled in so samples stay between voxel centres
float3 uvw = (local * 0.5 + 0.5 - 0.5) * g_ivSafeVoxelJacobian0.xyz + 0.5;

// 4. Push one voxel along the normal, to avoid light leaking from behind the surface
float3 dir = normalize(mul(g_envMapInvIv, float4(worldPos + N, 1)).xyz - localIvPos);
uvw += dir / g_envMapIvSamplingParam.xyz;

// 5. Sample and decode each of X, Y, Z, W (same for slot 1 with InterpRatio)
float4 t     = tex.SampleLevel(clampLinear, uvw, 0);
float  scale = exp2((t.a - 0.5) * 20) * 2;                               // per-texel exponent, BASE 2
float3 c     = (t.rgb - 0.5) * scale;  // W texture: L0 = max(c * 1, 0)   (no 0.5)
                                       // X/Y/Z:    L1 axis = c * 0.5
```

Decode constants are exact: `-0.5`, `* 20`, `exp2`, `* 2`, then L1 also `* 0.5`.
DXIL unary op 21 (`Exp`) is base 2, as op 23 (`Log`) is log2; HLSL `exp(x)` compiles to `Exp(x * log2(e))`.
Checked against the data: decoded with base 2, each volume's mean voxel L0 luminance is 0.5-4x its IVINFO
`ShLuminance.w`; with base e it is 2.4-22x off, always low (ivtool `units`).
Texture order in the DXIL: X, Y, Z, W samples feed L1.x, L1.y, L1.z, L0.

## SH evaluation (non-linear L1, Geomerics style)

Per RGB channel `c`, with `L0 = W.c`, `L1 = (X.c, Y.c, Z.c)`:

```hlsl
float  len = length(L1);
float3 dir = L1 / len;                        // len == 0 -> big constant, no NaN
float  q   = saturate((dot(N, dir) + 1) * 0.5);
float  R1  = saturate(len / (L0 + 1e-5));     // 0x3EE4F8B580000000 = 1e-5
float  p   = 1 + 2 * R1;
float  a   = saturate((1 - R1) / (1 + R1));
float  E   = L0 * (a + (1 - a) * (p + 1) * pow(q, p));
E = max(E, 0);
```

Forward path result: `irradiance * FC_IrradianceVolumeAScale` (cbModelParam offset 584), A and B added if both on.
Deferred diffuse output: `max(0, mul(SC_EnvColorConvert, lerp(E0, E1, InterpRatio) * g_envMapDiffuseColorScale)) * SC_PreExposure`.
`SC_EnvColorConvert` is a 3x3 colour matrix from cbSceneParam; use identity in Smithbox.

## Reflection normalisation (uses IVINFO)

```hlsl
float3 Elin = max(0, W + mul(float3x3(X, Y, Z), 0.5 * N));   // cheap linear SH, per RGB
float  lum  = dot(Elin, float3(0.299, 0.587, 0.114));
float  avg  = max(0, dot(g_ivShLuminance0, float4(0.5 * N, 1)));
float  k0   = g_ivShLuminance0.w > 0 ? lum / (avg + 1e-5) : 1;  // same for k1
float  k    = lerp(k0, k1, g_envMapInterpRatio);
spec = max(0, mul(SC_EnvColorConvert, cube.Sample(...))) * k * g_envMapColorScale ...;
```

So IVINFO's `ShLuminance` is the volume's average of the same linear SH. This explains why |XYZ| <= 2W in every file.

## Open questions (CPU-side values, not in any file)

- `g_ivSafeVoxelJacobian.xyz`: probably `(N - 1) / N` per axis so UVW 0 and 1 land on voxel centres.
- `g_envMapIvSamplingParam.xyz`: divides a unit direction, so probably voxel counts (one-voxel offset).
- `g_envMapInvIv` vs `g_envMapInvWorld`: both appear; probably the same box, InvIv possibly without scale.
- Order of overlapping regions: by `Priority`, then probably large to small (the coarse ID-0 box first).
- Which bake the game picks and how `InterpRatio` is driven: time of day; use bake `_00` and a preference.
- Whether `AmbientDiffuseIntensity` maps to `g_envMapDiffuseColorScale` (ER Env_Box902 has 0, so likely).
