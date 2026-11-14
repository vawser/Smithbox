// Modern family: Dark Souls III onwards (c_isModern, see GameFamily.glsl): DS3, BB, SDT, ER, AC6, NR and the
// Duskbloods. GGX lighting, metallic for the MATBIN games (c_metallic), DOL lightmaps and irradiance volumes.
// No hook overrides the shared core yet: those paths are data driven (material features and scene tables) rather
// than tests of the game. A rule for these games alone goes here, so it can't reach the older ones.
// The hooks are dispatched by Families.glsl. Plan: Documentation/Shader Family Split Plan.md, S3.

bool Modern_ShadeLight(Surface s, float LdotN, float NdotH, vec3 radiance, vec3 specularRadiance, out vec3 light)
{
	light = vec3(0.0);
	return false;
}

#ifdef MATERIAL_BLEND
void Modern_BakedAmbient(Surface s, inout vec3 ambientDiffuse)
{
}
#endif

bool Modern_IndirectLight(Surface s, vec3 ambientDiffuse, out vec3 light)
{
	light = vec3(0.0);
	return false;
}

void Modern_Albedo(inout vec4 diffuseColor)
{
}
