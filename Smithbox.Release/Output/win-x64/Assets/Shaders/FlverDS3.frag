#version 450
#extension GL_EXT_nonuniform_qualifier : require

layout(location = 0) in vec2 fsin_texcoord;
layout(location = 1) in vec3 fsin_view;
layout(location = 2) in mat3 fsin_worldToTangent;
layout(location = 5) in vec3 fsin_normal;
layout(location = 6) in vec4 fsin_bitangent;
layout(location = 7) in vec4 fsin_color;
layout(location = 8) flat in uint fsin_mat;
layout(location = 0) out vec4 fsout_color;

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
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
layout(set = 2, binding = 0) uniform textureCube globalTexturesCube[];

struct materialData
{
	uint colorTex;
	uint normalTex;
	uint specTex;
};

layout(set = 3, binding = 0, std140) buffer materials
{
    readonly materialData matdata[];
};

layout(set = 4, binding = 0) uniform sampler linearSampler;
layout(set = 4, binding = 1) uniform sampler anisoLinearSampler;

float Epsilon = 0.00001;

float LdotNPower = 0.1;

void main()
{
    //fsout_color = vec4(1.0, 1.0, 1.0, 1.0);
	vec3 lightdir = normalize(vec3(sceneparam.lightDirection));
	
	vec3 viewVec = normalize(fsin_view);
	vec4 diffuseColor = texture(sampler2D(globalTextures[nonuniformEXT(matdata[fsin_mat].colorTex)], anisoLinearSampler), fsin_texcoord.xy); //vec3(0.5) * vec3(0.5);
	if (diffuseColor.w < 0.5)
	{
		discard;
	}
	
	vec3 normalColor = texture(sampler2D(globalTextures[nonuniformEXT(matdata[fsin_mat].normalTex)], anisoLinearSampler), fsin_texcoord.xy).xyz;
	vec3 L = -lightdir;
	vec3 H = normalize(L + viewVec);
	vec3 F0 = texture(sampler2D(globalTextures[nonuniformEXT(matdata[fsin_mat].specTex)], anisoLinearSampler), fsin_texcoord.xy).xyz;
	//F0 *= F0;
	
	float roughness = 1.0 - normalColor.z;
	
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
	
	float alpha = roughness * roughness;
	float alphasquare = alpha * alpha;
	
	vec3 finalDiffuse = diffuseColor.xyz * LdotN;
	
	vec3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;
	float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
	
	float specPower = exp2((1 - roughness) * 13.0);
    specPower = max(1.0, specPower / (specPower * 0.01 + 1.0)) * 1;
    float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
	
	vec3 specular = D * F * pow(LdotN, LdotNPower);
	
	float envMip = min(6.0, -(1 - roughness) * 6.5 + 6.5);
	vec3 reflectVec = reflect(-viewVec, N);
	vec3 ambientSpec = textureLod(samplerCube(globalTexturesCube[nonuniformEXT(sceneparam.envmap)], linearSampler), vec3(reflectVec * vec3(1, 1, -1)), envMip).xyz;
	ambientSpec *= sceneparam.ambientLightMult;
	vec3 ambientDiffuse = textureLod(samplerCube(globalTexturesCube[nonuniformEXT(sceneparam.envmap)], linearSampler), vec3(N * vec3(1, 1, -1)), 5).xyz;
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
	//fsout_color = vec4(vec3((vec4(N, 1.0) / 2.0) + 0.5), 1.0);
}