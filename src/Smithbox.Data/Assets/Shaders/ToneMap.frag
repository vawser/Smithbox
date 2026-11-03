#version 450

// Takes a viewport's HDR scene target to the screen, see ToneMapPass.cs. The target holds linear light, with the
// GPARAM exposure already in it. This applies the viewport's own exposure, the curve, and the square root that the
// shaders used to finish with as their gamma, so the Clamp curve at an exposure of 1 looks as the viewport did
// before it had an HDR target.

#define OPERATOR_CLAMP 0
#define OPERATOR_REINHARD 1
#define OPERATOR_FILMIC 2

layout(set = 0, binding = 0) uniform ToneMapParams
{
	vec4 settings; // x: exposure, y: operator, z: white point
	vec4 offset;   // xy: where the target's first pixel is drawn
} params;

layout(set = 0, binding = 1) uniform texture2D sceneColor;
layout(set = 0, binding = 2) uniform sampler sceneSampler;

layout(location = 0) out vec4 fsout_color;

float Luminance(vec3 color)
{
	return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

// Extended Reinhard on luminance: white at the white point, and the hue kept
vec3 Reinhard(vec3 color, float white)
{
	float luminance = Luminance(color);

	if (luminance <= 0.0)
	{
		return vec3(0.0);
	}

	float mapped = luminance * (1.0 + luminance / (white * white)) / (1.0 + luminance);
	return color * (mapped / luminance);
}

// The ACES filmic curve, as fitted by Krzysztof Narkowicz
vec3 Filmic(vec3 color)
{
	const float a = 2.51;
	const float b = 0.03;
	const float c = 2.43;
	const float d = 0.59;
	const float e = 0.14;

	return (color * (a * color + b)) / (color * (c * color + d) + e);
}

void main()
{
	ivec2 size = textureSize(sampler2D(sceneColor, sceneSampler), 0);
	ivec2 pixel = clamp(ivec2(gl_FragCoord.xy - params.offset.xy), ivec2(0), size - 1);

	vec3 color = max(texelFetch(sampler2D(sceneColor, sceneSampler), pixel, 0).rgb, vec3(0.0)) * params.settings.x;

	int op = int(params.settings.y + 0.5);

	if (op == OPERATOR_REINHARD)
	{
		color = Reinhard(color, params.settings.z);
	}
	else if (op == OPERATOR_FILMIC)
	{
		color = Filmic(color);
	}

	fsout_color = vec4(sqrt(clamp(color, 0.0, 1.0)), 1.0);
}
