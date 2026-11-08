#version 450
#extension GL_GOOGLE_include_directive : require

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

#include "InstanceData.glsl"

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
layout(location = 8) out vec3 fsin_clipPos; // the world position, for CAMERA_CLIP

// Stale or wrong instance data can ask for any bone, and reading past the end of the buffer loses the device
mat4 ReadBone(uint index)
{
	return index < uint(bones.length()) ? bones[index] : mat4(1.0);
}

// materialID.z is 0 unless the instance is being animated, which keeps static models untouched:
//   1            - rigid, the bone only needs to reach the normals as well
//   high bit set - weighted, the low bits plus gl_VertexIndex index the skin data
mat4 GetBoneMatrix()
{
	uint skinRef = idata[gl_InstanceIndex].materialID.z;
	uint boneBase = idata[gl_InstanceIndex].materialID.y;

	if ((skinRef & 0x80000000u) != 0u)
	{
		uint skinIndex = (skinRef + uint(gl_VertexIndex)) & 0x7FFFFFFFu;
		if (skinIndex >= uint(skin.length()))
		{
			return mat4(1.0);
		}

		uvec4 s = skin[skinIndex];
		vec4 weights = unpackUnorm4x8(s.z);

		return ReadBone(boneBase + (s.x & 0xFFFFu)) * weights.x +
			ReadBone(boneBase + (s.x >> 16)) * weights.y +
			ReadBone(boneBase + (s.y & 0xFFFFu)) * weights.z +
			ReadBone(boneBase + (s.y >> 16)) * weights.w;
	}

	if (c_normalWBoneTransform)
	{
		return ReadBone(boneBase + (uint(normal.w) & 0xFFu));
	}

	return mat4(1.0);
}
void main()
{
	mat4 w = idata[gl_InstanceIndex].world;
	mat4 bone = GetBoneMatrix();
	fsin_clipPos = vec3(w * bone * vec4(position, 1));
	vec3 tnormal = normalize(mat3(idata[gl_InstanceIndex].materialID.z != 0u ? w * bone : w) * vec3(normal));
    fsin_color = vec4((vec3((vec4(tnormal, 1.0)) + sceneparam.simpleFlverBrightness) * sceneparam.simpleFlverSaturation), 1.0);
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;
    
	gl_Position = sceneparam.projection * sceneparam.view * w * bone * vec4(position, 1);

}