#version 450

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
    float SimpleFlver_Brightness;
    float SimpleFlver_Saturation;
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
	vec3 tnormal = normalize(mat3(w) * vec3(normal));
    fsin_color = vec4((vec3((vec4(tnormal, 1.0)) + sceneparam.SimpleFlver_Brightness) * sceneparam.SimpleFlver_Saturation), 1.0);
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
    
	if (c_normalWBoneTransform)
	{
		gl_Position = sceneparam.projection * sceneparam.view * w *
			(bones[idata[gl_InstanceIndex].materialID.y + normal.w] * vec4(position, 1));
	}
	else
	{
		gl_Position = sceneparam.projection * sceneparam.view * w * vec4(position, 1);
	}
}