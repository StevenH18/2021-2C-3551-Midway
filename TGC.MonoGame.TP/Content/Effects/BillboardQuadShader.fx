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
float Time = 0;

texture SpriteSheet;
sampler2D SpriteSheetSampler = sampler_state
{
    Texture = (SpriteSheet);
    ADDRESSU = clamp;
    ADDRESSV = clamp;
};

float2 SpritePixelSize;
float2 SpriteSheetSize;
float2 SpriteOffset;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 ScreenPosition : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float3 cameraRight = float3(View[0][0], View[1][0], View[2][0]);
    float3 cameraUp = float3(0, 1, 0);
    float3 vertice = input.Position.xyz;
    float3 billboardSize = float3(1, 1, 1);
    
    float4 worldPosition = float4(1, 1, 1, 1);
    
    // Billboard (las particulas siempre miran a la camara)
    worldPosition.xyz = float3(0, 0, 0)
    + cameraRight * vertice.x * billboardSize.x 
    + cameraUp * vertice.y * billboardSize.y;
	
    output.Position = mul(mul(mul(worldPosition, World), View), Projection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.ScreenPosition = output.Position;
    output.WorldPosition = worldPosition;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 waterSplash = tex2D(SpriteSheetSampler, (input.TextureCoordinates * SpritePixelSize + SpriteOffset) / SpriteSheetSize);
    
    waterSplash.rgb = pow(waterSplash.rgb, 2.2);
    
    return waterSplash;
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
    float4 waterSplash = tex2D(SpriteSheetSampler, (input.TextureCoordinates * SpritePixelSize + SpriteOffset) / SpriteSheetSize);
    
    float height = input.WorldPosition.y;
    float cameraDepth = input.ScreenPosition.w;
    return float4(height, cameraDepth, 0, 1) * step(0.5, waterSplash.a);
}

technique HeightMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL HeightMapPS();
    }
};