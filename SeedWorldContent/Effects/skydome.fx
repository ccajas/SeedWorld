float4x4 World;
float4x4 View;
float4x4 Projection;

float3 camPos;
float timeOfDay;
float dayLengthTime;

Texture skyTexture;

sampler TextureSampler = sampler_state 
{ 
	texture = <skyTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter= LINEAR; 
	AddressU = clamp; 
	AddressV = clamp;
};

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 LocalPos : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.LocalPos = input.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float currentTime = timeOfDay / dayLengthTime;
	float2 texCoord = float2(currentTime, 1 - input.LocalPos.y);
	float3 skyColorAtPosition = tex2D(TextureSampler, texCoord);

    return float4(skyColorAtPosition, 1);
}

technique Default
{
    pass Pass1
    {
		Cullmode = CW;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
