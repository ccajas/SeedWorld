float4x4 World;
float4x4 View;
float4x4 Projection;

float3 camPos;
float3 skyColor;
float timeOfDay;

Texture skyTexture;
Texture aoTexture;

sampler TextureSampler = sampler_state 
{ 
	texture = <skyTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter= LINEAR; 
	AddressU = clamp; 
	AddressV = clamp;
};

sampler OcclusionSampler = sampler_state 
{ 
	texture = <aoTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter= LINEAR; 
};

// Debug only
float toggleColors;

struct VertexShaderInput
{
    uint4 Position : POSITION0;
	uint4 ColorNormal : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	uint4  Color : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
	float3 LocalPos : TEXCOORD3;
	float  Shading : TEXCOORD4;
};

// Normal lookups

static float3 normals[6] = {
	float3(0, 1, 0),
	float3(0, -1, 0),
	float3(1, 0, 0), // West
	float3(-1, 0, 0),  // East
	float3(0, 0, -1), // North
	float3(0, 0, 1),  // South
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.Color = input.ColorNormal;
	output.Shading = input.Position.w * 0.00390625;

	// Get the normal face to use
	uint nID = input.ColorNormal.x % 8;

	output.Normal = normalize(mul(normals[nID], World));
	output.WorldPos = worldPosition.xyz;
	output.LocalPos = input.Position.xzy;

    return output;
}

static const float PI = 3.14159265f;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float colorR = (float)input.Color.y * 0.00390625; // 1 / 255f
	float colorG = (float)input.Color.z * 0.00390625;
	float colorB = (float)input.Color.w * 0.00390625;

	// Ambient occlusion is added from the fourth bit
	float ao = (float)(input.Color.x * 0.125f) % 4;
	float shading = (float)input.Shading;

	// Calculate the distance to lerp between
	float dist = distance(camPos.xz, input.WorldPos.xz);

	// Water and sky blend
	float alpha = (1 - saturate(dist/125 - 3.5f));

	ao = (ao * .25f) + .75f;
	ao = clamp(ao, 0, 10);

	// Sky/fog color and diffuse color
	float3 fog = tex2D(TextureSampler, float2(timeOfDay, 0.5f));
	float3 color = float3(colorR, colorG, colorB);

	float ambientCurve = sin((timeOfDay - 0.05) * PI * 2);
	ambientCurve = pow(ambientCurve, 8) * 0.2f;
	float ambientIntensity = 0.1f + ambientCurve;

	// Compute ambient color based on skymap (not physically accurate but it works)
	float2 ambientTexCoord = float2(timeOfDay, 1 - input.Normal.y);
	float3 ambient = tex2D(TextureSampler, ambientTexCoord);
	
	// Pseudo-reflection for water on sky color
	if ((uint)input.Color.x >= 128) color = lerp(color, fog, 0.5f);

	// Gamma correct and add ambient
	float3 totalAmbient = ambient * ambientIntensity * 1;
	color *= (1 - totalAmbient);
	color += (toggleColors >= 1) ? totalAmbient : ambientIntensity * 0.5f;

	// Basic shading
	// Adjust for time
	float timeShade = 0.98f * pow(sin((timeOfDay - 0.05) * PI), 2) + 0.02f;
	float3 lightDirection = normalize(float3(cos(timeShade), sin(timeShade), 0.5));

	float NdL = saturate(dot(input.Normal, lightDirection));
	NdL = saturate((NdL * (1 - ambientIntensity * 4)) + ambientIntensity * 4);

	// Gamma inverse
	color *= timeShade * ao * (NdL * shading);

	// Blend with fog
    return float4(lerp(color, fog, saturate(-2.75f + dist/150.f)), alpha);
}

technique Default
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		Cullmode = CCW;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
