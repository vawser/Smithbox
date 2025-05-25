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

layout(location = 0) in vec3 position;
layout(location = 1) in uvec4 color;
layout(location = 2) in vec3 normal;
layout(location = 0) out vec4 fsin_color;
layout(location = 1) out uint fsin_mat;
layout(location = 2) out uint fsin_entityid;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
    fsin_color = vec4(color) / 255.0;
	fsin_mat = idata[gl_InstanceIndex].materialID[0];
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
    gl_Position = sceneparam.projection * sceneparam.view * w * vec4(position, 1);
}