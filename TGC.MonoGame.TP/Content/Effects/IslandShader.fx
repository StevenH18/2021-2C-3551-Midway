#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define PI 3.1415926538

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseTransposeMatrix;

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

//Textura para Normals
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

//Textura para Metallic
texture MetallicTexture;
sampler2D MetallicSampler = sampler_state
{
    Texture = (MetallicTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Roughness
texture RoughnessTexture;
sampler2D RoughnessSampler = sampler_state
{
    Texture = (RoughnessTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Ambient Occlusion
texture AoTexture;
sampler2D AoSampler = sampler_state
{
    Texture = (AoTexture);
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

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float4 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	// Clear the output
    VertexShaderOutput output = (VertexShaderOutput) 0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldPosition = worldPosition;
    output.Normal = input.Normal;

	return output;
}

float3 GetNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(NormalSampler, textureCoordinates).xyz * 2.0 - 1.0;

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

float DistributionGGX(float3 normal, float3 halfVector, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(normal, halfVector), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 normal, float3 view, float3 light, float roughness)
{
    float NdotV = max(dot(normal, view), 0.0);
    float NdotL = max(dot(normal, light), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    float antiRoughness = 1.0 - roughness;
    return F0 + (max(float3(antiRoughness, antiRoughness, antiRoughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

float4 irradianceCalculation(VertexShaderOutput input)
{
    float3 N = normalize(input.WorldPosition);
	float3 irradiance = float3(0, 0, 0);

	// tangent space calculation from origin point
	float3 up = float3(0.0, 1.0, 0.0);
	float3 right = cross(up, N);
	up = cross(N, right);
    
    float sampleDelta = 0.025;
	float nrSamples = 0.0;

	float phi = 0.0;
    float theta = 0.0;
    
    while (phi < (2.0 * 3.14))
	{
		while (theta < (0.5 * 3.14))
		{
            float displacedTheta = theta - (0.25 * 3.14);
			// spherical to cartesian (in tangent space)
            float3 tangentSample = float3(sin(displacedTheta) * cos(phi), sin(displacedTheta) * sin(phi), cos(displacedTheta));
			// tangent space to world
			float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

            irradiance += texCUBE(EnvironmentMapSampler, sampleVec).rgb * cos(theta) * sin(theta);
			nrSamples++;
			theta += sampleDelta;
		}
		phi += sampleDelta;
	}

	irradiance = PI * irradiance * (1.0 / float(nrSamples));
	return float4(irradiance, 1.0);

	//FragColor = float4(irradiance, 1.0);

	// Tomamos la posicion en mesh space del cubo unitario como coordenadas de textura
	// Esta posicion va perfectamente con las coordenadas de textura necesarias para el cubemap
	// Usamos texCUBE sin lod para tener la maxima resolucion de mipmap
	//float3 cubeMapSampled = texCUBE(texCubeMap, input.MeshPosition).rgb;
	//return float4(cubeMapSampled, 1.0);*/
}

//Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 albedo = pow(tex2D(AlbedoSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
    float metallic = tex2D(MetallicSampler, input.TextureCoordinates).r;
    float roughness = tex2D(RoughnessSampler, input.TextureCoordinates).r;
    float ao = tex2D(AoSampler, input.TextureCoordinates).r;
    
    //ao = 0.05;

    float3 worldNormal = input.Normal;
    float3 N = GetNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, worldNormal);
    float3 V = normalize(EyePosition - input.WorldPosition);
    float3 R = reflect(-V, N);
	
    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
	// of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, albedo, metallic);

	// reflectance equation
    float3 Lo = float3(0, 0, 0);
    for (int i = 0; i < 4; ++i)
    {
		// calculate per-light radiance
        float3 L = normalize(LightPositions[i] - input.WorldPosition);
        float3 H = normalize(V + L);
        float distance = length(LightPositions[i] - input.WorldPosition);
        float attenuation = 1.0 / (distance);
        float3 radiance = LightColors[i] * attenuation;

		// Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughness);
        float G = GeometrySmith(N, V, L, roughness);
        float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

        float3 nominator = NDF * G * F;
        float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero.
        float3 specular = nominator / denominator;

		// kS is equal to Fresnel
        float3 kS = F;
		// for energy conservation, the diffuse and specular light can't
		// be above 1.0 (unless the surface emits light); to preserve this
		// relationship the diffuse component (kD) should equal 1.0 - kS.
        float3 kD = float3(1, 1, 1) - kS;
		// multiply kD by the inverse metalness such that only non-metals 
		// have diffuse lighting, or a linear blend if partly metal (pure metals
		// have no diffuse light).
        kD *= 1.0 - metallic;

		// scale light by NdotL
        float NdotL = max(dot(N, L), 0.0);

		// add to outgoing radiance Lo
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
    }

	// ambient lighting (we now use IBL as the ambient term)
    float3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);


    float3 kS = F;
    float3 kD = 1.0 - kS;
    kD *= 1.0 - metallic;

    float3 irradiance = irradianceCalculation(input).rgb;
    float3 diffuse = irradiance * albedo;

    float3 ambient = (kD * diffuse) * ao * 0.1;
    
    float3 color = ambient + Lo;

	// HDR tonemapping
    color = color / (color + float3(1, 1, 1));
	// gamma correct
    color = pow(color, float3(1.0 / 2.2, 1.0 / 2.2, 1.0 / 2.2));

    return float4(color, 1.0);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

// Depth Pass

float ShoreWidth;
float ShoreSmoothness;

struct DepthPassVertexShaderInput
{
    float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ScreenSpacePosition : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
};

DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
    DepthPassVertexShaderOutput output;
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    output.ScreenSpacePosition = mul(mul(mul(input.Position, World), View), Projection);
    output.WorldPosition = mul(input.Position, World);
    return output;
}

float4 DepthPS(in DepthPassVertexShaderOutput input) : COLOR
{
    // Depth based on y world position
    float depth = (ShoreWidth + input.WorldPosition.y) / ShoreSmoothness;
    return float4(depth, depth, depth, 1.0);
}

technique DepthPass
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL DepthVS();
        PixelShader = compile PS_SHADERMODEL DepthPS();
    }
}