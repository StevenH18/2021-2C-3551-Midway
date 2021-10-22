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
float4x4 InverseTransposeWorld;

// Illumination parameters
float3 AmbientColor; // Light's Ambient Color
float3 DiffuseColor; // Light's Diffuse Color
float3 SpecularColor; // Light's Specular Color
float KAmbient;
float KDiffuse;
float KSpecular;
float Shininess;
float3 LightPosition;
float3 EyePosition; // Camera position

// Texture parameters
texture DiffuseMap;
sampler2D DiffuseSampler = sampler_state
{
    Texture = (DiffuseMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
texture NormalMap;
sampler2D NormalSampler = sampler_state
{
    Texture = (NormalMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float Gravity;

float4 WaveA;
float4 WaveB;
float4 WaveC;

float4 IslandA;

float4 Islands[5];

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

float ClosenessToIsland(float3 position)
{
    float previousDistance = 0;
    
    for (int i = 0; i < 5; i++)
    {
        previousDistance = max(previousDistance, saturate(Islands[i].w / distance(position, Islands[i].xyz)));
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
    
    steepness = lerp(steepness, 0.01, ClosenessToIsland(vertex));
    
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
    float4 normal = float4(normalize(cross(binormal, tangent)), 0);
    input.Position.xyz = vertex;
    
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);		
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = worldPosition;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = normal;

    return output;
}

float3 GetNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal1 = tex2D(NormalSampler, textureCoordinates * 1 + float2(-Time * 0.03, -Time * 0.03)).xyz * 2.0 - 1.0;
    float3 tangentNormal2 = tex2D(NormalSampler, textureCoordinates * 1 + float2(Time * 0.03, Time * 0.03)).xyz * 2.0 - 1.0;
    float3 tangentNormal = tangentNormal1 * 0.5 + tangentNormal2 * 0.5;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Debug para saber dibuje un color verde dependiendo de que tan cerca se esta de una isla
    // step(1, ... es para que aparezca un color al rededor de las islas
    // step(2, ... para que no aparezca
    float island = step(2, ClosenessToIsland(input.WorldPosition));
    float3 diffuseColor = DiffuseColor * (1 - island) + float3(0.1, 0.4, 0.2) * island;
    
    // Base vectors
    float3 lightDirection = normalize(LightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(EyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    //float3 normal = input.Normal.xyz;
    float3 normal = GetNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, input.Normal.xyz);

	// Get the texture texel
    float4 texelColor1 = tex2D(DiffuseSampler, input.TextureCoordinates + float2(Time * 0.01, Time * 0.01));
    float4 texelColor2 = tex2D(DiffuseSampler, input.TextureCoordinates + float2(Time * 0.02, Time * 0.02));
    float4 texelColor = texelColor1 * 0.5 + texelColor2 * 0.5;

    
	// Calculate the diffuse light
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(normal, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * SpecularColor * pow(saturate(NdotH), Shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(AmbientColor * KAmbient + diffuseLight) + specularLight, 1);
    return finalColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
