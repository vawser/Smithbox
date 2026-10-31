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
#endif

layout (constant_id = 6) const bool c_transparent = false;
layout (constant_id = 99) const bool c_picking = false;

#define MAX_LAYERS 4
#define MAX_SCENE_LIGHTS 16

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
	layout(location = 11) in float fsin_blendWeight;
#endif

layout(location = 0) out vec4 fsout_color;

struct sceneLight
{
	vec4 positionRange;  // xyz: position, w: range (0 = no limit)
	vec4 directionType;  // xyz: direction the light travels, w: type
	vec4 colorIntensity; // rgb: color, a: intensity
	vec4 spot;           // x: cos(inner angle), y: cos(outer angle), z: 1 to fade across the range
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
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];
layout(set = 3, binding = 0) uniform textureCube globalTexturesCube[];

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

// The entry after that
struct effectsData
{
	vec4 emissiveColor;  // xyz: multiplies the emissive texture
	vec4 emissiveTiling; // xy: tiling, zw: UV movement each second
	vec4 scroll;         // xy: UV movement of layer 0's albedo each second, zw: of its normal
};

layout(set = 4, binding = 0, std140) buffer materialEffects
{
    readonly effectsData effectsdata[];
};

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
vec2 g_uv[2];
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

// Samples with explicit derivatives, so a layer can be skipped without making derivatives undefined
vec4 SampleAt(uint tex, int uvSet, vec2 tiling, vec2 scroll)
{
	return textureGrad(
		sampler2D(globalTextures[nonuniformEXT(int(tex))], anisoLinearSampler),
		(g_uv[uvSet] + scroll * sceneparam.materialTime) * tiling, g_dx[uvSet] * tiling, g_dy[uvSet] * tiling);
}

// Only the first layer scrolls. Specular and shininess follow the albedo
vec2 LayerScroll(int row, int layer)
{
	if (layer != 0)
	{
		return vec2(0.0);
	}

	vec4 scroll = effectsdata[g_mat + 2u].scroll;
	return row == ROW_NORMAL ? scroll.zw : scroll.xy;
}

// Specular and shininess follow the albedo of their layer. Layers after the first two aren't tiled
vec2 LayerTiling(int row, int layer)
{
	if (layer > 1)
	{
		return vec2(1.0);
	}

	vec4 tiling = layer == 0 ? shadingdata[g_mat + 1u].tiling : shadingdata[g_mat + 1u].tiling2;
	return row == ROW_NORMAL ? tiling.zw : tiling.xy;
}

// One layer of a texture row, using the UV set that layer is authored against
vec4 SampleLayer(int row, int layer)
{
	int uvSet = clamp(int(MaterialValue(ROW_UVSET, layer)), 0, 1);
	return SampleAt(MaterialValue(row, layer), uvSet, LayerTiling(row, layer), LayerScroll(row, layer));
}

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
				result += SampleLayer(row, i) * g_weight[i];
			}
		}
		return result;
	}
#endif
	return SampleLayer(row, 0);
}

#ifdef MATERIAL_BLEND
void ComputeLayerWeights()
{
  #ifdef MATERIAL_DEFORM
	if (c_blendFromTexCoord)
	{
		g_weight[1] = clamp(fsin_blendWeight, 0.0, 1.0);
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

// ---------------------------------------------------------------------------------------------
// Lighting
// ---------------------------------------------------------------------------------------------

struct Surface
{
	vec3 diffuse;
	vec3 F0;
	float roughness;
	vec3 N;
	vec3 V;
};

// Light arriving from direction L (pointing from the surface towards the light) with the given radiance
vec3 ShadeLight(Surface s, vec3 L, vec3 radiance)
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
		return (diffuse + specular) * radiance;
	}

	// PBR model
	float VdotH = clamp(dot(H, s.V), 0.0, 1.0);
	vec3 F = pow(1.0 - VdotH, 5.0) * (1.0 - s.F0) + s.F0;

	float specPower = exp2((1.0 - s.roughness) * 13.0);
	specPower = max(1.0, specPower / (specPower * 0.01 + 1.0));
	float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);

	vec3 specular = D * F * pow(LdotN, LdotNPower);
	vec3 diffuse = s.diffuse * LdotN * (1.0 - s.F0);

	return (diffuse + specular) * radiance;
}

// Works out the direction to, and the radiance from, a scene light at a point in the world
vec3 LightRadiance(sceneLight light, vec3 worldPos, out vec3 L)
{
	int type = int(light.directionType.w + 0.5);
	vec3 color = light.colorIntensity.rgb * light.colorIntensity.a;

	if (type == LIGHT_DIRECTIONAL)
	{
		L = -normalize(light.directionType.xyz);
		return color;
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

	return color * attenuation;
}

// Light from the environment map
vec3 IndirectLight(Surface s)
{
	vec3 ambientDiffuse = textureLod(
		samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler),
		vec3(s.N * vec3(1, 1, -1)), 5).xyz;
	ambientDiffuse *= sceneparam.ambientLightMult;

	// traditional phong model
	if (c_gameID == GAME_DES || c_gameID == GAME_DS1_PTDE)
	{
		return s.diffuse * ambientDiffuse;
	}

	// PBR model
	float envMip = min(6.0, -(1.0 - s.roughness) * 6.5 + 6.5);
	vec3 reflectVec = reflect(-s.V, s.N);
	vec3 ambientSpec = textureLod(
		samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler),
		vec3(reflectVec * vec3(1, 1, -1)), envMip).xyz;
	ambientSpec *= sceneparam.ambientLightMult;

	float NdotV = max(abs(clamp(dot(s.V, s.N), 0.0, 1.0)), Epsilon);
	float gloss = 1.0 - s.roughness;
	vec3 aF = pow(1.0 - NdotV, 5.0) * gloss * gloss * (1.0 - s.F0) + s.F0;

	vec3 indirectDiffuse = s.diffuse * ambientDiffuse * (1.0 - s.F0);
	vec3 indirectSpecular = ambientSpec * aF;
	float reflectionThing = clamp(dot(reflectVec, s.N) + 1.0, 0.0, 1.0);
	indirectSpecular *= reflectionThing * reflectionThing;

	return indirectDiffuse + indirectSpecular;
}

void main()
{
	g_mat = fsin_mat;

	vec3 viewVec = normalize(sceneparam.eye.xyz - fsin_worldpos);

	vec3 sunDir = vec3(sceneparam.lightDirection);
	sunDir = dot(sunDir, sunDir) > 1e-10 ? normalize(sunDir) : vec3(0.0, -1.0, 0.0);

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

	if (diffuseColor.w < diffuseAlpha.w)
	{
		discard;
	}

#ifdef SELECTED
	if (sceneparam.EnableDithering == 1)
	{
		// 4x4 Bayer ordered dither
		mat4 bayerMatrix = mat4(
			 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
			12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
			 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
			15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
		);

		ivec2 pixelCoord = ivec2(gl_FragCoord.xy) % 4;
		float threshold = bayerMatrix[pixelCoord.x][pixelCoord.y];

		if (sceneparam.ditherOpacity < threshold)
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

#ifdef MATERIAL_BLEND
	vec3 normalColor = SampleLayered(ROW_NORMAL, c_blendNormal).xyz;
	vec3 F0 = SampleLayered(ROW_SPECULAR, c_blendSpecular).xyz;
#else
	vec3 normalColor = SampleLayer(ROW_NORMAL, 0).xyz;
	vec3 F0 = SampleLayer(ROW_SPECULAR, 0).xyz;
#endif

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
	normalMap.z = sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
	normalMap = normalize(normalMap);
	normalMap = normalize(fsin_worldToTangent * normalMap);

	Surface surface;
	surface.diffuse = diffuseColor.xyz * diffuseAlpha.xyz;
	surface.F0 = F0;
	surface.roughness = roughness;
	surface.N = (gl_FrontFacing ? normalMap : -normalMap);
	surface.V = viewVec;

	// The sun, then every light in the scene
	vec3 direct = ShadeLight(surface, -sunDir, sceneparam.sunColor.rgb * sceneparam.sunColor.a);

	uint lightCount = min(sceneparam.lightCount, uint(MAX_SCENE_LIGHTS));
	for (uint i = 0; i < lightCount; i++)
	{
		vec3 L;
		vec3 radiance = LightRadiance(sceneparam.lights[i], fsin_worldpos, L);
		if (dot(radiance, radiance) > 0.0)
		{
			direct += ShadeLight(surface, L, radiance);
		}
	}

	vec3 indirect = IndirectLight(surface);

	vec3 litColor = (direct * sceneparam.directLightMult + indirect * sceneparam.indirectLightMult) *
		sceneparam.sceneBrightness;

	// Materials without an emissive texture have the black default, which is index 2
	uint emissiveTex = MaterialValue(ROW_EXTRA, 1);
	if (emissiveTex != 2u)
	{
		effectsData effects = effectsdata[g_mat + 2u];
		vec3 emissive = SampleAt(emissiveTex, 0, effects.emissiveTiling.xy, effects.emissiveTiling.zw).rgb;
		litColor += emissive * effects.emissiveColor.xyz * sceneparam.emissiveMapMult;
	}

#ifdef SELECTED
	if (sceneparam.EnableTinting == 1)
	{
		vec3 tint = sceneparam.selectionColor.rgb;
		float strength = sceneparam.selectionColor.a;
		litColor = mix(litColor, litColor * tint, strength);
	}
#endif

	fsout_color = vec4(sqrt(max(litColor, vec3(0.0))), c_transparent ? diffuseColor.w : 1.0);
}
