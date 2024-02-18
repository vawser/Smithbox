#version 450
#extension GL_EXT_nonuniform_qualifier : require
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable

layout(location = 0) in vec4 color;
layout(location = 0) out vec4 fsout_color;
layout(location = 1) flat in uint fsin_entityid;

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
    
    vec4 SimpleFlverSelectColor;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

void main()
{
	fsout_color = sceneparam.SimpleFlverSelectColor;
}