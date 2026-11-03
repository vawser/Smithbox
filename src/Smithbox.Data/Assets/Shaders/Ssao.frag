#version 450
#extension GL_EXT_nonuniform_qualifier : require

// The GPARAM SSAO Editor group, see SsaoPass.cs. Each draw is one step, picked by the mode:
//
// 0 occlusion: at half size, how much of the hemisphere over each pixel the scene nearby covers, from the scene
//   depth and the flvers' normals (or the depth's own slope where there are none, or without UseNormal). 16 taps
//   on a spiral, turned per pixel, out to Sample Offset x clamp(Kernel Scale Base Dist / depth, Kernel Scale Min,
//   Kernel Scale Max) pixels at 1080p. The occlusion is then saturate(raw x AO Scale + AO Offset), and faded out
//   from DepthFadeStart over DepthFadeRange, and gone past PrecisionDepth.
// 1, 2 blur: across, then down, weighted down where the depth differs by more than Depth Reject Rate of it
// 3 composite: the scene less the indirect diffuse light the occlusion covers, read from the Indirect attachment,
//   so the sun's light is left alone
//
// How Yebis reads these values isn't known; this is a reading of them. The output is the visibility, 1 open.

#define MODE_OCCLUSION 0
#define MODE_BLUR_X 1
#define MODE_BLUR_Y 2
#define MODE_COMPOSITE 3
#define TAPS 16
#define BLUR_RADIUS 4

layout (constant_id = 100) const bool c_clipSpaceYInverted = false;

layout(set = 0, binding = 0) uniform SsaoParams
{
	mat4 clipToWorld;  // the inverse of the camera's view and projection
	vec4 settings;     // x: mode, y: Sample Offset in this target's pixels, z: kernel scale min, w: max
	vec4 shape;        // x: kernel scale base distance, y: AO scale, z: AO offset, w: depth reject rate
	vec4 fade;         // x: fade start, y: fade range, z: precision depth, w: 1 to use the flvers' normals
	vec4 depth;        // xyz: projection [2][2], [3][2] and [2][3] for view depth, w: the scene depth's texture index
	vec4 eye;          // xyz: the camera, w: the projection's y scale, 1 / tan(half the vertical field of view)
	vec4 sizes;        // xy: the full size of the scene, zw: the size of the target drawn
	vec4 strength;     // x: the viewport's scale on the occlusion
} params;

layout(set = 0, binding = 1) uniform texture2D occlusion; // the step before's visibility, half size
layout(set = 0, binding = 2) uniform texture2D surface;   // SceneTarget.Surface
layout(set = 0, binding = 3) uniform texture2D indirect;  // SceneTarget.Indirect
layout(set = 0, binding = 4) uniform texture2D scene;     // the image to darken
layout(set = 0, binding = 5) uniform sampler linearClamp;

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

// The stored depth at a full size pixel, 0 where nothing was drawn
float StoredDepth(ivec2 pixel)
{
	uint index = uint(params.depth.w + 0.5);
	if (index == 0u || index >= 32768u)
		return 0.0;

	pixel = clamp(pixel, ivec2(0), ivec2(params.sizes.xy) - 1);
	return texelFetch(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), pixel, 0).r;
}

vec3 WorldPosition(ivec2 pixel, float stored)
{
	vec2 uv = (vec2(pixel) + 0.5) / params.sizes.xy;
	vec2 ndc = vec2(uv.x * 2.0 - 1.0, c_clipSpaceYInverted ? uv.y * 2.0 - 1.0 : 1.0 - uv.y * 2.0);
	vec4 world = params.clipToWorld * vec4(ndc, stored, 1.0);
	return world.xyz / world.w;
}

vec3 OctahedralDecode(vec2 e)
{
	vec3 n = vec3(e, 1.0 - abs(e.x) - abs(e.y));
	if (n.z < 0.0)
	{
		n.xy = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
	}

	return normalize(n);
}

// The normal from the depth's slope, taking the nearer neighbour on each axis so edges don't bend it
vec3 DepthNormal(ivec2 pixel, vec3 center)
{
	vec3 left = WorldPosition(pixel - ivec2(1, 0), StoredDepth(pixel - ivec2(1, 0)));
	vec3 right = WorldPosition(pixel + ivec2(1, 0), StoredDepth(pixel + ivec2(1, 0)));
	vec3 up = WorldPosition(pixel - ivec2(0, 1), StoredDepth(pixel - ivec2(0, 1)));
	vec3 down = WorldPosition(pixel + ivec2(0, 1), StoredDepth(pixel + ivec2(0, 1)));

	vec3 dx = distance(left, center) < distance(right, center) ? center - left : right - center;
	vec3 dy = distance(up, center) < distance(down, center) ? center - up : down - center;
	vec3 n = normalize(cross(dy, dx));

	// Facing the camera
	return dot(n, params.eye.xyz - center) < 0.0 ? -n : n;
}

float Visibility(ivec2 halfPixel)
{
	ivec2 pixel = halfPixel * 2;
	float stored = StoredDepth(pixel);

	if (stored <= 0.0)
		return 1.0;

	float depth = ViewDepth(stored);
	if (depth >= params.fade.z)
		return 1.0;

	vec3 center = WorldPosition(pixel, stored);

	vec4 s = texelFetch(sampler2D(surface, linearClamp), pixel, 0);
	vec3 normal = params.fade.w > 0.5 && s.z >= 0.0 ? OctahedralDecode(s.xy) : DepthNormal(pixel, center);

	// The kernel in pixels, and the world distance it spans at this depth: a pixel is 2 / (yScale x height)
	// of the view's height at a distance of 1, the same at every distance for an orthographic camera
	float kernelScale = clamp(params.shape.x / max(depth, 1e-3), params.settings.z, params.settings.w);
	float radiusPixels = max(params.settings.y * kernelScale, 1.0);
	bool orthographic = abs(params.depth.z) < 1e-6;
	float worldPerPixel = 2.0 / (max(params.eye.w, 1e-6) * params.sizes.y) * (orthographic ? 1.0 : depth);
	float radiusWorld = radiusPixels * worldPerPixel;

	// Interleaved gradient noise turns the spiral per pixel; the blur takes the noise out
	float turn = 6.2831853 * fract(52.9829189 * fract(dot(vec2(halfPixel), vec2(0.06711056, 0.00583715))));

	float occluded = 0.0;

	for (int i = 0; i < TAPS; i++)
	{
		float t = (float(i) + 0.5) / float(TAPS);
		float angle = turn + float(i) * 2.3999632; // the golden angle
		ivec2 offset = ivec2(round(vec2(cos(angle), sin(angle)) * t * radiusPixels));

		if (offset == ivec2(0))
			continue;

		float sampleStored = StoredDepth(pixel + offset);
		if (sampleStored <= 0.0)
			continue;

		vec3 v = WorldPosition(pixel + offset, sampleStored) - center;
		float vv = dot(v, v);

		// Occluders further off than the kernel reaches in the world count for less, so a wall far behind an
		// edge doesn't darken it
		float falloff = clamp(1.0 - vv / max(radiusWorld * radiusWorld, 1e-8), 0.0, 1.0);
		occluded += max(dot(v, normal) * inversesqrt(max(vv, 1e-8)) - 0.05, 0.0) * falloff;
	}

	float raw = clamp(2.0 * occluded / float(TAPS), 0.0, 1.0);
	float ao = clamp(raw * params.shape.y + params.shape.z, 0.0, 1.0) * params.strength.x;

	// Faded out with distance
	ao *= 1.0 - smoothstep(params.fade.x, params.fade.x + max(params.fade.y, 1e-3), depth);

	return 1.0 - ao;
}

float Blur(ivec2 halfPixel, ivec2 direction)
{
	float stored = StoredDepth(halfPixel * 2);
	float center = texelFetch(sampler2D(occlusion, linearClamp), halfPixel, 0).r;

	if (stored <= 0.0)
		return center;

	float depth = ViewDepth(stored);
	float tolerance = max(params.shape.w, 1e-3) * depth;
	ivec2 size = ivec2(params.sizes.zw);

	float total = center;
	float weights = 1.0;

	for (int i = 1; i <= BLUR_RADIUS; i++)
	{
		for (int side = -1; side <= 1; side += 2)
		{
			ivec2 tap = clamp(halfPixel + direction * i * side, ivec2(0), size - 1);
			float tapStored = StoredDepth(tap * 2);
			if (tapStored <= 0.0)
				continue;

			float difference = abs(ViewDepth(tapStored) - depth);
			float weight = exp(-float(i * i) / 8.0) * exp(-difference / tolerance);
			total += texelFetch(sampler2D(occlusion, linearClamp), tap, 0).r * weight;
			weights += weight;
		}
	}

	return total / weights;
}

// The half size visibility at a full size pixel, from the four texels round it, kept to those at its depth
float Upsample(ivec2 pixel, float depth)
{
	vec2 halfPosition = (vec2(pixel) + 0.5) * 0.5 - 0.5;
	ivec2 base = ivec2(floor(halfPosition));
	vec2 f = halfPosition - vec2(base);
	ivec2 halfSize = textureSize(sampler2D(occlusion, linearClamp), 0);
	float tolerance = max(params.shape.w, 1e-3) * depth;

	float total = 0.0;
	float weights = 0.0;

	for (int y = 0; y < 2; y++)
	{
		for (int x = 0; x < 2; x++)
		{
			ivec2 tap = clamp(base + ivec2(x, y), ivec2(0), halfSize - 1);
			float bilinear = (x == 0 ? 1.0 - f.x : f.x) * (y == 0 ? 1.0 - f.y : f.y);
			float tapStored = StoredDepth(tap * 2);
			float difference = tapStored > 0.0 ? abs(ViewDepth(tapStored) - depth) : 1e6;
			float weight = bilinear * exp(-difference / tolerance) + 1e-5 * bilinear;

			total += texelFetch(sampler2D(occlusion, linearClamp), tap, 0).r * weight;
			weights += weight;
		}
	}

	return weights > 0.0 ? total / weights : 1.0;
}

void main()
{
	ivec2 pixel = ivec2(gl_FragCoord.xy);
	int mode = int(params.settings.x + 0.5);

	if (mode == MODE_OCCLUSION)
	{
		fsout_color = vec4(Visibility(pixel), 0.0, 0.0, 1.0);
	}
	else if (mode == MODE_BLUR_X)
	{
		fsout_color = vec4(Blur(pixel, ivec2(1, 0)), 0.0, 0.0, 1.0);
	}
	else if (mode == MODE_BLUR_Y)
	{
		fsout_color = vec4(Blur(pixel, ivec2(0, 1)), 0.0, 0.0, 1.0);
	}
	else
	{
		vec4 color = texelFetch(sampler2D(scene, linearClamp), pixel, 0);
		float stored = StoredDepth(pixel);

		if (stored <= 0.0)
		{
			fsout_color = color;
			return;
		}

		float visibility = Upsample(pixel, ViewDepth(stored));
		vec3 ambient = texelFetch(sampler2D(indirect, linearClamp), pixel, 0).rgb;
		fsout_color = vec4(max(color.rgb - ambient * (1.0 - visibility), vec3(0.0)), color.a);
	}
}
