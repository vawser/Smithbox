#version 450
#extension GL_GOOGLE_include_directive : require
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable
layout(location = 0) in vec4 color;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;
layout(location = 1) flat in uint fsin_entityid;
layout(location = 8) in vec3 fsin_clipPos;

layout (constant_id = 99) const bool c_picking = false;

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

// The wireframe view, built with WIREFRAME as SimpleFlver_focus_wire, with SimpleFlver.geom before it
#ifdef WIREFRAME
#include "Wireframe.glsl"
#endif

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
	CAMERA_CLIP(fsin_clipPos);

#ifdef WIREFRAME
	// Before the discards, as it takes derivatives
	float wireCoverage = WireframeCoverage();
	if (WireframeOnly() && !WireframeKeeps(wireCoverage))
	{
		discard;
	}
#endif

	// Picking comes first, so the gaps in the dither can still be clicked
	if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}

	// The lines alone aren't dithered, so they stay whole; they are greyed all the same
	bool dither = true;
#ifdef WIREFRAME
	dither = !WireframeOnly();
#endif

	if (dither && FOCUS_OPACITY < OrderedDither())
	{
		discard;
	}

	fsout_color = vec4(FocusGrey(color.rgb), color.a);

#ifdef WIREFRAME
	fsout_color.rgb = WireframeOver(fsout_color.rgb, FocusGrey(sceneparam.wireframeColor.rgb), wireCoverage);
#endif

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
