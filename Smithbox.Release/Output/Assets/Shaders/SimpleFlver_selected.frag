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
    float simpleFlverBrightness; 
    float simpleFlverSaturation;  
    vec4 selectionColor;          
    vec4 outlineColor;            
    uint EnableDithering;      
    uint EnableTinting;
    float ditherOpacity;
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

    if(sceneparam.EnableDithering == 1)
    {
        // 4x4 Bayer ordered dither
        mat4 bayerMatrix = mat4(
             0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
            12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
             3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
            15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
        );

        ivec2 pixelCoord = ivec2(gl_FragCoord.xy) % 4;
        float threshold = bayerMatrix[pixelCoord.x][pixelCoord.y];

        float opacity = sceneparam.ditherOpacity;
        if (opacity < threshold)
        {
            discard;
        }
    }
    
    if(sceneparam.EnableTinting == 1)
    {
        fsout_color = mix(color, sceneparam.selectionColor, 0.5);
    }
    else if(sceneparam.EnableTinting == 0)
    {
        fsout_color = color;
    }
}