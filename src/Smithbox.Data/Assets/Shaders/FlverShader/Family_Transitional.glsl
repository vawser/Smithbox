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

// DS2's normal maps are DXT5nm (1687 of the 1689 map _n textures): red is 1 and x is in alpha. Its shaders
// (Flver_ColDifSpcBumpGI.fpo, disassembled) read x as red * alpha, which also takes red from the few that aren't,
// whose alpha is 1 (a DXT1, a DXT5 with alpha 1). DS1R's normal maps are DX10 formats and aren't checked
bool Transitional_IsDs2()
{
	return c_gameID == GAME_DS2 || c_gameID == GAME_DS2S;
}

void Transitional_NormalSample(inout vec4 c)
{
	if (Transitional_IsDs2())
		c.x *= c.w;
}

// The shared core's tangent space (x along T, y along B). DS2's shaders build N_world = x * B + y * T + z * N, with
// B = cross(N, T) * T.w (FlverVS_VsColorNBTGI_PsSingleNBTGI.vpo's TEXCOORD4), as ER's do: its models have the tangent
// along +V too, so x is swapped onto B. DS1R's tangents are along +V as well, but its shaders aren't checked
void Transitional_TangentNormal(inout vec3 n)
{
	if (Transitional_IsDs2())
		n.xy = n.yx;
}
