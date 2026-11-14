// The game a pipeline draws for, and the family of games that share its shading model.
// See Documentation/Shader Family Split Plan.md (§3, Appendix A).
//
// c_gameID is specialization constant 0, the project's ProjectType (FlverResource sets it), so the IDs here must be
// ProjectType's values. The family constants are worked out from it when the pipeline is made, so the branches of the
// other families cost nothing.
//
// A rule for one family tests its family constant. A rule for one game inside a family tests the game, but only
// within its family's code. The C# side's membership is RenderProfile.FamilyOf (Renderer/Scene/RenderProfile.cs),
// which MaterialBinding's gloss routing follows; Tools/RenderSnapshotDiff's families mode checks this file against it.

#define GAME_DES 1
#define GAME_DS1_PTDE 2
#define GAME_DS1_REMASTER 3
#define GAME_DS2S 4
#define GAME_DS3 5
#define GAME_BLOODBORNE 6
#define GAME_SEKIRO 7
#define GAME_ER 8
#define GAME_AC6 9
#define GAME_DS2 10
#define GAME_NR 15
#define GAME_DB_NT 16
#define GAME_DB 17

layout (constant_id = 0) const int c_gameID = GAME_DS3;

// Legacy: Phong lighting with HemEnv cubemaps; DES adds its material lightmaps and vertex colors
const bool c_isLegacy = c_gameID == GAME_DES || c_gameID == GAME_DS1_PTDE;

// Transitional: GGX lighting, with the gloss kept in the specular texture
const bool c_isTransitional = c_gameID == GAME_DS1_REMASTER || c_gameID == GAME_DS2 || c_gameID == GAME_DS2S;

// Modern: everything from DS3 on, metallic for the MATBIN games
const bool c_isModern = !c_isLegacy && !c_isTransitional;

// Roughness from the shininess row (1 - r), where the gloss is: the Transitional games, whose specular texture
// MaterialBinding routes there (glossInSpecular), and BB and DS1 with their own shininess samplers. The others take it
// from the normal map's blue. Before 2026-11-14 this tested a GAME_DS2 of 4, which is DS2S, so vanilla DS2 and DS1R
// had their gloss texture bound but never read (Appendix A3)
const bool c_roughnessFromShininess = c_isTransitional || c_gameID == GAME_BLOODBORNE || c_gameID == GAME_DS1_PTDE;
