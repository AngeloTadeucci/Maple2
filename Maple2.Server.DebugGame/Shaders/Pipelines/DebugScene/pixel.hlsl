#include <Common/pixelInput.hlsl>

Texture2D sampleTexture: register(t0);

SamplerState sampleSampler: register(s0);

float4 ps_main(vs_out input) : SV_TARGET{
    return input.color;
    //return sampleTexture.Sample(sampleSampler, input.texcoord);
}
