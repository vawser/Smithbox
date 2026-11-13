#version 450

// Geometry stage of the flver shader's _wire variants, the wireframe view. Each triangle is passed through as it
// is, with the barycentrics the fragment stage draws its edges from (Wireframe.glsl): an indexed mesh shares its
// vertices between triangles, so the vertex stage can't give them.
//
// The inputs are FlverShader.vert's outputs and the outputs FlverShader.frag's inputs, under the same macros
// (MATERIAL_BLEND, MATERIAL_DEFORM, MATERIAL_UV3), so a varying added there is added here as well.

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

layout(location = 0) in vec2 gsin_texcoord[];
layout(location = 1) in vec3 gsin_worldpos[];
layout(location = 2) in mat3 gsin_worldToTangent[];
layout(location = 7) in vec4 gsin_color[];
layout(location = 8) flat in uint gsin_mat[];
layout(location = 9) flat in uint gsin_entityid[];
#ifdef MATERIAL_BLEND
	layout(location = 10) in vec2 gsin_texcoord2[];
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	layout(location = 11) in vec4 gsin_vertexBlend[];
#endif
layout(location = 12) flat in uint gsin_flags[];
#ifdef MATERIAL_BLEND
	layout(location = 13) flat in uint gsin_lightmap[];
	layout(location = 14) flat in mat3 gsin_modelToWorld[];
#endif
layout(location = 17) flat in uint gsin_lightingSet[];
#ifdef MATERIAL_UV3
	layout(location = 19) in vec2 gsin_texcoord3[];
#endif

layout(location = 0) out vec2 fsin_texcoord;
layout(location = 1) out vec3 fsin_worldpos;
layout(location = 2) out mat3 fsin_worldToTangent;
layout(location = 7) out vec4 fsin_color;
layout(location = 8) flat out uint fsin_mat;
layout(location = 9) flat out uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) out vec2 fsin_texcoord2;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	layout(location = 11) out vec4 fsin_vertexBlend;
#endif
layout(location = 12) flat out uint fsin_flags;
#ifdef MATERIAL_BLEND
	layout(location = 13) flat out uint fsin_lightmap;
	layout(location = 14) flat out mat3 fsin_modelToWorld;
#endif
layout(location = 17) flat out uint fsin_lightingSet;
#ifdef MATERIAL_UV3
	layout(location = 19) out vec2 fsin_texcoord3;
#endif

// Wireframe.glsl
layout(location = 18) noperspective out vec3 fsin_bary;

void main()
{
	for (int i = 0; i < 3; i++)
	{
		gl_Position = gl_in[i].gl_Position;

		fsin_texcoord = gsin_texcoord[i];
		fsin_worldpos = gsin_worldpos[i];
		fsin_worldToTangent = gsin_worldToTangent[i];
		fsin_color = gsin_color[i];
		fsin_mat = gsin_mat[i];
		fsin_entityid = gsin_entityid[i];
#ifdef MATERIAL_BLEND
		fsin_texcoord2 = gsin_texcoord2[i];
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
		fsin_vertexBlend = gsin_vertexBlend[i];
#endif
		fsin_flags = gsin_flags[i];
#ifdef MATERIAL_BLEND
		fsin_lightmap = gsin_lightmap[i];
		fsin_modelToWorld = gsin_modelToWorld[i];
#endif
		fsin_lightingSet = gsin_lightingSet[i];
#ifdef MATERIAL_UV3
		fsin_texcoord3 = gsin_texcoord3[i];
#endif

		fsin_bary = vec3(i == 0 ? 1.0 : 0.0, i == 1 ? 1.0 : 0.0, i == 2 ? 1.0 : 0.0);

		EmitVertex();
	}

	EndPrimitive();
}
