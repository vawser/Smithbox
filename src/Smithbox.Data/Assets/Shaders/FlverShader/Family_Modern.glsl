// Modern family: Dark Souls III onwards (c_isModern, see GameFamily.glsl): DS3, BB, SDT, ER, AC6, NR and the
// Duskbloods. GGX lighting, metallic for the MATBIN games (c_metallic), DOL lightmaps and irradiance volumes.
// Most paths are data driven (material features and scene tables) rather than tests of the game; the one override
// is ER, AC6 and NR's normal map axes (Modern_TangentNormal). A rule for these games alone goes here, so it can't
// reach the older ones.
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

// ER, AC6 and NR's shaders (M[AMSN]_Gbuf, decompiled) build N_world = x * cross(N, T) * T.w + y * T + z * N: the normal
// map's x along the bitangent and y along the FLVER tangent. Their models have the tangent along +V with w = -1, so
// red runs along +U and green along +V. The shared core takes x along T, which swapped the two (found by the material
// oracle, Claude/Documentation/Material Fidelity/Oracle.md). DS3, BB, SDT and the Duskbloods aren't checked yet
void Modern_TangentNormal(inout vec3 n)
{
	if (c_gameID == GAME_ER || c_gameID == GAME_AC6 || c_gameID == GAME_NR)
		n.xy = n.yx;
}
