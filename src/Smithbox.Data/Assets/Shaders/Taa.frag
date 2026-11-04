#version 450
#extension GL_EXT_nonuniform_qualifier : require

// The GPARAM Anti-Alias-Editor group's TAA (AntiAliasType 5), see TaaPass.cs. The scene is drawn a sub-pixel off each
// frame (JitterCount Halton offsets); this blends each frame into the history of the ones before:
//
// - this frame is read where the pixel would be without the jitter: the 3 x 3 around it weighted by how near each is
//   to that point, so the image doesn't move by the jitter where little of the history is kept
// - the history is read where the pixel was last frame, found from the scene depth with the camera's unjittered
//   matrices, with a Catmull-Rom filter so it doesn't blur as the camera moves. There are no motion vectors, so
//   what moved itself is found from last frame's depth: where it isn't the depth the point should have had there,
//   something else was in front of it or it has moved, and the history is let go.
// - it is clipped to the colors around the pixel this frame, in YCoCg, to the box of their mean and spread (and no
//   wider than their range), so what moved or appeared doesn't smear
// - the pixel is sharpened against its neighbours by SharpenIntensity, the change kept within SharpenRange
// - the two are blended by FeedbackWeight, less of the history with depth: FBWeightCancelByDepth's four bands,
//   between FBWeightBorderDepth's three depths, each take that share of it away
// - nothing of the history where it is off the screen, or when there is none
//
// The blend is weighted by 1 / (1 + luminance), so a bright pixel doesn't flicker through the history. How the game
// reads the values isn't known; this is a reading of them.

layout (constant_id = 100) const bool c_clipSpaceYInverted = false;

layout(set = 0, binding = 0) uniform TaaParams
{
	mat4 clipToWorld;      // the inverse of this frame's unjittered view and projection
	mat4 previousViewProj; // the last frame's unjittered view and projection
	vec4 settings;         // x: feedback weight, y: 1 if there is a history, z: sharpen intensity, w: sharpen range
	vec4 borders;          // xyz: FBWeightBorderDepth(0-2)
	vec4 cancel;           // FBWeightCancelByDepth(0-3)
	vec4 depth;            // xyz: projection [2][2], [3][2] and [2][3] for view depth, w: the scene depth's texture index
	vec4 sizes;            // xy: the size of the scene
	vec4 jitter;           // xy: this frame's jitter in NDC, z: last frame's depth's texture index, w: 1 if it can be read
} params;

// How different, as a share of the view depth, last frame's depth can be from what the point's should be and still
// be the same surface
const float c_depthTolerance = 0.03;

// How many of the neighbourhood's standard deviations the history may be from its mean
const float c_varianceGamma = 1.25;

layout(set = 0, binding = 1) uniform texture2D current;
layout(set = 0, binding = 2) uniform texture2D history;
layout(set = 0, binding = 3) uniform sampler linearClamp;

layout(set = 1, binding = 0) uniform texture2D globalTextures[];

layout(location = 0) out vec4 fsout_color;

float ViewDepth(float stored)
{
	if (abs(params.depth.z) < 1e-6)
	{
		return abs((stored - params.depth.y) / params.depth.x);
	}

	return abs(params.depth.y / (stored * params.depth.z - params.depth.x));
}

float StoredDepth(ivec2 pixel)
{
	uint index = uint(params.depth.w + 0.5);
	if (index == 0u || index >= 32768u)
		return 0.0;

	return texelFetch(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), pixel, 0).r;
}

// Last frame's scene depth, as a view depth; nothing drawn is far away
float PreviousViewDepth(ivec2 pixel)
{
	uint index = uint(params.jitter.z + 0.5);
	float stored = texelFetch(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), pixel, 0).r;
	return stored > 0.0 ? ViewDepth(stored) : 1e6;
}

vec3 SampleHistory(vec2 uv)
{
	return max(textureLod(sampler2D(history, linearClamp), uv, 0.0).rgb, vec3(0.0));
}

// The history at uv through a Catmull-Rom filter, its 16 taps in 5 bilinear reads (the corners, which weigh least,
// left out), so it stays sharp when read between its texels as the camera moves
vec3 SampleHistoryCatmullRom(vec2 uv)
{
	vec2 size = params.sizes.xy;
	vec2 position = uv * size;
	vec2 centre1 = floor(position - 0.5) + 0.5;
	vec2 f = position - centre1;

	vec2 w0 = f * (-0.5 + f * (1.0 - 0.5 * f));
	vec2 w1 = 1.0 + f * f * (-2.5 + 1.5 * f);
	vec2 w2 = f * (0.5 + f * (2.0 - 1.5 * f));
	vec2 w3 = f * f * (-0.5 + 0.5 * f);

	vec2 w12 = w1 + w2;
	vec2 uv0 = (centre1 - 1.0) / size;
	vec2 uv3 = (centre1 + 2.0) / size;
	vec2 uv12 = (centre1 + w2 / w12) / size;

	vec3 result = SampleHistory(vec2(uv12.x, uv0.y)) * (w12.x * w0.y)
		+ SampleHistory(vec2(uv0.x, uv12.y)) * (w0.x * w12.y)
		+ SampleHistory(uv12) * (w12.x * w12.y)
		+ SampleHistory(vec2(uv3.x, uv12.y)) * (w3.x * w12.y)
		+ SampleHistory(vec2(uv12.x, uv3.y)) * (w12.x * w3.y);

	float weight = w12.x * w0.y + w0.x * w12.y + w12.x * w12.y + w3.x * w12.y + w12.x * w3.y;

	// The filter's negative lobes can overshoot below nothing next to a bright edge
	return max(result / max(weight, 1e-6), vec3(0.0));
}

vec3 ToYCoCg(vec3 c)
{
	return vec3(dot(c, vec3(0.25, 0.5, 0.25)), dot(c, vec3(0.5, 0.0, -0.5)), dot(c, vec3(-0.25, 0.5, -0.25)));
}

vec3 FromYCoCg(vec3 c)
{
	return vec3(c.x + c.y - c.z, c.x + c.z, c.x - c.y - c.z);
}

float Luminance(vec3 c)
{
	return dot(c, vec3(0.2126, 0.7152, 0.0722));
}

// Clips the history towards the neighbourhood's centre until it is inside its box, rather than clamping each channel,
// which would shift its hue
vec3 ClipToBox(vec3 colour, vec3 low, vec3 high)
{
	vec3 centre = 0.5 * (high + low);
	vec3 extent = 0.5 * (high - low) + 1e-5;
	vec3 offset = colour - centre;
	vec3 units = abs(offset / extent);
	float furthest = max(units.x, max(units.y, units.z));

	return furthest > 1.0 ? centre + offset / furthest : colour;
}

// How much of FeedbackWeight is left at a view depth, the bands blended where they meet
float DepthFeedbackScale(float depth)
{
	float band01 = smoothstep(params.borders.x * 0.9, params.borders.x * 1.1, depth);
	float band12 = smoothstep(params.borders.y * 0.9, params.borders.y * 1.1, depth);
	float band23 = smoothstep(params.borders.z * 0.9, params.borders.z * 1.1, depth);

	float cancel = mix(mix(mix(params.cancel.x, params.cancel.y, band01), params.cancel.z, band12), params.cancel.w, band23);
	return clamp(1.0 - cancel, 0.0, 1.0);
}

void main()
{
	ivec2 pixel = ivec2(gl_FragCoord.xy);
	ivec2 size = ivec2(params.sizes.xy);

	float alpha = texelFetch(sampler2D(current, linearClamp), pixel, 0).a;

	// Where the jitter moved this pixel's point to, in pixels: the same NDC to pixel mapping as the reprojection below
	vec2 jitterPixels = vec2(params.jitter.x, c_clipSpaceYInverted ? params.jitter.y : -params.jitter.y) *
		params.sizes.xy * 0.5;

	// The 3 x 3 around the pixel: its range and its mean and spread in YCoCg, the plain mean for the sharpening, and
	// the pixel without the jitter, each tap weighted by how near it is to where the point was drawn (a Gaussian
	// fitted to Blackman-Harris)
	vec3 low = vec3(1e9);
	vec3 high = vec3(-1e9);
	vec3 moment1 = vec3(0.0);
	vec3 moment2 = vec3(0.0);
	vec3 mean = vec3(0.0);
	vec3 centre = vec3(0.0);
	float centreWeight = 0.0;

	for (int y = -1; y <= 1; y++)
	{
		for (int x = -1; x <= 1; x++)
		{
			ivec2 tap = clamp(pixel + ivec2(x, y), ivec2(0), size - 1);
			vec3 c = max(texelFetch(sampler2D(current, linearClamp), tap, 0).rgb, vec3(0.0));
			vec3 ycocg = ToYCoCg(c);
			low = min(low, ycocg);
			high = max(high, ycocg);
			moment1 += ycocg;
			moment2 += ycocg * ycocg;
			mean += c;

			vec2 offset = vec2(x, y) - jitterPixels;
			float weight = exp(-2.29 * dot(offset, offset));
			centre += c * weight;
			centreWeight += weight;
		}
	}

	mean /= 9.0;
	centre /= max(centreWeight, 1e-6);

	// The box the history is clipped to: the mean and spread's, but no wider than the range
	moment1 /= 9.0;
	vec3 deviation = sqrt(max(moment2 / 9.0 - moment1 * moment1, vec3(0.0)));
	low = max(low, moment1 - c_varianceGamma * deviation);
	high = min(high, moment1 + c_varianceGamma * deviation);

	// Sharpened against the neighbours, the change limited so edges don't ring
	vec3 sharpened = centre;
	if (params.settings.z > 0.0)
	{
		float range = max(params.settings.w, 0.0);
		vec3 change = clamp((centre - mean) * params.settings.z, vec3(-range) * max(Luminance(mean), 1e-3),
			vec3(range) * max(Luminance(mean), 1e-3));
		sharpened = max(centre + change, vec3(0.0));
	}

	if (params.settings.y < 0.5)
	{
		fsout_color = vec4(sharpened, alpha);
		return;
	}

	// Where this pixel's point was last frame; pixels with nothing drawn are taken at the far plane
	float stored = StoredDepth(pixel);
	vec2 uv = (vec2(pixel) + 0.5) / params.sizes.xy;
	vec2 ndc = vec2(uv.x * 2.0 - 1.0, c_clipSpaceYInverted ? uv.y * 2.0 - 1.0 : 1.0 - uv.y * 2.0);
	vec4 world = params.clipToWorld * vec4(ndc, stored, 1.0);
	world /= world.w;

	vec4 previousClip = params.previousViewProj * vec4(world.xyz, 1.0);
	vec2 previousNdc = previousClip.xy / max(abs(previousClip.w), 1e-6) * sign(previousClip.w);
	vec2 previousUv = vec2(previousNdc.x * 0.5 + 0.5,
		c_clipSpaceYInverted ? previousNdc.y * 0.5 + 0.5 : 0.5 - previousNdc.y * 0.5);

	if (previousClip.w <= 0.0 || any(lessThan(previousUv, vec2(0.0))) || any(greaterThan(previousUv, vec2(1.0))))
	{
		fsout_color = vec4(sharpened, alpha);
		return;
	}

	float depth = stored > 0.0 ? ViewDepth(stored) : 1e6;

	// Whether last frame had this surface where the point was: the depth it should have had there against the
	// nearest of last frame's around it, so an edge drawn a jitter either way still finds itself. A mesh that moved,
	// and what it uncovered, don't, and take nothing from the history.
	if (params.jitter.w > 0.5)
	{
		float expected = stored > 0.0 ? ViewDepth(previousClip.z / previousClip.w) : 1e6;
		ivec2 previousPixel = ivec2(previousUv * params.sizes.xy);
		float nearest = 1e9;

		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				ivec2 tap = clamp(previousPixel + ivec2(x, y), ivec2(0), size - 1);
				nearest = min(nearest, abs(PreviousViewDepth(tap) - expected));
			}
		}

		if (nearest > expected * c_depthTolerance)
		{
			fsout_color = vec4(sharpened, alpha);
			return;
		}
	}

	vec3 previous = SampleHistoryCatmullRom(previousUv);
	previous = FromYCoCg(ClipToBox(ToYCoCg(previous), low, high));

	float feedback = clamp(params.settings.x, 0.0, 0.99) * DepthFeedbackScale(depth);

	// Weighted by 1 / (1 + luminance), so one bright pixel doesn't dominate the blend
	float currentWeight = (1.0 - feedback) / (1.0 + Luminance(sharpened));
	float historyWeight = feedback / (1.0 + Luminance(previous));
	vec3 blended = (sharpened * currentWeight + previous * historyWeight) / max(currentWeight + historyWeight, 1e-6);

	fsout_color = vec4(blended, alpha);
}
