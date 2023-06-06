#version 430

layout (location = 0) in vec4 outColor;
layout (location = 1) in vec2 outTexPos;

layout (set = 2, binding = 0) uniform texture2D Texture;
layout (set = 2, binding = 1) uniform sampler TextureSampler;

layout (location = 0) out vec4 FragColor;

void main() {
	float colorMult = 0.3;

	vec2 texPos = outTexPos;
	texPos.x = -texPos.x;

	vec4 imageColor = texture(sampler2D(Texture, TextureSampler), texPos);
	vec4 color = imageColor * (1 - colorMult) + outColor * colorMult;

	if (color.w < 0.04) discard;

	FragColor = color;
}
