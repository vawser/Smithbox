#version 450
layout(location = 0) in vec2 fsin_plane;
layout(location = 1) flat in uint fsin_mat;
layout(location = 2) in vec3 fsin_viewPos;
layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

// See DbgMaterial.cs
struct materialData
{
	uint color;
	float gridCells;
	// Distance from the camera where the grid has faded out, 0 for no fade
	float fadeDistance;
	// Bit 0: draw the axis lines through the grid's origin
	// Bits 16-31: squares between major lines, 0 or 1 for none
	uint flags;
	// Where the quad's center is, in squares from the grid's origin, when the grid follows the camera. Whole squares.
	vec2 offset;
	// Scales the line widths, 1 for the widths below
	float lineWidth;
	float pad;
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly materialData matdata[];
};

const uint c_flagAxisLines = 1u;

// The distance fade starts at this fraction of the fade distance
const float c_distanceFadeStart = 0.5;

// Lines fade out as their squares shrink from 8 pixels across to 2, as they would only shimmer below that
const float c_fadeStart = 1.0 / 8.0;
const float c_fadeEnd = 1.0 / 2.0;

// Line widths in pixels
const float c_minorWidth = 1.0;
const float c_majorWidth = 1.5;
const float c_axisWidth = 2.0;

// Minor lines are dimmed when there are major lines, so the major ones stand out
const float c_minorAlphaWithMajor = 0.45;

// The grid's local X axis (the line where z = 0) and Z axis (the line where x = 0), in display color
const vec3 c_axisXColor = vec3(0.85, 0.25, 0.25);
const vec3 c_axisZColor = vec3(0.25, 0.45, 0.9);

// Coverage of a line of the given width, from the distance to it in pixels
vec2 lineCoverage(vec2 pixelsToLine, float width)
{
	return 1.0 - clamp(pixelsToLine - (width - 1.0) * 0.5, 0.0, 1.0);
}

// Coverage of a set of grid lines one unit of cell apart
float gridCoverage(vec2 cell, vec2 cellsPerPixel, float width)
{
	vec2 pixelsToLine = abs(fract(cell - 0.5) - 0.5) / cellsPerPixel;
	vec2 lines = lineCoverage(pixelsToLine, width) * (1.0 - smoothstep(c_fadeStart, c_fadeEnd, cellsPerPixel));

	return max(lines.x, lines.y);
}

void main()
{
	uint col = matdata[fsin_mat].color;
	float gridCells = matdata[fsin_mat].gridCells;
	float fadeDistance = matdata[fsin_mat].fadeDistance;
	uint flags = matdata[fsin_mat].flags;
	float majorInterval = float(flags >> 16);
	vec2 offset = matdata[fsin_mat].offset;
	float lineWidth = max(matdata[fsin_mat].lineWidth, 0.25);

	// The plane is one square larger than the grid, so that the outer lines aren't cut in half by its edge.
	// cell is relative to the quad's center; the offset is whole squares, so the lines line up without it, and
	// leaving it out keeps the precision of a grid far from its origin.
	vec2 cell = fsin_plane * (gridCells + 1.0);
	vec2 cellsPerPixel = vec2(
		length(vec2(dFdx(cell.x), dFdy(cell.x))),
		length(vec2(dFdx(cell.y), dFdy(cell.y))));

	float lines = gridCoverage(cell, cellsPerPixel, c_minorWidth * lineWidth);

	// Major lines are spaced further apart, so they keep showing after the minor lines fade out
	if (majorInterval > 1.0)
	{
		// The major lines are every majorInterval squares from the grid's origin
		vec2 majorCell = (cell + mod(offset, majorInterval)) / majorInterval;
		float major = gridCoverage(majorCell, cellsPerPixel / majorInterval, c_majorWidth * lineWidth);
		lines = max(lines * c_minorAlphaWithMajor, major);
	}

	vec4 baseColor = vec4(col & 255, (col >> 8) & 255, (col >> 16) & 255, (col >> 24) & 255) / 255.0;
	vec3 color = baseColor.rgb;
	float alpha = baseColor.a * lines;

	// Axis lines don't fade as their squares shrink, as there is only one of each
	if ((flags & c_flagAxisLines) != 0u)
	{
		// The axes run through the grid's origin, not the quad's center
		vec2 axis = lineCoverage(abs(cell + offset) / cellsPerPixel, c_axisWidth * lineWidth);

		// axis.x is the line where x = 0, which runs along Z
		color = mix(color, c_axisZColor, axis.x);
		alpha = max(alpha, baseColor.a * axis.x);

		color = mix(color, c_axisXColor, axis.y);
		alpha = max(alpha, baseColor.a * axis.y);
	}

	vec2 pixelsOutside = (abs(cell) - gridCells) / cellsPerPixel;
	float coverage = alpha * (1.0 - clamp(max(pixelsOutside.x, pixelsOutside.y), 0.0, 1.0));

	// Fade with distance from the camera, so a large grid doesn't end in a hard edge
	if (fadeDistance > 0.0)
	{
		coverage *= 1.0 - smoothstep(fadeDistance * c_distanceFadeStart, fadeDistance, length(fsin_viewPos));
	}

	if (coverage <= 0.0)
	{
		discard;
	}

	fsout_color = vec4(color, coverage);

	// In the HDR target the tone map applies the gamma, so this display color is squared to linear light
	if (c_linearOutput)
		fsout_color.rgb *= fsout_color.rgb;
}
