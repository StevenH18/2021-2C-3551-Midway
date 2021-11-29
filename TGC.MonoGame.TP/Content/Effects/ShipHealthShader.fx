#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture HealthTexture;
sampler2D HealthSampler = sampler_state
{
    Texture = (HealthTexture);
    ADDRESSU = clamp;
    ADDRESSV = clamp;
};

float PlayerHealth;
float PlayerMaxHealth;

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
    float4 health = tex2D(HealthSampler, input.TextureCoordinates);
    float4 healthBehind = tex2D(HealthSampler, input.TextureCoordinates);
    
    healthBehind.g = 0;
    healthBehind.b = 0;
    
    float mask = PlayerHealth / PlayerMaxHealth;
    
    health = lerp(healthBehind, health, step(input.TextureCoordinates.x, mask));
    
    return health;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};