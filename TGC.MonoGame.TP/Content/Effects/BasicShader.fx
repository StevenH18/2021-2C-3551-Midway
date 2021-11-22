#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;

// Texture parameters
texture DiffuseMap;
sampler2D DiffuseSampler = sampler_state
{
    Texture = (DiffuseMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 UV : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float4 Normal : TEXCOORD3;
    float4 ScreenPosition : TEXCOORD4;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);	
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = worldPosition;
    output.ScreenPosition = output.Position;
    output.UV = input.UV;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texelColor = tex2D(DiffuseSampler, input.UV);
    
    float4 color = texelColor + float4(DiffuseColor, 1) * step(texelColor.x, 0);
    
    return color;
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
    float height = (input.WorldPosition.y) / 2000;
    float cameraDepth = pow(1 - saturate(input.ScreenPosition.w / 20000), 2);
    return float4(height, cameraDepth, height, 1);
}

technique HeightMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL HeightMapPS();
    }
};
