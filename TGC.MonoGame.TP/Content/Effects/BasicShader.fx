#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define PI 3.1415926538

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseTransposeWorld;

float3 AmbientColor; // Light's Ambient Color
float3 DiffuseColor; // Light's Diffuse Color
float3 SpecularColor; // Light's Specular Color
float KAmbient;
float KDiffuse;
float KSpecular;
float Shininess;
bool TexturedNormals;

float3 LightPosition;

float3 EyePosition; // Camera position

float Time = 0;

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

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float4 Normal : TEXCOORD3;
    float4 ScreenPosition : TEXCOORD4;
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
    output.WorldPosition = mul(input.Position, World);
    output.ScreenPosition = output.Position;
    output.Normal = mul(input.Normal, InverseTransposeWorld);

    return output;
}

float3 GetNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(NormalSampler, textureCoordinates).rgb * 2.0 - 1.0;

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

//Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
	// Get the texture texel
    float4 texelColor = tex2D(AlbedoSampler, input.TextureCoordinates);
    
    // Base vectors
    float3 lightDirection = normalize(LightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(EyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);

    float3 normal = GetNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, input.Normal.xyz);
    
    if (!TexturedNormals)
        normal = input.Normal.xyz;
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KDiffuse * DiffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(normal, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * SpecularColor * pow(saturate(NdotH), Shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(AmbientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, 1.0);
    
    //return float4(diffuseLight * texelColor.rgb, 1);
    //return float4(normal, 1);
    
    return finalColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

float4 HeightMapPS(VertexShaderOutput input) : COLOR
{
    float height = input.WorldPosition.y;
    float cameraDepth = input.ScreenPosition.w;
    return float4(height, cameraDepth, 0, 1);
}

technique HeightMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL HeightMapPS();
    }
};
