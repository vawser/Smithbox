#version 450

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
	uint envmap;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct instanceData
{
	mat4 world;
	// 0: material id
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
layout(location = 1) out vec3 fsin_view;
layout(location = 2) out mat3 fsin_worldToTangent;
layout(location = 5) out vec3 fsin_normal;
layout(location = 6) out vec4 fsin_bitangent;
layout(location = 7) out vec4 fsin_color;
layout(location = 8) out uint fsin_mat;
layout(location = 9) out uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) out vec2 fsin_texcoord2;
#endif

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_texcoord = vec2(uv) / 2048.0;
#ifdef MATERIAL_BLEND
	fsin_texcoord2 = vec2(uv2) / 2048.0;
#endif
	fsin_normal = normalize(mat3(w) * vec3(normal));
	fsin_bitangent = bitangent;
	fsin_view = normalize(sceneparam.eye.xyz - (w * vec4(position, 1)).xyz);
	fsin_mat = idata[gl_InstanceIndex].materialID.x;
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
	
	vec3 T = normalize(mat3(w) * vec3(bitangent));
	vec3 B = normalize(mat3(w) * vec3(binormal));
	vec3 N = normalize(mat3(w) * vec3(normal));
	fsin_worldToTangent = mat3(T, B, N);
	
	vec3 ssnormal = mat3(sceneparam.projection) * mat3(sceneparam.view) * fsin_normal;

	vec4 posbase;
	if (c_normalWBoneTransform)
	{
		posbase = sceneparam.projection * sceneparam.view * w *
			(bones[idata[gl_InstanceIndex].materialID.y + normal.w] * vec4(position, 1));
	}
	else
	{
		posbase = sceneparam.projection * sceneparam.view * w * vec4(position, 1);
	}
    gl_Position = posbase + vec4(ssnormal, 0.0) * posbase.w * 0.005;
}