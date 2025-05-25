#version 450
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable
layout(location = 0) in vec4 color;
layout(location = 0) out vec4 fsout_color;
layout(location = 1) flat in uint fsin_entityid;

layout (constant_id = 99) const bool c_picking = false;

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

void main()
{
    if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}

    //fsout_color = vec4(1.0, 1.0, 1.0, 1.0);
	fsout_color = color;
}