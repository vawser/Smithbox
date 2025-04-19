#version 450

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
	uint envmap;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct instanceData
{
	mat4 world;
	uint materialID[4];
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
    readonly instanceData idata[];
};

layout(location = 0) in vec3 position;
layout(location = 1) in ivec2 uv;
layout(location = 2) in ivec4 normal;
layout(location = 3) in ivec4 binormal;
layout(location = 4) in ivec4 bitangent;

layout(location = 0) out vec2 fsin_texcoord;
layout(location = 1) out vec3 fsin_view;
layout(location = 2) out mat3 fsin_worldToTangent;
layout(location = 5) out vec3 fsin_normal;
layout(location = 6) out vec4 fsin_bitangent;
layout(location = 7) out vec4 fsin_color;
layout(location = 8) out uint fsin_mat;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_texcoord = vec2(uv) / 2048.0;
	fsin_normal = normalize(mat3(w) * vec3(normal));
	fsin_bitangent = bitangent;
	fsin_view = normalize(sceneparam.eye.xyz - (w * vec4(position, 1)).xyz);
	fsin_mat = idata[gl_InstanceIndex].materialID[0];
	
	vec3 T = normalize(mat3(w) * vec3(bitangent));
	vec3 B = normalize(mat3(w) * vec3(binormal));
	vec3 N = normalize(mat3(w) * vec3(normal));
	fsin_worldToTangent = mat3(T, B, N);
	
    gl_Position = sceneparam.projection * sceneparam.view * w * vec4(position, 1);
}