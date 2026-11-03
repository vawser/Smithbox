#version 450
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable
layout(location = 0) in vec3 normal;
layout(location = 1) in vec4 color;
layout(location = 2) in vec3 view;
layout(location = 3) flat in uint fsin_entityid;
layout(location = 4) in vec2 fsin_barycentric;
layout(location = 5) in float fsin_worldY;
layout(location = 6) flat in uint fsin_volumeFlags;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

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

	// The nav volume slice, see NavVolumeSlice
	float navVolumeSliceHeight;
	uint navVolumeSliceFlags;
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

float gridFactor(vec2 b, float width)
{
	vec3 bary = vec3(b.x, b.y, 1.0 - b.x - b.y);
	vec3 d = fwidth(bary);
	vec3 a3 = smoothstep(d * (width - 0.5), d * (width + 0.5), bary);
	return min(min(a3.x, a3.y), a3.z);
}

// Nav volume vertices are flagged, see HavokNavmeshResource: 1 for the volume, 2 for the walls of its grid
#define VOLUME_FLAG 1u
#define VOLUME_WALL_FLAG 2u

// The nav volume slice, see NavVolumeSlice
#define SLICE_ENABLED 1u
#define SLICE_CROSS_SECTION 2u

#define CROSS_SECTION_COLOR vec3(1.0, 0.55, 0.15)

// Discards the parts of a nav volume that aren't shown: everything above the slice, and the walls of its grid.
// Returns whether the fragment is seen through the cut from inside the volume, so it is drawn as the cross-section
bool NavVolumeClip()
{
	if ((fsin_volumeFlags & VOLUME_FLAG) == 0u)
	{
		return false;
	}

	bool sliced = (sceneparam.navVolumeSliceFlags & SLICE_ENABLED) != 0u;

	if (sliced && fsin_worldY > sceneparam.navVolumeSliceHeight)
	{
		discard;
	}

	// The faces point out of the volume, so through the cut its inside is made of the faces pointing away
	bool crossSection = sliced
		&& (sceneparam.navVolumeSliceFlags & SLICE_CROSS_SECTION) != 0u
		&& sceneparam.eye.y > sceneparam.navVolumeSliceHeight
		&& dot(normal, view) < 0.0;

	// The walls only close the volume, so that the cross-section has no gaps at its edges
	if ((fsin_volumeFlags & VOLUME_WALL_FLAG) != 0u && !crossSection)
	{
		discard;
	}

	return crossSection;
}

void main()
{
	// Comes first, so the hidden parts of a nav volume can't be picked
	bool crossSection = NavVolumeClip();

	if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}

	if (crossSection)
	{
		fsout_color = vec4(CROSS_SECTION_COLOR, color.a * sceneparam.collisionOpacity);
		if (c_linearOutput)
			fsout_color.rgb *= fsout_color.rgb;
		return;
	}

    //fsout_color = vec4(1.0, 1.0, 1.0, 1.0);
	vec3 diffuse = vec3(clamp(abs(dot(-view, normal)), 0.0, 1.0));
	vec3 ambient = vec3(1.0, 1.0, 1.0);//color.xyz * vec3(0.3);
	//fsout_color = vec4(diffuse, 1.0);
	float grid = gridFactor(fsin_barycentric, 0.5);
	vec3 col = diffuse * color.xyz * 0.8 + ambient * color.xyz * 0.2;
	fsout_color = vec4(col * grid + (1.0 - grid) * col * 0.4, color.a * sceneparam.collisionOpacity);

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
