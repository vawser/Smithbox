#version 450
#extension GL_EXT_nonuniform_qualifier : require
#extension GL_EXT_shader_16bit_storage : enable
#extension GL_EXT_shader_explicit_arithmetic_types : enable

#define GAME_DES 1
#define GAME_DS1_PTDE 2
#define GAME_DS1_REMASTER 3
#define GAME_DS2 4
#define GAME_BLOODBORNE 6
#define GAME_DS3 5
#define GAME_SEKIRO 7
layout (constant_id = 0) const int c_gameID = GAME_DS3;
#ifdef MATERIAL_BLEND
layout (constant_id = 1) const bool c_blendNormal = false;
layout (constant_id = 2) const bool c_blendSpecular = false;
layout (constant_id = 3) const bool c_blendShininess = false;
#endif

layout (constant_id = 99) const bool c_picking = false;

layout(location = 0) in vec2 fsin_texcoord;
layout(location = 1) in vec3 fsin_view;
layout(location = 2) in mat3 fsin_worldToTangent;
layout(location = 5) in vec3 fsin_normal;
layout(location = 6) in vec4 fsin_bitangent;
layout(location = 7) in vec4 fsin_color;
layout(location = 8) flat in uint fsin_mat;
layout(location = 9) flat in uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) in vec2 fsin_texcoord2;
#endif

layout(location = 0) out vec4 fsout_color;

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
    
    vec4 SimpleFlverSelectColor;
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];
layout(set = 3, binding = 0) uniform textureCube globalTexturesCube[];

struct materialData
{
	uint16_t colorTex;
	uint16_t colorTex2;
	uint16_t normalTex;
	uint16_t normalTex2;
	uint16_t specTex;
	uint16_t specTex2;
	uint16_t emissiveTex;
	uint16_t shininessTex;
	uint16_t shininessTex2;
	uint16_t blendMaskTex;
	uint16_t lightmapTex;
	uint16_t lightmapTex2;
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly materialData matdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;
layout(set = 5, binding = 1) uniform sampler anisoLinearSampler;

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

void main()
{
	fsout_color = sceneparam.SimpleFlverSelectColor;
}