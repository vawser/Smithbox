// The family hooks: the places where a family of games shades differently from the shared core of FlverShader.frag.
// Each family file implements every hook; the dispatchers below pick one by its family constant (GameFamily.glsl),
// which is a specialization constant, so the pipeline of a game keeps only its own family's code.
//
// A hook that returns false (or leaves its inout alone) means "use the shared core's path", so the GGX/PBR code
// isn't copied into every family. A rule for one game goes inside its family's file, tested by c_gameID there.
// Plan: Documentation/Shader Family Split Plan.md, S3.
//
// Included after the core's helpers (Surface, EnvLight, Hemisphere, the material rows and the g_ globals), as the
// hooks use them; ShadeLight, which comes before them, uses the prototype declared above it.

#include "Family_Legacy.glsl"
#include "Family_Transitional.glsl"
#include "Family_Modern.glsl"

// A light's diffuse and highlight at the surface. Legacy: Phong
bool FamilyShadeLight(Surface s, float LdotN, float NdotH, vec3 radiance, vec3 specularRadiance, out vec3 light)
{
	if (c_isLegacy)
		return Legacy_ShadeLight(s, LdotN, NdotH, radiance, specularRadiance, light);

	if (c_isTransitional)
		return Transitional_ShadeLight(s, LdotN, NdotH, radiance, specularRadiance, light);

	return Modern_ShadeLight(s, LdotN, NdotH, radiance, specularRadiance, light);
}

#ifdef MATERIAL_BLEND
// A material's own baked light, in place of the ambient light from around. Legacy: DES g_Lightmap
void FamilyBakedAmbient(Surface s, inout vec3 ambientDiffuse)
{
	if (c_isLegacy)
		Legacy_BakedAmbient(s, ambientDiffuse);
	else if (c_isTransitional)
		Transitional_BakedAmbient(s, ambientDiffuse);
	else
		Modern_BakedAmbient(s, ambientDiffuse);
}
#endif

// The light from around (diffuse and reflections), given the ambient light after the scene multiplier. Legacy: HemEnv
bool FamilyIndirectLight(Surface s, vec3 ambientDiffuse, out vec3 light)
{
	if (c_isLegacy)
		return Legacy_IndirectLight(s, ambientDiffuse, light);

	if (c_isTransitional)
		return Transitional_IndirectLight(s, ambientDiffuse, light);

	return Modern_IndirectLight(s, ambientDiffuse, light);
}

// The sampled albedo, before the material's diffuse color and the overlays. Legacy: DES vertex color
void FamilyAlbedo(inout vec4 diffuseColor)
{
	if (c_isLegacy)
		Legacy_Albedo(diffuseColor);
	else if (c_isTransitional)
		Transitional_Albedo(diffuseColor);
	else
		Modern_Albedo(diffuseColor);
}
