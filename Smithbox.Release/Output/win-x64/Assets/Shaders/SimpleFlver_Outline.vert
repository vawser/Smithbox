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
layout(location = 1) in ivec4 normal;
layout(location = 0) out vec4 fsin_color;
layout(location = 1) out uint fsin_entityid;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	vec3 fsin_normal = normalize(mat3(w) * vec3(normal));
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
	
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