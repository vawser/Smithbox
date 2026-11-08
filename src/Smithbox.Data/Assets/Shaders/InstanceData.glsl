// The instances every scene shader draws, at set 1, binding 0, laid out as InstanceData.cs (std140, 96 bytes). The
// one definition: vertex shaders #include this rather than copying the struct.
#ifndef INSTANCE_DATA_GLSL
#define INSTANCE_DATA_GLSL

struct instanceData
{
	mat4 world;

	// x: the material id, with MeshRenderableProxy.NoShadowMaterialFlag in the top bit
	// y: the first bone in the bone buffer
	// z: the skin reference, or for a static instance its lightmap with InstanceData.LightmapFlag
	// w: the entity id, for picking
	uvec4 materialID;

	// x: the lighting set the instance is lit by (SceneLighting.glsl), 0 for the view's own; yzw reserved
	uvec4 lighting;
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
	readonly instanceData idata[];
};

#endif
