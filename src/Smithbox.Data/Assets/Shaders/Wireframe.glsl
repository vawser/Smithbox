// The wireframe view's lines, for the fragment stage of the _wire shader variants. Indexed meshes share their
// vertices between triangles, so the vertex stage can't say where a triangle's edges are: a geometry stage
// (FlverShader.geom, SimpleFlver.geom) passes each triangle through, giving its corners the barycentrics
// (1,0,0), (0,1,0) and (0,0,1). Across the triangle the smallest of them falls to 0 at the nearest edge.
//
// Included after the including shader's sceneparam block, which these read.
#ifndef WIREFRAME_GLSL
#define WIREFRAME_GLSL

// Past the flver shader's own inputs, which end at 17
layout(location = 18) noperspective in vec3 fsin_bary;

// How much of the pixel the lines cover, 0 to 1. Each triangle draws its half of the line along its edges, so the
// two triangles sharing an edge make a line of the full width, in pixels at any distance, with a pixel of fade
// either side. Takes derivatives, so it is called before any discard.
float WireframeCoverage()
{
	vec3 pixels = fsin_bary / max(fwidth(fsin_bary), vec3(1e-6));
	float nearest = min(pixels.x, min(pixels.y, pixels.z));

	return clamp(sceneparam.wireframeParams.x * 0.5 + 0.5 - nearest, 0.0, 1.0);
}

// Whether the surfaces are left out, so only the lines are drawn
bool WireframeOnly()
{
	return sceneparam.wireframeParams.y > 0.5;
}

// Whether a fragment of the lines alone is kept: the half covered most, as the lines are drawn without blending
bool WireframeKeeps(float coverage)
{
	return coverage >= 0.5;
}

// The surface's display color with the lines over it, or the line color alone when the surfaces are left out
vec3 WireframeOver(vec3 surface, vec3 line, float coverage)
{
	return WireframeOnly() ? line : mix(surface, line, coverage * sceneparam.wireframeColor.a);
}

#endif
