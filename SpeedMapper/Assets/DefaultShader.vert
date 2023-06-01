#version 430

#extension GL_KHR_vulkan_glsl: enable

layout (location = 0) in vec3 Position;
layout (location = 1) in vec4 Color;
layout (location = 2) in vec2 TexCoords;

layout (location = 3) in vec4 InstanceColumn1;
layout (location = 4) in vec4 InstanceColumn2;
layout (location = 5) in vec4 InstanceColumn3;
layout (location = 6) in vec4 InstanceColumn4;

layout (location = 0) out vec4 outColor;
layout (location = 1) out vec2 outTexPos;

layout (set = 0, binding = 0) uniform ViewBuffer        { mat4 View; };
layout (set = 0, binding = 1) uniform PerspectiveBuffer { mat4 Perspective; };

layout (set = 1, binding = 0) uniform SolidBuffer       { mat4 Solid; };

void main() {
	mat4 Instance = mat4(InstanceColumn1, InstanceColumn2, InstanceColumn3, InstanceColumn4);

	mat4 matrix = Perspective * View * Solid * Instance;

	gl_Position = matrix * vec4(Position, 1);
	outColor = Color;
	outTexPos = TexCoords;
}