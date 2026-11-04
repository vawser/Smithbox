#version 450
#ifdef MATERIAL_DEFORM
#extension GL_EXT_nonuniform_qualifier : require
#endif

// Vertex stage of the flver shader.
//
// Macros (set by the variant list in FlverShader.json):
//   MATERIAL_BLEND  - the mesh has a second UV set, which is passed on for blended materials
//   MATERIAL_DEFORM - the mesh has the TEXCOORD1-3 attributes, for vertex animation and blend weights
//   SELECTED        - used for selected objects (shares this file; the fragment stage does the tinting)
//   FOCUS           - used for maps that are out of focus (the fragment stage does the dithering)

#ifdef MATERIAL_DEFORM
#define MAX_SCENE_LIGHTS 16

struct sceneLight
{
	vec4 positionRange;
	vec4 directionType;
	vec4 colorIntensity;
	vec4 spot;
};
#endif

struct sceneParams
{
	mat4 projection;
	mat4 view;
	vec4 eye;
	vec4 lightDirection;
#ifdef MATERIAL_DEFORM
	// Only here to reach materialTime, which comes after them
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
	vec4 sunColor;
	sceneLight lights[MAX_SCENE_LIGHTS];

	float materialTime;
	float windPeriod;
	uint materialFeatures;
	vec4 wind; // xy: world XZ direction, z: steady force, w: force of the waves
#endif
};

layout(set = 0, binding = 0) uniform SceneParamBuffer
{
    sceneParams sceneparam;
};

struct instanceData
{
	mat4 world;
	// 0: material id
	// 1: bone base
	// 3: entity id
	uvec4 materialID;
};

layout(set = 1, binding = 0, std140) buffer WorldBuffer
{
    readonly instanceData idata[];
};

layout(set = 7, binding = 0, std140) buffer BoneBuffer
{
    readonly mat4 bones[];
};

layout(set = 7, binding = 0, std430) buffer SkinBuffer
{
    readonly uvec4 skin[];
};
layout (constant_id = 50) const bool c_normalWBoneTransform = false;

#ifdef MATERIAL_DEFORM
#define DEFORM_VAT 1
#define DEFORM_SWAY 2
#define DEFORM_WIND_BEND 3
#define DEFORM_WIND_PIVOT 4
#define DEFORM_VAT_LOOP 5
#define DEFORM_SWING 6
#define DEFORM_BIRDS 7
#define DEFORM_DIRECTIONAL_WAVE 8

#define FEATURE_WIND 1u
#define FEATURE_VERTEX_ANIMATION 2u
#define FEATURE_SWAY 4u
#define FEATURE_BIRDS 8u
#define FEATURE_LAYER_BLENDING 16u
#define FEATURE_OVERLAYS 32u
#define FEATURE_MAGMA 64u
#define FEATURE_EMISSIVE 128u
#define FEATURE_UV_SCROLL 256u
#define FEATURE_UNLIT 512u
#define FEATURE_WATER 1024u
layout (constant_id = 60) const int c_deformer = 0;

layout(set = 2, binding = 0) uniform texture2D globalTextures[];

// Three entries after a material in the material buffer. Same size as the fragment stage's materialData
struct deformData
{
	vec4 params;    // x: amplitude, y: frames or cycles a second, zw: as each deformer needs
	vec4 direction; // xyz: direction of sway
	uvec2 textures; // x: vertex animation positions, y: vertex animation normals
	vec2 extra;     // as each deformer needs
};

layout(set = 4, binding = 0, std140) buffer materials
{
    readonly deformData deformdata[];
};

layout(set = 5, binding = 0) uniform sampler linearSampler;
#endif

layout(location = 0) in vec3 position;
layout(location = 1) in ivec2 uv;
layout(location = 2) in ivec4 normal;
layout(location = 3) in ivec4 binormal;
layout(location = 4) in ivec4 bitangent;
layout(location = 5) in uvec4 color;

#if defined(MATERIAL_BLEND) || defined(MATERIAL_DEFORM)
	layout(location = 6) in ivec2 uv2;
#endif
#ifdef MATERIAL_DEFORM
	layout(location = 7) in vec4 texcoord1;
	layout(location = 8) in vec4 texcoord2;
	layout(location = 9) in vec4 texcoord3;
#endif

layout(location = 0) out vec2 fsin_texcoord;
layout(location = 1) out vec3 fsin_worldpos;
layout(location = 2) out mat3 fsin_worldToTangent;
layout(location = 7) out vec4 fsin_color;
layout(location = 8) flat out uint fsin_mat;
layout(location = 9) flat out uint fsin_entityid;
#ifdef MATERIAL_BLEND
	layout(location = 10) out vec2 fsin_texcoord2;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	layout(location = 11) out vec3 fsin_vertexBlend;
#endif
// 1: the instance receives no shadows
layout(location = 12) flat out uint fsin_flags;
#ifdef MATERIAL_BLEND
	// DS3's lightmaps (the variants with the second UV set): the instance's entry in the lightmap table plus 1, 0
	// for none, and its rotation from the model's space to the world's, for the lightmap's light direction
	layout(location = 13) flat out uint fsin_lightmap;
	layout(location = 14) flat out mat3 fsin_modelToWorld;
#endif

// materialID.z of a static instance with a lightmap: InstanceData.LightmapFlag and the entry
#define SKIN_KIND_MASK 0xC0000000u
#define SKIN_KIND_LIGHTMAP 0x40000000u

// Whether materialID.z animates the instance, rather than naming its lightmap or being 0
bool IsAnimated()
{
	uint z = idata[gl_InstanceIndex].materialID.z;
	return z != 0u && (z & SKIN_KIND_MASK) != SKIN_KIND_LIGHTMAP;
}

// The top bit of the instance's material ID is a flag, MeshRenderableProxy.NoShadowMaterialFlag
#define MATERIAL_ID_MASK 0x7FFFFFFFu
#define MATERIAL_NO_SHADOW 0x80000000u

// UVs are stored as signed 16 bit fixed point with 11 fractional bits
const float UV_SCALE = 1.0 / 2048.0;

#ifdef MATERIAL_DEFORM
const float TWO_PI = 6.2831853;
const float WIND_FORCE_SCALE = 0.05;
#endif

// Stale or wrong instance data can ask for any bone, and reading past the end of the buffer loses the device
mat4 ReadBone(uint index)
{
	return index < uint(bones.length()) ? bones[index] : mat4(1.0);
}

// materialID.z is 0 unless the instance is being animated, which keeps static models untouched:
//   1            - rigid, the bone only needs to reach the normals as well
//   high bit set - weighted, the low bits plus gl_VertexIndex index the skin data
mat4 GetBoneMatrix()
{
	uint skinRef = idata[gl_InstanceIndex].materialID.z;
	uint boneBase = idata[gl_InstanceIndex].materialID.y;

	if ((skinRef & 0x80000000u) != 0u)
	{
		uint skinIndex = (skinRef + uint(gl_VertexIndex)) & 0x7FFFFFFFu;
		if (skinIndex >= uint(skin.length()))
		{
			return mat4(1.0);
		}

		uvec4 s = skin[skinIndex];
		vec4 weights = unpackUnorm4x8(s.z);

		return ReadBone(boneBase + (s.x & 0xFFFFu)) * weights.x +
			ReadBone(boneBase + (s.x >> 16)) * weights.y +
			ReadBone(boneBase + (s.y & 0xFFFFu)) * weights.z +
			ReadBone(boneBase + (s.y >> 16)) * weights.w;
	}

	if (c_normalWBoneTransform)
	{
		return ReadBone(boneBase + (uint(normal.w) & 0xFFu));
	}

	return mat4(1.0);
}

#ifdef MATERIAL_DEFORM
// Freed material memory can be reused for anything, and a texture past the pool loses the device.
// Same as the size of GlobalTexturePool in SceneRenderer
int TextureIndex(uint tex)
{
	return tex < 32768u ? int(tex) : 0;
}

// One frame of a vertex animation. Columns are vertices and rows are frames.
// The position works out where to read, but a texture can be the 1x1 default until it is uploaded
// or after it is unloaded, and reading outside of a texture here makes the GPU lose the device
vec3 FetchVat(uint tex, int column, int row)
{
	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], linearSampler), 0);
	ivec2 texel = clamp(ivec2(column, row), ivec2(0), size - 1);

	return texelFetch(sampler2D(globalTextures[nonuniformEXT(TextureIndex(tex))], linearSampler), texel, 0).xyz;
}

// Plays a baked animation. The vertex's column is stored as two bytes, as vertices that were split on
// export share one, along with how strongly it follows the animation and an offset to its start
void DeformVat(deformData deform, vec2 columnBytes, float strength, float startOffset, inout vec3 pos, inout vec3 nrm)
{
	uint vat = deform.textures.x;
	uint van = deform.textures.y;

	// Still loading
	if (vat == 0u || van == 0u)
	{
		return;
	}

	ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(TextureIndex(vat))], linearSampler), 0);
	float height = float(size.y);

	float start = fract(startOffset);
	float frame = fract((height * start + sceneparam.materialTime) * deform.params.y / height) * height;

	int column = min(int(columnBytes.x * 255.0 + columnBytes.y * 65280.0 + 0.5), size.x - 1);
	int row = int(floor(frame));
	int nextRow = min(row + 1, size.y - 1);
	float blend = fract(frame);

	vec3 offset = mix(FetchVat(vat, column, row), FetchVat(vat, column, nextRow), blend);
	pos += offset * deform.params.x * strength;

	nrm = mix(FetchVat(van, column, row), FetchVat(van, column, nextRow), blend) * 2.0 - 1.0;
}

// Moves along a direction by a smoothed triangle wave. TEXCOORD1.x offsets its
// start and TEXCOORD1.z is how strongly the vertex follows it
void DeformSway(deformData deform, inout vec3 pos)
{
	float t = abs(fract(deform.params.y * (sceneparam.materialTime + texcoord1.x * 2.0)) * 2.0 - 1.0);
	float wave = t * t * (3.0 - 2.0 * t) * 2.0 - 1.0;

	pos += deform.direction.xyz * deform.params.x * texcoord1.z * wave;
}

// The game sums up to 8 wind sources it has at runtime. This is its sum for one source that
// blows everywhere: a wave travelling along the wind on top of a steady force
vec3 WindForce(vec3 worldPos, float timeOffset)
{
	vec2 direction = sceneparam.wind.xy;
	float time = sceneparam.materialTime + timeOffset;
	float phase = dot(worldPos.xz, direction);
	float wave = sin(TWO_PI / sceneparam.windPeriod * (time - phase));

	// Gusts aren't the game's, which gets them from its wind sources. These are two slow waves, so they never quite repeat
	float gust = 0.75 + 0.25 * sin(TWO_PI * 0.07 * time - phase * 0.05) * sin(TWO_PI * 0.19 * time - phase * 0.11);

	float force = (sceneparam.wind.z + sceneparam.wind.w * wave) * gust * WIND_FORCE_SCALE;

	return vec3(direction.x, 0.0, direction.y) * force;
}

// Bends over along the wind by TEXCOORD1.y, keeping the height of the vertex on a sphere
void DeformWindBend(inout vec3 worldPos)
{
	float strength = texcoord1.y;
	vec2 bend = WindForce(worldPos, 0.0).xz * strength;
	bend /= sqrt(dot(bend, bend) + 1.0);

	float height = sqrt(clamp(1.0 - dot(bend, bend), 0.0, 1.0)) - 1.0;

	worldPos += vec3(bend.x, height, bend.y) * strength;
}

// Rotates around the pivot in TEXCOORD3, tipping the instance's up axis towards the wind
void DeformWindPivot(deformData deform, mat4 toWorld, vec3 up, inout vec3 worldPos)
{
	vec3 force = WindForce(worldPos, deform.params.y * texcoord1.x);
	float magnitude = length(force);

	if (magnitude < 1e-5)
	{
		return;
	}

	vec3 axis = cross(normalize(up), force / magnitude);

	if (dot(axis, axis) < 1e-8)
	{
		return;
	}

	axis = normalize(axis);

	float angle = log(magnitude + 1.0) * deform.params.x;
	vec3 pivot = (toWorld * vec4(texcoord3.xyz, 1.0)).xyz;
	vec3 onAxis = pivot + axis * dot(axis, worldPos - pivot);
	vec3 fromAxis = worldPos - onAxis;

	vec3 rotated = onAxis + fromAxis * cos(angle) + cross(axis, fromAxis) * sin(angle);

	worldPos += (rotated - worldPos) * (1.0 - texcoord2.z);
}

// What is switched on in the viewport, see MaterialFeatures
bool HasFeature(uint feature)
{
	return (sceneparam.materialFeatures & feature) != 0u;
}

float SmoothTriangle(float time)
{
	float t = clamp(abs(fract(time) * 2.0 - 1.0), 0.0, 1.0);

	return t * t * (3.0 - 2.0 * t);
}

// Birds flap by a vertex animation, roll around their body and fly around an ellipse. Each bird in the
// mesh has its body's centre in TEXCOORD3 and its own speed in TEXCOORD2.x. The game's order of operations.
//   params: x radius, y frames a second, z speed, w offset
//   direction: xyz size of the ellipse in each axis, w roll
//   extra: x how much a bird's own speed counts (0 is off), y cycles a second of the bob (0 is off)
void DeformBirds(deformData deform, inout vec3 pos)
{
	float frameTime = sceneparam.materialTime;
	uint vat = deform.textures.x;

	if (vat != 0u)
	{
		ivec2 size = textureSize(sampler2D(globalTextures[nonuniformEXT(TextureIndex(vat))], linearSampler), 0);
		float height = float(size.y);
		float frame = fract((height * fract(texcoord2.x) + frameTime) * deform.params.y / height) * height;
		int column = min(int(texcoord1.x * 255.0 + texcoord1.y * 65280.0 + 0.5), size.x - 1);
		int row = int(floor(frame));

		pos += mix(FetchVat(vat, column, row), FetchVat(vat, column, min(row + 1, size.y - 1)), fract(frame));
	}

	float time = frameTime * deform.params.z;
	float angle = -time - deform.params.w;

	if (deform.extra.x != 0.0)
	{
		angle *= (fract(texcoord2.x) + 1.0) * deform.extra.x;
	}

	float radius = deform.params.x;
	float sinAngle = sin(angle);
	float cosAngle = cos(angle);

	float bobWave = SmoothTriangle(time * deform.extra.y);
	float bobHeight = deform.direction.y * (deform.extra.y != 0.0 ? bobWave : 1.0);
	vec3 orbit = vec3(deform.direction.x * cosAngle, bobHeight * sinAngle, sinAngle * deform.direction.z) * radius;
	float lift = deform.extra.y != 0.0 ? (bobWave - 0.5) * 2.0 : 0.0;

	if (dot(orbit, orbit) < 1e-8)
	{
		pos += orbit;
		return;
	}

	// Roll around the direction of travel, through the body
	vec2 heading = orbit.xz * inversesqrt(dot(orbit, orbit));
	vec2 axis = normalize(vec2(-heading.y, heading.x));

	vec3 fromBody = pos - texcoord3.xyz;
	vec2 onAxis = texcoord3.xz + axis * dot(axis, fromBody.xz);
	vec2 aside = pos.xz - onAxis;

	float roll = cosAngle * deform.direction.w;
	float sinRoll = sin(roll);
	float cosRoll = cos(roll);
	float sideways = aside.x * axis.y - aside.y * axis.x;

	vec3 rolled = vec3(
		onAxis.x + cosRoll * aside.x - sinRoll * axis.y * fromBody.y,
		texcoord3.y + cosRoll * fromBody.y + sinRoll * sideways,
		onAxis.y + cosRoll * aside.y + sinRoll * axis.x * fromBody.y);

	// Then turn to face along the path
	vec3 lifted = rolled - lift;
	float sinTurn = sin(-angle);
	float cosTurn = cos(-angle);

	pos = vec3(
		orbit.x + lift + cosTurn * lifted.x + sinTurn * lifted.z,
		orbit.y + lift + lifted.y,
		orbit.z + lift + cosTurn * lifted.z - sinTurn * lifted.x);
}

// Leans around the pivot in TEXCOORD3 and moves by a wave, with a phase from a noise texture at the pivot.
// TEXCOORD1.z is how far the vertex stays where it is, and TEXCOORD1.x how far it follows the wave.
//   params: x lean, y cycles a second, z how much the noise moves the phase, w world size of the noise
//   direction: xyz axis of the lean, w vertical wave
//   extra: horizontal wave
void DeformDirectionalWave(deformData deform, mat4 toWorld, inout vec3 worldPos)
{
	vec3 pivot = (toWorld * vec4(texcoord3.xyz, 1.0)).xyz;
	float noise = 0.0;

	if (deform.textures.x != 0u && deform.params.w != 0.0)
	{
		noise = textureLod(sampler2D(globalTextures[nonuniformEXT(TextureIndex(deform.textures.x))], linearSampler), pivot.xz / deform.params.w, 0.0).r;
	}

	float wave = SmoothTriangle((deform.params.z * noise + sceneparam.materialTime) * deform.params.y);
	float free = 1.0 - texcoord1.z;

	if (dot(deform.direction.xyz, deform.direction.xyz) > 1e-8)
	{
		vec3 axis = normalize(deform.direction.xyz);
		vec3 toVertex = worldPos - pivot;
		vec3 along = axis * dot(axis, toVertex);
		float lean = deform.params.x;

		vec3 leaned = pivot + along + (toVertex - along) * cos(lean) + cross(axis, toVertex) * sin(lean);
		worldPos += (leaned - worldPos) * free;
	}

	worldPos += vec3(deform.extra.x * texcoord1.x, deform.direction.w, deform.extra.y * texcoord1.x) * free * wave;
}

// Swings around the pivot in TEXCOORD3 by a smoothed triangle wave. TEXCOORD2.x offsets
// its start, and TEXCOORD1.z is how far the vertex stays where it is
void DeformSwing(deformData deform, mat4 toWorld, inout vec3 worldPos)
{
	float time = (deform.direction.w * texcoord2.x + sceneparam.materialTime) * deform.params.y;
	float t = abs(fract(time) * 2.0 - 1.0);
	float angle = (t * t * (3.0 - 2.0 * t) * 2.0 - 1.0) * deform.params.x;

	vec3 axis = mat3(toWorld) * deform.direction.xyz;

	if (dot(axis, axis) < 1e-8)
	{
		return;
	}

	axis = normalize(axis);

	vec3 pivot = (toWorld * vec4(texcoord3.xyz, 1.0)).xyz;
	vec3 toVertex = worldPos - pivot;

	vec3 rotated = pivot + axis * dot(axis, toVertex) * (1.0 - cos(angle)) + toVertex * cos(angle) + cross(axis, toVertex) * sin(angle);

	worldPos += (rotated - worldPos) * (1.0 - texcoord1.z);
}
#endif

void main()
{
	mat4 w = idata[gl_InstanceIndex].world;

	vec3 localPosition = position;
	vec3 localNormal = vec3(normal);

#ifdef MATERIAL_DEFORM
	// Stale instance data can hold any material, and reading past the material buffer loses the device
	uint deformIndex = (idata[gl_InstanceIndex].materialID.x & MATERIAL_ID_MASK) + 3u;
	deformData deform = deformIndex < uint(deformdata.length())
		? deformdata[deformIndex]
		: deformData(vec4(0.0), vec4(0.0), uvec2(0u), vec2(0.0));

	if (c_deformer == DEFORM_VAT && HasFeature(FEATURE_VERTEX_ANIMATION))
	{
		DeformVat(deform, texcoord2.xy, texcoord1.z, texcoord3.x, localPosition, localNormal);
	}
	else if (c_deformer == DEFORM_VAT_LOOP && HasFeature(FEATURE_VERTEX_ANIMATION))
	{
		DeformVat(deform, texcoord1.xy, 1.0, texcoord2.x, localPosition, localNormal);
	}
	else if (c_deformer == DEFORM_BIRDS && HasFeature(FEATURE_BIRDS))
	{
		DeformBirds(deform, localPosition);
	}
	else if (c_deformer == DEFORM_SWAY && HasFeature(FEATURE_SWAY))
	{
		DeformSway(deform, localPosition);
	}
#endif

	fsin_texcoord = vec2(uv) * UV_SCALE;
#ifdef MATERIAL_BLEND
	fsin_texcoord2 = vec2(uv2) * UV_SCALE;
#endif
#if defined(MATERIAL_BLEND) && defined(MATERIAL_DEFORM)
	fsin_vertexBlend = texcoord1.xyz;
#endif

	// Vertex colors are used as blend weights by blended materials
	fsin_color = vec4(color) / 255.0;

	fsin_mat = idata[gl_InstanceIndex].materialID.x & MATERIAL_ID_MASK;
	fsin_flags = (idata[gl_InstanceIndex].materialID.x & MATERIAL_NO_SHADOW) != 0u ? 1u : 0u;
	fsin_entityid = idata[gl_InstanceIndex].materialID.w;

#ifdef MATERIAL_BLEND
	uint skinKind = idata[gl_InstanceIndex].materialID.z;
	fsin_lightmap = (skinKind & SKIN_KIND_MASK) == SKIN_KIND_LIGHTMAP ? (skinKind & ~SKIN_KIND_MASK) + 1u : 0u;
	fsin_modelToWorld = mat3(w);
#endif

	mat4 bone = GetBoneMatrix();
	vec4 worldPos = w * bone * vec4(localPosition, 1.0);

#ifdef MATERIAL_DEFORM
	// Wind blows in world space, unlike the deformers above
	if (c_deformer == DEFORM_WIND_BEND && HasFeature(FEATURE_WIND))
	{
		DeformWindBend(worldPos.xyz);
	}
	else if (c_deformer == DEFORM_WIND_PIVOT && HasFeature(FEATURE_WIND))
	{
		DeformWindPivot(deform, w * bone, w[1].xyz, worldPos.xyz);
	}
	else if (c_deformer == DEFORM_SWING && HasFeature(FEATURE_SWAY))
	{
		DeformSwing(deform, w * bone, worldPos.xyz);
	}
	else if (c_deformer == DEFORM_DIRECTIONAL_WAVE && HasFeature(FEATURE_WIND))
	{
		DeformDirectionalWave(deform, w * bone, worldPos.xyz);
	}
#endif

	fsin_worldpos = worldPos.xyz;

	// Normals need the inverse transpose of the world matrix to stay correct under non-uniform scale.
	// Tangents are directions along the surface, so they use the matrix itself.
	mat3 wm = mat3(IsAnimated() ? w * bone : w);
	mat3 normalMatrix = wm;
	if (abs(determinant(wm)) > 1e-12)
	{
		normalMatrix = transpose(inverse(wm));
	}

	vec3 N = normalize(normalMatrix * localNormal);
	vec3 T = wm * vec3(bitangent);
	vec3 B = wm * vec3(binormal);

	// Meshes without tangents have them zeroed. Build any valid frame rather than letting
	// normalizing a zero vector poison the normal.
	if (dot(T, T) < 1e-8)
	{
		T = cross(abs(N.y) < 0.99 ? vec3(0.0, 1.0, 0.0) : vec3(1.0, 0.0, 0.0), N);
		B = vec3(0.0);
	}

	// Make the frame orthonormal, keeping the handedness the mesh was authored with
	T = normalize(T - N * dot(N, T));
	float handedness = dot(cross(N, T), B) < 0.0 ? -1.0 : 1.0;
	B = cross(N, T) * handedness;

	fsin_worldToTangent = mat3(T, B, N);

	gl_Position = sceneparam.projection * sceneparam.view * worldPos;

#ifdef OUTLINE
	// Pushed out along the normal in screen space, so it keeps its width at any distance
	vec3 screenNormal = mat3(sceneparam.projection) * mat3(sceneparam.view) * N;
	gl_Position += vec4(screenNormal, 0.0) * gl_Position.w * 0.005;
#endif
}
