#version 450

// Shows one of the scene target's surface attachments in place of the image, see SurfaceViewPass.cs. Normals,
// roughness and specular are display values, which the tone map passes through (it squares them back here first,
// as it takes a square root); the indirect light is light, tone mapped as the scene is. Pixels no flver drew show
// as a dark grey. The ambient occlusion is SSAO's visibility, white where open; the reflection is the environment's
// in the Reflection attachment, the screen reflections SSR's trace, and the glow the Glow attachment's light, all
// tone mapped.

#define VIEW_NORMALS 1
#define VIEW_ROUGHNESS 2
#define VIEW_SPECULAR 3
#define VIEW_INDIRECT 4
#define VIEW_OCCLUSION 5
#define VIEW_REFLECTION 6
#define VIEW_SCREEN_REFLECTIONS 7
#define VIEW_GLOW 8

layout(set = 0, binding = 0) uniform SurfaceViewParams
{
	vec4 settings; // x: what to show, y: 1 if SSAO ran this frame, z: 1 if SSR did
} params;

layout(set = 0, binding = 1) uniform texture2D surface;
layout(set = 0, binding = 2) uniform texture2D indirect;
layout(set = 0, binding = 3) uniform sampler pointSampler;
layout(set = 0, binding = 4) uniform texture2D occlusion; // SSAO's half size visibility
layout(set = 0, binding = 5) uniform texture2D reflection; // SceneTarget.Reflection
layout(set = 0, binding = 6) uniform texture2D screenReflections; // SSR's half size trace, color x weight
layout(set = 0, binding = 7) uniform texture2D glowLight; // SceneTarget.Glow

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

	if (view == VIEW_GLOW)
	{
		fsout_color = vec4(texelFetch(sampler2D(glowLight, pointSampler), pixel, 0).rgb, 1.0);
		return;
	}

	if (view == VIEW_REFLECTION)
	{
		fsout_color = vec4(texelFetch(sampler2D(reflection, pointSampler), pixel, 0).rgb, 1.0);
		return;
	}

	if (view == VIEW_SCREEN_REFLECTIONS)
	{
		vec3 traced = params.settings.z > 0.5 ? texelFetch(sampler2D(screenReflections, pointSampler), pixel / 2, 0).rgb : vec3(0.0);
		fsout_color = vec4(traced, 1.0);
		return;
	}

	if (view == VIEW_OCCLUSION)
	{
		float visibility = params.settings.y > 0.5 ? texelFetch(sampler2D(occlusion, pointSampler), pixel / 2, 0).r : 0.1;
		fsout_color = vec4(vec3(visibility * visibility), 1.0);
		return;
	}

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
