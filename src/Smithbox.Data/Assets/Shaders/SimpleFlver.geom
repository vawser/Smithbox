#version 450

// Geometry stage of the SimpleFlver _wire variants, the wireframe view: passes each triangle through as it is, with
// the barycentrics the fragment stage draws its edges from (Wireframe.glsl). The inputs are SimpleFlver.vert's
// outputs, which its _selected and _focus copies share. Built once for each of them, see compile.ps1.

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

layout(location = 0) in vec4 gsin_color[];
layout(location = 1) flat in uint gsin_entityid[];
layout(location = 8) in vec3 gsin_clipPos[];

layout(location = 0) out vec4 fsin_color;
layout(location = 1) flat out uint fsin_entityid;
layout(location = 8) out vec3 fsin_clipPos;

// Wireframe.glsl
layout(location = 18) noperspective out vec3 fsin_bary;

void main()
{
	for (int i = 0; i < 3; i++)
	{
		gl_Position = gl_in[i].gl_Position;

		fsin_color = gsin_color[i];
		fsin_entityid = gsin_entityid[i];
		fsin_clipPos = gsin_clipPos[i];
		fsin_bary = vec3(i == 0 ? 1.0 : 0.0, i == 1 ? 1.0 : 0.0, i == 2 ? 1.0 : 0.0);

		EmitVertex();
	}

	EndPrimitive();
}
