#version 430

layout (location = 0) in vec4 outColor;
layout (location = 1) in vec2 outTexPos;

layout (set = 2, binding = 0) uniform texture2D Texture;
layout (set = 2, binding = 1) uniform sampler TextureSampler;

layout (location = 0) out vec4 FragColor;

void main() {

	vec4 imageColor = texture(sampler2D(Texture, TextureSampler), outTexPos);
	vec4 color;

	float colorMult = 0.2;

	color = imageColor * (1 - colorMult) + outColor * colorMult;
	color.w = imageColor.w;

	FragColor = color;
}