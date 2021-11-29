#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float AspectRatio;
float4 Color;
bool Hover;
bool Pressed;
float HoverProgress;
float Time;

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
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 borderColor = Color;
    float4 hoverColor = Color / 2;
    float4 pressedColor = Color / 4;
    float4 transparent = float4(0, 0, 0, 0);
    
    float2 borderMask = float2(0, 0);
    
    borderMask.x = step(0.05, input.TextureCoordinates.x * AspectRatio) * step(input.TextureCoordinates.x * AspectRatio, AspectRatio - 0.05);
    borderMask.y = step(0.05, input.TextureCoordinates.y) * step(input.TextureCoordinates.y, 1 - 0.05);
	
    float4 border = lerp(borderColor, transparent, borderMask.x * borderMask.y);
    float4 background = lerp(hoverColor, transparent, step(HoverProgress, input.TextureCoordinates.x));
	
    return border + background + pressedColor * Pressed;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};