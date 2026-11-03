#version 450

// A triangle that covers the target, with no vertex buffer: Bloom.frag works out where it reads from the pixel

void main()
{
	vec2 position = vec2((gl_VertexIndex << 1) & 2, gl_VertexIndex & 2);
	gl_Position = vec4(position * 2.0 - 1.0, 0.0, 1.0);
}
