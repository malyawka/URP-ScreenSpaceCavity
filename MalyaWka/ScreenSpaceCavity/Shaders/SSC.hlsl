#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

float4 _CurvatureParams;
float4 _CavityParams;
float4 _SourceSize;

#define CURVATURE_SCALE         _CurvatureParams.x
#define CURVATURE_RIDGE         _CurvatureParams.z
#define CURVATURE_VALLEY        _CurvatureParams.w

#define CAVITY_DISTANCE         _CavityParams.x
#define CAVITY_ATTENUATION      _CavityParams.y
#define CAVITY_RIDGE            _CavityParams.z
#define CAVITY_VALLEY           _CavityParams.w

#if defined(SHADER_API_GLES) && !defined(SHADER_API_GLES3)
    #define CAVITY_SAMPLES 3
#else
    #define CAVITY_SAMPLES      _CurvatureParams.y
#endif

#define SCREEN_PARAMS           GetScaledScreenParams()
#define EPSILON                 1.0e-4

static const float kContrast = 0.6;
static const float kGeometryCoeff = 0.8;
static const float kBeta = 0.002;

// Hardcoded random UV values that improves performance.
// The values were taken from this function:
// r = frac(43758.5453 * sin( dot(float2(12.9898, 78.233), uv)) ));
// Indices  0 to 19 are for u = 0.0
// Indices 20 to 39 are for u = 1.0
static const float SSAORandomUV[40] =
{
    0.00000000,  // 00
    0.33984375,  // 01
    0.75390625,  // 02
    0.56640625,  // 03
    0.98437500,  // 04
    0.07421875,  // 05
    0.23828125,  // 06
    0.64062500,  // 07
    0.35937500,  // 08
    0.50781250,  // 09
    0.38281250,  // 10
    0.98437500,  // 11
    0.17578125,  // 12
    0.53906250,  // 13
    0.28515625,  // 14
    0.23137260,  // 15
    0.45882360,  // 16
    0.54117650,  // 17
    0.12941180,  // 18
    0.64313730,  // 19

    0.92968750,  // 20
    0.76171875,  // 21
    0.13333330,  // 22
    0.01562500,  // 23
    0.00000000,  // 24
    0.10546875,  // 25
    0.64062500,  // 26
    0.74609375,  // 27
    0.67968750,  // 28
    0.35156250,  // 29
    0.49218750,  // 30
    0.12500000,  // 31
    0.26562500,  // 32
    0.62500000,  // 33
    0.44531250,  // 34
    0.17647060,  // 35
    0.44705890,  // 36
    0.93333340,  // 37
    0.87058830,  // 38
    0.56862750,  // 39
};

float2 GetScreenSpacePosition(float2 uv)
{
    return uv * SCREEN_PARAMS.xy;
}

float2 CosSin(float theta)
{
    float sn, cs;
    sincos(theta, sn, cs);
    return float2(cs, sn);
}

float UVRandom(float u, float v)
{
    float f = dot(float2(12.9898, 78.233), float2(u, v));
    return frac(43758.5453 * sin(f));
}

half GetRandomUVForSSAO(float u, int index)
{
    return SSAORandomUV[u * 20 + index];
}

float3 PickSamplePoint(float2 uv, float randAddon, int index)
{
    float2 positionSS = GetScreenSpacePosition(uv);
    float gn = InterleavedGradientNoise(positionSS, index);

    float u = frac(gn) * 2.0 - 1.0;
    float theta = gn * TWO_PI;
    return float3(CosSin(theta) * sqrt(1.0 - u * u), u);
    
    //float u = frac(UVRandom(0.0, index + randAddon) + gn) * 2.0 - 1.0;
    //float theta = (UVRandom(1.0, index + randAddon) + gn) * TWO_PI;
    //return float3(CosSin(theta) * sqrt(1.0 - u * u), u);
    
    //const float u = frac(GetRandomUVForSSAO(0.0, index) + gn) * 2.0 - 1.0;
    //const float theta = (GetRandomUVForSSAO(1.0, index) + gn) * TWO_PI;
    //return float3(CosSin(theta) * sqrt(1.0 - u * u), u);
}

float RawToLinearDepth(float rawDepth)
{
    #if defined(_ORTHOGRAPHIC)
        #if UNITY_REVERSED_Z
            return ((_ProjectionParams.z - _ProjectionParams.y) * (1.0 - rawDepth) + _ProjectionParams.y);
        #else
            return ((_ProjectionParams.z - _ProjectionParams.y) * (rawDepth) + _ProjectionParams.y);
        #endif
    #else
        return LinearEyeDepth(rawDepth, _ZBufferParams);
    #endif
}

float SampleAndGetLinearDepth(float2 uv)
{
    float rawDepth = SampleSceneDepth(uv.xy).r;
    return RawToLinearDepth(rawDepth);
}

float3 ReconstructViewPos(float2 uv, float depth, float2 p11_22, float2 p13_31)
{
    #if defined(_ORTHOGRAPHIC)
        float3 viewPos = float3(((uv.xy * 2.0 - 1.0 - p13_31) * p11_22), depth);
    #else
        float3 viewPos = float3(depth * ((uv.xy * 2.0 - 1.0 - p13_31) * p11_22), depth);
    #endif
    return viewPos;
}

void SampleDepthNormalView(float2 uv, float2 p11_22, float2 p13_31, out float depth, out float3 normal, out float3 vpos)
{
    depth  = SampleAndGetLinearDepth(uv);
    vpos = ReconstructViewPos(uv, depth, p11_22, p13_31);
    normal = SampleSceneNormals(uv);
}

void SampleDepthView(float2 uv, float2 p11_22, float2 p13_31, out float depth, out float3 vpos)
{
    depth  = SampleAndGetLinearDepth(uv);
    vpos = ReconstructViewPos(uv, depth, p11_22, p13_31);
}

void SampleDepthNormal(float2 uv, out float depth, out float3 normal)
{
    depth  = SampleAndGetLinearDepth(uv);
    normal = SampleSceneNormals(uv);
}

float SampleDepth(float2 uv)
{
    return SampleAndGetLinearDepth(uv);
}

float3 SampleNormal(float2 uv)
{
    return SampleSceneNormals(uv);
}

float3 SampleView(float uv, float2 p11_22, float2 p13_31, float depth)
{
    return ReconstructViewPos(uv, depth, p11_22, p13_31);
}

float3x3 GetCoordinateConversionParameters(out float2 p11_22, out float2 p13_31)
{
    float3x3 camProj = (float3x3)unity_CameraProjection;

    p11_22 = rcp(float2(camProj._11, camProj._22));
    p13_31 = float2(camProj._13, camProj._23);

    return camProj;
}

float CurvatureSoftClamp(float curvature, float control)
{
    if (curvature < 0.5 / control)
    {
        return curvature * (1.0 - curvature * control);
    }
    return 0.25 / control;
}

void Curvature(float2 uv, out float curvature)
{
    curvature = 0.0;
    float3 offset = float3(_SourceSize.zw, 0.0) * (CURVATURE_SCALE);

    float normal_up = SampleNormal(uv + offset.zy).g;
    float normal_down = SampleNormal(uv - offset.zy).g;
    float normal_right = SampleNormal(uv + offset.xz).r;
    float normal_left = SampleNormal(uv - offset.xz).r;

    float normal_diff = (normal_up - normal_down) + (normal_right - normal_left);

    if (normal_diff >= 0.0)
    {
        curvature = 2.0 * CurvatureSoftClamp(normal_diff, CURVATURE_RIDGE);   
    }
    else
    {
        curvature = -2.0 * CurvatureSoftClamp(-normal_diff, CURVATURE_VALLEY);
    }
}

void Cavity(float2 uv, out float cavity, out float edges)
{
    cavity = edges = 0.0;
    float2 p11_22, p13_31;
    float3x3 camProj = GetCoordinateConversionParameters(p11_22, p13_31);

    float depth;
    float3 normal, vpos;
    SampleDepthNormalView(uv, p11_22, p13_31, depth, normal, vpos);

    if (depth == 1.0 || depth == 0.0)
    {
        return;
    }

    float randAddon = uv.x * 1e-10;
    float rcpSampleCount = rcp(CAVITY_SAMPLES);

    UNITY_LOOP
    for (int i = 0; i < int(CAVITY_SAMPLES); i++)
    {
        #if defined(SHADER_API_D3D11)
            i = floor(1.0001 * i);
        #endif
        
        float3 v_s1 = PickSamplePoint(uv, randAddon, i);
        v_s1 *= sqrt((i + 1.0) * rcpSampleCount ) * CAVITY_DISTANCE * 0.5;
        float3 vpos_s1 = vpos + v_s1;

        float3 spos_s1 = mul(camProj, vpos_s1);
        #if defined(_ORTHOGRAPHIC)
            float2 uv_s1_01 = clamp((spos_s1.xy + 1.0) * 0.5, 0.0, 1.0);
        #else
            float2 uv_s1_01 = clamp((spos_s1.xy * rcp(vpos_s1.z) + 1.0) * 0.5, 0.0, 1.0);
        #endif

        float depth_s1 = SampleAndGetLinearDepth(uv_s1_01);

        float3 vpos_s2 = ReconstructViewPos(uv_s1_01, depth_s1, p11_22, p13_31);
        float3 dir = vpos_s2 - vpos;
        float len = length(dir);
        float f_dot = dot(dir, normal);
        float f_cavities = f_dot - kBeta * depth;
        float f_edge = -f_dot - kBeta * depth;
        float f_bias = 0.05 * len + 0.0001;

        float attenuation = 1.0 / (len * (1.0 + len * len * CAVITY_ATTENUATION));

        if (f_cavities > -f_bias)
        {
            cavity += f_cavities * attenuation;
        }

        if (f_edge > f_bias)
        {
            edges += f_edge * attenuation;
        }
    }

    cavity *= 1.0 / CAVITY_SAMPLES; //CAVITY_DISTANCE;
    edges *= 1.0 / CAVITY_SAMPLES; //CAVITY_DISTANCE;

    cavity = PositivePow(cavity * rcpSampleCount, kContrast);
    edges = PositivePow(edges * rcpSampleCount, kContrast);
    
    cavity = clamp(cavity * CAVITY_VALLEY, 0.0, 1.0);
    edges = edges * CAVITY_RIDGE;
}

float SSC(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    float cavity = 0.0, edges = 0.0, curvature = 0.0;
    
    #ifdef _TYPE_CURVATURE
    Curvature(input.uv, curvature);
    #endif
    
    #ifdef _TYPE_CAVITY
    Cavity(input.uv, cavity, edges);
    #endif
           
    return clamp((1.0 - cavity) * (1.0 + edges) * (1.0 + curvature), 0.0, 4.0) * 0.25;
}