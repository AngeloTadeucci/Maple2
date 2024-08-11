struct vs_PositionBinding {
    float3 position : POSITION;
};

struct vs_AttributeBinding {
    float3 normal : NORMAL;
    uint color : COLOR;
    float2 texcoord : TEXCOORD;
};

struct vs_OrientationBinding {
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct vs_MorphBinding {
    float3 morphPos1 : MORPH_POSITION;
};

struct vs_BlendBinding {
    float3 blendWeight : BLENDWEIGHT;
    uint blendIndices : BLENDINDICES;
};

float4 intToVector(uint value) {
    float4 result;
    result.x = (float)((value >> (3 * 16)) & 0xFF);
    result.y = (float)((value >> (2 * 16)) & 0xFF);
    result.z = (float)((value >> (1 * 16)) & 0xFF);
    result.w = (float)(value & 0xFF);

	return result;
}

uint vectorToInt(float4 value) {
    value = saturate(value); // clamp from 0 to 1

    uint result = (uint)(value.w * 255);
    result += (uint)(value.x * 255) << (3 * 16);
    result += (uint)(value.y * 255) << (2 * 16);
    result += (uint)(value.z * 255) << (1 * 16);

    return result;
}
