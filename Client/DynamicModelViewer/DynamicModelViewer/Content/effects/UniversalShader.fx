float4x4 xWorld;

////////////////////////////////
//Camera
float3 xCameraPosition;
float4x4 xCameraView;
float4x4 xCameraProjection;

////////////////////////////////
//Textued Objects
texture xTexture;
sampler TextureSamplerWrap = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
sampler TextureSamplerClamp = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

////////////////////////////////
//Shadows
texture xShadowMapTexture;
bool xEnableShadows;
sampler ShadowMapSampler =
      sampler_state
      {
          Texture = <xShadowMapTexture>;
          MinFilter = POINT;
          MagFilter = POINT;
          MipFilter = POINT;
          AddressU = Clamp;
          AddressV = Clamp;
      };

////////////////////////////////
//Lighting	
float4x4 xLightView;
float4x4 xLightProjection;
float3 xLightPosition;	
bool xEnableLighting;		
float4 xLightColor;
float3 xFogSeedPosition;
float4 xAmbient;

////////////////////////////////
//Transparency
bool xEnableTransparency;

////////////////////////////////
//Color variables
float4 xMeshColor;
bool xForceMeshColor;

////////////////////////////////
//Skinned Models
#define MaxNumberOfBones 59
bool xSkinnedModel;
float4x4 xBones[MaxNumberOfBones];

////////////////////////////////
//Lazer Beam
float xCurrentTime;

struct VertexShaderInput
{
    float4 Position   	: POSITION;   
    float3 Normal       : NORMAL;
    float2 TextureCoords: TEXCOORD0;
    float4 BoneIndices  : BLENDINDICES0;
    float4 BoneWeights  : BLENDWEIGHT0;
};

struct VtoPPosLightTexWorld
{
    float4 Position   	: POSITION;  
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
    float4 WorldPosition: TEXCOORD2;
};

struct VtoPPosColor
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float4 WorldPosition: TEXCOORD0;
};

struct ShadowMapOutput
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};

/////////////////////////////////////////////////////////////
// Vertex Shader Helper Functions

float4 CalculateVertexSkinnedPosition(float4 inputPosition, float4 inputBoneIndices, float4 inputBoneWeights)
{
    float4x4 skinTransform = 0;
   
    skinTransform += xBones[inputBoneIndices.x] * inputBoneWeights.x;
    skinTransform += xBones[inputBoneIndices.y] * inputBoneWeights.y;
    skinTransform += xBones[inputBoneIndices.z] * inputBoneWeights.z;
    skinTransform += xBones[inputBoneIndices.w] * inputBoneWeights.w;
    
    return mul(inputPosition, skinTransform);
    
}

float4 WorldPosition(float4 position)
{
   return mul(position, xWorld);
}

float4 CalculateVertexFramePosition(float4 position)
{
    float4 viewPosition = mul(WorldPosition(position), xCameraView);
    return mul(viewPosition, xCameraProjection);
}

float CalculateVertexLightingFactor(float3 inputNormal)
{
    float lightingFactor = 1;
    if(xEnableLighting)
    {
	   float3 Normal = normalize(mul(normalize(inputNormal), xWorld));	
	   float3 lightDirection = normalize(-xLightView._31_32_33);
	   lightingFactor = saturate(dot(Normal, -lightDirection));
	}
	return lightingFactor;
}

//////////////////////////////////////////////////////////
//Vertex Shaders

VtoPPosLightTexWorld TexturedVS(VertexShaderInput input)
{
    VtoPPosLightTexWorld output;

    float4 position = input.Position;
    if(xSkinnedModel)
       position = CalculateVertexSkinnedPosition(input.Position, input.BoneIndices, input.BoneWeights);
    output.Position = CalculateVertexFramePosition(position);
    output.LightingFactor = CalculateVertexLightingFactor(input.Normal);
    output.TextureCoords = input.TextureCoords;
    output.WorldPosition = WorldPosition(position);

    return output;
}

VtoPPosColor VertexPositionColorShader(float4 position : POSITION,  float4 color: COLOR)
{
    VtoPPosColor output  = (VtoPPosColor)0;
    
    output.Position = CalculateVertexFramePosition(position);
    output.Color = color;
    output.WorldPosition = WorldPosition(position);
    return output;
}

ShadowMapOutput VertexShadowMap(float4 position : POSITION)
{
    ShadowMapOutput output;
	output.Position = mul(position, mul(xWorld, mul(xLightView, xLightProjection))); 
	output.Depth.x = 1-(output.Position.z/output.Position.w);
	return output;
}


////////////////////////////////////////////////////////
// Pixel Shader Utility

float4x4 CreateLookAt(float3 cameraPos, float3 target, float3 up)
{
	float3 zaxis = normalize(cameraPos - target);
	float3 xaxis = normalize(cross(up, zaxis));
	float3 yaxis = cross(zaxis, xaxis);
	
	float4x4 view = { xaxis.x, yaxis.x, zaxis.x, 0,
		xaxis.y, yaxis.y, zaxis.y, 0,
		xaxis.z, yaxis.z, zaxis.z, 0,
		-dot(xaxis, cameraPos), -dot(yaxis, cameraPos),
		-dot(zaxis, cameraPos), 1
	};

	return view;
}
float4 GetPositionFromLight(float4 position)
{
	float4x4 WorldViewProjection =
	 mul(mul(xWorld, xLightView), xLightProjection);
	return mul(position, WorldViewProjection);  
}

bool IsTransparent(float4 pixelWorldPosition)
{
    bool transparent = false;
    float distance = length((pixelWorldPosition.xy - xCameraPosition.xy));
    if(xEnableTransparency && distance < 500)
    {
      float2 right = normalize(xCameraView._11_21);
      float2 A = (xCameraPosition.xy + right*150).xy;
      float2 B = (xCameraPosition.xy - right*150).xy;
      float2 C = xCameraPosition.xy - normalize(xCameraView._13_23)*700;
      float2 P = pixelWorldPosition.xy;
	  float2 v0 = C - A;
	  float2 v1 = B - A;
	  float2 v2 = P - A;
      float dot00 = dot(v0, v0);
      float dot01 = dot(v0, v1);
      float dot02 = dot(v0, v2);
      float dot11 = dot(v1, v1);
      float dot12 = dot(v1, v2);
      float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
      float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      if( (u>0) && (v>0) && (u+v < 1) && length(pixelWorldPosition.xy - xCameraPosition.xy) < 500)
          transparent = true;
    }  
    return transparent;
}

bool IsInShadow(float4 worldPosition)
{    
     bool inShadow = false;
     if(xEnableShadows)
    {
       float4 lightingPosition 
	      = GetPositionFromLight(worldPosition);// Get our position on the shadow map
   
       // Get the shadow map depth value for this pixel   
       float2 ShadowTexC = 
	      0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 );
       ShadowTexC.y = 1.0f - ShadowTexC.y;

       float shadowdepth = tex2D(ShadowMapSampler, ShadowTexC).r;    
    
       // Check our value against the depth value
       float ourdepth = 1 - (lightingPosition.z / lightingPosition.w);
    
       // Check the shadowdepth against the depth of this pixel
       // a fudge factor is added to account for floating-point error
	   if (shadowdepth-0.03 > ourdepth)
	       inShadow = true;
	}
	
	return inShadow;	  
}

float3 CalculateFog(float3 foglessColor, float4 worldPosition)
{
    float depth = abs(length(worldPosition- xFogSeedPosition));
    float3 fogColor = saturate(float3(0.15, 0.15, 0.15) * xAmbient);
    return lerp(foglessColor,fogColor, saturate((depth)/(3500)));
}

//////////////////////////////////////////
//Pixel Shaders

//Helper
float4 ApplyLightTransparancyShadow(float4 oldColor, float lightingFactor, float4 worldPosition)
{
    float4 newColor = oldColor;
    if(xForceMeshColor)
        newColor = xMeshColor;
    float4 light = saturate(lightingFactor * xLightColor);
    if( IsInShadow(worldPosition) )
        light = float4(0,0,0,1);
	newColor.rgb *= saturate(light + xAmbient);
	newColor.rgb = CalculateFog(newColor, worldPosition);
	if(IsTransparent(worldPosition))
	  newColor.a = 0.25;
	
	return newColor;
}

float4 TexturedWrapPS(VtoPPosLightTexWorld input) : COLOR0
{
    float4 color = tex2D(TextureSamplerWrap, input.TextureCoords);
    
    return ApplyLightTransparancyShadow(color, input.LightingFactor, input.WorldPosition);
}
float4 TexturedClampPS(VtoPPosLightTexWorld input) : COLOR0
{
    float4 color = tex2D(TextureSamplerClamp, input.TextureCoords);
    
    return ApplyLightTransparancyShadow(color, input.LightingFactor, input.WorldPosition);
}

float4 ColoredPS(VtoPPosColor input) : COLOR0
{
    float4 outputColor = input.Color;
    outputColor.rgb = CalculateFog(outputColor.rgb , input.WorldPosition);
    return outputColor;
}

float4 PixelShadowMap(ShadowMapOutput input) : COLOR0
{
   return float4(input.Depth.x, 0, 0, 1);
}

///////////////////////////////////////////////
//Techniques

technique TexturedWrap
{
    pass RenderOpaquePixels
    {
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader = compile ps_2_0 TexturedWrapPS();
    }
}

technique TexturedWrapTransparent
{
    pass RenderTransparentPixels
    {
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader = compile ps_2_0 TexturedWrapPS();
    }
}

technique TexturedClamp
{
    pass RenderOpaquePixels
    {
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader = compile ps_2_0 TexturedClampPS();
    }
}
technique SimpleColor
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexPositionColorShader();
        PixelShader = compile ps_2_0 ColoredPS();
    }
}

technique ShadowMap
{
   pass Pass1
   {
      VertexShader = compile vs_2_0 VertexShadowMap();
      PixelShader = compile ps_2_0 PixelShadowMap();
   }
}


/////////////////////////////////////
//Single Purpose Shaders

//------- Technique: Lazer ---------
VtoPPosColor LazerVS( float4 inPos : POSITION, float4 inColor: COLOR, float2 inTime: TEXCOORD0)
{	
	VtoPPosColor Output = (VtoPPosColor)0;
	
	Output.Position = CalculateVertexFramePosition(inPos);
	Output.Color = inColor;
	Output.Color.a = 1 - (xCurrentTime - inTime.x);
	
	return Output;    
}
float4 LazerPS(VtoPPosColor PSIn) : COLOR0
{
	return PSIn.Color;
}

technique Lazer
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 LazerVS();
		PixelShader  = compile ps_2_0 LazerPS();
	}
}