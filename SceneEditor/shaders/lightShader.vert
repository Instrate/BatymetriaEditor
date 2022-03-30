#version 450 core


layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
//layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aNormal;

out vec3 vertexColor;
out vec3 Normal;
out vec3 FragPos;
out vec3 LightPos;
//out vec2 TexCoord;
//out float height;

uniform mat4 transform;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 lightPos;

uniform mat4 model_cramble;



void main()
{
	//gl_Position = vec4(aPos, 1.0) * model * view * projection;
	
	//gl_Position = projection * view * model * vec4(aPos, 1.0);
	vec4 transformed = transform * vec4(aPos, 1.0);
	gl_Position = transformed * model * view * projection;
	FragPos = vec3(model * transformed);
	Normal = mat3(model_cramble) * vec3(transform[0][0], transform[1][1], transform[2][2]) * aNormal;
	vertexColor = aColor;
	LightPos = lightPos;
	//TexCoord = vec2(aTexCoord.x, aTexCoord.y);
	//height = aPos.z / aPos.length + transform[3][2];
}