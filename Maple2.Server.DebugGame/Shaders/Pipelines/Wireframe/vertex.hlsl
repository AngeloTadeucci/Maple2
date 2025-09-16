#include <Common/sceneView.hlsl>
#include <Common/instance.hlsl>

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
    float4 worldPos = mul(float4(input.position, 1.0f), Transformation);

    // Transform to view space
    float4 viewPos = mul(worldPos, ViewMatrix);

    // Transform to projection space
    output.position = mul(viewPos, ProjectionMatrix);

    // Pass through color
    output.color = color;

    return output;
}
