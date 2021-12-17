Shader "Hidden/MalyaWka/ScreenSpaceCavity/Cavity"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 position : POSITION;
            float4 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 position : SV_POSITION;
            float4 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings VertDefault(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            
            output.position = float4(input.position.xyz, 1.0);
            
            #if UNITY_UV_STARTS_AT_TOP
                output.position.y *= -1;
            #endif
            
            output.uv = input.uv;
            output.uv += 1.0e-6;
            
            return output;
        }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull Off 
        ZWrite Off 
        ZTest Always
        
        Pass
        {
            Name "ScreenSpaceCavity"
            ZTest Always
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma multi_compile_local _ _TYPE_CURVATURE
            #pragma multi_compile_local _ _TYPE_CAVITY
            #pragma multi_compile_local _ _ORTHOGRAPHIC
            #pragma vertex VertDefault
            #pragma fragment SSC
            #include "SSC.hlsl"
			ENDHLSL    
        }
    }
}

