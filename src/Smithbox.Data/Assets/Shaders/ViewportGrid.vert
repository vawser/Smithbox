#version 450
#extension GL_GOOGLE_include_directive : require

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

#include "InstanceData.glsl"

layout(location = 0) in vec3 position;
layout(location = 0) out vec2 fsin_plane;
layout(location = 1) flat out uint fsin_mat;
// Position relative to the camera, for the distance fade
layout(location = 2) out vec3 fsin_viewPos;

// Moves the grid toward the camera by this fraction of its distance (a millimetre at ten metres), so a grid
// placed on a floor doesn't flicker with it. The depth is reversed, so a larger depth is nearer. Unlike a
// hardware depth bias this is the same fraction at every pixel, however far the plane reaches or however
// steeply it is seen, so the grid never comes out in front of an object standing on it.
const float c_depthBias = 1.0e-4;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_plane = position.xz;
	fsin_mat = idata[gl_InstanceIndex].materialID[0];

	vec4 viewPos = sceneparam.view * w * vec4(position, 1);
	fsin_viewPos = viewPos.xyz;
	gl_Position = sceneparam.projection * viewPos;
	gl_Position.z *= 1.0 + c_depthBias;
}
