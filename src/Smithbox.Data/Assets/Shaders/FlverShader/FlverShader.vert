#version 450
#ifdef MATERIAL_DEFORM
#extension GL_EXT_nonuniform_qualifier : require
#endif

// Vertex stage of the flver shader.
//
// Macros (set by the variant list in FlverShader.json):
//   MATERIAL_BLEND  - the mesh has a second UV set, which is passed on for blended materials
//   MATERIAL_DEFORM - the mesh has the TEXCOORD1-3 attributes, for vertex animation and blend weights
//   SELECTED        - used for selected objects (shares this file; the fragment stage does the tinting)

#ifdef MATERIAL_DEFORM
#define MAX_SCENE_LIGHTS 16

struct sceneLight
{
	vec4 positionRange;
	vec4 directionType;
	vec4 colorIntensity;
	vec4 spot;
};
#endif

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
#ifdef MATERIAL_DEFORM
	// Only here to reach materialTime, which comes after them
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
	vec4 sunColor;
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
#endif
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct instanceData
{
	mat4 world;
	// 0: material id
	// 1: bone base
	// 3: entity id
	uvec4 materialID;
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
    readonly instanceData idata[];
};

layout(set = 7, binding = 0, std140) buffer BoneBuffer
{
    readonly mat4 bones[];
};

layout(set = 7, binding = 0, std430) buffer SkinBuffer
{
    readonly uvec4 skin[];
};
layout (constant_id = 50) const bool c_normalWBoneTransform = false;

#ifdef MATERIAL_DEFORM
#define DEFORM_VAT 1
#define DEFORM_SWAY 2
layout (constant_id = 60) const int c_deformer = 0;

layout(set = 2, binding = 0) uniform texture2D globalTextures[];

// Two entries after a material in the material buffer. Same size as the fragment stage's materialData
struct deformData
{
	vec4 params;    // x: amplitude, y: frames or cycles a second
	vec4 direction; // xyz: direction of sway
	uvec4 textures; // x: vertex animation positions, y: vertex animation normals
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly deformData deformdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;
#endif

layout(location = 0) in vec3 position;
layout(location = 1) in ivec2 uv;
layout(location = 2) in ivec4 normal;
layout(location = 3) in ivec4 binormal;
layout(location = 4) in ivec4 bitangent;
layout(location = 5) in uvec4 color;

#if defined(MATERIAL_BLEND) || defined(MATERIAL_DEFORM)
	layout(location = 6) in ivec2 uv2;
#endif
#ifdef MATERIAL_DEFORM
	layout(location = 7) in vec4 texcoord1;
	layout(location = 8) in vec4 texcoord2;
	layout(location = 9) in vec4 texcoord3;
#endif

layout(location = 0) out vec2 fsin_texcoord;
layout(location = 1) out vec3 fsin_worldpos;
layout(location = 2) out mat3 fsin_worldToTangent;
layout(location = 7) out vec4 fsin_color;
layout(location = 8) flat out uint fsin_mat;
layout(location = 9) flat out uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) out vec2 fsin_texcoord2;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	layout(location = 11) out float fsin_blendWeight;
#endif

// UVs are stored as signed 16 bit fixed point with 11 fractional bits
const float UV_SCALE = 1.0 / 2048.0;

// materialID.z is 0 unless the instance is being animated, which keeps static models untouched:
//   1            - rigid, the bone only needs to reach the normals as well
//   high bit set - weighted, the low bits plus gl_VertexIndex index the skin data
mat4 GetBoneMatrix()
{
	uint skinRef = idata[gl_InstanceIndex].materialID.z;
	uint boneBase = idata[gl_InstanceIndex].materialID.y;

	if ((skinRef & 0x80000000u) != 0u)
	{
		uvec4 s = skin[(skinRef + uint(gl_VertexIndex)) & 0x7FFFFFFFu];
		vec4 weights = unpackUnorm4x8(s.z);

		return bones[boneBase + (s.x & 0xFFFFu)] * weights.x +
			bones[boneBase + (s.x >> 16)] * weights.y +
			bones[boneBase + (s.y & 0xFFFFu)] * weights.z +
			bones[boneBase + (s.y >> 16)] * weights.w;
	}

	if (c_normalWBoneTransform)
	{
		return bones[boneBase + normal.w];
	}

	return mat4(1.0);
}

#ifdef MATERIAL_DEFORM
// One frame of a vertex animation. Columns are vertices and rows are frames
vec3 FetchVat(uint tex, int column, int row)
{
	return texelFetch(sampler2D(globalTextures[nonuniformEXT(int(tex))], linearSampler), ivec2(column, row), 0).xyz;
}

// Plays a baked animation. The vertex's column is stored in TEXCOORD2, as vertices that were split
// on export share one. TEXCOORD1.z is how strongly the vertex follows it, TEXCOORD3.x offsets its start
void DeformVat(deformData deform, inout vec3 pos, inout vec3 nrm)
{
	uint vat = deform.textures.x;
	uint van = deform.textures.y;

	// Still loading
	if (vat == 0u || van == 0u)
	{
		return;
	}

	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(int(vat))], linearSampler), 0);
	float height = float(size.y);

	float start = fract(texcoord3.x);
	float frame = fract((height * start + sceneparam.materialTime) * deform.params.y / height) * height;

	int column = min(int(texcoord2.x * 255.0 + texcoord2.y * 65280.0 + 0.5), size.x - 1);
	int row = int(floor(frame));
	int nextRow = min(row + 1, size.y - 1);
	float blend = fract(frame);

	vec3 offset = mix(FetchVat(vat, column, row), FetchVat(vat, column, nextRow), blend);
	pos += offset * deform.params.x * texcoord1.z;

	nrm = mix(FetchVat(van, column, row), FetchVat(van, column, nextRow), blend) * 2.0 - 1.0;
}

// Moves along a direction by a smoothed triangle wave. TEXCOORD1.x offsets its
// start and TEXCOORD1.z is how strongly the vertex follows it
void DeformSway(deformData deform, inout vec3 pos)
{
	float t = abs(fract(deform.params.y * (sceneparam.materialTime + texcoord1.x * 2.0)) * 2.0 - 1.0);
	float wave = t * t * (3.0 - 2.0 * t) * 2.0 - 1.0;

	pos += deform.direction.xyz * deform.params.x * texcoord1.z * wave;
}
#endif

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;

	vec3 localPosition = position;
	vec3 localNormal = vec3(normal);

#ifdef MATERIAL_DEFORM
	deformData deform = deformdata[idata[gl_InstanceIndex].materialID.x + 2u];

	if (c_deformer == DEFORM_VAT)
	{
		DeformVat(deform, localPosition, localNormal);
	}
	else if (c_deformer == DEFORM_SWAY)
	{
		DeformSway(deform, localPosition);
	}
#endif

	fsin_texcoord = vec2(uv) * UV_SCALE;
#ifdef MATERIAL_BLEND
	fsin_texcoord2 = vec2(uv2) * UV_SCALE;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	fsin_blendWeight = texcoord1.x;
#endif

	// Vertex colors are used as blend weights by blended materials
	fsin_color = vec4(color) / 255.0;

	fsin_mat = idata[gl_InstanceIndex].materialID.x;
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;

	mat4 bone = GetBoneMatrix();
	vec4 worldPos = w * bone * vec4(localPosition, 1.0);
	fsin_worldpos = worldPos.xyz;

	// Normals need the inverse transpose of the world matrix to stay correct under non-uniform scale.
	// Tangents are directions along the surface, so they use the matrix itself.
	mat3 wm = mat3(idata[gl_InstanceIndex].materialID.z != 0u ? w * bone : w);
	mat3 normalMatrix = wm;
	if (abs(determinant(wm)) > 1e-12)
	{
		normalMatrix = transpose(inverse(wm));
	}

	vec3 N = normalize(normalMatrix * localNormal);
	vec3 T = wm * vec3(bitangent);
	vec3 B = wm * vec3(binormal);

	// Meshes without tangents have them zeroed. Build any valid frame rather than letting
	// normalizing a zero vector poison the normal.
	if (dot(T, T) < 1e-8)
	{
		T = cross(abs(N.y) < 0.99 ? vec3(0.0, 1.0, 0.0) : vec3(1.0, 0.0, 0.0), N);
		B = vec3(0.0);
	}

	// Make the frame orthonormal, keeping the handedness the mesh was authored with
	T = normalize(T - N * dot(N, T));
	float handedness = dot(cross(N, T), B) < 0.0 ? -1.0 : 1.0;
	B = cross(N, T) * handedness;

	fsin_worldToTangent = mat3(T, B, N);

	gl_Position = sceneparam.projection * sceneparam.view * worldPos;
}
