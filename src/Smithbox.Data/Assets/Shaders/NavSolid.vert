#version 450
#extension GL_GOOGLE_include_directive : require

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

#include "InstanceData.glsl"

layout(location = 0) in vec3 position;
layout(location = 1) in ivec4 normal;
layout(location = 2) in uvec4 color;
layout(location = 3) in uvec4 barycentric;
layout(location = 0) out vec3 fsin_normal;
layout(location = 1) out vec4 fsin_color;
layout(location = 2) out vec3 fsin_view;
layout(location = 3) out uint fsin_entityid;
layout(location = 8) out vec3 fsin_clipPos; // the world position, for CAMERA_CLIP
layout(location = 4) out vec2 fsin_barycentric;
layout(location = 5) out float fsin_worldY;
layout(location = 6) flat out uint fsin_volumeFlags;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_clipPos = vec3(w * vec4(position, 1));
	fsin_normal = mat3(w) * vec3((vec3(normal) / 255.0));
	fsin_color = vec4(color / 255.0);
	fsin_barycentric = vec2(barycentric.xy);
	// Nav volumes are sliced by world height, and flag their vertices in z
	fsin_worldY = (w * vec4(position, 1)).y;
	fsin_volumeFlags = barycentric.z;
    //fsin_color = vec4(vec3((vec4(tnormal, 1.0) / 255.0) + 0.5), 1.0);
	fsin_view = VIEW_DIRECTION(vec3(w * vec4(position, 1)));
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
    vec4 p = sceneparam.view * w * vec4(position, 1);
	p.z -= 0.002;
    gl_Position = sceneparam.projection * p;
	// Apply a bias
	//gl_Position.z -= 0.01;
}