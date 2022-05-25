#version 450 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aNormal;

out vec3 vertexColor;
out vec3 Normal;
out vec3 FragPos;
out vec3 lightPos;
out vec2 TexCoord;
out float height;
out vec2 TextureGradRange;

uniform mat4 transform;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec2 textureGradRange;

uniform mat4 model_cramble;

uniform vec3 LightPos = vec3(0, 0, 100);


void main()
{
	vec4 transformed = transform * vec4(aPos, 1.0);
	gl_Position = transformed * model * view * projection;
	FragPos = vec3(model * transformed);	

	//Normal = vec3(transform[0][0], transform[1][1], transform[2][2]) * aNormal mat3(model_cramble);
	Normal = aNormal;

	vertexColor = aColor;
	
	//vec2 pixelPlacement = transformed.xy;
	//LightPos = vec3(pixelPlacement, 10);
	//lightPos = LightPos + vec3(pixelPlacement + vec2(aPos.x, aPos.y), 0);
	lightPos = LightPos;

	TexCoord = aTexCoord;

	//height = transformed.z / transformed.length;
	height = aPos.z / aPos.length + transform[3][2];

	TextureGradRange = textureGradRange;
}