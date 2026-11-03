#version 450

// Shows one of the scene target's surface attachments in place of the image, see SurfaceViewPass.cs. Normals,
// roughness and specular are display values, which the tone map passes through (it squares them back here first,
// as it takes a square root); the indirect light is light, tone mapped as the scene is. Pixels no flver drew show
// as a dark grey.

#define VIEW_NORMALS 1
#define VIEW_ROUGHNESS 2
#define VIEW_SPECULAR 3
#define VIEW_INDIRECT 4

layout(set = 0, binding = 0) uniform SurfaceViewParams
{
	vec4 settings; // x: what to show
} params;

layout(set = 0, binding = 1) uniform texture2D surface;
layout(set = 0, binding = 2) uniform texture2D indirect;
layout(set = 0, binding = 3) uniform sampler pointSampler;

layout(location = 0) out vec4 fsout_color;

vec3 OctahedralDecode(vec2 e)
{
	vec3 n = vec3(e, 1.0 - abs(e.x) - abs(e.y));
	if (n.z < 0.0)
	{
		n.xy = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
	}

	return normalize(n);
}

void main()
{
	ivec2 pixel = ivec2(gl_FragCoord.xy);
	int view = int(params.settings.x + 0.5);

	if (view == VIEW_INDIRECT)
	{
		fsout_color = vec4(texelFetch(sampler2D(indirect, pointSampler), pixel, 0).rgb, 1.0);
		return;
	}

	vec4 s = texelFetch(sampler2D(surface, pointSampler), pixel, 0);
	vec3 display = vec3(0.1);

	if (s.z >= 0.0)
	{
		if (view == VIEW_NORMALS)
			display = OctahedralDecode(s.xy) * 0.5 + 0.5;
		else if (view == VIEW_ROUGHNESS)
			display = vec3(s.z);
		else
			display = vec3(s.w);
	}

	fsout_color = vec4(display * display, 1.0);
}
