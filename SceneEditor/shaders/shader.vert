#version 450 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aNormal;

out vec3 vertexColor;
out vec2 TexCoord;
out float height;
out vec3 Normal;

uniform mat4 transform;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = transform * vec4(aPos, 1.0) * model * view * projection;
	vertexColor = aColor;
	TexCoord = vec2(aTexCoord.x, aTexCoord.y);
	height = aPos.z / aPos.length + transform[3][2];
	Normal = aNormal;
}