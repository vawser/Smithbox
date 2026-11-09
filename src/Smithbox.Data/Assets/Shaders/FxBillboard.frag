#version 450
#extension GL_GOOGLE_include_directive : require
#extension GL_EXT_nonuniform_qualifier : require

// Fragment stage of the FX Editor's billboard particles (see FxBillboard.vert).
//
// - Texture: the layer's frame of a sprite sheet (columns x rows, totalFrames frames), blended with the next frame
//   when interpolating. MultiTextureBillboardEx multiplies its three layers, each with its own uv offset, scale and
//   colour; the first layer is the one split into frames. A texture that isn't loaded yet (or is missing) draws
//   as a soft round spot, so the particle can still be seen.
// - Alpha: clamped to 0-1, with the hard cut-off (alphaThreshold) and the stretched one (alphaFadeThreshold),
//   both 0-255 in the file and 0-1 here.
// - Depth blend: near opaque geometry the particle fades out, by the distance to the scene behind it.
// - Blend mode: the pipeline does the blending; multiply draws its colour toward white by its alpha.

layout(location = 0) in vec2 fsin_uv;
layout(location = 1) in vec4 fsin_color;
layout(location = 2) in vec4 fsin_frame;
layout(location = 3) in vec4 fsin_layer1;
layout(location = 4) in vec4 fsin_layer2;
layout(location = 5) in vec4 fsin_layer3;
layout(location = 6) in vec4 fsin_layer1Color;
layout(location = 7) in vec4 fsin_layer2Color;
layout(location = 8) in vec4 fsin_layer3Color;
layout(location = 9) flat in uint fsin_mat;

layout(location = 0) out vec4 fsout_color;

// True when drawn into the HDR scene target, see SceneTarget.cs
layout (constant_id = 97) const bool c_linearOutput = false;

#include "SceneParams.glsl"

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

layout(set = 2, binding = 0) uniform texture2D globalTextures[];

struct fxMaterial
{
	uvec4 textures;
	uvec4 params;
	uvec4 extra;
	vec4 tuning;
};

layout(set = 4, binding = 0, std140) buffer materials
{
	readonly fxMaterial matdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;

const uint FlagInterpolate = 1u;
const uint FlagDepthBlend = 2u;
const uint FlagPremultiply = 4u;
const uint FlagMultiTexture = 8u;
const uint FlagSolid = 32u;

int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// A texture, or a soft round spot when it isn't loaded
vec4 Sample(uint tex, vec2 uv, vec2 spotUv)
{
	if (tex == 0u)
	{
		float r = length(spotUv - 0.5) * 2.0;
		return vec4(1, 1, 1, clamp(1.0 - r, 0.0, 1.0));
	}
	return texture(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], linearSampler), uv);
}

// The first layer: a frame of a sprite sheet, with explicit gradients from the unwrapped uv so a wrap doesn't pick
// the smallest mip along the seam. With clampCell the uv stays half a texel (at the mip being read) inside the
// frame, so linear filtering takes nothing from the opposite edge (the sampler repeats) or the neighbouring frame:
// multiplied up by the alpha and rgb multipliers, that bleed showed as the quad's outline. Tiling layers
// (scrolled, offset or scaled uvs, and trails) keep wrapping.
vec4 SampleFrame(uint tex, vec2 uv, vec2 dx, vec2 dy, float frame, uint columns, uint total, bool clampCell, vec2 spotUv)
{
	if (tex == 0u)
	{
		float r = length(spotUv - 0.5) * 2.0;
		return vec4(1, 1, 1, clamp(1.0 - r, 0.0, 1.0));
	}

	uint rows = total <= 1u ? 1u : (total + columns - 1u) / columns;
	uint cols = total <= 1u ? 1u : columns;
	vec2 grid = vec2(cols, rows);
	uint f = total <= 1u ? 0u : uint(mod(max(frame, 0.0), float(total)));
	vec2 cell = vec2(f % cols, f / cols);

	vec2 local = fract(uv);
	if (clampCell)
	{
		vec2 cellTexels = vec2(textureSize(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], linearSampler), 0)) / grid;
		float rho = max(length(dx * cellTexels), length(dy * cellTexels));
		float texel = exp2(max(log2(max(rho, 1e-6)), 0.0));
		vec2 inset = min(vec2(0.5 * texel) / max(cellTexels, vec2(1.0)), vec2(0.5));
		local = clamp(uv, inset, 1.0 - inset);
	}

	return textureGrad(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], linearSampler),
		(cell + local) / grid, dx / grid, dy / grid);
}


// Distance from the camera along its axis of a value in the depth buffer
float ViewDepth(float depth)
{
	mat4 projection = sceneparam.projection;
	if (abs(projection[2][3]) < 1e-6)
	{
		return abs((depth - projection[3][2]) / projection[2][2]);
	}
	return abs(projection[3][2] / (depth * projection[2][3] - projection[2][2]));
}

void main()
{
	fxMaterial m = matdata[fsin_mat];
	uint flags = m.params.z;
	uint columns = max(m.params.x, 1u);
	uint total = max(m.params.y, 1u);
	float frame = fsin_frame.x;

	// The first layer, framed (and blended with the next frame when interpolating)
	vec2 uv1 = fsin_uv * fsin_layer1.zw + fsin_layer1.xy;
	vec2 dx1 = dFdx(uv1);
	vec2 dy1 = dFdy(uv1);
	const uint SegmentOrientation = 100u;
	bool tiling = m.params.w == SegmentOrientation
		|| any(greaterThan(abs(fsin_layer1.xy), vec2(1e-4)))
		|| any(greaterThan(abs(fsin_layer1.zw - 1.0), vec2(1e-4)));
	vec4 tex = SampleFrame(m.textures.x, uv1, dx1, dy1, frame, columns, total, !tiling, fsin_uv);
	if ((flags & FlagInterpolate) != 0u && total > 1u && fract(frame) > 0.001)
	{
		vec4 next = SampleFrame(m.textures.x, uv1, dx1, dy1, frame + 1.0, columns, total, !tiling, fsin_uv);
		tex = mix(tex, next, fract(frame));
	}

	// Lines have no texture: a solid strip
	if ((flags & FlagSolid) != 0u)
	{
		tex = vec4(1.0);
	}

	// Layers 2 and 3 by the layer opacity mode (extra.y): 0 their colours multiply and only layer 1's alpha is the
	// opacity; 1 (both) and 2 (layer 2 only) take their colour away and make their brightness the opacity, which is
	// how a texture without an alpha channel (DXT1) is cut out
	if ((flags & FlagMultiTexture) != 0u)
	{
		uint opacityMode = m.extra.y;
		tex *= fsin_layer1Color;
		if (m.textures.y != 0u)
		{
			vec4 l2 = Sample(m.textures.y, fsin_uv * fsin_layer2.zw + fsin_layer2.xy, fsin_uv) * fsin_layer2Color;
			if (opacityMode == 1u || opacityMode == 2u)
			{
				tex.a *= max(l2.r, max(l2.g, l2.b)) * l2.a;
			}
			else
			{
				tex.rgb *= l2.rgb;
			}
		}
		if (m.textures.z != 0u)
		{
			vec4 l3 = Sample(m.textures.z, fsin_uv * fsin_layer3.zw + fsin_layer3.xy, fsin_uv) * fsin_layer3Color;
			if (opacityMode == 1u)
			{
				tex.a *= max(l3.r, max(l3.g, l3.b)) * l3.a;
			}
			else
			{
				tex.rgb *= l3.rgb;
			}
		}
	}

	if ((flags & FlagPremultiply) != 0u)
	{
		tex.rgb *= tex.a;
	}

	vec4 color = tex * fsin_color;
	float alpha = clamp(color.a, 0.0, 1.0);

	// Hard cut-off, then the stretched one
	if (alpha < fsin_frame.y)
	{
		discard;
	}
	float fadeThreshold = fsin_frame.z;
	if (fadeThreshold > 0.0)
	{
		alpha = clamp((alpha - fadeThreshold) / max(1.0 - fadeThreshold, 1e-4), 0.0, 1.0);
	}

	// Soft edges where the particle meets the scene behind it
	if ((flags & FlagDepthBlend) != 0u && sceneparam.sceneDepthTexture != 0u && m.tuning.y > 0.0)
	{
		float stored = texelFetch(
			sampler2D(globalTextures[nonuniformEXT(TextureIndex(sceneparam.sceneDepthTexture))], linearSampler),
			ivec2(gl_FragCoord.xy), 0).r;
		if (stored > 0.0)
		{
			float gap = ViewDepth(stored) - ViewDepth(gl_FragCoord.z);
			alpha *= clamp(gap / m.tuning.y, 0.0, 1.0);
		}
	}

	if (alpha <= 0.0)
	{
		discard;
	}

	vec3 rgb = color.rgb * m.tuning.x;
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
