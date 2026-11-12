#version 450
#extension GL_GOOGLE_include_directive : require
#extension GL_EXT_nonuniform_qualifier : require

// Fragment stage of the FX Editor's Model particles (see FxModel.vert).
//
// - Colour: the material's albedo (a frame of a sprite sheet when the action splits it, at the scrolled uv) times
//   the particle's colour (the effect's color1-3 and multipliers) times the brightness setting. A texture that
//   isn't loaded yet draws as white, so the model's shape still shows in the effect's colour.
// - Shading: unlit materials (GXFT_InvalidLighting, NoLight shaders) as they are; lit ones get a simple light from
//   the camera, so debris keeps its shape.
// - Blend mode: the pipeline blends; multiply draws its colour toward white by its alpha, as the billboards do.

layout(location = 0) in vec2 fsin_uv;
layout(location = 1) in vec4 fsin_color;
layout(location = 2) in vec3 fsin_normal;
layout(location = 3) in vec3 fsin_worldpos;
layout(location = 4) in float fsin_frame;
layout(location = 5) flat in uint fsin_mat;

layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
	sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];

// As FxModelMaterial: textures.x albedo; params.x columns, y total frames, z flags; extra.x blend mode;
// tuning.x brightness
struct fxModelMaterial
{
	uvec4 textures;
	uvec4 params;
	uvec4 extra;
	vec4 tuning;
};

layout(set = 4, binding = 0, std140) buffer materials
{
	readonly fxModelMaterial matdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;

const uint FlagLit = 1u;

int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

void main()
{
	fxModelMaterial m = matdata[fsin_mat];
	uint columns = max(m.params.x, 1u);
	uint total = max(m.params.y, 1u);
	uint flags = m.params.z;

	vec4 tex = vec4(1.0);
	if (m.textures.x != 0u)
	{
		vec2 uv = fsin_uv;
		vec2 dx = dFdx(uv);
		vec2 dy = dFdy(uv);
		if (total > 1u)
		{
			// A sprite sheet: the frame's cell, the uv wrapped inside it
			uint rows = (total + columns - 1u) / columns;
			vec2 grid = vec2(columns, rows);
			uint f = uint(mod(max(fsin_frame, 0.0), float(total)));
			vec2 cell = vec2(f % columns, f / columns);
			uv = (cell + fract(uv)) / grid;
			dx /= grid;
			dy /= grid;
		}
		tex = textureGrad(sampler2D(globalTextures[nonuniformEXT(TextureIndex(m.textures.x))], linearSampler), uv, dx, dy);
	}

	vec4 color = tex * fsin_color;
	float alpha = clamp(color.a, 0.0, 1.0);
	if (alpha <= 0.002)
	{
		discard;
	}

	vec3 rgb = color.rgb * m.tuning.x;
	if ((flags & FlagLit) != 0u)
	{
		vec3 toEye = VIEW_DIRECTION(fsin_worldpos);
		vec3 n = normalize(fsin_normal);
		float facing = abs(dot(n, toEye));
		rgb *= 0.45 + 0.55 * facing;
	}

	if (!c_linearOutput)
	{
		rgb = clamp(rgb, 0.0, 1.0);
	}

	// Multiply: toward white where the particle is transparent, as the pipeline multiplies without alpha
	if (m.extra.x == 3u)
	{
		fsout_color = vec4(mix(vec3(1.0), rgb, alpha), 1.0);
		return;
	}

	fsout_color = vec4(rgb, alpha);
}
