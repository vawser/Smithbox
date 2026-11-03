#version 450
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable
layout(location = 0) in vec3 normal;
layout(location = 1) in vec4 color;
layout(location = 2) in vec3 view;
layout(location = 3) flat in uint fsin_entityid;
layout(location = 4) in vec2 fsin_barycentric;
layout(location = 0) out vec4 fsout_color;

layout (constant_id = 99) const bool c_picking = false;

#define MAX_SCENE_LIGHTS 16

struct sceneLight
{
	vec4 positionRange;
	vec4 directionType;
	vec4 colorIntensity;
	vec4 spot;
};

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

	// Only here to reach collisionOpacity, which comes after them
	uint lightCount;
	vec4 sunColor;
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
	float windPeriod;
	uint materialFeatures;
	uint sceneDepthTexture;
	vec4 wind;

	float collisionOpacity;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct updatePickingBuffer
{
	uint depth;
	uint pad;
	uint64_t identifier;
};

layout(set = 6, binding = 0, std140) buffer pickingBuffer
{
	volatile updatePickingBuffer pb;
};

void UpdatePickingBuffer(ivec2 pos, uint64_t identity, float z)
{
	if (sceneparam.curserPosition.x != pos.x || sceneparam.curserPosition.y != pos.y)
	{
		return;
	}

	uint d = floatBitsToUint(z);
	uint current_d_or_locked = 0;
	/*do
	{
		if (d >= pb.depth)
		{
			return;
		}

		current_d_or_locked = atomicMin(pb.depth, d);
		if (d < int(current_d_or_locked))
		{
			uint last_d = 0;
			last_d = atomicCompSwap(pb.depth, d, floatBitsToUint(-(int(d))));
			if (last_d == d)
			{
				pb.identifier = identity;
				atomicExchange(pb.depth, d);
			}
		}
	} while (int(current_d_or_locked) < 0);*/
	//uint d = uint(z);
	if (d <= pb.depth)
	{
		return;
	}
	pb.depth = d;
	pb.identifier = fsin_entityid;
}

// The share of pixels kept for a map that is out of focus
#define FOCUS_OPACITY 0.4

float OrderedDither()
{
	mat4 bayerMatrix = mat4(
		 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
		12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
		 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
		15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
	);

	ivec2 pixelCoord = ivec2(gl_FragCoord.xy) % 4;
	return bayerMatrix[pixelCoord.x][pixelCoord.y];
}

vec3 FocusGrey(vec3 color)
{
	return mix(vec3(dot(color, vec3(0.299, 0.587, 0.114))), vec3(0.5), 0.5);
}

void main()
{
	// Picking comes first, so the gaps in the dither can still be clicked
	if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}

	if (FOCUS_OPACITY < OrderedDither())
	{
		discard;
	}

	vec3 diffuse = vec3(clamp(abs(dot(-view, normal)), 0.0, 1.0));
	vec3 ambient = vec3(1.0, 1.0, 1.0);
	fsout_color = vec4(FocusGrey(diffuse * color.xyz * 0.8 + ambient * color.xyz * 0.2), color.a * sceneparam.collisionOpacity);
}
