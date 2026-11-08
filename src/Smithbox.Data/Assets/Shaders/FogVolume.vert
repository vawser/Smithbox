#version 450
#extension GL_GOOGLE_include_directive : require

// Vertex stage of the fog volume shader: places the unit box of a fog volume, and passes the world position of
// each pixel on so the fragment stage can march from the camera through the box.

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

#include "InstanceData.glsl"

layout(location = 0) in vec3 position;
layout(location = 1) in uvec4 color;
layout(location = 2) in vec3 normal;
layout(location = 0) out vec3 fsin_worldPos;
layout(location = 1) out uint fsin_mat;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	vec4 worldPos = w * vec4(position, 1);

	fsin_worldPos = worldPos.xyz;
	fsin_mat = idata[gl_InstanceIndex].materialID[0];
	gl_Position = sceneparam.projection * sceneparam.view * worldPos;
}
