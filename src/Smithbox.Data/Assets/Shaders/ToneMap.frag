#version 450

// Takes a viewport's HDR scene target to the screen, see ToneMapPass.cs. The target holds linear light, with the
// GPARAM exposure already in it. This applies the viewport's own exposure, the curve, and the square root that the
// shaders used to finish with as their gamma, so the Clamp curve at an exposure of 1 looks as the viewport did
// before it had an HDR target.
//
// Then the GPARAM ColorGrading[Yebis] LUT, see ColorGradingLuts.cs: the display color looked up in a 16 x 16 x 16
// cube, trilinearly, from an atlas of every LUT side by side. Between two of LutSourceId's keys the two LUTs are
// blended. The LUTs are 8 bit display values, taken to be indexed by the display color this writes.

#define OPERATOR_CLAMP 0
#define OPERATOR_REINHARD 1
#define OPERATOR_FILMIC 2

layout(set = 0, binding = 0) uniform ToneMapParams
{
	vec4 settings; // x: exposure, y: operator, z: white point, w: 1 to apply the auto exposure
	vec4 offset;   // xy: where the target's first pixel is drawn
	vec4 grading;  // x: 1 to grade, y: the first LUT's slot in the atlas, z: the second's, w: how far towards it
} params;

layout(set = 0, binding = 1) uniform texture2D sceneColor;
layout(set = 0, binding = 2) uniform sampler sceneSampler;

// The auto exposure's state, see AutoExposure.comp
layout(set = 0, binding = 3, std430) readonly buffer ExposureState
{
	float ev;
	float targetEv;
	float meanLog;
	float valid;
} exposureState;

layout(set = 0, binding = 4) uniform texture2D lutAtlas; // 16 x 256 per LUT, side by side
layout(set = 0, binding = 5) uniform sampler lutSampler; // linear, clamped

#define LUT_SIZE 16.0

// A display color through the LUT in a slot: red across, green down a 16 x 16 slice, blue from slice to slice
vec3 GradeWith(vec3 color, float slot)
{
	vec3 cell = clamp(color, 0.0, 1.0) * (LUT_SIZE - 1.0);
	float blue = floor(cell.b);
	float blueNext = min(blue + 1.0, LUT_SIZE - 1.0);

	vec2 atlas = vec2(textureSize(sampler2D(lutAtlas, lutSampler), 0));
	float x = slot * LUT_SIZE + cell.r + 0.5;

	vec3 a = textureLod(sampler2D(lutAtlas, lutSampler), vec2(x, blue * LUT_SIZE + cell.g + 0.5) / atlas, 0.0).rgb;
	vec3 b = textureLod(sampler2D(lutAtlas, lutSampler), vec2(x, blueNext * LUT_SIZE + cell.g + 0.5) / atlas, 0.0).rgb;

	return mix(a, b, cell.b - blue);
}

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

	float exposure = params.settings.x;

	if (params.settings.w > 0.5)
	{
		exposure *= exp2(exposureState.ev);
	}

	vec3 color = max(texelFetch(sampler2D(sceneColor, sceneSampler), pixel, 0).rgb, vec3(0.0)) * exposure;

	int op = int(params.settings.y + 0.5);

	if (op == OPERATOR_REINHARD)
	{
		color = Reinhard(color, params.settings.z);
	}
	else if (op == OPERATOR_FILMIC)
	{
		color = Filmic(color);
	}

	vec3 display = sqrt(clamp(color, 0.0, 1.0));

	if (params.grading.x > 0.5)
	{
		display = mix(GradeWith(display, params.grading.y), GradeWith(display, params.grading.z), params.grading.w);
	}

	fsout_color = vec4(display, 1.0);
}
