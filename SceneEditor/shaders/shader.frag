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
//uniform sampler2D texture1;
//uniform sampler2D texture2;
//uniform sampler2D texture3;

uniform vec3 lightColor = vec3(1);

uniform vec3 viewPos;

uniform float lightPosHeight;

//struct Material {
//    vec3 ambient;
//    vec3 diffuse;
//    vec3 specular;
//    float shininess;
//    float opacity;
//}; 
//
//uniform Material material;

float textureGradCalc(vec2 range, float value){
    
    float sizeRange = 1.0 / amountOfTextures;
    float[amountOfTextures] weights;

    if(range.x == 0 && range.y == 0)
    {
       return 0.5;
    }

    //float valueN = (value - range.x) / (range.y - range.x) * (0.95 - 0.8) + 0.2;
    float valueN = value * 0.2 + 0.2;
//    int tNum = int(valueN / sizeRange);
//    float innerRange = (valueN - tNum * sizeRange) / sizeRange;
//
//    weights[tNum] = innerRange;
//    for(int i = 0; i < amountOfTextures; i++)
//    {
//        if(i != tNum)
//        {
//            weights[i] = (1 - innerRange) / abs(tNum - i);
//        }
//    }
//    
    
    return valueN;
}

vec4 textureMixCalc(float grad){
    
    vec4 textureColor = vec4(0);



//    for(int i = 0; i < amountOfTextures; i++)
//    {
//        vec4 influence = 
//        textureColor += texture(textures[i], TexCoord) * grad;
//    }

    vec4 oTexture12 = mix(texture(textures[0], TexCoord), texture(textures[1], TexCoord), grad);
    vec4 oTexture23 = mix(texture(textures[1], TexCoord), texture(textures[2], TexCoord), grad);
    vec4 oTexture13 = mix(texture(textures[0], TexCoord), texture(textures[2], TexCoord), grad);

    vec4 oTexture1 = mix(oTexture12, oTexture13, grad);
    vec4 oTexture2 = mix(oTexture12, oTexture23, grad);
    vec4 oTexture = mix(oTexture1, oTexture2, grad);

    textureColor = oTexture;



    //textureColor = mix(mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.5),mix(texture(texture2, TexCoord), texture(texture3, TexCoord), 0.5), 0.5);

    return textureColor;
}

void main()
{
    float ambientStrength = 0.6;
    vec3 ambient = ambientStrength * lightColor;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

    float specularStrength = 0.1;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 1);
    vec3 specular = specularStrength * spec * lightColor; 

    float diffStrength = 0.1;
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor * diffStrength;

    //float textureGradient = 0.5;
    vec4 oTexture = textureMixCalc(textureGradCalc(TextureGradRange, height));

    vec3 colorBrightness = (ambient + diffuse) * vertexColor;

    FragColor = oTexture * vec4(colorBrightness, 1);
}
