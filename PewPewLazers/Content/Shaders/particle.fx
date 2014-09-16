float4x4 World;
float4x4 View;
float4x4 Projection;
float time;
Texture pic;

sampler textureSampler = sampler_state{
Texture = <pic>; 
magfilter = LINEAR;
minfilter = LINEAR;
mipfilter = LINEAR;
};

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Velocity : POSITION1;
	float4 Color : COLOR0;
	float1 Size : PSIZE0;
	float1 LifeTime : PSIZE1;
	float1 StartTime : PSIZE2;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float1 Size : PSIZE;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position = input.Position + (input.Velocity * (time-input.StartTime));
    input.Position.w = 1;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Size = input.Size;
	output.Color = float4(input.Color.xyz, 1);
	output.UV = float2(1.0f, 1.0f);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 UV = input.UV.xy;	
	return (tex2D(textureSampler, UV)*input.Color);
}
 
technique Technique1
{
    pass Pass1
    {
		PointSpriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = One;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
