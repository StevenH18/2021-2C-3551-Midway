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

float2 Direction;
float Gravity;
float Steepness;
float WaveLength;

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 MyPosition : TEXCOORD1;
};

// Prestado de https://catlikecoding.com/unity/tutorials/flow/waves/
float3 CalculateWaves(float4 vertexData)
{
    float3 p = vertexData.xyz;                      // Posicion
    float k = 2.0 * PI / WaveLength;                // WaveLength que no este en radianes
    float2 d = normalize(Direction);                // Normalizar la direccion 
    float c = sqrt(Gravity / k);                    // Calcular la velocidad de las olas dada una gravedad
    float f = k * (dot(d, p.xz) - Time * c);        // Funcion que va en el coseno y seno
    float a = Steepness / k;
    p.x += d.x * a * cos(f);
    p.y = a * sin(f);
    p.z += d.y * a * cos(f);
    
    float3 tangent = float3(
				1 - d.x * d.x * (Steepness * sin(f)),
				d.x * (Steepness * cos(f)),
				-d.x * d.y * (Steepness * sin(f))
			);
    float3 binormal = float3(
				-d.x * d.y * (Steepness * sin(f)),
				d.y * (Steepness * cos(f)),
				1 - d.y * d.y * (Steepness * sin(f))
			);
    float3 normal = normalize(cross(binormal, tangent));
    
    return p;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	// Hago algunas olas 
	// TODO hacer que las olas sean parametrizadas (supongo que la simulacion tiene que ser igual del lado de los barcos para saber su velocidad)
    input.Position.xyz = CalculateWaves(input.Position);
	
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);		
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
	
	// Guardo en otra variable la posicion en el mundo para que sea accesible en el Pixel Shader
    output.MyPosition = worldPosition;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 position = float4(input.MyPosition, 1.0);
    
    float height = max(min(CalculateWaves(position).y / 200, 0.5), 0);
	
    float red = 0.18 + height;
    float green = 0.5 + height;
    float blue = 0.69 + height;
	
    return float4(red, green, blue, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
