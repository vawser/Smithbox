#version 450
layout(location = 0) in vec2 fsin_plane;
layout(location = 1) flat in uint fsin_mat;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

struct materialData
{
	uint color;
	float gridCells;
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly materialData matdata[];
};

// Lines fade out as their squares shrink from 8 pixels across to 2, as they would only shimmer below that
const float c_fadeStart = 1.0 / 8.0;
const float c_fadeEnd = 1.0 / 2.0;

void main()
{
	uint col = matdata[fsin_mat].color;
	float gridCells = matdata[fsin_mat].gridCells;

	// The plane is one square larger than the grid, so that the outer lines aren't cut in half by its edge
	vec2 cell = fsin_plane * (gridCells + 1.0);
	vec2 cellsPerPixel = vec2(
		length(vec2(dFdx(cell.x), dFdy(cell.x))),
		length(vec2(dFdx(cell.y), dFdy(cell.y))));

	vec2 pixelsToLine = abs(fract(cell - 0.5) - 0.5) / cellsPerPixel;
	vec2 lines = (1.0 - min(pixelsToLine, vec2(1.0))) * (1.0 - smoothstep(c_fadeStart, c_fadeEnd, cellsPerPixel));

	vec2 pixelsOutside = (abs(cell) - gridCells) / cellsPerPixel;
	float coverage = max(lines.x, lines.y) * (1.0 - clamp(max(pixelsOutside.x, pixelsOutside.y), 0.0, 1.0));

	if (coverage <= 0.0)
	{
		discard;
	}

	fsout_color = vec4(col & 255, (col >> 8) & 255, (col >> 16) & 255, (col >> 24) & 255) / 255.0;
	fsout_color.a *= coverage;

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
