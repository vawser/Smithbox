#version 450
#extension GL_EXT_nonuniform_qualifier : require

// The GPARAM SSR ParamEditor group, see SsrPass.cs. Each draw is one step, picked by the mode:
//
// 0 trace: at half size, each glossy pixel's reflected ray marched across the screen against the scene depth.
//   Pixels are left out where shininess (1 - roughness) is under Shininess Reject Threshold, Schlick's Fresnel under
//   Fresnel Reject Threshold, or the depth past Depth Reject Threshold. The steps grow from Ray Trace Step Scale Min to
//   Max pixels at 1080p; the ray is Depth Fade Range metres long and fades out over its second half. A hit is where
//   the ray passes behind the scene by no more than Geom Crack Detect Factor of its depth, refined by halving. The
//   color is read from the scene's mips at Display Mip Offset + Display Mip Scale x the flver shader's own mip for
//   the roughness. Rays coming back at the camera fade out from Fade To Viewer Start, and hits near the screen's
//   edges fade too. Stored as color x weight, weight.
// 1 composite: the environment's reflection in the Reflection attachment swapped for the traced one, with the flver
//   shader's Fresnel term, by the weight. Where nothing was hit the environment's stays.
//
// How Yebis reads these values isn't known; this is a reading of them.

#define MODE_TRACE 0
#define MODE_COMPOSITE 1
#define MAX_STEPS 64
#define REFINE_STEPS 5
#define FLVER_MIP_RANGE 6.5

layout (constant_id = 100) const bool c_clipSpaceYInverted = false;

layout(set = 0, binding = 0) uniform SsrParams
{
	mat4 clipToWorld;  // the inverse of the camera's view and projection
	mat4 viewProjection;
	vec4 settings;     // x: mode, y: shininess reject, z: Fresnel reject, w: fade to viewer start
	vec4 trace;        // x: step min in the scene's pixels, y: step max, z: ray length, w: crack detect factor
	vec4 mip;          // x: mip scale, y: mip offset, z: the last mip, w: depth reject threshold
	vec4 depth;        // xyz: projection [2][2], [3][2] and [2][3] for view depth, w: the scene depth's texture index
	vec4 eye;          // xyz: the camera
	vec4 sizes;        // xy: the full size of the scene, zw: the size of the target drawn
	vec4 strength;     // x: the viewport's scale on the reflections
} params;

layout(set = 0, binding = 1) uniform texture2D colorMips;  // the image before the reflections, with mips
layout(set = 0, binding = 2) uniform texture2D surface;    // SceneTarget.Surface
layout(set = 0, binding = 3) uniform texture2D reflection; // SceneTarget.Reflection
layout(set = 0, binding = 4) uniform texture2D scene;      // the image to put the reflections in
layout(set = 0, binding = 5) uniform texture2D traced;     // the trace step's result, half size
layout(set = 0, binding = 6) uniform sampler linearClamp;

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

	pixel = clamp(pixel, ivec2(0), ivec2(params.sizes.xy) - 1);
	return texelFetch(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), pixel, 0).r;
}

vec3 WorldPosition(vec2 pixel, float stored)
{
	vec2 uv = pixel / params.sizes.xy;
	vec2 ndc = vec2(uv.x * 2.0 - 1.0, c_clipSpaceYInverted ? uv.y * 2.0 - 1.0 : 1.0 - uv.y * 2.0);
	vec4 world = params.clipToWorld * vec4(ndc, stored, 1.0);
	return world.xyz / world.w;
}

// Where a clip space point lands, in the scene's pixels as gl_FragCoord runs
vec2 ToPixels(vec4 clip)
{
	vec2 ndc = clip.xy / clip.w;
	vec2 uv = vec2(ndc.x * 0.5 + 0.5, c_clipSpaceYInverted ? ndc.y * 0.5 + 0.5 : 0.5 - ndc.y * 0.5);
	return uv * params.sizes.xy;
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

// FlverShader.frag's Fresnel term for its environment reflection, from the largest of F0's channels. Its gloss
// squared is the flver shader's own dimming of rough surfaces' reflections, so the swap uses it to match.
float Fresnel(float NdotV, float roughness, float F0)
{
	float gloss = 1.0 - roughness;
	return pow(1.0 - NdotV, 5.0) * gloss * gloss * (1.0 - F0) + F0;
}

// Schlick's Fresnel, which Fresnel Reject Threshold is held against: roughness is Shininess Reject Threshold's to
// test, so it isn't counted twice
float SchlickFresnel(float NdotV, float F0)
{
	return pow(1.0 - NdotV, 5.0) * (1.0 - F0) + F0;
}

struct Pixel
{
	bool valid;
	vec3 position;
	vec3 normal;
	vec3 view;
	float roughness;
	float fresnel;
	float depth;
};

Pixel ReadPixel(ivec2 pixel)
{
	Pixel p;
	p.valid = false;

	float stored = StoredDepth(pixel);
	vec4 s = texelFetch(sampler2D(surface, linearClamp), pixel, 0);

	if (stored <= 0.0 || s.z < 0.0)
		return p;

	p.depth = ViewDepth(stored);
	p.position = WorldPosition(vec2(pixel) + 0.5, stored);
	p.normal = OctahedralDecode(s.xy);
	p.view = normalize(params.eye.xyz - p.position);
	p.roughness = clamp(s.z, 0.0, 1.0);

	float NdotV = clamp(dot(p.normal, p.view), 1e-4, 1.0);
	p.fresnel = Fresnel(NdotV, p.roughness, s.w);

	p.valid = p.depth <= params.mip.w && 1.0 - p.roughness >= params.settings.y &&
		SchlickFresnel(NdotV, s.w) >= params.settings.z;
	return p;
}

// The camera's forward, from the same matrix the positions are unprojected with, so the depths along it agree
// with the depth buffer's however the view matrix is set up
vec3 CameraForward()
{
	vec2 center = params.sizes.xy * 0.5;
	return normalize(WorldPosition(center, 0.5) - WorldPosition(center, 1.0));
}

float ForwardDepth(vec3 worldPos, vec3 forward)
{
	return dot(worldPos - params.eye.xyz, forward);
}

vec4 Trace(ivec2 halfPixel)
{
	ivec2 pixel = halfPixel * 2;
	Pixel p = ReadPixel(pixel);

	if (!p.valid)
		return vec4(0.0);

	vec3 ray = reflect(-p.view, p.normal);
	vec3 forward = CameraForward();

	// Rays coming back at the camera find what is behind it, which the screen doesn't have
	float weight = 1.0 - smoothstep(params.settings.w, params.settings.w + 0.25, dot(ray, p.view));
	if (weight <= 0.0)
		return vec4(0.0);

	// Kept in front of the camera, so the end projects
	float length = max(params.trace.z, 0.1);
	float startDepth = ForwardDepth(p.position, forward);
	float along = dot(ray, forward);
	float nearest = 0.2;

	if (startDepth + length * along < nearest)
	{
		length = (nearest - startDepth) / along * 0.99;
	}

	vec3 end = p.position + ray * length;
	vec4 clipStart = params.viewProjection * vec4(p.position, 1.0);
	vec4 clipEnd = params.viewProjection * vec4(end, 1.0);
	vec2 screenStart = ToPixels(clipStart);
	vec2 screenEnd = ToPixels(clipEnd);
	float screenLength = distance(screenStart, screenEnd);

	if (screenLength < 1.0)
		return vec4(0.0);

	float endDepth = ForwardDepth(end, forward);
	float crack = max(params.trace.w, 1.0);

	// A screen fraction to the fraction along the ray, as the perspective divides it
	float travelled = 0.0;
	float previous = 0.0;
	float hit = -1.0;

	for (int i = 0; i < MAX_STEPS; i++)
	{
		float stepPixels = mix(params.trace.x, params.trace.y, float(i) / float(MAX_STEPS - 1));
		travelled += max(stepPixels, 1.0);

		if (travelled >= screenLength)
			break;

		float s = travelled / screenLength;
		float t = (s / clipEnd.w) / ((1.0 - s) / clipStart.w + s / clipEnd.w);
		vec2 at = mix(screenStart, screenEnd, s);

		if (any(lessThan(at, vec2(0.0))) || any(greaterThanEqual(at, params.sizes.xy)))
			break;

		float rayDepth = mix(startDepth, endDepth, t);
		float sceneStored = StoredDepth(ivec2(at));

		if (sceneStored > 0.0)
		{
			float sceneDepth = ViewDepth(sceneStored);

			if (rayDepth > sceneDepth && rayDepth < sceneDepth * crack + 0.05)
			{
				hit = s;
				break;
			}
		}

		previous = s;
	}

	if (hit < 0.0)
		return vec4(0.0);

	// Halved back towards where the ray went behind
	float low = previous;
	float high = hit;

	for (int i = 0; i < REFINE_STEPS; i++)
	{
		float s = 0.5 * (low + high);
		float t = (s / clipEnd.w) / ((1.0 - s) / clipStart.w + s / clipEnd.w);
		float rayDepth = mix(startDepth, endDepth, t);
		float sceneStored = StoredDepth(ivec2(mix(screenStart, screenEnd, s)));

		if (sceneStored > 0.0 && rayDepth > ViewDepth(sceneStored))
			high = s;
		else
			low = s;
	}

	float t = (high / clipEnd.w) / ((1.0 - high) / clipStart.w + high / clipEnd.w);
	vec2 uv = mix(screenStart, screenEnd, high) / params.sizes.xy;

	// Faded at the screen's edges and over the second half of the ray
	vec2 edge = min(uv, 1.0 - uv);
	weight *= smoothstep(0.0, 0.08, min(edge.x, edge.y));
	weight *= 1.0 - smoothstep(0.5, 1.0, t);

	float mip = clamp(params.mip.y + params.mip.x * FLVER_MIP_RANGE * p.roughness, 0.0, params.mip.z);
	vec3 color = max(textureLod(sampler2D(colorMips, linearClamp), uv, mip).rgb, vec3(0.0));

	return vec4(color * weight, weight);
}

void main()
{
	ivec2 pixel = ivec2(gl_FragCoord.xy);
	int mode = int(params.settings.x + 0.5);

	if (mode == MODE_TRACE)
	{
		fsout_color = Trace(pixel);
		return;
	}

	vec4 color = texelFetch(sampler2D(scene, linearClamp), pixel, 0);
	Pixel p = ReadPixel(pixel);

	if (!p.valid)
	{
		fsout_color = color;
		return;
	}

	// The trace is color x weight, so its filtering doesn't darken where neighbours missed
	vec4 t = textureLod(sampler2D(traced, linearClamp), (vec2(pixel) + 0.5) / params.sizes.xy, 0.0);
	float weight = clamp(t.a, 0.0, 1.0) * params.strength.x;

	if (weight <= 0.0)
	{
		fsout_color = color;
		return;
	}

	vec3 tracedColor = t.rgb / max(t.a, 1e-4);
	vec3 environment = texelFetch(sampler2D(reflection, linearClamp), pixel, 0).rgb;
	vec3 swapped = color.rgb + (tracedColor * p.fresnel - environment) * weight;

	fsout_color = vec4(max(swapped, vec3(0.0)), color.a);
}
