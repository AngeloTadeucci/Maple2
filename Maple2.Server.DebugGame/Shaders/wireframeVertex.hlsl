cbuffer ConstantBuffer : register(b0) {
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
    float4 color;
};

struct VertexInput {
    float3 position : POSITION;
};

struct VertexOutput {
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

VertexOutput vs_main(VertexInput input) {
    VertexOutput output;
    
    // Transform vertex position to world space
    float4 worldPos = mul(float4(input.position, 1.0f), worldMatrix);
    
    // Transform to view space
    float4 viewPos = mul(worldPos, viewMatrix);
    
    // Transform to projection space
    output.position = mul(viewPos, projectionMatrix);
    
    // Pass through color
    output.color = color;
    
    return output;
}
