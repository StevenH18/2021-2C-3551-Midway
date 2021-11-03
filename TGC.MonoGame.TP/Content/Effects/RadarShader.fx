#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define PI 3.1415926538

texture RadarTexture;
sampler2D RadarSampler = sampler_state
{
    Texture = (RadarTexture);
    ADDRESSU = clamp;
    ADDRESSV = clamp;
};

texture RadarLineTexture;
sampler2D RadarLineSampler = sampler_state
{
    Texture = (RadarLineTexture);
    ADDRESSU = clamp;
    ADDRESSV = clamp;
};

float3 CameraPosition;
float3 CameraForward;
float3 ShipPositions[50];

float RadarRange;

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
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = input.Position;
    
    output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

float2 rotateUV(float2 uv, float rotation, float2 mid)
{
    return float2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

float dotInRadar(VertexShaderOutput input, float2 rotation)
{
    float radarDot = 0;
    for (int i = 0; i < 50; i++)
    {        
        float2 shipPosition = float2(ShipPositions[i].x, ShipPositions[i].z);
        float2 cameraPosition = float2(CameraPosition.x, CameraPosition.z);
    
        float angle = atan2(CameraForward.z, CameraForward.x) + PI * 1.5;
        
        float2 radarOrientation = rotateUV(normalize(cameraPosition - shipPosition), angle, float2(0, 0));
        
        float dotColor = step(distance(input.TextureCoordinates, float2(0.5, 0.5) + distance(cameraPosition, shipPosition) * radarOrientation / RadarRange), 0.01);
        
        radarDot += lerp(0, dotColor, step(distance(input.TextureCoordinates, float2(0.5, 0.5)), 0.5));
    }
    return radarDot;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 radar = tex2D(RadarSampler, input.TextureCoordinates);
    
    float2 rotation = rotateUV(input.TextureCoordinates, Time, float2(0.5, 0.5));
    float4 radarLine = tex2D(RadarLineSampler, rotation);
    
    float dots = dotInRadar(input, rotation);
    
    //return step(distance(input.TextureCoordinates, float2(0.5, 0.5) + float2(cos(Time), sin(Time)) * 0.5), 0.1);
    
    //return float4(CameraForward, 1);
    return radar + radarLine + float4(0, dots, 0, 0);
    return float4(1,1,1,1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
        SrcBlend = SrcColor;
        DestBlend = DestAlpha;
    }
};