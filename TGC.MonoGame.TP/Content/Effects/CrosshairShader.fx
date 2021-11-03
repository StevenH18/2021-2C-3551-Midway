#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture CrosshairTexture;
sampler2D CrosshairSampler = sampler_state
{
    Texture = (CrosshairTexture);
    ADDRESSU = clamp;
    ADDRESSV = clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 crosshair = tex2D(CrosshairSampler, input.TextureCoordinates);	
    return crosshair;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};