// Additive glow for effects (F9.21-F9.25): trails, particles, shields, halos. Uses vertex colour,
// so particle systems and trail renderers can fade it. _Round > 0 turns quads into soft dots.
Shader "CodeLyoko/LyokoGlow"
{
    Properties
    {
        [MainColor] [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Round ("Round Mask (particles)", Range(0, 1)) = 0
        _EdgeFade ("Edge Fade (trails)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Glow"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Round;
                half _EdgeFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = input.color;
                half round = saturate(1.0h - length(input.uv * 2.0 - 1.0));
                color.a *= lerp(1.0h, round * round, _Round);
                half edge = 1.0h - abs(input.uv.y * 2.0 - 1.0);
                color.a *= lerp(1.0h, edge, _EdgeFade);
                return color;
            }
            ENDHLSL
        }
    }
}
