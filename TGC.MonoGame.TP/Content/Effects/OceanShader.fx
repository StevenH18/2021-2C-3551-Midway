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

// Illumination parameters
float3 LightPositions[5];
float3 LightColors[5];

float3 EyePosition; // Camera position

//Textura para Albedo
texture AlbedoTexture;
sampler2D AlbedoSampler = sampler_state
{
    Texture = (AlbedoTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Normales
texture NormalTexture;
sampler2D NormalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};
// Que tan pronunciada son las normales en el shader
float NormalIntensity;

//Textura para Foam
texture NoiseTexture;
sampler2D NoiseSampler = sampler_state
{
    Texture = (NoiseTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura del skybox o del ambiente
texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
    Texture = (EnvironmentMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

//Textura del cameraDepth del oceano
texture DepthTexture;
sampler2D DepthSampler = sampler_state
{
    Texture = (DepthTexture);
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura del color de lo que esta debajo del oceano
texture DepthColorTexture;
sampler2D DepthColorSampler = sampler_state
{
    Texture = (DepthColorTexture);
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

// Ocean waves parameters
float Gravity;

float4 WaveA;
float4 WaveB;
float4 WaveC;

// Islands
float4 Islands[5];

// Ship foam parameters
float4 TrailPositions[100];
float TrailLifeTimes[100];
float TrailFadeout = 0;

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
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 ScreenPosition : TEXCOORD3;
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
    
    steepness = lerp(steepness, steepness * 0.05, ClosenessToIsland(vertex));
    
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
    output.ScreenPosition = output.Position;

    return output;
}

float3 GetNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal1 = tex2D(NormalSampler, textureCoordinates * 0.8 + float2(-Time * 0.02, -Time * 0.02) + 500).xyz * 2.0 - 1.0;
    float3 tangentNormal2 = tex2D(NormalSampler, textureCoordinates + float2(Time * 0.02, Time * 0.02)).xyz * 2.0 - 1.0;
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

float3 GetNormals(VertexShaderOutput input)
{
    float3 worldNormal = input.Normal;
    float3 normal = GetNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, worldNormal);
    return lerp(worldNormal, normal, min(saturate(NormalIntensity), 1.1 - saturate(ClosenessToIsland(input.WorldPosition.xyz))));
}

float3 GetReflection(VertexShaderOutput input, float3 normal, float3 view)
{
    //Obtener texel de CubeMap
    float3 viewRotated = -view;
    float angle = PI;
    viewRotated.x = view.x * cos(angle) + view.z * sin(angle);
    viewRotated.z = view.z * cos(angle) + view.x * sin(angle);
    
    float3 reflection = reflect(viewRotated, normal);
    return texCUBE(EnvironmentMapSampler, reflection).rgb;
}

float2 GetTrails(VertexShaderOutput input)
{
    float2 trail = float2(0, 0);
    
    for (int i = 0; i < 100; i++)
    {
        float distanceFromWorld = distance(TrailPositions[i].xyz, input.WorldPosition.xyz);
        float maxSize = TrailPositions[i].w * (Time + 1 - TrailLifeTimes[i]);
        
        trail.x += (1 - smoothstep(0, maxSize, distanceFromWorld));
        trail.y += (1 - smoothstep(0, maxSize + 10, distanceFromWorld)) * (saturate(Time - TrailLifeTimes[i] - 2));

    }
    
    return trail;
}

//Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    input.ScreenPosition.y = -input.ScreenPosition.y;
    float3 normal = GetNormals(input);
    float3 view = normalize(EyePosition - input.WorldPosition.xyz);
    float fresnel = saturate((1.0 - dot(normal, view)));
    
    float3 foam1 = tex2D(NoiseSampler, input.TextureCoordinates * 10);
    float3 foam2 = tex2D(NoiseSampler, input.TextureCoordinates * 10);
    float outerTrail = saturate(sin(min(GetTrails(input).x, PI)));
    float innerTrail = saturate(GetTrails(input).x / 25);
    float trailMask = GetTrails(input).y;
    
    normal = lerp(normal, normal * 2, saturate(outerTrail - trailMask));
    
    float3 albedo = pow(tex2D(AlbedoSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
    float3 reflectionColor = GetReflection(input, normal, view);
    float3 underwaterColor = tex2D(DepthColorSampler, (input.ScreenPosition.xy / input.ScreenPosition.w + 1) / 2 + normal.xz);
    float depth = tex2D(DepthSampler, (input.ScreenPosition.xy / input.ScreenPosition.w + 1) / 2).r;
    float caustics = tex2D(DepthSampler, (input.ScreenPosition.xy / input.ScreenPosition.w + 1) / 2 + normal.xz).g;
    
    // waterColor es el color con los efectos de reflejos en el agua
    float3 waterColor = lerp(albedo, reflectionColor, saturate(fresnel - 0.25));
    
    
    // Agregamos reflejos al oleaje dependiendo del fresnel
    underwaterColor = lerp(underwaterColor, waterColor, saturate(fresnel - 0.25));
    
    // Si me alejo de las islas dibujo lo mismo que waterColor para que no se vea atravez de las olas
    // ESTO SE PODRIA MEJORAR
    underwaterColor = lerp(waterColor, underwaterColor, smoothstep(0.5, 1, ClosenessToIsland(EyePosition)));
    
    // dependiendo de que tan cerca este de la orilla dibujo el color de la orilla o el del agua
    float3 finalColor = lerp(waterColor, underwaterColor, saturate(depth) * caustics * 4);
    
    // agregamos foam al color final
    finalColor = finalColor + foam1 * innerTrail + saturate(saturate(pow(outerTrail + foam2 * outerTrail, 20)) - trailMask);
  
    return float4(finalColor, 1);
}

/*
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
*/

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
    float height = (input.WorldPosition.y) / 2000;
    float cameraDepth = pow(1 - saturate(input.ScreenPosition.w / 20000), 2);
    return float4(height, cameraDepth, height, 1);
}

technique HeightMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL HeightMapPS();
    }
};
