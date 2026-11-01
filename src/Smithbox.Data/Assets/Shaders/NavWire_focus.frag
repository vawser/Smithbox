#version 450
layout(location = 1) in vec4 color;
layout(location = 2) in vec3 view;
layout(location = 0) out vec4 fsout_color;

// The share of pixels kept for a map that is out of focus
#define FOCUS_OPACITY 0.4

float OrderedDither()
{
	mat4 bayerMatrix = mat4(
		 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
		12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
		 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
		15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
	);

	ivec2 pixelCoord = ivec2(gl_FragCoord.xy) % 4;
	return bayerMatrix[pixelCoord.x][pixelCoord.y];
}

vec3 FocusGrey(vec3 color)
{
	return mix(vec3(dot(color, vec3(0.299, 0.587, 0.114))), vec3(0.5), 0.5);
}

void main()
{
	if (FOCUS_OPACITY < OrderedDither())
	{
		discard;
	}

	fsout_color = vec4(FocusGrey(color.xyz), 0.5);
}
