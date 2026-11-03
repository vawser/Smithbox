#version 450
#extension GL_EXT_nonuniform_qualifier : require
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable

layout(location = 0) in vec4 color;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;
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
    vec4 OutlineColor;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

void main()
{
	fsout_color = sceneparam.OutlineColor;

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
