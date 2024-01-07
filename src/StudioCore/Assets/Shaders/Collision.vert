#version 450

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
layout(location = 1) in ivec4 normal;
layout(location = 2) in uvec4 color;
layout(location = 3) in uvec4 barycentric;
layout(location = 0) out vec3 fsin_normal;
layout(location = 1) out vec4 fsin_color;
layout(location = 2) out vec3 fsin_view;
layout(location = 3) out uint fsin_entityid;

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	fsin_normal = mat3(w) * vec3((vec3(normal) / 255.0));
	fsin_color = vec4(color / 255.0);
    //fsin_color = vec4(vec3((vec4(tnormal, 1.0) / 255.0) + 0.5), 1.0);
	fsin_view = normalize(sceneparam.eye.xyz - vec3(w * vec4(position, 1)));
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
	vec4 p = sceneparam.view * w * vec4(position, 1);
	p.z -= 0.001;
    gl_Position = sceneparam.projection * p;
	//gl_Position.z -= 0.003;
}