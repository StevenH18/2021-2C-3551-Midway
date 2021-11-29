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
texture UnderwaterNormals;
sampler2D UnderwaterNormalsSampler = sampler_state
{
    Texture = (UnderwaterNormals);
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};
bool Underwater;

float Time = 0;

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

float3 abovewater(VertexShaderOutput input)
{
    float3 mainScene = tex2D(MainSceneSampler, input.TextureCoordinate).rgb;
    float heightMap = tex2D(HeightMapSampler, input.TextureCoordinate).r;
    float cameraDepth = tex2D(HeightMapSampler, input.TextureCoordinate).g;
    float billboardAlpha = tex2D(HeightMapSampler, input.TextureCoordinate).b;
    
    heightMap = (heightMap) / 3000;
    cameraDepth = pow(1 - saturate(cameraDepth / 20000), 2);
    
    float3 fogColor = float3(0.39, 0.39, 0.39);
    
    float heightMask = lerp(heightMap, 1, saturate(cameraDepth));
    float3 finalColor = lerp(fogColor, mainScene, saturate(saturate(heightMask) + billboardAlpha));
    
    return finalColor;
}
float3 underwater(VertexShaderOutput input)
{
    float3 underWaterNormals1 = tex2D(UnderwaterNormalsSampler, input.TextureCoordinate * 0.1 + float2(-Time, Time) * 0.05).rgb;
    float3 underWaterNormals2 = tex2D(UnderwaterNormalsSampler, input.TextureCoordinate * 0.15 + float2(-Time, -Time) * 0.05).rgb;
    float3 underWaterNormals3 = tex2D(UnderwaterNormalsSampler, input.TextureCoordinate * 0.19 + float2(Time, -Time) * 0.05).rgb;
    float3 underWaterNormals = underWaterNormals1 + underWaterNormals2 - underWaterNormals3;
    float3 mainSceneWarped = tex2D(MainSceneSampler, input.TextureCoordinate * 0.8 + underWaterNormals.xz * 0.1).rgb;
    float cameraDepth = tex2D(HeightMapSampler, input.TextureCoordinate * 0.8 + underWaterNormals.xz * 0.1).g;
    
    float3 underwaterFogColor = float3(0.06, 0.06, 0.06);
    
    cameraDepth = pow(1 - saturate(cameraDepth / 1000), 2);
    
    float3 finalColor = lerp(underwaterFogColor, mainSceneWarped, saturate(cameraDepth));
    
    return finalColor;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 above = abovewater(input);
    float3 under = underwater(input);
    
    float3 finalColor = lerp(above, under, Underwater);
    
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