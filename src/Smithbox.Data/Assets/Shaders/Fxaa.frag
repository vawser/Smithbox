#version 450

// FXAA on the tone mapped image, see FxaaPass.cs, with the GPARAM Anti-Alias group's FXAA values: Reduce Min,
// Reduce Mul and Span Max are those of Timothy Lottes' original FXAA (1/128, 1/8 and 8 in Elden Ring's mapbase), so
// this is that version. Alpha blends between the image and the filtered result. The image holds display values, as
// FXAA expects.

layout(set = 0, binding = 0) uniform FxaaParams
{
	vec4 settings; // x: alpha, y: reduce min, z: reduce mul, w: span max
	vec4 offset;   // xy: where the image's first pixel is drawn
} params;

layout(set = 0, binding = 1) uniform texture2D image;
layout(set = 0, binding = 2) uniform sampler linearClamp;

layout(location = 0) out vec4 fsout_color;

float Luma(vec3 color)
{
	return dot(color, vec3(0.299, 0.587, 0.114));
}

vec3 Sample(vec2 uv)
{
	return textureLod(sampler2D(image, linearClamp), uv, 0.0).rgb;
}

void main()
{
	vec2 size = vec2(textureSize(sampler2D(image, linearClamp), 0));
	vec2 texel = 1.0 / size;
	vec2 uv = (gl_FragCoord.xy - params.offset.xy) / size;

	vec3 rgbM = Sample(uv);
	float lumaNW = Luma(Sample(uv + vec2(-1.0, -1.0) * texel));
	float lumaNE = Luma(Sample(uv + vec2( 1.0, -1.0) * texel));
	float lumaSW = Luma(Sample(uv + vec2(-1.0,  1.0) * texel));
	float lumaSE = Luma(Sample(uv + vec2( 1.0,  1.0) * texel));
	float lumaM = Luma(rgbM);

	float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
	float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

	// Across the edge: the direction in which the corners' luma changes most
	vec2 dir;
	dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
	dir.y = ((lumaNW + lumaSW) - (lumaNE + lumaSE));

	float reduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * 0.25 * params.settings.z, params.settings.y);
	float scale = 1.0 / (min(abs(dir.x), abs(dir.y)) + reduce);
	dir = clamp(dir * scale, vec2(-params.settings.w), vec2(params.settings.w)) * texel;

	vec3 rgbA = 0.5 * (Sample(uv + dir * (1.0 / 3.0 - 0.5)) + Sample(uv + dir * (2.0 / 3.0 - 0.5)));
	vec3 rgbB = rgbA * 0.5 + 0.25 * (Sample(uv + dir * -0.5) + Sample(uv + dir * 0.5));

	// The wider blend unless it overshoots the neighbourhood, when it has crossed another edge
	float lumaB = Luma(rgbB);
	vec3 filtered = (lumaB < lumaMin || lumaB > lumaMax) ? rgbA : rgbB;

	fsout_color = vec4(mix(rgbM, filtered, clamp(params.settings.x, 0.0, 1.0)), 1.0);
}
