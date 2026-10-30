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

layout(set = 7, binding = 0, std430) buffer SkinBuffer
{
    readonly uvec4 skin[];
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

	mat4 bone = GetBoneMatrix();
	vec4 worldPos = w * bone * vec4(position, 1.0);
	fsin_worldpos = worldPos.xyz;

	// Normals need the inverse transpose of the world matrix to stay correct under non-uniform scale.
	// Tangents are directions along the surface, so they use the matrix itself.
	mat3 wm = mat3(idata[gl_InstanceIndex].materialID.z != 0u ? w * bone : w);
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
