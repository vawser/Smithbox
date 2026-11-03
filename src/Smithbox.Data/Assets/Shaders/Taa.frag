#version 450
#extension GL_EXT_nonuniform_qualifier : require

// The GPARAM Anti-Alias-Editor group's TAA (AntiAliasType 5), see TaaPass.cs. The scene is drawn a sub-pixel off each
// frame (JitterCount Halton offsets); this blends each frame into the history of the ones before:
//
// - the history is read where the pixel was last frame, found from the scene depth with the camera's unjittered
//   matrices (map geometry doesn't move, so the camera's motion is all there is)
// - it is clamped to the colors around the pixel this frame, in YCoCg, so what moved or appeared doesn't smear
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
} params;

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

	vec3 centre = max(texelFetch(sampler2D(current, linearClamp), pixel, 0).rgb, vec3(0.0));
	float alpha = texelFetch(sampler2D(current, linearClamp), pixel, 0).a;

	// The 3 x 3 around the pixel: its box in YCoCg, and the plain mean for the sharpening
	vec3 low = vec3(1e9);
	vec3 high = vec3(-1e9);
	vec3 mean = vec3(0.0);

	for (int y = -1; y <= 1; y++)
	{
		for (int x = -1; x <= 1; x++)
		{
			ivec2 tap = clamp(pixel + ivec2(x, y), ivec2(0), size - 1);
			vec3 c = max(texelFetch(sampler2D(current, linearClamp), tap, 0).rgb, vec3(0.0));
			vec3 ycocg = ToYCoCg(c);
			low = min(low, ycocg);
			high = max(high, ycocg);
			mean += c;
		}
	}

	mean /= 9.0;

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

	vec3 previous = max(textureLod(sampler2D(history, linearClamp), previousUv, 0.0).rgb, vec3(0.0));
	previous = FromYCoCg(ClipToBox(ToYCoCg(previous), low, high));

	float depth = stored > 0.0 ? ViewDepth(stored) : 1e6;
	float feedback = clamp(params.settings.x, 0.0, 0.99) * DepthFeedbackScale(depth);

	// Weighted by 1 / (1 + luminance), so one bright pixel doesn't dominate the blend
	float currentWeight = (1.0 - feedback) / (1.0 + Luminance(sharpened));
	float historyWeight = feedback / (1.0 + Luminance(previous));
	vec3 blended = (sharpened * currentWeight + previous * historyWeight) / max(currentWeight + historyWeight, 1e-6);

	fsout_color = vec4(blended, alpha);
}
