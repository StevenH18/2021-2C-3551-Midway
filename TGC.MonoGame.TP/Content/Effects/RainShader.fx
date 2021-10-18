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

float3 CameraPosition;
float ParticleSeparation;

int ParticlesTotal;
float Speed;
float HeightStart;
float HeightEnd;
float Progress;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Index : TEXCOORD0;
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
VertexShaderOutput MainVS(in VertexShaderInput input, float4 index : POSITION1, float4 offset : POSITION2)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    
    // Hacer que haya un cuadrado de tamanio:"ParticleSeparation" donde siempre estan las particulas
    // Se teletransportan al otro lado si salen, como en PACMAN
    offset.x = offset.x + floor((CameraPosition.x - offset.x + ParticleSeparation / 2) / ParticleSeparation) * ParticleSeparation;
    offset.z = offset.z + floor((CameraPosition.z - offset.z + ParticleSeparation / 2) / ParticleSeparation) * ParticleSeparation;
	
    float x = input.Position.x;
    float z = input.Position.z;
    float angleToCamera = Time + offset.y; // Reemplazar con un calculo a la camara (me canse de probar)
    
    // Rotacion de las particulas con el tiempo
    input.Position.x = x * cos(angleToCamera) + z * sin(angleToCamera);
    input.Position.z = z * cos(angleToCamera) + x * sin(angleToCamera);

    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    
    // Muevo las particulas con su offset que se obtiene al azar y creo una animacion de caida
    worldPosition.x += offset.x;
    worldPosition.y += lerp(HeightStart, HeightEnd, frac((Time + offset.y) / (HeightStart - HeightEnd) * Speed));
    worldPosition.z += offset.z;
    
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.Index = index;
    output.Offset = offset;
	
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = float4(1, 1, 1, 1) * 0.2 * rand(input.Offset.xz);
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
