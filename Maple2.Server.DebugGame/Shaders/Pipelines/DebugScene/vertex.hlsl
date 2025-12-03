#include <Common/vertexAttributes.hlsl>
#include <Common/pixelInput.hlsl>
#include <Common/sceneInput.hlsl>
#include <Common/sceneView.hlsl>
#include <Common/instance.hlsl>

vs_out vs_main(vs_PositionBinding pos, vs_AttributeBinding attrib, vs_OrientationBinding orientation, vs_MorphBinding morph, vs_BlendBinding blend) {
    vs_out output;

    output.position = mul(ProjectionMatrix, mul(ViewMatrix, mul(Transformation, float4(pos.position, 1))));
    output.color = intToVector(attrib.color);
    output.normal = attrib.normal;
    output.texcoord = attrib.texcoord;
    output.tangent = orientation.tangent;
    output.binormal = orientation.binormal;

    return output;
}
