#version 450
#extension GL_EXT_nonuniform_qualifier : require

// The GPARAM LightShaft[Yebis] group, see LightShaftPass.cs. Each draw is one step, picked by the mode:
//
// 0 mask: at half size, the sky around the sun. A pixel is sky where nothing was drawn or the scene is further than
//   MaskDepthThreshold; it is weighted by how near its ray is to the sun, falling to 0 at SkyMaskSizeAngle with
//   SkyMaskCenterPower. Its color is the sky over SkyColorThreshold (after the auto exposure), greyed by
//   SkyColorSaturate, x Sky Multiply Color, plus Sky Add Color.
// 1 blur: the mask smeared towards the sun, ShaftBlurCount times, each pass taking 16 taps over a sixteenth of the
//   last one's length, weighted down along the shaft by ShaftBlurFallOff. Between the Parallel mode angles from the
//   sun, the smear turns from towards the sun's point on screen to the sun's direction across it.
// 2 composite: the scene plus the shafts, faded out between the Fade out angles from the sun.
//
// How Yebis reads these values isn't known; this is a reading of them. Colors are rgb x their intensity in w.

#define MODE_MASK 0
#define MODE_BLUR 1
#define MODE_COMPOSITE 2
#define TAPS 16

layout (constant_id = 100) const bool c_clipSpaceYInverted = false;

layout(set = 0, binding = 0) uniform LightShaftParams
{
	mat4 clipToWorld;  // the inverse of the camera's view and projection
	vec4 settings;     // x: mode, y: mask depth threshold, z: cos of the sky mask size angle, w: center power
	vec4 sun;          // xy: the sun's point on screen (0-1), z: how far towards the parallel blur, w: fade
	vec4 blur;         // xy: the parallel blur over the screen, z: the radial blur's reach, w: falloff
	vec4 multiply;     // rgb: Sky Multiply Color x w, w: sky color threshold
	vec4 add;          // rgb: Sky Add Color x w, with the exposure as far as ExposureInfluenceRate says, w: saturate
	vec4 depth;        // xyz: projection [2][2], [3][2] and [2][3] for view depth, w: the scene depth's texture index
	vec4 toSun;        // xyz: the direction towards the sun, w: this blur pass's share of the whole length
	vec4 eye;          // xyz: the camera
	vec4 sizes;        // zw: the size of the target drawn
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

layout(set = 1, binding = 0) uniform texture2D globalTextures[];

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

// Distance from the camera along its axis of a value in the depth buffer, as VolumetricFog.frag finds it
float ViewDepth(float stored)
{
	// Orthographic cameras keep w at 1
	if (abs(params.depth.z) < 1e-6)
	{
		return abs((stored - params.depth.y) / params.depth.x);
	}

	return abs(params.depth.y / (stored * params.depth.z - params.depth.x));
}

// Whether the pixel at uv is sky: nothing drawn there, or the scene past the threshold
bool IsSky(vec2 uv)
{
	uint index = uint(params.depth.w + 0.5);

	if (index == 0u || index >= 32768u)
	{
		return true;
	}

	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), 0);
	ivec2 texel = clamp(ivec2(uv * vec2(size)), ivec2(0), size - 1);
	float stored = texelFetch(sampler2D(globalTextures[nonuniformEXT(index)], linearClamp), texel, 0).r;

	// Reverse depth clears to 0
	return stored <= 0.0 || ViewDepth(stored) > params.settings.y;
}

vec3 Mask(vec2 uv)
{
	if (!IsSky(uv))
	{
		return vec3(0.0);
	}

	// The pixel's ray, from two points on it
	vec2 ndc = vec2(uv.x * 2.0 - 1.0, c_clipSpaceYInverted ? uv.y * 2.0 - 1.0 : 1.0 - uv.y * 2.0);
	vec4 a = params.clipToWorld * vec4(ndc, 1.0, 1.0);
	vec4 b = params.clipToWorld * vec4(ndc, 0.5, 1.0);
	vec3 ray = normalize(b.xyz / b.w - a.xyz / a.w);

	// 1 at the sun, 0 at the edge of the mask
	float cosAngle = dot(ray, params.toSun.xyz);
	float weight = clamp((cosAngle - params.settings.z) / max(1.0 - params.settings.z, 1e-4), 0.0, 1.0);
	weight = pow(weight, max(params.settings.w, 1e-3));

	if (weight <= 0.0)
	{
		return vec3(0.0);
	}

	// The threshold is on what the auto exposure will show, then the scene's units are kept
	float autoExposure = exposureState.valid > 0.5 ? exp2(exposureState.ev) : 1.0;
	vec3 sky = SampleA(uv) * autoExposure;
	float luminance = Luminance(sky);
	sky *= max(luminance - params.multiply.w, 0.0) / max(luminance, 1e-6);
	sky = mix(sky, vec3(Luminance(sky)), clamp(params.add.w, 0.0, 1.0));

	return (sky / autoExposure * params.multiply.rgb + params.add.rgb) * weight;
}

vec3 Blur(vec2 uv)
{
	// Towards the sun's point on screen, or across the screen in the sun's direction
	vec2 radial = (params.sun.xy - uv) * params.blur.z;
	vec2 direction = mix(radial, params.blur.xy, params.sun.z) * params.toSun.w;

	vec3 total = vec3(0.0);
	float weights = 0.0;

	for (int i = 0; i < TAPS; i++)
	{
		float t = float(i) / float(TAPS);
		vec2 tap = uv + direction * t;

		if (any(lessThan(tap, vec2(0.0))) || any(greaterThan(tap, vec2(1.0))))
			break;

		// Down along the whole shaft, of which this pass covers toSun.w
		float weight = exp(-params.blur.w * t * params.toSun.w);
		total += SampleA(tap) * weight;
		weights += weight;
	}

	return weights > 0.0 ? total / weights : vec3(0.0);
}

void main()
{
	// Every texture is read by where the pixel is in the target drawn, so the viewport's flip doesn't matter
	vec2 uv = gl_FragCoord.xy / params.sizes.zw;
	int mode = int(params.settings.x + 0.5);

	if (mode == MODE_MASK)
	{
		fsout_color = vec4(Mask(uv), 1.0);
	}
	else if (mode == MODE_BLUR)
	{
		fsout_color = vec4(Blur(uv), 1.0);
	}
	else
	{
		vec4 scene = textureLod(sampler2D(textureA, linearClamp), uv, 0.0);
		fsout_color = vec4(scene.rgb + SampleB(uv) * params.sun.w, scene.a);
	}
}
