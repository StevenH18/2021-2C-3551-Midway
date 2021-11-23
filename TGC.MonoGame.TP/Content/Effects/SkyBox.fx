#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float SkyProgress;

texture SkyBoxTextureRain;
texture SkyBoxTextureStorm;
samplerCUBE SkyBoxSamplerRain = sampler_state
{
    texture = <SkyBoxTextureRain>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};
samplerCUBE SkyBoxSambplerStorm = sampler_state
{
    texture = <SkyBoxTextureStorm>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float3 TextureCoordinate : TEXCOORD2;
    float4 ScreenPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = mul(viewPosition, Projection);
    output.ScreenPosition = output.Position;
    
    output.Normal = input.Normal;

    float4 VertexPosition = mul(input.Position, World);
    output.TextureCoordinate = VertexPosition.xyz - CameraPosition;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 skyRain = float4(texCUBE(SkyBoxSamplerRain, normalize(input.TextureCoordinate)).rgb, 1);
    float4 skyStorm = float4(texCUBE(SkyBoxSambplerStorm, normalize(input.TextureCoordinate)).rgb, 1);
    
    return skyStorm;
}

technique Skybox
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}

float4 HeightMapPS(VertexShaderOutput input) : COLOR
{
    float height = input.TextureCoordinate.y + 1;
    float cameraDepth = input.ScreenPosition.w;
    return float4(height, cameraDepth, 0, 1);
}

technique HeightMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL HeightMapPS();
    }
};
