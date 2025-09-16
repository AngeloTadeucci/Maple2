#include <Common/pixelInput.hlsl>

Texture2D sampleTexture: register(t0);

SamplerState sampleSampler: register(s0);

float4 ps_main(vs_out input) : SV_TARGET{
    //return float4(input.texcoord, 0, 1);
    return sampleTexture.Sample(sampleSampler, input.texcoord) * float4(1, 1, 1, 1);
}
