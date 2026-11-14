// Transitional family: Dark Souls: Remastered and Dark Souls II, vanilla and Scholar (c_isTransitional, see
// GameFamily.glsl). GGX lighting like the modern games, with the gloss kept in the specular texture, which
// MaterialBinding routes to the shininess row (read through c_roughnessFromShininess in the core).
// No hook overrides the shared core yet: a rule for these games alone goes here, so it can't reach the others.
// The hooks are dispatched by Families.glsl. Plan: Documentation/Shader Family Split Plan.md, S3.

bool Transitional_ShadeLight(Surface s, float LdotN, float NdotH, vec3 radiance, vec3 specularRadiance, out vec3 light)
{
	light = vec3(0.0);
	return false;
}

#ifdef MATERIAL_BLEND
void Transitional_BakedAmbient(Surface s, inout vec3 ambientDiffuse)
{
}
#endif

bool Transitional_IndirectLight(Surface s, vec3 ambientDiffuse, out vec3 light)
{
	light = vec3(0.0);
	return false;
}

void Transitional_Albedo(inout vec4 diffuseColor)
{
}
