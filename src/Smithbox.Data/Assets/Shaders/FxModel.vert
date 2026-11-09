#version 450
#extension GL_GOOGLE_include_directive : require

// Vertex stage of the FX Editor's Model particles (FX Editor Plan §18, FxModelRenderableProxy.cs): a FLVER submesh
// drawn once per particle, coloured by the effect. One 96 byte slot per instance, laid out as FxModelGpu; a batch
// is allocated 96 byte aligned and drawn with its first instance at (start / 96), so instance i reads slot i.
//
// Slot: d0-d3 world matrix rows (the submesh's own transform already in it); d4 colour; d5 uv offset (xy),
// sprite frame (z), material index (w, uint bits).
//
// Every textured FLVER layout has position at location 0, uv1 (x 2048) at 1 and the normal at 2, so this stage fits
// all of them; the rest of each layout is left unread.

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct fxModelSlot
{
	vec4 d[6];
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
	readonly fxModelSlot slots[];
};

layout(location = 0) in vec3 position;
layout(location = 1) in ivec2 uv;
layout(location = 2) in ivec4 normal;

layout(location = 0) out vec2 fsin_uv;
layout(location = 1) out vec4 fsin_color;
layout(location = 2) out vec3 fsin_normal;
layout(location = 3) out vec3 fsin_worldpos;
layout(location = 4) out float fsin_frame;
layout(location = 5) flat out uint fsin_mat;

const float UV_SCALE = 1.0 / 2048.0;

void main()
{
	fxModelSlot s = slots[gl_InstanceIndex];

	// The rows of a System.Numerics matrix, which transforms row vectors: as columns, the same transform
	mat4 world = mat4(s.d[0], s.d[1], s.d[2], s.d[3]);

	vec4 worldPos = world * vec4(position, 1.0);
	fsin_worldpos = worldPos.xyz;
	fsin_normal = normalize(mat3(world) * vec3(normal.xyz));
	fsin_uv = vec2(uv) * UV_SCALE + s.d[5].xy;
	fsin_color = s.d[4];
	fsin_frame = s.d[5].z;
	fsin_mat = floatBitsToUint(s.d[5].w);

	gl_Position = sceneparam.projection * sceneparam.view * worldPos;
}
