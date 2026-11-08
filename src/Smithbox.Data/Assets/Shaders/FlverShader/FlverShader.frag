#version 450
#extension GL_EXT_nonuniform_qualifier : require
#extension GL_EXT_shader_explicit_arithmetic_types_int64 : enable

// Fragment stage of the flver shader.
//
// Macros (set by the variant list in FlverShader.json):
//   MATERIAL_BLEND       - the material has more than one texture layer
//   MATERIAL_BLEND_MASK  - layer weights come from a blend mask texture instead of the vertex colors
//   MATERIAL_DEFORM      - the mesh has the TEXCOORD1-3 attributes, which can hold the layer weight
//   SELECTED             - used for selected objects: adds selection dithering and tinting
//   OUTLINE              - only draws the selection outline, for meshes with a deformer to outline
//   FOCUS                - used for maps that are out of focus: dithers and greys out the object
//
// Lighting: one sun (sceneparam.lightDirection / sunColor) plus up to MAX_SCENE_LIGHTS point, spot or
// directional lights from the scene's light list.
//
// Blending: a material can have up to MAX_LAYERS layers of albedo (and optionally normal, specular and
// shininess). Layer weights are "splat" weights:
//     layer k (k >= 1) = channel k-1 of the weight source, layer 0 = whatever is left over.
// With two layers this is exactly mix(layer0, layer1, channel0).
// Weight source:
//     MATERIAL_BLEND_MASK  -> blend mask texture, channels r, g, b
//     c_blendFromTexCoord  -> TEXCOORD1.x, for two layers
//     c_blendFromRecipe    -> worked out as the game's shader does, see ComputeRecipeWeights
//     otherwise            -> vertex color, channels a, r, g

#define GAME_DES 1
#define GAME_DS1_PTDE 2
#define GAME_DS1_REMASTER 3
#define GAME_DS2 4
#define GAME_BLOODBORNE 6
#define GAME_DS3 5
#define GAME_SEKIRO 7
layout (constant_id = 0) const int c_gameID = GAME_DS3;
#ifdef MATERIAL_BLEND
layout (constant_id = 1) const bool c_blendNormal = false;
layout (constant_id = 2) const bool c_blendSpecular = false;
layout (constant_id = 3) const bool c_blendShininess = false;
layout (constant_id = 4) const int c_layerCount = 2;
layout (constant_id = 5) const bool c_blendFromTexCoord = false;
layout (constant_id = 7) const bool c_blendFromRecipe = false;
#endif

layout (constant_id = 6) const bool c_transparent = false;

// The material's MaterialBlending: 0 opaque, 1 alpha, 2 add, 3 subtract
layout (constant_id = 11) const int c_blending = 0;
#define BLENDING_ALPHA 1
layout (constant_id = 8) const bool c_detail = false;
layout (constant_id = 9) const bool c_magma = false;
layout (constant_id = 10) const int c_surface = 0;
layout (constant_id = 99) const bool c_picking = false;

// True when drawn into the HDR scene target, where the output is linear light, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

// True for the sun's shadow map, which takes depth alone: only the alpha test is done, see ShadowCascades.cs
layout (constant_id = 98) const bool c_shadowPass = false;

#define FEATURE_WIND 1u
#define FEATURE_VERTEX_ANIMATION 2u
#define FEATURE_SWAY 4u
#define FEATURE_BIRDS 8u
#define FEATURE_LAYER_BLENDING 16u
#define FEATURE_OVERLAYS 32u
#define FEATURE_MAGMA 64u
#define FEATURE_EMISSIVE 128u
#define FEATURE_UV_SCROLL 256u
#define FEATURE_UNLIT 512u
#define FEATURE_WATER 1024u
#define FEATURE_FUR 2048u

#define SURFACE_UNLIT 1
#define SURFACE_WATER 2
#define SURFACE_FUR 3

#define MAX_LAYERS 4
#define MAX_SCENE_LIGHTS 16

// The share of pixels kept for a map that is out of focus
#define FOCUS_OPACITY 0.4

#define LIGHT_DIRECTIONAL 0
#define LIGHT_POINT 1
#define LIGHT_SPOT 2

layout(location = 0) in vec2 fsin_texcoord;
layout(location = 1) in vec3 fsin_worldpos;
layout(location = 2) in mat3 fsin_worldToTangent;
layout(location = 7) in vec4 fsin_color;
layout(location = 8) flat in uint fsin_mat;
layout(location = 9) flat in uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) in vec2 fsin_texcoord2;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	layout(location = 11) in vec3 fsin_vertexBlend;
#endif
// 1: the instance receives no shadows, an MSB part's IsShadowDest
layout(location = 12) flat in uint fsin_flags;
#ifdef MATERIAL_BLEND
	// DS3's lightmaps: the entry in lightmapdata plus 1 (0 for none), and the model's rotation into the world
	layout(location = 13) flat in uint fsin_lightmap;
	layout(location = 14) flat in mat3 fsin_modelToWorld;
#endif

#define INSTANCE_NO_SHADOW 1u

layout(location = 0) out vec4 fsout_color;

// Beside the color in the HDR target, for the screen space effects, see SceneTarget.cs. Only opaque draws have them
// written; elsewhere the pipeline leaves them alone, or there is no attachment for them.
layout(location = 1) out vec4 fsout_surface;  // xy: normal, octahedral, z: roughness, w: specular
layout(location = 2) out vec4 fsout_indirect; // rgb: the indirect diffuse light in the color
layout(location = 3) out vec4 fsout_reflection; // rgb: the environment's reflection (indirect specular) in the color
layout(location = 4) out vec4 fsout_glow;       // rgb: the light the surface gives off itself, emissive and magma

struct sceneLight
{
	vec4 positionRange;  // xyz: position, w: range (0 = no limit)
	vec4 directionType;  // xyz: direction the light travels, w: type
	vec4 colorIntensity; // rgb: color, a: intensity
	vec4 spot;           // x: cos(inner angle), y: cos(outer angle), z: 1 to fade across the range
};

// A directional light of the map's GPARAM
struct mapDirectionalLight
{
	vec4 direction;  // xyz: the direction the light travels in
	vec4 color;      // rgb: color, a: intensity with the exposure
	vec4 specColor;  // the color and intensity of its highlights
};

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
	ivec4 curserPosition;
	uint envmap;

	float ambientLightMult;
	float directLightMult;
	float indirectLightMult;
	float emissiveMapMult;
	float sceneBrightness;
	float simpleFlverBrightness;
	float simpleFlverSaturation;
	vec4 selectionColor;
	vec4 outlineColor;
	uint EnableDithering;
	uint EnableTinting;
	float ditherOpacity;

	uint lightCount;
	vec4 sunColor;       // rgb: color, a: intensity
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
	float windPeriod;
	uint materialFeatures;
	uint sceneDepthTexture; // index in globalTextures of the depth behind transparent draws, 0 if there is none

	vec4 wind;
	float collisionOpacity;
	float navVolumeSliceHeight;
	uint navVolumeSliceFlags;

	// The irradiance volumes to light with: entries of volumedata, see IrradianceVolumeTable
	uint irradianceVolumeStart;
	uint irradianceVolumeCount;
	float irradianceVolumeExposure; // scales the volumes' light to the viewport's
	uint irradianceVolumeFlags;     // 1: scale reflections by the volumes' light

	uint mapLightingFlags;          // 1: lit by the map's GPARAMs, 2: the hemisphere adds to the volumes' light
	vec4 sunSpecColor;              // the sun's highlights, rgb: color, a: intensity

	// The map's GPARAM hemisphere light, rgb already multiplied by intensity and exposure
	vec4 hemiUp;
	vec4 hemiDown;
	vec4 envMapScale;               // reflections' color scale, rgb
	float envMapSaturate;           // reflections' saturation, 0 grey, 1 unchanged

	// The model set of the map's GPARAM LightSet, for characters when mapLightingFlags has 4: its sun, Light1,
	// LocalLight1 and LocalLight2, and its hemisphere
	mapDirectionalLight modelLights[4];
	vec4 modelHemiUp;
	vec4 modelHemiDown;

	// The map's lights near the camera: entries of maplightdata, see SceneLightTable
	uint mapLightStart;
	uint mapLightCount;
	// DS3's lightmaps: entries of lightmapdata, see LightmapTable (padding in the other shaders' copies)
	uint lightmapStart;
	uint lightingDebugView; // CFG.Viewport_Lighting_Debug_View, see SceneParam.LightingDebugView

	// The sun's cascaded shadow maps, see ShadowCascades.cs. Depth is reversed: 1 nearest the sun.
	mat4 shadowMatrices[4];  // world to each cascade's clip space
	vec4 shadowSplits;       // how far along the view each cascade ends
	vec4 shadowTexelSizes;   // meters per shadow map texel in each cascade
	vec4 shadowDepthScales;  // shadow map depth per meter towards the sun in each cascade
	uint shadowTexture;      // index in globalTextures of the map holding the cascades, 0 for no shadows
	uint shadowCascadeCount;
	float shadowFadeStart;   // where shadows start to fade out along the view
	float shadowFadeEnd;     // and where they are gone
	float shadowDepthBias;   // the GPARAM's Depth Bias
	float shadowSlopeBias;   // and Slope Scaled Depth Bias
	float shadowCascadeFade; // the share of each cascade's range that blends into the next
	float shadowStrength;    // how much of the sun a shadow takes away
	float shadowMapSize;     // texels across the whole map, two cascades to a side

	// The GPARAM Wet group in a weather that wets, see SceneParam.cs; the rates are 0 when it is dry
	vec4 wetAlbedo;          // rgb: the albedo wet surfaces lean towards, a: how far
	vec4 wetShininess;       // x: the shininess they lean towards, y: how far

	// The volumetric fog, for transparent draws, see VolumetricFogPass.cs and SceneParam.cs
	uint fogVolumeTexture;   // index in globalTexturesVolume, 0 for no fog this frame
	float fogNear;           // the volume's near depth
	float fogLogRatio;       // log(far / near)
	float fogSlices;
	vec4 fogRange;           // x: far depth, y: end depth past it (0 for none), zw: the target's size
	vec4 fogDensity;         // x: fixed, y: height scale, z: distribution, w: height offset
	vec4 fogExtinction;      // x: extra extinction from AbsorptionScale
	vec4 fogSky;             // rgb: sky light, w: height attenuation
	vec4 fogDirect;          // rgb: sun light towards the sun, w: height attenuation
	vec4 fogOpposite;        // rgb: sun light away from it, w: ScatteringG
	vec4 fogSun;             // xyz: the direction the sun's light travels

	// What the rain reaches, see RainOcclusion.cs: the scene's depth seen straight down round the camera
	mat4 rainMatrix;         // world to the map: xy -1..1 across, z the depth, 1 at the top
	uint rainTexture;        // index in globalTextures, 0 when it isn't read
	float rainMapSize;       // texels across
	float rainTexelSize;     // meters per texel
	float rainDepthScale;    // depth per meter of height

	// The per-part lighting sets: entries of lightsetdata, see LightSetTable. Last, so the other shaders' copies of
	// this struct, which stop earlier, are unchanged.
	uint lightSetStart;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];
layout(set = 3, binding = 0) uniform textureCube globalTexturesCube[];

// The irradiance volumes of AC6, ER and NR maps, sharing the cube textures' set
layout(set = 3, binding = 1) uniform texture3D globalTexturesVolume[];

// Texture indices into globalTextures, stored as 16 bit values packed two to a 32 bit word.
// Six rows of four values:
//   row 0: albedo, layers 0-3          row 3: shininess, layers 0-3
//   row 1: normal, layers 0-3          row 4: x blend mask, y emissive, z lightmap, w lightmap 2
//   row 2: specular, layers 0-3        row 5: the UV set (0 or 1) used by layer 0-3
struct materialData
{
	uvec4 words0;
	uvec4 words1;
	uvec4 words2;
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly materialData matdata[];
};

// An irradiance volume, laid out as IrradianceVolumeData. The table is an allocation of the material buffer,
// starting at entry sceneparam.irradianceVolumeStart.
struct irradianceVolumeData
{
	vec4 boundsMin;         // xyz: world bounds of the box, tested first
	vec4 boundsMax;
	mat4 worldToLocal;      // world to the volume's box, -1 to 1 on each axis
	vec4 extentsFade;       // xyz: half the box size in metres, w: 1 / fade distance
	vec4 voxelCountsScale;  // xyz: voxel counts, w: diffuse scale
	uvec4 bake0;            // X, Y, Z, W in globalTexturesVolume
	uvec4 bake1;            // the same for the bake blended towards
	vec4 shLuminance0;      // the IVINFO of each bake, its average light
	vec4 shLuminance1;
	vec4 info;              // x: volume ID, y: bake, z: bake blended towards, w: how far towards it
	vec4 params;            // x: reflection scale
	vec4 reserved0;
	vec4 reserved1;
};

// A lightmap of a part (DS3's DOL textures), laid out as LightmapData. Entry 0 holds the GPARAM's Dominant Light
// Map values: uvScaleOffset.x the sharpness offset, hdrScale.rgb the color scale. See LightmapTable.
struct lightmapData
{
	uvec4 textures;      // x: DOL 1 (color, sun shadow in alpha), y: DOL 2 (direction, sharpness in alpha)
	vec4 uvScaleOffset;  // the second UV set's scale (xy) and offset (zw) into the texture
	vec4 hdrScale;       // rgb: DOL 1's HDR scale
};

layout(set = 4, binding = 0, std140) buffer lightmaps
{
    readonly lightmapData lightmapdata[];
};

// A part's own map lighting, laid out as LightSetData: what the scene params hold for the whole scene, for the
// instances that name the set in their material ID (Demon's Souls' LightIDs). See LightSetTable.
struct lightSetData
{
	mapDirectionalLight sun;
	vec4 hemiUp;
	vec4 hemiDown;
	mapDirectionalLight modelLights[4];
	vec4 modelHemiUp;
	vec4 modelHemiDown;
	vec4 pad;
};

layout(set = 4, binding = 0, std140) buffer lightSets
{
    readonly lightSetData lightsetdata[];
};

layout(set = 4, binding = 0, std140) buffer irradianceVolumes
{
    readonly irradianceVolumeData volumedata[];
};

// A light of the map, with the color and intensity of its highlights, which BTL gives separately
struct mapLight
{
	sceneLight light;
	vec4 specular;   // rgb: color of the highlights, a: their intensity
};

// The map's lights near the camera, in the same buffer, see SceneLightTable
layout(set = 4, binding = 0, std140) buffer mapLights
{
    readonly mapLight maplightdata[];
};

// The entry after a material in the same buffer
struct shadingData
{
	vec4 diffuseAlpha; // xyz: multiplies the albedo, w: alpha below which a pixel is discarded
	vec4 tiling;       // xy: albedo of layer 0, zw: normal of layer 0
	vec4 tiling2;      // xy: albedo of layer 1, zw: normal of layer 1
};

layout(set = 4, binding = 0, std140) buffer materialShading
{
    readonly shadingData shadingdata[];
};

// Eight entries after a material, for materials with an overlay
struct detailData
{
	vec4 tiling;        // xy: albedo, zw: normal
	vec4 colorStrength; // rgb: multiplies the albedo, w: strength of the normal
	uvec4 textures;     // x: albedo, y: normal, 0 if there is none
};

layout(set = 4, binding = 0, std140) buffer materialDetail
{
    readonly detailData detaildata[];
};

// Nine and ten entries after a material, for materials with a glow
struct magmaFlowData
{
	vec4 flow0;   // xy: UV movement of the first sample each second, zw: its UV scale
	vec4 flow1;   // xy: UV movement of the second sample each second, zw: its UV scale
	vec4 tiling;  // xy: tiling, z: strength, w: power
};

struct magmaColorData
{
	vec4 distort; // xy: how far the red bends the third sample
	vec4 color;   // rgb: color, w: the most it can give off
	uvec4 info;   // x: texture index, y: which sample has the red
};

layout(set = 4, binding = 0, std140) buffer materialMagmaFlow
{
    readonly magmaFlowData magmaflowdata[];
};

layout(set = 4, binding = 0, std140) buffer materialMagmaColor
{
    readonly magmaColorData magmacolordata[];
};

// Eleven and twelve entries after a material, for water
struct waterDetailData
{
	vec4 tiling; // xy: tiling of the second normal, zw: UV movement each second
	vec4 bend;   // xy: how far the first normal's blue bends its UVs, z: its strength, w: gloss
	vec4 tint;   // rgb: multiplies the scattered light, w: 1 if the color comes from the volume
};

struct waterVolumeData
{
	vec4 scatter;    // rgb: scattering, w: what depth is multiplied by to fade in at the shore
	vec4 absorption; // rgb: absorption
	vec4 unused;
};

layout(set = 4, binding = 0, std140) buffer materialWaterDetail
{
    readonly waterDetailData waterdetaildata[];
};

layout(set = 4, binding = 0, std140) buffer materialWaterVolume
{
    readonly waterVolumeData watervolumedata[];
};

// The entry after that
struct effectsData
{
	vec4 emissiveColor;  // xyz: multiplies the emissive texture, w: how far the dither moves the alpha of fur
	vec4 emissiveTiling; // xy: tiling, zw: UV movement each second
	vec4 scroll;         // xy: UV movement of layer 0's albedo each second, zw: of its normal
};

layout(set = 4, binding = 0, std140) buffer materialEffects
{
    readonly effectsData effectsdata[];
};

#ifdef MATERIAL_BLEND
// The entries after the deform entry, for materials with a blend recipe
struct layerTilingData
{
	vec4 tiling3;    // xy: albedo of layer 2, zw: normal of layer 2
	vec4 tiling4;    // xy: albedo of layer 3, zw: normal of layer 3
	vec4 maskTiling; // xy: blend mask, zw: detail mask
};

struct blendData
{
	vec4 sharpness; // xyz: layers 1 to 3, w: bias of the detail mask
	vec4 detail;    // xyz: layers 1 to 3, w: scale of the weights
	uvec4 sources;  // xyz: layers 1 to 3 as below, w: texture index of the detail mask
};

struct layerTintData
{
	vec4 tint[3];   // rgb: layers 1 to 3, and the three w are layer 0
};

struct layerScrollData
{
	vec4 scroll[3]; // layers 1 to 3, xy: albedo, zw: normal
};

layout(set = 4, binding = 0, std140) buffer materialLayerTiling
{
    readonly layerTilingData layertilingdata[];
};

layout(set = 4, binding = 0, std140) buffer materialBlend
{
    readonly blendData blenddata[];
};

layout(set = 4, binding = 0, std140) buffer materialLayerTint
{
    readonly layerTintData layertintdata[];
};

layout(set = 4, binding = 0, std140) buffer materialLayerScroll
{
    readonly layerScrollData layerscrolldata[];
};

// A source is a channel: 0-2 the blend mask, 3-5 TEXCOORD1, 6 the vertex color. On top of that are these flags
#define SOURCE_CHANNEL_MASK 7u
#define SOURCE_INVERT 8u
#define SOURCE_SOFT 16u
#endif

layout(set = 5, binding = 0) uniform sampler linearSampler;
layout(set = 5, binding = 1) uniform sampler anisoLinearSampler;

float Epsilon = 0.00001;

float LdotNPower = 0.1;

struct updatePickingBuffer
{
	uint depth;
	uint pad;
	uint64_t identifier;
};

layout(set = 6, binding = 0, std140) buffer pickingBuffer
{
	volatile updatePickingBuffer pb;
};

void UpdatePickingBuffer(ivec2 pos, uint64_t identity, float z)
{
	if (sceneparam.curserPosition.x != pos.x || sceneparam.curserPosition.y != pos.y)
	{
		return;
	}

	uint d = floatBitsToUint(z);
	if (d <= pb.depth)
	{
		return;
	}
	pb.depth = d;
	pb.identifier = fsin_entityid;
}

// ---------------------------------------------------------------------------------------------
// Texture sampling
// ---------------------------------------------------------------------------------------------

// State shared by the sampling helpers, set once at the start of main
uint g_mat;
bool g_modelLit = false; // lit by the model set of the map's GPARAM, see ModelLit
uint g_lightSet = 0u;     // the instance's own lighting set, 0 for the scene's, see LightSet
vec2 g_uv[2];
vec3 g_indirectDiffuse = vec3(0.0); // the diffuse part of IndirectLight, before the scene multipliers
vec3 g_indirectSpecular = vec3(0.0); // and its specular part
float g_probeSunMask = 1.0; // the share of the sun DS3 and Bloodborne's light probes let through here
// For the lighting debug view: where the light from around came from (0 the hemisphere or environment map, 1 the
// volumes or light probes, 2 a lightmap, 3 a lightmapped material without its lightmap), and that light
uint g_lightSource = 0u;
vec3 g_ambientRaw = vec3(0.0);
vec2 g_dx[2];
vec2 g_dy[2];
float g_weight[MAX_LAYERS];

#define ROW_ALBEDO 0
#define ROW_NORMAL 1
#define ROW_SPECULAR 2
#define ROW_SHININESS 3
#define ROW_EXTRA 4
#define ROW_UVSET 5

// One of the 16 bit values of the current material
uint MaterialValue(int row, int index)
{
	uint word = uint(row * 2 + (index >> 1));
	uint packed;
	if (word < 4u)
	{
		packed = matdata[g_mat].words0[word];
	}
	else if (word < 8u)
	{
		packed = matdata[g_mat].words1[word - 4u];
	}
	else
	{
		packed = matdata[g_mat].words2[word - 8u];
	}

	return (packed >> uint((index & 1) * 16)) & 0xFFFFu;
}

// What is switched on in the viewport, see MaterialFeatures
bool HasFeature(uint feature)
{
	return (sceneparam.materialFeatures & feature) != 0u;
}

// Freed material memory can be reused for anything, and a texture past the pool loses the device.
// Same as the size of GlobalTexturePool in SceneRenderer
int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// Samples with explicit derivatives, so a layer can be skipped without making derivatives undefined
vec4 SampleAt(uint tex, int uvSet, vec2 tiling, vec2 scroll)
{
	return textureGrad(
		sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], anisoLinearSampler),
		(g_uv[uvSet] + (HasFeature(FEATURE_UV_SCROLL) ? scroll * sceneparam.materialTime : vec2(0.0))) * tiling, g_dx[uvSet] * tiling, g_dy[uvSet] * tiling);
}

// Layers past the first only scroll with a blend recipe. Specular and shininess follow the albedo
vec2 LayerScroll(int row, int layer)
{
	if (layer != 0)
	{
#ifdef MATERIAL_BLEND
		if (c_blendFromRecipe)
		{
			vec4 layerScroll = layerscrolldata[g_mat + 7u].scroll[layer - 1];
			return row == ROW_NORMAL ? layerScroll.zw : layerScroll.xy;
		}
#endif
		return vec2(0.0);
	}

	vec4 scroll = effectsdata[g_mat + 2u].scroll;
	return row == ROW_NORMAL ? scroll.zw : scroll.xy;
}

// Specular and shininess follow the albedo of their layer. Layers after the first two need a blend recipe
vec2 LayerTiling(int row, int layer)
{
	if (layer > 1)
	{
#ifdef MATERIAL_BLEND
		if (c_blendFromRecipe)
		{
			vec4 upper = layer == 2 ? layertilingdata[g_mat + 4u].tiling3 : layertilingdata[g_mat + 4u].tiling4;
			return row == ROW_NORMAL ? upper.zw : upper.xy;
		}
#endif
		return vec2(1.0);
	}

	vec4 tiling = layer == 0 ? shadingdata[g_mat + 1u].tiling : shadingdata[g_mat + 1u].tiling2;
	return row == ROW_NORMAL ? tiling.zw : tiling.xy;
}

// One layer of a texture row, using the UV set that layer is authored against
vec4 SampleLayer(int row, int layer)
{
	int uvSet = clamp(int(MaterialValue(ROW_UVSET, layer) & 0xFFu), 0, 1);
	return SampleAt(MaterialValue(row, layer), uvSet, LayerTiling(row, layer), LayerScroll(row, layer));
}

#ifdef MATERIAL_BLEND
vec3 LayerTint(int layer)
{
	vec4 tints[3] = layertintdata[g_mat + 6u].tint;

	return layer == 0 ? vec3(tints[0].w, tints[1].w, tints[2].w) : tints[layer - 1].rgb;
}
#endif

// Either layer 0 only, or all layers mixed by their weights
vec4 SampleLayered(int row, bool blendLayers)
{
#ifdef MATERIAL_BLEND
	if (blendLayers)
	{
		vec4 result = vec4(0.0);
		for (int i = 0; i < MAX_LAYERS; i++)
		{
			if (i < c_layerCount && g_weight[i] > 0.0)
			{
				vec4 layerColor = SampleLayer(row, i);
				if (c_blendFromRecipe && row == ROW_ALBEDO)
				{
					layerColor.rgb *= LayerTint(i);
				}
				result += layerColor * g_weight[i];
			}
		}
		return result;
	}
#endif
	return SampleLayer(row, 0);
}

#ifdef MATERIAL_BLEND
// The game's shaders mix each layer over the ones before it. The rest of this
// file adds up layers by weight, so each mix takes its share from the layers below
void ComputeRecipeWeights()
{
	layerTilingData tiling = layertilingdata[g_mat + 4u];
	blendData blend = blenddata[g_mat + 5u];

	vec3 mask = SampleAt(MaterialValue(ROW_EXTRA, 0), 0, tiling.maskTiling.xy, vec2(0.0)).rgb;
	float detailMask = SampleAt(blend.sources.w, 0, tiling.maskTiling.zw, vec2(0.0)).r;

	g_weight[0] = 1.0;

	for (int i = 1; i < MAX_LAYERS; i++)
	{
		g_weight[i] = 0.0;

		if (i >= c_layerCount)
		{
			continue;
		}

		uint source = blend.sources[i - 1];
		uint channel = source & SOURCE_CHANNEL_MASK;

		float value = 0.0;
		if (channel < 3u)
		{
			value = mask[channel];
		}
		else if (channel == 6u)
		{
			value = fsin_color.r;
		}
  #ifdef MATERIAL_DEFORM
		else
		{
			value = fsin_vertexBlend[channel - 3u];
		}
  #endif

		if ((source & SOURCE_INVERT) != 0u)
		{
			value = 1.0 - value;
		}

		float sharpness = blend.sharpness[i - 1];
		float detail = blend.detail[i - 1] * detailMask + blend.sharpness.w;
		float weight;

		if ((source & SOURCE_SOFT) != 0u)
		{
			weight = clamp(value - sharpness * (1.0 - value) * (detail + 0.01), 0.0, 1.0);
		}
		else
		{
			weight = clamp(1.0 - blend.detail.w * sharpness * (2.0 * value - detail), 0.0, 1.0);
		}

		for (int j = 0; j < i; j++)
		{
			g_weight[j] *= 1.0 - weight;
		}
		g_weight[i] = weight;
	}
}

void ComputeLayerWeights()
{
	// Fur only uses the second layer for its strands, which main puts on top of the first itself
	if (c_surface == SURFACE_FUR)
	{
		g_weight = float[MAX_LAYERS](1.0, 0.0, 0.0, 0.0);
		return;
	}

	if (c_blendFromRecipe)
	{
		if (HasFeature(FEATURE_LAYER_BLENDING))
		{
			ComputeRecipeWeights();
		}
		else
		{
			g_weight = float[MAX_LAYERS](1.0, 0.0, 0.0, 0.0);
		}
		return;
	}

  #ifdef MATERIAL_DEFORM
	if (c_blendFromTexCoord)
	{
		g_weight[1] = clamp(fsin_vertexBlend.x, 0.0, 1.0);
		g_weight[0] = 1.0 - g_weight[1];
		g_weight[2] = 0.0;
		g_weight[3] = 0.0;
		return;
	}
  #endif

	vec3 channels;
  #ifdef MATERIAL_BLEND_MASK
	channels = SampleAt(MaterialValue(ROW_EXTRA, 0), 0, vec2(1.0), vec2(0.0)).rgb;
  #else
	channels = vec3(fsin_color.a, fsin_color.r, fsin_color.g);
  #endif
	channels = clamp(channels, 0.0, 1.0);

	// If the weights add up to more than one, scale them down so the layers don't over-brighten
	float total = 0.0;
	for (int i = 1; i < MAX_LAYERS; i++)
	{
		if (i < c_layerCount)
		{
			total += channels[i - 1];
		}
	}
	float scale = total > 1.0 ? 1.0 / total : 1.0;

	float used = 0.0;
	for (int i = 1; i < MAX_LAYERS; i++)
	{
		float w = i < c_layerCount ? channels[i - 1] * scale : 0.0;
		g_weight[i] = w;
		used += w;
	}
	g_weight[0] = max(1.0 - used, 0.0);
}
#endif

// The flowing glow of [magma] shaders. See MaterialMagmaRecipe
vec3 MagmaGlow()
{
	magmaFlowData flow = magmaflowdata[g_mat + 9u];
	magmaColorData magma = magmacolordata[g_mat + 10u];

	uint tex = magma.info.x;
	if (tex == 0u)
	{
		return vec3(0.0);
	}

	vec2 tiling = flow.tiling.xy;

	vec4 sample0 = SampleAt(tex, 0, tiling * flow.flow0.zw, flow.flow0.xy);
	vec4 sample1 = SampleAt(tex, 0, tiling * flow.flow1.zw, flow.flow1.xy);

	float red = magma.info.y == 0u ? sample0.r : sample1.r;
	float green = magma.info.y == 0u ? sample1.g : sample0.g;

	vec2 bent = (g_uv[0] + magma.distort.xy * red) * tiling;
	float blue = textureGrad(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], anisoLinearSampler), bent, g_dx[0] * tiling, g_dy[0] * tiling).b;

	float glow = red * green * pow(max(flow.tiling.z * blue, 0.001), (green + 0.5) * flow.tiling.w);

	return min(glow * magma.color.rgb, vec3(magma.color.w));
}

// ---------------------------------------------------------------------------------------------
// Lighting
// ---------------------------------------------------------------------------------------------

// A unit vector folded onto the octahedron and flattened to -1..1 on each axis
vec2 OctahedralEncode(vec3 n)
{
	n /= abs(n.x) + abs(n.y) + abs(n.z);
	vec2 folded = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
	return n.z >= 0.0 ? n.xy : folded;
}

struct Surface
{
	vec3 diffuse;
	vec3 F0;
	float roughness;
	vec3 N;
	vec3 V;
};

// Light arriving from direction L (pointing from the surface towards the light), with the radiance it lights
// surfaces with and the radiance of its highlights, which a GPARAM sun gives separately
vec3 ShadeLight(Surface s, vec3 L, vec3 radiance, vec3 specularRadiance)
{
	float LdotN = clamp(dot(s.N, L), 0.0, 1.0);

	vec3 halfway = L + s.V;
	vec3 H = dot(halfway, halfway) > 1e-8 ? normalize(halfway) : s.N;
	float NdotH = clamp(dot(H, s.N), 0.0, 1.0);

	// traditional phong model
	if (c_gameID == GAME_DES || c_gameID == GAME_DS1_PTDE)
	{
		vec3 diffuse = s.diffuse * LdotN;
		// no highlight on the side facing away from the light
		vec3 specular = s.F0 * pow(NdotH, 4.0) * (LdotN > 0.0 ? 1.0 : 0.0);
		return diffuse * radiance + specular * specularRadiance;
	}

	// PBR model
	float VdotH = clamp(dot(H, s.V), 0.0, 1.0);
	vec3 F = pow(1.0 - VdotH, 5.0) * (1.0 - s.F0) + s.F0;

	float specPower = exp2((1.0 - s.roughness) * 13.0);
	specPower = max(1.0, specPower / (specPower * 0.01 + 1.0));
	float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);

	vec3 specular = D * F * pow(LdotN, LdotNPower);
	vec3 diffuse = s.diffuse * LdotN * (1.0 - s.F0);

	return diffuse * radiance + specular * specularRadiance;
}

// Works out the direction to a scene light at a point in the world, and how much of its light reaches it, from
// its distance and cone, leaving out its color
float LightAttenuation(sceneLight light, vec3 worldPos, out vec3 L)
{
	int type = int(light.directionType.w + 0.5);

	if (type == LIGHT_DIRECTIONAL)
	{
		L = -normalize(light.directionType.xyz);
		return 1.0;
	}

	vec3 toLight = light.positionRange.xyz - worldPos;
	float dist = length(toLight);
	L = toLight / max(dist, 1e-4);

	float attenuation;
	float range = light.positionRange.w;
	if (light.spot.z > 0.5 && range > 0.0)
	{
		// full strength at the light, fading away across its range
		float fade = clamp(1.0 - dist / range, 0.0, 1.0);
		attenuation = fade * fade;
	}
	else
	{
		// inverse square falloff, windowed to zero at the light's range
		attenuation = 1.0 / (dist * dist + 1.0);
		if (range > 0.0)
		{
			float window = clamp(1.0 - pow(dist / range, 4.0), 0.0, 1.0);
			attenuation *= window * window;
		}
	}

	if (type == LIGHT_SPOT)
	{
		float cosAngle = dot(-L, normalize(light.directionType.xyz));
		attenuation *= smoothstep(light.spot.y, light.spot.x, cosAngle);
	}

	return attenuation;
}

// Works out the direction to, and the radiance from, a scene light at a point in the world
vec3 LightRadiance(sceneLight light, vec3 worldPos, out vec3 L)
{
	return light.colorIntensity.rgb * light.colorIntensity.a * LightAttenuation(light, worldPos, L);
}

// ---------------------------------------------------------------------------------------------
// Irradiance volumes (AC6, ER, NR), as the game's GXEnvMapRegion and forward flver shaders read them.
// See Documentation/Full GI/Shaders.md.
// ---------------------------------------------------------------------------------------------

// A texel holds one band of the light's spherical harmonics for each color channel, signed around 0.5,
// with alpha as a base 2 exponent
vec3 DecodeVolumeTexel(vec4 t)
{
	return (t.rgb - 0.5) * (exp2((t.a - 0.5) * 20.0) * 2.0);
}

vec4 SampleVolume(uint tex, vec3 uvw)
{
	return textureLod(sampler3D(globalTexturesVolume[nonuniformEXT(tex)], linearSampler), uvw, 0.0);
}

// The game's non-linear evaluation of L1 spherical harmonics for one channel, which stays positive and
// keeps the contrast of a strongly directional light
float EvaluateSHChannel(float L0, vec3 L1, vec3 N)
{
	L0 = max(L0, 0.0);

	float len = length(L1);
	vec3 dir = len > 0.0 ? L1 / len : vec3(0.0);

	float q = clamp((dot(N, dir) + 1.0) * 0.5, 0.0, 1.0);
	float R1 = clamp(len / (L0 + 1e-5), 0.0, 1.0);
	float p = 1.0 + 2.0 * R1;
	float a = clamp((1.0 - R1) / (1.0 + R1), 0.0, 1.0);

	return max(L0 * (a + (1.0 - a) * (p + 1.0) * pow(q, p)), 0.0);
}

// The light of one bake of a volume at uvw, and how much its reflections are scaled by, as the game's
// GXEnvMapRegion pass does: the luminance of the light here, from the cheap linear evaluation, over the volume's
// average from its IVINFO (none if the IVINFO's L0 is 0 or less)
vec3 SampleVolumeBake(uvec4 tex, vec3 uvw, vec3 N, vec4 shLuminance, out float reflectionScale)
{
	// The L1 bands are stored at twice their value
	vec3 X = DecodeVolumeTexel(SampleVolume(tex.x, uvw)) * 0.5;
	vec3 Y = DecodeVolumeTexel(SampleVolume(tex.y, uvw)) * 0.5;
	vec3 Z = DecodeVolumeTexel(SampleVolume(tex.z, uvw)) * 0.5;
	vec3 W = DecodeVolumeTexel(SampleVolume(tex.w, uvw));

	reflectionScale = 1.0;

	if (shLuminance.w > 0.0)
	{
		vec3 linearLight = max(W + X * N.x + Y * N.y + Z * N.z, vec3(0.0));
		float luminance = dot(linearLight, vec3(0.299, 0.587, 0.114));
		float average = max(dot(shLuminance, vec4(0.5 * N, 1.0)), 0.0);
		reflectionScale = luminance / (average + 1e-5);
	}

	return vec3(
		EvaluateSHChannel(W.r, vec3(X.r, Y.r, Z.r), N),
		EvaluateSHChannel(W.g, vec3(X.g, Y.g, Z.g), N),
		EvaluateSHChannel(W.b, vec3(X.b, Y.b, Z.b), N));
}

// Defined below, for the light probe grids
bool MapLighting();
vec3 Hemisphere(vec3 D);

// The light arriving at a point from the volumes around it, how much of it is covered by them, and how much
// the reflections there are scaled by. Volumes are in drawing order, coarse first, and each fades in over the
// ones before it.
vec3 VolumeIrradiance(vec3 P, vec3 N, out float coverage, out float reflectionScale)
{
	vec3 E = vec3(0.0);
	coverage = 0.0;
	reflectionScale = 1.0;

	for (uint i = 0u; i < sceneparam.irradianceVolumeCount; i++)
	{
		uint index = sceneparam.irradianceVolumeStart + i;

		// Most volumes are far from any given point, so only their bounds are read for those
		if (any(lessThan(P, volumedata[index].boundsMin.xyz)) || any(greaterThan(P, volumedata[index].boundsMax.xyz)))
			continue;

		irradianceVolumeData v = volumedata[index];

		vec3 local = (v.worldToLocal * vec4(P, 1.0)).xyz;

		// Fade in from the faces, over the distance in metres the box gives
		vec3 d3 = (1.0 - abs(local)) * v.extentsFade.xyz;
		float d = min(d3.x, min(d3.y, d3.z));

		if (d < 0.0)
			continue;

		float fade = smoothstep(0.0, 1.0, clamp(d * v.extentsFade.w, 0.0, 1.0));

		// One voxel along the normal, so a surface isn't lit by the voxels behind it, then held to the outer
		// voxel centres, as the game does and as the repeating sampler needs
		vec3 counts = max(v.voxelCountsScale.xyz, vec3(1.0));
		vec3 uvw = local * 0.5 + 0.5;
		uvw += normalize(mat3(v.worldToLocal) * N) / counts;
		uvw = clamp(uvw, 0.5 / counts, 1.0 - 0.5 / counts);

		// A light probe grid (DS3, Bloodborne; see LightProbeGridBank): plain float bands, read linearly as the
		// game's GXEnvMapRegion_IvDiff does, with the share of the sun that reaches it in W's alpha
		if (v.params.y > 0.5)
		{
			vec4 W = SampleVolume(v.bake0.w, uvw);
			vec3 X = SampleVolume(v.bake0.x, uvw).rgb;
			vec3 Y = SampleVolume(v.bake0.y, uvw).rgb;
			vec3 Z = SampleVolume(v.bake0.z, uvw).rgb;
			// Times the GPARAM's Dominant Light Map SH color, which the entry keeps where a volume keeps its IVINFO
			vec3 Ep = max(W.rgb + X * N.x + Y * N.y + Z * N.z, vec3(0.0)) * v.voxelCountsScale.w * v.shLuminance0.rgb;

			// Reflections stand in for the game's cubemaps with the hemisphere, so they follow how bright the
			// probes are here against it, darkening indoors
			float kp = 1.0;
			if (MapLighting())
			{
				float hemi = dot(Hemisphere(N), vec3(0.299, 0.587, 0.114));
				float probe = dot(Ep * sceneparam.irradianceVolumeExposure, vec3(0.299, 0.587, 0.114));
				kp = clamp(probe / max(hemi, 1e-4), 0.0, 2.0);
			}

			E = mix(E, Ep, fade);
			reflectionScale = mix(reflectionScale, kp, fade);
			coverage = mix(coverage, 1.0, fade);
			g_probeSunMask = mix(g_probeSunMask, clamp(W.a, 0.0, 1.0), fade);
			continue;
		}

		float k;
		vec3 Ev = SampleVolumeBake(v.bake0, uvw, N, v.shLuminance0, k);

		// Blend towards a second bake, as the game does between times of day
		if (v.info.w > 0.0)
		{
			float k1;
			vec3 E1 = SampleVolumeBake(v.bake1, uvw, N, v.shLuminance1, k1);
			Ev = mix(Ev, E1, v.info.w);
			k = mix(k, k1, v.info.w);
		}

		E = mix(E, Ev * v.voxelCountsScale.w, fade);
		reflectionScale = mix(reflectionScale, k * v.params.x, fade);
		coverage = mix(coverage, 1.0, fade);
	}

	return E * sceneparam.irradianceVolumeExposure;
}

// Whether the map's GPARAMs light the scene, see GparamLighting
bool MapLighting()
{
	return (sceneparam.mapLightingFlags & 1u) != 0u;
}

// The instance's own lighting set under map lighting (bits 8-14 of fsin_flags, see MeshRenderableProxy.LightSet), 0
// for the scene's. The helpers below read the set where there is one and the scene params otherwise.
uint LightSet()
{
	return MapLighting() ? (fsin_flags >> 8u) & 0x7Fu : 0u;
}

lightSetData CurrentLightSet()
{
	return lightsetdata[sceneparam.lightSetStart + g_lightSet];
}

// The direction the sun's light travels in, normalized
vec3 SunTravel()
{
	vec3 travel = g_lightSet != 0u ? CurrentLightSet().sun.direction.xyz : sceneparam.lightDirection.xyz;
	return dot(travel, travel) > 1e-10 ? normalize(travel) : vec3(0.0, -1.0, 0.0);
}

// The sun's color and intensity (a), and its highlights'
vec4 SunColor()
{
	return g_lightSet != 0u ? CurrentLightSet().sun.color : sceneparam.sunColor;
}

vec4 SunSpecColor()
{
	return g_lightSet != 0u ? CurrentLightSet().sun.specColor : sceneparam.sunSpecColor;
}

// One of the model set's four lights
mapDirectionalLight ModelLight(uint i)
{
	return g_lightSet != 0u ? CurrentLightSet().modelLights[i] : sceneparam.modelLights[i];
}

// The map's GPARAM hemisphere light arriving from direction D: its up color from above, its down color from below
// Characters take the model set's hemisphere; a part with its own lighting set takes that set's
vec3 Hemisphere(vec3 D)
{
	vec3 up;
	vec3 down;

	if (g_lightSet != 0u)
	{
		lightSetData set = CurrentLightSet();
		up = g_modelLit ? set.modelHemiUp.rgb : set.hemiUp.rgb;
		down = g_modelLit ? set.modelHemiDown.rgb : set.hemiDown.rgb;
	}
	else
	{
		up = g_modelLit ? sceneparam.modelHemiUp.rgb : sceneparam.hemiUp.rgb;
		down = g_modelLit ? sceneparam.modelHemiDown.rgb : sceneparam.hemiDown.rgb;
	}

	return mix(down, up, clamp(D.y * 0.5 + 0.5, 0.0, 1.0));
}

// Whether the current material is a character's lit by the model set of the map's GPARAM LightSet: its own sun,
// the fill lights, and its hemisphere. The flag is above the first UV set, see Material.ModelLightingFlag.
bool ModelLit()
{
	return MapLighting() && (sceneparam.mapLightingFlags & 4u) != 0u && (MaterialValue(ROW_UVSET, 0) & 0x100u) != 0u;
}

// Ambient light from direction N where there are no volumes: the map's hemisphere when its GPARAMs light the
// scene, otherwise the environment map
vec3 AmbientBase(vec3 N)
{
	if (MapLighting())
		return Hemisphere(N);

	return textureLod(
		samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler),
		vec3(N * vec3(1, 1, -1)), 5).xyz;
}

#ifdef MATERIAL_BLEND
// The scale on a DES g_Lightmap's light, see IndirectLight. 1 until matched against the game
#define DES_LIGHTMAP_SCALE 1.0

// fsin_lightmap of a lightmapped material whose part found no lightmap: InstanceData.MissingLightmap plus 1
#define LIGHTMAP_MISSING 0x40000000u

// The light of a DS3 map piece's lightmap at the pixel, as the game's GXFlver_*DOL* shaders read it (DS3 DXBC): DOL 1's
// color squared, times its HDR scale and the GPARAM's Dominant Light Map color; DOL 2's direction (the model's space,
// x 2 - 1) and its sharpness plus the GPARAM's offset, which splits the light into a part from that direction and a
// part from all around. DOL 1's alpha is how much of the sun reaches here, which the game scales the sun by.
vec3 LightmapIrradiance(vec3 N, out float sunMask)
{
	lightmapData lm = lightmapdata[sceneparam.lightmapStart + fsin_lightmap - 1u];
	lightmapData globals = lightmapdata[sceneparam.lightmapStart];

	vec2 uv = g_uv[1] * lm.uvScaleOffset.xy + lm.uvScaleOffset.zw;

	vec4 dol1 = texture(sampler2D(globalTextures[nonuniformEXT(TextureIndex(lm.textures.x))], linearSampler), uv);
	vec4 dol2 = texture(sampler2D(globalTextures[nonuniformEXT(TextureIndex(lm.textures.y))], linearSampler), uv);

	sunMask = clamp(dol1.a, 0.0, 1.0);

	vec3 color = dol1.rgb * dol1.rgb * lm.hdrScale.rgb * globals.hdrScale.rgb;

	vec3 dir = fsin_modelToWorld * (dol2.rgb * 2.0 - 1.0);
	dir = dot(dir, dir) > 1e-8 ? normalize(dir) : N;

	float sharpness = clamp(dol2.a + globals.uvScaleOffset.x, 0.0, 1.0);

	vec3 E = color * sharpness * clamp(dot(dir, N), 0.0, 1.0) + color * (1.0 - sharpness);
	return E * sceneparam.irradianceVolumeExposure;
}
#endif

// Light from the environment map or the map's hemisphere, or the irradiance volumes where there are some
vec3 IndirectLight(Surface s)
{
	vec3 ambientDiffuse = AmbientBase(s.N);

	// Where there are volumes, reflections are scaled by how bright the volumes' light is there compared with
	// their average, so they darken in shadow; reflectionScale is already 1 outside every volume
	float reflectionScale = 1.0;

	if (sceneparam.irradianceVolumeCount > 0u)
	{
		float coverage;
		vec3 volumeLight = VolumeIrradiance(fsin_worldpos, s.N, coverage, reflectionScale);

		// Unknown whether the game adds the hemisphere inside the volumes too, so it is a choice
		if (MapLighting() && (sceneparam.mapLightingFlags & 2u) != 0u)
			volumeLight += Hemisphere(s.N);

		ambientDiffuse = mix(ambientDiffuse, volumeLight, coverage);

		if (coverage > 0.5)
			g_lightSource = 1u;

		if ((sceneparam.irradianceVolumeFlags & 1u) == 0u)
			reflectionScale = 1.0;
	}

#ifdef MATERIAL_BLEND
	// A DS3 map piece's lightmap is all of its light from around, in place of the probes or the hemisphere, and
	// its shadow of the sun replaces theirs
	if (fsin_lightmap == LIGHTMAP_MISSING)
	{
		g_lightSource = 3u;
	}
	else if (fsin_lightmap != 0u)
	{
		float sunMask;
		ambientDiffuse = LightmapIrradiance(s.N, sunMask);
		g_probeSunMask = sunMask;
		g_lightSource = 2u;

		// Reflections follow how bright the lightmap is against the hemisphere they stand in for
		reflectionScale = 1.0;
		if (MapLighting() && (sceneparam.irradianceVolumeFlags & 1u) != 0u)
		{
			float hemi = dot(Hemisphere(s.N), vec3(0.299, 0.587, 0.114));
			reflectionScale = clamp(dot(ambientDiffuse, vec3(0.299, 0.587, 0.114)) / max(hemi, 1e-4), 0.0, 2.0);
		}
	}

	// A DES map piece's g_Lightmap, a texture of its own material read through the second UV set: its baked light
	// stands in for the light from around. Squared from its stored gamma, as DS3's DOL 1 is; the game's own scale
	// isn't traced, so it is DES_LIGHTMAP_SCALE until matched by eye. 0 until the texture is on the GPU
	if (c_gameID == GAME_DES)
	{
		uint desLightmap = MaterialValue(ROW_EXTRA, 2);
		if (desLightmap != 0u)
		{
			vec3 lm = texture(sampler2D(globalTextures[nonuniformEXT(TextureIndex(desLightmap))], linearSampler), g_uv[1]).rgb;
			ambientDiffuse = lm * lm * DES_LIGHTMAP_SCALE;
			g_lightSource = 2u;
		}
	}
#endif

	g_ambientRaw = ambientDiffuse;
	ambientDiffuse *= sceneparam.ambientLightMult;

	// traditional phong model
	if (c_gameID == GAME_DES || c_gameID == GAME_DS1_PTDE)
	{
		g_indirectDiffuse = s.diffuse * ambientDiffuse;
		return g_indirectDiffuse;
	}

	// PBR model
	float envMip = min(6.0, -(1.0 - s.roughness) * 6.5 + 6.5);
	vec3 reflectVec = reflect(-s.V, s.N);
	vec3 ambientSpec;

	if (MapLighting())
	{
		// The hemisphere stands in for the map's environment maps, which aren't loaded, with the GPARAM's
		// color conversion of them: towards grey by the saturation, then scaled
		ambientSpec = Hemisphere(reflectVec);
		float luminance = dot(ambientSpec, vec3(0.299, 0.587, 0.114));
		ambientSpec = mix(vec3(luminance), ambientSpec, sceneparam.envMapSaturate) * sceneparam.envMapScale.rgb;
	}
	else
	{
		ambientSpec = textureLod(
			samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler),
			vec3(reflectVec * vec3(1, 1, -1)), envMip).xyz;
	}

	ambientSpec *= sceneparam.ambientLightMult * reflectionScale;

	float NdotV = max(abs(clamp(dot(s.V, s.N), 0.0, 1.0)), Epsilon);
	float gloss = 1.0 - s.roughness;
	vec3 aF = pow(1.0 - NdotV, 5.0) * gloss * gloss * (1.0 - s.F0) + s.F0;

	vec3 indirectDiffuse = s.diffuse * ambientDiffuse * (1.0 - s.F0);
	g_indirectDiffuse = indirectDiffuse;
	vec3 indirectSpecular = ambientSpec * aF;
	float reflectionThing = clamp(dot(reflectVec, s.N) + 1.0, 0.0, 1.0);
	indirectSpecular *= reflectionThing * reflectionThing;
	g_indirectSpecular = indirectSpecular;

	return indirectDiffuse + indirectSpecular;
}

// ---------------------------------------------------------------------------------------------
// Water
// ---------------------------------------------------------------------------------------------

// How deep water is taken to be where the scene behind it can't be read, in meters
#define WATER_ASSUMED_DEPTH 2.0
#define WATER_OPAQUE_DEPTH 10000.0

// The share of the light that scattering sends back up: the shaders' 1 / (4 pi), times the pi lighting leaves out here
#define WATER_SCATTER_PHASE 0.25

// ---------------------------------------------------------------------------------------------
// Fog on transparent draws
// ---------------------------------------------------------------------------------------------

// The volumetric fog between the camera and this transparent pixel, as VolumetricFog.frag finds it for the opaque
// scene: the froxel volume at the pixel's own depth, and past the volume's far depth the same fog marched out to the
// pixel. rgb: light scattered towards the camera, a: the share of the pixel let through.

float FogDensity(float y)
{
	float height = clamp(-sceneparam.fogDensity.z * (y - sceneparam.fogDensity.w), -80.0, 80.0);
	return clamp(sceneparam.fogDensity.x + sceneparam.fogDensity.y * exp(height), 0.0, 4.0);
}

float FogPhase(float cosAngle, float g)
{
	float gg = g * g;
	float peak = (1.0 - gg) / pow(max(1.0 - g, 1e-3), 3.0);
	return (1.0 - gg) / pow(max(1.0 + gg - 2.0 * g * cosAngle, 1e-6), 1.5) / peak;
}

float FogHeightAttenuation(float y, float attenuation)
{
	return exp(-max(attenuation, 0.0) * max(y - sceneparam.fogDensity.w, 0.0));
}

#define FOG_FAR_STEPS 8

vec4 FogAtPixel(vec3 worldPos)
{
	uint index = sceneparam.fogVolumeTexture;
	if (index == 0u || index >= 2048u)
		return vec4(0.0, 0.0, 0.0, 1.0);

	float depth = abs((sceneparam.view * vec4(worldPos, 1.0)).z);
	float far = sceneparam.fogRange.x;
	float slices = max(sceneparam.fogSlices, 1.0);

	// The volume at min(depth, far): slice i holds the fog up to its far side, faded in from the camera before the
	// first. The sampler wraps, so the coordinates are kept half a texel inside.
	float s = log(max(min(depth, far), sceneparam.fogNear) / sceneparam.fogNear) / max(sceneparam.fogLogRatio, 1e-6) * slices;
	float sliceIndex = clamp(s - 1.0, 0.0, slices - 1.0);

	vec3 size = vec3(textureSize(sampler3D(globalTexturesVolume[nonuniformEXT(index)], linearSampler), 0));
	vec3 uvw = vec3(gl_FragCoord.xy / max(sceneparam.fogRange.zw, vec2(1.0)), (sliceIndex + 0.5) / slices);
	uvw = clamp(uvw, 0.5 / size, 1.0 - 0.5 / size);

	vec4 fog = textureLod(sampler3D(globalTexturesVolume[nonuniformEXT(index)], linearSampler), uvw, 0.0);
	fog = s < 1.0 ? mix(vec4(0.0, 0.0, 0.0, 1.0), fog, clamp(s, 0.0, 1.0)) : fog;

	// With EnableAnalyticalFog, marched on from the far depth to the pixel, lit by the sky and the unshadowed sun
	float end = sceneparam.fogRange.y;
	if (end > far && depth > far)
	{
		vec3 eye = sceneparam.eye.xyz;
		vec3 toPixel = worldPos - eye;
		vec3 rayDir = normalize(toPixel);
		float rayScale = length(toPixel) / max(depth, 1e-4);

		vec3 sunTravel = dot(sceneparam.fogSun.xyz, sceneparam.fogSun.xyz) > 1e-10
			? normalize(sceneparam.fogSun.xyz) : vec3(0.0, -1.0, 0.0);
		vec3 sunColor = mix(sceneparam.fogOpposite.rgb, sceneparam.fogDirect.rgb,
			FogPhase(dot(rayDir, -sunTravel), sceneparam.fogOpposite.w));

		float to = min(depth, end);
		float stepDepth = (to - far) / float(FOG_FAR_STEPS);
		vec3 scattered = vec3(0.0);
		float transmittance = 1.0;

		for (int i = 0; i < FOG_FAR_STEPS; i++)
		{
			float d = far + (float(i) + 0.5) * stepDepth;
			vec3 p = eye + toPixel * (d / max(depth, 1e-4));
			float density = FogDensity(p.y);

			vec3 light = sceneparam.fogSky.rgb * FogHeightAttenuation(p.y, sceneparam.fogSky.w) +
				sunColor * FogHeightAttenuation(p.y, sceneparam.fogDirect.w);

			float extinction = density * (1.0 + sceneparam.fogExtinction.x);
			float stepTransmittance = exp(-extinction * stepDepth * rayScale);
			scattered += transmittance * light * density * (1.0 - stepTransmittance) / max(extinction, 1e-6);
			transmittance *= stepTransmittance;
		}

		fog = vec4(fog.rgb + fog.a * scattered, fog.a * transmittance);
	}

	return fog;
}

bool IsWater()
{
	return c_surface == SURFACE_WATER && HasFeature(FEATURE_WATER);
}

// The finer ripples, on top of the first normal, whose blue bends them. Only the first normal's scroll is in its row
vec2 WaterRipples(float height)
{
	waterDetailData water = waterdetaildata[g_mat + 11u];

	vec2 scroll = HasFeature(FEATURE_UV_SCROLL) ? water.tiling.zw * sceneparam.materialTime : vec2(0.0);
	vec2 uv = (g_uv[0] + scroll + water.bend.xy * clamp(height, 0.0, 1.0)) * water.tiling.xy;

	vec2 ripple = textureGrad(
		sampler2D(globalTextures[nonuniformEXT(TextureIndex(MaterialValue(ROW_NORMAL, 1)))], anisoLinearSampler),
		uv, g_dx[0] * water.tiling.xy, g_dy[0] * water.tiling.xy).xy;

	return (ripple - 0.5) * water.bend.z;
}

// Distance from the camera along its axis of a value in the depth buffer
float ViewDepth(float depth)
{
	mat4 projection = sceneparam.projection;

	// Orthographic cameras keep w at 1
	if (abs(projection[2][3]) < 1e-6)
	{
		return abs((depth - projection[3][2]) / projection[2][2]);
	}

	return abs(projection[3][2] / (depth * projection[2][3] - projection[2][2]));
}

// How much water the view passes through before it reaches the scene. Only transparent
// draws can ask, as the depth is copied after everything opaque has been drawn
float WaterThickness()
{
	if (!c_transparent)
	{
		return WATER_OPAQUE_DEPTH;
	}

	if (sceneparam.sceneDepthTexture == 0u)
	{
		return WATER_ASSUMED_DEPTH;
	}

	// Kept within the texture bound, which can be the 1x1 default or an older, smaller copy, see ShadowCompare
	ivec2 size = textureSize(
		sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], linearSampler), 0);
	float stored = texelFetch(
		sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], linearSampler),
		clamp(ivec2(gl_FragCoord.xy), ivec2(0), size - 1), 0).r;

	return max(ViewDepth(stored) - ViewDepth(gl_FragCoord.z), 0.0);
}

// The color and alpha of water, from what its surface reflects. Blending can only let the same amount of each color
// through, so the amount of the color that gets through the least is left to it, and the rest of what gets
// through is added here. That can't know what is behind the water, so it takes the sky's light to be close enough
vec4 WaterVolume(Surface s, vec3 reflection, float thickness)
{
	waterDetailData detail = waterdetaildata[g_mat + 11u];
	waterVolumeData volume = watervolumedata[g_mat + 12u];

	vec3 scatter = volume.scatter.rgb;
	vec3 extinction = scatter + volume.absorption.rgb;
	vec3 transmitted = exp(-extinction * thickness);

	float NdotV = clamp(dot(s.N, s.V), 0.0, 1.0);
	float fresnel = 0.02 + 0.98 * pow(1.0 - NdotV, 5.0);

	vec3 sky = AmbientBase(s.N) * sceneparam.ambientLightMult * sceneparam.indirectLightMult;

	vec3 sunDir = SunTravel();
	vec4 sunColor = SunColor();
	vec3 sun = sunColor.rgb * sunColor.a * sceneparam.directLightMult
		* clamp(dot(s.N, -sunDir), 0.0, 1.0);

	vec3 light = (sky + sun) * sceneparam.sceneBrightness;

	vec3 albedo = detail.tint.rgb * scatter / max(extinction, vec3(1e-4));
	vec3 scattered = albedo * light * WATER_SCATTER_PHASE * (1.0 - transmitted);

	vec3 through = transmitted * (1.0 - fresnel);
	float blended = min(through.r, min(through.g, through.b));

	vec3 color = reflection + scattered * (1.0 - fresnel) + (through - blended) * sky * sceneparam.sceneBrightness;

	float alpha = max(1.0 - blended, 0.001);

	return vec4(color / alpha, alpha);
}

// Water thins out to nothing where it meets the shore
float WaterEdge(float thickness)
{
	return clamp(thickness * watervolumedata[g_mat + 12u].scatter.w, 0.0, 1.0);
}

// 4x4 Bayer ordered dither, from 0 up to 1
float OrderedDither()
{
	mat4 bayerMatrix = mat4(
		 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
		12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
		 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
		15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
	);

	ivec2 pixelCoord = ivec2(gl_FragCoord.xy) % 4;
	return bayerMatrix[pixelCoord.x][pixelCoord.y];
}

bool IsFur()
{
	return c_surface == SURFACE_FUR && HasFeature(FEATURE_FUR);
}

// 1 if the shadow map texel is no nearer the sun than depth, kept within the cascade's square of the map
float ShadowCompare(ivec2 texel, ivec2 low, ivec2 high, float depth)
{
	// Also kept within the texture bound. Its slot can hold the 1x1 default until the map's descriptor is written,
	// and fetching outside an image reads past it, which loses the device
	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(sceneparam.shadowTexture)], linearSampler), 0);
	texel = clamp(clamp(texel, low, high), ivec2(0), size - 1);
	float stored = texelFetch(sampler2D(globalTextures[nonuniformEXT(sceneparam.shadowTexture)], linearSampler),
		texel, 0).r;

	return depth >= stored ? 1.0 : 0.0;
}

// 1 if the rain map's texel is no higher than depth: nothing above the point
float RainCompare(ivec2 texel, float depth)
{
	// Kept within the texture bound as well as the map, see ShadowCompare
	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(sceneparam.rainTexture)], linearSampler), 0);
	ivec2 last = min(ivec2(int(sceneparam.rainMapSize) - 1), size - 1);
	texel = clamp(texel, ivec2(0), last);
	float stored = texelFetch(sampler2D(globalTextures[nonuniformEXT(sceneparam.rainTexture)], linearSampler),
		texel, 0).r;

	return depth >= stored ? 1.0 : 0.0;
}

// How much rain reaches a point, from 0 under cover to 1 in the open, from the scene's depth seen from above (see
// RainOcclusion.cs). The point is moved out along its face by a texel and a half, so a wall reads the ground in front of
// it rather than its own top, and half a meter up, plus more on slopes, so a surface doesn't cover itself. Filtered over
// 2x2 texels, and faded to 1 (open) over the last tenth of the square, past which nothing is known.
float RainExposure(vec3 worldPos, vec3 faceNormal)
{
	if (sceneparam.rainTexture == 0u)
	{
		return 1.0;
	}

	vec3 offsetPos = worldPos + faceNormal * sceneparam.rainTexelSize * 1.5;
	vec3 map = (sceneparam.rainMatrix * vec4(offsetPos, 1.0)).xyz;
	float edge = max(abs(map.x), abs(map.y));

	// Outside the square, or above or below the height it covers
	if (edge >= 1.0 || map.z > 1.0 || map.z < 0.0)
	{
		return 1.0;
	}

	float up = clamp(faceNormal.y, 0.0, 1.0);
	float slope = clamp(sqrt(1.0 - up * up) / max(up, 0.05), 0.0, 8.0);
	float depth = map.z + (0.5 + sceneparam.rainTexelSize * slope) * sceneparam.rainDepthScale;

	vec2 coord = (map.xy * 0.5 + 0.5) * sceneparam.rainMapSize - 0.5;
	ivec2 base = ivec2(floor(coord));
	vec2 f = coord - vec2(base);

	float exposed = mix(
		mix(RainCompare(base, depth), RainCompare(base + ivec2(1, 0), depth), f.x),
		mix(RainCompare(base + ivec2(0, 1), depth), RainCompare(base + ivec2(1, 1), depth), f.x),
		f.y);

	return mix(exposed, 1.0, smoothstep(0.9, 1.0, edge));
}

// How lit a point is in one cascade, from 0 in shadow to 1. Filtered over a 3x3 texel tent, made of 4x4 fetches
// weighted by where the point falls between texels, so the edges are smooth however the camera moves.
float SampleCascade(int cascade, vec3 worldPos, float bias)
{
	vec4 clip = sceneparam.shadowMatrices[cascade] * vec4(worldPos, 1.0);
	vec3 ndc = clip.xyz / clip.w;

	// Outside the cascade, or past where it reaches towards the sun, nothing is known to shade the point
	if (any(greaterThan(abs(ndc.xy), vec2(1.0))) || ndc.z > 1.0)
	{
		return 1.0;
	}

	int size = int(sceneparam.shadowMapSize) / 2;
	ivec2 low = ivec2(cascade % 2, cascade / 2) * size;
	ivec2 high = low + size - 1;

	vec2 coord = (ndc.xy * 0.5 + 0.5) * float(size) + vec2(low) - 0.5;
	ivec2 base = ivec2(floor(coord));
	vec2 f = coord - vec2(base);

	float wx[4] = float[](1.0 - f.x, 1.0, 1.0, f.x);
	float wy[4] = float[](1.0 - f.y, 1.0, 1.0, f.y);
	float depth = ndc.z + bias;
	float lit = 0.0;

	for (int y = 0; y < 4; y++)
	{
		for (int x = 0; x < 4; x++)
		{
			lit += wx[x] * wy[y] * ShadowCompare(base + ivec2(x - 1, y - 1), low, high, depth);
		}
	}

	return lit / 9.0;
}

// The point in one cascade, moved off its surface by the GPARAM's bias, here in texels of the cascade: the depth bias
// along the normal, and the slope scaled bias towards the sun, more where the sun grazes the surface
float ShadowInCascade(int cascade, vec3 worldPos, vec3 geometryNormal, float slope)
{
	float texel = sceneparam.shadowTexelSizes[cascade];
	vec3 offsetPos = worldPos + geometryNormal * texel * 0.5 * sceneparam.shadowDepthBias;
	float bias = texel * 0.5 * sceneparam.shadowSlopeBias * slope * sceneparam.shadowDepthScales[cascade];

	return SampleCascade(cascade, offsetPos, bias);
}

// How much of the sun reaches a point, from 1 - shadowStrength in shadow to 1. sunTravel is the direction the
// sun's light travels in; geometryNormal faces the side being drawn.
float SunShadow(vec3 worldPos, vec3 geometryNormal, vec3 sunTravel)
{
	if (sceneparam.shadowTexture == 0u || sceneparam.shadowCascadeCount == 0u || (fsin_flags & INSTANCE_NO_SHADOW) != 0u)
	{
		return 1.0;
	}

	float viewDepth = (sceneparam.view * vec4(worldPos, 1.0)).z;

	if (viewDepth >= sceneparam.shadowFadeEnd)
	{
		return 1.0;
	}

	int count = int(min(sceneparam.shadowCascadeCount, 4u));
	int cascade = count - 1;

	for (int i = 0; i < count; i++)
	{
		if (viewDepth < sceneparam.shadowSplits[i])
		{
			cascade = i;
			break;
		}
	}

	float NdotL = clamp(dot(geometryNormal, -sunTravel), 0.0, 1.0);
	float slope = clamp(sqrt(1.0 - NdotL * NdotL) / max(NdotL, 0.05), 0.0, 8.0);

	float lit = ShadowInCascade(cascade, worldPos, geometryNormal, slope);

	// The end of each cascade's range blends into the next, so the change in detail doesn't show as a line
	if (cascade + 1 < count && sceneparam.shadowCascadeFade > 0.0)
	{
		float start = cascade > 0 ? sceneparam.shadowSplits[cascade - 1] : 0.0;
		float end = sceneparam.shadowSplits[cascade];
		float blendStart = end - (end - start) * sceneparam.shadowCascadeFade;

		if (viewDepth > blendStart)
		{
			float next = ShadowInCascade(cascade + 1, worldPos, geometryNormal, slope);
			lit = mix(lit, next, clamp((viewDepth - blendStart) / max(end - blendStart, 1e-4), 0.0, 1.0));
		}
	}

	lit = mix(lit, 1.0, smoothstep(sceneparam.shadowFadeStart, sceneparam.shadowFadeEnd, viewDepth));

	return mix(1.0, lit, sceneparam.shadowStrength);
}

void main()
{
#ifdef OUTLINE
	fsout_color = sceneparam.outlineColor;
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
	return;
#endif

	g_mat = fsin_mat;
	g_modelLit = ModelLit();
	g_lightSet = LightSet();

	vec3 viewVec = normalize(sceneparam.eye.xyz - fsin_worldpos);

	vec3 sunDir = SunTravel();

	// Texture coordinates and their derivatives, taken before any branching
	g_uv[0] = fsin_texcoord;
#ifdef MATERIAL_BLEND
	g_uv[1] = fsin_texcoord2;
#else
	g_uv[1] = fsin_texcoord;
#endif
	for (int i = 0; i < 2; i++)
	{
		g_dx[i] = dFdx(g_uv[i]);
		g_dy[i] = dFdy(g_uv[i]);
	}

#ifdef MATERIAL_BLEND
	ComputeLayerWeights();
	vec4 diffuseColor = SampleLayered(ROW_ALBEDO, true);
#else
	vec4 diffuseColor = SampleLayer(ROW_ALBEDO, 0);
#endif

	vec4 diffuseAlpha = shadingdata[g_mat + 1u].diffuseAlpha;

#ifdef MATERIAL_BLEND
	// The shape of fur comes from its strands alone, which are as dark on average as the overlays below
	if (IsFur())
	{
		vec4 strands = SampleLayer(ROW_ALBEDO, 1);
		diffuseColor.rgb = clamp(diffuseColor.rgb * strands.rgb * 4.55, 0.0, 1.0);
		diffuseColor.w = strands.w * fsin_color.a
			+ (OrderedDither() - 0.5) * effectsdata[g_mat + 2u].emissiveColor.w;
	}
#endif

	// These textures average about 0.22, so 4.55 leaves the albedo as it was on average
	if (c_detail && HasFeature(FEATURE_OVERLAYS) && detaildata[g_mat + 8u].textures.x != 0u)
	{
		detailData overlay = detaildata[g_mat + 8u];
		vec3 overlayColor = SampleAt(overlay.textures.x, 0, overlay.tiling.xy, vec2(0.0)).rgb;
		diffuseColor.rgb = clamp(diffuseColor.rgb * overlayColor * 4.55 * overlay.colorStrength.rgb, 0.0, 1.0);
	}

	if (diffuseColor.w < diffuseAlpha.w)
	{
		discard;
	}

	// The shadow map has no color, so all it needs is which pixels the alpha test keeps
	if (c_shadowPass)
	{
		fsout_color = vec4(0.0);
		return;
	}

#ifdef SELECTED
	if (sceneparam.EnableDithering == 1)
	{
		if (sceneparam.ditherOpacity < OrderedDither())
		{
			discard;
		}
	}
#endif

	// Do picking after discard
	if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}

#ifdef FOCUS
	// After picking, so the gaps in the dither can still be clicked
	if (FOCUS_OPACITY < OrderedDither())
	{
		discard;
	}
#endif

#ifdef MATERIAL_BLEND
	vec3 normalColor = SampleLayered(ROW_NORMAL, c_blendNormal).xyz;
	vec3 F0 = SampleLayered(ROW_SPECULAR, c_blendSpecular).xyz;

	// Each strand bends the normal underneath it, and knows how glossy it is. Index 1 is the default normal,
	// which isn't quite flat: most fur only has the strands' normals, so that would lean all of them over
	if (IsFur())
	{
		if (MaterialValue(ROW_NORMAL, 0) == 1u)
		{
			normalColor.xy = vec2(0.5);
		}

		if (MaterialValue(ROW_NORMAL, 1) != 1u)
		{
			vec3 strandNormal = SampleLayer(ROW_NORMAL, 1).xyz;
			normalColor.xy += strandNormal.xy - 0.5;
			normalColor.z = strandNormal.z;
		}
	}
#else
	vec3 normalColor = SampleLayer(ROW_NORMAL, 0).xyz;
	vec3 F0 = SampleLayer(ROW_SPECULAR, 0).xyz;
#endif

	bool volumeWater = false;

	if (IsWater())
	{
		normalColor.xy += WaterRipples(normalColor.z);
		normalColor.z = waterdetaildata[g_mat + 11u].bend.w;
		F0 = vec3(0.02);
		volumeWater = waterdetaildata[g_mat + 11u].tint.w > 0.5;
	}

	float roughness;
	if (c_gameID == GAME_BLOODBORNE || c_gameID == GAME_DS1_PTDE || c_gameID == GAME_DS2)
	{
#ifdef MATERIAL_BLEND
		vec3 shininessColor = SampleLayered(ROW_SHININESS, c_blendShininess).xyz;
#else
		vec3 shininessColor = SampleLayer(ROW_SHININESS, 0).xyz;
#endif
		roughness = 1.0 - shininessColor.r;
	}
	else
	{
		roughness = 1.0 - normalColor.z;
	}
	roughness = clamp(roughness, 0.0, 1.0);

	vec3 normalMap;
	normalMap.xy = normalColor.xy * 2.0 - 1.0;

	if (c_detail && HasFeature(FEATURE_OVERLAYS) && detaildata[g_mat + 8u].textures.y != 0u)
	{
		detailData overlay = detaildata[g_mat + 8u];
		vec2 overlayNormal = SampleAt(overlay.textures.y, 0, overlay.tiling.zw, vec2(0.0)).xy * 2.0 - 1.0;
		normalMap.xy += overlayNormal * overlay.colorStrength.w;
	}

	normalMap.z = sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
	normalMap = normalize(normalMap);
	normalMap = normalize(fsin_worldToTangent * normalMap);

	// In rain, what faces the sky darkens and turns glossier, walls half as much, and what faces down not at all.
	// What is under a roof stays dry, by the rain map.
	vec3 albedo = diffuseColor.xyz * diffuseAlpha.xyz;
	if (sceneparam.wetAlbedo.a > 0.0 || sceneparam.wetShininess.y > 0.0)
	{
		vec3 faceNormal = normalize(fsin_worldToTangent[2]) * (gl_FrontFacing ? 1.0 : -1.0);
		float exposed = (IsWater() || (c_surface == SURFACE_UNLIT && HasFeature(FEATURE_UNLIT)))
			? 0.0 : smoothstep(-0.3, 0.3, faceNormal.y);

		if (exposed > 0.0)
		{
			exposed *= RainExposure(fsin_worldpos, faceNormal);
		}

		albedo = mix(albedo, sceneparam.wetAlbedo.rgb, sceneparam.wetAlbedo.a * exposed);
		roughness = 1.0 - mix(1.0 - roughness, clamp(sceneparam.wetShininess.x, 0.0, 1.0),
			sceneparam.wetShininess.y * exposed);
	}

	Surface surface;
	// The color of water comes from its volume, so all that is lit here is what its surface reflects
	surface.diffuse = volumeWater ? vec3(0.0) : albedo;
	surface.F0 = F0;
	surface.roughness = roughness;
	surface.N = (gl_FrontFacing ? normalMap : -normalMap);
	surface.V = viewVec;

	// The sun's shadow is found with the mesh's own normal, which the normal map doesn't bend, facing the side drawn
	vec3 geometryNormal = normalize(fsin_worldToTangent[2]);
	geometryNormal = gl_FrontFacing ? geometryNormal : -geometryNormal;
	float sunShadow = SunShadow(fsin_worldpos, geometryNormal, sunDir);

	// The indirect light first, as DS3 and Bloodborne's light probes found with it also say how much of the sun
	// reaches here (their LightMask, as the game's shaders scale the sun by ShLightMask)
	vec3 indirect = IndirectLight(surface);
	sunShadow *= g_probeSunMask;

	// The sun, then every light in the scene
	vec3 direct = vec3(0.0);

	if (g_modelLit)
	{
		// The model set's sun and fill lights. Its sun is the one that casts the shadows.
		for (uint i = 0u; i < 4u; i++)
		{
			mapDirectionalLight light = ModelLight(i);
			if (light.color.a <= 0.0 && light.specColor.a <= 0.0)
				continue;

			vec3 travel = light.direction.xyz;
			travel = dot(travel, travel) > 1e-10 ? normalize(travel) : vec3(0.0, -1.0, 0.0);
			direct += ShadeLight(surface, -travel, light.color.rgb * light.color.a, light.specColor.rgb * light.specColor.a) *
				(i == 0u ? sunShadow : 1.0);
		}
	}
	else
	{
		vec4 sunColor = SunColor();
		vec4 sunSpecColor = SunSpecColor();
		direct = ShadeLight(surface, -sunDir, sunColor.rgb * sunColor.a, sunSpecColor.rgb * sunSpecColor.a) * sunShadow;
	}

	uint lightCount = min(sceneparam.lightCount, uint(MAX_SCENE_LIGHTS));
	for (uint i = 0; i < lightCount; i++)
	{
		vec3 L;
		vec3 radiance = LightRadiance(sceneparam.lights[i], fsin_worldpos, L);
		if (dot(radiance, radiance) > 0.0)
		{
			direct += ShadeLight(surface, L, radiance, radiance);
		}
	}

	// The map's lights near the camera. Most are out of range of any given pixel, so only their position and
	// range are read for those.
	for (uint i = 0u; i < sceneparam.mapLightCount; i++)
	{
		uint index = sceneparam.mapLightStart + i;
		vec4 positionRange = maplightdata[index].light.positionRange;
		vec3 toLight = positionRange.xyz - fsin_worldpos;

		if (positionRange.w > 0.0 && dot(toLight, toLight) > positionRange.w * positionRange.w &&
			int(maplightdata[index].light.directionType.w + 0.5) != LIGHT_DIRECTIONAL)
			continue;

		// Copied out, as the buffer's readonly entries can't be passed to a function
		mapLight light = maplightdata[index];

		vec3 L;
		float attenuation = LightAttenuation(light.light, fsin_worldpos, L);
		if (attenuation > 0.0)
		{
			vec3 radiance = light.light.colorIntensity.rgb * light.light.colorIntensity.a * attenuation;
			vec3 specularRadiance = light.specular.rgb * light.specular.a * attenuation;
			direct += ShadeLight(surface, L, radiance, specularRadiance);
		}
	}

	vec3 litColor = (direct * sceneparam.directLightMult + indirect * sceneparam.indirectLightMult) *
		sceneparam.sceneBrightness;

	// The lighting debug view, see SceneParam.LightingDebugView
	if (sceneparam.lightingDebugView == 1u)
	{
		const vec3 sources[4] = vec3[4](vec3(1.0, 0.1, 0.1), vec3(0.15, 0.35, 1.0), vec3(0.1, 1.0, 0.1), vec3(1.0, 0.9, 0.0));
		// Shaded a little by facing, so the shapes still read
		litColor = sources[min(g_lightSource, 3u)] * (0.35 + 0.65 * clamp(surface.N.y * 0.5 + 0.5, 0.0, 1.0));
	}
	else if (sceneparam.lightingDebugView == 2u)
	{
		litColor = g_ambientRaw;
	}
	else if (sceneparam.lightingDebugView == 3u)
	{
		litColor = vec3(g_probeSunMask);
	}

	// Editor mode's fullbright, see RenderMode: the albedo as it is, lit by nothing
	bool fullbright = sceneparam.lightingDebugView == 4u;
	if (fullbright)
	{
		litColor = albedo;
	}

	if (c_surface == SURFACE_UNLIT && HasFeature(FEATURE_UNLIT))
	{
		litColor = diffuseColor.rgb * diffuseAlpha.rgb * sceneparam.sceneBrightness * sceneparam.emissiveMapMult;
	}

	float alpha = c_transparent ? diffuseColor.w : 1.0;

	if (IsWater())
	{
		float thickness = WaterThickness();

		if (volumeWater)
		{
			vec4 water = WaterVolume(surface, litColor, thickness);
			litColor = water.rgb;
			alpha = c_transparent ? water.a : 1.0;
		}

		if (c_transparent)
		{
			alpha *= WaterEdge(thickness);
		}
	}

	// What the surface gives off itself, kept apart for the glow filter as well as added to the color.
	// Materials without an emissive texture have the black default, which is index 2.
	vec3 glow = vec3(0.0);

	// Fullbright leaves it out, as it would only wash out the albedo with nothing to tone map it
	uint emissiveTex = MaterialValue(ROW_EXTRA, 1);
	if (emissiveTex != 2u && HasFeature(FEATURE_EMISSIVE) && !fullbright)
	{
		effectsData effects = effectsdata[g_mat + 2u];
		vec3 emissive = SampleAt(emissiveTex, 0, effects.emissiveTiling.xy, effects.emissiveTiling.zw).rgb;
		glow += emissive * effects.emissiveColor.xyz * sceneparam.emissiveMapMult;
	}

	if (c_magma && HasFeature(FEATURE_MAGMA) && !fullbright)
	{
		glow += MagmaGlow() * sceneparam.emissiveMapMult;
	}

	litColor += glow;

#ifdef SELECTED
	if (sceneparam.EnableTinting == 1)
	{
		vec3 tint = sceneparam.selectionColor.rgb;
		float strength = sceneparam.selectionColor.a;
		litColor = mix(litColor, litColor * tint, strength);
	}
#endif

	// Transparents are drawn over the fogged scene, so they fog themselves to their own depth. Alpha blending
	// lets the fog's light through by the alpha, as the scene behind has it; an additive or subtractive one only
	// dims, as its light would be added again on each layer.
	if (c_transparent && c_linearOutput && sceneparam.fogVolumeTexture != 0u)
	{
		vec4 fog = FogAtPixel(fsin_worldpos);
		litColor = max(litColor, vec3(0.0)) * fog.a + (c_blending == BLENDING_ALPHA ? fog.rgb : vec3(0.0));
	}

	vec3 finalColor = sqrt(max(litColor, vec3(0.0)));

#ifdef FOCUS
	finalColor = mix(vec3(dot(finalColor, vec3(0.299, 0.587, 0.114))), vec3(0.5), 0.5);
#endif

	// In the HDR target the tone map applies the square root, so it is undone: what is left is linear light, kept
	// above 1 for the tone map to bring down
	if (c_linearOutput)
	{
		finalColor *= finalColor;
	}

	fsout_color = vec4(finalColor, alpha);

	// Unlit surfaces take no ambient light, so ambient occlusion has nothing to take from them
	bool unlit = c_surface == SURFACE_UNLIT && HasFeature(FEATURE_UNLIT);
	fsout_surface = vec4(OctahedralEncode(surface.N), surface.roughness, max(F0.r, max(F0.g, F0.b)));
	fsout_indirect = vec4(unlit ? vec3(0.0) :
		max(g_indirectDiffuse, vec3(0.0)) * sceneparam.indirectLightMult * sceneparam.sceneBrightness, 1.0);
	fsout_reflection = vec4(unlit ? vec3(0.0) :
		max(g_indirectSpecular, vec3(0.0)) * sceneparam.indirectLightMult * sceneparam.sceneBrightness, 1.0);
	fsout_glow = vec4(max(glow, vec3(0.0)), 1.0);
}
