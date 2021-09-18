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

float Gravity;

float4 WaveA;
float4 WaveB;
float4 WaveC;

int IslandsAmmount = 20;


float4 Islands[20];

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 MyPosition : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

float ClosenessToIsland(float3 position)
{
    float previousDistance = 1;
    
    //previousDistance = Islands[0][0];
    
    for (int i = 0; i < IslandsAmmount; i++)
    {
        //previousDistance = min(previousDistance, clamp((distance(position, Islands[i][0]) - Islands[i].w) / min((log(Islands[i].w) * 200), 1), 0, 1));
    }
    return previousDistance;
}

// Tipo de olas: Gerstner Waves o tambien conocido como Trochoidal Waves
// Implementacion basada en https://catlikecoding.com/unity/tutorials/flow/waves/
float3 CalculateWave(float4 wave, float3 vertex, inout float3 tangent, inout float3 binormal)
{
    float2 direction = wave.xy;
    float steepness = wave.z;
    float wavelength = wave.w;
    
    steepness = lerp(0, steepness, ClosenessToIsland(vertex));
    
    float3 p = vertex;                              // Posicion del vertice
    float k = 2.0 * PI / wavelength;                // Pasar el WaveLength a radianes
    float2 d = normalize(direction);                // Normalizar la direccion 
    float c = sqrt(Gravity / k);                    // Calcular la velocidad de las olas dada una gravedad
    float f = k * (dot(d, p.xz) - Time * c);        // Funcion del tiempo
    float a = steepness / k;                        // Mientras Steepnes este entre 0 y 1 no se romperan las olas
    
    tangent += float3(
		-d.x * d.x * (steepness * sin(f)),
		d.x * (steepness * cos(f)),
		-d.x * d.y * (steepness * sin(f))
	);
    binormal += float3(
		-d.x * d.y * (steepness * sin(f)),
		d.y * (steepness * cos(f)),
		-d.y * d.y * (steepness * sin(f))
	);
    
    
    return float3(
        d.x * a * cos(f),
        a * sin(f),
        d.y * a * cos(f)
    );
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	// V2 posibilidad de usar muchas olas
    // Primero obtengo la posicion del vertice y creo una tangente
    // y binormal para luego sumarles las tangentes y binormales de la funcion
    float3 vertex = input.Position.xyz;
    float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
    // Aca es donde puedo agregar mas olas
    vertex += CalculateWave(WaveA, vertex, tangent, binormal);
    vertex += CalculateWave(WaveB, vertex, tangent, binormal);
    vertex += CalculateWave(WaveC, vertex, tangent, binormal);
    // Calculo la normal y guardo la posicion del vertice transformada
    float3 normal = normalize(cross(binormal, tangent));
    input.Position.xyz = vertex;
    
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);		
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
	
	// Guardo en otra variable la posicion en el mundo para que sea accesible en el Pixel Shader
    output.MyPosition = worldPosition;
    output.Normal = normal; //mul(float4(normal, 1), World);

    return output;
}

float MaxHeight(float4 wave)
{
    float steepness = WaveA.z;
    float wavelength = WaveA.w;
    
    float k = 2.0 * PI / wavelength;
    
    return steepness / k;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 position = float4(input.MyPosition, 1.0);
	
    float4 color1 = float4(0, 0.3, 0.8, 1);
    float4 color2 = float4(0, 0.7, 0.9, 1);
    
    float maxHeight = 0;
    
    maxHeight += MaxHeight(WaveA);
    maxHeight += MaxHeight(WaveB);
    maxHeight += MaxHeight(WaveC);
    
    color1 = lerp(float4(0.5, 0.9, 0.5, 1), color1, ClosenessToIsland(position.xyz));
    
    float4 color = lerp(color1, color2, (position.y + maxHeight / 2) / maxHeight);
	
    return color; //float4(red, green, blue, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
