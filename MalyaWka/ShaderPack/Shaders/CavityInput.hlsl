#ifndef UNIVERSAL_INPUT_CAVITY_INCLUDED
#define UNIVERSAL_INPUT_CAVITY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D_X(_ScreenSpaceCavityTexture);
SAMPLER(sampler_ScreenSpaceCavityTexture);

half SampleCavity(float2 normalizedUV)
{
    float2 uv = UnityStereoTransformScreenSpaceTex(normalizedUV);
    return SAMPLE_TEXTURE2D_X(_ScreenSpaceCavityTexture, sampler_ScreenSpaceCavityTexture, uv).r;
}

#endif