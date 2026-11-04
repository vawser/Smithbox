#version 450

// The GPARAM Bloom group (fields named Glare), see BloomPass.cs. Each draw is one step, picked by the mode:
//
// 0 prefilter: the scene into the first level at half size, keeping the share of its luma over Glare Threshold after
//   the auto exposure, as the game's bright pass does, scaled by Glare Luminance
// 1 downsample: a level into the next, half its size, with a 13-tap filter
// 2 upsample: a level plus the level below it at a weight (a half for the bloom, 1 for the glow), spread by a tent
//   filter scaled by GaussianRadiusScale
// 3 composite: the scene plus the first level's sum, x GlareBloom Alpha, over the sum of the levels' weights
//
// Every texture is read by where the pixel is in the target being drawn, so nothing depends on the viewport's flip.
// The bloom is kept in the scene's units (the auto exposure is divided back out), so the tone map exposes both alike.
// The filters are the usual ones for a mip chain bloom; the game's Yebis glare is not decoded.

#define MODE_PREFILTER 0
#define MODE_DOWNSAMPLE 1
#define MODE_UPSAMPLE 2
#define MODE_COMPOSITE 3

layout(set = 0, binding = 0) uniform BloomParams
{
	vec4 settings; // x: mode, y: threshold, z: knee, w: luminance
	vec4 shape;    // x: saturation control, y: radius scale, z: alpha, w: 1 / level count
	vec4 sizes;    // xy: the size of a texel of texture B (A in the prefilter and downsample), zw: the target's size
} params;

layout(set = 0, binding = 1) uniform texture2D textureA;
layout(set = 0, binding = 2) uniform texture2D textureB;
layout(set = 0, binding = 3) uniform sampler linearClamp;

layout(set = 0, binding = 4, std430) readonly buffer ExposureState
{
	float ev;
	float targetEv;
	float meanLog;
	float valid;
} exposureState;

layout(location = 0) out vec4 fsout_color;

float Luminance(vec3 color)
{
	return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

vec3 SampleA(vec2 uv)
{
	return max(textureLod(sampler2D(textureA, linearClamp), uv, 0.0).rgb, vec3(0.0));
}

vec3 SampleB(vec2 uv)
{
	return max(textureLod(sampler2D(textureB, linearClamp), uv, 0.0).rgb, vec3(0.0));
}

// The weight that stops single bright pixels flickering through the chain (Karis)
float KarisWeight(vec3 color)
{
	return 1.0 / (1.0 + Luminance(color));
}

// 13 taps in a 4 x 4 texel area: four overlapping boxes around the centre and one in it (Jimenez, CoD: AW)
vec3 Downsample13(vec2 uv, vec2 texel, bool karis)
{
	vec3 a = SampleA(uv + texel * vec2(-2.0, -2.0));
	vec3 b = SampleA(uv + texel * vec2( 0.0, -2.0));
	vec3 c = SampleA(uv + texel * vec2( 2.0, -2.0));
	vec3 d = SampleA(uv + texel * vec2(-1.0, -1.0));
	vec3 e = SampleA(uv + texel * vec2( 1.0, -1.0));
	vec3 f = SampleA(uv + texel * vec2(-2.0,  0.0));
	vec3 g = SampleA(uv);
	vec3 h = SampleA(uv + texel * vec2( 2.0,  0.0));
	vec3 i = SampleA(uv + texel * vec2(-1.0,  1.0));
	vec3 j = SampleA(uv + texel * vec2( 1.0,  1.0));
	vec3 k = SampleA(uv + texel * vec2(-2.0,  2.0));
	vec3 l = SampleA(uv + texel * vec2( 0.0,  2.0));
	vec3 m = SampleA(uv + texel * vec2( 2.0,  2.0));

	vec3 center = (d + e + i + j) * 0.25;
	vec3 topLeft = (a + b + f + g) * 0.25;
	vec3 topRight = (b + c + g + h) * 0.25;
	vec3 bottomLeft = (f + g + k + l) * 0.25;
	vec3 bottomRight = (g + h + l + m) * 0.25;

	if (!karis)
	{
		return center * 0.5 + (topLeft + topRight + bottomLeft + bottomRight) * 0.125;
	}

	float wc = KarisWeight(center) * 0.5;
	float w0 = KarisWeight(topLeft) * 0.125;
	float w1 = KarisWeight(topRight) * 0.125;
	float w2 = KarisWeight(bottomLeft) * 0.125;
	float w3 = KarisWeight(bottomRight) * 0.125;

	return (center * wc + topLeft * w0 + topRight * w1 + bottomLeft * w2 + bottomRight * w3) / (wc + w0 + w1 + w2 + w3);
}

// A 3 x 3 tent of the smaller level, its taps spread by the radius scale
vec3 UpsampleTent(vec2 uv, vec2 texel, float radius)
{
	vec2 d = texel * radius;

	vec3 sum = SampleB(uv) * 4.0;
	sum += (SampleB(uv + vec2(-d.x, 0.0)) + SampleB(uv + vec2(d.x, 0.0)) +
		SampleB(uv + vec2(0.0, -d.y)) + SampleB(uv + vec2(0.0, d.y))) * 2.0;
	sum += SampleB(uv + vec2(-d.x, -d.y)) + SampleB(uv + vec2(d.x, -d.y)) +
		SampleB(uv + vec2(-d.x, d.y)) + SampleB(uv + vec2(d.x, d.y));

	return sum / 16.0;
}

vec3 Prefilter(vec3 color)
{
	float exposure = exp2(exposureState.ev);
	vec3 exposed = color * exposure;

	// As the game's ToneMap_BrightPassPS does it (Sekiro DXBC, Nightreign DXIL): the Rec. 601 luma over the
	// threshold, as a share of the luma, with no knee. Taking the brightest channel instead let saturated colours
	// bloom too much.
	float threshold = params.settings.y;
	float brightness = dot(exposed, vec3(0.29891, 0.58661, 0.11448));
	float contribution = max(brightness - threshold, 0.0) / (brightness + 1e-4);

	vec3 bright = exposed * contribution * params.settings.w;
	bright = mix(bright, vec3(Luminance(bright)), clamp(params.shape.x, 0.0, 1.0));

	return bright / exposure;
}

void main()
{
	vec2 uv = gl_FragCoord.xy / params.sizes.zw;
	int mode = int(params.settings.x + 0.5);

	vec3 color;

	if (mode == MODE_PREFILTER)
	{
		color = Prefilter(Downsample13(uv, params.sizes.xy, true));
	}
	else if (mode == MODE_DOWNSAMPLE)
	{
		color = Downsample13(uv, params.sizes.xy, false);
	}
	else if (mode == MODE_UPSAMPLE)
	{
		// Each coarser level at settings.z of the weight of the one above: a half for the bloom, as the game's
		// ToneMap_BloomCompositePS adds its levels (1, 1/2, 1/4; Sekiro DXBC, Nightreign DXIL), 1 for the glow
		color = SampleA(uv) + params.settings.z * UpsampleTent(uv, params.sizes.xy, params.shape.y);
	}
	else
	{
		color = SampleA(uv) + SampleB(uv) * params.shape.z * params.shape.w;
	}

	fsout_color = vec4(color, 1.0);
}
