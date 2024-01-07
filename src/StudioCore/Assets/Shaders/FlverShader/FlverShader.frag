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

float Epsilon = 0.00001;

float LdotNPower = 0.1;

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

void main()
{
    //fsout_color = vec4(1.0, 1.0, 1.0, 1.0);
	vec3 lightdir = normalize(vec3(sceneparam.lightDirection));
	vec3 viewVec = normalize(fsin_view);
	
#ifdef MATERIAL_BLEND
	vec4 d1 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].colorTex))], anisoLinearSampler), fsin_texcoord.xy);
	vec4 d2 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].colorTex2))], anisoLinearSampler), fsin_texcoord2.xy);
  #ifdef MATERIAL_BLEND_MASK
	float blend = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].blendMaskTex))], anisoLinearSampler), fsin_texcoord.xy).r;
  #else
	float blend = fsin_color.a;
  #endif
	vec4 diffuseColor = mix(d1, d2, blend);
#else
	vec4 diffuseColor = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].colorTex))], anisoLinearSampler), fsin_texcoord.xy);
#endif
	
	if (diffuseColor.w < 0.5)
	{
		discard;
	}

	// Do picking after discard
	if (c_picking)
	{
		ivec2 coord = ivec2(gl_FragCoord.xy - vec2(0.49, 0.49));
		UpdatePickingBuffer(coord, uint64_t(fsin_entityid), gl_FragCoord.z);
	}
	
#ifdef MATERIAL_BLEND
	vec3 normalColor;
	if (c_blendNormal)
	{
		vec4 n1 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].normalTex))], anisoLinearSampler), fsin_texcoord.xy);
		vec4 n2 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].normalTex2))], anisoLinearSampler), fsin_texcoord2.xy);
		normalColor = mix(n1, n2, blend).rgb;
	}
	else
	{
		normalColor = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].normalTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
	}
#else
	vec3 normalColor = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].normalTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
#endif
	vec3 L = -lightdir;
	vec3 H = normalize(L + viewVec);
#ifdef MATERIAL_BLEND
	vec3 F0;
	if (c_blendSpecular)
	{
		vec4 s1 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].specTex))], anisoLinearSampler), fsin_texcoord.xy);
		vec4 s2 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].specTex2))], anisoLinearSampler), fsin_texcoord2.xy);
		F0 = mix(s1, s2, blend).rgb;
	}
	else
	{
		F0 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].specTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
	}
#else
	vec3 F0 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].specTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
#endif
	//F0 *= F0;
	
	float roughness;
	if (c_gameID == GAME_BLOODBORNE || c_gameID == GAME_DS1_PTDE || c_gameID == GAME_DS2)
	{
#ifdef MATERIAL_BLEND
		vec3 shininessColor;
		if (c_blendShininess)
		{
			vec4 s1 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].shininessTex))], anisoLinearSampler), fsin_texcoord.xy);
			vec4 s2 = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].shininessTex2))], anisoLinearSampler), fsin_texcoord2.xy);
			shininessColor = mix(s1, s2, blend).rgb;
		}
		else
		{
			shininessColor = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].shininessTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
		}
#else
		vec3 shininessColor = texture(sampler2D(globalTextures[nonuniformEXT(int(matdata[fsin_mat].shininessTex))], anisoLinearSampler), fsin_texcoord.xy).xyz;
#endif
		//roughness = 1.0 - (normalColor.z * shininessColor.r);
		roughness = 1.0 - shininessColor.r;
	}
	else
	{
		roughness = 1.0 - normalColor.z;
	}
	
	vec3 normalMap;
	normalMap.xy = normalColor.xy * 2.0 - 1.0;
	normalMap.z = sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
	normalMap = normalize(normalMap);
	normalMap = normalize(fsin_worldToTangent * normalMap);
	
	vec3 N = (gl_FrontFacing ? normalMap : -normalMap);
	
	float LdotN = clamp(dot(N, L), 0.0, 1.0);
	float NdotV = abs(clamp(dot(viewVec, N), 0.0, 1.0));
	float NdotH = abs(clamp(dot(H, N), 0.0, 1.0));
	float VdotH = clamp(dot(H, viewVec), 0.0, 1.0);
	
	// traditional phong model
	if (c_gameID == GAME_DES || c_gameID == GAME_DS1_PTDE)
	{
		// diffuse
		vec3 finalDiffuse = diffuseColor.xyz * LdotN;

		// ambient
		vec3 ambientDiffuse = diffuseColor.xyz * textureLod(samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler), vec3(N * vec3(1, 1, -1)), 5).xyz;
		ambientDiffuse *= sceneparam.ambientLightMult;

		// specular
		vec3 specular = F0 * pow(NdotH, 4);

		vec3 direct = finalDiffuse + specular;
		vec3 indirect = ambientDiffuse;
		
		fsout_color = vec4((direct * sceneparam.directLightMult + indirect * sceneparam.indirectLightMult) * sceneparam.sceneBrightness, 1.0);
	}
	// PBR model
	else
	{
		float alpha = roughness * roughness;
		float alphasquare = alpha * alpha;
		
		vec3 finalDiffuse = diffuseColor.xyz * LdotN;
		
		vec3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;
		float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
		
		float specPower = exp2((1 - roughness) * 13.0);
		specPower = max(1.0, specPower / (specPower * 0.01 + 1.0)) * 1;//8;
		float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
		
		vec3 specular = D * F * pow(LdotN, LdotNPower);
		
		float envMip = min(6.0, -(1 - roughness) * 6.5 + 6.5);
		vec3 reflectVec = reflect(-viewVec, N);
		vec3 ambientSpec = textureLod(samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler), vec3(reflectVec * vec3(1, 1, -1)), envMip).xyz;
		ambientSpec *= sceneparam.ambientLightMult;
		vec3 ambientDiffuse = textureLod(samplerCube(globalTexturesCube[nonuniformEXT(int(sceneparam.envmap))], linearSampler), vec3(N * vec3(1, 1, -1)), 5).xyz;
		ambientDiffuse *= sceneparam.ambientLightMult;
		
		NdotV = max(NdotV, Epsilon);
		vec3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;
		
		vec3 diffuse = finalDiffuse * (1 - F0);
		vec3 indirectDiffuse = diffuseColor.xyz * ambientDiffuse * (1 - F0);
		vec3 indirectSpecular = ambientSpec * aF;
		float reflectionThing = clamp(dot(reflectVec, N) + 1.0, 0, 1);
		reflectionThing *= reflectionThing;
		indirectSpecular *= reflectionThing;
		
		vec3 direct = diffuse + specular;
		vec3 indirect = indirectDiffuse + indirectSpecular;
		
		fsout_color = vec4((direct * sceneparam.directLightMult + indirect * sceneparam.indirectLightMult) * sceneparam.sceneBrightness, 1.0);
	}
	fsout_color = sqrt(fsout_color);
	//fsout_color = vec4(vec3((vec4(N, 1.0) / 2.0) + 0.5), 1.0);
}