// Legacy family: Demon's Souls and Dark Souls: Prepare to Die (c_isLegacy, see GameFamily.glsl).
// Phong lighting, the HemEnv env cubemaps, and Demon's Souls' material lightmaps and vertex colors.
// The hooks are dispatched by Families.glsl; a hook that returns false leaves the shared core's path to run.
// Plan: Documentation/Shader Family Split Plan.md, S3.

// Traditional Phong: no Fresnel or roughness, a fixed power 4 highlight
bool Legacy_ShadeLight(Surface s, float LdotN, float NdotH, vec3 radiance, vec3 specularRadiance, out vec3 light)
{
	vec3 diffuse = s.diffuse * LdotN;
	// no highlight on the side facing away from the light
	vec3 specular = s.F0 * pow(NdotH, 4.0) * (LdotN > 0.0 ? 1.0 : 0.0);
	light = diffuse * radiance + specular * specularRadiance;
	return true;
}

#ifdef MATERIAL_BLEND
// The scale on a DES g_Lightmap's light. 1 until matched against the game
#define DES_LIGHTMAP_SCALE 1.0

// A DES map piece's g_Lightmap, a texture of its own material read through its last UV set (the second, or the
// third for a multi texture material, whose second is layer 2's): its baked light stands in for the light from
// around. Squared from its stored gamma, as DS3's DOL 1 is; the game's own scale isn't traced, so it is
// DES_LIGHTMAP_SCALE until matched by eye. 0 until the texture is on the GPU
void Legacy_BakedAmbient(Surface s, inout vec3 ambientDiffuse)
{
	if (c_gameID != GAME_DES)
		return;

	uint desLightmap = MaterialValue(ROW_EXTRA, 2);
	if (desLightmap == 0u)
		return;

  #ifdef MATERIAL_UV3
	vec2 lightmapUv = fsin_texcoord3;
  #else
	vec2 lightmapUv = g_uv[1];
  #endif
	vec3 lm = texture(sampler2D(globalTextures[nonuniformEXT(TextureIndex(desLightmap))], linearSampler), lightmapUv).rgb;
	ambientDiffuse = lm * lm * DES_LIGHTMAP_SCALE;
	g_lightSource = 2u;
}
#endif

// The light from around, given the ambient light after the scene multiplier. A DES map material's HemEnv lighting
// puts the env cubemaps on top of the hemisphere, the specular one through the specular map; a lightmap, where there
// is one, already is the light from around, so the diffuse env is left out. Without HemEnv, diffuse only
bool Legacy_IndirectLight(Surface s, vec3 ambientDiffuse, out vec3 light)
{
	if (g_envLit)
	{
		vec3 envDiffuse;
		vec3 envSpecular;
		EnvLight(s.N, reflect(-s.V, s.N), envDiffuse, envSpecular);

		if (g_lightSource != 2u)
			ambientDiffuse += envDiffuse * sceneparam.ambientLightMult;

		g_indirectDiffuse = s.diffuse * ambientDiffuse;
		g_indirectSpecular = s.F0 * envSpecular * sceneparam.ambientLightMult;
		light = g_indirectDiffuse + g_indirectSpecular;
		return true;
	}

	g_indirectDiffuse = s.diffuse * ambientDiffuse;
	light = g_indirectDiffuse;
	return true;
}

// Every DES shader is a "Col" one (DS_Phn_ColDif...): the diffuse times the vertex color. FLVER0 vertices without
// a color are filled white (FlverResource.FillDesColor). The alpha is the multi texture weight, not opacity
void Legacy_Albedo(inout vec4 diffuseColor)
{
	if (c_gameID == GAME_DES)
		diffuseColor.rgb *= fsin_color.rgb;
}
