// The map's lights near the camera, as SceneLightTable holds them, and the cone of their spot lights. Shared by
// FlverShader.frag and VolumetricFog.comp, which each bind the table themselves. Include after SceneParams.glsl,
// which defines sceneLight.
#ifndef MAP_LIGHT_GLSL
#define MAP_LIGHT_GLSL

// A light of the map, laid out as MapLightData (std140, 96 bytes): with the color and intensity of its highlights,
// which BTL gives separately, and the shape of a spot light's cone
struct mapLight
{
	sceneLight light;
	vec4 specular;   // rgb: color of the highlights, a: their intensity
	vec4 shape;      // spot lights, xyz: the cone's horizontal axis, w: tan of its horizontal outer angle; 0: round
};

// How much of a spot light reaches a point, toPoint being the way from the light to it. A round cone fades between
// the outer and inner angles of light.spot. A cone wider one way than the other (BTL's HorizontalFov against its
// VerticalFov) is an elliptical cone: the point's offset across the axis, over its distance along it, is scaled by
// each way's outer tangent, so the edge is at 1 both ways, and it fades in from the inner angle's share of that.
float MapSpotCone(mapLight light, vec3 toPoint)
{
	vec3 axis = normalize(light.light.directionType.xyz);

	if (light.shape.w <= 0.0)
	{
		return smoothstep(light.light.spot.y, light.light.spot.x, dot(normalize(toPoint), axis));
	}

	float along = dot(toPoint, axis);
	if (along <= 0.0)
	{
		return 0.0;
	}

	float cosOuter = max(light.light.spot.y, 1e-3);
	float cosInner = max(light.light.spot.x, 1e-3);
	float tanVertical = max(sqrt(max(1.0 - cosOuter * cosOuter, 0.0)) / cosOuter, 1e-4);
	float tanInner = sqrt(max(1.0 - cosInner * cosInner, 0.0)) / cosInner;

	vec3 horizontal = light.shape.xyz;
	vec3 vertical = cross(axis, horizontal);

	vec2 across = vec2(dot(toPoint, horizontal) / light.shape.w, dot(toPoint, vertical) / tanVertical) / along;

	return 1.0 - smoothstep(min(tanInner / tanVertical, 0.999), 1.0, length(across));
}

#endif
