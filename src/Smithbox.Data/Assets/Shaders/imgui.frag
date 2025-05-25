#version 450
#extension GL_EXT_nonuniform_qualifier : require

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 0, binding = 1) uniform sampler FontSampler;
layout(set = 1, binding = 0) uniform texture2D[] globalTextures;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;
layout (location = 2) flat in uint tex;
layout (location = 0) out vec4 outputColor;

void main()
{
    outputColor = color * texture(sampler2D(globalTextures[nonuniformEXT(tex)], FontSampler), texCoord);
}
