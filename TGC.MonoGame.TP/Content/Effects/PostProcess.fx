#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture MainScene;
sampler2D MainSceneSampler = sampler_state
{
    Texture = (MainScene);
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};
texture HeightMap;
sampler2D HeightMapSampler = sampler_state
{
    Texture = (HeightMap);
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = input.Position;
    output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 mainScene = tex2D(MainSceneSampler, input.TextureCoordinate).rgb;
    float heightMap = tex2D(HeightMapSampler, input.TextureCoordinate).r;
    float cameraDepth = tex2D(HeightMapSampler, input.TextureCoordinate).g;
    
    float3 fogColor = float3(0.39, 0.39, 0.39);
    
    float heightMask = lerp(heightMap, 1, saturate(cameraDepth));
    float3 finalColor = lerp(fogColor, mainScene, saturate(heightMask));
    
    //return float4(heightMap, heightMap, heightMap, 1);
    return float4(finalColor, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};