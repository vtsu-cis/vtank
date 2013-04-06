
/////////////////////////
//  Techniques
//      Rotation
//      NoRotation
//
//  Parameters

float ViewportHeight;
float4x4 View;
float4x4 Projection;

float CurrentTime;

float Duration;
float DurationRandomness;


float3 GlobalForce;

float4 MinColor;
float4 MaxColor;


float2 ForceSensitivity;
float2 EndHorizontalVelocity;
float2 EndVerticalVelocity;
float2 StartSize;
float2 EndSize;
float2 RotateSpeed;
float3 xFogSeedPosition;
float4 xAmbient;

//Texturing
texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

////////////////////////////////////////////////////////////////////
//                      Shader Structs
/////////////////////////////////////////////////////////////////////
struct Input
{
    float4 Position : POSITION0;
    float3 Velocity : NORMAL0;
    float4 RandomValue : COLOR0;
    float Time : TEXCOORD0;
};

struct Output
{
    float4 Position : POSITION0;
    float Size : PSIZE0;
    float4 Color : COLOR0;
    float4 Rotation : COLOR1;
    float4 WorldPosition : TEXCOORD0;
};

///////////////////////////////////////////////////////////////////////
//                     Vertex Shader Position Helper Function
///////////////////////////////////////////////////////////////////////
float4 WorldPositionToFrame(float4 position)
{
   return mul(mul(position, View), Projection);
}
float4 ComputeParticleWorldPosition(float3 position, float3 velocity, float age, float normalizedAge, float randomValue)
{
    float2 horizontal = velocity.xy;
    float startVertical = velocity.z;
    float startHorizontal = length(horizontal);
    float endHorizontal = lerp(EndHorizontalVelocity.x, EndHorizontalVelocity.y, randomValue);
    float endVertical = lerp(EndVerticalVelocity.x, EndVerticalVelocity.y, randomValue);
    
    float horizontalVelocityIntegral = startHorizontal * normalizedAge +
                                      (endHorizontal - startHorizontal) * 
                                       normalizedAge * normalizedAge / 2;
                              
    float verticalVelocityIntegral =  startVertical * normalizedAge +
                                      (endVertical - startVertical) * 
                                       normalizedAge * normalizedAge / 2;
     
    position.z += normalize(velocity.z) * verticalVelocityIntegral * Duration;
    position.xy += normalize(velocity.xy)* horizontalVelocityIntegral * Duration;
    
    float3 force = GlobalForce * lerp(ForceSensitivity.x, ForceSensitivity.y, randomValue);
    
    position += force * age * normalizedAge;
    
    return float4(position, 1);
}

///////////////////////////////////////////////////////////////////////
//                     Vertex Shader Size Helper Function
///////////////////////////////////////////////////////////////////////
float ComputeParticleSize(float4 position, float randomValue, float normalizedAge)
{
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);
    
    float size = lerp(startSize, endSize, normalizedAge);
    
    // Project the size into screen coordinates.
    return size * Projection._m11 / position.w * ViewportHeight / 2;
}

///////////////////////////////////////////////////////////////////////
//                     Vertex Shader Color Helper Function
///////////////////////////////////////////////////////////////////////
float4 ComputeParticleColor(float normalizedAge, float randomValue)
{
    float4 color = lerp(MinColor, MaxColor, randomValue);
    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;
    return color;
}

///////////////////////////////////////////////////////////////////////
//                     Vertex Shader Rotation Helper Function
///////////////////////////////////////////////////////////////////////
float4 ComputeParticleRotation(float age, float randomValue)
{
    float rotationSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);
    float rotation = rotationSpeed * age;
    
    float s = sin(rotation);
    float c = cos(rotation);
    
    float4 rotMatrix = float4(c, -s, s, c);
    rotMatrix *= 0.5;
    rotMatrix += 0.5;
    return rotMatrix;
}

///////////////////////////////////////////////////////////////////////
//                        Vertex Shader
///////////////////////////////////////////////////////////////////////
Output VertexShaderFunction(Input input)
{
    Output output;
    float age = CurrentTime - input.Time;
    age *= 1 + input.RandomValue.x * DurationRandomness;
    
    float normalizedAge = saturate(age / Duration);

    float4 position = ComputeParticleWorldPosition(input.Position, input.Velocity, age,
                                          normalizedAge, input.RandomValue.x);
    output.WorldPosition = position;
    output.Position = WorldPositionToFrame(position);
                                          
    output.Size = ComputeParticleSize(output.Position, input.RandomValue.y, 
                                     normalizedAge);
                                     
    output.Rotation = ComputeParticleRotation(age, input.RandomValue.z);
    
    output.Color = ComputeParticleColor(normalizedAge, input.RandomValue.w);

    return output;
}
//////////////////////////////////////////////////////////
//            Technique: NoRotation
//////////////////////////////////////////////////////////
float3 CalculateFog(float3 foglessColor, float4 worldPosition)
{
    float depth = abs(length(worldPosition- xFogSeedPosition));
    float3 fogColor = saturate(float3(0.15, 0.15, 0.15) * xAmbient);
    return lerp(foglessColor,fogColor, saturate((depth)/(3000)));
} 

struct NonRotatingShaderInput
{
    float4 Color : COLOR0;
    float4 WorldPosition : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

float4 NonRotatingShader(NonRotatingShaderInput input) : COLOR0
{
    float4 color = input.Color;
    //color.rgb = CalculateFog(color.rgb, input.WorldPosition);
    return tex2D(Sampler, input.TextureCoordinate) * color;
}

technique NoRotation
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 NonRotatingShader();
    }
}

//////////////////////////////////////////////////////////
//                  Technique: Rotation
//////////////////////////////////////////////////////////
struct RotatingShaderInput
{
    float4 Color : COLOR0;
    float4 Rotation : COLOR1;
    float4 WorldPosition : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

float4 RotatingShader(RotatingShaderInput input): COLOR0
{
    float2 textureCoordinate = input.TextureCoordinate - 0.5;
    
    float4 rotation = input.Rotation * 2 -1;
    textureCoordinate = mul(textureCoordinate, float2x2(rotation));
    
    textureCoordinate *= sqrt(2);
    textureCoordinate += 0.5;
    
    float4 color = input.Color;
    //color.rgb = CalculateFog(color.rgb, input.WorldPosition);
    return tex2D(Sampler, input.TextureCoordinate) * color;
}

technique Rotation
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 RotatingShader();
    }
}
