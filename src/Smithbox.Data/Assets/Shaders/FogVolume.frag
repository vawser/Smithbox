#version 450
#extension GL_GOOGLE_include_directive : require
#extension GL_EXT_nonuniform_qualifier : require

// Fragment stage of the fog volume shader.
//
// Each pixel of the box marches along the view ray from where it enters the box to where it leaves, or to the
// scene behind it if that is nearer, read from the scene depth. The fog is the "FogVolume ParamEditor" row of a
// GPARAM, see FogVolumeMaterial.cs. It is an approximation of the game's fog: Perlin noise scrolled over time,
// faded at the edges of the box, by the camera distance, with height and under the height map, and lit by the
// directional light colors facing towards and away from the sun. The ray is cut into SteppingDistance steps,
// taken as fractions of the ray through the box, and stops once the light let through falls below
// SuspendedTransmittance. Shadowing, the light processing interval and the attenuation values are not modelled.
//
// Both sides of the box are drawn without depth testing, so only the far side is kept: there the pixel is where
// the ray leaves the box.

#define MAX_SCENE_LIGHTS 16
#define MAX_STEPS 64

layout(location = 0) in vec3 fsin_worldPos;
layout(location = 1) flat in uint fsin_mat;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];

struct fogMaterial
{
	mat4 inverseWorld;    // world to the unit box, -0.5 to 0.5 on each axis
	vec4 sizeDensity;     // xyz: size of the box, w: density
	vec4 fade;            // near fade min, near fade max, far fade min, far fade max
	vec4 shape;           // edge fade distance, edge fade exponent, height falloff, height map fade distance
	vec4 noise;           // frequency, lacunarity, gain, offset
	vec4 scroll;          // xyz: speed, w: detail scale
	vec4 directionalColor;
	vec4 oppositeColor;
	vec4 ambientColor;
	vec4 light;           // xy: directional light angle, z: 1 if the angle overrides the sun, w: color map intensity
	vec4 colorMap;        // xy: color map tiling, z: attenuation intensity, w: attenuation offset
	uvec4 textures;       // x: height map, y: color map, z: noise octaves, w: max sample count
	vec4 misc;            // x: max injecting depth, 0 for none, y: stepping distance, z: suspended transmittance
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly fogMaterial matdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;

int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// Distance from the camera along its axis of a value in the depth buffer
float ViewDepth(float depth)
{
	mat4 projection = sceneparam.projection;

	// Orthographic cameras keep w at 1
	if (abs(projection[2][3]) < 1e-6)
	{
		return abs((depth - projection[3][2]) / projection[2][2]);
	}

	return abs(projection[3][2] / (depth * projection[2][3] - projection[2][2]));
}

vec3 Hash3(vec3 p)
{
	p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
	         dot(p, vec3(269.5, 183.3, 246.1)),
	         dot(p, vec3(113.5, 271.9, 124.6)));

	return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

// Gradient noise, roughly -1 to 1
float Perlin(vec3 p)
{
	vec3 i = floor(p);
	vec3 f = fract(p);
	vec3 u = f * f * (3.0 - 2.0 * f);

	return mix(mix(mix(dot(Hash3(i + vec3(0, 0, 0)), f - vec3(0, 0, 0)),
	                   dot(Hash3(i + vec3(1, 0, 0)), f - vec3(1, 0, 0)), u.x),
	               mix(dot(Hash3(i + vec3(0, 1, 0)), f - vec3(0, 1, 0)),
	                   dot(Hash3(i + vec3(1, 1, 0)), f - vec3(1, 1, 0)), u.x), u.y),
	           mix(mix(dot(Hash3(i + vec3(0, 0, 1)), f - vec3(0, 0, 1)),
	                   dot(Hash3(i + vec3(1, 0, 1)), f - vec3(1, 0, 1)), u.x),
	               mix(dot(Hash3(i + vec3(0, 1, 1)), f - vec3(0, 1, 1)),
	                   dot(Hash3(i + vec3(1, 1, 1)), f - vec3(1, 1, 1)), u.x), u.y), u.z) * 1.4;
}

// Fractal noise scrolled over time, 0 to 1. The detail scale slows the scroll of the finer octaves
float FogNoise(fogMaterial m, vec3 worldPos)
{
	uint octaves = clamp(m.textures.z, 1u, 6u);
	float frequency = m.noise.x;
	float amplitude = 1.0;
	float total = 0.0;
	float weight = 0.0;

	for (uint i = 0u; i < octaves; i++)
	{
		vec3 speed = m.scroll.xyz * (i == 0u ? 1.0 : m.scroll.w);
		total += Perlin((worldPos - speed * sceneparam.materialTime) * frequency) * amplitude;
		weight += amplitude;
		frequency *= m.noise.y;
		amplitude *= m.noise.z;
	}

	return clamp(total / max(weight, 1e-4) * 0.5 + 0.5 + m.noise.w, 0.0, 1.0);
}

// How much fog there is at a point of the box, given in the box's space and in the world
float Density(fogMaterial m, vec3 local, vec3 worldPos)
{
	vec3 size = m.sizeDensity.xyz;

	// Distance to the nearest side of the box, in meters
	vec3 inside = (0.5 - abs(local)) * size;
	float edge = min(inside.x, min(inside.y, inside.z));
	float edgeFade = pow(clamp(edge / max(m.shape.x, 1e-3), 0.0, 1.0), max(m.shape.y, 1e-3));

	// Thinner towards the top of the box
	float height01 = local.y + 0.5;
	float heightFade = exp(-m.shape.z * height01);

	// The height map is the surface of the fog, seen from above. It fades out over the fade distance above it
	float heightMap = 1.0;
	if (m.textures.x != 0u)
	{
		float surface = textureLod(sampler2D(globalTextures[nonuniformEXT(TextureIndex(m.textures.x))], linearSampler),
			local.xz + 0.5, 0).r * size.y;
		float above = height01 * size.y - surface;

		heightMap = m.shape.w > 0.0
			? 1.0 - clamp(above / m.shape.w, 0.0, 1.0)
			: step(above, 0.0);
	}

	return m.sizeDensity.w * FogNoise(m, worldPos) * edgeFade * heightFade * heightMap;
}

void main()
{
	fogMaterial m = matdata[fsin_mat];

	vec3 eye = sceneparam.eye.xyz;
	vec3 toPixel = fsin_worldPos - eye;

	// The ray in the box's space, where s = 0 at the camera and s = 1 at this pixel
	vec3 origin = (m.inverseWorld * vec4(eye, 1.0)).xyz;
	vec3 dir = (m.inverseWorld * vec4(fsin_worldPos, 1.0)).xyz - origin;

	// Axes the ray runs along are nudged off zero, so their slabs give infinities of the right sign
	vec3 safeDir = mix(dir, vec3(1e-8), lessThan(abs(dir), vec3(1e-8)));
	vec3 invDir = 1.0 / safeDir;
	vec3 t0 = (vec3(-0.5) - origin) * invDir;
	vec3 t1 = (vec3(0.5) - origin) * invDir;
	vec3 tMin = min(t0, t1);
	vec3 tMax = max(t0, t1);
	float sEnter = max(max(tMin.x, tMin.y), max(tMin.z, 0.0));
	float sExit = min(min(tMax.x, tMax.y), tMax.z);

	// Keep only the far side of the box, where the ray leaves it. On the near side it leaves further on
	if (sExit > 1.001)
	{
		discard;
	}

	float sEnd = min(sExit, 1.0);

	// Stop at the scene in front of the far side
	if (sceneparam.sceneDepthTexture != 0u)
	{
		float stored = texelFetch(
			sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], linearSampler),
			ivec2(gl_FragCoord.xy), 0).r;

		// Reverse depth clears to 0, where there is nothing but sky
		if (stored > 0.0)
		{
			sEnd = min(sEnd, ViewDepth(stored) / max(ViewDepth(gl_FragCoord.z), 1e-4));
		}
	}

	float rayLength = length(toPixel);
	if (m.misc.x > 0.0)
	{
		sEnd = min(sEnd, m.misc.x / max(rayLength, 1e-4));
	}

	if (sEnd <= sEnter)
	{
		discard;
	}

	vec3 rayDir = toPixel / max(rayLength, 1e-4);

	// Light from the sun's side of the fog, or from the side away from it
	vec3 sunTravel = dot(sceneparam.lightDirection.xyz, sceneparam.lightDirection.xyz) > 1e-10
		? normalize(sceneparam.lightDirection.xyz) : vec3(0.0, -1.0, 0.0);

	if (m.light.z > 0.5)
	{
		vec2 angle = m.light.xy;
		sunTravel = -vec3(cos(angle.x) * sin(angle.y), sin(angle.x), cos(angle.x) * cos(angle.y));
	}

	float towardsSun = clamp(dot(rayDir, -sunTravel) * 0.5 + 0.5, 0.0, 1.0);
	vec3 lightColor = m.ambientColor.rgb + mix(m.oppositeColor.rgb * m.oppositeColor.a, m.directionalColor.rgb * m.directionalColor.a, towardsSun);

	// SteppingDistance 0.1 is 10 steps, no more than MaxSampleCount
	uint steps = m.misc.y > 0.0
		? clamp(min(m.textures.w, uint(ceil(1.0 / m.misc.y))), 4u, uint(MAX_STEPS))
		: clamp(m.textures.w, 8u, uint(MAX_STEPS));
	float stepS = (sEnd - sEnter) / float(steps);
	float stepLength = stepS * rayLength;

	// Start each pixel at a different point of its first step, so the steps don't show as bands
	float jitter = fract(sin(dot(gl_FragCoord.xy, vec2(12.9898, 78.233))) * 43758.5453);

	float transmittance = 1.0;
	vec3 scattered = vec3(0.0);

	for (uint i = 0u; i < steps; i++)
	{
		float s = sEnter + (float(i) + jitter) * stepS;
		vec3 local = origin + dir * s;
		vec3 worldPos = eye + toPixel * s;
		float distance = s * rayLength;

		float density = Density(m, local, worldPos);

		// Near and far fades
		if (m.fade.y > m.fade.x)
		{
			density *= smoothstep(m.fade.x, m.fade.y, distance);
		}

		if (m.fade.w > m.fade.z)
		{
			density *= 1.0 - smoothstep(m.fade.z, m.fade.w, distance);
		}

		if (density <= 0.0)
		{
			continue;
		}

		vec3 color = lightColor;
		if (m.textures.y != 0u)
		{
			vec3 colorMap = textureLod(sampler2D(globalTextures[nonuniformEXT(TextureIndex(m.textures.y))], linearSampler),
				(local.xz + 0.5) * m.colorMap.xy, 0).rgb;
			color *= mix(vec3(1.0), colorMap, clamp(m.light.w, 0.0, 1.0));
		}

		float stepTransmittance = exp(-density * stepLength);
		scattered += transmittance * (1.0 - stepTransmittance) * color;
		transmittance *= stepTransmittance;

		if (transmittance < (m.misc.z > 0.0 ? m.misc.z : 0.001))
		{
			break;
		}
	}

	float alpha = 1.0 - transmittance;
	if (alpha < 1e-4)
	{
		discard;
	}

	// Blended as color * alpha + scene * (1 - alpha), so the light gathered is divided back out
	fsout_color = vec4(scattered / alpha * sceneparam.sceneBrightness, alpha);

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
