float4x4 vpMatrix :WORLDVIEWPROJ;	//world view proj matrix
float4x4 world;
uniform extern texture textureImage;// store texture

// Diffuse Variables
float4 shipColor;
float shipLight;
float3 shipPos;

float wallPos;
float wallLight;
float4 wallColor;

float3 cameraPosition;

// filter (like a brush) for showing texture
sampler textureSampler  = sampler_state
{
	Texture				= <textureImage>;
	magfilter			= LINEAR;	// magfilter - bigger than actual
	minfilter			= LINEAR;	// minfilter - smaller than actual
	mipfilter			= LINEAR;
};

// input to vertex shader
struct VSinput
{									 // input to vertex shader
	float4 position		: POSITION0; // position semantic x,y,z,w
	float3 normal		: NORMAL0;	 
	float2 uv			: TEXCOORD0; // texture semantic u,v
};

// vertex shader output
struct VStoPS
{ 
	// vertex shader output
	float4 position		: POSITION0; // position semantic x,y,z,w		 
	float2 uv			: TEXCOORD0; // texture semantic u,v
	float3 normal		: TEXCOORD1;
	float3 lightNormal	: TEXCOORD2;
	float distance      : TEXCOORD3;
	float wallDistance      : TEXCOORD4;
};

// pixel shader output
struct PSoutput
{									// pixel shader output
	float4 color		: COLOR0;   // colored pixel is output
};

// alter vertex inputs
void VertexShader(in VSinput IN, out VStoPS OUT)
{
	OUT.position = mul(IN.position, mul(world, vpMatrix)); // transform object
	OUT.normal = normalize(mul(IN.normal,world));
	IN.position = mul(IN.position, world);
	OUT.lightNormal = normalize(shipPos - IN.position);
	OUT.distance = length(shipPos - IN.position);
	OUT.wallDistance =  IN.position.z - wallPos;
	OUT.uv = IN.uv;	
}

// convert color and texture data from vertex shader to pixels
void PixelShader(in VStoPS IN, out PSoutput OUT)
{
	OUT.color  = tex2D(textureSampler, IN.uv);	
	OUT.color *= //saturate(
		dot(IN.normal, IN.lightNormal) * (shipColor * (shipLight/IN.distance) * 2 / 3 +
		wallColor * (wallLight / IN.wallDistance));
		//);
	OUT.color.w = 1;
}

// the shader starts here
technique TextureShader
{
	pass p0
	{
		// texture sampler initialized
		sampler[0]		= (textureSampler);
		
		// declare and initialize vs and ps
		vertexshader	= compile vs_1_1 VertexShader();
		pixelshader		= compile ps_2_0 PixelShader();
	}
}

