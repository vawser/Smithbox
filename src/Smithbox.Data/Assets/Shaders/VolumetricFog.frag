#version 450
#extension GL_EXT_nonuniform_qualifier : require

// Lays the GPARAM VolumetricFog over the scene, see VolumetricFogPass.cs. Drawn into the HDR target after the opaque
// draws, at the copy of their depth, so it lands under the transparent ones. Each pixel reads the froxel volume
// VolumetricFog.comp filled at its depth: the light scattered towards the camera, added, and the share of the scene
// let through, which the blend multiplies the scene by. Past the volume's far depth, with EnableAnalyticalFog, the
// same fog is marched out to FogEndDepth, lit by the sky and the unshadowed sun. Pixels with nothing behind them
// take the whole range.

#define MAX_SCENE_LIGHTS 16
#define FAR_STEPS 8

layout(location = 0) out vec4 fsout_color;

layout (constant_id = 100) const bool c_clipSpaceYInverted = false;

struct sceneLight
{
	vec4 positionRange;
	vec4 directionType;
	vec4 colorIntensity;
	vec4 spot;
};

// Only as far as the scene depth, which is all this reads
struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
	ivec4 curserPosition;
	uint envmap;

	float ambientLightMult;
	float directLightMult;
	float indirectLightMult;
	float emissiveMapMult;
	float sceneBrightness;
	float simpleFlverBrightness;
	float simpleFlverSaturation;
	vec4 selectionColor;
	vec4 outlineColor;
	uint EnableDithering;
	uint EnableTinting;
	float ditherOpacity;

	uint lightCount;
	vec4 sunColor;
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
	float windPeriod;
	uint materialFeatures;
	uint sceneDepthTexture; // index in globalTextures of the depth after the opaque draws, 0 if there is none
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
	sceneParams sceneparam;
};

layout(set = 1, binding = 0) uniform texture2D globalTextures[];

// Laid out as VolumetricFogParam, see VolumetricFog.comp
struct fogParams
{
	mat4 clipToWorld;
	vec4 volume;
	vec4 depthRange;
	vec4 density;
	vec4 extinction;
	vec4 sky;
	vec4 direct;
	vec4 opposite;
	vec4 start;
	vec4 sun;
	vec4 flags;
};

layout(set = 2, binding = 0) uniform FogParamBuffer
{
	fogParams fog;
};

layout(set = 2, binding = 1) uniform texture3D volume;
layout(set = 2, binding = 2) uniform sampler linearClamp;

int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// Distance from the camera along its axis of a value in the depth buffer, as FogVolume.frag finds it
float ViewDepth(float depth)
{
	mat4 projection = sceneparam.projection;

	if (abs(projection[2][3]) < 1e-6)
	{
		return abs((depth - projection[3][2]) / projection[2][2]);
	}

	return abs(projection[3][2] / (depth * projection[2][3] - projection[2][2]));
}

vec3 Unproject(vec2 ndc, float depth)
{
	vec4 world = fog.clipToWorld * vec4(ndc, depth, 1.0);
	return world.xyz / world.w;
}

float Density(float y)
{
	float height = clamp(-fog.density.z * (y - fog.density.w), -80.0, 80.0);
	return clamp(fog.density.x + fog.density.y * exp(height), 0.0, 4.0);
}

float Phase(float cosAngle, float g)
{
	float gg = g * g;
	float peak = (1.0 - gg) / pow(max(1.0 - g, 1e-3), 3.0);
	return (1.0 - gg) / pow(max(1.0 + gg - 2.0 * g * cosAngle, 1e-6), 1.5) / peak;
}

float HeightAttenuation(float y, float attenuation)
{
	return exp(-max(attenuation, 0.0) * max(y - fog.density.w, 0.0));
}

// The volume at a view depth: rgb scattered, a let through. Slice i holds the fog up to its far side, so before
// the first slice's far side the fog is faded in from the camera.
vec4 SampleVolume(vec2 uv, float depth)
{
	float slices = fog.volume.z;
	float s = log(max(depth, fog.depthRange.x) / fog.depthRange.x) / fog.depthRange.z * slices;
	float index = clamp(s - 1.0, 0.0, slices - 1.0);
	vec4 value = textureLod(sampler3D(volume, linearClamp), vec3(uv, (index + 0.5) / slices), 0.0);

	return s < 1.0 ? mix(vec4(0.0, 0.0, 0.0, 1.0), value, clamp(s, 0.0, 1.0)) : value;
}

// The fog from one view depth to another along the pixel's ray, lit by the sky and the unshadowed sun
vec4 MarchFar(vec2 ndc, float fromDepth, float toDepth)
{
	vec3 a = Unproject(ndc, 1.0);
	vec3 b = Unproject(ndc, 0.5);
	float depthA = abs((sceneparam.view * vec4(a, 1.0)).z);
	float depthB = abs((sceneparam.view * vec4(b, 1.0)).z);
	vec3 perDepth = (b - a) / max(abs(depthB - depthA), 1e-6) * sign(depthB - depthA);
	float rayScale = length(perDepth);
	vec3 rayDir = perDepth / max(rayScale, 1e-6);

	vec3 sunTravel = dot(fog.sun.xyz, fog.sun.xyz) > 1e-10 ? normalize(fog.sun.xyz) : vec3(0.0, -1.0, 0.0);
	vec3 sunColor = mix(fog.opposite.rgb, fog.direct.rgb, Phase(dot(rayDir, -sunTravel), fog.opposite.w));

	float stepDepth = (toDepth - fromDepth) / float(FAR_STEPS);
	vec3 scattered = vec3(0.0);
	float transmittance = 1.0;

	for (int i = 0; i < FAR_STEPS; i++)
	{
		float depth = fromDepth + (float(i) + 0.5) * stepDepth;
		vec3 worldPos = a + perDepth * (depth - depthA);
		float density = Density(worldPos.y);

		vec3 light = fog.sky.rgb * HeightAttenuation(worldPos.y, fog.sky.w) +
			sunColor * HeightAttenuation(worldPos.y, fog.direct.w);

		float extinction = density * (1.0 + fog.extinction.x);
		float stepTransmittance = exp(-extinction * stepDepth * rayScale);
		scattered += transmittance * light * density * (1.0 - stepTransmittance) / max(extinction, 1e-6);
		transmittance *= stepTransmittance;
	}

	return vec4(scattered, transmittance);
}

void main()
{
	vec2 uv = gl_FragCoord.xy / fog.flags.zw;

	float far = fog.depthRange.y;
	float fogEnd = fog.flags.y > 0.5 ? max(fog.depthRange.w, far) : far;
	float depth = fogEnd;

	if (sceneparam.sceneDepthTexture != 0u)
	{
		float stored = texelFetch(
			sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], linearClamp),
			ivec2(gl_FragCoord.xy), 0).r;

		// Reverse depth clears to 0, where there is nothing but sky
		if (stored > 0.0)
		{
			depth = min(ViewDepth(stored), fogEnd);
		}
	}

	vec4 fogged = SampleVolume(uv, min(depth, far));

	if (depth > far)
	{
		vec2 ndc = vec2(uv.x * 2.0 - 1.0, c_clipSpaceYInverted ? uv.y * 2.0 - 1.0 : 1.0 - uv.y * 2.0);
		vec4 beyond = MarchFar(ndc, far, depth);
		fogged = vec4(fogged.rgb + fogged.a * beyond.rgb, fogged.a * beyond.a);
	}

	// Blended as scene x a + rgb
	fsout_color = fogged;
}
