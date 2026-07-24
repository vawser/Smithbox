#version 450

layout(set = 0, binding = 0) uniform QuadParams
{
    vec4 Color;
} quadParams;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = quadParams.Color;
}