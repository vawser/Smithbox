#version 450
#extension GL_GOOGLE_include_directive : require
#extension GL_EXT_nonuniform_qualifier : require

// The older games' classic distance fog (Demon's Souls' FogBank), see DistanceFogPass.cs. Drawn into the HDR target
// after the opaque draws, at the copy of their depth, so it lands under the transparent ones. Each pixel's distance
// along the camera's axis gives the fog: none before Begin, rising linearly to MaxDensity at End and staying there.
// Pixels with nothing drawn behind them are left as they are.

#define MAX_SCENE_LIGHTS 16

layout(location = 0) out vec4 fsout_color;

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
	sceneParams sceneparam;
};

layout(set = 1, binding = 0) uniform texture2D globalTextures[];

// Laid out as DistanceFogParam
layout(set = 2, binding = 0) uniform FogParamBuffer
{
	vec4 range; // x: begin, y: end, z: density at the end
	vec4 color; // rgb: the fog's light
} fog;

layout(set = 2, binding = 1) uniform sampler pointClamp;

int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// Distance from the camera along its axis of a value in the depth buffer, as VolumetricFog.frag finds it
float ViewDepth(float depth)
{
	mat4 projection = sceneparam.projection;

	if (abs(projection[2][3]) < 1e-6)
	{
		return abs((depth - projection[3][2]) / projection[2][2]);
	}

	return abs(projection[3][2] / (depth * projection[2][3] - projection[2][2]));
}

void main()
{
	if (sceneparam.sceneDepthTexture == 0u)
	{
		discard;
	}

	float stored = texelFetch(
		sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], pointClamp),
		ivec2(gl_FragCoord.xy), 0).r;

	// Reverse depth clears to 0, where nothing was drawn
	if (stored <= 0.0)
	{
		discard;
	}

	float depth = ViewDepth(stored);
	float t = clamp((depth - fog.range.x) / max(fog.range.y - fog.range.x, 1e-3), 0.0, 1.0);
	float amount = t * fog.range.z;

	// Blended as scene x a + rgb
	fsout_color = vec4(fog.color.rgb * amount, 1.0 - amount);
}
