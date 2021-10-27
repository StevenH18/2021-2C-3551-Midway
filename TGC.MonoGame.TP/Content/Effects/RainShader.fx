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
float Time = 0;

float3 CameraPosition;
float ParticleSeparation;

float ParticleHeight;
float ParticleWidth;

int ParticlesTotal;
float Speed;
float HeightStart;
float HeightEnd;
float Progress;


struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexInstanceInputSimple
{
    float4 Offset : TEXCOORD0; // the number used must match the vertex declaration.
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Offset : TEXCOORD1;
};

float rand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float SkipParticle(float index, float progress)
{
    return step((index) / ParticlesTotal, progress - 0.01);
}

// ?????????????????? NO SE PORQUE NO SE PUEDE SACAR INDEX ????????? ESTOY PASANDO EL INDEX POR offset.w
VertexShaderOutput MainVS(in VertexShaderInput input, VertexInstanceInputSimple instance)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    
    float4 offset = instance.Offset;
    
    input.Position.y *= -ParticleHeight;
    input.Position.x *= ParticleWidth;
    
    // PROBANDO PROBANDO ACA
    // Hacer que haya un cuadrado de tamanio:"ParticleSeparation" donde siempre estan las particulas
    // Se teletransportan al otro lado si salen, como en PACMAN
    offset.x = offset.x + floor((CameraPosition.x - offset.x + ParticleSeparation / 2) / ParticleSeparation) * ParticleSeparation;
    offset.y = lerp(HeightStart, HeightEnd, frac((Time + offset.y) / (HeightStart - HeightEnd) * Speed));
    offset.z = offset.z + floor((CameraPosition.z - offset.z + ParticleSeparation / 2) / ParticleSeparation) * ParticleSeparation;
    
    float3 cameraRight = float3(View[0][0], View[1][0], View[2][0]);
    float3 cameraUp = float3(0, 1, 0);
    float3 vertice = input.Position.xyz;
    float3 billboardSize = float3(1, 1, 1);
    
    float4 worldPosition = float4(1, 1, 1, 1);
    
    // Billboard (las particulas siempre miran a la camara)
    worldPosition.xyz = float3(offset.x, offset.y, offset.z)
    + cameraRight * vertice.x * billboardSize.x 
    + cameraUp * vertice.y * billboardSize.y;
    
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.Offset = offset;
	
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = float4(1, 1, 1, 1) * 0.1 * rand(input.Offset.xz);
    // Controlar cuantas particulas de lluvia se muestran.
    // Progress == 0   -> ninguna particula
    // Progress == 0.5 -> la mitad de las particulas se muestran
    // Progress == 1   -> todas las particulas aparecen en pantalla
    float skip = SkipParticle(input.Offset.w, Progress);
	
    return color * skip;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
