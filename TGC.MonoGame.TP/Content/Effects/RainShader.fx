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
float Time = 0;

float Speed = 1000;
float HeightStart = 5000;
float HeightEnd = -500;


struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 MyPosition : TEXCOORD0;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
	
    input.Position.y += lerp(HeightStart, HeightEnd, frac(Time / (HeightStart - HeightEnd) * Speed));
	
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
	
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);		
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.MyPosition = worldPosition.xyz;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = float4(0.6, 0.9, 1, 1);
	
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
