// The scene parameters every shader reads at set 0, binding 0, laid out as SceneParam.cs (std140), and the
// light structs inside them. The one definition: shaders #include this rather than copying the struct, so a
// field added here (and in SceneParam.cs, keeping its 16 byte rows) reaches all of them.
#ifndef SCENE_PARAMS_GLSL
#define SCENE_PARAMS_GLSL

#ifndef MAX_SCENE_LIGHTS
#define MAX_SCENE_LIGHTS 16
#endif

struct sceneLight
{
	vec4 positionRange;  // xyz: position, w: range (0 = no limit)
	vec4 directionType;  // xyz: direction the light travels, w: type
	vec4 colorIntensity; // rgb: color, a: intensity
	vec4 spot;           // x: cos(inner angle), y: cos(outer angle), z: 1 to fade across the range
};

// A directional light of the map's GPARAM
struct mapDirectionalLight
{
	vec4 direction;  // xyz: the direction the light travels in
	vec4 color;      // rgb: color, a: intensity with the exposure
	vec4 specColor;  // the color and intensity of its highlights
};

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	// The sun, hemisphere and model lights are in the lighting sets, see SceneLighting.glsl
	ivec4 curserPosition;
	uint envmap;

	float ambientLightMult;
	float directLightMult;
	float indirectLightMult;
	float emissiveMapMult;
	float sceneBrightness;
	float simpleFlverBrightness;
	float simpleFlverSaturation;
	vec4 selectionColor;
	vec4 outlineColor;
	uint EnableDithering;
	uint EnableTinting;
	float ditherOpacity;

	uint lightCount;
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
	float windPeriod;
	uint materialFeatures;
	uint sceneDepthTexture; // index in globalTextures of the depth behind transparent draws, 0 if there is none

	vec4 wind;
	float collisionOpacity;
	float navVolumeSliceHeight;
	uint navVolumeSliceFlags;

	// The irradiance volumes to light with: entries of volumedata, see IrradianceVolumeTable
	uint irradianceVolumeStart;
	uint irradianceVolumeCount;
	float irradianceVolumeExposure; // scales the volumes' light to the viewport's
	uint irradianceVolumeFlags;     // 1: scale reflections by the volumes' light

	uint mapLightingFlags;          // 1: lit by the map's GPARAMs, 2: the hemisphere adds to the volumes' light,
	                                // 4: characters take the model set (SceneLighting.glsl)

	vec4 envMapScale;               // reflections' color scale, rgb
	float envMapSaturate;           // reflections' saturation, 0 grey, 1 unchanged

	// The map's lights near the camera: entries of maplightdata, see SceneLightTable
	uint mapLightStart;
	uint mapLightCount;
	// DS3's lightmaps: entries of lightmapdata, see LightmapTable (padding in the other shaders' copies)
	uint lightmapStart;
	uint lightingDebugView; // CFG.Viewport_Lighting_Debug_View, see SceneParam.LightingDebugView

	// The sun's cascaded shadow maps, see ShadowCascades.cs. Depth is reversed: 1 nearest the sun.
	mat4 shadowMatrices[4];  // world to each cascade's clip space
	vec4 shadowSplits;       // how far along the view each cascade ends
	vec4 shadowTexelSizes;   // meters per shadow map texel in each cascade
	vec4 shadowDepthScales;  // shadow map depth per meter towards the sun in each cascade
	uint shadowTexture;      // index in globalTextures of the map holding the cascades, 0 for no shadows
	uint shadowCascadeCount;
	float shadowFadeStart;   // where shadows start to fade out along the view
	float shadowFadeEnd;     // and where they are gone
	float shadowDepthBias;   // the GPARAM's Depth Bias
	float shadowSlopeBias;   // and Slope Scaled Depth Bias
	float shadowCascadeFade; // the share of each cascade's range that blends into the next
	float shadowStrength;    // how much of the sun a shadow takes away
	float shadowMapSize;     // texels across the whole map, two cascades to a side

	// The GPARAM Wet group in a weather that wets, see SceneParam.cs; the rates are 0 when it is dry
	vec4 wetAlbedo;          // rgb: the albedo wet surfaces lean towards, a: how far
	vec4 wetShininess;       // x: the shininess they lean towards, y: how far

	// The volumetric fog, for transparent draws, see VolumetricFogPass.cs and SceneParam.cs
	uint fogVolumeTexture;   // index in globalTexturesVolume, 0 for no fog this frame
	float fogNear;           // the volume's near depth
	float fogLogRatio;       // log(far / near)
	float fogSlices;
	vec4 fogRange;           // x: far depth, y: end depth past it (0 for none), zw: the target's size
	vec4 fogDensity;         // x: fixed, y: height scale, z: distribution, w: height offset
	vec4 fogExtinction;      // x: extra extinction from AbsorptionScale
	vec4 fogSky;             // rgb: sky light, w: height attenuation
	vec4 fogDirect;          // rgb: sun light towards the sun, w: height attenuation
	vec4 fogOpposite;        // rgb: sun light away from it, w: ScatteringG
	vec4 fogSun;             // xyz: the direction the sun's light travels

	// What the rain reaches, see RainOcclusion.cs: the scene's depth seen straight down round the camera
	mat4 rainMatrix;         // world to the map: xy -1..1 across, z the depth, 1 at the top
	uint rainTexture;        // index in globalTextures, 0 when it isn't read
	float rainMapSize;       // texels across
	float rainTexelSize;     // meters per texel
	float rainDepthScale;    // depth per meter of height
};

#endif
