#version 450

struct sceneParams
{
	mat4 projection;
	mat4 view;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct instanceData
{
	mat4 world;
	// 0: material id
	// 3: entity id
	uvec4 materialID;
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
    readonly instanceData idata[];
};

layout(location = 0) in vec3 position;
layout(location = 0) out vec2 fsin_plane;
layout(location = 1) flat out uint fsin_mat;
// Position relative to the camera, for the distance fade
layout(location = 2) out vec3 fsin_viewPos;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_plane = position.xz;
	fsin_mat = idata[gl_InstanceIndex].materialID[0];

	vec4 viewPos = sceneparam.view * w * vec4(position, 1);
	fsin_viewPos = viewPos.xyz;
	gl_Position = sceneparam.projection * viewPos;
}
