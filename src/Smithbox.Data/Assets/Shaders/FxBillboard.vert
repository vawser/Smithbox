#version 450
#extension GL_GOOGLE_include_directive : require

// Vertex stage of the FX Editor's billboard particles (PointSprite, BillboardEx, MultiTextureBillboardEx; see
// FxBillboardRenderableProxy.cs). One instance per particle; each takes two 96 byte slots of the instance buffer,
// laid out as FxBillboardGpu. A batch is allocated 192 byte aligned and drawn with its first instance at
// (start / 192), so instance i's slots are 2i and 2i + 1. The quad's corners are -0.5 to 0.5 in x and y.
//
// Slot A: d0 centre, z rotation (rad); d1 size, frame, material (uint bits); d2 colour;
//         d3 x and y rotation (rad), alpha thresholds (packUnorm2x16 bits), depth offset; d4 layer 1 uv; d5 layer 2 uv
// Slot B: d0 layer 3 uv; d1-d3 layer colours; d4 node right; d5 node up

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

// The instance buffer, read as particle slots rather than instanceData
struct fxSlot
{
	vec4 d[6];
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
	readonly fxSlot slots[];
};

struct fxMaterial
{
	uvec4 textures; // x, y, z: texture of each layer (0 for none), w: layer count
	uvec4 params;   // x: columns, y: total frames, z: flags (see the fragment stage), w: orientation mode
	uvec4 extra;    // x: blend mode
	vec4 tuning;    // x: brightness multiplier, y: depth fade distance
};

layout(set = 4, binding = 0, std140) buffer materials
{
	readonly fxMaterial matdata[];
};

layout(location = 0) in vec3 position;
layout(location = 1) in uvec4 color;
layout(location = 2) in vec3 normal;

layout(location = 0) out vec2 fsin_uv;
layout(location = 1) out vec4 fsin_color;
layout(location = 2) out vec4 fsin_frame;    // x: frame, y: alpha threshold, z: alpha fade threshold
layout(location = 3) out vec4 fsin_layer1;   // uv offset, uv scale
layout(location = 4) out vec4 fsin_layer2;
layout(location = 5) out vec4 fsin_layer3;
layout(location = 6) out vec4 fsin_layer1Color;
layout(location = 7) out vec4 fsin_layer2Color;
layout(location = 8) out vec4 fsin_layer3Color;
layout(location = 9) flat out uint fsin_mat;

// Rotation of v around a unit axis by an angle (Rodrigues)
vec3 RotateAround(vec3 v, vec3 axis, float angle)
{
	float c = cos(angle);
	float s = sin(angle);
	return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
}

void main()
{
	uint index = 2u * uint(gl_InstanceIndex);
	vec4 d0 = slots[index].d[0];
	vec4 d1 = slots[index].d[1];
	vec4 d2 = slots[index].d[2];
	vec4 d3 = slots[index].d[3];
	vec4 d4 = slots[index].d[4];
	vec4 d5 = slots[index].d[5];
	vec4 e0 = slots[index + 1u].d[0];
	vec4 e1 = slots[index + 1u].d[1];
	vec4 e2 = slots[index + 1u].d[2];
	vec4 e3 = slots[index + 1u].d[3];
	vec4 e4 = slots[index + 1u].d[4];
	vec4 e5 = slots[index + 1u].d[5];

	uint mat = floatBitsToUint(d1.w);
	fxMaterial m = matdata[mat];
	vec2 thresholds = unpackUnorm2x16(floatBitsToUint(d3.z));

	// Segment mode (trails and lines): corner x picks end A (-0.5) or B (+0.5), corner y the side; each end has its
	// own half-width vector and colour. A: d0 end, d2 colour, d5 half-width; B: e4 end, e1 colour, e0 half-width.
	if (m.params.w == 100u)
	{
		bool atB = position.x > 0.0;
		vec3 end = atB ? e4.xyz : d0.xyz;
		vec3 across = atB ? e0.xyz : d5.xyz;
		vec3 segPos = end + across * (position.y * 2.0);

		fsin_uv = vec2(position.x + 0.5, 0.5 - position.y);
		fsin_color = atB ? e1 : d2;
		fsin_frame = vec4(d1.z, thresholds.x, thresholds.y, 0);
		fsin_layer1 = d4;
		fsin_layer2 = vec4(0, 0, 1, 1);
		fsin_layer3 = vec4(0, 0, 1, 1);
		fsin_layer1Color = vec4(1);
		fsin_layer2Color = vec4(1);
		fsin_layer3Color = vec4(1);
		fsin_mat = mat;
		gl_Position = sceneparam.projection * sceneparam.view * vec4(segPos, 1.0);
		return;
	}

	// Lens flare mode (FX Editor Plan §20.8): a camera-facing quad at the source, world-sized, moved on screen along
	// the reflection vector (1 Radial: through the screen centre; 2 Horizontal: mirrored left-right) by the flare's
	// offset, and faded by the attenuation radius (the screen centre must lie within the sphere of that radius
	// around the source). The source's screen position, depth and radius go to the fragment stage in fsin_layer3,
	// which tests how much of the source the scene depth shows. A0 source; A1 size; A2 colour; A3 offset,
	// reflection, -, attenuation radius; B0.x source diameter.
	if (m.params.w == 101u)
	{
		vec3 src = d0.xyz;
		mat4 viewProj = sceneparam.projection * sceneparam.view;
		vec4 srcClip = viewProj * vec4(src, 1.0);

		vec3 camRightF = vec3(sceneparam.view[0][0], sceneparam.view[1][0], sceneparam.view[2][0]);
		vec3 camUpF = vec3(sceneparam.view[0][1], sceneparam.view[1][1], sceneparam.view[2][1]);
		vec2 cornerF = position.xy;
		vec4 clip = viewProj * vec4(src + camRightF * (cornerF.x * d1.x) + camUpF * (cornerF.y * d1.y), 1.0);

		fsin_uv = vec2(cornerF.x + 0.5, 0.5 - cornerF.y);
		fsin_frame = vec4(0, thresholds.x, thresholds.y, 0);
		fsin_layer1 = d4;
		fsin_layer2 = vec4(0, 0, 1, 1);
		fsin_layer1Color = vec4(1);
		fsin_layer2Color = vec4(1);
		fsin_layer3Color = vec4(1);
		fsin_mat = mat;

		// Behind the camera: nothing
		if (srcClip.w <= 1e-4)
		{
			fsin_color = vec4(0);
			fsin_layer3 = vec4(0, 0, -1, 0);
			gl_Position = vec4(0, 0, -1, 1);
			return;
		}

		vec2 ndc = srcClip.xy / srcClip.w;
		uint reflection = uint(d3.y + 0.5);
		vec2 target = reflection == 1u ? -ndc : reflection == 2u ? vec2(-ndc.x, ndc.y) : ndc;
		vec2 shift = (target - ndc) * d3.x;
		clip.xy += shift * clip.w;

		// How large a world length at the source is on screen, in NDC (along y; x is scaled by the aspect)
		float perMeter = abs(sceneparam.projection[1][1]) / srcClip.w;
		float aspect = abs(sceneparam.projection[1][1]) / max(abs(sceneparam.projection[0][0]), 1e-6);

		float fade = 1.0;
		if (d3.w > 0.0)
		{
			float radiusNdc = d3.w * perMeter;
			float fromCentre = length(vec2(ndc.x * aspect, ndc.y));
			fade = clamp(1.0 - fromCentre / max(radiusNdc, 1e-6), 0.0, 1.0);
		}

		// A source off the screen can't be seen
		bool onScreen = all(lessThanEqual(abs(ndc), vec2(1.0)));
		fsin_color = vec4(d2.rgb, d2.a * fade);
		fsin_layer3 = vec4(ndc * 0.5 + 0.5, onScreen ? srcClip.z / srcClip.w : -1.0, 0.5 * e0.x * perMeter * 0.5);
		gl_Position = clip;
		return;
	}

	vec3 center = d0.xyz;
	float rotZ = d0.w;
	vec2 size = d1.xy;
	vec3 nodeRight = e4.xyz;
	vec3 nodeUp = e5.xyz;
	vec3 eye = sceneparam.eye.xyz;

	// The quad's axes by orientation mode
	vec3 camRight = vec3(sceneparam.view[0][0], sceneparam.view[1][0], sceneparam.view[2][0]);
	vec3 camUp = vec3(sceneparam.view[0][1], sceneparam.view[1][1], sceneparam.view[2][1]);
	vec3 toEye = eye - center;
	vec3 right = camRight;
	vec3 up = camUp;
	uint orientation = m.params.w;

	if (orientation == 0u || orientation == 3u)
	{
		// South: faces global -Z
		right = vec3(1, 0, 0);
		up = vec3(0, 1, 0);
	}
	else if (orientation == 5u)
	{
		// East: faces global +X
		right = vec3(0, 0, 1);
		up = vec3(0, 1, 0);
	}
	else if (orientation == 2u)
	{
		// Local south: faces the node's -Z
		right = nodeRight;
		up = nodeUp;
	}
	else if (orientation == 4u || orientation == 7u)
	{
		// Yaw: upright along the global (4) or node (7) Y, turned toward the camera
		up = orientation == 4u ? vec3(0, 1, 0) : nodeUp;
		vec3 level = toEye - up * dot(toEye, up);
		right = dot(level, level) > 1e-8 ? normalize(cross(up, level)) : camRight;
	}
	else if (orientation == 6u)
	{
		// Camera: faces the camera's position rather than its plane
		vec3 f = dot(toEye, toEye) > 1e-8 ? normalize(toEye) : -camUp;
		vec3 worldUp = abs(f.y) < 0.999 ? vec3(0, 1, 0) : vec3(1, 0, 0);
		right = normalize(cross(worldUp, f));
		up = cross(f, right);
	}

	// Rotation around the quad's normal; scale before it (by default) keeps non-square particles' shape
	vec2 corner = position.xy;
	bool scaleFirst = (m.params.z & 16u) == 0u;
	float c = cos(rotZ);
	float s = sin(rotZ);
	mat2 rot = mat2(c, s, -s, c);
	vec2 offset2 = scaleFirst ? rot * (corner * size) : (rot * corner) * size;
	vec3 offset = right * offset2.x + up * offset2.y;

	// The X and Y rotations tilt particles that don't face the camera
	if (orientation != 1u && orientation != 6u)
	{
		offset = RotateAround(offset, normalize(up), d3.y);
		offset = RotateAround(offset, normalize(right), d3.x);
	}

	vec3 worldPos = center + offset;

	// The depth offset moves the particle toward the camera (positive) or away from it
	if (d3.w != 0.0 && dot(toEye, toEye) > 1e-8)
	{
		worldPos += normalize(toEye) * d3.w;
	}

	fsin_uv = vec2(corner.x + 0.5, 0.5 - corner.y);
	fsin_color = d2;
	fsin_frame = vec4(d1.z, thresholds.x, thresholds.y, 0);
	fsin_layer1 = d4;
	fsin_layer2 = d5;
	fsin_layer3 = e0;
	fsin_layer1Color = e1;
	fsin_layer2Color = e2;
	fsin_layer3Color = e3;
	fsin_mat = mat;

	gl_Position = sceneparam.projection * sceneparam.view * vec4(worldPos, 1.0);
}
