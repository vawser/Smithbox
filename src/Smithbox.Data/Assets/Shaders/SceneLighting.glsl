// The view's lighting sets at set 0, binding 1, beside the scene params, laid out as SceneLightingData.cs (std430).
// Entry 0 is the view's own lighting: the map's GPARAM lighting where it lights the scene, otherwise the viewport's
// sun. The entries after it are for per-part lighting sets (Demon's Souls' LightIDs).
//
// Read single fields, as LIGHTING.sun.color, never the whole entry into a local: copying a struct out of a storage
// buffer is what crashed the NVIDIA driver's shader compiler (see DES Light Set Redesign.md).
#ifndef SCENE_LIGHTING_GLSL
#define SCENE_LIGHTING_GLSL

#include "SceneParams.glsl"

struct sceneLighting
{
	mapDirectionalLight sun;          // direction: the way the light travels; color/specColor: rgb, a: intensity with the exposure

	// The hemisphere light, rgb already multiplied by intensity and exposure
	vec4 hemiUp;
	vec4 hemiDown;

	// The model set of the map's GPARAM LightSet, for characters when sceneparam.mapLightingFlags has 4: its sun,
	// Light1, LocalLight1 and LocalLight2, and its hemisphere
	mapDirectionalLight modelLights[4];
	vec4 modelHemiUp;
	vec4 modelHemiDown;
};

layout(set = 0, binding = 1, std430) readonly buffer SceneLightingBuffer
{
	sceneLighting lightingSets[];
};

// The set the current draw is lit by; always 0 until per-part sets are drawn
#ifndef LIGHTING_SET_INDEX
#define LIGHTING_SET_INDEX 0u
#endif

#define LIGHTING lightingSets[LIGHTING_SET_INDEX]

#endif
