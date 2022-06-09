#version 450 core
out vec4 FragColor;
  
in vec3 vertexColor;
in vec3 Normal;
in vec3 FragPos; 
in vec3 lightPos;
in vec2 TexCoord;


in float height;
in vec2 TextureGradRange;
const int amountOfTextures = 3;

uniform sampler2D textures[amountOfTextures];

uniform vec3 lightColor = vec3(1);

uniform vec3 viewPos;

uniform float lightPosHeight;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
    float opacity;
}; 

uniform Material material;

float textureGradCalc(vec2 range, float value){
    if(range.x == 0 && range.y == 0)
    {
       return 0.5;
    }

    float valueN = value * 0.2 + 0.2;
    
    return valueN;
}

vec4 textureMixCalc(float grad){
    
    vec4 textureColor = vec4(0);

    vec4 oTexture12 = mix(texture(textures[0], TexCoord), texture(textures[1], TexCoord), grad);
    vec4 oTexture23 = mix(texture(textures[1], TexCoord), texture(textures[2], TexCoord), grad);
    vec4 oTexture13 = mix(texture(textures[0], TexCoord), texture(textures[2], TexCoord), grad);

    vec4 oTexture1 = mix(oTexture12, oTexture13, grad);
    vec4 oTexture2 = mix(oTexture12, oTexture23, grad);
    vec4 oTexture = mix(oTexture1, oTexture2, grad);

    textureColor = oTexture;

    return textureColor;
}

void main()
{
    vec3 ambient = material.ambient * lightColor;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = material.specular * spec * lightColor; 

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor * material.diffuse;

    vec4 oTexture = textureMixCalc(textureGradCalc(TextureGradRange, height));

    vec3 colorBrightness = (ambient + diffuse) * vertexColor;

    FragColor = oTexture * vec4(colorBrightness, material.opacity);
}
