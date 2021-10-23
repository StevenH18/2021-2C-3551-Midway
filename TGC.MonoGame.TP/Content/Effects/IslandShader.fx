#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define PI 3.1415926538

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseTransposeMatrix;

// Illumination parameters
float3 LightPositions[5];
float3 LightColors[5];

float3 EyePosition; // Camera position

//Textura para Albedo
texture AlbedoTexture;
sampler2D AlbedoSampler = sampler_state
{
    Texture = (AlbedoTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Normals
texture NormalTexture;
sampler2D NormalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Metallic
texture MetallicTexture;
sampler2D MetallicSampler = sampler_state
{
    Texture = (MetallicTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Roughness
texture RoughnessTexture;
sampler2D RoughnessSampler = sampler_state
{
    Texture = (RoughnessTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Ambient Occlusion
texture AoTexture;
sampler2D AoSampler = sampler_state
{
    Texture = (AoTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura del skybox o del ambiente
texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
    Texture = (EnvironmentMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float4 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	// Clear the output
    VertexShaderOutput output = (VertexShaderOutput) 0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldPosition = worldPosition;
    output.Normal = input.Normal;

	return output;
}

float3 GetNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(NormalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

float DistributionGGX(float3 normal, float3 halfVector, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(normal, halfVector), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 normal, float3 view, float3 light, float roughness)
{
    float NdotV = max(dot(normal, view), 0.0);
    float NdotL = max(dot(normal, light), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

//Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 albedo = pow(tex2D(AlbedoSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
    float metallic = tex2D(MetallicSampler, input.TextureCoordinates).r;
    float roughness = tex2D(RoughnessSampler, input.TextureCoordinates).r;
    float ao = tex2D(AoSampler, input.TextureCoordinates).r;

    float3 worldNormal = input.Normal;
    float3 normal = GetNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, worldNormal);
    float3 view = normalize(EyePosition - input.WorldPosition.xyz);

    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, albedo, metallic);
	
	// Reflectance equation
    float3 Lo = float3(0.0, 0.0, 0.0);
	
    for (int index = 0; index < 4; index++)
    {
        float3 light = LightPositions[index] - input.WorldPosition.xyz;
        float distance = length(light);
		// Normalize our light vector after using its length
        light = normalize(light);
        float3 halfVector = normalize(view + light);
        float attenuation = 1.0 / (distance);
        float3 radiance = LightColors[index] * attenuation;


		// Cook-Torrance BRDF
        float NDF = DistributionGGX(normal, halfVector, roughness);
        float G = GeometrySmith(normal, view, light, roughness);
        float3 F = fresnelSchlick(max(dot(halfVector, view), 0.0), F0);

        float3 nominator = NDF * G * F;
        float denominator = 4.0 * max(dot(normal, view), 0.0) + 0.001;
        float3 specular = nominator / denominator;

        float3 kS = F;
        
        float3 kD = float3(1.0, 1.0, 1.0) - kS;
        
        kD *= 1.0 - metallic;

		// Scale light by NdotL
        float NdotL = max(dot(normal, light), 0.0);

        //TODO
        Lo += (kD * NdotL * albedo / PI + specular) * radiance;
    }
    
    float3 ambient = float3(0.03, 0.03, 0.03) * albedo * ao;

    float3 color = ambient + Lo;

	// HDR tonemapping
    color = color / (color + float3(1, 1, 1));
    
    float exponent = 1.0 / 2.2;
	// Gamma correct
    color = pow(color, float3(exponent, exponent, exponent));
    
    // cambiar 'albedo' por 'color' o 'Lo' hace que no se renderice la lluvia    
    return float4(albedo, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};