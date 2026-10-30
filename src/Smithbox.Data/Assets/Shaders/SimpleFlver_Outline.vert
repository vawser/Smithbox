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
	// 0: material id
	// 1: bone base
	// 3: entity id
	uvec4 materialID;
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
    readonly instanceData idata[];
};

layout(set = 7, binding = 0, std140) buffer BoneBuffer
{
    readonly mat4 bones[];
};

layout(set = 7, binding = 0, std430) buffer SkinBuffer
{
    readonly uvec4 skin[];
};
layout (constant_id = 50) const bool c_normalWBoneTransform = false;

layout(location = 0) in vec3 position;
layout(location = 1) in ivec4 normal;
layout(location = 0) out vec4 fsin_color;
layout(location = 1) out uint fsin_entityid;

// materialID.z is 0 unless the instance is being animated, which keeps static models untouched:
//   1            - rigid, the bone only needs to reach the normals as well
//   high bit set - weighted, the low bits plus gl_VertexIndex index the skin data
mat4 GetBoneMatrix()
{
	uint skinRef = idata[gl_InstanceIndex].materialID.z;
	uint boneBase = idata[gl_InstanceIndex].materialID.y;

	if ((skinRef & 0x80000000u) != 0u)
	{
		uvec4 s = skin[(skinRef + uint(gl_VertexIndex)) & 0x7FFFFFFFu];
		vec4 weights = unpackUnorm4x8(s.z);

		return bones[boneBase + (s.x & 0xFFFFu)] * weights.x +
			bones[boneBase + (s.x >> 16)] * weights.y +
			bones[boneBase + (s.y & 0xFFFFu)] * weights.z +
			bones[boneBase + (s.y >> 16)] * weights.w;
	}

	if (c_normalWBoneTransform)
	{
		return bones[boneBase + normal.w];
	}

	return mat4(1.0);
}
void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	mat4 bone = GetBoneMatrix();
	vec3 fsin_normal = normalize(mat3(idata[gl_InstanceIndex].materialID.z != 0u ? w * bone : w) * vec3(normal));
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
	
	vec3 ssnormal = mat3(sceneparam.projection) * mat3(sceneparam.view) * fsin_normal;

	vec4 posbase = sceneparam.projection * sceneparam.view * w * bone * vec4(position, 1);

    gl_Position = posbase + vec4(ssnormal, 0.0) * posbase.w * 0.005;
}