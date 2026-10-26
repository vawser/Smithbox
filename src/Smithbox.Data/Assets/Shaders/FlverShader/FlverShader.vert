#version 450

// Vertex stage of the flver shader.
//
// Macros (set by the variant list in FlverShader.json):
//   MATERIAL_BLEND  - the mesh has a second UV set, which is passed on for blended materials
//   SELECTED        - used for selected objects (shares this file; the fragment stage does the tinting)

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
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

layout (constant_id = 50) const bool c_normalWBoneTransform = false;

layout(location = 0) in vec3 position;
layout(location = 1) in ivec2 uv;
layout(location = 2) in ivec4 normal;
layout(location = 3) in ivec4 binormal;
layout(location = 4) in ivec4 bitangent;
layout(location = 5) in uvec4 color;

#ifdef MATERIAL_BLEND
	layout(location = 6) in ivec2 uv2;
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

// UVs are stored as signed 16 bit fixed point with 11 fractional bits
const float UV_SCALE = 1.0 / 2048.0;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;

	fsin_texcoord = vec2(uv) * UV_SCALE;
#ifdef MATERIAL_BLEND
	fsin_texcoord2 = vec2(uv2) * UV_SCALE;
#endif

	// Vertex colors are used as blend weights by blended materials
	fsin_color = vec4(color) / 255.0;

	fsin_mat = idata[gl_InstanceIndex].materialID.x;
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;

	vec4 localPos = vec4(position, 1.0);
	if (c_normalWBoneTransform)
	{
		localPos = bones[idata[gl_InstanceIndex].materialID.y + normal.w] * localPos;
	}

	vec4 worldPos = w * localPos;
	fsin_worldpos = worldPos.xyz;

	// Normals need the inverse transpose of the world matrix to stay correct under non-uniform scale.
	// Tangents are directions along the surface, so they use the matrix itself.
	mat3 wm = mat3(w);
	mat3 normalMatrix = wm;
	if (abs(determinant(wm)) > 1e-12)
	{
		normalMatrix = transpose(inverse(wm));
	}

	vec3 N = normalize(normalMatrix * vec3(normal));
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
