#version 450 core
out vec4 FragColor;
  
in vec3 vertexColor;
in vec3 Normal;
in vec3 FragPos; 
in vec3 LightPos;
in vec2 TexCoord;
in float height;

uniform sampler2D texture1;
uniform sampler2D texture2;

uniform vec3 lightColor = vec3(1);

uniform vec3 viewPos;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
    float opacity;
}; 

uniform Material material;

void main()
{
    float gradient = height * 0.05 + 0.7;
    float textureGradient = height * 0.2 + 0.2;
    if(height < 0.3){
        gradient = 0.3;
    }
    else if (height > 0.95){
        gradient = 0.95;
    }

    //float textureGradient = 0.5;
    //FragColor = oTexture * gradient * vec4(vertexColor, 1);

    float ambientStrength = 0.5;
    vec3 ambient = ambientStrength * lightColor;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(LightPos - FragPos);

    float specularStrength = 0.7;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor; 

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    vec4 oTexture = mix(texture(texture1, TexCoord), texture(texture2, TexCoord), textureGradient);
    vec4 Textured = oTexture * vec4(vertexColor, 1);

    vec3 colorBrightness = (ambient + diffuse + specular) * gradient;

    FragColor = Textured * vec4(colorBrightness, 1);
}
